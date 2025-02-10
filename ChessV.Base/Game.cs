
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

using ChessV.Base;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ChessV
{
  public partial class Game : ExObject
  {
    // *** CONSTANTS *** //

    #region Constants
    public const int MAX_DIRECTIONS = 48;
    public const int MAX_PIECE_TYPES = 24; // necessary maximum when hashing MoveInfo Tag functionality
    public const int MAX_PIECES = 64;
    public const int MAX_PLY = 128;
    //	Internal indicator for movement matrices that a square is disconnected
    public const int NOT_CONNECTED = -1;
    #endregion


    // *** PUBLIC PROPERTIES *** //

    #region Public Properties
    //	The GameAttribute with the metadata for the game index for this variant (author, year invented, etc.)
    public GameAttribute GameAttribute { get; private set; }

    //	The Registry Key that hold the user's settings for this particular variant (colors, etc.)
    public RegistryKey RegistryKey { get; set; }

    //	The Board object (stores the pieces, position hash code, etc)
    public virtual Board Board { get; private set; }

    //	The Symmetry controls how piece moves translate between players (mirror, rotational, etc.)
    public Symmetry Symmetry { get; private set; }

    //	The number of players in this game (right now only two player games supported)
    public int NumPlayers { get; private set; }

    //	The number of ranks on the main part of the board (excludes pocket squares)
    public int NumRanks { get; private set; }

    //	The number of files on the main part of the board (excludes pocket squares)
    public int NumFiles { get; protected set; }

    //	The format of the FEN for this game
    public string FENFormat
    {
      get { return fen.FormatString; }
      set { FEN = new FEN(value); }
    }

    public string OriginalFENStart { get; protected set; }

    //	Map to the placement of pieces to square notation at game start
    public Dictionary<string, GenericPiece> StartingPieces;

    //	Map of occupied squares at start of game (1=occupied, 0=unoccupied).
    //	This is used for measuring development progress so we can evaluate 
    //	piece development and have the eval adjustment taper out.
    public int[,] StartingPieceSquares;

    //	Count of player pieces on the board at the start of the game
    public int[] StartingPieceCount;

    //	The FEN object that understands the FEN format and handles the parsing
    public FEN FEN
    {
      get { updateFEN(); return fen; }
      private set { fen = value; }
    }

    //	The FEN string of the current game position
    public string FENString
    { get { return null; } }

    //	A dictionary that looks up piece types by notation by player
    public Dictionary<string, PieceType>[] TypesByNotation { get; protected set; }

    //	Snapshot of the values of game variables before user editing/loading
    public Dictionary<string, object> GameVariableSnapshot;

    //	ArrayBeingLoaded - if true, suspends certain triggers while board is being set up
    public bool ArrayBeingLoaded { get; protected set; }

    //	Variation - a value from 0 to 3 controlling how much variation there is 
    //	from game to game (assuming the same situations.)  Set to 0 ChessV is 
    //	completely deterministic.
    public int Variation { get; set; }

    //	GameMoveNumber - the current number of moves into the actual game (on the board, does 
    //	not count thinking depth.)  And this is the number of player moves, not the turn number.
    //	In Chess language, they sometimes call both a white move and a black a "Move".  For 
    //	GameMoveNumber, this counts as two moves.  The Chess usage is called GameTurnNumber (below.)
    public int GameMoveNumber
    { get { return gameHistoryCount; } }

    //	GameTurnNumber - the current number of the turn.  Will be 1 for both white's first move 
    //	and black's first move, then will increment to 2.  In double-move variants like Marseillais 
    //	Chess, it still reflects what the player would consider to be the turn number to be.
    public int GameTurnNumber
    { get { return moveCompletionRule.TurnNumber; } }

    //	The active PromotionRule (if any.)  If there are multiple promotion 
    //	rules, this will only return one.
    public PromotionRule ActivePromotionRule
    { get { return (PromotionRule)FindRule(typeof(PromotionRule), true); } }

    //	Total board material value at which we start to shift from midgame evaluation to endgame
    public int MidgameMaterialThreshold { get; set; }

    //	Total board material value at which we are using fully endgame evaluation
    public int EndgameMaterialThreshold { get; set; }

    //	True if pieces have no complex moves (such as cannon capture)
    public bool SimpleMoveGeneration { get; protected set; }

    //	An object that assigns Zobrist keys to piece types, rules, and whatever else needs them
    public HashKeys HashKeys { get; protected set; }

    //	The engines responsible for computer play in this game instance and current assignment
    public bool[] ComputerControlled { get; private set; }

    //	Is this game an automated match (no dialog boxes to user upon completion)
    public bool IsAutomatedMatch { get; set; }

    //	The outcome of the game (Result.IsNone will be true if not yet completed)
    public Result Result { get; set; }

    //	The time used per player
    public Int64[] TimeUsed { get; private set; }

    //	The number of the side who is currently to move
    public int CurrentSide { get; set; }

    //  Is the internal engine within this Game object currently thinking?
    public bool IsThinking { get; private set; }

    //	The number of the side to move next after the current move is completed.  If the next side 
    //	depends on what move is made, as in Marseillais Chess, this will not be a valid result 
    //	until the MoveBeingMade event is sent to the MoveCompletionRule.
    public int NextSide
    { get { return moveCompletionRule.GetNextSide(); } }

    //	The Player object of the player who's turn it is to move
    public Player CurrentPlayer
    { get { return Match == null ? null : Match.GetPlayer(CurrentSide); } }

    //	The squares to be highlighted to show the user the last move by the computer
    public List<int> HighlightSquares { get; private set; }

    //	Structures used by the internal engine for computer play
    public int Ply { get; private set; }
    public SearchStack[] SearchStack { get { return searchStack; } }
    public UInt32[] Killers1 { get { return killers1; } }
    public UInt32[] Killers2 { get { return killers2; } }
    public Statistics Statistics { get; private set; }
    public Movement[] SearchPath;

    public Match Match { get; set; }

    public TimerFactory TimerFactory { get; private set; }

    public BoardMoveStack BoardMoveStack { get; private set; }

    public DebugMessageLog MessageLog
    { get { return messageLog; } }
    #endregion


    // *** GAME VARIABLES *** //

    #region Game Variables
    [GameVariable] public string Name { get; set; }
    [GameVariable] public string Invented { get; set; }
    [GameVariable] public string InventedBy { get; set; }
    [GameVariable(Hidden = true)] public string GameDescription1 { get; set; }
    [GameVariable(Hidden = true)] public string GameDescription2 { get; set; }
    [GameVariable] public int NumberOfSquareColors { get; set; }
    public string[] PlayerNames { get; set; }
    public char[] PlayerDesignations { get; set; }

    //	The FEN of the start position for this game
    [GameVariable] public string FENStart { get; set; }

    //	The array component of the FEN - assuming FENStart still contains %{Array}
    [GameVariable] public string Array { get; set; }

    //	Controls whether the move generator needs to detect and skip generation
    //	of moves that would otherwise be duplicates (e.g., CylindricalChess)
    [GameVariable] public bool DeduplicateMoves { get; set; }

    //	Controls whether the Static Exchange Evaluator (SEE) should be used
    [GameVariable] public bool StaticExchangeEvaluation { get; set; }
    #endregion


    // *** EVENTS *** //

    #region Events
    //	MoveBeingPlayed and MovePlayed events - fired when a move is 
    //	actually played on the board.  These are used by rules to 
    //	update information (e.g., castling rights) and by user interface 
    //	elements to update the display.  There are two different events 
    //	with MoveBeingPlayed being called before MovePlayed so that 
    //	events that need to be handled first can be.
    public delegate void MoveNotificationHandler(MoveInfo move);
    public event MoveNotificationHandler MoveBeingPlayed;
    public event MoveNotificationHandler MovePlayed;

    //	Take back move event - fired when a move is taken back 
    public delegate void TakeBackMoveHandler();
    public event TakeBackMoveHandler MoveTakenBack;

    //	Thinking callback
    public ThinkingCallback ThinkingCallback { get; set; }
    #endregion


    // *** CONSTRUCTION and INITIALIZATION *** //

    // TEST ONLY
    public Game() { }

    #region Constructor
    public Game
      (int nPlayers,             // number of players
        int nFiles,               // number of files on main part of board
        int nRanks,               // number of ranks on main part of board
        Symmetry symmetry)        // symmetry determining board mirroring/rotation
    {
      HashKeys = new ChessV.HashKeys();
      PlayerNames = new string[nPlayers];
      TimeUsed = new long[nPlayers];
      ComputerControlled = new bool[nPlayers];
      NumPlayers = nPlayers;
      NumRanks = nRanks;
      NumFiles = nFiles;
      Ply = 0;
      directions = new Direction[MAX_DIRECTIONS];
      nDirections = 0;
      nSlidingDirections = 0;
      playerDirections = new int[nPlayers, MAX_DIRECTIONS];
      maxAttackRange = new int[nPlayers, MAX_DIRECTIONS];
      oppositeDirections = new int[MAX_DIRECTIONS];
      pieceTypes = new PieceType[MAX_PIECE_TYPES];
      nPieceTypes = 0;
      disabledPieceTypes = new PieceType[MAX_PIECE_TYPES];
      nDisabledPieceTypes = 0;
      for (int player = 0; player < nPlayers; player++)
        TimeUsed[player] = 0;
      pieces = new Piece[nPlayers, MAX_PIECES];
      nPieces = new int[nPlayers];
      moveLists = new MoveList[MAX_PLY];
      searchStack = new SearchStack[MAX_PLY];
      killers1 = new UInt32[MAX_PLY];
      killers2 = new UInt32[MAX_PLY];
      razorMargin = new int[] { 300, 350, 400, 450, 450, 450, 450, 450 };
      seeAttackers = new List<Piece>[2];
      seeAttackers[0] = new List<Piece>(24);
      seeAttackers[1] = new List<Piece>(24);
      Statistics = new global::Statistics();
      SearchPath = new Movement[MAX_PLY];
      for (int x = 0; x < MAX_PLY; x++)
        searchStack[x].Initialize();
      gameHistory = new MoveInfo[MAX_GAME_LENGTH];
      gameHistoryTurnNumbers = new int[MAX_GAME_LENGTH];
      TypesByNotation = new Dictionary<string, PieceType>[2];
      TypesByNotation[0] = new Dictionary<string, PieceType>();
      TypesByNotation[1] = new Dictionary<string, PieceType>();
      pieceTypeNumbers = new Dictionary<PieceType, int>();
      classesThatCreatedPieceTypes = new Dictionary<PieceType, Type>();
      rules = new List<Rule>();
      evaluations = new List<Evaluation>();
      rulesHandlingGetPositionHashCode = new List<Rule>();
      rulesHandlingMoveBeingGenerated = new List<Rule>();
      rulesHandlingMoveBeingMade = new List<Rule>();
      rulesHandlingMoveMade = new List<Rule>();
      rulesHandlingMoveBeingUnmade = new List<Rule>();
      rulesHandlingTestForWinLossDraw = new List<Rule>();
      rulesHandlingNoMovesResult = new List<Rule>();
      rulesHandlingGenerateSpecialMoves = new List<Rule>();
      rulesHandlingSearchExtensions = new List<Rule>();
      rulesHandlingIsSquareAttacked = new List<Rule>();
      rulesHandlingAdjustEvaluation = new List<Rule>();
      ArrayBeingLoaded = false;
      IsAutomatedMatch = false;
      TimerFactory = new TimerFactory();
      messageLog = new DebugMessageLog();
      hashtable = null;
      Symmetry = symmetry;
      TTSizeInMB = 128;
      Variation = 0;
      Weakening = 0;
      IsThinking = false;
      finalized = false;
    }
    #endregion

    #region Initialize
    public void Initialize
      (GameAttribute gameAttribute,
        Dictionary<string, string> definitions,
        InitializationHelper helper)
    {
      GameAttribute = gameAttribute;

      try
      {
        // Create board with virtual function
        Board = CreateBoard(NumPlayers, NumFiles, NumRanks, Symmetry);
        Board.PostCreate(this);

        Symmetry.Board = Board;
        sign = new int[] { 1, -1 };

        // *** GAME VARIABLES *** //

        //	1 - virtual function sets game variables
        SetGameVariables();

        //	2 - apply variables set with game attribute
        HandleDefinitions(GameAttribute.Definitions);

        //	Take a Snapshot of all game variable values.
        //	This is so that we can compare later and see what changes 
        //	beyond this point.  Any such changes will need to be 
        //	stored if we save the game.
        GameVariableSnapshot = GetAllGameVariables();
        OriginalFENStart = FENStart;

        //	3 - apply definitions passed in table to this function
        HandleDefinitions(definitions);

        //	4 - if any unassigned game variables remain, use the 
        //	helper function passed to the constructor to handle.
        //	These may randomize and/or prompt the user.
        HandleUnassigned(helper);

        //	5 - allow game-derived classes to set custom properties
        //	based on the game variables selected.  This allows them 
        //	to react to choices made in step 4.
        SetOtherVariables();


        // *** PIECE TYPES *** //

        //	1 - virtual function adds piece types
        AddPieceTypes();

        //	2 - remove any piece types not Enabled
        int ntype;
        for (ntype = 0; ntype < nPieceTypes;)
        {
          if (!pieceTypes[ntype].Enabled)
          {
            disabledPieceTypes[nDisabledPieceTypes++] = pieceTypes[ntype];
            if (ntype < nPieceTypes - 1)
              pieceTypes[ntype] = pieceTypes[--nPieceTypes];
            else
              nPieceTypes--;
          }
          else
            ntype++;
        }
        for (; ntype < MAX_PIECE_TYPES; ntype++)
          pieceTypes[ntype] = null;

        //	3 - Initialize map of piece type numbers
        for (ntype = 0; ntype < nPieceTypes; ntype++)
          pieceTypeNumbers.Add(pieceTypes[ntype], ntype);

        #region Apply all PieceTypeProperties applied by attributes
        MemberInfo[] members = GetType().GetMembers();
        foreach (MemberInfo member in members)
        {
          if (member.MemberType == MemberTypes.Field || member.MemberType == MemberTypes.Property)
          {
            if ((member.MemberType == MemberTypes.Field &&
               (((FieldInfo)member).FieldType == typeof(PieceType) || ((FieldInfo)member).FieldType.IsSubclassOf(typeof(PieceType)))) ||
              (member.MemberType == MemberTypes.Property &&
               (((PropertyInfo)member).PropertyType == typeof(PieceType) || ((PropertyInfo)member).PropertyType.IsSubclassOf(typeof(PieceType)))))
            {
              object[] attrs = member.GetCustomAttributes(typeof(PieceTypePropertyAttribute), true);
              for (int x = 0; x < attrs.Length; x++)
              {
                PieceType pieceType = member.MemberType == MemberTypes.Field
                  ? (PieceType)((FieldInfo)member).GetValue(this)
                  : (PieceType)((PropertyInfo)member).GetValue(this, null);
                if (pieceType != null)
                  pieceType.AddAttribute((PieceTypePropertyAttribute)attrs[x]);
              }
            }
          }
        }
        #endregion

        #region Initialize pre-definied Directions
        nDirections = 8;
        directions[PredefinedDirections.N] = new Direction(1, 0);
        directions[PredefinedDirections.S] = new Direction(-1, 0);
        directions[PredefinedDirections.E] = new Direction(0, 1);
        directions[PredefinedDirections.W] = new Direction(0, -1);
        directions[PredefinedDirections.NE] = new Direction(1, 1);
        directions[PredefinedDirections.SW] = new Direction(-1, -1);
        directions[PredefinedDirections.NW] = new Direction(1, -1);
        directions[PredefinedDirections.SE] = new Direction(-1, 1);
        #endregion

        #region Call AddMoves for dynamic piece types defined by external scripts
        //	this hack is a little bit cheezy.  the constructor for the dynamically 
        //	created piece types don't call the AddMoves function like the built-in 
        //	ones do because the code for AddMoves is executed by the interpreter and 
        //	it isn't set up at construction time.
        for (int x = 0; x < nPieceTypes; x++)
        {
          PieceType piecetype = pieceTypes[x];
          if (piecetype.NumMoveCapabilities == 0 &&
            piecetype.GetType().GetMember("FunctionCodeLookup").Length > 0)
          {
            FieldInfo myEnvironmentField = GetType().GetField("Environment");
            FieldInfo pieceEnvironmentField = piecetype.GetType().GetField("Environment");
            pieceEnvironmentField.SetValue(piecetype, myEnvironmentField.GetValue(this));
            MethodInfo mi = piecetype.GetType().GetMethod("AddMoves");
            if (mi != null)
              mi.Invoke(piecetype, null);
          }
        }
        #endregion

        #region Modify piece movements based on any applied attributes
        for (ntype = 0; ntype < nPieceTypes; ntype++)
        {
          Attribute[] propertyAttributes = pieceTypes[ntype].GetCustomAttributes(typeof(PieceTypePropertyAttribute));
          foreach (Attribute propertyAttribute in propertyAttributes)
            ((PieceTypePropertyAttribute)propertyAttribute).AdjustMovement(pieceTypes[ntype]);
        }
        #endregion

        #region Initialize all Direction info from incorporated piece types
        //	two passes - fisrt pass is sliding directions; 
        //	second pass adds single-step only directions
        for (int pass = 1; pass <= 2; pass++)
        {
          for (int player = 0; player < NumPlayers; player++)
          {
            for (int x = 0; x < nPieceTypes; x++)
            {
              PieceType piecetype = pieceTypes[x];

              //	initialize directions from piece type movements
              MoveCapability[] moves;
              int nMoves = piecetype.GetMoveCapabilities(out moves);
              for (int y = 0; y < nMoves; y++)
              {
                if (pass == 2 || moves[y].MaxSteps > 1)
                {
                  Direction dir = Symmetry.Translate(player, moves[y].Direction);
                  //	see if this direction is new
                  bool isNew = true;
                  for (int z = 0; z < nDirections && isNew; z++)
                    if (directions[z] == dir)
                    {
                      isNew = false;
                      if (player == 0)
                        moves[y].NDirection = z;
                    }
                  if (isNew)
                  {
                    directions[nDirections++] = dir;
                    if (moves[y].MaxSteps > 1)
                      nSlidingDirections++;
                    if (player == 0)
                      moves[y].NDirection = nDirections - 1;
                  }
                }
              }
            }
          }
        }

        //	initialize playerDirections (directions translated as 
        //	determined by player and symmetry type)
        for (int player = 0; player < NumPlayers; player++)
          for (int dir = 0; dir < nDirections; dir++)
            playerDirections[player, dir] =
              GetDirectionNumber(Symmetry.Translate(player, directions[dir]));

        //	initialize oppositeDirections
        for (int dir = 0; dir < nDirections; dir++)
        {
          Direction direction = directions[dir];
          bool foundOpposite = false;
          for (int opdir = 0; !foundOpposite && opdir < nDirections; opdir++)
            if (direction.FileOffset == -directions[opdir].FileOffset &&
              direction.RankOffset == -directions[opdir].RankOffset)
            {
              oppositeDirections[dir] = opdir;
              foundOpposite = true;
            }
          if (!foundOpposite)
            throw new Exception("Assymetry of this type not yet implemented.");
        }
        #endregion

        // *** PIECE NOTATIONS *** //

        //	Enter all piece notations into the lookup table
        for (int x = 0; x < nPieceTypes; x++)
          if (pieceTypes[x].Notation[0] != null && pieceTypes[x].Notation[1] != null)
          {
            TypesByNotation[0].Add(pieceTypes[x].Notation[0], pieceTypes[x]);
            TypesByNotation[1].Add(pieceTypes[x].Notation[1], pieceTypes[x]);
          }

        // *** PIECE PLACEMENT *** //

        //	Expand variables in starting FEN
        bool FENUnchanged = FENStart == OriginalFENStart;
        ExpandVariablesInFEN();

        //	load the FEN object with the string representation
        fen.Load(FENStart);
        //	map out where the pieces will start
        try
        {
          parseStartingArray(fen["array"]);
        }
        catch (Exception ex)
        {
          throw new Exceptions.FENParseFailureException("Array", fen["array"], ex.Message, ex);
        }


        // *** RULES *** //

        //	Derived game classes add their game-specific rules
        AddRules();

        //	Give derived game classes an opportunity to reorder the 
        //	rules if some need to handle messages first.  This is 
        //	primarily so that the RepetitionDrawRule can be moved 
        //	to the end of the lsit.
        ReorderRules();

        //	rules initialize default values in the starting FEN
        foreach (Rule rule in rules)
          rule.SetDefaultsInFEN(fen);
        fen.SetUninitializedDefaults();
        FENStart = fen.ToString();
        if (FENUnchanged)
          GameVariableSnapshot["FENStart"] = FENStart;

        //	initialize maxAttackRange which stores the maximum attack range reachable by 
        //	any piece of the given player in the given direction
        for (int dir = 0; dir < nDirections; dir++)
          for (int player = 0; player <= 1; player++)
            for (int nPieceType = 0; nPieceType < nPieceTypes; nPieceType++)
            {
              PieceType type = pieceTypes[nPieceType];
              MoveCapability[] moveCapabilities;
              int nMoveCapabilities = type.GetMoveCapabilities(out moveCapabilities);
              for (int nMoveCapability = 0; nMoveCapability < nMoveCapabilities; nMoveCapability++)
              {
                MoveCapability moveCapability = moveCapabilities[nMoveCapability];
                int playerDirection = PlayerDirection(player, dir);
                if (dir == moveCapability.NDirection &&
                  (moveCapability.CanCapture || moveCapability.SpecialAttacks != 0) &&
                  moveCapability.MaxSteps > maxAttackRange[player, playerDirection])
                  maxAttackRange[player, playerDirection] = moveCapability.MaxSteps;
              }
            }


        //	Initialize the Board
        Board.Initialize();

        //	Initialize the PieceTypes
        //	this is where PSTs are calculated, mobility statistics, 
        //	and slices (i.e., determination of colorbinding)
        for (int x = 0; x < nPieceTypes; x++)
        {
          PieceType piecetype = pieceTypes[x];
          piecetype.Initialize(this);
        }


        // *** EVALUATION *** //
        AddEvaluations();

        // *** ANY REMAINING OPERATIONS *** //
        finishInitialization();

        //	initialize piece types and attack ranges
        postInitialize();
      }
      catch (Exception ex)
      {
        throw new Exceptions.GameInitializationException(gameAttribute,
          "Unable to create game '" + gameAttribute.GameName + "'", ex);
      }
    }
    #endregion

    #region finishInitialization
    protected virtual void finishInitialization()
    {
      //	Determine if game requires complex move generation
      if (Board.DisableSimpleMoveGeneration)
        SimpleMoveGeneration = false;
      else
      {
        SimpleMoveGeneration = true;
        for (int x = 0; x < NPieceTypes; x++)
          if (!pieceTypes[x].SimpleMoveGeneration)
            SimpleMoveGeneration = false;
      }
      if (!SimpleMoveGeneration)
        //	If we don't have simple move generation, we can't have SEE
        StaticExchangeEvaluation = false;

      //	Initialize game Result
      Result = new Result(ResultType.NoResult);

      //	Allocate array of history counters (move ordering history heuristic)
      historyCounters = new UInt32[NumPlayers, NPieceTypes, Board.NumSquaresExtended];

      //	Allocate array of butterfly counters (anoter move history heuristic)
      butterflyCounters = new UInt32[NumPlayers, NPieceTypes, Board.NumSquaresExtended];

      //	Allocate array of countermoves (move ordering countermove heuristic)
      countermoves = new UInt32[Board.NumSquaresExtended, Board.NumSquaresExtended];

      //	Allocate the MovementLists
      for (int x = 0; x < MAX_PLY; x++)
        moveLists[x] = new MoveList(Board, searchStack, killers1, killers2, historyCounters, butterflyCounters, x);
      moveLists[1].LegalMovesOnly = true;

      //	Build the lists of rules that handle specific 
      //	"events" (although not technically events in 
      //	a .NET sense.)  We'll call only these events during 
      //	thinking to greatly increase efficiency.

      //	GetPositionHashCode handlers
      findRulesThatHandleEvent("GetPositionHashCode", rulesHandlingGetPositionHashCode);

      //	MoveBeingGenerated handlers
      findRulesThatHandleEvent("MoveBeingGenerated", rulesHandlingMoveBeingGenerated);

      //	MoveBeingMade event
      findRulesThatHandleEvent("MoveBeingMade", rulesHandlingMoveBeingMade);

      //	MoveMade event
      findRulesThatHandleEvent("MoveMade", rulesHandlingMoveMade);

      //	MoveBeingUnmade event
      findRulesThatHandleEvent("MoveBeingUnmade", rulesHandlingMoveBeingUnmade);

      //	TestForWinLossDraw event
      findRulesThatHandleEvent("TestForWinLossDraw", rulesHandlingTestForWinLossDraw);
      evalHandlingTestForWinLossDraw = findEvalThatHandlesEvent("TestForWinLossDraw");

      //	NoMovesResult event
      findRulesThatHandleEvent("NoMovesResult", rulesHandlingNoMovesResult);

      //	GenerateSpecialMoves event
      findRulesThatHandleEvent("GenerateSpecialMoves", rulesHandlingGenerateSpecialMoves);

      //	PositionalSearchExtension event
      findRulesThatHandleEvent("PositionalSearchExtension", rulesHandlingSearchExtensions);

      //	IsSquareAttacked event
      findRulesThatHandleEvent("IsSquareAttacked", rulesHandlingIsSquareAttacked);

      //	AdjustEvaluation event
      findRulesThatHandleEvent("AdjustEvaluation", rulesHandlingAdjustEvaluation);
    }
    #endregion

    #region postInitialize
    protected virtual void postInitialize()
    {
      //	post-initialize all rules
      foreach (Rule rule in rules)
        rule.PostInitialize();

      //  post-initialize all evaluations
      foreach (Evaluation evaluation in evaluations)
        evaluation.PostInitialize();

      //	initialize the BoardMoveStack, which stores the list of 
      //	moves that have actually been played on the board
      BoardMoveStack = new BoardMoveStack(Board);

      //	initialize the game based on the starting FEN
      LoadFEN(FENStart);

      //	calculate the Betza mobilities for the pieces...
      //	first, determine board occupation density
      int totalPieces = 0;
      for (int sq = 0; sq < Board.NumSquares; sq++)
        if (Board[sq] != null)
          totalPieces++;
      //double density = (double) totalPieces / (double) Board.NumberOfSquares;
      //	now each piece type can calculate its Betza mobility
      for (int nPieceType = 0; nPieceType < nPieceTypes; nPieceType++)
        pieceTypes[nPieceType].CalculateMobilityStatistics(this, 0.7);

      MidgameMaterialThreshold = (int)((Board.GetPlayerMaterial(0) + Board.GetPlayerMaterial(1)) * 0.80);
      EndgameMaterialThreshold = (int)((Board.GetPlayerMaterial(0) + Board.GetPlayerMaterial(1)) * 0.30);
    }
    #endregion

    #region Variable Handling
    protected void HandleUnassigned(InitializationHelper helper)
    {
      bool hasUnassigned = false;
      PropertyInfo[] properties = GetType().GetProperties();
      foreach (PropertyInfo property in properties)
      {
        object[] attributes = property.GetCustomAttributes(typeof(GameVariableAttribute), false);
        if (attributes.Length > 0)
        {
          if (property.PropertyType == typeof(ChoiceVariable))
          {
            ChoiceVariable choice = (ChoiceVariable)property.GetValue(this, null);
            if (choice.Choices.Count > 0 && choice.Value == null)
            {
              //	this property is unassigned
              hasUnassigned = true;
              if (helper == null)
              {
                //	no helper is provided so we will set a random value
                int choicenumber = Program.Random.Next(choice.Choices.Count);
                choice.Value = choice.Choices[choicenumber];
              }
              else
                break;
            }
          }
          else if (property.PropertyType == typeof(IntRangeVariable))
          {
            IntRangeVariable val = (IntRangeVariable)property.GetValue(this, null);
            if (val.Value == null)
            {
              //	this property is unassigned
              hasUnassigned = true;
              if (helper == null)
              {
                //	no helper is provided so we will set a random value
                val.Value = Program.Random.Next((int)val.MaxValue - (int)val.MinValue + 1) + val.MinValue;
              }
              else
                break;
            }
          }
        }
      }
      //	a helper function was provided so call it and let it assign values 
      //	to the unassigned variables (by asking the user, for example)
      if (hasUnassigned && helper != null)
        helper(this);
    }

    public void HandleDefinitions(string definitions)
    {
      if (definitions != null)
      {
        string[] split = definitions.Split(';');
        foreach (string definition in split)
        {
          int cursor = 0;
          //	scan until we find '='
          for (; cursor < definition.Length && definition[cursor] != '='; cursor++)
            ;
          if (cursor == definition.Length)
            throw new Exception("Error in variable definition; unexpected end searching for =");
          string variableName = definition.Substring(0, cursor).Trim();
          cursor++;
          while (cursor < definition.Length && Char.IsWhiteSpace(definition[cursor]))
            cursor++;
          if (cursor == definition.Length)
            throw new Exception("Error in variable definition; encountered end before value");
          string value = null;
          if (definition[cursor] == '"')
            // TODO
            value = null;
          else if (definition[cursor] == '\'')
            // TODO
            value = null;
          else
            value = definition.Substring(cursor).Trim();
          HandleDefinition(variableName, value);
        }
      }
    }

    public void HandleDefinitions(Dictionary<string, string> definitions)
    {
      if (definitions != null)
      {
        foreach (KeyValuePair<string, string> pair in definitions)
          HandleDefinition(pair.Key, pair.Value);
      }
    }

    private void HandleDefinition(string variableName, string value)
    {
      PropertyInfo[] properties = GetType().GetProperties();
      foreach (PropertyInfo property in properties)
      {
        object[] attributes = property.GetCustomAttributes(typeof(GameVariableAttribute), false);
        if (attributes.Length > 0)
        {
          if (property.Name == variableName)
          {
            if (property.PropertyType == typeof(string))
              property.SetValue(this, value, null);
            else if (property.PropertyType == typeof(ChoiceVariable))
            {
              ChoiceVariable choicevar = (ChoiceVariable)property.GetValue(this, null);
              choicevar.Value = value;
            }
            else if (property.PropertyType == typeof(IntRangeVariable))
            {
              IntRangeVariable intvar = (IntRangeVariable)property.GetValue(this, null);
              intvar.Value = Convert.ToInt32(value);
            }
          }
        }
      }
    }
    #endregion

    #region parseStartingArray
    protected void parseStartingArray(string array)
    {
      //	get the Board class to parse the array string into a piece map
      Dictionary<int, GenericPiece> pieceMap = Board.ArrayToPieceMap(TypesByNotation, array);
      //	initialize the StartingPieces map of initial piece placement by notation
      StartingPieces = new Dictionary<string, GenericPiece>();

      for (int square = 0; square < Board.NumSquares; square++)
      {
        if (!pieceMap.ContainsKey(square))
          throw new Exception("The Array does not cover the board");
        string squareNotation = GetSquareNotation(square);
        if (pieceMap[square] != null)
          StartingPieces.Add(squareNotation, pieceMap[square]);
        else if (squareNotation.IndexOf(' ') < 0)
          //	Add in null values for square with no starting piece, but only 
          //	if the notation doesn't contain a space.  Space is a generic 
          //	placeholder for games that override GetSquareNotation such as 
          //	OmegaChess.
          StartingPieces.Add(squareNotation, null);
      }

      //	initialize the StartingPieceSquares index of squares occupied at game start
      StartingPieceSquares = new int[NumPlayers, Board.NumSquaresExtended];
      StartingPieceCount = new int[NumPlayers];
      for (int player = 0; player < NumPlayers; player++)
      {
        StartingPieceCount[player] = 0;
        for (int square = 0; square < Board.NumSquares; square++)
        {
          StartingPieceSquares[player, square] = 0;
          if (pieceMap.ContainsKey(square) && pieceMap[square] != null && pieceMap[square].Player == player)
          {
            StartingPieceSquares[player, square] = 1;
            StartingPieceCount[player]++;
          }
        }
        for (int square = Board.NumSquares; square < Board.NumSquaresExtended; square++)
          StartingPieceSquares[player, square] = 0;
      }
    }
    #endregion

    #region placePiecesByArray
    protected void placePiecesByArray(string array)
    {
      //	get the Board class to parse the array string into a piece map
      Dictionary<int, GenericPiece> pieceMap = Board.ArrayToPieceMap(TypesByNotation, array);
      //  set ArrayBeingLoaded to turn off various event handlers during board setup
      ArrayBeingLoaded = true;
      //	iterate through the map, placing pieces
      foreach (KeyValuePair<int, GenericPiece> pair in pieceMap)
        if (pair.Value != null)
          AddPiece(new Piece(this, pair.Value, pair.Key));
      //	turn off ArrayBeingLoaded - we are through setting up the board
      ArrayBeingLoaded = false;
    }
    #endregion


    #region ** Overridable Game Initialization Functions **
    #region CreateBoard
    public virtual Board CreateBoard(int nPlayers, int nFiles, int nRanks, Symmetry symmetry)
    { return new Board(nFiles, nRanks); }
    #endregion

    #region SetGameVariables
    public virtual void SetGameVariables()
    {
      Name = GameAttribute.GameName;
      Invented = GameAttribute.Invented;
      InventedBy = GameAttribute.InventedBy;
      PlayerNames[0] = "White";
      PlayerNames[1] = "Black";
      DeduplicateMoves = false;
      StaticExchangeEvaluation = true;
      NumberOfSquareColors = 2;
    }
    #endregion

    #region SetOtherVariables
    public virtual void SetOtherVariables()
    {
    }
    #endregion

    #region AddPieceTypes
    public virtual void AddPieceTypes()
    { }
    #endregion

    #region ExpandVariablesInFEN
    public virtual void ExpandVariablesInFEN()
    {
      //	Loop until there are no more substitutions.
      //	This accomodates variables contained in other variables.
      while (true)
      {
        string newFENStart = FENStart;
        Regex regex = new Regex("#{[A-Za-z_][A-Za-z0-9_]*}");
        MatchCollection matches = regex.Matches(FENStart);
        if (matches.Count == 0)
          break;
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
          string variable = match.Value.Substring(2, match.Value.Length - 3);
          object value = LookupGameVariable(variable);
          if (value == null)
          {
            value = GetCustomProperty(variable);
            if (value == null)
              throw new Exception("Unrecognized game variable in FEN: " + match.Value);
          }
          newFENStart = newFENStart.Replace(match.Value, value.ToString());
        }
        FENStart = newFENStart;
      }
    }
    #endregion

    #region LookupGameVariable
    public virtual object LookupGameVariable(string variableName)
    {
      PropertyInfo[] properties = GetType().GetProperties();
      foreach (PropertyInfo property in properties)
      {
        object[] attributes = property.GetCustomAttributes(typeof(GameVariableAttribute), false);
        if (attributes.Length > 0)
        {
          if (property.Name == variableName)
            return property.GetValue(this, null);
        }
      }
      return GetCustomProperty(variableName);
    }
    #endregion

    #region PerformSymbolExpansion
    public virtual string PerformSymbolExpansion(string input)
    {
      string translation = input;
      while (translation.IndexOf("#{") >= 0)
      {
        int start = translation.IndexOf("#{");
        int end = translation.IndexOf("}", start + 2);
        if (end > start)
        {
          string token = translation.Substring(start + 2, end - start - 2);
          object lookup = LookupGameVariable(token);
          string translated = lookup == null ? "#UNDEFINED" : lookup.ToString();
          translation = translation.Substring(0, start) + translated + translation.Substring(end + 1);
        }
        else
          translation = translation.Substring(0, start) + translation.Substring(start + 2);
      }
      return translation;
    }
    #endregion

    #region GetAllGameVariables
    public virtual Dictionary<string, object> GetAllGameVariables()
    {
      Dictionary<string, object> returnmap = new Dictionary<string, object>();
      PropertyInfo[] properties = GetType().GetProperties();
      foreach (PropertyInfo property in properties)
      {
        object[] attributes = property.GetCustomAttributes(typeof(GameVariableAttribute), false);
        if (attributes.Length > 0)
        {
          object original = LookupGameVariable(property.Name);
          //object original = property.GetValue( this, null );
          if (original is ICloneable)
            returnmap.Add(property.Name, ((ICloneable)original).Clone());
          else
            returnmap.Add(property.Name, original);
        }
      }
      return returnmap;
    }
    #endregion

    #region AddRules
    public virtual void AddRules()
    {
      //	add default MoveCompletionRule
      AddRule(new MoveCompletionDefaultRule());

      //	add rules automatically applied by PieceTypeProperties
      for (int ntype = 0; ntype < nPieceTypes; ntype++)
      {
        Attribute[] propertyAttributes = pieceTypes[ntype].GetCustomAttributes(typeof(PieceTypePropertyAttribute));
        foreach (Attribute propertyAttribute in propertyAttributes)
          ((PieceTypePropertyAttribute)propertyAttribute).AddRules(pieceTypes[ntype], this);
      }
    }
    #endregion

    #region ReorderRules
    public virtual void ReorderRules()
    {
      //	put the MoveCompletionRule at the front of the list
      rules.Remove(moveCompletionRule);
      rules.Insert(0, moveCompletionRule);
    }
    #endregion

    #region AddEvaluations
    public virtual void AddEvaluations()
    {
    }
    #endregion

    #region LoadFEN
    public virtual void LoadFEN(string newFEN)
    {
      //	place pieces as indicated in the 'array' portion
      fen = new FEN(FENFormat, newFEN);
      try
      {
        placePiecesByArray(fen["array"]);
      }
      catch (Exception ex)
      {
        throw new Exceptions.FENParseFailureException("Array", fen["array"], ex.Message, ex);
      }

      //	notify all rules so they can initialize with information 
      //	contained in the fen as necessary (castling rights, etc.)
      foreach (Rule rule in rules)
        rule.PositionLoaded(fen);
      gameHistoryCount = 0;
      Ply = 1;
      generateMoves(CurrentSide, 1, 0);
    }
    #endregion

    #region ClearGameState
    public virtual void ClearGameState()
    {
      Board.ClearBoard();
      pieces = new Piece[NumPlayers, MAX_PIECES];
      nPieces = new int[NumPlayers];
      moveLists = new MoveList[MAX_PLY];
      searchStack = new SearchStack[MAX_PLY];
      killers1 = new UInt32[MAX_PLY];
      killers2 = new UInt32[MAX_PLY];
      for (int x = 0; x < MAX_PLY; x++)
        searchStack[x].Initialize();
      gameHistory = new MoveInfo[MAX_GAME_LENGTH];
      gameHistoryTurnNumbers = new int[MAX_GAME_LENGTH];
      gameHistoryCount = 0;
      hashtable = null;
      foreach (Rule rule in rules)
        rule.ClearGameState();
      //	Initialize game Result
      Result = new ChessV.Result(ResultType.NoResult);
      //	Allocate array of history counters
      historyCounters = new UInt32[NumPlayers, NPieceTypes, Board.NumSquaresExtended];
      //	Allocate array of butterfly counters
      butterflyCounters = new UInt32[NumPlayers, NPieceTypes, Board.NumSquaresExtended];
      //	Allocate array of countermoves
      countermoves = new UInt32[Board.NumSquaresExtended, Board.NumSquaresExtended];
      //	Allocate the MovementLists
      for (int x = 0; x < MAX_PLY; x++)
        moveLists[x] = new MoveList(Board, searchStack, killers1, killers2, historyCounters, butterflyCounters, x);
      moveLists[1].LegalMovesOnly = true;
    }
    #endregion

    #region AddEngine
    public void AddEngine(EngineConfigurationWithAdaptor engineConfig, int side)
    {
      EngineBuilder builder = new EngineBuilder(messageLog, engineConfig.Configuration);
      Match.SetPlayer(side, builder.Create(TimerFactory), engineConfig.Adaptor);
    }
    #endregion

    #region AddInternalEngine
    public void AddInternalEngine(int side)
    {
      Match.SetPlayer(side, new InternalEngine(messageLog, TimerFactory));
    }
    #endregion

    #region AddHuman
    public void AddHuman(int side)
    {
      Match.SetPlayer(side, new HumanPlayer(messageLog, TimerFactory));
    }
    #endregion

    #region StartMatch
    public virtual void StartMatch()
    {
      Match = new Match(this, new PGNGame(), messageLog, TimerFactory);
    }
    #endregion

    #region StartGame
    public virtual void StartGame()
    {
      if (Match != null)
        Match.Start();
    }
    #endregion
    #endregion


    #region Protected - GenerateMoves
    protected void generateMoves(int player, int ply, UInt32 movehash, bool capturesOnly = false)
    {
      UInt32 countermove = ply == 1 ? 0 : countermoves[SearchPath[ply - 1].FromSquare, SearchPath[ply - 1].ToSquare];
      moveLists[ply].Reset(movehash, countermove);
      for (int nPiece = 0; nPiece < nPieces[player]; nPiece++)
        if (pieces[player, nPiece].Square >= 0)
          pieces[player, nPiece].GenerateMoves(moveLists[ply], capturesOnly);
      GenerateSpecialMoves(moveLists[ply], capturesOnly);
    }
    #endregion

    #region TryCreateAdaptor
    public virtual EngineGameAdaptor TryCreateAdaptor(EngineConfiguration config)
    {
      return null;
    }
    #endregion

    #region VariationChanged
    public void VariationChanged()
    {
      //	recalculate PSTs for all piece types
      for (int x = 0; x < nPieceTypes; x++)
        pieceTypes[x].InitializePST(Variation);

      //	recalculate the incrementally updated material evaluations
      //	since these also incorporate the PST values
      Board.RecalculateMaterialEvaluations();

      //	update the variation for all evaluations
      foreach (Evaluation evaluation in evaluations)
        evaluation.SetVariation(Variation);
    }
    #endregion


    // *** BOARD GEOMETRY AND DIRECTIONS *** //

    #region Board Geometry and Attacking Directions
    public int NDirections
    { get { return nDirections; } }

    public int NSlidingDirections
    { get { return nSlidingDirections; } }

    public Direction GetDirection(int nDirection)
    { return directions[nDirection]; }

    public int GetDirections(out Direction[] directions)
    { directions = this.directions; return nDirections; }

    public int GetDirectionNumber(Direction direction)
    {
      for (int x = 0; x < nDirections; x++)
        if (direction == directions[x])
          return x;
      throw new Exception("Unknown direction");
    }

    public int DirectionLookup(ref MoveInfo move)
    { return playerDirections[move.Player, Board.DirectionLookup(move.FromSquare, move.ToSquare)]; }

    public int DirectionLookup(Movement move)
    { return playerDirections[move.Player, Board.DirectionLookup(move.FromSquare, move.ToSquare)]; }

    public int GetPieceTypes(out PieceType[] pieceTypes)
    { pieceTypes = this.pieceTypes; return nPieceTypes; }

    public int PlayerDirection(int player, int nDirection)
    { return playerDirections[player, nDirection]; }

    public int OppositeDirection(int nDirection)
    { return oppositeDirections[nDirection]; }

    public int MaxAttackRange(int player, int nDirection)
    { return maxAttackRange[player, nDirection]; }
    #endregion


    // *** PIECES AND PIECE TYPES *** //

    #region Pieces and Piece Types
    public int NPieceTypes
    { get { return nPieceTypes; } }

    public PieceType GetPieceType(int pieceTypeNumber)
    { return pieceTypes[pieceTypeNumber]; }

    public int NDisabledPieceTypes
    { get { return nDisabledPieceTypes; } }

    public int GetPieceTypeNumber(PieceType type)
    { return pieceTypeNumbers[type]; }

    public PieceType GetTypeByNotation(string notation)
    {
      if (TypesByNotation[0].ContainsKey(notation))
        return TypesByNotation[0][notation];
      if (TypesByNotation[1].ContainsKey(notation))
        return TypesByNotation[1][notation];
      return null;
    }

    #region AddPieceType
    public PieceType AddPieceType(PieceType type)
    {
#if DEBUG
			//	Debug mode only - track which classes define which types 
			//	for the purpose of automatic documentation generation
			StackTrace st = new StackTrace( false );
			for( int x = 0; x < st.FrameCount; x++ )
			{
				StackFrame sf = st.GetFrame( x );
				MethodBase mb = sf.GetMethod();
				if( mb.Name == "AddPieceTypes" )
				{
					classesThatCreatedPieceTypes.Add( type, mb.DeclaringType );
					break;
				}
			}
#endif

      pieceTypes[nPieceTypes] = type;
      if (type.Name != null)
        SetCustomProperty(type.Name, type);
      return pieceTypes[nPieceTypes++];
    }

    public PieceType AddPieceType(Type pieceType, string name, string notation, int midgameValue, int endgameValue, string preferredImageName = null)
    {
      ConstructorInfo ci = pieceType.GetConstructor(new Type[] { typeof(string), typeof(string), typeof(int), typeof(int), typeof(string) });
      pieceTypes[nPieceTypes] = (PieceType)ci.Invoke(new object[] { name, notation, midgameValue, endgameValue, preferredImageName });
      SetCustomProperty(name, pieceTypes[nPieceTypes]);
      return pieceTypes[nPieceTypes++];
    }
    #endregion

    public Type GetClassThatCreatedPieceType(PieceType type)
    {
      return classesThatCreatedPieceTypes.ContainsKey(type) ? classesThatCreatedPieceTypes[type] : null;
    }

    public List<PieceType> ParseTypeListFromString(string types)
    {
      List<PieceType> typelist = new List<PieceType>();
      int cursor = 0;
      while (cursor < types.Length)
      {
        PieceType pieceType = ParsePieceTypeFromString(types, ref cursor);
        typelist.Add(pieceType);
      }
      return typelist;
    }

    public PieceType ParsePieceTypeFromString(string str, ref int cursor)
    {
      PieceType pieceType;
      if (str[cursor] == '_')
      {
        //	an underscore forces the recognition of a two-character piece notation
        if (cursor + 2 >= str.Length)
          throw new Exception("Failure to parse piece type in Game.ParsePieceTypeFromString: " + str.Substring(cursor));
        string notation = str.Substring(cursor, 3);
        //	first, try to find the notation in the map with the underscore
        bool found = TypesByNotation[0].TryGetValue(notation, out pieceType);
        if (!found)
          found = TypesByNotation[1].TryGetValue(notation, out pieceType);
        if (!found)
          //	try finding without underscore (which would have been unnecessary if the first 
          //	character of the two-character notation is not also a valid single-char notation)
          found = TypesByNotation[0].TryGetValue(notation.Substring(1), out pieceType);
        if (!found)
          found = TypesByNotation[1].TryGetValue(notation.Substring(1), out pieceType);
        if (!found)
          throw new Exception("Piece type not found in Game.ParsePieceTypeFromString: " + notation);
        cursor += 3;
        return pieceType;
      }
      else if ((str[cursor] >= 'A' && str[cursor] <= 'Z') ||
           (str[cursor] >= 'a' && str[cursor] <= 'z') ||
           (str[cursor] >= 'À' && str[cursor] <= 'Ö') ||
           (str[cursor] >= 'à' && str[cursor] <= 'ö'))
      {
        //	A character designates the beginning of what might be a one or two character 
        //	long notation of a piece
        int start = cursor++;

        //	Check to see if this character is the notation of a valid piece type.  If it is we go with it.
        //	The first character of a two-character notation can't overlap with a single character notation 
        //	unless preceeded with an underscore (handled above)
        bool found = TypesByNotation[0].TryGetValue(str.Substring(start, 1), out pieceType);
        if (!found)
          found = TypesByNotation[1].TryGetValue(str.Substring(start, 1), out pieceType);
        if (!found &&
          ((str[cursor] >= 'A' && str[cursor] <= 'Z') ||
           (str[cursor] >= 'a' && str[cursor] <= 'z') ||
           (str[cursor] >= 'À' && str[cursor] <= 'Ö') ||
           (str[cursor] >= 'à' && str[cursor] <= 'ö') ||
           str[cursor] == '!' || str[cursor] == '\''))
        {
          found = TypesByNotation[0].TryGetValue(str.Substring(start, 2), out pieceType);
          if (!found)
            found = TypesByNotation[1].TryGetValue(str.Substring(start, 2), out pieceType);
          cursor++;
        }
        if (!found)
          throw new Exception("Unrecognized type notation passed to Game.ParsePieceTypeFromString: " + str[start]);
        return pieceType;
      }
      throw new Exception("Failure to parse piece type in Game.ParsePieceTypeFromString: " + str.Substring(cursor));
    }

    public List<Piece> GetPieceList()
    {
      List<Piece> pieceList = new List<Piece>();
      for (int player = 0; player < NumPlayers; player++)
        for (int piece = 0; piece < nPieces[player]; piece++)
          if (pieces[player, piece].Square >= 0)
            pieceList.Add(pieces[player, piece]);
      return pieceList;
    }

    public List<Piece> GetPieceList(int player)
    {
      List<Piece> pieceList = new List<Piece>();
      for (int piece = 0; piece < nPieces[player]; piece++)
        if (pieces[player, piece].Square >= 0)
          pieceList.Add(pieces[player, piece]);
      return pieceList;
    }

    public List<Piece> GetCapturedPieceList(int player)
    {
      List<Piece> pieceList = new List<Piece>();
      for (int piece = 0; piece < nPieces[player]; piece++)
        if (pieces[player, piece].Square < 0)
          pieceList.Add(pieces[player, piece]);
      return pieceList;
    }

    public void AddPiece(Piece piece)
    {
      pieces[piece.Player, nPieces[piece.Player]++] = piece;
      if (piece.Square >= 0)
        Board[piece.Square] = piece;
    }

    public List<string> GetPieceTypeNotes(PieceType type)
    {
      List<string> notes = new List<string>();
      foreach (Rule rule in rules)
        rule.GetNotesForPieceType(type, notes);
      foreach (Evaluation eval in evaluations)
        eval.GetNotesForPieceType(type, notes);
      return notes;
    }
    #endregion


    // *** MOVES *** //

    #region GetRootMoves
    public int GetRootMoves(out MoveInfo[] moves)
    { return moveLists[1].GetMoves(out moves); }

    public int GetRootMoves(out MoveInfo[] moves, Piece movingPiece)
    {
      int moveCount = 0;
      MoveInfo[] allMoves;
      int nMoves = moveLists[1].GetMoves(out allMoves);
      for (int x = 0; x < nMoves; x++)
        if (allMoves[x].PieceMoved == movingPiece)
          moveCount++;
      moves = new MoveInfo[moveCount];
      for (int x = 0, y = 0; x < nMoves; x++)
        if (allMoves[x].PieceMoved == movingPiece)
          moves[y++] = allMoves[x];
      return moveCount;
    }
    #endregion

    #region GetHistoricalMove
    public MoveInfo GetHistoricalMove(int moveNumber)
    {
      if (gameHistoryCount <= moveNumber)
        throw new Exception("Error in Game.GetHistoricalMove - invalid move number specified");
      return gameHistory[moveNumber];
    }

    public MoveInfo GetHistoricalMove(int moveNumber, out int turnNumber)
    {
      if (gameHistoryCount <= moveNumber)
        throw new Exception("Error in Game.GetHistoricalMove - invalid move number specified");
      turnNumber = gameHistoryTurnNumbers[moveNumber];
      return gameHistory[moveNumber];
    }
    #endregion

    #region MakeMove
    //	MakeMove is used to instruct the Game class to perform the given 
    //	move; highlightMove specifies whether the move should be highlighted.
    //	NOTE: This function makes an actual move on the board which is 
    //	committed to the game history.  Moves made/unmade in a search 
    //	do not call this.
    public void MakeMove(MoveInfo move, bool highlightMove)
    {
      //	make the move
      if (!moveLists[1].MakeMove(move))
        throw new Exception("Game.PerformMove: invalid move specified");

      //	enter the move into the game history
      BoardMoveStack.MakingMove(moveLists[1], move);
      gameHistoryTurnNumbers[gameHistoryCount] = GameTurnNumber;
      gameHistory[gameHistoryCount++] = move;
      //			Ply = 1;


      //	send the MoveMade message to all rules that handle it
      foreach (Rule rule in rulesHandlingMoveMade)
        rule.MoveMade(move, Ply);

      //	store move highlighting information
      if (highlightMove)
      {
        //	Clear out the previous highligh squares only if the previous move was not 
        //	made by the same player (so in the case of multi-move variants, we see the 
        //	highlight squares for all moves made at once.)
        if (BoardMoveStack.MoveCount < 2 ||
          BoardMoveStack.GetMove(BoardMoveStack.MoveCount - 2).Player != move.Player)
          HighlightSquares = new List<int>();
        //	Highlight the squares for this move
        if (move.MoveType != MoveType.Pass)
        {
          HighlightSquares.Add(move.FromSquare);
          HighlightSquares.Add(move.ToSquare);
        }
      }
      else
        HighlightSquares = null;
      if (Result.IsNone)
      {
        //	test for end-of-game
        MoveEventResponse response = TestForWinLossDraw(CurrentSide);
        if (response != MoveEventResponse.NotHandled)
        {
          if (response == MoveEventResponse.GameDrawn)
            Result = new Result(ResultType.Draw);
          else if (response == MoveEventResponse.GameLost)
            Result = new Result(ResultType.Win, CurrentSide ^ 1);
          else if (response == MoveEventResponse.GameWon)
            Result = new Result(ResultType.Win, CurrentSide);
        }
      }
      //	raise MoveBeingPlayed event first, then MovePlayed event
      MoveBeingPlayed?.Invoke(move);
      MovePlayed(move);
      if (move != lastMove)
      {
        ApmwCore.getInstance().NewMovePlayed.ForEach((handler) => handler(move));
        lastMove = move;
      }

      if (Result.IsNone)
      {
        //	generate moves for new position
        moveLists[1].Reset();
        Ply = 1;
        generateMoves(CurrentSide, Ply, 0);

        //	if no legal moves, determine result 
        if (moveLists[1].Count == 0)
        {
          MoveEventResponse result = NoMovesResult(CurrentSide);
          if (result == MoveEventResponse.GameDrawn)
            Result = new Result(ResultType.Draw);
          if (result == MoveEventResponse.GameWon)
            Result = new Result(ResultType.Win, CurrentSide);
          if (result == MoveEventResponse.GameLost)
            Result = new Result(ResultType.Win, CurrentSide ^ 1);

          ApmwCore.getInstance().MatchFinished.ForEach((handler) => handler(Match));
        }
      }
      else
      {
        //	perform some cleanup
        cleanup();
      }
    }

    /** used to prevent emitting extra moves for the same action, which keeps happening */
    MoveInfo lastMove;

    //	Version of PerformMove for when only a Movement is available, 
    //	not a MoveInfo.  This finds the appropriate MoveInfo from the 
    //	root MoveList and passes it to the other overload.
    public void MakeMove(Movement move, bool highlightMove)
    {
      MoveInfo[] moves;
      int nMoves = moveLists[1].GetMoves(out moves);
      for (int x = 0; x < nMoves; x++)
      {
        if (moves[x].Hash == move.Hash)
        {
          MakeMove(moves[x], highlightMove);
          return;
        }
      }
      throw new Exception("Attempt to execute an illegal move in Game::MakeMove");
    }
    #endregion

    #region UndoMove
    public void UndoMove(bool userCommand = false)
    {
      if (finalized)
        return;
      BoardMoveStack.UnmakeMove();
      gameHistoryCount--;
      moveLists[1].Reset();
      Ply = 1;
      generateMoves(CurrentSide, Ply, 0);
      MoveTakenBack?.Invoke();
      if (userCommand)
      {
        HighlightSquares.Clear();
        Result = new Result(ResultType.NoResult);
        Player player = Match.GetPlayer(CurrentSide);
        if (player is HumanPlayer)
          ((HumanPlayer)player).State = PlayerState.Thinking;
      }
    }
    #endregion

    #region PlayMoves
    public void PlayMoves(IEnumerable<string> moves, MoveNotation format = MoveNotation.StandardAlgebraic, List<ulong> hashcodes = null)
    {
      foreach (string moveNotation in moves)
      {
        Movement move = MoveFromDescription(moveNotation, format);
        if (move == null)
          throw new Exception("Notation does not describe a legal move: " + moveNotation);
        MakeMove(move, true);
        hashcodes?.Add(GetPositionHashCode(1));
      }
    }

    public void PlayMoves(string moves, MoveNotation format = MoveNotation.StandardAlgebraic)
    {
      PlayMoves(moves.Split(' '), format);
    }
    #endregion

    #region TakeBackMoves
    public void TakeBackMoves(int numMoves)
    {
      for (int x = 0; x < numMoves; x++)
        UndoMove();
    }
    #endregion

    #region GenerateSpecialMoves
    public void GenerateSpecialMoves(MoveList list, bool capturesOnly)
    {
      foreach (Rule rule in rulesHandlingGenerateSpecialMoves)
        rule.GenerateSpecialMoves(list, capturesOnly, Ply);
    }
    #endregion

    #region MoveBeingGenerated
    public bool MoveBeingGenerated(MoveList moves, int from, int to, MoveType type)
    {
      foreach (Rule rule in rulesHandlingMoveBeingGenerated)
        if (rule.MoveBeingGenerated(moves, from, to, type) != MoveEventResponse.NotHandled)
          return true;
      return false;
    }
    #endregion

    #region MoveBeingMade
    public bool MoveBeingMade(MoveInfo move)
    {
      bool moveIsLegal = true;

      //	send the MoveBeingMade message to all rules that handle it
      foreach (Rule rule in rulesHandlingMoveBeingMade)
        if (rule.MoveBeingMade(move, Ply) == MoveEventResponse.IllegalMove)
          //	rule has declared this move illegal
          moveIsLegal = false;

      //	send the message to all evaluations
      foreach (Evaluation evaluation in evaluations)
        evaluation.MoveBeingMade(move, Ply);

      //	send the message to the one and only MoveCompletionRule 
      //	which updates the CurrentPlayer and MoveNumber appropriately 
      //	(supporting multi-move variants like Marseillais Chess)
      moveCompletionRule.CompleteMove(move, Ply);

      //	send the MoveMade message to all rules that handle it
      foreach (Rule rule in rulesHandlingMoveMade)
        rule.MoveMade(move, Ply);

      Ply++;

      return moveIsLegal;
    }
    #endregion

    #region MoveBeingUnmade
    public void MoveBeingUnmade(MoveInfo move)
    {
      Ply--;

      //	send the message to the one and only MoveCompletionRule 
      //	which updates the CurrentPlayer and MoveNumber appropriately 
      //	(supporting multi-move variants like Marseillais Chess)
      moveCompletionRule.UndoingMove();

      //	send the message to all rules that handle it
      foreach (Rule rule in rulesHandlingMoveBeingUnmade)
        rule.MoveBeingUnmade(move, Ply);

      //	send the message to all evaluations
      foreach (Evaluation evaluation in evaluations)
        evaluation.MoveBeingUnmade(move, Ply);
    }
    #endregion

    #region TestForWinLossDraw
    public MoveEventResponse TestForWinLossDraw(int currentPlayer)
    {
      foreach (Rule rule in rulesHandlingTestForWinLossDraw)
      {
        MoveEventResponse response = rule.TestForWinLossDraw(currentPlayer, Ply);
        if (response != MoveEventResponse.NotHandled)
          return response;
      }
      if (evalHandlingTestForWinLossDraw != null)
        return evalHandlingTestForWinLossDraw.TestForWinLossDraw(currentPlayer, Ply);
      return MoveEventResponse.NotHandled;
    }
    #endregion

    #region NoMoves
    public MoveEventResponse NoMovesResult(int currentPlayer)
    {
      foreach (Rule rule in rulesHandlingNoMovesResult)
      {
        MoveEventResponse response = rule.NoMovesResult(currentPlayer, Ply);
        if (response != MoveEventResponse.NotHandled)
          return response;
      }
      throw new Exception("No rule handled the NoMovesResult message");
    }
    #endregion

    #region CanPruneMove
    public virtual bool CanPruneMove(MoveInfo move)
    { return true; }
    #endregion

    #region DescribeMove
    public virtual string DescribeMove(MoveInfo move, MoveNotation format)
    {
      //	first, check to see if a rule provides a custom description
      string description = null;
      foreach (Rule rule in rules)
        if (rule.DescribeMove(move, format, ref description) == MoveEventResponse.Handled)
          return description;

      //	next, if we are seeking format MoveNotation.MoveSelectionText,
      //	we don't provide that here unless it was handled above.  Instead, 
      //	those descriptions get provided in MoveSelectForm.cs.  That is probably 
      //	not the best place for it, but we can't do it here because describing 
      //	the choices to dis-ambiguate moves can provide better descriptions if 
      //	all the potential moves being selected from are known.  Here we only 
      //	know the one move, not the other options from which the user is selecting.
      if (format == MoveNotation.MoveSelectionText)
        return null;

      #region Format: Standard Algebraic
      if (format == MoveNotation.StandardAlgebraic)
      {
        if (move.MoveType == MoveType.Drop)
        {
          description = move.PieceMoved.PieceType.Notation + "@" +
            GetSquareNotation(move.ToSquare);
          return description;
        }
        else if (move.MoveType == MoveType.NullMove)
          return "NULL";
        else
        {
          description =
            GetSquareNotation(move.FromSquare) +
            GetSquareNotation(move.ToSquare);
          if ((move.MoveType & (MoveType.PromotionProperty | MoveType.DropOrReplaceProperty)) != 0)
            description += GetPieceType(move.PromotionType).Notation;
          return description;
        }
      }
      #endregion

      #region Format: XBoard
      else if (format == MoveNotation.XBoard)
      {
        if (move.MoveType == MoveType.Drop)
        {
          description = move.PieceMoved.PieceType.Notation + "@" +
            Convert.ToChar('a' + Board.GetFile(move.ToSquare)) +
              (Board.NumRanks == 10 ?
               Board.GetRank(move.ToSquare).ToString() :
               (Board.GetRank(move.ToSquare) + 1).ToString());
          return description;
        }
        else
        {
          description =
            Convert.ToChar('a' + Board.GetFile(move.FromSquare)) +
              (Board.NumRanks == 10 ?
               Board.GetRank(move.FromSquare).ToString() :
               (Board.GetRank(move.FromSquare) + 1).ToString()) +
            Convert.ToChar('a' + Board.GetFile(move.ToSquare)) +
              (Board.NumRanks == 10 ?
               Board.GetRank(move.ToSquare).ToString() :
               (Board.GetRank(move.ToSquare) + 1).ToString());
          if ((move.MoveType & (MoveType.PromotionProperty | MoveType.DropOrReplaceProperty)) != 0)
            description += GetPieceType(move.PromotionType).Notation;
          //	EXCEPTION - Castling in Shuffle Variants uses O-O notation
          if (move.MoveType == MoveType.Castling && GameAttribute.TagList.Contains("Random Array") &&
              GameAttribute.GameName != "Chess256" /* Cheesy hack! */ )
          {
            if (move.FromSquare < move.ToSquare)
              description = "O-O";
            else
              description = "O-O-O";
          }
          return description;
        }
      }
      #endregion

      #region Format: Descriptive
      else if (format == MoveNotation.Descriptive)
      {
        if (move.MoveType == MoveType.Drop)
        {
          return move.PieceMoved.PieceType.NotationClean[move.Player].ToUpper() + " * " +
            GetSquareNotation(move.ToSquare);
        }
        else if (move.MoveType == MoveType.Castling)
        {
          if (move.FromSquare > move.ToSquare)
            return "O-O-O";
          else
            return "O-O";
        }
        else if (move.MoveType == MoveType.NullMove)
          return "NULL";
        else if (move.MoveType == MoveType.Pass)
          return "Pass";
        else if (move.MoveType == MoveType.MoveRelay)
        {
          return
            GetSquareNotation(move.FromSquare) + "-" +
            GetSquareNotation(move.ToSquare) + " " +
            GetSquareNotation(move.ToSquare) + "-" +
            GetSquareNotation(move.Tag);
        }

        description = GetSquareNotation(move.FromSquare);
        if (move.MoveType == MoveType.StandardMove ||
          move.MoveType == MoveType.MoveWithPromotion ||
          move.MoveType == MoveType.MoveReplace)
        {
          description += " - " +
            GetSquareNotation(move.ToSquare);
          if ((move.MoveType & (MoveType.PromotionProperty | MoveType.DropOrReplaceProperty)) != 0)
            description += " = " + GetPieceType(move.PromotionType).NotationClean[move.Player].ToUpper();
        }
        else if (move.MoveType == MoveType.StandardCapture ||
          move.MoveType == MoveType.CaptureWithPromotion ||
          move.MoveType == MoveType.CaptureReplace)
        {
          description += " x " +
            GetSquareNotation(move.ToSquare);
          if ((move.MoveType & (MoveType.PromotionProperty | MoveType.DropOrReplaceProperty)) != 0)
            description += " = " + GetPieceType(move.PromotionType).NotationClean[move.Player].ToUpper();
          description += "\t" +
            GetPieceType(move.OriginalType).NotationClean[move.Player].ToUpper() + "x" +
            move.PieceCaptured.PieceType.NotationClean[move.Player ^ 1].ToUpper();
        }
        else if (move.MoveType == MoveType.BaroqueCapture)
        {
          if (move.ToSquare == move.Tag)
          {
            //	"rifle capture" - capturing piece doesn't move
            description = "x " +
              GetSquareNotation(move.ToSquare);
            description += "\tx" + move.PieceCaptured.PieceType.Notation[move.Player ^ 1].ToUpper().Replace("_", "") +
              " rifle capture";
          }
          else
          {
            description += " - " +
              GetSquareNotation(move.ToSquare);
            description += "(x " +
              GetSquareNotation(move.Tag) + ")";
            description += "\tx" + move.PieceCaptured.PieceType.Notation[move.Player ^ 1].ToUpper().Replace("_", "") +
              " baroque capture";
          }
        }
        else if (move.MoveType == MoveType.EnPassant)
        {
          description += " x " +
            GetSquareNotation(move.ToSquare);
          description += "\t" +
            GetPieceType(move.OriginalType).Notation[move.Player].ToUpper().Replace("_", "") + "x" +
            move.PieceCaptured.PieceType.Notation[move.Player ^ 1].ToUpper().Replace("_", "") + " e.p.";
        }
        return description;
      }
      #endregion

      return null;
    }

    public virtual string DescribeMove(UInt32 movehash, MoveNotation format)
    {
      #region Format: Standard Algebraic
      if (format == MoveNotation.StandardAlgebraic)
      {
        if (Movement.GetMoveTypeFromHash(movehash) == MoveType.Drop)
        {
          string description = GetPieceType(Movement.GetTagFromHash(movehash)).Notation[Movement.GetPlayerFromHash(movehash)] + "*" +
            GetSquareNotation(Movement.GetToSquareFromHash(movehash));
          return description;
        }
        else if (Movement.GetMoveTypeFromHash(movehash) == MoveType.NullMove)
          return "NULL";
        else if (Movement.GetMoveTypeFromHash(movehash) == MoveType.Pass)
          return "Pass";
        else
        {
          string description =
            GetSquareNotation(Movement.GetFromSquareFromHash(movehash)) +
            GetSquareNotation(Movement.GetToSquareFromHash(movehash));
          if ((Movement.GetMoveTypeFromHash(movehash) & (MoveType.PromotionProperty | MoveType.DropOrReplaceProperty)) != 0)
            description += GetPieceType(Movement.GetTagFromHash(movehash)).Notation[Movement.GetPlayerFromHash(movehash)].ToLower();
          return description;
        }
      }
      #endregion

      #region Format: XBoard
      else if (format == MoveNotation.XBoard)
      {
        if (Movement.GetMoveTypeFromHash(movehash) == MoveType.Drop)
        {
          int square = Movement.GetToSquareFromHash(movehash);
          string description = GetPieceType(Movement.GetTagFromHash(movehash)).Notation[Movement.GetPlayerFromHash(movehash)] + "@" +
            Convert.ToChar('a' + Board.GetFile(square)) +
              (Board.NumRanks == 10 ?
               Board.GetRank(square).ToString() :
               (Board.GetRank(square) + 1).ToString());
          return description;
        }
        else
        {
          string description =
            Convert.ToChar('a' + Board.GetFile(Movement.GetFromSquareFromHash(movehash))) +
              (Board.NumRanks == 10 ?
               Board.GetRank(Movement.GetFromSquareFromHash(movehash)).ToString() :
               (Board.GetRank(Movement.GetFromSquareFromHash(movehash)) + 1).ToString()) +
            Convert.ToChar('a' + Board.GetFile(Movement.GetToSquareFromHash(movehash))) +
              (Board.NumRanks == 10 ?
               Board.GetRank(Movement.GetToSquareFromHash(movehash)).ToString() :
               (Board.GetRank(Movement.GetToSquareFromHash(movehash)) + 1).ToString());
          if ((Movement.GetMoveTypeFromHash(movehash) & (MoveType.PromotionProperty | MoveType.DropOrReplaceProperty)) != 0)
            description += GetPieceType(Movement.GetTagFromHash(movehash)).Notation[Movement.GetPlayerFromHash(movehash)].ToLower();
          //	EXCEPTION - Castling in Shuffle Variants uses O-O notation
          if (Movement.GetMoveTypeFromHash(movehash) == MoveType.Castling && GameAttribute.TagList.Contains("Random Array") &&
            GameAttribute.GameName != "Chess256" /* Cheesy hack! */ )
          {
            if (Movement.GetToSquareFromHash(movehash) < Movement.GetToSquareFromHash(movehash))
              description = "O-O";
            else
              description = "O-O-O";
          }
          return description;
        }
      }
      #endregion

      else
        throw new Exception("DescribeMove: format not implemented from move hash");
    }

    public virtual string DescribeMove(Movement move, MoveNotation format)
    {
      return DescribeMove(move.Hash, format);
    }
    #endregion

    #region MoveFromDescription
    public virtual Movement MoveFromDescription(string description, MoveNotation format)
    {
      //	check for a drop move
      if (description.IndexOf('@') > 0)
      {
        string[] split = description.Split('@');
        //	find piece type
        PieceType type = null;
        for (int x = 0; x < nPieceTypes && type == null; x++)
          if (pieceTypes[x].Notation[0] == split[0] || pieceTypes[x].Notation[1] == split[0])
            type = pieceTypes[x];
        if (type == null)
          throw new Exception("!");
        int square = notationToSquare(split[1], format);
        if (square == -1)
          throw new Exception("!");
        MoveInfo[] moves;
        int nMoves = GetRootMoves(out moves);
        for (int x = 0; x < nMoves; x++)
        {
          if (moves[x].MoveType == MoveType.Drop && moves[x].PieceMoved.TypeNumber == type.TypeNumber && square == moves[x].ToSquare)
          {
            return new Movement(moves[x].FromSquare, square, moves[x].Player, moves[x].MoveType, moves[x].PromotionType);
          }
        }
      }
      //	check for castling notation
      if (description == "O-O" || description == "O-O-O")
      {
        MoveInfo[] moves;
        int nMoves = GetRootMoves(out moves);
        for (int x = 0; x < nMoves; x++)
        {
          if (moves[x].MoveType == MoveType.Castling)
          {
            if ((description == "O-O" && moves[x].FromSquare < moves[x].ToSquare) ||
              (description == "O-O-O" && moves[x].FromSquare > moves[x].ToSquare))
            {
              return new Movement(moves[x].FromSquare, moves[x].ToSquare, moves[x].Player, moves[x].MoveType, moves[x].PromotionType);
            }
          }
        }

      }
      //	try standard coordinate notation
      description = description.Trim();
      string promotionType = null;
      if (description.Length >= 4)
      {
        int cursor = 0;
        if (!Char.IsDigit(description[0]))
        {
          cursor = 1;
          while (cursor < description.Length && Char.IsDigit(description[cursor]))
            cursor++;
          string fromSquareNotation = description.Substring(0, cursor);
          if (cursor < description.Length)
          {
            int start = cursor++;
            while (cursor < description.Length && Char.IsDigit(description[cursor]))
              cursor++;
            string toSquareNotation = description.Substring(start, cursor - start);
            if (cursor < description.Length)
              promotionType = description.Substring(cursor);
            //	find the square for the locations we have parsed
            int fromSquare = notationToSquare(fromSquareNotation, format);
            int toSquare = notationToSquare(toSquareNotation, format);
            //	see if we have a move that matches this 
            MoveInfo[] moves;
            int nMoves = GetRootMoves(out moves);
            for (int x = 0; x < nMoves; x++)
            {
              if (fromSquare == moves[x].FromSquare && toSquare == moves[x].ToSquare)
              {
                //	this might be the move we are looking for - check promotion
                if ((moves[x].MoveType & MoveType.PromotionProperty) == 0 ||
                  promotionType.ToLower() == GetPieceType(moves[x].PromotionType).Notation[moves[x].Player].ToLower())
                {
                  return new Movement(fromSquare, toSquare, moves[x].Player, moves[x].MoveType, moves[x].PromotionType);
                }
              }
            }
          }
        }
      }
      return null;
    }
    #endregion

    #region SaveGame
    public void SaveGame(TextWriter output)
    {
      //	Write the name of the variant
      output.WriteLine(GameAttribute.GameName);
      if (Match != null)
      {
        //	Write the names of the players
        if (Match.GetPlayer(0) != null)
          output.WriteLine("Player(" + PlayerNames[0] + ") = " + Match.GetPlayer(0).Name);
        if (Match.GetPlayer(1) != null)
          output.WriteLine("Player(" + PlayerNames[1] + ") = " + Match.GetPlayer(1).Name);
      }
      //	Write the variables that have custom definitions
      //	(e.g., those that have been changed since the 
      //	game object was first created)
      Dictionary<string, object> currentGameVariables = GetAllGameVariables();
      foreach (KeyValuePair<string, object> pair in currentGameVariables)
      {
        if (!GameVariableSnapshot.ContainsKey(pair.Key))
          writeGameVariableToFile(pair, output);
        else if (!compareGameVariablesForEquality(pair.Value, GameVariableSnapshot[pair.Key]))
          writeGameVariableToFile(pair, output);
      }
      //	Write the list of moves
      output.WriteLine("Moves = {");
      output.Write("   ");
      for (int x = 0; x < gameHistoryCount; x++)
        output.Write(" " + DescribeMove(gameHistory[x], MoveNotation.StandardAlgebraic));
      output.WriteLine();
      output.WriteLine("}");
      if (!Result.IsNone)
        output.WriteLine("Result = " + Result.VerboseString);
    }
    #endregion

    #region GetPositionHashCode
    //	Zobrist hash code of the current position
    public UInt64 GetPositionHashCode(int ply)
    {
      UInt64 hash = Board.HashCode;
      foreach (Rule rule in rulesHandlingGetPositionHashCode)
        hash = hash ^ rule.GetPositionHashCode(ply);
      return hash;
    }
    #endregion

    #region SEE_GE
    public bool SEE_GE(int from, int to, int value)

    /*	Static Exchange Evaluation (SEE) Greather Than or Equal
      Performs an SEE on the given move and determines only if 
      it is greather than or equal to the provided value.
      This logic comes from Stockfish.  This function only works 
      on basic from-to type moves (no promotion, baroque capture, 
      or other funny business.)  Also, SEE is disabled completely 
      in games where it wouldn't work, such as games with 
      Chinese Chess Cannon-type pieces.  */
    {
      Piece nextVictim = Board[from];
      int side = nextVictim.Player ^ 1;
      int balance = Board[to] == null ? 0 : Board[to].MidgameValue;

      if (balance < value)
        return false;

      if (nextVictim.MidgameValue == 0 /* Royal */ )
        return true;

      balance -= nextVictim.MidgameValue;

      if (balance >= value)
        return true;

      //	find all attackers and place them in the attacker lists
      seeAttackers[0].Clear();
      seeAttackers[1].Clear();
      int dir;
      for (dir = 0; dir < nSlidingDirections; dir++)
      {
        int steps = 1;
        int nextSquare = Board.NextSquare(dir, to);
        while (nextSquare >= 0)
        {
          Piece pieceOnSquare = Board[nextSquare];
          if (pieceOnSquare != null && nextSquare != from)
          {
            if (pieceOnSquare.GetAttackRange(OppositeDirection(dir)) >= steps)
              seeAttackers[pieceOnSquare.Player].Add(pieceOnSquare);
            nextSquare = -1;
          }
          else
          {
            nextSquare = Board.NextSquare(dir, nextSquare);
            steps++;
          }
        }
      }
      for (; dir < nDirections; dir++)
      {
        int nextSquare = Board.NextSquare(dir, to);
        if (nextSquare >= 0)
        {
          Piece pieceOnSquare = Board[nextSquare];
          if (pieceOnSquare != null && nextSquare != from)
          {
            if (pieceOnSquare.GetAttackRange(OppositeDirection(dir)) >= 1)
              seeAttackers[pieceOnSquare.Player].Add(pieceOnSquare);
          }
        }
      }

      bool relativeSide = true; // true if opponent is to move

      while (true)
      {
        if (seeAttackers[side].Count == 0)
          return relativeSide;

        //	find next least valuable attacker
        nextVictim = null;
        foreach (Piece piece in seeAttackers[side])
          if (nextVictim == null || (piece.MidgameValue < nextVictim.MidgameValue && piece.MidgameValue != 0))
            nextVictim = piece;

        //	remove that attacker from the list
        seeAttackers[side].Remove(nextVictim);

        if (nextVictim.MidgameValue == 0 /* Royal */ )
          return relativeSide == (seeAttackers[side ^ 1].Count != 0);

        balance += relativeSide ? nextVictim.MidgameValue
                              : -nextVictim.MidgameValue;

        relativeSide = !relativeSide;

        if (relativeSide == (balance >= value))
          return relativeSide;

        //	look behind this attacker and add any new attacker
        dir = Board.DirectionLookup(to, nextVictim.Square);
        if (dir < nSlidingDirections)
        {
          int steps = Board.GetDistance(to, nextVictim.Square) + 1;
          int nextSquare = Board.NextSquare(dir, nextVictim.Square);
          while (nextSquare >= 0)
          {
            Piece pieceOnSquare = Board[nextSquare];
            if (pieceOnSquare != null)
            {
              if (pieceOnSquare.GetAttackRange(OppositeDirection(dir)) >= steps)
                seeAttackers[pieceOnSquare.Player].Add(pieceOnSquare);
              nextSquare = -1;
            }
            else
            {
              nextSquare = Board.NextSquare(dir, nextSquare);
              steps++;
            }
          }
        }

        side = side ^ 1;
      }
    }
    #endregion

    #region IsSquareAttacked
    public bool IsSquareAttacked(int square, int player)
    {
      return IsSquareAttacked(square, player, out _, false);
    }

    public bool IsSquareAttacked(int square, int player, out List<Piece> attackers, bool findAttackers = false)
    {
      attackers = new List<Piece>();
      bool result = false;
      //	validate inputs - this can often go bad, for example, in a game with a 
      //	checkmate rule where the king has been allowed to be captured for some reason
      if (square < 0 || square >= Board.NumSquares || player < 0 || player > 1)
        throw new Exception($"Board.IsSquareAttacked called with invalid argument: Square {square} or player {player}");
      //	iterate through all directions in the game
      for (int dir = 0; dir < NDirections; dir++)
      {
        //	find the max attack range of any piece in this direction and 
        //	start stepping until we are blocked or find an attacker
        int maxAttackRange = MaxAttackRange(player, OppositeDirection(dir));
        int steps = 1;
        int nextSquare = Board.NextSquare(dir, square);
        if (SimpleMoveGeneration)
        {
          #region Simple Move Generation
          while (nextSquare >= 0 && steps <= maxAttackRange)
          {
            Piece pieceOnSquare = Board[nextSquare];
            if (pieceOnSquare != null)
            {
              if (pieceOnSquare.Player == player)
              {
                if (pieceOnSquare.GetAttackRange(OppositeDirection(dir)) >= steps)
                {
                  //	if the piece has either multi-path moves or moves with 
                  //	conditional locations, we need to find the associated move capability
                  MoveCapability move = null;
                  if (pieceOnSquare.PieceType.HasMovesWithPaths || pieceOnSquare.PieceType.HasMovesWithConditionalLocation)
                  {
                    MoveCapability[] moves;
                    int nMoves = pieceOnSquare.PieceType.GetMoveCapabilities(out moves);
                    for (int x = 0; x < nMoves; x++)
                      if (PlayerDirection(player, moves[x].NDirection) == OppositeDirection(dir))
                      {
                        move = moves[x];
                        break;
                      }
                  }
                  if (move != null && move.ConditionalBySquare != null && !move.ConditionalBySquare[player][nextSquare])
                    break;
                  if (!pieceOnSquare.PieceType.HasMovesWithPaths)
                  {
                    attackers.Add(pieceOnSquare);
                    if (!findAttackers)
                      return true;
                    result = true;
                    break;
                  }
                  //	we have encountered a piece that might attack this square, but 
                  //	it has moves with paths so we need to check to see if there are 
                  //	any clear paths
                  if (move.PathInfo == null)
                  {
                    attackers.Add(pieceOnSquare);
                    if (!findAttackers)
                      return true;
                    result = true;
                    break;
                  }
                  foreach (List<int> stepDirList in move.PathInfo.PathNDirections)
                  {
                    int sq = square;
                    bool blocked = false;
                    foreach (int stepDir in stepDirList)
                    {
                      sq = Board.NextSquare(PlayerDirection(player, stepDir), sq);
                      if (sq == NOT_CONNECTED || Board[sq] != null)
                      {
                        blocked = true;
                        break;
                      }
                    }
                    if (!blocked)
                    {
                      attackers.Add(pieceOnSquare);
                      if (!findAttackers)
                        return true;
                      result = true;
                      break;
                    }
                  }
                }
              }
              //	blocked - we can step no farther in this direction
              nextSquare = -1;
            }
            else
              nextSquare = Board.NextSquare(dir, nextSquare);
            steps++;
          }
          #endregion
        }
        else
        {
          #region Complex Move Generation
          bool passedScreen = false;
          while (nextSquare >= 0 && steps <= maxAttackRange)
          {
            Piece pieceOnSquare = Board[nextSquare];
            if (pieceOnSquare != null)
            {
              if (pieceOnSquare.Player == player)
              {
                if ((!passedScreen && pieceOnSquare.GetAttackRange(OppositeDirection(dir)) >= steps) ||
                   (passedScreen && pieceOnSquare.GetCannonAttackRange(OppositeDirection(dir)) >= steps))
                {
                  //	if the piece has either multi-path moves or moves with 
                  //	conditional locations, we need to find the associated move capability
                  //	NOTE: multiple move capabilities in same direction not supported here
                  MoveCapability move = null;
                  if (pieceOnSquare.PieceType.HasMovesWithPaths || pieceOnSquare.PieceType.HasMovesWithConditionalLocation)
                  {
                    MoveCapability[] moves;
                    int nMoves = pieceOnSquare.PieceType.GetMoveCapabilities(out moves);
                    for (int x = 0; x < nMoves; x++)
                      if (PlayerDirection(player, moves[x].NDirection) == OppositeDirection(dir))
                      {
                        move = moves[x];
                        break;
                      }
                  }
                  //	handle moves only available on some squares
                  if (move != null && move.ConditionalBySquare != null && !move.ConditionalBySquare[player][nextSquare])
                    //	move conditional by square and this square isn't one
                    break;
                  if (!pieceOnSquare.PieceType.HasMovesWithPaths)
                  {
                    attackers.Add(pieceOnSquare);
                    //	the piece doesn't have paths so this square is attacked
                    if (!findAttackers)
                      return true;
                    result = true;
                    nextSquare = Board.NextSquare(dir, nextSquare);
                    continue;
                  }
                  if (move.PathInfo == null)
                  {
                    attackers.Add(pieceOnSquare);
                    if (!findAttackers)
                      return true;
                    result = true;
                    nextSquare = Board.NextSquare(dir, nextSquare);
                    continue;
                  }
                  //	we have encountered a piece that might attack this square, but 
                  //	it has moves with paths so we need to check to see if there are 
                  //	any clear paths
                  foreach (List<int> stepDirList in move.PathInfo.PathNDirections)
                  {
                    int sq = nextSquare;
                    bool blocked = false;
                    foreach (int stepDir in stepDirList)
                    {
                      sq = Board.NextSquare(PlayerDirection(player, stepDir), sq);
                      if (sq == square)
                      {
                        attackers.Add(pieceOnSquare);
                        if (!findAttackers)
                          return true;
                        result = true;
                        break;
                      }
                      if (sq == NOT_CONNECTED || Board[sq] != null)
                      {
                        blocked = true;
                        break;
                      }
                    }
                    if (!blocked)
                    {
                      attackers.Add(pieceOnSquare);
                      if (!findAttackers)
                        return true;
                      result = true;
                      break;
                    }
                  }
                }
              }
              if (!passedScreen)
              {
                passedScreen = true;
                nextSquare = Board.NextSquare(dir, nextSquare);
              }
              else
                //	blocked - we can step no farther in this direction
                nextSquare = -1;
            }
            else
              nextSquare = Board.NextSquare(dir, nextSquare);
            steps++;
          }
          #endregion
        }
      }
      //	allow any Rules that have an IsSquareAttacked handler to 
      //	determine that this square is attacked
      foreach (Rule rule in rulesHandlingIsSquareAttacked)
        if (rule.IsSquareAttacked(square, player))
          return true;
      //	if we made it here, the square is not attacked
      return result;
    }
    #endregion


    // *** SPECIAL RULES *** //

    #region GetRules
    public List<Rule> GetRules()
    { return rules; }
    #endregion

    #region AddRule
    public void AddRule(Rule rule)
    {
      //	If this is a MoveCompletionRule, automatically remove the 
      //	existing one (if any.)  There must be exactly one MoveCompletionRule, 
      //	and a MoveCompletionDefaultRule is automatically added by Game.  If 
      //	another one is being added, remove the existing one.
      if (rule is MoveCompletionRule)
      {
        if (moveCompletionRule != null)
          rules.Remove(moveCompletionRule);
        moveCompletionRule = (MoveCompletionRule)rule;
      }

      //	add the rule to the rules list
      rules.Add(rule);

      //	initialize the rule to this Game
      rule.Initialize(this);
    }
    #endregion

    #region FindRule
    public Rule FindRule(Type ruleType, bool inheritedTypes = false)
    {
      foreach (Rule rule in rules)
        if (ruleType == rule.GetType() || (rule.GetType().IsSubclassOf(ruleType) && inheritedTypes))
          return rule;
      return null;
    }
    #endregion

    #region RemoveRule
    public virtual void RemoveRule(Type ruleType, bool inheritedTypes = false)
    {
      List<Rule> rulesToRemove = new List<Rule>();
      foreach (Rule rule in rules)
        if (ruleType == rule.GetType() || (ruleType.IsSubclassOf(rule.GetType()) && inheritedTypes))
          rulesToRemove.Add(rule);
      foreach (Rule ruleToRemove in rulesToRemove)
      {
        rules.Remove(ruleToRemove);
        ruleToRemove.RuleRemoved();
      }
    }
    #endregion

    #region ReplaceRule
    //	Replace the oldRule (if found) with the newRule, keeping the 
    //	rules in the same order.  Returns true if a replacement was made.
    public virtual bool ReplaceRule(Rule oldRule, Rule newRule)
    {
      List<Rule> newRules = new List<Rule>();
      bool replacementMade = false;
      foreach (Rule rule in rules)
      {
        if (rule == oldRule)
        {
          newRule.Initialize(this);
          newRules.Add(newRule);
          replacementMade = true;
          if (newRule is MoveCompletionRule)
            moveCompletionRule = (MoveCompletionRule)newRule;
          rule.RuleRemoved();
        }
        else
          newRules.Add(rule);
      }
      rules = newRules;
      return replacementMade;
    }
    #endregion


    // *** CUSTOM APPEARANCE and NOTATION *** //

    #region GetSquareNotation
    public virtual string GetSquareNotation(int square)
    { return Board.GetDefaultSquareNotation(square); }
    #endregion

    #region NotationToSquare
    public virtual int NotationToSquare(string notation)
    { return Board.DefaultNotationToSquare(notation); }
    #endregion

    #region GetSquareColor
    public virtual int GetSquareColor(Location location, int nColors)
    {
      int color = 0;
      if (nColors == 2)
      {
        color = (Math.Max(location.Rank, 0) + Math.Max(location.File, 0)) % 2;
        //	now, ensure light color is on bottom right by inverting if even number of files
        if (Board.NumFiles % 2 == 0)
          color = color ^ 1;
      }
      else if (nColors == 3)
      {
        if ((Math.Max(location.Rank, 0) + Math.Max(location.File, 0)) % 2 != Board.NumFiles % 2)
          color = 0;
        else
          color = (Math.Max(location.Rank, 0) % 2) + 1;
      }
      return color;
    }
    #endregion

    #region GetCustomThemes
    public virtual List<string> GetCustomThemes()
    { return null; }
    #endregion

    #region GetDefaultCustomTheme
    public virtual string GetDefaultCustomTheme()
    { return null; }
    #endregion

    #region GetThemeSquareSize
    public virtual void GetThemeSquareSize(string themeName, ref int squareSize)
    { }
    #endregion

    #region RenderCustomThemeBoard
    public virtual void RenderCustomThemeBoard(Graphics gr, int borderWidth, string customThemeName)
    { }
    #endregion


    // *** EVALUATIONS *** //

    #region GetEvaluations
    public List<Evaluation> GetEvaluations()
    { return evaluations; }
    #endregion

    #region AddEvaluation
    public void AddEvaluation(Evaluation eval)
    {
      evaluations.Add(eval);
      eval.Initialize(this);
    }
    #endregion

    #region FindEvaluation
    public Evaluation FindEvaluation(Type evaluationType, bool inheritedTypes = false)
    {
      foreach (Evaluation evaluation in evaluations)
        if (evaluationType == evaluation.GetType() || (evaluationType.IsSubclassOf(evaluation.GetType()) && inheritedTypes))
          return evaluation;
      return null;
    }
    #endregion

    #region RemoveEvaluation
    public void RemoveEvaluation(Type evalType, bool inheritedTypes = false)
    {
      List<Evaluation> evalsToRemove = new List<Evaluation>();
      foreach (Evaluation eval in evaluations)
        if (evalType == eval.GetType() || (evalType.IsSubclassOf(eval.GetType()) && inheritedTypes))
          evalsToRemove.Add(eval);
      foreach (Evaluation evalToRemove in evalsToRemove)
        evaluations.Remove(evalToRemove);
    }
    #endregion

    #region ReplaceEvaluation
    //	Replace the oldEvaluation (if found) with the newEvaluation. 
    //	Returns true if a replacement was made.
    public bool ReplaceEvaluation(Evaluation oldEvaluation, Evaluation newEvaluation)
    {
      bool replacementMade = false;
      if (evaluations.Contains(oldEvaluation))
      {
        evaluations.Remove(oldEvaluation);
        evaluations.Add(newEvaluation);
        newEvaluation.Initialize(this);
        replacementMade = true;
      }
      return replacementMade;
    }
    #endregion


    // *** MISCELLANEOUS OPERATIONS *** //

    #region ReleaseMemoryAllocations
    public void ReleaseMemoryAllocations()
    {
      hashtable = null;
      moveLists = null;
      searchStack = null;
      killers1 = null;
      killers2 = null;
      historyCounters = null;
      butterflyCounters = null;
      countermoves = null;
      SearchPath = null;
      seeAttackers = null;
      foreach (Rule rule in rules)
        rule.ReleaseMemoryAllocations();
      foreach (Evaluation eval in evaluations)
        eval.ReleaseMemoryAllocations();
      finalized = true;
    }
    #endregion


    // *** PROTECTED DATA AND FUNCTIONS *** //

    #region Protected Data
    protected List<Rule> rules;
    protected MoveCompletionRule moveCompletionRule;
    protected List<Evaluation> evaluations;
    protected List<Rule> rulesHandlingMoveBeingGenerated;
    protected List<Rule> rulesHandlingMoveBeingMade;
    protected List<Rule> rulesHandlingMoveMade;
    protected List<Rule> rulesHandlingMoveBeingUnmade;
    protected List<Rule> rulesHandlingTestForWinLossDraw;
    protected List<Rule> rulesHandlingNoMovesResult;
    protected List<Rule> rulesHandlingGenerateSpecialMoves;
    protected List<Rule> rulesHandlingSearchExtensions;
    protected List<Rule> rulesHandlingGetPositionHashCode;
    protected List<Rule> rulesHandlingIsSquareAttacked;
    protected List<Rule> rulesHandlingAdjustEvaluation;
    protected Evaluation evalHandlingTestForWinLossDraw;

    protected Direction[] directions;
    protected int nDirections;
    protected int nSlidingDirections;
    protected int[,] playerDirections;
    protected int[,] maxAttackRange;
    protected int[] oppositeDirections;

    protected PieceType[] pieceTypes;
    protected int nPieceTypes;
    protected PieceType[] disabledPieceTypes;
    protected int nDisabledPieceTypes;

    protected Piece[,] pieces;
    protected int[] nPieces;

    protected MoveList[] moveLists;
    protected SearchStack[] searchStack;
    protected UInt32[] killers1;
    protected UInt32[] killers2;
    protected UInt32[,,] historyCounters;
    protected UInt32[,,] butterflyCounters;
    protected UInt32 lmrHistoryCutoff;
    protected UInt32[,] countermoves;
    protected List<Piece>[] seeAttackers;

    public const int MAX_GAME_LENGTH = 1000;
    protected MoveInfo[] gameHistory;
    protected int gameHistoryCount;
    protected int[] gameHistoryTurnNumbers;

    protected Dictionary<PieceType, int> pieceTypeNumbers;
    protected FEN fen;

    protected Dictionary<PieceType, Type> classesThatCreatedPieceTypes;

    protected Int64 nodes;
    protected int idepth;
    protected bool stopThinking;
    protected DateTime thinkStartTime;
    protected TimeControl timeControl;
    protected long maxSearchTime;
    protected long absoluteMaxSearchTime;
    protected long exactMaxTime;
    protected bool abortSearch;
    protected DebugMessageLog messageLog;

    protected Hashtable hashtable;
    protected bool finalized;

    public const int ONEPLY = 2;
    public const int INFINITY = 1000000;
    public int[] sign = { 1, -1 };
    #endregion

    #region Helper Functions
    #region cleanup
    protected virtual void cleanup()
    {
      hashtable = null;
      //TypesByNotation = null;
    }
    #endregion

    #region updateFEN
    protected void updateFEN()
    {
      fen["array"] = buildArray();
      foreach (Rule rule in rules)
        rule.SavePositionToFEN(fen);
    }
    #endregion

    #region buildArray
    protected virtual string buildArray()
    {
      StringBuilder array = new StringBuilder(80);
      for (int rank = Board.NumRanks - 1; rank >= 0; rank--)
      {
        if (rank != Board.NumRanks - 1)
          array.Append('/');
        int emptySpaceCount = 0;
        for (int file = 0; file < Board.NumFiles; file++)
        {
          Piece piece = Board[Board.LocationToSquare(new Location(rank, file))];
          if (piece == null)
            emptySpaceCount++;
          else
          {
            if (emptySpaceCount > 0)
            {
              array.Append(emptySpaceCount.ToString());
              emptySpaceCount = 0;
            }
            string notation = piece.PieceType.Notation[piece.Player];
            //	determine if we need to prepend an _
            if (notation.Length == 2)
            {
              for (int x = 0; x < nPieceTypes; x++)
                if (pieceTypes[x].Notation.Length == 1 &&
                  pieceTypes[x].Notation[piece.Player][0] == notation[0])
                  notation = "_" + notation;
            }
            //	make lower case if second player
            //						if( piece.Player == 1 )
            //							notation = notation.ToLower();
            array.Append(notation);
          }
        }
        if (emptySpaceCount > 0)
          array.Append(emptySpaceCount.ToString());
      }
      return array.ToString();
    }
    #endregion

    #region replacePieceType
    protected void replacePieceType(PieceType oldType, PieceType newType)
    {
      for (int x = 0; x < nPieceTypes; x++)
        if (pieceTypes[x] == oldType)
        {
          oldType.Enabled = false;
          disabledPieceTypes[nDisabledPieceTypes++] = oldType;
          pieceTypes[x] = newType;
          return;
        }
    }
    #endregion

    #region notationToSquare
    protected int notationToSquare(string notation, MoveNotation format)
    {
      if (format == MoveNotation.XBoard)
      {
        int file = ((int)notation[0]) - 'a';
        int rankInteger = Convert.ToInt32(notation.Substring(1));
        int rank = Board.NumRanks == 10 ? rankInteger : rankInteger - 1;
        return file * Board.NumRanks + rank;
      }
      else if (format == MoveNotation.StandardAlgebraic)
      {
        return NotationToSquare(notation);
      }
      throw new Exception("Game.notationToSquare: Format not supported");
    }
    #endregion

    #region findRulesThatHandleEvent
    protected void findRulesThatHandleEvent(string methodName, List<Rule> outputList)
    {
      foreach (Rule rule in rules)
      {
        Type ruletype = rule.GetType();
        while (ruletype != typeof(Rule))
        {
          if (ruletype.GetMethod(methodName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance) != null)
          {
            outputList.Add(rule);
            break;
          }
          ruletype = ruletype.BaseType;
        }
      }
    }
    #endregion

    #region findEvalThatHandlesEvent
    protected Evaluation findEvalThatHandlesEvent(string methodName)
    {
      foreach (Evaluation eval in evaluations)
      {
        Type evaltype = eval.GetType();
        while (evaltype != typeof(Evaluation))
        {
          if (evaltype.GetMethod(methodName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance) != null)
            return eval;
          evaltype = evaltype.BaseType;
        }
      }
      return null;
    }
    #endregion

    #region writeGameVariableToFile
    protected void writeGameVariableToFile(KeyValuePair<string, object> variable, TextWriter file)
    {
      if (variable.Value is string)
      {
        string value = (string)variable.Value;
        if (value != null)
        {
          file.Write(variable.Key);
          file.Write(" = ");
          if (value.IndexOf(' ') >= 0)
          {
            file.Write('"');
            file.Write(value);
            file.WriteLine('"');
          }
          else
            file.WriteLine(value);
        }
      }
      else if (variable.Value is IntRangeVariable)
      {
        IntRangeVariable value = (IntRangeVariable)variable.Value;
        if (value.Value != null)
        {
          file.Write(variable.Key);
          file.Write(" = ");
          file.WriteLine(value.Value.ToString());
        }
      }
      else if (variable.Value is ChoiceVariable)
      {
        ChoiceVariable value = (ChoiceVariable)variable.Value;
        if (value.Value != null)
        {
          file.Write(variable.Key);
          file.Write(" = ");
          file.WriteLine(value.Value);
        }
      }
      else if (variable.Value is bool)
      {
        file.WriteLine(variable.Key + " = " + ((bool)variable.Value ? "true" : "false"));
      }
      else if (variable.Value is int)
      {
        file.WriteLine(variable.Key + " = " + ((int)variable.Value).ToString());
      }
      else if (variable.Value is PieceType)
      {
        //	just skip this for now
      }
      else
        throw new Exception("Unexpected game variable type to write to file");
    }
    #endregion

    #region compareGameVariablesForEquality
    protected bool compareGameVariablesForEquality(object var1, object var2)
    {
      if (var1 == null || var2 == null)
        return var1 == null && var2 == null;
      else if (var1 is string)
      {
        if (!(var2 is string))
          return false;
        return (string)var1 == (string)var2;
      }
      else if (var1 is IntRangeVariable)
      {
        if (!(var2 is IntRangeVariable))
          return false;
        return ((IntRangeVariable)var1).Value == ((IntRangeVariable)var2).Value;
      }
      else if (var1 is ChoiceVariable)
      {
        if (!(var2 is ChoiceVariable))
          return false;
        return ((ChoiceVariable)var1).Value == ((ChoiceVariable)var2).Value;
      }
      else if (var1 is bool)
      {
        if (!(var2 is bool))
          return false;
        return ((bool)var1) == ((bool)var2);
      }
      else if (var1 is int)
      {
        if (!(var2 is int))
          return false;
        return (int)var1 == (int)var2;
      }
      else if (var1 is PieceType)
      {
        //	do something better here down the road
        return true;
      }
      else
        throw new Exception("Unexpected game variable type used in comparison");
    }
    #endregion
    #endregion
  }
}
