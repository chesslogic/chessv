using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
using ChessV;
using ChessV.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Archipelago.APChessV
{
  public class LocationHandler
  {
    public static LocationHandler _instance;

    public static LocationHandler GetInstance()
    {
      if (_instance == null)
        lock (typeof(LocationHandler))
          if (_instance == null)
            _instance = new LocationHandler();
      return _instance;
    }

    protected LocationHandler()
    {
      seHandler = StartMatch;
      ApmwCore.getInstance().StartedEventHandlers.Add(seHandler);
      msHandler = SetupMove;
      ApmwCore.getInstance().NewMoveSetup.Add(msHandler);
      mpHandler = HandleMove;
      ApmwCore.getInstance().NewMovePlayed.Add(mpHandler);
      feHandler = HandleMatch;
      ApmwCore.getInstance().MatchFinished.Add(feHandler);
      Initialized = false;
    }

    /// <summary>
    /// Test-only constructor. Wires up the location-check helper and marks the handler
    /// initialized without requiring an Archipelago session. Not intended for production use.
    /// </summary>
    internal LocationHandler(ILocationCheckHelper locationCheckHelper) : this()
    {
      LocationCheckHelper = locationCheckHelper;
      Initialized = true;
      CaptureLookup = new CaptureLookup();
    }

    /// <summary>
    /// Test-only constructor that also injects a pre-configured Match, bypassing
    /// <see cref="StartMatch"/>. Not intended for production use.
    /// </summary>
    internal LocationHandler(ILocationCheckHelper locationCheckHelper, Match match)
      : this(locationCheckHelper)
    {
      this.match = match;
      humanPlayer = match.GetPlayer(0).IsHuman ? 0 : 1;
    }

    public void Initialize(ILocationCheckHelper locationCheckHelper, ArchipelagoSession session)
    {
      //if (Initialized)
      //  throw new InvalidOperationException("Cannot reinitialize LocationHandler");
      LocationCheckHelper = locationCheckHelper;
      Initialized = true;
      victory = () => new Task(() => Victory(session)).Start();
      deathlink = (reason) => new Task(() => Deathlink(session, reason)).Start();
      CaptureLookup = new CaptureLookup();
    }

    public ILocationCheckHelper LocationCheckHelper { get; private set; }
    public CaptureLookup CaptureLookup { get; private set; }
    ArchipelagoSession session;

    public bool Initialized { get; private set; }
    private StartedEventHandler seHandler;
    private Action<Match> feHandler;
    private Action<MoveInfo> msHandler;
    private Action<MoveInfo> mpHandler;
    private Action victory;
    private Action<string> deathlink;
    private Match match;
    public HashSet<Match> DeathlinkedMatches = new HashSet<Match>();
    private int humanPlayer;
    private int capturedPieces;
    private int capturedPawns;
    private Dictionary<int, int> currentSquaresToOriginalSquares = new Dictionary<int, int>();
    private Dictionary<int, Piece> lastPiecesSeen = new Dictionary<int, Piece>();

    public void StartMatch(Match match)
    {
      if (!Initialized && match.Game.GameAttribute.GameName == ApmwConstants.GameNameStandard)
        throw new InvalidOperationException("LocationHandler has not been initialized");
      this.match = match;
      //match.Game.MovePlayed += (move) => this.HandleMove(move);
      humanPlayer = this.match.GetPlayer(0).IsHuman ? 0 : 1;
      // TODO(chesslogic): why does this continue to increment between games?
      capturedPawns = 0;
      capturedPieces = 0;
      currentSquaresToOriginalSquares = new Dictionary<int, int>();

      this.match.Finished += HandleMatch;
      TryValidatePlayingArchipelago();
    }

    public void EndMatch()
    {
      if (!Initialized && match.Game.GameAttribute.GameName == ApmwConstants.GameNameStandard)
        throw new InvalidOperationException("LocationHandler has not been initialized");
      TryValidatePlayingArchipelago();
      if (this.match == null)
      {
        // TODO(chesslogic): mention "no match to end" in logger
        return;
      }
      if (match.Result.Winner != this.humanPlayer)
        deathlink("resigned.");

      //ApmwCore.getInstance().NewMovePlayed.Remove(mpHandler);
      //mpHandler = null;
      // ApmwCore.getInstance().StartedEventHandlers.Remove(seHandler);
      // seHandler = null;
      this.capturedPawns = 0;
      this.capturedPieces = 0;
      this.match.Finished -= HandleMatch;
    }

    /** unused */
    //public void HandleMove(Movement move)
    //{
    //  if (move == null)
    //    return; // probably never happens

    //  MoveInfo info = new MoveInfo();
    //  info.Player = move.Player;
    //  info.MoveType = move.MoveType;
    //  info.FromSquare = move.FromSquare;
    //  info.ToSquare = move.ToSquare;
    //  info.PieceMoved = match.Game.Board[move.FromSquare];
    //  info.PieceCaptured = match.Game.Board[move.ToSquare];
    //  HandleMove(info);
    //}

    /// When a Checkers piece multi-captures, we need to figure out all of the captures it made.
    /// The only way I can figure out how to do that is to manually compare every single square.
    /// Consider that a multi-capture ending on the same file is ambiguous: did the Checkers go right or left?
    public void SetupMove(MoveInfo info) {
      if (!Initialized && match.Game.GameAttribute.GameName == ApmwConstants.GameNameStandard)
        throw new InvalidOperationException("LocationHandler has not been initialized");
      if (!TryValidatePlayingArchipelago())
        return;
      // Store the current board state before the move
      Dictionary<int, Piece> preMoveState = new Dictionary<int, Piece>();
      for (int square = 0; square < match.Game.Board.NumSquares; square++)
      {
          Piece boardPiece = match.Game.Board[square];
          if (boardPiece != null)
          {
              preMoveState[square] = boardPiece;
          }
      }
      // Store the state for comparison in HandleMove
      lastPiecesSeen = preMoveState;

    }

    private void HandleCheckersMultiCapture(MoveInfo info)
    {
      // Compare board states to find captured pieces
      var capturedSquares = new List<int>();
      foreach (var kvp in lastPiecesSeen)
      {
        int square = kvp.Key;
        Piece prePiece = kvp.Value;
            
        // If a piece was on this square before but isn't now, and it's not the moving piece's square
        if (match.Game.Board[square] == null && square != info.FromSquare)
        {
          capturedSquares.Add(square);
        }
      }

      // Sort captured squares by rank progression to determine capture order
      capturedSquares.Sort((a, b) => {
        int rankA = match.Game.Board.GetRank(a);
        int rankB = match.Game.Board.GetRank(b);
        int startRank = match.Game.Board.GetRank(info.FromSquare);
        
        // For each player, determine which direction is "forward"
        int rankDiffA = info.Player == 0 ? rankA - startRank : startRank - rankA;
        int rankDiffB = info.Player == 0 ? rankB - startRank : startRank - rankB;
        
        return rankDiffA.CompareTo(rankDiffB);
      });

      // Generate individual capture events for each captured piece
      int currentSquare = info.FromSquare;
      foreach (int capturedSquare in capturedSquares)
      {
        // Calculate landing square after this capture
        int currentFile = match.Game.Board.GetFile(currentSquare);
        int capturedFile = match.Game.Board.GetFile(capturedSquare);
        int currentRank = match.Game.Board.GetRank(currentSquare);
        int capturedRank = match.Game.Board.GetRank(capturedSquare);

        // Calculate file difference considering wrapping
        int directFileDiff = capturedFile - currentFile;
        int wrappedFileDiff1 = (capturedFile + match.Game.Board.NumFiles) - currentFile;
        int wrappedFileDiff2 = capturedFile - (currentFile + match.Game.Board.NumFiles);
        int fileDiff;
        
        // Choose the smallest absolute difference
        if (Math.Abs(directFileDiff) <= Math.Abs(wrappedFileDiff1) && 
            Math.Abs(directFileDiff) <= Math.Abs(wrappedFileDiff2))
        {
          fileDiff = directFileDiff;
        }
        else if (Math.Abs(wrappedFileDiff1) <= Math.Abs(wrappedFileDiff2))
        {
          fileDiff = wrappedFileDiff1;
        }
        else
        {
          fileDiff = wrappedFileDiff2;
        }

        // Calculate rank difference based on player's direction
        int rankDiff = info.Player == 0 ? 
          capturedRank - currentRank : 
          currentRank - capturedRank;

        // Calculate next square, handling wrapping
        int nextFile = ((capturedFile + fileDiff) % match.Game.Board.NumFiles + match.Game.Board.NumFiles) 
                      % match.Game.Board.NumFiles;
        int nextRank = info.Player == 0 ?
          capturedRank + rankDiff :
          capturedRank - rankDiff;
        
        // Only create move if the next rank is valid
        if (nextRank >= 0 && nextRank < match.Game.Board.NumRanks)
        {
          int nextSquare = match.Game.Board.LocationToSquare(new Location(nextRank, nextFile));

          // Create capture move info
          MoveInfo captureInfo = new MoveInfo();
          captureInfo.Player = info.Player;
          captureInfo.FromSquare = currentSquare;
          captureInfo.ToSquare = capturedSquare; // we don't actually move to this square, but we need it for the capture lookup
          captureInfo.MoveType = MoveType.StandardCapture;
          captureInfo.PieceMoved = info.PieceMoved;
          captureInfo.PieceCaptured = lastPiecesSeen[capturedSquare];

          // Update for next iteration
          currentSquare = nextSquare;

          // Process this capture
          HandleMove(captureInfo);
        }
      }

      // Clear the pre-move state
      lastPiecesSeen.Clear();
    }

    public void HandleMove(MoveInfo info)
    {
      if (!Initialized && match.Game.GameAttribute.GameName == ApmwConstants.GameNameStandard)
        throw new InvalidOperationException("LocationHandler has not been initialized");
      if (!TryValidatePlayingArchipelago())
        return;
      if (info == null)
        return; // probably never happens

      CheckVictoryAndDeathlink();

      var locations = new List<long>();
      if (info.Player != humanPlayer)
      {
        // CPU can't emit locations - we've updated state, so return early
        UpdateMoveState(info);
        return;
      }

      RecordKingMoveLocations(info, locations);
      RecordSurvivalLocation(locations);
      // The captures section may early-return after dispatching to HandleCheckersMultiCapture.
      if (RecordCaptureLocations(info, locations)) return;
      RecordThreatLocations(locations);

      if (locations.Count > 0 && !DeathlinkedMatches.Contains(match))
        new Task(() => LocationCheckHelper.CompleteLocationChecks(locations.ToArray())).Start();

      UpdateMoveState(info);
    }

    private long Loc(string name)
      => LocationCheckHelper.GetLocationIdFromName(ApmwConstants.TrackerName, name);

    private void CheckVictoryAndDeathlink()
    {
      //
      // BEGIN victory ...
      //

      // I could join all of these with a &&, but this is more dramatic.
      if (match.Result != null && !match.Result.IsNone)
        if (match.Result.Type == ResultType.Win)
        {
          if (match.Result.Winner == this.humanPlayer)
            victory();
          else
            deathlink("was checkmated.");
        }

      //
      // END victory ...
      //
    }

    private void RecordKingMoveLocations(MoveInfo info, List<long> locations)
    {
      //
      // BEGIN various king moves ...
      //

      Piece piece = info.PieceMoved;
      // check if move is early and is directly forward one step
      if (ApmwCore.getInstance().kings.Contains(piece.PieceType))
      {
        // TODO(chesslogic): Math.min(match.Game.Board pieces count, 4) 
        if (match.Game.GameTurnNumber <= 10 &&
          match.Game.Board.GetFile(info.ToSquare) == 4 &&
          (match.Game.Board.GetRank(info.ToSquare) == 1 || match.Game.Board.GetRank(info.ToSquare) == 6))
        {
          locations.Add(Loc("King to E2/E7 Early"));
        }
        // check if move is to A file
        if (match.Game.Board.GetFile(info.ToSquare) == 0)
        {
          locations.Add(Loc("King to A File"));
        }
        // check if move is to distant rank
        if ((info.Player == 1 && match.Game.Board.GetRank(info.ToSquare) == 0) ||
          (info.Player == 0 && match.Game.Board.GetRank(info.ToSquare) == 7))
        {
          // TODO(chesslogic): info.ToSquare probably isn't based on Board.PlayerSquare (used for PST eval)
          // TODO(chesslogic): ... but if it is, just check info.GetRank==7, ignore info.Player
          locations.Add(Loc("King to Back Rank"));
        }
        // check if move is to center
        if ((match.Game.Board.GetFile(info.ToSquare) == 3 || match.Game.Board.GetFile(info.ToSquare) == 4) &&
          (match.Game.Board.GetRank(info.ToSquare) == 3 || match.Game.Board.GetRank(info.ToSquare) == 4))
        {
          locations.Add(Loc("King to Center"));
        }

        if (info.MoveType.HasFlag(MoveType.Castling))
        {
          if (info.FromSquare > info.ToSquare)
          {
            locations.Add(Loc("O-O-O Castle"));
          }
          else
          {
            locations.Add(Loc("O-O Castle"));
          }
        }
      }

      //
      // END various king moves ...
      //
    }

    private void RecordSurvivalLocation(List<long> locations)
    {
      //
      // BEGIN survive ...
      //

      var currentTurn = match.Game.GameTurnNumber;
      locations.Add(Loc($"Current Objective: Survive {currentTurn} Turns"));

      //
      // END survive ...
      //
    }

    /// <summary>
    /// Records capture-related locations. Returns true if HandleMove should return immediately
    /// (i.e. a Checkers multi-capture was dispatched).
    /// </summary>
    private bool RecordCaptureLocations(MoveInfo info, List<long> locations)
    {
      //
      // START captures ...
      //

      Piece piece = info.PieceMoved;
      // check if move is capture
      if (info.MoveType.HasFlag(MoveType.CaptureProperty))
      {
        // handle king captures
        if (ApmwCore.getInstance().kings.Contains(piece.PieceType))
        {
          locations.Add(Loc("King Captures Anything"));
        }
        /*
        if (info.PieceCaptured.PieceType.Name.Equals("King"))
        {
          locations.Add(Loc("Checkmate Maxima"));
          // TODO(chesslogic): count # of pieces and pawns, emit corresponding checkmate
        }
        */

        // handle specific piece
        int moves = match.Game.BoardMoveStack.MoveCount;
        int originalSquare = info.ToSquare;
        // En Passant
        if (info.MoveType.HasFlag(MoveType.EnPassant))
          originalSquare = info.ToSquare + (1 - info.Player * 2);
        // Checkers
        else if (info.MoveType == MoveType.ExtraCapture ||
            info.MoveType == (MoveType.ExtraCapture | MoveType.PromotionProperty))
        {
          HandleCheckersMultiCapture(info);
          return true;
        }
        // Lookup starting square - this is the original square a piece started the game at
        if (currentSquaresToOriginalSquares.ContainsKey(originalSquare))
          originalSquare = currentSquaresToOriginalSquares[originalSquare];
        int originalFile = match.Game.Board.GetFile(originalSquare);
        int originalRank = match.Game.Board.GetRank(originalSquare);
        string fileNotation = match.Game.Board.GetFileNotation(originalFile);
        fileNotation = fileNotation.ToUpper();
        bool isPiece = originalRank == 0 || originalRank == 7;
        //bool isPiece = !ApmwCore.getInstance().pawns.Contains(info.PieceCaptured.PieceType);
        string locationName;
        if (isPiece)
          locationName = CaptureLookup.fileToLocation(match.Game.NumFiles, fileNotation);
        else
          locationName = "Capture Pawn " + fileNotation;
        locations.Add(Loc(locationName));

        // handle piece sequence
        int captures;
        if (isPiece)
          captures = ++capturedPieces;
        else
          captures = ++capturedPawns;
        // capture any
        var totalCaptures = capturedPawns + capturedPieces;
        if (totalCaptures > 1) {
          if (totalCaptures < 15 || (ApmwConfig.getInstance().Goal != Goal.Single && totalCaptures < 19))
          {
            locationName = "Capture Any " + totalCaptures;
            locations.Add(Loc(locationName));
          }
        }
        if (captures > 1)
        {
          // capture piece
          if (isPiece)
          {
            locationName = "Capture " + captures + " Pieces";
            if (capturedPawns >= captures)
            {
              locations.Add(Loc(locationName));
              locationName = "Capture " + captures + " Of Each";
            }
          }
          // capture pawn
          else
          {
            locationName = "Capture " + captures + " Pawns";
            if (capturedPieces >= captures)
            {
              locations.Add(Loc(locationName));
              locationName = "Capture " + captures + " Of Each";
            }
          }
          locations.Add(Loc(locationName));
          // capture everything
          if ((ApmwConfig.getInstance().Goal == Goal.Single && capturedPieces >= 7 && capturedPawns >= 8) ||
            (ApmwCore.getInstance().isGrand && capturedPieces >= 9 && capturedPawns >= 10))
          {
            locations.Add(Loc("Capture Everything"));
          }
        }
      }
      if ((info.MoveType & MoveType.EnPassant) == MoveType.EnPassant)
        locations.Add(Loc("French Move"));

      //
      // END captures ...
      //

      return false;
    }

    private void RecordThreatLocations(List<long> locations)
    {
      //
      // BEGIN threats ...
      //

      Dictionary<Piece, int> forkers = new Dictionary<Piece, int>();
      Dictionary<Piece, int> trueForkers = new Dictionary<Piece, int>();
      Dictionary<Piece, (bool, bool)> kingAttacked = new Dictionary<Piece, (bool, bool)>();
      Dictionary<Piece, (bool, bool)> queenAttacked = new Dictionary<Piece, (bool, bool)>();
      for (int square = 0; square < match.Game.Board.NumSquares; square++)
      {
        // Make a list of all pieces attacking {square}
        List<Piece> attackers;
        if (match.Game.IsSquareAttacked(square, humanPlayer, out attackers))
        {
          var loc = match.Game.Board.SquareToLocation(square);
          Piece attackedPiece = match.Game.Board[square];

          if (attackedPiece == null || attackedPiece.Player == humanPlayer) { continue; }

          bool attackedPieceIsPawn = ApmwCore.getInstance().pawns.Contains(attackedPiece.PieceType);
          if (attackedPieceIsPawn)
          {
            locations.Add(Loc("Threaten Pawn"));
            continue;
          }
          bool attackedPieceIsMinor = ApmwCore.getInstance().minors.Contains(attackedPiece.PieceType);
          if (attackedPieceIsMinor)
            locations.Add(Loc("Threaten Minor"));
          bool attackedPieceIsMajor = ApmwCore.getInstance().majors.Contains(attackedPiece.PieceType);
          if (attackedPieceIsMajor)
            locations.Add(Loc("Threaten Major"));
          bool attackedPieceIsQueen = ApmwCore.getInstance().queens.Contains(attackedPiece.PieceType);
          if (attackedPieceIsQueen)
            locations.Add(Loc("Threaten Queen"));
          bool attackedPieceIsKing = ApmwCore.getInstance().kings[0] == attackedPiece.PieceType;
          if (attackedPieceIsKing)
            locations.Add(Loc("Threaten King"));

          // For each piece attacking {square}, note it found a target, and check if it's attacking elsewhere
          for (int i = 0; i < attackers.Count; i++)
          {
            RecordForksForAttacker(
              square, attackers[i], attackedPiece,
              attackedPieceIsKing, attackedPieceIsQueen,
              forkers, trueForkers, kingAttacked, queenAttacked,
              locations);
          }

          // TODO(chesslogic): pin??? how would??? maybe check if target has no moves... for king pins?
          // TODO(chesslogic): wait, this uses extinction not checkmate, so that won't even work!
          // TODO(chesslogic): maybe temporarily add CannonMove?? can a cannon pin?????
          /*
          MoveList moveList = new MoveList(
            match.Game.Board, new ChessV.SearchStack[] { },
            new uint[] { }, new uint[] { }, new uint[,,] { }, new uint[,,] { }, 1);
          piece.GenerateMoves(moveList, false);
          if (moveList.Count > 0)
          {
            locations.Add(Loc("Pin"));
            // all pieces matter?? I don't know ... this only detects pins to king...
            locations.Add(Loc("Skewer"));
          }
          */
        }
      }

      //
      // END threats ...
      //
    }

    private void RecordForksForAttacker(
      int square, Piece attacker, Piece attackedPiece,
      bool attackedPieceIsKing, bool attackedPieceIsQueen,
      Dictionary<Piece, int> forkers,
      Dictionary<Piece, int> trueForkers,
      Dictionary<Piece, (bool, bool)> kingAttacked,
      Dictionary<Piece, (bool, bool)> queenAttacked,
      List<long> locations)
    {
      // This doesn't calculate whether a single move could defend both pieces.
      // Imagine a rook, blocked by a pawn offensively, sliding to a defensive location between two pieces
      // - or a "discovered defend" where two different pieces are used to defend the forked pieces.
      // We have to add "true forks" at each step, because we count the target, not just the source
      // There are conceivable moves which can protect both pieces but those are SO complicated, dude
      // And on the other hand, forked pieces defending each other might still be a fork!
      // A King protected by a Queen is not defended...
      bool isTrueFork =
        !match.Game.IsSquareAttacked(attacker.Square, humanPlayer ^ 1) && // will live to attack
        (
          // TODO: attacker has less value than a queen
          attackedPiece.PieceType.MidgameValue >= (attacker.MidgameValue + 100) || // recapture still loses material
          attackedPieceIsKing || // no king can be defended
          !match.Game.IsSquareAttacked(square, humanPlayer ^ 1) // not defended
        );
      if (isTrueFork)
      {
        if (!trueForkers.ContainsKey(attacker))
          trueForkers[attacker] = 0;
        trueForkers[attacker]++;
      }

      // This is used to determine if a fork is royal.
      if (attackedPieceIsKing)
        kingAttacked[attacker] = (true, isTrueFork);
      if (attackedPieceIsQueen)
        queenAttacked[attacker] = (true, isTrueFork);

      if (!forkers.ContainsKey(attacker))
        forkers[attacker] = 0;
      if (++forkers[attacker] > 1)
      {
        locations.Add(Loc("Fork, Sacrificial"));
        if (trueForkers.ContainsKey(attacker) && trueForkers[attacker] > 1)
          locations.Add(Loc("Fork, True"));
        if (forkers[attacker] > 2)
        {
          locations.Add(Loc("Fork, Sacrificial Triple"));
          if (trueForkers.ContainsKey(attacker) && trueForkers[attacker] > 2)
            locations.Add(Loc("Fork, True Triple"));
        }
        if (kingAttacked.ContainsKey(attacker) && queenAttacked.ContainsKey(attacker))
          if (kingAttacked[attacker].Item1 && queenAttacked[attacker].Item1)
          {
            locations.Add(Loc("Fork, Sacrificial Royal"));
            if (kingAttacked[attacker].Item2 && queenAttacked[attacker].Item2)
              locations.Add(Loc("Fork, True Royal"));
          }
      }
    }

    internal void UpdateMoveState(MoveInfo info)
    {
      if (!TryValidatePlayingArchipelago())
        return;
      // update original positions
      if (currentSquaresToOriginalSquares.ContainsKey(info.FromSquare))
        currentSquaresToOriginalSquares[info.ToSquare] = currentSquaresToOriginalSquares[info.FromSquare];
      else
        currentSquaresToOriginalSquares[info.ToSquare] = info.FromSquare;
      if (info.MoveType.HasFlag(MoveType.Castling))
      {
        var flipBoard = 7 * (1 - humanPlayer);
        if (info.ToSquare > info.FromSquare)
          currentSquaresToOriginalSquares[info.ToSquare - 8] = 56 + flipBoard;
        else
          currentSquaresToOriginalSquares[info.ToSquare + 8] = 0 + flipBoard;
        // TODO: figure out where the rook moved from
      }
    }

    public void HandleMatch(Match match)
    {
      if (!TryValidatePlayingArchipelago())
        return;
      List<long> locations = new List<long>();
      if (match == null) return;
      if (match.Result == null) return;
      if (match.Result.Type == ResultType.Win && match.Result.Winner == this.humanPlayer)
      {
        victory();
      }
    }

    public void Victory(ArchipelagoSession session)
    {
      if (!TryValidatePlayingArchipelago())
        return;
      LocationCheckHelper.CompleteLocationChecks(LocationCheckHelper.GetLocationIdFromName(ApmwConstants.TrackerName, "Checkmate Minima"));
      if (ApmwCore.getInstance().isGrand && match.Game.GameAttribute.GameName == ApmwConstants.GameNameGrand)
      {
        LocationCheckHelper.CompleteLocationChecks(LocationCheckHelper.GetLocationIdFromName(ApmwConstants.TrackerName, "Checkmate Maxima"));
        var statusUpdatePacket = new StatusUpdatePacket();
        statusUpdatePacket.Status = ArchipelagoClientState.ClientGoal;
        session.Socket.SendPacket(statusUpdatePacket);
      }
      else if (ApmwConfig.getInstance().Goal == Goal.Single)
      {
        var statusUpdatePacket = new StatusUpdatePacket();
        statusUpdatePacket.Status = ArchipelagoClientState.ClientGoal;
        session.Socket.SendPacket(statusUpdatePacket);
      }
    }

    public void Deathlink(ArchipelagoSession session, string reason = null)
    {
      if (!TryValidatePlayingArchipelago())
        return;
      lock (DeathlinkedMatches)
      {
        if (DeathlinkedMatches.Contains(match))
          return;
        DeathlinkedMatches.Add(match);
      }
      if (session.ConnectionInfo.Tags.Contains("DeathLink"))
      {
        var deathLinkService = session.CreateDeathLinkService();
        var deathLink = new DeathLink(session.Players.GetPlayerName(session.ConnectionInfo.Slot), reason);
        deathLinkService.SendDeathLink(deathLink);
        var message = string.Join(" ", session.Players.GetPlayerName(session.ConnectionInfo.Slot), reason);
        ArchipelagoClient.getInstance().nonSessionMessages.Add(string.Join(" ", "DeathLink sent:", message));
      }
    }

    /// <summary>
    /// Throws an exception if the player has ever connected to AP but is playing a non-Archipelago game.
    /// </summary>
    /// <returns>
    /// True if the player has ever connected during this client's lifetime, and is playing Archipelago.
    /// False if the user is not connected (and, hopefully, not playing ApmwChessGame).
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    public bool TryValidatePlayingArchipelago()
    {
      // TODO(chesslogic): Player can't "disconnect" without restarting.
      if (Initialized && match != null && match.Game.GameAttribute != null &&
          match.Game.GameAttribute.GameName != ApmwConstants.GameNameStandard &&
          match.Game.GameAttribute.GameName != ApmwConstants.GameNameGrand)
        throw new InvalidOperationException("Please disconnect from Archipelago when using other ChessV features");
      return Initialized;
    }
  }
}
