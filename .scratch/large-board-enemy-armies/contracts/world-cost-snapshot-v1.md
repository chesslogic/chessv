# World-bound Location cost snapshot, version 1

Status: Accepted engineering specification under ADR 0016, confirmed at Q35
Owner: ChecksMate
Scope: The 0.4.0 data contract, not tracker UI or production implementation

[ADR 0016](../../../docs/adr/0016-publish-world-bound-location-cost-snapshots.md)
owns the release scope and shared-cost decision.
This document specifies a concrete data shape for later implementation.
Literal field names are engineering selections, not additional user decisions.
The [shared-data specification](shared-data-contract-v4.md) supplies the
shared-contract and Location-profile schemas.

## Authority and data flow

ChecksMate owns the calibration inputs, formulas, and authored corrections.
World generation derives exact intrinsic requirements from those inputs.
It then applies that world's difficulty, adjustments, and projection caps.
The resulting snapshot supplies the generator's resource comparisons.

The generator publishes that same snapshot in slot data under
`apmw_location_costs`.
It does not regenerate prices separately in `fill_slot_data`.
The client validates and retains the snapshot before a match starts.
A future tracker reads its effective costs without a client-local
calibration model.

The snapshot belongs to the authenticated slot's generated data.
It is not a global table fetched from the latest release.
Two worlds with identical cost inputs can have identical snapshot hashes.
The hash identifies content, not a unique seed or a digital signature.

## Compatibility and hash direction

The shared gameplay/projection contract declares support for cost schema 1.0.
The snapshot binds to that contract and its army and Location-profile hashes.
The shared contract does not contain a particular world's snapshot hash.
Thus, the hash graph has no cycle.

```text
Pinned army data + Location profile + shared projection semantics
    -> shared contract hash

Shared contract + calibration revision + world cost inputs
    -> world cost snapshot hash
```

The projector package remains pinned to the shared semantic contract.
It does not require a new package for each supported numeric cost revision.
An unknown semantic contract remains incompatible even when its cost schema
is 1.0.

The client checks the snapshot's three bindings against the accepted contract.
A version string alone cannot substitute for a matching content hash.
New numeric calibration revisions remain data, not permission to change
capture semantics or projection values.

## Root fields

Every listed field is required. Version 1.0 rejects unknown root fields.
Our raw snapshot/fixture parsers reject duplicate JSON keys before conversion.
Stock decoded snapshot objects retain every exact field/type/hash/binding check.
They do not carry a claim of raw-wire duplicate detection already lost upstream.
Shared contract section 3 defines these input scopes.

| Field | Type and meaning |
| --- | --- |
| `schema` | The literal `apmw_location_costs` |
| `version` | Object with integer `major: 1` and `minor: 0` |
| `manifest_sha256` | Canonical content SHA-256, as 64 lowercase hexadecimal characters |
| `bindings` | Exact `contract_sha256`, `army_sha256`, and `location_profile_sha256` fields |
| `calibration` | Exact `model_version`, `revision`, and `manifest_sha256` fields |
| `difficulty` | Positive exact rational value for this world's material scaling |
| `absolute_adjustment` | Nonnegative integer material adjustment before the intrinsic-value condition |
| `stage_caps` | Ordered records for the geometries in this world's configured progression |
| `locations` | Ordered cost records for this world's published numeric Location IDs |

The first calibration model identifier is `apmw-calibration-v1`.
Its positive integer `revision` starts at 1.
The calibration hash covers its reference inputs, derivation model, and
explicit correction records.
The compiled client does not whitelist one calibration revision or hash.

`contract_sha256` also binds the player-projection algorithms and values.
The snapshot does not introduce a second player-projection model.
Both itemization modes remain subject to the accepted contract.

## Exact numbers

An exact rational uses these two fields:

```json
{"numerator":"361045","denominator":"162"}
```

Both fields are canonical decimal strings, not floating-point numbers.
The denominator is positive.
The pair is reduced by its greatest common divisor.
Zero has the sole representation `{"numerator":"0","denominator":"1"}`.
Leading zeroes, a leading plus sign, whitespace, decimal points, and exponents
are invalid.

Snapshot material values are nonnegative.
Signed rational deltas belong only to the authored correction artifact.
The string representation prevents loss through an intermediate JSON
floating-point parser.

Difficulty factors use their exact decimal meaning during generation.
For example, 1.35 is 27/20, not the binary floating-point approximation.
Player material metrics and the final effective thresholds remain integers.

## Stage caps and Location records

Each `stage_caps` entry has exactly `stage_id`, `material`, and `chessmen`.
The two caps are nonnegative integers from the world's obtainable-player
projection, not from the CPU inventory.
The stage order follows the declared progression.

Each `locations` entry has exactly `location_id` and `profiles`.
IDs are the numeric IDs in the bound Location profile.
The ID order is ascending.
The snapshot contains each published numeric ID exactly once.
Event-only entries without numeric IDs are not snapshot Locations.

Each `profiles` entry has these fields:

