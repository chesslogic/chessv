using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ChessV.GUI
{
  public class RecoverableDiagnosticForm : Form
  {
    public static void Install(Form owner)
    {
      RecoverableDiagnostics.Handler = diagnostic => ShowDiagnostic(owner, diagnostic);
    }

    public static RecoverableDiagnosticResponse ShowDiagnostic(Form owner, RecoverableDiagnostic diagnostic)
    {
      if (RecoverableDiagnostics.SuppressDialogsForSession)
        return RecoverableDiagnosticResponse.Continue;
      if (owner != null)
      {
        if (owner.IsDisposed)
          return RecoverableDiagnosticResponse.Continue;
        if (owner.InvokeRequired)
          return (RecoverableDiagnosticResponse)owner.Invoke(
            new Func<Form, RecoverableDiagnostic, RecoverableDiagnosticResponse>(ShowDiagnostic),
            owner,
            diagnostic);
      }

      using (RecoverableDiagnosticForm form = new RecoverableDiagnosticForm(diagnostic))
      {
        if (owner != null && !owner.IsDisposed)
          form.ShowDialog(owner);
        else
          form.ShowDialog();
        if (form.SuppressForSession)
          HideDebugForms();
        return form.SuppressForSession
          ? RecoverableDiagnosticResponse.ContinueAndSuppressDialogs
          : RecoverableDiagnosticResponse.Continue;
      }
    }

    public RecoverableDiagnosticForm(RecoverableDiagnostic diagnostic)
    {
      if (diagnostic == null)
        throw new ArgumentNullException(nameof(diagnostic));
      this.diagnostic = diagnostic;
      InitializeComponent();
    }

    public bool SuppressForSession
    { get { return chkSuppress.Checked; } }

    private void InitializeComponent()
    {
      lblTitle = new Label();
      lblMessage = new Label();
      txtDetails = new TextBox();
      chkSuppress = new CheckBox();
      btnSaveLog = new Button();
      btnContinue = new Button();
      saveFileDialog = new SaveFileDialog();

      SuspendLayout();

      lblTitle.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
      lblTitle.Location = new Point(12, 12);
      lblTitle.Name = "lblTitle";
      lblTitle.Size = new Size(550, 32);
      lblTitle.Text = "Recoverable diagnostic";
      lblTitle.TextAlign = ContentAlignment.MiddleCenter;

      lblMessage.Location = new Point(12, 52);
      lblMessage.Name = "lblMessage";
      lblMessage.Size = new Size(550, 64);
      lblMessage.Text = diagnostic.Message;
      lblMessage.TextAlign = ContentAlignment.MiddleCenter;

      txtDetails.AcceptsReturn = true;
      txtDetails.Location = new Point(12, 124);
      txtDetails.Multiline = true;
      txtDetails.Name = "txtDetails";
      txtDetails.ReadOnly = true;
      txtDetails.ScrollBars = ScrollBars.Both;
      txtDetails.Size = new Size(550, 190);
      txtDetails.TabIndex = 0;
      txtDetails.Text = diagnostic.FormatForLog();
      txtDetails.WordWrap = false;

      chkSuppress.Checked = true;
      chkSuppress.Location = new Point(12, 323);
      chkSuppress.Name = "chkSuppress";
      chkSuppress.Size = new Size(300, 24);
      chkSuppress.TabIndex = 1;
      chkSuppress.Text = "Do not show recoverable diagnostics again this session";
      chkSuppress.UseVisualStyleBackColor = true;

      btnSaveLog.Location = new Point(310, 349);
      btnSaveLog.Name = "btnSaveLog";
      btnSaveLog.Size = new Size(123, 32);
      btnSaveLog.TabIndex = 2;
      btnSaveLog.Text = "&Save Log";
      btnSaveLog.UseVisualStyleBackColor = true;
      btnSaveLog.Click += btnSaveLog_Click;

      btnContinue.DialogResult = DialogResult.OK;
      btnContinue.Location = new Point(439, 349);
      btnContinue.Name = "btnContinue";
      btnContinue.Size = new Size(123, 32);
      btnContinue.TabIndex = 3;
      btnContinue.Text = "&Continue";
      btnContinue.UseVisualStyleBackColor = true;

      AcceptButton = btnContinue;
      AutoScaleDimensions = new SizeF(6F, 13F);
      AutoScaleMode = AutoScaleMode.Font;
      CancelButton = btnContinue;
      ClientSize = new Size(574, 393);
      Controls.Add(lblTitle);
      Controls.Add(lblMessage);
      Controls.Add(txtDetails);
      Controls.Add(chkSuppress);
      Controls.Add(btnSaveLog);
      Controls.Add(btnContinue);
      FormBorderStyle = FormBorderStyle.FixedDialog;
      MaximizeBox = false;
      MinimizeBox = false;
      Name = "RecoverableDiagnosticForm";
      StartPosition = FormStartPosition.CenterParent;
      Text = "ChessV Diagnostic";

      ResumeLayout(false);
      PerformLayout();
    }

    private void btnSaveLog_Click(object sender, EventArgs e)
    {
      saveFileDialog.Filter = "Log Files (*.log)|*.log";
      saveFileDialog.Title = "Save the Diagnostic Log";
      if (saveFileDialog.ShowDialog(this) == DialogResult.OK && saveFileDialog.FileName != null)
      {
        using (TextWriter writer = new StreamWriter(saveFileDialog.FileName))
          writer.Write(diagnostic.FormatForLog());
      }
    }

    private static void HideDebugForms()
    {
      for (int x = Application.OpenForms.Count - 1; x >= 0; x--)
      {
        DebugForm debugForm = Application.OpenForms[x] as DebugForm;
        if (debugForm != null)
          debugForm.Visible = false;
      }
    }

    private readonly RecoverableDiagnostic diagnostic;
    private Label lblTitle;
    private Label lblMessage;
    private TextBox txtDetails;
    private CheckBox chkSuppress;
    private Button btnSaveLog;
    private Button btnContinue;
    private SaveFileDialog saveFileDialog;
  }
}
