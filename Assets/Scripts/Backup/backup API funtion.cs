// using UnityEngine;
// using TMPro;
// using UnityEngine.Networking;
// using System.Collections;
// using System;
// using System.Collections.Generic;

// public class CharacterController : MonoBehaviour
// {
//     Master myMaster;
//     public string apiKey;
//     private string apiUrl = "https://api.openai.com/v1/chat/completions";
//     private float retryDelay = 2f;
//     public GameObject dialoguePrefab;
//     public List<GameObject> Dialogues;
//     public List<PersonController> Characters;

//     // Conversation histories for each character.
//     private List<Message> conversationHistory1 = new List<Message>();
//     private List<Message> conversationHistory2 = new List<Message>();

//     // System prompts defining each character's personality.
//     private string systemPrompt1 = "IMPORTANT: You are Character 1. You are a confident, sarcastic personality who enjoys teasing the user. You MUST output EXACTLY one JSON object with exactly two keys: 'main_response' and 'additional_note'. Do not output any extra text. Format your output like: { \"main_response\": \"Your main reply here.\", \"additional_note\": \"Your additional note here.\" }";
//     private string systemPrompt2 = "IMPORTANT: You are Character 2. You are a calm, friendly personality who always tries to be helpful and polite. You MUST output EXACTLY one JSON object with exactly two keys: 'main_response' and 'additional_note'. Do not output any extra text. Format your output like: { \"main_response\": \"Your main reply here.\", \"additional_note\": \"Your additional note here.\" }";

//     void Start()
//     {
//         myMaster = GetComponentInParent<Master>();
//         apiKey = myMaster.key; // Ensure your Master script provides the API key.
//     }

//     // Called when the user sends a message. Calls the coroutine for both characters.
//     public void ParsedText(string userInput)
//     {
//         Debug.Log("Received input: " + userInput);
//         StartCoroutine(SendRequestForCharacter(userInput, 1, conversationHistory1, systemPrompt1));
//         StartCoroutine(SendRequestForCharacter(userInput, 2, conversationHistory2, systemPrompt2));
//     }

//     // Coroutine that sends a request using function calling and structured outputs for a given character.
//     private IEnumerator SendRequestForCharacter(string userMessage, int characterNo, List<Message> history, string systemPrompt, int retryAttempts = 0)
//     {
//         const int maxRetryAttempts = 3;

//         // Add the user's message to this character's conversation history.
//         history.Add(new Message { role = "user", content = userMessage });

//         // Build the messages list: system prompt first, then the conversation history.
//         List<Message> messages = new List<Message>();
//         messages.Add(new Message { role = "system", content = systemPrompt });
//         messages.AddRange(history);

//         // Clean up each message's content.
//         foreach (var msg in messages)
//         {
//             if (!string.IsNullOrEmpty(msg.content))
//             {
//                 msg.content = msg.content.Trim().Replace("\n", " ");
//             }
//         }

//         // Create our function definition.
//         FunctionDefinition[] functions = new FunctionDefinition[] { GetCharacterResponseFunction() };

//         // Manually build the JSON payload.
//         string jsonBody = BuildJsonPayload(messages, functions);

//         UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
//         byte[] jsonToSend = System.Text.Encoding.UTF8.GetBytes(jsonBody);
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

//             OpenAIResponse openAIResponse = JsonUtility.FromJson<OpenAIResponse>(response);
//             if (openAIResponse == null || openAIResponse.choices == null || openAIResponse.choices.Length == 0)
//             {
//                 Debug.LogError("Response did not contain expected choices for Character " + characterNo);
//                 request.Dispose();
//                 yield break;
//             }

//             // Get the assistant's message.
//             Message assistantMessage = openAIResponse.choices[0].message;
//             if (assistantMessage.function_call != null && !string.IsNullOrEmpty(assistantMessage.function_call.arguments))
//             {
//                 Debug.Log("Function call detected for Character " + characterNo + ": " + assistantMessage.function_call.name);
//                 Debug.Log("Function arguments: " + assistantMessage.function_call.arguments);

//                 // Ensure the function_call.arguments is properly trimmed.
//                 string functionArgs = assistantMessage.function_call.arguments.Trim();
//                 // Check if the string starts with '{' and ends with '}'
//                 if (functionArgs.StartsWith("{") && functionArgs.EndsWith("}"))
//                 {
//                     ChatResponse chatResponse = JsonUtility.FromJson<ChatResponse>(functionArgs);
//                     if (chatResponse != null)
//                     {
//                         // Combine main_response and additional_note (if available) into one display string.
//                         string displayText = chatResponse.main_response;
//                         if (!string.IsNullOrEmpty(chatResponse.additional_note))
//                         {
//                             displayText += "\n" + chatResponse.additional_note;
//                         }

//                         Debug.Log("Character " + characterNo + " display text: " + displayText);

//                         // Append the assistant's main response to the history.
//                         history.Add(new Message { role = "assistant", content = chatResponse.main_response });

