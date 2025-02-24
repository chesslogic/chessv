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
    private const string NAME_OF_ARCHIPELAGO_GAME_ATTRIBUTE = "Archipelago Multiworld";
    private const string NAME_OF_GRAND_ARCHIPELAGO_GAME_ATTRIBUTE = "Archipelago Multiworld Super-Sized";
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
      if (!Initialized && match.Game.GameAttribute.GameName == NAME_OF_ARCHIPELAGO_GAME_ATTRIBUTE)
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
      if (!Initialized && match.Game.GameAttribute.GameName == NAME_OF_ARCHIPELAGO_GAME_ATTRIBUTE)
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
      if (!Initialized && match.Game.GameAttribute.GameName == NAME_OF_ARCHIPELAGO_GAME_ATTRIBUTE)
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
          captureInfo.ToSquare = nextSquare;
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
      if (!Initialized && match.Game.GameAttribute.GameName == NAME_OF_ARCHIPELAGO_GAME_ATTRIBUTE)
        throw new InvalidOperationException("LocationHandler has not been initialized");
      if (!TryValidatePlayingArchipelago())
        return;
      List<long> locations = new List<long>();
      if (info == null)
        return; // probably never happens

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

      // CPU can't emit locations - we've updated state, so return early
      if (info.Player != humanPlayer)
      {
        if (locations.Count > 0)
          new Task(() => LocationCheckHelper.CompleteLocationChecks(locations.ToArray())).Start();
        UpdateMoveState(info);
        return;
      }

      Piece piece = info.PieceMoved;
      string pieceName = piece.PieceType.Name;

      // TODO(chesslogic): refactor these, extract into individual methods, probably reduce them to 7 lines

      //
      // BEGIN various king moves ...
      //

      // check if move is early and is directly forward one step
      if (ApmwCore.getInstance().kings.Contains(piece.PieceType))
      {
        // TODO(chesslogic): Math.min(match.Game.Board pieces count, 4) 
        if (match.Game.GameTurnNumber <= 10 &&
          match.Game.Board.GetFile(info.ToSquare) == 4 &&
          (match.Game.Board.GetRank(info.ToSquare) == 1 || match.Game.Board.GetRank(info.ToSquare) == 6))
        {
          locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "King to E2/E7 Early"));
        }
        // check if move is to A file
        if (match.Game.Board.GetFile(info.ToSquare) == 0)
        {
          locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "King to A File"));
        }
        // check if move is to distant rank
        if ((info.Player == 1 && match.Game.Board.GetRank(info.ToSquare) == 0) ||
          (info.Player == 0 && match.Game.Board.GetRank(info.ToSquare) == 7))
        {
          // TODO(chesslogic): info.ToSquare probably isn't based on Board.PlayerSquare (used for PST eval)
          // TODO(chesslogic): ... but if it is, just check info.GetRank==7, ignore info.Player
          locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "King to Back Rank"));
        }
        // check if move is to center
        if ((match.Game.Board.GetFile(info.ToSquare) == 3 || match.Game.Board.GetFile(info.ToSquare) == 4) &&
          (match.Game.Board.GetRank(info.ToSquare) == 3 || match.Game.Board.GetRank(info.ToSquare) == 4))
        {
          locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "King to Center"));
        }

        if (info.MoveType.HasFlag(MoveType.Castling))
        {
          if (info.FromSquare > info.ToSquare)
          {
            locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "O-O-O Castle"));
          }
          else
          {
            locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "O-O Castle"));
          }
        }
      }

      //
      // END various king moves ...
      //

      //
      // BEGIN survive ...
      //

      var currentTurn = match.Game.GameTurnNumber;
      locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", $"Current Objective: Survive {currentTurn} Turns"));

      //
      // END survive ...
      //

      //
      // START captures ...
      //

      // check if move is capture
      if (info.MoveType.HasFlag(MoveType.CaptureProperty))
      {
        // handle king captures
        if (ApmwCore.getInstance().kings.Contains(piece.PieceType))
        {
          locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "King Captures Anything"));
        }
        /*
        if (info.PieceCaptured.PieceType.Name.Equals("King"))
        {
          locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Checkmate Maxima"));
          // TODO(chesslogic): count # of pieces and pawns, emit corresponding checkmate
        }
        */

        // handle specific piece
        int moves = match.Game.BoardMoveStack.MoveCount;
        int originalSquare = info.ToSquare;
        // En Passant
        if (info.MoveType.HasFlag(MoveType.EnPassant))
          originalSquare = info.ToSquare + 2 * (1 - info.Player * 2);
        // Checkers
        else if (info.MoveType == MoveType.ExtraCapture ||
            info.MoveType == (MoveType.ExtraCapture | MoveType.PromotionProperty))
        {
          HandleCheckersMultiCapture(info);
          return;
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
        locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", locationName));

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
            locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", locationName));
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
              locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", locationName));
              locationName = "Capture " + captures + " Of Each";
            }
          }
          // capture pawn
          else
          {
            locationName = "Capture " + captures + " Pawns";
            if (capturedPieces >= captures)
            {
              locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", locationName));
              locationName = "Capture " + captures + " Of Each";
            }
          }
          locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", locationName));
          // capture everything
          if ((ApmwConfig.getInstance().Goal == Goal.Single && capturedPieces >= 7 && capturedPawns >= 8) ||
            (ApmwCore.getInstance().isGrand && capturedPieces >= 9 && capturedPawns >= 10))
          {
            locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Capture Everything"));
          }
        }
      }
      if ((info.MoveType & MoveType.EnPassant) == MoveType.EnPassant)
        locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "French Move"));

      //
      // END captures ...
      //

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
            locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Threaten Pawn"));
            continue;
          }
          bool attackedPieceIsMinor = ApmwCore.getInstance().minors.Contains(attackedPiece.PieceType);
          if (attackedPieceIsMinor)
            locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Threaten Minor"));
          bool attackedPieceIsMajor = ApmwCore.getInstance().majors.Contains(attackedPiece.PieceType);
          if (attackedPieceIsMajor)
            locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Threaten Major"));
          bool attackedPieceIsQueen = ApmwCore.getInstance().queens.Contains(attackedPiece.PieceType);
          if (attackedPieceIsQueen)
            locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Threaten Queen"));
          bool attackedPieceIsKing = ApmwCore.getInstance().kings[0] == attackedPiece.PieceType;
          if (attackedPieceIsKing)
            locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Threaten King"));

          // For each piece attacking {square}, note it found a target, and check if it's attacking elsewhere
          for (int i = 0; i < attackers.Count; i++)
          {
            // This doesn't calculate whether a single move could defend both pieces.
            // Imagine a rook, blocked by a pawn offensively, sliding to a defensive location between two pieces
            // - or a "discovered defend" where two different pieces are used to defend the forked pieces.
            // We have to add "true forks" at each step, because we count the target, not just the source
            // There are conceivable moves which can protect both pieces but those are SO complicated, dude
            // And on the other hand, forked pieces defending each other might still be a fork!
            // A King protected by a Queen is not defended...
            bool isTrueFork =
              !match.Game.IsSquareAttacked(attackers[i].Square, humanPlayer ^ 1) && // will live to attack
              (
                // TODO: attacker has less value than a queen
                attackedPiece.PieceType.MidgameValue >= (attackers[i].MidgameValue + 100) || // recapture still loses material
                attackedPieceIsKing || // no king can be defended
                !match.Game.IsSquareAttacked(square, humanPlayer ^ 1) // not defended
              );
            if (isTrueFork)
            {
              if (!trueForkers.ContainsKey(attackers[i]))
                trueForkers[attackers[i]] = 0;
              trueForkers[attackers[i]]++;
            }

            // This is used to determine if a fork is royal.
            if (attackedPieceIsKing)
              kingAttacked[attackers[i]] = (true, isTrueFork);
            if (attackedPieceIsQueen)
              queenAttacked[attackers[i]] = (true, isTrueFork);

            if (!forkers.ContainsKey(attackers[i]))
              forkers[attackers[i]] = 0;
            if (++forkers[attackers[i]] > 1)
            {
              locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Fork, Sacrificial"));
              if (trueForkers.ContainsKey(attackers[i]) && trueForkers[attackers[i]] > 1)
                locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Fork, True"));
              if (forkers[attackers[i]] > 2)
              {
                locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Fork, Sacrificial Triple"));
                if (trueForkers.ContainsKey(attackers[i]) && trueForkers[attackers[i]] > 2)
                  locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Fork, True Triple"));
              }
              if (kingAttacked.ContainsKey(attackers[i]) && queenAttacked.ContainsKey(attackers[i]))
                if (kingAttacked[attackers[i]].Item1 && queenAttacked[attackers[i]].Item1)
                {
                  locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Fork, Sacrificial Royal"));
                  if (kingAttacked[attackers[i]].Item2 && queenAttacked[attackers[i]].Item2)
                    locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Fork, True Royal"));
                }
            }
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
            locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Pin"));
            // all pieces matter?? I don't know ... this only detects pins to king...
            locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Skewer"));
          }
          */
        }
      }

      //
      // END threats ...
      //

      if (locations.Count > 0 && !DeathlinkedMatches.Contains(match))
        new Task(() => LocationCheckHelper.CompleteLocationChecks(locations.ToArray())).Start();

      UpdateMoveState(info);
    }

    protected void UpdateMoveState(MoveInfo info)
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
      LocationCheckHelper.CompleteLocationChecks(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Checkmate Minima"));
      if (ApmwCore.getInstance().isGrand && match.Game.GameAttribute.GameName == NAME_OF_GRAND_ARCHIPELAGO_GAME_ATTRIBUTE)
      {
        LocationCheckHelper.CompleteLocationChecks(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Checkmate Maxima"));
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
      if (Initialized && match != null &&
          match.Game.GameAttribute.GameName != NAME_OF_ARCHIPELAGO_GAME_ATTRIBUTE &&
          match.Game.GameAttribute.GameName != NAME_OF_GRAND_ARCHIPELAGO_GAME_ATTRIBUTE)
        throw new InvalidOperationException("Please disconnect from Archipelago when using other ChessV features");
      return Initialized;
    }
  }
}
