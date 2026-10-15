using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EncounterData
{
    public string id, type, displayName, hint;
    public int weight;
    public string[] introLines;
    public EncounterChoice[] choices;
}

[Serializable]
public class EncounterChoice
{
    public string id, label, kind;
    public EncounterResult result;
    public EncounterPuzzle puzzle;
    public EncounterChance chance;
}

[Serializable]
public class EncounterChance
{
    public float successRate;
    public EncounterResult successResult;
    public EncounterResult failureResult;
}

[Serializable]
public class EncounterPuzzle
{
    public string prompt;
    public string[] options;
    public int correctOptionIndex;
    public EncounterResult successResult;
    public EncounterResult failureResult;
}

[Serializable]
public class EncounterResult
{
    public string[] lines;
    public int goldDelta;
    public string[] itemIds;
}

[Serializable]
public class EncounterFile
{
    public EncounterData[] encounters;
}

public static class EncounterCatalog
{
    public static bool TryLoad(TextAsset source, out EncounterData[] encounters, out string error)
    {
        encounters = null;
        error = null;

        if (source == null)
        {
            error = "Assign encounters.json to the scene controller.";
            return false;
        }

        EncounterFile file;
        try
        {
            file = JsonUtility.FromJson<EncounterFile>(source.text);
        }
        catch (Exception exception)
        {
            error = $"encounters.json is not valid JSON: {exception.Message}";
            return false;
        }

        if (file?.encounters == null || file.encounters.Length == 0)
        {
            error = "encounters.json needs at least one encounter.";
            return false;
        }

        HashSet<string> encounterIds = new();
        for (int i = 0; i < file.encounters.Length; i++)
        {
            EncounterData encounter = file.encounters[i];
            string location = $"encounters[{i}]";

            //Checks if encounter or encounter parameters are missing
            if (encounter == null || Missing(encounter.id) || Missing(encounter.type) ||
                Missing(encounter.displayName) || Missing(encounter.hint))
            {
                error = $"{location} is missing a(n) id, type, displayName, and hint";
                return false;
            }
            //Checks for duplicated IDs
            if (!encounterIds.Add(encounter.id))
            {
                error = $"Duplicate encounter id: {encounter.id}";
                return false;
            }
            //Verifies if encounter has a weight of 1 or more than 1
            if (encounter.weight < 1)
            {
                error = $"{encounter.id}: weight must be at least 1";
                return false;
            }

            //Checks if there's intro lines for the choices
            //And if choices are already written and is less than 3 and or more than 1
            if (!HasLines(encounter.introLines) || encounter.choices == null ||
                encounter.choices.Length == 0 || encounter.choices.Length > 3)
            {
                error = $"{encounter.id}: add introLines and 1-3 choices";
                return false;
            }

            HashSet<string> choiceIds = new();
            for (int j = 0; j < encounter.choices.Length; j++)
            {
                EncounterChoice choice = encounter.choices[j];
                string choiceLocation = $"{encounter.id}.choices[{j}]";

                //Checks if choice or choice parameters are missing
                if (choice == null || Missing(choice.id) || Missing(choice.label) || !choiceIds.Add(choice.id))
                {
                    error = $"{choiceLocation} needs a unique id and a label";
                    return false;
                }
                if (choice.kind == "result")
                {
                    if (!ValidResult(choice.result))
                    {
                        error = $"{choiceLocation}.result needs lines and valid item IDs";
                        return false;
                    }
                    continue;
                }

                if (choice.kind == "chance")
                {
                    EncounterChance chance = choice.chance;
                    if (chance == null || float.IsNaN(chance.successRate) ||
                        chance.successRate < 0f || chance.successRate > 1f ||
                        !ValidResult(chance.successResult) || !ValidResult(chance.failureResult))
                    {
                        error = $"{choiceLocation}.chance needs successRate from 0 - 1 and complete success/failure results";
                        return false;
                    }
                    continue;
                }

                if (choice.kind != "puzzle")
                {
                    error = $"{choiceLocation}.kind must be result, puzzle, or chance.";
                    return false;
                }

                EncounterPuzzle puzzle = choice.puzzle;
                if (puzzle == null)
                {
                    error = $"{choiceLocation}.puzzle is missing.";
                    return false;
                }
                if (Missing(puzzle.prompt) || puzzle.options == null || puzzle.options.Length < 2 ||
                    puzzle.options.Length > 3 ||
                    puzzle.correctOptionIndex < 0 || puzzle.correctOptionIndex >= puzzle.options.Length)
                {
                    error = $"{choiceLocation}.puzzle needs a prompt, two or three options, and a valid correctOptionIndex.";
                    return false;
                }
                foreach (string option in puzzle.options)
                {
                    if (Missing(option))
                    {
                        error = $"{choiceLocation}.puzzle has an empty option.";
                        return false;
                    }
                }
                if (!ValidResult(puzzle.successResult) || !ValidResult(puzzle.failureResult))
                {
                    error = $"{choiceLocation}.puzzle needs successResult and failureResult with lines.";
                    return false;
                }
            }
        }

        encounters = file.encounters;
        return true;
    }

    static bool Missing(string value) => string.IsNullOrWhiteSpace(value);

    static bool HasLines(string[] lines)
    {
        if (lines == null || lines.Length == 0) return false;
        foreach (string line in lines)
            if (Missing(line)) return false;
        return true;
    }

    static bool ValidResult(EncounterResult result)
    {
        if (result == null || !HasLines(result.lines)) return false;
        if (result.itemIds != null)
            foreach (string itemId in result.itemIds)
                if (Missing(itemId)) return false;
        return true;
    }
}
