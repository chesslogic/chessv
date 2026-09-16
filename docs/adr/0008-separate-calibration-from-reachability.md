# Separate formation calibration from runtime reachability

Status: accepted
Date: 2026-09-14

The estimator uses explicit geometry-matched calibration profiles and retains
their documented authored corrections. Player difficulty, projection caps,
and access across playable boards are separate steps. This preserves the
calibration inputs that a blanket base/grand minimum can hide.

## Consequences

An unavailable board cannot supply a cheaper reachable profile.
Historical data from another geometry can serve as an explicit derivation
anchor, but that does not make the reference board playable.
Each derived requirement belongs to its declared target formation.

Player piece/pawn configuration factors remain distinct from the client's
free Enemy Army choice. The Standard CPU reference does not remove those
player-configuration factors.

For capture goals, the required player-chessmen estimate is the required
capture count minus one, before projection caps. The allowance is for the
player's starting King, not one allowance per enemy King.
Regicide therefore has a zero additional-chessmen estimate, but still requires
its material estimate and actual capture condition.

These are generator estimates, not restrictions on earned checks.
A player who completes a capture below the estimate still earns its Location.

The later-board-unlock bypass remains as an explicit resource shortcut.
It does not waive actual goal conditions, geometry eligibility, or special
move conditions. No shortcut uses a stage beyond the configured endpoint.

Material retains one-unit precision. Apply difficulty and the applicable
absolute adjustment, round upward once, then apply the projected-stage cap.
Capture-series values must not decrease with count. Conflicts require explicit
correction rather than silent changes to authored anchors.

[Provisional endgame budgets](0009-anchor-provisional-endgame-budgets.md)
fix the endgame anchors. Individual accessibility transfer and series
continuation remain open. This decision does not approve the earlier local-only
material equation or choose a Regions topology.

The later [Regions decision](../../.scratch/large-board-enemy-armies/issues/20-decide-region-based-reachability.md)
defers that refactor from 0.4.0. The single-Region structure can still express
the approved board-specific rule alternatives.

## Source

[Material calibration contract](../../.scratch/large-board-enemy-armies/issues/16-define-material-calibration-contract.md#decisions-in-progress).
The user selected the recovered chessmen heuristic and separate calibration
profiles in the 2026-09-14 source-history interview.
