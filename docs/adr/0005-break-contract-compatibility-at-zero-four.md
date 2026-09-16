# Use a new contract at the 0.4.0 release boundary

Status: accepted
Date: 2026-09-14

The 0.4.0 client supports the new declared contract, not legacy world
contracts. Older worlds use matching older clients. This removes the need to
maintain legacy gameplay and Location mappings inside the new client.

Legacy world-contract support is distinct from the existing Legacy itemization
mode. This decision does not remove an itemization mode.

## Consequences

Compatible client and world builds identify their shared contract and pinned
data. An application version of 0.4.0 alone does not establish compatibility.
The client rejects incompatible contracts before a match starts instead of
guessing mappings or substituting a different layout.

An old contract remains incompatible even when it requests a supported board
such as 8x8. Its incidental inclusion of a 12x12 record does not change this
release boundary. The separate 12x12 entry rejection also remains in force.

Numeric Location IDs can change for this release. They can also remain
unchanged where convenient. Neither universal renumbering nor legacy aliases
are requirements.

The exact schema, distribution mechanism, pins, and integration fixtures
remain specification work. This decision does not authorize production edits,
release publication, or conversion of existing worlds and progress.

## Source

[Versioned cross-repository contract](../../.scratch/large-board-enemy-armies/issues/17-define-versioned-cross-repo-contract.md#decisions-in-progress).
The user selected new-contract-only support in the 2026-09-14 interview.
