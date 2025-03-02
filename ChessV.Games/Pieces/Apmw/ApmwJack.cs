using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessV.Games.Pieces.Apmw
{
  /// <summary>
  /// Provides various pieces estimated to be 7 material.
  /// 
  /// FIDE gains a combined Rook + Elephant, which moves like a Rook but also a 2,2 leaper.
  /// 
  /// Colorbound Clobberers gain a combined Cleric + Camel, which moves like a Cleric but also a 1,3 leaper.
  /// 
  /// Reliable Rookies gain a combined Rook + non-leaping Camel, which moves like a Rook but can also
  /// side-step 1 square after 3 sliding squares.
  /// 
  /// Nutty Knights gain a combined Knight + Camel, which moves like a Knight but also a 1,3 leaper.
  /// 
  /// Cannons gain a combined Cannon + Dao, similar to the Queennon without knight cannon moves nor King captures.
  /// 
  /// Petals gain a Ribbon that can also move 1 further step before it first rotates.
  /// </summary>
  internal class ApmwJack
  {
  }
}
