# Fundamental Itemization: Slot Graduation Planner — Design & Tracking

> **Status:** Living tracking document. Persisted from agent session state on **2026-07-05** so
> this design history, evidence, and the list of open decisions survive independently of any
> single chat/session — if session state is ever lost, start here to recover full context.
> **Keep this file updated** (not just session-local `plan.md`) as work on this feature continues.
>
> Snapshot reconciled against repo HEAD `a19ee88` ("Split unreleased docs into README_PENDING")
> on `develop`, which itself follows `967b0df` ("Add weighted tie-breaks for upgrade actions") —
> both committed directly by @chesslogic in a concurrent session while this tracking doc was being
> assembled. See §4 and §8 for how that concurrent work was reconciled into this document.

## Contents

1. [Open decisions needed from @chesslogic](#1-open-decisions-needed-from-chesslogic)
2. [Problem statement](#2-problem-statement)
3. [Approved design](#3-approved-design-per-chesslogics-answers)
4. [Implementation status (current)](#4-implementation-status-current)
5. [Deep-dive: chessmen-growth dilution-bug fix trade-offs](#5-deep-dive-chessmen-growth-dilution-bug-fix-trade-offs)
6. [Implementation note: integration point correction](#6-implementation-note-integration-point-correction)
7. [Verification status](#7-verification-status)
8. [Document history](#8-document-history)

---

## 1. Open decisions needed from @chesslogic

Three concrete, unresolved product/architecture questions remain. Everything else in this
document is either implemented, verified, and green, or is settled historical record.

### 1.1 — Chessmen-growth dilution bug (unfixed; see full analysis in §5)

Recomputing the Fundamental roster with **more `Chessmen` but the same `Material`** does not
extend the previous roster the way growing `Material` alone does — it can **completely reverse**
it, retroactively downgrading or removing previously-generated Queens/Amazons. Confirmed still
reproducible against current code (verified 2026-07-05, `DistinctPriorities`-style all-unique
priorities, `Material` fixed at 5000, same seed, only `Chessmen` count changes):

| Chessmen | Major | Jack | Queen | Amazon | spare |
|---|---|---|---|---|---|
| 5 | 0 | 0 | 3 | 2 | 200 |
| 9 | 2 | 7 | 0 | 0 | 30 |

This is a **real player-visible regression risk**, not a cosmetic issue. Root cause: the
simulation treats all `N` slots as one shared, fungible pool competing "wave style" for a single
`spareMaterial` counter — every eligible slot at a priority tier advances together before the scan
drops to the next tier, so adding more competing slots permanently dilutes the same fixed budget.
This is a **different** problem from the (already-solved) per-slot-randomness prefix-instability
issue — it happens even with zero ties/randomness involved.

**This is not an implementation mistake** — the current wave-style algorithm is a literal
implementation of @chesslogic's own explicit design instruction ("When we upgrade something, reset
its priority to max" — see §3). Any fix necessarily requires deviating from that instruction in
some way, which is why no fix has been implemented without explicit sign-off. Four candidate
approaches have been rigorously analyzed (empirically, not just theoretically) — see §5 for full
detail. Top recommendation: **event-sourced replay from actual received-item order**
(`ReceivedItemsHelper.AllItemsReceived`), with memoized incremental state as backup. **Needs your
decision on which approach (if any) to implement.**

### 1.2 — Pawn-quality unification (`new-pawn`/`better-pawn`/`more-pawn`/`pool-pawn-upgrade`)

@chesslogic's original request included "New-pawn and pawn upgrades should be unified to this
model in order to simplify the codebase." As of the 2026-07-04 concurrent commit (`967b0df`),
`NewPawn` now shares a default **priority number** with the tier-graduation actions (see §4), but
**mechanically it still "rides along untouched"** — `PickNewOrMorePawnAction`/`UpgradePawns`
(`ItemGeneration.cs`) only ever check whether `NewPawn` is *enabled*, never compare its priority
against another action in the weighted/seeded graduation draw. Separately,
`PoolPawnUpgrade`/`MorePawn`/`BetterPawn` were explicitly confirmed (via code comment,
`Config.cs:625-631`) to be "a disjoint subsystem (pawn board-slot *variant* selection, not
`ChessmanTier` graduation)" whose **relative priority order is load-bearing** and must not be
folded into the same weighted-draw mechanism casually.

Open sub-questions blocking full unification (unchanged from earlier analysis): does `foundPawns`
merge into the same simulation/budget as `foundChessmen`, or stay a separate simulation reusing
the mechanism? What does `new-pawn` mean mechanically as a graduation action (quota-claim vs.
same-tier quality pick)? Does `PawnGeneration`'s variant-selection logic remain an untouched
downstream step (like `MajorPieceGeneration`) or get folded in too? **Needs your input** on
whether/how far to take this unification, given the disjoint-subsystem boundary the concurrent
session already drew.

### 1.3 — `more-pawn` / `pool-pawn-upgrade` removal

@chesslogic mentioned "Items outside this list will probably be removed before release" (referring
to the planned final action list: `new-pawn=7, better-pawn=3, pawn-to-minor=6, minor-to-major=3,
major-to-jack=2, minor-to-jack=2, major-to-queen=1, jack-to-queen=1, queen-to-amazon=1`), which
would put `more-pawn`/`pool-pawn-upgrade` on the chopping block. **These two actions are confirmed
genuinely Legacy-reachable and meaningful today**, not just a theoretical risk:
`Config.LegacyPieceUpgradePreferences(FairyPawnUpgrades.Off/Pool/Max/SuperMax)` explicitly
includes both actions, `ResolvePieceUpgradeActions(...)` honors them for Legacy whenever slot-data
is absent or a custom `piece_upgrade_preferences` list/map is provided, and
`ItemGeneration.PickPawns -> PickPawnUsingUpgradePreferences -> PickNewOrMorePawnAction /
ApplyPoolPawnUpgradeAction` consumes the resolved config with **no** `ProgressionItemization` gate.
Removing these two actions would change real Legacy seed behavior for anyone using
`fairy_chess_pawn_upgrades` or a custom `piece_upgrade_preferences` list — **this must not be done
without your explicit confirmation**, since your "probably removed" comment was likely made
without full awareness of this Legacy entanglement.

---

## 2. Problem statement

`ProgressionItemization.Fundamental` (introduced in commit `f6a270e`, 2026-07-03) originally
computed per-family piece counts via `FundamentalMaterialAllocationPlanner`: a greedy loop that
repeatedly scanned an ordered `List<FundamentalPieceRecipe>` (built from
`config.PieceUpgradePreferences`) and applied the first affordable recipe, over and over, until
slots or material ran out. That recipe-based planner and its dead code have since been **fully
removed**.

@chesslogic wanted a design that mirrors the "legacy placement algorithm" (kept as-is): decide the
*entire* roster of pieces up front via a seeded, per-slot simulation, rather than an aggregate
greedy scan. This gives genuine per-slot randomness (fair tie-breaking among equally-eligible
pieces) while keeping the same deterministic priority contract (`piece_upgrade_preferences`) and
seed-consistency guarantees Legacy already has (recomputing from scratch with a larger budget must
extend, not reshuffle, the smaller-budget roster).

Scope also includes unifying pawns into the same model (per @chesslogic: "New-pawn and pawn
upgrades should be unified to this model in order to simplify the codebase") — for the
**Fundamental path only**. Legacy's pawn/piece pipeline is untouched. (See §1.2 — this remains
only partially done.)

---

## 3. Approved design (per @chesslogic's answers)

**Unified tiers**: `Pawn(0) -> Minor(1) -> Major(2) -> Jack/Queen(3) -> Amazon`. Every one of the
`N = foundChessmen` slots starts at tier Pawn. `NonPawnPieceFamily` stays as-is for Legacy/shared
placement code; the planner tracks tier per slot internally.

**Candidate structure — "array of sets keyed by priority"**:
- For every *enabled* upgrade action (`Priority > 0`), the action has a `From` tier and a `To`
  tier (e.g. `MajorToQueen`: From=Major, To=Queen).
- Candidate sets are keyed by **distinct priority values actually present** (not a dense array
  indexed by raw priority number, since `piece_upgrade_preferences` can also arrive as an
  arbitrary JSON priority map with sparse/large values).
- A slot's index is inserted into **every** priority-level set for which an enabled action's
  `From` tier matches the slot's current tier (confirmed by @chesslogic: "the same index must also
  exist in the lower-priority set indicating a possible upgrade to Jack").
- Castler-locked majors are seeded directly at tier Major and **excluded** from all candidate sets
  (never eligible for further upgrade), matching current behavior.

**Simulation loop** (drives all graduation, pawn and non-pawn):
1. Start scan at the highest present priority level.
2. If that level's set is empty, drop to the next-lower **present** level; if none remain, stop.
3. Otherwise draw a (seeded, and — as of `967b0df` — proportion-weighted when actions tie)
   random slot from that level's set.
4. Check affordability against the running `spareMaterial` counter. If not affordable, drop the
   scan to the next-lower present priority level and retry (the slot/action stays valid, just not
   affordable *right now*).
5. If valid, apply the upgrade: debit cost from `spareMaterial`, move the slot to its new tier,
   remove it from every candidate set it was in, re-insert into sets for the new tier's outgoing
   actions. **Reset the scan to the highest priority level again** (@chesslogic: "When we upgrade
   something, reset its priority to max") so the next pick always prefers the globally
   most-preferred still-affordable action. **This exact step is the root cause of the dilution bug
   in §1.1/§5 — it is working as literally designed, not a bug in the traditional sense.**
6. Repeat until step 2's stop condition triggers.

**Unified spare-material accounting**: one running `spareMaterial` counter
(`ref int spareMaterial`), initialized to `materialBudget`, debited for locked-Castler cost up
front and for every applied upgrade — no separate reconciliation formula.

**Seeding**: a dedicated `config.fundamentalGraduationSeed` (derived in `Config.seed()` the same
way as `pocketSeed`/`pawnSeed`/etc.) so slot-selection randomness is independent from
`majorSeed`/`minorSeed`/`queenSeed`, which remain owned by the untouched downstream "legacy
placement algorithm."

**Output / integration**: the planner produces final tier counts, the locked-major count, and the
single leftover `spareMaterial` value, feeding `PlayerPieceSetGeneration.Generate` as pre-netted
`DirectCounts` with an **empty** `NonPawnGenerationPlan.UpgradeActions` list —
`NonPawnUpgradeGeneration`/`NonPawnFamilySubstitution`/`MajorPieceGeneration`/`PiecePlacement` (the
"legacy placement algorithm") stay completely unmodified.

**Invariants to preserve**: `MaxGeneratedNonKingPieces`, `MaxNonPawnPipelineSlots`,
`MaxCastlingMajorSlots` bounds; determinism; prefix-stability under growing Material (confirmed
holding); prefix-stability under growing Chessmen (confirmed **not** holding — §1.1/§5).

**Test data is not sacred**: since Fundamental is unreleased, existing exact-count assertions may
be updated if the new algorithm legitimately produces different (but still correct) numbers.

---

## 4. Implementation status (current)

Phase 1 (the core planner) is implemented and green. The implementation **deviated from the
original design above in several ways**, each forced by issues discovered while
implementing/validating, and has since been extended further by a concurrent session:

1. **Pure per-tier counts, not per-slot arrays.** `FundamentalSlotGraduationPlanner.Simulate`
   (`APMW.Client/ItemGeneration.cs:549-640`) tracks only `int[] tierCounts` (6 entries), not
   per-slot candidate sets/`HashSet`s. A per-slot pool design was tried first but is **not
   prefix-stable**: a random draw from a pool of size N consumes a different amount of `Random`
   state depending on N, so recomputing with a larger budget would reshuffle rather than extend
   the smaller roster. Since every slot at a given tier is completely interchangeable, tracking
   counts is all correctness requires.

2. **Two gateways out of Pawn: `pawn-to-minor` and `pawn-to-major`.** Originally only
   `pawn-to-minor` existed (a slot could reach Major/Jack only via Minor). The 2026-07-04
   concurrent commit (`967b0df`) added `pawn-to-major` (`ApmwConstants.cs`, `Config.cs:83,622`,
   `ItemGeneration.cs:669`) as a second, direct Pawn→Major gateway. Slots taking this path never
   have a Minor placeholder at all, so `BuildNonPawnPlan` seeds them directly into
   `directCounts[Major]` (see point 4 below) — same treatment as castler-locked majors, which are
   also pre-seeded directly and never pass through Minor.

3. **Weighted tie-breaking via `piece_upgrade_proportion`.** Also added in `967b0df`: an optional
   `piece_upgrade_proportion` dictionary weights which equally-preferred action is chosen when two
   or more share a priority tier (`Config.cs` `ProportionFor`, ~line 593). An action absent from
   this dictionary defaults to a weight of `1.0`; a weight of `0` always loses to any competitor
   still in contention, but still applies as a last resort if it becomes the only viable action
   left (verified by test `Plan_ZeroProportionActionStillAppliesAsLastResortOnceItsCompetitorIsExhausted`,
   `NonPawnUpgradeGenerationPlanTests.cs:118-157`). This same weighted tie-break mechanism was
   **also retrofitted onto Legacy's own `NonPawnUpgradeGeneration.Plan`**
   (`ItemGeneration.cs:891-947`) for its own upgrade actions (e.g. Minor→Major/Jack, Major/Jack→
   Queen) whenever two of them share a priority — a deliberate, shared cross-cutting change, not
   Fundamental-only.

4. **Fundamental's "no-config" default is now a sensible tied set (resolves what was previously
   an open question — see historical note below).** As of `967b0df`,
   `LegacyPieceUpgradePreferences(...)` (`Config.cs:599-657`) special-cases
   `progressionItemization == Fundamental`: instead of falling through to Legacy's own per-mode
   defaults (which never included `pawn-to-minor`, causing every slot to degenerately stay Pawn
   forever), Fundamental now defaults to **five actions tied at priority 1**, differentiated only
   by proportion: `NewPawn`, `PawnToMinor`, `MinorToMajor`, `PawnToMajor`, `MajorToQueen`
   (`Config.cs:618-623`). This was an explicit @chesslogic decision confirmed directly in code
   comments ("`@chesslogic confirmed (2026-07) that the new tied graduation-action default set is
   scoped to Fundamental-mode slot graduation only`", `Config.cs:603-608`) — **a fresh Fundamental
   seed now graduates pieces out of the box with zero manual configuration**, closing the gap that
   an earlier draft of this document had flagged as an open product question needing your
   decision. `PoolPawnUpgrade`/`MorePawn`/`BetterPawn` remain a disjoint subsystem outside this
   tied set, with per-`FairyPawnUpgrades`-mode priority values preserved in relative order
   (`Config.cs:625-656`) — see §1.3, still unresolved on removal.

5. **No synthetic Pawn→Major/Pawn→Jack *implicit* fallbacks.** Per @chesslogic's explicit
   clarification of "the planned final list of actions" (`new-pawn=7, better-pawn=3,
   pawn-to-minor=6, minor-to-major=3, major-to-jack=2, minor-to-jack=2, major-to-queen=1,
   jack-to-queen=1, queen-to-amazon=1` — 9 named actions), gateways out of Pawn are normal,
   opt-in, symmetric configured actions (`pawn-to-minor`, `pawn-to-major`), not implicit/hardcoded
   fallbacks. If nothing is configured at all (impossible now given point 4's default, but still
   true for a deliberately empty custom config), every slot simply stays a Pawn — a well-defined
   outcome, not a special case.

6. **`BuildNonPawnPlan` double-counting bug fix — two stages, now extended for `pawn-to-major`.**
   Current code (`ItemGeneration.cs:695-747`, fix logic at `716-724`):
   `directCounts[Minor] = finalTierCounts[Minor] + AppliedCount(MinorToMajor) +
   AppliedCount(MinorToJack)`; `directCounts[Major] = lockedMajorCount +
   AppliedCount(PawnToMajor)`; `directCounts[Jack] = 0` (Jack, like Queen/Amazon, is reached
   exclusively via substitution, never a direct placeholder). This is the corrected form of a bug
   that went through two stages: the *original* bug added a placeholder count at every tier a slot
   ever passed through (double-placing pieces with no substitution left to consume them); a
   *first fix* set `directCounts[Major]/[Jack]` to just `finalTierCounts[Major]/[Jack]`, which was
   **itself still wrong** for non-locked slots reaching Major purely via
   Pawn→Minor→Major-by-substitution (already counted once via the Minor pass-through, then
   double-counted again as a phantom extra direct Major piece) — confirmed via real end-to-end
   generation (10 Chessmen all reaching Major produced 20 physical pieces instead of 10). Fixed by
   scoping `directCounts[Major]` to only genuinely-direct populations (locked Castlers +
   `pawn-to-major` slots). Regression test:
   `Plan_MajorSurvivorsDoNotReserveExtraDirectSlotsDuringGeneration`
   (`APMW.Test/FundamentalSlotGraduationPlannerTests.cs`) — the first test in that file to exercise
   the *real* end-to-end `PlayerPieceSetGeneration.Generate` pipeline rather than just `Plan()` in
   isolation, which is exactly why the original 7 unit tests missed this (they only asserted on
   `allocation.NonPawnCount(family)`, never on `PrecomputedNonPawnPlan.DirectCounts` or actual
   generated piece counts).

7. **Separately noted, NOT fixed (pre-existing, deliberate, still latent):**
   `Simulate`'s `actionsByPriority` collapses two configured actions that explicitly share both
   the same priority *and* the same `FromTier` by deterministically keeping the cheaper one, unless
   a `piece_upgrade_proportion` differentiates them (comment: "keep the cheaper one so that
   particular collision is at least deterministic"). This does not occur with @chesslogic's real
   planned chain (no two actions there share both priority and `FromTier` without an explicit
   proportion), so it's a latent edge case reachable only via a custom priority map with no
   proportions set — flagged for awareness, not requiring action.

8. **Fuzz-coverage gaps found and fixed** (both the original gap and one introduced by the
   concurrent commit's new default): `ListMinorToJackFirst`/`PriorityMapDisableMajorToQueen` fuzz
   profiles never configured `pawn-to-minor`, silently degenerating to "every slot stays a Pawn"
   post-redesign; fixed by adding `pawn-to-minor` to both. A new `FundamentalPlannedChain` profile
   encodes @chesslogic's exact planned action list. The concurrent commit further added a
   `TiedGraduationWithProportions` fuzz profile exercising ties across `pawn-to-minor`/
   `pawn-to-major`, `minor-to-major`/`minor-to-jack`, and `major-to-queen`/`jack-to-queen`
   (`ApmwFuzzCase.cs`), plus matching boundary fuzz cases in `ApmwFuzzCaseGenerator.cs`.

---

## 5. Deep-dive: chessmen-growth dilution-bug fix trade-offs

Four candidate approaches were rigorously analyzed (empirically probed, not just theorized) for
fixing §1.1's dilution bug. **None have been implemented — all require @chesslogic's sign-off**,
since every option deviates in some way from the literal "reset priority to max" design in §3.

### 1) Incremental / memoized simulation (persist exact state and only advance)

Persist each previous run's exact per-slot tier state; simulate forward from there (new slots
start Pawn; extra material/slots only ever add to the existing state).

- **Preserves greedy semantics exactly.** Empirically confirmed prefix-stable under growing
  Chessmen.
- **Risk**: no natural home for persisted state in the current "recompute everything fresh from
  `(N, materialBudget, config)` each time" architecture (used everywhere else, Legacy included).
  Cache invalidation and save-resume desync are real hazards — what happens if the persisted state
  and the freshly-supplied `(N, materialBudget, config)` disagree (e.g. after a config edit
  mid-game, or state loss)?

### 2) Depth-first / stable-order per-slot processing

Process slots in a fixed stable order (slot 0 pushed as far as budget allows, then slot 1, etc.)
instead of bulk/wave-advancing all eligible slots at a tier simultaneously.

- Empirically confirmed prefix-stable.
- **Materially changes what "priority" means** — from a global population-wide preference to a
  local per-slot tie-break. This likely **contradicts the intent behind @chesslogic's own
  weights** (e.g. `pawn-to-minor=6` >> `minor-to-major=3` reads as "spread upgrades broadly by
  priority across the whole population first," which depth-first would violate in favor of "max
  out earlier slots first"). Not recommended unless @chesslogic explicitly wants that product
  behavior change.

### 3) Pre-generated weighted-random event-type sequence

Generate a full sequence of "next action type" draws up front (weighted by priority/proportion),
independent of slot count, then apply them to slots in order.

- **Empirically disproven as a standalone fix.** Probed directly: `DistinctPriorities`-style
  config, 5 Chessmen → `{3 Queen, 2 Amazon}`; 9 Chessmen, same seed/material → `{3 Queen, 1 Jack,
  5 Major}` — still a regression (Amazons vanish, more Majors appear), just a *different* one. It
  does not, on its own, achieve prefix-stability. Not recommended as a standalone fix; would need
  to be combined with a richer history/state model (making it effectively option 1 or 4).

### 4) Event-sourced replay from authoritative received-item order (distinct from memoized cache)

Rebuild the simulation from the actual ordered item-received history (not just aggregate counts)
on every call — i.e., replay "at time T you had received items I1..Ik in this order" rather than
just "you currently have counts (N, M)".

- Preserves the exact greedy semantics (§3's "reset to max" rule) with no persisted mutable state
  — it's a pure function of the received-item-history prefix, computed fresh each time (fits the
  existing "recompute everything" architectural pattern much better than option 1).
- Empirically confirmed prefix-stable **for actual gameplay item-history prefixes** (i.e., stable
  across recomputations as real games actually progress, item by item).
- **Changes the invariant** from "count-lattice monotonic" (stable for *any* `(Chessmen, Material)`
  pair reachable from a smaller one, regardless of arrival order) to "received-item-history
  monotonic" (stable only along the *actual order items arrived in*). E.g., if all 9 Chessmen items
  arrive before any Material item, replay is allowed to spend material across all 9 at once —
  which is fine for real gameplay (item order is fixed by how the player actually received them)
  but means the planner is no longer a pure function of `(N, M)` alone; it needs the ordered
  history as an input.

### Cross-cutting conclusion

Options 1 and 4 are the only two that actually achieve genuine prefix-stability without changing
the meaning of "priority." Option 2 achieves stability at the cost of likely contradicting the
intent behind the user's own priority weights. Option 3 does not work standalone.

**Recommendation (pending explicit sign-off): event-sourced replay from
`ReceivedItemsHelper.AllItemsReceived` order**, with memoized incremental state as the backup
choice only if @chesslogic explicitly wants order-*insensitive* behavior within a live session.
Event-sourced replay is the best trade-off found because it preserves @chesslogic's explicit
greedy design ("reset priority to max"), gives real monotonic progression along actual gameplay
history, and avoids the cache invalidation / save-resume hazards of storing mutable planner state
in memory. Depth-first is not recommended unless @chesslogic explicitly wants the product behavior
to change from "spread upgrades broadly by priority" to "max out earlier slots first." The naked
weighted-sequence idea (option 3) is not recommended as a standalone fix.

---

## 6. Implementation note: integration point correction

While implementing, it was discovered that `PlayerPieceSetGeneration.Generate` independently calls
`NonPawnUpgradeGeneration.Plan(allocation, config)` to re-derive `DirectCounts`/`UpgradeActions`
from `allocation.NonPawnCount(family)` gross totals — re-netting from scratch using
`config.PieceUpgradePreferences`, rather than trusting whatever the allocator already decided. That
re-derivation assumes each family's gross count has a *single homogeneous source*. The old recipe
system satisfied this by construction; the new per-slot simulation can legitimately produce a
family whose count is a *mix* of origins (e.g. some slots direct-to-Major via `pawn-to-major`,
others Minor-to-Major via chain) — feeding that mixed gross count through the unchanged netting
would misattribute some slots and silently refund them as spare material instead of placing the
piece.

**Fix**: `PieceGenerationAllocation` gets an optional `PrecomputedNonPawnPlan` (null for Legacy).
`FundamentalSlotGraduationPlanner` builds the `NonPawnGenerationPlan` directly from its own precise
simulation tally — no re-netting needed. `PlayerPieceSetGeneration.Generate`'s one line becomes
`allocation.PrecomputedNonPawnPlan ?? NonPawnUpgradeGeneration.Plan(allocation, config)` — purely
additive; Legacy behavior is provably unchanged (still always null there, still always goes
through the same call as today). `UpgradeActions` is emitted in a fixed topological order over the
(small, hardcoded) upgrade DAG so `ApplyUpgrades`'s substitution always finds the placeholders it
needs already on the board. This also means the old `TryAddAmazonRecipeForQueenSourceAction`
special case is no longer needed: a slot can only reach Amazon by *actually* visiting Queen as a
real simulation step first, regardless of configured priority order.

---

## 7. Verification status

As of **2026-07-05**, reconciled against HEAD `a19ee88`:

- Full solution build (`dotnet build ChessV.sln --configuration Debug --no-restore`): **0
  errors**.
- `dotnet test APMW.Test\APMW.Test.csproj --no-restore --verbosity minimal`: **218 passed / 0
  failed / 7 skipped** (225 total).
- `dotnet test ChessV.Test\ChessV.Test.csproj --no-restore --verbosity minimal`: **116 passed / 0
  failed / 2 skipped** (118 total).
- The chessmen-growth dilution bug (§1.1) is independently re-confirmed reproducible against this
  exact HEAD — it is not stale/historical, it is a live, current-code characteristic.
- New test coverage from the concurrent `967b0df` commit (weighted tie-breaking, `pawn-to-major`,
  tied defaults, locked-major substitution) is in place and passing: notably
  `NonPawnUpgradeGenerationPlanTests.cs` (new file, 3 tests covering tied shared-source splitting,
  zero-proportion exclusion, and zero-proportion-as-last-resort), 6 new tests in
  `ApmwConfigTests.cs`, 2 new tests in `FundamentalSlotGraduationPlannerTests.cs`, and 1 new
  characterization test for locked-major substitution safety.

---

## 8. Document history

- **2026-07-05**: Initial version persisted to `docs/planning/` from agent session state (session
  `b80a1572-18af-4976-a053-d578d9503a8b`), at @chesslogic's explicit request ("saved to a
  docs/planning/ or docs/tracking/ folder... so that if there are any future issues with session
  state we can recover and investigate further"). Content reconciled against two concurrent
  commits (`967b0df`, `a19ee88`) that landed on `develop` mid-session from a separate, parallel
  agent session — see §4 point 4 for the specific open question (§1) those commits resolved
  (the Legacy-profile-default question) versus what they left untouched (§1.1, §1.2, §1.3).
