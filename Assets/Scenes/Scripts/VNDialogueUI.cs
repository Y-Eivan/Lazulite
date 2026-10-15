using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class VNDialogueUI : MonoBehaviour
{
    public Button dialogueClickArea;
    public TMP_Text dialogueText;
    public TMP_Text speakerText;
    public GameObject choicePanel;
    public RectTransform choiceList;
    public Button choiceTemplate;
    public GameObject topBar;

    public void ShowDialogue(string text, UnityAction next)
    {
        choicePanel.SetActive(false);
        ClearChoices();
        dialogueText.text = text;
        topBar.SetActive(true);
        dialogueClickArea.onClick.RemoveAllListeners();
        dialogueClickArea.onClick.AddListener(next);
        dialogueClickArea.interactable = true;
    }

    public void BeginChoices(string prompt)
    {
        dialogueClickArea.interactable = false;
        ClearChoices();
        dialogueText.text = prompt;
        topBar.SetActive(false);
        choicePanel.SetActive(true);
    }

    public void AddChoice(string label, UnityAction action)
    {
        Button button = Instantiate(choiceTemplate, choiceList);
        button.gameObject.SetActive(true);
        button.GetComponentInChildren<TMP_Text>(true).text = label;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    void ClearChoices()
    {
        foreach (Transform child in choiceList)
        {
            if (child == choiceTemplate.transform) continue;
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
    }
}

