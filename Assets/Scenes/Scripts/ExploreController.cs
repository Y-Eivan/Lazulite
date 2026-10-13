using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ExploreController : MonoBehaviour
{
    [SerializeField] TextAsset encountersJson;
    [SerializeField] TMP_Text encounterText;

    void Start()
    {
        List<EncounterData> pool = new();

        EncounterFile file = JsonUtility.FromJson<EncounterFile>(encountersJson.text);

        foreach (EncounterData encounter in file.encounters)
        {
            for (int i = 0; i < encounter.weight; i++)
                pool.Add(encounter);
        }
        if (pool.Count == 0)
        {
            encounterText.text = "No available encounters.";
            return;
        }
        EncounterData chosen = pool[Random.Range(0, pool.Count)];
        encounterText.text = $"{chosen.displayName}\n{chosen.hint}";

        Debug.Log($"Loaded {file.encounters.Length} encounters");
        Debug.Log($"Gold in Explore: {GameSession.Instance.gold}");
    }
}