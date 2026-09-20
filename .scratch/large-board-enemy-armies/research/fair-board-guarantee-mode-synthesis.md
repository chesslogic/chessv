# Fair-board guarantee: mode comparison

Date: 2026-09-18.
Status: evidence synthesis, not a policy decision.

Decision follow-up: Q44 selects retention of Legacy pawn-material
conversion.
[ADR 0022](../../../docs/adr/0022-retain-legacy-pawn-material-conversion.md)
records that choice.
The unresolved-rule discussion describes the evidence available before
the decision and does not supply the missing rule.

Q56 follow-up recovered the original July 4 user answer.
It describes best-effort Pawn/non-Pawn composition for `standard_and_pawns`,
not merely counting found Pawns toward the same total.
The [prompt-intent report](fair-board-guarantee-prompt-intent.md)
supersedes the earlier source-description-only interpretation.
Fundamental was already conceived and implemented client-side before that answer.
The July 19 date below is its producer introduction, not its first conception.

Current-authority update, 2026-09-19:
[Q59](../issues/24-specify-settings-support-and-retirement.md#q59-total-unit-and-non-pawn-minimums)
selects total-unit and non-Pawn minimums, with no separate Pawn minimum or implied ceiling.
An all-Minor army can satisfy both minimums without fallback.
This current decision supersedes treating the recovered example as mandatory Pawn retention.

## Evidence

The ChecksMate research examined source at
`0772bac724ff483043b56979f0ef078b9c2d264a`.
Its full report remains in the retained child-session artifact:
`C:\Users\aaedi\.copilot\session-state\0e307ab2-83cc-498a-9ba7-57e56973d013\files\fair-board-guarantee-producer-findings.md`.

The [client lineage](fair-board-guarantee-client-lineage.md) separately
records the historical executable SuperMax rule.
Both investigations use source evidence.
Neither ran generation, tests, or live servers.

The current producer/projector uses contract v3.
It is evidence, not the accepted future v4 authority.
In particular, its ten-rank capacity values must not replace the approved
v4 values.

## The meaningful distinction

Historical SuperMax reduced the pawn-slot reservation while preserving the
pawn material budget.
That permitted fewer, stronger pawn-family units.
It counted received non-pawn items, not post-upgrade active units.

Current Legacy semantic planning creates one pawn for every effective Pawn
item after non-pawn planning.
It never spends those Pawn items' material on upgrades.
Current Fundamental creates one owned slot for every effective Chessmen
item, then spends separate Material on upgrades.
Its upgrades retain those owned slots, but can change all their families.

Thus the old conversion policy is not equivalent to either current
invariant.
An absent consumer does not establish policy redundancy.

| Meaning | Legacy evidence | Fundamental evidence |
| --- | --- | --- |
| Historical release and conversion of pawn material | Meaningful but absent from the current semantic planner | No direct translation. Slot grants and upgrade Material are separate |
| Preserve owned Chessmen slots | Not a universal non-pawn invariant because configured merges can combine units | Already supplied for Chessmen slots |
| Keep some pawn-family units | Current Pawn grants remain pawns, without guarantee-specific conversion | Not supplied. Every pawn family can graduate |
| Preserve active unit count | Received counts do not establish the active roster | Not supplied by owned-count preservation alone |
| Make the eventual item pool affordable for AP requirements | Existing generator protections, conditional on accessibility | Existing protections and minimum plans, conditional on accessibility |

The last row concerns pool construction.
It is not a runtime reservation rule at each received-inventory prefix.
None of these paths consumes `fair_board_guarantee`.
Values 0, 1, and 2 only change its serialized world value in the examined
paths.

Sources under `worlds\checksmate`:
`apmw_projection\legacy.py:63-173`,
`apmw_projection\fundamental.py:70-217,261-369`,
`apmw_projection\placement.py:175-245`,
`item_removal.py:149-254`,
`item_pool.py:311-441`.

## Fixed capacity counterexample

This static case distinguishes owned count from active count.
It does not select a new guarantee threshold.

Both snapshots use current-v3 Fundamental/Stable on 8x8.
The baseline is 6x8 with one Board Files unlock and no Board Ranks unlock.
Seeds are pocket 101, pawn 202, minor 303, major 404, and queen 505.
Chessmen count is 16.
The sole explicit projector preference is `pawn-to-minor`, with priority 1
and proportion 1.
All other item counts are zero, including Castler, Consul, Pocket, King
Promotion, Forwardness, and Play as White.

Only Material count changes.
Each Material supplies 400.
Each pawn-to-minor transition spends 200 to change expected value from 100
to 300.
The fixed primary royal contributes no material.

| Aggregate output | Material 0 | Material 8 |
| --- | --- | --- |
| Starting upgrade budget | 0 | 3200 |
| Owned non-primary units | 16 Pawns | 16 Minors |
| Active non-primary units | 16 Pawns | 15 Minors |
| Reserve units | 0 | 1 Minor |
| Exact active material | 1600 | 4500 |
| Reserve granted material | 0 | 300 |
| Normalized grant and total accounted material | 1600 | 4800 |
| Dormant and unallocated material | 0 | 0 |

All three world guarantee values produce these same current results.
The flag is not a projector input.
The 8x8 non-pawn capacity is 15 and gross pawn capacity is 32.
Those capacities also agree with the future v4 8x8 profile.

The output boundary is aggregate counts and accounting, not exact source
IDs or board coordinates.
The full report's fixture E gives the remaining aggregate fields.
This is not a runtime result or evidence that a particular proposed floor
would fail.

## Source details that constrain the decision

The validated names `new-pawn`, `more-pawn`, `better-pawn`, and
`pool-pawn-upgrade` do not imply scheduled actions in either current
semantic planner.
Legacy also does not schedule the pawn-to-minor or pawn-to-major gateways.
Sources: `apmw_projection\planning.py:64-87`,
`apmw_projection\legacy.py:34-41,202-212`,
`apmw_projection\fundamental.py:39-48,301-311`.

Legacy non-pawn merges can also change count and normalize item credit.
One Minor plus one Major can become one Major-valued slot.
Its raw item-table credit is 785, but its normalized roster grant and
accounted material are both 485.
This describes the current normalization, not an asserted accounting bug.
It reinforces the distinction between received items and deployed units.

## Decisions and remaining rule

Q44 retains Legacy pawn-reservation release and material conversion.
Later decisions specify its count basis, geometry targets, funding, and
approximate spending.
The final mode algorithms remain open.
The recovered July 4 answer gives a distinct composition purpose relevant
to Fundamental without discarding its owned-slot model.

Q49 now selects the geometry targets.
No recovered source supplies a complete `standard_and_pawns` equation.
Human capacity, CPU starting counts, owned slots, active pieces, and AP
reachability estimates are not interchangeable.
The later fixtures must identify exactly which count the policy uses.

No retention, removal, new projector field, or human entitlement change
follows from this report alone.