//                         // Display the combined text.
//                         ShowDialog(displayText, characterNo);
//                     }
//                     else
//                     {
//                         Debug.LogError("Failed to parse function call arguments for Character " + characterNo);
//                         if (retryAttempts < maxRetryAttempts)
//                         {
//                             Debug.Log("Retrying request for Character " + characterNo + " (attempt " + (retryAttempts + 1) + ")...");
//                             yield return new WaitForSeconds(retryDelay);
//                             StartCoroutine(SendRequestForCharacter(userMessage, characterNo, history, systemPrompt, retryAttempts + 1));
//                         }
//                         else
//                         {
//                             Debug.LogError("Max retry attempts reached for Character " + characterNo);
//                         }
//                     }
//                 }
//                 else
//                 {
//                     Debug.LogError("Function arguments are not valid JSON for Character " + characterNo + ": " + functionArgs);
//                     // ShowDialog(functionArgs, characterNo); // Fallback: display raw text
//                 }
//             }
//             else
//             {
//                 Debug.Log("No function call in response for Character " + characterNo + ". Using plain text.");
//                 Debug.Log("Content: " + assistantMessage.content);
//                 // ShowDialog(assistantMessage.content, characterNo);
//             }
//         }
//         request.Dispose();
//         yield break;
//     }

//     // Function definition for our structured output.
//     private FunctionDefinition GetCharacterResponseFunction()
//     {
//         return new FunctionDefinition
//         {
//             name = "getCharacterResponse",
//             description = "Return a JSON object with keys 'main_response' and 'additional_note'.",
//             parameters = new ParameterDefinition
//             {
//                 type = "object",
//                 properties = new Properties
//                 {
//                     main_response = new Schema { type = "string", description = "The main response." },
//                     additional_note = new Schema { type = "string", description = "Additional notes." }
//                 },
//                 required = new string[] { "main_response" }
//             }
//         };
//     }

//     // Manually builds the JSON payload for the API request.
//     private string BuildJsonPayload(List<Message> messages, FunctionDefinition[] functions)
//     {
//         System.Text.StringBuilder sb = new System.Text.StringBuilder();
//         sb.Append("{");
//         // The model is set here.
//         sb.Append("\"model\": \"gpt-3.5-turbo\",");  // Change this to your desired model if needed.
//         sb.Append("\"messages\": [");

//         for (int i = 0; i < messages.Count; i++)
//         {
//             Message msg = messages[i];
//             sb.Append("{");
//             sb.Append("\"role\": \"" + EscapeJson(msg.role) + "\",");
//             sb.Append("\"content\": \"" + EscapeJson(msg.content) + "\"");
//             sb.Append("}");
//             if (i < messages.Count - 1)
//                 sb.Append(",");
//         }
//         sb.Append("],");

//         // Add the functions array.
//         sb.Append("\"functions\": [");
//         for (int i = 0; i < functions.Length; i++)
//         {
//             sb.Append(BuildFunctionJson(functions[i]));
//             if (i < functions.Length - 1)
//                 sb.Append(",");
//         }
//         sb.Append("]");
//         sb.Append("}");
//         return sb.ToString();
//     }

//     // Builds the JSON for the function definition.
//     private string BuildFunctionJson(FunctionDefinition functionDef)
//     {
//         return "{"
//              + "\"name\": \"" + EscapeJson(functionDef.name) + "\","
//              + "\"description\": \"" + EscapeJson(functionDef.description) + "\","
//              + "\"parameters\": {"
//              + "\"type\": \"" + EscapeJson(functionDef.parameters.type) + "\","
//              + "\"properties\": {"
//              + "\"main_response\": { \"type\": \"" + EscapeJson(functionDef.parameters.properties.main_response.type) + "\", \"description\": \"" + EscapeJson(functionDef.parameters.properties.main_response.description) + "\" },"
//              + "\"additional_note\": { \"type\": \"" + EscapeJson(functionDef.parameters.properties.additional_note.type) + "\", \"description\": \"" + EscapeJson(functionDef.parameters.properties.additional_note.description) + "\" }"
//              + "},"
//              + "\"required\": [\"main_response\"]"
//              + "}"
//              + "}";
//     }

//     // Escapes special characters in strings for JSON.
//     private string EscapeJson(string str)
//     {
//         if (string.IsNullOrEmpty(str)) return "";
//         return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", " ");
//     }

//     // Displays the dialogue using the specified character's dialogue holder.
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

// // ---------------- Helper Classes ----------------

// [Serializable]
// public class Message
// {
//     public string role;
//     public string content;
//     public FunctionCall function_call; // For function calling support.
// }

// [Serializable]
// public class FunctionCall
// {
//     public string name;
//     public string arguments; // JSON string containing your structured output.
// }

// [Serializable]
// public class ConversationWrapper
// {
//     public string model;
//     public Message[] messages;
//     public FunctionDefinition[] functions;
// }

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
// public class FunctionDefinition
// {
//     public string name;
//     public string description;
//     public ParameterDefinition parameters;
// }

// [Serializable]
// public class ParameterDefinition
// {
//     public string type;
//     public Properties properties;
//     public string[] required;
// }

// [Serializable]
// public class Properties
// {
//     public Schema main_response;
//     public Schema additional_note;
// }

// [Serializable]
// public class Schema
// {
//     public string type;
//     public string description;
// }

// [Serializable]
// public class ChatResponse
// {
//     public string main_response;
//     public string additional_note;
// }
