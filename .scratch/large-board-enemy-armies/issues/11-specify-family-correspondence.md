# Specify the family correspondence matrix

Type: grilling  
Status: open  
Blocked by: 02, 04, 07, 08

## Question

For every added 10x10 and 12x10 formation role, which exact piece type represents that role in Standard, Colourbound Clobberers, Remarkable Rookies, and Nutty Knights, and what value/movement tolerance makes two pieces valid correspondents?

The answer must provide a complete matrix for:

- each added minor-role slot;
- each Tower/Gardener-role slot;
- Queen and Amazon roles;
- any primary or additional royal role;
- any castler-like role;
- promotion fragments if the new starting piece must also become a CPU promotion option.

It must also decide whether “10x10 uses Towers” means a literal Tower in Standard only, a shared cross-family role anchored to Tower value/movement, or a literal Tower in every family despite the family-preservation rule.

## Resolution requirements

- Name exact catalog piece types, not tiers alone.
- State the comparison basis: midgame value, endgame value, weighted value, movement role, colorbinding, or an explicit combination.
- Identify any family slot that has no acceptable existing analogue and choose whether to reuse, relax the tolerance, or reject the candidate formation.
- Keep player `major_to_amazon` lineage separate from CPU correspondence authority.
