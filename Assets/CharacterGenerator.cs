using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class CharacterGenerator : MonoBehaviour
{
    public PromptsController promptsController;
    public GeneratedCharacters generatedCharacters;
    public Button generateButton;
    public GameObject generatingText;
    public string mainSceneName = "start";
    public string modelName;
    public float temperature = 1f;

    void Start()
    {
        generateButton.GetComponentInChildren<TextMeshProUGUI>().text = "Generate Characters";
        generateButton.onClick.AddListener(OnGeneratePressed);
    }

    void OnGeneratePressed()
    {
        generateButton.gameObject.SetActive(false);
        generatingText.SetActive(true);
        StartCoroutine(GenerateAllCharacters());
    }

    IEnumerator GenerateAllCharacters()
    {
        yield return StartCoroutine(GenerateCharacter(1));
        yield return StartCoroutine(GenerateCharacter(2));

        generatingText.SetActive(false);
        generateButton.onClick.RemoveAllListeners();
        generateButton.onClick.AddListener(() => SceneManager.LoadScene(mainSceneName));
        generateButton.GetComponentInChildren<TextMeshProUGUI>().text = "Start";
        generateButton.gameObject.SetActive(true);
    }

    IEnumerator GenerateCharacter(int charNo)
    {
        var p = promptsController;
        string setupSystemPrompt = p.mainGameSystemPrompt + " " + p.characterSetupSystemPrompt;
        string generatedName = "";

        yield return StartCoroutine(APIRequestHandler.SendOpenAIRequest(
            setupSystemPrompt, p.characterNameUserPrompt, charNo, temperature, modelName, 0,
            (response) => { generatedName = response.choices[0].message.content.Trim(); },
            this));

        if (string.IsNullOrEmpty(generatedName))
        {
            Debug.LogError($"Character {charNo} name generation failed.");
            yield break;
        }

        Debug.Log($"Character {charNo} name: {generatedName}");

        string generatedDescription = "";

        yield return StartCoroutine(APIRequestHandler.SendOpenAIRequest(
            setupSystemPrompt, p.characterSetupUserPrompt + " " + generatedName, charNo, temperature, modelName, 0,
            (response) => { generatedDescription = response.choices[0].message.content; },
            this));

        if (string.IsNullOrEmpty(generatedDescription))
        {
            Debug.LogError($"Character {charNo} description generation failed.");
            yield break;
        }

        Debug.Log($"Character {charNo} description generated.");

        while (generatedCharacters.characters.Count <= charNo)
            generatedCharacters.characters.Add(new CharacterEntry());

        generatedCharacters.characters[charNo] = new CharacterEntry
        {
            name = generatedName,
            description = generatedDescription,
            systemPrompt = p.characterDialogueSystemPrompt + "\n " + generatedDescription + "\n " + p.characterDialogueSystemPromptEnding
        };
    }
}
