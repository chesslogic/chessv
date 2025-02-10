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

namespace ChessV
{
  public class Board
  {
    // *** CONSTANTS *** //

    #region Constants
    //	The maximum number of files that can be supported
    public const int MAX_FILES = 16;

    //	The maximum number of ranks that can be supported
    public const int MAX_RANKS = 16;

    //	The maximum number of squares that can be supported
    public const int MAX_SQUARES = MAX_FILES * MAX_RANKS;

    //	Internal indicator for movement matrices that a square is disconnected
    public const int NOT_CONNECTED = -1;
    #endregion


    // *** PROPERTIES *** //

    #region Properties
    //	The Game being played
    public Game Game { get; private set; }

    //	The number of files on the board(s)
    public int NumFiles { get; protected set; }

    //	The number of ranks on the board(s)
    public int NumRanks { get; protected set; }

    //	The number of squares on the board(s) excluding virtual (drop) squares
    public int NumSquares { get; protected set; }

    //	The total number of squares across all boards, and virtual squares, 
    //	such as the drop squares from which pieces drop in Pocket Knight
    public int NumSquaresExtended { get; protected set; }

    //	The number of "Directions" in which pieces move.  Every square offset is a 
    //	different direction.  A Rook attacks in four Directions.  A Bishop attacks 
    //	in four different Directions.  A Knight attacks in yet eight more Directions.  
    //	NOTE: The four moves of a Dababbah, (0, 2) leaper, are different than the 
    //	four directions of a Rook.  They might appear similar to us, but they are very 
    //	different.  A piece that blocks a rook-slide might not block a dababbah slide.  
    //	A piece that moves as a Rook can potentially fork four pieces.  A piece that 
    //	moves as a Dababbah can potentially fork four pieces.  A piece that moves as 
    //	a Rook or Dababbah can potentially fork eight.  Finally, understand that 
    //	for each Direction there is a big matrix for each square that indicates the 
    //	number of the next square in this Direction from the current square (or 
    //	NOT_CONNECTED (-1) if the next step takes us off the board.)  This architecture 
    //	is fundamental to how this universal engine handles move generation with 
    //	arbitrary pieces, so it is important to understand.
    public int NumberOfDirections { get; protected set; }

    //	Allow the Board class to disable Simple Move Generation.  This is because 
    //	cylindrical and circular boards, for example, would mess up SEE.  This should 
    //	be improved some day.
    public bool DisableSimpleMoveGeneration { get; protected set; }

    //	The Zobrist Hash Code that identifies this position for repetition detection, 
    //	transposition table lookup, etc.
    public UInt64 HashCode
    { get { return hashcode; } }

    //	The Hash Code that identifies this position of just the pawns (if this game has 
    //	Orthodox Chess Pawns) so that a transposition table of pawn positions can look up 
    //	the evaluation features of this pawn structure.  Since the pawns are slow-moving 
    //	pieces, this table is very helpful because even a small table has a very high 
    //	hit ratio and saves a lot of work.
    public UInt64 PawnHashCode { get; protected set; }

    //	The material hash code identifies the amount of material on the board (the 
    //	number of pieces of each type per player.)  It does not matter which squares 
    //	the pieces occupy except for colorbound ("sliced") pieces.  A light-squared 
    //	bishop is considered a different piece type form a dark-squared one.  This is 
    //	also true for pieces with more than two slices, such as a dababbah.
    public UInt64 MaterialHashCode { get; protected set; }

    //  indexer that returns or updates the contents of a square by number
    public Piece this[int square]
    {
      get
      { return squares[square]; }

      set
      {
        if (value == null)
          ClearSquare(square);
        else
          SetSquare(value, square);
      }
    }

    //  indexer that returns or updates the contents of a square by notation
    public Piece this[string notation]
    {
      get
      { return squares[Game.NotationToSquare(notation)]; }

      set
      {
        if (value == null)
          ClearSquare(Game.NotationToSquare(notation));
        else
          SetSquare(value, Game.NotationToSquare(notation));
      }
    }
    #endregion Properties


    // *** PUBLIC INQURIY FUNCTIONS *** //

