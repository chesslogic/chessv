using ChessV.Base;
using ChessV.Games;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Archipelago.APChessV
{
  public class CaptureLookup
  {
    public static Dictionary<string, string> MinimaNames =
      new Dictionary<string, string>()
      {
        { "A", "Queen's Rook" },
        { "B", "Queen's Knight" },
        { "C", "Queen's Bishop" },
        { "D", "Queen" },
        { "E", "Checkmate Minima" }, // not used
        { "F", "King's Bishop" },
        { "G", "King's Knight" },
        { "H", "King's Rook" }
      };
    public static Dictionary<string, string> MaximaNames =
      new Dictionary<string, string>()
      {
        { "A", "Queen's Rook" },
        { "B", "Queen's Attendant" },
        { "C", "Queen's Knight" },
        { "D", "Queen's Bishop" },
        { "E", "Queen" },
        { "F", "Checkmate Maxima" }, // not used
        { "G", "King's Knight" },
        { "H", "King's Bishop" },
        { "I", "King's Attendant" },
        { "J", "King's Rook" },
      };

    public string fileToLocation(string fileNotation)
    {
      string qualifiedName = fileToSubname(fileNotation);
      return "Capture Piece " + qualifiedName;
    }

    private string fileToSubname(string fileNotation)
    {
      if (fileNotation == null)
        throw new ArgumentNullException(nameof(fileNotation));
      // if we gained the Super-Size Me item, use maxima names
      if (ApmwCore.getInstance().isGrand)
        return MaximaNames[fileNotation];
      else
        return MinimaNames[fileNotation];
    }
  }
}
