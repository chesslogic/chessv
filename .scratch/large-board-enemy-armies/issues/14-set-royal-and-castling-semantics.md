# Set royal, check, stalemate, castling, and evaluation semantics

Type: grilling  
Status: resolved  
Blocked by: 11, 12, 13

## Question

Given the resolved piece identities and arrays, which CPU pieces are royal, what ends the game, which piece may castle with which castler, and which engine evaluations must recognize the new pieces?

## Decision record

### 2026-09-13: The additional King is an extra life

The user selected: "The additional King is an extra life; settle
extinction/check rules next."

The additional ordinary King on 12x10 is not merely a non-royal king-moving
piece. The subsequent rounds settle phase behavior, terminal outcomes, and
Amazon exclusion below. Human-side behavior remains a separate preserved or
explicitly changed contract.

The existing `CovenantRule` offers an all-specified-types-extinct loss condition.
The current APMW setup selects it through human King-upgrade state. That source
precedent does not settle the new CPU check or survival rules.

Sources: `ChessV.Games\Rules\Extinction\CovenantRule.cs`;
`ChessV.Games\MiscellaneousGames\ApmwChess.cs:184-200`.

### 2026-09-13: Extinction until only one CPU King remains

The user rejected the proposed requirement to keep at least one King safe while
multiple Kings survive. Only extinction behavior applies in that phase.

| Surviving CPU Kings | Chosen behavior |
| --- | --- |
| Two or more | Extinction survival, with no compulsory check avoidance |
| One | Checkmate behavior and compulsory protection of the last King |
| Zero | CPU defeat by extinction |

The rule depends on the surviving King count, not on which King survives.
The user specifically identified Checkers multi-capture as a case that can
remove multiple Kings in one turn. Removing both Kings must cause extinction
defeat without assuming an intervening one-King turn.

Both rule components exist throughout the game. Their active conditions are
complementary; the runtime must not swap rule objects mid-session.
Loading a position and undoing a capture must select the phase that matches
the restored surviving King count.

[CPU royal survival phases](../../../docs/adr/0003-cpu-royal-survival-phases.md)
records this decision. Exact no-legal-move behavior while multiple Kings remain
and the preserved APMW stalemate outcome are settled in the subsequent table.

### 2026-09-13: No castling inheritance

On 12x10, only the original primary Mounted King can castle with the existing
corner castler roles. The additional ordinary King never inherits that role.
Losing the primary Mounted King ends CPU castling eligibility. Undo must
restore the earlier rights rather than create new inherited rights.

This decision does not change the separate human back-rank Major/Jack castler
rule or the existing primary-King identity on other geometries.

### 2026-09-13: Amazon is non-royal

The user confirmed that Amazon is always non-royal and never supplies a King
life. Its capture does not change the survival phase or supply another royal
defeat condition.

### 2026-09-13: CPU terminal-state table

The user adopted the complete table:

| CPU state | Result |
| --- | --- |
| No Kings survive, including both captured by one Checkers move | CPU loses by extinction |
| Two or more Kings survive and legal moves exist | Continue without check obligations |
| Two or more Kings survive and no legal moves exist | CPU wins; human loses |
| One King survives, no legal moves, King attacked | CPU loses by checkmate |
| One King survives, no legal moves, King not attacked | CPU wins by the preserved stalemate policy |

The zero-King outcome takes precedence over no-move or attack classification.
With one King and available legal moves, normal checkmate-rule legality
applies. Human-side rules remain unchanged.

This supplies the missing multiple-King no-move result. Current
`CheckmateRule.NoMovesResult` returns an unhandled zero in that case; it is not
a valid preserved gameplay outcome.

### 2026-09-13: Castling keeps normal attack restrictions

The user chose to keep normal castling attack restrictions even when multiple
CPU Kings survive. The primary King cannot castle from, through, or onto an
attacked square in either survival phase.

This is an explicit castling exception to the absence of ordinary check
obligations while multiple Kings survive. Clear paths, existing rights, and
the original King/castler identities remain required. Human castling behavior
stays unchanged.

The existing geometry-derived destinations remain the baseline. In the table,
`K` denotes the primary King, and `R` denotes the corner castler role.
Colourbound destinations apply to the CPU Cleric corner castlers.

| Geometry | Queen-side, normal | King-side, normal | Colourbound difference |
| --- | --- | --- | --- |
| 6x8 | Disabled | Disabled | None |
| 8x8 | K E1-C1, R A1-D1 | K E1-G1, R H1-F1 | Queen-side R ends on E1 |
| 10x8 | K F1-D1, R A1-E1 | K F1-H1, R J1-G1 | King-side R ends on F1 |
| 10x10 | K F1-D1, R A1-E1 | K F1-H1, R J1-G1 | King-side R ends on F1 |
| 12x10 | K G1-E1, R A1-F1 | K G1-I1, R L1-H1 | Queen-side R ends on G1 |