    //	These functions inquire about the properties of the board and its contents, 
    //	as well as data related to how the pieces in the game being played can 
    //	move on this board (such as the IsSquareAttacked function.)

    //	These functions do not alter the board in any way.

    #region Public Inquiry Functions
    //	Lookup for the char notation of a given file
    public virtual string GetFileNotation(int nFile)
    { return fileNotations[nFile].ToString(); }

    //	Lookup for the char notation of a given rank
    public virtual string GetRankNotation(int nRank)
    { return rankNotations[nRank].ToString(); }

    //	Lookup for the notaiton of a square
    public virtual string GetDefaultSquareNotation(int square)
    { return GetFileNotation(GetFile(square)) + GetRankNotation(GetRank(square)); }

    //	Find the Rank for a given square
    public int GetRank(int square)
    { return rankBySquare[square]; }

    //	Find the File for a given square
    public virtual int GetFile(int square)
    { return fileBySquare[square]; }

    //	Get the distance (in King-steps) between the two squares
    public int GetDistance(int square1, int square2)
    { return distances[square1, square2]; }

    //	This is the all-important look-up matrix for how pieces move from square to square.  
    //	Each move capability is in a numbered direction.  This matrix gives the number of the 
    //	next square from the given square in the given direction (or -1, NOT_CONNECTED, if 
    //	we are at the end of the board in that direction.)  Moves between boards, or other 
    //	strangeness, is generally handled outside this matrix (e.g., Alice Chess)
    public int NextSquare(int nDirection, int square)
    { return nextStep[nDirection, square]; }

    //	This version of NextSquare is player-specific and adjusts the nDirection to the 
    //	appropriate player by translating the direction according to the Symmetry
    public int NextSquare(int nPlayer, int nDirection, int square)
    { return nextStep[Game.PlayerDirection(nPlayer, nDirection), square]; }

    //	This matrix translates the squares of the board between players.  For example, 
    //	the Chess Pawn double-space move can only happen with Pawns on the squares of the 
    //	second rank.  Only one set of squares is provided.  How then do we determine if 
    //	a pawn for the second player is on such a square?  We call this function, which is 
    //	based on a matrix of this information (flipSquare), which itself is based on the 
    //	Symmetry object specified for this Game.
    public int PlayerSquare(int player, int square)
    { return flipSquare[player, square]; }

    //	Look up the direction of travel from the 'from' square to the 'to' square
    public int DirectionLookup(int from, int to)
    { return directionLookup[from, to]; }

    //	Converts a square (identified by its number) to an actual location (Rank, File)
    public virtual Location SquareToLocation(int square)
    { return new Location(rankBySquare[square], fileBySquare[square]); }

    //	Finds the square number for a Location
    public virtual int LocationToSquare(Location location)
    { return location.File * NumRanks + location.Rank; }

    //	Finds the square number for specified rank and file
    public virtual int LocationToSquare(int rank, int file)
    { return file * NumRanks + rank; }

    //	Finds the square number for the given notation 
    public int DefaultNotationToSquare(string notation)
    {
      try
      {
        int file = fileLookup[notation[0]];
        int rank = rankLookup[notation.Substring(1)];
        return file * NumRanks + rank;
      }
      catch (Exception ex)
      {
        throw new Exception("Board.NotationToSquare - unknown square notation: " + notation, ex);
      }
    }

    //	returns 1 or 0 depending if the given square is in the small center (for PST determination)
    public int InSmallCenter(int square)
    { return pstInSmallCenter[square]; }

    //	returns 1 or 0 depending if the given square is in the large center (for PST determination)
    public int InLargeCenter(int square)
    { return pstInLargeCenter[square]; }

    //	returns value for forwardness  (for PST determination)
    public int Forwardness(int square)
    { return pstForwardness[square]; }

    //  returns the BitBoard containing all the pieces of the given player
    //  (this information is updated incrementally so it is always available)
    public BitBoard GetPlayerPieceBitboard(int player)
    { return playerPieceBitboards[player]; }
    //	same but optimized for boards with 64 squares or less
    public BitBoard64 GetPlayerPieceBitboard64(int player)
    { return playerPieceBitboards64[player]; }

