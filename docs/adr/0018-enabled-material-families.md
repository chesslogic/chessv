# Enable material families with cumulative Fundamental allocation

Status: accepted
Date: 2026-09-20

New Archipelago worlds use Fundamental itemization with a small named distribution policy.
AP unit supply and runtime material allocation are separate stages.
Cumulative counts replace the old upgrade graph, ordinary source-family roles, and intermediate promotion history.
This document records the implementation contract, not a claim that the behavior is already implemented.

This decision extends [ADR0005](0005-break-contract-compatibility-at-zero-four.md).
The new client supports its declared contract only. Older worlds use matching older clients.
ADR0005 left Legacy itemization intact. This decision separately retires Legacy itemization for new worlds.
The [earlier slot-graduation plan](../planning/fundamental-itemization-slot-graduation.md) remains historical evidence, not the new allocation contract.

## Configuration and scope

`enabled_material_values` enables nominal families, not exact concrete-piece evaluations:

| Label | Family | Normalized material |
| --- | --- | ---: |
| 1 | Pawn | 100 |
| 3 | Minor | 300 |
| 5 | Major | 485 |
| 7 | Jack | 700 |
| 9 | Queen | 900 |
| 13 | Amazon | 1300 |

Castler is a separate unit/material grant, not another enabled family. Weak remains 75.
Stronger Pawns require no value-2 family or new Sergeant-generation feature.
Retain current army/fairy filters and concrete Pawn choices.

Require a numeric collection at producer input and a numeric array in JSON.
Normalize values as a set: accept integral numeric forms, deduplicate, and sort ascending.
Reject booleans, strings, nulls, nonfinite/fractional numbers, and unknown labels before conversion.
Thus `[9,1,5,5,3.0]` becomes `[1,3,5,9]`.

Every valid world palette must include **1 and 3 and at least one of 5 or 7**, regardless of castling objectives.
Families 9 and 13 are optional. Exactly 12 palettes satisfy this rule.
Reject `[1]`, `[1,7]`, `[1,9]`, `[1,5,9]`, empty sets, and all other nonconforming palettes explicitly.
Turning castling objectives off does not relax this rule.
This availability rule does not guarantee a minimum final-family count in each roster.

The allowlist constrains final generated ordinary and protected player families, not just old upgrade edges.
Pockets, CPU armies, CPU capture roles, and royal/common grant rules remain separate.
Do not apply another enabled-family filter to promotion targets.
The human promotion-input change is specified independently in this decision.

### Retained concrete-type variety

Retain the current concrete chooser and these per-type preferences:

| Setting | Positive threshold range | Default and no-limit sentinel | Families that use the setting |
| --- | --- | ---: | --- |
| `minor_piece_limit_by_type` | 1..15 | 0 | Minor |
| `major_piece_limit_by_type` | 1..11 | 0 | Major, Jack |
| `queen_piece_limit_by_type` | 1..9 | 0 | Queen, Amazon |

A positive threshold prefers concrete types whose choice count is below that threshold.
When every available type reaches the threshold, the chooser reuses the full available list.
Zero disables this preference, not the family. It means no type limit.
Keep the current seeded choice and per-family bookkeeping, shared between protected and ordinary materialization.
Major and Jack use the same configured threshold, but retain separate family choice counts. Queen and Amazon do likewise.

These controls encourage variety. They are not hard final-roster caps, family quotas, or minimum piece counts.
They do not block a legal W boundary or delete an allocated unit.
The global palette rule remains independent of these preferences.

### Deferred to a follow-up ADR

Modal concrete-type variety, including the suggested round-robin/chaos interface, belongs in a **follow-up ADR**.
New overall quantity limits belong in that follow-up, not this contract.
Their metrics, count scope, enforcement, modes, and defaults are not selected here.
The earlier `1+` discussion does not create new hard maxima or final-roster minima.
No implementation of either redesign is a prerequisite for this decision.

## Two-stage distribution preset

The default is **Classic** with **`[1,3,5,9]`**.
Wire IDs are `classic`, `royals`, `welterweight`, and `horde`.
Do not expose numeric overrides for distribution weights.

| Preset | Pawn: AP Chessmen | Minor: W | Major: W | Jack: W | Queen: W | Amazon: W |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Classic | 8 | 8 | 4 | 1 | 3 | 1 |
| Royals | 6 | 1 | 1 | 12 | 12 | 12 |
| Welterweight | 4 | 24 | 6 | 1 | 1 | 1 |
| Horde | 36 | 12 | 144 | 1 | 1 | 1 |

