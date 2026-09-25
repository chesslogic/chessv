# ADR implementation architectural assessment

Status: Initial probing complete. Settled work is ready for dependency-gated decomposition.
Date: 2026-09-22
Scope: ChessV and its ChecksMate integration

## Result

The accepted design needs coordinated changes across five domains.
It does not require a replacement search engine.
The principal changes are exact setup data, reversible committed progress,
CPU royal rules, guarded reporting, and generator/package agreement.

The existing project directories do not isolate those responsibilities.
`Game.cs`, `ApmwChess.cs`, `GameForm.cs`, and contract consumers join several domains.
They require single write owners before parallel implementation.
The [development specification](spec.md) supplies those owners, interfaces,
dependencies, and acceptance gates.

ADRs 0001-0021 support a bounded implementation design.
ADR 0022 contains accepted capabilities and explicit unresolved algorithms.
Its missing policy blocks allocator implementation and the final production
semantic pin, not every independent module.
This assessment does not select that policy or authorize production changes.

## Evidence and provenance

| Source | Baseline or authority |
| --- | --- |
| ChessV source | `78b604e43f684c8032d1af23955485d1e196954f`, initially clean checkout |
| ChecksMate source | `C:\GitHub\rft50-checksmate`, `0772bac724ff483043b56979f0ef078b9c2d264a`, read-only clean checkout |
| Product decisions | `docs\adr\0001-0022` and their linked decision records |
| Wire, array, and package authority | `.scratch\large-board-enemy-armies\contracts\shared-data-contract-v4.md` |
| Cost authority | `.scratch\large-board-enemy-armies\contracts\world-cost-snapshot-v1.md` |
| Royal acceptance authority | `.scratch\large-board-enemy-armies\contracts\royal-lifecycle-fixtures.md` |
| Reporting authority | `.scratch\large-board-enemy-armies\contracts\apmw-connection-reporting.md` |
| Remaining settings authority | `.scratch\large-board-enemy-armies\issues\24-specify-settings-support-and-retirement.md` |

Four child sessions produced the specialist reports.
The parent inspected the Python producer, restore script, and packaging workflow.
These reports preserve symbol-level evidence and candidate designs:

| Domain | Report | Main result |
| --- | --- | --- |
| Library | [Library probe](assessments/library.md) | One owner must control connection publication, retirement, match journals, and atomic packet admission |
| Handler | [Handler probe](assessments/handler.md) | Pre-move setup is not a valid history commit. Stable starting identity and complete committed deltas are prerequisites |
| Search | [Search probe](assessments/search.md) | Existing rule, make/unmake, and hash seams suffice. CPU royalty needs independent, reversible policy |
| Board | [Board probe](assessments/board.md) | Current two-row profiles cannot encode the accepted three-rank arrays. CPU registration and promotion need separation from human entitlements |
| Integration | [Integration probe](assessments/integration.md) | Producer v3, client v2, metadata mismatch, and absent snapshots require coordinated migration |

The consolidated spec resolves candidate-name and ownership differences between
the reports. Reports are evidence, not additional product authority.
Line references describe the recorded baseline and can move during implementation.

## Architectural assessment

### Library: connection lifetime is not match lifetime

The current singleton couples a replaceable session, mutable configuration,
the active match, and direct reporting helpers.
Disconnect cleanup can unload match state.
This conflicts with offline earned progress and process-memory replay.

The proposed connection owner hides transport tasks, retirement evidence,
helper containment, packet admission, and journals behind a small interface.
The handler gives it immutable earned events.
The UI reads a shared snapshot.
Neither caller decides whether a socket is safe to use.

### Handler: commit facts must precede progress

`LocationHandler.SetupMove` pushes a diff before a committed move succeeds.
A failed move can therefore leave phantom Undo history.
The correction belongs at the authoritative `Game.MakeMove` seam.
Adding more counters to the handler does not fix that ordering.