    //  returns the BitBoard containing all the pieces of the given player and type
    //  (this information is updated incrementally so it is always available)
    public BitBoard GetPieceTypeBitboard(int player, int pieceType)
    { return pieceTypeBitboards[player, pieceType]; }
    //	same but optimized for boards with 64 squares or less
    public BitBoard64 GetPieceTypeBitboard64(int player, int pieceType)
    { return pieceTypeBitboards64[player, pieceType]; }

    //  returns the BitBoard containing all the pieces of the given player, type, 
    //  and "slice" so that colorbound pieces are stored separately based on binding
    public BitBoard GetPieceTypeBitboardSliced(int player, int pieceType, int slice)
    { return pieceTypeBitboardsSliced[player, pieceType, slice]; }

    //  returns the total midgame material value for the player (without PST)
    public int GetPlayerMaterial(int player)
    { return playerMaterial[player]; }

    //  returns the total endgame material value for the player (without PST)
    public int GetPlayerEndgameMaterial(int player)
    { return playerEndgameMaterial[player]; }

    //  returns the total midgame material value for the player including midgame PST
    public int GetMidgameMaterialEval(int player)
    { return midgameMaterialEval[player]; }

    //	returns the total midtamge material value + PST for the game (both players)
    public int GetMidgameMaterialEval()
    { return midgameMaterialEval[0] - midgameMaterialEval[1]; }

    //  returns the total endgame material value for the player including endgame PST
    public int GetEndgameMaterialEval(int player)
    { return endgameMaterialEval[player]; }
    #endregion


    // *** CONSTRUCTION *** //

    // TEST ONLY
    public Board() { }

    #region Construction
    public Board(int nFiles, int nRanks) : this(nFiles, nRanks, nRanks * nFiles)
    { }

