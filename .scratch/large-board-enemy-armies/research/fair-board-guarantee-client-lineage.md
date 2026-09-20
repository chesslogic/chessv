# Client lineage of the fair-board guarantee

Date: 2026-09-18.
Scope: source and history evidence, not runtime validation.
ChessV baseline: `d14300c0b5e6091fa57dda2cf073ead9b5fc5ad0`.

Decision follow-up:
[ADR 0022](../../../docs/adr/0022-retain-legacy-pawn-material-conversion.md)
retains Legacy conversion and selects the new stage targets.
The historical helper below is evidence, not the complete new rule.

## Historical policy

ChessV commit `db60c573076b340a346eefab7a429b210ec3dafe`, dated 2026-06-24,
introduced the SuperMax pawn-upgrade mode.
Its commit message describes a lower pawn-slot guarantee based on board
width and found non-pawn counts.
The pawn material budget remains available for upgrades.

The current historical helper retains this exact calculation:

```text
target = numFiles == 10 ? 19 : 15
nonPawnCount = foundConsuls + foundJacks + foundMajors + foundMinors
pawnGuarantee = min(foundPawns, max(0, target - nonPawnCount))
```

Source: `APMW.Client\ItemGeneration.cs:1643-1646`.
The helper does not count the primary King.
It counts received categories, not every concrete piece after an upgrade.
The same method uses 15 for every width other than 10.
That fallback is historical behavior, not a selected rule for new geometries.

The subsequent picker is budget-based:
`PickPawns` adds a selected unit, subtracts its material value, and
continues while budget and space remain.
The parameter named `foundPawns` receives the computed reservation in this
call path, not necessarily the raw item count.
`PigeonholeAllowsSergeant` protects material for the remaining reserved
slots before it permits a stronger unit.
Sources: `ItemGeneration.cs:1465-1477,1544-1589,1695`.

`GeneratePawns` retains `foundPawns` as the pawn material count.
It passes the reduced guarantee separately to the pawn allocator.
Sources: `ItemGeneration.cs:1649-1663,1689-1698`.
Thus a smaller guarantee permits fewer, stronger pawn-family units without
reducing the original pawn material count.
It does not itself guarantee a particular final piece quality or exact
number of deployed pieces.

## Fixed evidence cases

The existing characterization fixture supplies this input:

| Field | Value |
| --- | --- |
| Geometry | Standard 8-file board |
| Pawn mode | Vanilla |
| Upgrade mode | Historical SuperMax |
| Pawn items | 32 |
| Minor items | 8 |
| Major items | 4 |
| Jack items | 2 |
| Consul items | 1 |
| Major-to-Queen items | 0 |
| Count toward the helper's non-pawn total | 15 |
| Helper's target | 15 |
| Calculated pawn guarantee | 0 |

The fixture expects some Sergeant/Odin Pawn upgrades and fewer pawn-family
units than Pawn items.
Source:
`APMW.Test\ApmwItemHandlerCharacterizationTests.cs:399-444`.
This investigation read that expectation but did not run it.

A separate arithmetic case changes only `numFiles` to 10.
The helper's target becomes 19 and its guarantee becomes 4.
This is a direct consequence of the helper, not an executed roster fixture.
It does not select a target for 10x10 or 12x10.

For the later Q50 clarification, change the inputs to seven other counted
non-pawns and eight Pawn items on eight files.
The helper returns eight reserved pawn-allocation units.
It does not permit their replacement by only four units.
That hypothetical replacement was not a current path or a Fundamental
operation.

## Current interpretation boundaries

### Concrete choices and bounded picker traces

The current APMW catalog supplies these MidgameValue costs:

| Choice | Cost | Existing pool |
| --- | ---: | --- |
| Pawn | 100 | Ordinary |
| Berolina Pawn | 85 | Ordinary |
| Checkers | 40 | Ordinary |
| Sergeant | 200 | Stronger |
| Odin Pawn | 150 | Stronger |

