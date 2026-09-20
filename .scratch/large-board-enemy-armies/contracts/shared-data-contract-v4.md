# Shared army and world data contract, version 4

Status: Accepted engineering specification, confirmed at Q35
Date: 2026-09-15
Owning record: `.scratch\large-board-enemy-armies\issues\17-define-versioned-cross-repo-contract.md`
Scope: Data schemas, compatibility, offline distribution, and wire fixtures for 0.4.0

This document specifies settled decisions. It is not an execution handoff.
It does not authorize production changes, publication, an adapter, or conversion of existing worlds.
The parent owns the royal lifecycle fixtures and integration of this draft.
Tracker UI, royal implementation, evaluation tuning, and calibration repricing are outside this document.

## 1. Authority and terminology

**D** means an author decision. **E** means current source evidence.
**S** means an engineering specification choice confirmed in the Q35 design review.
**Q** means a consequential open question.
Unless a passage states D, E, or Q, its normative serialization and integration requirements are S.
An S identifier does not claim a human decision or an existing implementation.

Repository-relative source references without a prefix refer to `C:\GitHub\chessv`.
**ChecksMate** references refer to the read-only evidence in `C:\GitHub\rft50-checksmate`.
The vocabulary follows `CONTEXT.md:5-53`.

| Exact claim | Class | Authority | Excludes |
| --- | --- | --- | --- |
| Supported geometries are exactly 6x8, 8x8, 10x8, 10x10, and 12x10 | D | Owning record, lines 44-66, and the task authorization | A selectable 12x12 stage or a legacy game path |
| ChessV publishes exact armies, identities, starting roles, and CPU promotion permissions | D | `docs\adr\0001-shared-exact-deployment-data.md:5-21` and `docs\adr\0017-separate-cpu-promotion-permissions.md:10-44` | A versions-and-counts-only army manifest |
| ChecksMate owns AP IDs, names, goal mappings, and authored calibration | D | ADR 0001, lines 13-23, and the task authorization | AP IDs inside ordinary-game army data |
| Absent or unknown CPU family input resolves to Standard | D | Task authorization and ADR 0017, lines 19-20 | A different promotion list for implicit Standard |
| The 0.4.0 client accepts only the new compatible contract | D | `docs\adr\0005-break-contract-compatibility-at-zero-four.md:5-27` | Acceptance of an old 8x8 world |
| Legacy itemization remains available | D | ADR 0005, lines 9-11 | Confusion between an itemization mode and a legacy world contract |
| Generator rules and the client consume one frozen world cost snapshot | D | `docs\adr\0016-publish-world-bound-location-cost-snapshots.md:10-35` | Client recalibration or a latest-cost fetch |
| Compatible numeric calibration revisions do not require rebuilt client or projector binaries | D | ADR 0016, lines 20-24 | A calibration revision inside the compiled semantic pin |
| Player projection values remain integer human-item values | E, preserved by D | `APMW.Client\ApmwContractV2.cs:54-70` and the task authorization | Replacement by CPU catalog values or intrinsic goal budgets |
| Exact schemas and the publication order in this document are engineering selections | S | The author delegated this specification work | Fabricated release history or new gameplay policy |

**Army** means the ChessV-owned deployment artifact.
**Location profile** means the ChecksMate-owned mapping from semantic goals to AP identities.
**Shared contract** means the frozen combination of army, Location, and human-projection semantics.
**Cost snapshot** means the world-specific records defined by `world-cost-snapshot-v1.md`.
**Semantic hash** means the canonical content hash defined in section 8.
**Raw hash** means SHA-256 of the exact file, executable, or archive bytes.

## 2. Selected schemas and compatibility boundary

The first implementation uses this exact schema set.
The file names and version identifiers are S, not additional gameplay decisions.

| Artifact or interface | Owner | Selected identifier and version | Selected file or slot field |
| --- | --- | --- | --- |
| Army | ChessV | `chessv_armies`, version 1.0 | `chessv-armies-v1.json` |
| CPU layout | ChessV, inside Army | `apmw-cpu-layout-v2` | `cpu_layout_version` |
| Army publication descriptor | ChessV | `chessv_armies_release`, integer version 1 | `chessv-armies-release-v1.json` |
| Location profile | ChecksMate | `apmw_location_profile`, version 2.0, profile ID `apmw-location-profile-v2` | `apmw-location-profile-v2.json` |
| Shared contract | ChecksMate assembles the agreed inputs | `apmw_contract`, version 4.0 | `apmw_contract_v4.json`, slot `apmw_contract` |
| Calibration source manifest | ChecksMate | `apmw_calibration_manifest`, version 1.0 | `apmw-calibration-manifest-v1.json` |
| Cost snapshot | ChecksMate | `apmw_location_costs`, version 1.0 | Slot `apmw_location_costs` |
| World binding | ChecksMate | `apmw_world_binding`, version 1.0 | Slot `apmw_world_binding` |
| Projector transport | ChecksMate producer and ChessV consumer | Protocol integer 1 | Existing request/response envelope |
| Projector runtime | ChecksMate | `0.2.0` for the first new frozen package | Release tag `apmw-projector-v0.2.0` |
| Projector build/release/lock metadata | Their existing owners | Existing schema names, integer version 2 | Existing metadata file names |

Version 4.0 uses `minor_compatibility: "exact"`.
Initial consumers accept only the exact data schemas in this table and their supported semantic hash.
They do not infer compatibility from a matching major version.
Unknown fields, unsupported minor versions, and unknown algorithm identifiers fail validation.

The runtime version identifies the new executable package.
It does not replace the contract version or transport protocol.
Protocol 1 remains sufficient because the envelope still carries a semantic hash and an opaque projection input.
The new projection input and output meanings belong to contract 4.0.
A necessary envelope change requires a separately reviewed protocol change, not silent reuse of protocol 1.

### Current source gaps, not target behavior

| Evidence | Exact source |
| --- | --- |
| The producer freezes v3 with hash `91cf4323ae53663d1e3a7ea8facb449c74fdcd5093fa1620337f51cc5cdbe00c` and minimum client 0.4.0 | ChecksMate `worlds\checksmate\apmw_projection\data\apmw_contract_v3.json:1-9` |
| The resource loader selects that frozen file | ChecksMate `worlds\checksmate\apmw_projection\resource.py:17-45` |
| The live client calls the v2 parser | `APMW.Client\ApmwGeometrySelection.cs:105-137` |
| That parser supports 2.0 and requires `minimum_client_version` | `APMW.Client\ApmwContractV2.cs:22-23,154-192` |
| The connection permits a missing contract before a later mode-specific gate | `APMW.Client\Client.cs:232-267` |
| Restoration pins runtime 0.1.0, protocol 1, and the old v2 semantic hash | `APMW.Client\ApmwProjectorLock.cs:84-87,111-137` and `tools\Restore-ApmwProjector.ps1:26,124-135` |
| Both producer manifests carry `minimum_client_version` | ChecksMate `worlds\checksmate\tools\build_apmw_projector.py:76-107` and `worlds\checksmate\tools\create_apmw_projector_release_manifest.py:69-117` |
| The restore parser excludes that field from the embedded build manifest | `tools\Restore-ApmwProjector.ps1:285-300` |

The old restore hash is `f1456e916285bf79dd4be6f4c8c6e5798ed7bb1eebd2f6e1f81075f39e8ffc15`.
Neither existing hash is the future v4 hash.
The build-manifest mismatch is not a world-contract parser mismatch.
The four formats require separate, strict parsers.

## 3. Common wire types

Every listed field is required.
Every object has exactly its listed fields, unless an explicit tagged variant applies.
Duplicate properties fail before conversion to a dictionary.
JSON comments, trailing commas, floating-point numbers, and boolean substitutes for integers fail.

| Type | Definition |
| --- | --- |
| `Version` | Object with exactly nonnegative integer `major` and `minor` |
| `Hash` | String of exactly 64 lowercase hexadecimal characters |
| `Id` | Nonempty printable ASCII semantic identifier, compared ordinally and case-sensitively |
| `Text` | Nonempty printable ASCII string |
| `Int` | Signed 32-bit JSON integer, with the stated nonnegative or positive constraint |
| `LocationId` | Positive signed 64-bit JSON integer, compatible with AP numeric IDs |
| `Size` | Positive signed 64-bit JSON integer measured in bytes |
| `Semver` | Canonical three-part nonnegative decimal version string |
| `Square` | Object with exactly nonnegative integer `file` and `rank` |
| `Binding` | Object with exactly `schema: Id`, `version: Version`, and `manifest_sha256: Hash` |
| `FileRecord` | Object with exactly `relative_path: Text`, `sha256: Hash`, and `size: Size` |
| `DataFile` | A `FileRecord` plus `schema: Id`, `version: Version`, and `manifest_sha256: Hash` |

