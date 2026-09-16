# Define the versioned cross-repository contract

Type: grilling  
Status: resolved  
Blocked by: 14, 15, 16, 20

## Question

Which repository publishes the shared exact deployment data for the formal geometry set `6x8`, `8x8`, `10x8`, `10x10`, and `12x10`, and how do both consumers enforce its versions, Location contract, material contract, and rejection of 12x12?

## Decision record

### Current boundary at Q35

The user selected a world-bound cost snapshot for the 0.4.0 contract.
Tracker UI remains deferred.
The [cost-snapshot ADR](../../../docs/adr/0016-publish-world-bound-location-cost-snapshots.md)
records the decision.
The earlier calibration-only alternatives remain history, not open choices.
The [shared-data specification](../contracts/shared-data-contract-v4.md)
supplies the literal schemas, package metadata, publication order, and wire
fixture templates.
Q35 confirms these engineering selections.
Actual hashes and release pins are future build outputs, not missing human
decisions.

### 2026-09-13: Shared exact deployment data

The user selected shared versioned exact deployment data consumed by both
repositories. The shared artifact contains exact starting arrays and
starting-role bindings, not only versions and summary counts.

[Shared exact deployment data](../../../docs/adr/0001-shared-exact-deployment-data.md)
records this architectural decision. The next round settles the publication
split below. The exact schema and version identifiers remain open.

### 2026-09-13: Publication and data ownership

The user selected this split:

- ChessV owns and publishes the versioned army data, including piece
  identities, exact starting arrays, and starting-role bindings.
- ChecksMate owns the Archipelago Location mapping, including numeric IDs,
  names/aliases, and authored difficulty values.
- ChecksMate consumes a pinned version of the army data. The world contract
  identifies the compatible versions.

Both consumers use the same exact deployment definition. The generator does
not independently reconstruct the arrays. Ordinary-game army definitions do
not depend on Archipelago-specific Location bookkeeping.

### 2026-09-13: Reject 12x12

The user clarified that no release contains 12x12 support and chose rejection,
not a legacy 12x12 compatibility path. The selector must not offer 12x12.
The five agreed geometries remain available within their progression rules,
with 12x10 as the maximum.

[Reject unsupported 12x12 games](../../../docs/adr/0002-reject-unsupported-twelve-by-twelve.md)
records the boundary. Existing unreleased 12x12 source and fixtures do not
create a support obligation. This does not decide compatibility with other
old contracts.

The implementation plan uses a simple rejection at the shared geometry entry
point and excludes 12x12 from the selector. It does not require a broad 12x12
migration framework or repeated guards throughout gameplay.

This decision concerns entry into a 12x12 game. Acceptance of an old manifest
that mentions 12x12 but requests another board remains part of the separate
old-contract compatibility decision. Supported goal completion must not depend
on an unreachable 12x12 stage.

### 2026-09-14: Breaking-release target

The user selected 0.4.0 as the next planned breaking release. Location IDs
can change across that boundary, but there is no instruction to renumber them
unnecessarily.

This fixes the application release target, not the wire-schema or algorithm
version identifiers. The following answer settles old-world support.

### 2026-09-14: New-contract-only compatibility

The user selected new-contract-only support for the 0.4.0 client. Older
worlds use matching older clients, not a legacy mode inside the new client.
This concerns world contracts, not removal of the existing Legacy itemization
mode.

The client rejects incompatible contracts before a match starts. Compatibility
depends on the declared contract and pinned data, not merely matching
application version strings. Mixed layout and Location contracts cannot use
guessed mappings or silent fallback.

This also resolves the earlier old-manifest question: an older contract is
not accepted even when it requests 8x8 rather than its listed 12x12 geometry.
No legacy translation or progress migration is required across this boundary.

[The 0.4.0 contract boundary](../../../docs/adr/0005-break-contract-compatibility-at-zero-four.md)
records the policy. The exact package, fields, pins, and rejection fixtures
remain to be specified.

### 2026-09-14: Region representation was proposed, then deferred

The user retained the resource bypass and suggested that Regions could express
the behavior. They later deferred that refactor from 0.4.0.
The [Regions decision](20-decide-region-based-reachability.md) records the
door-cost intent and the deferral.

The existing single-Region structure remains. It can use per-geometry rule
alternatives, each with a complete set of qualifying conditions.
Board-specific requirements and the bypass remain accepted behavior.
Do not create duplicate Locations or mix conditions from different boards.