Pawn controls AP unit supply.
Its weight is the **total** Chessmen proposal multiplicity, not extra copies beyond the existing base occurrence.
Material has raw proposal weight **3** and supported denomination **400**.
There is no configurable Material-denomination feature.
Retain eligible Castler weight 1 and existing common weights, including Pocket weight 3.
Retain resource caps, quotas, capacity/rejection guards, and common-item behavior.

Successful AP proposals sample with replacement. Existing rejection can prune an occurrence.
Changing the denominator and draw path can change accepted Pocket/common counts.
Their counts are not guaranteed unchanged.
The raw additional-material-per-Chessman rate ratios are 150, 200, 300, and `100/3`, respectively.
These use `400*3/PawnWeight` when both candidate types are admissible.
They exclude Castlers, common items, locks, capacity, and guards. They are not final-world shares.

Do not derive Material frequency from the sum of W weights.
The five non-Pawn weights control ordinary runtime allocation W.
Keep each weight attached to its destination family and mask absent families without redistributing their weights.
At fixed grants, palette, destination weights, and semantic seeds, changing Pawn weight alone cannot change W.
It creates no unreceived units, no wait/no-op draw, and no permission to leave an affordable legal upgrade unused.
All presets are soft biases. Saturation is allowed. Do not silently retune the table.

### Material eligibility

Every valid palette permits non-Pawn material use. There is no valid Pawn-only world branch.
The automatic-Material omission proposal is discarded, not selected.
Retain Material's raw weight 3 and ordinary eligibility/quantity/budget guards.
Do not prune productive Material merely because units or affordability are temporarily absent.
Do not mask actual, starting, locked, or validated tracker grants.
No new no-objective Castler-suppression policy is introduced.
Palette validity and full pool capacity still do not establish world viability.

## Grants, caps, and allocation phases

Let `S` be effective ordinary Chessmen grants, `K` effective Castler grants, and `m` effective Material-item count.
Set `M=400*m`.
Keep per-item caps `S<=107`, `K<=2`, and `m<=321`, with existing reversible raw/excess handling.
Derive Castler effects from the effective accepted count, not raw excess or the Ignore flag.
Removing an excess receipt leaves grants unchanged until the effective count decreases.

An effective Castler grants one new nonroyal unit and **500 total**, including its 100 base.
It neither reserves an existing Chessman nor grants 500 extra.

```text
Resource-backed units = S + K
Total normalized material = 100*S + M + 500*K
Reserved base material = 100*(S + K)
Above-base pool B = M + 400*K
```

Do not impose a combined 107-unit cap: 107 Chessmen plus two Castlers owns 109 units.
Geometry selects active units and reserves without deleting grants.
Primary royals, Consuls, pockets, and other common grants retain separate accounting.
Record received grants, allocated normalized credits, unspent material, and concrete-value adjustments without double counting.
Each materialized unit receives its allocated normalized family credit, not another received grant.
Retain existing concrete-value adjustment logic.

The phase order is:

1. Reserve each granted unit's 100 base.
2. With protection enabled, select Major when 5 is enabled, otherwise Jack.
3. Fund `P=min(K,floor(B/(target_value-100)))` protected units and debit their above-base costs.
4. Run ordinary W on **`N=S+K-P`** units and the remaining pool.
5. If both 5 and 7 are enabled, upgrade affordable protected Majors to Jack after ordinary W.

Major funding costs 385 above base. Jack funding costs 600.
The protected postpass costs 215 per upgrade and stops at Jack.
It upgrades `min(protected_Majors,floor(remaining/215))` counts.
The 15 left from a 500 grant after a 485-material Major stays in the pool.
These are normalized family debits, not the old extra-500/700 reservation charges.

A valid world always has an enabled protected family.
When local protection is disabled, set `P=0` and allocate all resources ordinarily.
Reject a world without 5/7 rather than invoking a no-target fallback.
Unfunded protection claims are counts only.
There are no waiting-piece identities, waiting-Pawn records, or origin-specific ordinary categories.
When protection is requested and `P<K`, greedy funding leaves less than another target's above-base cost.
Ordinary W can fund lower enabled families, but cannot fund another target from that remainder.

### Local protection control

Keep the checkbox as **Ignore Castler protection**.
When enabled, it sets reserved protection to zero but preserves every accepted unit and 500-total grant.
All affected resources enter ordinary allocation without the protected Jack ceiling.
It removes reserved protection, not otherwise legal castling by ordinary pieces.
Retain the existing connection/reset lifecycle.

