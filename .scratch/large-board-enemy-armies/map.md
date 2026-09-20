# Geometry-driven large-board enemy armies

Status: Q35 design resolved. Additional settings coverage is open before full handoff.

## Destination

An execution-ready cross-repository design for the formal geometry set `6x8`, `8x8`, `10x8`, `10x10`, and `12x10`, including exact per-family placement arrays for geometry-driven CPU army augmentation on 10x10 and 12x10, royal/check/castling semantics, stable role-based ChecksMate Location identity, and a calibrated material contract suitable for a later spec and ticket handoff.

## Notes

- This is a planning map, not production implementation.
- [Decision-resolution question network](qn_resolution-rounds.md) tracks the
  current breadth, partial answers, and next decision frontier.
- The [2026-09-16 follow-up audit](qn_resolution-rounds.md#pass-4-follow-up-breadth-audit-2026-09-16)
  covers architecture, Fundamental itemization portability, Options, and
  historical behavior. The original 20 decisions remain resolved.
  A complete settings support and retirement record remains necessary for
  the broader 0.4.0 handoff.
- [APMW interaction support](issues/21-define-apmw-interaction-support.md)
  records the rejected FEN-resume path and reliable Undo in 0.4.0.
  Computer takeover is permitted, with permanently disabled progress
  reporting for that match.
  Q45 permits earlier library submissions to finish, but no new submission
  or journal replay after takeover.
- [APMW reporting-status badge](issues/22-specify-apmw-reporting-status.md)
  distinguishes permanent match reporting loss from an unavailable server
  connection. Reconnection resends the match's earned Location set.
  Journals remain in process memory after window closure; no disk format or
  restart recovery is planned.
  Q43 gives the separate final-goal marker the same lifetime and replay gates.
- [Connection lifecycle](issues/23-specify-connection-lifecycle.md) owns safe
  repeated attempts, cleanup, and room-bound journal reattachment.
  Source and primary-library research are complete.
  Q42 selects normal AP generation-name/team/slot identity with
  frozen-contract checks and no new identifier.
  Q45 selects a library-admission cutoff without a dependency extension.
  The [bounded engineering contract](contracts/apmw-connection-reporting.md)
  completes the stock adapter, helper containment, and retirement design.
  Tickets 22 and 23 are resolved. Runtime proof remains implementation work.
- [Settings support and retirement](issues/24-specify-settings-support-and-retirement.md)
  owns the remaining public settings choices and the complete support
  matrix. It does not reopen retained Legacy itemization.
- [Recovered guarantee intent](research/fair-board-guarantee-prompt-intent.md)
  identifies a distinct best-effort Pawn composition purpose for
  `standard_and_pawns`, explicitly discussed with Fundamental.
  Q56's one-mode simplification is unselected; Off does not remove Chessmen grants.
- The [guarantee enhancement breadth pass](qn_resolution-rounds.md#pass-6-guarantee-enhancement-and-instrumentation-2026-09-19)
  separates composition precedence, quotas, pockets, shortages, lifecycle,
  explanations, and cross-repository agreement.
  Local diagnostics and a setup preview are proposals, not implemented behavior.
  The user's viability objection now precedes composition priority.
  Q58 selects best-effort preferences with shared fallback and generation validation.
  Q59 selects total-unit and non-Pawn minimums, with no Pawn floor or implied ceiling.
  Numeric target pairs and the exact viability rule remain open.
  Completed `locked_items` research distinguishes guaranteed pool resources
  from runtime allocation. Its distribution trace confirms ordinary weighted
  draws that satisfy reserved minima, not a seed-first process.
  No intermediate baseline phase or generator-algorithm port is selected.
- Formal support is exactly `[6,8,10]x8` plus `[10,12]x10`: `6x8`, `8x8`, `10x8`, `10x10`, and `12x10`.
- Existing 12x12 source and fixtures are historical evidence, not a shipped
  compatibility obligation. The 2026-09-13 decision rejects 12x12 and removes it
  from the selector. No legacy 12x12 support path is planned.
- Preserve the existing four-family Enemy Army selector. Geometry augments the selected family automatically; no new dropdown entries are planned.
- Source authority is: decisions supplied for this effort, then current source/docs/tests, then historical prompt lineage.
- Work array questions as concrete prototypes with the user. Work semantic Location naming with both grilling and domain-modeling.
- Treat `ChessV.Test\ApmwGeometryProfileTests.cs` and `APMW.Test\ApmwGameCharacterizationTests.cs` as primary ChessV characterization surfaces.
- The local sibling at `C:\GitHub\rft50-checksmate` is evidence only for this map. Do not edit or publish it from this effort.
- A map resolution may describe required cross-repository behavior and ownership, but production patches, migration execution, and calibration runs begin only after this map is complete and handed off.
- The decision-to-execution repository adapter remains absent. Generic
  planning is available, but automatic projection and implementation through
  that workflow remain disabled. This pass does not repair the adapter or
  publish an execution handoff.

## Decisions so far

- [Keep one family selector and make augmentation geometry-driven](issues/01-keep-one-family-selector.md) — the existing four Enemy Army families remain the only UI choices.
- [Use the existing CPU profile and setup seam](issues/02-use-existing-cpu-profile-seam.md) — geometry profiles, family resolution, FEN composition, and castling plans are the current integration boundary; its current 12x12 behavior is compatibility evidence, not support scope.
- [Respect the versioned geometry and projection contract](issues/03-respect-versioned-geometry-contract.md) — current stage capacities and CPU profile versions, including the legacy 12x12 record, are cross-repository compatibility data with frozen-hash coupling.
- [Use catalog facts without assuming rule compatibility](issues/04-use-catalog-facts-with-rule-review.md) — movement/value facts are known, but additional royals, castlers, and rook-like evaluation need explicit semantics.
- [Treat current ChecksMate targets as hand-authored compatibility data](issues/05-treat-checksmate-targets-as-hand-authored.md) — Locations, thresholds, counts, and legacy 12-file scaling are static authored series rather than a derived material model; they do not make 12x12 supported.
- [Rank current decisions above historical lineage](issues/06-rank-current-decisions-above-lineage.md) — prior Amazon and added-rank decisions constrain the discussion but do not authorize a CPU square.
- [Target a family-preserving 10x10 augmentation budget](issues/07-target-family-preserving-ten-by-ten-budget.md) — aim for roughly twenty pawn units without erasing family identity; the later correspondence decision supplies the first-release Elephant pair.
- [Carry forward the settled formation constraints](issues/08-carry-forward-formation-constraints.md) — the edge wedge and pawn screen, middle-out insertion, bishop-color protection, king alignment, and queen/Amazon roles constrain the unresolved arrays.
- [Adopt hybrid material recalibration](issues/09-adopt-hybrid-material-recalibration.md) — catalog deltas seed new values, while authored offsets and calibration remain part of the contract.
- [Migrate file-based Locations to role identity](issues/10-migrate-locations-to-role-identity.md) — a square's strategic role, not its file letter, should determine Location identity.
- [Specify the family correspondence matrix](issues/11-specify-family-correspondence.md) — added Bishop/Knight pairs reuse each selected army's established correspondents; the first release uses a universal 250/250 Elephant pair, a universal Amazon, family-specific forward Queens, a primary Mounted King, and an additional ordinary King.
- [Allocate CPU, neutral, and human formation bands](issues/19-allocate-formation-bands.md) — 8-rank boards use 2/1/5 CPU-neutral-human ranks; 10-rank boards use 3/1/6, with the added human rank mixed and the neutral rank empty at setup.
- [Normalize exact 10x10 placement arrays](issues/12-normalize-ten-by-ten-arrays.md) — candidate C retains the home rows, with twelve pawns and `Elephant, Knight-role, Bishop-role, Bishop-role, Knight-role, Elephant` on C2-H2.
- [Normalize exact 12x10 placement arrays](issues/13-normalize-twelve-file-arrays.md) — 34 pieces, fourteen pawns, universal 500/500 Lions on B1/K1, and the approved two-rank Amazon/Queen/Mounted-King/King center.
- [Define semantic capture Locations](issues/15-define-location-role-identity.md) — the complete role catalog, pawn names, Regicide, King-count eligibility, endpoint clear condition, and capture-series ceilings are resolved.
- [Royal, castling, promotion, and evaluation policy](issues/14-set-royal-and-castling-semantics.md) - Q35 confirms the selected rules, Minor Elephant, preserved coefficients, and independent lifecycle fixtures.
- [Material calibration contract](issues/16-define-material-calibration-contract.md) - released references, exact formulas, initial tables, and correction records are resolved.
- [Versioned cross-repository contract](issues/17-define-versioned-cross-repo-contract.md) - exact schemas, world costs, strict pins, and offline publication order are resolved.
- [Handoff acceptance contract](issues/18-define-handoff-acceptance-contract.md) - owner roles, behavioral evidence, and packaged integration requirements are resolved.
- [APMW interaction support](issues/21-define-apmw-interaction-support.md) - reject FEN resume, support reliable Undo in 0.4.0, and permit controller changes with permanent match-local progress disconnection.
- [Replay identity](issues/23-specify-connection-lifecycle.md#q42-selected-replay-identity) - Q42 retains normal AP identity and frozen-contract checks. No new generation identifier or collision-proof identity guarantee is selected.
- [Final-goal replay](issues/22-specify-apmw-reporting-status.md#q43-final-goal-retention) - Q43 retains the earned goal marker with the process-memory Location journal, including after window closure.
- [Connection/reporting contract](contracts/apmw-connection-reporting.md) - stock 6.6.0, atomic library admission, fresh helper state, positive retirement evidence, and bounded bare-host routes. No transport fork or guaranteed hard abort.
- [Legacy pawn-material conversion](issues/24-specify-settings-support-and-retirement.md#q44-selected-capability) - Q44 retains the capability for 0.4.0. Q49 sets prepared-board targets to 11/15/19/27/33. Its conversion rule and Fundamental applicability remain open.
- [Guarantee count basis](issues/24-specify-settings-support-and-retirement.md#q46-count-basis) - Q46 counts actual deployment plus pocket contents, not received items or board reserves. Q47 retains the existing rank rules; Q48 uses the board being prepared.
- [Legacy funding](issues/24-specify-settings-support-and-retirement.md#q51-retain-other-legacy-surplus-funding) - Q51 retains other eligible Legacy surplus alongside Pawn-item credit. Explicit transfers must prevent reserve double spending.
- [Legacy approximation](issues/24-specify-settings-support-and-retirement.md#q53-keep-the-historical-approximation-rules) - Q52 floors net surplus at zero. Q53 retains historical allowance and overspending rules, not the proposed 100-centipawn cap.
- [Superseded placement material](issues/24-specify-settings-support-and-retirement.md#q55-preserve-material-through-supersession) - Q55 preserves all involved Legacy credit. The example's funding base stays 785 while the resulting Major's expected value stays 485.

## Accepted ADRs

- [Retain Legacy pawn-material conversion](../../docs/adr/0022-retain-legacy-pawn-material-conversion.md) - retain fewer, stronger units from Pawn material when the reservation permits conversion. The rule and contract revision still need specification.
- [Reject FEN-based APMW resume](../../docs/adr/0018-reject-fen-based-apmw-resume.md) - a starting FEN in SGF is a recording idea, not resume authority. Internal rollback remains required.
- [Support reliable APMW Undo in 0.4.0](../../docs/adr/0019-support-reliable-apmw-undo-in-zero-four.md) - extend the reversible move ledger instead of disabling backtracking. Local rollback does not retract accepted checks.
- [Stop progress after controller changes](../../docs/adr/0020-stop-apmw-progress-after-controller-changes.md) - allow takeover, but permanently stop new Location and final-goal submissions. Earlier library submissions can finish. Keep the AP session connected.
- [Replay earned Locations after reconnect](../../docs/adr/0021-replay-earned-locations-after-reconnect.md) - preserve the match and resend its earned-ID set to the same world/slot. Server deduplication permits repeated reports.
- [CPU royal survival phases](../../docs/adr/0003-cpu-royal-survival-phases.md) — extinction with multiple Kings, checkmate behavior with the last King, CPU victory on stalemate, and no castling inheritance.
- [Shared exact deployment data](../../docs/adr/0001-shared-exact-deployment-data.md) — ChessV publishes army data; ChecksMate owns AP Location mappings and authored difficulty, with pinned compatible versions.
- [Reject unsupported 12x12 games](../../docs/adr/0002-reject-unsupported-twelve-by-twelve.md) — no selector option or legacy support path.
- [Standard-army Location calibration](../../docs/adr/0004-standard-army-location-calibration.md) — use the approved Standard family and midgame values, not a maximum across family choices.
- [The 0.4.0 contract boundary](../../docs/adr/0005-break-contract-compatibility-at-zero-four.md) — new-contract-only client support; older worlds use matching older clients. Numeric Location IDs can change but do not require universal renumbering.
- [Starting-role capture names](../../docs/adr/0006-name-capture-locations-by-starting-role.md) — stable Standard/FIDE names across families, with distinct Center Rook and Queen checks. The [glossary](../../CONTEXT.md) records the resolved vocabulary.
- [Starting-army capture counts](../../docs/adr/0007-count-captures-from-the-starting-army.md) — Regicide and King counting use the initial multi-King setup. Capture Everything clears all non-Kings and spare King lives on the configured ending board.
- [Separate calibration and reachability](../../docs/adr/0008-separate-calibration-from-reachability.md) — geometry-matched intrinsic requirements, separate runtime adjustments, and captures-minus-one player-chessmen estimates.
- [Anchor provisional endgame budgets](../../docs/adr/0009-anchor-provisional-endgame-budgets.md) — retain the established 8x8/10x8 anchors and use exact inventory deltas for the new formations.
- [Transfer capture-series curves](../../docs/adr/0010-transfer-capture-series-curves-by-endpoints.md) — retain authored reference curves through endpoint-relative interpolation and clear-budget scaling, with explicit corrections.
- [Preserve task-specific calibration](../../docs/adr/0011-preserve-task-specific-calibration.md) — retain other authored task budgets and tie True Royal Fork to the target checkmate budget.
- [Floor non-pawn accessibility growth](../../docs/adr/0012-floor-non-pawn-accessibility-growth.md) — bounded targets receive half of added other material as a minimum growth buffer; sliders receive one quarter.
- [Use released individual references](../../docs/adr/0013-use-released-individual-capture-references.md) — Lions use released Knight analogues. The bank includes the compact downward transfer and zero initial additional non-pawn corrections.
- [Price Regicide near victory](../../docs/adr/0014-price-regicide-between-queen-capture-and-victory.md) — its intrinsic requirement uses three quarters of the Queen-to-checkmate gap, without intermediate rounding.
- [Pawn curves and Rearguard premiums](../../docs/adr/0015-price-pawns-with-curves-and-rearguard-premiums.md) — side-specific cubic curves and a central-pressure law set the baseline. Rearguard adds one quarter of the same-file main pawn's accessibility budget.
- [Publish world-bound cost snapshots](../../docs/adr/0016-publish-world-bound-location-cost-snapshots.md) — the 0.4.0 contract supplies the same frozen costs to the generator and client. Tracker UI remains deferred.
- [Separate CPU promotion permissions](../../docs/adr/0017-separate-cpu-promotion-permissions.md) — explicit family/geometry lists do not borrow human entitlements. No basic Elephant or legacy12 Nightrider promotion is added.

## Q35: Design closure

Q34 settles the last open gameplay field.
Basic Elephant is a Minor tactical target without human pool or promotion
permissions.
The user selected: "Confirm the design; mark the map ready for a later
handoff".
This confirms shared understanding of the full packet.
All 20 decision tickets are resolved.

| Surface | Owning specification | Closure point |
| --- | --- | --- |
| Arrays and formation | Tickets 11-13 and 19 | Five geometries, four families, both colors, approved Lion pair, and exact capacities |
| Royal rules and promotions | Ticket 14, ADRs 0003/0017, and royal fixtures | Selected terminal table, original-primary castling, explicit CPU lists, Minor Elephant, and separate human entitlements |
| Capture semantics | Ticket 15 and ADRs 0006/0007 | Starting-role identities, initial King eligibility, full series coverage, and endpoint-only Capture Everything |
| Calibration | Ticket 16 and ADRs 0004/0008-0015 | Released references, selected exact formulas, provisional tables, and reviewed correction records |
| Shared data and packaging | Ticket 17, ADRs 0001/0005/0016, and shared-data specification | Offline publication, strict semantic and package pins, world-bound costs, and old-world rejection |
| Acceptance and ownership | Ticket 18 and both fixture specifications | Named cross-repository evidence, complete geometry paths, lifecycle isolation, and real packages |

The confirmed engineering defaults preserve existing evaluation coefficients.
Existing Lion and Mounted King hooks follow registration.
The ordinary-King-only trapping anchor remains unchanged.
Basic Elephant and Amazon receive no new invented bonuses.

Actual serialization, hashes, release pins, and executed acceptance results
are implementation deliverables.
Their absence does not reopen a design choice.
The calibration tables are initial estimates, not proof of gameplay balance.

This sign-off does not authorize production work, publication, adapter
repair, or an execution handoff.
The later handoff must assign bounded implementation ownership and blocking
edges without reopening these decisions.

## Supporting specifications and later handoff

- The final spec and implementation-ticket partition follow design closure.
  Ticket 18 defines acceptance ownership without assigning live agents.
- The [material history report](research/material-calibration-history.md)
  reconstructs the existing resource/accessibility model and count rules.
  The [0.3.2 reference audit](research/calibration-reference-provenance.md)
  confirms the released base8/grand10 bank and separates later additions.
  Release presence does not establish gameplay calibration. The reference
  bank, curved pawn model, and Regicide tier are selected.
  The Rearguard premium is selected. The
  [individual-capture report](research/individual-capture-model.md) and its
  105-row table cover all five geometries, alongside the existing series,
  endgame, and task tables. Ticket 18 and the fixture documents specify
  integration acceptance.
- The [contract-mechanics report](research/cross-repo-contract-mechanics.md)
  separates generator goal budgets from integer player projection metrics.
  The planned built-in tracker requires synchronized cost definitions.
  Q32 selects a world-bound cost snapshot in 0.4.0, without tracker UI.
  The [shared-data specification](contracts/shared-data-contract-v4.md)
  supplies the concrete schemas, binding graph, and publication order.
- The [royal/capability report](research/royal-capability-frontier.md)
  identifies the current promotion-list coupling.
  Q33 selects explicit CPU lists without basic Elephant or the former
  legacy12 Nightrider extension. The approved starting arrays remain unchanged.
- Q34 settles Basic Elephant's tactical classification as Minor.
  Q35 confirms the full packet for a later handoff.

## Out of scope

- [Regions refactor](issues/20-decide-region-based-reachability.md) — deferred
  from 0.4.0. Per-geometry thresholds and the retained bypass still belong in
  the existing Location rules.
- Production implementation in ChessV, including source, tests, README, or release notes.
- Exact edits, commits, or pull requests in `chesslogic/Archipelago` / `C:\GitHub\rft50-checksmate`.
- PopTracker implementation or presentation changes.
- Built-in tracker UI implementation. Its world-bound cost data is in scope.
- Running play-test calibration or tuning thresholds from play results; this map only defines the later calibration contract.
- Redesigning player itemization, including `major_to_amazon`, except where its existing contract constrains interoperability.
- Adding special Enemy Army dropdown entries.
- Formal support or a legacy support path for 12x12, including placement arrays,
  calibration targets, or an accepted contract stage. Existing source does not
  override the explicit rejection decision.
- Creating an ADR, `CONTEXT.md`, or `CONTEXT-MAP.md` before a canonical term or durable architectural decision is actually resolved.
- A future family-specific 250-material light-piece quartet. The first release uses Elephants; any later new piece types must use notation and icons not already assigned in APMW.