Empty arrays represent an empty set where the schema permits one.
No runtime data artifact in this document requires a JSON null.
The parent correction record separately permits `supersedes: null`.
Section 7 preserves that correction format without extending shared-contract canonicalization.

All array orders are semantic unless a section fixes a producer order.
Producers emit stage order, then family order, then rank/file order for deployment records.
String-keyed sets use ascending ordinal order unless an approved list defines its order.
Consumers reject duplicate entries even when the JSON object keys are unique.

Archive paths use the existing portable archive convention.
They are relative, slash-separated paths without empty, current, parent, rooted, drive, or stream segments.
This is a wire-path rule, not a Windows filesystem command.
Consumers retain the existing traversal, link, extraction, provenance, size, and digest checks.

## 4. ChessV Army artifact

### Required root and nested records

| Root field | Type and invariant |
| --- | --- |
| `schema`, `version`, `manifest_sha256` | `chessv_armies`, 1.0, semantic `Hash` |
| `cpu_layout_version` | Literal `apmw-cpu-layout-v2` |
| `coordinate_system` | Literal `owner-relative-file-preserving-v1` |
| `family_order` | Exactly the four family IDs in the following table |
| `fallback_family_id` | Literal `standard` |
| `stage_order` | Exactly `["6x8","8x8","10x8","10x10","12x10"]` |
| `pieces` | Nonempty ordered array of `Piece` records, sorted by `type_id` |
| `roles` | Ordered array of `Role` records, sorted by `role_id` |
| `royal_policy` | The exact policy object defined in section 4.3 |
| `formations` | Exactly 20 `Formation` records, in stage/family order |

| Family ID | Meaning |
| --- | --- |
| `standard` | Standard/FIDE |
| `colourbound-clobberers` | Colourbound Clobberers |
| `remarkable-rookies` | Remarkable Rookies |
| `nutty-knights` | Nutty Knights |

The external Enemy Army selector can resolve absent or unknown input to Standard.
That fallback does not apply to malformed artifacts.
An unknown family ID inside `formations`, a duplicate pair, or a missing Standard formation is an error.

`Piece` has exactly these fields:

| Field | Type and meaning |
| --- | --- |
| `type_id` | Unique `Id` for the concrete piece type |
| `display_name` | `Text`, independent of identity and capture-role names |
| `movement_id` | `Id` for a known, versioned ChessV movement implementation |
| `notation` | Object with exactly `white_scalar: Int` and `black_scalar: Int` |
| `image_id` | `Id` for the registered image resource |
| `catalog_material` | Object with exactly nonnegative integer `midgame` and `endgame` |

Notation scalars are valid Unicode scalar values, not surrogate values.
Integer scalars preserve existing non-ASCII notation without changing ASCII canonicalization.
They identify one registered notation character for each color.
The client does not treat a scalar, image name, display name, or movement ID as a `type_id`.

The known registry must resolve every starting type and CPU promotion target.
It must reject an unknown movement ID instead of substituting a related piece.
New registration must not collide with notation or an image assigned to another game piece.
The existing Amazon and Mounted King registrations remain distinct.

E: `ApmwPieceCatalog.cs:21-47` distinguishes King, Mounted King, War Elephant, Lion, NarrowKnight, and Amazon.
The full path is `ChessV.Games\MiscellaneousGames\ApmwPieceCatalog.cs`.
`NarrowKnight` has the display name Lancer.
The basic Elephant is not War Elephant.
D: The accepted Elephant value is 250/250, and the existing Lion value is 500/500.
These catalog facts do not replace human projection values or Location costs.

`Role` has exactly `role_id: Id` and `starting_class`.
`starting_class` is one of `pawn`, `non-king-non-pawn`, or `king`.
The role catalog is the complete machine-role catalog in issue 15, lines 287-354.
It includes primary and additional King roles without creating individual King Locations.
An Army role has no AP numeric ID, AP alias, or material requirement.

`Formation` has exactly these fields:

| Field | Type and meaning |
| --- | --- |
| `stage_id`, `family_id` | IDs from the root orders |
| `files`, `ranks` | Positive integers equal to the named geometry |
| `cpu_deployment_depth` | Positive integer, 2 or 3 as specified in section 6 |
| `units` | Ordered array of `StartingUnit` |
| `empty_squares` | Ordered `Square` array, the complement of `units` across the CPU layer's entire board |
| `starting_counts` | Object with exactly nonnegative integers `pawns`, `non_king_non_pawns`, `kings`, and `total` |
| `royal_units` | Object with exactly `primary_unit_id: Id` and `king_unit_ids: Id[]` |
| `castling` | Ordered array of `CastleRoute`, queen-side before king-side |
| `promotion` | Object with exactly `pawn_behavior_id: Id`, `promotion_rank: Int`, and `target_type_ids: Id[]` |

`StartingUnit` has exactly `unit_id: Id`, `type_id: Id`, `role_id: Id`, and `square: Square`.
Each `unit_id` is unique within its formation.
S: Its initial value equals its unique starting `role_id`.
Movement, promotion, load, and capture retain that starting-unit identity.
Neither a current square nor a current type can replace it.

Every square occurs exactly once in either `units` or `empty_squares`.
Empty CPU-layer squares outside the CPU band do not declare the human layer empty.
Every starting unit lies inside the CPU band.
Counts must equal the actual records, not a separately maintained summary.
All four families have the same starting-role set for a geometry.

### 4.1. Exact array authority

The artifact contains expanded coordinate records, not formulas for consumers to reconstruct.
The following sources define their complete contents.
Historical proposals within the issues do not override their accepted final sections.

| Geometry | Exact authority |
| --- | --- |
| 6x8 | Existing four-family home rows in `ChessV.Games\MiscellaneousGames\ApmwProfiles.cs:362-377`, plus one ordinary pawn per file at relative rank 1 |
| 8x8 | Existing four-family home rows in the same file, lines 378-392, plus one ordinary pawn per file at relative rank 1 |
| 10x8 | Existing four-family home rows in the same file, lines 394-408, plus one ordinary pawn per file at relative rank 1 |
| 10x10 | `.scratch\large-board-enemy-armies\issues\12-normalize-ten-by-ten-arrays.md:46-114` |
| 12x10 | `.scratch\large-board-enemy-armies\issues\13-normalize-twelve-file-arrays.md:115-205` |
| Starting-role bindings on all five geometries | `.scratch\large-board-enemy-armies\issues\15-define-location-role-identity.md:287-354` |

Small-board notation expands through the pinned piece registry.
The publication step can perform that expansion once.
Neither runtime consumer repeats home-row synthesis.
The old twelve-file Nightrider row is not an input to the new artifact.

| Geometry | Pawns | Non-King non-pawns | Kings | Total |
| --- | ---: | ---: | ---: | ---: |
| 6x8 | 6 | 5 | 1 | 12 |
| 8x8 | 8 | 7 | 1 | 16 |
| 10x8 | 10 | 9 | 1 | 20 |
| 10x10 | 12 | 15 | 1 | 28 |
| 12x10 | 14 | 18 | 2 | 34 |

The 12x10 Rookies formation has six literal Lions with distinct starting roles.
The forward Queen role does not duplicate the old home Queen role.
Amazon, primary Mounted King, and additional ordinary King have separate records.
No Champion or Nightrider appears in the new CPU starting arrays.

### 4.2. Coordinates and composition

Files and ranks are zero-based.
An owner's relative rank zero is its home edge.
White uses board rank `r`.
Black uses board rank `R - 1 - r`, where `R` is board height.
The file never changes.

For example, White CPU B2 is `{file:1,rank:1}`.
On a ten-rank board, the same Black CPU record produces B9.
A White CPU primary Mounted King on G1 produces a Black CPU primary on G10.
A 180-degree rotation that also reverses files is invalid.

Both consumers preserve owner-relative records.
The client applies reflection once during final board composition.
Composition validates CPU/human separation, the neutral rank, unique occupancy, FEN rank count, and FEN row width.
Pawn Forwardness cannot place a human pawn in the neutral rank.