| Field | Meaning |
| --- | --- |
| `stage_id` | One supported geometry in the declared world progression |
| `intrinsic_material` | Exact rational requirement after authored corrections |
| `intrinsic_chessmen` | Nonnegative integer estimate before its separate cap |
| `effective_material` | Nonnegative integer after adjustment, one ceiling, and the material cap |
| `effective_chessmen` | Nonnegative integer after the chessmen cap |

Each Location has one profile per statically eligible geometry.
Profiles follow progression order.
Duplicate `(location_id, stage_id)` pairs are invalid.
An absent role or impossible capability has no profile, not a zero-cost
profile.
A published Location must have at least one eligible profile.

Static eligibility uses the bound goal semantics.
Changing item counts during a playthrough does not add a new snapshot profile.
Dynamic conditions, such as possession of castlers, remain part of the
complete access path.

```text
applied_adjustment =
    absolute_adjustment if intrinsic_material > 90 else 0

effective_material =
    min(stage_material_cap,
        ceil(intrinsic_material * difficulty + applied_adjustment))

effective_chessmen =
    min(stage_chessmen_cap, intrinsic_chessmen)
```

The generator uses these effective integers in its resource comparisons.
It does not apply difficulty, a second ceiling, or another cap to them.
The client retains intrinsic values as provenance.
It does not derive costs again from the calibration curves.

## Availability is not one cost

The complete path for geometry `g` retains the accepted structure:

```text
board_available(g)
and static_goal_eligibility(g)
and special_conditions(g)
and (
    later_board_resource_bypass(g)
    or (
        player_material(g) >= effective_material(g)
        and player_chessmen(g) >= effective_chessmen(g)
    )
)
```

A Location qualifies through at least one complete path.
Material from one geometry cannot combine with chessmen or special
conditions from another.
The bypass does not modify the frozen cost or waive special conditions.
Actual earned checks remain independent of these generator estimates.

Examples include no 6x8 castling profile and no single-King Regicide profile.
Capture Everything has only the configured endpoint profile.
A published Any 15 goal can have an 8x8 full-clear profile in a larger world.
The bound goal semantics, not the snapshot parser alone, determine these sets.

Tracker presentation and the eventual client evaluator for complete
availability remain outside this document.
The cost snapshot cannot claim that sufficient material alone makes a goal
available.

## Generation and reconstruction lifecycle

The generator completes recovery, effective policy, semantic seeds and final obtainable counts as one draft context.
It then derives conservative metrics/caps and freezes policy, rules and S before dependent fill/final acceptance.
Rule closures and the pure slot-data serializer consume that same immutable state.

Closure-r2 extends the allocation domain with paid Fundamental Pawn quality.
Expected Pawn family material remains 100, separate from concrete/funded values and incremental Material spending.
PM must include those branches in conservative metrics for every permitted result and inventory prefix.
The small amended model does not prove the general bound or native cross-language agreement.

Recovery must preserve attainable quantity obligations before quality consumes their completion funding.

Current source establishes the obtainable-count dependency at
`C:\GitHub\rft50-checksmate\worlds\checksmate\__init__.py:267-279,364-371`.
Caps use those counts at
`worlds\checksmate\logic_projection.py:221-229`.
The implementation must respect that dependency instead of freezing costs
from the temporary contract maxima.

At ChecksMate `0772bac724ff483043b56979f0ef078b9c2d264a`,
`Main.py:148-181` transfers start-from-pool items and applies item links after initial counts.
The finalized context must account for these changes.
`Main.py:183-193` precedes fill.
Later post-fill/finalize hooks cannot reduce accessibility.
`AutoWorld.py:435-450` also prohibits sphere changes in pre_output.

LC's source-backed candidate uses a bounded pre-early-plando barrier, not a serializer-time repair.
The G2 candidate reserves `Main.py`, `worlds\AutoWorld.py` and `Fill.py`.
Its normalized-inventory barrier follows start/link processing and precedes early plando reachability/fill.
Planning includes retained exogenous receipts.

An existing pre_fill hook alone is insufficient.
No post-fill repricing or sphere mutation applies.
The initial generator packet supports barrier placement.
The retained-choice design and combined review must accept this scope before implementation.

AutoWorld's only proposed callback is `prepare_generation_context(NormalizedInventoryCandidate)`.
Its default is a no-op.
It cannot submit, create or remove AP items.
Stable rule delegates refuse resource queries before a candidate exists.

The collector counts unique recipient item objects from pools, locks, starts and stable pre-fill accessors.
It does not double-count group transport and locked recipient representations.
Parsed plando requests are not final retained receipts.
A finite candidate predicts possible retained exogenous receipts before its first dependent query.

After core plando, its actual receipt vector must match exactly.
Fill/balancing/finalization retain that fingerprint.
A mismatch fails without repricing.
Another candidate needs a fresh isolated draft, not shallow rollback of a filled MultiWorld.

The corrected R2 contract retains literal target/clamp/sample/object/disposition/provenance and every ordinary RNG boundary.
Its signatures include retained object identities, not only counts.
The driver replays the same accepted pre-submission plan in fresh isolated processes.
It cannot reroll ordinary choices to force an assumed signature.

