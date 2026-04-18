
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

using ChessV.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ChessV.GUI
{
  public partial class ExceptionForm : Form
  {
    public Exception Exception { get; private set; }

    public ExceptionForm(Exception exception, Game game, string gameName = null)
    {
      if (exception is System.Reflection.TargetInvocationException &&
        exception.InnerException != null)
        Exception = exception.InnerException;
      else
        Exception = exception;
      //	Generate text for the exception log in case the user saves it
      exceptionLogText = new StringBuilder();
      if (game != null)
      {
        MemoryStream stream = new MemoryStream();
        StreamWriter writer = new StreamWriter(stream);
        game.SaveGame(writer);
        writer.Flush();
        stream.Position = 0;
        StreamReader reader = new StreamReader(stream);
        exceptionLogText.Append(reader.ReadToEnd());
        exceptionLogText.Append(Environment.NewLine);
        exceptionLogText.Append(Environment.NewLine);
      }
      else if (gameName != null)
      {
        exceptionLogText.Append("Game (creation failed): " + gameName);
        exceptionLogText.Append(Environment.NewLine);
        exceptionLogText.Append(Environment.NewLine);
      }
      exceptionLogText.Append(CrashReportFormatter.FormatExceptionChain(exception));
      InitializeComponent();
    }

    private void ExceptionForm_Load(object sender, EventArgs e)
    {
      outerExceptions = new Stack<Exception>();
      if (Exception is GameInitializationException)
      {
        GameInitializationException gameInitException = (GameInitializationException)Exception;
        label1.Text = "An error occured trying to create game:" + Environment.NewLine +
          gameInitException.GameAttribute.GameName;
        if (gameInitException.InnerException != null)
        {
          if (gameInitException.InnerException is FENParseFailureException)
          {
            FENParseFailureException fenException = (FENParseFailureException)gameInitException.InnerException;
            label2.Text = "The FEN component '" + fenException.FENPartName + "' could not be interpreted" +
              Environment.NewLine + "specified value: " + fenException.FENPartValue + Environment.NewLine +
              fenException.Message;
          }
          else
            label2.Text = gameInitException.InnerException.Message;
        }
        else
          label2.Visible = false;
      }
      else
      {
        label1.Text = "Unhandled exception occured of type:" + Environment.NewLine + Exception.GetType().FullName;
        label2.Text = Exception.Message;
      }
      currentException = Exception;
      updateExceptionDetail();
    }

    private void btnInnerException_Click(object sender, EventArgs e)
    {
      outerExceptions.Push(currentException);
      currentException = currentException.InnerException;
      updateExceptionDetail();
    }

    private void btnOuterException_Click(object sender, EventArgs e)
    {
      currentException = outerExceptions.Pop();
      updateExceptionDetail();
    }

    private void txtExceptionDetails_Enter(object sender, EventArgs e)
    {
      txtExceptionDetails.Select(0, 0);
    }

    private void updateExceptionDetail()
    {
      txtExceptionDetails.Text = getExceptionText(currentException);
      btnInnerException.Enabled = currentException.InnerException != null;
      btnOuterException.Enabled = outerExceptions.Count > 0;
    }

    private string getExceptionText(Exception exception)
    {
      return CrashReportFormatter.FormatException(exception);
    }

    private void btnSaveLog_Click(object sender, EventArgs e)
    {
      saveFileDialog.Filter = "Log Files (*.log)|*.log";
      saveFileDialog.Title = "Save the Error Log";
      saveFileDialog.ShowDialog();
      if (saveFileDialog.FileName != null)
      {
        TextWriter writer = new StreamWriter(saveFileDialog.FileName);
        writer.Write(exceptionLogText.ToString());
        writer.Close();
      }
    }

    protected Exception currentException;
    protected Stack<Exception> outerExceptions;
    protected StringBuilder exceptionLogText;
  }
}