The checkbox is local. It changes neither world collection nor AP reachability.
AP logic evaluates protection enabled, which remains available to the player.
Include the Boolean `ignore_castler_protection` in shared projection input and cache identity.
AP supplies false. The client supplies its local value.
Help text: "Keep Castler units and material. Allocate them as ordinary pieces without reserved castling protection."

## Ordinary cumulative allocator W

For sorted enabled families `T=[t0,...,tk]`, initialize `C=[N,0,...,0]`.
`C[i]` counts units at or above that enabled threshold.
Every result must satisfy `C[0]>=C[1]>=...>=C[k]>=0`.
Final counts are `F[i]=C[i]-C[i+1]`, with a final zero sentinel.
Unit count is `C[0]`, never `sum(C)`.

A boundary `i>0` is legal when `C[i-1]>C[i]` and the remaining budget covers `V[i]-V[i-1]`.
Each legal boundary contributes its fixed destination weight **once**, not `eligibleCount*weight`.
Select among all legal boundaries, increment only `C[i]`, and debit that cost.
Continue until no legal ordinary upgrade is affordable.
Blocked boundaries consume no draws and remain eligible for later reconsideration.
There is no finite bag, refill, source-piece choice, quota target, or shape-preserving stop.

Only consecutive enabled thresholds participate:

| Valid palette | Consecutive above-base transition costs |
| --- | --- |
| `[1,3,5,9]` | 200, 185, 415 |
| `[1,3,5,7,9,13]` | 200, 185, 215, 200, 400 |
| `[1,3,7]` | 200, 400 |

Combine ordinary counters with protected final-family totals after allocation.
Each protected unit contributes to every enabled threshold at or below its final family.
Keep no separate waiting-count term.
Every snapshot recomputes from effective grants and resolved inputs.
Roster rebalancing after receipts is permitted. Counter or piece monotonicity is not promised.
Receipt order and persistent piece identities do not determine allocation.

### Deterministic draws

Reuse the shared SHA-256 counter PRF under `fundamental.cumulative.v1.draw`.
The root joins invariant decimal seed strings in this order: pocket, pawn, minor, major, queen, separated by `|`.
Connected seed values use the existing nonnegative 31-bit domain.
Do not add Pawn supply weight, receipt history, counts, timestamps, or presentation RNG state to the root.

For counter `n`, hash UTF-8 `root + LF + namespace + LF + invariant_decimal(n)`.
Read the first eight digest bytes as unsigned big-endian 64-bit.
Use `(value >> 11) / 2^53` with the existing binary64 operation.
Multiply by the total legal weight.
Select the first ascending boundary whose cumulative weight is strictly greater than that draw.
All policy weights are positive. Do not enter the old zero-weight fallback.

Reset the counter to zero for each snapshot.
Consume exactly one value per accepted ordinary increment, including a sole legal boundary.
Consume none for scans, blocked termination, protected funding/postpass, or claim bookkeeping.
Concrete choices and placement retain their separate series.

## Final pieces, placement, and human promotion rights

Use one `ordinary-nonpawn` placement category, independent of upgrade origin.
Use the existing `SemanticExpectedMaterial` descending order, then granted material descending, source ordinal ascending, and stable ID in ordinal string order.
Royals and funded protected Castlers retain special priority.
Retain existing capacity, rank, coordinate, and Pawn-placement algorithms.
CPU capture roles and starting formations are unchanged.

Materialize protected final families first, descending by normalized value, then ordinary non-Pawn families in the same order.
Assign ordinary non-Pawn category ordinals from zero across that order.
Format their stable IDs as `ordinary-nonpawn:` followed by the six-digit, zero-padded ordinal in invariant decimal.
Use one fresh family chooser and the existing `piece-type.<family>` series.
Share its retained per-family choice counts across protected and ordinary final pieces.
Choose ordinary Pawns with one `ChoosePawn` ordinal sequence.
Only funded protected pieces use `castler-protected`. Do not create `castler-waiting` records.
These are presentation identifiers, not persistent allocator piece identities.
Royal/common construction keeps its separate existing sequence.

Retain current army and exhausted-type-choice fallbacks **within** an allocated family.
Retain Pawn/pocket helpers and configured presentation behavior.
A genuinely empty or uninitialized catalogue produces an explicit error, not a deleted unit or another family.
Do not draw transient intermediate concrete types or add Sergeant generation.

Human promotion candidates come only from final active concrete piece types, subject to retained game-legal exclusions.
Reserve types remain in the separate reserve catalogue, not active rights.
No visited-family history, prior-snapshot union, or extra enabled-list filter supplies rights.
A final Queen does not grant absent final Minor/Major types merely because counters crossed their thresholds.
Losing a type from the current active roster can remove its human promotion right.
CPU promotion policy remains governed by [ADR0017](0017-separate-cpu-promotion-permissions.md).