### 4.3. Royal, castling, and promotion metadata

D: The policy comes from issue 14, lines 30-131.
This section serializes that policy. It does not replace the parent fixture document.

`royal_policy` has exactly this structure and these values:

```json
{
  "id": "cpu-survival-v1",
  "count_basis": "surviving-cpu-king-units",
  "extinction_precedes_no_moves": true,
  "zero_kings": "cpu-loss-extinction",
  "one_king": {
    "must_avoid_check": true,
    "no_moves_attacked": "cpu-loss-checkmate",
    "no_moves_unattacked": "cpu-win-stalemate"
  },
  "multiple_kings": {
    "must_avoid_check": false,
    "no_moves": "cpu-win"
  }
}
```

`king_unit_ids` contains the ordinary primary King on the first four geometries.
On 12x10, it contains the primary Mounted King and the additional ordinary King.
Amazon and Lion never enter this set.
Single-King legality applies to either surviving type.
The initial King count remains separate from the live count used by this policy.

`CastleRoute` has exactly these fields:

| Field | Type and invariant |
| --- | --- |
| `side` | `queen-side` or `king-side` |
| `king_unit_id`, `castler_unit_id` | Original starting-unit IDs, not type substitutions |
| `king_from`, `king_to`, `castler_from`, `castler_to` | Owner-relative `Square` values |
| `empty_squares` | Ordered array of required empty squares |
| `king_safety_squares` | Ordered array from King source through King destination |
| `white_right`, `black_right` | `Q`/`q` for queen-side, `K`/`k` for king-side |
| `requires_unmoved_actors` | Literal `true` |
| `attack_restrictions_in_all_phases` | Literal `true` |
| `inheritance` | Literal `false` |

The first release uses the exact route table in issue 14, lines 106-113.
6x8 has an empty route array.
Other geometries have exactly two routes, using only the original primary and corner castler roles.
Colourbound destination differences must appear as exact coordinates, not a consumer-side parity guess.

For each route, the producer expands both horizontal actor paths.
Required empty squares are their union, including destinations but excluding the two actor source squares.
King safety covers the King source, each crossed square, and the destination.
These requirements apply even when two Kings survive.
An actor's return to its starting square does not restore lost rights.
The additional King, outer Lions, and forward pieces never inherit CPU rights.
Human Major/Jack castling remains a separate preserved rule.

`promotion.pawn_behavior_id` is `apmw-cpu-pawn-v1`.
It denotes the preserved ordinary CPU pawn behavior, not a new movement rule.
`promotion_rank` is `ranks - 1` in owner-relative coordinates.
`target_type_ids` is the exact ordered list from ADR 0017, lines 12-17.
Ten-file and twelve-file lists append only that ADR's additional targets.

D: Basic Elephant, Amazon, ordinary King, Mounted King, and the old Nightrider extension are excluded from CPU pawn promotion.
Only Rookies acquire Lion promotion through their family list.
Army type registration grants no human promotion entitlement, pocket entitlement, or random-pool membership.
Explicit, absent, and unknown family selections produce the same Standard promotion list after resolution.

## 5. ChecksMate Location profile

The profile supplies AP identity and goal mapping, not arrays or calibrated costs.
It binds to one exact Army semantic hash.

| Root field | Type and invariant |
| --- | --- |
| `schema`, `version`, `manifest_sha256` | `apmw_location_profile`, 2.0, semantic `Hash` |
| `profile_id` | Literal `apmw-location-profile-v2` |
| `army` | `Binding` for the Army |
| `cpu_layout_version` | Literal `apmw-cpu-layout-v2`, equal to the Army |
| `goal_semantics_id` | Literal `apmw-goals-v2` |
| `target_classes` | Exact object with `pawn`, `minor`, `major`, `queen`, and `king` arrays of Army `type_id` values |
| `locations` | Array of `Location` records, in ascending numeric ID order |
| `events` | Array of `EventLocation` records, sorted by `location_key` |

`Location` has exactly these fields:

| Field | Type and meaning |
| --- | --- |
| `location_key` | Unique semantic `Id`, stable across geometries |
| `location_id` | Unique `LocationId` assigned by ChecksMate |
| `display_name` | Canonical AP `Text` |
| `aliases` | Ordered `Text[]`, empty when none |
| `condition` | One exact tagged variant from the following table |
| `tactics_group` | `none`, `turns`, or `fork` |
| `eligible_formations` | Ordered records with exactly `stage_id: Id` and `family_id: Id` |
| `special_condition_ids` | Ordered array of known predicate IDs, empty when no extra generator condition applies |

`EventLocation` has the same fields except `location_id`.
An event does not receive a synthetic numeric ID or a cost-snapshot Location.
AP display names and aliases must resolve unambiguously within the profile.
Numeric continuity across releases does not imply semantic compatibility.
ChecksMate can retain convenient IDs without retaining legacy names or progress.

### Condition variants

Each row specifies the exact fields for that variant.
An unknown kind, field, enum value, evaluator, role, or parameter is an error.
The condition record is declarative. It is not executable code from slot data.

| `kind` | Other required fields | Meaning or authority |
| --- | --- | --- |
| `capture-role` | `role_id: Id` | Capture the named starting role, independent of its current type or square |
| `capture-count` | `series`, `count: positive Int` | Series is `pawns`, `pieces`, `of-each`, or `any`. Arithmetic follows issue 15, lines 222-285 |
| `regicide` | None | The accepted multi-King starting-setup capture condition |
| `capture-everything` | None | The accepted endpoint-only, single-match clear condition |
| `stage-victory` | `stage_id: Id`, `evaluator_id: Id` | The stage's existing victory event, reconciled with the parent's accepted royal outcomes |
| `king-action` | `action`, `evaluator_id: Id` | Action is `home-pawn-square-early`, `center`, `a-file`, `capture`, or `enemy-back-rank` |
| `survive-turns` | `count: positive Int`, `evaluator_id: Id` | Existing turn-count semantics, with counts 3, 5, 10, or 20 |
| `threaten` | `target_class`, `evaluator_id: Id` | One target class from `target_classes`, with existing threat semantics |
| `fork` | `quality`, `targets: Int`, `royal: bool`, `evaluator_id: Id` | Quality is `sacrificial` or `true`. Targets is 2 or 3. Royal variants use 2 |
| `castle` | `side`, `evaluator_id: Id` | Side is `queen-side` or `king-side`, using the preserved human castling event |

S: Preserved evaluator IDs use `apmw-event-<kind>-v1`.
The Army's CPU royal policy and the parent's fixture outcomes constrain the stage-victory evaluator.
The current event catalog is ChecksMate `worlds\checksmate\locations.py:187-193,294-326`.
Its numeric estimates are not part of this semantic profile.
The implementation must carry over the existing event predicates, not derive them from English display names.
No new early-move limit, fork rule, or victory interpretation is selected here.

S: `special_condition_ids` uses this finite registry:

| Predicate ID | Records that require it | Exact retained generator condition on geometry `g` |
| --- | --- | --- |
| `human-tactic-source-v1` | All six fork records, plus Threaten Minor, Major, Queen, and King | Fundamental requires `logic_projection.metrics(state, player, g).chessmen >= 1`. Legacy requires at least one received Progressive Minor Piece, Progressive Major Piece, or Progressive Jack |
| `human-castler-king-side-v1` | The king-side castle record | Stage logic castlers are at least 1 in Fundamental, or the shared contract's castler maximum in Legacy |
| `human-castler-queen-side-v1` | The queen-side castle record | The same mode-dependent count predicate, together with the queen-side geometry capability |

The king-side record also requires its geometry's king-side capability.
These are generator predicates, not replacements for the actual castling
actors or executed tactic conditions.
Threaten Pawn and the other initial records have no additional predicate.

The resource bypass cannot waive these conditions.
Each geometry path evaluates them alongside its own material and chessmen
requirements.
Legacy's item predicate remains its existing grant-based condition, not an
invented active-piece or promotion predicate.

E: ChecksMate `worlds\checksmate\rules.py:43-44,339-387` and
`worlds\checksmate\item_utils.py:150-153` define these conditions.
A new predicate requires an explicit definition and review before publication.
It cannot arrive as an unexplained string.

### Required catalog and eligibility invariants

