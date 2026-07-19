# Pending README Content

> This file holds `README.md` documentation for functionality that exists on `develop` but is not yet part of a tagged release (currently anything newer than `v0.3.2` / commit `6fe9997`, which is what the `release` branch and the published ChecksMate client / `checksmate.apworld` actually ship). GitHub renders `develop`'s `README.md` live by default, so documenting unreleased functionality directly there makes the project look unfinished or unreliable to anyone reading it before a matching release exists.
>
> **Convention:** when you add README documentation for a feature that has not shipped in a tagged release yet, add it here instead of editing `README.md` directly. When cutting the next release, fold each section below into the matching section of `README.md` as described, then empty this file back down to this template.

## ChecksMate Client

### Supported Options

Add this bullet (after "Piece Limits...", before "Extra Kings..."):

```
 - Progression Itemization. Legacy seeds use family-specific board-material items; fundamental seeds use `Chessmen`, `Material`, and optional `Castler` items instead.
```

Change the last bullet in this list (currently ends "...fairy pawns, and Jacks!") to:

```
 - Difficulty, AI Intelligence malus, enemy army, Super Mode, DeathLink, fairy pawns, Jacks, and Amazons!
```

#### Option reference notes

Add these bullets (after "Difficulty and AI Intelligence differ...", before "The \"Change Enemy Army\" dropdown..."):

```
 - `progression_itemization` may be legacy or fundamental. Fundamental replaces only board-material progression with `Chessmen`, `Material`, and optional `Castler`; pockets, AI malus, Play as White, Super-Size Me, DeathLink, and similar progression remain unchanged.
 - In fundamental itemization, each `Chessmen` grants one non-king generated slot plus 100 base material, and each `Material` grants `material_item_value` material (default 400). Each generated slot independently graduates from Pawn toward Minor, Major, Jack, Queen, and Amazon as the shared material budget allows; `piece_upgrade_preferences` remains the deterministic spending priority contract, an optional `piece_upgrade_proportion` dictionary weights which equally-preferred action is chosen when two or more share a priority tier (an action absent from this dictionary defaults to a weight of 1; a weight of 0 always loses to any competitor still in contention, though it still applies as a last resort if it becomes the only viable action left), and an internal seeded random choice (not list/config order) breaks such ties -- proportionally to weight when they differ, otherwise uniformly. Legacy itemization now shares this same seeded per-unit tie-break for its own upgrade actions (e.g. Minor to Major/Jack, Major/Jack to Queen) whenever two of them share a priority. Unspent budget carries forward as spare material for later piece generation.
 - When `piece_upgrade_preferences` is omitted, fundamental itemization defaults to a tied set -- New Pawn, Pawn to Minor, Minor to Major, Pawn to Major, and Major to Queen all share the top priority -- so a fresh fundamental seed graduates pieces out of the box without any manual configuration. New Pawn only checks whether it is individually enabled and never actually competes against the other four for the same weighted draw (it fills already-decided pawn board slots rather than graduating a `Chessmen` slot's tier), so sharing a priority with them is harmless either way. Legacy itemization's own per-`fairy_chess_pawn_upgrades`-mode defaults are unchanged from their original, strictly-ordered behavior.
 - Known fundamental-itemization limitation: recomputing with more `Material` only ever extends your existing roster upward, but recomputing with more `Chessmen` does not currently carry that same guarantee. Every generated slot shares one material budget, so adding more slots can dilute how far the whole roster reaches and may unexpectedly downgrade or remove previously-generated Queens/Amazons. Not yet fixed.
 - Each active `Castler` locks one `Chessmen` plus 500 material into a rook-like/major-family castling piece, capped by `castling_location_count`; castling locations are still emitted by moves. The local, non-persisted "Ignore Castlers Received" checkbox is default off and only enabled for fundamental seeds.
```

Add this bullet (after "`Asymmetric Trades: Jacks`...", before "`fairy_chess_pawns`..."):

