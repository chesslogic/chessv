# Decide whether Regions should express board and goal reachability

Type: grilling  
Status: resolved  
Blocked by: 15

## Question

Should 0.4.0 express the accepted per-geometry requirements and later-board
resource bypass through Archipelago Regions? If so, which graph preserves one
Location per goal and keeps every path's conditions coherent?

## Fixed inputs

- Each goal has one Location identity across geometries, not per-board copies.
- Intrinsic requirements belong to explicit target formations.
- The later-board-unlock bypass remains, but it waives only resource estimates.
- Actual capture conditions, geometry eligibility, and special move conditions
  remain in force.
- Capture Everything requires the configured ending geometry.
- The configured progression contains only the five supported geometries.

The accepted policies are in the semantic Location and material calibration
records. Exact remaining material coefficients do not block this graph
decision.

## Current-source facts

The generator puts every enabled Location in one `Menu` Region.
Source: sibling `worlds\checksmate\__init__.py:281-295`.

An Archipelago Location has one `parent_region`. Its reachability includes that
parent's reachability. The location register rejects duplicate names for a
player.
Source: sibling `BaseClasses.py:1295-1299,1481-1514`.

Thus, putting copies of the same named Location into several board Regions is
not a valid way to preserve a shared check.

This restriction does not prevent different entrance costs.
An Entrance has its own access rule and can connect to a Region that also has
other entrances. A shared goal Region can therefore hold one Location while
several board-specific entrances supply different thresholds.
Source: sibling `BaseClasses.py:1191-1220`.

## Decisions to resolve

- Include the Regions refactor in 0.4.0, defer it, or prototype it first.
- Choose how board-specific paths reach a shared goal without duplicate
  Locations.
- Keep material, chessmen, and board-dependent special conditions together on
  each qualifying path. Do not combine conditions from different boards.
- Represent the retained bypass without enabling absent boards, unsupported
  geometries, or an endpoint-only clear on an earlier board.
- Bound the regression fixtures and the graph's public interface.

## Representative cases

| Case | Required behavior |
| --- | --- |
| Queen available on 8x8 and 10x8 | One Location, reachable through either complete qualifying path |
| World starts at 10x8 | No cheaper 8x8 access path |
| Capture Any 15 on 8x8 in a larger world | The count goal can complete, but endpoint Capture Everything cannot |
| Later board unlocked | The accepted resource shortcut applies without removing actual goal conditions |
| Cross-board condition mixing | Material sufficient on one board and castlers sufficient only on another do not form one valid path |

## Comments

### 2026-09-14: Region suggestion

The user said: "Yes, we should keep the bypass, although it might be good to
refactor the existing behavior to use Regions".

Keeping the bypass is an accepted decision. A Regions refactor is a proposal,
not yet an accepted topology or permission to implement.

This ticket remains decision work. The sibling repository is read-only during
the current interview.

### 2026-09-14: Door-cost intent and deferral

The user described the intended model as one Location accessible through
multiple Regions, with a cost determined by the access door.
They then chose to defer the Regions refactor.

The door-specific threshold model is feasible through entrances into a shared
goal Region. Direct multi-parent Location registration is the restriction,
not alternative access rules. Deferral must not be recorded as a finding that
the desired semantics are impossible.

## Answer

Defer the Regions refactor from 0.4.0. Keep the existing single-Region
structure while implementing the accepted per-geometry access rules.

This does not defer board-specific requirements. A Location rule can combine
several qualifying board paths without changing its parent Region.
Each path must keep its material, chessmen, geometry, and special conditions
together before the rule takes the OR of those paths.
The retained later-board resource bypass remains explicit.

A future Regions refactor can represent the same paths as entrances into a
shared goal Region. It requires a separately selected scope.

This is a deferral, not a resolved implementation topology.
No sibling source or production graph changed. No new execution handoff was
published.
