using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class RestController : MonoBehaviour
{
    [SerializeField] GameObject vnCanvas;
    [SerializeField] TextAsset restJson;
    VNDialogueUI vnUI;

    RestContent content;
    RestChoice selectedChoice;
    int lineIndex;

    void Start()
    {
        if (vnCanvas == null)
        {
            Debug.LogError("RestScene needs a VN canvas reference.");
            return;
        }

        vnUI = vnCanvas.GetComponent<VNDialogueUI>();
        if (vnUI == null)
        {
            Debug.LogError("RestScene's VN canvas needs VNDialogueUI.");
            return;
        }

        vnUI.speakerText.text = "Rest";

        if (!TryLoadContent())
        {
            ShowDialogue("Rest data is missing or incomplete. Click to return.", GoBack);
            return;
        }

        lineIndex = 0;
        ShowIntroLine();
    }

    bool TryLoadContent()
    {
        if (restJson == null) return false;
        try { content = JsonUtility.FromJson<RestContent>(restJson.text); }
        catch (ArgumentException error)
        {
            Debug.LogError("Rest data could not be read: " + error.Message);
            return false;
        }

        if (content == null || content.introLines == null || content.introLines.Length == 0 ||
            content.choices == null || content.choices.Length == 0)
            return false;

        foreach (RestChoice choice in content.choices)
        {
            if (choice == null || string.IsNullOrWhiteSpace(choice.label) ||
                choice.resultLines == null || choice.resultLines.Length == 0)
                return false;
        }
        return true;
    }

    void ShowIntroLine()
    {
        ShowDialogue(content.introLines[lineIndex], () =>
        {
            lineIndex++;
            if (lineIndex < content.introLines.Length) ShowIntroLine();
            else ShowChoices();
        });
    }

    void ShowChoices()
    {
        vnUI.BeginChoices("How will you rest?");
        foreach (RestChoice choice in content.choices)
        {
            RestChoice selected = choice;
            AddButton(selected.label, () => SelectChoice(selected));
        }
    }

    void SelectChoice(RestChoice choice)
    {
        // Party HP is not implemented yet, so no recovery or gold cost is applied here.
        selectedChoice = choice;
        lineIndex = 0;
        vnUI.speakerText.text = choice.label;
        ShowResultLine();
    }

    void ShowResultLine()
    {
        bool lastLine = lineIndex == selectedChoice.resultLines.Length - 1;
        ShowDialogue(selectedChoice.resultLines[lineIndex], () =>
        {
            if (lastLine) GoBack();
            else
            {
                lineIndex++;
                ShowResultLine();
            }
        });
    }

    void ShowDialogue(string text, UnityAction next)
    {
        vnUI.ShowDialogue(text, next);
    }

    void AddButton(string label, UnityAction action)
    {
        vnUI.AddChoice(label, action);
    }

    void GoBack() => SceneManager.LoadScene("ExplorationScene");

    [Serializable]
    class RestContent
    {
        public string[] introLines;
        public RestChoice[] choices;
    }

    [Serializable]
    class RestChoice
    {
        public string id;
        public string label;
        public string recoveryType;
        public float recoveryValue;
        public int costGold;
        public string[] resultLines;
    }
}
