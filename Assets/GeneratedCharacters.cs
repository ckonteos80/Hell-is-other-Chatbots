using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CharacterEntry
{
    public string name;
    public string description;
    public string systemPrompt;
}

public class GeneratedCharacters : MonoBehaviour
{
    public static GeneratedCharacters Instance { get; private set; }

    public List<CharacterEntry> characters = new();

    public bool useHuggingFaceProvider;

    public ModelNamesController modelNames;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
