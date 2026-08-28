# Keep one family selector and make augmentation geometry-driven

Type: grilling  
Status: resolved

## Question

Should the supported 10x10 and 12x10 CPU formations add new Enemy Army choices, or should geometry augment the family already selected?

## Answer

Keep the existing four-family Enemy Army selector unchanged. The selected family and formal board geometry together determine the CPU formation; geometry-specific augmentation is automatic and must not appear as a separate army choice. The formal set is exactly `6x8`, `8x8`, `10x8`, `10x10`, and `12x10`; new augmentation targets are 10x10 and 12x10, while 12x12 is unsupported.

The four visible choices remain Standard (FIDE), Colourbound Clobberers, Remarkable Rookies, and Nutty Knights. Unknown or absent values continue to require a deterministic Standard fallback.

## Provenance

- Current user decision: “Do not add special dropdown entries. Keep the existing Enemy Army family selector; augment CPU setup automatically by geometry.”
- `ChessV.GUI\Forms\ApmwForm.Designer.cs` defines the four visible choices.
- `ChessV.GUI\Forms\ApmwForm.cs` passes the selected value as `enemy_army`.
- `ChessV.Games\MiscellaneousGames\ApmwProfiles.cs` defines the four profile keys and `ResolveCpuArmy` fallback.
