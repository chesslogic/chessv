
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
using ChessV.Games;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ChessV.GUI
{
  public partial class MainForm : Form
  {
    // *** PROPERTIES *** //

    //	the Manager object which list and creates Games, Engines, etc.
    public Manager.Manager Manager { get; private set; }
    private ApmwForm apmwForm;

    // *** CONSTRUCTION *** //

    #region Construction
    public MainForm()
    {
      //	initialize reference to Manager object so we don't have 
      //	to use Program.Manager all over the place
      Manager = Program.Manager;

      //	initialize map of nodes in the game catalog to tab pages
      tabControlPages = new Dictionary<CatalogNode, TabPage>();

      //	basic initialization of filtering parameters for master game list
      gameListIsFiltered = false;
      gameListRequiredTags = new List<string>();

      //	call .NET Forms Designer-generated initialization
      InitializeComponent();
      btnApmw_Click(this, new EventArgs());
    }
    #endregion


    // *** EVENT HANDLERS *** //

    #region Event Handlers
    #region Form Load Event
    private void MainForm_Load(object sender, EventArgs e)
    {
      //	Tweek the user interface depending on whether or not we 
      //	are running on Windows.  This is because the program looks 
      //	best with "Visual Styles" but Mono on Linux does not support 
      //	this and it will look extra cheezy if we don't compensate.
      //	NOTE: Windows versions prior to Vista also don't support 
      //	Visual Styles, but we don't bother to compensate for that.
      if (Program.RunningOnWindows)
        BackColor = Color.LemonChiffon;
      else
        panelGameIndexHeader.BackColor = Color.DarkGray;

      //	Populate the master game list with all known games
      updateMasterGameList();


      // *** POPULATE TAB CONTROL WITH GAME BUTTONS *** //

      //	Determine scaling in case the tab page size has been changed
      //	(by operating system hiDPI settings for example.)
      double xScale = (double)tabIndexPage.Width / 1086;
      double yScale = (double)tabIndexPage.Height / 546;
      mainTabControl.SuspendLayout();
      int pageNumber = 0;
      //	Iterate through the GameCatalog makeing a tab for each top-level category
      foreach (CatalogNode node in Program.GameCatalog.Root.Children)
      {
        //	Create a tab page for this category
        TabPage page = new TabPage(node.Name);
        //	Tweek colors if we're running on Windows (and thus 
        //	we assume we have Visual Styles available)
        if (Program.RunningOnWindows)
          page.BackColor = Color.White;
        //	Insert the tab page into the tab control
        tabControlPages.Add(node, page);
        mainTabControl.TabPages.Insert(pageNumber++, page);

        //	For now, no more than 6 games per top-level category supported
        if (node.Children.Count <= 6)
        {
          // *** BUTTON VIEW *** //

          //	determine number of rows and columns of buttons
          int nRows, nColumns;
          calcRowAndColumnCountsForButtonDisplay(node.Children.Count, out nRows, out nColumns);

          //	layout game nodes into rows and columns
          CatalogNode[,] nodes = new CatalogNode[nRows, nColumns];
          for (int x = 0; x < node.Children.Count; x++)
            nodes[x / nColumns, x % nColumns] = node.Children[x];

          int placed = 0;
          foreach (CatalogNode gamenode in node.Children)
          {
            if ((!gamenode.IsAbstract && Program.Manager.GameClasses.ContainsKey(gamenode.Name)) ||
              (gamenode.IsAbstract && gamenode.SampleGameName != null && Program.Manager.GameClasses.ContainsKey(gamenode.SampleGameName)))
            {
              try
              {
                //	calculate basic button position based on number of columns/rows
                int xoffset = (int)((nColumns == 3 ? 48 : (nColumns == 2 ? 218 : 388)) * xScale);
                int yoffset = (int)((nRows == 2 ? 32 : 162) * yScale);
                //	additional adjustments to compensate for auto-scaling based on font size
                //xoffset += (ClientSize.Width - 1118) / 2;
                //yoffset += (ClientSize.Height - 648) / 2;
                //	create an actual Game object of this type so we can ask it to 
                //	draw a presentation of itself which we will draw on the button
                Game game = createGame(gamenode.IsAbstract ? gamenode.SampleGameName : gamenode.Name);
                //	create the button and configure it
                Button newbutton = new Button();
                newbutton.BackgroundImage = createBitmapRenderingOfGame(game, xScale, yScale);
                newbutton.BackgroundImageLayout = ImageLayout.Center;
                newbutton.Location = new Point((int)((340 * (placed % nColumns) + xoffset) * xScale), (int)((260 * (placed / nColumns) + yoffset) * yScale));
                newbutton.Name = "btnChess";
                newbutton.Size = new Size((int)(293 * xScale), (int)(206 * yScale));
                newbutton.TabIndex = 0;
                newbutton.BackColor = SystemColors.ControlLight;
                newbutton.UseVisualStyleBackColor = true;
                newbutton.Tag = gamenode;
                newbutton.Click += new EventHandler(btnSelectorButtons_Click);
                //	add this button to the controls on the tab page
                page.Controls.Add(newbutton);
                //	now create a label to go underneath the button
                Label newlabel = new Label();
                newlabel.AutoSize = false;
                newlabel.Size = new Size((int)(293 * xScale), (int)(24 * yScale));
                newlabel.Location = new Point((int)((340 * (placed % nColumns) + xoffset) * xScale), (int)((260 * (placed / nColumns) + yoffset + 208) * yScale));
                newlabel.TextAlign = ContentAlignment.MiddleCenter;
                newlabel.Text = gamenode.Name;
                newlabel.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                //	add the label to the controls on the tab page
                page.Controls.Add(newlabel);
                //	keep track of how many we've placed so the 
                //	next one will be placed in the correct location
                placed++;
              }
              catch (Exception ex)
              {
                //	Something went horribly wrong.  Sadly, this isn't all that 
                //	uncommon when creating new kinds of games, so we display a 
                //	fancy ExceptionForm that gives great detail about the 
                //	exception and allows navigating to inner exceptions.
                ExceptionForm exceptionForm = new ExceptionForm(ex, null);
                exceptionForm.ShowDialog();
              }
            }
          }
        }
      }
      //	we now switch pages from zero to one and back to zero 
      //	to compensate for a glitch in the Mono tab control on Linux
      mainTabControl.SelectedTab = mainTabControl.TabPages[1];
      mainTabControl.ResumeLayout();
      mainTabControl.SelectedTab = mainTabControl.TabPages[0];

      //	start the timer (which will only fire once, just for initialization.)
      //	when it goes off, we initialize the catalog of engines and discover 
      //	any new engines automatically (if auto-discovery is enabled.)
      startTimer.Start();
    }
    #endregion

    #region Timer Tick Event
    private void startTimer_Tick(object sender, EventArgs e)
    {
      //	Stop the timer, never to be started again
      startTimer.Stop();

      // *** INITIALIZE ENGINE CATALOG *** //

      RegistryKey enginesKey = RegistrySettings.RegistryKey.OpenSubKey("Engines", true);
      if (enginesKey == null)
        enginesKey = RegistrySettings.RegistryKey.CreateSubKey("Engines");
      Manager.EngineLibrary.InitializeEngines(enginesKey, RegistrySettings.AutodetectNewEngines);
    }
    #endregion

    #region Game Selector Button Click Event
    private void btnSelectorButtons_Click(object sender, EventArgs e)
    {
      //	This function handles the click event for all the large "selector" buttons 
      //	that are automatically created based on the game catalog.  Find the node 
      //	in the game catalog that corresponds with the pressed button.
      CatalogNode node = (CatalogNode)((ButtonBase)sender).Tag;

      if (!node.IsAbstract)
      {
        //	The node is not abstract so it selects a specific game.
        //	Create the game and fire it up!
        Game game = createGame(node.Name, null, true);
        startGame(game);
      }
      else
      {
        //	This is an "abstract" node in the game catalog, so 
        //	it doesn't represent a game but rather a group of 
        //	similar games which we will display in a new list.
        //	For now, only two sub-categories exist and they are 
        //	hard-coded.  Obviously this needs to be improved.
        mainTabControl.Visible = false;
        panelSubGames.Visible = true;
        lblSubGameCategory.Text = node.Name;
        if (node.Name.ToUpper() == "CAPABLANCA VARIANTS")
        {
          //	Display pre-defined panel with information 
          //	already laid out describing Capablanca variants
          panelCapablancaVariants.Visible = true;
          panelShuffledVariants.Visible = false;
        }
        else if (node.Name.ToUpper() == "SHUFFLED VARIANTS")
        {
          //	Display pre-defined panel with information 
          //	already laid out describing shuffle variants
          panelCapablancaVariants.Visible = false;
          panelShuffledVariants.Visible = true;
        }
        //	Clear the list of games in the sub-category 
        //	just in case we've been here before
        lvGames.Items.Clear();
        //	Populate the list with the catalog child nodes
        foreach (CatalogNode childnode in node.Children)
        {
          Game game = createGame(childnode.Name);
          ListViewItem lvi = new ListViewItem(game.GameAttribute.GameName);
          lvi.SubItems.Add(game.GameAttribute.InventedBy.Replace(";", " and "));
          lvi.SubItems.Add(game.GameAttribute.Invented);
          lvi.Tag = game;
          lvi.ImageIndex = game.GameAttribute.Tags.Contains("Historic") ? 1 : 0;
          lvGames.Items.Add(lvi);
        }
        lvGames.Items[0].Selected = true;
        lvGames.Focus();
      }
    }
    #endregion

    #region Quit Button Click Event
    private void btnQuit_Click(object sender, EventArgs e)
    {
      Close();
    }
    #endregion

    #region Sub-Game List Selection Changed Event
    private void olvGames_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (lvGames.SelectedItems.Count == 1)
      {
        Game game = (Game)lvGames.SelectedItems[0].Tag;
        lblGameName.Text = game.Name;
        BoardPresentation presentation = PresentationFactory.CreatePresentation(game, true);
        Bitmap b = presentation.Render();
        Bitmap scaled = null;
        double xscale = (double)pictSubGamePreview.Size.Width / (double)b.Width;
        xscale = xscale > 1.0 ? 1.0 : xscale;
        double yscale = (double)pictSubGamePreview.Size.Height / (double)b.Height;
        yscale = yscale > 1.0 ? 1.0 : yscale;
        double scale = xscale > yscale ? yscale : xscale;
        scaled = new Bitmap(b, (int)(b.Width * scale), (int)(b.Height * scale));
        pictSubGamePreview.Image = scaled;
        if (game is ChessV.Games.CapablancaChess)
          lblRuleNote.Text = "Castling: " + ((ChessV.Games.CapablancaChess)game).Castling;
      }
    }
    #endregion

    #region Sub-Game List Back Button Click Event
    private void btnBack_Click(object sender, EventArgs e)
    {
      //	Hide the panel with the list of sub-games
      panelSubGames.Hide();
      //	Show the master tab control again
      mainTabControl.Show();
    }
    #endregion

    #region Sub-Game List Start Button Click Event
    private void btnStartGame_Click(object sender, EventArgs e)
    {
      if (lvGames.SelectedItems.Count == 1)
        startGame(createGame(((Game)lvGames.SelectedItems[0].Tag).GameAttribute.GameName, null, true));
    }
    #endregion

    #region Sub-Game List Random Button Click Event
    private void btnRandom_Click(object sender, EventArgs e)
    {
      List<Game> gamesInList = new List<Game>();
      foreach (ListViewItem item in lvGames.Items)
        gamesInList.Add((Game)item.Tag);
      int random = Program.Random.Next(gamesInList.Count);
      startGame(createGame(((Game)gamesInList[random]).GameAttribute.GameName, null, true));
    }
    #endregion

    #region Master Game List Double-Click Event
    private void lvMasterIndex_DoubleClick(object sender, EventArgs e)
    {
      if (lvMasterIndex.SelectedItems.Count == 1)
        //	Create the selected game and fire it up
        startGame(createGame(((GameAttribute)lvMasterIndex.SelectedItems[0].Tag).GameName, null, true));
    }
    #endregion

    #region Master Game List Filter Button Click Event
    private void btnFilterIndex_Click(object sender, EventArgs e)
    {
      GameIndexFilterForm form = new GameIndexFilterForm();
      form.ListIsFiltered = gameListIsFiltered;
      form.RequiredTags = gameListRequiredTags;
      form.ShowDialog();
      gameListIsFiltered = form.ListIsFiltered;
      updateMasterGameList();
    }
    #endregion

    #region About Button Click Event
    private void btnAbout_Click(object sender, EventArgs e)
    {
      AboutForm aboutform = new AboutForm();
      aboutform.ShowDialog();
    }
    #endregion

    #region Archipelago Multiworld Button Click Event
    private void btnApmw_Click(object sender, EventArgs e)
    {
      // btnApmw.Enabled = false;
      if (apmwForm == null || apmwForm.IsDisposed)
      {
        apmwForm = new ApmwForm(this);
        apmwForm.Show();
        apmwForm.FormClosing +=
          (object _s, FormClosingEventArgs _e) =>
          {
            apmwForm.Dispose();
            //btnApmw.Enabled = true;
          };
      }
      apmwForm.BringToFront();
      //Scheduler.Default.Schedule(TimeSpan.FromMilliseconds(200), apmwForm.BringToFront);

      //if (typeof(Game) == typeof(ChessV.Games.ApmwChessGame))
    }
    #endregion



    #region Engines Button Click Event
    private void btnEngines_Click(object sender, EventArgs e)
    {
      EngineListForm form = new EngineListForm();
      form.ShowDialog();
    }
    #endregion

    #region Load Game Button Click Event
    private void btnLoadGame_Click(object sender, EventArgs e)
    {
      if (openFileDialog.ShowDialog() == DialogResult.OK)
      {
        string filename = openFileDialog.FileName;
        using (TextReader reader = new StreamReader(filename))
        {
          try
          {
            Game loadedGame = Manager.LoadGame(reader);
            startGame(loadedGame);
          }
          catch (Exception ex)
          {
            ExceptionForm exceptionForm = new ExceptionForm(ex, null);
            exceptionForm.ShowDialog();
          }
        }
      }
    }
    #endregion

    #region Tools Button Click Event
    private void btnTools_Click(object sender, EventArgs e)
    {
      ToolsForm form = new ToolsForm();
      form.Show();
    }
    #endregion
    #endregion


    // *** PRIVATE HELPER FUNCTIONS *** //

    #region createGame
    private Game createGame(string name, Dictionary<string, string> definitions = null, bool interactive = false)
    {
      try
      {
        Game newgame = interactive ?
            Manager.CreateGame(name, definitions, new UnassignedGameVariablesForm(this).Helper)
          : newgame = Manager.CreateGame(name, definitions);

        return newgame;
      }
      catch (Exception ex)
      {
        ExceptionForm exceptionForm = new ExceptionForm(ex, null, name);
        exceptionForm.ShowDialog();
        return null;
      }
    }
    #endregion

    #region calcRowAndColumnCountsForButtonDisplay
    private void calcRowAndColumnCountsForButtonDisplay(int nGames, out int nRows, out int nColumns)
    {
      if (nGames <= 3)
      {
        nRows = 1;
        nColumns = nGames;
      }
      else if (nGames == 4)
      {
        nRows = 2;
        nColumns = 2;
      }
      else if (nGames <= 6)
      {
        nRows = 2;
        nColumns = 3;
      }
      else
        throw new Exception("Not Supported");
    }
    #endregion

    #region updateMasterGameList
    private void updateMasterGameList()
    {
      lvMasterIndex.Items.Clear();
      foreach (KeyValuePair<string, GameAttribute> pair in Program.Manager.GameAttributes)
        if (!pair.Value.Template && !pair.Value.Hidden)
        {
          bool showGame = true;
          if (gameListIsFiltered)
            foreach (string tag in gameListRequiredTags)
              if (!pair.Value.Tags.Contains(tag))
                showGame = false;
          if (showGame)
          {
            ListViewItem lvi = new ListViewItem(pair.Value.GameName);
            lvi.Tag = pair.Value;
            lvi.SubItems.Add(pair.Value.BoardGeometry.ToString());
            lvi.SubItems.Add(pair.Value.Invented);
            lvi.SubItems.Add(formatInventorForDisplay(pair.Value.InventedBy));
            lvMasterIndex.Items.Add(lvi);
          }
        }
    }
    #endregion

    #region formatInventorForDisplay
    private string formatInventorForDisplay(string inventor)
    {
      if (inventor == null)
        return "unknown";
      string[] inventors = inventor.Split(';');
      StringBuilder returnval = new StringBuilder(100);
      returnval.Append(inventors[0]);
      for (int x = 1; x < inventors.Length - 1; x++)
      {
        returnval.Append(", ");
        returnval.Append(inventors[x]);
      }
      if (inventors.Length > 1)
      {
        returnval.Append(" and ");
        returnval.Append(inventors[inventors.Length - 1]);
      }
      return returnval.ToString();
    }
    #endregion

    #region createBitmapRenderingOfGame
    private Bitmap createBitmapRenderingOfGame(Game game, double xScale, double yScale)
    {
      try
      {
        BoardPresentation presentation = PresentationFactory.CreatePresentation(game, true);
        Bitmap b = presentation.Render();
        Bitmap scaled = null;
        if (b.Height < 550)
        {
          if (b.Width < 625)
            scaled = new Bitmap(b, (int)(b.Width * 0.365 * xScale), (int)(b.Height * 0.365 * yScale));
          else
            scaled = new Bitmap(b, (int)(b.Width * 0.275 * xScale), (int)(b.Height * 0.275 * yScale));
        }
        else
        {
          if (b.Height > 675)
            scaled = new Bitmap(b, (int)(b.Width * 0.25 * xScale), (int)(b.Height * 0.25 * yScale));
          else
            scaled = new Bitmap(b, (int)(b.Width * 0.3 * xScale), (int)(b.Height * 0.3 * yScale));
        }
        return scaled;
      }
      catch (Exception ex)
      {
        ExceptionForm exform = new ExceptionForm(ex, game);
        exform.ShowDialog();
        Close();
        return null;
      }
    }
    #endregion

    #region startGame
    private void startGame(Game game)
    {
      //	Create and show a GameSettingsForm so the user can specify 
      //	sides, engines, time controls, etc.
      if (game.GetType() == typeof(ApmwChessGame))
      {
        StartChecksMate(game);
        return;
      }
      GameSettingsForm gameSettingsForm = new GameSettingsForm(game);
      DialogResult result = gameSettingsForm.ShowDialog();
      TimeControl timeControl = gameSettingsForm.TimeControl;

      if (result == DialogResult.OK)
      {
        EngineSettingsForm engineSettingsForm = new EngineSettingsForm();
        engineSettingsForm.TTSizeInMB = game.TTSizeInMB;
        engineSettingsForm.Variation = game.Variation;
        engineSettingsForm.Weakening = game.Weakening;
        if (engineSettingsForm.ShowDialog() == DialogResult.OK)
        {
          if (engineSettingsForm.Variation != game.Variation)
          {
            game.Variation = engineSettingsForm.Variation;
            game.VariationChanged();
          }
          game.TTSizeInMB = engineSettingsForm.TTSizeInMB;
          game.Weakening = engineSettingsForm.Weakening;
        }
        if (gameSettingsForm.ComputerPlayerCount == 0)
        {
          //	Two humans ...
          game.ComputerControlled[0] = false;
          game.ComputerControlled[1] = false;
          game.AddHuman(0);
          game.AddHuman(1);
        }
        else if (gameSettingsForm.ComputerPlayerCount == 1)
        {
          //	Human vs. Computer
          game.ComputerControlled[0] = (gameSettingsForm.ComputerSide == 0);
          game.ComputerControlled[1] = (gameSettingsForm.ComputerSide == 1);
          //	Add engine (the interal engine or external XBoard engine)
          if (gameSettingsForm.EngineConfigurations[0].Configuration == Manager.InternalEngine)
          {
            game.AddInternalEngine(gameSettingsForm.ComputerSide);
          }
          else
          {
            if (gameSettingsForm.EngineConfigurations[0].Configuration.SupportedFeatures.Contains("memory"))
              gameSettingsForm.EngineConfigurations[0].Configuration.AddOption(new EngineOptions.EngineTextOption("memory", engineSettingsForm.XBoardMemory));
            if (gameSettingsForm.EngineConfigurations[0].Configuration.SupportedFeatures.Contains("cores"))
              gameSettingsForm.EngineConfigurations[0].Configuration.AddOption(new EngineOptions.EngineTextOption("cores", engineSettingsForm.XBoardCores));
            game.AddEngine(gameSettingsForm.EngineConfigurations[0], gameSettingsForm.ComputerSide);
          }
          //	Add the human
          game.AddHuman(gameSettingsForm.ComputerSide ^ 1);
        }
        else if (gameSettingsForm.ComputerPlayerCount == 2)
        {
          //	Two computers
          game.ComputerControlled[0] = true;
          game.ComputerControlled[1] = true;
          //	Add first engine (the interal engine or external XBoard engine)
          if (gameSettingsForm.EngineConfigurations[0].Configuration == Manager.InternalEngine)
          {
            game.AddInternalEngine(0);
          }
          else
          {
            if (gameSettingsForm.EngineConfigurations[0].Configuration.SupportedFeatures.Contains("memory"))
              gameSettingsForm.EngineConfigurations[0].Configuration.AddOption(new EngineOptions.EngineTextOption("memory", engineSettingsForm.XBoardMemory));
            if (gameSettingsForm.EngineConfigurations[0].Configuration.SupportedFeatures.Contains("cores"))
              gameSettingsForm.EngineConfigurations[0].Configuration.AddOption(new EngineOptions.EngineTextOption("cores", engineSettingsForm.XBoardCores));
            game.AddEngine(gameSettingsForm.EngineConfigurations[0], 0);
          }
          //	Add second engine (the interal engine or external XBoard engine)
          if (gameSettingsForm.EngineConfigurations[1].Configuration == Manager.InternalEngine)
          {
            game.AddInternalEngine(1);
          }
          else
          {
            if (gameSettingsForm.EngineConfigurations[1].Configuration.SupportedFeatures.Contains("memory"))
              gameSettingsForm.EngineConfigurations[1].Configuration.AddOption(new EngineOptions.EngineTextOption("memory", engineSettingsForm.XBoardMemory));
            if (gameSettingsForm.EngineConfigurations[1].Configuration.SupportedFeatures.Contains("cores"))
              gameSettingsForm.EngineConfigurations[1].Configuration.AddOption(new EngineOptions.EngineTextOption("cores", engineSettingsForm.XBoardCores));
            game.AddEngine(gameSettingsForm.EngineConfigurations[1], 1);
          }
        }
        //	Configure the "Match" object with the time control
        game.Match.SetTimeControl(timeControl);
        //	Create and show the GameForm (the actual Game GUI)
        GameForm gameForm = new GameForm(game);
        gameForm.Show();
      }
    }
    #endregion

    public void StartChecksMate(Game game)
    {
      var difficulty = Math.Pow(3, 10 - ApmwCore.getInstance().EngineWeakeningProvider());

      TimeControl timeControl = new TimeControl();
      timeControl.Infinite = true;
      timeControl.NodeLimit = Math.Min(25000, Convert.ToInt64(difficulty));
      game.Match.SetTimeControl(timeControl);

      int player = ApmwCore.getInstance().GeriProvider();
      game.AddHuman(player);
      game.AddInternalEngine(player ^ 1);
      game.ComputerControlled[player] = false;
      game.ComputerControlled[player ^ 1] = true;
      game.TTSizeInMB = 256;
      game.Variation = 1;
      game.Weakening = Math.Min(15, 3 * ApmwCore.getInstance().EngineWeakeningProvider());

      GameForm gameForm = new GameForm(game);
      gameForm.Show();
      gameForm.boardControl.RotateBoard = player != 0;
      gameForm.label_check.Visible = false;
    }

    // *** PRIVATE DATA MEMBERS *** //

    private Dictionary<CatalogNode, TabPage> tabControlPages;
    private List<string> gameListRequiredTags;
    private bool gameListIsFiltered;
  }
}
