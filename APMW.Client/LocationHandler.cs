﻿using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Helpers;
using ChessV;
using ChessV.Base;
using ChessV.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Archipelago.APChessV
{
  public class LocationHandler
  {
    public static LocationHandler _instance;

    public static LocationHandler GetInstance()
    {
      if (_instance == null)
        lock(typeof(LocationHandler))
          if (_instance == null)
            _instance = new LocationHandler();
      return _instance;
    }

    protected LocationHandler()
    {
      seHandler = (match) => this.StartMatch(match);
      ApmwCore.getInstance().StartedEventHandlers.Add(seHandler);
      mpHandler = (move) => this.HandleMove(move);
      ApmwCore.getInstance().NewMovePlayed.Add(mpHandler);
      Initialized = false;
    }

    public void Initialize(ILocationCheckHelper locationCheckHelper)
    {
      //if (Initialized)
      //  throw new InvalidOperationException("Cannot reinitialize LocationHandler");
      this.LocationCheckHelper = locationCheckHelper;
      Initialized = true;
    }

    public ILocationCheckHelper LocationCheckHelper { get; private set; }

    public bool Initialized { get; private set; }
    private StartedEventHandler seHandler;
    private FinishedEventHandler feHandler;
    private Action<MoveInfo> mpHandler;
    private ChessV.Match match;
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
      this.humanPlayer = this.match.GetPlayer(0).IsHuman ? 0 : 1;
      // TODO(chesslogic): why does this continue to increment between games?
      this.capturedPawns = 0;
      this.capturedPieces = 0;
      this.currentSquaresToOriginalSquares = new Dictionary<int, int>();

      this.match.Finished += HandleMatch;
    }

    public void EndMatch()
    {
      if (!Initialized)
        throw new InvalidOperationException("LocationHandler has not been initialized");
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
      if (match.Result != null)
        if (match.Result.Type == ResultType.Win)
          if (match.Result.Winner == this.humanPlayer)
            locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Checkmate Maxima"));

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
      if (pieceName.Equals("King"))
      {
        if (match.Game.GameTurnNumber <= 2 &&
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
      }

      //
      // END various king moves ...
      //

      //
      // START captures ...
      //

      // check if move is capture
      if ((info.MoveType & MoveType.CaptureProperty) != 0)
      {
        // handle king captures
        if (pieceName.Equals("King"))
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
        string fileNotation = match.Game.Board.GetFileNotation(originalFile);
        fileNotation = fileNotation.ToUpper();
        bool isPiece = !info.PieceCaptured.PieceType.Name.Contains("Pawn");
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
      Dictionary<Piece, bool> kingAttacked = new Dictionary<Piece, bool>();
      Dictionary<Piece, bool> queenAttacked = new Dictionary<Piece, bool>();
      for (int square = 0; square < match.Game.Board.NumSquares; square++)
      {
        List<Piece> attackers;
        if (match.Game.IsSquareAttacked(square, humanPlayer, out attackers))
        {
          var loc = match.Game.Board.SquareToLocation(square);
          Piece attackedPiece = match.Game.Board[square];


          if (attackedPiece == null || attackedPiece.Player == humanPlayer) { continue; }
          if (ApmwCore.getInstance().pawns.Contains(attackedPiece.PieceType))
          {
            locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Threaten Pawn"));
            continue;
          }
          if (ApmwCore.getInstance().minors.Contains(attackedPiece.PieceType))
          {
            locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Threaten Minor"));
          }
          if (ApmwCore.getInstance().majors.Contains(attackedPiece.PieceType))
          {
            locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Threaten Major"));
          }
          if (ApmwCore.getInstance().queens.Contains(attackedPiece.PieceType))
          {
            locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Threaten Queen"));
          }
          if (ApmwCore.getInstance().king == attackedPiece.PieceType)
          {
            locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Threaten King"));
          }

          // check if a single piece threatens two non-pawns, call it a "fork" and stick it done (that's a joke)
          for (int i = 0; i < attackers.Count; i++)
          {
            if (!forkers.ContainsKey(attackers[i]))
              forkers[attackers[i]] = 0;
            if (++forkers[attackers[i]] > 1)
            {
              locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Fork"));
              if (forkers[attackers[i]] > 2)
                locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Triple Fork"));
              if (ApmwCore.getInstance().king == attackedPiece.PieceType)
                kingAttacked[attackers[i]] = true;
              if (ApmwCore.getInstance().queens.Contains(attackedPiece.PieceType))
                queenAttacked[attackers[i]] = true;
              if (kingAttacked.ContainsKey(attackers[i]) && queenAttacked.ContainsKey(attackers[i]))
                if (kingAttacked[attackers[i]] && queenAttacked[attackers[i]])
                  locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Royal Fork"));
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

      if (locations.Count > 0)
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
        locations.Add(LocationCheckHelper.GetLocationIdFromName("ChecksMate", "Checkmate Maxima"));
      }

      if (locations.Count > 0)
      {
        new Task(() => LocationCheckHelper.CompleteLocationChecks(locations.ToArray())).Start();
      }
    }
  }
}