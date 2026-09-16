# Cross-repository contract mechanics

Status: Source investigation complete. Q32 selects a world-bound cost snapshot
in the 0.4.0 contract and defers tracker UI.

This report describes current source, not an implemented version of the
accepted army-data ownership policy.
The investigation changed no production files in either repository.

Source prefixes in this report mean:

| Prefix | Repository path |
| --- | --- |
| ChessV | `C:\GitHub\chessv` |
| ChecksMate | `C:\GitHub\rft50-checksmate` |

## Current artifact ownership

ChecksMate owns the existing frozen contract resource and its package loader.
The resource is `data/apmw_contract_v3.json`.
The loader derives the frozen hash and minimum client version from that file.

Source: ChecksMate
`worlds\checksmate\apmw_projection\resource.py:17-45`.

The examined projector build packages that frozen contract.
Its metadata contains the runtime version, protocol version, contract hash,
and minimum client version.
The release builder requires matching metadata across the Windows x86 and
x64 archives.

Sources: ChecksMate
`worlds\checksmate\tools\build_apmw_projector.py:56-76,86-107`;
`worlds\checksmate\tools\create_apmw_projector_release_manifest.py:49-117`.

This is existing offline package plumbing.
The separate ChessV-owned army artifact remains an accepted design requirement,
not an implemented publication path in those build scripts.
The existing producer contract contains CPU profile identifiers.
Those fields alone do not implement shared exact army definitions.

Source: ChecksMate
`worlds\checksmate\apmw_projection\data\apmw_contract_v3.json:400-402`.

## Goal budgets and player material are different data

ChecksMate stores per-Location material and chessmen estimates in
`CMLocationData`. Its current material fields are `int | None`.
The base/grand selector can choose their minimum.

Source: ChecksMate `worlds\checksmate\locations.py:53-104`.

The actual Location material rule computes:

```text
target = material * difficulty + (absolute_relaxation if material > 90 else 0)
target = min(target, projected_stage_maximum)
qualifies = projected_player_material >= target
```

The current difficulty calculation uses floating-point factors.
The rule has no explicit final ceiling.
The accepted exact-fraction and single-ceiling design replaces that arithmetic
for goal requirements.

Sources: ChecksMate `worlds\checksmate\rules.py:115-152,227-251,310-327`.

The `base_material * 100 * difficulty` expression is a different path.
It belongs to the minimum/maximum item-pool material calculation.
It is not the per-Location rule.

Source: ChecksMate `worlds\checksmate\rules.py:155-211`.

ChessV's `ExpectedMaterial` map supplies integer player-item values.
Its entries include `material_item`, `play_as_white`, `pocket`, and
`king_promotion`. The projection engine uses those values for the player's
primary royal, entitlements, and granted material.
They are not the intrinsic CPU capture budgets from the new CSV.

Sources: ChessV `APMW.Client\ApmwContractV2.cs:54-70`;
`APMW.Client\ProjectionV2SemanticEngine.cs:371-392,622-634`;
`APMW.Client\ApmwSidecarSnapshot.cs:342`.

Exact goal-budget arithmetic can therefore remain in ChecksMate.
The selected fractional intrinsic values do not themselves require fractional
player projection metrics or a new C# goal-budget evaluator.
The new geometry and contract still require separate client changes.

## Current incompatibility gaps

| Surface | Current behavior | Consequence for the new contract |
| --- | --- | --- |
| Producer resource | Loads frozen v3, whose minimum client version is 0.4.0 | This application label alone does not establish client support |
| Live client parser | `ParseCurrentContract` calls `ApmwContractV2Parser.Parse`, which rejects a major other than 2 | The live parser must implement and require the new agreed contract |
| Missing contract | The connection path permits absence before later mode-specific checks | New-contract-only support needs a required-contract gate |
| Restore lock | Requires runtime 0.1.0, protocol 1, and the frozen v2 hash | The restored package pin must change coherently |
| Producer build manifest | Includes `minimum_client_version` | The consumer must validate this metadata rather than discard it |
| Restore build-manifest parser | Its exact field list excludes `minimum_client_version` | The current producer output and restore parser are incompatible |

Sources:
ChessV `APMW.Client\ApmwGeometrySelection.cs:105-137`;
`APMW.Client\ApmwContractV2.cs:154-181`;
`APMW.Client\Client.cs:209-267`;
`APMW.Client\ApmwProjectorLock.cs:84-87,111-137`;
`tools\Restore-ApmwProjector.ps1:26,124-135,285-300`.
ChecksMate `worlds\checksmate\apmw_projection\data\apmw_contract_v3.json:3-8`;
`worlds\checksmate\tools\build_apmw_projector.py:89-94`.

The strict restore failure concerns the embedded **build manifest**.
It is not evidence that the world-contract parser rejects its own
`minimum_client_version` field. That parser requires the field.
The lock, release manifest, build manifest, and world contract are distinct
formats.

## Existing mechanisms to retain

Canonical contract hashing rejects duplicate keys, blanks the root
`manifest_sha256`, sorts object keys, and preserves array order.
The ASCII serialization produces UTF-8 bytes for SHA-256.

Source: ChessV `APMW.Client\ApmwContractV2.cs:14-19,640-701`.

The sidecar response must match the request's protocol and contract hash.
The new data must preserve that binding rather than weaken it.

Source: ChessV `APMW.Client\ApmwSidecarProtocol.cs:243-250`.

The engineering default is a pinned, vendored army input in the existing
offline package process. No examined requirement calls for a new service.
Literal file names and schema version strings are specification work, not
gameplay questions.

Numerator/denominator records are a conventional exact representation for
authored calibration data. Player material metrics can remain integers.
The production serialization and field placement still need a contract
specification.

## Newly exposed compatibility choice

The agreed owner of calibration data is ChecksMate.
The agreed owner of exact army data is ChessV.
The earlier compatibility decision does not explicitly settle whether a
calibration-only revision requires a new client and sidecar package.

| Alternative | Consequence |
| --- | --- |
| Bind every calibration revision into the client/sidecar contract pin | Numeric tuning requires a coordinated client and projector update |
| Pin calibration separately from shared gameplay/projection semantics | A new world can use revised goal estimates with the same compatible client |

The recommendation is separate calibration provenance.
Only numeric goal estimates, their difficulty coefficients, and authored
corrections qualify for this independent revision path.
Army definitions, role/Location identities, actual capture conditions,
geometry, and shared player projection semantics remain compatibility-bound.

This recommendation is not yet an author decision.
It does not permit old pre-boundary contracts, mixed army/Location semantics,
or silent substitution of unknown data.

### Round 14: Future client consumption changes the recommendation

The user identified a planned built-in tracker that requires synchronized
cost definitions. Provenance-only metadata does not supply those definitions
to the future tracker.

This does not imply a cost table compiled into every client binary.
A versioned, world-bound cost snapshot can supply both consumers.
Within a compatible schema, a new numeric snapshot can retain the same client.
The client and generator must not maintain independent cost copies.

Q32 selects the snapshot in the 0.4.0 contract.
The tracker UI remains separate scope.
The [owning record](../issues/17-define-versioned-cross-repo-contract.md#round-14-feedback-a-built-in-tracker-will-consume-costs)
and [ADR 0016](../../../docs/adr/0016-publish-world-bound-location-cost-snapshots.md)
record the decision. The schema and package integration remain specification
work.
