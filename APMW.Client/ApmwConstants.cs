namespace Archipelago.APChessV
{
  /// <summary>
  /// Centralized string constants for the APMW Archipelago integration.
  /// Tracker game names, ChessV game-attribute names, client version, and the
  /// progressive-item names sent by the ChecksMate Archipelago world.
  /// </summary>
  internal static class ApmwConstants
  {
    /// <summary>The Archipelago tracker / game name registered with the server.</summary>
    public const string TrackerName = "ChecksMate";

    /// <summary>The ChessV GameAttribute.GameName for the standard 8x8 APMW game.</summary>
    public const string GameNameStandard = "Archipelago Multiworld";

    /// <summary>The ChessV GameAttribute.GameName for the 10x10 grand variant.</summary>
    public const string GameNameGrand = "Archipelago Multiworld Super-Sized";

    /// <summary>Client version string sent during connection handshake.</summary>
    public const string ClientVersion = "0.3.3";

    /// <summary>Legacy slot-data key controlling how bonus material upgrades pawns.</summary>
    public const string SlotKeyFairyChessPawnUpgrades = "fairy_chess_pawn_upgrades";

    /// <summary>Resolved slot-data key with the ordered piece upgrade action preference list.</summary>
    public const string SlotKeyPieceUpgradePreferences = "piece_upgrade_preferences";

    /// <summary>Slot-data key selecting legacy family-specific or fundamental board-material itemization.</summary>
    public const string SlotKeyProgressionItemization = "progression_itemization";

    /// <summary>Slot-data key overriding how much material each Material item grants in fundamental itemization.</summary>
    public const string SlotKeyMaterialItemValue = "material_item_value";

    /// <summary>Slot-data key describing how many castling special-move locations can be locked by Castler items.</summary>
    public const string SlotKeyCastlingLocationCount = "castling_location_count";

    /// <summary>Action names used by piece_upgrade_preferences.</summary>
    public static class PieceUpgradeActions
    {
      public const string NewPawn = "new-pawn";
      public const string MorePawn = "more-pawn";
      public const string BetterPawn = "better-pawn";
      public const string PoolPawnUpgrade = "pool-pawn-upgrade";
      public const string PawnToMinor = "pawn-to-minor";
      public const string MinorToMajor = "minor-to-major";
      public const string MajorToJack = "major-to-jack";
      public const string MinorToJack = "minor-to-jack";
      public const string MajorToQueen = "major-to-queen";
      public const string JackToQueen = "jack-to-queen";
      public const string QueenToAmazon = "queen-to-amazon";
    }

    /// <summary>Names of "Progressive ..." (and a few non-progressive) items received from the Archipelago server.</summary>
    public static class ProgressiveItems
    {
      public const string Pocket = "Progressive Pocket";
      public const string PocketRange = "Progressive Pocket Range";
      public const string PocketGems = "Progressive Pocket Gems";
      public const string PlayAsWhite = "Play as White";
      public const string AIIntelligenceMalus = "Progressive AI Intelligence Malus";
      public const string Pawn = "Progressive Pawn";
      public const string MinorPiece = "Progressive Minor Piece";
      public const string MajorPiece = "Progressive Major Piece";
      public const string Jack = "Progressive Jack";
      public const string MajorToQueen = "Progressive Major To Queen";
      public const string Amazon = "Progressive Amazon";
      public const string PawnForwardness = "Progressive Pawn Forwardness";
      public const string Consul = "Progressive Consul";
      public const string KingPromotion = "Progressive King Promotion";
      public const string SuperSizeMe = "Super-Size Me";
      public const string Chessmen = "Chessmen";
      public const string Material = "Material";
      public const string Castler = "Castler";
    }
  }
}