A committed observation contains every capture and piece transition.
The progress module applies one observation atomically and returns earned events.
Undo restores local state, but never retracts the library journal.
Starting identity remains distinct from current tactical type.

Current history controls also perform committed Undo/replay on the live game.
They are not a separate view-only path.
The interaction work must cover those controls and indirect controller replacements,
not only menu buttons.

### Search: deepen the rule module, retain search

The CPU policy currently depends on human King upgrades.
Existing royal membership also has load and capture weaknesses.
Square-based castling rights do not establish original-actor identity.

One APMW CPU-survival coordinator must own CPU check, extinction, and no-move
classification. It replaces competing CPU authority rather than adding another
rule that can contradict it.
Human behavior remains separately delegated.
The CPU side is immutable setup identity, not the current controller type.

Existing search dispatch already calls reversible rule operations and hashes.
The required changes belong to rules, actor-bound castling, promotion selection,
and committed lifecycle observations.
Core alpha-beta, transposition-table algorithms, and move ordering remain unchanged.

### Board: one data authority, not parallel reconstructions

The Army artifact contains twenty explicit owner-relative formations.
Both consumers use those records.
ChessV reflects ranks once for Black and preserves files.
The artifact includes blanks, starting roles, CPU promotion permissions, and
castling actors/routes.

Human deployment has five ranks on eight-rank boards and six on ten-rank boards.
The added human rank is mixed.
The current `ranks - 3` formula is therefore wrong for the new ten-rank CPU band.
Both C# and Python projection consumers must change together.

### Integration: three different material concepts

CPU catalog material, human projection material, and Location costs have
different owners and meanings.
The new calibration must not replace the human expected-value map.
The generator's conservative logic metrics must not become exact roster totals.

ChecksMate derives exact intrinsic costs from Standard formations.
It freezes effective costs after obtainable counts settle.
Rules and slot serialization use that same object.
ChessV validates and retains it without recalibration.

Package metadata is a separate migration.
The current producer includes `minimum_client_version`, while restoration rejects it.
Protocol 1 remains selected. Semantic contract 4.0 and runtime 0.2.0 have different roles.

## ADR coverage and primary implementation ownership

