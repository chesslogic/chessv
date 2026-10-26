"""Bounded evidence probe, not a successor allocator or production test."""

import json
import sys
from pathlib import Path

sys.path.insert(0, str(Path.cwd()))

from BaseClasses import CollectionState
from test.general import setup_multiworld
from worlds.checksmate import CMWorld
from worlds.checksmate.locations import BoardStage
from worlds.checksmate.semantic_projection import project_semantic_roster


multiworld = setup_multiworld(
    CMWorld,
    steps=("generate_early", "create_regions", "create_items"),
    seed=7,
    options={"progression_itemization": "legacy", "difficulty": "grandmaster"},
)
world = multiworld.worlds[1]
projection = world.logic_projection
projection.set_obtainable_counts({"Progressive Major Piece": 2})
counts = {"Progressive Major Piece": 2, "Progressive Major To Queen": 1}
reported = projection.metrics_from_counts(counts, BoardStage.Board8x8)
exact = project_semantic_roster(
    projection.contract, projection._projection_input(counts, BoardStage.Board8x8)
)
assert reported.castlers == 2
assert len(exact.castling_eligible_slots) == 1

state = CollectionState(multiworld)
world.collect(state, world.create_item("Progressive Pocket"))
pocket_metrics = projection.metrics(state, 1, BoardStage.Board8x8)
assert pocket_metrics.chessmen == 1
assert pocket_metrics.material == 0

projection.set_obtainable_counts({"Progressive Major Piece": 2})
world._set_logic_obtainable_counts({"Progressive Major Piece": 2})
before = world.fill_slot_data()["logic_obtainable_counts"]
multiworld.itempool.append(world.create_item("Progressive Major To Queen"))
after = world.fill_slot_data()["logic_obtainable_counts"]
assert before == after
assert after["Progressive Major To Queen"] == 0

# A finite, independent arithmetic model. These are synthetic admitted witnesses.
# Five mandatory Pawns cost40 each, three mandatory non-Pawns cost325 each.
# All optional contributions are nonnegative. This does not prove admission.
board_floor = 5 * 40 + 3 * 325
semantic_floor = 5 * 100 + 3 * 300
supported_semantic = 300 + 300 + 100
supported_count = 3
pocket_floor = min(supported_semantic, board_floor // 2, 300 * supported_count)
assert (board_floor, pocket_floor, board_floor + pocket_floor) == (1175, 587, 1762)
assert semantic_floor + pocket_floor == 1987
witnesses = 0
for extra_actors in range(3):
    for extra_board in (0, 40, 325):
        n = 8 + extra_actors
        board = board_floor + extra_board
        p = min(supported_count, n // 2)
        h = min(supported_semantic, board // 2, 300 * p)
        assert n + p >= 11
        assert 3 + 2 >= 5
        assert board + h >= 1762
        assert semantic_floor + h >= 1987
        witnesses += 1

print(json.dumps({
    "legacy_stale_queen_bound": {
        "frozen_obtainable_queens": 0,
        "actual_counts": counts,
        "logic_castlers": reported.castlers,
        "semantic_castling_eligible": len(exact.castling_eligible_slots),
    },
    "unsupported_pocket_current_metrics": {
        "board_nonprimary": 0,
        "pocket_items": 1,
        "logic_chessmen": pocket_metrics.chessmen,
        "logic_material": pocket_metrics.material,
        "successor_supported_pockets": 0,
    },
    "late_pool_addition_refreshes_export": before != after,
    "synthetic_witness_floor": {
        "board": board_floor,
        "pocket": pocket_floor,
        "combined": board_floor + pocket_floor,
        "semantic_board": semantic_floor,
        "semantic_plus_pocket": semantic_floor + pocket_floor,
        "composition": [11, 5],
        "finite_witnesses_checked": witnesses,
        "scope": "arithmetic only; funding, placement, and future preservation assumed",
    },
}, indent=2))