    protected Board(int nFiles, int nRanks, int nSquaresExtended)
    {
      NumFiles = nFiles;
      NumRanks = nRanks;
      int nSquares = nFiles * nRanks;
      NumSquares = nSquares;
      NumSquaresExtended = nSquaresExtended;
      DisableSimpleMoveGeneration = false;
      hashcode = 0;
      PawnHashCode = 0;
      MaterialHashCode = 0;
      squares = new Piece[nSquaresExtended];
      fileBySquare = new int[nSquaresExtended];
      rankBySquare = new int[nSquaresExtended];
      rankNotations = new string[MAX_RANKS];
      fileNotations = new char[MAX_FILES];
      rankLookup = new Dictionary<string, int>();
      fileLookup = new Dictionary<char, int>();
      pstInSmallCenter = new int[nSquaresExtended];
      pstInLargeCenter = new int[nSquaresExtended];
      pstForwardness = new int[nSquaresExtended];
      for (int rank = 0; rank < nRanks; rank++)
        rankNotations[rank] = (rank + 1).ToString();
      for (int file = 0; file < nFiles; file++)
        fileNotations[file] = (char)('a' + file);

      //	create "distances" lookup and initialize all values to NOT_CONNECTED for now
      distances = new int[nSquaresExtended, nSquaresExtended];
      for (int s1 = 0; s1 < nSquaresExtended; s1++)
        for (int s2 = 0; s2 < nSquaresExtended; s2++)
          distances[s1, s2] = NOT_CONNECTED;

      int[] distanceFromEdge = new int[nSquaresExtended];
      int[] maxDistanceFromEdgeByBoardSize = { 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7 };
      int maxDistanceFromEdge = nFiles < nRanks ? maxDistanceFromEdgeByBoardSize[nFiles] : maxDistanceFromEdgeByBoardSize[nRanks];
      int[] forwardness8Ranks = { -2, -1, 0, 1, 2, 3, 4, 5 };
      int[] forwardness9Ranks = { -2, -1, 0, 1, 1, 2, 3, 4, 5 };
      int[] forwardness10Ranks = { -3, -2, -2, 0, 1, 2, 3, 4, 4, 5 };
      int[] forwardness12Ranks = { -3, -3, -2, -1, 0, 1, 2, 3, 3, 4, 4, 5 };
      int sq1;
      for (sq1 = 0; sq1 < nRanks * nFiles; sq1++)
      {
        fileBySquare[sq1] = sq1 / NumRanks;
        rankBySquare[sq1] = sq1 % NumRanks;
      }
      for (sq1 = 0; sq1 < nRanks * nFiles; sq1++)
      {
        int file1 = GetFile(sq1);
        int rank1 = GetRank(sq1);
        int fileFromEdge = file1 < nFiles - file1 - 1 ? file1 : nFiles - file1 - 1;
        int rankFromEdge = rank1 < nRanks - rank1 - 1 ? rank1 : nRanks - rank1 - 1;
        distanceFromEdge[sq1] = Math.Min(fileFromEdge, rankFromEdge);
        if (nFiles >= 9 && nRanks >= 9)
        {
          pstInSmallCenter[sq1] = distanceFromEdge[sq1] >= maxDistanceFromEdge - 1 ? 1 : 0;
          pstInLargeCenter[sq1] = distanceFromEdge[sq1] >= maxDistanceFromEdge - 2 ? 1 : 0;
        }
        else
        {
          pstInSmallCenter[sq1] = distanceFromEdge[sq1] == maxDistanceFromEdge ? 1 : 0;
          pstInLargeCenter[sq1] = distanceFromEdge[sq1] >= maxDistanceFromEdge - 1 ? 1 : 0;
        }
        if (nRanks <= 8)
          pstForwardness[sq1] = forwardness8Ranks[rank1 + (8 - nRanks)];
        else if (nRanks == 9)
          pstForwardness[sq1] = forwardness9Ranks[rank1];
        else if (nRanks == 10)
          pstForwardness[sq1] = forwardness10Ranks[rank1];
        else if (nRanks == 12)
          pstForwardness[sq1] = forwardness12Ranks[rank1];
        else
          throw new Exception("Board constructor forwardness");
        int sq2;
        for (sq2 = 0; sq2 < nRanks * nFiles; sq2++)
        {
          int file2 = GetFile(sq2);
          int rank2 = GetRank(sq2);
          int fileOffset = file2 > file1 ? file2 - file1 : file1 - file2;
          int rankOffset = rank2 > rank1 ? rank2 - rank1 : rank1 - rank2;
          distances[sq1, sq2] = fileOffset > rankOffset ? fileOffset : rankOffset;
        }
      }
    }
    #endregion


    // *** INITIALIZATION *** //

    #region PostCreate
    public virtual void PostCreate(Game game)
    {
      Game = game;
      //	initialize file/rank notation lookup dictionaries
      for (int file = 0; file < NumFiles; file++)
        if (fileNotations[file] != ' ')
          fileLookup.Add(fileNotations[file], file);
      for (int rank = 0; rank < NumRanks; rank++)
        if (rankNotations[rank] != " ")
          rankLookup.Add(rankNotations[rank], rank);
    }
    #endregion

    #region Initializaiton
    public virtual void Initialize()
    {
      //	build "nextStep" matrix for next square number in any direction from each square
      buildNextStepMatrix();

      //	build "flipSquare" matrix that translates square numbers for a player based on symmetry (mirror/rotational/etc.)
      buildFlipSquareMatrix();

      //	build the "directionLookup" matrix that determines what direction of travel to 
      buildDirectionLookupMatrix();

      //	arrays to store material and quick-eval information that is updated incrementally
      playerMaterial = new int[Game.NumPlayers];
      playerEndgameMaterial = new int[Game.NumPlayers];
      midgameMaterialEval = new int[Game.NumPlayers];
      endgameMaterialEval = new int[Game.NumPlayers];
      pieceCountByType = new int[Game.NumPlayers, Game.NPieceTypes];
      playerPieceBitboards = new BitBoard[Game.NumPlayers];
      playerPieceBitboards64 = new BitBoard64[Game.NumPlayers];
      pieceTypeBitboards = new BitBoard[Game.NumPlayers, Game.NPieceTypes];
      pieceTypeBitboards64 = new BitBoard64[Game.NumPlayers, Game.NPieceTypes];
      pieceTypeBitboardsSliced = new BitBoard[Game.NumPlayers, Game.NPieceTypes, 8];
      for (int player = 0; player < Game.NumPlayers; player++)
      {
        playerMaterial[player] = 0;
        playerEndgameMaterial[player] = 0;
        midgameMaterialEval[player] = 0;
        endgameMaterialEval[player] = 0;
        playerPieceBitboards[player] = new BitBoard(NumSquaresExtended);
        for (int x = 0; x < Game.NPieceTypes; x++)
        {
          pieceCountByType[player, x] = 0;
          pieceTypeBitboards[player, x] = new BitBoard(NumSquaresExtended);
          for (int y = 0; y < 8; y++)
            pieceTypeBitboardsSliced[player, x, y] = new BitBoard(NumSquaresExtended);
        }
      }
    }
    #endregion


