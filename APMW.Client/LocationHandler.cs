﻿using Archipelago.MultiClient.Net;
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
    }

    public ILocationCheckHelper LocationCheckHelper { get; private set; }

    public bool Initialized { get; private set; }
    private StartedEventHandler seHandler;
    private Action<Match> feHandler;
    private Action<MoveInfo> mpHandler;
    private Action victory;
    private Action<string> deathlink;
    private Match match;
    public HashSet<Match> DeathlinkedMatches = new HashSet<Match>();
    private int humanPlayer;
    private int capturedPieces;
    private int capturedPawns;

    private Dictionary<int, int> currentSquaresToOriginalSquares = new Dictionary<int, int>();

    public void StartMatch(Match match)
    {
      if (!Initialized)
        throw new InvalidOperationException("LocationHandler has not been initialized");
      this.match = match;
      //match.Game.MovePlayed += (move) => this.HandleMove(move);
      humanPlayer = this.match.GetPlayer(0).IsHuman ? 0 : 1;
      // TODO(chesslogic): why does this continue to increment between games?
      capturedPawns = 0;
      capturedPieces = 0;
      currentSquaresToOriginalSquares = new Dictionary<int, int>();

      this.match.Finished += HandleMatch;
    }

    public void EndMatch()
    {
      if (!Initialized)
        throw new InvalidOperationException("LocationHandler has not been initialized");
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

    public void HandleMove(MoveInfo info)
    {
      if (!Initialized)
        throw new InvalidOperationException("LocationHandler has not been initialized");
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
        if (match.Game.GameTurnNumber <= 4 &&
          match.Game.Board.GetFile(info.ToSquare) == 4 &&
          (match.Game.Board.GetRank(info.ToSquare) == 1 || match.Game.Board.GetRank(info.ToSquare) == 6))
        {
          locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Bongcloud Once"));
        }
        // check if move is to A file
        if (match.Game.Board.GetFile(info.ToSquare) == 0)
        {
          locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Bongcloud A File"));
        }
        // check if move is to distant rank
        if ((info.Player == 1 && match.Game.Board.GetRank(info.ToSquare) == 0) ||
          (info.Player == 0 && match.Game.Board.GetRank(info.ToSquare) == 7))
        {
          // TODO(chesslogic): info.ToSquare probably isn't based on Board.PlayerSquare (used for PST eval)
          // TODO(chesslogic): ... but if it is, just check info.GetRank==7, ignore info.Player
          locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Bongcloud Promotion"));
        }
        // check if move is to center
        if ((match.Game.Board.GetFile(info.ToSquare) == 3 || match.Game.Board.GetFile(info.ToSquare) == 4) &&
          (match.Game.Board.GetRank(info.ToSquare) == 3 || match.Game.Board.GetRank(info.ToSquare) == 4))
        {
          locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Bongcloud Center"));
        }

        if (info.MoveType.HasFlag(MoveType.Castling))
        {
          if (info.FromSquare > info.ToSquare)
          {
            locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "O-O Castle"));
          }
          else
          {
            locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "O-O-O Castle"));
          }
        }
      }

      //
      // END various king moves ...
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
          locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Bongcloud Capture"));
        }
        /*
        if (info.PieceCaptured.PieceType.Name.Equals("King"))
        {
          locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Checkmate Maxima"));
          // TODO(chesslogic): count # of pieces and pawns, emit corresponding checkmate
        }
        */

        // handle specific piece
        int originalSquare = info.ToSquare;
        if (currentSquaresToOriginalSquares.ContainsKey(info.ToSquare))
          originalSquare = currentSquaresToOriginalSquares[info.ToSquare];
        int originalFile = match.Game.Board.GetFile(originalSquare);
        int originalRank = match.Game.Board.GetRank(originalSquare);
        string fileNotation = match.Game.Board.GetFileNotation(originalFile);
        fileNotation = fileNotation.ToUpper();
        bool isPiece = originalRank == 0 || originalRank == 7;
        //bool isPiece = !ApmwCore.getInstance().pawns.Contains(info.PieceCaptured.PieceType);
        string locationName;
        if (isPiece)
          locationName = "Capture Piece " + fileNotation;
        else
          locationName = "Capture Pawn " + fileNotation;
        locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", locationName));

        // handle piece sequence
        int captures;
        if (isPiece)
          captures = ++capturedPieces;
        else
          captures = ++capturedPawns;
        if (captures > 1)
        {
          if (isPiece)
          {
            locationName = "Capture " + captures + " Pieces";
            if (capturedPawns >= captures)
            {
              locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", locationName));
              locationName = "Capture " + captures + " Of Each";
            }
          }
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
          if (capturedPieces >= 7 && capturedPawns >= 8)
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
            bool isTrueFork = !match.Game.IsSquareAttacked(attackers[i].Square, humanPlayer ^ 1) &&
              !(attackedPieceIsKing || match.Game.IsSquareAttacked(square, humanPlayer ^ 1));
            if (attackedPieceIsKing)
              kingAttacked[attackers[i]] = (true, isTrueFork);
            if (attackedPieceIsQueen)
              queenAttacked[attackers[i]] = (true, isTrueFork);

            if (!forkers.ContainsKey(attackers[i]))
              forkers[attackers[i]] = 0;
            if (++forkers[attackers[i]] > 1)
            {
              locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Fork, Sacrificial"));

              // We have to add "true forks" at each step, because we count the target, not just the source
              // There are conceivable moves which can protect both pieces but those are SO complicated, dude
              // And on the other hand, forked pieces defending each other might still be a fork!
              // A King protected by a Queen is not defended...
              if (isTrueFork)
              {
                if (!trueForkers.ContainsKey(attackers[i]))
                  trueForkers[attackers[i]] = 0;
                trueForkers[attackers[i]]++;
              }
              if (trueForkers.ContainsKey(attackers[i]) && trueForkers[attackers[i]] > 2)
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
      // update original positions
      if (currentSquaresToOriginalSquares.ContainsKey(info.FromSquare))
        currentSquaresToOriginalSquares[info.ToSquare] = currentSquaresToOriginalSquares[info.FromSquare];
      else
        currentSquaresToOriginalSquares[info.ToSquare] = info.FromSquare;
    }

    public void HandleMatch(Match match)
    {
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
      LocationCheckHelper.CompleteLocationChecks(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Checkmate Maxima"));
      var statusUpdatePacket = new StatusUpdatePacket();
      statusUpdatePacket.Status = ArchipelagoClientState.ClientGoal;
      session.Socket.SendPacket(statusUpdatePacket);
    }

    public void Deathlink(ArchipelagoSession session, string reason = null)
    {
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
  }
}
