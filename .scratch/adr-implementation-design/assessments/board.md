# Board implementation source probe

**Scope.** Read-only source assessment at `78b604e43f684c8032d1af23955485d1e196954f`. This is a design handoff, not an implementation plan authorization. The parent owns external generator/package integration. The handler owner owns `LocationHandler`/`CaptureLookup` role-map consumption and reporting lineage. The library connection owner owns Config compatibility/transport. Rules own search rules, castling, and evaluation.

## Authority and target boundary

The producer is one immutable `chessv-armies-v1.json` Army artifact, not a new runtime array generator. The exact schema, 20 formation count, ordered five-stage list, role-bearing `StartingUnit`, `royal_units`, explicit `CastleRoute`, and CPU-only promotion metadata are authoritative at:

- `.scratch\large-board-enemy-armies\contracts\shared-data-contract-v4.md:130-337` (Army schema, coordinates, roles, exact-array authority, royal/castling/promotion metadata);
- `shared-data-contract-v4.md:474-588` (five geometries, depths, capacities, contract algorithms); and
- `shared-data-contract-v4.md:900-1031` (ChessV-first publication and package dependency order).

Do not copy arrays from prose into code. The golden array sources are `issues\12-normalize-ten-by-ten-arrays.md:46-114`, `issues\13-normalize-twelve-file-arrays.md:115-205`, and starting roles in `issues\15-define-location-role-identity.md:287-354` (the latter is an authority pointer; it was not redefined in this probe). The royal gameplay golden fixtures are `.scratch\large-board-enemy-armies\contracts\royal-lifecycle-fixtures.md`, especially its check-by-survivor-count, capture/undo, extinction, castling, and registration/isolation sections.

| Geometry | Target CPU/neutral/human bands | CPU inventory (P/non-K non-P/K/total) | Current profile condition |
| --- | --- | --- | --- |
| 6x8 | 2/1/5 | 6/5/1/12 | Existing two-row home+pawn layout can be expanded once into artifact records. |
| 8x8 | 2/1/5 | 8/7/1/16 | Existing two-row layout can be expanded once. |
| 10x8 | 2/1/5 | 10/9/1/20 | Existing two-row layout can be expanded once. |
| 10x10 | 3/1/6 | 12/15/1/28 | Current code uses a two-row CPU layer and seven human rows; replace from the accepted exact array. |
| 12x10 | 3/1/6 | 14/18/2/34 | Current code uses an obsolete two-row, Nightrider-bearing layout and seven human rows; replace from the accepted exact array. |

Reflection is file-preserving: owner-relative `(file, rank)` maps to White `(file, rank)` and Black `(file, boardHeight - 1 - rank)`. No file reversal. Setup must reject duplicate occupancy, non-band units, neutral occupants, wrong row width, and wrong FEN rank count. The 12x12 profile, mapping, selector result, tests, and registration are target removals/rejections; it is not a migration compatibility stage.

## Current source seams and required ownership

