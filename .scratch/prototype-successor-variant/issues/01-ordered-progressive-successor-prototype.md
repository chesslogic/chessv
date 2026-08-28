# Ordered Progressive successor UI prototype

Type: prototype  
Status: resolved

## Question

What could an experimental successor to `Goal = Ordered Progressive` look like if it begins on the 6×8 position `nbrkbn/pppppp/6/6/6/6/PPPPPP/NBRKBN`, while the existing Ordered Progressive goal stays unchanged?

## Assumption

The likely sequence is `6×8 → 8×8 → 10×8 → 10×10 → 12×10 → 12×12`: one new compact prologue followed by the current contract's Ordered Progressive geometries. Any implementation must append a goal ordinal after the existing values and must first confirm generation-side support for 6×8.

## Material sanity check

ChessV's 6×8 piece analysis reports no warnings for the lineup's standard rook, bishops, and knights. Per side, the starting non-pawn material is one 500/550 rook, two 325/350 bishops, and two 325/325 knights.

## Prototype

Open [successor-goal-ui-prototype.html](../successor-goal-ui-prototype.html) directly in a browser. Compare `?variant=A`, `?variant=B`, and `?variant=C`; the fixed switcher and left/right arrow keys also cycle concepts.

## Implemented outcome

The client now uses the explicit experimental name `OrderedProgressive6x8 = 4` while preserving goal ordinals 0–3 and legacy Ordered Progressive behavior.

- Goal parsing, unlock shifting, stage selection, game-name mapping, projection, locations, captures, and sidecar shapes: `APMW.Client/Config.cs`, `ApmwGeometrySelection.cs`, `ItemHandler.cs`, `ActiveRosterProjection.cs`, `LocationHandler.cs`, `CaptureLookup.cs`, `ApmwSidecarSnapshot.cs`, and `ApmwSidecarProtocol.cs`.
- Registered 6×8 ChessV profile and setup: `ChessV.Games/MiscellaneousGames/ApmwProfiles.cs`, `ApmwChess.cs`, and `ApmwSixByEightChess.cs`.
- Cross-boundary regression coverage: `APMW.Test/ApmwGeometrySelectionTests.cs`, `ApmwGeometryAwareProviderIntegrationTests.cs`, `LocationHandlerUnitTests.cs`, and `ChessV.Test/ApmwSixByEightProfileTests.cs`.
- Release-facing experimental caveat and generation requirements: `README_PENDING.md`.

The production profile supports the exact array from this prototype, disables castling on 6×8, and keeps standard promotion. The released generation project still does not expose goal 4; it must provide the compact starting inventory/placement, locations and reachability, the third file unlock, and compatible projector support before this can be advertised as generation-supported.

The HTML remains the throwaway visual comparison artifact and is intentionally retained.
