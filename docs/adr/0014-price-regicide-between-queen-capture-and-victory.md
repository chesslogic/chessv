# Price Regicide between Queen capture and victory

Status: accepted

Capturing a King requires a substantially greater budget than capturing a
Queen. Nominal King material does not represent that feat. The user placed
Regicide three quarters of the way from Queen capture to checkmate.

```text
Regicide_intrinsic = Q + 3/4 * (W - Q)
```

`Q` is the intrinsic Queen-capture requirement on 12x10.
`W` is the intrinsic checkmate requirement on 12x10.
Both inputs include their explicit authored corrections and retain fractions.
The initial Regicide-specific correction is zero.

With the selected references, `Q = 823700/121` and `W = 11220`.
Regicide therefore equals `1224140/121`, not a stored rounded 10117.
Difficulty and applicable absolute adjustment precede the single final
ceiling. The projection cap applies last.

This relationship assumes `Q <= W`. A correction that reverses that ordering
requires an explicit decision, not a negative gap or silent clamp.
The selected coefficient is a provisional pacing decision, not a historical
formula or a measured gameplay result.

Either CPU King still completes Regicide once in an initially multi-King
setup. The player-chessmen estimate remains zero.
This decision changes neither King material values nor the capture condition.
It adds no Regicide access path on a single-King starting board.

The earlier 4725 proposal, the midpoint alternative, and the full checkmate
budget are not selected.

## Source

[Material calibration contract](../../.scratch/large-board-enemy-armies/issues/16-define-material-calibration-contract.md),
round 12, Q29: "Three quarters of the Queen-to-checkmate gap, approximately
10117".

[Capture semantics](0007-count-captures-from-the-starting-army.md) and
[released individual references](0013-use-released-individual-capture-references.md)
remain separate decisions.
