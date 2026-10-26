"""Throwaway bounded completion probe. No production imports or public schema."""

import hashlib
import itertools
import json
from dataclasses import dataclass
from pathlib import Path


HERE = Path(__file__).resolve().parent
EXPECTATIONS = HERE / "tiny-completion-expectations.json"
BOARD_EXPECTATIONS = HERE / "tiny-board-price-expectations.json"
BOARD_DATA = json.loads(BOARD_EXPECTATIONS.read_text(encoding="utf-8"))
PROFILES = BOARD_DATA["profiles"]

# Concrete identity and role predicates do not depend on board price.
# Owned slots protect100; synthetic extra charge=max(0,price-100). Births pay price.
DP_PROFILE = {
    "a-checker": (0, 0, ("M", "P")),
    "b-pawn": (0, 0, ("M", "P")),
    "c-better": (0, 0, ("M", "P")),
    "d-minor": (1, 0, ("H", "M")),
    "e-castler": (1, 1, ("H",)),
}

# Oracle shares declared price inputs, not DP metadata, rows, predicates or reducers.


@dataclass(frozen=True)
class Label:
    occupancy: tuple
    n: int
    nonpawn: int
    castlers: int
    board: int
    spend: int
    rows: tuple


def dp(case, include_board=True):
    """Slot-layer DP, then bounded birth layers. Exact B belongs in the key."""
    bands = ("H", "M", "P")
    capacity = tuple(sum(s.startswith(b) for s in case["squares"]) for b in bands)
    pin_squares = list(case["pins"].values())
    if len(set(pin_squares)) != len(pin_squares):
        return None, {"transitions": 0, "max_labels": 0}
    if not set(pin_squares).issubset(case["squares"]):
        return None, {"transitions": 0, "max_labels": 0}
    owned_count = len(case["slots"])
    prices = PROFILES[case.get("stage", "tiny-A")]
    layers = list(case["slots"]) + [
        [None, "a-checker", "b-pawn"] for _ in range(case["births"])
    ]
    start = Label((0, 0, 0), 0, 0, 0, 0, 0, ())
    states = {(): start}
    transitions = 0
    max_labels = 1
    for index, members in enumerate(layers):
        next_states = {}
        pin = case["pins"].get(str(index))
        for old in states.values():
            for member in members:
                if member is None:
                    options = [(None, None)]
                else:
                    options = [
                        (member, band) for band in DP_PROFILE[member][2]
                        if pin is None or pin.startswith(band)
                    ]
                for member, band in options:
                    transitions += 1
                    if member is None:
                        new = Label(
                            old.occupancy, old.n, old.nonpawn, old.castlers,
                            old.board, old.spend, old.rows + ((None, None),),
                        )
                    else:
                        nonpawn, castler, _ = DP_PROFILE[member]
                        mg = prices[member]
                        debit = max(0, mg - 100) if index < owned_count else mg
                        occupancy = list(old.occupancy)
                        occupancy[bands.index(band)] += 1
                        if any(a > b for a, b in zip(occupancy, capacity)):
                            continue
                        if old.spend + debit > case["funds"]:
                            continue
                        new = Label(
                            tuple(occupancy), old.n + 1, old.nonpawn + nonpawn,
                            old.castlers + castler, old.board + mg,
                            old.spend + debit, old.rows + ((member, band),),
                        )
                    key = (new.occupancy, new.n, new.nonpawn, new.castlers)
                    if include_board:
                        key += (new.board,)
                    previous = next_states.get(key)
                    row_key = tuple((m or "", b or "") for m, b in new.rows)
                    prior_key = () if previous is None else tuple(
                        (m or "", b or "") for m, b in previous.rows
                    )
                    if previous is None or (new.spend, row_key) < (previous.spend, prior_key):
                        next_states[key] = new
        states = next_states
        max_labels = max(max_labels, len(states))
    winners = []
    for label in states.values():
        pocket_order = sorted(
            enumerate(case["pockets"]), key=lambda pair: (pair[1] == "pawn", pair[0])
        )
        supported = [family for _, family in pocket_order[:label.n // 2]]
        pocket_nonpawn = sum(family != "pawn" for family in supported)
        pocket_material = sum(100 if family == "pawn" else 300 for family in supported)
        credit = min(pocket_material, label.board // 2, 300 * len(supported))
        if label.n + len(supported) < case["total_min"]:
            continue
        if label.nonpawn + pocket_nonpawn < case["nonpawn_min"]:
            continue
        if label.castlers < case["castlers"]:
            continue
        if label.board + credit < case["access_min"]:
            continue
        # Pins reserve exact squares before interchangeable class expansion.
        free = sorted(set(case["squares"]) - set(pin_squares))
        entries = []
        for index, (member, band) in enumerate(label.rows):
            if member is None:
                continue
            square = case["pins"].get(str(index))
            if square is None:
                square = next(s for s in free if s.startswith(band))
                free.remove(square)
            entries.append((index, member, square))
        winners.append({
            "spend": label.spend, "B": label.board, "H": credit,
            "access": label.board + credit, "n": label.n,
            "nonpawn": label.nonpawn, "castler_count": label.castlers,
            "supported": supported,
            "birth_count": sum(i >= owned_count for i, _, _ in entries),
            "protected_base": 100 * owned_count,
            "base_refund": 0,
            "extra_remaining": case["funds"] - label.spend,
            "stage": case.get("stage", "tiny-A"),
            "entries": entries,
        })
    best = min(winners, key=lambda w: (w["spend"], w["entries"])) if winners else None
    return best, {"transitions": transitions, "max_labels": max_labels}


def oracle(case):
    """Enumerate concrete member/square products directly. No DP state merging."""
    choices = []
    for index, allowed in enumerate(case["slots"]):
        choices.append([
            (index, member, square)
            for member in allowed for square in case["squares"]
            if str(index) not in case["pins"] or case["pins"][str(index)] == square
        ])
    for birth in range(case["births"]):
        index = len(case["slots"]) + birth
        choices.append([None] + [
            (index, member, square)
            for member in ("a-checker", "b-pawn")
            for square in case["squares"]
        ])
    results = []
    combinations = 0
    for combination in itertools.product(*choices):
        combinations += 1
        entries = [entry for entry in combination if entry is not None]
        squares = [entry[2] for entry in entries]
        if len(set(squares)) != len(squares):
            continue
        legal = True
        for _, member, square in entries:
            if member == "e-castler":
                legal &= square.startswith("H")
            elif member == "d-minor":
                legal &= square[0] in "HM"
            else:
                legal &= square[0] in "MP"
        if not legal:
            continue
        prices = BOARD_DATA["profiles"][case.get("stage", "tiny-A")]
        spend = 0
        for index, member, _ in entries:
            value = prices[member]
            if index >= len(case["slots"]):
                spend += value
            elif value > 100:
                spend += value - 100
        if spend > case["funds"]:
            continue
        n = len(entries)
        nonpawn = sum(m in ("d-minor", "e-castler") for _, m, _ in entries)
        castlers = sum(m == "e-castler" for _, m, _ in entries)
        ordered = [p for p in case["pockets"] if p != "pawn"]
        ordered += [p for p in case["pockets"] if p == "pawn"]
        supported = ordered[:n // 2]
        if n + len(supported) < case["total_min"]:
            continue
        if nonpawn + sum(p != "pawn" for p in supported) < case["nonpawn_min"]:
            continue
        if castlers < case["castlers"]:
            continue
        board = sum(prices[m] for _, m, _ in entries)
        pocket_sum = sum({"pawn": 100, "minor": 300}[p] for p in supported)
        credit = min(pocket_sum, board // 2, 300 * len(supported))
        if board + credit < case["access_min"]:
            continue
        results.append({
            "spend": spend, "B": board, "H": credit,
            "entries": entries, "supported": supported,
        })
    return results, combinations


def compare(case):
    witness, stats = dp(case)
    exhaustive, combinations = oracle(case)
    assert (witness is not None) == bool(exhaustive), ("feasibility", case, witness, exhaustive)
    if witness is not None:
        assert witness["spend"] == min(w["spend"] for w in exhaustive), ("cost", case)
        exact = [
            w for w in exhaustive if w["entries"] == witness["entries"]
            and w["spend"] == witness["spend"] and w["B"] == witness["B"]
            and w["H"] == witness["H"] and w["supported"] == witness["supported"]
        ]
        assert exact, ("witness not in independent oracle", case, witness)
        assert witness["base_refund"] == 0
        assert witness["protected_base"] == 100 * len(case["slots"])
        assert witness["spend"] + witness["extra_remaining"] == case["funds"]
    return witness, {**stats, "oracle_combinations": combinations}


def main():
    frozen = json.loads(EXPECTATIONS.read_text(encoding="utf-8"))
    default = frozen["defaults"]
    edges = []
    totals = {"comparisons": 0, "oracle_combinations": 0, "dp_transitions": 0, "max_labels": 0}

    def run(case):
        witness, stats = compare(case)
        totals["comparisons"] += 1
        totals["oracle_combinations"] += stats["oracle_combinations"]
        totals["dp_transitions"] += stats["transitions"]
        totals["max_labels"] = max(totals["max_labels"], stats["max_labels"])
        return witness

    for entry in frozen["cases"] + BOARD_DATA["cases"]:
        case = {**default, **entry}
        witness = run(case)
        expected = entry["expected"]
        assert (witness is not None) == expected["feasible"], ("frozen expectation", entry["id"])
        for key, value in expected.items():
            if key != "feasible":
                assert witness[key] == value, ("frozen field", entry["id"], key, witness)
        edges.append({"id": entry["id"], "expected": expected, "witness": witness})

    by_id = {entry["id"]: {**default, **entry} for entry in frozen["cases"]}
    transactions = []
    for name, before_id, after_id, standalone_debit, expected_admission in (
        ("castler-funding-guard", "castler-funds-before", "castler-funds-after", 50, False),
        ("castler-placement-guard", "castler-placement-before", "castler-placement-after", 20, False),
        ("castler-compatible-purchase", "castler-placement-before", "castler-placement-compatible", 20, True),
    ):
        before = run(by_id[before_id])
        after = run(by_id[after_id])
        assert before is not None
        # Price-only comparison intentionally excludes the mandatory completion debit.
        assert standalone_debit <= by_id[after_id]["funds"]
        assert (after is not None) == expected_admission
        transactions.append({
            "id": name, "standalone_affordable": True,
            "post_purchase_admitted": after is not None,
            "before": before, "after": after,
        })

    improved_case = by_id["all-prefix-improvement"]
    before_case = {**improved_case, "slots": [["b-pawn"]]}
    before = run(before_case)
    improved = run(improved_case)
    assert before is not None and improved is not None
    assert before["entries"][0][0] == improved["entries"][0][0] == 0
    assert before["entries"][0][2] == improved["entries"][0][2] == "P0"
    assert before["n"] == improved["n"] == 1
    assert improved["access"] >= before["access"]
    assert improved["spend"] - before["spend"] == 50
    transactions.append({
        "id": "all-prefix-improvement", "standalone_affordable": True,
        "post_purchase_admitted": True, "before": before, "after": improved,
        "preserved": ["actor0", "P0", "count1", "Pawn role", "funded100 base", "access floor100"],
        "castler_requirement": 0,
    })

    members = ["a-checker", "b-pawn", "c-better", "d-minor", "e-castler"]
    grid_count = 0
    for stage, square_set, owned, births, funds, targets, pockets, access in itertools.product(
        ["tiny-A", "tiny-B"],
        [("H0", "M0", "P0"), ("H0", "H1", "M0", "P0"), ("M0", "P0")],
        [0, 1, 2], [0, 1], [0, 20, 40, 50, 60, 80, 110],
        [(0, 0, 0), (2, 1, 0), (3, 1, 1)],
        [[], ["minor"], ["pawn", "minor"]],
        [0, 100, 300],
    ):
        total, nonpawn, castlers = targets
        case = {
            **default, "stage": stage, "squares": list(square_set), "slots": [members] * owned,
            "births": births, "funds": funds, "total_min": total,
            "nonpawn_min": nonpawn, "castlers": castlers,
            "pockets": pockets, "access_min": access,
        }
        run(case)
        grid_count += 1

    # Mutation: remove B from the equivalence key. Find the smallest no-birth case.
    counterexample = None
    for owned in (0, 1):
        for access in range(1, 101):
            case = {
                **default, "squares": ["P0"],
                "slots": [["a-checker", "b-pawn"]] * owned,
                "access_min": access,
            }
            exhaustive, _ = oracle(case)
            defective, _ = dp(case, include_board=False)
            if bool(exhaustive) != (defective is not None):
                corrected, _ = dp(case)
                counterexample = {
                    "case": case, "defective": defective, "corrected": corrected,
                    "exhaustive_min_spend": min(w["spend"] for w in exhaustive),
                    "minimality_scope": "0..1 owned actors, no births, funds0, one P square, thresholds1..100, two base members",
                }
                break
        if counterexample:
            break
    assert counterexample and counterexample["case"]["access_min"] == 41

    result = {
        "status": "pass",
        "scope": "synthetic-tiny-fundamental-v1; not native, scheduler, full compiler, monotonicity, or performance closure",
        "profile": DP_PROFILE,
        "board_profiles": PROFILES,
        "frozen_expectations_sha256": hashlib.sha256(EXPECTATIONS.read_bytes()).hexdigest(),
        "board_expectations_sha256": hashlib.sha256(BOARD_EXPECTATIONS.read_bytes()).hexdigest(),
        "probe_sha256": hashlib.sha256(Path(__file__).read_bytes()).hexdigest(),
        "explicit_edge_cases": len(edges), "grid_cases": grid_count,
        "totals": totals, "edge_results": edges, "transaction_results": transactions,
        "B_omission_mutant_counterexample": counterexample,
    }
    output = HERE / "tiny-completion-results.json"
    output.write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({
        "status": result["status"], "edge_cases": len(edges), "grid_cases": grid_count,
        **totals, "B_omission_smallest_threshold": 41, "output": str(output),
    }, indent=2))


if __name__ == "__main__":
    main()
