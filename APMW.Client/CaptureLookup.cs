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
        { "B", "Queen's Knight" },
        { "C", "Queen's Attendant" },
        { "D", "Queen's Bishop" },
        { "E", "Queen" },
        { "F", "Checkmate Maxima" }, // not used
        { "G", "King's Bishop" },
        { "H", "King's Attendant" },
        { "I", "King's Knight" },
        { "J", "King's Rook" },
      };

    public string fileToLocation(int numFiles, string fileNotation)
    {
      string qualifiedName = fileToSubname(numFiles, fileNotation);
      return "Capture Piece " + qualifiedName;
    }

    private string fileToSubname(int numFiles, string fileNotation)
    {
      if (fileNotation == null)
        throw new ArgumentNullException(nameof(fileNotation));

      // Use maxima names for 10x10 board, minima names for 8x8
      if (numFiles == 10)
        return MaximaNames[fileNotation];
      else
        return MinimaNames[fileNotation];
    }
  }
}
