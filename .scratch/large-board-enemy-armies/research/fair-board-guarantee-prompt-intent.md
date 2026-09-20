# Fair-board guarantee: recovered author intent

Date: 2026-09-18.
Status: source and prompt recovery, not a new mode decision.
ChessV baseline: `d14300c0b5e6091fa57dda2cf073ead9b5fc5ad0`.
Producer baseline: `0772bac724ff483043b56979f0ef078b9c2d264a`.

Current-authority update, 2026-09-19:
[Q59](../issues/24-specify-settings-support-and-retirement.md#q59-total-unit-and-non-pawn-minimums)
selects total-unit and non-Pawn minimums, with no separate Pawn minimum.
This supersedes using the historical example as a mandatory Pawn-component requirement.
The historical evidence and its original interpretation remain recorded here.
Q58 also selects the best-effort fallback direction.

## Question and finding

Q56 proposed collapsing the two named guarantee modes into one policy plus Off.
The user did not select that proposal.
They requested original comments and prompt intent, particularly for Fundamental.

The original answer gives `standard_and_pawns` a distinct purpose:
best-effort Pawn/non-Pawn composition, not merely a different count of received items.
It explicitly mentions Fundamental and permits Sergeants in the Pawn component.
The earlier interpretation missed that distinction.
There is no recovered instruction to disable Fundamental's Chessmen entitlement.

## Primary prompt sources

The exports reside under `C:\GitHub\gdd\prompts`.
Quoted answers below are user-authored, not choices inferred from recommended defaults.
Duplicate summary and full-entry copies count as one answer.

| Key | Stable source | Meaning |
| --- | --- | --- |
| P0 | `chessv__semantic__2026-04__part2.md:142-190`, session `2d2ee441-a8c1-4d21-9efb-615b010b3780`, entries 7-10 | Ordinary Pawn upgrades must protect granted Pawn-family count. More units are acceptable. A later answer corrects the comparison with Off |
| P1 | `chessv__cli__plan__2026-06.md:139-154`, session `d51c2bed-beda-4f74-81f3-876cba297483`, prompt 1 | SuperMax deliberately relaxes the Pawn-item count reservation once other units satisfy part of the Location need |
| P2 | `chessv__cli__plan__2026-07.md:127-141`, session `e5f6e6eb-bf72-4243-9fd2-bc5325e82b93`, prompt 1 | Fundamental separates one piece plus 100 material from a Material-only item |
| P3 | `chessv__ask_user.md:944-993`, same session, question 1, answered `2026-07-03T19:26:20Z` | Material defaults to 400. Castlers constrain existing Chessmen/material rather than adding independent units |
| P4 | `Archipelago__ask_user.md:157-195`, session `f0850d63-7c24-4328-94cd-c3a4d331c939`, question 2, answered `2026-07-04T21:02:39Z` | Three guarantee choices, with a distinct best-effort Pawn composition choice |
| P5 | `chessv__ask_user.md:1040-1074`, session `b80a1572-18af-4976-a053-d578d9503a8b`, question 2 | Pawn-quality unification questions received an unavailable-user response, not a selected slot/quality mechanism |
| P6 | `chessv__cli__plan__2026-07.md:282-308` and `chessv__ask_user.md:1543-1588`, session `61e5ff36-da4a-4ba8-ba6b-fcc8757c36c3` | Later geometry work retains shared-wave Fundamental and distinguishes owned roster from active deployment/reserves |

P1 is independently present in local session history, turn 0 at
`2026-06-21T17:49:36.742Z`.
P2 is present in local session history, turn 0 at `2026-07-03T19:18:41.785Z`.
The later SuperMax session repeats P1; it is not another independent decision.
Question-response timestamps come from the exports, not inferred filenames.

### Before: Pawn guarantees and SuperMax

P0 first stresses the importance of retaining granted chessmen for trades.
Its correction narrows that requirement to received Pawn items versus
placed Pawns plus Sergeants, not equality with Off's potentially larger roster.
It rejects deficits from expensive non-pawns reducing that Pawn count.

P1 later introduces an explicit exception: enough other units can reduce
the number of Pawn allocations that must be retained.
Its example uses five Pawn items but requires only three Pawn units.
The released 200 material can improve those remaining units.
This is a reservation/count trade-off, not an instruction to ignore all count.

P1's 15/19 targets are historical.
Q46-Q49 now control contribution, prepared geometry, and targets.

### Transition: the standalone option

P4 answers an agent warning that priority and ratio cannot express the
separate SuperMax count-reservation behavior.
The user wrote:

> Under the Fundamental generation rules we should expect a number of
> chessmen... but I get your point.

They then proposed `standard_count`, `standard_and_pawns`, and `none`.
The second choice would:

> try its best not just to limit to 15 chessmen but also to force e.g.
> 8 pawns and 7 pieces. (Depending on other priority settings the 8 pawns
> might be some or all sergeants...)

This is explicit composition intent.
It is not the subsequently written description that received Pawns also
count toward satisfying the same total.
The answer also rejects a significant difficulty effect from Pawn/priority settings.

The phrase "try its best" matters.
The example does not define shortage behavior, an exact cap versus target,
which actions yield to the guarantee, or extensions to other geometries.
It does not authorize eight free Pawns.
It does not explain `none` as removing the Chessmen item constraint.

Fundamental was already proposed and implemented client-side on July 3.
The July 19 producer introduction therefore cannot establish that the July 4
guarantee had no Fundamental purpose.
That earlier inference from producer commit order was incomplete.

## Source-text drift

The July 4 introducing producer commit is
`6fbf68628aa71f51b1803a49536ca63b89f8d02f`.
Its `Options.py:476-477` says that `standard_and_pawns` lets already found
Pawns count toward the standard board-location requirements.
The same wording survives the July 19 Fundamental producer transition.

The recovered answer describes preserving a Pawn component instead.
Treat the producer description as implementation/documentation drift,
not an equally authoritative alternative author decision.
Git author metadata alone does not establish authorship of that prose.
The delegated source review found no correcting guarantee-specific commit
comment or relevant PR discussion.
Both introducing commits associate with
[ArchipelagoMW/Archipelago#2507](https://github.com/ArchipelagoMW/Archipelago/pull/2507).
The bounded review read its six issue comments and nonempty review summaries,
and searched all 99 inline review comments.
Both direct commit-comment endpoints were empty.
Older Chessmen hint-group discussion concerns a different concept.

The absence of a current planner consumer is a separate integration gap.
It does not erase the recovered distinction or justify mode removal.

## Fundamental: three separate count concepts

| Concept | Existing authority | What a guarantee could add |
| --- | --- | --- |
| Owned units supplied by Chessmen | P2/P3: one slot plus 100 base material per item. Material changes the slot's family | No extra guard is needed merely to restate this entitlement |
| Pawn-family composition | Slot graduation can turn every Pawn into another family | The recovered `standard_and_pawns` purpose can retain a Pawn component while allowing stronger Pawn variants |
| Actual active units on the selected board | Geometry and placement roles can put owned slots in reserve | A deployment-aware policy could constrain graduation, but that mechanism is not selected by the historical answer |

The generator's per-Location chessmen requirement is a fourth, separate quantity.
Neither a count-guarantee option nor its Off value authorizes bypassing that
reachability requirement.

Current client evidence supports the owned-slot distinction:
`APMW.Client\OwnedRosterGeneration.cs:478-561` creates the Chessmen slots,
reclassifies Castlers, and upgrades existing slots.
`README_PENDING.md:28-32` records the separate Chessmen and Material grants.
These are current implementation facts, not substitutes for P2/P3.

The existing source-derived capacity fixture has 16 owned Chessmen on 8x8.
After its configured upgrades, 16 owned Minors project to 15 active and one reserve.
This shows that owned and deployed counts differ.
It does not show a failure of the selected 15-unit target.
See [mode synthesis](fair-board-guarantee-mode-synthesis.md#fixed-capacity-counterexample)
for complete inputs and accounting.

A roster can also satisfy its total count while containing no Pawn-family units.
That is precisely the distinction a composition guarantee can address.
The producer's existing static case uses five Chessmen, three Material items,
and a configured Pawn-to-Minor route on 8x8.
It produces five owned and active Minors, no Pawns, and 200 unspent material.
No count entitlement is lost, but no Pawn composition remains.
Shared action ratios alone do not establish a minimum composition.
The guarantee is unnecessary for the basic one-slot-per-Chessman property,
but potentially useful for the recovered Pawn-composition purpose.

## Claim ledger and current disposition

| Claim | Class and source | Disposition or limit |
| --- | --- | --- |
| Each Fundamental Chessmen item grants one piece and 100 material | Author instruction P2; P3 constrains Castlers to that budget | Retain. No count-disabling exception was recovered |
| SuperMax releases excess Pawn reservations without losing their material | Author instruction P1 | Retain as lineage; Q44 and later decisions govern restoration |
| `standard_and_pawns` has a best-effort Pawn/non-Pawn composition purpose | Author freeform answer P4 | Recover this distinct purpose. The exact current rule is still open |
| The guarantee was necessarily conceived before Fundamental | Earlier inference from producer dates | Correct: client conception predates P4, which names Fundamental explicitly |
| `standard_and_pawns` merely adds received Pawn items to a count | Producer prose, contradicted by P4 | Do not use as author intent or derive a subtraction formula from it |
| Off disables the Chessmen item count or all Location chessmen checks | No supporting author instruction recovered | Reject as an implementation assumption |
| A deployment-aware Fundamental rule must now change the wave planner | Possible engineering proposal | Not selected. P6 and current shared-contract authority still apply |
| Q56 selected one guarantee plus Off | Unanswered agent proposal | Withdraw the redundancy-based recommendation; keep the decision open |
| The old eight-Pawn example fixes every geometry's composition quota | Not supplied by P4 | Open. No automatic extension, free units, or hard eight-Pawn minimum |

## Coverage and remaining limits

Recovery examined current ADR 0022, ticket 24, the owning map, current client
slot construction, and the retained producer evidence.
Prompt searches covered all Markdown formats and namespaces under the configured
export root, including the Archipelago, ChessV, and generic chat namespaces.
Adjacent original questions, answers, later corrections, and unavailable-user
responses were read rather than treating summaries as decisions.
Bounded local/cloud session queries covered the June-July origins and later
project turns through September 12.

No later recovered answer ratifies the changed producer description or disables
Fundamental's Chessmen entitlement.
This is a bounded negative finding, not proof that no missing discussion exists.
The precise mode algorithms, shortages, composition quotas, priorities, and
interaction with the preserved wave planner remain unresolved.

No production, dependency, Git, generator, or runtime changes follow from this report.

The delegated producer comparison and detailed source/discussion citations remain at:
`C:\Users\aaedi\.copilot\session-state\0e307ab2-83cc-498a-9ba7-57e56973d013\files\fair-board-guarantee-intent-recovery.md`.
