# Use the existing CPU profile and setup seam

Type: research  
Status: resolved

## Question

Where does current ChessV behavior select a CPU family, compose its formation, and derive castling?

## Answer

`ApmwGeometryProfile` is the current seam. It owns geometry, king file, castling geometry, and a dictionary of `ApmwCpuArmyProfile` values. `ResolveCpuArmy` selects by Enemy Army string and falls back to Standard.

Current 10x8 and 10x10 stages share the ten-file back-rank profiles; 12x10 and 12x12 share the twelve-file profiles. `ComposeFenRows` currently emits exactly one CPU back rank, one full-width pawn rank, and an empty separation rank. Large-rank geometry does not yet add CPU forward pieces.

`ApmwChessGame` consumes the profile for FEN setup and registers profile-driven castling. Exact 10x10 and 12x10 augmentation should deepen this seam rather than create an unrelated setup path.

Scope distinction: the 12x12 profile above is a current-source fact, not a supported target. Formal support is exactly `6x8`, `8x8`, `10x8`, `10x10`, and `12x10`; a later contract must reject, hide, or otherwise mark the current/legacy 12x12 stage unsupported rather than extend its formation.

## Provenance

- `ChessV.Games\MiscellaneousGames\ApmwProfiles.cs`
- `ChessV.Games\MiscellaneousGames\ApmwChess.cs`
- `ChessV.Test\ApmwGeometryProfileTests.cs`
- `APMW.Test\ApmwGameCharacterizationTests.cs`