Package IDs refer to [spec.md](spec.md#4-work-packages).

| ADR | Required outcome | Primary packages | Evidence disposition |
| --- | --- | --- | --- |
| 0001 | One exact Army artifact, ChessV publication, ChecksMate consumption | B1, G1, C1, I1 | Absent as a shared runtime artifact |
| 0002 | Exactly five geometries, no 12x12 entry | B1, C1, P1, G1, U1 | Old live paths and fixtures remain |
| 0003 | CPU 2/1/0 royal policy and original-primary castling | R1, E1, U1, T1 | Current human-dependent rule wiring is insufficient |
| 0004 | Standard-based midgame calibration | G2 | Existing authored references remain inputs, not the target model |
| 0005 | New-contract-only 0.4.0 compatibility | C1, N1, G2, I1 | Producer/client semantic versions disagree |
| 0006 | Stable starting-role Location identity | B1, G1, H1 | File/rank inference must go |
| 0007 | Initial King eligibility and endpoint clear condition | H1, G1, G2 | Width-based counters/endpoints must go |
| 0008 | Geometry-matched costs and complete access alternatives | G2, P1 | Current base/grand minimum and single rule stage are insufficient |
| 0009 | Exact inventory-delta endgame anchors | G2 | Target 12x10 budgets are 11220/11250 |
| 0010 | Endpoint-interpolated capture series | G2 | Requires rational derivation and monotonicity evidence |
| 0011 | Task-specific budgets and True Royal Fork link | G1, G2 | Preserve special predicates, not blanket repricing |
| 0012 | Movement-class accessibility floor | G2 | Bounded 1/2 and slider 1/4 remain explicit |
| 0013 | Released analogue bank and compact transfer | G2 | No invented role reference or rounded intermediate |
| 0014 | Regicide at three quarters of Queen-to-win gap | G2, H1 | Cost estimate and actual King capture remain separate |
| 0015 | Exact pawn curves and Rearguard premium | G2, H1 | Role identity survives movement and promotion |
| 0016 | Frozen world costs shared with client | C1, G2, N1 | Snapshot/publication path is absent |
| 0017 | CPU promotion independent of human permissions | B1, R1, U1 | Current promotion string mixes both players |
| 0018 | Reject user-facing FEN APMW resume | U1 | Internal setup/load fixtures remain valid |
| 0019 | Reliable committed Undo and history | E1, H1, U1, T1 | Existing diff ledger needs commit truth |
| 0020 | Permanent match suppression on controller change | E1, N1, U1 | No current atomic reporting gate |
| 0021 | Same-world/slot earned Location and goal replay | N1, C1, U1 | Requires independent process-memory match records |
| 0022 | Retained Legacy funding/conversion and composition intent | D22, A22, P1, I2 | Partly settled. Algorithm and final semantic publication are blocked |

## Important file dispositions

The specification contains the exclusive write sets.
The reports contain expanded source maps.
These distinctions prevent unrelated or ineffective work:

| Surface | Disposition |
| --- | --- |
| `ChessV.Base\Game.cs`, `ChessV.Base\ApmwCore.cs` | E1 owns committed lifecycle facts |
| `ChessV.Games\MiscellaneousGames\ApmwChess.cs` | U1 alone wires setup, rules, promotions, and identity |
| `APMW.Client\LocationHandler.cs`, `CaptureLookup.cs` | H1 owns semantic progress, not transport |
| `APMW.Client\Client.cs` | N1 owns transport and reporting lifecycle |
| `APMW.Client\Config.cs`, `ApmwContractV2.cs` | C1 owns strict v4 migration and frozen world context |
| `ChessV.GUI\Forms\GameForm.cs`, `ApmwForm.cs` | U1 owns interaction and shared-state presentation |
| `ChessV.Base\Search.cs` | Characterization only unless a fixture proves a missing seam |
| `APMW.Core\ApmwEvents.cs`, root event/manager remnants | No new authority and no unrelated cleanup |
| Shared project files | U1 registers resources and sources. No concurrent project edits |
| `setup.iss` | Historical, inspect-only unless a later release decision adopts it |

## Open questions and exclusions

| ID | Open matter | Resolver | Blocking effect |
| --- | --- | --- | --- |
| D22 | Reservation equation, composition target pairs/membership, fallback, transfer ledger, portable inputs and algorithm identity | Existing settings decision owner, ticket 24 | Blocks A22 and final C/package publication |
| D-SETTINGS | Broader public settings support and retirement matrix | Ticket 24 owner | Blocks a claim of complete 0.4.0 release readiness |
| ENG-SEAMS | Exact DTO declarations and assembly visibility | S0 implementation owner | Blocks parallel consumers until the seam gate passes |
| ENG-ACTOR | Actor-right representation and player-specific promotion adapter | R1 with B1/U1 | Contained engineering work, not a reason to reopen royal policy |
| ENG-RUNTIME | Actual stock-library/runtime admission and retirement behavior | N1 and I2 | Required runtime acceptance, not a transport-fork proposal |

Tracker UI, Regions refactoring, ordinary-game data migration, durable journals,
automatic reconnect, legacy world conversion, and balance tuning remain excluded.
No implementation or release action occurred in this assessment.

## Handoff readiness

The bounded work can proceed after the S0 interface gate and normal execution
authorization. The dependency graph prevents shared-file fanout collisions.
Independent modules can use complete synthetic fixtures without fabricating
production hashes.

The full release cannot proceed while D22 and the settings closure remain open.
The [question network](qn_architecture.md) records this partial closure.
The [fanout manifest](fanout.json) is a portable assignment inventory, not a
claim of compatibility with an uninspected fanout tool.