## Compatibility acceptance cases

The new artifact's literal schema identifiers and hash remain open. Each case
uses the same approved artifact as its reference once those fields are fixed.
These cases define required outcomes, not a completed wire fixture set.

| Client and world inputs | Required outcome |
| --- | --- |
| The 0.4.0 client and world declare the new compatible contract and pinned data. The requested geometry is supported | Contract compatibility permits the match, subject to the normal setup requirements |
| 0.4.0 client and an older world contract request 8x8 | Reject before the match. A supported geometry does not make an old contract compatible |
| Both applications report 0.4.0, but their declared contracts or pinned data differ incompatibly | Reject before the match. The application version alone is insufficient |
| A new CPU layout arrives with a legacy Location profile | Reject before the match. Do not substitute or guess the missing mapping |
| An older client receives the new world contract | It is not a supported pair. Specify an enforceable incompatibility gate during integration |
| A request selects 12x12 | Reject through the agreed geometry boundary |

The user selected the compatibility boundary. Source evidence about parseable
older manifests does not create an exception. Exact gate placement and wire
fixtures remain open specification work.

## Contract-mechanics investigation

The [source report](../research/cross-repo-contract-mechanics.md) distinguishes
the current package formats and their consumers.
The existing frozen contract belongs to ChecksMate.
The separate ChessV-owned exact army artifact is not an implemented
publication path in the examined source.

Per-Location intrinsic budgets belong to ChecksMate's goal rules.
ChessV's integer `ExpectedMaterial` map describes player projection values.
The new fractional goal requirements do not imply fractional player metrics
or a client-side goal-budget evaluator.

Current integration has concrete gaps.
The producer freezes v3 while the live ChessV parser requires v2.
Restoration pins the old hash and rejects the producer build manifest's
`minimum_client_version` field.
The world-contract parser itself requires that field.
The new design must align these separate formats without weaker validation.

### Earlier alternatives: Calibration-only revisions

Two compatibility models remain possible:

| Model | Result |
| --- | --- |
| One client/sidecar pin includes every calibration revision | A numeric goal-budget change requires coordinated package updates |
| A separate calibration pin records generator-only estimates | Numeric goal-budget changes can use the same compatible client and sidecar |

The recommendation is a separate calibration provenance pin.
This path covers intrinsic material/chessmen estimates, difficulty
coefficients, and authored corrections.
It does not cover army arrays, capture meanings, Location identities,
geometry, or shared projection/item values.
Those shared semantics remain compatibility-bound.

The user must select this compatibility boundary before the final field
placement and package-pin specification.
The existing new-contract-only policy still rejects old pre-0.4.0 contracts.

### Round 14 feedback: A built-in tracker will consume costs

The user identified a planned built-in client tracker.
It requires synchronized Location cost definitions between the client and
world. Thus, calibration cannot remain generator-only data for that future
consumer.

The user did not select a permanent provenance-only contract.
Their "Until then perhaps it's not necessary" leaves the current release's
scope open. This response does not authorize tracker UI implementation.

The earlier question conflated two forms of binding:

| Binding | Effect |
| --- | --- |
| A cost revision compiled into each binary's compatibility pin | A numeric revision requires new matching binaries |
| A world-bound cost snapshot that both consumers read | Compatible binaries can consume revised numeric data without independent cost tables |

The second form can synchronize the tracker and generator without a client
release for each rebalance. It requires a versioned cost-data schema.
The generator must use the same frozen cost records that it publishes for
the client. The snapshot must identify the world calibration revision and
its compatible shared gameplay/projection contract.

The revised recommendation is to include this snapshot in the 0.4.0 contract
and leave tracker UI work separate.
The alternative is provenance-only metadata in 0.4.0, followed by the snapshot
when the tracker arrives. A fixed compiled calibration pin also remains an
available, more tightly coupled choice.
The later Q32 answer selects the snapshot for 0.4.0.

The snapshot concerns exact per-geometry cost definitions.
It does not itself specify the tracker UI or all availability conditions.
It cannot erase the retained resource bypass, capability limits, or
complete-path requirement.

### 2026-09-15: Include the cost snapshot in 0.4.0

The user selected: "Include the world-bound cost snapshot in 0.4.0; defer
tracker UI".
ChecksMate publishes the same frozen cost records that its generator uses.
The client consumes those records, not a separately maintained price table.

