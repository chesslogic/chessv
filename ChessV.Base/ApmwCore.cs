
using System;
using System.Collections.Generic;

namespace ChessV.Base
{
  public class ApmwCore
  {
    public static ApmwCore _instance;

    public static ApmwCore getInstance()
    {
      if (_instance == null)
      {
        lock (typeof(ApmwCore))
        {
          if (_instance == null)
          {
            _instance = new ApmwCore();
          }
        }
      }
      return _instance;
    }

    public int foundPockets = -1;
    public int foundPocketRange = -1;
    public int foundPocketGems = -1;
    public int foundPawns = -1;
    public int foundMinors = -1;
    public int foundMajors = -1;
    public int foundQueens = -1;
    public int foundConsuls = -1;
    public int foundKingPromotions = -1;
    public int foundPawnForwardness = -1;
    public bool isGrand = false;

    public PieceType king;
    public List<PieceType> kings;
    public HashSet<PieceType> pawns;
    public HashSet<PieceType> minors;
    public HashSet<PieceType> majors;
    public HashSet<PieceType> queens;
    public HashSet<PieceType> colorbound;
    public List<HashSet<PieceType>> armies;
    public HashSet<PieceType> foundArmy;
    public List<HashSet<PieceType>> pocketSets;


    /** ChessV.Base.Match */
    public List<StartedEventHandler> StartedEventHandlers = new List<StartedEventHandler>();
    /** ChessV.Base.PieceType */
    public Func<(Dictionary<KeyValuePair<int, int>, PieceType>, string)> PlayerPieceSetProvider =
      () => (new Dictionary<KeyValuePair<int, int>, PieceType>(), "");
    public Func<List<PieceType>> PlayerPocketPiecesProvider =
      () => new List<PieceType>();
    /** Provides 0 if the player has found PlayAsWhite item, otherwise provides 1 */
    public Func<int> GeriProvider = () => 1;
    public List<Action<MoveInfo>> NewMovePlayed = new List<Action<MoveInfo>>();
    public List<Action<Match>> MatchFinished = new List<Action<Match>>();
    public Func<int> EngineWeakeningProvider = () => 0;

  }
}
