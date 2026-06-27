using Archipelago.APChessV.Generation.Environment;
using Archipelago.APChessV.Generation.Settings;
using Archipelago.APChessV.Generation.Subprocess;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChessV.GUI
{
  public sealed class ApmwGenerationForm : Form
  {
    public ApmwGenerationForm()
    {
      currentSettings = new ChecksMateGenerationSettings();

      InitializeComponent();
      InitializeComboBoxes();
      ApplySettingsToControls(currentSettings);
      AppendStatus("Configure ChecksMate generation settings, then save/load YAML or generate a solo room.");
      AppendStatus("After generation, Launch Server is available when MultiServer.py is detected.");
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
      if (generationCancellation != null)
      {
        MessageBox.Show(
          this,
          "Generation is still running. Cancel it before closing this dialog.",
          "ChecksMate Generation",
          MessageBoxButtons.OK,
          MessageBoxIcon.Information);
        e.Cancel = true;
        return;
      }

      if (IsServerRunning())
      {
        if (MessageBox.Show(
          this,
          "A local Archipelago server launched from this dialog is still running. Stop it before closing?",
          "ChecksMate Generation",
          MessageBoxButtons.OKCancel,
          MessageBoxIcon.Information) != DialogResult.OK)
        {
          e.Cancel = true;
          return;
        }

        try
        {
          StopActiveServerAsync(CancellationToken.None).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
          MessageBox.Show(
            this,
            "The local Archipelago server could not be stopped: " + ex.Message,
            "ChecksMate Generation",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
          e.Cancel = true;
          return;
        }
      }

      base.OnFormClosing(e);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        generationCancellation?.Dispose();
        activeServerLaunch?.ServerProcess?.Dispose();
      }

      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      txtPlayerName = new TextBox();
      txtDescription = new TextBox();
      txtRequiredVersion = new TextBox();
      numProgressionBalancing = new NumericUpDown();
      cmbAccessibility = new ComboBox();
      cmbGoal = new ComboBox();
      cmbPieceLocations = new ComboBox();
      cmbPieceTypes = new ComboBox();
      cmbFairyChessPieces = new ComboBox();
      cmbFairyChessArmy = new ComboBox();
      cmbFairyChessPawns = new ComboBox();
      numPocketLimitByPocket = new NumericUpDown();
      chkDeathLink = new CheckBox();
      txtArchipelagoRoot = new TextBox();
      btnBrowseArchipelagoRoot = new Button();
      txtOutputDirectory = new TextBox();
      btnBrowseOutputDirectory = new Button();
      btnSaveYaml = new Button();
      btnLoadYaml = new Button();
      btnGenerate = new Button();
      btnCancel = new Button();
      btnLaunchSoloServer = new Button();
      btnClose = new Button();
      txtStatus = new TextBox();
      openYamlDialog = new OpenFileDialog();
      saveYamlDialog = new SaveFileDialog();
      folderBrowserDialog = new FolderBrowserDialog();

      ((System.ComponentModel.ISupportInitialize)numProgressionBalancing).BeginInit();
      ((System.ComponentModel.ISupportInitialize)numPocketLimitByPocket).BeginInit();
      SuspendLayout();

      var settingsGroup = new GroupBox();
      settingsGroup.Location = new Point(12, 12);
      settingsGroup.Name = "settingsGroup";
      settingsGroup.Size = new Size(736, 410);
      settingsGroup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      settingsGroup.TabStop = false;
      settingsGroup.Text = "ChecksMate YAML Settings";

      var settingsLayout = new TableLayoutPanel();
      settingsLayout.ColumnCount = 2;
      settingsLayout.RowCount = 13;
      settingsLayout.Location = new Point(12, 24);
      settingsLayout.Name = "settingsLayout";
      settingsLayout.Size = new Size(712, 374);
      settingsLayout.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      settingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190F));
      settingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
      for (int row = 0; row < settingsLayout.RowCount; row++)
      {
        settingsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
      }

      ConfigureTextBox(txtPlayerName, "txtPlayerName");
      ConfigureTextBox(txtDescription, "txtDescription");
      ConfigureTextBox(txtRequiredVersion, "txtRequiredVersion");
      ConfigureComboBox(cmbAccessibility, "cmbAccessibility");
      ConfigureComboBox(cmbGoal, "cmbGoal");
      ConfigureComboBox(cmbPieceLocations, "cmbPieceLocations");
      ConfigureComboBox(cmbPieceTypes, "cmbPieceTypes");
      ConfigureComboBox(cmbFairyChessPieces, "cmbFairyChessPieces");
      ConfigureComboBox(cmbFairyChessArmy, "cmbFairyChessArmy");
      ConfigureComboBox(cmbFairyChessPawns, "cmbFairyChessPawns");

      numProgressionBalancing.Minimum = 0;
      numProgressionBalancing.Maximum = 100;
      numProgressionBalancing.Name = "numProgressionBalancing";
      numProgressionBalancing.Dock = DockStyle.Fill;

      numPocketLimitByPocket.Minimum = 0;
      numPocketLimitByPocket.Maximum = 999;
      numPocketLimitByPocket.Name = "numPocketLimitByPocket";
      numPocketLimitByPocket.Dock = DockStyle.Fill;

      chkDeathLink.Name = "chkDeathLink";
      chkDeathLink.Text = "Enable DeathLink";
      chkDeathLink.Dock = DockStyle.Fill;
      chkDeathLink.UseVisualStyleBackColor = true;

      AddSettingsRow(settingsLayout, 0, "Player name", txtPlayerName);
      AddSettingsRow(settingsLayout, 1, "Description", txtDescription);
      AddSettingsRow(settingsLayout, 2, "Required AP version", txtRequiredVersion);
      AddSettingsRow(settingsLayout, 3, "Progression balancing", numProgressionBalancing);
      AddSettingsRow(settingsLayout, 4, "Accessibility", cmbAccessibility);
      AddSettingsRow(settingsLayout, 5, "Goal", cmbGoal);
      AddSettingsRow(settingsLayout, 6, "Piece locations", cmbPieceLocations);
      AddSettingsRow(settingsLayout, 7, "Piece types", cmbPieceTypes);
      AddSettingsRow(settingsLayout, 8, "Fairy chess pieces", cmbFairyChessPieces);
      AddSettingsRow(settingsLayout, 9, "Fairy chess army", cmbFairyChessArmy);
      AddSettingsRow(settingsLayout, 10, "Fairy chess pawns", cmbFairyChessPawns);
      AddSettingsRow(settingsLayout, 11, "Pocket limit by pocket", numPocketLimitByPocket);
      AddSettingsRow(settingsLayout, 12, "DeathLink", chkDeathLink);
      settingsGroup.Controls.Add(settingsLayout);

      var environmentGroup = new GroupBox();
      environmentGroup.Location = new Point(12, 430);
      environmentGroup.Name = "environmentGroup";
      environmentGroup.Size = new Size(736, 95);
      environmentGroup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      environmentGroup.TabStop = false;
      environmentGroup.Text = "Archipelago Environment";

      var lblRoot = CreateLabel("Archipelago root");
      lblRoot.Location = new Point(12, 25);
      lblRoot.Size = new Size(125, 23);
      txtArchipelagoRoot.Location = new Point(145, 23);
      txtArchipelagoRoot.Name = "txtArchipelagoRoot";
      txtArchipelagoRoot.Size = new Size(480, 23);
      txtArchipelagoRoot.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      btnBrowseArchipelagoRoot.Location = new Point(634, 22);
      btnBrowseArchipelagoRoot.Name = "btnBrowseArchipelagoRoot";
      btnBrowseArchipelagoRoot.Size = new Size(90, 25);
      btnBrowseArchipelagoRoot.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      btnBrowseArchipelagoRoot.Text = "Browse...";
      btnBrowseArchipelagoRoot.UseVisualStyleBackColor = true;
      btnBrowseArchipelagoRoot.Click += btnBrowseArchipelagoRoot_Click;

      var lblOutput = CreateLabel("Output folder");
      lblOutput.Location = new Point(12, 57);
      lblOutput.Size = new Size(125, 23);
      txtOutputDirectory.Location = new Point(145, 55);
      txtOutputDirectory.Name = "txtOutputDirectory";
      txtOutputDirectory.Size = new Size(480, 23);
      txtOutputDirectory.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      btnBrowseOutputDirectory.Location = new Point(634, 54);
      btnBrowseOutputDirectory.Name = "btnBrowseOutputDirectory";
      btnBrowseOutputDirectory.Size = new Size(90, 25);
      btnBrowseOutputDirectory.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      btnBrowseOutputDirectory.Text = "Browse...";
      btnBrowseOutputDirectory.UseVisualStyleBackColor = true;
      btnBrowseOutputDirectory.Click += btnBrowseOutputDirectory_Click;

      environmentGroup.Controls.Add(lblRoot);
      environmentGroup.Controls.Add(txtArchipelagoRoot);
      environmentGroup.Controls.Add(btnBrowseArchipelagoRoot);
      environmentGroup.Controls.Add(lblOutput);
      environmentGroup.Controls.Add(txtOutputDirectory);
      environmentGroup.Controls.Add(btnBrowseOutputDirectory);

      btnSaveYaml.Location = new Point(12, 535);
      btnSaveYaml.Name = "btnSaveYaml";
      btnSaveYaml.Size = new Size(95, 32);
      btnSaveYaml.Text = "Save YAML";
      btnSaveYaml.UseVisualStyleBackColor = true;
      btnSaveYaml.Click += btnSaveYaml_Click;

      btnLoadYaml.Location = new Point(113, 535);
      btnLoadYaml.Name = "btnLoadYaml";
      btnLoadYaml.Size = new Size(95, 32);
      btnLoadYaml.Text = "Load YAML";
      btnLoadYaml.UseVisualStyleBackColor = true;
      btnLoadYaml.Click += btnLoadYaml_Click;

      btnGenerate.Location = new Point(214, 535);
      btnGenerate.Name = "btnGenerate";
      btnGenerate.Size = new Size(145, 32);
      btnGenerate.Text = "Generate Solo Room";
      btnGenerate.UseVisualStyleBackColor = true;
      btnGenerate.Click += btnGenerate_Click;

      btnCancel.Enabled = false;
      btnCancel.Location = new Point(365, 535);
      btnCancel.Name = "btnCancel";
      btnCancel.Size = new Size(80, 32);
      btnCancel.Text = "Cancel";
      btnCancel.UseVisualStyleBackColor = true;
      btnCancel.Click += btnCancel_Click;

      btnLaunchSoloServer.Enabled = false;
      btnLaunchSoloServer.Location = new Point(451, 535);
      btnLaunchSoloServer.Name = "btnLaunchSoloServer";
      btnLaunchSoloServer.Size = new Size(170, 32);
      btnLaunchSoloServer.Text = "Launch Server";
      btnLaunchSoloServer.UseVisualStyleBackColor = true;
      btnLaunchSoloServer.Click += btnLaunchSoloServer_Click;

      btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      btnClose.DialogResult = DialogResult.Cancel;
      btnClose.Location = new Point(658, 535);
      btnClose.Name = "btnClose";
      btnClose.Size = new Size(90, 32);
      btnClose.Text = "Close";
      btnClose.UseVisualStyleBackColor = true;
      btnClose.Click += btnClose_Click;

      var lblStatus = CreateLabel("Status / output");
      lblStatus.Location = new Point(12, 579);
      lblStatus.Size = new Size(200, 20);

      txtStatus.Location = new Point(12, 602);
      txtStatus.Multiline = true;
      txtStatus.Name = "txtStatus";
      txtStatus.ReadOnly = true;
      txtStatus.ScrollBars = ScrollBars.Both;
      txtStatus.Size = new Size(736, 146);
      txtStatus.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      txtStatus.WordWrap = false;

      openYamlDialog.DefaultExt = "yaml";
      openYamlDialog.Filter = "YAML files (*.yaml;*.yml)|*.yaml;*.yml|All files (*.*)|*.*";
      openYamlDialog.Title = "Load ChecksMate YAML";

      saveYamlDialog.AddExtension = true;
      saveYamlDialog.DefaultExt = "yaml";
      saveYamlDialog.Filter = "YAML files (*.yaml;*.yml)|*.yaml;*.yml|All files (*.*)|*.*";
      saveYamlDialog.Title = "Save ChecksMate YAML";

      AcceptButton = btnGenerate;
      AutoScaleDimensions = new SizeF(7F, 15F);
      AutoScaleMode = AutoScaleMode.Font;
      CancelButton = btnClose;
      ClientSize = new Size(760, 760);
      Controls.Add(settingsGroup);
      Controls.Add(environmentGroup);
      Controls.Add(btnSaveYaml);
      Controls.Add(btnLoadYaml);
      Controls.Add(btnGenerate);
      Controls.Add(btnCancel);
      Controls.Add(btnLaunchSoloServer);
      Controls.Add(btnClose);
      Controls.Add(lblStatus);
      Controls.Add(txtStatus);
      MinimumSize = new Size(776, 740);
      Name = "ApmwGenerationForm";
      StartPosition = FormStartPosition.CenterParent;
      Text = "ChecksMate Solo Room Generation";

      ((System.ComponentModel.ISupportInitialize)numProgressionBalancing).EndInit();
      ((System.ComponentModel.ISupportInitialize)numPocketLimitByPocket).EndInit();
      ResumeLayout(false);
      PerformLayout();
    }

    private void InitializeComboBoxes()
    {
      PopulateEnumComboBox<ChecksMateAccessibility>(cmbAccessibility);
      PopulateEnumComboBox<ChecksMateGoal>(cmbGoal);
      PopulateEnumComboBox<ChecksMatePieceLocations>(cmbPieceLocations);
      PopulateEnumComboBox<ChecksMatePieceTypes>(cmbPieceTypes);
      PopulateEnumComboBox<ChecksMateFairyChessPieces>(cmbFairyChessPieces);
      PopulateEnumComboBox<ChecksMateFairyChessArmy>(cmbFairyChessArmy);
      PopulateEnumComboBox<ChecksMateFairyChessPawns>(cmbFairyChessPawns);
    }

    private static void ConfigureTextBox(TextBox textBox, string name)
    {
      textBox.Name = name;
      textBox.Dock = DockStyle.Fill;
    }

    private static void ConfigureComboBox(ComboBox comboBox, string name)
    {
      comboBox.Name = name;
      comboBox.Dock = DockStyle.Fill;
      comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
    }

    private static void AddSettingsRow(
      TableLayoutPanel layout,
      int row,
      string labelText,
      Control control)
    {
      var label = CreateLabel(labelText);
      label.Dock = DockStyle.Fill;
      control.Margin = new Padding(3);
      layout.Controls.Add(label, 0, row);
      layout.Controls.Add(control, 1, row);
    }

    private static Label CreateLabel(string text)
    {
      return new Label
      {
        AutoSize = false,
        Text = text,
        TextAlign = ContentAlignment.MiddleLeft,
      };
    }

    private static void PopulateEnumComboBox<TEnum>(ComboBox comboBox)
      where TEnum : struct
    {
      comboBox.Items.Clear();
      foreach (object value in Enum.GetValues(typeof(TEnum)))
      {
        comboBox.Items.Add(value);
      }
    }

    private void ApplySettingsToControls(ChecksMateGenerationSettings settings)
    {
      txtPlayerName.Text = settings.Name;
      txtDescription.Text = settings.Description;
      txtRequiredVersion.Text = settings.RequiredArchipelagoVersion;
      numProgressionBalancing.Value = ClampToNumericRange(
        settings.ProgressionBalancing,
        numProgressionBalancing);
      cmbAccessibility.SelectedItem = settings.Accessibility;
      cmbGoal.SelectedItem = settings.Goal;
      cmbPieceLocations.SelectedItem = settings.PieceLocations;
      cmbPieceTypes.SelectedItem = settings.PieceTypes;
      cmbFairyChessPieces.SelectedItem = settings.FairyChessPieces;
      cmbFairyChessArmy.SelectedItem = settings.FairyChessArmy;
      cmbFairyChessPawns.SelectedItem = settings.FairyChessPawns;
      numPocketLimitByPocket.Value = ClampToNumericRange(
        settings.PocketLimitByPocket,
        numPocketLimitByPocket);
      chkDeathLink.Checked = settings.DeathLink;
    }

    private static decimal ClampToNumericRange(int value, NumericUpDown numericUpDown)
    {
      return Math.Min(
        numericUpDown.Maximum,
        Math.Max(numericUpDown.Minimum, value));
    }

    private bool TryBuildSettingsFromControls(out ChecksMateGenerationSettings settings)
    {
      settings = currentSettings ?? new ChecksMateGenerationSettings();
      settings.Name = txtPlayerName.Text.Trim();
      settings.Game = ChecksMateGenerationSettings.DefaultGame;
      settings.Description = txtDescription.Text.Trim();
      settings.RequiredArchipelagoVersion = txtRequiredVersion.Text.Trim();
      settings.ProgressionBalancing = decimal.ToInt32(numProgressionBalancing.Value);
      settings.Accessibility = GetSelectedEnum(cmbAccessibility, settings.Accessibility);
      settings.Goal = GetSelectedEnum(cmbGoal, settings.Goal);
      settings.PieceLocations = GetSelectedEnum(cmbPieceLocations, settings.PieceLocations);
      settings.PieceTypes = GetSelectedEnum(cmbPieceTypes, settings.PieceTypes);
      settings.FairyChessPieces = GetSelectedEnum(cmbFairyChessPieces, settings.FairyChessPieces);
      settings.FairyChessArmy = GetSelectedEnum(cmbFairyChessArmy, settings.FairyChessArmy);
      settings.FairyChessPawns = GetSelectedEnum(cmbFairyChessPawns, settings.FairyChessPawns);
      settings.PocketLimitByPocket = decimal.ToInt32(numPocketLimitByPocket.Value);
      settings.DeathLink = chkDeathLink.Checked;

      IReadOnlyList<ChecksMateGenerationValidationError> errors = settings.Validate();
      if (errors.Count > 0)
      {
        AppendStatus("Settings validation failed:");
        AppendStatusLines(errors.Select(error => "  " + error));
        MessageBox.Show(
          this,
          "Fix the highlighted settings before saving or generating." + Environment.NewLine +
          string.Join(Environment.NewLine, errors.Select(error => error.ToString())),
          "Invalid ChecksMate Settings",
          MessageBoxButtons.OK,
          MessageBoxIcon.Warning);
        return false;
      }

      currentSettings = settings;
      return true;
    }

    private static TEnum GetSelectedEnum<TEnum>(ComboBox comboBox, TEnum fallback)
      where TEnum : struct
    {
      return comboBox.SelectedItem is TEnum selected
        ? selected
        : fallback;
    }

    private void btnSaveYaml_Click(object sender, EventArgs e)
    {
      if (!TryBuildSettingsFromControls(out ChecksMateGenerationSettings settings))
      {
        return;
      }

      saveYamlDialog.FileName = GetPlayerFileName(settings);
      if (saveYamlDialog.ShowDialog(this) != DialogResult.OK ||
          string.IsNullOrWhiteSpace(saveYamlDialog.FileName))
      {
        return;
      }

      try
      {
        ChecksMateGenerationSettingsYamlSerializer.Save(saveYamlDialog.FileName, settings);
        AppendStatus("Saved YAML: " + saveYamlDialog.FileName);
      }
      catch (Exception ex) when (IsUserVisibleFileException(ex) || ex is InvalidOperationException)
      {
        AppendStatus("Save YAML failed: " + ex.Message);
        MessageBox.Show(
          this,
          ex.Message,
          "Save YAML Failed",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error);
      }
    }

    private void btnLoadYaml_Click(object sender, EventArgs e)
    {
      if (openYamlDialog.ShowDialog(this) != DialogResult.OK ||
          string.IsNullOrWhiteSpace(openYamlDialog.FileName))
      {
        return;
      }

      try
      {
        ChecksMateGenerationSettings loadedSettings =
          ChecksMateGenerationSettingsYamlSerializer.Load(openYamlDialog.FileName);
        currentSettings = loadedSettings;
        ApplySettingsToControls(currentSettings);
        AppendStatus("Loaded YAML: " + openYamlDialog.FileName);
      }
      catch (Exception ex) when (
        IsUserVisibleFileException(ex) ||
        ex is ChecksMateGenerationSettingsYamlException ||
        ex is InvalidOperationException)
      {
        AppendStatus("Load YAML failed: " + ex.Message);
        MessageBox.Show(
          this,
          ex.Message,
          "Load YAML Failed",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error);
      }
    }

    private async void btnGenerate_Click(object sender, EventArgs e)
    {
      if (!TryBuildSettingsFromControls(out ChecksMateGenerationSettings settings))
      {
        return;
      }

      string outputDirectoryPath = GetOutputDirectoryOrPrompt();
      if (outputDirectoryPath == null)
      {
        return;
      }

      generationCancellation = new CancellationTokenSource();
      SetGenerationInProgress(true);
      lastEnvironment = null;
      lastGenerationResult = null;
      UpdateLaunchServerButton();

      try
      {
        string configuredRootPath = string.IsNullOrWhiteSpace(txtArchipelagoRoot.Text)
          ? null
          : txtArchipelagoRoot.Text.Trim();
        AppendStatus("Detecting Archipelago environment...");

        var detector = new ArchipelagoEnvironmentDetector();
        ArchipelagoEnvironmentInfo environment = await Task.Run(
          () => detector.Detect(configuredRootPath),
          generationCancellation.Token);
        AppendEnvironmentStatus(environment);

        if (!environment.IsValidForGeneration)
        {
          AppendStatus("Generation skipped because no valid Archipelago environment was found.");
          MessageBox.Show(
            this,
            "No valid Archipelago environment was found. Set the Archipelago root path and try again.",
            "Archipelago Environment Not Found",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
          return;
        }

        txtArchipelagoRoot.Text = environment.RootPath ?? txtArchipelagoRoot.Text;
        txtOutputDirectory.Text = outputDirectoryPath;

        var request = new ArchipelagoGenerationRequest(
          settings,
          environment,
          Path.Combine(Path.GetTempPath(), "ChessV", "ArchipelagoGeneration"),
          outputDirectoryPath)
        {
          PlayerFileName = GetPlayerFileName(settings),
        };

        AppendStatus("Generating solo room into: " + outputDirectoryPath);
        var service = new ArchipelagoGenerationService();
        ArchipelagoGenerationResult result = await service.GenerateAsync(
          request,
          generationCancellation.Token);
        AppendGenerationResult(result, environment);

        if (result.Success)
        {
          MessageBox.Show(
            this,
            GetGenerationSuccessMessage(result, environment),
            "ChecksMate Generation Complete",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        }
        else if (!result.WasCancelled)
        {
          MessageBox.Show(
            this,
            result.FailureMessage ?? "Archipelago generation failed.",
            "ChecksMate Generation Failed",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
        }
      }
      catch (OperationCanceledException)
      {
        AppendStatus("Generation was cancelled.");
      }
      catch (Exception ex)
      {
        AppendStatus("Generation failed unexpectedly: " + ex);
        MessageBox.Show(
          this,
          ex.Message,
          "ChecksMate Generation Failed",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error);
      }
      finally
      {
        generationCancellation.Dispose();
        generationCancellation = null;
        SetGenerationInProgress(false);
      }
    }

    private void AppendEnvironmentStatus(ArchipelagoEnvironmentInfo environment)
    {
      if (environment == null)
      {
        AppendStatus("Environment detection returned no result.");
        return;
      }

      if (environment.IsValidForGeneration)
      {
        AppendStatus("Using Archipelago root: " + environment.RootPath);
        AppendStatus("Generate.py: " + environment.GenerateScriptPath);
        AppendStatus("ChecksMate world: " + environment.ChecksMateWorldPath);
      }

      foreach (ArchipelagoEnvironmentValidationMessage message in environment.ValidationMessages)
      {
        if (!environment.IsValidForGeneration ||
            message.Severity == ArchipelagoEnvironmentValidationSeverity.Warning)
        {
          AppendStatus(message.ToString());
        }
      }

      if (environment.HasOptionalSoloServer)
      {
        AppendStatus("MultiServer.py was detected; local server launch will be available after generation creates a .archipelago artifact.");
      }
      else
      {
        AppendStatus("MultiServer.py was not detected; YAML saving and generation can still work, but local server launch is unavailable.");
      }
    }

    private void AppendGenerationResult(
      ArchipelagoGenerationResult result,
      ArchipelagoEnvironmentInfo environment)
    {
      AppendStatus(result.Success
        ? "Archipelago generation completed successfully."
        : result.FailureMessage ?? "Archipelago generation failed.");

      AppendStatusLines(result.LogLines);

      var roomArtifacts = result.ArtifactPaths
        .Where(path => string.Equals(Path.GetExtension(path), ".archipelago", StringComparison.OrdinalIgnoreCase))
        .ToList();
      if (roomArtifacts.Count > 0)
      {
        AppendStatus("Generated .archipelago artifacts:");
        AppendStatusLines(roomArtifacts.Select(path => "  " + path));
      }
      else if (result.Success)
      {
        AppendStatus("Generation succeeded, but no .archipelago artifact was found in the output folder.");
      }

      if (result.Success && environment != null && environment.HasOptionalSoloServer)
      {
        if (!string.IsNullOrWhiteSpace(result.PrimaryArchipelagoArtifactPath))
        {
          lastGenerationResult = result;
          lastEnvironment = environment;
          AppendStatus("Launch Server is available for: " + result.PrimaryArchipelagoArtifactPath);
        }
        else
        {
          AppendStatus("MultiServer.py was detected, but server launch needs a generated .archipelago artifact.");
        }
      }

      UpdateLaunchServerButton();
    }

    private static string GetGenerationSuccessMessage(
      ArchipelagoGenerationResult result,
      ArchipelagoEnvironmentInfo environment)
    {
      if (environment != null &&
          environment.HasOptionalSoloServer &&
          !string.IsNullOrWhiteSpace(result.PrimaryArchipelagoArtifactPath))
      {
        return "Generation completed. Use Launch Server to host the generated .archipelago file locally.";
      }

      return "Generation completed. Use the generated .archipelago file with Archipelago.";
    }

    private string GetOutputDirectoryOrPrompt()
    {
      if (!string.IsNullOrWhiteSpace(txtOutputDirectory.Text))
      {
        return txtOutputDirectory.Text.Trim();
      }

      folderBrowserDialog.Description = "Select where the generated .archipelago artifact should be saved.";
      if (folderBrowserDialog.ShowDialog(this) != DialogResult.OK ||
          string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
      {
        return null;
      }

      txtOutputDirectory.Text = folderBrowserDialog.SelectedPath;
      return folderBrowserDialog.SelectedPath;
    }

    private void btnBrowseArchipelagoRoot_Click(object sender, EventArgs e)
    {
      folderBrowserDialog.Description = "Select the Archipelago install/root folder.";
      folderBrowserDialog.SelectedPath = Directory.Exists(txtArchipelagoRoot.Text)
        ? txtArchipelagoRoot.Text
        : string.Empty;
      if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
      {
        txtArchipelagoRoot.Text = folderBrowserDialog.SelectedPath;
      }
    }

    private void btnBrowseOutputDirectory_Click(object sender, EventArgs e)
    {
      folderBrowserDialog.Description = "Select where generated .archipelago artifacts should be saved.";
      folderBrowserDialog.SelectedPath = Directory.Exists(txtOutputDirectory.Text)
        ? txtOutputDirectory.Text
        : string.Empty;
      if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
      {
        txtOutputDirectory.Text = folderBrowserDialog.SelectedPath;
      }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
      if (generationCancellation == null)
      {
        return;
      }

      btnCancel.Enabled = false;
      generationCancellation.Cancel();
      AppendStatus("Cancellation requested.");
    }

    private async void btnLaunchSoloServer_Click(object sender, EventArgs e)
    {
      if (IsServerRunning())
      {
        try
        {
          await StopActiveServerAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
          AppendStatus("Stop server failed: " + ex.Message);
          MessageBox.Show(
            this,
            ex.Message,
            "Stop Server Failed",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
          UpdateLaunchServerButton();
        }

        return;
      }

      if (lastGenerationResult == null ||
          string.IsNullOrWhiteSpace(lastGenerationResult.PrimaryArchipelagoArtifactPath))
      {
        MessageBox.Show(
          this,
          "Generate a .archipelago artifact before launching a local server.",
          "Launch Server",
          MessageBoxButtons.OK,
          MessageBoxIcon.Information);
        return;
      }

      if (lastEnvironment == null || !lastEnvironment.HasOptionalSoloServer)
      {
        MessageBox.Show(
          this,
          "MultiServer.py was not found in the selected Archipelago environment.",
          "Launch Server",
          MessageBoxButtons.OK,
          MessageBoxIcon.Warning);
        return;
      }

      btnLaunchSoloServer.Enabled = false;
      try
      {
        AppendStatus("Launching local Archipelago server...");
        var service = new ArchipelagoServerService();
        ArchipelagoServerLaunchResult result = await service.LaunchAsync(
          new ArchipelagoServerLaunchRequest(
            lastEnvironment,
            lastGenerationResult.PrimaryArchipelagoArtifactPath));

        AppendServerLaunchResult(result);
        if (result.Success)
        {
          activeServerLaunch = result;
          MessageBox.Show(
            this,
            "Local Archipelago server launched at " + result.ConnectionAddress + "." +
            Environment.NewLine +
            "Use this address and your YAML slot name in the Archipelago connection form.",
            "Launch Server",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        }
        else
        {
          MessageBox.Show(
            this,
            result.FailureMessage ?? "Archipelago server launch failed.",
            "Launch Server Failed",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
        }
      }
      finally
      {
        UpdateLaunchServerButton();
      }
    }

    private async Task StopActiveServerAsync(CancellationToken cancellationToken)
    {
      if (activeServerLaunch?.ServerProcess == null)
      {
        return;
      }

      btnLaunchSoloServer.Enabled = false;
      AppendStatus("Stopping local Archipelago server...");
      await activeServerLaunch.ServerProcess.StopAsync(cancellationToken);
      activeServerLaunch.ServerProcess.Dispose();
      AppendStatus("Local Archipelago server stopped.");
      activeServerLaunch = null;
      UpdateLaunchServerButton();
    }

    private void AppendServerLaunchResult(ArchipelagoServerLaunchResult result)
    {
      AppendStatus(result.Success
        ? "Local Archipelago server launched at " + result.ConnectionAddress + "."
        : result.FailureMessage ?? "Local Archipelago server launch failed.");
      AppendStatusLines(result.LogLines);
      AppendStatusLines(result.ValidationErrors.Select(error => "validation: " + error));
    }

    private void btnClose_Click(object sender, EventArgs e)
    {
      Close();
    }

    private void SetGenerationInProgress(bool isGenerating)
    {
      btnSaveYaml.Enabled = !isGenerating;
      btnLoadYaml.Enabled = !isGenerating;
      btnGenerate.Enabled = !isGenerating;
      btnBrowseArchipelagoRoot.Enabled = !isGenerating;
      btnBrowseOutputDirectory.Enabled = !isGenerating;
      btnCancel.Enabled = isGenerating;
      UseWaitCursor = isGenerating;
      UpdateLaunchServerButton();
    }

    private void UpdateLaunchServerButton()
    {
      if (btnLaunchSoloServer == null)
      {
        return;
      }

      if (IsServerRunning())
      {
        btnLaunchSoloServer.Text = "Stop Server";
        btnLaunchSoloServer.Enabled = generationCancellation == null;
        return;
      }

      btnLaunchSoloServer.Text = "Launch Server";
      btnLaunchSoloServer.Enabled =
        generationCancellation == null &&
        lastEnvironment != null &&
        lastEnvironment.HasOptionalSoloServer &&
        lastGenerationResult != null &&
        !string.IsNullOrWhiteSpace(lastGenerationResult.PrimaryArchipelagoArtifactPath);
    }

    private bool IsServerRunning()
    {
      return activeServerLaunch?.ServerProcess != null &&
        activeServerLaunch.Status == ArchipelagoServerProcessStatus.Running;
    }

    private void AppendStatus(string message)
    {
      if (txtStatus.TextLength > 0)
      {
        txtStatus.AppendText(Environment.NewLine);
      }

      txtStatus.AppendText(
        "[" + DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture) + "] " + message);
    }

    private void AppendStatusLines(IEnumerable<string> lines)
    {
      foreach (string line in lines ?? Array.Empty<string>())
      {
        AppendStatus(line);
      }
    }

    private static string GetPlayerFileName(ChecksMateGenerationSettings settings)
    {
      string fileName = string.IsNullOrWhiteSpace(settings.Name)
        ? "ChecksMate"
        : settings.Name.Trim();

      foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
      {
        fileName = fileName.Replace(invalidCharacter, '_');
      }

      if (!fileName.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) &&
          !fileName.EndsWith(".yml", StringComparison.OrdinalIgnoreCase))
      {
        fileName += ".yaml";
      }

      return fileName;
    }

    private static bool IsUserVisibleFileException(Exception ex)
    {
      return ex is IOException ||
        ex is UnauthorizedAccessException ||
        ex is SecurityException ||
        ex is ArgumentException ||
        ex is NotSupportedException;
    }

    private ChecksMateGenerationSettings currentSettings;
    private CancellationTokenSource generationCancellation;
    private ArchipelagoEnvironmentInfo lastEnvironment;
    private ArchipelagoGenerationResult lastGenerationResult;
    private ArchipelagoServerLaunchResult activeServerLaunch;
    private TextBox txtPlayerName;
    private TextBox txtDescription;
    private TextBox txtRequiredVersion;
    private NumericUpDown numProgressionBalancing;
    private ComboBox cmbAccessibility;
    private ComboBox cmbGoal;
    private ComboBox cmbPieceLocations;
    private ComboBox cmbPieceTypes;
    private ComboBox cmbFairyChessPieces;
    private ComboBox cmbFairyChessArmy;
    private ComboBox cmbFairyChessPawns;
    private NumericUpDown numPocketLimitByPocket;
    private CheckBox chkDeathLink;
    private TextBox txtArchipelagoRoot;
    private Button btnBrowseArchipelagoRoot;
    private TextBox txtOutputDirectory;
    private Button btnBrowseOutputDirectory;
    private Button btnSaveYaml;
    private Button btnLoadYaml;
    private Button btnGenerate;
    private Button btnCancel;
    private Button btnLaunchSoloServer;
    private Button btnClose;
    private TextBox txtStatus;
    private OpenFileDialog openYamlDialog;
    private SaveFileDialog saveYamlDialog;
    private FolderBrowserDialog folderBrowserDialog;
  }
}
