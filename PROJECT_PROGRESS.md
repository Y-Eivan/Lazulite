# Lazulite progress

Updated: 8 October 2026

## Current goal

Finish Explore as a visual novel event. The flow is dialogue, choices in the area used for the portrait in `ExplorationScene`, an optional puzzle, then a result. The Explore choice should still hint at possible encounter types before entering the scene. Shop, Rest, and Battle come after Explore.

## What works now

- The Explore choice opens `ExploreScene`. This was tested in Unity.
- `GameSession` survives the move between scenes. This was tested in Unity. It currently holds gold, character IDs, and item IDs.
- Both exploration scenes have a reference to `Assets/Data/encounters.json`.
- `ExplorationUI` builds an Explore button label from up to three distinct encounter types in the JSON file. The button layout still needs testing with real data.
- `ExploreController` reads the JSON file and uses `weight` to choose an encounter. This still needs a Unity test with multiple entries.
- The JSON file is empty so the friend adding encounters has a clean starting point.

## Encounter data

Each entry in `Assets/Data/encounters.json` currently has an `id`, `type`, `displayName`, `hint`, and `weight`. For example:

```json
{
  "id": "gold_pouch",
  "type": "Gold",
  "displayName": "Gold Pouch",
  "hint": "You might find gold.",
  "weight": 1
}
```

IDs should be unique. The weighted selection code adds an encounter to the draw once for each point of `weight`. It needs a test with multiple entries. The current fields are enough to pick and preview an encounter, but they do not yet describe dialogue, choices, puzzles, or results. Agree on that structure before the friend fills the final data.

## Work left for Explore

1. Define one complete example encounter with dialogue, choices, an optional puzzle, and a result for each choice.
2. Reuse the `ExplorationScene` dialogue layout in `ExploreScene`. Put the choice buttons where the portrait normally appears, and add a hidden puzzle area. The result can use the dialogue area.
3. Extend the JSON data and C# classes to hold dialogue, choices, puzzle IDs, results, and effects.
4. Make `ExploreController` move through dialogue, choices, optional puzzle, and result. Apply rewards or costs when the result is reached.
5. Test the flow with the example encounter. Test weighted selection and check that the preview text fits the button.
6. Have the friend add real encounter data after the format is stable.
7. Test the full loop and confirm that player data remains correct after returning to `ExplorationScene`.

The Console test already confirmed that `ExploreScene` can read player data from `GameSession`. A resource display is optional UI work.

## Other scenes

- Later, overhaul `ExplorationScene` as the main hub. It should show the player's inventory or backpack, characters, and a stage counter. Keep this separate from the current `ExploreScene` event work.
- `RestScene` and `BattleScene` are in the scene list, but their gameplay is separate work.
- `ShopScene` has not been created. The Shop branch in `ExplorationUI` also checks for `Fight` instead of `Shop`, so it cannot open yet.
- The choice list contains two `Explore` entries. This can show two Explore buttons. Decide whether that is intended.
- Battle integration will need an agreement with the friend building it about which player data it reads and what result it writes back.
