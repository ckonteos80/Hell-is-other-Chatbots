using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Text;
using TMPro;

public class CharacterGenerator : MonoBehaviour
{
    public PromptsController promptsController;
    public GeneratedCharacters generatedCharacters;
    public Button generateButton;
    public GameObject generatingText;
    public string mainSceneName = "start";
    public float temperature = 1f;

    private struct ParsedCharData
    {
        public string occupation;
        public string causeOfDeath;
        public string proseBody;
        public string reasonTrue;
        public string reasonSelfTold;
        public string refuseToAdmit;
        public string personalityTrait;
        public string want;
    }

    private ParsedCharData _char1Data;

    void Start()
    {
        APIRequestHandler.useHuggingFaceProvider = generatedCharacters.useHuggingFaceProvider;
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
        string gender = charNo == 1 ? "male" : "female";
        int randomAge = Random.Range(25, 65);
        string label = $"Char {charNo}";

        // ── Step 1: Name ──────────────────────────────────────────────────────
        string generatedName = "";
        string nameUserPrompt = p.characterNameUserPrompt + " " + string.Format(p.characterNameGenderSuffix, gender);

        Debug.Log($"[{label} / Name] PROMPT: {nameUserPrompt}");
        yield return StartCoroutine(APIRequestHandler.SendOpenAIRequest(
            p.characterSetupSystemPrompt, nameUserPrompt, charNo, temperature,
            generatedCharacters.modelNames.modelGeneration, 0,
            (response) => { generatedName = response.choices[0].message.content.Trim(); },
            this));

        Debug.Log($"[{label} / Name] RESPONSE: {generatedName}");

        if (string.IsNullOrEmpty(generatedName))
        {
            Debug.LogError($"[{label}] Name generation failed. Aborting character.");
            yield break;
        }

        // ── Step 2: Life ──────────────────────────────────────────────────────
        string lifeUserPrompt = p.characterLifeUserPrompt
            .Replace("{NAME}", generatedName)
            .Replace("{AGE}", randomAge.ToString())
            .Replace("{GENDER}", gender);

        if (charNo == 2)
        {
            lifeUserPrompt += "\n\n" + p.characterLifeAntiDuplicationBlock
                .Replace("{OTHER_OCCUPATION}", _char1Data.occupation)
                .Replace("{OTHER_CAUSE_OF_DEATH}", _char1Data.causeOfDeath);
        }

        string lifeResponse = "";
        for (int attempt = 0; attempt < 2; attempt++)
        {
            string capturedResponse = "";
            Debug.Log($"[{label} / Life] PROMPT (attempt {attempt + 1}):\n{lifeUserPrompt}");
            yield return StartCoroutine(APIRequestHandler.SendOpenAIRequest(
                p.characterSetupSystemPrompt, lifeUserPrompt, charNo, temperature,
                generatedCharacters.modelNames.modelGeneration, 0,
                (response) => { capturedResponse = response.choices[0].message.content; },
                this));
            Debug.Log($"[{label} / Life] RESPONSE:\n{capturedResponse}");

            string occ = ExtractField(capturedResponse, "Occupation");
            string cod = ExtractField(capturedResponse, "Cause of Death");
            string prose = ExtractField(capturedResponse, "Life");

            if (!string.IsNullOrWhiteSpace(occ) && !string.IsNullOrWhiteSpace(cod) && !string.IsNullOrWhiteSpace(prose))
            {
                lifeResponse = capturedResponse;
                break;
            }
            if (attempt == 1)
                Debug.LogError($"[{label}] Life call failed to parse after 2 attempts. Raw:\n{capturedResponse}");
        }

        string occupation = ExtractField(lifeResponse, "Occupation") ?? "[generation failed]";
        string causeOfDeath = ExtractField(lifeResponse, "Cause of Death") ?? "[generation failed]";
        string proseBody = ExtractField(lifeResponse, "Life") ?? "[generation failed]";

        // ── Step 3: Sin ───────────────────────────────────────────────────────
        string sinUserPrompt = p.characterSinUserPrompt
            .Replace("{NAME}", generatedName)
            .Replace("{AGE}", randomAge.ToString())
            .Replace("{GENDER}", gender)
            .Replace("{OCCUPATION}", occupation)
            .Replace("{CAUSE_OF_DEATH}", causeOfDeath)
            .Replace("{PROSE_BODY}", proseBody);

        if (charNo == 2)
        {
            sinUserPrompt += "\n\n" + p.characterSinAntiDuplicationBlock
                .Replace("{OTHER_REASON_TRUE}", _char1Data.reasonTrue);
        }

        string sinResponse = "";
        for (int attempt = 0; attempt < 2; attempt++)
        {
            string capturedResponse = "";
            Debug.Log($"[{label} / Sin] PROMPT (attempt {attempt + 1}):\n{sinUserPrompt}");
            yield return StartCoroutine(APIRequestHandler.SendOpenAIRequest(
                p.characterSetupSystemPrompt, sinUserPrompt, charNo, temperature,
                generatedCharacters.modelNames.modelGeneration, 0,
                (response) => { capturedResponse = response.choices[0].message.content; },
                this));
            Debug.Log($"[{label} / Sin] RESPONSE:\n{capturedResponse}");

            string rt = ExtractField(capturedResponse, "Reason for Damnation — True");
            string rst = ExtractField(capturedResponse, "Reason for Damnation — Self-told");
            string rta = ExtractField(capturedResponse, "What you refuse to admit about yourself");

            if (!string.IsNullOrWhiteSpace(rt) && !string.IsNullOrWhiteSpace(rst) && !string.IsNullOrWhiteSpace(rta))
            {
                sinResponse = capturedResponse;
                break;
            }
            if (attempt == 1)
                Debug.LogError($"[{label}] Sin call failed to parse after 2 attempts. Raw:\n{capturedResponse}");
        }

        string reasonTrue = ExtractField(sinResponse, "Reason for Damnation — True") ?? "[generation failed]";
        string reasonSelfTold = ExtractField(sinResponse, "Reason for Damnation — Self-told") ?? "[generation failed]";
        string refuseToAdmit = ExtractField(sinResponse, "What you refuse to admit about yourself") ?? "[generation failed]";

        // ── Step 4: Stance ────────────────────────────────────────────────────
        string stanceUserPrompt = p.characterStanceUserPrompt
            .Replace("{NAME}", generatedName)
            .Replace("{AGE}", randomAge.ToString())
            .Replace("{GENDER}", gender)
            .Replace("{OCCUPATION}", occupation)
            .Replace("{CAUSE_OF_DEATH}", causeOfDeath)
            .Replace("{REASON_TRUE}", reasonTrue)
            .Replace("{REASON_SELF_TOLD}", reasonSelfTold)
            .Replace("{REFUSE_TO_ADMIT}", refuseToAdmit)
            .Replace("{PROSE_BODY}", proseBody);

        if (charNo == 2)
        {
            stanceUserPrompt += "\n\n" + p.characterStanceAntiDuplicationBlock
                .Replace("{OTHER_WANT}", _char1Data.want);
        }

        string stanceResponse = "";
        for (int attempt = 0; attempt < 2; attempt++)
        {
            string capturedResponse = "";
            Debug.Log($"[{label} / Stance] PROMPT (attempt {attempt + 1}):\n{stanceUserPrompt}");
            yield return StartCoroutine(APIRequestHandler.SendOpenAIRequest(
                p.characterSetupSystemPrompt, stanceUserPrompt, charNo, temperature,
                generatedCharacters.modelNames.modelGeneration, 0,
                (response) => { capturedResponse = response.choices[0].message.content; },
                this));
            Debug.Log($"[{label} / Stance] RESPONSE:\n{capturedResponse}");

            string pt = ExtractField(capturedResponse, "Defining personality trait");
            string want = ExtractField(capturedResponse, "What you want from the others in the room");

            if (!string.IsNullOrWhiteSpace(pt) && !string.IsNullOrWhiteSpace(want))
            {
                stanceResponse = capturedResponse;
                break;
            }
            if (attempt == 1)
                Debug.LogError($"[{label}] Stance call failed to parse after 2 attempts. Raw:\n{capturedResponse}");
        }

        string personalityTrait = ExtractField(stanceResponse, "Defining personality trait") ?? "[generation failed]";
        string want2 = ExtractField(stanceResponse, "What you want from the others in the room") ?? "[generation failed]";

        // ── Step 5: Store Character 1's parsed data for anti-duplication ──────
        if (charNo == 1)
        {
            _char1Data = new ParsedCharData
            {
                occupation = occupation,
                causeOfDeath = causeOfDeath,
                proseBody = proseBody,
                reasonTrue = reasonTrue,
                reasonSelfTold = reasonSelfTold,
                refuseToAdmit = refuseToAdmit,
                personalityTrait = personalityTrait,
                want = want2
            };
        }

        // ── Step 6: Assemble bio ──────────────────────────────────────────────
        var sb = new StringBuilder();
        sb.AppendLine("**Occupation**");
        sb.AppendLine(occupation);
        sb.AppendLine();
        sb.AppendLine("**Cause of Death**");
        sb.AppendLine(causeOfDeath);
        sb.AppendLine();
        sb.AppendLine("**Reason for Damnation — True**");
        sb.AppendLine(reasonTrue);
        sb.AppendLine();
        sb.AppendLine("**Reason for Damnation — Self-told**");
        sb.AppendLine(reasonSelfTold);
        sb.AppendLine();
        sb.AppendLine("**Defining personality trait**");
        sb.AppendLine(personalityTrait);
        sb.AppendLine();
        sb.AppendLine("**What you refuse to admit about yourself**");
        sb.AppendLine(refuseToAdmit);
        sb.AppendLine();
        sb.AppendLine("**What you want from the others in the room**");
        sb.AppendLine(want2);
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.Append(proseBody);
        string generatedDescription = sb.ToString();

        Debug.Log($"[{label}] Bio assembled:\n{generatedDescription}");

        // ── Step 7: Store in GeneratedCharacters ──────────────────────────────
        while (generatedCharacters.characters.Count <= charNo)
            generatedCharacters.characters.Add(new CharacterEntry());

        generatedCharacters.characters[charNo] = new CharacterEntry
        {
            name = generatedName,
            description = generatedDescription,
            systemPrompt = p.characterDialogueSystemPrompt + "\n " + generatedDescription + "\n " + p.characterDialogueSystemPromptEnding,
            gender = gender,
            age = randomAge
        };

        Debug.Log($"[{label}] Generation complete. Name: {generatedName}");
    }

    private string ExtractField(string response, string fieldName)
    {
        if (string.IsNullOrEmpty(response)) return null;
        string marker = "**" + fieldName + "**";
        int start = response.IndexOf(marker);
        if (start < 0) return null;
        start += marker.Length;
        while (start < response.Length && (response[start] == '\n' || response[start] == '\r'))
            start++;
        int end = response.IndexOf("**", start);
        string content = end < 0
            ? response[start..]
            : response[start..end];
        return content.Trim();
    }
}
