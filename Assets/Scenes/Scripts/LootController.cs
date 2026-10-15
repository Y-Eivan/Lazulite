using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LootController : MonoBehaviour
{
    [SerializeField] VNDialogueUI vnUI;
    [SerializeField] TextAsset lootJson;

    LootContent content;
    LootChoice selectedChoice;
    int lineIndex;
    bool claimed;

    void Start()
    {
        if (vnUI == null)
        {
            Debug.LogError("LootScene needs a VNDialogueUI reference.");
            return;
        }

        vnUI.speakerText.text = "Loot";
        if (!TryLoadContent())
        {
            Debug.LogError("Loot data is missing or incomplete. Check loot.json.");
            vnUI.ShowDialogue("Loot data could not be loaded. Click to return.", GoBack);
            return;
        }

        ShowIntroLine();
    }

    bool TryLoadContent()
    {
        if (lootJson == null) return false;
        try { content = JsonUtility.FromJson<LootContent>(lootJson.text); }
        catch (ArgumentException) { return false; }

        if (content == null || !HasLines(content.introLines) ||
            string.IsNullOrWhiteSpace(content.prompt) || content.choices == null ||
            content.choices.Length < 1 || content.choices.Length > 3)
            return false;

        foreach (LootChoice choice in content.choices)
        {
            if (choice == null || string.IsNullOrWhiteSpace(choice.label) ||
                !HasLines(choice.resultLines) ||
                (choice.kind != "gold" && choice.kind != "placeholder") ||
                choice.goldAmount < 0 || (choice.kind == "placeholder" && choice.goldAmount != 0))
                return false;
        }
        return true;
    }

    static bool HasLines(string[] lines)
    {
        if (lines == null || lines.Length == 0) return false;
        foreach (string line in lines)
            if (string.IsNullOrWhiteSpace(line)) return false;
        return true;
    }

    void ShowIntroLine()
    {
        vnUI.ShowDialogue(content.introLines[lineIndex], () =>
        {
            lineIndex++;
            if (lineIndex < content.introLines.Length) ShowIntroLine();
            else ShowChoices();
        });
    }

    void ShowChoices()
    {
        vnUI.BeginChoices(content.prompt);
        foreach (LootChoice choice in content.choices)
        {
            LootChoice currentChoice = choice;
            vnUI.AddChoice(currentChoice.label, () => SelectChoice(currentChoice));
        }
    }

    void SelectChoice(LootChoice choice)
    {
        if (claimed) return;
        claimed = true;
        selectedChoice = choice;

        if (choice.kind == "gold")
        {
            GameSession session = GameSession.Instance;
            if (session == null)
                session = new GameObject("GameSession").AddComponent<GameSession>();

            session.gold = (int)Math.Max(0L,
                Math.Min(int.MaxValue, (long)session.gold + choice.goldAmount));
        }
        // Placeholder rewards display their result without adding unfinished items or heroes.

        vnUI.speakerText.text = choice.label;
        lineIndex = 0;
        ShowResultLine();
    }

    void ShowResultLine()
    {
        vnUI.ShowDialogue(selectedChoice.resultLines[lineIndex], () =>
        {
            lineIndex++;
            if (lineIndex < selectedChoice.resultLines.Length) ShowResultLine();
            else GoBack();
        });
    }

    void GoBack() => SceneManager.LoadScene("ExplorationScene");

    [Serializable]
    class LootContent
    {
        public string[] introLines;
        public string prompt;
        public LootChoice[] choices;
    }

    [Serializable]
    class LootChoice
    {
        public string label;
        public string kind;
        public int goldAmount;
        public string[] resultLines;
    }
}
