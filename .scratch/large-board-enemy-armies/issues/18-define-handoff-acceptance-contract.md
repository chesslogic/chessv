# Define the spec handoff and acceptance contract

Type: grilling  
Status: open  
Blocked by: 17

## Question

What evidence must a later cross-repository implementation spec require before geometry-driven CPU augmentation and semantic Locations are considered complete for exactly `6x8`, `8x8`, `10x8`, `10x10`, and `12x10`?

## Resolution requirements

- Exact golden setup arrays for every formal geometry, side, and Enemy Army family, including the augmented 10x10 and 12x10 arrays.
- FEN/setup, pawn/non-king/royal count, promotion, and fallback assertions.
- Check, royal-capture, stalemate, castling-right, and engine-evaluation characterization.
- Stable role-to-coordinate and old-to-new Location migration fixtures.
- Contract schema/hash/runtime consistency checks in both repositories for exactly the five formal geometries.
- Material table derivation snapshots, authored-offset review, and monotonicity checks.
- Explicit separation between deterministic automated acceptance and downstream play-test calibration.
- A compatibility matrix for old/new client, world, layout profile, and Location profile versions.
- Negative compatibility acceptance showing that the current/legacy 12x12 stage is rejected, hidden, or explicitly unsupported rather than accepted as a formal geometry.
- Named primary test surfaces, including `ChessV.Test\ApmwGeometryProfileTests.cs` and `APMW.Test\ApmwGameCharacterizationTests.cs`.
- A handoff rule that converts the resolved map into a spec and implementation tickets without reopening settled design questions.