Source: `ChessV.Games\MiscellaneousGames\ApmwPieceCatalog.cs:24-32,66-67`.
Vanilla selects ordinary Pawn for its baseline and Sergeant for its stronger
route.
Sources: `ItemGeneration.cs:1398-1410,1425-1432`.
These costs are not replacements for the shared human expected-material map.

Two static direct-picker cases use Vanilla, better-before-more, more enabled,
no preferred pool route, and exactly 800 supplied budget.
The prepared board is 8x8, with target 15 and no pockets.
The fixed primary royal does not contribute.
The other units are already deployed Minor-source units with no later
replacement.
They occupy seven back-rank positions, then available mixed-rank positions.
No spare credit or 45-unit allowance enters either direct call.

| Other qualifying units | Reservation | Available pawn positions | Result |
| --- | ---: | ---: | --- |
| 15 | 0 | 24 | Four Sergeants at 200 each. Remaining budget is zero |
| 7 | 8 | 32 | Eight ordinary Pawns at 100 each. Every stronger candidate fails the reservation guard |

These are supplied-input `PickPawns` traces, not default `GeneratePawns`
results or executed tests.
The first demonstrates real consolidation without inventing a Minor merge.
The second explains why Q50's four-unit alternative is not valid with
seven other qualifying units.

Historical accounting is not a hard affordability bound.
With reservation zero and a supplied budget of 845, the same picker emits
four Sergeants and one ordinary Pawn.
It spends 900 and ends with a remainder of -55.
The ordinary fallback requires positive budget, not enough budget for its
chosen piece.
Post-loop replacement can also overspend.
Sources: `ItemGeneration.cs:1465-1479,1520-1529,1574-1579,1595-1640`.

### Other Legacy material in the historical budget

The full generator uses `max(100*P, 100*P + spareMaterial + 45)`.
The Legacy allocation starts with zero `spareMaterial`.
Non-pawn generation changes that value before pawn generation.
Sources: `ItemGeneration.cs:370-390,1319-1358,1688-1695`.

`RecordPromotion` adds expected value minus concrete MidgameValue.
Substitution first refunds the previous concrete value.
Unused upgrades and unplaced direct pieces also add credit.
Sources: `ItemGeneration.cs:106-137,1789,1846`.
Thus this accumulator mixes concrete-value differences with unused credit.
It is not an identified balance in the modern normalized grant ledger.

The picker changes a local list, remaining budget, and RNG.
It has no grant-ledger interface.
This does not establish that every outer roster path lacks accounting.
Restored surplus funding needs explicit ownership and transfer rules.
A reserve grant cannot also fund active pawn allocation.
Q44 does not select the old 45 allowance, overspending, or removal of
cross-family funding.

### Historical rounding can exceed one Pawn

This static local-picker trace refutes a universal 100-centipawn bound.
It is not an executed game or a claim about every preset.

| Fixed input | Value |
| --- | --- |
| Funded Pawn material | Two Pawn credits, 200 |
| Legacy surplus | Zero |
| Supplied picker budget | 245, including the historical 45 allowance |
| Reservation and space | Two protected units, 32 available positions |
| Pawn profile | Checkers, ordinary option cost 40 |
| Enabled route | Better Pawn |
| Disabled routes | More Pawn and Pool Pawn Upgrade |
| Stronger-choice draws | Sergeant at 200 whenever selected |

An enabled action is preferred before a disabled action.
Source: `APMW.Client\Config.cs:263-273`.
Thus Better Pawn precedes the disabled More Pawn route.

| Step | Choice or operation | Remaining budget |
| --- | --- | ---: |
| 1 | Emit Sergeant. The remaining 45 covers one required Checkers at 40 | 45 |
| 2 | Reject an unaffordable Sergeant and emit Checkers | 5 |
| 3 | Stop emission because More Pawn is disabled | 5 |
| 4 | Refund Checkers at 40, then replace it with Sergeant at 200 | -155 |

The replacement pass requires only a positive remainder before the refund.
Its Best route has no ordinary choice at or above the Weak value of 75.
It therefore selects a stronger choice.
Sources: `ItemGeneration.cs:1432-1436,1595-1617`.

