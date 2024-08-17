using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using ChessV;
using ChessV.Base;
using ChessV.Games;
using ChessV.Games.Pieces.Berolina;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Archipelago.APChessV
{
  public class ArchipelagoClient
  {
    public static ArchipelagoClient _instance;
    public static ArchipelagoClient getInstance()
    {
      if (_instance == null)
      {
        lock (typeof(ArchipelagoClient))
        {
          if (_instance == null)
          {
            _instance = new ArchipelagoClient();
          }
        }
      }
      return _instance;
    }

    private ArchipelagoClient()
    {
      nonSessionMessages = new List<string>();

      StartedEventHandler seHandler = (match) =>
      {
        this.match = match;
        match.Finished += (Match m) => this.UnloadMatch();
      };
      ApmwCore.getInstance().StartedEventHandlers.Add(seHandler);
      // TODO(chesslogic): PlayAsWhite
      ApmwCore.getInstance().GeriProvider = () => 1;
    }

    /** Currently not used - could be used for a book layout */
    public void loadPieces()
    {
      var King = new King("King", "K", 0, 0);
      var Pawn = new Pawn("Pawn", "P", 100, 125);
      var Queen = new Queen("Queen", "Q", 950, 1000);
      var Rook = new Rook("Rook", "R", 500, 550);
      var Bishop = new Bishop("Bishop", "B", 325, 350);
      var Knight = new Knight("Knight", "N", 325, 325);

      // Berolina
      var BerolinaPawn = new BerolinaPawn("Berolina Pawn", "Ƥ/ƥ", 100, 125, preferredImageName: "Ferz");
      // Cwda
      var Archbishop = new Archbishop("Archbishop", "A", 875, 875);
      var WarElephant = new WarElephant("War Elephant", "E", 475, 475);
      var Phoenix = new Phoenix("Phoenix", "X", 315, 315);
      var Cleric = new Cleric("Cleric", "Ċ/ċ", 450, 500);
      var Chancellor = new Chancellor("Chancellor", "C", 950, 950);
      var ShortRook = new ShortRook("Short Rook", "S", 400, 425);
      var Tower = new Tower("Tower", "T", 325, 325);
      var Lion = new Lion("Lion", "I", 500, 500);
      var ChargingRook = new ChargingRook("Charging Rook", "Ṙ/ṙ", 495, 530);
      var NarrowKnight = new NarrowKnight("Lancer", "L", 325, 325);
      var ChargingKnight = new ChargingKnight("Charging Knight", "Ṅ/ṅ", 365, 365);
      var Colonel = new Colonel("Colonel", "K̇/k̇", 950, 950);
      // Eurasian
      var Cannon = new Cannon("Cannon", "O", 400, 275);
      var Vao = new Vao("Vao", "V", 300, 175);

      var starters = (new Dictionary<KeyValuePair<int, int>, PieceType>(), "QRNB");
      ApmwCore.getInstance().PlayerPieceSetProvider = () => starters;
      for (int i = 0; i < 8; i++)
      {
        starters.Item1.Add(new KeyValuePair<int, int>(1, i), Pawn);
      }
      starters.Item1.Add(new KeyValuePair<int, int>(2, 0), Rook);
      starters.Item1.Add(new KeyValuePair<int, int>(2, 1), Knight);
      starters.Item1.Add(new KeyValuePair<int, int>(2, 2), Bishop);
      starters.Item1.Add(new KeyValuePair<int, int>(2, 3), Queen);
      starters.Item1.Add(new KeyValuePair<int, int>(2, 4), King);
      starters.Item1.Add(new KeyValuePair<int, int>(2, 5), Bishop);
      starters.Item1.Add(new KeyValuePair<int, int>(2, 6), Knight);
      starters.Item1.Add(new KeyValuePair<int, int>(2, 7), Rook);
    }

    public delegate void ClientDisconnected(ushort code, string reason, bool wasClean);
    public event ClientDisconnected OnClientDisconnect;

    public delegate void ClientConnected(ArchipelagoSession session);
    public event ClientConnected OnConnect;

    internal LocationHandler LocationHandler { get; private set; }
    internal ItemHandler ItemHandler { get; private set; }

    public ArchipelagoSession Session { get; private set; }
    public List<String> nonSessionMessages { get; private set; }

    private DataPackagePacket dataPackagePacket;
    private ConnectedPacket connectedPacket;
    private LocationInfoPacket locationInfoPacket;
    private ConnectPacket connectPacket;

    private Match match;

    private (string, int) lastServerUrl;
    private string lastSlotName;
    private static Task connectionTask;

    private bool IsInGame
    {
      get
      {
        return match != null;
      }
    }

    public void Connect(string hostName, int port, string slotName, string password = null)
    {
      lock (typeof(ArchipelagoClient))
      {
        if (connectionTask != null && !connectionTask.IsCompleted && !connectionTask.IsFaulted)
        {
          nonSessionMessages.Add("Connection task currently processing");
          return;
        }
        if ((hostName, port) == lastServerUrl && slotName == lastSlotName)
        {
          nonSessionMessages.Add("Reconnect attempt prevented. If you don't successfully connect, try restarting this client");
          return;
        }

        //ChatMessage.SendColored($"Attempting to connect to Archipelago at ${url}.", Color.green);
        Dispose();

        lastServerUrl = (hostName, port);
        lastSlotName = slotName;
        Session = ArchipelagoSessionFactory.CreateSession(hostName, port);
        ArchipelagoSession session = Session;
        //ItemLogic = new ArchipelagoItemLogicController(session);
        //LocationCheckBar = new ArchipelagoLocationCheckProgressBarUI();
        connectionTask = new Task(() =>
        {
          var result = Session.TryConnectAndLogin(
            "ChecksMate",
            slotName,
            itemsHandlingFlags: ItemsHandlingFlags.AllItems,
            new Version(0, 2, 10),
            tags: new string[] { "ChecksMate V" },
            password: password,
            requestSlotData: true);

          if (!result.Successful)
          {
            LoginFailure failureResult = (LoginFailure)result;
            foreach (var errCode in failureResult.ErrorCodes)
            {
              nonSessionMessages.Add("Error code: " + errCode.ToString());
            }
            foreach (var err in failureResult.Errors)
            {
              nonSessionMessages.Add(err);
              //ChatMessage.SendColored(err, Color.red);
              //Log.LogError(err);
            }
            return;
          }
          lastServerUrl = (hostName, port);

          LoginSuccessful successResult = (LoginSuccessful)result;
          nonSessionMessages.Add("Connection successful - interpreting slot data");
          Convenience.getInstance().success(port.ToString(), slotName, hostName);

          //if (connectionTask.IsFaulted || session == null)
          //{
          //  return;
          //}

          LocationHandler = LocationHandler.GetInstance();
          LocationHandler.Initialize(session.Locations, session);

          ItemHandler = new ItemHandler(session.Items);

          var slotData = successResult.SlotData;
          ApmwConfig.getInstance().Instantiate(slotData);
          var isDeathLink = 0 < Convert.ToInt32(slotData.GetValueOrDefault("death_link", 0));
          if (isDeathLink)
          {
            var deathLinkService = session.CreateDeathLinkService();
            new Task(() =>
            {
              deathLinkService.EnableDeathLink();
              deathLinkService.OnDeathLinkReceived += (DeathLink deathLink) =>
              {
                lock (LocationHandler.DeathlinkedMatches)
                {
                  if (LocationHandler.DeathlinkedMatches.Contains(match))
                    return;
                  LocationHandler.DeathlinkedMatches.Add(match);
                }
                string reason = string.Join(" due to ", deathLink.Source, deathLink.Cause);
                nonSessionMessages.Add(string.Join(" ", "DeathLink received:", reason));
                match.Death(reason);
              };
            }).Start();
          }

          //LocationCheckBar.ItemPickupStep = ItemLogic.ItemPickupStep;

          //session.Socket.PacketReceived += Session_PacketReceived;
          session.Socket.SocketClosed += (reason) => Session_SocketClosed(reason, session);

          //OnConnect(session);
        });
        connectionTask.Start();
      }
    }

    public void UnloadMatch()
    {
      if (LocationHandler != null)
      {
        LocationHandler.EndMatch();
      }
    }

    public void Dispose()
    {
      nonSessionMessages.Add("Disconnecting from Archipelago, disposing of evidence");
      if (Session != null && Session.Socket.Connected)
      {
        Session.Socket.DisconnectAsync();
      }
      this.UnloadMatch();
      if (ItemHandler != null)
      {
        ItemHandler.Unhook();
      }

      Session = null;
      lastServerUrl = ("", -1);
    }

    // TODO(chesslogic): warn user to reconnect
    private void Session_SocketClosed(string reason, ArchipelagoSession session)
    {
      if (this.Session != session)
        return;
      Dispose();
      // new ArchipelagoEndMessage().Send(NetworkDestination.Clients);

      nonSessionMessages.Add($"{reason}");
    }
  }
}
