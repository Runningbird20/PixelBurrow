# You Are the Wind

A peaceful exploration prototype where the player is the wind, restoring life by carrying nature items through a quiet world.

## Current Gameplay Loop

1. Drift through the map as wind (WASD/Arrow keys or mouse steering).
2. Pick up nearby nature items (seeds by default, with support for petals/clouds).
3. Drop items with `Space` into restoration zones.
4. Watch areas regrow with plants/animals and world ambience evolve.

## Scene Setup (Quick)

1. Add `WindController` to your wind player object.
2. Create an empty child transform and assign it as `carryPoint`.
3. Add `Seed` to item prefabs and choose `itemType` (`Seed`, `Petal`, or `Cloud`).
4. Add `RestoreZone` to trigger volumes and set:
   - `requiredItemType`
   - `itemsRequired`
   - optional barren/restored visuals, plant prefabs, animal prefabs.
5. (Optional) Add `WindWorldDirector` to a manager object and connect music/light for progression mood.
6. (Optional) Add `WindHUD` to any active object for minimal on-screen controls text.

## Visual Style Notes

- Use soft pastel materials and fog for a calming mood.
- Attach a subtle particle system to `windTrail` on `WindController`.
- Keep UI sparse and ambient audio low/organic.