D: Issue 15, lines 287-354, supplies all 34 distinct individual non-King role keys across the full progression.
Each profile record uses `location_key: "capture-" + role_id`.
Its display name is `Capture ` plus that catalog's role label.
Primary and additional King roles do not receive individual capture records.
They feed one Regicide condition.

Per geometry, the individual target counts are 11, 15, 19, 27, and 32.
Each starting non-King unit maps to exactly one individual Location.
An absent role has no eligible formation record.
One role retains one Location across stages and families.
Center Rook, Queen, and Queen's Rook remain distinct.

D: The initial setup controls King contributions to capture counters.
Promotion retains the starting class.
Captured King lives cannot substitute for an uncaptured non-King in Capture Everything.
Issue 15, lines 222-285, defines the exact counter and ceiling equations.
The endpoint ceiling vectors for Pawns/Pieces/Of Each/Any are:

```text
6x8:   6 /  5 /  5 / 10
8x8:   8 /  7 /  7 / 14
10x8: 10 /  9 /  9 / 18
10x10:12 / 15 / 12 / 26
12x10:14 / 19 / 14 / 32
```

Each series contains every integer from 2 through its endpoint ceiling.
A published count goal can qualify on an earlier capable geometry.
Capture Everything qualifies only on the world's configured endpoint.
No Location requires a 12x12 stage.

`eligible_formations` records static capability from the accepted Army and goal semantics.
It includes CPU promotion permissions where a promotion can supply the target.
It is not a table of numeric resource thresholds.
The generator does not reprice a goal from the client's free Enemy Army selection.
The client can use the family-specific eligibility records for the actual match.

Target classification and starting-role identity remain separate.
Both ordinary and Mounted Kings belong to the King tactical target class.
The accepted true-fork King treatment remains active in the multi-King setup.
Queen-family targeting retains its existing meaning.
Registering Amazon does not make it royal.

D: Q34 in issue 14 selects Minor for the basic Elephant tactical class.
It belongs to `target_classes.minor`, not the Major or Queen class.
This classification grants no human random-pool or promotion permission.
The client events and profile must use the same class.
The independent threat fixture is in `royal-lifecycle-fixtures.md`.

## 6. Shared contract: geometry and human projection

### Required root fields

The root retains the existing contract subtrees listed here.
The source of preserved subtrees is the frozen producer v3 file, not the live v2 parser's old geometry constants.

| Field | Type or normative definition |
| --- | --- |
| `schema`, `version`, `manifest_sha256` | `apmw_contract`, 4.0, semantic `Hash` |
| `minimum_client_version` | `Semver`, initially `0.4.0` |
| `minor_compatibility` | Literal `exact` |
| `army`, `location_profile` | Exact `Binding` objects |
| `cost_snapshot_schema`, `world_binding_schema` | Objects with exactly `schema: Id` and `version: Version`, fixed to section 2 |
| `geometry` | Exact object defined in section 6.1 |
| `algorithms` | Exact six-key map defined in section 6.2 |
| `metrics` | Exact metric descriptor defined in section 6.3 |
| `cpu_profiles` | Exactly `layout_version`, `location_profile_version`, and `army_ids`, matching sections 4 and 5 |
| `expected_material` | Exact integer map in section 6.2 |
| `effective_item_maxima` | Exact object with `common`, `legacy`, and `fundamental` integer maps in section 6.2 |
| `itemization_modes`, `ordering_modes`, `mode_combinations` | Preserved subtrees from the producer v3 file, lines 10-52 |
| `source_roles`, `upgrade_dag`, `castler` | Preserved subtrees from that file, lines 215-355 |
| `semantic_series_ids`, `presentation_series_ids`, `overflow_policy` | Preserved subtrees from that file, lines 356-399 |

The normative file is ChecksMate `worlds\checksmate\apmw_projection\data\apmw_contract_v3.json`.
Its frozen semantic hash is the v3 hash recorded in section 2.
Importing a preserved subtree means preserving every field, type, value, and array order.
It does not mean preserving the v3 root, geometry, CPU profiles, maxima, or old compatibility rule.
This explicit import defines the retained fields without a second, divergent copy of their large value sets.

The supported mode combinations remain Legacy/Stable, Legacy/Chaos, and Fundamental/Stable.
Fundamental/Chaos is not newly authorized.
The contract contains neither AP calibration curves nor a world's cost hash.

### 6.1. Geometry fields and capacities

`geometry` has exactly `base`, `file_ladder`, `rank_ladder`, `stage_order`, `valid_pairs`, `unlocks`, and `pawn_capacity_formula`.
`base` has exactly `files: 6` and `ranks: 8`.
The ladders are `[6,8,10,12]` and `[8,10]`.
`stage_order` is the exact five-stage order from section 4.
Ladder membership alone does not make another pair valid.

Each `valid_pairs` record has exactly these integer fields plus `stage_id: Id`:

```text
files, ranks
cpu_deployment_depth, neutral_depth, human_deployment_depth
combined_non_primary_capacity, non_pawn_capacity
gross_pawn_capacity, forwardness_capacity
cpu_pawn_count, cpu_non_king_non_pawn_count, cpu_king_count
```

`deployment_depth` is not a v4 field.
The old hard-coded `all_tactics_locations`, `turns_locations`, and `no_tactics_locations` fields are also absent.
The Location profile and world selection determine those counts.
Consumers cannot retain the old width-based Location-count formulas.

D: Capacity and rank ownership follow issue 19, lines 50-164.

| Stage | CPU/neutral/human depths | Non-primary | Non-pawn | Gross pawn | Forwardness |
| --- | --- | ---: | ---: | ---: | ---: |
| 6x8 | 2/1/5 | 29 | 11 | 24 | 18 |
| 8x8 | 2/1/5 | 39 | 15 | 32 | 24 |
| 10x8 | 2/1/5 | 49 | 19 | 40 | 30 |
| 10x10 | 3/1/6 | 59 | 29 | 50 | 40 |
| 12x10 | 3/1/6 | 71 | 35 | 60 | 48 |

The depths sum to board height.
For width `W` and human depth `H`, the derived values are:

```text
combined_non_primary_capacity = W * H - 1
non_pawn_capacity = W * (H - 3) - 1
gross_pawn_capacity = W * (H - 1)
forwardness_capacity = W * (H - 2)
active_pawn_capacity =
    gross_pawn_capacity - max(0, active_non_primary_non_pawns - (W - 1))
```

The human band has one back rank, `H - 4` mixed ranks, and three pawn-only ranks.
The sixth human rank on a ten-rank board is mixed.
The primary royal occupies the excluded slot.
Both itemization modes use these capacities.

`unlocks` retains exactly `roles` and `selection_policy`.
The policy remains `largest-componentwise-unlocked-valid-pair`.
Each role retains exactly `role_id`, `base`, `increment`, and `maximum`.
The file role is `board-file-unlock`, 6/2/12.
The rank role is `board-rank-unlock`, 8/2/10.
The effective unlock maxima are three file items and one rank item.
Existing configured progression rules still limit the selected stages and endpoint.

`pawn_capacity_formula` retains its four exact keys:

| Key | v4 value |
| --- | --- |
| `gross_pawn_capacity_algorithm` | `width-times-human-depth-minus-one-v1` |
| `non_pawns_beyond_back_algorithm` | `max-zero-n-minus-width-minus-one-v1` |
| `active_pawn_capacity_algorithm` | `gross-minus-non-pawns-beyond-back-v1` |
| `back_rank_primary_royal_slots` | Integer 1 |

The new gross formula uses human depth, not board height minus an assumed CPU depth.
Every serialized capacity must equal its formula.
CPU counts must equal every bound Army formation for that stage.

### 6.2. Preserved projection semantics and maxima

The six required `algorithms` keys have these values:

```json
{
  "capacity": "expanded-formation-v3",
  "projection": "placement-role-material-v3",
  "overflow": "material-first-reserve-v2",
  "ordering": "independent-semantic-series-v1",
  "series_prf": "sha256-counter-v1",
  "resource_logic": "world-logic-envelope-v1"
}
```

The new capacity and projection IDs account for the new bands and explicit army composition.
The retained overflow and ordering algorithms do not change.
The resource-logic ID pins the existing conservative metric algorithm described in section 6.3.
Changing another algorithm requires another explicit semantic revision.

The exact human `expected_material` map remains:

```json
{
  "weak": 75, "pawn": 100, "minor": 300, "major": 485,
  "castler": 500, "jack": 700, "queen": 900, "amazon": 1300,
  "material_item": 400, "play_as_white": 50, "pocket": 110,
  "consul": 325, "king_promotion": 425
}
```

The exact maxima are:

| Map | Required entries |
| --- | --- |
| `common` | `Play as White: 1`, `Progressive AI Intelligence Malus: 5`, `Progressive Pocket: 12`, `Progressive Pocket Range: 6`, `Progressive King Promotion: 2`, `Progressive Consul: 2` |
| `legacy` | `Progressive Pawn: 60`, `Progressive Pawn Forwardness: 13`, `Progressive Minor Piece: 15`, `Progressive Major Piece: 11`, `Progressive Major To Queen: 9`, `Progressive Jack: 9` |
| `fundamental` | `Chessmen: 71`, `Material: 213`, `Castler: 2` |

S: Fundamental maxima follow the existing invariants with the new largest capacity.
`Chessmen` equals 71, and `Material` equals three times 71.
E: The client parser requires these relationships at `APMW.Client\ApmwContractV2.cs:599-602`.
Legacy item-count maxima remain unchanged.
The Forwardness item count of 13 is not a board-slot capacity of 13.

The existing role priority remains primary royal, additional royal, locked castler, Jack, Major, Minor, then Pawn.
Within each role, activation uses final expected material descending, granted material descending, then source ordinal ascending.
Reserve entry order reverses those priorities as specified by the preserved overflow subtree.
Each reserve slot contributes its normalized granted material to missing material exactly once.
`reserve_count` equals owned non-primary count minus active non-primary count.

Locked castlers still reclassify existing chessmen.
They do not create extra units or accept higher-than-Jack upgrades.
Larger geometries can reactivate reserves.
Semantic series remain isolated from presentation series.
Chaos changes presentation, not aggregate semantic metrics.
Excess Pawn Forwardness remains unspent.
No CPU catalog value changes these human accounting rules.

Q44 adds a bounded Legacy capability after the Q35 design:
[ADR 0022](../../../docs/adr/0022-retain-legacy-pawn-material-conversion.md)
selects pawn-material conversion rather than unconditional pawn-slot
preservation.
Ticket 24 still owns its exact reservation and conversion rule.
The accepted capability is not yet encoded by this section's existing
algorithm identities or a new projector input.
The final rule must supply the necessary semantic revision, input contract,
and compatible logic metrics before that path is ready for handoff.
This does not change the listed human values or select Fundamental
conversion behavior.
Q48 makes the prepared board the context for the Legacy reservation target.
The implementation cannot use the world endpoint as that target on every
earlier board.
Q49 sets that target to the prepared CPU formation's unit count minus its
primary royal: 11, 15, 19, 27, and 33 in stage order.
This is the explicitly selected human count relationship.
It does not borrow CPU material values or promotion entitlements.
The conversion equation and count-protection boundary remain open.

### 6.3. Exact roster metrics and generator logic metrics

The required `metrics` object has exactly these keys and values:

```json
{
  "id": "apmw-human-metrics-v1",
  "exact_material": "sum-active-final-expected-material",
  "exact_chessmen": "active-non-primary-slot-count",
  "logic": "monotone-world-logic-envelope",
  "stage_caps": "obtainable-count-logic-maxima",
  "material_unit": "one-material-unit",
  "chessmen_unit": "one-non-primary-unit-estimate",
  "numeric_type": "nonnegative-int32",
  "logic_envelope_cell_limit": 10000
}
```

E: Exact active material sums the final expected values of active slots.
Missing material instead sums granted values of reserve slots.
Their ledgers preserve total grant conservation.
Source: `APMW.Client\ProjectionV2SemanticEngine.cs:890-907,938-945`.

E: Generator `LogicMetrics` contains integer `material`, `chessmen`, and `castlers`.
These are conservative reachability metrics, not a renamed exact-roster total.
Small Fundamental count spaces use a componentwise suffix minimum over exact projections.
Legacy and larger Fundamental spaces retain the active-slot floor.
The current cell limit is 10000.
Source: ChecksMate `worlds\checksmate\logic_projection.py:30-61,295-357`.

The existing count normalization, obtainable-count bounds, King-promotion contribution, and castler calculation remain unchanged.
The rule-facing chessmen metric retains the occupied-pocket contribution.
Stage caps retain the existing maximum methods over finalized obtainable counts.
Sources: ChecksMate `worlds\checksmate\logic_projection.py:99-229,257-293`.
The new band capacities replace their old geometry inputs.
This specification does not replace the conservative envelope with exact active material.

In section 7's access expression, `player_material` and `player_chessmen` mean the rule-facing logic metrics.
The cost snapshot supplies thresholds, not a newly implemented client availability evaluator.
The projector can retain its existing exact-roster response shape.
A later complete availability evaluator must obey the pinned logic metrics, not compare incompatible quantities.
Changing metric units, envelope behavior, or human values changes C.
A numeric revision of intrinsic goal costs alone does not.

## 7. Calibration, costs, and world binding

### 7.1. Calibration source manifest

This descriptor gives a precise meaning to the snapshot's calibration hash.
It does not define new numeric estimates or replace the parent's correction schema.

| Field | Required type and meaning |
| --- | --- |
| `schema`, `version`, `manifest_sha256` | `apmw_calibration_manifest`, 1.0, semantic `Hash` |
| `model_version` | Literal `apmw-calibration-v1` |
| `revision` | Positive integer, initially 1 |
| `army_sha256`, `location_profile_sha256` | Exact semantic hashes of the derivation inputs |
| `sources` | Ordered records with exactly `purpose: Id` and `file: FileRecord` |

`purpose` is one of `reference-inputs`, `derivation-model`, `authored-corrections`, or `derived-requirements`.
The source set must cover all four purposes.
Records sort by purpose and relative path.
ChecksMate owns the source-file formats.
These files are generation inputs, not client wire payloads.
The source bytes cover exact reference inputs, formulas, corrections, and derived intrinsic records.
Their raw hashes prevent an unrecorded formula or correction change.

The parent correction records retain their required fields and rational representation.
Their source file can contain the parent's explicit `supersedes: null`.
The manifest hashes that file's bytes through `FileRecord`.
It does not pass null through the shared-contract canonicalizer.

The snapshot's `calibration.manifest_sha256` equals this manifest's semantic hash.
Its model version and revision must agree.
The generator validates all source bytes before it derives a snapshot.
The client does not require those source files or execute their formulas.

An independent numeric revision can change this manifest and the resulting snapshot.
It cannot change Army, Location, or projection semantics under an unchanged semantic pin.
The initial correction policy and dependency regeneration remain those in `world-cost-snapshot-v1.md:219-255`.

### 7.2. Snapshot schema and use

The parent owns `world-cost-snapshot-v1.md`.
Its sections at lines 58-159 and 194-271 normatively define the complete snapshot shape and arithmetic.
This document does not fork that schema.

Required root fields remain `schema`, `version`, `manifest_sha256`, `bindings`, `calibration`, `difficulty`, `absolute_adjustment`, `stage_caps`, and `locations`.
The binding fields are exactly `contract_sha256`, `army_sha256`, and `location_profile_sha256`.
Exact intrinsic material and difficulty use reduced numerator/denominator decimal strings.
Final material and chessmen thresholds are nonnegative integers.

D: Generation applies the applicable adjustment after exact difficulty multiplication.
It performs one upward rounding, then applies the material cap.
When intrinsic material exceeds 90, the adjustment applies.
Chessmen uses its separate cap.
Neither consumer applies another scaling, rounding, or cap to the effective values.

The generator freezes the snapshot after it obtains the finalized obtainable-item counts and semantic seeds.
Its rule closures and `fill_slot_data` use that same immutable object.
It does not rebuild a similar-looking snapshot during serialization.
The client retains the received intrinsic provenance and effective integers.
The client does not reprice from `ExpectedMaterial`, CPU catalog values, or local calibration code.

Each Location has one cost profile per statically eligible stage in the world progression.
The snapshot has no family-price axis.
An absent role or impossible capability has no profile, not a zero-cost profile.
The producer validates profile completeness against the bound Location profile.

A qualifying generator path retains this structure:

```text
board_available(g)
and static_goal_eligibility(g)
and special_conditions(g)
and (
    later_board_resource_bypass(g)
    or (
        player_material(g) >= snapshot.effective_material(g)
        and player_chessmen(g) >= snapshot.effective_chessmen(g)
    )
)
```

One complete geometry path must qualify.
Material, chessmen, and special conditions cannot come from different paths.
The retained bypass waives only resource comparisons.
It cannot use a stage beyond the configured endpoint.
Actual earned checks do not depend on meeting generator estimates.

### 7.3. World binding and slot fields

`apmw_world_binding` contains exactly these fields:

| Field | Required type and invariant |
| --- | --- |
| `schema`, `version`, `manifest_sha256` | `apmw_world_binding`, 1.0, semantic `Hash` |
| `contract_sha256` | The shared contract's semantic `Hash` |
| `cost_snapshot_sha256` | This world's snapshot semantic `Hash` |
| `progression_start`, `progression_end` | Supported stage IDs |
| `stages` | Nonempty, unique ordered stage-ID array |
| `itemization`, `ordering` | One supported mode combination |
| `tactics_mode` | `all`, `turns`, or `none` |
| `published_location_ids` | Unique ascending `LocationId[]` |
| `published_event_keys` | Unique ascending `Id[]` |

Stages must agree with the configured progression and follow the contract's stage order.
The first and last entries equal the declared start and endpoint.
This schema does not add a new progression mode or skip policy.
The five-stage contract supplies all possible stages, not a demand that every world uses all five.

Published sets follow the Location profile and existing tactics-mode selection.
Fork records require `all`.
Turn records permit `all` or `turns`.
Non-tactic records remain subject to their role, endpoint, and capability conditions.
No numeric ID appears twice or names an absent profile record.

Required slot fields are:

```text
required_chess_client_version
apmw_contract
apmw_world_binding
apmw_location_costs
```

The version field equals `apmw_contract.minimum_client_version`.
The three artifact fields contain JSON objects, not JSON-encoded strings.
The AP envelope can retain unrelated existing fields.
These three objects still use their exact field sets.

Existing projection seeds, human configuration, and obtainable-count fields retain their versioned projection meanings.
They must agree with the World binding and shared contract.
If compatibility mirrors remain, they are assertions rather than alternative authorities.
Examples are `apmw_contract_version`, `material_item_value`, `castling_location_count`, and `geometry_unlock_items`.
A conflicting mirror fails instead of overriding its authoritative artifact.
Absence of a required v4 artifact never selects an older path.

The snapshot IDs equal `published_location_ids`.
Its stage caps cover exactly the declared world stages.
Its profiles cover the eligible stages for each published Location.
The World binding's snapshot hash must equal the actual snapshot hash.
The client retains this binding with the authenticated slot context.
Another world's snapshot cannot replace it merely because both use contract 4.0.

Q42 adds a reconnect rule without changing these artifact fields.
The replay key uses AP generation name and authenticated team/slot.
Replay also requires the original game and validated World binding,
contract, and snapshot hashes.
These protocol identity values remain outside the artifact hash graph.
No new generated-world identifier is selected.
[ADR 0021](../../../docs/adr/0021-replay-earned-locations-after-reconnect.md)
records the collision limit and journal lifetime.

## 8. Hash dependency graph and canonicalization

Let `A`, `L`, `C`, `K`, `S`, and `W` name semantic hashes of Army, Location profile, shared contract, calibration manifest, snapshot, and World binding.
These symbols are not literal hashes for future artifacts.

```text
Army contents ------------------------------------------> A
A + AP identities and goal semantics --------------------> L
A + L + geometry + human algorithms and metrics ---------> C
A + L + calibration source byte hashes ------------------> K
C + A + L + K + world cost inputs and effective records --> S
C + S + progression, modes, and published identities ----> W

Exact A/L/C file bytes + executable bytes
    -> build metadata -> archive raw hashes
    -> release metadata -> ChessV restore lock
```

`C` declares support for cost schema 1.0, not a particular `S` or `K`.
The projector request carries `C`, never `S` as its `contract_hash`.
Neither `A` nor `L` points back to `C`.
`S` does not contain `W`.
This direction has no semantic or build cycle.

Canonical hashing retains the current convention:

1. Parse strict JSON without duplicate properties at every depth.
2. Replace only the root `manifest_sha256` value with the empty string.
3. Sort every object's keys by ordinal ASCII order.
4. Preserve each array's order.
5. Emit printable ASCII JSON without insignificant whitespace.
6. Preserve nested hash strings and serialize integers canonically.
7. Hash the resulting UTF-8 bytes with SHA-256.

E: `APMW.Client\ApmwContractV2.cs:640-773` defines this behavior.
Quoted characters and backslashes retain the existing escaping.
Decoded non-ASCII strings do not enter these canonical runtime artifacts.
Notation scalars use integers for that reason.
Object-key order and insignificant input whitespace cannot change a semantic hash.
Array order and nested content can.

Raw hashes do not blank fields, sort keys, or normalize newlines.
A whitespace-only file rewrite can retain `A` while changing its raw hash.
The release descriptor and lock must then describe the actual new file bytes.
Semantic compatibility does not excuse a raw archive mismatch.
Hashes establish content identity, not authorship by themselves.
Pinned repository, commit, immutable release, and existing provenance validation establish publication trust.

## 9. Offline distribution and publication order

### 9.1. Army publication

ChessV publishes the Army without first restoring a ChecksMate projector.
The data publication job must not depend on a finished 0.4.0 desktop package.
ChecksMate vendors the immutable Army bytes rather than querying ChessV at runtime.

The Army release descriptor has exactly:

```text
schema: "chessv_armies_release"
version: 1
source_repository: "chesslogic/chessv"
source_commit: lowercase 40-hex commit
release_tag: nonempty immutable tag
artifact: DataFile
```

The artifact file is `chessv-armies-v1.json`.
Its schema/version and semantic hash must match the contents.
Its size and raw hash must match the published bytes.
The descriptor is external to the Army, so the Army does not hash its own future commit or release.
The exact source commit and tag are publication deliverables.
This specification assigns neither a fictitious commit nor a published asset URL.

### 9.2. Projector manifests and restore lock

ChecksMate packages the same Army, Location profile, and shared contract bytes for both Windows architectures.
S: Their portable resource paths are `data/chessv-armies-v1.json`, `data/apmw-location-profile-v2.json`, and `data/apmw_contract_v4.json`.
Calibration sources and world snapshots are not compiled projector pins.

The build manifest retains schema `apmw_projector_build_manifest`, with integer `version: 2`.
Its exact fields are:

```text
schema, version
runtime_semantic_version: Semver
protocol_version: positive Int
contract_hash: Hash
minimum_client_version: Semver
target_platform: "windows"
target_architecture: "x86" or "x64"
executable_relative_path: Text
executable_sha256: Hash
data_files: { army: DataFile, location_profile: DataFile, contract: DataFile }
```

The release manifest retains schema `apmw_projector_release_manifest`, with integer `version: 2`.
The restore lock retains schema `apmw_projector_lock`, with integer `version: 2`.
Both have this exact remaining field set:

```text
runtime_semantic_version: Semver
protocol_version: positive Int
contract_hash: Hash
minimum_client_version: Semver
source_repository: Text
source_commit: lowercase 40-hex commit
release_tag: Text
data_files: { army: DataFile, location_profile: DataFile, contract: DataFile }
assets: { windows-x86: Asset, windows-x64: Asset }
```

`Asset` retains exactly `filename: Text`, `sha256: Hash`, `size: Size`, and `executable`.
`executable` retains exactly `relative_path: Text` and `sha256: Hash`.
An asset filename is a plain unique `.zip` filename.
Release tags remain `apmw-projector-v<runtime_semantic_version>`.
`source_repository` is the actual ChecksMate publication repository, not a guessed owner from its local directory name.

Runtime, protocol, semantic hash, minimum client, and all three data descriptors must agree across architectures.
The release/lock asset entries must match the embedded build manifests and extracted executable bytes.
The data descriptors must match extracted data bytes and their semantic hashes.
The contract descriptor's semantic hash must equal `contract_hash`.
The Army and Location descriptors must equal the contract bindings.

`minimum_client_version` is required in build, release, and lock metadata.
All three must equal the shared contract's field.
The restored package must fit the consuming client version.
The restore parser must validate this field, not delete it or allow arbitrary unknown fields.
The C# lock parser and PowerShell restore parser must enforce the same v2 schema.