| Exact claim | Class | Authority | Excludes |
| --- | --- | --- | --- |
| The 0.4.0 contract includes the world-bound cost snapshot | Author decision | Q32 | Provenance-only metadata or deferral until tracker UI |
| The generator and client use the same world-specific cost records | Author decision | Q32 proposal and selected option | Independent client calibration curves |
| Existing worlds retain their original costs | Author decision | Q32 proposal and selected option | Updating a world from the latest calibration on reconnect |
| Numeric revisions can retain compatible binaries | Author decision | Q32 proposal and selected option | A mandatory client/sidecar rebuild for every cost change |
| Schema and shared gameplay compatibility still apply | Author decision | Q32 and the earlier compatibility boundary | Accepting unknown semantics because costs parse |
| Tracker UI implementation remains deferred | Author decision | Q32 | Adding tracker presentation to the implementation scope |

The snapshot carries calibration provenance and compatible army, Location,
and projection bindings.
It preserves per-geometry costs.
Resource bypass, capability limits, and complete-path requirements remain
separate and binding.

### Cost-snapshot schema and lifecycle

The [version 1 schema draft](../contracts/world-cost-snapshot-v1.md)
specifies `apmw_location_costs` as a separate slot-data object.
It binds to the shared contract rather than changing the projector package's
hash for every numeric calibration revision.

The draft retains exact intrinsic values and the world's effective integer
costs.
The generator uses those effective costs.
The client retains the same records without a separate calibration model.
The snapshot freezes only after the obtainable-count context supplies the
stage caps.

The document also specifies correction records, canonical fractions,
canonical hashing, and exact reconstruction fixtures.
It is an engineering specification under Q32, not a new gameplay decision
or an execution handoff.
The shared-data specification supplies the army and Location schemas.
The actual build hashes come from implementation artifacts.

### Shared-data specification reviewed

The parent reviewed the bounded
[shared-data specification](../contracts/shared-data-contract-v4.md).
Its engineering selections are shared contract 4.0, Army schema 1.0,
Location profile 2.0, and cost snapshot 1.0.
The first new projector runtime is 0.2.0 with transport protocol 1.
Build, release, and restore-lock metadata advance together to version 2.

The hash graph separates shared semantics from world-specific calibration.
The publication order starts with ChessV army data, then the ChecksMate
package, then the ChessV restore pin.
It does not create a circular desktop-build dependency or a runtime data
service.

The spec preserves current generator predicates for fork and non-pawn
threat goals.
It also preserves the mode-dependent castler requirements.
Those conditions remain outside the resource bypass and inside each
complete geometry path.
Ordinary-game conversion remains outside this effort.

Q34 settles Basic Elephant's tactical class as Minor.
Q35 confirms the concrete engineering selections.
The schema fields and version strings do not require separate interviews.

## Resolution requirements

- Define the schema and distribution mechanism for ChessV-owned exact
  deployment data, including coordinates and starting-role bindings.
- Define the next CPU layout and Location profile version identifiers.
- Specify all contract fields needed for the five formal geometry records, including augmented stage arrays, role IDs, royal/castling metadata, CPU counts, and material-calibration metadata.
- Define canonical hashing and the required manifest-hash bump.
- Specify rejection of old slot data, old clients, and mixed layout/Location
  contracts under the accepted 0.4.0 boundary. Reject 12x12 instead of
  preserving or silently converting it.
- Name the conceptual consumers in ChessV and `worlds\checksmate` without prescribing exact production diffs.
- Specify the world-bound cost snapshot and its calibration/schema bindings.
  The generator must use the same frozen records that the client receives.
  Existing worlds must not read newer costs on reconnect.
- Define the validation responsibilities for ChessV-owned army data and
  ChecksMate-owned Location mappings and authored thresholds.
- Specify version negotiation and publication order for the pinned compatible
  components. Publication must not let one repository silently drift.

Exact external repository edits remain downstream; this ticket settles the contract they must implement.

## Answer

Q35 confirms the [shared-data specification](../contracts/shared-data-contract-v4.md)
and [world cost snapshot](../contracts/world-cost-snapshot-v1.md).
They define exact ownership, schemas, semantic bindings, strict compatibility,
package metadata, and offline publication order.

Army data publishes before the ChecksMate package and ChessV restore pin.
World-specific numeric revisions do not change the shared semantic pin.
The client retains each world's original costs.
Older worlds require their matching older clients.

Actual serialization, hashes, release artifacts, and restore pins remain
implementation deliverables.
This resolution does not edit or publish either production repository.