The final two Sergeants cost 400 against 200 funded credit.
The excess is 200, not 100.
Both protected units still exist, so count protection does not prevent
this material overspend.
A future fixed allowance must cover emission and replacement together.

### Current semantic boundary

`Config.cs:276-278` enables this helper through the historical SuperMax enum.
Current-contract upgrade value 3 means Configure.
The same value under legacy parsing means SuperMax.
Source: `Config.cs:391-449`.
This mapping does not read the standalone `fair_board_guarantee` field.

The owned-roster path has a different initial count invariant.
Legacy creates one initial pawn slot per received Pawn.
Fundamental creates one initial pawn slot per received Chessman before
graduation.
Sources: `OwnedRosterGeneration.cs:431-477,480-521`.
These owned counts are not proof that the old reservation policy is
equivalent, implemented, or unnecessary.
Projection can also distinguish owned units from active deployment.

### Pool selection needs a complete reservation guard

The stronger-choice guard alone does not prove count protection for Mixed Pool.
`ApplyPoolPawnUpgradeAction` returns an ordinary candidate without the guard.
Source: `APMW.Client\ItemGeneration.cs:1544-1559`.

This static picker fixture uses seven Pawn credits, zero surplus, budget 745,
reservation seven, and 31 available pawn positions.
Its ordinary choices are Pawn 100, Berolina Pawn 85, and Checkers 40.
Its stronger choices are Sergeant 200 and Odin Pawn 150.
Pool is preferred before Better and More.
The controlled draws select two Sergeants, then four ordinary Pawns.
There are no other choices or replacements.

Both Sergeant admissions satisfy the existing guard.
After the second, the budget is 345 and five protected units remain.
The guard reserves only their cheapest completion cost of 200.
Four unguarded ordinary Pawn draws then reduce the budget to -55.
The picker stops with six units rather than the required seven.
The replacement pass has no positive budget and changes nothing.

For the selected 8x8 target of 15, eight other deployed units and no pockets
make that shortfall significant.
The resulting usable count is 14.
Those eight other units can occupy seven back-rank and one mixed-rank positions.
Space does not cause this fixture's failure.

This is a controlled source trace, not an executed game or a frequency claim.
The future guarantee needs a completion guard for ordinary candidates too.
The fix must preserve Q53's approximation after count protection is satisfied.
Count protection and a strict spending cap are different requirements.

The preceding producer history remains relevant:

| Date | Repository and commit | Recorded change |
| --- | --- | --- |
| 2026-06-24 | ChessV `db60c573076b340a346eefab7a429b210ec3dafe` | Introduced the executable SuperMax reservation rule |
| 2026-07-04 | ChecksMate `6fbf68628aa71f51b1803a49536ca63b89f8d02f` | Extracted the advertised guarantee into a standalone option and removed the named Super Max choice |
| 2026-07-19 | ChecksMate `17df6adc6d89af7b88bf2bcd3002c4e66defd535` | Added Fundamental itemization |

## Claim boundaries

| Claim | Class | Excluded interpretation |
| --- | --- | --- |
| SuperMax lowered a pawn-count reservation without lowering its pawn material count | Source evidence | A guarantee of final material quality or a newly granted material budget |
| The standalone option describes a replacement for that policy | Documented historical intent | A recovered user instruction to preserve every old formula |
| The current owned-roster paths start from one slot per relevant item | Source evidence | Proof of equivalent active deployment or correct standalone-option behavior |
| Q49 selects targets 11/15/19/27/33 from CPU starting count minus its primary royal | Author decision | Carrying the old width-only 15/19 fallback forward |
| `standard_and_pawns` has an exact new formula | Open question, not established by this client helper | Subtracting Pawn items a second time without explicit authority |
| Retain Legacy pawn-material conversion | Author decision, Q44 | Treating an absent consumer as a removal decision; the exact control meanings still need specification |

The ChecksMate research stream owns the producer/projector comparison.
Its result must distinguish physical capacity, owned/deployed unit counts,
and Location reachability estimates.
This note does not choose a new human-itemization policy.
