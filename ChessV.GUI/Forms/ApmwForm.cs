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
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Models;

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
      archipelagoClient.OnConnect += ArchipelagoClient_OnConnect;
      archipelagoClient.OnClientDisconnect += ArchipelagoClient_OnClientDisconnect;
      archipelagoClient.GeometryStateChanged += ArchipelagoClient_GeometryStateChanged;
      archipelagoClient.MatchStateChanged += ArchipelagoClient_MatchStateChanged;
      Disposed += ApmwForm_Disposed;
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
    private DeathLinkService deathLinkService;
    private Match currentMatch;
    private LocationHandler locationHandler;
    private bool updatingGeometrySelection;

    private void ApmwForm_Load(object sender, EventArgs e)
    {
      timer.Start();
      convenience = new Convenience();
      textBox1.Text += convenience.getRecentUrl();
      textBox2.Text += convenience.getRecentSlotName();

      checkBoxDeathlink.Enabled = false;
      checkBoxDeathlink.Checked = false;
      RefreshGeometryControls();
      UpdateIgnoreCastlersReceivedCheckbox();
    }

    private void timer_Tick(object sender, EventArgs e)
    {
      UpdateLaunchState();

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

      UpdateIgnoreCastlersReceivedCheckbox();
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
      catch (InvalidOperationException)
      {
        archipelagoClient.nonSessionMessages.Add(
          "Enter an Archipelago room address containing a host and port.");
        return;
      }
      var slot = textBox2.Text;
      var password = textBox3.Text ?? null;

      archipelagoClient.Connect(host, port, slot, password);
      if (archipelagoClient.Session != null && messageLog != archipelagoClient.Session.MessageLog)
      {
        if (mrHandler != null)
        {
          messageLog.OnMessageReceived -= mrHandler;
        }
        mrHandler = (message) => InvokeOnUiThread(() => pastMessages.Add(message));
        messageLog = archipelagoClient.Session.MessageLog;
        messageLog.OnMessageReceived += mrHandler;
      }
    }

    private void button2_Click(object sender, EventArgs e)
    {
      ApmwGeometryOption selectedGeometry =
        comboBoxGeometry.SelectedItem as ApmwGeometryOption ??
        archipelagoClient.GeometrySelection.SelectedOption;
      if (selectedGeometry == null || archipelagoClient.IsMatchActive)
        return;

      Dictionary<string, string> options = null;
      if (comboBoxEnemyArmy.SelectedItem != null)
      {
        options = new Dictionary<string, string>();
        options["enemy_army"] = comboBoxEnemyArmy.SelectedItem.ToString();
        
        // TODO(chesslogic): Apparently if I pass the player army here it'll be stored in save files?
        // This would mean calling out to ItemHandler from external to the ApmwChess instance.
      }
      Game game = mainForm.Manager.CreateGame(selectedGeometry.RegisteredGameName, options);
      game.StartMatch();
      currentMatch = game.Match;
      game.Match.Finished += (match) => {
        archipelagoClient.UnloadMatch();
        currentMatch = null;
      };
      mainForm.StartChecksMate(game);
    }

    private void checkBoxDeathlink_CheckedChanged(object sender, EventArgs e)
    {
      if (deathLinkService != null)
      {
        if (checkBoxDeathlink.Checked)
        {
          deathLinkService.EnableDeathLink();
        }
        else
        {
          deathLinkService.DisableDeathLink();
        }
      }
    }

    private void checkBoxIgnoreCastlersReceived_CheckedChanged(object sender, EventArgs e)
    {
      ApmwCore.getInstance().IgnoreCastlersReceived =
        checkBoxIgnoreCastlersReceived.Enabled && checkBoxIgnoreCastlersReceived.Checked;
    }

    private void UpdateIgnoreCastlersReceivedCheckbox()
    {
      bool enabled = archipelagoClient.GeometrySelection.IsConnected &&
        ApmwConfig.getInstance().UsesFundamentalProgressionItemization;

      checkBoxIgnoreCastlersReceived.Enabled = enabled;
      if (!enabled)
        checkBoxIgnoreCastlersReceived.Checked = false;
      ApmwCore.getInstance().IgnoreCastlersReceived =
        enabled && checkBoxIgnoreCastlersReceived.Checked;
    }

    private void ArchipelagoClient_OnConnect(
      Archipelago.MultiClient.Net.ArchipelagoSession session)
    {
      archipelagoClient.nonSessionMessages.Add(
        "Thank you for playing today. UI updating based on slot data ...");
      var config = ApmwConfig.getInstance();
      if (config.SlotData == null)
        throw new InvalidOperationException("Slot data was not initialized before OnConnect.");

      bool isDeathLink = 0 < Convert.ToInt32(
        config.SlotData.GetValueOrDefault("death_link", 0));
      InvokeOnUiThread(() =>
      {
        checkBoxDeathlink.Enabled = isDeathLink;
        checkBoxDeathlink.Checked = isDeathLink;
        RefreshGeometryControls();
        UpdateIgnoreCastlersReceivedCheckbox();
      });

      if (!isDeathLink)
        return;

      deathLinkService = session.CreateDeathLinkService();
      locationHandler = LocationHandler.GetInstance();
      if (checkBoxDeathlink.Checked)
        deathLinkService.EnableDeathLink();
      archipelagoClient.nonSessionMessages.Add("DeathLink service initialized");
      deathLinkService.OnDeathLinkReceived += (DeathLink deathLink) =>
      {
        Match matchToKill = null;
        InvokeOnUiThread(() =>
        {
          if (checkBoxDeathlink.Checked)
            matchToKill = currentMatch;
        });
        if (matchToKill == null)
          return;

        lock (locationHandler.DeathlinkedMatches)
        {
          if (locationHandler.DeathlinkedMatches.Contains(matchToKill))
            return;
          locationHandler.DeathlinkedMatches.Add(matchToKill);
        }
        string reason = string.Join(" ", deathLink.Source, deathLink.Cause);
        archipelagoClient.nonSessionMessages.Add(
          string.Join(" ", "DeathLink received:", reason));
        matchToKill.Death(reason);
      };
    }

    private void ArchipelagoClient_OnClientDisconnect(
      ushort code,
      string reason,
      bool wasClean)
    {
      InvokeOnUiThread(() =>
      {
        checkBoxDeathlink.Enabled = false;
        checkBoxDeathlink.Checked = false;
        checkBoxIgnoreCastlersReceived.Enabled = false;
        checkBoxIgnoreCastlersReceived.Checked = false;
        ApmwCore.getInstance().IgnoreCastlersReceived = false;
        RefreshGeometryControls();
      });
      deathLinkService = null;
      locationHandler = null;
    }

    private void ArchipelagoClient_GeometryStateChanged(object sender, EventArgs e)
    {
      InvokeOnUiThread(RefreshGeometryControls);
    }

    private void ArchipelagoClient_MatchStateChanged(object sender, EventArgs e)
    {
      InvokeOnUiThread(() =>
      {
        UpdateLaunchState();
        UpdateIgnoreCastlersReceivedCheckbox();
      });
    }

    private void comboBoxGeometry_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (updatingGeometrySelection ||
          !(comboBoxGeometry.SelectedItem is ApmwGeometryOption option))
      {
        return;
      }

      if (archipelagoClient.SelectGeometry(option.StageId))
        RefreshGeometryPreview(option);
    }

    private void RefreshGeometryControls()
    {
      ApmwGeometrySelectionModel selection = archipelagoClient.GeometrySelection;
      updatingGeometrySelection = true;
      comboBoxGeometry.Items.Clear();
      comboBoxGeometry.Items.AddRange(selection.AvailableOptions.Cast<object>().ToArray());
      comboBoxGeometry.SelectedItem = selection.AvailableOptions.FirstOrDefault(
        option => option.StageId == selection.SelectedOption.StageId);
      updatingGeometrySelection = false;

      comboBoxGeometry.Enabled = selection.IsConnected && !archipelagoClient.IsMatchActive;
      RefreshGeometryPreview(selection.IsConnected ? selection.SelectedOption : null);
      UpdateLaunchState();
    }

    private void RefreshGeometryPreview(ApmwGeometryOption option)
    {
      ApmwGeometryPreview preview = option == null
        ? null
        : archipelagoClient.GetGeometryPreview(option);
      if (preview == null)
      {
        labelGeometryDiagnostics.Text = "Disconnected";
        return;
      }

      var diagnostics = new List<string>
      {
        "Active " + preview.ActiveCount,
        "Reserves " + preview.ReserveCount,
        "Missing " + preview.MissingMaterial,
      };
      if (preview.DormantMaterial != 0)
        diagnostics.Add("Dormant " + preview.DormantMaterial);
      if (preview.UnallocatedMaterial != 0)
        diagnostics.Add("Unallocated " + preview.UnallocatedMaterial);
      if (preview.UnspentForwardness != 0)
        diagnostics.Add("Forwardness " + preview.UnspentForwardness);
      labelGeometryDiagnostics.Text = string.Join(" | ", diagnostics);
    }

    private void UpdateLaunchState()
    {
      bool matchActive = archipelagoClient.IsMatchActive;
      comboBoxGeometry.Enabled =
        archipelagoClient.GeometrySelection.IsConnected && !matchActive;
      button2.Enabled =
        archipelagoClient.GeometrySelection.IsConnected && !matchActive;
    }

    private void InvokeOnUiThread(Action action)
    {
      if (IsDisposed || Disposing || !IsHandleCreated)
        return;
      if (InvokeRequired)
        Invoke((MethodInvoker)(() => action()));
      else
        action();
    }

    private void ApmwForm_Disposed(object sender, EventArgs e)
    {
      archipelagoClient.OnConnect -= ArchipelagoClient_OnConnect;
      archipelagoClient.OnClientDisconnect -= ArchipelagoClient_OnClientDisconnect;
      archipelagoClient.GeometryStateChanged -= ArchipelagoClient_GeometryStateChanged;
      archipelagoClient.MatchStateChanged -= ArchipelagoClient_MatchStateChanged;
      if (messageLog != null && mrHandler != null)
        messageLog.OnMessageReceived -= mrHandler;
    }
  }
}
