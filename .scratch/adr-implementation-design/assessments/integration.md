# Cross-repository integration probe

Status: Source assessment complete. No runtime implementation.
ChessV baseline: `78b604e43f684c8032d1af23955485d1e196954f`
ChecksMate baseline: `0772bac724ff483043b56979f0ef078b9c2d264a`

ChecksMate means `C:\GitHub\rft50-checksmate`.
Its checkout is read-only evidence for this assessment.
Paths with the `CM:` prefix are relative to that checkout.
Other paths are relative to ChessV.

## Architectural result

The producer and client already have useful strict-parser and projector seams.
They do not yet share the accepted semantic set.
Changing only the desktop army arrays cannot implement the ADRs.
The generator must replace width-based identities and scalar prices with
published semantic goals and frozen geometry-specific costs.

The package build also has a separate incompatibility.
The producer emits `minimum_client_version`, but the current restore parser
rejects that field.
Neither package compatibility nor the final semantic pin follows from the
application label `0.4.0`.

## Current implementation evidence

| Existing file and symbol | Current behavior | Required disposition |
| --- | --- | --- |
| `CM:worlds\checksmate\apmw_projection\resource.py:17-45`, `CONTRACT_RESOURCE`, `load_frozen_contract` | Loads `apmw_contract_v3.json` and derives one frozen hash | Load validated Army, Location profile, and v4 shared contract as one semantic set |
| `CM:worlds\checksmate\apmw_projection\contract.py:21-94,187-204,650-742`, `parse_contract`, `GeometryStage` | Accepts v2/v3, includes 12x12, derives human depth from `ranks - 3`, stores fixed Location totals | Add strict v4 data model and new formulas. Production entry accepts only the selected v4 pin |
| `CM:worlds\checksmate\apmw_projection\placement.py:280-500`, `_region_usage`, `_place_active_slots`, `_apply_forwardness`, `_formation_band` | Derives mixed ranks and forwardness from total board ranks | Consume declared human depth. Ten-rank boards have six human ranks and two mixed ranks |
| `CM:worlds\checksmate\geometry_progression.py:7-174`, `BoardStage`, `_GEOMETRY_BY_STAGE` | Selectable set excludes 12x12, but legacy paths retain it. Expanded CPU counts remain 10/9 and 12/11 | Remove live unsupported paths and derive counts from Army data. Preserve selected start/endpoint restrictions |
| `CM:worlds\checksmate\locations.py:53-112,115-187,348-465`, `CMLocationData`, `location_names_for_stage`, `rule_stage_for_series` | Uses file-named Pawns, two scalar reference prices, a base/grand minimum, and one selected rule stage | Bind canonical semantic keys, reviewed numeric IDs, static eligibility, and complete per-stage profiles |
| `CM:worlds\checksmate\rules.py:114-151,184-213,226-397`, difficulty functions and `set_rules` | Uses floating-point difficulty and separately attached material/chessmen/special rules | Use exact factors and one complete predicate per eligible geometry. Costs come from the frozen snapshot |
| `CM:worlds\checksmate\__init__.py:177-256`, `fill_slot_data`, `interpret_slot_data` | Emits v3 and mirrors, no cost snapshot/world binding. Reconstructs legacy goal geometry | Emit the same immutable snapshot that rules use. Reject incompatible reconstruction instead of converting old worlds |
| `CM:worlds\checksmate\__init__.py:264-297,359-371`, `create_items`, `_set_logic_obtainable_counts` | Final obtainable counts arrive after item construction | Freeze caps and costs after this point, before final rule evaluation. Do not freeze from temporary maxima |
| `CM:worlds\checksmate\logic_projection.py:44-61,140-229,257-357`, `WorldLogicProjection` | Distinguishes exact roster totals from conservative rule metrics. Uses a 10,000-cell Fundamental envelope | Preserve this distinction for settled geometry work. Revisit Legacy soundness only under the completed ADR 0022 contract |
| `CM:worlds\checksmate\items.py:70-76` | Chessmen 107, Material 321, Board Ranks 2 | Use agreed maxima 71, 213, and 1. Keep human expected values unchanged |
| `CM:worlds\checksmate\pool_state.py:44-86`, `PoolCapacity.for_world` | Counts selected Locations through `location_names_for_stage` | Reuse the interface. Verify counts after the Location catalog changes |
| `CM:worlds\checksmate\apmw_projection\protocol.py:13-14,76-117` | Protocol 1, runtime 0.1.0, request/response contract binding | Retain protocol 1. Introduce runtime 0.2.0 with the finalized shared semantic pin |
| `CM:worlds\checksmate\tools\build_apmw_projector.py:21-27,76-107,124-150` | Packages only the v3 contract. Build manifest 1 includes minimum client | Package A/L/C bytes and strict build metadata 2 |
| `CM:worlds\checksmate\tools\create_apmw_projector_release_manifest.py:17-28,49-119` | Combines two architecture archives into metadata 1 | Validate matching data descriptors and metadata 2 before release |
| `tools\Restore-ApmwProjector.ps1:26,124-135,279-319` | Pins old semantic contract v2 and runtime 0.1.0. Exact build fields omit minimum client | Upgrade lock/build validation together. Retain integrity, extraction, provenance, and exclusive restore checks |
| `APMW.Test\ProjectorRestoreValidation.ps1:14-52` | Uses a four-byte fake executable in a synthetic archive | Retain negative parser tests, but add separate real-package consumption evidence |
| `.github\workflows\dotnet-desktop.yml:87-102` | Unit tests run before optional projector restoration | Add mandatory release-gated x86/x64 restoration and post-restore client consumption |
| `setup.iss:1-34` | Historical ChessV 2.2 installer | Inspect-only. No evidence establishes it as the current 0.4.0 package path |

