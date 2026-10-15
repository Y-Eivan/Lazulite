using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExploreController : MonoBehaviour
{
    [SerializeField] TextAsset encountersJson;
    [SerializeField] VNDialogueUI vnUI;
    [SerializeField] TMP_Text encounterText;

    [SerializeField] TMP_Text titleText;
    [SerializeField] RectTransform buttonArea;
    [SerializeField] Button choiceTemplate;
    [SerializeField] GameObject topBar;
    Button dialogueClickArea;
    GameObject choicePanel;
    EncounterData selected;
    EncounterResult currentResult;
    string rewardSummary;
    int lineIndex;

    void Start()
    {
        dialogueClickArea = encounterText != null ? encounterText.GetComponentInParent<Button>(true) : null;
        choicePanel = buttonArea != null && buttonArea.parent != null ? buttonArea.parent.gameObject : null;
        if (encounterText == null || titleText == null || buttonArea == null || choiceTemplate == null ||
            topBar == null || dialogueClickArea == null || choicePanel == null)
        {
            Debug.LogError("ExploreScene is missing a VN canvas reference or the DialoguePanel Button.");
            return;
        }

        choiceTemplate.gameObject.SetActive(false);

        if (!EncounterCatalog.TryLoad(encountersJson, out EncounterData[] encounters, out string error))
        {
            Debug.LogError(error);
            ShowError(error);
            return;
        }

        selected = ChooseWeighted(encounters);
        titleText.text = selected.displayName;
        lineIndex = 0;
        ShowIntroLine();
    }

    EncounterData ChooseWeighted(EncounterData[] encounters)
    {
        long totalWeight = 0;
        foreach (EncounterData encounter in encounters)
            totalWeight += encounter.weight;

        double roll = UnityEngine.Random.value * totalWeight;
        foreach (EncounterData encounter in encounters)
        {
            roll -= encounter.weight;
            if (roll < 0) return encounter;
        }
        return encounters[encounters.Length - 1];
    }

    void ShowIntroLine()
    {
        ShowDialogue(selected.introLines[lineIndex], () =>
        {
            lineIndex++;
            if (lineIndex < selected.introLines.Length) ShowIntroLine();
            else ShowChoices();
        });
    }

    void ShowChoices()
    {
        ShowChoiceButtons();
        encounterText.text = "What will you do?";
        foreach (EncounterChoice choice in selected.choices)
        {
            EncounterChoice currentChoice = choice;
            AddButton(currentChoice.label, () =>
            {
                if (currentChoice.kind == "puzzle") ShowPuzzle(currentChoice.puzzle);
                else if (currentChoice.kind == "chance")
                {
                    EncounterChance chance = currentChoice.chance;
                    bool succeeded = chance.successRate >= 1f ||
                        (chance.successRate > 0f && UnityEngine.Random.value < chance.successRate);
                    ShowResult(succeeded ? chance.successResult : chance.failureResult);
                }
                else ShowResult(currentChoice.result);
            });
        }
    }

    void ShowPuzzle(EncounterPuzzle puzzle)
    {
        ShowChoiceButtons();
        encounterText.text = puzzle.prompt;
        for (int i = 0; i < puzzle.options.Length; i++)
        {
            int optionIndex = i;
            AddButton(puzzle.options[i], () => ShowResult(
                optionIndex == puzzle.correctOptionIndex ? puzzle.successResult : puzzle.failureResult));
        }
    }

    void ShowResult(EncounterResult result)
    {
        currentResult = result;
        rewardSummary = ApplyResult(result);
        lineIndex = 0;
        ShowResultLine();
    }

    void ShowResultLine()
    {
        bool lastLine = lineIndex == currentResult.lines.Length - 1;
        string line = currentResult.lines[lineIndex];
        if (lastLine && !string.IsNullOrEmpty(rewardSummary))
            line += "\n\n" + rewardSummary;

        ShowDialogue(line, () =>
        {
            if (lastLine) SceneManager.LoadScene("ExplorationScene");
            else
            {
                lineIndex++;
                ShowResultLine();
            }
        });
    }

    string ApplyResult(EncounterResult result)
    {
        GameSession session = GameSession.Instance;
        if (session == null)
            session = new GameObject("GameSession").AddComponent<GameSession>();

        List<string> updates = new();
        int previousGold = session.gold;
        long requestedGold = (long)previousGold + result.goldDelta;
        session.gold = (int)Math.Max(0L, Math.Min(int.MaxValue, requestedGold));
        int actualGoldChange = session.gold - previousGold;
        if (actualGoldChange != 0)
            updates.Add(actualGoldChange > 0 ? $"Gold +{actualGoldChange}" : $"Gold {actualGoldChange}");

        if (session.itemIds == null) session.itemIds = new List<string>();
        foreach (string itemId in result.itemIds ?? Array.Empty<string>())
        {
            if (itemId.EndsWith("_TBD", StringComparison.OrdinalIgnoreCase))
            {
                updates.Add("Artifact reward pending item data.");
                Debug.LogWarning($"Encounter {selected.id} uses placeholder item ID {itemId}; no item was added.");
                continue;
            }

            session.itemIds.Add(itemId);
            updates.Add("Item received.");
        }
        return string.Join("\n", updates);
    }

    void ShowError(string error)
    {
        titleText.text = "Explore";
        ShowDialogue("Encounter data error:\n" + error,
            () => SceneManager.LoadScene("ExplorationScene"));
    }

    void ShowDialogue(string text, UnityAction next)
    {
        vnUI.ShowDialogue(text, next);
    }

    void ShowChoiceButtons()
    {
        vnUI.BeginChoices("");
    }

    void ClearButtons()
    {
        foreach (Transform child in buttonArea)
        {
            if (child == choiceTemplate.transform) continue;
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
    }

    void AddButton(string label, UnityAction action)
    {
        vnUI.AddChoice(label, action);
    }
}