General candidate coverage, process safety, observer equivalence and earliest-query/no-op proof remain open LC obligations.
An additional driver entrypoint needs explicit accepted path ownership.
The [remaining proof checklist](../../adr-implementation-design/engineering-closure.md#33-remaining-proof-checklist-and-stopping-criteria)
defines consumers, decisive observations, and refusal conditions.

The real `MultiWorld.fulfills_accessibility` validates the filled world and its configured accessibility.
An all-inventory projection is not that validation.
`Main.py:219-221` currently returns for skip_output before the output-path core invocation.
The final acceptance route must also cover skip_output.

The existing stage_pre_output dispatch permits a proposed ChecksMate override before output/skip_output/spoiler_only.
That ChecksMate override is not currently implemented.
It calls real `fulfills_accessibility()` without a supplied all-inventory state.
Existing FillError details propagate, and false fails even if beatability succeeds.

An unsuccessful world follows the existing failure path without lowering frozen thresholds or replacing the core validator.

During generation, a change to a cost input invalidates the draft snapshot
and its dependent rules.
The completed world does not permit a post-freeze update.
Publication must fail if its rules and serialized snapshot differ.

A tracker reconstruction reads the original snapshot from the slot data.
It restores the original effective policy and S, without recovery or replacement prices from newer code.
Missing or incompatible snapshot data produces an explicit error under the
new-contract-only boundary.

## Authored corrections

ChecksMate stores corrections with the calibration artifact, separate from
the ChessV army data.
A correction record contains:

| Field | Meaning |
| --- | --- |
| `correction_id` | Unique stable identifier |
| `location_key` | Semantic key from the bound Location profile, not a raw file coordinate |
| `stage_id` | Exact target geometry |
| `material_delta` | Signed reduced rational, applied to the intrinsic value |
| `decision_ref` | Owning decision or approved calibration record |
| `reason` | Explanation of the intended change |
| `evidence_refs` | Source or later gameplay evidence, with its evidence class |
| `supersedes` | Prior correction identifier, or null for the first correction |

The initial revision has zero additional material corrections for every
derived profile.
Historical adjustments already present in released references remain part
of those references.
Only one active correction exists per `(location_key, stage_id)`.
An unspecified correction does not authorize a guessed nonzero delta.
Unknown keys, duplicate active corrections, and stale bindings are errors.

A revision includes the exact before/after intrinsic values and every
affected dependent profile.
The review covers reference provenance, goal semantics, monotonicity, and
the accepted rounding order.
Negative requirements or a reversed Queen-to-checkmate ordering fail
validation instead of receiving a silent clamp.

The selected linked formulas remain linked.
For example, a correction to the Queen input requires regeneration of
Regicide under its selected three-quarter-gap rule.
A reviewer must identify any intentional change to a dependency rule.
A delta record alone cannot redefine that rule.

## Canonical hashing

The snapshot uses the existing contract canonicalization convention.
The hash calculation blanks only the root `manifest_sha256`.
It preserves nested hashes, sorts object keys, preserves array order,
and emits ASCII JSON without insignificant whitespace.
SHA-256 applies to the resulting UTF-8 bytes.

Array ordering follows the stage and Location rules in this document.
The rational representation has only one permitted reduced form.
This prevents different encodings of the same number from creating
unexplained provenance differences.

SHA-256 establishes content identity, not trust in a remote author's prices.
The generator remains the authority for world costs.

## Required fixtures

| Fixture | Complete relevant inputs and operation | Required result |
| --- | --- | --- |
| Single final ceiling | King's Rearguard on 12x10, intrinsic 361045/162, difficulty 27/20, adjustment 240, material cap 5000, chessmen 0 | Both consumers use effective material 3249 and chessmen 0. Premature rounding to 3250 is incorrect |
| Cap after ceiling | Same inputs, with material cap 3000 | Effective material is 3000. The retained intrinsic value remains 361045/162 |
| Compatible numeric revision | Synthetic revision B adds 100 intrinsic material to that one goal, retaining schema, bindings, difficulty, adjustment, and cap 5000 | B uses intrinsic 377245/162 and effective material 3384. The same compatible client accepts both snapshots. No production correction is approved by this fixture |
| Reconnect to original world | World A retains the first snapshot after revision B exists | A still supplies 3249. No latest-calibration fetch or client-local substitution occurs |
| Wrong binding | Keep A's costs but replace its army hash with a different hash | Reject before a match, even if the cost schema parses |
| Duplicate or missing record | Duplicate an existing ID/stage pair, or remove a profile required by the bound world semantics | Reject instead of selecting one duplicate or supplying zero |
| Unsupported capability | A 6x8-only world has no castling capability | No castling Location or zero-cost castling profile is invented |

The two revision fixtures use synthetic calibration changes.
They demonstrate compatibility and immutability, not permission to retune
the selected calibration.

## Remaining dependencies

The [shared-data specification](shared-data-contract-v4.md) supplies literal
identifiers, army/Location schemas, predicate bindings, and package metadata.
Actual hashes remain build-derived implementation outputs.
Q34 settles the basic Elephant tactical class as Minor.
Q35 confirms the full packet for a later handoff.
This specification does not authorize execution.
