# Select CPU royal behavior by surviving King count

Status: accepted
Date: 2026-09-13

The additional ordinary King on 12x10 supplies an extra life. While multiple
CPU Kings survive, extinction behavior applies without compulsory check
avoidance. The last surviving King instead uses checkmate behavior.

## Consequences

Both rule components remain installed throughout the game. They apply under
complementary conditions rather than replacing rule objects mid-session.
The surviving count, not the primary-King designation, selects the phase.

Zero surviving Kings causes extinction defeat. This includes a Checkers move
that captures both Kings in one turn, without an intervening one-King position.
Loading and undo must recover the behavior for the resulting board state.

Only the original primary Mounted King owns the 12x10 CPU castling role.
The additional King does not inherit its castling rights.
Normal castling attack restrictions apply in both survival phases, even while
ordinary moves have no compulsory check avoidance. Castling cannot start,
cross, or finish on an attacked square.

Amazon is always non-royal and never contributes to the King-life count.

| CPU state | Terminal result |
| --- | --- |
| Zero Kings | CPU loses by extinction |
| Multiple Kings and no legal moves | CPU wins; human loses |
| One King, no legal moves, attacked | CPU loses by checkmate |
| One King, no legal moves, not attacked | CPU wins by the existing stalemate policy |

Zero-King extinction takes precedence over no-move classification. Human-side
rules remain unchanged. The owning ticket records the retained
geometry-derived castling coordinates and the remaining integration fixtures.

## Source

[Set royal, check, stalemate, castling, and evaluation semantics](../../.scratch/large-board-enemy-armies/issues/14-set-royal-and-castling-semantics.md#decision-record).
The user specified the extinction-to-checkmate transition and rejected castling
inheritance during the 2026-09-13 grill-with-docs rounds.
