# Publish a world-bound Location cost snapshot

Status: accepted
Date: 2026-09-15

The planned built-in tracker requires the same Location costs as the world.
The 0.4.0 contract will include a frozen cost snapshot for each generated
world. Tracker UI implementation remains outside this effort.

## Shared cost data

ChecksMate owns the calibration and publishes the snapshot.
The generator uses the same cost records that the client receives.
The client does not maintain a separate price table or independently
recalculate the calibration curves.

The snapshot records the calibration version and hash.
It binds to the compatible army, Location, and player-projection definitions.
It retains the separate per-geometry cost paths for each published Location.
A scalar minimum across boards cannot replace these paths.

Existing worlds retain their original costs.
New worlds can use revised numeric costs without new client binaries when
the schema and shared gameplay semantics remain compatible.
An authored correction still requires an explicit record.

The snapshot does not relax the new-contract-only compatibility boundary.
It also does not replace capability limits, actual capture conditions, the
resource bypass, or the requirement for one complete qualifying geometry path.

## Exclusions

The user did not defer the cost snapshot until tracker implementation.
Provenance metadata alone is insufficient for this contract.
A calibration revision does not automatically require matching rebuilt
client and sidecar binaries.

The snapshot schema and package integration remain specification work.
This decision does not authorize tracker UI or production implementation.

## Source

Q32: "Include the world-bound cost snapshot in 0.4.0; defer tracker UI".

[Contract record](../../.scratch/large-board-enemy-armies/issues/17-define-versioned-cross-repo-contract.md)
and [calibration record](../../.scratch/large-board-enemy-armies/issues/16-define-material-calibration-contract.md).
