
/***************************************************************************

                                 ChessV

                  COPYRIGHT (C) 2012-2017 BY GREG STRONG
  
  THIS FILE DERIVED FROM CUTE CHESS BY ILARI PIHLAJISTO AND ARTO JONSSON

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
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Text;

namespace ChessV
{
  // delegates for the event handlers used by the Player class
  public delegate void HumanEnabledEventHandler(bool humanEnabled);
  public delegate void FENChangedEventHandler(string fen);
  public delegate void MatchMoveEventHandler(Movement move, string sanString, string comment);
  public delegate void StartedEventHandler(Match match = null);
  public delegate void DeathedEventHandler(Match match = null, string reason = "Unknown DeathLink");
  public delegate void FinishedEventHandler(Match match = null);
  public delegate void StartFailedEventHandler(Match match = null);
  public delegate void PlayersReadyEventHandler();

  public class Match
  {
    // TEST ONLY
    public Match() { }

    public Match(Game game, PGNGame pgn, IDebugMessageLog messageLog, TimerFactory timerFactory)
    {
      Game = game;
      StartDelay = 0;
      IsFinished = false;
      m_gameInProgress = false;
      m_paused = false;
      PGN = pgn;
      Moves = new List<Movement>();
      m_timerFactory = new TimerFactory();
      m_startTimer = timerFactory.NewTimer();
      m_timeControl = new TimeControl[2];
      m_messageLog = messageLog;
      Adjudicator = new GameAdjudicator();

      m_player = new Player[2];
      m_adaptor = new EngineGameAdaptor[2];
      m_book = new OpeningBook[2];
      m_bookDepth = new int[2];
      for (int i = 0; i < 2; i++)
      {
        m_player[i] = null;
        m_book[i] = null;
        m_bookDepth[i] = 0;
      }

      HumanEnabled = delegate { };
      Started = (match) =>
      {
        foreach (StartedEventHandler handler in ApmwCore.getInstance().StartedEventHandlers)
        {
          handler(match);
        }
      };
      FENChanged = delegate { };
      MoveMade = delegate { };
      Finished = delegate { };
      Deathed = delegate { };
    }

    public string ErrorString { get; set; }

    public virtual Player GetPlayer(int side)
    {
      if (side != 0 && side != 1)
        throw new Exception("ERROR in Match.GetPlayer: Cannot get null player");
      return m_player[side];
    }

    public Player PlayerToMove
    { get { return Game.CurrentSide == -1 ? null : m_player[Game.CurrentSide]; } }

    public Player PlayerToWait
    { get { return Game.CurrentSide == -1 ? null : m_player[Game.CurrentSide ^ 1]; } }

    public bool IsFinished { get; private set; }

    public PGNGame PGN { get; private set; }

    public virtual Game Game { get; private set; }

    public string StartingFen { get; set; }

    public List<Movement> Moves { get; private set; }

    public GameAdjudicator Adjudicator { get; set; }

    public int StartDelay { get; set; }

    public Result Result
    {
      get { return Game.Result; }
      set { Game.Result = value; }
    }

    public void SetPlayer(int side, Player player, EngineGameAdaptor adaptor = null)
    {
      if (m_player[side] == null)
      {
        m_player[side] = player;
        m_adaptor[side] = adaptor;
        if (player is Engine)
          ((Engine)player).Adaptor = adaptor;
      }
      else
        throw new Exception();
    }

    public void SetPlayerToHuman(int side)
    {
      //	We can only convert InternalEngine to HumanPlayer
      if (m_player[side] == null || !(m_player[side] is InternalEngine))
        throw new Exception();
      InternalEngine oldPlayer = (InternalEngine)m_player[side];
      HumanPlayer newPlayer = new HumanPlayer(oldPlayer.MessageLog, m_timerFactory);
      newPlayer.AttachGame(side, m_player[side ^ 1], Game, oldPlayer.TimeControl);
      newPlayer.MoveMade += OnMoveMade;
      newPlayer.WokeUp += Resume;
      m_player[side] = newPlayer;
    }

    public void SetPlayerToInternalEngine(int side)
    {
      //	We can only convert HumanPlayer to InternalEngine
      if (m_player[side] == null || !(m_player[side] is HumanPlayer))
        return; // throw new Exception();
      HumanPlayer oldPlayer = (HumanPlayer)m_player[side];
      InternalEngine newPlayer = new InternalEngine(oldPlayer.MessageLog, m_timerFactory);
      newPlayer.AttachGame(side, m_player[side ^ 1], Game, oldPlayer.TimeControl);
      newPlayer.MoveMade += OnMoveMade;
      m_player[side] = newPlayer;
      //			if( Game.CurrentSide == side )
      //				m_player[side].Go( m_player[side] );
    }

    public void SetTimeControl(TimeControl timeControl, int side = -1)
    {
      if (side != 0)
        m_timeControl[1] = timeControl.Clone();
      if (side != 1)
        m_timeControl[0] = timeControl.Clone();
    }

    public TimeControl GetTimeControl(int side)
    {
      return m_timeControl[side];
    }

    public void SetMoves(PGNGame pgn)
    {
      //Q_ASSERT(pgn.variant() == m_board->variant());

      StartingFen = pgn.StartingFEN;
      resetBoard();
      Moves.Clear();

      foreach (PGNGame.MoveData md in pgn.Moves)
      {
        Movement move = md.Move;
        //Q_ASSERT(m_board->isLegalMove(move));

        Game.MakeMove(move, true);
        if (!Game.Result.IsNone)
          return;

        Moves.Add(move);
      }
    }

    public void SetMoves(string movesstring)
    {
      resetBoard();
      Moves.Clear();

      if (movesstring != "")
      {
        string[] moves = movesstring.Split(' ');
        foreach (string movestring in moves)
        {
          Movement move = Game.MoveFromDescription(movestring, MoveNotation.XBoard);
          Game.MakeMove(move, true);
          if (!Game.Result.IsNone)
            return;

          Moves.Add(move);
        }
      }
    }

    public void SetOpeningBook(OpeningBook book, int side = -1, int depth = 1000)
    {
      // Q_ASSERT(!m_gameInProgress);

      if (side == -1)
      {
        SetOpeningBook(book, 0, depth);
        SetOpeningBook(book, 1, depth);
      }
      else
      {
        m_book[side] = book;
        m_bookDepth[side] = depth;
      }
    }

    public void GenerateOpening()
    {
      if (m_book[0] == null || m_book[1] == null)
        return;
      resetBoard();

      // First play moves that are already in the opening
      foreach (Movement move in Moves)
      {
        //Q_ASSERT(m_board->isLegalMove(move));

        Game.MakeMove(move, true);
        if (!Game.Result.IsNone)
          return;
      }

      // Then play the opening book moves
      while (true)
      {
        List<Movement> moves = bookMove(Game.CurrentSide);
        if (moves == null)
          break;

        foreach (Movement move in moves)
          Game.MakeMove(move, true);
        if (!Game.Result.IsNone)
          break;

        Moves.AddRange(moves);
      }
    }

    public void LockThread()
    {
      //			if (QThread::currentThread() == thread())
      //				return;

      //			QMetaObject::invokeMethod(this, "pauseThread", Qt::QueuedConnection);
      //			m_pauseSem.acquire();
    }

    public void UnlockThread()
    {
      //			if (QThread::currentThread() == thread())
      //				return;

      //			m_resumeSem.release();
    }


    // *** PUBLIC SLOTS *** //

    public void Start()
    {
      if (StartDelay > 0)
      {
        m_startTimer.Interval = StartDelay; // QTimer::singleShot( StartDelay, this, SLOT(start()) );
        m_startTimer.Tick += onStart;
        m_startTimer.Start();
        StartDelay = 0;
        return;
      }

      for (int i = 0; i < 2; i++)
      {
        m_player[i].ResultClaim += onResultClaim;
        // connect(m_player[i], SIGNAL(resultClaim(Chess::Result)),
        //	this, SLOT(onResultClaim(Chess::Result)));
      }

      PlayersReady += startGame; // connect(this, SIGNAL(playersReady()), this, SLOT(startGame()));

      // TODO - from CUTECHESS: Start the game in the correct thread
      syncPlayers(); // QMetaObject::invokeMethod(this, "syncPlayers", Qt::QueuedConnection);
    }

    public void Pause()
    {
      m_paused = true;
    }

    public void Resume()
    {
      if (!m_paused)
        return;
      m_paused = false;

      startTurn(); // QMetaObject::invokeMethod( this, "startTurn", Qt::QueuedConnection );
    }

    public void Stop()
    {
      if (IsFinished)
        return;

      //	Don't actually cancel the game if both players are either 
      //	HumanPlayer or InternalEngine so that we can still take back 
      //	moves and keep playing.
      if ((m_player[0] is HumanPlayer || m_player[0] is InternalEngine) &&
        (m_player[1] is HumanPlayer || m_player[1] is InternalEngine))
        return;

      IsFinished = true;
      //	raise HumanEnabled event 
      HumanEnabled(false);
      if (!m_gameInProgress)
      {
        Result = new Result();
        finish();
        return;
      }

      m_gameInProgress = false;
      PGN.SetTag("PlyCount", PGN.Moves.Count.ToString());
      PGN.Result = Result;
      PGN.SetResultDescription(Result.Description);

      m_player[0].EndGame(Result);
      m_player[1].EndGame(Result);

      PlayersReady += finish; // connect(this, SIGNAL(playersReady()), this, SLOT(finish()), Qt::QueuedConnection);
      syncPlayers();
    }

    public void Kill()
    {
      for (int i = 0; i < 2; i++)
      {
        if (m_player[i] != null)
          m_player[i].Kill();
      }

      Stop();
    }

    public void Death(string reason)
    {
      IsFinished = true;
      Result = new Result(ResultType.Resignation, m_player[0] is HumanPlayer ? 0 : 1, "Killed by a distant ally: " + reason);

      m_gameInProgress = false;
      PGN.SetTag("PlyCount", PGN.Moves.Count.ToString());
      PGN.Result = Result;
      PGN.SetResultDescription(Result.Description);

      m_player[0].EndGame(Result);
      m_player[1].EndGame(Result);
      for (int i = 0; i < 2; i++)
      {
        if (m_player[i] != null)
          m_player[i].Kill();
      }
      Deathed(this, Result.Description);
      Finished(this);
      HumanEnabled(false);
    }

    public void EmitStartFailed()
    {
      //	raise the StartFailed event
      StartFailed(this);
    }

    public void OnMoveMade(Player sender, List<Movement> moves)
    {
      // Q_ASSERT(m_gameInProgress);
      // Q_ASSERT(m_board->isLegalMove(move));
      if (sender != PlayerToMove)
      {
        //qDebug("%s tried to make a move on the opponent's turn", qPrintable(sender->name()));
        return;
      }

      Moves.AddRange(moves);
      addPGNMove(moves, evalString(sender.Evaluation));

      // Debug: Track board state before temporary moves
      string boardStateBefore = GetBoardDebugInfo("Before temporary moves");
      
      // Get the result before sending the move to the opponent
      if (DebugFlags.ValidateMovesBeforeCommit)
      {
        try
        {
          foreach (Movement move in moves)
          {
            string moveDesc = $"{Game.GetSquareNotation(move.FromSquare)} to {Game.GetSquareNotation(move.ToSquare)}";
            Game.MakeMove(move, true);
          }
          
          // Debug: Track board state after temporary moves
          string boardStateAfter = GetBoardDebugInfo("After temporary moves");
          
          if (Result.IsNone)
          {
            Adjudicator.AddEval(Game, sender.Evaluation);
            if (Adjudicator.Result != null)
              Result = Adjudicator.Result;
          }
          
          // Debug: Track board state before undo
          string boardStateBeforeUndo = GetBoardDebugInfo("Before undo");
          
          for (int x = 0; x < moves.Count; x++)
          {
            try
            {
              Game.UndoMove();
            }
            catch (Exception ex)
            {
              if (DebugFlags.ThrowOnInvariantViolation)
              {
                throw new Exception($"Failed to undo move {x + 1} of {moves.Count}. " +
                  $"Move was: {Game.GetSquareNotation(moves[moves.Count - 1 - x].FromSquare)} to {Game.GetSquareNotation(moves[moves.Count - 1 - x].ToSquare)}\n" +
                  $"{boardStateBefore}\n{boardStateAfter}\n{boardStateBeforeUndo}\n" +
                  $"BoardMoveStack count: {Game.BoardMoveStack.MoveCount}\n" +
                  $"Original error: {ex.Message}", ex);
              }
            }
          }
        }
        catch (Exception ex)
        {
          if (DebugFlags.ThrowOnInvariantViolation)
            throw new Exception($"Error in Match.OnMoveMade during move validation sequence.\n{boardStateBefore}\nOriginal error: {ex.Message}", ex);
          // else continue without pre-validation
        }
      }

      Player player = PlayerToWait;
      player.MakeMove(moves);
      foreach (Movement move in moves)
        Game.MakeMove(move, true);

      if (Result.IsNone)
        startTurn();
      else
        Stop();

      emitLastMove();
    }

    public void OnMoveTakenBack()
    {

    }

    // *** EVENTS *** //

    public event HumanEnabledEventHandler HumanEnabled;
    public FENChangedEventHandler FENChanged;
    public MatchMoveEventHandler MoveMade;
    public StartedEventHandler Started;
    public FinishedEventHandler Finished;
    public DeathedEventHandler Deathed;
    public StartFailedEventHandler StartFailed;
    public PlayersReadyEventHandler PlayersReady;


    // *** PRIVATE SLOTS *** //

    private void onStart(object sender, System.EventArgs e)
    {
      m_startTimer.Stop();
      Start();
    }


    private void startGame()
    {
      Result = new Result();

      //	raise HumanEnabled event
      HumanEnabled(false);

      PlayersReady -= startGame; //disconnect(this, SIGNAL(playersReady()), this, SLOT(startGame()));
      if (IsFinished)
        return;

      m_gameInProgress = true;
      for (int i = 0; i < 2; i++)
      {
        Player player = m_player[i];
        //Q_ASSERT(player != 0);
        //Q_ASSERT(player->isReady());

        if (player.State == PlayerState.Disconnected)
          return;
        /*	if (!player->supportsVariant(m_board->variant()))
          {
            qDebug("%s doesn't support variant %s",
              qPrintable(player->name()), qPrintable(m_board->variant()));
            m_result = Chess::Result(Chess::Result::ResultError);
            stop();
            return;
          }*/
      }

      resetBoard();
      initializePgn();

      Started(this); // Raise a Started event
      FENChanged(Game.FENStart); // Raise a FENChanged event

      for (int side = 0; side < 2; side++)
      {
        // Q_ASSERT(m_timeControl[side].isValid());
        m_player[side].TimeControl = m_timeControl[side];
        m_player[side].NewGame(side, m_player[side ^ 1], Game);
      }

      // Play the forced opening moves first
      for (int i = 0; i < Moves.Count; i++)
      {
        List<Movement> move = new List<Movement>() { Moves[i] };
        // Q_ASSERT(m_board->isLegalMove(move));

        addPGNMove(move, "book");

        PlayerToMove.MakeBookMove(move);
        PlayerToWait.MakeMove(move);
        //Game.PerformMove( move, true );

        emitLastMove();

        if (!Game.Result.IsNone)
        {
          // qDebug("Every move was played from the book");
          Stop();
          return;
        }
      }

      for (int i = 0; i < 2; i++)
      {
        m_player[i].MoveMade += OnMoveMade; // connect(m_player[i], SIGNAL(moveMade(Chess::Move)), this, SLOT(onMoveMade(Chess::Move)));
        if (m_player[i].IsHuman)
          ((HumanPlayer)m_player[i]).WokeUp += Resume; //connect(m_player[i], SIGNAL(wokeUp()), this, SLOT(resume()));
      }

      startTurn();
    }

    private void startTurn()
    {
      if (m_paused)
        return;

      int side = Game.CurrentSide;

      HumanEnabled(m_player[side].IsHuman); // raise HumanEnabled event

      List<Movement> move = bookMove(side);
      if (move == null)
        m_player[side].Go(m_player[side]);
      else
        m_player[side].MakeBookMove(move);
    }

    private void finish()
    {
      PlayersReady -= finish; //disconnect(this, SIGNAL(playersReady()), this, SLOT(finish()));
      for (int i = 0; i < 2; i++)
      {
        if (m_player[i] != null)
        {
          // Need to simulate Qt's Disconnect funciton here: m_player[i].Disconnect();
          // Not exactly sure how to do it, so here's an attempt:
          m_player[i].ResultClaim -= onResultClaim;
          m_player[i].MoveMade -= OnMoveMade;
          m_player[i].Ready -= onPlayerReady;
          m_player[i].Disconnected -= onPlayerReady;
        }
      }

      Finished(this); // raise Finished event
    }

    private void onResultClaim(Player sender, Result result)
    {
      if (IsFinished)
        return;

      if (!m_gameInProgress && result.Winner == -1)
      {
        //				qWarning("Unexpected result claim from %s: %s",
        //					 qPrintable(sender->name()),
        //					 qPrintable(result.toVerboseString()));
      }
      else if (sender.ClaimsValidated && result != Game.Result)
      {
        //				qWarning("%s forfeits by invalid result claim: %s",
        //					 qPrintable(sender->name()),
        //					 qPrintable(result.toVerboseString()));
        Result = new Result(ResultType.Adjudication, sender.Side ^ 1, "Invalid result claim");
      }
      else
        Result = result;

      Stop();
    }

    private void onPlayerReady(Player sender)
    {
      sender.Ready -= onPlayerReady; // disconnect( sender, SIGNAL(ready()), this, SLOT(onPlayerReady()));
      sender.Disconnected -= onPlayerReady; // disconnect( sender, SIGNAL(disconnected()), this, SLOT(onPlayerReady()));

      for (int i = 0; i < 2; i++)
      {
        if (!m_player[i].IsReady && m_player[i].State != PlayerState.Disconnected)
          return;
      }

      PlayersReady(); // raise PlayersReady event
    }

    private void syncPlayers()
    {
      bool ready = true;

      for (int i = 0; i < 2; i++)
      {
        Player player = m_player[i];
        // Q_ASSERT(player != 0);

        if (!player.IsReady && player.State != PlayerState.Disconnected)
        {
          ready = false;
          player.Ready += onPlayerReady; // connect( player, SIGNAL(ready()), this, SLOT(onPlayerReady())) ;
          player.Disconnected += onPlayerReady; // connect( player, SIGNAL(disconnected()), this, SLOT(onPlayerReady()) );
        }
      }
      if (ready)
        PlayersReady(); // raise PlayersReady event
    }

    private void pauseThread()
    {
      //			m_pauseSem.release();
      //			m_resumeSem.acquire();
    }


    // *** PRIVATE HELPER FUNCTIONS *** //

    private List<Movement> bookMove(int side)
    {
      /*			if( m_book[side] == null ||
              m_moves.Count >= m_bookDepth[side] * 2 )
              return null;

            Chess::GenericMove bookMove = m_book[side]->move(m_board->key());
            Chess::Move move = m_board->moveFromGenericMove(bookMove);
            if (move.isNull())
              return Chess::Move();

            if (!m_board->isLegalMove(move))
            {
              qWarning("Illegal opening book move for %s: %s",
                 qPrintable(side.toString()),
                 qPrintable(m_board->moveString(move, Chess::Board::LongAlgebraic)));
              return Chess::Move();
            }

            if (m_board->isRepetition(move))
              return Chess::Move();

            return move;*/
      return null;
    }

    private void resetBoard()
    {
      string fen = m_startingFen;
      if (fen == null)
      {
        fen = Game.FENStart;
        if (Game.GameAttribute.TagList != null &&
          Game.GameAttribute.TagList.Contains("Random Array"))
          m_startingFen = fen;
      }

      //			if( !m_board->setFenString(fen) )
      //				qFatal("Invalid FEN string: %s", qPrintable(fen));
    }

    private void initializePgn()
    {
      PGN.Variant = Game.GameAttribute.GameName;
      string currentPlayer = Game.FEN["current player"];
      PGN.SetStartingFenString(currentPlayer == "w" ? 0 : 1, m_startingFen);
      PGN.Date = DateTime.Now.ToString("yyyy.MM.dd");
      PGN.SetPlayerName(0, m_player[0].Name);
      PGN.SetPlayerName(1, m_player[1].Name);
      PGN.Result = Result;

      if (m_timeControl[0] == m_timeControl[1])
        PGN.SetTag("TimeControl", m_timeControl[0].ToString());
      else
      {
        PGN.SetTag("WhiteTimeControl", m_timeControl[0].ToString());
        PGN.SetTag("BlackTimeControl", m_timeControl[1].ToString());
      }
    }

    private void addPGNMove(List<Movement> move, string comment)
    {
      PGNGame.MoveData md = new PGNGame.MoveData();
      md.Key = Game.Board.HashCode;
      md.Move = move[0];
      md.MoveString = Game.DescribeMove(move[0], MoveNotation.StandardAlgebraic /* TODO: change to SAN */ );
      md.Comment = comment;
      PGN.AddMove(md);
    }

    private void emitLastMove()
    {
      PGNGame.MoveData md = PGN.Moves[PGN.Moves.Count - 1];
      MoveMade(md.Move, md.MoveString, md.Comment);
    }

    protected static string evalString(MoveEvaluation eval)
    {
      if (eval.IsBookEval)
        return "book";
      if (eval == null)
        return "";

      string str = "";
      if (eval.Depth > 0)
      {
        int score = eval.Score;
        int absScore = score >= 0 ? score : -score;
        if (score > 0)
          str += "+";

        // Detect mate-in-n scores
        if (absScore > 9900 &&
          (absScore = 1000 - (absScore % 1000)) < 100)
        {
          if (score < 0)
            str += "-";
          str += "M" + absScore.ToString();
        }
        else
          str += ((double)score / 100.0).ToString("F2");

        str += "/" + eval.Depth.ToString() + " ";
      }

      long t = eval.Time;
      if (t == 0)
        return str + "0s";

      int precision = 0;
      if (t < 100)
        precision = 3;
      else if (t < 1000)
        precision = 2;
      else if (t < 10000)
        precision = 1;
      str += ((double)t / 1000.0).ToString("F" + precision.ToString()) + 's';

      return str;
    }

    private string GetBoardDebugInfo(string context)
    {
      var sb = new System.Text.StringBuilder();
      sb.AppendLine($"=== {context} ===");
      sb.AppendLine($"Current Player: {Game.CurrentSide}, Move #: {Game.GameMoveNumber}, Ply: {Game.Ply}");
      sb.AppendLine($"BoardMoveStack moves: {Game.BoardMoveStack.MoveCount}");
      
      // Show key squares around e4 (where the error occurred)
      sb.AppendLine("Board state around e4:");
      for (int rank = 5; rank >= 2; rank--)
      {
        for (int file = 3; file <= 5; file++)
        {
          var loc = new ChessV.Location(rank, file);
          int square = Game.Board.LocationToSquare(loc);
          var piece = Game.Board[square];
          string pieceInfo = piece != null ? $"{piece.PieceType.Name[0]}{piece.Player}" : "..";
          sb.Append($"{Game.Board.GetDefaultSquareNotation(square)}:{pieceInfo} ");
        }
        sb.AppendLine();
      }
      return sb.ToString();
    }


    // *** PRIVATE MEMBER VARIABLES *** //

    private Player[] m_player;
    private EngineGameAdaptor[] m_adaptor;
    private TimeControl[] m_timeControl;
    private OpeningBook[] m_book;
    private int[] m_bookDepth;
    private bool m_gameInProgress;
    private bool m_paused;
    private string m_startingFen;
    private Timer m_startTimer;
    private TimerFactory m_timerFactory;
    //		private QSemaphore m_pauseSem;
    //		private QSemaphore m_resumeSem;

    IDebugMessageLog m_messageLog;
  }
}
