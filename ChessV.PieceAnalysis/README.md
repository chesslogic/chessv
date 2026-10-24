# ChessV.PieceAnalysis

Headless console tool for ChessV piece mobility analysis across board geometries
and the standard density sweep. Reports compare pieces against the APMW tier
roster and existing material values. The tool does not change game values.

## CLI

```text
ChessV.PieceAnalysis.exe [options]
dotnet run --project ChessV.PieceAnalysis -- [options]
dotnet exec <path-to-dll> [options]
```

Options:

- `--piece <Name>` repeatable piece class name from `ChessV.Games` (example: `ShortRook`, `Queen`, `Cannon`)
- `--all` analyze every constructible `PieceType` subclass in `ChessV.Games`
- `--apmw` analyze the full APMW catalog, including Pawns, Pawn variants, Sergeants, and King variants
- `--compare-8x8` include an independent 8x8 baseline and mobility ratios for each piece
- `--tier <TierName>` optional hint for pieces without an assigned tier: `Weak|Pawn|Minor|Major|Jack|Queen|Amazon`
- `--files <N>` board files, default `8`
- `--ranks <N>` board ranks, default `8`
- `--out <path>` write JSON to a file instead of stdout

Behavior:

- Without `--piece`, `--all`, or `--apmw`, the tool analyzes the curated APMW tier roster.
- `--apmw` cannot combine with `--piece` or `--all`.
- All APMW catalog pieces retain their canonical notation and existing MG/EG values on every analysis board.
- An unpriced engine piece has `materialValueSource: null`. Its constructor placeholder of zero is not a material estimate.
- Stdout is JSON only. Human diagnostics such as ignored tier hints are written to stderr.
- `--all` reports per-piece construction failures in `pieces[].constructionError` without aborting the run.
- Explicit `--piece` names that cannot be found or constructed are reported in `unresolvedPieces`.

## Geometry comparisons

Example:

```text
dotnet run --project ChessV.PieceAnalysis -- --apmw --files 12 --ranks 10 --compare-8x8
```

The 0.4.0 valuation study covers 6x8, 8x8, 10x8, 10x10, and 12x10.
Other analysis dimensions do not establish supported APMW game geometries.

The report sets `eightByEightThresholdsApplied` to `false` outside 8x8.
The 8x8 absolute thresholds and fixed tier-value comparisons do not apply there.
Statistical comparisons still use tier peers on the requested geometry.
The peer population remains the curated tier roster, including in `--apmw` mode.

Each piece includes these additional fields:

| Field | Meaning |
| --- | --- |
| `materialValueSource` | `ApmwChess` for canonical catalog values, or `null` for an unpriced engine piece |
| `analysisLimitations` | Movement behavior that the directional mobility calculation does not model |
| `emptyBoardTravel` | Empty-board noncapturing travel statistics for player 0 |
| `comparison` | The 8x8 mobility and travel baseline, or `null` without `--compare-8x8` |

The comparison includes `baselineBoard`, `baselineMobilityByDensityPercent`,
`mobilityRatioByDensityPercent`, and `baselineEmptyBoardTravel`.
Each ratio divides requested-board mobility by baseline mobility at the same density.
A zero baseline produces a `null` ratio, not zero or infinity.
These ratios are diagnostics, not recommended material multipliers.

### Empty-board travel

The travel model uses shortest paths between ordered pairs of distinct squares.
It includes move-only capabilities and excludes capture-only capabilities.
It ignores occupancy, checks, promotions, and game-added movement rules, including Pawn double moves.

| Field | Meaning |
| --- | --- |
| `model` | `empty_board_noncapturing_player_0` |
| `unsupportedReason` | A limitation that prevents this travel model from analyzing the piece, or `null` |
| `reachableSquareFraction` | Reachable ordered pairs divided by all ordered pairs of distinct squares |
| `averageMovesToReachableSquare` | Mean shortest-path length for reachable pairs only |
| `homeToFarRankReachableFraction` | Reachable home-rank/far-rank pairs divided by all file pairs |
| `averageMovesHomeToFarRank` | Mean shortest-path length for reachable home-rank/far-rank pairs only |

Unreachable squares do not receive a finite distance.
For a Bishop on 8x8, `reachableSquareFraction` is `31/63`, and `homeToFarRankReachableFraction` is `1/2`.
If no pair is reachable, its average move count is `null`.
Custom generators, special attacks, and movement paths produce `unsupportedReason` and `null` travel measurements.

Directional mobility counts capture-capable directions with a density weight.
It does not model custom generators, screens, or path constraints.
It also excludes move-only capabilities.
The report lists these limitations separately from the travel model.
The existing safe-check statistic does not establish a legal or tactically safe check.

## JSON example

This example shows the core fields. The tables describe the additional geometry diagnostics.

```json
{
  "board": { "files": 8, "ranks": 8 },
  "eightByEightThresholdsApplied": true,
  "densityPercents": [50, 45, 40, 35, 30, 25, 20, 15, 10],
  "generatedAtUtc": "2026-07-05T21:08:00Z",
  "pieces": [
    {
      "name": "ShortRook",
      "notation": "S",
      "tier": "Minor",
      "tierSource": "ApmwChess",
      "midgameValue": 400,
      "endgameValue": 425,
      "materialValueSource": "ApmwChess",
      "averageDirectionsAttacked": 4.25,
      "averageSafeChecks": 2.25,
      "mobilityByDensityPercent": {
        "10": 5.6,
        "15": 5.2,
        "20": 4.9,
        "25": 4.6,
        "30": 4.4,
        "35": 4.1,
        "40": 3.9,
        "45": 3.7,
        "50": 3.5
      },
      "percentiles": {
        "vsTierPeers": {
          "averageMobilityAtDensity10": 95.8,
          "averageDirectionsAttacked": 91.7,
          "averageSafeChecks": 87.5
        },
        "vsAllPieces": {
          "averageMobilityAtDensity10": 72.0,
          "averageDirectionsAttacked": 78.0,
          "averageSafeChecks": 65.0
        }
      },
      "warnings": [
        {
          "type": "statistical_outlier",
          "severity": "warning",
          "message": "..."
        },
        {
          "type": "absolute_threshold",
          "severity": "warning",
          "message": "..."
        },
        {
          "type": "value_tier_mismatch",
          "severity": "info",
          "message": "..."
        }
      ],
      "constructionError": null
    }
  ],
  "unresolvedPieces": [
    {
      "name": "MissingPiece",
      "reason": "Piece type was not found in ChessV.Games."
    }
  ]
}
```

Notes:

- `mobilityByDensityPercent` keys are stringified density percentages.
- `percentiles.vsTierPeers` is empty/null-valued when a piece has no known tier cohort.
- `warnings` may be empty.
- Failed `--all` constructions keep the piece entry but set most analysis fields to `null` and populate `constructionError`.
