using Archipelago.MultiClient.Net.Helpers;
using ChessV;
using ChessV.Base;
using ChessV.Games;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Archipelago.APChessV
{
  internal sealed class ForkBoardFixture : IDisposable
  {
    private readonly ApmwCore previousCore;
    private readonly ApmwConfig previousConfig;
    private readonly Dictionary<string, long> locationIds = new Dictionary<string, long>();
    private readonly Dictionary<long, string> locationNames = new Dictionary<long, string>();
    private TaskCompletionSource<HashSet<string>> pendingCompletion;
    private BoardLocationHandler handler;
    private bool disposed;

    private ForkBoardFixture()
    {
      previousCore = ApmwCore._instance;
      previousConfig = ApmwConfig._instance;
      ApmwCore._instance = new ApmwCore
      {
        foundPockets = 0,
        foundPocketRange = 0,
        foundPocketGems = 0,
        foundPawns = 0,
        foundMinors = 0,
        foundMajors = 0,
        foundJacks = 0,
        foundQueens = 0,
        foundConsuls = 0,
        foundKingPromotions = 0,
        foundPawnForwardness = 0,
        GeriProvider = () => 0
      };
      ApmwConfig._instance = new ApmwConfig();
    }

    public ApmwChessGame Game { get; private set; }

    public static ForkBoardFixture Create(string array, int knightValue = 300, int bishopValue = 325)
    {
      var fixture = new ForkBoardFixture();
      bool initialized = false;
      try
      {
        fixture.Initialize(array, knightValue, bishopValue);
        initialized = true;
        return fixture;
      }
      finally
      {
        if (!initialized)
          fixture.Dispose();
      }
    }

    private void Initialize(string array, int knightValue, int bishopValue)
    {
      ApmwCore core = ApmwCore.getInstance();
      core.PlayerPieceSetProvider = files =>
      {
        PieceType king = core.kings[0];
        PieceType rook = core.majors.Single(piece => piece.Name == "Rook");
        PieceType pawn = core.pawns.Single(piece => piece.Name == "Pawn");
        var pieces = new Dictionary<KeyValuePair<int, int>, PieceType>();
        for (int file = 0; file < files; file++)
          pieces[new KeyValuePair<int, int>(3, file)] = pawn;
        pieces[new KeyValuePair<int, int>(4, 0)] = rook;
        pieces[new KeyValuePair<int, int>(4, files / 2)] = king;
        pieces[new KeyValuePair<int, int>(4, files - 1)] = rook;
        return (pieces, "QRNB");
      };

      Game = new ForkBoardGame(array, knightValue, bishopValue);
      var attribute = (GameAttribute)typeof(ForkBoardGame)
        .GetCustomAttributes(typeof(GameAttribute), false).Single();
      Game.Initialize(attribute, null, null);
      Game.Match = new ChessV.Match(Game, new PGNGame(), null, new TimerFactory());
      Game.Match.SetPlayer(0, new HumanPlayer(null, new TimerFactory()));
      Game.Match.SetPlayer(1, new HumanPlayer(null, new TimerFactory()));

      var locations = new Mock<ILocationCheckHelper>(MockBehavior.Strict);
      locations.Setup(helper => helper.GetLocationIdFromName("ChecksMate", It.IsAny<string>()))
        .Returns<string, string>((gameName, name) =>
        {
          if (!locationIds.TryGetValue(name, out long id))
          {
            id = locationIds.Count + 1;
            locationIds.Add(name, id);
            locationNames.Add(id, name);
          }
          return id;
        });
      locations.Setup(helper => helper.CompleteLocationChecks(It.IsAny<long[]>()))
        .Callback<long[]>(ids =>
        {
          TaskCompletionSource<HashSet<string>> completion = pendingCompletion
            ?? throw new InvalidOperationException("Location completion arrived without a pending scan.");
          completion.SetResult(new HashSet<string>(ids.Select(id => locationNames[id])));
        });

      handler = new BoardLocationHandler();
      handler.Initialize(locations.Object, null);
      handler.StartMatch(Game.Match);

      // Only ScanAsync emits location notifications. Tactical continuations must not
      // invoke victory/deathlink callbacks against the deliberately absent session.
      core.StartedEventHandlers.Remove(handler.StartMatch);
      core.NewMoveSetup.Remove(handler.SetupMove);
      core.NewMovePlayed.Remove(handler.HandleMove);
      core.MatchFinished.Remove(handler.HandleMatch);
      Game.Match.Finished -= handler.HandleMatch;
    }

    public async Task<HashSet<string>> ScanAsync(string movedPieceSquare)
    {
      ThrowIfDisposed();
      if (pendingCompletion != null)
        throw new InvalidOperationException("A location scan is already pending.");
      Piece piece = Game.Board[movedPieceSquare];
      if (piece == null || piece.Player != 0)
        throw new ArgumentException("The scan requires a human piece on the named square.", nameof(movedPieceSquare));
      if (!Game.Result.IsNone)
        throw new InvalidOperationException("A completed game cannot be scanned without an Archipelago session.");

      var completion = new TaskCompletionSource<HashSet<string>>(TaskCreationOptions.RunContinuationsAsynchronously);
      pendingCompletion = completion;
      try
      {
        // Replay the public post-move notification without changing the frozen board.
        handler.HandleMove(new MoveInfo
        {
          Player = piece.Player,
          FromSquare = piece.Square,
          ToSquare = piece.Square,
          MoveType = MoveType.StandardMove,
          PieceMoved = piece
        });
        return await completion.Task.WaitAsync(TimeSpan.FromSeconds(10));
      }
      finally
      {
        pendingCompletion = null;
      }
    }

    public string[] Attackers(string square, int player, bool findAll = true)
    {
      ThrowIfDisposed();
      Game.IsSquareAttacked(Game.NotationToSquare(square), player, out List<Piece> attackers, findAll);
      return attackers.Select(piece => piece.PieceType.Name + "@" + Game.GetSquareNotation(piece.Square)).ToArray();
    }

    public void PlayMoves(string xboardMoves)
    {
      ThrowIfDisposed();
      if (pendingCompletion != null)
        throw new InvalidOperationException("Wait for location completion before playing a continuation.");
      Game.PlayMoves(xboardMoves, MoveNotation.XBoard);
    }

    public void Dispose()
    {
      if (disposed)
        return;
      ApmwCore._instance = previousCore;
      ApmwConfig._instance = previousConfig;
      disposed = true;
    }

    private void ThrowIfDisposed()
    {
      if (disposed)
        throw new ObjectDisposedException(nameof(ForkBoardFixture));
    }

    private sealed class BoardLocationHandler : LocationHandler
    {
      public BoardLocationHandler() : base() { }
    }

    [Game("Archipelago Multiworld", typeof(ChessV.Geometry.Rectangular), 8, 8, Template = true)]
    private sealed class ForkBoardGame : ApmwChessGame
    {
      private readonly string initialArray;
      private readonly int knightValue;
      private readonly int bishopValue;

      public ForkBoardGame(string array, int knightValue, int bishopValue)
      {
        initialArray = array;
        this.knightValue = knightValue;
        this.bishopValue = bishopValue;
      }

      public override void SetGameVariables()
      {
        base.SetGameVariables();
        Castling.Value = "None";
      }

      public override void SetOtherVariables()
      {
        base.SetOtherVariables();
        Pawn.MidgameValue = 100;
        Knight.MidgameValue = knightValue;
        Bishop.MidgameValue = bishopValue;
        Rook.MidgameValue = 500;
        Queen.MidgameValue = 950;
        King.MidgameValue = 325;
        FENStart = initialArray + " b - - - 0 20";
      }
    }
  }
}
