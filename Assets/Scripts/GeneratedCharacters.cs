using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CharacterEntry
{
    public string name;
    public string description;
    public string systemPrompt;
    public string gender;
    public int age;
}

public class GeneratedCharacters : MonoBehaviour
{
    public static GeneratedCharacters Instance { get; private set; }

    public List<CharacterEntry> characters = new();

    public bool useHuggingFaceProvider;

    public ModelNamesController modelNames;

    [Header("Temperature")]
    public float generationTemperature = 1f;
    public float dialogueTemperature = 0.5f;
    public float addressingTemperature = 0.3f;
    public float infoTemperature = 0f;

    [Header("Max Tokens")]
    public int maxGenerationTokens = 300;
    public int maxDialogueTokens = 150;
    public int maxAddressingTokens = 10;
    public int maxInfoTokens = 50;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