The Python wrapper files `apmw_contract.py`, `contract_resource.py`, and
`semantic_projection.py` re-export canonical package types.
They need import/export migration, not separate parser or algorithm implementations.

## Proposed modules and interfaces

These names are engineering proposals, not existing files or new gameplay decisions.
The consolidated spec assigns their exclusive owners.

| Module | Proposed interface | Implementation and lifetime |
| --- | --- | --- |
| Army resource | `load_army(bytes, expected_binding)` | Validates exact records, identities, counts, bands, and hashes. Immutable process resource |
| Location profile | `load_location_profile(bytes, army)` | Resolves semantic goals, IDs, static eligibility, and special predicate IDs. No pricing |
| Calibration | `derive_intrinsic(army, profile, references, corrections)` | Pure rational arithmetic for ADRs 0004/0008-0015. Independent from player projection |
| World costs | `freeze_costs(contract, intrinsic, progression, world_inputs, caps)` | Produces the sole immutable cost snapshot after obtainable counts settle |
| Access paths | `compile_location_rule(profile, snapshot, world_context)` | OR of complete geometry predicates. Each predicate contains its own resources and special conditions |
| World publication | `publish_world_binding(snapshot, progression, modes, published_sets)` | Serializes the same frozen snapshot and derives its world binding. No recalibration |
| Package assembly | Build metadata and release descriptor schema 2 | Packages exact A/L/C bytes. Calibration and per-world snapshots are not executable pins |

Proposed new Python paths:

- `worlds\checksmate\apmw_projection\army_resource.py`
- `worlds\checksmate\apmw_projection\location_profile.py`
- `worlds\checksmate\calibration.py`
- `worlds\checksmate\location_costs.py`
- `worlds\checksmate\world_binding.py`

The strict JSON/canonicalization primitive belongs to the existing contract
module or one extracted helper used by those readers.
It must reject duplicate properties before dictionary conversion.
A new independent parser in every module would lose locality.

## Authority and release dependency

The exact wire names already exist in
`shared-data-contract-v4.md`, sections 2-9, and `world-cost-snapshot-v1.md`.
Their absence from runtime code is implementation work, not an open policy question.
Actual hashes, release commits, and IDs require materialized artifacts.
They must not be fabricated in the design.

```text
ChessV Army A
    -> ChecksMate Location profile L
    -> Shared semantics C

A + L + reference/model/correction bytes -> Calibration K
C + A + L + K + world inputs            -> Snapshot S
C + S + progression/modes/IDs           -> World binding W

Released A/L/C + projector executable
    -> build metadata 2
    -> x86/x64 archives + release metadata 2
    -> ChessV lock 2
    -> real restored client consumption
```

