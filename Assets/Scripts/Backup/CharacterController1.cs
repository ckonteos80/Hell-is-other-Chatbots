// using UnityEngine;
// using TMPro;
// using UnityEngine.Networking;
// using System.Collections;
// using System;
// using System.Collections.Generic;

// [Serializable]
// public class OpenAIResponse
// {
//     public Choice[] choices;
// }

// [Serializable]
// public class Choice
// {
//     public Message message;
// }

// [Serializable]
// public class Message
// {
//     public string role;
//     public string content;
// }

// // This is the structured output we expect from ChatGPT.
// [Serializable]
// public class ChatResponse
// {
//     public string main_response;
//     public string additional_note;
// }

// // Wrapper class to hold the conversation for JSON serialization.
// [Serializable]
// public class ConversationWrapper
// {
//     public string model;
//     public Message[] messages;
// }

// public class CharacterController : MonoBehaviour
// {
//     Master myMaster;
//     public string apiKey;
//     private string apiUrl = "https://api.openai.com/v1/chat/completions";
//     private float retryDelay = 2f;
//     public GameObject dialoguePrefab;
//     public List<GameObject> Dialogues;
//     public List<PersonController> Characters;

//     // Conversation histories for two characters
//     public List<Message> conversationHistory1 = new List<Message>();
//     public List<Message> conversationHistory2 = new List<Message>();

//     // System prompts defining each character’s personality
//     public string systemPrompt1 = "You are a confident, sarcastic personality who enjoys teasing the user. When you respond, output your answer as JSON in this format: { \"main_response\": \"Your main reply here.\", \"additional_note\": \"Any extra note here.\" }";
//     public string systemPrompt2 = "You are a calm, friendly personality who always tries to be helpful and polite. When you respond, output your answer as JSON in this format: { \"main_response\": \"Your main reply here.\", \"additional_note\": \"Any extra note here.\" }";

//     void Start()
//     {
//         myMaster = GetComponentInParent<Master>();
//         apiKey = myMaster.key;
//     }

//     // Called when the user sends a message. We call the API request twice—once per personality.
//     public void ParsedText(string userInput)
//     {
//         Debug.Log("Received input: " + userInput);
//         // Start two separate API requests: one for Character 1 and one for Character 2.
//         StartCoroutine(SendRequestForCharacter(userInput, 1, conversationHistory1, systemPrompt1));
//         StartCoroutine(SendRequestForCharacter(userInput, 2, conversationHistory2, systemPrompt2));
//     }

//     // Coroutine for sending a request for a given character, including its conversation history.
//     private IEnumerator SendRequestForCharacter(string userMessage, int characterNo, List<Message> history, string systemPrompt, int retryAttempts = 0)
//     {
//         const int maxRetryAttempts = 3;  // Maximum number of retries

//         // Add the user's message to this character's conversation history.
//         history.Add(new Message { role = "user", content = userMessage });

//         // Build the messages list: start with the system prompt, then all history.
//         List<Message> messages = new List<Message>();
//         messages.Add(new Message { role = "system", content = systemPrompt });
//         messages.AddRange(history);

//         // Wrap into a conversation object for JSON serialization.
//         ConversationWrapper conversation = new ConversationWrapper
//         {
//             model = "gpt-3.5-turbo",
//             messages = messages.ToArray()
//         };

//         string jsonBody = JsonUtility.ToJson(conversation);
//         UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
//         byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonBody);
//         request.uploadHandler = new UploadHandlerRaw(jsonToSend);
//         request.downloadHandler = new DownloadHandlerBuffer();
//         request.SetRequestHeader("Content-Type", "application/json");
//         request.SetRequestHeader("Authorization", "Bearer " + apiKey);

//         Debug.Log("Sending request for Character " + characterNo + ": " + jsonBody);

//         yield return request.SendWebRequest();

//         if (request.result != UnityWebRequest.Result.Success)
//         {
//             if (request.responseCode == 429)
//             {
//                 Debug.LogError("Rate limit exceeded for Character " + characterNo + ". Retrying...");
//                 yield return new WaitForSeconds(retryDelay);
//                 StartCoroutine(SendRequestForCharacter(userMessage, characterNo, history, systemPrompt, retryAttempts));
//             }
//             else
//             {
//                 Debug.LogError("Request failed for Character " + characterNo + ": " + request.error);
//                 Debug.LogError("Response Code: " + request.responseCode);
//                 Debug.LogError("Response Body: " + request.downloadHandler.text);
//             }
//             request.Dispose();
//             yield break;
//         }
//         else
//         {
//             string response = request.downloadHandler.text;
//             Debug.Log("Raw Response for Character " + characterNo + ": " + response);

