# Allocate CPU, neutral, and human formation bands

Type: grilling  
Status: resolved
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

## Answer

### Rank ownership

Each formal geometry has one required neutral rank. This rank must be empty at
setup.

| Geometry | CPU ranks | Neutral ranks | Human ranks |
| --- | ---: | ---: | ---: |
| `6x8` | 2 | 1 | 5 |
| `8x8` | 2 | 1 | 5 |
| `10x8` | 2 | 1 | 5 |
| `10x10` | 3 | 1 | 6 |
| `12x10` | 3 | 1 | 6 |

The five-rank human envelope is not a formal invariant. The two 10-rank
geometries add one mixed human rank. They do not add a pawn-only rank.

A human formation has these bands:

- One back rank.
- `human_deployment_depth - 4` mixed ranks.
- Three pawn-only ranks.

Thus, an 8-rank board has one mixed rank. A 10-rank board has two mixed ranks.

### Orientation

Formation coordinates are relative to the home edge of their owner. The file
does not change for the opposite color.

For a board with `R` ranks and a side-relative rank `r`:

```text
White board rank = r
Black board rank = R - 1 - r
```

The internal ranks are zero-based. This transform preserves king-side and
queen-side file meanings.

The exact board bands are:

| Board height | Human is White | Human is Black |
| --- | --- | --- |
| 8 | Human `0..4`, neutral `5`, CPU `6..7` | CPU `0..1`, neutral `2`, human `3..7` |
| 10 | Human `0..5`, neutral `6`, CPU `7..9` | CPU `0..2`, neutral `3`, human `4..9` |

### Capacity

Let `W` be the file count. Let `H` be `human_deployment_depth`.

```text
combined_non_primary_capacity = W * H - 1
non_pawn_capacity             = W * (H - 3) - 1
gross_pawn_capacity           = W * (H - 1)
active_pawn_capacity          = gross_pawn_capacity
                                - max(0, active_non_primary_non_pawns - (W - 1))
forwardness_capacity          = W * (H - 2)
```

The primary King uses the one slot excluded from
`combined_non_primary_capacity`.

| Geometry | Non-primary | Total with primary King | Non-pawn | Gross pawn | Active pawn at full non-pawn use | Pawn Forwardness |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| `6x8` | 29 | 30 | 11 | 24 | 18 | 18 |
| `8x8` | 39 | 40 | 15 | 32 | 24 | 24 |
| `10x8` | 49 | 50 | 19 | 40 | 30 | 30 |
| `10x10` | 59 | 60 | 29 | 50 | 30 | 40 |
| `12x10` | 71 | 72 | 35 | 60 | 36 | 48 |

The active-piece selection keeps the current role priority and material order.
An owned slot that does not fit enters the reserve.

`reserve_count` equals the owned non-primary count minus the active
non-primary count. Role limits and pawn displacement determine the active
count.

Missing material remains the sum of granted material for reserve slots. Each
reserve slot contributes exactly once.

Pawn Forwardness can move a pawn only inside the human band. It cannot move a
pawn into the neutral rank. The projection records excess Pawn Forwardness as
unspent.

### Contract and profile ownership

Each formal stage must store these fields:

```text
cpu_deployment_depth
neutral_depth
human_deployment_depth
```

Their sum must equal the board height. The contract must also store and
validate these derived fields:

```text
combined_non_primary_capacity
non_pawn_capacity
gross_pawn_capacity
forwardness_capacity
```

The new contract replaces the ambiguous `deployment_depth` field with
`human_deployment_depth`.

The change supersedes the current `expanded-formation-v2` capacity semantics
and `placement-role-material-v2` projection semantics. Both algorithm
identifiers need new versions.

The CPU layout profile needs a new version when the exact 10x10 and 12x10
arrays land. This ticket does not change the Location profile version.
[Define the versioned cross-repository contract](17-define-versioned-cross-repo-contract.md)
owns the exact identifiers, schema version, hash change, and release boundary.

### Setup validation

Setup creation must stop if:

- The three depth values do not equal the board height.
- A coordinate is outside its owner band.
- A coordinate is outside the file range.
- Two starting pieces use one board square.
- A starting piece enters the neutral rank.
- A FEN row has the wrong width.
- The FEN has the wrong rank count.

Validation must cover both human colors. It must apply the file-preserving rank
reflection before it checks board-square collisions.

### Unsupported geometry

These formulas do not authorize a 12x12 stage. Current 12x12 source and
contract records are migration evidence only. They must not produce a
selectable formation.

### Claim ledger

| Exact claim | Class | Authority or basis | Excludes |
| --- | --- | --- | --- |
| Ten-rank boards use 3 CPU, 1 neutral, and 6 human ranks. | Author decision | Resolution interview | A 5-rank or 7-rank human band |
| The sixth human rank is mixed. | Author decision | Resolution interview | A fourth pawn-only rank |
| Opposite colors keep the file and reflect the rank. | Author decision | Resolution interview and current ChessV behavior | A 180-degree file reflection |
| The neutral rank is empty at setup. | Author decision | Resolution interview | Pawn Forwardness entering the neutral rank |
| Reserve ordering and missing-material accounting remain unchanged. | Evidence | Current projection source and contract | Repricing or dropping overflow slots |
| Exact release identifiers belong to the versioned-contract ticket. | Scope boundary | Existing map dependency | Naming cross-repository versions in this ticket |
| 12x12 is unsupported. | Author decision | Map destination and prior decision | Deriving a 12x12 formation from these formulas |
