/***************************************************************************

                                 ChessV

                  COPYRIGHT (C) 2012-2019 BY GREG STRONG

This file is part of ChessV.  ChessV is free software; you can redistribute
it and/or modify it under the terms of the GNU General Public License as 
published by the Free Software Foundation, either version 3 of the License, 
or (at your option) any later version.

ChessV is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or 
FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for 
more details; the file 'COPYING' contains the License text, but if for
some reason you need a copy, please visit <http://www.gnu.org/licenses/>.

****************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;

namespace ChessV
{
  public delegate bool CustomMoveGenerationHandler(PieceType pieceType, Piece piece, MoveList moveList, bool capturesOnly);

  public class PieceType : ExObject
  {
    // *** CONSTANTS *** //

    #region Constants
    const int MAX_MOVE_CAPABILITIES = 32;
    #endregion


    // *** PUBLIC PROPERTIES *** //

    #region Public Properties
    public Game Game { get; private set; }
    public Board Board { get; private set; }

    public string InternalName { get; private set; }
    public virtual string Name { get; set; }
    public string[] Notation { get; protected set; }
    public string[] NotationClean { get; protected set; }
    public List<string> ImagePreferenceList { get; protected set; }
    public string PreferredImage { get; set; }
    public string FallbackImage { get; set; }
    public bool SimpleMoveGeneration { get; protected set; }
    public bool HasMovesWithPaths { get; protected set; }
    public bool HasMovesWithConditionalLocation { get; protected set; }
    public bool IsPawn { get; protected set; }

    public bool Enabled { get; set; }

    public int TypeNumber;
    public int[] AttackRangePerDirection;
    public int[] CannonAttackRangePerDirection;

    public int MidgameValue { get; set; }
    public int EndgameValue { get; set; }

    public bool IsSliced { get; protected set; }
    public int NumSlices { get; private set; }
    public int[] SliceLookup { get; private set; }

    public CustomMoveGenerationHandler CustomMoveGenerator { get; set; }

    public int NumMoveCapabilities
    { get { return nMoveCapabilities; } }

    //	variables in calculating the piece-square-tables
    public int PSTMidgameInSmallCenter { get; set; }
    public int PSTMidgameInLargeCenter { get; set; }
    public int PSTMidgameSmallCenterAttacks { get; set; }
    public int PSTMidgameLargeCenterAttacks { get; set; }
    public int PSTMidgameForwardness { get; set; }
    public int PSTMidgameGlobalOffset { get; set; }
    public int PSTEndgameInSmallCenter { get; set; }
    public int PSTEndgameInLargeCenter { get; set; }
    public int PSTEndgameSmallCenterAttacks { get; set; }
    public int PSTEndgameLargeCenterAttacks { get; set; }
    public int PSTEndgameForwardness { get; set; }
    public int PSTEndgameGlobalOffset { get; set; }
    public Double AverageMobility { get; set; }
    public Double AverageDirectionsAttacked { get; set; }
    public Double AverageSafeChecks { get; set; }
    #endregion


    // *** CONSTRUCTION *** //

    // TEST ONLY
    public PieceType() { }

    #region Constructor
    protected PieceType(string internalName, string name, string notation, int midgameValue, int endgameValue, string preferredImageName = null)
    {
      InternalName = internalName;
      Name = name;
      Notation = new string[2];
      NotationClean = new string[2];
      if (notation != null)
        SetNotation(notation);
      ImagePreferenceList = new List<string>();
      ImagePreferenceList.Add(internalName);
      if (internalName != name)
        ImagePreferenceList.Add(name);
      PreferredImage = preferredImageName;
      SimpleMoveGeneration = true;
      HasMovesWithPaths = false;
      HasMovesWithConditionalLocation = false;
      IsPawn = false;
      Enabled = true;
      IsSliced = true;
      moveCapabilities = new MoveCapability[MAX_MOVE_CAPABILITIES];
      nMoveCapabilities = 0;
      AttackRangePerDirection = new int[Game.MAX_DIRECTIONS];
      CannonAttackRangePerDirection = new int[Game.MAX_DIRECTIONS];
      MidgameValue = midgameValue;
      EndgameValue = endgameValue;
      CustomMoveGenerator = null;

      PSTMidgameInSmallCenter = 3;
      PSTMidgameInLargeCenter = 3;
      PSTMidgameSmallCenterAttacks = 1;
      PSTMidgameLargeCenterAttacks = 1;
      PSTMidgameForwardness = 0;
      PSTMidgameGlobalOffset = -15;
      PSTEndgameInSmallCenter = 3;
      PSTEndgameInLargeCenter = 3;
      PSTEndgameSmallCenterAttacks = 1;
      PSTEndgameLargeCenterAttacks = 1;
      PSTEndgameForwardness = 0;
      PSTEndgameGlobalOffset = -15;
    }
    #endregion


    // *** INITIALIZATION *** //

    #region Initialize
    public virtual void Initialize(Game game)
    {
      Game = game;
      Board = game.Board;

      //	add extra image names to image list
      if (PreferredImage != null)
        ImagePreferenceList.Insert(0, PreferredImage);
      if (FallbackImage != null)
        ImagePreferenceList.Add(FallbackImage);

      //	find this piece type in the Game's index
      TypeNumber = game.GetPieceTypeNumber(this);

      //	initialize all move capabilities
      for (int x = 0; x < nMoveCapabilities; x++)
      {
        moveCapabilities[x].Initialize(game);
        //	find the number of this direction in the Game's index of Directions
        Direction direction = moveCapabilities[x].Direction;
        Direction[] gameDirections;
        int nGameDirections = game.GetDirections(out gameDirections);
        for (int y = 0; y < nGameDirections; y++)
        {
          Direction gameDirection = gameDirections[y];
          if (direction == gameDirection)
          {
            moveCapabilities[x].NDirection = y;
            break;
          }
        }
        //	if this move has a move path, initialize all the 
        //	direction numbers for the directions of the individual 
        //	steps down the path
        if (moveCapabilities[x].PathInfo != null)
        {
          SimpleMoveGeneration = false;
          foreach (List<Direction> dirPath in moveCapabilities[x].PathInfo.PathDirections)
          {
            List<int> path = new List<int>();
            foreach (Direction dir in dirPath)
            {
              for (int y = 0; y < nGameDirections; y++)
              {
                Direction gameDirection = gameDirections[y];
                if (dir == gameDirection)
                {
                  path.Add(y);
                  break;
                }
              }
            }
            moveCapabilities[x].PathInfo.PathNDirections.Add(path);
          }
        }
        //	update AttackRangePerDirection
        if (moveCapabilities[x].CanCapture &&
          AttackRangePerDirection[moveCapabilities[x].NDirection] < moveCapabilities[x].MaxSteps)
          AttackRangePerDirection[moveCapabilities[x].NDirection] = moveCapabilities[x].MaxSteps;
        //	update CannonAttackRangePerDirection
        if ((moveCapabilities[x].SpecialAttacks & SpecialAttacks.CannonCapture) != 0 &&
          CannonAttackRangePerDirection[moveCapabilities[x].NDirection] < moveCapabilities[x].MaxSteps)
          CannonAttackRangePerDirection[moveCapabilities[x].NDirection] = moveCapabilities[x].MaxSteps;
      }

      //	Calculate slices.  Slices are sets of squares that cannot be reached from 
      //	other squares by this piece ("colorbound").  For most pieces, the entire 
      //	board is one slice.  For a bishop, the board has two slices.  A dabbabah has four.
      NumSlices = 0;
      SliceLookup = new int[Board.NumSquaresExtended];
      //	For pieces that are designated not sliced, we skip all of this and consider the whle
      //	board one slice.  For example, pawns on the grounds that a pawn can promote and then see 
      //	everything.  Also, most of the functionality this drives (two-bishop bonus, etc) would 
      //	be harmed by considering a pawn to be colorbound.
      for (int square = 0; square < Board.NumSquaresExtended; square++)
        SliceLookup[square] = IsSliced ? (square < Board.NumSquares ? -1 : 0) : 0;
      for (int square = 0; square < Board.NumSquares; square++)
        if (SliceLookup[square] == -1)
          findSquare(square, NumSlices++);
      if (NumSlices == 0)
        NumSlices = 1;

      //	initialize hash keys
      hashKeyIndex = new int[game.NumPlayers];
      pawnHashKeyIndex = new int[game.NumPlayers];
      materialHashKeyIndex = new int[game.NumPlayers, NumSlices];
      for (int player = 0; player < game.NumPlayers; player++)
      {
        hashKeyIndex[player] = game.HashKeys.TakeKeys(game.Board.NumSquaresExtended);
        pawnHashKeyIndex[player] = 0;
        for (int slice = 0; slice < NumSlices; slice++)
          materialHashKeyIndex[player, slice] = game.HashKeys.TakeMaterialKeys(32);
      }

      //	initialize PST
      InitializePST(game.Variation);
    }
    #endregion

    #region InitializePST
    public virtual void InitializePST(int variation)
    {
      int zPSTMidgameInSmallCenter = PSTMidgameInSmallCenter;
      int zPSTMidgameInLargeCenter = PSTMidgameInLargeCenter;
      int zPSTMidgameSmallCenterAttacks = PSTMidgameSmallCenterAttacks;
      int zPSTMidgameLargeCenterAttacks = PSTMidgameLargeCenterAttacks;
      int zPSTMidgameForwardness = PSTMidgameForwardness;
      int zPSTMidgameGlobalOffset = PSTMidgameGlobalOffset;
      int zPSTEndgameInSmallCenter = PSTEndgameInSmallCenter;
      int zPSTEndgameInLargeCenter = PSTEndgameInLargeCenter;
      int zPSTEndgameSmallCenterAttacks = PSTEndgameSmallCenterAttacks;
      int zPSTEndgameLargeCenterAttacks = PSTEndgameLargeCenterAttacks;
      int zPSTEndgameForwardness = PSTEndgameForwardness;
      int zPSTEndgameGlobalOffset = PSTEndgameGlobalOffset;

      //	Perform random adjustments to PST components
      if (variation > 0)
      {
        int[] randomAdjustments = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        if (variation == 1)
          randomAdjustments = new int[] { -1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1 };
        else if (variation == 2)
          randomAdjustments = new int[] { -2, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 2 };
        else if (variation == 3)
          randomAdjustments = new int[] { -2, -2, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 2, 2 };
        if (PSTMidgameInSmallCenter == PSTEndgameInSmallCenter)
        {
          int adj = randomAdjustments[Program.Random.Next(16)];
          zPSTMidgameInSmallCenter = Math.Max(PSTMidgameInSmallCenter + adj, 0);
          zPSTEndgameInSmallCenter = Math.Max(PSTEndgameInSmallCenter + adj, 0);
        }
        else
        {
          zPSTMidgameInSmallCenter = Math.Max(PSTMidgameInSmallCenter + randomAdjustments[Program.Random.Next(16)], 0);
          zPSTEndgameInSmallCenter = Math.Max(PSTEndgameInSmallCenter + randomAdjustments[Program.Random.Next(16)], 0);
        }
        if (PSTMidgameInLargeCenter == PSTEndgameInLargeCenter)
        {
          int adj = randomAdjustments[Program.Random.Next(16)];
          zPSTMidgameInLargeCenter = Math.Max(PSTMidgameInLargeCenter + adj, 0);
          zPSTEndgameInLargeCenter = Math.Max(PSTEndgameInLargeCenter + adj, 0);
        }
        else
        {
          zPSTMidgameInLargeCenter = Math.Max(PSTMidgameInLargeCenter + randomAdjustments[Program.Random.Next(16)], 0);
          zPSTEndgameInLargeCenter = Math.Max(PSTEndgameInLargeCenter + randomAdjustments[Program.Random.Next(16)], 0);
        }
        if (PSTMidgameSmallCenterAttacks == PSTEndgameSmallCenterAttacks)
        {
          int adj = randomAdjustments[Program.Random.Next(16)];
          zPSTMidgameSmallCenterAttacks = Math.Max(PSTMidgameSmallCenterAttacks + adj, 0);
          zPSTEndgameSmallCenterAttacks = Math.Max(PSTEndgameSmallCenterAttacks + adj, 0);
        }
        else
        {
          zPSTMidgameSmallCenterAttacks = Math.Max(PSTMidgameSmallCenterAttacks + randomAdjustments[Program.Random.Next(16)], 0);
          zPSTEndgameSmallCenterAttacks = Math.Max(PSTEndgameSmallCenterAttacks + randomAdjustments[Program.Random.Next(16)], 0);
        }
        if (PSTMidgameLargeCenterAttacks == PSTEndgameLargeCenterAttacks)
        {
          int adj = randomAdjustments[Program.Random.Next(16)];
          zPSTMidgameLargeCenterAttacks = Math.Max(PSTMidgameLargeCenterAttacks + adj, 0);
          zPSTEndgameLargeCenterAttacks = Math.Max(PSTEndgameLargeCenterAttacks + adj, 0);
        }
        else
        {
          zPSTMidgameLargeCenterAttacks = Math.Max(PSTMidgameLargeCenterAttacks + randomAdjustments[Program.Random.Next(16)], 0);
          zPSTEndgameLargeCenterAttacks = Math.Max(PSTEndgameLargeCenterAttacks + randomAdjustments[Program.Random.Next(16)], 0);
        }
        if (PSTMidgameGlobalOffset == PSTEndgameGlobalOffset)
        {
          int adj = randomAdjustments[Program.Random.Next(16)];
          zPSTMidgameGlobalOffset += adj;
          zPSTEndgameGlobalOffset += adj;
        }
        else
        {
          zPSTMidgameGlobalOffset += randomAdjustments[Program.Random.Next(16)];
          zPSTEndgameGlobalOffset += randomAdjustments[Program.Random.Next(16)];
        }
        if (PSTMidgameForwardness > 0 && PSTEndgameForwardness > 0)
        {
          if (PSTMidgameForwardness == PSTEndgameForwardness)
          {
            int adj = randomAdjustments[Program.Random.Next(16)];
            zPSTMidgameForwardness = Math.Max(PSTMidgameForwardness + adj, 1);
            zPSTEndgameForwardness = Math.Max(PSTEndgameForwardness + adj, 1);
          }
          else
          {
            zPSTMidgameForwardness = Math.Max(PSTMidgameForwardness + randomAdjustments[Program.Random.Next(16)], 1);
            zPSTEndgameForwardness = Math.Max(PSTEndgameForwardness + randomAdjustments[Program.Random.Next(16)], 1);
          }
        }
      }

      //	Calculate some values that will be later used to build the PST
      pstSmallCenterAttacks = new int[Board.NumSquares];
      pstLargeCenterAttacks = new int[Board.NumSquares];
      bool[] reachableSquares = new bool[Board.NumSquares];
      for (int sq = 0; sq < Board.NumSquares; sq++)
      {
        pstSmallCenterAttacks[sq] = 0;
        pstLargeCenterAttacks[sq] = 0;

        //	clear out reachableSquares matrix
        for (int y = 0; y < Board.NumSquares; y++)
          reachableSquares[y] = false;

        GetEmptyBoardMobility(Game, 0, sq, reachableSquares);
        for (int y = 0; y < Board.NumSquares; y++)
          if (reachableSquares[y])
          {
            pstSmallCenterAttacks[sq] += Board.InSmallCenter(y);
            pstLargeCenterAttacks[sq] += Board.InLargeCenter(y);
          }
      }

      //	Initialize PST
      pstMidgame = new int[Board.NumSquaresExtended];
      pstEndgame = new int[Board.NumSquaresExtended];
      for (int sq = 0; sq < Board.NumSquares; sq++)
      {
        pstMidgame[sq] = zPSTMidgameGlobalOffset +
          zPSTMidgameInLargeCenter * Board.InLargeCenter(sq) +
          zPSTMidgameInSmallCenter * Board.InSmallCenter(sq) +
          zPSTMidgameLargeCenterAttacks * pstLargeCenterAttacks[sq] +
          zPSTMidgameSmallCenterAttacks * pstSmallCenterAttacks[sq] +
          zPSTMidgameForwardness * Board.Forwardness(sq);
        pstEndgame[sq] = zPSTMidgameGlobalOffset +
          zPSTEndgameInLargeCenter * Board.InLargeCenter(sq) +
          zPSTEndgameInSmallCenter * Board.InSmallCenter(sq) +
          zPSTEndgameLargeCenterAttacks * pstLargeCenterAttacks[sq] +
          zPSTEndgameSmallCenterAttacks * pstSmallCenterAttacks[sq] +
          zPSTEndgameForwardness * Board.Forwardness(sq);
      }
      for (int sq = Board.NumSquares; sq < Board.NumSquaresExtended; sq++)
      {
        // TODO: don't hard-code these numbers
        pstMidgame[sq] = 50;
        pstEndgame[sq] = 0;
      }
    }
    #endregion

    #region SetNotation
    public void SetNotation(string notation)
    {
      if (notation.IndexOf('/') > 0)
      {
        //	for piece types that have different notation for each player 
        //	(aside from the standard upper-case, lower-case), we specify 
        //	the two notations with a / between.
        Notation[0] = notation.Substring(0, notation.IndexOf('/'));
        Notation[1] = notation.Substring(notation.IndexOf('/') + 1);
      }
      else
      {
        Notation[0] = notation.ToUpper();
        Notation[1] = notation.ToLower();
      }
      //	NotationClean is the Notation but with any leading underscore stripped.
      //	ChessV internally needs a leading underscore for two-letter notations sometimes 
      //	to resolve ambiguities, but the _ is not displayed to the user, so when 
      //	displaying with show NotationClean instead.
      NotationClean[0] = Notation[0][0] == '_' ? Notation[0].Substring(1) : Notation[0];
      NotationClean[1] = Notation[1][0] == '_' ? Notation[1].Substring(1) : Notation[1];
    }
    #endregion


    // *** MOVE CAPABILITIES *** //

    #region Getting Move Capabilities
    public int GetMoveCapabilities(out MoveCapability[] moves)
    { moves = moveCapabilities; return nMoveCapabilities; }

    public MoveCapability FindMove(Direction dir)
    {
      for (int x = 0; x < nMoveCapabilities; x++)
        if (moveCapabilities[x].Direction == dir)
          return moveCapabilities[x];
      return null;
    }
    #endregion

    #region Adding Move Capabilities
    public static void AddMoves(PieceType type)
    { }

    public MoveCapability Step(Direction direction)
    { moveCapabilities[nMoveCapabilities] = MoveCapability.Step(direction); return moveCapabilities[nMoveCapabilities++]; }

    public MoveCapability Slide(Direction direction)
    { moveCapabilities[nMoveCapabilities] = MoveCapability.Slide(direction); return moveCapabilities[nMoveCapabilities++]; }

    public MoveCapability Slide(Direction direction, int maxSteps)
    { moveCapabilities[nMoveCapabilities] = MoveCapability.Slide(direction, maxSteps); return moveCapabilities[nMoveCapabilities++]; }

    public MoveCapability StepMoveOnly(Direction direction)
    { moveCapabilities[nMoveCapabilities] = MoveCapability.StepMoveOnly(direction); return moveCapabilities[nMoveCapabilities++]; }

    public MoveCapability SlideMoveOnly(Direction direction)
    { moveCapabilities[nMoveCapabilities] = MoveCapability.SlideMoveOnly(direction); return moveCapabilities[nMoveCapabilities++]; }

    public MoveCapability StepCaptureOnly(Direction direction)
    { moveCapabilities[nMoveCapabilities] = MoveCapability.StepCaptureOnly(direction); return moveCapabilities[nMoveCapabilities++]; }

    public MoveCapability SlideCaptureOnly(Direction direction)
    { moveCapabilities[nMoveCapabilities] = MoveCapability.SlideCaptureOnly(direction); return moveCapabilities[nMoveCapabilities++]; }

    public MoveCapability CannonMove(Direction direction)
    {
      SimpleMoveGeneration = false;
      moveCapabilities[nMoveCapabilities] = MoveCapability.CannonMove(direction);
      return moveCapabilities[nMoveCapabilities++];
    }

    public MoveCapability RifleCapture(Direction direction, int maxSpaces)
    {
      SimpleMoveGeneration = false;
      moveCapabilities[nMoveCapabilities] = MoveCapability.RifleCapture(direction, maxSpaces);
      return moveCapabilities[nMoveCapabilities++];
    }

    public MoveCapability AddMoveCapability(MoveCapability moveCapability)
    {
      moveCapabilities[nMoveCapabilities] = moveCapability;
      if (moveCapability.PathInfo != null)
        HasMovesWithPaths = true;
      if (moveCapability.Condition != null)
        HasMovesWithConditionalLocation = true;
      return moveCapabilities[nMoveCapabilities++];
    }

    public void AddMovesOf(PieceType other)
    {
      MoveCapability[] otherMoves;
      int nmoves = other.GetMoveCapabilities(out otherMoves);
      for (int x = 0; x < nmoves; x++)
        AddMoveCapability(otherMoves[x]);
    }

    public void AddMovesOf(Type otherType)
    {
      var method = otherType.GetMethod("AddMoves", BindingFlags.Static | BindingFlags.Public);
      if (method == null)
      {
        if (otherType.IsSubclassOf(typeof(PieceType)))
        {
          //	construct object of this type and call the other AddMovesOf
          ConstructorInfo ci = otherType.GetConstructor(new Type[] { typeof(string), typeof(string), typeof(int), typeof(int), typeof(string) });
          if (ci == null)
            throw new Exception("Error in PieceType.AddMovesOf - cannot construct piece type with expected constructor arguments");
          PieceType piecetype = (PieceType)ci.Invoke(new object[] { null, null, 0, 0, null });
          MethodInfo mi = piecetype.GetType().GetMethod("AddMoves", BindingFlags.Static | BindingFlags.Public);
          if (mi != null)
            mi.Invoke(piecetype, null);
          else
          {
            //	ok, if we are here that means this is not a native C# piece type but a 
            //	dynamic one created in our scripting language, so ugliness must follow ...
            if (piecetype.GetType().GetMember("FunctionCodeLookup").Length > 0)
            {
              FieldInfo environmentField = (Game != null ? Game.GetType().GetField("Environment") : GetType().GetField("Environment"));
              FieldInfo pieceEnvironmentField = piecetype.GetType().GetField("Environment");
              pieceEnvironmentField.SetValue(piecetype, environmentField.GetValue(Game != null ? (object)Game : this));
              MethodInfo mi2 = piecetype.GetType().GetMethod("AddMoves");
              if (mi2 != null)
                mi2.Invoke(piecetype, null);
            }
          }
          AddMovesOf(piecetype);
          return;
        }
        else
          throw new Exception("Error in PieceType.AddMovesOf - provided object is not a PieceType");
      }
      method.Invoke(null, new object[] { this });
    }
    #endregion

    #region Clearing Move Capabilities
    public void RemoveMoveCapability(Direction direction)
    {
      for (int x = 0; x < nMoveCapabilities; x++)
        if (moveCapabilities[x].Direction == direction)
        {
          for (int y = x; y < nMoveCapabilities - 1; y++)
            moveCapabilities[y] = moveCapabilities[y + 1];
          nMoveCapabilities--;
          break;
        }
    }

    public void ResetMoveCapabilities()
    {
      nMoveCapabilities = 0;
      SimpleMoveGeneration = true;
      HasMovesWithPaths = false;
      HasMovesWithConditionalLocation = false;
    }
    #endregion


    // *** OPERATIONS *** //

    #region GetEmptyBoardMobility
    public void GetEmptyBoardMobility(Game game, int player, int square, bool[] boardSquares)
    {
      for (int x = 0; x < nMoveCapabilities; x++)
      {
        MoveCapability move = moveCapabilities[x];
        if (!move.MustCapture && (move.ConditionalBySquare == null || move.ConditionalBySquare[player][square]))
        {
          int steps = 1;
          int nextSquare = game.Board.NextSquare(game.PlayerDirection(player, move.NDirection), square);
          while (nextSquare >= 0 && steps <= move.MaxSteps)
          {
            if (steps >= move.MinSteps)
              boardSquares[nextSquare] = true;
            steps++;
            nextSquare = game.Board.NextSquare(game.PlayerDirection(player, move.NDirection), nextSquare);
          }
        }
      }
    }
    #endregion

    #region CalculateMobilityStatistics
    public double CalculateMobilityStatistics(Game game, double density)
    {
      Board board = game.Board;
      double[] mobilityPerSquare = new double[board.NumSquares];
      double[] directionsPerSquare = new double[board.NumSquares];
      double[] safeChecksPerSquare = new double[board.NumSquares];
      for (int square = 0; square < board.NumSquares; square++)
      {
        int rank = board.GetRank(square);
        int file = board.GetFile(square);
        double mobility = 0.0;
        double directions = 0.0;
        double safeChecks = 0.0;
        for (int x = 0; x < nMoveCapabilities; x++)
        {
          MoveCapability move = moveCapabilities[x];
          if (move.CanCapture && (move.ConditionalBySquare == null || move.ConditionalBySquare[0][square]))
          {
            double directionalMobility = 0.0;
            double currentWeight = 1.0;
            int steps = 1;
            int nextSquare = game.Board.NextSquare(move.NDirection, square);
            if (nextSquare >= 0)
              directions++;
            while (nextSquare >= 0 && steps <= move.MaxSteps)
            {
              if (steps >= move.MinSteps)
              {
                directionalMobility += currentWeight;
                currentWeight = currentWeight * density;
                //	is this a safe check?
                int currentRank = board.GetRank(nextSquare);
                int currentFile = board.GetFile(nextSquare);
                if (currentFile > file + 1 || currentFile < file - 1 ||
                  currentRank > rank + 1 || currentRank < rank - 1)
                  safeChecks++;
              }
              steps++;
              nextSquare = game.Board.NextSquare(move.NDirection, nextSquare);
            }
            mobility += directionalMobility;
          }
        }
        mobilityPerSquare[square] = mobility;
        directionsPerSquare[square] = directions;
        safeChecksPerSquare[square] = safeChecks;
      }
      //	calculate averages
      double totalMobility = 0.0;
      double totalDirections = 0.0;
      double totalSafeChecks = 0.0;
      for (int square = 0; square < board.NumSquares; square++)
      {
        totalMobility += mobilityPerSquare[square];
        totalDirections += directionsPerSquare[square];
        totalSafeChecks += safeChecksPerSquare[square];
      }
      AverageMobility = totalMobility / board.NumSquares;
      AverageDirectionsAttacked = totalDirections / board.NumSquares;
      AverageSafeChecks = totalSafeChecks / board.NumSquares;
      return AverageMobility;
    }
    #endregion

    public int[] GetMidgamePST()
    { return pstMidgame; }

    public int[] GetEndgamePST()
    { return pstEndgame; }

    public int GetMidgamePST(int square)
    { return pstMidgame[square]; }

    public int GetEndgamePST(int square)
    { return pstEndgame[square]; }

    public UInt64 GetHashKey(int player, int square)
    { return HashKeys.Keys[hashKeyIndex[player] + square]; }

    public UInt64 GetPawnHashKey(int player, int square)
    { return HashKeys.Keys[pawnHashKeyIndex[player] + square]; }

    public UInt64 GetMaterialHashKey(int player, int nSlice, int nPieces)
    { return HashKeys.Keys[materialHashKeyIndex[player, nSlice] + nPieces]; }


    // *** HELPER FUNCTIONS *** //

    #region findSquare
    private void findSquare(int square, int slice)
    {
      SliceLookup[square] = slice;
      for (int x = 0; x < nMoveCapabilities; x++)
      {
        if (moveCapabilities[x].ConditionalBySquare == null ||
          moveCapabilities[x].ConditionalBySquare[0][square])
        {
          int nextSquare = Game.Board.NextSquare(moveCapabilities[x].NDirection, square);
          if (nextSquare >= 0 && SliceLookup[nextSquare] == -1)
          {
            if (moveCapabilities[x].MinSteps > 1)
              findSquare(nextSquare, slice, 1, moveCapabilities[x]);
            else
              findSquare(nextSquare, slice);
          }
        }
      }
    }

    private void findSquare(int square, int slice, int step, MoveCapability move)
    {
      if (step >= move.MinSteps && move.CanCapture)
        findSquare(square, slice);
      else if (move.MaxSteps <= step && (move.ConditionalBySquare == null || move.ConditionalBySquare[0][square]))
      {
        int nextSquare = Game.Board.NextSquare(move.NDirection, square);
        if (nextSquare >= 0 && SliceLookup[nextSquare] == -1)
          findSquare(nextSquare, slice, step + 1, move);
      }
    }
    #endregion


    // *** PROTECTED DATA *** //

    #region Protected Data
    protected MoveCapability[] moveCapabilities;
    protected int nMoveCapabilities;
    protected int[] hashKeyIndex;
    protected int[] pawnHashKeyIndex;
    protected int[,] materialHashKeyIndex;

    protected int[] pstSmallCenterAttacks;
    protected int[] pstLargeCenterAttacks;

    protected int[] pstMidgame;
    protected int[] pstEndgame;
    #endregion
  }
}
