using System;

[Serializable]
public class EncounterData
{
    public string id, type, displayName, hint;
    public int weight;
}

[Serializable]
public class EncounterFile
{
    public EncounterData[] encounters;
}