Army publication must not depend on a restored projector or a finished desktop
package. This breaks the cross-repository build cycle.
Numeric calibration changes alter K/S/W, not C or the executable pin.
Changed semantics alter C.

ADR 0022 affects C because allocation inputs, algorithm identity, and logic
metrics remain incomplete.
Schema implementations and pure calibration work can proceed with test fixtures.
No final production C, package release, or lock can claim completion before that
decision closes or an explicit release-scope decision separates it.

## Acceptance evidence

The complete wire fixtures remain authoritative in shared-data sections 10.1-10.4.
The following cases identify the minimum integration evidence:

| Case | Required result |
| --- | --- |
| 20 Army formations, both colors | 40 exact boards, all blanks and identities, rank-only reflection |
| 10x10 capacities | Non-primary 59, non-pawn 29, gross Pawn 50, forwardness 40 |
| 12x10 capacities | Non-primary 71, non-pawn 35, gross Pawn 60, forwardness 48 |
| Intrinsic `361045/162`, difficulty `27/20`, adjustment 240, cap 5000 | Effective material 3249, not 3250 |
| Same cost with cap 3000 | Effective material 3000, intrinsic unchanged |
| Synthetic compatible revision adds 100 intrinsic | `377245/162`, effective 3384, same compatible binary |
| Reconstruct original world after that revision | Original 3249, no replacement calibration |
| Resources split across two geometries | Unreachable unless one whole path qualifies |
| Later-stage bypass without tactic/castler predicate | Unreachable |
| Any 15 published in a larger world, completed on 8x8 | Eligible full-clear path with 8x8 clear budget |
| 6x8 castling, single-King Regicide | No invented Location/profile |
| New client with old 8x8 world or missing contract | Reject before match setup |
| Numeric ID/stage missing or duplicated | Reject, no zero/default fallback |
| Whitespace-only Army change | Same semantic hash, different raw hash |
| Actual architecture archives | Both restore and respond under their actual release pin |

Existing focused suites include `test_geometry_location_profiles.py`,
`test_rules_thresholds.py`, `test_location_logic.py`, `test_logic_projection.py`,
`test_lifecycle_capacity.py`, `test_contract_resource.py`,
`test_contract_runtime_consistency.py`, `test_apmw_projection_protocol.py`,
`test_apmw_projector_build.py`, and `test_apmw_projector_release_manifest.py`.

New suites need independent golden costs, complete-path negatives, strict
artifact parsing, and freeze/reconstruction lifecycle cases.
Current assertions that preserve 12x12 or the old capacity totals are migration
targets, not acceptance authority.

## Validation commands for later implementation

These commands are proposed, not executed by this source probe.
Run them from the indicated repository.

```powershell
# ChecksMate
$env:AP_TEST_WORLDS = 'checksmate'
python -m pytest -q worlds\checksmate\test\test_contract_resource.py worlds\checksmate\test\test_contract_runtime_consistency.py worlds\checksmate\test\test_geometry_location_profiles.py worlds\checksmate\test\test_rules_thresholds.py worlds\checksmate\test\test_location_logic.py worlds\checksmate\test\test_lifecycle_capacity.py
python -m pytest -q worlds\checksmate\test\test_apmw_projection_protocol.py worlds\checksmate\test\test_apmw_projector_build.py worlds\checksmate\test\test_apmw_projector_release_manifest.py

# ChessV
pwsh -NoProfile -File .\APMW.Test\ProjectorRestoreValidation.ps1
dotnet test .\APMW.Test\APMW.Test.csproj --filter "FullyQualifiedName~ApmwProjectorLockTests|FullyQualifiedName~ApmwSidecarProtocolTests|FullyQualifiedName~ApmwProjectionBackendTests"
```

Use the environment's existing Python interpreter and dependencies.
Real archive builds require the existing pinned cx_Freeze toolchain and both
Windows architectures. Synthetic manifests do not replace that evidence.

## Exclusions and unresolved work

The Regions refactor and tracker UI remain excluded.
Pool accounting is a consumer of changed Location counts, not permission for an
unrelated pool redesign.
ADR 0022 requires a later explicit transfer ledger, reservation equation,
composition targets, fallback, and portable semantic contract.
The final design must show that blocked path rather than hide it inside package work.
