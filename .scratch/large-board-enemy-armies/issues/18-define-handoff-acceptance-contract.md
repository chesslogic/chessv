# Define the spec handoff and acceptance contract

Type: grilling  
Status: resolved  
Blocked by: 17

## Question

What evidence must a later cross-repository implementation spec require before geometry-driven CPU augmentation and semantic Locations are considered complete for exactly `6x8`, `8x8`, `10x8`, `10x10`, and `12x10`?

## Acceptance ownership

These roles own later evidence, not active agent assignments.
The final implementation tickets define file boundaries after design closure.

| Owner role | Required evidence | Primary surface |
| --- | --- | --- |
| ChessV army-data owner | All 20 formations, both colors, exact roles, counts, bands, promotion lists, and castling metadata | `ChessV.Test\ApmwGeometryProfileTests.cs` and the shared-data fixtures |
| ChessV game-rules owner | CPU survival, castling actors, isolated promotion permissions, preserved human rules, and existing evaluation hooks | `APMW.Test\ApmwGameCharacterizationTests.cs` and the royal fixtures |
| ChessV client owner | Stable capture identity, committed-move reporting, rollback/load isolation, tactical classes, and world snapshot retention | Existing Location and client-contract suites |
| ChecksMate world/projector owner | Role mappings, derived budgets, correction provenance, complete rule paths, immutable snapshots, and shared projection data | Existing geometry, Location, rules, logic-projection, and contract-resource suites |
| Cross-repository integration owner | Strict compatibility gates, matching manifests, and real x86/x64 package restoration and consumption | The shared-data specification's package and lifecycle matrix |

The integration owner collects evidence from every boundary.
A source-only fixture result does not replace real packaged interoperability.
Play-test calibration remains a separate later activity.

## Resolution requirements

- The independent [royal lifecycle fixtures](../contracts/royal-lifecycle-fixtures.md),
  including their attached-match, reflection, and fault-injection boundaries.
- Exact golden setup arrays for every formal geometry, side, and Enemy Army family, including the augmented 10x10 and 12x10 arrays.
- FEN/setup, pawn/non-king/royal count, promotion, and fallback assertions.
- Check, royal-capture, stalemate, castling-right, and engine-evaluation characterization.
- CPU royal membership after a human capture, in both color assignments and
  with human King-upgrade counts 0, 1, and 2.
  The CPU policy must not depend on those human upgrades.
- Separate successful undo, failed speculative move, failed committed move,
  and repeated-load fixtures. Each restores board state, royal phase, actor
  identity, castling rights, capture counters, and pending reporting state.
- Explicit observer boundaries for failed committed moves. A setup
  notification must not survive as a successful capture or Regicide report.
- Legal castling with both CPU Kings and with only the original primary.
  Both phases retain the from/transit/to attack exclusions.
  The additional King alone has no castling rights.
- Promotion-list construction independent of type registration, followed by
  the [compact promoted-Queen fixtures](../research/royal-capability-frontier.md#independent-compact-tactic-fixtures)
  under [ADR 0017](../../../docs/adr/0017-separate-cpu-promotion-permissions.md).
- Exact CPU promotion sets across all four families and five geometries.
  Vary human inventory, pockets, and King upgrades without changing those
  sets. Explicit, absent, and unknown Standard input must agree.
- No basic Elephant or legacy12 Nightrider CPU promotion.
  Rookies retain Lion promotion, but other families do not gain it.
  CPU registration must not grant new human promotion rights.
- Basic Elephant qualifies for Threaten Minor, but not Threaten Major or
  Threaten Queen. Q34 grants no human random-pool membership.
- Both last-King checkmate and zero-King extinction produce human stage
  victories. The direct-extinction fixture awards victory without Capture
  Everything because a CPU Rook remains.
- Stable role-to-coordinate fixtures and an old-to-new semantic ledger.
  Runtime Location migration is not required across the 0.4.0 boundary.
- Capture fixtures from ticket 15: the six distinct Rookies Lion roles,
  promotion retaining pawn classification, two-King Regicide, and crossed
  series thresholds from one committed multi-capture.
- Initial-setup King eligibility, without a counter-policy change when one
  King remains. A captured King cannot substitute for an uncaptured non-King
  in Capture Everything.
- Endpoint-only Capture Everything and earlier-board count goals, including
  Capture Any 15 on 8x8 in a larger world. A client must not report an
  unpublished threshold.
- Complete per-geometry access alternatives within the retained single-Region
  structure. Material, chessmen, and board-dependent special conditions must
  qualify together on one path, not combine across different boards.
- Capability exclusions remain stronger than budget values. A zero-cost
  castling reference cannot create a 6x8 castling path.
- Contract schema/hash/runtime consistency checks in both repositories for exactly the five formal geometries.
- The [shared-data specification](../contracts/shared-data-contract-v4.md),
  including strict positive/negative wire pairs and real package restoration
  for both Windows architectures.
- Existing tactic-source and mode-dependent castler predicates on each
  qualifying geometry path. Neither the resource bypass nor zero material
  may remove those predicates.
- Material table derivation snapshots, authored-offset review, and monotonicity checks.
- The 0.4.0 world-bound cost snapshot, with the same records for generator
  rules and client consumption. Reconnection retains the original world
  costs. A compatible numeric revision does not require rebuilt binaries.
- Cost-snapshot rejection cases for malformed or unsupported schemas,
  mismatched army/Location/projection bindings, duplicate records, and missing
  required records. No client-local price fallback is permitted.
- Tracker UI remains outside this handoff. Its data contract is in scope.
- Explicit separation between deterministic automated acceptance and downstream play-test calibration.
- A compatibility matrix for new client, world, layout profile, and Location
  profile versions, with rejection cases for old and incompatible mixed
  contracts. Older worlds use matching older clients.
- Bounded acceptance showing that 12x12 is absent from the selector and rejected
  at the shared geometry entry point. No legacy 12x12 migration matrix is
  required.
- Named primary test surfaces, including `ChessV.Test\ApmwGeometryProfileTests.cs` and `APMW.Test\ApmwGameCharacterizationTests.cs`.
- A handoff rule that converts the resolved map into a spec and implementation tickets without reopening settled design questions.

## Answer

Q35 confirms the acceptance contract and its owner roles.
The [royal lifecycle fixtures](../contracts/royal-lifecycle-fixtures.md),
[shared-data specification](../contracts/shared-data-contract-v4.md), and
[cost-snapshot specification](../contracts/world-cost-snapshot-v1.md)
define the required behavioral and wire evidence.

The map is ready for a later spec and implementation-ticket handoff.
That handoff must preserve the accepted decisions and exclusions.
It must assign bounded file ownership and declare dependencies for parallel
implementation.
It must not treat initial calibration estimates as gameplay evidence.

Design sign-off does not publish that handoff, repair the absent execution
adapter, or authorize production work.
Real packages, runtime evidence, and release actions remain downstream.
