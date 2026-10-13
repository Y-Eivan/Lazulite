using TMPro;
using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ExplorationUI : MonoBehaviour
{
    public GameObject dialoguePanel;
    public GameObject choicePanel;
    public Button dialogueClickArea;
    public TMP_Text dialogueText;
    [SerializeField] TextAsset encountersJson;

    public GameObject portrait;
    public Button[] choiceButtons = new Button[3];

    [TextArea(2, 4)]
    public string[] lines = {
        "The path ahead is quiet.",
        "Where should we go next?"
    };

    int lineIndex = 0;

    void Start()
    {
        dialoguePanel.SetActive(true);
        choicePanel.SetActive(false);

        if (lines.Length > 0)
        {
            dialogueText.text = lines[0];
        }
        else
        {
            ShowChoices();
        }

        dialogueClickArea.onClick.AddListener(NextLine);
    }

    void NextLine()
    {
        lineIndex = lineIndex + 1;

        if (lineIndex < lines.Length)
        {
            dialogueText.text = lines[lineIndex];
        }
        else
        {
            ShowChoices();
        }
    }

    void ShowChoices()
    {
        // A bag of choices we can draw from.
        List<string> available = new List<string>
        {
            "Fight", "Explore", "Explore", "Shop", "Rest"
        };

        int numberOfChoices = Random.Range(2, 4);

        EncounterFile file = JsonUtility.FromJson<EncounterFile>(encountersJson.text);
        string exploreLabel = "Explore";

        HashSet<string> shown = new();

        foreach (EncounterData encounter in file.encounters)
        {
            if (shown.Count == 3) break;

            if (shown.Add(encounter.type))
                exploreLabel += "\nPossible: " + encounter.type;
        }
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i >= numberOfChoices)
            {
                choiceButtons[i].gameObject.SetActive(false);
                continue;
            }

            choiceButtons[i].gameObject.SetActive(true);

            int randomIndex = Random.Range(0, available.Count);
            string pickedChoice = available[randomIndex];
            available.RemoveAt(randomIndex); // Prevents duplicates.

            choiceButtons[i].GetComponentInChildren<TMP_Text>().text =
                pickedChoice == "Explore" ? exploreLabel : pickedChoice;

            choiceButtons[i].onClick.AddListener(() => SelectChoice(pickedChoice));
        }

        portrait.SetActive(false);
        dialogueClickArea.interactable = false;
        choicePanel.SetActive(true);
    }

    void SelectChoice(string choice)
    {
        if (choice == "Explore")
        {
            SceneManager.LoadScene("ExploreScene");
        }
        else if (choice == "Fight")
        {
            SceneManager.LoadScene("BattleScene");
        }
        else if (choice == "Rest")
        {
            SceneManager.LoadScene("RestScene");
        }
        else if (choice == "Fight")
        {
            SceneManager.LoadScene("ShopScene");
        }
    }
}
