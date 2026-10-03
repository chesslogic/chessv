# Specify 0.4.0 settings support and retirement

Type: grilling
Status: claimed
Blocked by: 17

Current frontier, 2026-10-01: Policy consolidation and engineering proof.
The D22 closure round settles the listed modes, targets, reservation, recovery,
limits, local controls, and recent connection details.
Earlier open summaries describe their historical frontier.
They do not reopen those selections.
This ticket remains claimed until the matrix, shared contract, and implementation design agree.
The production hold and all-design-before-implementation boundary remain in force.

## Question

Which public settings does 0.4.0 support, and what does each setting mean
across generation, projection, client setup, and local play?
Which historical settings must it reject rather than silently ignore?

## Authority and exclusions

The user requested a cohesive settings plan after Q35.
The follow-up audit identified missing policy, not permission to remove
every control with an old name.
This record does not authorize production implementation or a handoff.
The sibling repository remains read-only evidence.

| Exact claim | Class | Authority or basis | Excludes |
| --- | --- | --- | --- |
| Legacy and Fundamental itemization remain supported under the new contract | Author decision | ADR 0005 and ticket 17 | Removing Legacy because old world contracts retire |
| World binding and the original cost snapshot remain authoritative | Author decision | ADR 0016 and ticket 17 | Saved local settings or reconnect recalculating world costs |
| FEN resume is unsupported; reliable Undo and restricted computer takeover remain supported | Author decision | ADRs 0018-0020 and ticket 21 | Reintroducing arbitrary position import or banning controller changes |
| Preserve the existing Ignore Castlers effect across backends | Temporary assumption | Preserve current behavior unless the user selects its removal or a semantic change | Silently removing the checkbox or changing received inventory |
| Pool and Max remain public deprecated pawn presets in current source | Evidence | Sibling `worlds\checksmate\options.py:390-416,520-554` | Assuming deprecation already authorizes removal |
| Retain pawn-material conversion for Legacy in 0.4.0 | Author decision | Q44 and ADR 0022 | Treating the current one-Pawn-per-item behavior as equivalent, or retiring the capability because its new flag lacks a consumer |
| Use actually deployable contribution plus actual pocket contents for the guarantee | Author decision | Q46 | Counting received-item totals or undeployed board reserves as available units |
| Nonzero fair-board guarantees have no examined end-to-end consumer | Evidence | Sibling `options.py:562-581` and the follow-up audit | Claiming the guarantees work because serialization succeeds |
| The guarantee extraction predates Fundamental's producer implementation, not its client conception | Evidence | Q56 prompt recovery and ChecksMate commits `6fbf68628aa71f51b1803a49536ca63b89f8d02f` and `17df6adc6d89af7b88bf2bcd3002c4e66defd535` | Inferring that the guarantee had no Fundamental purpose from producer dates |
| Current type and pocket limits can relax to preserve received pieces | Evidence | `OwnedRosterGeneration.cs:371-404`, `ItemGeneration.cs:2333-2376`, and `Config.cs:920-934` in `APMW.Client` | Calling every limit a hard deployment cap |
| Priority 0 is ordinarily inactive but recoverable; -1 is a hard ban | Author decision | Closure-r2 user selection, 2026-10-01 | Treating zero as a hard ban or recovery as permission to enable -1 |
| Fundamental quality spends incremental Material and refunds only released extra funding | Author decision | Closure-r2 user selection, 2026-10-01 | Free stronger variants, refund of base100 or additional Chessmen slots |
| Fundamental raw-empty/Off preserves four unit-weight priority-1 gates with paid quality disabled | Source-preservation instruction | Coordinator instruction during closure-r2, not an additional user selection | Automatic paid quality, all-disabled fallback or a new raw-presence protocol |

## Decision surface

| Dimension | State | Required disposition |
| --- | --- | --- |
| Formal geometry, itemization, costs, and CPU families | Settled | Preserve the existing accepted contracts |
| Semantic ordering versus concrete type/location presentation | Engineering specification | Preserve the existing distinction unless the user selects a change |
| Legacy pawn-material conversion | Policy settled through the D22 closure round | Preserve funding, prepared reservation, historical approximation, and exact accounting proof |
| Human Minor setup ranks | Retained at Q47 | Preserve one mixed human rank on eight-rank boards and two on ten-rank boards; no extra Minor-only rank |
| Fundamental guarantee interpretation | Settled at Q61/Q71-Q73 | Keep fixed runtime slots and generator-owned recovery |
| Fundamental paid Pawn quality and action zero | Settled during closure-r2 | Spend above funded100, refund only released extra funding, keep slots and hard bans |
| Pool and Max pawn presets | Retired at Q76 | Reject textual and numeric values, including old option files |
| Per-type limits and pocket surplus | Settled at Q67/Q77/Q78 | Ordinary limits precede recovery overflow. Received overflow is not discarded |
| Local AI reduction | Settled at Q79 | Add the local level to collected malus, cap five, next match only |
| Local-control timing and persistence | Settled at Q80-Q82 | Preserve same-world controls, connection-time DeathLink initialization, and separate recent details |
| Absent, empty, disabled, invalid, and conflicting fields | Engineering specification | Normalize each meaning once without success-shaped defaults |
| Fundamental representation and Castler parity | Engineering specification | Use the World binding and preserve effective allocation across backends |
| Ordinary-game settings, tracker UI, and Regions refactor | Out of scope | No broad preferences reset or unrelated redesign |
| Transport cleanup and journal dispatch | Separate owner | Ticket 23 retains those obligations |

## Q44: Fair board guarantee

The producer exposes `fair_board_guarantee` with:

| Value | Name |
| --- | --- |
| 0 | `none` |
| 1 | `standard_count` |
| 2 | `standard_and_pawns` |

The examined implementation declares and serializes this setting.
The audit found no corresponding consumer that implements its advertised
guarantee in the current gameplay path.
This is source evidence, not a runtime test.

The initial recommendation was to retire the public control and reject
nonzero inputs.
The user did not select that recommendation.
They asked whether the option was recent and intended for Fundamental.
The history establishes a policy extraction, not merely an obsolete flag.
The retirement recommendation is withdrawn pending that distinction.

### Provenance clarification

Rename-aware Git history supplies two separate introductions:

| Date | Commit | Change |
| --- | --- | --- |
| 2026-07-04 | `6fbf68628aa71f51b1803a49536ca63b89f8d02f` | Added Preference Priority, Preference Ratio, and Fair Board Guarantee. Removed Super Max as a pawn-upgrade choice and documented the standalone guarantee as its replacement |
| 2026-07-19 | `17df6adc6d89af7b88bf2bcd3002c4e66defd535` | Added the separate `progression_itemization` selector and Fundamental Chessmen/Material/Castler items |

The guarantee's introducing description makes it independent of the
Pawn Upgrades mode.
It permits excess pawn material to become upgrades after pieces satisfy
board-location requirements.
`standard_count` counts found non-pawn pieces toward those requirements.
`standard_and_pawns` also counts found pawns.

This is historical source wording, not the recovered user instruction.
The later Q56 investigation below found the original July 4 answer.
It explicitly discusses Fundamental and a Pawn-composition objective.
The found-Pawn wording does not preserve that objective.
July 19 is the producer implementation date; client conception began July 3.
Neither date establishes a Fundamental-only design.
It also does not prove that the standalone option controls current play.
The introducing commit targeted client 0.3.3, while the later Fundamental
commit raised the required client to 0.4.0.

`108a5ca8e` renamed modules to lowercase on 2026-07-20.
A history search restricted to the new lowercase filename finds that
rename instead of the true introduction.

### Q44: Selected capability

The user selected:

> Retain pawn-material conversion for Legacy; specify its rule next (Recommended)

[ADR 0022](../../../docs/adr/0022-retain-legacy-pawn-material-conversion.md)
records retention of the capability in 0.4.0.
This is not a decision to reuse every historical constant or encoding.
The Legacy reservation rule still needs its count basis, target, geometry
scope, conversion actions, and accounting.
Fundamental applicability remains separate.
No production implementation is authorized.

### Orchestrated evidence work

The [client lineage note](../research/fair-board-guarantee-client-lineage.md)
records the historical pawn-reservation calculation and its exact fixture.
SuperMax itself entered ChessV on 2026-06-24.
It reduces the reserved pawn count without reducing the pawn material count.
The current owned-roster path uses a different initial count invariant.
Neither fact settles the standalone option's future behavior.

The ChecksMate research is complete.
The [mode synthesis](../research/fair-board-guarantee-mode-synthesis.md)
records its findings and points to the full retained report.
Current Legacy does not convert Pawn material into upgrades.
Fundamental retains owned Chessmen slots while separate Material upgrades
them, but this does not preserve pawn families or active count.
The historical conversion policy is therefore distinct from either
current invariant.

Q44 retains that conversion capability for Legacy.
It still needs an exact rule.
Retention does not automatically select an active-army floor, new geometry
thresholds, or consumption of Fundamental Chessmen credit.
The comparison must not derive a human minimum from the enlarged CPU army.

### Q46: Count basis

The user answered:

> I think 17 or more might fit depending on how minor pieces can rank, but anyway, only what we can actually deploy plus what's in the pocket.

The count basis is actual deployment plus actual pocket contents.
It is not received-item count or all owned units including reserve.
The guarantee must derive the qualifying contribution from the setup, not
use 11 or another capacity constant as that contribution.

The historical helper counts received non-pawn items.
The modern planner can merge those items into fewer owned units.
Projection can leave some owned units in reserve.
These are three different counts.

`PocketItemGeneration.Generate` returns one piece or null for each of three
pocket positions.
Its first tier can contain a pawn.
Source: `APMW.Client\ItemGeneration.cs:2200-2224`.
The guarantee must not substitute Pocket item count, empty capacity, or
upgrade level for the actual pieces.
The producer's conservative occupied-pocket estimate is not automatically
the same as the client allocation.
The delegated producer follow-up owns that input comparison.
Pocket-piece eligibility under the two named guarantee modes remains open.

The question's comparison occurs after non-pawn upgrades and before pawn
conversion.
The final rule still needs its evaluation order, board context, and target.
It does not yet select recursive relaxation by newly converted units.

### Q47: Minor placement clarification

The prior numerical example used the current formation authority:
[ticket 19](19-allocate-formation-bands.md).
An 8-rank board has one human back rank, one mixed rank, and three
pawn-only ranks.
On 6x8, that permits 11 non-primary non-pawns.
The current projection shares one non-pawn capacity across Jack, Major,
and Minor source roles.
Sources: `APMW.Client\ActiveRosterProjection.cs:65-68,319-371`.

Permitting Minors on another full human rank can change that ceiling.
At Q47, the user clarified:

> If this has parity with other variants, it's fine to keep it as is. I think the 10 rank variant might have another mixed row, so I probably got some wires crossed.