    // *** OVERRIDABLE INITIALIZATION FUNCTIONS *** //

    #region buildNextStepMatrix
    protected virtual void buildNextStepMatrix()
    {
      //	get "directions" used from the Game class
      Direction[] directions;
      int nDirections = Game.GetDirections(out directions);
      NumberOfDirections = nDirections;

      //	build "nextStep" matrix for next square number in any direction from each square
      nextStep = new int[nDirections, NumSquaresExtended];
      for (int x = 0; x < nDirections; x++)
      {
        for (int y = 0; y < NumSquaresExtended; y++)
        {
          if (y < NumSquares)
          {
            Location location = SquareToLocation(y);
            Direction direction = directions[x];
            Location nextLocation = new Location(location.Rank + direction.RankOffset, location.File + direction.FileOffset);
            if (nextLocation.File >= 0 && nextLocation.File < NumFiles &&
              nextLocation.Rank >= 0 && nextLocation.Rank < NumRanks)
              nextStep[x, y] = LocationToSquare(nextLocation);
            else
              nextStep[x, y] = -1;
          }
          else
            nextStep[x, y] = -1;
        }
      }
    }
    #endregion

    #region buildFlipSquareMatrix
    protected virtual void buildFlipSquareMatrix()
    {
      //	build "flipSquare" matrix that translates square numbers for a player based on symmetry (mirror/rotational/etc.)
      flipSquare = new int[Game.NumPlayers, NumSquaresExtended];
      for (int player = 0; player < Game.NumPlayers; player++)
      {
        for (int square = 0; square < NumSquares; square++)
        {
          Location location = Game.Board.SquareToLocation(square);
          flipSquare[player, square] = Game.Board.LocationToSquare(
            Game.Symmetry.Translate(player, location));
        }
        for (int square = NumSquares; square < NumSquaresExtended; square++)
          flipSquare[player, square] = square;
      }
    }
    #endregion

    #region buildDirectionLookupMatrix
    protected virtual void buildDirectionLookupMatrix()
    {
      //	get "directions" used from the Game class
      Direction[] directions;
      int nDirections = Game.GetDirections(out directions);
      NumberOfDirections = nDirections;

      //	build the "directionLookup" matrix that determines what direction of travel to 
      //	get from one square to another
      directionLookup = new int[NumSquares, NumSquares];
      //	first, initialize the entire matrix with -1 to indicate squares not connected by direction
      for (int from = 0; from < NumSquares; from++)
        for (int to = 0; to < NumSquares; to++)
          directionLookup[from, to] = -1;
      //	now, starting with each square, travel out in each direction marking squares appropriately
      for (int from = 0; from < NumSquares; from++)
        for (int direction = 0; direction < nDirections; direction++)
        {
          int current = nextStep[direction, from];
          while (current >= 0 && current < NumSquares)
          {
            if (directionLookup[from, current] == -1)
              //	only record first direction found to give preference 
              //	to the eight primary directions (in case of Dababbah moves, etc.)
              directionLookup[from, current] = direction;
            else if (current == from)
              //	we have wrapped all the way around a circular or cylindrical board
              break;
            current = nextStep[direction, current];
          }
        }
    }
    #endregion


