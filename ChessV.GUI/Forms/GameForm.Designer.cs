
/***************************************************************************

                                 ChessV

                  COPYRIGHT (C) 2012-2017 BY GREG STRONG

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

namespace ChessV.GUI
{
	partial class GameForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if( disposing && (components != null) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      components = new System.ComponentModel.Container();
      splitContainer1 = new System.Windows.Forms.SplitContainer();
      splitContainer2 = new System.Windows.Forms.SplitContainer();
      boardControl = new BoardControl();
      listThinking1 = new System.Windows.Forms.ListView();
      headerScore = new System.Windows.Forms.ColumnHeader();
      headerTime = new System.Windows.Forms.ColumnHeader();
      headerNodes = new System.Windows.Forms.ColumnHeader();
      headerPV = new System.Windows.Forms.ColumnHeader();
      splitContainer3 = new System.Windows.Forms.SplitContainer();
      pictFirst = new System.Windows.Forms.PictureBox();
      pictPrevious = new System.Windows.Forms.PictureBox();
      pictLast = new System.Windows.Forms.PictureBox();
      pictNext = new System.Windows.Forms.PictureBox();
      pictStop = new System.Windows.Forms.PictureBox();
      panelClocks = new System.Windows.Forms.Panel();
      gems_label = new System.Windows.Forms.Label();
      labelTime1 = new System.Windows.Forms.Label();
      labelTime0 = new System.Windows.Forms.Label();
      labelPlayer1 = new System.Windows.Forms.Label();
      labelPlayer0 = new System.Windows.Forms.Label();
      lblReviewMode = new System.Windows.Forms.Label();
      listMoves = new System.Windows.Forms.ListView();
      columnHeader1 = new System.Windows.Forms.ColumnHeader();
      columnHeader2 = new System.Windows.Forms.ColumnHeader();
      columnHeader3 = new System.Windows.Forms.ColumnHeader();
      evaluationTabControl = new System.Windows.Forms.TabControl();
      tabMaterial = new System.Windows.Forms.TabPage();
      mbControl = new MaterialBalanceControl();
      tabEvalHistory = new System.Windows.Forms.TabPage();
      ehControl = new EvaluationHistoryControl();
      menuStrip1 = new System.Windows.Forms.MenuStrip();
      menu_File = new System.Windows.Forms.ToolStripMenuItem();
      menuitem_SaveGame = new System.Windows.Forms.ToolStripMenuItem();
      menuitem_Exit = new System.Windows.Forms.ToolStripMenuItem();
      menu_Game = new System.Windows.Forms.ToolStripMenuItem();
      menuitem_TakeBackMove = new System.Windows.Forms.ToolStripMenuItem();
      menuitem_TakeBackAllMoves = new System.Windows.Forms.ToolStripMenuItem();
      toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
      menuitem_ComputerPlays0 = new System.Windows.Forms.ToolStripMenuItem();
      menuitem_ComputerPlays1 = new System.Windows.Forms.ToolStripMenuItem();
      menuitem_StopThinking = new System.Windows.Forms.ToolStripMenuItem();
      toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
      menuitem_QuickAnalysis = new System.Windows.Forms.ToolStripMenuItem();
      menuitem_StaticEvaluation = new System.Windows.Forms.ToolStripMenuItem();
      menuitem_MultiPVAnalysis = new System.Windows.Forms.ToolStripMenuItem();
      menuitem_Perft = new System.Windows.Forms.ToolStripMenuItem();
      toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
      menuitem_LoadPositionByFEN = new System.Windows.Forms.ToolStripMenuItem();
      menu_Options = new System.Windows.Forms.ToolStripMenuItem();
      menuitem_Appearance = new System.Windows.Forms.ToolStripMenuItem();
      menuitem_EnableCustomTheme = new System.Windows.Forms.ToolStripMenuItem();
      toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
      menuitem_UncheckeredBoard = new System.Windows.Forms.ToolStripMenuItem();
      menuitem_CheckeredBoard = new System.Windows.Forms.ToolStripMenuItem();
      menuitem_ThreeColorBoard = new System.Windows.Forms.ToolStripMenuItem();
      toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
      menuitem_HighlightComputerMove = new System.Windows.Forms.ToolStripMenuItem();
      toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
      menuitem_RotateBoard = new System.Windows.Forms.ToolStripMenuItem();
      menu_Tools = new System.Windows.Forms.ToolStripMenuItem();
      menuitem_ShowEngineDebugWindow = new System.Windows.Forms.ToolStripMenuItem();
      menuitem_ShowEngineStatisticsWindow = new System.Windows.Forms.ToolStripMenuItem();
      menu_Help = new System.Windows.Forms.ToolStripMenuItem();
      menuitem_About = new System.Windows.Forms.ToolStripMenuItem();
      menuPieceContext = new System.Windows.Forms.ContextMenuStrip(components);
      propertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      timer = new System.Windows.Forms.Timer(components);
      saveFileDialog = new System.Windows.Forms.SaveFileDialog();
      label_check = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
      splitContainer1.Panel1.SuspendLayout();
      splitContainer1.Panel2.SuspendLayout();
      splitContainer1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
      splitContainer2.Panel1.SuspendLayout();
      splitContainer2.Panel2.SuspendLayout();
      splitContainer2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)splitContainer3).BeginInit();
      splitContainer3.Panel1.SuspendLayout();
      splitContainer3.Panel2.SuspendLayout();
      splitContainer3.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)pictFirst).BeginInit();
      ((System.ComponentModel.ISupportInitialize)pictPrevious).BeginInit();
      ((System.ComponentModel.ISupportInitialize)pictLast).BeginInit();
      ((System.ComponentModel.ISupportInitialize)pictNext).BeginInit();
      ((System.ComponentModel.ISupportInitialize)pictStop).BeginInit();
      panelClocks.SuspendLayout();
      evaluationTabControl.SuspendLayout();
      tabMaterial.SuspendLayout();
      tabEvalHistory.SuspendLayout();
      menuStrip1.SuspendLayout();
      menuPieceContext.SuspendLayout();
      SuspendLayout();
      // 
      // splitContainer1
      // 
      splitContainer1.BackColor = System.Drawing.SystemColors.Control;
      splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
      splitContainer1.IsSplitterFixed = true;
      splitContainer1.Location = new System.Drawing.Point(0, 24);
      splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      splitContainer1.Panel1.Controls.Add(splitContainer2);
      // 
      // splitContainer1.Panel2
      // 
      splitContainer1.Panel2.BackColor = System.Drawing.Color.BlanchedAlmond;
      splitContainer1.Panel2.Controls.Add(splitContainer3);
      splitContainer1.Size = new System.Drawing.Size(1078, 832);
      splitContainer1.SplitterDistance = 499;
      splitContainer1.SplitterWidth = 5;
      splitContainer1.TabIndex = 0;
      // 
      // splitContainer2
      // 
      splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
      splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
      splitContainer2.IsSplitterFixed = true;
      splitContainer2.Location = new System.Drawing.Point(0, 0);
      splitContainer2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      splitContainer2.Name = "splitContainer2";
      splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer2.Panel1
      // 
      splitContainer2.Panel1.Controls.Add(boardControl);
      // 
      // splitContainer2.Panel2
      // 
      splitContainer2.Panel2.BackColor = System.Drawing.Color.Black;
      splitContainer2.Panel2.Controls.Add(label_check);
      splitContainer2.Panel2.Controls.Add(listThinking1);
      splitContainer2.Size = new System.Drawing.Size(499, 832);
      splitContainer2.SplitterDistance = 498;
      splitContainer2.SplitterWidth = 5;
      splitContainer2.TabIndex = 0;
      // 
      // boardControl
      // 
      boardControl.HighlightMove = false;
      boardControl.Location = new System.Drawing.Point(0, 0);
      boardControl.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      boardControl.Name = "boardControl";
      boardControl.RotateBoard = false;
      boardControl.Size = new System.Drawing.Size(497, 497);
      boardControl.TabIndex = 0;
      // 
      // listThinking1
      // 
      listThinking1.BackColor = System.Drawing.Color.FromArgb(255, 255, 204);
      listThinking1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { headerScore, headerTime, headerNodes, headerPV });
      listThinking1.Dock = System.Windows.Forms.DockStyle.Fill;
      listThinking1.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      listThinking1.LabelEdit = true;
      listThinking1.Location = new System.Drawing.Point(0, 0);
      listThinking1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      listThinking1.MultiSelect = false;
      listThinking1.Name = "listThinking1";
      listThinking1.Size = new System.Drawing.Size(495, 325);
      listThinking1.TabIndex = 0;
      listThinking1.UseCompatibleStateImageBehavior = false;
      listThinking1.View = System.Windows.Forms.View.Details;
      // 
      // headerScore
      // 
      headerScore.Text = "Score";
      headerScore.Width = 80;
      // 
      // headerTime
      // 
      headerTime.Text = "Time";
      headerTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      headerTime.Width = 70;
      // 
      // headerNodes
      // 
      headerNodes.Text = "Nodes";
      headerNodes.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      headerNodes.Width = 70;
      // 
      // headerPV
      // 
      headerPV.Text = "PV";
      headerPV.Width = 400;
      // 
      // splitContainer3
      // 
      splitContainer3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
      splitContainer3.IsSplitterFixed = true;
      splitContainer3.Location = new System.Drawing.Point(0, 0);
      splitContainer3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      splitContainer3.Name = "splitContainer3";
      splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer3.Panel1
      // 
      splitContainer3.Panel1.BackColor = System.Drawing.SystemColors.Control;
      splitContainer3.Panel1.Controls.Add(pictFirst);
      splitContainer3.Panel1.Controls.Add(pictPrevious);
      splitContainer3.Panel1.Controls.Add(pictLast);
      splitContainer3.Panel1.Controls.Add(pictNext);
      splitContainer3.Panel1.Controls.Add(pictStop);
      splitContainer3.Panel1.Controls.Add(panelClocks);
      splitContainer3.Panel1.Controls.Add(lblReviewMode);
      splitContainer3.Panel1.Controls.Add(listMoves);
      // 
      // splitContainer3.Panel2
      // 
      splitContainer3.Panel2.BackColor = System.Drawing.Color.FromArgb(255, 255, 204);
      splitContainer3.Panel2.Controls.Add(evaluationTabControl);
      splitContainer3.Size = new System.Drawing.Size(574, 832);
      splitContainer3.SplitterDistance = 652;
      splitContainer3.SplitterWidth = 5;
      splitContainer3.TabIndex = 0;
      // 
      // pictFirst
      // 
      pictFirst.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
      pictFirst.Image = Properties.Resources.icon_gray_first;
      pictFirst.Location = new System.Drawing.Point(179, 616);
      pictFirst.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      pictFirst.Name = "pictFirst";
      pictFirst.Size = new System.Drawing.Size(36, 29);
      pictFirst.TabIndex = 7;
      pictFirst.TabStop = false;
      pictFirst.Click += pictFirst_Click;
      // 
      // pictPrevious
      // 
      pictPrevious.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
      pictPrevious.Image = Properties.Resources.icon_gray_previous;
      pictPrevious.Location = new System.Drawing.Point(227, 616);
      pictPrevious.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      pictPrevious.Name = "pictPrevious";
      pictPrevious.Size = new System.Drawing.Size(29, 29);
      pictPrevious.TabIndex = 6;
      pictPrevious.TabStop = false;
      pictPrevious.Click += pictPrevious_Click;
      pictPrevious.DoubleClick += pictPrevious_Click;
      // 
      // pictLast
      // 
      pictLast.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
      pictLast.Image = Properties.Resources.icon_gray_last;
      pictLast.Location = new System.Drawing.Point(352, 616);
      pictLast.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      pictLast.Name = "pictLast";
      pictLast.Size = new System.Drawing.Size(36, 29);
      pictLast.TabIndex = 5;
      pictLast.TabStop = false;
      pictLast.Click += pictLast_Click;
      // 
      // pictNext
      // 
      pictNext.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
      pictNext.Image = Properties.Resources.icon_gray_next;
      pictNext.Location = new System.Drawing.Point(311, 616);
      pictNext.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      pictNext.Name = "pictNext";
      pictNext.Size = new System.Drawing.Size(29, 29);
      pictNext.TabIndex = 4;
      pictNext.TabStop = false;
      pictNext.Click += pictNext_Click;
      pictNext.DoubleClick += pictNext_Click;
      // 
      // pictStop
      // 
      pictStop.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
      pictStop.Image = Properties.Resources.icon_gray_stop;
      pictStop.ImageLocation = "";
      pictStop.Location = new System.Drawing.Point(269, 616);
      pictStop.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      pictStop.Name = "pictStop";
      pictStop.Size = new System.Drawing.Size(28, 28);
      pictStop.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
      pictStop.TabIndex = 3;
      pictStop.TabStop = false;
      pictStop.Click += pictStop_Click;
      // 
      // panelClocks
      // 
      panelClocks.Anchor = System.Windows.Forms.AnchorStyles.Top;
      panelClocks.BackColor = System.Drawing.Color.Transparent;
      panelClocks.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      panelClocks.Controls.Add(gems_label);
      panelClocks.Controls.Add(labelTime1);
      panelClocks.Controls.Add(labelTime0);
      panelClocks.Controls.Add(labelPlayer1);
      panelClocks.Controls.Add(labelPlayer0);
      panelClocks.Location = new System.Drawing.Point(46, 3);
      panelClocks.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      panelClocks.Name = "panelClocks";
      panelClocks.Size = new System.Drawing.Size(477, 85);
      panelClocks.TabIndex = 1;
      // 
      // gems_label
      // 
      gems_label.Anchor = System.Windows.Forms.AnchorStyles.None;
      gems_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      gems_label.Location = new System.Drawing.Point(181, 6);
      gems_label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      gems_label.Name = "gems_label";
      gems_label.Size = new System.Drawing.Size(113, 17);
      gems_label.TabIndex = 4;
      gems_label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // labelTime1
      // 
      labelTime1.BackColor = System.Drawing.Color.Black;
      labelTime1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      labelTime1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      labelTime1.ForeColor = System.Drawing.Color.White;
      labelTime1.Location = new System.Drawing.Point(240, 24);
      labelTime1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      labelTime1.Name = "labelTime1";
      labelTime1.Size = new System.Drawing.Size(106, 40);
      labelTime1.TabIndex = 3;
      labelTime1.Text = "0:00";
      labelTime1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // labelTime0
      // 
      labelTime0.BackColor = System.Drawing.Color.White;
      labelTime0.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      labelTime0.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      labelTime0.ForeColor = System.Drawing.Color.Black;
      labelTime0.Location = new System.Drawing.Point(127, 24);
      labelTime0.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      labelTime0.Name = "labelTime0";
      labelTime0.Size = new System.Drawing.Size(106, 40);
      labelTime0.TabIndex = 2;
      labelTime0.Text = "0:00";
      labelTime0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // labelPlayer1
      // 
      labelPlayer1.BackColor = System.Drawing.Color.Transparent;
      labelPlayer1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      labelPlayer1.Location = new System.Drawing.Point(354, 14);
      labelPlayer1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      labelPlayer1.Name = "labelPlayer1";
      labelPlayer1.Size = new System.Drawing.Size(117, 58);
      labelPlayer1.TabIndex = 1;
      labelPlayer1.Text = "ChessV";
      labelPlayer1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // labelPlayer0
      // 
      labelPlayer0.BackColor = System.Drawing.Color.Transparent;
      labelPlayer0.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      labelPlayer0.Location = new System.Drawing.Point(4, 14);
      labelPlayer0.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      labelPlayer0.Name = "labelPlayer0";
      labelPlayer0.Size = new System.Drawing.Size(117, 58);
      labelPlayer0.TabIndex = 0;
      labelPlayer0.Text = "Player";
      labelPlayer0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // lblReviewMode
      // 
      lblReviewMode.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
      lblReviewMode.Location = new System.Drawing.Point(465, 620);
      lblReviewMode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      lblReviewMode.Name = "lblReviewMode";
      lblReviewMode.Size = new System.Drawing.Size(99, 23);
      lblReviewMode.TabIndex = 2;
      lblReviewMode.Text = "Review Mode";
      lblReviewMode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      lblReviewMode.Visible = false;
      // 
      // listMoves
      // 
      listMoves.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
      listMoves.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3 });
      listMoves.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      listMoves.FullRowSelect = true;
      listMoves.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
      listMoves.Location = new System.Drawing.Point(-2, 96);
      listMoves.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      listMoves.Name = "listMoves";
      listMoves.Size = new System.Drawing.Size(574, 516);
      listMoves.TabIndex = 0;
      listMoves.UseCompatibleStateImageBehavior = false;
      listMoves.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader1
      // 
      columnHeader1.Text = "Move No";
      columnHeader1.Width = 55;
      // 
      // columnHeader2
      // 
      columnHeader2.Text = "Move";
      columnHeader2.Width = 150;
      // 
      // columnHeader3
      // 
      columnHeader3.Text = "Description";
      columnHeader3.Width = 157;
      // 
      // evaluationTabControl
      // 
      evaluationTabControl.Controls.Add(tabMaterial);
      evaluationTabControl.Controls.Add(tabEvalHistory);
      evaluationTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      evaluationTabControl.Location = new System.Drawing.Point(0, 0);
      evaluationTabControl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      evaluationTabControl.Name = "evaluationTabControl";
      evaluationTabControl.SelectedIndex = 0;
      evaluationTabControl.Size = new System.Drawing.Size(570, 171);
      evaluationTabControl.TabIndex = 0;
      // 
      // tabMaterial
      // 
      tabMaterial.BackColor = System.Drawing.Color.FromArgb(255, 255, 204);
      tabMaterial.Controls.Add(mbControl);
      tabMaterial.Location = new System.Drawing.Point(4, 24);
      tabMaterial.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      tabMaterial.Name = "tabMaterial";
      tabMaterial.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      tabMaterial.Size = new System.Drawing.Size(562, 143);
      tabMaterial.TabIndex = 0;
      tabMaterial.Text = "Material Balance";
      // 
      // mbControl
      // 
      mbControl.BackColor = System.Drawing.Color.White;
      mbControl.Dock = System.Windows.Forms.DockStyle.Fill;
      mbControl.Location = new System.Drawing.Point(4, 3);
      mbControl.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      mbControl.Name = "mbControl";
      mbControl.Size = new System.Drawing.Size(554, 137);
      mbControl.TabIndex = 0;
      mbControl.Theme = null;
      // 
      // tabEvalHistory
      // 
      tabEvalHistory.BackColor = System.Drawing.Color.FromArgb(255, 255, 204);
      tabEvalHistory.Controls.Add(ehControl);
      tabEvalHistory.Location = new System.Drawing.Point(4, 24);
      tabEvalHistory.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      tabEvalHistory.Name = "tabEvalHistory";
      tabEvalHistory.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
      tabEvalHistory.Size = new System.Drawing.Size(558, 143);
      tabEvalHistory.TabIndex = 1;
      tabEvalHistory.Text = "Evaluation History";
      // 
      // ehControl
      // 
      ehControl.Dock = System.Windows.Forms.DockStyle.Fill;
      ehControl.Location = new System.Drawing.Point(4, 3);
      ehControl.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      ehControl.Name = "ehControl";
      ehControl.Size = new System.Drawing.Size(550, 137);
      ehControl.TabIndex = 0;
      // 
      // menuStrip1
      // 
      menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { menu_File, menu_Game, menu_Options, menu_Tools, menu_Help });
      menuStrip1.Location = new System.Drawing.Point(0, 0);
      menuStrip1.Name = "menuStrip1";
      menuStrip1.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
      menuStrip1.Size = new System.Drawing.Size(1078, 24);
      menuStrip1.TabIndex = 1;
      menuStrip1.Text = "menuStrip1";
      // 
      // menu_File
      // 
      menu_File.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { menuitem_SaveGame, menuitem_Exit });
      menu_File.Name = "menu_File";
      menu_File.Size = new System.Drawing.Size(37, 20);
      menu_File.Text = "File";
      // 
      // menuitem_SaveGame
      // 
      menuitem_SaveGame.Name = "menuitem_SaveGame";
      menuitem_SaveGame.Size = new System.Drawing.Size(132, 22);
      menuitem_SaveGame.Text = "Save Game";
      menuitem_SaveGame.Click += menuitem_SaveGame_Click;
      // 
      // menuitem_Exit
      // 
      menuitem_Exit.Name = "menuitem_Exit";
      menuitem_Exit.Size = new System.Drawing.Size(132, 22);
      menuitem_Exit.Text = "Exit";
      menuitem_Exit.Click += menuitem_Exit_Click;
      // 
      // menu_Game
      // 
      menu_Game.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { menuitem_TakeBackMove, menuitem_TakeBackAllMoves, toolStripSeparator1, menuitem_ComputerPlays0, menuitem_ComputerPlays1, menuitem_StopThinking, toolStripSeparator4, menuitem_QuickAnalysis, menuitem_StaticEvaluation, menuitem_MultiPVAnalysis, menuitem_Perft, toolStripSeparator6, menuitem_LoadPositionByFEN });
      menu_Game.Name = "menu_Game";
      menu_Game.Size = new System.Drawing.Size(50, 20);
      menu_Game.Text = "Game";
      menu_Game.DropDownOpening += menu_Game_DropDownOpening;
      // 
      // menuitem_TakeBackMove
      // 
      menuitem_TakeBackMove.Enabled = false;
      menuitem_TakeBackMove.Name = "menuitem_TakeBackMove";
      menuitem_TakeBackMove.Size = new System.Drawing.Size(207, 22);
      menuitem_TakeBackMove.Text = "Take Back Move";
      menuitem_TakeBackMove.Click += menuitem_TakeBackMove_Click;
      // 
      // menuitem_TakeBackAllMoves
      // 
      menuitem_TakeBackAllMoves.Enabled = false;
      menuitem_TakeBackAllMoves.Name = "menuitem_TakeBackAllMoves";
      menuitem_TakeBackAllMoves.Size = new System.Drawing.Size(207, 22);
      menuitem_TakeBackAllMoves.Text = "Take Back All Moves";
      menuitem_TakeBackAllMoves.Click += menuitem_TakeBackAllMoves_Click;
      // 
      // toolStripSeparator1
      // 
      toolStripSeparator1.Name = "toolStripSeparator1";
      toolStripSeparator1.Size = new System.Drawing.Size(204, 6);
      // 
      // menuitem_ComputerPlays0
      // 
      menuitem_ComputerPlays0.Enabled = false;
      menuitem_ComputerPlays0.Name = "menuitem_ComputerPlays0";
      menuitem_ComputerPlays0.Size = new System.Drawing.Size(207, 22);
      menuitem_ComputerPlays0.Text = "Computer Plays 0";
      menuitem_ComputerPlays0.Click += menuitem_ComputerPlays0_Click;
      // 
      // menuitem_ComputerPlays1
      // 
      menuitem_ComputerPlays1.Enabled = false;
      menuitem_ComputerPlays1.Name = "menuitem_ComputerPlays1";
      menuitem_ComputerPlays1.Size = new System.Drawing.Size(207, 22);
      menuitem_ComputerPlays1.Text = "Computer Plays 1";
      menuitem_ComputerPlays1.Click += menuitem_ComputerPlays1_Click;
      // 
      // menuitem_StopThinking
      // 
      menuitem_StopThinking.Name = "menuitem_StopThinking";
      menuitem_StopThinking.Size = new System.Drawing.Size(207, 22);
      menuitem_StopThinking.Text = "Stop Thinking";
      menuitem_StopThinking.Click += menuitem_StopThinking_Click;
      // 
      // toolStripSeparator4
      // 
      toolStripSeparator4.Name = "toolStripSeparator4";
      toolStripSeparator4.Size = new System.Drawing.Size(204, 6);
      // 
      // menuitem_QuickAnalysis
      // 
      menuitem_QuickAnalysis.Name = "menuitem_QuickAnalysis";
      menuitem_QuickAnalysis.Size = new System.Drawing.Size(207, 22);
      menuitem_QuickAnalysis.Text = "Quick Analysis";
      menuitem_QuickAnalysis.Click += quickAnalysisToolStripMenuItem_Click;
      // 
      // menuitem_StaticEvaluation
      // 
      menuitem_StaticEvaluation.Name = "menuitem_StaticEvaluation";
      menuitem_StaticEvaluation.Size = new System.Drawing.Size(207, 22);
      menuitem_StaticEvaluation.Text = "Static Evaluation";
      menuitem_StaticEvaluation.Click += menuitem_StaticEvaluation_Click;
      // 
      // menuitem_MultiPVAnalysis
      // 
      menuitem_MultiPVAnalysis.Name = "menuitem_MultiPVAnalysis";
      menuitem_MultiPVAnalysis.Size = new System.Drawing.Size(207, 22);
      menuitem_MultiPVAnalysis.Text = "Multi-PV Analysis ...";
      menuitem_MultiPVAnalysis.Click += menuitem_MultiPVAnalysis_Click;
      // 
      // menuitem_Perft
      // 
      menuitem_Perft.Name = "menuitem_Perft";
      menuitem_Perft.Size = new System.Drawing.Size(207, 22);
      menuitem_Perft.Text = "Perft";
      menuitem_Perft.Click += perftToolStripMenuItem_Click;
      // 
      // toolStripSeparator6
      // 
      toolStripSeparator6.Name = "toolStripSeparator6";
      toolStripSeparator6.Size = new System.Drawing.Size(204, 6);
      // 
      // menuitem_LoadPositionByFEN
      // 
      menuitem_LoadPositionByFEN.Name = "menuitem_LoadPositionByFEN";
      menuitem_LoadPositionByFEN.Size = new System.Drawing.Size(207, 22);
      menuitem_LoadPositionByFEN.Text = "Get or Set Position FEN ...";
      menuitem_LoadPositionByFEN.Click += menuitem_LoadPositionByFEN_Click;
      // 
      // menu_Options
      // 
      menu_Options.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { menuitem_Appearance, menuitem_EnableCustomTheme, toolStripSeparator2, menuitem_UncheckeredBoard, menuitem_CheckeredBoard, menuitem_ThreeColorBoard, toolStripSeparator5, menuitem_HighlightComputerMove, toolStripSeparator3, menuitem_RotateBoard });
      menu_Options.Name = "menu_Options";
      menu_Options.Size = new System.Drawing.Size(61, 20);
      menu_Options.Text = "Options";
      menu_Options.DropDownOpening += optionsToolStripMenuItem_DropDownOpening;
      // 
      // menuitem_Appearance
      // 
      menuitem_Appearance.Name = "menuitem_Appearance";
      menuitem_Appearance.Size = new System.Drawing.Size(214, 22);
      menuitem_Appearance.Text = "Appearance";
      menuitem_Appearance.Click += appearanceToolStripMenuItem_Click;
      // 
      // menuitem_EnableCustomTheme
      // 
      menuitem_EnableCustomTheme.Name = "menuitem_EnableCustomTheme";
      menuitem_EnableCustomTheme.Size = new System.Drawing.Size(214, 22);
      menuitem_EnableCustomTheme.Text = "Enable Custom Theme";
      menuitem_EnableCustomTheme.Click += menuitem_EnableCustomTheme_Click;
      // 
      // toolStripSeparator2
      // 
      toolStripSeparator2.Name = "toolStripSeparator2";
      toolStripSeparator2.Size = new System.Drawing.Size(211, 6);
      // 
      // menuitem_UncheckeredBoard
      // 
      menuitem_UncheckeredBoard.Name = "menuitem_UncheckeredBoard";
      menuitem_UncheckeredBoard.Size = new System.Drawing.Size(214, 22);
      menuitem_UncheckeredBoard.Text = "Uncheckered Board";
      menuitem_UncheckeredBoard.Click += menuitem_UncheckeredBoard_Click;
      // 
      // menuitem_CheckeredBoard
      // 
      menuitem_CheckeredBoard.Name = "menuitem_CheckeredBoard";
      menuitem_CheckeredBoard.Size = new System.Drawing.Size(214, 22);
      menuitem_CheckeredBoard.Text = "Checkered Board";
      menuitem_CheckeredBoard.Click += menuitem_CheckeredBoard_Click;
      // 
      // menuitem_ThreeColorBoard
      // 
      menuitem_ThreeColorBoard.Name = "menuitem_ThreeColorBoard";
      menuitem_ThreeColorBoard.Size = new System.Drawing.Size(214, 22);
      menuitem_ThreeColorBoard.Text = "Three-Color Board";
      menuitem_ThreeColorBoard.Click += menuitem_ThreeColorBoard_Click;
      // 
      // toolStripSeparator5
      // 
      toolStripSeparator5.Name = "toolStripSeparator5";
      toolStripSeparator5.Size = new System.Drawing.Size(211, 6);
      // 
      // menuitem_HighlightComputerMove
      // 
      menuitem_HighlightComputerMove.Name = "menuitem_HighlightComputerMove";
      menuitem_HighlightComputerMove.Size = new System.Drawing.Size(214, 22);
      menuitem_HighlightComputerMove.Text = "Highlight Computer Move";
      menuitem_HighlightComputerMove.Click += menuitem_HighlightComputerMove_Click;
      // 
      // toolStripSeparator3
      // 
      toolStripSeparator3.Name = "toolStripSeparator3";
      toolStripSeparator3.Size = new System.Drawing.Size(211, 6);
      // 
      // menuitem_RotateBoard
      // 
      menuitem_RotateBoard.Name = "menuitem_RotateBoard";
      menuitem_RotateBoard.Size = new System.Drawing.Size(214, 22);
      menuitem_RotateBoard.Text = "Rotate Board";
      menuitem_RotateBoard.Click += menuitem_RotateBoard_Click;
      // 
      // menu_Tools
      // 
      menu_Tools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { menuitem_ShowEngineDebugWindow, menuitem_ShowEngineStatisticsWindow });
      menu_Tools.Name = "menu_Tools";
      menu_Tools.Size = new System.Drawing.Size(46, 20);
      menu_Tools.Text = "Tools";
      menu_Tools.DropDownOpening += menu_Tools_DropDownOpening;
      // 
      // menuitem_ShowEngineDebugWindow
      // 
      menuitem_ShowEngineDebugWindow.Name = "menuitem_ShowEngineDebugWindow";
      menuitem_ShowEngineDebugWindow.Size = new System.Drawing.Size(238, 22);
      menuitem_ShowEngineDebugWindow.Text = "Show Engine Debug Window";
      menuitem_ShowEngineDebugWindow.Click += menuitem_ShowEngineDebugWindow_Click;
      // 
      // menuitem_ShowEngineStatisticsWindow
      // 
      menuitem_ShowEngineStatisticsWindow.Name = "menuitem_ShowEngineStatisticsWindow";
      menuitem_ShowEngineStatisticsWindow.Size = new System.Drawing.Size(238, 22);
      menuitem_ShowEngineStatisticsWindow.Text = "Show Engine Statistics Window";
      menuitem_ShowEngineStatisticsWindow.Click += menuitem_ShowEngineStatisticsWindow_Click;
      // 
      // menu_Help
      // 
      menu_Help.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { menuitem_About });
      menu_Help.Name = "menu_Help";
      menu_Help.Size = new System.Drawing.Size(44, 20);
      menu_Help.Text = "Help";
      // 
      // menuitem_About
      // 
      menuitem_About.Name = "menuitem_About";
      menuitem_About.Size = new System.Drawing.Size(148, 22);
      menuitem_About.Text = "About ChessV";
      menuitem_About.Click += menuitem_About_Click;
      // 
      // menuPieceContext
      // 
      menuPieceContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { propertiesToolStripMenuItem });
      menuPieceContext.Name = "menuPieceContext";
      menuPieceContext.Size = new System.Drawing.Size(128, 26);
      // 
      // propertiesToolStripMenuItem
      // 
      propertiesToolStripMenuItem.Name = "propertiesToolStripMenuItem";
      propertiesToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
      propertiesToolStripMenuItem.Text = "Properties";
      propertiesToolStripMenuItem.Click += propertiesToolStripMenuItem_Click;
      // 
      // timer
      // 
      timer.Interval = 400;
      timer.Tick += timer_Tick;
      // 
      // label_check
      // 
      label_check.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
      label_check.AutoSize = true;
      label_check.BackColor = System.Drawing.Color.Firebrick;
      label_check.ForeColor = System.Drawing.SystemColors.ControlLightLight;
      label_check.Location = new System.Drawing.Point(379, 6);
      label_check.Name = "label_check";
      label_check.Size = new System.Drawing.Size(111, 15);
      label_check.TabIndex = 1;
      label_check.Text = "YOU ARE IN CHECK";
      label_check.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // GameForm
      // 
      AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      ClientSize = new System.Drawing.Size(1078, 856);
      Controls.Add(splitContainer1);
      Controls.Add(menuStrip1);
      MainMenuStrip = menuStrip1;
      Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      Name = "GameForm";
      Text = "Chess";
      FormClosing += GameForm_FormClosing;
      Load += GameForm_Load;
      splitContainer1.Panel1.ResumeLayout(false);
      splitContainer1.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
      splitContainer1.ResumeLayout(false);
      splitContainer2.Panel1.ResumeLayout(false);
      splitContainer2.Panel2.ResumeLayout(false);
      splitContainer2.Panel2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
      splitContainer2.ResumeLayout(false);
      splitContainer3.Panel1.ResumeLayout(false);
      splitContainer3.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)splitContainer3).EndInit();
      splitContainer3.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)pictFirst).EndInit();
      ((System.ComponentModel.ISupportInitialize)pictPrevious).EndInit();
      ((System.ComponentModel.ISupportInitialize)pictLast).EndInit();
      ((System.ComponentModel.ISupportInitialize)pictNext).EndInit();
      ((System.ComponentModel.ISupportInitialize)pictStop).EndInit();
      panelClocks.ResumeLayout(false);
      evaluationTabControl.ResumeLayout(false);
      tabMaterial.ResumeLayout(false);
      tabEvalHistory.ResumeLayout(false);
      menuStrip1.ResumeLayout(false);
      menuStrip1.PerformLayout();
      menuPieceContext.ResumeLayout(false);
      ResumeLayout(false);
      PerformLayout();
    }

    #endregion

    private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.SplitContainer splitContainer2;
		public BoardControl boardControl;
		private MaterialBalanceControl mbControl;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem menu_File;
		private System.Windows.Forms.ToolStripMenuItem menu_Game;
		private System.Windows.Forms.ToolStripMenuItem menu_Options;
		private System.Windows.Forms.ToolStripMenuItem menu_Help;
		private System.Windows.Forms.ListView listThinking1;
		private System.Windows.Forms.ColumnHeader headerScore;
		private System.Windows.Forms.ColumnHeader headerTime;
		private System.Windows.Forms.ColumnHeader headerNodes;
		private System.Windows.Forms.ColumnHeader headerPV;
		private System.Windows.Forms.SplitContainer splitContainer3;
		private System.Windows.Forms.TabControl evaluationTabControl;
		private System.Windows.Forms.TabPage tabMaterial;
		private System.Windows.Forms.TabPage tabEvalHistory;
		private System.Windows.Forms.ListView listMoves;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ToolStripMenuItem menuitem_QuickAnalysis;
		private System.Windows.Forms.ToolStripMenuItem menuitem_Perft;
		private System.Windows.Forms.ToolStripMenuItem menuitem_Appearance;
		private System.Windows.Forms.ToolStripMenuItem menuitem_SaveGame;
		private System.Windows.Forms.ToolStripMenuItem menuitem_Exit;
		private System.Windows.Forms.ToolStripMenuItem menuitem_TakeBackMove;
		private System.Windows.Forms.ToolStripMenuItem menuitem_TakeBackAllMoves;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem menuitem_RotateBoard;
		private System.Windows.Forms.ToolStripMenuItem menuitem_About;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem menuitem_UncheckeredBoard;
		private System.Windows.Forms.ToolStripMenuItem menuitem_CheckeredBoard;
		private System.Windows.Forms.ToolStripMenuItem menuitem_ThreeColorBoard;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripMenuItem menuitem_ComputerPlays0;
		private System.Windows.Forms.ToolStripMenuItem menuitem_ComputerPlays1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
    private System.Windows.Forms.ToolStripMenuItem menuitem_HighlightComputerMove;
		private System.Windows.Forms.Panel panelClocks;
		private System.Windows.Forms.Label labelTime1;
		private System.Windows.Forms.Label labelTime0;
		private System.Windows.Forms.Label labelPlayer1;
		private System.Windows.Forms.Label labelPlayer0;
		private System.Windows.Forms.ContextMenuStrip menuPieceContext;
		private System.Windows.Forms.ToolStripMenuItem propertiesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem menu_Tools;
		private System.Windows.Forms.ToolStripMenuItem menuitem_ShowEngineDebugWindow;
		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
		private EvaluationHistoryControl ehControl;
		private System.Windows.Forms.ToolStripMenuItem menuitem_EnableCustomTheme;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
		private System.Windows.Forms.ToolStripMenuItem menuitem_LoadPositionByFEN;
		private System.Windows.Forms.ToolStripMenuItem menuitem_StopThinking;
		private System.Windows.Forms.Label lblReviewMode;
		private System.Windows.Forms.PictureBox pictStop;
		private System.Windows.Forms.PictureBox pictNext;
		private System.Windows.Forms.PictureBox pictLast;
		private System.Windows.Forms.PictureBox pictFirst;
		private System.Windows.Forms.PictureBox pictPrevious;
		private System.Windows.Forms.ToolStripMenuItem menuitem_MultiPVAnalysis;
		private System.Windows.Forms.ToolStripMenuItem menuitem_ShowEngineStatisticsWindow;
		private System.Windows.Forms.ToolStripMenuItem menuitem_StaticEvaluation;
    private System.Windows.Forms.Label gems_label;
    public System.Windows.Forms.Label label_check;
  }
}