The agreed APMW model uses the same role rule across supported geometries
and both itemization modes.
Ten-rank boards add one mixed human rank, for two in total.
This satisfies the clarification.
Keep ticket 19's rank rules and capacities unchanged.
No extra Minor-only rank, CPU-band change, neutral-rank change, or
human-depth revision is selected.
The guarantee still counts actual deployment, not maximum capacity.

### Actual pocket input: research result

The producer follow-up is complete.
Its estimate is `min(3, ceil(Pocket items / per-pocket limit))` for positive
inputs.
That estimate supplies conservative logic, not actual pocket contents.
For two items and limit four, the estimate is one.
Abstract allocations `[2,0,0]` and `[1,1,0]` instead contain one and two
pocket pieces.
These are information-loss examples, not predictions for a particular seed.

Current slot data includes pocket seed, order, limits, and human settings.
The strict projector input does not include those limits or a resolved
three-position pocket allocation.
No shared Python allocator for the actual pieces was found.
The generator's conservative estimate cannot silently replace the actual
contribution selected at Q46.

The later contract needs a reproducible pocket input or a shared portable
allocator.
The guarantee and displayed pockets must use the same inventory/setup
snapshot and pinned eligibility rule.
Pocket pawn eligibility remains open.
This does not change Pocket material accounting or replace conservative
AP logic with exact roster totals.

The full source report's section 17 records the producer evidence:
`C:\Users\aaedi\.copilot\session-state\0e307ab2-83cc-498a-9ba7-57e56973d013\files\fair-board-guarantee-producer-findings.md`.

### Q48: Board context for the target

The user selected:

> Use the board being prepared (Recommended)

For a 6x8-to-12x10 world, a prepared 6x8 game uses the 6x8 target.
It does not use the ending board's target.
The contribution comes from units that actually deploy in that setup plus
its qualifying pocket contents.
Changing board geometry can affect pawn-material allocation despite
unchanged inventory.
The world victory condition and frozen costs remain unchanged.

### Q49: Reservation target

The historical 15/19 targets match the non-primary unit count of ordinary
two-rank armies on eight and ten files.
That evidence does not choose the target for the new formations.

Two concrete policies differ on the ten-rank boards:

| Prepared geometry | Traditional two-rank count | CPU starting count, excluding its primary royal |
| --- | ---: | ---: |
| 6x8 | 11 | 11 |
| 8x8 | 15 | 15 |
| 10x8 | 19 | 19 |
| 10x10 | 19 | 27 |
| 12x10 | 23 | 33 |

The second column follows board width.
The third follows the accepted army's unit count, including an additional
royal but excluding the primary royal.
For 10x10, it counts 12 pawns and 15 other non-King pieces.
For 12x10, it counts 14 pawns, 18 non-King pieces, and one additional King.
All four CPU families have the same counts on each geometry.

The user selected:

> Use CPU-sized targets: 11, 15, 19, 27, 33 (Recommended)

Use the third column.
This explicitly relates the target to CPU starting count minus the primary
royal.
The larger formation receives a correspondingly larger target.
It is a count policy, not a material-price or promotion-permission rule.
The two-rank alternatives on ten-rank boards are not selected.

A target does not grant missing units or promise that low inventory
satisfies it.
The exact reservation equation still needs to bound protection by the
available Pawn material.
Detailed membership and the two named guarantee modes also remain open.

### Q50: Path clarification, not a policy decision

The user asked which path could produce the proposed count reduction.
They correctly distinguished Fundamental Chessmen slots from upgrade-only
operations.
This was not an answer selecting either offered count policy.

Fundamental preserves owned Chessmen slots during upgrades.
Material changes their families, not their owned count.
Projection can place some units in reserve, but that is not eight
Chessmen becoming four owned pieces.
No new Fundamental count-consuming operation is selected.

The proposed distinction used this isolated, hypothetical Legacy count
example:

| Fixed input or operation | Value |
| --- | --- |
| Prepared board | 8x8 |
| Selected target | 15 non-primary units |
| Other deployable non-pawn units | 7, unchanged by the candidate conversion |
| Qualifying pocket pieces | 0 |
| Pawn material | 800 from 8 Pawn items |
| Initial pawn allocation | 8 units at 100 each |
| Candidate replacement for the count comparison | 2 Minor-valued units at 300 each and 2 Pawn units at 100 each |
| Final total after that replacement | 11 usable non-primary units |
| Material change within the pawn allocation | None |

The replacement is a hypothetical count-guard input, not authorization
for those specific conversion actions.
It holds material constant and varies unit count.
It fits the selected 8x8 non-pawn and pawn capacities.

That replacement is not a demonstrated current conversion path.
The question advanced beyond the still-unspecified Legacy operations.
Withdraw it as a choice between two already supported paths.
It can remain a synthetic negative count fixture, not conversion authority.

The real historical path is `GeneratePawns` to `PickPawns`.
It separates the Pawn material budget from the computed unit reservation.
The picker can spend that budget on fewer stronger units when the
reservation permits it.
`PigeonholeAllowsSergeant` protects the remaining required slots.
Sources: `APMW.Client\ItemGeneration.cs:1465-1477,1544-1589,1643-1695`.

With seven counted non-pawns, eight Pawn items, and target 15, the historical
SuperMax helper returns a reservation of eight.
Its count guard does not permit the proposed reduction to four units.
The existing 32-Pawn characterization instead supplies 15 other counted
pieces, reducing the historical reservation to zero.
These are different cases.

The modern Legacy semantic planner does not yet restore that budget-based
pawn allocation.
Q44 selects restoration of the capability, not a newly invented
many-Pawns-to-Minor action or a Fundamental change.
Specify the actual Legacy conversion operations before deriving further
policy choices from a hypothetical replacement.

### Bounded allocation design

The follow-up design proposes one Legacy phase after non-pawn planning and
actual-pocket resolution, before final projection.
Its two internal operations emit an existing permitted concrete choice or
replace an allocated choice.
Replacement refunds its previous allocation debit before charging the new
choice.
No many-Pawns-to-Minor action or Fundamental slot consumption is proposed.