    // *** CONFIGURATION *** //

    #region SetFileNotation
    public void SetFileNotation(char[] fileNotation)
    { fileNotations = fileNotation; }
    #endregion

    #region SetRankNotation
    public void SetRankNotation(string[] rankNotation)
    { rankNotations = rankNotation; }
    #endregion


    // *** OPERATIONS *** //

    #region ArrayToPieceMap
    public Dictionary<int, GenericPiece> ArrayToPieceMap
      (Dictionary<string, PieceType>[] typesByNotation,
        string array)
    {
      Dictionary<int, GenericPiece> map;   //  the map we're building
      int cursor = 0;                      //  current index into the array string
      int file = 0;                        //  current board file we're writing to
      int rank = NumRanks - 1;        //  current board rank we're writing to
      PieceType newPieceType;              //  piece type read

      map = new Dictionary<int, GenericPiece>();
      while (cursor < array.Length)
      {
        if (array[cursor] == '/')
        {
          //	the / indicates we are starting a new file.  if we are not already 
          //	at the end of the rank, we will go ahead and consider the array valid 
          //	and fill the skipped squares with nulls
          while (file < NumFiles)
            map.Add(LocationToSquare(new Location(rank, file++)), null);
          //	advance to new rand & file
          file = 0;
          rank--;
          cursor++;
        }
        else if (array[cursor] >= '0' && array[cursor] <= '9')
        {
          //	a digit indicates we are skipping spaces - grab all digits
          int start = cursor++;
          while (cursor < array.Length && array[cursor] >= '0' && array[cursor] <= '9')
            cursor++;
          //	convert to int and calculate new file
          int newfile = file + Convert.ToInt32(array.Substring(start, cursor - start));
          if (newfile > NumFiles)
            throw new Exception("extends pass board's right edge at or near character " + cursor.ToString());
          //	fill the empty files in the map with nulls
          while (file < newfile)
            map.Add(LocationToSquare(new Location(rank, file++)), null);
        }
        else if ((array[cursor] >= 'A' && array[cursor] <= 'Z') ||
             (array[cursor] >= 'a' && array[cursor] <= 'z') ||
              array[cursor] >= 'À' && array[cursor] <= 'Ö' ||
              array[cursor] >= 'à' && array[cursor] <= 'ö' ||
              array[cursor] == '_')
        {
          //	determine player based on case - uppercase (including accented) is player 0
          int player;
          char c = array[cursor] == '_' && array.Length > cursor + 1 ? array[cursor + 1] : array[cursor];
          if ((c >= 'A' && c <= 'Z') || (c >= 'À' && c <= 'Ö'))
              player = 0;
          else
              player = 1;
          //	ask the Game class to parse piece type from string
          newPieceType = Game.ParsePieceTypeFromString(array, ref cursor);
          //	add the piece to the map
          try
          {
            map.Add(LocationToSquare(new Location(rank, file++)), new GenericPiece(player, newPieceType));
          }
          catch (Exception ex)
          {
            throw new Exception("Error - failed to place pieces by array", ex);
          }
        }
        else
          throw new Exception("unexpected character in piece array: " + array[cursor]);
      }
      return map;
    }
    #endregion

    #region ClearSquare
    public Piece ClearSquare(int square)
    {
      Piece piece = squares[square];
      if (piece == null)
        throw new Exception("!");
      squares[square] = null;
      piece.Square = -1;
      playerMaterial[piece.Player] -= piece.PieceType.MidgameValue;
      playerEndgameMaterial[piece.Player] -= piece.PieceType.EndgameValue;
      midgameMaterialEval[piece.Player] -= piece.PieceType.MidgameValue + piece.PieceType.GetMidgamePST(flipSquare[piece.Player, square]);
      endgameMaterialEval[piece.Player] -= piece.PieceType.EndgameValue + piece.PieceType.GetEndgamePST(flipSquare[piece.Player, square]);
      playerPieceBitboards[piece.Player].ClearBit(square);
      playerPieceBitboards64[piece.Player].ClearBit(square);
      pieceTypeBitboards[piece.Player, piece.TypeNumber].ClearBit(square);
      pieceTypeBitboards64[piece.Player, piece.TypeNumber].ClearBit(square);
      pieceTypeBitboardsSliced[piece.Player, piece.TypeNumber, piece.PieceType.SliceLookup[square]].ClearBit(square);
      hashcode = hashcode ^ piece.PieceType.GetHashKey(piece.Player, square);
      PawnHashCode = PawnHashCode ^ piece.PieceType.GetPawnHashKey(piece.Player, square);
      MaterialHashCode = MaterialHashCode ^ piece.PieceType.GetMaterialHashKey(piece.Player, piece.PieceType.SliceLookup[square], pieceCountByType[piece.Player, piece.TypeNumber]);
      pieceCountByType[piece.Player, piece.TypeNumber] -= 1;
      return piece;
    }
    #endregion

