
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
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ChessV.GUI
{
  static class Program
  {
    // *** GLOBALS *** //

    //	The ChessV Manager singleton object
    static public ChessV.Manager.Manager Manager;

    //	Are we running a Microsoft Windows platform?
    static public bool RunningOnWindows;

    //	GameCatalog contains bookkeeping information about all variants currently supported
    static public GameCatalog GameCatalog = new GameCatalog();

    //	A Random object used for generating pseudo-random numbers throughout the program
    static public Random Random = new Random();

    //	The absolute path of the program directory
    static public string ProgramPath = Directory.GetCurrentDirectory();


    [STAThread]
    static void Main()
    {
      // *** INITIALIZATION *** //

      Manager = new ChessV.Manager.Manager();

      //	Are we running on Windows?
      RunningOnWindows = Path.DirectorySeparatorChar == '\\';

      //	Base manager for registry key settings
      int currentRegistryVersion = RegistrySettings.Initialize();

      //	Factory object for dynamically creating a BoardPresentation for any Game object
      PresentationFactory.Initialize();

      //	Library object containing the collection of bitmap textures
      TextureLibrary.Initialize();

      //	Library object cointaining the collection of defined color schemes
      ColorSchemeLibrary.Initialize(currentRegistryVersion);

      //	Library object containing the collection of defined piece sets
      PieceSetLibrary.Initialize();
      //	Make sure we loaded at least a standard piece set successfully or 
      //	else we'll need to inform the user to reinstall and then exit
      if (PieceSetLibrary.PieceSets == null)
      {
        MessageBox.Show("Directory of piece set graphics could not be found.\n" +
          "Please re-install ChessV.", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
        Application.Exit();
        return;
      }
      if (!PieceSetLibrary.PieceSets.ContainsKey("Standard"))
      {
        MessageBox.Show("The 'Standard' piece set  graphics could not be found.\n" +
          "Please re-install ChessV.", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
        Application.Exit();
        return;
      }


      // *** INITIALIZE GAME CATALOG *** //
      string gameCatalog =
        ".Chess" +
          "\n\tChess" +
          "\n\t.Shuffled Variants|Fischer Random Chess" +
            "\n\t\tFischer Random Chess" +
            "\n\t\tChess480" +
            "\n\t\tWild Castle" +
            "\n\t\tChess256" +
          "\n\tPocket Knight" +
          "\n\tChess with Different Armies" +
          "\n\tKnightmate" +
          "\n\tAlice Chess" +
        "\n.Medium" +
          "\n\t.Capablanca Variants|Capablanca Chess" +
            "\n\t\tCapablanca Chess" +
            "\n\t\tCarrera's Chess" +
            "\n\t\tBird's Chess" +
            "\n\t\tModern Carrera's Chess" +
            "\n\t\tGothic Chess" +
            "\n\t\tGrotesque Chess" +
            "\n\t\tEmbassy Chess" +
            "\n\t\tVictorian Chess" +
            "\n\t\tLadorean Chess" +
            "\n\t\tSchoolbook Chess" +
            "\n\t\tUnivers Chess" +
            "\n\t\tOpti Chess" +
          "\n\tGrand Chess" +
          "\n\tFalcon Chess" +
          "\n\tOdin's Rune Chess" +
          "\n\tEurasian Chess" +
          "\n\tUnicorn Great Chess" +
        "\n.Large" +
          "\n\tWildebeest Chess" +
          "\n\tOmega Chess" +
          "\n\tGross Chess" +
          "\n\tDouble Chess (16 x 8)" +
          "\n\tOdyssey" +
          "\n\tChess on a 12 by 12 Board" +
        "\n.Historical" +
          "\n\tShatranj" +
          "\n\tCourier Chess" +
          "\n\tArchchess" +
          "\n\tEmperor's Game";
      byte[] gameCatalogBytes = Encoding.ASCII.GetBytes(gameCatalog);
      TextReader textReader = new StreamReader(new MemoryStream(gameCatalogBytes));
      GameCatalog.LoadIndex(textReader);


      // *** START APPLICATION *** //

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      try
      {
        MainForm mainForm = new MainForm();
        RecoverableDiagnosticForm.Install(mainForm);
        Application.Run(mainForm);
      }
      catch (Exception ex)
      {
        ExceptionForm exform;
        if (ex is ChessV.Exceptions.ChessVException)
          exform = new ExceptionForm(((ChessV.Exceptions.ChessVException)ex).InnerException, ((ChessV.Exceptions.ChessVException)ex).Game);
        else
          exform = new ExceptionForm(ex, null);
        exform.ShowDialog();
        return;
      }
    }
  }
}