Black uses the same files on its home rank: 8 for eight-rank boards and 10 for
ten-rank boards. The final 12x10 array supplies the Mounted King for this
primary role.

CPU rights retain `K/Q` for White and `k/q` for Black. Human rights retain
their source-file letters. A moved or captured primary King loses its rights;
returning or substituting another King does not restore them. Move undo and
supported FEN reconstruction must preserve the correct rights and actors.

For the preserved human rule, an eligible back-rank Major/Jack at source file
`s` uses King destination `KingFile +/- 2` and castler destination
`KingFile +/- 1`. A colorbound castler whose normal destination changes color
instead uses the vacated King file. This does not give the CPU B1/K1 Lions
castling rights.

Sources: `ChessV.Games\MiscellaneousGames\ApmwProfiles.cs:140-196`;
`ChessV.Games\MiscellaneousGames\ApmwChess.cs:537-598`;
`ChessV.Games\Rules\CastlingRule.cs:225-298`.

## Q33: Explicit CPU promotion permissions

The user adopted the proposed CPU-only lists without additional targets.
[ADR 0017](../../../docs/adr/0017-separate-cpu-promotion-permissions.md)
contains the exact lists.
The basic Elephant and legacy12 Nightrider extension are excluded.
Rookies retain Lion through their existing family list.
Other families do not gain Lion promotion from the outer pair.

CPU promotion permissions are independent of human inventory and pockets.
Absent or unknown family input uses the same Standard permissions as explicit
Standard.
Human entitlements remain separate from CPU type registration.
This resolves the promoted-Queen capability prerequisite on Standard 6x8.

## Q34: Basic Elephant is a Minor tactical target

The user selected: "Minor tactical target, without human pool or promotion
permissions".

Threatening a basic Elephant qualifies for Threaten Minor.
It does not qualify for Threaten Major or Threaten Queen.
Its capture keeps the starting Elephant role, separate from this tactical
classification.
War Elephant keeps its separate identity and existing classification.

This decision grants no human random-pool membership or promotion permission.
It changes neither the selected CPU promotion lists nor the 250/250 value.
The ordinary fork rules already consider non-pawn targets.
No new fork rule or evaluation bonus follows from the Minor classification.

## Specified integration requirements

The [royal lifecycle fixtures](../contracts/royal-lifecycle-fixtures.md)
define concrete positions for both surviving types, both capture transitions,
direct extinction, rollback, repeated load, and castling actors.
The multiple-King no-move branch has an explicit terminal-policy unit fixture.
It is not mislabeled as a demonstrated no-move board.

The [royal/capability source report](../research/royal-capability-frontier.md)
identifies the current rule seams and their limits.
The existing `ApmwStalemateRule` does not implement the new CPU phases.
Its human-mover early return does not update captured CPU royal membership.
Repeated loads also require reconstruction rather than additions to stale
royal sets.

The source investigation exposed the CPU promotion-list decision.
Current construction shares human, pocket, and CPU targets.
It also treats an absent raw Enemy Army value differently from explicit
Standard promotion input.
Q33 settles those lists independently of the earlier array decision.

Existing engine-evaluation policy remains the engineering default.
Mounted King recognition for royal survival and forks does not turn it into
an ordinary-King anchor for the back-rank trapping bonus.
The first release adds no invented Elephant/Amazon evaluation coefficients.
Q35 confirms this engineering default.
Q34 selects Minor for the basic Elephant tactical class.
Its CPU-only registration must not add a human random-pool or promotion
permission.

The selected policy and fixture document cover these requirements:

- geometry-specific fixtures for the accepted King-count-dependent CPU
  behavior and terminal-state table;
- how Extinction/Covenant and `ApmwStalemateRule` implement the accepted phases
  without changing the preserved human behavior;
- exact castling squares and rights for the original primary King and existing
  corner castlers, without rights for forward pieces or inheritance by the
  additional King;
- saved FEN castling-right identity and colorbound-castler parity;
- rook/open-file/seventh-rank, outpost, king-safety, and material evaluation registration for each new starting type.

## Resolution requirements

Provide truth tables for check legality, royal capture, no-legal-move outcomes, and castling rights for both human and CPU sides on each formal geometry (`6x8`, `8x8`, `10x8`, `10x10`, and `12x10`), distinguishing preserved baseline behavior from augmented behavior. Do not rely on base `CheckmateRule` multi-royal support without reconciling APMW's replacement rules.

## Answer

Q35 confirms the selected royal, terminal, castling, promotion, and tactical
policies, together with the preserved evaluation defaults.
The [royal lifecycle fixtures](../contracts/royal-lifecycle-fixtures.md)
specify the required transitions, actor identities, reporting boundaries,
and human-side isolation.

The design is resolved.
Later implementation must supply executed evidence for these fixtures.
This resolution does not authorize production edits or new evaluation tuning.
