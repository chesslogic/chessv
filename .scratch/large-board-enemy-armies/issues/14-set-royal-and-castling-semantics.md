# Set royal, check, stalemate, castling, and evaluation semantics

Type: grilling  
Status: open  
Blocked by: 11, 12, 13

## Question

Given the resolved piece identities and arrays, which CPU pieces are royal, what ends the game, which piece may castle with which castler, and which engine evaluations must recognize the new pieces?

The answer must settle:

- whether every ordinary or Mounted King in the starting array is royal;
- whether the CPU has one primary royal, multiple simultaneous royals, or capturable non-royal king-movers;
- how Extinction/Covenant and `ApmwStalemateRule` behave after each royal capture;
- whether an Amazon is ever royal;
- whether castling remains tied to the primary King and corner castlers;
- whether moved/forward/additional royals receive castling rights;
- whether Tower or family analogues are castlers;
- saved FEN castling-right identity and colorbound-castler parity;
- rook/open-file/seventh-rank, outpost, king-safety, and material evaluation registration for each new starting type.

## Resolution requirements

Provide truth tables for check legality, royal capture, no-legal-move outcomes, and castling rights for both human and CPU sides on each formal geometry (`6x8`, `8x8`, `10x8`, `10x10`, and `12x10`), distinguishing preserved baseline behavior from augmented behavior. Do not rely on base `CheckmateRule` multi-royal support without reconciling APMW's replacement rules.
