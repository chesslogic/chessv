# Material policy: evidence and alternatives

> **Historical evidence for superseded ADR0018.**
> [ADR0019](../adr/0019-seeded-hierarchical-allocation.md) is the current implementation contract.
> The original rationale, normalized prices, W measurements, and phase fixtures below remain historical.
> They do not validate the successor's concrete-price hierarchical allocator.

This appendix supports [ADR0018](../adr/0018-enabled-material-families.md). It is not a second normative contract.
The ADR contains the required behavior and exact fixtures without a dependency on local research files.
Source evidence is pinned. The accepted decisions take precedence over old source behavior.

New worlds require 1, 3, and at least one of 5/7 globally. Queen and Amazon remain optional.
Reduced palettes in this appendix are historical isolated probes, not valid new-seed acceptance.
No automatic-Material omission policy or invalid-world fallback follows from them.
Modal variety and new overall quantity limits are deferred to a follow-up ADR.
The current soft per-type preferences remain in scope.

## Why this policy

Nominal families distinguish a family label from each concrete piece's evaluation.
Final-roster filtering prevents disabled families from surviving through unrelated old upgrade paths.
Cumulative threshold counts preserve the standard fixture without unit identities or per-piece source transfers.

Weighted legal-boundary draws replace finite/refilling bags.
One boundary contributes one fixed weight, independently of source multiplicity.
A finite increment-ticket bag can stop with an affordable upgrade still available.
Refill policy adds scale and stall behavior without solving a requested problem.
Neither bag belongs in the new runtime. Neither represents a bag of individual pieces.

The author selected fixed-family preferences, not palette-position curves, and soft biases with affordable spending.
At saturation, every ordinary unit can reach the highest enabled family.
The old one-boundary and Pawn-only probes describe fixed-input arithmetic, not valid new-world presets.

Pawn supply is a separate producer-stage control.
A fixed Material anchor prevents the sum of runtime weights from changing producer supply accidentally.
Doubling all five runtime weights preserves W.
A sum-derived Material anchor instead doubles raw Material supply relative to an unchanged Chessmen weight.
The ratified Pawn totals are 8/6/4/36. Earlier 8/4/6/16 proposals are historical, not accepted settings.
Horde36 means 16 plus20, not a 20-percent increase.

Castlers supply their own unit and 500 total material.
Pooled protected-first funding replaces the old reserved-existing-unit/extra-charge contract.
The author rejected forced waiting Pawns: units without funded protection join ordinary W.
On `[1,3,7]`, one Castler produces Minor300 plus 200 unspent, without castling entitlement.
An additional 200 reaches Jack700 after stateless reallocation of the same pooled resources.
The local Ignore control removes protection only, not grants.

Ordinary first-gateway placement roles and reconstructed intermediate promotion histories were rejected.
One material-first ordinary category and final-active-type human rights replace them.
CPU capture roles and CPU promotion policy remain separate.

## Bounded observations

The initial ordinary-only comparison contained 4,824 parameter rows and 617,472 allocations.
It covered five palettes, seven unit counts, affordability/saturation budgets, four experimental profiles, and three candidate algorithms.
Those measurements used a demo RNG and earlier unapproved tables.
They support the bag/spending rationale, not claims about current full-world distribution.

A bounded 120-table search changed the standard Classic/Mixed destination ratio from 7:3:1 to 8:4:3.
Those ratios refer to Minor/Major/Queen on `[1,3,5,9]`.
The separate semantic-PRF reference corpus gave mean standard counts 8.045/3.957/2.072/0.926 at N15/B2370.
Mean residue was 40.15. These are not guaranteed counts or a stop-to-preserve-shape rule.
Mixed is a historical name, not a current wire alias.

The revised Royals comparison used eight fixed-resource cases, three paired rows, and 1024 roots per case.
Roots used indices 129-1152 and `i|i+1|i+2|i+3|i+4` under `fundamental.cumulative.v1.draw`.
There were 24,576 corpus allocations and 128 narrow invariance replays.
Each comparison held normalized grants and seeds fixed. It did not simulate complete AP generation.
The historical Power destination row was 1/12/12/12/1 for Minor/Major/Jack/Queen/Amazon.
The ratified Royals row is 1/1/12/12/12. The current Classic row is 8/4/1/3/1.

| Fixed fixture / mean metric | Earlier Power (historical) | Ratified Royals W row | Classic |
| --- | ---: | ---: | ---: |
| Standard N10/B800: Queens | 0.852 | 0.458 | 0.096 |
| All six N15/B2370: Amazons | 0.715 | 0.958 | 0.036 |
| Same fixture: Queens | 1.187 | 0.604 | 0.235 |
| Same fixture: Jack/Queen/Amazon combined | 2.236 | 1.777 | 0.517 |
| All six N10/B3000: Amazons | 1.201 | 1.775 | 0.093 |
| Historical `[1,5,9]`, N10/B800: Queens | 0.504 | 0.928 | 0.437 |