The existing exact archive, executable, size, platform, source-commit, and provenance checks remain.
A missing declared file fails restoration.
No fallback to a cached older hash or a different architecture is permitted.

### 9.3. Publication sequence

This sequence describes future engineering work, not actions authorized by this document:

1. Freeze and validate the accepted ChessV Army data.
2. Publish its immutable bytes and external release descriptor.
3. Vendor those pinned bytes in ChecksMate.
4. Generate the reviewed Location profile and freeze shared contract 4.0.
5. Build the projector from that pinned semantic set for Windows x86 and x64.
6. Publish immutable archives and their matching release manifest with source provenance.
7. Create the ChessV restore lock from the real released metadata.
8. Restore that package and include its compatible data in the ChessV desktop package.
9. Publish compatible world/client releases only after the integration gates pass.

ChecksMate world generation also packages its reviewed calibration sources.
That package can publish a later numeric calibration revision without changing the projector archive.
No production restore lock can contain placeholder hashes or a nonexistent release.
The army publication step breaks the potential ChessV-to-ChecksMate-to-ChessV build cycle.

### 9.4. Runtime consumers

The Army artifact does not depend on the AP profile.
This permits ordinary-game reuse without requiring it in this effort.
Only the APMW consumers are implementation scope here.
Existing ordinary games do not require conversion to this data path.
The AP client uses the Army by `C.army` and the Location profile by `C.location_profile`.
It loads both from installed, pinned resources.
It does not infer arrays from AP names, counts, notation strings, or numeric ID ranges.

The starting-unit map supplies capture role and starting class.
The Location profile maps the resulting semantic goal to the AP numeric ID.
The World binding limits reports to published IDs and events.
This map survives movement and promotion because the original unit identity survives.
The client applies the Army's explicit CPU promotion and castling metadata independently of human entitlements.

The projector supplies the human roster and exact integer metrics under `C`.
The generator derives the separately pinned conservative logic metrics in section 6.3.
Neither consumer can substitute one metric definition for the other.
Its response must match request ID, protocol, runtime, and semantic hash.
E: The current response gate is `APMW.Client\ApmwSidecarProtocol.cs:241-260`.

The client reads effective costs from the authenticated world's snapshot.
It does not contact an artifact service or download the latest calibration.
Reconnect and tracker reconstruction retain that world's original snapshot.
This is an offline artifact requirement, not a prohibition on the normal AP server connection.

## 10. Lifecycle gates and required wire fixtures

### 10.1. Compatibility gates

The existing version gate remains the enforceable first barrier for an older client.
The producer sets `required_chess_client_version` to the contract's minimum.
E: Older-client source compares this value before contract setup at `APMW.Client\Client.cs:209-230`.
An older supported client below 0.4.0 must refuse the new world.
The release evidence must exercise this real gate, not assume that every older binary understands schema 4.

The new client then requires all v4 artifacts and matching installed resources.
Its strict schema, semantic-pin, data-binding, and snapshot gates run before match setup.
Malformed JSON must fail before a lossy dictionary parser can erase duplicates.
Successful application-version comparison alone never permits a match.

| Client/world pair | Required result |
| --- | --- |
| Compatible 0.4.0 client, exact C/A/L, valid W/S, supported requested stage | Permit contract entry, subject to normal setup requirements |
| Older client below the declared minimum, new world | Refuse through the client-version gate |
| New client, old world contract, requested 8x8 | Refuse through the required-contract/schema/pin gates |
| New client, missing contract, Legacy itemization | Refuse. Legacy itemization does not restore a legacy world path |
| New client, exact new contract, Legacy itemization | Permit contract entry |
| Both application labels are 0.4.0, but semantic data differ | Refuse |
| Valid semantic set, requested 12x12 | Refuse at the shared geometry entry point |
| Same supported semantic set, compatible newer calibration revision | Accept its valid new-world snapshot without a binary update |
| Reconnect to an existing world after a numeric revision | Retain the original S and costs |

A contract failure must identify the artifact or binding that failed.
The client must not guess a layout, downgrade a profile, or silently continue without costs.

### 10.2. Fixture construction rules

These are templates for future fixtures, not claims that new tests ran.
The implementation must materialize complete JSON inputs and expected results.

**Positive reference P8** uses the first released A/L/C set, runtime 0.2.0, protocol 1, and client 0.4.0.
Its world starts and ends at 8x8, with Legacy/Stable and tactics `all`.
Its requested match is Standard CPU 8x8.
Its numeric IDs come from the actual reviewed Location profile.
Its valid S and W use computed hashes, not placeholder digest strings.
All installed resource descriptors, manifests, and response bindings agree.

**Positive reference P12** uses the same A/L/C and versions.
Its declared progression is 6x8, 8x8, 10x8, 10x10, and 12x10.
Its endpoint is 12x10, with Fundamental/Stable and tactics `all`.
Its published IDs, eligibility sets, and snapshot profiles follow that progression.

For each negative case, clone its named positive input and apply only the stated mutation.
The different-hash mutation changes only the first hexadecimal character.
If that character is `0`, replace it with `1`.
Otherwise, replace it with `0`.
Byte-integrity cases keep the original expected digest.
Structural cases recompute the changed document's root digest before schema validation.
Binding cases retain the referenced authoritative document.
Their changed containing document receives a valid digest, so the test can exercise the binding error rather than malformed hashing alone.
Dependent container hashes, such as W's reference to S, update mechanically where necessary.
These mechanical updates are not extra semantic mutations.

Isolated parser fixtures can supply a synthetic supported pin for their complete fixture set.
That harness facility is not permission for the production client to accept arbitrary semantic hashes.
Package and lifecycle fixtures must also exercise the actual released supported pin.

| Case and positive counterpart | Single mutation or operation | Required result |
| --- | --- | --- |
| Missing-contract / P8 | Remove `apmw_contract` | Refuse even with Legacy itemization and 8x8 |
| Old-schema / P8 | Substitute the complete frozen old v2 contract | Refuse before match setup. Supported geometry is irrelevant |
| Producer-v3 / P8 | Substitute the current complete producer v3 contract | Refuse despite its minimum-client value of 0.4.0 |
| Old-client / P8 | Change the consuming client version to a supported older version below 0.4.0 | The existing client-version gate refuses |
| Minimum disagreement / P8 | Change only the slot's required-client mirror from 0.4.0 to 0.4.1 | Refuse conflicting authorities |
| Layout mismatch / P8 | Replace the installed Army with a different valid Army | Refuse the raw or semantic binding mismatch |
| Location mismatch / P8 | Replace the installed Location profile with a legacy profile | Refuse, without reconstruction or aliases |
| Algorithm mismatch / P8 | Change only `algorithms.projection` to an unsupported ID | Strict semantic validation refuses |
| Metric mismatch / P8 | Change only `expected_material.major` from 485 to the catalog Rook value 500 | Refuse. Catalog values cannot repair projection data |
| Unsupported geometry / P12 | Request 12x12 | Refuse at the geometry gate |
| Unsupported record / P12 | Add one 12x12 stage record | Reject the artifact, not only a later selector request |
| Wrong snapshot binding / P8 | Replace only S's `bindings.army_sha256` with a different well-formed hash | Refuse despite a valid S digest |
| Missing cost / P8 | Remove one required Location/stage profile from S | Refuse. No zero-cost default |
| Duplicate cost / P8 | Duplicate one Location/stage profile in S | Refuse. No first/last record selection |
| Unknown schema field / P8 | Add one unknown root field to S | Refuse while the unchanged P8 snapshot remains valid |
| Unknown minor / P8 | Change S version from 1.0 to 1.1 | Refuse under the initial exact-schema rule |
| Family fallback / P8 | Separately omit the selector or supply an unknown selector string | Both resolve to the same Standard units and promotions as explicit Standard |
| Artifact family error / P8 | Change one Army formation's family ID to an unknown ID | Reject. Selector fallback does not repair data |
| Capacity equality / P12 | Inspect the 10x10 and 12x10 records in both itemization modes | Capacities are 59/29/50/40 and 71/35/60/48 |
| Capacity mismatch / P12 | Change only 12x10 non-primary capacity from 71 to 83 | Reject the stale seven-rank result |
| Maxima equality / P12 | Inspect Fundamental maxima | Chessmen 71, Material 213, Castler 2 |
| Maxima mismatch / P12 | Change only Fundamental Material maximum to 321 | Reject |
| Build-metadata acceptance / P8 | Use the complete new x86 or x64 build manifest, including minimum client | Strict restore validation accepts the matching package |
| Build-metadata omission / P8 | Remove only `minimum_client_version` from that manifest | Strict restore validation refuses |
| Build-metadata conflict / P8 | Change only its minimum client from 0.4.0 to 0.4.1 | Strict restore validation refuses |
| Archive integrity / P8 | Alter one byte in the archive without updating the lock | Refuse the raw hash mismatch |
| Response binding / P8 | Change only response `contract_hash` from C to S | Refuse. The projector binds shared semantics, not per-world costs |
| Tactic predicate retained / P12 | Use a Fundamental stage with logic chessmen 0 and an unlocked later stage | The later-stage resource bypass does not make a fork or non-pawn threat path qualify |
| Tactic predicate retained / P8 | Use Legacy with none of the three required item types, even when the material estimate or bypass qualifies | The non-pawn threat or fork path remains unavailable |

