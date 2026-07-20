using Archipelago.APChessV;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using ChessV.Games;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ChessV.Test
{
  [TestClass]
  public class ApmwProjectionBackendTests
  {
    private sealed class RecordingBackend : IApmwProjectionBackend
    {
      private readonly CurrentCSharpProjectionBackend inner =
        new CurrentCSharpProjectionBackend();

      public int GenerateCalls { get; private set; }
      public int ProjectCalls { get; private set; }
      public int Invalidations { get; private set; }

      public GeneratedRoster GenerateOwnedRoster()
      {
        GenerateCalls++;
        return inner.GenerateOwnedRoster();
      }

      public ActiveRosterProjection Project(ProjectionGeometry geometry)
      {
        ProjectCalls++;
        return inner.Project(geometry);
      }

      public void Invalidate()
      {
        Invalidations++;
      }
    }

    [TestMethod]
    public void ItemHandler_DelegatesCurrentContractProjectionThroughBackendSeam()
    {
      ApmwFuzzCase fuzzCase = ApmwFuzzCase.DefaultStandard();
      ApmwConfig.getInstance().Instantiate(fuzzCase.BuildSlotData());
      new ApmwChessGame().earlyPopulatePieceTypes();
      var backend = new RecordingBackend();
      var receivedItems = new Mock<IReceivedItemsHelper>();
      receivedItems.SetupGet(helper => helper.AllItemsReceived).Returns(
        new ReadOnlyCollection<ItemInfo>(new List<ItemInfo>()));
      receivedItems
        .Setup(helper => helper.GetItemName(It.IsAny<long>(), It.IsAny<string>()))
        .Returns((string)null);
      var handler = new ItemHandler(receivedItems.Object, backend);
      try
      {
        handler.GenerateOwnedRoster();
        handler.ProjectOwnedRoster(ProjectionGeometry.For(8, 8));

        Assert.AreEqual(1, backend.GenerateCalls);
        Assert.AreEqual(1, backend.ProjectCalls);
        Assert.AreEqual(1, backend.Invalidations);
      }
      finally
      {
        handler.Unhook();
      }
    }
  }
}