Royals favors stronger final families without increasing Queens or upper-family unit counts in every comparison.
The old proposed mean-Queen threshold of 0.7 was never author authority and was not enforced.
The final Pawn-weight changes leave these conditional W observations unchanged.
The 2370/3000 budgets are arithmetic probes, not fractional Material-item claims.

Eleven targeted Castler phase fixtures conserved units/material and reproduced the selected Minor/Jack and protection-only cases.
Seven used the current `[1,3,7]` palette. Four preserved historical `[1,7]` arithmetic.
These fixtures do not establish board placement, legal castling moves, or whole-world viability.
Browser checks exercised stage separation, common-scale invariance, revised Royals, and historical Pawn-only arithmetic.
No production implementation or full-generation validation occurred during this design stage.

The validity revision accepted exactly 12 of the 64 family subsets.
Invalid palettes displayed an explicit AP-supply error without a fallback or Material-mask control.
The ordinary allocator and PRF source remained unchanged. Eight paired replays preserved the prior results.
These checks added no calibration or quantity-limit/variety redesign.

## Pinned source pointers

Client baseline: `chesslogic/chessv@d14300c0b5e6091fa57dda2cf073ead9b5fc5ad0`.
Producer baseline: `chesslogic/Archipelago@0772bac724ff483043b56979f0ef078b9c2d264a`, branch `checks_mate`.

