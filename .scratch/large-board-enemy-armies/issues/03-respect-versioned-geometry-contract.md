# Respect the versioned geometry and projection contract

Type: research  
Status: resolved

## Question

Which existing geometry and projection contracts constrain a cross-repository CPU-layout design?

## Answer

The current v2 contract fixes the stage order `8x8`, `10x8`, `10x10`, `12x10`, `12x12`, exposes per-stage CPU pawn/non-king counts, and identifies CPU layout and Location profile versions. Current counts still model 10x10 as 10 pawns plus 9 non-kings and both twelve-file stages as 12 pawns plus 11 non-kings, matching a single back rank rather than the intended augmentation.

ChessV validates the same contract shape and performs geometry selection from it. The semantic projector has geometry-driven placement/capacity logic for the player roster, but the CPU contract currently contains only profile versions and family IDs, not exact CPU coordinate arrays or role identities.

The JSON manifest hash is canonical and frozen across runtime/protocol/build/world tests. Any schema or value change needs an intentional version/hash migration rather than an uncoordinated local constant change.

Scope distinction: the five-stage order above records the current v2 source contract, not the newly settled support scope. Formal support is exactly `6x8`, `8x8`, `10x8`, `10x10`, and `12x10`; the current/legacy 12x12 record is compatibility evidence whose migrated contract behavior must be explicitly rejected, hidden, or otherwise unsupported.

## Provenance

- `APMW.Client\ApmwGeometrySelection.cs`
- `APMW.Client\ApmwContractV2.cs`
- `APMW.Client\ProjectionV2SemanticEngine.cs`
- `C:\GitHub\rft50-checksmate\worlds\checksmate\apmw_projection\data\apmw_contract_v2.json`
- `C:\GitHub\rft50-checksmate\worlds\checksmate\apmw_projection\resource.py`
- `C:\GitHub\rft50-checksmate\worlds\checksmate\apmw_projection\__init__.py`
- `C:\GitHub\rft50-checksmate\worlds\checksmate\test\test_apmw_contract_v2.py`
- `C:\GitHub\rft50-checksmate\worlds\checksmate\test\test_contract_runtime_consistency.py`