```
 - `Asymmetric Trades: Amazons` adds `Progressive Amazon`. Amazons are roughly 13-material upgrades such as Amazon and Herald. They upgrade queen-family pieces when available; otherwise, unused upgrade material feeds the same spare-material budget used by later piece generation. They do not add extra board slots or castling privileges.
```

Change the `fairy_chess_pawns` / `fairy_chess_pawn_upgrades` bullet (currently ends "...Max prefers upgrades when the budget can still reach your earned pawn count.") to:

```
 - `fairy_chess_pawns` includes standard pawns, Berolina, Checkers, or one of the mixed pools. `fairy_chess_pawn_upgrades` controls stronger pawn upgrades drawn from the pawn budget: Off keeps the legacy post-selection upgrade pass, Pool adds upgrades as random pool options while guarding pawn count, Max prefers upgrades when the budget can still reach your earned pawn count, and SuperMax is an inline pawn-upgrade/Sergeant option, not a separate `pawn_count_guarantee` setting. SuperMax behaves like Max, but when the board-location pawn requirement is lower than your collected `Progressive Pawn` count, it keeps the full collected pawn material budget and can convert excess pawn material into Sergeant/Odin Pawn upgrades. The board-location guarantee counts 16 chessmen including the base King on standard boards (15 non-base-king slots), or 20 including the base King on Super-Sized boards (19 non-base-king slots); known Consuls, jacks, majors, and minors reduce how many pawn slots still need to be guaranteed.
```

### Army PieceTypes reference

Append this sentence to the intro paragraph (after "...not a Camel or Petal army assignment."):

```
Amazon-family upgrades are Amazon and Herald.
```

Change the Eurasian army's Queen line from `**Queen:** Herald/Queennon.` to `**Queen:** Queennon.` (Herald moves out to the new Amazon family below, since it is no longer Eurasian-specific).

Add this subsection (after "Petal", before "Movement definitions"):

```
#### Amazon family

 - **Amazon:** Amazon/Herald.
```

Add this row to the "Orthodox baseline and atoms" table (after the `Queen` row):

```
| Amazon | Queen slides + Knight leaps. |
```

## Runtime board geometry and reserves

Add this subsection after the ChecksMate Client option reference notes:

```
### Board geometry, reserves, and contract v2

Current-contract ChecksMate worlds can unlock five board profiles: 8x8, 10x8, 10x10, 12x10, and 12x12. `Board Files` and `Board Ranks` advance independently in two-square steps, while the client only offers the valid profile pairs published by the world. The board selector defaults to the largest unlocked profile on connection, can select a smaller unlocked board as a handicap, and is locked while a match is active. Legacy worlds retain their original 8x8 / Super-Sized 10x8 behavior.

Your generated roster is owned independently of the selected board. The client projects that roster onto the selected profile using stable source roles and deterministic material-first priority; pieces that do not fit become reserves rather than disappearing or entering pockets. The connection panel previews the active count, reserve count, missing expected material, and unspent Pawn Forwardness for the highlighted board. Reserves cannot enter during a match, and reserve-only promotion types are unavailable until a larger board activates the corresponding pieces.

Expanded profiles use geometry-derived player formation bands, Pawn Forwardness, CPU layouts, promotions, castling, capture thresholds, and checkmate locations. Twelve-file armies have explicit inner and outer attendant bands. Current-contract Progressive and Ordered Progressive goals advance through 10x8, 10x10, 12x10, and 12x12; Super starts beyond 8x8, while Single remains an 8x8 goal.

Fundamental's exact shared-wave roster can still redistribute tiers when additional `Chessmen` arrive. ChessV uses that exact result for board setup and reserve diagnostics, but the Archipelago world uses a conservative monotonic, stage-local strength envelope for reachability so collecting an item cannot revoke an already-reachable location. A later geometry unlock permanently certifies earlier-stage strength checks.

Contract v2 requires ChecksMate client 0.4.0 or newer. The client validates the canonical contract hash and finite geometry list before enabling these profiles. A client and `checksmate.apworld` from the same release remain the supported pairing.

APMW saved-game reconstruction remains unsupported. Finish an active match before changing board profile, and reconnect to rebuild unlock state after disconnecting.
```
