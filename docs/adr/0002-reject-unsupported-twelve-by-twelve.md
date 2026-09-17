# Reject unsupported 12x12 games

Status: accepted
Date: 2026-09-13

The new supported geometry set is exactly `6x8`, `8x8`, `10x8`, `10x10`, and
`12x10`. The user clarified that no release includes 12x12 support. The new
behavior rejects 12x12 instead of preserving a legacy path or silently
converting it to 12x10.

## Consequences

The selector does not offer 12x12. A simple rejection at the shared geometry
entry point enforces the boundary without a broad migration mechanism.

Existing unreleased source and fixtures do not create a 12x12 support
obligation. Compatibility with other old contracts remains a separate
decision.

The rejection concerns entry into a 12x12 game. It does not by itself reject
every old manifest that mentions 12x12 while requesting another geometry.
Supported progression must still terminate at its configured goal without
requiring an unreachable 12x12 stage.

## Source

[Define the versioned cross-repository contract](../../.scratch/large-board-enemy-armies/issues/17-define-versioned-cross-repo-contract.md#decision-record).
The user rejected the proposed 12x12 legacy path during the 2026-09-13
grill-with-docs round.