//             // Parse the raw response from OpenAI into our OpenAIResponse object.
//             OpenAIResponse openAIResponse = JsonUtility.FromJson<OpenAIResponse>(response);
//             if (openAIResponse == null || openAIResponse.choices == null || openAIResponse.choices.Length == 0)
//             {
//                 Debug.LogError("Response did not contain expected choices for Character " + characterNo);
//                 request.Dispose();
//                 yield break;
//             }

//             // Get the assistant's message content.
//             string fullContent = openAIResponse.choices[0].message.content.Trim();
//             Debug.Log("Full content before cleaning for Character " + characterNo + ": " + fullContent);

//             // Attempt to extract a JSON object from the output.
//             int firstBrace = fullContent.IndexOf('{');
//             int lastBrace = fullContent.LastIndexOf('}');
//             if (firstBrace != -1 && lastBrace != -1 && lastBrace > firstBrace)
//             {
//                 fullContent = fullContent.Substring(firstBrace, lastBrace - firstBrace + 1);
//                 Debug.Log("Cleaned JSON Content for Character " + characterNo + ": " + fullContent);
//             }
//             else
//             {
//                 // Debug.LogError("Response is not in valid JSON format for Character " + characterNo + ": " + fullContent);
//                 request.Dispose();

//                 if (retryAttempts < maxRetryAttempts)
//                 {
//                     Debug.Log("Retrying request for Character " + characterNo + " (attempt " + (retryAttempts + 1) + ")...");
//                     yield return new WaitForSeconds(retryDelay);
//                     StartCoroutine(SendRequestForCharacter(userMessage, characterNo, history, systemPrompt, retryAttempts + 1));
//                 }
//                 else
//                 {
//                     Debug.LogError("Max retry attempts reached for Character " + characterNo);
//                 }
//                 yield break;
//             }

//             // Now parse the structured JSON output.
//             ChatResponse chatResponse = JsonUtility.FromJson<ChatResponse>(fullContent);
//             if (chatResponse != null)
//             {
//                 Debug.Log("Character " + characterNo + " main response: " + chatResponse.main_response);
//                 Debug.Log("Character " + characterNo + " additional note: " + chatResponse.additional_note);

//                 // Append the assistant's main response to the history for future context.
//                 history.Add(new Message { role = "assistant", content = chatResponse.main_response });

//                 // Display the main response using your existing ShowDialog method.
//                 ShowDialog(chatResponse.main_response, characterNo);
//             }
//             else
//             {
//                 Debug.LogError("Failed to parse structured response for Character " + characterNo);
//                 if (retryAttempts < maxRetryAttempts)
//                 {
//                     Debug.Log("Retrying request for Character " + characterNo + " (attempt " + (retryAttempts + 1) + ")...");
//                     yield return new WaitForSeconds(retryDelay);
//                     StartCoroutine(SendRequestForCharacter(userMessage, characterNo, history, systemPrompt, retryAttempts + 1));
//                 }
//                 else
//                 {
//                     Debug.LogError("Max retry attempts reached for Character " + characterNo);
//                 }
//             }
//         }

//         request.Dispose();
//         yield break;
//     }


//     // ShowDialog remains the same: it instantiates a dialogue prefab under the appropriate character's dialogue holder.
//     public void ShowDialog(string message, int characterNo)
//     {
//         GameObject newObject = Instantiate(dialoguePrefab, Characters[0].dialogueHolder);
//         newObject.transform.SetParent(Characters[characterNo].dialogueHolder);
//         newObject.transform.localPosition = Vector3.zero; // Reset position relative to parent

//         Debug.Log("Displaying for Character " + characterNo + ": " + message);

//         dialogueDisplay myDialogueDisplay = newObject.GetComponent<dialogueDisplay>();
//         myDialogueDisplay.showMessage(message);

//         Dialogues.Add(newObject);
//     }
// }