    #region SetSquare
    public void SetSquare(Piece piece, int square)
    {
      if (squares[square] != null)
        throw new Exception("Error in SetSquare - square is already occupied");
      squares[square] = piece;
      piece.Square = square;
      playerMaterial[piece.Player] += piece.PieceType.MidgameValue;
      playerEndgameMaterial[piece.Player] += piece.PieceType.EndgameValue;
      midgameMaterialEval[piece.Player] += piece.PieceType.MidgameValue + piece.PieceType.GetMidgamePST(flipSquare[piece.Player, square]);
      endgameMaterialEval[piece.Player] += piece.PieceType.EndgameValue + piece.PieceType.GetEndgamePST(flipSquare[piece.Player, square]);
      playerPieceBitboards[piece.Player].SetBit(square);
      playerPieceBitboards64[piece.Player].SetBit(square);
      pieceTypeBitboards[piece.Player, piece.TypeNumber].SetBit(square);
      pieceTypeBitboards64[piece.Player, piece.TypeNumber].SetBit(square);
      pieceTypeBitboardsSliced[piece.Player, piece.TypeNumber, piece.PieceType.SliceLookup[square]].SetBit(square);
      pieceCountByType[piece.Player, piece.TypeNumber] += 1;
      hashcode = hashcode ^ piece.PieceType.GetHashKey(piece.Player, square);
      PawnHashCode = PawnHashCode ^ piece.PieceType.GetPawnHashKey(piece.Player, square);
      MaterialHashCode = MaterialHashCode ^ piece.PieceType.GetMaterialHashKey(piece.Player, piece.PieceType.SliceLookup[square], pieceCountByType[piece.Player, piece.TypeNumber]);
    }
    #endregion

    #region RecalculateMaterialEvaluations
    public void RecalculateMaterialEvaluations()
    {
      midgameMaterialEval[0] = midgameMaterialEval[1] = 0;
      endgameMaterialEval[0] = endgameMaterialEval[1] = 0;

      for (int x = 0; x < NumSquaresExtended; x++)
      {
        if (squares[x] != null)
        {
          Piece piece = squares[x];
          midgameMaterialEval[piece.Player] += piece.PieceType.MidgameValue + piece.PieceType.GetMidgamePST(flipSquare[piece.Player, x]);
          endgameMaterialEval[piece.Player] += piece.PieceType.EndgameValue + piece.PieceType.GetEndgamePST(flipSquare[piece.Player, x]);
        }
      }
    }
    #endregion

    #region CalculateStandardMovePST
    public int CalculateStandardMovePST(int from, int to)
    {
      Piece piece = squares[to];
      if (piece == null)
        throw new Exception("!");
      return piece.PieceType.GetMidgamePST(flipSquare[piece.Player, to]) - piece.PieceType.GetMidgamePST(flipSquare[piece.Player, from]);
    }
    #endregion

    #region ClearBoard
    public void ClearBoard()
    {
      for (int x = 0; x < NumSquaresExtended; x++)
        if (this[x] != null)
          ClearSquare(x);
    }
    #endregion


    // *** DEBUG OPERATIONS *** //

