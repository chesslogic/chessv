# Reject FEN-based APMW resume

Status: accepted
Date: 2026-09-17

## Decision

APMW does not support starting or resuming user play from an imported or
edited FEN.
The client must not offer that operation as a supported APMW continuation.
Internal FEN composition for generated setups remains permitted.
Ordinary-game FEN behavior is unchanged.

The user reaffirmed this boundary on 2026-09-16:

> Historically, we have disavowed support for FEN within APMW.

They proposed disabling resume from FEN.
The earlier audit's choice between supported import and analysis-only import
is therefore not an open requirement.
No new analysis mode is selected.

## Consequences

A FEN describes a position, not the starting-role lineage required by APMW.
Identical pieces with different capture roles can produce the same FEN.
Import must not invent their identities or grant new castling actors.

Embedding a starting FEN in an SGF file remains a possible recording feature.
It is not permission to resume APMW from that FEN.
The existing executable `FENStart` save variable is not automatically the
proposed informational metadata.
A later design must distinguish recording from executable restoration.

Internal rollback and lineage-aware reconstruction fixtures remain required.
They do not themselves promise a player-facing FEN resume or Undo command.
Location checks already accepted by Archipelago remain separate from
match-local rollback.

The user described a possible restriction on Undo if Location backtracking
is not fixed before 1.0.0.
That condition does not select an Undo deadline, a 0.4.0 restriction, or
removal of every control that can assist the player.
The interaction-support record separates those choices.
The subsequent Q36 response accepts genuinely view-only traversal and
favors reversible capture accounting over a blanket UI restriction.
It does not change the FEN-resume decision.
Q37 subsequently selects reliable Undo for 0.4.0 in
[ADR 0019](0019-support-reliable-apmw-undo-in-zero-four.md).

## Source

[APMW interaction support](../../.scratch/large-board-enemy-armies/issues/21-define-apmw-interaction-support.md).
The 2026-09-16 author statement supplies the support decision.
This ADR records design only; it does not authorize production changes.
