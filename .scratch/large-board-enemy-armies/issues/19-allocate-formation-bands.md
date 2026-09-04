# Allocate CPU, neutral, and human formation bands

Type: grilling  
Status: open  
Blocked by: 03

## Question

How many ranks does each side own on every formal geometry, and where does the
neutral separation band live once 10x10 and 12x10 use multi-rank CPU formations?

Current contract behavior and legacy generation disagree:

- legacy generation has a five-rank human envelope;
- contract v2 and the current world v3 records give 10x10 and 12x10 seven human
  formation ranks, with capacities 69 and 83;
- current FEN composition reserves two CPU ranks, one empty rank, and all
  remaining ranks for the human;
- the proposed CPU armies require more than two CPU ranks.

The array prototypes must not silently overlap a seven-rank human projection or
remove the neutral band.

## Resolution requirements

- Define CPU, neutral, and human rank ownership for `6x8`, `8x8`, `10x8`,
  `10x10`, and `12x10`.
- Decide whether five human ranks becomes a formal invariant for the two
  ten-rank geometries.
- Recalculate active capacity, reserve, missing-material, and Pawn Forwardness
  semantics for any changed human depth.
- State whether the neutral band is required, optional, or absent by geometry.
- Define orientation-independent transforms for both human colors.
- Require setup validation that CPU and human slots never overlap.
- Identify the contract fields and profile versions that carry the allocation.
- Keep current/legacy 12x12 evidence explicitly unsupported rather than deriving
  a selectable formation from it.

## Provenance

- `APMW.Client\ActiveRosterProjection.cs`
- `APMW.Test\ActiveRosterProjectionTests.cs`
- `APMW.Test\ApmwGeometryAwareProviderIntegrationTests.cs`
- `ChessV.Games\MiscellaneousGames\ApmwProfiles.cs`
- `worlds\checksmate\apmw_projection\data\apmw_contract_v3.json` at
  `cb6d77ce98536f5eb4e1afc4a959106f6beaf0ca`
