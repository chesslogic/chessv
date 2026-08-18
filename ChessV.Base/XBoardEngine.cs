
/***************************************************************************

                                 ChessV

                  COPYRIGHT (C) 2012-2019 BY GREG STRONG
  
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

using ChessV.EngineOptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ChessV
{
  public class XBoardEngine : Engine
  {
    //	Stores a list of the XBoard features that the engine supports
    public List<string> SupportedFeatures { get; private set; }

    //	Creates a new XboardEngine
    public XBoardEngine(IDebugMessageLog messageLog, TimerFactory factory, Process process) :
      base(messageLog, factory, process)
    {
      SupportedFeatures = new List<string>();
      forceMode = false;
      drawOnNextMove = false;
      ftName = false;
      ftPing = false;
      ftSetboard = false;
      ftTime = true;
      ftUsermove = false;
      ftReuse = true;
      ftNps = false;
      gotResult = false;
      lastPing = 0;
      notation = MoveNotation.XBoard;

      //	set up the init timer
      initTimer = factory.NewTimer();
      initTimer.Interval = 8000;
      initTimer.Tick += initialize; // connect(m_initTimer, SIGNAL(timeout()), this, SLOT(initialize()));
      initTimer.Tick += onSingleShotTimerTick;

      //	set up the finish game timer
      finishGameTimer = factory.NewTimer();
      finishGameTimer.Interval = 200;
      finishGameTimer.Tick += pong;
      finishGameTimer.Tick += onSingleShotTimerTick;

      addVariant("normal");
      Name = "XboardEngine";
    }


    // *** OPERATIONS *** //

    public void SetBoard(string fen)
    {
      if (ftSetboard)
        Write("setboard " + fen);
      else
        throw new Exception("Game requires setboard, but the engine doesn't support it");
    }


    // *** INHERITED FROM PLAYER *** //

    #region EndGame
    public override void EndGame(Result result)
    {
      if (State != PlayerState.Thinking && State != PlayerState.Observing)
        return;

      if (State != PlayerState.Thinking)
        gotResult = true;

      StopThinking();
      setForceMode(true);
      Write("result " + result.VerboseString);

      base.EndGame(result);

      // If the engine can't be pinged, we may have to wait for
      // for a move or a result, or an error, or whatever. We
      // would like to extend our middle fingers to every engine
      // developer who fails to support the ping command.
      if (!ftPing && gotResult)
        finishGame();
    }
    #endregion

    #region MakeMove
    public override void MakeMove(List<Movement> moves)
    {
      Movement move = moves[0];
      string moveString;
      if (nextMove != null && move == nextMove[0])
        moveString = nextMoveString;
      else
        moveString = this.moveString(move);

      // If we're not in force mode, we'll have to wait for the
      // 'go' command until the move can be sent to the engine.
      if (!forceMode)
      {
        if (nextMove == null)
        {
          nextMove = moves;
          nextMoveString = moveString;
          return;
        }
        else if (move != nextMove[0])
          setForceMode(true);
      }

      if (ftUsermove)
        Write("usermove " + moveString);
      else
        Write(moveString);

      nextMove = null;
    }
    #endregion

    #region Protocol
    public override string Protocol
    { get { return "xboard"; } }
    #endregion


    // *** INHERITED FROM ENGINE *** //

    #region sendPing
    protected override bool sendPing()
    {
      if (!ftPing)
      {
        if (State == PlayerState.FinishingGame)
          return true;
        return false;
      }

      // Ping the engine with a random number. The engine should
      // later send the number back at us.
      lastPing = Program.Random.Next(32) + 1;
      Write(string.Format("ping {0}", lastPing));
      return true;
    }
    #endregion

    #region sendStop
    protected override void sendStop()
    {
      Write("?");
    }
    #endregion

    #region sendQuit
    public override void sendQuit()
    {
      Write("quit");
    }
    #endregion

    #region startProtocol
    protected override void startProtocol()
    {
      //	tell the engine to turn on xboard mode
      Write("xboard");
      //	tell the engine that we're using Xboard protocol 2
      Write("protover 2");

      //	give the engine 2 seconds to reply to the protover command - 
      //	this is how Xboard deals with protocol 1 engines.
      initTimer.Start();
    }
    #endregion

    #region startGame
    protected override void startGame()
    {
      drawOnNextMove = false;
      gotResult = false;
      forceMode = false;
      nextMove = null;

      //	send the "new" command
      Write("new");

      //	send the "variant" command?
      if (Game.Name != "Chess")
        //	Inform the engine of the variant we are playing.  If we are using 
        //	an EngineGameAdaptor, use the name specified in the adaptor 
        //	(which is the game the engine thinks we are playing.)
        Write("variant " + (Adaptor != null ? Adaptor.XBoardName : Game.GameAttribute.XBoardName));

      //	send the "setboard" command?
      if ((Game.GameAttribute.TagList != null && Game.GameAttribute.TagList.Contains("Random Array")) ||
        (Adaptor != null && Adaptor.IssueSetboard)) //	||  board()->fenString() != board()->defaultFenString())
      {
        SetBoard(Adaptor != null ? Adaptor.TranslateFEN(Game.FENStart) : Game.FENStart);
      }

      //	send the time controls
      TimeControl myTc = TimeControl;
      if (myTc.Infinite)
      {
        if (myTc.PlyLimit == 0 && myTc.NodeLimit == 0)
          Write(string.Format("st {0}", infiniteSec));
      }
      else if (myTc.TimePerMove > 0)
        Write(string.Format("st {0}", myTc.TimePerMove / 1000));
      else
        Write(string.Format("level {0} {1} {2}", myTc.MovesPerTC, msToXboardTime(myTc.TimePerTC), (double)myTc.TimeIncrement / 1000.0));

      if (myTc.PlyLimit > 0)
        Write(string.Format("sd {0}", myTc.PlyLimit));
      if (myTc.NodeLimit > 0)
      {
        if (ftNps)
          Write(string.Format("st 1\nnps {0}", myTc.NodeLimit));
        else
          throw new Exception("Engine doesn't support the nps command");
      }

      //	send the "post" command to show thinking
      Write("post");

      //	send the "easy" command to disable pondering
      Write("easy");
      setForceMode(true);

      //	inform the engine of the opponent's type and name
      if (ftName)
      {
        if (!Opponent.IsHuman)
          Write("computer");
        Write("name " + Opponent.Name);
      }

      //	if the Game has any moves already played, send them 
      //	now while we are in "force" mode
      List<Movement> moves = new List<Movement>();
      for (int nMove = 0; nMove < Game.GameMoveNumber; nMove++)
      {
        int turnNumber;
        MoveInfo moveMade = Game.GetHistoricalMove(nMove, out turnNumber);
        //	we do not play the moves one-by-one but first accumulate all the
        //	moves by the same player to accomodate multi-move variants
        if (moves.Count > 0 && moves[0].Player != moveMade.Player)
        {
          MakeMove(moves);
          moves.Clear();
        }
        moves.Add(moveMade);
      }
      if (moves.Count > 0)
      {
        MakeMove(moves);
      }
    }
    #endregion

    #region startThinking
    protected override void startThinking()
    {
      setForceMode(false);
      sendTimeLeft();

      if (nextMove == null)
        Write("go");
      else
        MakeMove(nextMove);
    }
    #endregion

    #region parseLine
    protected override void parseLine(string line)
    {
      int cursor;
      if (line.IndexOf("pong") > 0)
      {
        char a0 = line[0];
        char a1 = line[1];
        char a2 = line[2];
        char a3 = line[3];
        char a4 = line[4];
        char a5 = line[5];
        char a6 = line[6];
        int q = 0;
      }
      string command = firstToken(line, out cursor);
      if (command.Trim() == "")
        return;

      //	check for game ending commands
      if (command == "1-0" || command == "0-1" || command == "*" || command == "1/2-1/2" || command == "resign")
      {
        if ((State != PlayerState.Thinking && State != PlayerState.Observing) || Game.Result.IsNone)
        {
          finishGame();
          return;
        }

        string description = nextToken(command, ref cursor, true);
        if (description != null)
        {
          if (description.Length >= 1 && description[0] == '{')
            description = description.Substring(1);
          if (description.Length >= 1 && description[description.Length - 1] == '}')
            description = description.Substring(0, description.Length - 1);
        }

        if (command == "*")
          claimResult(new Result(ResultType.NoResult, -1, description));
        else if (command == "1/2-1/2")
        {
          if (State == PlayerState.Thinking && false /* && ClaimsValidated */ )
            // The engine claims that its next move will draw the game
            drawOnNextMove = true;
          else
            claimResult(new Result(ResultType.Draw, -1, description));
        }
        else if ((command == "1-0" && Side == 0) || (command == "0-1" && Side == 1))
          claimResult(new Result(ResultType.Win, Side, description));
        else
          forfeit(ResultType.Resignation);

        return;
      }
      else if (Char.IsDigit(command[0])) // principal variation
      {
        bool ok = false;
        int val = 0;

        Dictionary<string, string> thinking = new Dictionary<string, string>();

        // Search depth
        ok = Int32.TryParse(command, out val);
        if (!ok)
          return;
        Evaluation.Depth = val;
        thinking.Add("Depth", command);

        // Evaluation
        if ((command = nextToken(line, ref cursor)) == null)
          return;
        ok = Int32.TryParse(command, out val);
        if (ok)
        {
          if (WhiteEvalPov && Side == 1)
            val = -val;
          Evaluation.Score = val;
        }
        thinking.Add("Score", xboardScoreToString(val));

        // Search time
        if ((command = nextToken(line, ref cursor)) == null)
          return;
        ok = Int32.TryParse(command, out val);
        if (ok)
          Evaluation.Time = val * 10;
        thinking.Add("Time", xboardTimeToString(val * 10));

        // Node count
        if ((command = nextToken(line, ref cursor)) == null)
          return;
        ok = Int32.TryParse(command, out val);
        if (ok)
          Evaluation.NodeCount = val;
        thinking.Add("Nodes", xboardNodesToString(val));

        // Principal variation
        if ((command = nextToken(line, ref cursor, true)) == null)
          return;
        string pv = TranslatePVString(command);
        if (Adaptor != null)
          command = Adaptor.TranslatePV(pv, false);
        Evaluation.PV = pv;
        thinking.Add("PV", pv);

        Game.ThinkingCallback(thinking);

        return;
      }

      string args = nextToken(line, ref cursor, true);

      if (command == "move")
      {
        if (State != PlayerState.Thinking)
        {
          if (State == PlayerState.FinishingGame)
            finishGame();
          else
            ;//qDebug("Unexpected move from %s", qPrintable(name()));
          return;
        }

        if (Adaptor != null)
          args = Adaptor.TranslateMove(args, false);
        Movement move = Game.MoveFromDescription(args, MoveNotation.XBoard);
        if (move == null)
        {
          forfeit(ResultType.IllegalMove, args);
          return;
        }

        if (drawOnNextMove)
        {
          drawOnNextMove = false;
          Result boardResult;
          Game.MakeMove(move, true, MoveExecutionMode.Speculative);
          boardResult = Game.Result;
          Game.UndoMove(false, MoveExecutionMode.Speculative);

          //	If the engine claimed a draw before this move, the
          //	game must have ended in a draw by now
          if (!boardResult.IsDraw)
          {
            claimResult(new Result(ResultType.Draw));
            return;
          }
        }

        emitMove(new List<Movement>() { move });
      }
      else if (command == "pong")
      {
        int pingarg;
        if (Int32.TryParse(args, out pingarg))
          if (pingarg == lastPing)
            pong(null, null);
      }
      else if (command == "feature")
      {
        MatchCollection matches = Regex.Matches(args, "\\w+\\s*=\\s*(\"[^\"]*\"|\\d+)");
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
          string[] split = match.Value.Split('=');
          if (split.Length != 2)
            continue;
          string feature = split[0].Trim();
          string val = split[1].Trim();
          val = val.Replace("\"", "");
          setFeature(feature, val);
        }
      }
      else if (command == "Error")
      {
        // If the engine complains about an unknown result command,
        // we can assume that it's safe to finish the game.
        if (args.IndexOf(':') >= 0)
        {
          string str = args.Substring(args.IndexOf(':') + 1).Trim();
          if (str.Length >= 6 && str.Substring(0, 6) == "result")
            finishGame();
        }
      }
    }
    #endregion

    #region sendOption
    protected override void sendOption(string name, object value)
    {
      if (name == "memory" || name == "cores" || (name.Length >= 8 && name.Substring(0, 8) == "egtpath "))
        Write(name + " " + value.ToString());
      else
      {
        if (value == null)
          Write("option " + name);
        else
        {
          string tmp;
          if (value.GetType() == typeof(Boolean))
            tmp = (bool)value ? "1" : "0";
          else
            tmp = value.ToString();
          Write("option " + name + "=" + tmp);
        }
      }
    }
    #endregion

    #region restartsBetweenGames
    protected override bool restartsBetweenGames
    {
      get
      {
        if (Restart == EngineConfiguration.RestartMode.Auto)
          return !ftReuse;
        return base.restartsBetweenGames;
      }
    }
    #endregion

    #region onTimeout
    // Inherited from ChessEngine
    protected override void onTimeout()
    {
      if (drawOnNextMove)
      {
        //Q_ASSERT(State == PlayerState.Thinking);

        drawOnNextMove = false;
        claimResult(new Result(ResultType.Draw));
      }
      else
        base.onTimeout();
    }
    #endregion

    #region initialize
    //	Initializes the engine, and emits the 'ready' signal
    private void initialize(object sender, System.EventArgs e)
    {
      if (State == PlayerState.Starting)
      {
        onProtocolStart();
        ready();
      }
    }
    #endregion

    #region parseOption
    private EngineOption parseOption(string line)
    {
      int start = line.IndexOf('-');
      if (start < 2)
        return null;

      string name = line.Substring(0, start - 1);

      start++;
      int end = line.IndexOf(' ', start);
      if (end == -1)
        end = line.Length;
      string type = line.Substring(start, end - start);

      if (type == "button" || type == "save")
        return new EngineButtonOption(name);
      if (type == "check")
      {
        bool value = line.Substring(end + 1) == "1";
        return new EngineCheckOption(name, value, value);
      }
      if (type == "string" || type == "file" || type == "path")
      {
        string value = line.Substring(end + 1);
        EngineTextOption.EditorType editorType;

        if (type == "file")
          editorType = EngineTextOption.EditorType.FileDialog;
        else if (type == "path")
          editorType = EngineTextOption.EditorType.FolderDialog;
        else
          editorType = EngineTextOption.EditorType.LineEdit;

        return new EngineTextOption(name, value, value, null, editorType);
      }
      if (type == "spin" || type == "slider")
      {
        List<string> parameters = new List<string>(line.Substring(end + 1).Split(' '));
        if (parameters.Count != 3)
          return null;

        try
        {
          int value = Convert.ToInt32(parameters[0]);
          int min = Convert.ToInt32(parameters[1]);
          int max = Convert.ToInt32(parameters[2]);

          return new EngineSpinOption(name, value, value, min, max);
        }
        catch
        {
          // TODO some qdebug thing or other
          return null;
        }
      }
      if (type == "combo")
      {
        List<string> choices = new List<string>(line.Substring(end + 1).Split(new string[] { " /// " }, StringSplitOptions.RemoveEmptyEntries));
        if (choices.Count == 0)
          return null;

        string value = null;
        for (int x = 0; x < choices.Count; x++)
        {
          string item = choices[x];
          if (item[0] == '*')
          {
            item = item.Substring(1);
            choices[x] = item;
            value = item;
          }
        }
        if (value == null)
          value = choices[0];

        return new EngineComboOption(name, value, value, choices);
      }

      return null;
    }
    #endregion

    #region setFeature
    private void setFeature(string name, string val)
    {
      if (name == "ping")
      {
        ftPing = (val == "1");
        if (ftPing)
          SupportedFeatures.Add("ping");
      }
      else if (name == "setboard")
      {
        ftSetboard = (val == "1");
        if (ftSetboard)
          SupportedFeatures.Add("setboard");
      }
      /*			else if( name == "san" )
						{
							if( val == "1" )
								m_notation = MoveNotation.StandardAlgebraic;
							else
								m_notation = MoveNotation.LongAlgebraic;
						}*/
      else if (name == "usermove")
      {
        ftUsermove = (val == "1");
        if (ftUsermove)
          SupportedFeatures.Add("usermove");
      }
      else if (name == "nps")
      {
        ftNps = (val == "1");
        if (ftNps)
          SupportedFeatures.Add("nps");
      }
      else if (name == "time")
      {
        ftTime = (val == "1");
        if (ftTime)
          SupportedFeatures.Add("time");
      }
      else if (name == "reuse")
      {
        ftReuse = (val == "1");
        if (ftReuse)
          SupportedFeatures.Add("reuse");
      }
      else if (name == "myname")
      {
        if (Name == "XboardEngine")
          Name = val;
      }
      else if (name == "variants")
      {
        clearVariants();
        string[] variants = val.Split(',');
        foreach (string str in variants)
        {
          string variant = str.Trim();
          if (variant != "")
            addVariant(variant);
        }
      }
      else if (name == "name")
      {
        ftName = (val == "1");
        if (ftName)
          SupportedFeatures.Add("name");
      }
      else if (name == "memory")
      {
        if (val == "1")
        {
          addOption(new EngineSpinOption("memory", 32, 32, 0, 32768));
          SupportedFeatures.Add("memory");
        }
      }
      else if (name == "smp")
      {
        if (val == "1")
        {
          addOption(new EngineSpinOption("cores", 1, 1, 0, Environment.ProcessorCount));
          SupportedFeatures.Add("smp");
        }
      }
      else if (name == "egt")
      {
        string[] splitVal = val.Split(',');
        foreach (string str in splitVal)
        {
          string egtType = string.Format("egtpath {0}", str.Trim());
          addOption(new EngineTextOption(egtType, null, null));
        }
      }
      else if (name == "option")
      {
        EngineOption option = parseOption(val);
        if (option == null || !option.IsValid())
          ; //qDebug() << "Invalid Xboard option from" << this->name() << ":" << val;
        else
          addOption(option);
      }
      else if (name == "done")
      {
        Write("accepted done", WriteMode.Unbuffered);
        initTimer.Stop();

        if (val == "1")
          initialize(this, null);
        return;
      }
      else
      {
        Write("rejected " + name, WriteMode.Unbuffered);
        return;
      }

      Write("accepted " + name, WriteMode.Unbuffered);
    }
    #endregion

    #region setForceMode
    private void setForceMode(bool enable)
    {
      if (enable && !forceMode)
      {
        forceMode = true;
        Write("force");

        // If there's a move pending, and we didn't get the
        // 'go' command, we'll send the move in force mode.
        if (nextMove != null)
          MakeMove(nextMove);
      }
      forceMode = enable;
    }
    #endregion

    #region sendTimeLeft
    private void sendTimeLeft()
    {
      if (!ftTime)
        return;

      if (TimeControl.Infinite)
      {
        Write(string.Format("time {0}", infiniteSec));
        return;
      }

      long csLeft = TimeControl.TimeLeft / 10;
      long ocsLeft = Opponent.TimeControl.TimeLeft / 10;

      if (csLeft < 0)
        csLeft = 0;
      if (ocsLeft < 0)
        ocsLeft = 0;

      Write(string.Format("time {0}", csLeft));
      Write(string.Format("otim {0}", ocsLeft));
    }
    #endregion

    #region finishGame
    private void finishGame()
    {
      if (!ftPing && State == PlayerState.FinishingGame)
      {
        // Give the engine enough time to send all pending
        // output relating to the current game
        gotResult = true;
        finishGameTimer.Start();
      }
    }
    #endregion

    #region moveString
    private string moveString(Movement move)
    {
      // Xboard always uses SAN for castling moves in random variants
      /*	if( m_notation == MoveNotation.LongAlgebraic && Game.GameAttribute.TagList.Contains( "Random Array" ) )
        {
          string str = Game.DescribeMove( move, MoveNotation.StandardAlgebraic );
          if( str.Length >= 3 && str.Substring( 0, 3 ) == "O-O" )
            return str;
        }*/

      string movestring = Game.DescribeMove(move, notation);
      if (Adaptor != null)
        movestring = Adaptor.TranslateMove(movestring, true);
      return movestring;
    }
    #endregion

    #region msToXBoardTime
    protected string msToXboardTime(long ms)
    {
      long sec = ms / 1000;

      string number = (sec / 60).ToString();
      if (sec % 60 != 0)
        number += string.Format(":{0}", (sec % 60).ToString("D2"));

      return number;
    }
    #endregion

    #region xboardScoreToString
    protected string xboardScoreToString(int score)
    {
      string str = "";
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

      return str;
    }
    #endregion

    #region xboardTimeToString
    protected string xboardTimeToString(int time)
    {
      string str = "";
      if (time == 0)
        return str + "0s";
      int precision = 0;
      if (time < 100)
        precision = 3;
      else if (time < 1000)
        precision = 2;
      else if (time < 10000)
        precision = 1;
      str += ((double)time / 1000.0).ToString("F" + precision.ToString()) + 's';
      return str;
    }
    #endregion

    #region xboardNodesToString
    protected string xboardNodesToString(long nodes)
    {
      if (nodes < 1000)
        return nodes.ToString();
      if (nodes < 200000)
        return ((double)nodes / 1000.0).ToString("F2") + "k";
      if (nodes < 200000000)
        return ((double)nodes / 1000000.0).ToString("F2") + "M";
      else
        return ((double)nodes / 1000000000.0).ToString("F2") + "B";
    }
    #endregion


    // *** HELPER FUNCTIONS *** //

    #region TranslatePVString
    //	The main purpose of TranslatePVString is to 'translate' moves 
    //	modifying the notation of squares from the generic XBoard notation
    //	(which always names squares the same way) to game-specific notation
    protected virtual string TranslatePVString(string pv)
    {
      //	split into sections
      string[] parts = pv.Split(' ');
      //	create list to hold translated parts
      List<string> newParts = new List<string>();
      //	translate each part
      foreach (string part in parts)
      {
        //	Each part could be lots of different things depending 
        //	on standard notation vs. SAN, whether move numbers are 
        //	displayed in the string, and whether extra information 
        //	is shown on the end.  We'll make educated guesses and 
        //	handle as best we can.
        if (part.Length == 0)
          //	blank part because we had multiple spaces - ignore
          continue;
        if (Char.IsDigit(part[0]))
          //	starts with a number, so probably a move number ("1.")
          //	we'll just leave it as is
          newParts.Add(part);
        else if (part[0] >= 'a' && part[0] <= 'p' && part.Length >= 4)
        {
          //	we'll assume this is the notation of a column starting 
          //	an xboard notation move and try to parse the move
          char fromFileNotation = part[0];
          int cursor = 1;
          //	match 1 or 2 digits for rank of moving piece
          if (Char.IsDigit(part[cursor++]))
          {
            if (Char.IsDigit(part[cursor]))
              cursor++;
            string fromRankNotation = part.Substring(1, cursor - 1);
            //	try to match notation of destination file
            if (part[cursor] >= 'a' && part[cursor] <= 'p')
            {
              char toFileNotation = part[cursor++];
              //	match 1 or 2 digits for destination rank
              int startCursor = cursor;
              if (cursor < part.Length && Char.IsDigit(part[cursor++]))
              {
                if (cursor < part.Length && Char.IsDigit(part[cursor]))
                  cursor++;
                string toRankNotation = part.Substring(startCursor, cursor - startCursor);
                //	try to compile new movement notation with this information
                if (fromFileNotation - 'a' + 1 <= Game.Board.NumFiles &&
                  toFileNotation - 'a' + 1 <= Game.Board.NumFiles &&
                  ((Game.Board.NumRanks == 10 &&
                    Convert.ToInt32(fromRankNotation) < 10 &&
                    Convert.ToInt32(toRankNotation) < 10) ||
                   (Game.Board.NumRanks != 10 &&
                    Convert.ToInt32(fromRankNotation) >= 1 &&
                    Convert.ToInt32(fromRankNotation) <= Game.Board.NumRanks &&
                    Convert.ToInt32(toRankNotation) >= 1 &&
                    Convert.ToInt32(toRankNotation) <= Game.Board.NumRanks)))
                {
                  int fromSquare = Game.Board.LocationToSquare(new Location(Game.Board.NumRanks == 10 ? Convert.ToInt32(fromRankNotation) : Convert.ToInt32(fromRankNotation) - 1, fromFileNotation - 'a'));
                  int toSquare = Game.Board.LocationToSquare(new Location(Game.Board.NumRanks == 10 ? Convert.ToInt32(toRankNotation) : Convert.ToInt32(toRankNotation) - 1, toFileNotation - 'a'));
                  string newNotation = Game.GetSquareNotation(fromSquare) + Game.GetSquareNotation(toSquare);
                  //	the only thing we should possibly expect is a char for promotion type
                  if (part.Length == cursor + 1 && Char.IsLetter(part[cursor]))
                    newNotation += part[cursor++];
                  //	assuming we've reached the end of the part, we're good.  If there's 
                  //	extra junk, we'll just take that and stick that on the end of our 
                  //	new string.
                  if (part.Length > cursor)
                    newNotation += part.Substring(cursor);
                  //	store the newly translated move
                  newParts.Add(newNotation);
                  continue;
                }
              }
            }
          }
          //	if we're here, we didn't match a sensible xboard 
          //	notation move, so we'll just leave it as is
          newParts.Add(part);
        }
        else if (part[0] >= 'A' && part[0] <= 'Z' && part.Length >= 3)
        {
          //	probably SAN notation (e.g. "Nd3" or "Nxd3")
          int cursor = 0;
          char pieceTypeNotation = part[cursor++];
          string capture = "";
          if (part[cursor] == 'x')
          {
            capture = "x";
            cursor++;
          }
          if (part[cursor] >= 'a' && part[cursor] <= 'p')
          {
            char fileNotation = part[cursor++];
            if (cursor < part.Length && Char.IsDigit(part[cursor]))
            {
              int startCursor = cursor++;
              if (cursor < part.Length && Char.IsDigit(part[cursor]))
                cursor++;
              string rankNotation = part.Substring(startCursor, cursor - startCursor);
              if (fileNotation - 'a' + 1 <= Game.Board.NumFiles &&
                ((Game.Board.NumRanks == 10 && Convert.ToInt32(rankNotation) < 10) ||
                 (Game.Board.NumRanks != 10 && Convert.ToInt32(rankNotation) >= 1 && Convert.ToInt32(rankNotation) <= Game.Board.NumRanks)))
              {
                int square = Game.Board.LocationToSquare(new Location(Game.Board.NumRanks == 10 ? Convert.ToInt32(rankNotation) : Convert.ToInt32(rankNotation) - 1, fileNotation - 'a'));
                string newNotation = pieceTypeNotation + capture + Game.GetSquareNotation(square);
                //	if there's anything extra, stick it on the end
                if (cursor < part.Length)
                  newNotation += part.Substring(cursor);
                //	store the newly translated move
                newParts.Add(newNotation);
                continue;
              }
            }
          }
        }
        else if (part[0] >= 'a' && part[0] <= 'p')
        {
          //	Less than 4 chars, so probably just the notation of a single 
          //	square (a pawn move in SAN notation).  Is the first char a 
          //	valid file?
          if (part[0] - 'a' + 1 <= Game.Board.NumFiles)
          {
            //	The remaining must be 1 or 2 digits
            if ((part.Length == 2 && Char.IsDigit(part[1])) ||
              (part.Length == 3 && Char.IsDigit(part[1]) && Char.IsDigit(part[2])))
            {
              int rank = Convert.ToInt32(part.Substring(1));
              if ((Game.Board.NumRanks == 10 && rank <= 9) ||
                (Game.Board.NumRanks != 10 && rank >= 1 && rank <= Game.Board.NumRanks))
              {
                newParts.Add(Game.Board.GetFileNotation(part[0] - 'a') +
                  Game.Board.GetRankNotation(Game.Board.NumRanks == 10 ? rank : rank - 1));
                continue;
              }
            }
          }
        }
        else
          //	we don't know what this is - add it as is
          newParts.Add(part);
      }
      //	assemble the new pieces back into a new string
      StringBuilder newPV = new StringBuilder(120);
      foreach (string part in newParts)
      {
        if (newPV.Length > 0)
          newPV.Append(' ');
        newPV.Append(part);
      }
      return newPV.ToString();
    }
    #endregion


    // *** PRIVATE DATA MEMBERS *** //

    bool forceMode;
    bool drawOnNextMove;

    // Engine features
    bool ftName;
    bool ftPing;
    bool ftSetboard;
    bool ftTime;
    bool ftUsermove;
    bool ftReuse;
    bool ftNps;

    bool gotResult;
    int lastPing;
    List<Movement> nextMove;
    string nextMoveString;
    MoveNotation notation;
    const int infiniteSec = 86400;

    protected Timer initTimer;
    protected Timer finishGameTimer;
  }
}