## AP generation and reachability

Apply the same resolved preset during generation and supported reconstruction.
Preserve the full quota catalogue independently of proposal multiplicities.
Update composite Castler counts/material in pool budgets, support minima, removal guards, and obtainable totals.
Do not synthesize redundant parent Chessmen or Material grants.
Retain occupied-Pocket credit in the unit guard and the precedence of validated tracker totals.
Obtainable counts include the pool, starting inventory, and locked-location grants.
Use Material400 consistently in pool arithmetic, starting/locked grants, projection, ledgers, slot export, and tracker reconstruction.

AP metrics must remain monotonic across effective item-count increases despite stateless roster redistribution.
Use fixed policy/seeds/stage and canonical protection-enabled projection.
Retain the bounded exact-envelope architecture.
Within 10,000 cells, evaluate the complete capped future-count rectangle and take componentwise suffix minima.
The metric is the minimum exact active material over componentwise greater-or-equal represented states, not a historical maximum.
The axes are Chessmen, Material items, Castler, and Progressive Consul.

Freeze complete obtainable upper bounds, including supported item-link effects.
Clamp bounds to contract caps and validate effective counts against them.
Unexpected effective counts beyond those bounds are explicit contract errors, not silent bound widening.
Raw capped excess retains normal handling.
Do not switch evaluation mode after receipts.
Keep caches scoped to fixed policy, seeds, stage, and bounds.

For larger spaces, the initial conservative count floor is:

```text
100 * min(S + K + effective_Consuls, stage.non_pawn_capacity)
```

Retain separate King Promotion and pocket-credit contributions.
Protected support uses the funded count `P` and the retained safe home-rank-slot bound.
Receipt alone, unfunded claims, or unallocated Material cannot grant castling access or fictitious active strength.
Spatial and legal-move restrictions remain separate.

**Soundness is not sufficient.** Normal supported worlds must generate, fill, and reach their goals under all four presets.
Cover standard/all-six supported settings with paired seeds `1,7,42,4242`, plus existing multiplayer/item-link and accessibility regressions.
Exercise exact-envelope and large-bound paths, legal deployment, collection spheres, required Locations, and goal completion.
Also verify adjacent-count monotonicity and `conservative <= exact`.

A sound but unusably low floor does not meet this gate.
Refine an insufficient bound rather than relabeling an otherwise supported normal world as unsupported.
The threshold and initial floor are engineering starting points, not authority to weaken playable support.
Genuinely infeasible configurations need actionable errors identifying policy, stage, counts, requirements, exact capacity, and conservative mode/bounds.
Distinguish actual infeasibility from an insufficient conservative bound.
Do not secretly change enabled families, weights, grants, objectives, or access expectations.

## Contract, migration, and retirement

New slot data requires these resolved fields:

```json
{
  "material_policy_version": 1,
  "enabled_material_values": [1, 3, 5, 9],
  "material_distribution": "classic"
}
```

This excerpt supplements the existing world declaration.
Producer omissions use the declared defaults. Missing or malformed connected-world fields fail explicitly.
Diagnostics identify the field, offending value, and required condition.
Do not substitute a default palette, another preset, or an empty roster for invalid connected input.
Reject unsupported policy/preset IDs, Legacy itemization, retired action keys, duplicate JSON properties, and incompatible contracts.
Keep itemization fixed to Fundamental.

The contract hash binds both supply and W tables, grant values, resource caps, PRF namespace, role semantics, and final-type rights.
Canonical input includes normalized policy fields and the local protection flag.
Equivalent palette orderings and numeric duplicates produce identical canonical inputs and cache keys.
Retain the existing configuration scope for army/fairy and presentation inputs.
Keep offline/preconnection previews separate from connected-world validation.

Keep the C# default backend, existing sidecar selection points, Python package, transport, and release architecture.
Introduce no mandatory new backend, downloader, or policy service.
Update all live C#/Python consumers and shared fixtures together.
From the pinned baseline, reserve contract v4.0, policy1, sidecar protocol2, and semantic runtime0.2.0.
If identifiers are allocated concurrently, coordinate the next free equivalents without changing this decision.
Generate canonical resources/hashes with existing tools and update producer/client/package/build pins atomically.
The minimum client is the first shipping compatible build.
Future hashes, source commits, compatible-build metadata, and complete fixture outputs are implementation deliverables, not open product decisions.

