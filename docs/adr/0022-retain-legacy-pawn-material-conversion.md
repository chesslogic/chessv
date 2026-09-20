# Retain Legacy pawn-material conversion

Status: accepted
Date: 2026-09-18

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

## Scope and unresolved rule

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
Its rule, wire representation, algorithm identity, and effect on generator
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

## Sources

[Ticket 24](../../.scratch/large-board-enemy-armies/issues/24-specify-settings-support-and-retirement.md)
owns the remaining settings decisions.
The [client lineage](../../.scratch/large-board-enemy-armies/research/fair-board-guarantee-client-lineage.md)
records the historical conversion.
The [mode synthesis](../../.scratch/large-board-enemy-armies/research/fair-board-guarantee-mode-synthesis.md)
distinguishes that conversion from current Legacy and Fundamental planning.
