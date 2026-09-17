# Share exact deployment data across generator and client

Status: accepted
Date: 2026-09-13

ChessV and ChecksMate consume shared, versioned data containing exact starting
arrays and starting-role bindings. A versions-and-counts-only manifest leaves
the two consumers dependent on separate layout interpretations. Shared exact
data gives both consumers one deployment definition.

## Consequences

ChessV owns and publishes the versioned army data: piece identities, exact
starting arrays, and starting-role bindings. Movement and rule implementation
remain separate from deployment data.

ChecksMate owns Archipelago numeric Location IDs, names/aliases, and authored
difficulty values. It consumes a pinned version of the army data. The world
contract identifies the compatible versions.

The generator does not independently reconstruct the arrays. Ordinary-game
army definitions remain independent of Archipelago-specific bookkeeping.

This decision does not select the complete schema, distribution mechanism,
or version identifiers. It does not approve an array that remains unresolved.

## Source

[Define the versioned cross-repository contract](../../.scratch/large-board-enemy-armies/issues/17-define-versioned-cross-repo-contract.md#decision-record).
The user selected shared exact deployment data during the 2026-09-13
grill-with-docs round.
