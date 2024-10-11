
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

using Archipelago.APChessV;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using ChessV.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ChessV.GUI
{
  public partial class ApmwForm : Form
  {
    public ApmwForm(MainForm mainForm)
    {
      linesSeen = 0;

      InitializeComponent();
      archipelagoClient = ArchipelagoClient.getInstance();
      this.mainForm = mainForm;
      thisForm = this;
    }

    public static ApmwForm thisForm;

    private ArchipelagoClient archipelagoClient;
    private Convenience convenience;
    private IMessageLogHelper messageLog;
    private List<LogMessage> pastMessages = new List<LogMessage>();
    private int linesSeen = 0;
    private int nonSessionLinesSeen = 0;
    private readonly MainForm mainForm;
    private MessageLogHelper.MessageReceivedHandler mrHandler;

    private void ApmwForm_Load(object sender, EventArgs e)
    {
      timer.Start();
      convenience = new Convenience();
      textBox1.Text += convenience.getRecentUrl();
      textBox2.Text += convenience.getRecentSlotName();

      checkBoxSuper.Enabled = ApmwCore.getInstance().isGrand;
      if (!checkBoxSuper.Enabled)
      {
        checkBoxSuper.Checked = false;
      }
    }

    private void timer_Tick(object sender, EventArgs e)
    {
      var enableButton = false;
      try
      {
        if (archipelagoClient.Session != null
          && archipelagoClient.Session.ConnectionInfo != null)
        {
          enableButton = archipelagoClient.Session.ConnectionInfo.Slot != -1;
          // TODO(chesslogic): disable while a game is active, enable once it ends
        }
      }
      catch (NullReferenceException ex) { }
      if (button2.Enabled != enableButton)
        button2.Enabled = enableButton;

      StringBuilder append = new StringBuilder(10000);
      if (pastMessages != null)
      {
        for (int x = linesSeen; x < pastMessages.Count; x++)
          append.Append(pastMessages[linesSeen++] + "\r\n");
      }
      for (int x = nonSessionLinesSeen; x < archipelagoClient.nonSessionMessages.Count; x++)
        append.Append(archipelagoClient.nonSessionMessages[nonSessionLinesSeen++] + "\r\n");
      txtApmwOutput.AppendText(append.ToString());
      pastMessages = new List<LogMessage>();
      linesSeen = 0;

      if (!checkBoxSuper.Enabled && ApmwCore.getInstance().isGrand)
      {
        checkBoxSuper.Checked = true;
      }
      checkBoxSuper.Enabled = ApmwCore.getInstance().isGrand;
    }

    private void ApmwForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      Visible = false;
      e.Cancel = true;
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
      timer1.Stop();
      button1_Click(sender, e);
    }

    private void timer2_Tick(object sender, EventArgs e)
    {
      button1.Enabled = true;
      timer2.Stop();
    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {
      //timer1.Stop();
      //timer1.Start();
    }

    private void textBox2_TextChanged(object sender, EventArgs e)
    {
      //timer1.Stop();
      //timer1.Start();
    }

    private void textBox1_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter)
      {
        timer1.Stop();
        button1_Click(sender, e);
      }
    }

    private void textBox2_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter)
      {
        timer1.Stop();
        button1_Click(sender, e);
      }
    }

    private void button1_Click(object sender, EventArgs e)
    {
      // TODO(chesslogic): change to disconnect mode (for now, players just close the program, lol)
      if (button1.Enabled == false)
      {
        return;
      }
      button1.Enabled = false;
      timer2.Stop();
      timer2.Start();

      //linesSeen = 0;
      //nonSessionLinesSeen = 0;
      string host;
      int port;
      try
      {
        string[] interpreting = textBox1.Text.Split('/');
        // we want a port and host, which we assume is the last item with a colon.
        // Maybe some weird private server doesn't use a specific port. What a wild world that would be.
        UriBuilder urlBuilder = new UriBuilder("wss://" + textBox1.Text.Split('/').Last(item => item.Contains(":")));
        host = urlBuilder.Host;
        port = urlBuilder.Port;
      }
      catch (UriFormatException ex)
      {
        archipelagoClient.nonSessionMessages.Add(ex.Message);
        return;
      }
      var slot = textBox2.Text;
      var password = textBox3.Text ?? null;
      //messageLog.OnMessageReceived -= (message) => pastMessages.Add(message);
      archipelagoClient.OnConnect += (session) => button2.Enabled = true;
      archipelagoClient.Connect(host, port, slot, password);
      if (archipelagoClient.Session != null && messageLog != archipelagoClient.Session.MessageLog)
      {
        if (mrHandler != null)
        {
          messageLog.OnMessageReceived -= mrHandler;
        }
        mrHandler = (message) => pastMessages.Add(message);
        messageLog = archipelagoClient.Session.MessageLog;
        messageLog.OnMessageReceived += mrHandler;
      }
    }

    private void button2_Click(object sender, EventArgs e)
    {
      // check if we've received Super-Size Me from the ItemHandler
      ApmwCore core = ApmwCore.getInstance();
      Game game;
      if (core.isGrand && checkBoxSuper.Enabled && checkBoxSuper.Checked)
      {
        game = mainForm.Manager.CreateGame("Archipelago Multiworld Super-Sized", null);
      }
      else
      {
        game = mainForm.Manager.CreateGame("Archipelago Multiworld", null);
      }
      game.StartMatch();
      game.Match.Finished += (match) => { archipelagoClient.UnloadMatch(); };
      mainForm.StartChecksMate(game);
    }
  }
}