### 10.3. Deployment and cost fixtures

The accepted array/catalog references are complete fixture inputs, not descriptive suggestions.
A materialized fixture must expand all 20 formations, not one easier representative.
For both colors, it validates every starting record, declared blank, role binding, count, and band.
The black expected array reflects ranks without reversing files.

| Positive case | Isolated negative counterpart |
| --- | --- |
| Approved 10x10 has basic Elephants on C2/H2 and role bindings from issue 15 | Change one Elephant's type to War Elephant. Reject the approved-array mismatch |
| Approved Rookies 12x10 has six Lions with distinct unit/role IDs | Alias the B1 Lion role to the E1 Bishop role. Reject duplicate or incorrect role coverage |
| The White CPU 12x10 primary at G1 maps to Black G10 | Reverse the file as well as the rank. Reject the transformed array |
| The normal CPU castling table uses the original primary and corner actors | Replace one route's primary ID with the additional King's ID. Reject |
| The full 6x8 Standard list includes Queen promotion | Remove Queen from that list. Reject even though the starting array lacks a Queen |
| Every family follows ADR 0017 exactly | Add basic Elephant, Amazon, King, or the old Nightrider extension to one target list. Reject |
| Basic Elephant belongs to the Minor tactical class under Q34 | Remove it from Minor or add it to Major or Queen. Reject |
| P8 has no invented Regicide profile | Add a single-King Regicide profile. Reject static eligibility |
| A 6x8-only positive world has no castling Location/profile | Add a zero-cost castling profile. Reject rather than treating zero as capability |

The parent owns the legal-board, royal transition, castling-right, undo, and load fixtures.
Their authority is `.scratch\large-board-enemy-armies\contracts\royal-lifecycle-fixtures.md`.
This document adds data-integrity counterparts, not substitute gameplay boards.

The cost arithmetic fixtures reuse `world-cost-snapshot-v1.md:273-287` exactly:

| Inputs held fixed unless stated | Required effective result |
| --- | --- |
| King's Rearguard on 12x10, intrinsic 361045/162, difficulty 27/20, adjustment 240, material cap 5000, chessmen 0 | Material 3249, chessmen 0 |
| Same case, material cap 3000 | Material 3000, intrinsic still 361045/162 |
| Synthetic compatible revision adds 100 intrinsic material, all other first-row inputs unchanged | Intrinsic 377245/162, effective material 3384 |
| Reconnect to the first world after the synthetic revision exists | Original material 3249 |

The synthetic revision approves no production repricing.
The same compatible client accepts both valid snapshots.
Premature rounding that produces 3250 is an isolated negative arithmetic case.
Client-side recomputation is not the mechanism for accepting the positive case.

### 10.4. Deterministic canonicalization fixtures

These are hash-primitive inputs, not complete release manifests.
The words `ignored` and `nested` are test strings, not future hash values.

Input:

```json
{"z":2,"manifest_sha256":"ignored","a":{"manifest_sha256":"nested"},"b":[2,1]}
```

Exact canonical bytes, before UTF-8 hashing:

```json
{"a":{"manifest_sha256":"nested"},"b":[2,1],"manifest_sha256":"","z":2}
```

| Positive reference | One change | Required result |
| --- | --- | --- |
| Primitive input above | Reorder object properties and add insignificant whitespace | Same canonical bytes and semantic digest |
| Primitive input above | Change only the root hash text | Same canonical bytes and semantic digest |
| Primitive input above | Reverse `b` to `[1,2]` | Different canonical bytes and semantic digest |
| Primitive input above | Change only the nested hash text | Different canonical bytes and semantic digest |
| Primitive input above | Duplicate root key `z` | Reject before hashing |
| Primitive input above | Duplicate the nested `manifest_sha256` key | Reject before hashing |
| A valid artifact's integer field | Replace the integer with a floating-point number, string, or boolean | Reject the schema rather than coerce |
| The same valid Army bytes | Change only input whitespace | Same Army semantic hash, different raw file hash |
| The valid original archive and lock | Use the whitespace-changed Army without updating declared byte hashes | Reject package integrity despite semantic equality |

### 10.5. Future evidence scope

The future implementation uses existing targeted test surfaces.
It does not need a new test framework or broad gameplay campaign to prove these wire contracts.

| Boundary | Existing test surface |
| --- | --- |
| Client contract, lifecycle version, and geometry gates | `APMW.Test\ApmwContractV2Tests.cs`, `ClientVersionContractTests.cs`, `ApmwGeometrySelectionTests.cs` |
| Capacity and unchanged projection behavior | `APMW.Test\ActiveRosterProjectionTests.cs`, `ApmwGeometryAwareProviderIntegrationTests.cs`, `ProjectionV2FixtureParityTests.cs` |
| Lock, restoration, and real package consumption | `APMW.Test\ApmwProjectorLockTests.cs`, `ProjectorRestoreValidation.ps1`, and the existing sidecar/projection boundary |
| Producer resource, protocol, and packaging | ChecksMate `worlds\checksmate\test\test_contract_resource.py`, `test_contract_runtime_consistency.py`, `test_apmw_projection_protocol.py`, `test_apmw_projector_build.py`, `test_apmw_projector_release_manifest.py` |
| Location/cost data and rule reconstruction | ChecksMate `worlds\checksmate\test\test_geometry_location_profiles.py`, `test_rules_thresholds.py`, `test_location_logic.py`, `test_logic_projection.py`, `test_lifecycle_capacity.py` |

Future selectors can extend or rename those suites for v4 without retaining old-world support in the live client.
Package evidence must restore and consume the real release for both architectures.
Source-only parity or a fake executable is not proof of real package consumption.
E: `.github\workflows\dotnet-desktop.yml:87-102` runs unit tests before the optional projector restoration.
That current order cannot by itself establish the required package evidence.
No build, gameplay test, or release validation is claimed by this specification.

## 11. Remaining dependencies

### Final design closure

Q34 settles the basic Elephant tactical class as Minor.
It grants no human random-pool or promotion permission.
No unresolved gameplay policy field remains in this specification.

No new distribution or version question requires a gameplay decision.
The parent reviewed the concrete schema and publication choices.
Q35 confirms these engineering selections and marks the map ready for a
later handoff.
The Q32 cost scope, Q33 CPU lists, five geometries, and new-contract-only boundary are settled.

### Implementation evidence, not missing human decisions

Actual Army serialization, collision-free notation/image registration, reviewed AP numeric IDs, evaluator bindings, and complete capability records remain future deliverables.
The implementation must demonstrate that preserved event predicates and projection subtrees retain their intended behavior.
The finite special-condition registry explicitly retains the current tactic
and castler predicates.
Integration must demonstrate their presence on each complete geometry path.
An uncovered behavior choice must return to the parent rather than become an invented predicate.

Actual source commits, semantic hashes, raw hashes, sizes, archive names, published assets, and restore pins come from produced artifacts.
Their absence is not a missing human design decision.
No placeholder value can enter a production lock, slot object, or release manifest.
The producer-v3/client-v2 and restore-manifest gaps remain implementation work.

### Parent-owned specification and integration

The parent-owned royal fixtures specify the selected terminal outcomes and
their stage-victory reporting boundary.
Q35 completes the parent-owned design-closure review.
This file does not edit or resolve those records.
The owning records separately record their resolutions.
This specification does not authorize execution.
