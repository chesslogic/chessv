# ADR implementation architectural assessment

Status: Pre-S0 reconciliation. Engineering gates remain open. No immediate implementation.
Date: 2026-10-01
Scope: ChessV and its ChecksMate integration

Current disposition, 2026-10-25:
This assessment preserves October 1 source evidence, not the current gate statuses.
The [current frontier](engineering-closure.md#current-authority-and-frontier) records the accepted guarantee-first successor, retained funding, and complete CV02 design/reference acceptance.
Allocation/access/integration proof remains open. Older lifecycle holds and recovery/trial mechanisms do not override that disposition.
This update claims no new production-source review or native conformance.

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

ADRs 0001-0022 and ticket 24's closure round settle the listed product policy.
No new general product-policy blocker emerged from the three current-source reviews.
The allocator, metric, literal schema and lifecycle proofs remain open.
The [engineering closure register](engineering-closure.md) assigns those gates.
All design work must close before S0 implementation starts.
Reconciliation does not authorize production changes.

## Evidence and provenance

| Source | Baseline or authority |
| --- | --- |
| Initial ChessV probe | `78b604e43f684c8032d1af23955485d1e196954f`, initially clean checkout. Historical reports retain this baseline |
| Current ChessV readiness review | `ace9362535652a70c022ef959198055bd4a932fb`. Existing uncommitted CONTEXT, ADR 0022 and ticket 24 are authority inputs, not review output |
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

### Current-source digest, 2026-10-01

This digest preserves bounded evidence, not raw review transcripts.
All three reviews reject implementation under the unamended packet.
Missing v4 production code remains expected backlog, not proof of an architecture failure.

| Surface and fixed baseline | Symbol or source anchor | Finding and consuming gate |
| --- | --- | --- |
| Engine, ChessV `ace9362` | `Game.MakeMove:1598-1603`; `MoveList.MakeMove(int):1090-1107`; `MakeMove(MoveInfo):1187-1192`; indexed Unmake:1197; parameterless Unmake:1218 | Late normal false needs exact committed rollback. Early no-match is mutation-free. Partial exception rollback differs. E1 owns conditional MoveList seam, TX |
| Terminal/controller, ChessV `ace9362` | `Match.onResultClaim:611-632`; adjudication:395-465; Death:367-386; `GameForm`:599,707,723,780,799; `Game.MakeMove` MovePlayed/progress/no-moves order | Complete source/dedup/order matrix, not generic victory rewards. E1/U1/H1/N1, TR |
| Cold setup/reset, ChessV `ace9362` | `Game.Initialize`, `postInitialize`, `LoadFEN`, `StartMatch` | Metadata/types/rules precede pieces, bindings precede root moves. Identity-aware cache reset, CS |
| Search/coordinates, ChessV `ace9362` | Existing rule make/unmake/hash seams; `Checkers.cs:114-116`; Board notation and XBoard notation adapters | No search rewrite. R1 owns literal eight-rank stop audit. Separate coordinate fixtures, no Army convention change |
| Client contracts, ChessV `ace9362` | `ApmwGeometryResolver.ParseCurrentContract`; `ApmwConfig.Instantiate`; `ApmwSidecarSnapshot`; presentation adapter; `ItemGeneration` | Effective fields, absent/empty/disabled distinctions, immutable setup and stale-response refusal missing. EP/NM/CS |
| Local/network, ChessV `ace9362` | `Client` and `ApmwForm` create DeathLink services; `Convenience.success/getRecentUrl/getRecentSlotName` stores one entry | One N1 DeathLink owner with generation-checked posted work. U1 owns five successful details, F19 |
| Allocation, ChecksMate `0772bac` | `legacy.generate_legacy/expected_normalized_grant`; `planning.resolve_actions`; `fundamental.simulate_fundamental` | Source credit can collapse to 485. Empty actions can default-enable. Existing upward DAG does not prove recovery termination. AL/NM |
| Metrics, ChecksMate `0772bac` | `WorldLogicProjection._logic_chessmen_floor/_envelope`; `item_utils.py:33-36,58-70`; `placement.project_roster:95-124` | Grant floor and unconditional pockets not proved after conversion. Primary included in old exact material. PM |
| Policy/lifecycle, ChecksMate `0772bac` | `ProjectionInput`; `fill_slot_data`; `create_items:268-279`; `item_pool.py:703-730,845-860` | Requested settings can replace effective policy. Unchanged failed candidate can repeat. Pool obligations need finite recovery and exclusive ledger. EP/AL/LC |
| Core timing, ChecksMate `0772bac` | `Main.py:114-118,148-193,203-221,239,365-370`; `AutoWorld.py:435-450`; `BaseClasses.fulfills_accessibility:651-710` | Initial counts precede links/start transfer. Final core validator occurs after skip_output return. Frozen state and no-output acceptance, LC |
| Package boundary, both fixed baselines | Build/release metadata and ChessV restore fake-executable harness | Protocol 1, runtime 0.2.0, C4 and metadata2 remain distinct. Actual package evidence stays I2/PK |
| Stock decode boundary, installed NuGet6.6.0 source `75d4c5e6a52bb0c8bb1d4bc368652613509c7acb` | `ArchipelagoPacketConverter.ReadJson`; `BaseArchipelagoSocketHelper_system.net.websockets.OnMessageReceived`; public socket interface/factory | Installed converter replaces duplicate probe fields before callbacks. RI scopes raw-parser rejection separately from full decoded-object validation. Not an S0 blocker |

The generator review includes a semantic-only 785-credit counterexample.
Its current normalized output is one Major with grant 485.
That is not an ordinary-world failure rate or a concrete Rook realization.
ADR 0022 fixes source 785, expected 485, concrete debit 500 and remainder 285.
The ledger still needs conservation and exclusive transfer proof.

The historical connection report inspected source commit `063dfa5ad14bafbfe646bd67d54deb1a8fa2f036`.
It does not replace the installed NuGet provenance or prove strict raw ingress.
The installed converter probe is not a full client ingress rejection test.

### Closure-r2 engine evidence

The applied `cab829d...` reconciliation is this revision's documentation baseline.
The engine child characterized unchanged `ace9362` Base/Games source in session artifacts.
Its Roslyn runner targets/runs installed .NET/WindowsDesktop 7.0.20 and performs no restore or repository build.
The source manifest and provenance retain input hashes, commands and environment.

No source/model pass count becomes future native APMW acceptance.

| Input/source anchor | Characterized result | Scope and consuming design |
| --- | --- | --- |
| Game.MakeMove, MoveList exact index, BoardMoveStack transient entry | Board/side/Ply recover, but Move50 history/search arrays and repetition scratch hashes differ | Complete rollback requires more than exact-index unmake, TX |
| Stateful-rule throw and occupied-square drop, debug off/on | Partial rollback and debug throw differ. Entered drop can leave MoveCount changed after board restore | Absolute typed checkpoint with exclusive restore owner, TX |
| Bare repeated LoadFEN and repetition terminal | LoadFEN does not clear occupied board/prior result. Draw can lack MatchFinished | Full internal reset and explicit terminal publisher, CS/TR |
| Initialize/PostInitialize/PositionLoaded/root trace | Metadata precedes rule initialization. Match is null at root generation | Two placement hooks and captured pocket/account inputs, CS |
| Checkers ten-rank stop, two-King pickup range, notation adapters | Literal stop prevents second jump. Both Kings can occur in one delta. G1/G10 and g0/g9 share indexes but different syntax | R1 geometry hypothesis review and separate coordinate fixtures, not survival/reporting proof |
| PieceType.GetMaterialHashKey one type/player/slice | Finite reservation preserves old indexes and rejects out-of-domain counts | Bounded existing per-game reservation candidate, not whole-composition proof |
| Independent lifecycle model | Literal role/counter, source/order, generation, admission and reset cases agree with the stated model | Design-input/model evidence only, not native gameplay/UI/library proof |
| Separate BaseProtocol/ClientAdapter/BridgeProbe sandbox | Ten reduced cases cover registration, Prepare/install failures, exact receipts and delivery-only retry | Public assembly/phase feasibility. Native history is a list, delta one integer, journal a HashSet; no real Game/H1/N1 acceptance |
| Player.Gems and nonempty Black CardDrop source probe | Capture A4/live B9 attaches with10 gems, drops to6 and wrongly permits a second card; correct A gives5/1 | Characterizes live read and cold generation-side defect. Backing-field Player attachment is not Match.Start or candidate account/reset proof |

Raw evidence remains in engine session `2c2f4b74-443d-4663-b88e-7fadbaeecfae`.
The durable digest cites `tx-tr-cs-design.md`, `lifecycle-provenance.json`,
`lifecycle-probe\source-manifest.json`, `results.jsonl` and `lifecycle-model-results.json`.
The closure register owns the candidate contract, not the raw probe transcript.
The royal fixtures own complete state/reset requirements and proposed literal fixture inputs.

The corrected R2 design SHA-256 is `2b44046ff37014dc8bb1749fec2d97e5bed269286e81ee2af48bf20dc8bbda58`.
Its revision3 fixture SHA-256 is `287554c2378210a737c0d403ebfbda7f00dfb5f2931e1c73a154c70826cef64f`.
The former `8772a70c...` design and `8f1de1d2...` provenance remain historical.
The final provenance SHA-256 is `3bf5f07e95a1c323384d6be3e35db280c1d03e2efd208a8b68564b16093ffa09`.
It binds the revised common manifest.

R2 defers exact capture spellings to common policy-schema section4 and C's allocation.capture_fields.
It adds no alias or separate engine-owned field list.
The model/bridge/card/fixture bytes remain unchanged.
The earlier E1/E2/E3 recheck remains design support.

The final source-alignment recheck and combined owner acceptance remain separate.
The independent engine recheck resolves E1/E2/E3 at design threshold.
Its SHA-256 is `dbdac5e9ddeda163d1d94ec5433114070952d805ee4fafdc86a9a626cdb45070`.
The corrections cover the Base-owned preparation bridge, captured Player/CardDrop accounts and preserved live toolbar.

The final retained-history engine review SHA-256 is `29a85a059fef4db94392c4cc840c9f54b1914d3a58a7904f19914bf8baf3bb99`.
Its revised-byte recheck resolves only the capture-spelling/provenance alignment.

TX/TR/CS support designated document integration, not overall S0 or production acceptance.
The 88 model assertions and ten sandbox cases retain their limited evidence domains.
The source-card bench constructs two Rooks specifically for account reads.
It does not prove Pocket4 allocation, full K12 parsing/hashing, or native captured handover.

Complete native equality, measured checkpoint cost, real Match/N1 delivery and account/cache reset remain later implementation acceptance.
Consolidated ownership and full common/generator acceptance remain pending.

### Closure-r2 generator evidence

The initially reviewed generator packet retains clean ChecksMate source at `0772bac724ff483043b56979f0ef078b9c2d264a`.
Its original model/log/results remain in session `75701715-2f4e-4856-8293-9a1b12ee03b7`.
The model SHA-256 is `97c43ba22694d8a51fad1c8755bf02a29afbe16468d73351fac5447fa4128183`.
The result SHA-256 is `ae5068f70ddf44fa141a170b35563db9044d5908db540567e3ab8dd0695f17f9`.

The documentation writer recorded its input fingerprints without a source/model rerun.
The independent generator review requested the bounded G1/G2/G3 revision described in the corrected digest.
The old `03265dbcd34ea66a0c60fa27c8f01ec8d91084c9843ccbfc1befacfc7bb368fa` design is not final input.
G1 requires practical full-domain recurrence/pruning/certificates and useful target/deployment bounds.

G2 requires literal retained-plando choices, RNG/provenance and a bounded isolated-trial driver.
G3 counts direct non-Pawn entitlement units in D, never conversion emissions funded by their surplus.
The final common quality-helper bytes and conformance receipt also need agreement.
The helper hash drift alone demonstrates no funding behavior regression.

| Fixed input/source | Supported evidence | Exclusion and consumer |
| --- | --- | --- |
| `logic_projection.py:422-456`, `planning.py:45-55`, `__init__.py:205-230` | Empty-enabled fallback loses explicit all-disabled policy; serialization resolves requested settings again | EP/NM/A22 use complete effective records and pure frozen serialization. Not implemented |
| `fundamental.py:278-348`, imported upgrade DAG | Upward interfamily paths are finite, but same-family quality and reverse recovery need canonical finite state | AL's graph/witness/proof-limit contract, not a family-rank termination claim |
| F14 arithmetic and two replacement-pass marks | Original785 survives expected485/concrete500; signed deficits and fixed45/explicit overdraw remain separate | Bounded ledger model, not executed client generation |
| Separate paid-quality corpus | Expected Pawn100 differs from funded/concrete150/200. Extra-only refunds retain one slot and cannot retract an ordinary admission witness | Bounded unlocked domain only. Old corpus remains interfamily-only |
| Paid prefix/cost/cohort checks | 420 prefixes, 8590 eleven-quantity comparisons, 8096 concrete comparisons; independent cohort oracle agrees in its declared subset | No full role/castler/Consul/selector/large-domain certificate or native parity |
| Historical common funding/default conformance | 31 funding/cost records, ten atomic transitions, three invalid bindings and exact default12 agree | Producer interface feasibility. R1 refreshed receipt is separate; no production DTO/full response acceptance |
| `Main.py:148-187`, `Fill.py:1075-1130` | Start/link and retained plando inventory change counts before early reachability | LC's optional pre-plando barrier; variable exogenous materialization remains draft |
| Actual `BaseClasses.fulfills_accessibility` and `link_items` | Bounded Full/Minimal/circular/link cases exercise the real core | Route wrapper and immutable restoration are artifact models, not whole-world generation |

The generator's reviewed common paid-quality contract is producer-feasible.
Its exact normalization and ledger records belong to the common owner.
The packet supports a bounded engineering design, not blanket AL/PM/LC closure.
The exclusion set remains explicit in the closure register.

## Architectural assessment

### Closure-r2 common literal and normalization evidence

The corrected common revision is `paid-quality-v1-owned-slot-D-entitlement-retained-pocket-gems-unit-source-rejection`.
Its fourteen-source/151-serialized-artifact manifest SHA-256 is
`e5d52c326eec4d2f43dd2fc03b47ac604317cef0248ac4a60d44c39fd4ba7cb1`.
The policy design SHA-256 is `ddc4e4338e0455d10eb5641aaa9ea50ce70704c7ccd4b6ca3d40106ec7e37b51`.
The C fragment SHA-256 is `1bd6a30f2206292b0ae06411967fd5370d5ccc8abb3864c6a3814048145e1c17`.

The reference helper SHA-256 is `67340eb6993c14b541fecc75c81eba6f93f8426dad9d3b94fca8cd9e6fb23c95`.
The separate quality helper SHA-256 is `1c07bf28e8a5256fefe88e55a225ef763bc2264879177e1ca1803fa54222b103`.
The former `7fd7da3a...` common fixed point remains historical.
R1 rejects Unit Gem sources before the source-maximum lookup with `ContractError(source-key-item)`, not KeyError.

The final common review SHA-256 is `fa7fb5e31a54507d4959299f23b355fb3460e90b52958da67237092735318d8a`.
The reviewer accepted both valid Gems4 responses, then reproduced the exact forbidden Unit-source failures.
All150 generated outputs matched.
The151-artifact manifest also includes the standalone retained-Gems C node.
This is bounded literal evidence, not full-C/native acceptance.

The durable [allocation design JSON](fixtures/allocation-contract-v4.json) composes 21 complete supplied cases and expectation records.
It includes literal normalization, projected/exhausted responses, source785/concrete500 accounting, all-disabled, soft-zero, paid-quality, and A4/B9 captures.
Raw duplicate inputs remain strings. This preserves the deliberate malformed payload.
The wrapper is fixture data, not another runtime schema or policy authority.

Synthetic C/W hashes and runtime context do not prove full A/L/C/S/W admission.
The C amendment remains a fragment, not a complete frozen production C.

| Evidence | Exact demonstrated domain | Exclusion |
| --- | --- | --- |
| Final reference validator | 50 accepted/40 rejected requests, three accepted/13 rejected responses | Supplied structural/binding cases, not all C/role/capacity/metric invariants |
| Gems | Both modes at counts0/4/9/13/2147483647, fixed Pocket4, raw absence0, missing/alias/type/raw-duplicate/stale rejection | Int32-max checks representation, not huge replay or native gem arithmetic |
| Immutable capture | A4/B9 changes input hash/generation, not policy hash/pocket levels. Old A refuses generation2 | Not native setup/account handover |
| Paid quality | 96 queries, 488 states, 1147 inverse edges, ten transactions, twelve negatives, seven complete wire cases | 0..3 unlocked slots and twelve residual budgets. No full Castler/Consul/PM/core acceptance |
| Non-army Unit source | Both Legacy and Fundamental manifest-bound Gem-source negatives reject explicitly before maximum lookup | Literal failure-path correction, not full mixed-source allocation |
| Reservation origin | Seven original-token/provenance cases distinguish D from money | Not a practical coupled full-domain allocator |
| Settings | All 73 existing rows plus effective-policy row covered | Literal design/fixtures precede native consumer binding |
| Historic completion | Handwritten seven-protected trace and bounded 53527-node/42050-terminal enumeration | Not a reproduced production failure or general completion oracle |

ChessV anchors include `ApmwConstants.cs:69`, `ItemHandler.cs:98,144,202-220`, and `Player.cs:74-76`.
Progressive Pocket Gems is distinct from Progressive Pocket.
Producer-reported anchors are `items.py:88`, `item_allowed_in_mode:138-145`, and `AutoWorld.collect_item:548-554`.
It is AP4901023, both-mode filler, source quantity `sys.maxsize`, material0.

Advancement/strength axes are not actual recipient receipts.
This is source preservation, not a new mechanic, cap, or product selection.
Full general AL/PM/LC and final independent common/combined review remain separate.

### Closure-r2 corrected generator evidence

The original `03265dbc...` packet and earlier helper-only conformance are historical inputs.
The corrected R2 packet preserves general AL/PM/LC as open.
Its source/common-conformance seals and bounded rechecks precede the separate combined review.
Raw models/results remain in their original generator session.

The nine-file bounded R2 manifest SHA-256 is `ec60fa45b7647f2db6eb3d53eae9629cf68271898e4413c190ab64212c38c648`.
The supplemental design SHA-256 is `76a8f28a26f1f285e0bc5c8bd49db12d586c5217b141faf6dc8f2ee26a813c34`.
The model/result SHA-256 values are `1ad352f6cd1273ec3e17c4d2c6e0da84a0ffd7dc270201cbf5bafdd37eb6cc7a` and `591eaff09c2fae87168e83ceed5632cef0945e7e5e047ef583ede91a12997e5d`.
The final retained-history review SHA-256 is `6ed4d0cddfa1805ea8e82e9b3be22f548110194f2132fe9997711911f7ced0a4`.
It supersedes the `f4710605...` fingerprint and preserves its bounded verdict/history.
The separate receipt-only recheck changes no R2 proof domain.

The separate conformance receipt SHA-256 is `f6befbdd69dd7826243fed39a0df767c33806f3fe9a3aa3f9af299e092c99672`.
Its integration manifest SHA-256 is `34df0b16bc9893b3242047647546140c4fb1a025f34f48788a23d2f8fcc8015c`.
The receipt binds common e5d and both corrected helpers without rewriting the nine reviewed R2 files.
Records31/transactions10/default12 and both exact Unit-source rejections are preserved.
The independent refresher replay matched byte-for-byte and made no peer output writes.

The bounded certificate uses unlocked Fundamental N0..71 and Material0..40.
Its ten positive actions and spend-complete terminal predicate are explicit consumer preconditions.
It excludes selective/default masks, guard-induced early stopping, exact selectors, Castlers/Consuls, and complete reserve accounting.
Material41..213 is outside its certificate domain.

| Executed evidence | Exact result | Exclusion |
| --- | --- | --- |
| Cohort coverage | 14760 raw stage/count/budget cells, 4674 exact no-reserve cells, 675 small-graph comparisons | Not a full practical allocator or legal multi-obligation witness |
| Relaxed placement certificate | 1044 integer unit inequalities, actual-subset inclusion, 474630 feasible branches | Declared role/funding preconditions only |
| Independent minima | N11/M12 gives non-Pawn4/E5285/B5375. N33/M33 gives9/14100/15025 | Counterexamples to automatic target5/19 satisfaction, not guarantee changes |
| Prefix closure | 58666 ordered inventory/geometry edges and298480 pocket-prefix cells | Three declared upper boxes, not every allowed C/world |
| Forward semantic witnesses | Paid150/200 prefixes reach11 or33 non-Pawns without retracting the purchase | No exact type/seed or whole-world acceptance |
| Original-token D | Funding785 ->500 direct +200 conversion +85 residual. Eight protected Pawns give1585=500+200+800+85 and D1/R8 | Funding-origin D3/R7 is wrong. Not a general allocation proof |
| Actual Fill reconstruction | Fresh one-player seed7 trials retain one original Chessman and one fabricated Material with matching locks/counts/RNG boundaries | No Main barrier, optional/event/link/partial-retention coverage or safe general process driver |
| Actual strict core | Beatable/full-inaccessible fails output/skip_output/spoiler_only in normal and optimized Python; frozen binding unchanged | Proposed adapter characterization, not an existing ChecksMate override |

Exact bitset/budget pruning and scalar recurrence avoid N33's18643560 multiset enumeration.
Scalar minima are separate bounds, not one roster.
The legal forward oracle cannot substitute the metric overapproximation for a concrete witness.
Useful bounded caps do not close all rule quantities or full policy domains.

Source anchors remain `Main.py:148-193`, `Fill.py:1086-1090`, and `AutoWorld.py:435-450`.
Reachability precedes target draws in early plando.
The internal retained-choice record preserves object/occurrence identity, clamps, natural RNG, disposition, and provenance.
A fresh deterministic fixture does not prove arbitrary hooks, external effects, caches, or complete trial coverage.

The [existing proof checklist](engineering-closure.md#33-remaining-proof-checklist-and-stopping-criteria) names owners, consumers, decisive observations, and stopping criteria.
No new ticket or automatic tracker transition follows from partial delivery.
D22 remains claimed, S0 remains not ready, and the design-first hold stays active.

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

The prepared entry installs reversibly before acceptance.
The accepted observation delivers resulting earned intent, never a second ledger write.
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
| 0022 | Legacy funding/conversion, Fundamental recovery, effective policy and local settings | D22, S0, A22, P1, G2, U1, I2 | Policy settled. Engineering gates and final semantic publication remain open |

## Important file dispositions

The specification contains the exclusive write sets.
The reports contain expanded source maps.
These distinctions prevent unrelated or ineffective work:

| Surface | Disposition |
| --- | --- |
| `ChessV.Base\Game.cs`, `ChessV.Base\MoveList.cs`, `ChessV.Base\ApmwCore.cs` | E1 owns committed lifecycle facts and the conditional exact-attempt seam |
| `ChessV.Games\MiscellaneousGames\ApmwChess.cs` | Proposed E1 -> B1 -> U1 serial ownership. Engine/account hooks, Army bindings, then final attached composition |
| `APMW.Client\LocationHandler.cs`, `CaptureLookup.cs` | H1 owns semantic progress, not transport |
| `APMW.Client\Client.cs` | N1 owns transport and reporting lifecycle |
| `APMW.Client\Config.cs`, `ApmwContractV2.cs` | C1 owns strict v4 migration. Config transfers serially to A22 for effective allocation consumption |
| `APMW.Client\Convenience.cs` | U1 owns selected recent successful connection details |
| `ChessV.GUI\Forms\GameForm.cs`, `ApmwForm.cs` | U1 owns interaction and shared-state presentation |
| `ChessV.Base\Search.cs` | Characterization only unless a fixture proves a missing seam |
| `APMW.Core\ApmwEvents.cs`, root event/manager remnants | No new authority and no unrelated cleanup |
| Shared project files | U1 registers resources and sources. No concurrent project edits |
| `setup.iss` | Historical, inspect-only unless a later release decision adopts it |

## Open questions and exclusions

| ID | Open matter | Resolver | Blocking effect |
| --- | --- | --- | --- |
| D22 | Selected-policy consolidation and EP/NM/AL/PM/LC/TX/TR/CS engineering closure | Design owner, package specialists in closure register | Blocks S0 readiness under the all-design-first hold |
| D-SETTINGS | Per-key normalized defaults, literal fields and retirement agreement | Ticket 24 matrix, C1/A22/U1 | Blocks S0 readiness. Settled settings are not re-grilled |
| ENG-SEAMS | Exact DTO declarations and assembly visibility | S0 implementation owner | Blocks parallel consumers until the seam gate passes |
| ENG-ACTOR | Actor-right representation and player-specific promotion adapter | R1 with B1/U1 | Contained engineering work, not a reason to reopen royal policy |
| ENG-RUNTIME | Actual stock-library/runtime admission and retirement behavior | N1 and I2 | Required runtime acceptance, not a transport-fork proposal |

Tracker UI, Regions refactoring, ordinary-game data migration, durable journals,
automatic reconnect, legacy world conversion, and balance tuning remain excluded.
No implementation or release action occurred in this assessment.

## Handoff readiness

The documentation reconciliation can finish while engineering gates remain open.
S0 is not ready until those gates have supported closure and explicit execution authorization.
The initial S0 declaration PR then needs user approval before consumers.
The dependency graph records final A22-dependent acceptance and serial file transfers.
Independent modules can use complete synthetic fixtures without fabricating
production hashes.

No immediate implementation or full release readiness is declared.
RI records the user-directed stock decode scope correction, not a readiness blocker.
The closure register preserves the exact user quote and separates the coordinator's interpretation.
The [question network](qn_architecture.md) records this partial closure.
The [fanout manifest](fanout.json) is a portable assignment inventory, not a
claim of compatibility with an uninspected fanout tool.