| Package / exclusive owner | Must change | Reuse or test-only | Evidence and risk |
| --- | --- | --- | --- |
| **NEW: Army data model + serializer in `ChessV.Games`** | Introduce a small shared `ApmwArmyData` interface: resolve a formation by `(stageId, familyId)`, enumerate role-bearing units, and expose CPU promotion/castling/royal metadata. Add strict artifact validation and production data file. | Keep `ApmwPieceCatalog` as the known type/notation/image/movement registry; extend only registrations genuinely absent from it. | `ApmwPieceCatalog.cs:21-47, 84-137, 214-260` currently owns catalog instances/categories but not formation identity. Its current `Amazons` set must not make Amazon royal or promoteable. Basic Elephant needs a collision-free APMW registration at 250/250, separate from `WarElephant`; `MovementAtoms.cs:57-76` supplies its movement. |
| **`ChessV.Games\MiscellaneousGames\ApmwProfiles.cs`** | Replace mutable string-row authority with a geometry descriptor: five supported stages, deployment depths, explicit artifact lookup, validated owner-relative composition. Remove `TwelveByTwelve`, its `Stages` membership, and old twelve-file generator. | Retain `ApmwGeometryProfile` metadata/FEN coordination only after depths no longer derive as `Ranks - 3`. Retain colorbound castler behavior only as explicit route data. | `:75-78` hard-codes human depth as `Ranks - 3`; `:117-123` does acceptable Standard fallback; `:209-247` can only compose CPU back-rank/pawn/empty/human rows; `:291-358` registers 12x12 and omits 6x8 from `Stages`; `:362-424` contains the current legacy rows. This is a hot conflict file. |
| **`ChessV.Games\MiscellaneousGames\ApmwChess.cs`** | Compose FEN from full artifact units plus projected human rows; register all selected CPU start/promotion types independent of human inventory; install CPU royal policy from resolved formation; bind CPU castling to original unit actors; preserve role IDs through setup/load/moves. | Keep `ConfigureBoardGeometry` and board-with-cards construction (`:163-176`), human projection provider call (`:375-390`), and human Major/Jack castling behavior (`:537-568`) isolated. | Setup currently leaks CPU family data into `PromotionTypes` (`:403-419`) and only inserts family promotions when raw `EnemyArmy` is non-null, violating explicit/absent/unknown Standard parity. `AddPieceTypes` (`:329-365`) filters most registrations through human-loadable types, so it cannot safely register the target CPU deployment. `SetOtherVariables` (`:375-485`) assumes string ranks and no CPU actor identity. This is the primary hot conflict. |
| **`ChessV.Games\Rules\Apmw\ApmwStalemateRule.cs` and a NEW CPU survival rule/adapter** | Implement the 0/1/multiple surviving CPU-King policy from artifact metadata; rebuild membership exactly on load; support committed/speculative undo; emit CPU win for multiple-King/no-move; enforce one-King check avoidance for either survivor. | Reuse `CheckmateRule` attack queries and base rule lifecycle only where its assumptions fit. | `ApmwStalemateRule.cs:21-27` rebuilds from piece types, not original unit roles; `:30-44` allows captured royals based on human mover and can remove the wrong side; `:61-69` cannot correct base multi-royal `0`. `CheckmateRule.cs:87-121` rejects royal capture and returns unhandled `0` for multiple royals. Existing `ApmwChess.cs:179-204` chooses rules from **human** `foundKingPromotions`, not the CPU formation. |
| **`ChessV.Games\Rules\CastlingRule.cs` plus `ApmwChess.cs`** | Bind CPU routes to artifact `king_unit_id`/`castler_unit_id`, original unmoved actors, exact empty/safety squares, and no inheritance. Keep attack checks active in every CPU survival phase. | Reuse existing move registration machinery if it can take explicit actors/routes. | Current default CPU casts from `KingFile` and corners (`ApmwChess.cs:570-599`); profile parity calculation (`ApmwProfiles.cs:140-196`) is a consumer-side reconstruction prohibited by the artifact. 6x8 must install no CPU castling route. |
| **`APMW.Client\Config.cs` and `ItemHandler.cs`** | Library connection owner consumes strict v4 compatibility/bindings and preserves validated Army/contract transport into setup. | Reuse Config's strict parse/setup flow and ItemHandler's provider lifecycle (`ItemHandler.cs:30-72`). Do not move external projector/package logic here. | This connection boundary transports projection state and validated contract data; it must not become the owner of runtime capture lineage. ADR 0022's Legacy pawn-material conversion still lacks a selected equation/new projector input, so defer only its allocator semantics and final contract pin. |
| **`APMW.Client\LocationHandler.cs` and `CaptureLookup.cs`** | Handler owner installs role maps from Army data and tracks original CPU unit identity for capture reporting, royal/castling restoration, and fixture-visible transitions. | Reuse existing move-diff rollback and reporting boundaries. | `LocationHandler.cs:99-164` tracks square-to-original-square only; it needs a formation-unit map. `LocationHandler.cs:761-789` is the fixture pointer for victory boundary. Geometry capacity/band changes are settled and remain in scope; ADR 0022 blocks neither those changes nor their tests. |
| **`APMW.Client\ApmwGeometrySelection.cs` and `ChessV.GUI\Forms\ApmwForm.cs`** | Restrict maps/options to exactly five stages and reject unsupported records/requests at the shared resolver. Ensure selector and launch route through validated current-contract options. | Retain current option model and form launch flow. `ApmwForm.cs:197-218` already launches selected registered name; `:352-397` already renders/locks the selection. | `ApmwGeometrySelection.cs:35-44` maps 12x12, `:47-103` accepts contract stage lists, and `:147-155` only rejects unmapped names. `LocationHandler.IsApmwGame` also admits the 12x12 game. `GameForm.cs:115-116,132,197` uses exact-type checks that omit new 6x8/10x10/12x10 subclasses for UI behavior: change to the APMW base type. |
| **Evaluation / type registration in `ChessV.Games`** | Register basic Elephant as a Minor tactical type and bind CPU starting/promotion types independently of human pools. Confirm Mounted King and ordinary King receive chosen royal/fork recognition without new balance coefficients. | Reuse `ApmwChessGame.AddEvaluations` (`:237-301`) and existing catalog values; no invented Elephant/Amazon tuning. | `ApmwPieceCatalog.cs:84-137` currently has no basic Elephant property/Minor membership. `MountedKing.cs:3-27` is King+Knight; `ChessMissingCompounds.cs:61-77` is Amazon Queen+Knight. Amazon remains non-royal/non-promotion/non-Queen tactical set. |