| Source | Evidence and migration significance |
| --- | --- |
| [Client material constants](https://github.com/chesslogic/chessv/blob/d14300c0b5e6091fa57dda2cf073ead9b5fc5ad0/APMW.Client/ItemGeneration.cs#L9-L23) | Major485, separate Castler500, and Weak75 |
| [Client caps/backend](https://github.com/chesslogic/chessv/blob/d14300c0b5e6091fa57dda2cf073ead9b5fc5ad0/APMW.Client/ItemHandler.cs#L20-L45) | Per-item 107/321/2 caps and the C# default backend |
| [Client PRF/root](https://github.com/chesslogic/chessv/blob/d14300c0b5e6091fa57dda2cf073ead9b5fc5ad0/APMW.Client/OwnedRosterGeneration.cs#L247-L335) and [Python PRF](https://github.com/chesslogic/Archipelago/blob/0772bac724ff483043b56979f0ef078b9c2d264a/worlds/checksmate/apmw_projection/models.py#L274-L298) | Existing shared SHA-256 algorithm reused under a new namespace |
| [Concrete chooser](https://github.com/chesslogic/chessv/blob/d14300c0b5e6091fa57dda2cf073ead9b5fc5ad0/APMW.Client/OwnedRosterGeneration.cs#L368-L406) and [threshold grouping](https://github.com/chesslogic/chessv/blob/d14300c0b5e6091fa57dda2cf073ead9b5fc5ad0/APMW.Client/OwnedRosterGeneration.cs#L833-L842) | Retained soft per-type preference, exhausted-choice reuse, and separate family counters |
| [Client preference fields](https://github.com/chesslogic/chessv/blob/d14300c0b5e6091fa57dda2cf073ead9b5fc5ad0/APMW.Client/Config.cs#L371-L376) and [Pawn chooser](https://github.com/chesslogic/chessv/blob/d14300c0b5e6091fa57dda2cf073ead9b5fc5ad0/APMW.Client/OwnedRosterGeneration.cs#L766-L774) | Three by-type fields default to0. Pawn selection retains its existing helper. |
| [Army fallback](https://github.com/chesslogic/chessv/blob/d14300c0b5e6091fa57dda2cf073ead9b5fc5ad0/APMW.Client/ItemGeneration.cs#L2333-L2372) | Existing fallback stays inside the allocated nominal family |
| [Material-first sort](https://github.com/chesslogic/chessv/blob/d14300c0b5e6091fa57dda2cf073ead9b5fc5ad0/APMW.Client/ActiveRosterProjection.cs#L487-L507) | Reuse current intra-role order globally for ordinary non-Pawns |
| [Shared input capture](https://github.com/chesslogic/chessv/blob/d14300c0b5e6091fa57dda2cf073ead9b5fc5ad0/APMW.Client/ApmwSidecarSnapshot.cs#L266-L375) | Existing canonical input/cache boundary receives new policy fields and protection flag |
| [Old Ignore flag](https://github.com/chesslogic/chessv/blob/d14300c0b5e6091fa57dda2cf073ead9b5fc5ad0/ChessV.Base/ApmwCore.cs#L37-L47) | Suppressing effective protection must no longer suppress composite resource grants |
| [AP candidate multiplicities](https://github.com/chesslogic/Archipelago/blob/0772bac724ff483043b56979f0ef078b9c2d264a/worlds/checksmate/item_pool.py#L746-L761) | One base occurrence plus extras gives old totals Chessmen4/Material3/Castler1/Pocket3 |
| [AP quota/draw loop](https://github.com/chesslogic/Archipelago/blob/0772bac724ff483043b56979f0ef078b9c2d264a/worlds/checksmate/item_pool.py#L685-L744) | Quotas derive from candidate names. Success retains tickets, rejection can prune one, and `min_material` is unused in the loop. |
| [Unit reserve guard](https://github.com/chesslogic/Archipelago/blob/0772bac724ff483043b56979f0ef078b9c2d264a/worlds/checksmate/item_removal.py#L149-L167) | Composite Castler unit effects and occupied-Pocket credit must reach the same guard |
| [Suffix-minimum envelope](https://github.com/chesslogic/Archipelago/blob/0772bac724ff483043b56979f0ef078b9c2d264a/worlds/checksmate/logic_projection.py#L296-L357) | Exact future-state suffix minima and the old larger-space floor are separate from displayed snapshots |
| [Obtainable totals](https://github.com/chesslogic/Archipelago/blob/0772bac724ff483043b56979f0ef078b9c2d264a/worlds/checksmate/__init__.py#L268-L279) and [tracker reconstruction](https://github.com/chesslogic/Archipelago/blob/0772bac724ff483043b56979f0ef078b9c2d264a/worlds/checksmate/__init__.py#L364-L416) | Preserve pool/start/locked totals and validated tracker precedence |

The old source is not authority for new grant semantics or selected behavior.
Source constants do not prove new-world liveness.
The ADR's full generation, fill, and reachability gate remains mandatory for all four presets.

## Historical reduced-palette fixtures

These exact inputs and outputs are preserved without relabeling their palettes.
`[1]`, `[1,7]`, `[1,9]`, and `[1,5,9]` are invalid world palettes under ADR0018.
The new contract rejects them. Arithmetic probes do not require a live old-world reader.

| Historical palette | S/K/M | Exact isolated final families | Unspent | Protected P |
| --- | --- | --- | ---: | ---: |
| `[1,7]` | 0/1/0 | Pawn1 | 400 | 0 |
| `[1,7]` | 0/1/400 | Jack1 | 200 | 1 |
| `[1,7]` | 0/2/0 | Pawn1, Jack1 | 200 | 1 |
| `[1,7]` | 0/2/400 | Jack2 | 0 | 2 |

The current `[1,3,7]` one-Castler result is Minor300 plus 200 unspent.
Its two-Castler result is Jack700 plus Minor300, with zero unspent.
Adding one Material item to that two-Castler case produces two protected Jacks with zero unspent.
The 500-total grant is unchanged. The enabled palette and resulting allocations differ.

Under Classic and semantic root `1|2|3|4|5`, the historical ordinary-only cases were:

| Historical palette | N/B | Final counts in palette order | Unspent | Draws |
| --- | --- | --- | ---: | ---: |
| `[1,5,9]` | 10/800 | 8/2/0 | 30 | 2 |
| `[1,9]` | 10/799 | 10/0 | 799 | 0 |
| `[1,9]` | 10/800 | 9/1 | 0 | 1 |
| `[1]` | 10/800 | 10 | 800 | 0 |

These probes include the historical 385/415 costs on `[1,5,9]` and the 800-cost boundary on `[1,9]`.
They are not substitutes for the current valid-palette fixtures in the ADR.

## Historical limits versus retained variety

The user explicitly deferred modal variety and new overall quantity limits to a follow-up ADR.
Neither strict per-type enforcement nor a new family quota was selected.
The current positive thresholds and 0-as-unlimited behavior remain soft concrete-type variety controls.
The historical Python options do not establish a positive-only aggregate Minor/Major rule:

| Source scope | Historical behavior |
| --- | --- |
| Python option inputs | Minor-by-type has positive range1..15, Major-by-type1..11, Queen-by-type1..9, and aggregate Queen-upgrade limit1..9. All four also allow disabled0/default0. |
| Python Legacy admission | By-type allowances multiply selected-army type counts and include parent-upgrade headroom. Positive computed limits are hard family-token admission bounds, not roster minima. |
| Python Fundamental | Chessmen/Material/Castler have no family-cap mapping, and shared projection has no type-limit input fields. No Python combined Major/Jack cap exists. |
| Client concrete selection | Nonpositive values mean unlimited. Positive values prefer under-threshold types, with full-list reuse when exhausted. This is not a hard final-roster maximum. |

For example, FIDE Minor-by-type1 permits Knight1 and Bishop1 in the Legacy allowance.
It guarantees neither raw Minor receipt nor a surviving Minor in the final roster.
The aggregate Queen limit governs Legacy Queen-upgrade admission and is not exported to Fundamental shared projection.
Family availability, concrete-type variety, generator admission, and final-roster quantity are distinct concepts.

Pinned Python references: [option definitions](https://github.com/chesslogic/Archipelago/blob/0772bac724ff483043b56979f0ef078b9c2d264a/worlds/checksmate/options.py#L598-L670),
[declared mappings](https://github.com/chesslogic/Archipelago/blob/0772bac724ff483043b56979f0ef078b9c2d264a/worlds/checksmate/options.py#L729-L748),
[PieceModel limits](https://github.com/chesslogic/Archipelago/blob/0772bac724ff483043b56979f0ef078b9c2d264a/worlds/checksmate/piece_model.py#L47-L137),
and [generated maxima](https://github.com/chesslogic/Archipelago/blob/0772bac724ff483043b56979f0ef078b9c2d264a/worlds/checksmate/item_utils.py#L100-L145).
