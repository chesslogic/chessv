# Treat current ChecksMate targets as hand-authored compatibility data

Type: research  
Status: resolved

## Question

Are current ChecksMate Location identities and material thresholds generated from CPU piece values?

## Answer

No. `locations.py` is a static `CMLocationData` table with hand-authored `material_expectations_grand` and `chessmen_expectations` series. Pawn capture identity is currently a literal file name (`Capture Pawn A` through `Capture Pawn L`), while back-rank pieces use hand-maintained file-to-role names such as Queen's Bishop or King's Attendant.

Current checkmate targets are 6020 for 10x10 and 8020 for both 12x10 and 12x12. `rules.py` scales super-sized material from the 12x12 target, so the shared 12-file value affects more than the Location row itself.

ChessV's `CaptureLookup` and `LocationHandler` reproduce these names from original file/rank. Their tests pin the file mappings. A migration must therefore coordinate stable Location identity, name lookup, threshold data, counts, and scaling behavior.

Scope distinction: the 12x12 target and scaling behavior above are current-source compatibility facts, not a calibration target or supported geometry. Formal support ends at 12x10; later decisions must determine whether 12x10 keeps or separates from the legacy shared 8020 value and how 12x12 is made explicitly unsupported.

## Provenance

- `APMW.Client\CaptureLookup.cs`
- `APMW.Client\LocationHandler.cs`
- `APMW.Test\LocationHandlerUnitTests.cs`
- `C:\GitHub\rft50-checksmate\worlds\checksmate\locations.py`
- `C:\GitHub\rft50-checksmate\worlds\checksmate\rules.py`