    #region Validate
    public void Validate()
    {
      List<Piece> pieces = Game.GetPieceList();
      foreach (Piece piece in pieces)
        if (piece != this[piece.Square])
          throw new Exception("Board fails to validate - pieces");
      for (int square = 0; square < NumSquares; square++)
      {
        Piece pieceOnSquare = this[square];
        if (pieceOnSquare != null && !pieces.Contains(pieceOnSquare))
          throw new Exception("Board fails to validate - piece on square");
        if (pieceOnSquare != null && playerPieceBitboards[pieceOnSquare.Player].GetBit(square) != 1)
          throw new Exception("Board fails to validate - BitBoard");
        if (NumSquaresExtended <= 64 && pieceOnSquare != null && playerPieceBitboards64[pieceOnSquare.Player].GetBit(square) != 1)
          throw new Exception("Board fails to validate - BitBoard64");
        if (pieceOnSquare != null && pieceTypeBitboards[pieceOnSquare.Player, pieceOnSquare.TypeNumber].GetBit(square) != 1)
          throw new Exception("Board fails to validate - BitBoard");
        if (NumSquaresExtended <= 64 && pieceOnSquare != null && pieceTypeBitboards64[pieceOnSquare.Player, pieceOnSquare.TypeNumber].GetBit(square) != 1)
          throw new Exception("Board fails to validate - BitBoard64");
        if (pieceOnSquare != null && pieceTypeBitboardsSliced[pieceOnSquare.Player, pieceOnSquare.TypeNumber, pieceOnSquare.PieceType.SliceLookup[square]].GetBit(square) != 1)
          throw new Exception("Board fails to validate - BitBoard");
      }
    }
    #endregion


    // *** PROTECTED DATA MEMBERS *** //

    #region Protected data members
    //	Array of the piece (if any) on each square
    protected Piece[] squares;

    //	nextStep[direction, square] gives the next square in the given direction 
    protected int[,] nextStep;

    //	flipSquare[player, square] gives the translated (rotated) square for the given player
    protected int[,] flipSquare;

    //	directionLookup[from, to] gives the number of the direction to go from from to to
    protected int[,] directionLookup;

    //	notation character for each rank
    protected string[] rankNotations;

    //	notation character for each file
    protected char[] fileNotations;

    //	distances[sq1, sq2] is the number of king steps go get from sq1 to sq2
    protected int[,] distances;

    //	the number of the file of the given square
    protected int[] fileBySquare;

    //	the number of the rank of the given square
    protected int[] rankBySquare;

    //	look up the number of the rank by the notation
    protected Dictionary<string, int> rankLookup;

    //	look up the number of the file by the notation char
    protected Dictionary<char, int> fileLookup;

    //	position hash code (Zobrist hash).  When the public HashCode is retreived,
    //	this is combined with the side to move, etc.
    protected UInt64 hashcode;

    //	total midgame material value of pieces remaining for player (midgame values, no PST)
    protected int[] playerMaterial;

    //	total endgame material value of pieces remaining for player (midgame values, no PST)
    protected int[] playerEndgameMaterial;

    //	total midgame value of all remaining pieces for given player plus PST values
    protected int[] midgameMaterialEval;

    //	total endgame value of all remaining pieces for given player plus PST values
    protected int[] endgameMaterialEval;

    //	pieceCountByType[player, pieceType] is the number of pieces of type player still has
    protected int[,] pieceCountByType;

    //	bitboards marking all the pieces of the given player
    protected BitBoard[] playerPieceBitboards;
    protected BitBoard64[] playerPieceBitboards64;

    //	bitboards marking all the squares containing pieces of the given type and player
    protected BitBoard[,] pieceTypeBitboards;
    protected BitBoard64[,] pieceTypeBitboards64;

    //	bitboards marking all the squares containing pieces of the given type, player, and slice
    protected BitBoard[,,] pieceTypeBitboardsSliced;

    //	1 if the given square is in the "small center" (used for generating piece-square-tables)
    protected int[] pstInSmallCenter;

    //	1 if the given square is in the "large center" (used for generating piece-square-tables)
    protected int[] pstInLargeCenter;

    //	value of the "forwardness" of a given square (used for generating piece-square-tables)
    protected int[] pstForwardness;
    #endregion
  }
}
