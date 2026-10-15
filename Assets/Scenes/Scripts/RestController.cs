using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RestController : MonoBehaviour
{
    [SerializeField] GameObject vnCanvas;
    [SerializeField] TextAsset restJson;
    VNDialogueUI vnUI;

    TMP_Text titleText;
    TMP_Text dialogueText;
    Button dialogueClickArea;
    RectTransform buttonArea;
    Button choiceTemplate;
    GameObject choicePanel;
    GameObject topBar;
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

        Transform root = vnCanvas.transform;
        Transform dialoguePanel = root.Find("DialoguePanel");
        Transform choiceList = root.Find("ChoicePanel/ChoiceList");
        Transform dialogueLabel = root.Find("DialoguePanel/DialogueBox/DialogueText");
        Transform speakerLabel = root.Find("DialoguePanel/DialogueBox/SpeakerTag/SpeakerText");
        Transform template = root.Find("ChoicePanel/ChoiceList/Choice1");
        Transform topBarTransform = root.Find("TopBar");
        if (dialoguePanel == null || choiceList == null || dialogueLabel == null ||
            speakerLabel == null || template == null || topBarTransform == null)
        {
            Debug.LogError("RestScene's VN canvas is missing a dialogue or choice object.");
            return;
        }

        dialogueClickArea = dialoguePanel.GetComponent<Button>();
        dialogueText = dialogueLabel.GetComponent<TMP_Text>();
        titleText = speakerLabel.GetComponent<TMP_Text>();
        buttonArea = choiceList.GetComponent<RectTransform>();
        choiceTemplate = template.GetComponent<Button>();
        choicePanel = choiceList.parent.gameObject;
        topBar = topBarTransform.gameObject;
        if (dialogueClickArea == null || dialogueText == null || titleText == null ||
            buttonArea == null || choiceTemplate == null)
        {
            Debug.LogError("RestScene's VN canvas is missing a required UI component.");
            return;
        }

        Transform portrait = root.Find("DialoguePanel/Portrait");
        if (portrait != null) portrait.gameObject.SetActive(false);
        choiceTemplate.gameObject.SetActive(false);
        titleText.text = "Rest";

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
        titleText.text = choice.label;
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
