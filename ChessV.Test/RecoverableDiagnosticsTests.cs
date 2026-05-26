using ChessV;

namespace ChessV.Test
{
  [TestClass]
  public class RecoverableDiagnosticsTests
  {
    [TestCleanup]
    public void ResetRecoverableDiagnostics()
    {
      RecoverableDiagnostics.ResetForTest();
    }

    [TestMethod]
    public void Report_WithNoHandler_LogsAndContinues()
    {
      DebugMessageLog log = new DebugMessageLog();

      RecoverableDiagnosticResponse response = RecoverableDiagnostics.Report(
        new RecoverableDiagnostic(
          "test-source",
          "test message",
          "test details",
          messageLog: log));

      Assert.AreEqual(RecoverableDiagnosticResponse.Continue, response);
      Assert.AreEqual(1, log.MessageCount);
      StringAssert.Contains(log.Messages[0], "test-source");
      StringAssert.Contains(log.Messages[0], "test message");
      StringAssert.Contains(log.Messages[0], "test details");
    }

    [TestMethod]
    public void Report_SuppressResponse_SuppressesLaterDialogsButStillLogs()
    {
      DebugMessageLog log = new DebugMessageLog();
      int handlerCalls = 0;
      RecoverableDiagnostics.Handler = diagnostic =>
      {
        handlerCalls++;
        return RecoverableDiagnosticResponse.ContinueAndSuppressDialogs;
      };

      RecoverableDiagnostics.Report(new RecoverableDiagnostic("source", "first", messageLog: log));
      RecoverableDiagnostics.Report(new RecoverableDiagnostic("source", "second", messageLog: log));

      Assert.IsTrue(RecoverableDiagnostics.SuppressDialogsForSession);
      Assert.AreEqual(1, handlerCalls);
      Assert.AreEqual(2, log.MessageCount);
      StringAssert.Contains(log.Messages[1], "second");
    }
  }
}