## Array, family, and registration implications

The current arrays are strings: six `nbrkbn`, eight `rnbqkbnr`, ten `rnabqkbcnr`, and twelve `rjnabqkbcnjr` for Standard, with analogous family strings at `ApmwProfiles.cs:362-424`. They only represent home ranks and accept all widths through `CreateTenFileArmies`/`CreateTwelveFileArmies`; that is inadequate for target 10x10/12x10 three-rank arrays.

The resolved target adds the accepted 10x10 and 12x10 pieces without synthesizing from family strings. For 10x10, the artifact contains the 12-pawn two-rank wedge plus six added non-pawns, yielding 28 units. For 12x10, it contains the 14-pawn wedge, two Lions, basic Elephant pair, Amazon, primary Mounted King, additional ordinary King, and moved family Queen, yielding 34 units; no Champion or Nightrider. Four family IDs and both colors require 40 resolved setup cases (20 owner-relative formations and their reflected boards). The target contract requires 20 serialized formations, not 40 duplicated records.

Promotion is per resolved CPU formation, not `PromotionTypes` concatenation. Use ADR 0017's exact family lists and its geometry split; Standard absent selector and unknown selector must resolve identically to explicit Standard. CPU registration must include setup and promotion types but must not add human pocket, random-pool, or entitlement access.

## Acceptance fixtures and commands

Golden inputs:

- Data integrity/array negative pairs: `shared-data-contract-v4.md:1120-1152`.
- Royal lifecycle and castling actor/load/undo matrix: `royal-lifecycle-fixtures.md` sections **Check obligation by surviving count**, **Capture, transition, and undo**, **Two-King extinction**, **Rollback and load matrix**, and **Castling fixture construction**.
- Existing targeted homes: `ChessV.Test\ApmwGeometryProfileTests.cs`, `ChessV.Test\ApmwSetupTests.cs`, `APMW.Test\ApmwGeometrySelectionTests.cs`, `APMW.Test\ApmwGeometryAwareProviderIntegrationTests.cs`, `APMW.Test\ApmwContractV2Tests.cs`, and `APMW.Test\ActiveRosterProjectionTests.cs`.

Expected commands after implementation:

```powershell
dotnet test ChessV.Test\ChessV.Test.csproj --filter "FullyQualifiedName~Apmw"
dotnet test APMW.Test\APMW.Test.csproj --filter "FullyQualifiedName~ApmwGeometry|FullyQualifiedName~ApmwContract|FullyQualifiedName~ActiveRosterProjection"
```

New test-only files should be explicitly labeled `NEW` and split by boundary: Army schema/formation validation, ChessV setup+FEN reflection, royal/castling lifecycle, and client role-map/reporting. Extend the existing test projects rather than introduce a framework.

## Sequencing and conflicts

1. **Parent:** freeze/validate/publish Army bytes first, then provide exact release descriptor. No real source commit/tag/hash/URL may be invented. ChecksMate vendors the pinned bytes; only after that may external projector/package work proceed (`shared-data-contract-v4.md:900-975`).
2. **Shared DTO/seam tranche:** define the smallest Army formation, unit-role, royal, castling, and promotion interfaces consumed by the following owners.
3. **Board data/profile/catalog/composer package:** artifact model + registry + five-stage geometry gate + FEN composer. This package exclusively owns `ApmwProfiles.cs` and any NEW Army model/data files.
4. **Rules package:** CPU royal/castling/evaluation changes. It exclusively owns the NEW rule(s) and rule tests.
5. **Handler package:** `LocationHandler`/`CaptureLookup` role-map and reporting-lineage integration.
6. **One integrator:** sole owner of `ApmwChess.cs`, `GameForm.cs`, and project registration; connects the DTO, board, rules, handler, Config/ItemHandler, and UI changes without cross-package edits to those hot files.

Hot conflicts are `ApmwChess.cs`, `ChessV.Games.csproj` (embedded data/new source inclusion), `APMW.Client\Config.cs`, and `ChessV.GUI\Forms\GameForm.cs`. Keep changes to those files serial and integration-owned. The high-risk semantic errors are: treating basic Elephant as War Elephant; retaining 12x12 through an old contract; reconstructing castling parity/arrays rather than consuming the artifact; deriving CPU royal policy from human upgrades; and allowing a second King to inherit the primary's route.

**Result for parent:** this assessment is at `.scratch\adr-implementation-design\assessments\board.md`. The only unresolved interface intentionally left blocked is ADR 0022's Legacy pawn-material conversion semantic revision; nothing in this source probe allocates or implements it.
