using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using ChessV;
using ChessV.Base;
using ChessV.Games;
using ChessV.Games.Pieces.Berolina;
using ChessV.Games.Pieces.Apmw;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
        MatchStateChanged?.Invoke(this, EventArgs.Empty);
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
      var BerolinaPawn = new BerolinaPawn("Berolina Pawn", "Ƥ", 100, 125, preferredImageName: "Ferz");
      // Checkers
      var Checkers = new Checkers("Checkers", "Ç", 100, 125, preferredImageName: "CircleLittle");
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
      ApmwCore.getInstance().PlayerPieceSetProvider = (numFiles) => starters;
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
    public event EventHandler GeometryStateChanged;
    public event EventHandler MatchStateChanged;

    internal LocationHandler LocationHandler { get; private set; }
    internal ItemHandler ItemHandler { get; private set; }

    public ArchipelagoSession Session { get; private set; }
    public List<String> nonSessionMessages { get; private set; }

    private DataPackagePacket dataPackagePacket;
    private ConnectedPacket connectedPacket;
    private LocationInfoPacket locationInfoPacket;
    private ConnectPacket connectPacket;

    private Match match;
    private readonly object geometryStateLock = new object();
    private readonly ApmwGeometrySelectionModel geometrySelection =
      new ApmwGeometrySelectionModel();

    private (string, int) lastServerUrl;
    private string lastSlotName;
    private static Task connectionTask;

    public bool IsMatchActive
    {
      get
      {
        return match != null;
      }
    }

    public ApmwGeometrySelectionModel GeometrySelection
    {
      get { return geometrySelection; }
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
            ApmwConstants.TrackerName,
            slotName,
            itemsHandlingFlags: ItemsHandlingFlags.AllItems,
            tags: new string[] { "ChecksMate V", $"Release {ApmwConstants.ClientVersion}" },
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

          //LocationCheckBar.ItemPickupStep = ItemLogic.ItemPickupStep;

          //session.Socket.PacketReceived += Session_PacketReceived;
          session.Socket.SocketClosed += (reason) => Session_SocketClosed(reason, session);

          var slotData = successResult.SlotData;
          
          // Check client version compatibility
          object rawRequiredClientVersion = slotData.GetValueOrDefault("required_chess_client_version", "0.1.0");
          var currentClientVersion = ApmwConstants.ClientVersion;

          if (!TryParseRequiredClientVersion(rawRequiredClientVersion, out Version requiredClientVersion, out string versionError))
          {
            nonSessionMessages.Add(versionError);
            nonSessionMessages.Add("Connection refused because the world supplied an invalid required client version.");
            session.Socket.DisconnectAsync();
            return;
          }

          if (!IsClientVersionCompatible(requiredClientVersion))
          {
            nonSessionMessages.Add($"Client version mismatch: This client is version {currentClientVersion}, but the world requires version {requiredClientVersion} or higher");
            nonSessionMessages.Add("Please update your ChecksMate client (see https://github.com/chesslogic/chessv/releases) or ask the world generator to use an older APMW world version");
            session.Socket.DisconnectAsync();
            return;
          }

          ApmwContractV2 contract = null;
          if (slotData.TryGetValue("apmw_contract", out object rawContract))
          {
            try
            {
              contract = ApmwGeometryResolver.ParseCurrentContract(rawContract);
            }
            catch (ApmwContractException exception)
            {
              nonSessionMessages.Add("Invalid apmw_contract: " + exception.Message);
              nonSessionMessages.Add(
                "Connection refused because the current world contract is malformed or unsupported.");
              session.Socket.DisconnectAsync();
              return;
            }

            Version contractMinimum = new Version(contract.MinimumClientVersion);
            if (!IsClientVersionCompatible(contractMinimum))
            {
              nonSessionMessages.Add(
                $"APMW contract version mismatch: This client is version {currentClientVersion}, " +
                $"but apmw_contract requires version {contractMinimum} or higher");
              nonSessionMessages.Add(
                "Connection refused until this client implements the complete advertised APMW contract.");
              session.Socket.DisconnectAsync();
              return;
            }
          }
           
          ApmwConfig config = ApmwConfig.getInstance();
          config.Instantiate(slotData, contract);
          if (config.Goal == Goal.OrderedProgressive6x8 && contract == null)
          {
            nonSessionMessages.Add(
              "Ordered Progressive (6x8 Start) requires the current APMW geometry contract.");
            nonSessionMessages.Add(
              "Connection refused because this world cannot expose the complete 6x8-to-12x12 stage sequence.");
            session.Socket.DisconnectAsync();
            return;
          }
          ItemHandler = new ItemHandler(session.Items);
          ItemHandler.ReceivedItemsChanged += ItemHandler_ReceivedItemsChanged;
          RefreshGeometrySelection(true);
          var isDeathLink = 0 < Convert.ToInt32(slotData.GetValueOrDefault("death_link", 0));
          if (isDeathLink)
          {
            var deathLinkService = session.CreateDeathLinkService();
            deathLinkService.EnableDeathLink();
            deathLinkService.OnDeathLinkReceived += (DeathLink deathLink) =>
            {
              Match activeMatch = match;
              if (activeMatch == null)
                return;
              lock (LocationHandler.DeathlinkedMatches)
              {
                if (LocationHandler.DeathlinkedMatches.Contains(activeMatch))
                  return;
                LocationHandler.DeathlinkedMatches.Add(activeMatch);
              }
              string reason = string.Join(" due to ", deathLink.Source, deathLink.Cause);
              nonSessionMessages.Add(string.Join(" ", "DeathLink received:", reason));
              activeMatch.Death(reason);
            };
          }

          OnConnect?.Invoke(session);
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
      if (match != null)
      {
        match = null;
        MatchStateChanged?.Invoke(this, EventArgs.Empty);
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
        ItemHandler.ReceivedItemsChanged -= ItemHandler_ReceivedItemsChanged;
        ItemHandler.Unhook();
        ItemHandler = null;
      }

      Session = null;
      lastServerUrl = ("", -1);
      ApmwConfig.getInstance().ResetConnectionState();
      lock (geometryStateLock)
      {
        geometrySelection.Reset();
        ApplySelectedGeometryCompatibilityState();
      }
      GeometryStateChanged?.Invoke(this, EventArgs.Empty);
      OnClientDisconnect?.Invoke(0, "Disconnecting from Archipelago, disposing of evidence", true);
    }

    // TODO(chesslogic): warn user to reconnect
    // TODO(chesslogic): Figure out the thread where this exception is thrown:
    // Archipelago.MultiClient.Net.Exceptions.ArchipelagoSocketClosedException
    private void Session_SocketClosed(string reason, ArchipelagoSession session)
    {
      bool isThisSession = this.Session == session;
      string article = isThisSession ? "This" : "A";
      nonSessionMessages.Add($"{article} local session ended" + (reason.Length == 0 ? "" : $": {reason}"));
      if (isThisSession)
      {
        Dispose();
      }

      // new ArchipelagoEndMessage().Send(NetworkDestination.Clients);
    }

    internal static bool TryParseRequiredClientVersion(
      object rawValue,
      out Version requiredVersion,
      out string errorMessage)
    {
      requiredVersion = null;
      string value;
      if (rawValue is string stringValue)
        value = stringValue;
      else if (rawValue is JValue jValue && jValue.Type == JTokenType.String)
        value = (string)jValue.Value;
      else
      {
        errorMessage =
          "Invalid required_chess_client_version: expected a semantic version string in major.minor.patch form.";
        return false;
      }

      string[] parts = value.Split('.');
      if (parts.Length != 3 || parts.Any(part => part.Length == 0 || part.Any(character => !char.IsDigit(character))) ||
        !Version.TryParse(value, out requiredVersion))
      {
        errorMessage =
          $"Invalid required_chess_client_version '{value}': expected major.minor.patch using decimal digits.";
        requiredVersion = null;
        return false;
      }

      errorMessage = null;
      return true;
    }

    internal static bool IsClientVersionCompatible(Version requiredVersion)
    {
      var current = new Version(ApmwConstants.ClientVersion);
      return current >= requiredVersion;
    }

    public bool SelectGeometry(string stageId)
    {
      bool changed;
      lock (geometryStateLock)
      {
        if (IsMatchActive || !geometrySelection.IsConnected)
          return false;
        string previous = geometrySelection.SelectedOption.StageId;
        if (!geometrySelection.Select(stageId))
          return false;
        ApplySelectedGeometryCompatibilityState();
        changed = previous != geometrySelection.SelectedOption.StageId;
      }
      if (changed)
        GeometryStateChanged?.Invoke(this, EventArgs.Empty);
      return true;
    }

    public ApmwGeometryPreview GetGeometryPreview(ApmwGeometryOption option)
    {
      if (option == null)
        throw new ArgumentNullException(nameof(option));
      ItemHandler handler = ItemHandler;
      if (handler == null)
        return null;
      return handler.GetGeometryPreview(option.Files, option.Ranks);
    }

    private void ItemHandler_ReceivedItemsChanged(object sender, EventArgs e)
    {
      RefreshGeometrySelection(false);
    }

    private void RefreshGeometrySelection(bool initialConnection)
    {
      ItemHandler handler = ItemHandler;
      if (handler == null)
        return;

      ApmwConfig config = ApmwConfig.getInstance();
      ApmwGeometryUnlockSnapshot unlocks = handler.GeometryUnlocks;
      IReadOnlyList<ApmwGeometryOption> options = config.CurrentContract == null
        ? ApmwGeometryResolver.ResolveLegacy(unlocks.LegacySuperSizeUnlocked)
        : ApmwGeometryResolver.ResolveCurrent(
          config.CurrentContract,
          unlocks.BoardFileUnlockCount,
          unlocks.BoardRankUnlockCount,
          config.Goal);
      lock (geometryStateLock)
      {
        if (initialConnection)
          geometrySelection.Connect(options);
        else
          geometrySelection.Refresh(options);
        ApplySelectedGeometryCompatibilityState();
      }
      GeometryStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ApplySelectedGeometryCompatibilityState()
    {
      ApmwCore.getInstance().isGrand =
        geometrySelection.IsConnected && geometrySelection.SelectedOption.Files > 8;
    }
  }
}
