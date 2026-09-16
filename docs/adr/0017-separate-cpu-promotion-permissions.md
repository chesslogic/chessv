# Separate CPU promotion permissions from human entitlements

Status: accepted
Date: 2026-09-15

The CPU's promotion permissions come from its resolved family and geometry.
They do not borrow human inventory or pocket entitlements.
Registering a CPU piece does not grant promotion rights to either side.

## CPU promotion lists

| Family | 6x8 and 8x8 | Additional targets on 10x8, 10x10, and 12x10 |
| --- | --- | --- |
| Standard | Rook, Knight, Bishop, Queen | Archbishop, Chancellor |
| Colourbound Clobberers | Cleric, Phoenix, War Elephant, Archbishop | Queen, Chancellor |
| Remarkable Rookies | Short Rook, Tower, Lion, Chancellor | Archbishop, Queen |
| Nutty Knights | Charging Rook, Lancer, Charging Knight, Colonel | Archbishop, Chancellor |

Explicit Standard, absent family input, and unknown family input produce the
same Standard list.
The approved CPU pawn behavior and promotion rank remain unchanged.

The first release does not add basic Elephant as a promotion target.
It also removes the old twelve-file Nightrider promotion extension.
Lion remains a Rookies target through the family list.
The outer Lion pair does not grant Lion promotion to other families.
The accepted Amazon and royal promotion exclusions remain unchanged.

## Consequences

Standard 6x8 can create a Queen through pawn promotion despite its absent
starting Queen role.
That promoted Queen can supply a tactical Queen target.
Its starting pawn identity remains unchanged for individual captures and
capture counters.

CPU promotion permissions belong in the shared army artifact.
Type registration must cover the starting army and its promotion targets.
That registration must not expand human promotion permissions or random
piece pools.

This decision does not redesign human item progression.
It replaces the accidental shared-list dependency with explicit CPU
permissions.
The implementation must preserve the separately authorized human entitlements.

## Source

Q33: "Adopt these CPU-only lists, with no additional promotion targets".

[Royal/capability report](../../.scratch/large-board-enemy-armies/research/royal-capability-frontier.md)
and [family correspondence](../../.scratch/large-board-enemy-armies/issues/11-specify-family-correspondence.md).
