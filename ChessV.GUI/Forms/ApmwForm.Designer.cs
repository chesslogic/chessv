
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
  partial class ApmwForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ApmwForm));
      txtApmwOutput = new System.Windows.Forms.TextBox();
      timer = new System.Windows.Forms.Timer(components);
      textBox1 = new System.Windows.Forms.TextBox();
      label1 = new System.Windows.Forms.Label();
      label2 = new System.Windows.Forms.Label();
      button1 = new System.Windows.Forms.Button();
      timer1 = new System.Windows.Forms.Timer(components);
      timer2 = new System.Windows.Forms.Timer(components);
      textBox2 = new System.Windows.Forms.TextBox();
      label3 = new System.Windows.Forms.Label();
      button2 = new System.Windows.Forms.Button();
      textBox3 = new System.Windows.Forms.TextBox();
      label4 = new System.Windows.Forms.Label();
      checkBoxSuper = new System.Windows.Forms.CheckBox();
      comboBoxDifficulty = new System.Windows.Forms.ComboBox();
      labelDivider = new System.Windows.Forms.Label();
      label5 = new System.Windows.Forms.Label();
      SuspendLayout();
      // 
      // txtApmwOutput
      // 
      txtApmwOutput.BackColor = System.Drawing.Color.White;
      txtApmwOutput.Location = new System.Drawing.Point(15, 166);
      txtApmwOutput.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
      txtApmwOutput.MaxLength = 8388352;
      txtApmwOutput.Multiline = true;
      txtApmwOutput.Name = "txtApmwOutput";
      txtApmwOutput.ReadOnly = true;
      txtApmwOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      txtApmwOutput.Size = new System.Drawing.Size(1003, 578);
      txtApmwOutput.TabIndex = 0;
      // 
      // timer
      // 
      timer.Tick += timer_Tick;
      // 
      // textBox1
      // 
      textBox1.Location = new System.Drawing.Point(19, 45);
      textBox1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
      textBox1.Name = "textBox1";
      textBox1.Size = new System.Drawing.Size(218, 31);
      textBox1.TabIndex = 1;
      textBox1.TextChanged += textBox1_TextChanged;
      textBox1.KeyDown += textBox1_KeyDown;
      // 
      // label1
      // 
      label1.AutoSize = true;
      label1.Location = new System.Drawing.Point(19, 15);
      label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      label1.Name = "label1";
      label1.Size = new System.Drawing.Size(160, 25);
      label1.TabIndex = 2;
      label1.Text = "Archipelago Room";
      // 
      // label2
      // 
      label2.AutoSize = true;
      label2.Location = new System.Drawing.Point(250, 15);
      label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      label2.Name = "label2";
      label2.Size = new System.Drawing.Size(183, 25);
      label2.TabIndex = 4;
      label2.Text = "Archipelago User Slot";
      // 
      // button1
      // 
      button1.Location = new System.Drawing.Point(716, 12);
      button1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
      button1.Name = "button1";
      button1.Size = new System.Drawing.Size(121, 72);
      button1.TabIndex = 6;
      button1.Text = "Where's the 'Any' key?";
      button1.UseVisualStyleBackColor = true;
      button1.Click += button1_Click;
      // 
      // timer1
      // 
      timer1.Interval = 300;
      timer1.Tick += timer1_Tick;
      // 
      // timer2
      // 
      timer2.Interval = 410;
      timer2.Tick += timer2_Tick;
      // 
      // textBox2
      // 
      textBox2.Location = new System.Drawing.Point(250, 45);
      textBox2.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
      textBox2.Name = "textBox2";
      textBox2.Size = new System.Drawing.Size(218, 31);
      textBox2.TabIndex = 7;
      textBox2.TextChanged += textBox2_TextChanged;
      textBox2.KeyDown += textBox2_KeyDown;
      // 
      // label3
      // 
      label3.AutoSize = true;
      label3.Font = new System.Drawing.Font("Segoe UI", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      label3.Location = new System.Drawing.Point(15, 86);
      label3.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      label3.Name = "label3";
      label3.Size = new System.Drawing.Size(217, 19);
      label3.TabIndex = 8;
      label3.Text = "to connect, press enter or any key";
      // 
      // button2
      // 
      button2.Enabled = false;
      button2.Image = Properties.Resources.icon_apmw;
      button2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      button2.Location = new System.Drawing.Point(849, 12);
      button2.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
      button2.Name = "button2";
      button2.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
      button2.Size = new System.Drawing.Size(170, 72);
      button2.TabIndex = 9;
      button2.Text = "       Start a game!";
      button2.UseVisualStyleBackColor = true;
      button2.Click += button2_Click;
      // 
      // textBox3
      // 
      textBox3.Location = new System.Drawing.Point(481, 45);
      textBox3.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
      textBox3.Name = "textBox3";
      textBox3.Size = new System.Drawing.Size(218, 31);
      textBox3.TabIndex = 10;
      // 
      // label4
      // 
      label4.AutoSize = true;
      label4.Location = new System.Drawing.Point(481, 15);
      label4.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      label4.Name = "label4";
      label4.Size = new System.Drawing.Size(187, 25);
      label4.TabIndex = 11;
      label4.Text = "Archipelago Password";
      // 
      // checkBoxSuper
      // 
      checkBoxSuper.AutoSize = true;
      checkBoxSuper.Checked = true;
      checkBoxSuper.CheckState = System.Windows.Forms.CheckState.Checked;
      checkBoxSuper.Location = new System.Drawing.Point(145, 125);
      checkBoxSuper.Name = "checkBoxSuper";
      checkBoxSuper.Size = new System.Drawing.Size(92, 29);
      checkBoxSuper.TabIndex = 12;
      checkBoxSuper.Text = "Super?";
      checkBoxSuper.UseVisualStyleBackColor = true;
      // 
      // comboBoxDifficulty
      // 
      comboBoxDifficulty.FormattingEnabled = true;
      comboBoxDifficulty.Items.AddRange(new object[] { "Default Difficulty", "Difficulty -1", "Difficulty -2", "Difficulty -3", "Difficulty -4", "Difficulty -5" });
      comboBoxDifficulty.Location = new System.Drawing.Point(250, 121);
      comboBoxDifficulty.Name = "comboBoxDifficulty";
      comboBoxDifficulty.Size = new System.Drawing.Size(218, 33);
      comboBoxDifficulty.TabIndex = 13;
      comboBoxDifficulty.Text = "Choose Difficulty";
      // 
      // labelDivider
      // 
      labelDivider.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      labelDivider.Location = new System.Drawing.Point(19, 112);
      labelDivider.Name = "labelDivider";
      labelDivider.Size = new System.Drawing.Size(1000, 2);
      labelDivider.TabIndex = 14;
      // 
      // label5
      // 
      label5.AutoSize = true;
      label5.Location = new System.Drawing.Point(15, 126);
      label5.Name = "label5";
      label5.Size = new System.Drawing.Size(82, 25);
      label5.TabIndex = 15;
      label5.Text = "settings?";
      // 
      // ApmwForm
      // 
      AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
      AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      ClientSize = new System.Drawing.Size(1037, 752);
      Controls.Add(label5);
      Controls.Add(labelDivider);
      Controls.Add(comboBoxDifficulty);
      Controls.Add(checkBoxSuper);
      Controls.Add(label4);
      Controls.Add(textBox3);
      Controls.Add(button2);
      Controls.Add(label3);
      Controls.Add(textBox2);
      Controls.Add(button1);
      Controls.Add(label2);
      Controls.Add(label1);
      Controls.Add(textBox1);
      Controls.Add(txtApmwOutput);
      Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
      Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
      Name = "ApmwForm";
      Text = "ApmwForm";
      FormClosing += ApmwForm_FormClosing;
      Load += ApmwForm_Load;
      ResumeLayout(false);
      PerformLayout();
    }

    #endregion

    private System.Windows.Forms.TextBox txtApmwOutput;
    private System.Windows.Forms.Timer timer;
    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Timer timer1;
    private System.Windows.Forms.Timer timer2;
    private System.Windows.Forms.TextBox textBox2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.TextBox textBox3;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.CheckBox checkBoxSuper;
    public System.Windows.Forms.ComboBox comboBoxDifficulty;
    private System.Windows.Forms.Label labelDivider;
    private System.Windows.Forms.Label label5;
  }
}