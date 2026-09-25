# ADR implementation question network

Status: Closure review - settled scope mapped, allocation and release closure blocked
Authority: Navigation only. Accepted ADRs and their linked contracts own behavior.
Baseline: `78b604e43f684c8032d1af23955485d1e196954f`

## Purpose and boundary

Identify the files, interfaces, dependencies, and acceptance evidence needed to
implement the finalized ADRs. Produce an architectural assessment and development
design for later parallel assignment. This work does not implement production
behavior, publish packages, or authorize unresolved settings choices.

The five investigation domains are library, handler, search, board, and
integration. These are investigation scopes, not a claim that current project
directories already isolate those responsibilities.

## Landed baseline

ADRs 0001-0021 and their linked accepted specifications define the main 0.4.0
contract. ADR 0022 selects several Legacy capabilities, but explicitly leaves
the reservation equation, composition targets, fallback, and portable contract
unfinished. Its `accepted` status does not finalize those fields.

The existing large-board map and question network remain historical decision
navigation. This assessment does not rewrite their decisions or closure claims.

## Breadth map

| ID | Question | Why it matters | Cases | Resolver | Edges | Status |
| --- | --- | --- | --- | --- | --- | --- |
| A01 | Which accepted requirements remain incomplete? | Defines implementable scope without selecting missing policy | All ADRs; ADR 0022 partial acceptance | Parent source analysis | All domains | Settled classification in assessment ADR matrix. D22 remains open |
| A02 | Where should transport admission, journals, and compatibility live? | Keeps reporting eligibility independent of connection lifetime | Takeover; reconnect; stale helper replay; incompatible world | Library probe | A03, A06 | Mapped to C1/N1/U1 |
| A03 | Which handlers own reversible game progress and immutable capture identity? | Prevents speculative or undone moves from corrupting progress | Promotion; multi-capture; failed commit; Undo | Handler probe | A02, A04, A05 | Mapped to S0/E1/H1 |
| A04 | Which search and rule paths must understand surviving royals? | Search and committed play must classify the same position | Two Kings; last King; extinction; castling actors | Search probe | A03, A05 | Mapped to R1/E1/U1. No core search rewrite |
| A05 | Which setup and board paths consume exact formation data? | Removes duplicated layouts without changing ordinary games | Twenty formations; reflection; five geometries; reject 12x12 | Board probe | A04, A06 | Mapped to B1/C1/P1/U1 |
| A06 | What joins both repositories and packaged consumers? | Source parity alone cannot prove release compatibility | Canonical hashes; rational costs; real x86/x64 projector packages | Parent integration probe | A02-A05, A07 | Mapped to G1/G2/P1/I1/I2. Final release waits for D22 |
| A07 | What can be assigned concurrently without shared-file races? | Makes fanout work executable rather than a topic list | Contract-first work; shared game file; project registration; blocked projection policy | Parent synthesis | All domains | Sixteen packages in spec and manifest, with explicit serial transfers |

## Representative cases

These are indices into authoritative fixture specifications, not replacement
fixtures. The final design must link their full construction and expected output.

| Case | Authority |
| --- | --- |
| Exact 10x10 candidate C and 12x10 Lion arrays, both colors | `.scratch\large-board-enemy-armies\issues\12-normalize-ten-by-ten-arrays.md`, ticket 13, shared-data contract |
| Additional King capture, primary King capture, direct two-King extinction, Undo | `contracts\royal-lifecycle-fixtures.md` in the same effort |
| Single final ceiling: intrinsic 361045/162, difficulty 27/20, adjustment 240, cap 5000 gives 3249 | `contracts\world-cost-snapshot-v1.md` |
| Controller change versus library admission; same-world/slot replay; helper retirement | `contracts\apmw-connection-reporting.md` |
| Minor-plus-Major funding 785; expected Major 485; concrete Rook 500 leaves 285 | ADR 0022 Q55; not a complete allocation algorithm |

## Pass 1 questions

Each probe must return exact existing paths and symbols, current-versus-required
behavior, proposed interfaces, exclusive write ownership, dependencies, test
surfaces, and unresolved decisions. Suggested new files must be labeled new.
Do not treat project names as proof of responsibility.

## Waysigns

Source analysis resolves implementation questions. Only unresolved
behavior-changing policy goes back to its decision owner. Parent synthesis owns
the design and file arbitration. No production implementation starts in this pass.

## Deferred and out of scope

- Unselected ADR 0022 algorithms and complete settings retirement decisions.
- Tracker UI, a Regions refactor, ordinary-game expansion, balance play-testing,
  durable reconnect journals, FEN-based APMW resume, and legacy 12x12 support.
- Automatic execution-workflow adapter repair and live implementation assignment.

## Source update map

- `assessments\library.md`: library and connection probe.
- `assessments\handler.md`: event, capture, history, and allocation probe.
- `assessments\search.md`: search, royal rules, and evaluation probe.
- `assessments\board.md`: exact setup, geometry, and board probe.
- `assessments\integration.md`: generator, cost, resource, and packaging probe.
- `assessment.md`: parent-owned architectural synthesis and integration evidence.
- `spec.md`: parent-owned development design and fanout contract.
- `fanout.json`: portable assignment inventory and dependency graph.

## Pass 2 - New source findings

| ID | Finding or question | Resolver | Effect and disposition |
| --- | --- | --- | --- |
| A08 | Pre-move setup can push history before a committed move fails | E1/H1 | Explicit commit/abort/Undo seam. No handler-only patch |
| A09 | Current history toolbar uses live committed Undo/replay | U1/E1/H1 | Preserve ledger-aware live behavior. Do not claim a view-only implementation |
| A10 | Controller replacement has no event and occurs outside menu clicks | E1/U1/N1 | Gate Match setters and automatic restoration before changed-controller progress |
| A11 | FEN apply and executable SGF variables can enter through different UI routes | U1 | Guard continuation before mutation. Preserve ordinary-game loading |
| A12 | Existing profile class names can collide with a new typed reader | C1/H1 | Use distinct v2 Location-profile type and remove the legacy interpretation through H1 |
| A13 | Python placement independently derives old human depths | P1 | Change both languages, not only the Army and client profile |
| A14 | Exact contract and cost schemas already exist | C1/G1/G2/I1 | Materialize fixtures. Do not request another product decision for existing fields |

## Closure review

All investigated domains have file-level evidence, an implementation owner, and
acceptance pointers in the development specification.
Every representative case maps to its full authoritative fixture.
Cross-domain files have one write owner.
The only planned duplicate write sets are explicit P1-to-A22 serial handoffs.

This is a partial execution design, not full release closure.
D22 blocks the allocator and final shared semantic pin.
The wider settings matrix remains a separate release gate.
S0 must materialize the selected interfaces before consumer fanout.
Real library/runtime and package evidence remains implementation work.

No source change, package publication, or gameplay validation is claimed.
