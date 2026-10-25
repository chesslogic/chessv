# Retain Legacy pawn-material conversion

Status: accepted funding and mode boundaries; conflicting selection procedures superseded by ADR0024
Date: 2026-09-18
Reconciliation date: 2026-10-25

## Current decision and supersession

Ticket 24's **D22 closure round** settles the retained funding and composition policy summarized here.
[ADR0024's current decision](0024-seeded-hierarchical-allocation.md#current-decision)
records the October 21 guarantee-first successor.
It replaces incompatible selection procedures, not the retained Legacy/Fundamental funding laws.
Both modes remain supported.
ADR0024's historical September retirement of Legacy does not control the successor.
Its later Fundamental amendments permit post-prefix upgrades or additional Pawns from remaining material.
That is not a new receipt or permission to spend protected funds.
The exact optional-selection and funding composition remains open.

Under the October 21 baseline, preferred final-slot assignments precede affordability.
The allocator promises the longest affordable prefix that permits mandatory completion, then makes one finite forward optional pass.
Exact funding qualifies under the lawful mode-specific charges.
Castlers arise through normal selection with protected funding and compatible capacity for the remaining feasible shortfall.
Ordinary promised pieces must be active, not in reserve.

The historical action schedules, repeat-until-affordable stopping rules, and general recovery search are not required successor algorithms.
Source protections, hard bans, actual tokens, lawful releases, composition minimums, and truthful shortages remain.
The [current frontier](../../.scratch/adr-implementation-design/engineering-closure.md#current-authority-and-frontier)
separates these decisions from unresolved engineering and the separate board-value proposal.

Ticket question numbers and ADR question numbers differ.
The named decision and its document anchor identify the authority.
Early open statements in this ADR describe the decision history, not the current frontier.

| Current matter | Ticket 24 authority | ADR authority |
| --- | --- | --- |
| Modes, reservation, post-cap viability, spending stop | Q61, Q63, Q65, Q66 | Current rules in this section |
| Prepared target pairs and family membership | Q62, Q64 | Q60, Q61 |
| Final pocket families, overflow, and material credit | Q67/Q78, Q68 frontier | Q62/Q72, Q63 |
| Integrated recovery and effective instructions | Q69-Q73 | Q64-Q68 |
| Optional future identity, retirement, staged limits | Q75-Q77 | Q69-Q71 |
| Local controls, DeathLink, recent connections | Q79-Q82 | Q73-Q76 |
| Conservative bounds and resilience | Q83 | Engineering closure obligations |
| Soft-zero versus hard-disabled actions | Closure-r2 user decision | Funded-quality section |
| Paid Fundamental Pawn quality and released-funding refunds | Closure-r2 user decision | Funded-quality section |

`fair_board_guarantee` retains `none`, `standard_count`, and `standard_and_pawns`.
`none` applies no Legacy reservation or conversion.
Legacy then creates one Pawn per effective Pawn item.
`standard_count` applies the total-unit minimum.
`standard_and_pawns` applies both the total-unit and non-Pawn minimums.
Fundamental preserves its grant-backed Chessmen tokens.
Material never creates additional received tokens.
The later Fundamental surplus decision permits derived Pawns after the prefix, subject to funding and placement.
Its non-Pawn obligation can prioritize permitted Pawn-to-Minor-or-better actions.

The closure-r2 funded-quality decision also applies to existing Fundamental slots.
It does not turn Material into additional Chessmen.

For Legacy, let T be the prepared total target.
D counts actually deployable units from non-Pawn items.
P counts supported actual pocket families under Q61.
The reservation is `R = min(availablePawnItems, max(0, T - D - P))`.
Each reserved unit protects 100 material.
The allocation shows a shortage instead of inventing inventory.

Viability uses existing core reachability with frozen, post-cap requirements.
No second pre-cap strength gate applies.
The October 21 successor selects one finite optional pass after the guaranteed prefix.
The later Fundamental pool-elimination and surplus-upgrade direction needs the explicit composition recorded in ADR0024.
The retained historical approximation rules still apply.
The transfer ledger must distinguish ordinary affordability from their explicit allowance and overdraw.

Policy selection does not prove the allocator, conservative metric, or wire contract.
The [engineering closure register](../../.scratch/adr-implementation-design/engineering-closure.md)
owns those open proofs and their implementation owners.
No production implementation or S0 declaration is authorized by this reconciliation.

Q44 selects pawn-material conversion for Legacy in 0.4.0.
The user chose: "Retain pawn-material conversion for Legacy; specify its
rule next (Recommended)".
The missing consumer for the newer guarantee option is not a reason to
remove this capability.

The selected effect permits fewer, stronger units from Legacy Pawn material
after the pawn reservation permits conversion.
Current semantic planning instead creates one pawn per effective Pawn item.
The later implementation must add the selected behavior rather than
declare the existing one-unit-per-item path equivalent.

## Historical decision record

The named rounds that follow preserve their original questions, answers, and examples.
An old open question or scheduling prescription does not override the current decision.

### Historical scope and unresolved rule

This section records the Q44 frontier.
The current decision section and later named decisions supersede its open policy questions.

The decision selects a capability, not the old SuperMax enum or its 15/19
constants.
Detailed count membership, conversion actions, and the exact distinction
between guarantee choices remain open.
The rule must preserve the intended material budget and explicit accounting.
It cannot silently discard material or invent extra grants.

Fundamental applicability remains a separate decision.
This ADR does not consume Fundamental Chessmen credit or change its
separate slot and Material model.
Only Q49's explicit target relationship uses CPU army counts.
CPU catalog values do not define the human material budget.

This is a bounded addition to human progression after the Q35 design.
Its rule, wire representation, semantic identification, and effect on generator
logic need an explicit contract update before implementation.
It does not authorize a general human-itemization redesign or production
changes.

## Q46: Deployment and pocket contribution

The user selected "only what we can actually deploy plus what's in the
pocket".
The guarantee must use the deployable contribution and actual pocket
contents, not received-item totals or undeployed board reserves.
Pocket progression items are not themselves a count of pocket pieces.

This settles the count basis, not a fixed capacity or reservation target.
Q47 retains the existing formation record after the user clarified the
additional mixed rank on ten-rank boards.
Eight-rank boards have one mixed human rank, and ten-rank boards have two.
Minors use the shared non-pawn placement limits.
Detailed pocket-piece eligibility and the final reservation equation remain
part of ticket 24.

## Q48: Prepared-board context

The user selected the board being prepared as the context for the
reservation target.
Do not apply the world's ending-board target to every earlier board.
The contribution comes from that prepared setup's actual deployment and
qualifying pocket contents.

Changing board geometry can change the pawn-material allocation even with
unchanged inventory.
This does not change the world's victory condition or original cost
snapshot.
Q49 supplies the numeric target.

## Q49: CPU-sized unit targets

The user selected targets 11, 15, 19, 27, and 33.
Each equals the prepared board's CPU starting unit count minus its primary
royal.
An additional CPU King counts as one unit.
This is an explicit count relationship, not a transfer of CPU material
prices or human promotion permissions.

| Prepared board | Target |
| --- | ---: |
| 6x8 | 11 |
| 8x8 | 15 |
| 10x8 | 19 |
| 10x10 | 27 |
| 12x10 | 33 |

The target concerns usable non-primary units, not that many pawns.
The selected deployable and pocket contributions reduce what the pawn
allocation must protect.
The target does not create missing inventory.
The alternative two-rank targets of 19 and 23 on the ten-rank boards are
not selected.
The final reservation equation and its interaction with conversions remain
to specify.

## Q51: Retain other Legacy surplus funding

The user selected:

> Retain funding from other Legacy surplus (Recommended)

The restored allocation is not limited to Pawn-item credit.
It also retains funding from eligible surplus in Legacy non-pawn generation.
Historical sources include concrete-value differences and unused piece or
upgrade credit.
The modern contract must define their accounting explicitly.

Transferred credit cannot remain available in its previous owner.
A reserve grant cannot also fund an active pawn-allocation unit.
The historical `spareMaterial` integer is not itself proof of a valid
modern transfer.
This decision does not change Fundamental funding or its owned Chessmen slots.

Q51 selects the funding scope, not historical overspending or the 45-point
allowance.
The spending rule and exact transfer accounting remain to specify.

## Q52: Negative surplus and approximate spending

Negative Legacy surplus contributes zero to the pawn budget.
It does not consume Pawn-item credit.
The user identified above-average concrete pieces as a valid source of
such deficits.

The user did not select the proposed hard funded-material cap.
They prefer the gameplay effect of approximate spending and invite a
better bounded rule.
They consider an extra Pawn or Pawn-to-Sergeant improvement, about 100
centipawns without difficulty scaling, reasonable.
Q53 supplies the allowance disposition.
The historical implementation does not itself prove a 100-point bound.

## Q53: Keep the historical approximation rules

The user selected:

> Keep the historical approximation rules

This rejects the proposed single shared 100-centipawn ceiling.
The old 45-point allowance, positive-remainder ordinary emission, and
approximate replacement rules remain part of the selected Legacy behavior.
Their combined excess can exceed 100 centipawns.
That excess must remain visible in the accounting.
It is not additional received inventory or evidence of a new entitlement.

Q52's zero floor applies to net Legacy surplus before the allowance.
A negative surplus does not reduce Pawn-item credit.
The fixed allowance does not scale with difficulty.
The implementation must not replace these rules with strict affordability
or per-piece 100-point caps.

This does not restore the old geometry targets or received-item count basis.
Q46-Q49 still govern those inputs.
The exact transfer ledger, guarantee membership, and portable algorithm
remain to specify.

## Q54: Count all actual pocket pieces

Each actual pocket piece counts as one unit, including Pawns and Pawn variants.
An empty pocket contributes zero.
Upgrade levels do not multiply this count.
This counts actual chessmen, not received Fundamental Chessmen or Pocket items.
There is no novel Pawn exclusion.

This settles pocket membership in the prepared setup's contribution.
The named guarantee modes and final reservation equation remain open.

## Q55: Preserve material through supersession

When a Legacy placement is superseded, its material remains part of the
funding calculation.
For the Minor-plus-Major example, the funding base remains 785, not 485.
The resulting Major's shared expected value remains 485.
A concrete Rook at 500 leaves 285 before netting with other eligible sources.

This explicitly revises normalization that removes the consumed source's credit.
The contract must preserve the provenance of all involved eligible Legacy credit.
It does not restore the consumed non-pawn slot or grant the same material twice.
It does not change Fundamental itemization or the human expected-value map.

The user suspects that this particular case mainly concerns crowded boards
at low difficulty.
That is a source question, not a restriction on the selected material rule.
The minimal configured projector fixture does not require either condition.
It does not establish frequency under ordinary world defaults.

Normal Major-to-Queen progression already preserves its involved credit:
485 from the Major and 415 from its upgrade produce 900.
The final Queen's expected value is also 900.
Q55 preserves input grants, not a sum that counts final-family values as
additional upgrade grants.

## Q56: Recovered composition intent, unresolved algorithm

Q56's proposed collapse to one guarantee plus Off was not selected.
The user requested original prompt intent before changing the modes.
The recovered July 4 answer explicitly names Fundamental and gives
`standard_and_pawns` a best-effort Pawn/non-Pawn composition purpose.
Its example is eight Pawns and seven other pieces, with Sergeants allowed.

The later producer wording about counting found Pawns is not equivalent.
It must not replace the recovered purpose as author intent.
No recovered instruction disables Fundamental's one-slot-per-Chessman entitlement.
The basic owned count and optional composition constraints are separate.

This recovery does not select a new Fundamental algorithm, fixed quotas
for every geometry, or precedence over action preferences.
It also does not reverse Q44-Q55's Legacy decisions.
The [intent report](../../.scratch/large-board-enemy-armies/research/fair-board-guarantee-prompt-intent.md)
records the original answer, source-text drift, and remaining limits.

## Q58 and Q59: Best effort and composition minimums

The user selected best-effort preferences with a declared fallback and shared generator validation.
The exact viability criterion, relaxable preferences, order, and stopping rule remain open.
This does not authorize overriding explicit bans, owned-slot entitlements, or hard placement limits.

The user then selected minimums for total units and non-Pawn units.
There is no separate Pawn minimum and neither target is a ceiling.
With illustrative targets of 15 total units and 7 non-Pawns, 15 Minors satisfy both without fallback.
No Pawn retention is necessary to satisfy those predicates.
Additional available units are not removed to match the targets.

This current decision supersedes a required Pawn-component interpretation of the historical example.
The history remains evidence of the earlier composition purpose, not authority to override this clarification.
The exact numeric target pairs, membership, and interaction with other preferences remain to specify.
Fundamental still preserves its owned Chessmen slots.
Legacy still follows its selected funding and approximation rules.

## Q60: Prepared-board composition targets

The user adopted total-unit and non-Pawn minimums derived from the prepared
CPU army, excluding its primary royal.

| Prepared board | Total-unit minimum | Non-Pawn minimum |
| --- | ---: | ---: |
| 6x8 | 11 | 5 |
| 8x8 | 15 | 7 |
| 10x8 | 19 | 9 |
| 10x10 | 27 | 15 |
| 12x10 | 33 | 19 |

On 12x10, the additional King contributes to both minimums.
The primary royal contributes to neither minimum.
The targets are minimums, not ceilings.
They do not grant missing units or change the CPU formation.

## Q61: Human-unit and pocket membership

The primary King contributes to neither composition minimum and does not
provide pocket support.
Every other deployed human unit contributes to the total-unit minimum.
Deployed non-Pawns, including additional royals, also contribute to the
non-Pawn minimum.

Pocket contribution uses one global two-to-one support pool.
If B eligible non-primary chessmen are deployed on the board, at most
floor(B / 2) actual pocket pieces contribute.
When support is insufficient for every occupied pocket, non-Pawn pocket pieces
contribute before Pawn-family pocket pieces; order within each class follows
the canonical prepared-setup pocket order.
A contributing pocket non-Pawn counts toward both minimums.
A contributing Pawn or Pawn variant counts toward the total-unit minimum only.
Unsupported pocket pieces remain present but contribute to neither minimum.

## Q62 and Q72: Pocket distribution authority

The generator emits a suggested pocket distribution for generation and
accessibility analysis.
ChessV may replace that suggestion when it distributes the received Pocket
items into the prepared setup.
ChessV's captured final ordered pocket families control runtime composition
credit, pocket-material credit, and the displayed setup.
The final record identifies Pawn, Minor, Major, or Queen family, not the
concrete chessman, such as Phoenix or Rook.

Pocket items forced into the generated pool remain valid received inventory.
An excess over the ordinary configured capacity does not by itself reject the
world.
ChessV permits overflow beyond the configured ordinary limit, up to level four
in each existing pocket.
The tiers remain Pawn, Minor, Major, and Queen.
ChessV ignores items that would require level five or higher.
With three pockets, at most twelve received Pocket items affect the setup.
Ignored items remain in the received inventory but contribute no pocket unit
or material credit.
They do not cause generation failure.
ChessV does not create additional slots or move the excess to dormant inventory.
For each effective Pocket item, ChessV increments the currently lowest-level pocket.
Ties follow the generator's suggested slot order.
The same rule applies beyond the ordinary limit until every pocket reaches level four.

Jack and Amazon pocket tiers and selectable tier indexes remain future ideas.
They are not part of the selected contract.

A future contract can make the value or piece family at each pocket-tier index configurable.
An index means the upgrade level within a pocket, not a pocket slot.
The future mapping can omit the Pawn tier or include Jack and Amazon tiers.
This idea does not change the current four-tier mapping or authorize implementation.
Its defaults, allowed mappings, costs, and wire fields require a separate design decision.

The generator's accessibility estimate must remain conservative relative to
ChessV's final pocket distribution.
Regression fixtures must cover distribution differences, support thresholds,
and ignored surplus items.

## Q63: Pocket-material credit basis

Only supported pocket pieces receive pocket-material credit.
Let S be their semantic family material, B the exact active non-primary
on-board material after allocation, and C their count.
Pocket-material credit is `min(S, floor(B / 2), 300 * C)`.
The primary King and all pocket material are excluded from B.
This credit contributes only to the conservative reachability and viability
material metric.
It is not spendable conversion funding, exact active material, or pocket
inventory.

## Q64: Composition recovery and accessibility

The composition allocator searches or backtracks among permitted allocations
to satisfy the requested minimums and produce a configuration that passes the
existing core accessibility logic.
It does not weaken hard bans or invent inventory.
If no such allocation exists for a generation attempt, the existing generation
retry or failure path applies.
The APMW layer does not accept an inaccessible result or replace the core
accessibility validator.

Regression fixtures must force initially unsuitable Pawn/non-Pawn allocations
into this recovery path and prove that generation recovers to a configuration
accepted by the core validator.

## Q65: Ordered preference relaxation

Composition recovery may break the player's preferred family selection when
necessary; a selection such as Pawn plus Major is not an absolute family ban.
The recovery order is:

1. relax Pawn-count maxima and similar quantity targets;
2. relax ratios, weights, and enabled-action priority order;
3. relax preferred family eligibility.

The preferred army concept is therefore the last preference to give way.
Explicitly disabled actions, actual inventory, entitlements, physical
deployment capacity, and immutable contract limits remain hard.
Each relaxation and its reason must be visible in diagnostics.

## Q66: Integrated composition obligations

Composition minimums use integrated outstanding obligations rather than a
seed-first baseline or post-generation replacement.
Ordinary weighted generation continues while reserving enough material and
slots to keep the outstanding total-unit and non-Pawn obligations reachable.
An ordinary matching draw satisfies an obligation instead of adding a duplicate.
If obligations remain near completion, composition recovery and ordered
relaxation fulfill them.

This adopts the useful `locked_items` pattern without mechanically copying all
historical affordability, weighting, or terminal-append behavior.

## Q67: Fundamental reservation and downgrade recovery

In Fundamental mode, fixed Chessmen slots reserve enough capacity for the
outstanding non-Pawn obligation.
The allocator plans those slots together with the world's obtainable Material,
because final Pawn membership can become non-Pawn membership through upgrades.
It may need to backtrack a Material allocation to create another Chessman, or
backtrack a Chessman allocation to preserve enough upgrade material.

When a high-value plan leaves too few units or non-Pawns, recovery downgrades or
splits realized pieces until the obligations and core accessibility check can
both pass.
At each step, the world seed selects uniformly among realized pieces for which
a permitted downgrade or split makes measurable progress without violating a
hard limit.
Recovery applies the smallest progressive transformation, then recomputes the
outstanding obligations and accessibility.
This can cascade through permitted adjacent families from higher-value pieces
to lower-value pieces and, when necessary, to Pawns.

The user's configured distribution still governs ordinary generation.
Using that distribution to weight recovery candidates is an unselected
enhancement; the selected recovery sampling is seeded and uniform.

## Q68: Generator-owned effective allocation policy

The generator executes the seeded obligation and recovery algorithm.
It emits the effective instructions for allocating received Chessmen, Material,
Pocket items, and enabled armies.
Those instructions include the normalized upgrade order and other effective
settings needed by ChessV.
They do not describe a hypothetical completed roster.

Slot data contains the effective allocation policy, not allocation reasoning,
rejected draws, recovery traces, hypothetical family counts, concrete ChessV
piece names, or board coordinates.
Generation keeps its reasoning local and uses the existing core accessibility
validator before accepting the world.
ChessV applies the effective policy to the player's actual received inventory
and retains deterministic family-to-piece selection and placement.
For pockets, the generator may include a suggested distribution, but ChessV's
captured final distribution is authoritative at runtime.

If generation relaxes preferences under the ordered recovery policy, slot data
contains the recovered effective upgrade order and settings.
It does not contain the superseded requested values.
The original request and the reasons for recovery remain generator-local
diagnostic information.

## Q69: Optional future policy identity

The current decision does not require a new policy field.
A future contract may add an allocation-policy identifier when settings fields
alone no longer identify the semantics.
For that optional field, absence selects the default legacy behavior.
An explicit unknown value is rejected; it must not silently fall back.

## Q70: Retire Pool and Max

Contract v4 does not expose the deprecated Pool or Max pawn-upgrade modes.
Their old option values are rejected everywhere, including old option files.
They are not accepted as generation aliases and never appear in slot data.
Supported explicit upgrade settings replace them.

## Q71: Staged per-type limits

Ordinary generation enforces each configured piece-type limit.
If that pass cannot meet composition, difficulty, or accessibility requirements
with other available types, recovery may exceed the limit as a last-resort
overflow.
The initial series therefore respects the configured distribution before
overflow begins.

Overflow pieces are legitimate received inventory.
ChessV deploys or reserves them normally and never reapplies the generation
limit as a runtime discard, deactivation, or replacement rule.

## Q73: Local AI reduction

Retain the local Reduce AI Intelligence control.
Its selected level adds to the collected Progressive AI Intelligence Malus
count, and the effective total remains capped at five.
It changes the engine limit for the next match only.
It does not change world logic, generation, item counts, costs, previews, or an
active match.

The local selection persists across previews and matches in the same world.
It resets to default when the client switches worlds.

## Q74: Local setup-control timing

CPU family and Ignore Castlers changes affect the preview and the next match.
They never mutate an active match.
They persist across matches and reconnects to the same world.
They reset to their defaults when the client switches worlds.

DeathLink remains separate.
The world must authorize DeathLink before the local control can enable sending
or receiving it.

## Q75: Initialize DeathLink from the world setting

On connection, including reconnect, initialize the local DeathLink choice to
enabled when the world enables DeathLink.
When the world disables DeathLink, force the local choice off and disable the
control.
Do not reset an authorized world's DeathLink choice to disabled by default.
This replaces the unaccepted reset-to-disabled proposal.

Same-world local-control persistence does not by itself require saving settings
to disk.
Q82 in ticket 24 selects recent connection details separately.

## Q76: Recent connection details

The client remembers five recent successful connections across application restarts.
Each entry contains only host, port, and slot name.
It stores no password or world settings.
Selecting an entry fills the form without connecting.
This disk record does not persist same-world controls, slot data, or earned journals.
Its implementation owner is U1, including `APMW.Client\Convenience.cs`.

## Closure-r2: Funded Fundamental quality and action priorities

The user selected this bounded amendment during closure-r2 on 2026-10-01.
It supersedes earlier unresolved Fundamental applicability for these two quality actions.
Priority `0` is inactive in ordinary allocation.
Weights/order relaxation can activate that action.

Priority `-1` remains a hard ban through every recovery phase.

Fundamental `better-pawn` and `pool-pawn-upgrade` spend Material for stronger Pawn variants.
The initial funded Pawn value is 100.
Pawn150 costs 50 extra.
A later Pawn200 costs another 50.

Downgrade returns only the released extra Material funding.
The same owned Chessman remains one slot through these changes.
Neither stronger quality nor a refund creates another slot.

This selection does not enable paid quality automatically.
The coordinator separately instructed preservation of the current raw-empty/Off default.
That source default has four priority-1/unit-weight gates and disabled paid quality.
Base entitlement remains separate from extra Material funding.

Normalized twelve-action all-`-1` is hard-disabled policy, not the raw-empty default.
No AP raw-presence extension follows from this distinction.

The [engineering closure register](../../.scratch/adr-implementation-design/engineering-closure.md#closure-r2-quality-authority-and-pending-proof)

separates these user selections from candidate derived ledger equations.
The common/generator owners must reconcile literal fields, defaults, transition accounting and the conservative metric.
This amendment does not prove their algorithms or close S0 readiness.

## Sources

[Ticket 24](../../.scratch/large-board-enemy-armies/issues/24-specify-settings-support-and-retirement.md)
owns settings authority and the reconciliation matrix.
The [client lineage](../../.scratch/large-board-enemy-armies/research/fair-board-guarantee-client-lineage.md)
records the historical conversion.
The [mode synthesis](../../.scratch/large-board-enemy-armies/research/fair-board-guarantee-mode-synthesis.md)
distinguishes that conversion from current Legacy and Fundamental planning.