The [client lineage](../research/fair-board-guarantee-client-lineage.md#concrete-choices-and-bounded-picker-traces)
records complete local picker traces.
On 8x8, eight Pawn credits can fund four 200-cost Sergeants after 15 other
units satisfy the target.
With only seven other units, the reservation protects eight ordinary Pawns.
These are direct-picker traces, not default generated-game outputs.

A small bound profile must make existing choice IDs, allocation costs,
eligibility, roles, entitlements, and deterministic selection portable.
Concrete allocation costs remain distinct from shared human expected values.
The generator must prove or revise its conservative count bounds.
Received Pawn count cannot remain an assumed unit count after consolidation.
This does not authorize replacing conservative logic with one favorable roster.

The retained proposal is:
`C:\Users\aaedi\.copilot\session-state\0e307ab2-83cc-498a-9ba7-57e56973d013\files\legacy-pawn-allocation-contract-proposal.md`.
It is engineering input, not an accepted conversion algorithm or handoff.

### Q51: Retain other Legacy surplus funding

The historical pawn budget also uses `spareMaterial` from non-pawn generation.
That value includes concrete-value differences and unused or unplaced credit.
It is not a proven transferable balance in the current normalized ledger.
Restricting the new budget to Pawn-item credit removes part of that behavior.
Retaining other funding requires explicit transfers without reserve double counting.

The user selected:

> Retain funding from other Legacy surplus (Recommended)

Other Legacy surplus remains eligible funding.
A Pawn-only budget is not the selected scope.
The exact modern transfer accounting must prevent double spending.
Passing the old mixed accumulator unchanged does not satisfy that requirement.
[ADR 0022](../../../docs/adr/0022-retain-legacy-pawn-material-conversion.md)
records Q51.

The next question concerns strict affordability versus historical approximation.
A supplied budget of 845 illustrates that separate choice:
the old picker can spend 900 on four Sergeants and one Pawn.
A strict allocator instead leaves 45 unspent after four Sergeants.
This comparison does not authorize an extra 45-unit grant.

Detailed membership, the reservation equation, named guarantee modes, and
Fundamental applicability remain open.
Neither the proposal nor these source traces settle them.

### Q52: Preserve approximation and clamp negative surplus

The user identified valid negative surplus from above-average concrete
pieces and requested a zero floor.
Negative surplus must not consume the Pawn-item credit.
The user prefers approximate spending over the proposed strict cap.
They invite a better rule and consider about 100 extra centipawns reasonable
without difficulty scaling.

This is not approval of the historical formula or proof of its maximum.
The [client trace](../research/fair-board-guarantee-client-lineage.md#historical-rounding-can-exceed-one-pawn)
shows that the old allowance and replacement pass can stack beyond 100.
The next proposal replaces those separate mechanisms with one shared
100-centipawn ceiling for the entire pawn allocation.
Emission and replacement must share that ceiling.
The amount is fixed, not multiplied by difficulty or applied per piece.
Q53 rejects that proposed replacement.

### Q53: Keep the historical approximation rules

The user selected:

> Keep the historical approximation rules

The single shared 100-centipawn ceiling is rejected.
Retain the 45-point allowance and historical ordinary-emission and replacement
affordability behavior.
Their total adjustment is not universally capped at 100.
The Checkers counterexample is a source trace for that distinction.

The zero floor from Q52 applies to net surplus before the allowance.
Accounting must distinguish funded material, the allowance, and actual
overdraw rather than label all of them received credit.
The fixed allowance does not scale with difficulty.
The transfer ledger and portable realization still need an engineering
specification under these choices.

The earlier strict-budget proposal is not the implementation policy.
Q46-Q49's prepared-board target and count basis remain unchanged.
Detailed pocket membership, named modes, and the reservation equation remain open.

### Q54: Count actual chessmen in pockets, including Pawns

The user confirmed that chessman counting includes Pawns.
Every actual pocket piece contributes one unit, regardless of family or
upgrade level.
Empty positions contribute zero.
No novel Pawn exception is selected.
This is not a count of received Fundamental Chessmen or Pocket items.

The guarantee needs exact occupied-pocket count from the same prepared setup.
It does not require a Pawn/non-Pawn classification for this count.
The conservative producer estimate still cannot replace actual occupancy.

### Q55: Preserve material through supersession

The bounded accounting amendment identifies one consequential distinction.
The existing custom Minor-plus-Major merge uses raw item values of 300 and
485, but leaves a normalized grant of 485.
The removed 300 is not a surviving reserve, dormant, or unallocated claim.
It cannot transfer to Pawn allocation while that normalization remains unchanged.

A decision fixture fixes the resulting concrete choice as the existing
500-value Rook:

| Funding interpretation | Signed surplus before the selected zero floor | Transfer after that floor |
| --- | ---: | ---: |
| Preserve the current normalized 485 | -15 | 0 |
| Retain the full received 785 as the funding base | 285 | 285 |

This compares proposed funding semantics, not two observed default games.
The concrete Rook cost comes from
`ChessV.Games\MiscellaneousGames\ApmwPieceCatalog.cs:25`.
Its shared expected Major value remains 485 under either interpretation.

Q51 selects retention of surplus funding, but does not explicitly settle
the removed 300.
Recovering it requires an explicit Legacy normalization revision.
It cannot hide in the 45 allowance or approximation overdraw.
The user selected preservation of all material involved when a placement
is superseded.
For this example, retain the full 785 funding base.
The shared expected Major value stays 485.
The new Legacy contract must explicitly revise the 485-only normalization
and preserve source-credit provenance.
It must not recover the consumed non-pawn slot or count its credit twice.

The user suspects that this case requires low difficulty and a crowded board.
It is not a condition on the accepted funding rule.
The existing custom fixture uses only one Minor and one Major on 8x8,
with Minor-to-Major priority 10.
It yields one active non-primary slot and no reserve.
The planner has no difficulty input.
Thus those conditions are not direct planner prerequisites.
This forced configuration does not establish ordinary default-path frequency
or complete-world reachability at every difficulty.

The normal Major-to-Queen transition already retains Major credit 485 plus
upgrade credit 415 as a normalized grant of 900.
Its expected Queen value remains 900.
Consuming an upgrade does not necessarily erase credit or merge two board units.

The child proposal's section 12 separates normalized source provenance,
unique credit ownership, concrete debits, allowance use, and overdraw.
It also preserves signed netting before the zero floor.
Those representation details remain engineering proposals under the accepted
funding and approximation decisions.

### Reservation enforcement remains an engineering obligation

The [Pool counterexample](../research/fair-board-guarantee-client-lineage.md#pool-selection-needs-a-complete-reservation-guard)
shows that the old stronger-piece guard alone does not prove count protection.
An ordinary Pool draw can spend material needed by later protected units.
The new guarantee must cover every candidate, not only the stronger choices.
This obligation is separate from Q53's retained approximate spending.
It does not authorize a hard material cap or another Pawn exclusion.

## Matrix to complete

### Q56: Recover mode intent before selecting retirement

The user did not select the proposed single guarantee plus Off.
They asked to recover comments and original prompt intent, including
Fundamental applicability and the meaning of Chessmen count.

The [prompt-intent report](../research/fair-board-guarantee-prompt-intent.md)
recovers the July 4 freeform answer.
It explicitly describes a best-effort composition of eight Pawns and seven
other pieces, with Sergeants allowed in the Pawn component.
That is a distinct purpose for `standard_and_pawns`.
The later producer description, which merely counts found Pawns toward a
total, does not faithfully preserve that answer.

Fundamental conception predates this answer, which names Fundamental directly.
Producer implementation date alone cannot establish the option's intended scope.
The July 3 request gives each Chessmen item one piece and 100 material.
No recovered instruction lets Off remove that entitlement.

The guarantee is not needed merely to preserve owned Chessmen slots.
It can still be meaningful for Pawn composition and possibly active deployment.
Those are distinct constraints, not new count-free itemization.
The exact algorithm and its interaction with the retained wave planner remain open.
The simplification recommendation based on alleged redundancy is withdrawn.

### Enhancement and instrumentation frontier, 2026-09-19

The user requested enhancement and instrumentation after the intent recovery.
They recall a similar audience concern from the preceding week.
That recollection establishes relevance, not the exact wording of the external feedback.
The bounded session-history search did not recover that original feedback.
Its absence does not block the present design discussion.

The [enhancement breadth pass](../qn_resolution-rounds.md#pass-6-guarantee-enhancement-and-instrumentation-2026-09-19)
keeps behavior, lifecycle, accounting, explanation, and compatibility visible together.
No new mode algorithm, default, or enforcement rule is selected yet.

| Exact claim | Class | Authority or basis | Excludes |
| --- | --- | --- | --- |
| Explore enhancements and instrumentation for the recovered composition setting | Author request | Current user message, 2026-09-19 | Treating research completion as production authorization |
| Protect a feasible Pawn-family target before cross-family upgrade preferences | Temporary proposal | A distinct composition setting needs an observable effect when preferences conflict | Claiming that the original best-effort wording already selected strict precedence |
| Explain each prepared setup with local diagnostics and a concise preview | Temporary proposal | User request to instrument the setting | External telemetry, an analytics service, or a new tracker UI |
| Every actual pocket piece contributes once to the selected total count | Author decision | Q46/Q54 | Counting received Pocket items or pocket upgrade levels |
| Whether pocket Pawns also satisfy the composition target remains unresolved | Open question | Q56 recovers composition intent without a pocket rule | Assuming that total-count membership also defines composition membership |

The first user decision concerns precedence, not the numeric target.
The illustrative target of eight Pawns remains a fixture input.
It is not a selected target for all 8x8 games or other geometries.

#### Q57: Composition versus upgrade preference

This is a family-allocation decision fixture, not an executed engine test.
It isolates the final permitted Pawn-to-Minor action.

| Field | Fixed value |
| --- | --- |
| Prepared geometry and mode | 8x8, Fundamental, Stable |
| Inventory | 15 Chessmen, 4 Material, zero Castlers and other independent unit or upgrade items |
| Material units | Each Chessman supplies one slot and 100 base material. Each Material item supplies 400 |
| Enabled actions | Only Pawn-to-Minor, at 200 material per transition |
| Other inputs | No pockets, additional royals, Pawn-quality actions, or binding type limits. The primary King is separate |
| Proposed target for this case | 15 non-primary deployed units, including at least 8 Pawn-family units |
| Operation | Allocate the owned slots, apply seven Pawn-to-Minor transitions, then consider the eighth transition |
| State before the decision | 8 Pawns and 7 Minors, all active, zero reserve, 200 upgrade material unspent |
| Candidate A | Retain that state. The composition target takes precedence |
| Candidate B | Upgrade one more Pawn. Result: 7 Pawns, 8 Minors, zero reserve, zero upgrade material unspent |
| Open output | Whether this mode rejects the eighth transition |
| Held constant | Inventory, geometry, enabled action, cost, capacity, and proposed target |
| Excluded substitution | Enabling a Sergeant improvement, adding a pocket, changing the target, or consuming an owned slot |

The initial recommendation was candidate A.
The user did not select it.
Their response identified goal viability as a missing prerequisite.
An unconditional composition-first recommendation is withdrawn pending that decision.
Shortages, excess inventory, and conflicts with Castler locks remain separate decisions.

#### Q57 response: Goal viability precedes the preference decision

The user considers a configuration invalid when its allocation rules prevent a winnable state.
They supplied 3900 on-board material as the illustrative checkmate requirement.
They proposed two remedies: rejection during generation, or ordered client relaxation after the preferred allocations are satisfied.
Neither remedy is selected yet.
Their relaxation proposal continues toward spending all material.
The treatment of an unaffordable remainder or exhausted upgrade paths remains open.

E1 fixes a current inventory snapshot, not the maximum obtainable inventory for a world.
Both E1 candidates fall below 3900 in non-primary expected material: 2900 and 3100.
That alone does not establish an invalid world.
A viability decision needs the goal, permitted geometry paths, obtainable inventory, and effective allocation rules.

The existing contract separates exact active material from conservative generator metrics.
Material estimates do not prohibit a player from earning an actual check below the estimate.
The user's 3900 example does not replace the accepted intrinsic 8x8 anchor of 4020.
Difficulty, adjustments, caps, and the world-bound cost profile still determine the applicable comparison.
The precise validity criterion remains open.

There is also a policy interaction with
[retained projection caps](../../../docs/adr/0008-separate-calibration-from-reachability.md).
Those caps can reduce an effective requirement to a low attainable maximum.
A new viability check cannot infer sufficient strength merely from that capped comparison.
Whether this introduces a separate pre-cap check remains an explicit decision.
No general cap removal or cost-snapshot change is selected.

| Exact claim | Class | Authority or basis | Excludes |
| --- | --- | --- | --- |
| A setting combination that prevents a winnable state needs rejection or a remedy | Author direction | Q57 response, 2026-09-19 | Accepting the earlier composition-first recommendation without a viability analysis |
| Generation rejection and ordered preference relaxation are alternatives | Open question | The user proposed both | Treating either as the selected algorithm |
| A current inventory shortage alone does not prove an unreachable final state | Evidence | E1 supplies no world inventory ceiling | Rejecting ordinary early progression for missing late-game material |
| Relaxation continues until all material is spent | Proposed behavior, not selected | Q57 response | Silently replacing the proposal with stopping at the goal threshold |
| Explicitly disabled actions can become fallback actions | Open question | No permission was selected | Treating priorities, ratios, composition, bans, and entitlements as interchangeable |
| A generator viability comparison uses the same allocation policy as the client | Engineering consequence | Shared-contract authority and the proposed remedies | Proving reachability with a fallback that the client never performs |

#### Q58: Best-effort preferences with shared validation

The user accepted the best-effort route.
The selected direction tries preferences first, then uses a declared fallback.
Generation validates the same allocation policy.
A configuration that still fails the agreed viability criterion needs rejection.
The alternative of treating every preference as strict is not selected.

Later decisions must name the relaxable settings and their order.
They must also distinguish meeting the goal budget from spending further usable material.
An exhausted action graph can leave material unspent even after the goal budget is met.
The exact viability criterion and its interaction with projection caps remain open.
This decision does not authorize overriding disabled actions, owned-slot entitlements, or hard placement limits.

#### Q59: Total-unit and non-Pawn minimums

The user proposed that the setting can express minimums rather than maximums.
They also proposed internal minimums for chessmen and pieces, rather than Pawns and pieces.
They then selected:

> Yes: total-unit and non-Pawn minimums, with no separate Pawn minimum

This is the current author decision.
It supersedes a mandatory Pawn-component interpretation of the historical example.
It does not retroactively change what the recovered original answer said.

For this question, "pieces" means non-Pawn units.
The illustrative counts exclude the primary King and have no pockets or additional royals.
Membership in other cases and the numeric target table remain open.

The selected meaning differs from the separate Pawn-minimum alternative:

| Predicate and disposition | Illustrative minimums | Does 15 Minors with zero Pawns satisfy the requested composition? |
| --- | --- | --- |
| Selected: total units plus non-Pawns | At least 15 non-primary units and at least 7 non-Pawns | Yes, without fallback |
| Not selected: Pawns plus non-Pawns | At least 8 Pawn-family units and at least 7 non-Pawns | No. The Pawn component requires relaxation |

Both candidates use minimums, not ceilings.
Both accept 10 Pawns plus 7 non-Pawns when inventory and capacity permit those 17 units.
Neither candidate removes excess owned units or creates missing inventory.
Their difference is whether the setting protects a Pawn component at all.

The decision concerns satisfaction of the requested preferences before fallback.
Q58 can permit a fallback result that misses a preference.
It does not make that result satisfy the original preference.
Diagnostics must retain that distinction.
The earlier E6 fixed-inventory arithmetic remains unchanged.
Its eight-Pawn restriction is not selected.
Under Q59, 15 Minors satisfy both illustrative minimums without relaxation.
The alleged 2900 composition ceiling does not apply to this selected interpretation.
The current-inventory shortage and the terminal upgrade ceiling remain distinct.

#### Pawn-cap clarification

The user asked whether Pawns have a piece cap.
The current client distinguishes concrete-type limits, placement capacity, and owned-unit entitlement.
None of these creates an eight-Pawn maximum.

| Limit | Current evidence | Consequence |
| --- | --- | --- |
| Per-concrete-type preference | `APMW.Client\Config.cs:163-165` defines Minor, Major, and Queen type limits. `OwnedRosterGeneration.cs:766-774,833-844` selects Pawns without a repetition limit | No corresponding per-Pawn-type cap exists in this selection path |
| Non-Pawn type-limit enforcement | `OwnedRosterGeneration.cs:395-404` restores the full pool when every type reaches its limit | These existing limits are soft preferences, not absolute deployment bans |
| Pawn placement | `ActiveRosterProjection.cs:68-77,366-372` bounds active Pawns by geometry and occupied mixed-band capacity | On 8x8, gross Pawn capacity is 32. Non-Pawns beyond the seven optional back-rank squares reduce that capacity |
| Fundamental ownership | `OwnedRosterGeneration.cs:478-559` creates one slot per Chessmen item and upgrades those slots | Material alone does not create extra Pawn units |
| Historical Legacy allocation | `ItemGeneration.cs:1645-1695` bounds emission by available squares and an optional `maxPawnPieces` argument | The default helper passes `-1`, but physical space still limits Pawn count |

These are source facts, not new policy choices or runtime results.
The proposed ten-rank contract has different capacities from some current source paths.
The 8x8 example does not assert those old formulas for the new ten-rank formations.

Under Q59, additional Pawns can coexist with the satisfied total-unit and non-Pawn minimums.
Eight is not a cap.
Additional allocation still requires the applicable inventory, funding, and placement capacity.
For Fundamental, no per-type Pawn limit does not authorize creating slots from Material.
The Legacy restoration still needs its explicit allocation and funding contract.

This distinction also constrains instrumentation.
A type preference, a physical capacity limit, and absent unit entitlement need different explanations.
They must not all appear as an undifferentiated piece-cap reason.

#### Orchestrated follow-up: `locked_items` and baseline allocation

The user explicitly requested orchestration and added:

> I agree. So it seems that the objective this would have (see locked_items in the generator)
> would be to place at least 7 minor and 8 pawns, but excess material could be distributed
> to extra pawns or to pawn upgrades.

The existing ChecksMate research session owns the bounded `locked_items` trace.
Its scope covers declaration, default, consumers, itemization translation, and generation feasibility.
It will distinguish pool inclusion, starting inventory, received inventory, and final deployment.
Only a session artifact is authorized.
The parent owns this decision record and the bounded ChessV consumer comparison.
No production implementation or Git operation is authorized.

This reference does not yet settle the lifetime of the example's Pawn component.
A baseline allocation can contain eight Pawns without requiring all eight to remain Pawns after upgrades.
A persistent eight-Pawn floor is a different rule and conflicts with Q59's selected predicate.
The latest statement must not be silently forced into either meaning.
The word "minor" also needs its source meaning: exact Minor family, minimum strength, or an item-pool entry.
The generator evidence precedes the next user question.
Q60's target pairs remain unasked and unselected.

##### Parent-side source boundary

Client evidence is pinned to `d14300c0b5e6091fa57dda2cf073ead9b5fc5ad0`.
The client source has no working-tree changes in this investigation.

| Layer | Source evidence | Limit of the conclusion |
| --- | --- | --- |
| Received inventory | `APMW.Client\ItemHandler.cs:145-179` reads separate Legacy family items or Fundamental Chessmen/Material/Castler counts | Fundamental does not directly treat received Legacy Pawn/Minor items as its unit grants. Any producer translation needs separate evidence |
| Legacy owned roster | `OwnedRosterGeneration.cs:431-475` plans non-Pawns, applies upgrades, then creates one Pawn slot per effective received Pawn item | This is the current path, not the authorized future conversion implementation |
| Fundamental allocation | `ItemGeneration.cs:557-618` starts from received Chessmen slots and a separate material budget, then applies tier transitions | The resulting Pawn count can change as existing slots graduate. The function does not add slots from leftover material |
| Projector input | `ApmwSidecarSnapshot.cs:333-378` captures effective received counts and enabled preferences | This snapshot is not evidence that the world supplied or the player received every guaranteed pool item |
| `locked_items` client symbol | A bounded search in `APMW.Client`, `ChessV.GUI`, and `APMW.Test` found no matching `locked_items` or `LockedItems` symbol | Absence in these paths is not proof of the producer semantics or a universal absence of related behavior |

Instrumentation needs separate records for requested pool minimums, normalized guarantees,
actual received inventory, owned allocation, and prepared deployment.
It must not show a guaranteed future item as an active present unit.
An excess-material explanation also needs the remaining unit entitlement and placement capacity.
This remains a proposed explanation structure until the producer result and policy decisions are complete.

##### Completed producer findings

The producer report examined HEAD `0772bac724ff483043b56979f0ef078b9c2d264a`.
It is retained at:
`C:\Users\aaedi\.copilot\session-state\0e307ab2-83cc-498a-9ba7-57e56973d013\files\locked-items-baseline-findings.md`.
The report contains complete static inputs, references, and explicit execution exclusions.
It describes current contract v3, not an implementation of the selected future contract.

`locked_items` guarantees literal AP item copies in the randomized pool.
Its inherited default is empty.
Neither fact rejects the user's proposed future baseline objective.
The setting does not define start inventory, a named Location placement, or a final army template.
Source: ChecksMate `options.py:673-696`, root `Options.py:905-906`, and `item_pool.py:309-318,643-680,708-729`.

The minimum plan adds locked copies after it takes the larger of mandatory and from-pool starting counts.
The framework then removes the from-pool starting copies.
Those precollected copies do not reduce the locked obligation.
Extra start inventory and fixed Early Material are also separate.
Incompatible maxima or pool capacity cause rejection rather than silent weakening of accepted locks.
Source: `item_pool.py:195-217,273-318`, root `Main.py:80-89,147-181`.

| Finding | Evidence and consequence |
| --- | --- |
| Seven Minor locks require seven literal `Progressive Minor Piece` items | A Major item does not substitute at the pool boundary. Later roster upgrades can change final families. Source: `item_pool.py:309-318,664-668,845-862` and `apmw_projection\legacy.py:169-264` |
| The Legacy 7-Minor/8-Pawn input minimum is not an exact final pool | The report's minimal-accessibility case also includes mandatory Play as White. Non-minimal accessibility adds two Majors for castling. These static plans do not prove whole-world fill feasibility |
| Fundamental rejects the same Legacy lock dictionary | There is no existing translation to Chessmen and Material. Source: `item_pool.py:259-266` and `items.py:138-147` |
| Equivalent strength does not encode the requested split | Fifteen Chessmen supply 1500 baseline credit. Seven Minor graduations require 1400 more. Material arrives in whole 400-credit items |
| Fifteen Chessmen plus four Material items do not stop at seven Minors | With only Pawn-to-Minor enabled, the current projector spends 1600 on eight graduations. It produces eight Minors and seven Pawns |
| Fifteen Chessmen plus eight Material items can produce no Pawns | Under that same sole action, the result is fifteen Minors and 200 unspent material. Q59's illustrative 15/7 predicates are satisfied without fallback |
| Lock provenance does not establish a persistent Pawn floor | `locked_items` is absent from current slot data and semantic input. The current Legacy Pawn-slot invariant is independent. Source: `__init__.py:175-235`, `logic_projection.py:377-406`, `apmw_projection\legacy.py:99-115` |

The complete Fundamental static inputs are in the report's section 5.
They use Stable ordering, 15 Chessmen, no Castlers or common items, and only a priority-1 Pawn-to-Minor action.
All five semantic seeds are `"0"`.
A 6x8 geometry baseline plus one file unlock selects 8x8.
The aggregates are seed-invariant.
These examples are projector inputs, not generated pools or executed tests.

The source resolves the existing behavior, not the desired phase of the new guarantee.
An explicit baseline-allocation stage is still a proposed enhancement.
The remaining question is whether seven Minor and eight Pawn units describe resource inputs,
an intermediate setup allocation, or a final validation rule.

##### Withdrawn baseline-boundary framing

The earlier recommendation was an intermediate setup allocation, followed by surplus allocation.
The user did not select it and corrected the generator-process analogy instead.
This recommendation is withdrawn, not an implicit phase ordering.
Such a design can preserve Q59 only if later permitted upgrades can change the baseline Pawn units' families.
No proposal implies free resources or permits Material to create Fundamental slots.

A final requirement to retain eight Pawn-family units is a different policy.
Selecting that interpretation explicitly revises Q59.
Requiring only pool resources is also different: it does not itself constrain the runtime allocation.
The exact meaning of Minor quality and the permitted surplus actions remain separate decisions.
No numeric target table or implementation handoff follows from the source findings.

##### User correction: Distribution process, not only item minima

The user did not select a boundary from the preceding alternatives.
They clarified:

> Locked_items pursues a slightly different and significantly more complex process that tries
> to maintain the original distribution while ensuring the correct minimums, so that with
> excess material, generation eventually morphs toward a wider distribution that isn't seeded
> by locked_items

The earlier source findings describe the item-count obligations.
They do not fully explain the distribution process the user identifies.
The intermediate-baseline recommendation is therefore not an accepted design.
It must not become an implicit phase ordering for the new allocator.

| Claim | Class | Disposition |
| --- | --- | --- |
| The generator's intended process preserves the original distribution while satisfying minimums, with a wider mix as material increases | Author-supplied rationale | Retain this process description. Source analysis must identify the mechanism and its limits |
| A fixed seeded batch followed by independent ordinary sampling fully describes that process | Unproven simplification | Do not use it as the design analogy |
| The new army allocator must copy the generator's entire distribution algorithm | Not selected | The user describes the generator as different and more complex |
| The user selected the proposed intermediate setup phase | Not selected | Their response corrects the premise instead of choosing a boundary |

The existing ChecksMate session now traces ordinary weights, outstanding locks, candidate removal,
material accounting, AP-slot reservations, and the transition after lock obligations are satisfied.
Its follow-up must distinguish source invariants from a tendency toward a wider distribution.
The source result will refine the analogy before another product question.
Q58 and Q59 remain selected.
The numeric targets, relaxation order, and final stopping rule remain open.

##### Distribution-process findings

Section 10 of the retained producer report completes the bounded trace at the same HEAD.
It supports the user's process distinction.
The generator samples an ordinary weighted candidate list while reserving outstanding minimum obligations.
It does not seed all locked items before sampling, derive weights from lock counts, or rebalance toward fixed output ratios.

| Step or state | Current source behavior |
| --- | --- |
| Ordinary choice | Repeated names in the candidate list provide base weights. Locks do not define those weights |
| Outstanding obligations | The planner reserves their material and AP pool slots. These obligations also affect selected count and prerequisite guards |
| Accepted matching draw | The drawn copy satisfies one obligation. It does not become an extra copy on top of that obligation |
| Accepted unmatched draw | The copy is additional to the remaining obligations |
| Constraint rejection | The loop removes one weighted occurrence of the candidate. Failed prerequisites instead leave the list unchanged |
| Completion | Later phases retain outstanding reservations. A terminal append supplies obligations that ordinary draws never satisfied |
| All obligations satisfied | Sampling continues from the remaining candidate list. There is no distribution reset |

Primary source: ChecksMate `item_pool.py:643-668,685-862`,
`item_removal.py:30-101,103-128,150-253`, and `piece_model.py:47-143`.
The report separates each mutable counter and the predicates that read it.

One important implementation detail is not a new design requirement.
Affordability includes both the candidate cost and all outstanding lock material before a matching obligation is decremented.
A matching accepted draw leaves committed material and reserved slot use unchanged, but still needs extra candidate-cost headroom to pass.
This producer behavior must not enter a future army allocator by an unreviewed mechanical port.

The report supplies controlled local traces with two Pawn obligations and one Minor obligation.
Both use the same attempted sequence: Minor, Pawn, Pawn, Pawn, Major.
They differ only in the material budget.
The traces are not real seed outputs or full-world generation results.

| Controlled procedure | Budget | Final material-bearing copies |
| --- | ---: | --- |
| Current algorithm, tighter budget | 600 | 3 Pawns and 1 Minor |
| Current algorithm, larger budget | 1100 | 3 Pawns, 1 Minor, and 1 Major |
| Hypothetical seed-first algorithm, same attempted prefix | 1100 | 4 Pawns and 2 Minors. It exhausts six local slots before the Major attempt |

The exact initial accounting, ordinary weights, guards, tail behavior, and filler exclusions remain in the report.
This comparison establishes that seed-first allocation is not equivalent to the current process.
It does not establish a universal diversity or convergence guarantee.
More material can admit candidates outside the lock dictionary, subject to finite slots, caps, prerequisites, and the sampled path.

For the proposed guarantee, the useful distinction is between minimum requirements and ordinary allocation preferences.
Ordinary choices can satisfy minimums rather than always add units on top of a prebuilt baseline.
This is a design analogy, not a selected sampler or a change to Fundamental's retained wave planner.
Q58/Q59 still control the current policy.
No persistent Pawn minimum, fixed initial phase, automatic action enablement, or generator-algorithm port is selected.

Instrumentation can make this distinction inspectable.
The proposed diagnostics separate minimums already satisfied from outstanding obligations.
They identify ordinary choices, additional allocation, eliminated choices and their reasons, material reservations, and explicit fallback relaxations.
They must not label base weights as promised output ratios.
The exact record schema follows the later allocation contract.

#### Q60 frontier: Prepared-board target pairs

Q59 selects the predicates, not the numeric targets.
The proposal is to derive both targets from the prepared CPU army, excluding its primary royal.
The total-unit target follows Q49's existing Legacy relationship.
The non-Pawn target counts the non-Pawn units in that same reference set.
This proposal extends the target pair to the composition setting in both supported itemization modes.

| Prepared board | Proposed total-unit minimum | Proposed non-Pawn minimum |
| --- | ---: | ---: |
| 6x8 | 11 | 5 |
| 8x8 | 15 | 7 |
| 10x8 | 19 | 9 |
| 10x10 | 27 | 15 |
| 12x10 | 33 | 19 |

The source counts are in the
[shared army contract](../contracts/shared-data-contract-v4.md).
The 12x10 reference has 18 non-Pawn, non-King units plus a second King.
That second King contributes to both proposed targets.
Excluding every King instead would make the last non-Pawn target 18.
This reference rule is a proposal, not a selected treatment of the player's pocket or royal membership.

These targets do not impose ceilings or grant missing units.
The exact best-effort allocation under a shortage remains open.
No target pair is accepted merely because it derives from an existing array.

#### Q60 answer: Adopt the prepared-board target pairs

The user adopted the proposed pairs.
The prepared CPU army, excluding its primary royal, supplies both minimums.
The total-unit minimum counts every other starting unit.
The non-Pawn minimum counts every other starting non-Pawn unit.

| Prepared board | Total-unit minimum | Non-Pawn minimum |
| --- | ---: | ---: |
| 6x8 | 11 | 5 |
| 8x8 | 15 | 7 |
| 10x8 | 19 | 9 |
| 10x10 | 27 | 15 |
| 12x10 | 33 | 19 |

On 12x10, the additional King contributes to both minimums.
The primary royal contributes to neither minimum.
These values are minimums, not ceilings.
They do not grant missing units or change the prepared CPU army.

Membership for deployed human royals and pocket units remains a separate
decision. Q58 still permits declared best-effort fallback when a minimum is
not feasible.

#### E6: Bounded terminal-inventory comparison

This is a synthetic family-allocation fixture, not a generated world or runtime result.
Unlike E1, it explicitly fixes the maximum inventory.
It illustrates the policy alternatives without selecting a new material requirement.

| Field | Fixed value |
| --- | --- |
| Goal context | 8x8-only world, checkmate goal, no later-board resource bypass |
| Maximum obtainable inventory | 15 Chessmen and 8 Material. No Castlers, pockets, additional royals, or other independent grants |
| Material and transitions | 1500 base material in 15 owned Pawn slots, plus 3200 upgrade material. Only Pawn-to-Minor is enabled, at 200 per transition |
| Composition input | At least 8 Pawn-family units. Other type limits do not bind |
| Comparison requirement | Stipulated 3900 non-primary expected material, before a projection cap. This is a decision input, not the released cost profile |
| Operation | Allocate the complete inventory, apply the composition policy, then consider permitted relaxation |
| Fixed invariants | All 15 units fit actively at every listed allocation. Primary King is separate. No source slot is removed or invented |
| Open dimensions | Rejection versus fallback, relaxable constraints, stopping rule, and formal comparison metric |

| Allocation | Active non-primary expected material | Upgrade material spent | Upgrade material unspent |
| --- | ---: | ---: | ---: |
| 8 Pawns + 7 Minors | 2900 | 1400 | 1800 |
| 3 Pawns + 12 Minors | 3900 | 2400 | 800 |
| 15 Minors | 4500 | 3000 | 200 |

The first allocation cannot meet the stipulated requirement with that fixed composition and action graph.
The second meets it but does not spend all usable upgrade material.
The third exhausts the only enabled transition and still leaves 200 unspent.
No larger inventory, new action, stronger concrete Pawn choice, or changed requirement is implied.

#### Proposed instrumentation boundary

Existing projection data separates active units, reserves, family counts, and material categories.
The current geometry preview supplies several totals but no explanation of composition decisions.
Its `ActiveCount` includes the primary King.
The selected guarantee targets exclude that King.
New diagnostics must use explicitly named counts rather than reuse that total without adjustment.

The proposed record contains the prepared geometry, effective policy, inventory snapshot, seed bindings, and algorithm versions.
It also contains requested targets, active family counts, actual pocket contents, reserves, and unmet targets.
Decision records identify actions rejected by the guarantee and material left unspent.
Legacy records retain separate funded credit, concrete debit, allowance, and overdraw.
Those fields must not replace the shared expected-value metrics.

A shortfall alone does not prove that the target is impossible.
The explanation must distinguish an unmet target, an established constraint, a selected relaxation, and an execution error.
The final status vocabulary and proof obligations remain engineering work after policy selection.
The viability proposal adds the applicable goal requirement and the maximum-inventory comparison.
For a fallback, diagnostics also identify each relaxed preference and its effect.
Requested and effective preferences remain separate.
Any projection cap remains visible beside the pre-cap requirement.
The proposed preview and diagnostic export use the same captured setup result.
They must not reroll allocation or substitute live pocket contents from a different moment.

No external data collection or implementation is authorized.
The repository adapter remains absent, so this work stays in the generic planning route.

### D22 closure round, 2026-09-28

The user accepted all six recommendations in one answer.
This session continues the ticket record because no other session holds a recorded claim.

#### Q61: Guarantee values

`fair_board_guarantee` keeps three values.
`none` applies no reservation and no conversion; Legacy keeps one Pawn per effective Pawn item.
`standard_count` applies the total-unit minimum only.
`standard_and_pawns` applies the total-unit and non-Pawn minimums.
In Fundamental, slots are fixed and Material cannot create units, so only the non-Pawn minimum has an effect.
It gives Pawn-to-Minor-or-better actions priority until that minimum is met.
The old documentation ("found pawns also count") is replaced, not preserved.

#### Q62: Target pairs (closes Q60)

| Prepared board | Total-unit minimum | Non-Pawn minimum |
| --- | ---: | ---: |
| 6x8 | 11 | 5 |
| 8x8 | 15 | 7 |
| 10x8 | 19 | 9 |
| 10x10 | 27 | 15 |
| 12x10 | 33 | 19 |

The 12x10 second CPU King counts as a non-Pawn, as it already counts as a unit in Q49.

#### Q63: Legacy reservation equation

Let T be the total-unit minimum, D the deployable non-Pawn units planned from non-Pawn items, and P the pocket contribution.
The Pawn allocation protects R = max(0, T - D - P) units at 100 material each.
All other eligible Pawn credit and positive Legacy surplus fund conversion.
R never exceeds the available Pawn items; a shortfall stays visible and is not invented.

#### Q64: Family membership

Each actual chessman counts once, under its current family.
The primary King contributes to neither minimum and does not provide pocket support.
Every other deployed unit contributes to the total-unit minimum.
Deployed non-Pawns, including additional royals, also contribute to the non-Pawn minimum.

Pocket contribution uses one global two-to-one support pool.
If B eligible non-primary chessmen are deployed on the board, at most
floor(B / 2) actual pocket pieces contribute.
When support is insufficient for every occupied pocket, non-Pawn pocket pieces
contribute before Pawn-family pocket pieces; order within each class follows
the canonical prepared-setup pocket order.
A contributing pocket non-Pawn counts toward both minimums.
A contributing Pawn or Pawn variant, including a Sergeant or Berolina, counts
toward the total-unit minimum only.
Unsupported pocket pieces remain present in the prepared setup but contribute
to neither minimum.

#### Q65: Viability

Viability reuses the existing logic reachability check with frozen, post-cap effective requirements.
The shared allocation policy feeds its metrics.
No second pre-cap strength check is added; projection caps keep their ADR 0008 meaning.

#### Q66: Stopping rule

After the minimums are met, allocation keeps spending usable material through permitted actions.
It stops when no permitted action is affordable.
Leftover material stays visible in diagnostics.

#### Q67 and Q78: Pocket distribution authority

Q63 and Q64 require the generator and client to agree on the actual pocket contents.
They currently disagree:

- The generator counts `occupied_pockets = min(3, ceil(items / limit))` (`item_utils.py:33-36`), which assumes pockets fill one at a time.
- The generator emits a stable `pocket_order` array (`__init__.py:482-491`), but ChessV does not read it.
- ChessV splits items with `generatePocketValues` (`Config.cs:913-956`), which its own tests call unstable across item counts. In the test seed, two items give two Pawns there but one occupied pocket in the generator.
- A pocket holding k items holds a tier-k piece: Pawn, Minor, Major, or Queen (`ItemGeneration.cs:2200-2226`).

The generator emits a suggested pocket distribution for generation and
accessibility analysis.
ChessV may replace that suggestion when it distributes the received Pocket
items into the prepared setup.
ChessV's captured final ordered pocket families control runtime composition
credit, pocket-material credit, and the displayed setup.
The final record identifies Pawn, Minor, Major, or Queen family, not the
concrete chessman, such as Phoenix or Rook.
Its order is the canonical tie-break order within each Q64 pocket family.

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

Jack and Amazon pocket tiers remain future ideas.
Selectable pocket-tier indexes, including omission of the Pawn tier, remain
future ideas.
Neither proposal is part of this decision.

#### Future extension: Configurable pocket-tier values

Each pocket-tier index can have a configurable value or piece family in a future contract.
Here, an index means the upgrade level within a pocket, not one of the three pocket slots.
The future settings can change the level-to-family mapping or omit a level,
such as the Pawn tier.
Jack and Amazon are candidate families for that extension.

This idea does not change the current Pawn, Minor, Major, and Queen mapping.
It does not permit levels five or higher in the current contract.
Defaults, allowed mappings, disabled-level behavior, costs, and wire fields
require a separate design decision before implementation.

#### Q83: Conservative pocket credit and resilience fixtures

The generator's accessibility estimate must remain conservative relative to
ChessV's final pocket distribution.
Its estimate cannot depend on credit that the permitted runtime distribution
does not provide.
Regression fixtures must cover differences between the suggested and final
distributions, the two-to-one support threshold, and ignored surplus items.

The regression corpus must include forced recovery cases and known historical
counterexamples.
The retained [Pool reservation trace](../research/fair-board-guarantee-client-lineage.md#pool-selection-needs-a-complete-reservation-guard)
provides one source-backed case.
It produces six protected units instead of seven because ordinary candidates
can spend the reserved completion budget.
This is a controlled source trace, not an executed generation failure.
Retirement of Pool does not remove the need for this reservation fixture.

Broader fixtures must cover scarce Material, scarce Chessmen, expensive upgrade
preferences, disabled actions, staged type-limit overflow, and recovery termination.
They must validate material conservation and the existing core accessibility result.
New investigations must distinguish reproduced failures from static counterexamples.

Generator clarity improvements follow the regression corpus.
They must preserve the selected behavior and use those fixtures to detect regressions.
This record does not authorize production changes.

#### Closure obligations

Accepted ADR status does not establish an implementation-ready contract.
Ticket 24 remains claimed until the decision records, settings matrix, shared
contract, and implementation design agree.
The closure work must distinguish superseded open questions from remaining
engineering obligations.

The shared contract must include the selected reservation, pocket-credit,
recovery, and effective-policy rules.
The engineering work must establish recovery termination and material conservation.
Fixtures must cover conservative generator credit relative to the final client
distribution and the existing core accessibility validator.
The final review must preserve the current production-implementation hold.

#### Q68 frontier: Pocket material credit

Current logic credits pockets with zero material (`test_raw_pocket_material_does_not_gate_v2_legacy_rules`) and one chessman per occupied pocket.
The contract table still lists 110 per Pocket item.
Tier values at 100/300/485/900 make twelve items worth up to 2700.
The user proposed credit as the least of actual pocket material, half the board material, and 300 per chessman.
This affects logic metrics, and through Q65, viability.
It does not affect Q63 conversion funding, which uses unit counts.

Only Q64-supported pocket pieces receive pocket-material credit.
Let S be their semantic family material, B the exact active non-primary
on-board material after allocation, and C their count.
Pocket-material credit is `min(S, floor(B / 2), 300 * C)`.
The primary King and all pocket material are excluded from B.
This credit contributes only to the conservative reachability and viability
material metric.
It is not spendable conversion funding, exact active material, or pocket
inventory.

#### Q69: Composition recovery and accessibility

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

#### Q70: Ordered preference relaxation

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

#### Q71: Integrated composition obligations

Composition minimums use integrated outstanding obligations rather than a
seed-first baseline or post-generation replacement.
Ordinary weighted generation continues while reserving enough material and
slots to keep the outstanding total-unit and non-Pawn obligations reachable.
An ordinary matching draw satisfies an obligation instead of adding a duplicate.
If obligations remain near completion, Q69 recovery and Q70 ordered relaxation
fulfill them.

This adopts the useful `locked_items` pattern without mechanically copying all
historical affordability, weighting, or terminal-append behavior.

#### Q72: Fundamental reservation and downgrade recovery

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
This can cascade from Queen to Jack, Major to Minor, lower pieces to Pawns, or
through other permitted adjacent families.

The user's configured distribution still governs ordinary generation.
Using that distribution to weight recovery candidates is an unselected
enhancement; the selected recovery sampling is seeded and uniform.

#### Q73: Generator-owned effective allocation policy

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

If generation relaxes preferences under Q70, slot data contains the recovered
effective upgrade order and settings.
It does not contain the superseded requested values.
The original request and the reasons for recovery remain generator-local
diagnostic information.

#### Q75: Optional future policy identity

The current decision does not require a new policy field.
A future contract may add an allocation-policy identifier when settings fields
alone no longer identify the semantics.
For that optional field, absence selects the default legacy behavior.
An explicit unknown value is rejected; it must not silently fall back.

#### Q76: Retire Pool and Max

Contract v4 does not expose the deprecated Pool or Max pawn-upgrade modes.
Their old option values are rejected everywhere, including old option files.
They are not accepted as generation aliases and never appear in slot data.
Supported explicit upgrade settings replace them.

#### Q77: Staged per-type limits

Ordinary generation enforces each configured piece-type limit.
If that pass cannot meet composition, difficulty, or accessibility requirements
with other available types, recovery may exceed the limit as a last-resort
overflow.
The initial series therefore respects the configured distribution before
overflow begins.

Overflow pieces are legitimate received inventory.
ChessV deploys or reserves them normally and never reapplies the generation
limit as a runtime discard, deactivation, or replacement rule.

#### Q79: Local AI reduction

Retain the local Reduce AI Intelligence control.
Its selected level adds to the collected Progressive AI Intelligence Malus
count, and the effective total remains capped at five.
It changes the engine limit for the next match only.
It does not change world logic, generation, item counts, costs, previews, or an
active match.

The local selection persists across previews and matches in the same world.
It resets to default when the client switches worlds.

#### Q80: Local setup-control timing

CPU family and Ignore Castlers changes affect the preview and the next match.
They never mutate an active match.
They persist across matches and reconnects to the same world.
They reset to their defaults when the client switches worlds.

DeathLink remains separate.
The world must authorize DeathLink before the local control can enable sending
or receiving it.

#### Q81: Initialize DeathLink from the world setting

On connection, including reconnect, initialize the local DeathLink choice to
enabled when the world enables DeathLink.
When the world disables DeathLink, force the local choice off and disable the
control.
Do not reset an authorized world's DeathLink choice to disabled by default.
This replaces the unaccepted reset-to-disabled proposal.

#### Q82: Recent connection details

Remember the five most recent successful connections locally across application
restarts.
Each entry stores only host, port, and slot name.
Do not store passwords or world settings.
Selecting an entry fills the connection form without connecting automatically.

This is separate from Q79-Q80's same-world local-control lifecycle.
Those decisions do not by themselves require disk persistence or saved
connection profiles.
Selecting connection details must not restore an old world's slot data or
override the connected world's DeathLink setting.

### Required matrix fields

Each setting needs its canonical key, owner, lifetime, modes, default,
normalization, generator effect, wire representation, client surface,
retirement status, and acceptance cases.
Ticket 17 remains the authority for shared artifacts.
This matrix must not create a second world-binding mechanism.

| Coverage group | Required settings or behavior |
| --- | --- |
| Board progression | Minimum/maximum geometry, contiguous transitions, configured endpoint, old `goal` interpretation |
| Itemization and presentation | Legacy/Fundamental, type ordering, placement ordering, human collections, named presets |
| Pawn upgrades | Pawn modes, Off/Pool/Max/Configure, preference list, priority map, ratios, guarantees, all-disabled actions |
| Limits and inventory | Per-type limits, Queen-item limit, pockets, human Kings, Early Material, external surplus |
| Costs and capability | Difficulty, derived adjustment, tactics, obtainable counts, frozen cost inputs |
| Local controls | CPU family, Ignore Castlers, AI reduction, DeathLink, preview/match timing, retained preferences |
| Parser boundaries | Required fields, canonical values, optional omissions, explicit invalids, obsolete keys, conflicting mirrors |

One normalized world result must feed generator rules, frozen costs, slot
data, and client setup.
Local choices must not mix values from different setup contexts.

## Initial acceptance dimensions

These are future fixture requirements, not executed results.
Open outcomes stay open until the corresponding policy is selected.

| Case | Fixed input and operation | Required or open outcome |
| --- | --- | --- |
| Guarantee disposition | Valid new-contract world in both modes. Substitute absent, 0, 1, and 2 | Q61 selects none/total/both. Absence uses the declared normalized default, never an old-world path |
| Deprecated pawn inputs | Fixed seeds and inventory. Substitute Off, Pool, Max, and Configure separately in both modes | Reject Pool/Max. Preserve Off/Configure under the normalized action contract |
| All-disabled upgrades | Fundamental, one Chessman, two Material, no Castler, pockets, or King upgrades. All 12 action priorities are `-1` | Do not substitute default actions. Preserve the selected disabled semantics |
| Major-limit exhaustion | Legacy, FIDE pool, stable presentation, Major type limit 1, no Queen upgrades, two Major items through start inventory | Q77 enforces ordinary generation limits before last-resort overflow. Runtime retains received inventory |
| Pocket surplus | Per-pocket limit 1, three pockets, four received Pocket items | Levels are 2/1/1 in the suggested tie-break order. The forced fourth item does not fail generation |
| Pocket ceiling | Three pockets with twelve, thirteen, and more received Pocket items | Levels stop at 4/4/4. Additional items remain received but add no unit or material credit |
| Pocket-credit parity | Generator suggestion differs from the permitted final client distribution | Generator accessibility credit does not exceed the runtime credit it can rely on |
| Reservation regression | Historical Pool trace: seven protected units, ordinary candidates spend the completion budget | Every candidate preserves feasible completion. This fixture does not restore the retired Pool setting |
| Recovery resilience | Scarce Material or Chessmen, expensive preferences, disabled actions, and exhausted ordinary type limits | Recovery terminates, preserves hard boundaries and material, and uses core accessibility validation |
| Local setting timing | Capture setup context A and its projection response. Change one local setting to B before adaptation | Use all of A or regenerate all of B; no mixed context |
| World switching | Connect world A, choose local controls, then connect B with different world settings | Preserve B's authority and apply the selected local reset/persistence rules |

## Current per-key settings matrix

This matrix reconciles accepted behavior with source defaults.
It does not turn current source behavior into a new product decision.
Source defaults identify the preservation baseline unless an accepted decision supersedes them.

The common-owner proposal supplies all 74 normalized rows without an unselected product-default branch.
Independent literal-schema/fixture review remains separate from general allocator and lifecycle proof.

Source baselines are ChessV `ace9362535652a70c022ef959198055bd4a932fb`
and ChecksMate `0772bac724ff483043b56979f0ef078b9c2d264a`.
The producer inventory is `options.py:706-736`.
Default declarations are in `options.py:11-704`.
Current serialization is `CMWorld.fill_slot_data`, `__init__.py:175-230`.
Current client defaults are in `ApmwConfig.Instantiate`, `Config.cs:299-380`.
They disagree in some places.
The generator's normalized result must replace client-side fallback choices.

### Matrix conventions and gates

`W` means immutable generated-world lifetime.
`G` means generator-only input, retained locally for diagnostics where relevant.
`L/F` means Legacy and Fundamental.
`P` means the effective-policy schema gate EP.
`N` means the per-field normalization/default gate NM.
`F15-F19` refer to the [fixture register](../../adr-implementation-design/spec.md#6-acceptance-fixture-register).
A `P` cell requires literal fields and matching consumers before S0 readiness.
It does not select a new mandatory policy-ID field.

| Canonical key | Owner / lifetime / modes | Default and normalization | Generator effect / wire / client consumer | Retirement and acceptance |
| --- | --- | --- | --- | --- |
| `min_board_size` | G1/G2 / W / L/F | Default8x8. Strict supported Stage, start<=end | W progression_start/stages. G1/G2/C1/U1 | Retained. F01/F07/F18 |
| `max_board_size` | G1/G2 / W / L/F | Default12x10. No12x12 alias | W progression_end/stages. S profiles and G1/G2/C1 | Retained. No 12x12 alias. F07/F10 |
| `goal` | C1/G2 / historical input | No v4 default | Reject historical input/mirror. W replaces old goal enum | No old-world acceptance. F07 |
| `progression_itemization` | A22 / W / L/F | Legacy0/Fundamental1, default0 | Policy.itemization equals W. Config/projector/ItemGeneration | Retained. F07/F15 |
| `piece_types` | P1/A22 / W / L/F | Stable1 default,Chaos0. Book2 rejects public v4 | Policy.ordering. Fundamental/Chaos rejects | Retained. Stable/Chaos semantics fixture F15 |
| `piece_locations` | P1/A22 / W / L/F | Chaos0 default,Stable1. Ordered2 rejects | Policy.piece_locations, captured presentation seeds | Retained known values. Absent raw values normalize before freeze. Conflicting mirrors reject. F17 |
| `difficulty` | G2 / W / L/F | Daily1 default. GM0/Bullet2/Relaxed3 known | G2 exact S scaling. Client retains S, no repricing | Retained. F08/F18 |
| `enable_tactics` | G1/G2 / W / L/F | Turns1 default. All0/None2 known | W.tactics_mode and published sets | Retained. F10/F12 |
| `early_material` | A22 / G / L/F | Off0 default. Pawn/Minor/Major/Jack/Piece/Any known | G-only lock behavior, Fundamental Chessmen mapping | Retained baseline. F16/F18 |
| `max_engine_penalties` | A22 / G / L/F | 5 default,0..5 | G pool. Actual collected count+local AI capped5 | Retained. F19 |
| `max_pocket` | A22 / G, effective P / L/F | 12 default,0..12 | Policy.limits. G preference, not runtime receipt cap | Retained. F15/F16 |
| `max_kings` | A22 / G / L/F | 1 default,1..3 | G primary plus Consul pool. Actual entitlements in capture | Retained. F03/F15 |
| `fairy_kings` | A22 / G / L/F | 0 default,0..2 | G King-promotion pool. Does not control CPU rules | Retained. F03/F15 |
| `fairy_chess_pieces` | A22 / W / L/F | FIDE0 default,Betza1,Full2,Configure3 | Expand preset once to Policy.enabled_armies | Retained. F15/F17 |
| `fairy_chess_pieces_configure` | A22 / W / L/F | Default seven named armies. Empty/absent Configure -> explicit FIDE source fallback | Resolve known names to pinned human indexes. Recovery preference | Retained. F15/F16 |
| `fairy_chess_army` | A22 / W / L/F | Chaos0 default,Stable1 | Policy.fairy_chess_army. Limited2 rejects | Retained. F17 |
| `army` | A22/C1 / W / L/F | Required resolved assertion if retained | Exact integer array equals Policy.enabled_armies. No empty fallback | Retained function. F15/F17 |
| `fairy_chess_pawns` | A22 / W / L/F | Producer inherited0 wins over client Mixed1 fallback | Policy.fairy_chess_pawns. Reserved4 rejects | Retained supported choices, reject Reserved. F15/F17 |
| `fairy_chess_pawn_upgrades` | A22/C1 / W / L/F | Off0 default,Configure3 under v4 | Policy selector. Retired/ambiguous values reject | Pool 1 and Max 2 reject everywhere. SuperMax is not a v4 alias. F15 |
| `piece_upgrade_preferences` | A22 / W / L/F | Absent/empty source default. Nonempty Configure list order | Normalize complete map/order. No client re-resolution | Retained. F15/F16 |
| `piece_upgrade_priority` | A22 / W / L/F | Absent/empty source list/default. Nonempty map missing1, -1 hard ban | Full map. User-selected0 soft inactive, recoverable after quantity | Retained. F15/F16 |
| `piece_upgrade_ratio` | A22 / W / L/F | Consumed option paths use individual twelve defaults/partial merge. Fundamental empty fallback uses four unit weights | Reduced exact weights. Negative/null rejects; no6/3/2/1 fallback change | Retained. F15/F16 |
| `piece_upgrade_proportion` | C1/A22 / historical input | No v4 default | Reject obsolete key. No independent alias | Historical only. No legacy-world fallback. F07/F15 |
| `fair_board_guarantee` | A22 / W / L/F | None0 default,total1,both2 | Policy enum, prepared targets/R. Both modes | Retained three modes. F15/F16 |
| `minor_piece_limit_by_type` | A22 / W / L/F | 0 default,0..15 | Policy.limits. Ordinary first, overflow last, no runtime discard | Retained staged limit. F16 |
| `major_piece_limit_by_type` | A22 / W / L/F | 0 default,0..11 | Same | Retained. F16 |
| `queen_piece_limit_by_type` | A22 / W / L/F | 0 default,0..9 | Same | Retained. F16 |
| `queen_piece_limit` | A22 / G / L/F | 0 default,0..9 | G ordinary quantity preference. Forced owned inventory wins | Retained baseline, not new runtime loss. F16 |
| `pocket_limit_by_pocket` | A22 / W / L/F | 4 default,0..4 | 0 suppresses ordinary generation, not actual received items | Retained ordinary limit. Forced items never discarded. F15/F16 |
| `locked_items` | A22 / G / L/F | Empty default. Known items/nonnegative counts | G outstanding obligations consume matching draw once | Retained. F16/F18 |
| `start_inventory_from_pool` | A22/G2 / G / L/F | Empty default. Core validation | G2 final recipient counts after actual pool transfer | Retained core semantics. F18 |
| `start_inventory` | A22/G2 / G / L/F | Empty default. Core maximum10000 per inherited entry | Final obtainable counts. Actual capture preserves receipts | Retained core semantics. F16/F18 |
| `accessibility` | G2 / G / L/F | Full0 default,Minimal2, retained core aliases | Real core validation. No client or replacement validator | Retained core validator. F18 |
| `progression_balancing` | G2 / G / L/F | 50 default,0..99 | Core owns it. Cannot mutate frozen policy/rules/S | Retained core semantics. F18 |
| `local_items` | A22/G2 / G / L/F | Empty default, core item/group validation | Core placement restriction, no client field | Retained. F18 |
| `non_local_items` | A22/G2 / G / L/F | Empty default, core validation | Same, no new entitlement | Retained. F18 |
| `start_hints` | G2 / G / L/F | Empty default | Core hints only | Retained, no new APMW behavior |
| `start_location_hints` | G2 / G / L/F | Empty default | Core hints/profile validation only | Retained, no new APMW behavior |
| `exclude_locations` | A22/G2 / G / L/F | Empty default | Core exclusion, retained Early Material override | Retained. F18 |
| `priority_locations` | A22/G2 / G / L/F | Empty default | Core placement preference, no cost mutation | Retained. F18 |
| `item_links` | A22/G2 / G / L/F | Empty default, exact inherited AP schema | G2 normalized recipient counts before freeze | Retained core semantics. F18 |
| `plando_items` | A22/G2 / G / L/F | Empty default, exact inherited AP schema | G2 actual obtainable counts after core transfer | Retained core semantics. F16/F18 |
| `asymmetric_trades` | A22 / W-effective, G input / L/F | Disabled0 default,Jacks1 | Policy. Effective action/family eligibility | Retained. F15/F16 |
| `death_link` | N1 / W plus connection-local choice / L/F | Inherited disabled default | World slot bool/int normalization for N1, no allocation field | Retained. F05/F19 |
| `required_chess_client_version` | G2/C1 / W / L/F | Required, no fallback | Exact C minimum0.4.0 initially | Retained mandatory assertion. F07 |
| `apmw_contract` | C1/P1/I2 / semantic release / L/F | Required JSON object | Exact C4.0 and pins. Old/string/missing reject | Old/string/missing rejects. F07 |
| `apmw_world_binding` | G2/C1/N1 / W / L/F | Required JSON object | Exact W plus policy SHA amendment. No UUID | Retained. F07/F11/F17 |
| `apmw_location_costs` | G2/C1 / W / L/F | Required original S object | C/A/L/W bindings. No local recomputation | Retained. F08/F18 |
| `apmw_contract_version` | G2/C1 / W mirror / L/F | Optional compatibility assertion | Equal C or reject | Conflicts reject. F07 |
| `material_item_value` | G2/C1 / semantic mirror / L/F | Optional assertion400 | Equal C. No separate price input | Conflicts reject. F07 |
| `castling_location_count` | G2/C1 / semantic mirror / L/F | Optional assertion2 | Equal C. No6x8 capability grant | Conflicts reject. F07/F12 |
| `geometry_unlock_items` | G1/C1 / semantic mirror / L/F | Optional assertion | Equal C unlock mapping | Conflicts reject. F07 |
| `geometry_start` | G1/G2/C1 / W mirror / L/F | Optional assertion | Equal W.progression_start | Retained function. F07/F18 |
| `geometry_end` | G1/G2/C1 / W mirror / L/F | Optional assertion | Equal W.progression_end | Retained function. F07/F18 |
| `geometry_baseline` | G1/G2/C1 / W mirror / L/F | Optional files/ranks assertion | Equal C geometry for W start | Retained function. F07/F17 |
| `geometry_start_unlocks` | G1/G2/C1 / W mirror / L/F | Optional role-count assertion | Exact C/W start counts | Retained function. F07/F18 |
| `geometry_generated_unlocks` | G1/G2/C1 / W mirror / L/F | Optional role-count assertion | Exact configured contiguous transitions | Retained function. F07/F18 |
| `geometry_end_unlocks` | G1/G2/C1 / W mirror / L/F | Optional role-count assertion | Exact C/W endpoint counts | Retained function. F07/F18 |
| `geometry_unlock_distribution` | C1/G2 / historical mirror / L/F | No v4 default | Reject obsolete key. No old-goal reconstruction | Historical only. F07/F18 |
| `logic_obtainable_counts` | A22/G2 / W / L/F | Required finalized map under G2 frozen state | Actual final counts, not C maxima. Rules/S/restoration | Retained function. F18 |
| `total_queens` | A22/G2 / W-derived / L/F | Optional derived assertion | Exact finalized Queen-upgrade count. Never a second grant | Retained only as an exact assertion. Conflicts reject. F15/F18 |
| `pocket_order` | A22 / W / L/F | Required12 entries, four each | Policy full order. Captured first-occurrence scan rule | Retained function, no invented fixed order. F15/F17 |
| `pocket_seed` | A22/P1 / W / L/F | Generated31-bit, required after freeze | Policy.seeds and capture | Retained function. F15/F17 |
| `pawn_seed` | A22/P1 / W / L/F | Same | Same | Retained function. F15/F17 |
| `minor_seed` | A22/P1 / W / L/F | Same | Same | Retained function. F15/F17 |
| `major_seed` | A22/P1 / W / L/F | Same | Same | Retained function. F15/F17 |
| `queen_seed` | A22/P1 / W / L/F | Same | Same | Retained function. F15/F17 |
| Presentation/graduation seed properties | A22/P1 / captured setup / L/F | No new slot fields. Existing LocSeed and graduation values | Actual capture values, independent semantic/presentation keys | Engineering representation gate. F17 |
| `enemy_army` (local selector) | U1 / same-world local / L/F | Absent/unknown selector ->Standard source fallback | Same-world L, preview/next match, no S recalibration | Retained, same-world reconnect persists, world-switch resets. F03/F19 |
| `IgnoreCastlersReceived` (local property) | U1 / same-world local / Fundamental surface | False source default. Fundamental-only surface | Capture actual inventory unchanged, effective castlers0 | Retained. No invented slot key. F17/F19 |
| Reduce AI Intelligence (local control) | U1 / same-world local / L/F | Default means additional reduction0. Levels1..5 | At StartMatch min(5,collected+local). Same-world memory only | Retained. Same-world persistence, world-switch default. F19 |
| Local DeathLink choice | N1 / connection generation / L/F | On/enabled if world authorizes, else off/disabled each connect | N1 one service and generation-safe callbacks | Retained. F19 |
| Recent host/port/slot entries | U1 / local disk, application restarts / all | Five most recent successful triples, absent file ->empty list | U1/Convenience storage, not world settings | Selected Q82. No passwords, slot data or world settings. F19 |

| `apmw_effective_policy` | P1->A22 / W-bound generated authority / L/F | Required JSON object | Full Policy. Generator effective result is authority | Required exact v4 object and policy hash. All-disabled, soft-zero, mismatch and stale fixtures. F07/F15/F17 |

The twelve canonical actions remain:
`new-pawn`, `more-pawn`, `better-pawn`, `pool-pawn-upgrade`,
`pawn-to-minor`, `pawn-to-major`, `minor-to-major`, `major-to-jack`,
`minor-to-jack`, `major-to-queen`, `jack-to-queen`, and `queen-to-amazon`.
Retiring the Pool preset does not retire the `pool-pawn-upgrade` action.
Source ratio defaults in that order are `7,1,3,1,6,2,3,2,2,1,1,1`.

Those source values do not replace Fundamental's preserved four unit-weight raw-empty/Off gates.
The effective policy publishes the complete map, resolved order, weights, disabled set, and seeds.
A normalized all-disabled set cannot select default actions.

### Normalization precedence and action outcomes

These are common-owner engineering definitions of the selected/source-preserved behavior.
They do not authorize production changes or close general AL/PM/LC.
Raw generator preferences differ from complete normalized world instructions.

```text
nonempty piece_upgrade_priority:
  all twelve actions, missing priorities 1
else nonempty Configure preference list:
  listed actions in order, unlisted actions hard-disabled
else Legacy Off/Configure-empty:
  new-pawn, more-pawn, better-pawn, major-to-queen
else Fundamental Off/Configure-empty:
  pawn-to-minor, pawn-to-major, minor-to-major, major-to-queen
  priority 1 and weight 1 for these four; eight other actions hard-disabled
```

Raw absent/empty maps and lists select the source defaults.
Normalized maps require all twelve records.
Normalized empty maps reject.
Twelve explicit priority -1 records preserve all twelve bans and an empty order.

No AP raw-presence extension is necessary.
Baseline received entitlements remain independent of discretionary actions.
Off does not mean a global ban on source-default actions.
Legacy guarantee-none separately forbids conversion.

A nonempty priority map outranks a simultaneous preference list.
Generator diagnostics retain the superseded list.
A slot mirror of that list must equal the effective order or reject.
Duplicate/unknown action names, null values, invalid priorities, ratios, and enums reject.

Positive priority groups run in descending order with canonical ties and exact relative weights.
Priority-zero actions stay represented but inactive until weights/order recovery.
Priority -1 stays hard-disabled in every phase.

Missing raw ratios use their individual source defaults on paths that consume that map.
Exact decimal ratios become reduced fractions before freeze.
Negative, nonfinite, excessive-precision, or out-of-bound fractions reject.
Zero weight is not a hard ban.

Ordinary positive-weight groups exclude zero-weight members.
An all-zero viable group uses the preserved uniform source behavior.
Fundamental raw-empty defaults retain four executed unit weights.
Unused requested ratios cannot become effective mirrors.

Pool1/Max2, their names, SuperMax, old-client Configure4, and unversioned ambiguous3 reject.
Configure3 is valid only under admitted v4 encoding.
Raw Configure-empty army preferences become explicit FIDE `[0]`.
This preserves the producer fallback.
Normalized army arrays remain nonempty, known, ascending, and unique.

Q70 still permits preferred-family relaxation.
This normalization does not introduce a hard family ban.

| Action | Source ratio | Legacy admission | Fundamental admission |
| --- | ---: | --- | --- |
| `new-pawn` | 7 | Discretionary emission requires conversion credit and free capacity. Baseline Pawn entitlement is separate | Only an existing unfilled owned slot. No extra Chessman |
| `more-pawn` | 1 | Permitted additional Pawn-family emission with full protected-completion guard | Only an existing unfilled owned slot. No extra Chessman |
| `better-pawn` | 3 | Source refund, new concrete debit, bounded same-family historical pass | Unlocked Pawn paid100->150/200 or150->200, exact extra and concrete tier, forward completion guard |
| `pool-pawn-upgrade` | 1 | Every permitted ordinary/stronger draw has the same completion guard | Same paid-tier rule, no slot/grant or automatic default enablement |
| `pawn-to-minor` | 6 | Owned Pawn-funded realization plus eligible conversion credit | Delta300-current funded value, atomically replacing quality extra |
| `pawn-to-major` | 2 | Same to Major, without a fabricated Major item | Delta485-current funded value |
| `minor-to-major` | 3 | Recorded Minor+Major merge or eligible conversion funding, preserving source keys | Unlocked Minor delta185 |
| `major-to-jack` | 2 | Recorded Major+Jack merge or eligible conversion funding | Ordinary Major delta215. Locked Castler delta200, Jack ceiling |
| `minor-to-jack` | 2 | Recorded Minor+Jack merge or eligible conversion funding | Unlocked Minor delta400 |
| `major-to-queen` | 1 | Existing source plus unique415 Queen-upgrade token, or eligible funded transformation | Unlocked Major delta415 |
| `jack-to-queen` | 1 | Same unique Queen-upgrade token domain, no independent Queen grant | Unlocked Jack delta200 |
| `queen-to-amazon` | 1 | Eligible conversion funding, no invented Amazon item | Unlocked Queen delta400 |

Every action also obeys source role, family, captured catalogs, hard bans, ownership, entitlements, and physical capacity.
Same-family actions require the finite operation proof, not an increasing family rank.
The [shared literal schema and ledger](../contracts/shared-data-contract-v4.md#65-literal-effective-policy-capture-and-projection-records)
define normalized fields and funded-state admission.

### Closure-r2 funded-quality amendment

The user selected priority-zero recovery and paid Fundamental Pawn quality on 2026-10-01.
Priority `0` is inactive ordinarily and recoverable during weights/order relaxation.
Priority `-1` stays hard-disabled.
`better-pawn` and `pool-pawn-upgrade` spend only the increment above the currently funded Pawn value.

Starting at funded100, Pawn150 costs 50 extra and later Pawn200 costs another 50.
Downgrade returns only released extra Material funding.
The change preserves one slot per owned Chessman.

The coordinator's source-preservation instruction retains the current raw-empty/Off Fundamental defaults.
Those defaults have four priority-1/unit-weight gates and disabled paid quality.
This instruction is not a new user distribution selection.
Base entitlement remains separate.

Normalized all-twelve-`-1` cannot become those defaults.
No raw-presence extension is required.

The common owner's candidate ledger equations and generator's small-domain model are engineering evidence, not further user choices.
The complete normalization/literal proposal awaits final independent review.
General quality-domain AL/PM/LC proof remains open.
The [ADR amendment](../../../docs/adr/0022-retain-legacy-pawn-material-conversion.md#closure-r2-funded-fundamental-quality-and-action-priorities)
records the selected scope.

The [closure register](../../adr-implementation-design/engineering-closure.md#closure-r2-quality-authority-and-pending-proof)

records derived accounting, the non-Pawn counterexample and remaining proof.

Inherited defaults are from `Options.py:906,1026,1343-1425,1460-1565,1608-1610,1737-1747`.
Current geometry mirrors are from `CMWorld._geometry_slot_fields:604-623`.
Generated semantic seeds are from `_SEMANTIC_SEED_NAMES:46-52` and `_ensure_semantic_seeds:468-480`.

Final independent review must accept the literal records and conflicting-mirror outcomes before consolidated EP/NM acceptance.
Unknown or obsolete values cannot silently select success-shaped defaults.

The [engineering closure register](../../adr-implementation-design/engineering-closure.md)
names the owners, gates, and required evidence for every remaining representation gap.

## Sources

The [question network](../qn_resolution-rounds.md#q13-remaining-settings-questions)
records the follow-up audit and branch ownership.
The [shared contract](../contracts/shared-data-contract-v4.md) records
artifact authority and supported semantics.
The current source references are evidence, not a substitute for future
cross-repository acceptance.