Retire selectable Legacy itemization, old action dictionaries/priorities/ratios, family tickets/cascades, and new-runtime old-contract dispatch.
Remove live Legacy-only family-grant admission limits, without deleting the retained concrete-type variety preferences.
Replace live Fundamental dependencies before removing old planner/model imports.
Keep common quota, rejection, capacity, army/fairy, Pawn/pocket, royal, and reconstruction responsibilities.
Retain historical source/vectors where useful, without requiring old worlds to run in the new client.
Unrelated standalone chess modes are outside this AP decision.

## Required exact fixtures and delivery gate

The standard interpretation fixture is `C=[15,7,3,1]` on `[1,3,5,9]`.
It means `F=[8,4,2,1]`, 15 units, counter sum26, and 3870 normalized material.
Its 2370 upgrade budget is an arithmetic fixture, not fractional AP items or a random guarantee.
On the same palette, ten ordinary units and 800 additional material can legally produce six Pawns/four Minors or nine Pawns/one Queen.
Both outcomes spend 800. Neither outcome is mandatory for a preset.

The phase fixtures exclude royals, pockets, and placement.
Protection is enabled unless stated otherwise. `M` is normalized additional material:

| Palette | S/K/M | Final families | Unspent | Protected P |
| --- | --- | --- | ---: | ---: |
| `[1,3,7]` | 0/1/0 | Minor1 | 200 | 0 |
| `[1,3,7]` | 0/1/200 | Jack1 | 0 | 1 |
| `[1,3,7]` | 0/1/400 | Jack1 | 200 | 1 |
| `[1,3,7]` | 0/2/0 | Minor1, Jack1 | 0 | 1 |
| `[1,3,7]` | 0/2/400 | Jack2 | 0 | 2 |
| `[1,3,7]` | 1/1/0 | Minor2 | 0 | 0 |
| `[1,3,5]` | 0/1/0 | Major1 | 15 | 1 |
| `[1,3,5,7]` | 0/1/200 | Jack1 | 0 | 1 |
| `[1,3,5,7]` | 0/1/400 | Jack1 | 200 | 1 |
| `[1,3,5,7]` | 1/1/400 | Major2 | 30 | 1 |

The first row has no castling entitlement and needs **another 200**, not 400, to fund Jack700.
The 200-threshold rows create no new item denomination. A real Material receipt adds 400.
The last row spends on the ordinary unit before the protected postpass. Only one of its two Majors is protected.
These outputs are independent of positive preset weights and semantic seeds because the ordinary choices in these cases are forced.

Protection disabled on `[1,3,7]`, S/K/M=0/1/400, produces Jack1/200 totals with P=0 and unchanged grants.
Actual legal castling is not asserted by these allocation fixtures.
Historical reduced-palette outputs remain in the optional appendix, not valid new-seed acceptance or compatibility requirements.
Do not relabel an old fixture with a different enabled palette.

Under Classic and root `1|2|3|4|5`, ordinary W must produce:

| Palette | N/B | Final counts in palette order | Unspent | Draws |
| --- | --- | --- | ---: | ---: |
| `[1,3,5,9]` | 15/2370 | 7/4/4/0 | 30 | 12 |
| `[1,3,5,7,9,13]` | 15/2370 | 7/5/2/1/0/0 | 0 | 12 |
| `[1,3,5,9]` | 0/800 | 0/0/0/0 | 800 | 0 |

For the stated namespace, root `0|0|0|0|0`, counter0, the first eight digest bytes are `7a7f3e7f62132d6c`.
The top-53 integer is `4309981695066725`.

Delivery also requires complete cross-language active/reserve, material, concrete-type, promotion, placement, and castling outputs.
Cover all 12 legal palettes, rejected palettes, retained variety behavior, resource caps/removal, 109-unit ownership, draw consumption, and Pawn-supply invariance.
Freeze concrete/coordinate goldens after wiring retained choosers and the declared ordinary namespace.
Preserve historical vectors unchanged.
No implementation is complete before the full generation/fill/reachability gate passes.

## Authority and supporting evidence

The user selected the policy, weights, grants, phase behavior, final-family scope, and new-contract-only retirement during the 2026-09-19/20 design.
The final scope decision was: "Finish this ADR; design variety modes and quantity limits in a follow-up."
The deterministic namespaces, schema, backend boundaries, and release procedure make those decisions a concrete implementation contract.
They do not leave routine implementation outputs as further product decisions.

Optional alternatives, measurements, and pinned source pointers are in [the evidence appendix](../planning/material-policy-design-evidence.md).
The normative rules and exact fixtures in this ADR do not depend on that appendix or session artifacts.
