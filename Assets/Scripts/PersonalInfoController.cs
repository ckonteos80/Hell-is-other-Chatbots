using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

// // Global Models
// [Serializable]
// public class OpenAIResponseFuntion
// {
//     public ChoiceFuntion[] choices;
// }

// [Serializable]
// public class ChoiceFuntion
// {
//     public MessageFuntion message;
// }

// [Serializable]
// public class MessageFuntion
// {
//     public string role;
//     public string content;
//     // Optional field for function calls
//     public FunctionCall function_call;
// }

// [Serializable]
// public class ConversationWrapperFuntion
// {
//     public string model;
//     public float temperature;
//     public MessageFuntion[] messages;
// }

// [Serializable]
// public class ChatRequestFuntion
// {
//     public string model;
//     public float temperature;
//     public List<MessageFuntion> messages;
//     public List<FunctionDefinition> functions;
//     public string function_call;
// }

// [Serializable]
// public class FunctionDefinition
// {
//     public string name;
//     public string description;
//     public FunctionParameters parameters;
//     public bool strict; // Must be included per documentation.
// }

// [Serializable]
// public class FunctionParameters
// {
//     public string type;
//     public FunctionParameterProperties properties;
//     public List<string> required;
// }

// [Serializable]
// public class FunctionParameterProperties
// {
//     public FunctionParameter found_personal_info;
//     public FunctionParameter personal_info;
// }

// [Serializable]
// public class FunctionParameter
// {
//     public string type;
//     public string description;
// }

// [Serializable]
// public class FunctionCall
// {
//     public string name;
//     public string arguments;
// }

// [Serializable]
// public class FunctionCallArguments
// {
//     public bool found_personal_info;
//     public string personal_info;
// }

// This script handles function-calling requests.
public class PersonalInfoController : MonoBehaviour
{
    // // Set your API key (either via the Inspector or in code).
    // public string apiKey;
    // private string apiUrl = "https://api.openai.com/v1/chat/completions";
    // public float temp = 0.5f; // Adjust temperature as needed.
    // Master myMaster;

    // /// <summary>
    // /// Sends a function-calling request to the OpenAI API.
    // /// </summary>
    // /// <param name="systemMessage">The system prompt message.</param>
    // /// <param name="userMessage">The user prompt message.</param>
    // /// <param name="functions">A list of function definitions to include.</param>
    // /// <param name="callback">Callback invoked with the parsed OpenAIResponse.</param>
    // public IEnumerator SendFunctionRequest(string systemMessage, string userMessage, List<FunctionDefinition> functions, Action<OpenAIResponseFuntion> callback)
    // {
    //     // Build the messages list.
    //     List<MessageFuntion> messagesFuntion = new List<MessageFuntion>
    //     {
    //         new MessageFuntion { role = "system", content = systemMessage, function_call = null },
    //         new MessageFuntion { role = "user", content = userMessage, function_call = null }
    //     };

    //     // Clean up each message's content and ensure non-assistant messages don't include function_call.
    //     foreach (var msg in messagesFuntion)
    //     {
    //         if (!string.IsNullOrEmpty(msg.content))
    //             msg.content = msg.content.Trim().Replace("\n", " ");
    //         if (msg.role != "assistant")
    //             msg.function_call = null;
    //     }

    //     // Create a ChatRequest payload that includes function definitions.
    //     ChatRequestFuntion requestData = new ChatRequestFuntion
    //     {
    //         model = "gpt-3.5-turbo",  // Use a valid model.
    //         temperature = temp,
    //         messages = messagesFuntion,
    //         functions = functions,
    //         function_call = "auto"
    //     };

    //     // Serialize the request.
    //     string jsonData = JsonUtility.ToJson(requestData);

    //     // Use a regular expression to remove any function_call field that is either {} or null.
    //     // This regex removes any occurrences of "function_call": followed by {} or null (with optional whitespace and comma).
    //     jsonData = Regex.Replace(jsonData, "\"function_call\"\\s*:\\s*(null|\\{\\})\\s*,?", "");

    //     Debug.Log("Sending function request: " + jsonData);

    //     UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
    //     byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonData);
    //     request.uploadHandler = new UploadHandlerRaw(jsonToSend);
    //     request.downloadHandler = new DownloadHandlerBuffer();
    //     request.SetRequestHeader("Content-Type", "application/json");
    //     request.SetRequestHeader("Authorization", "Bearer " + apiKey);

    //     yield return request.SendWebRequest();

    //     if (request.result != UnityWebRequest.Result.Success)
    //     {
    //         Debug.LogError("Function request failed: " + request.error);
    //         Debug.LogError("Response Code: " + request.responseCode);
    //         Debug.LogError("Response Body: " + request.downloadHandler.text);
    //         request.Dispose();
    //         yield break;
    //     }
    //     else
    //     {
    //         string response = request.downloadHandler.text;
    //         Debug.Log("Raw Function Response: " + response);
    //         OpenAIResponseFuntion openAIResponse = JsonUtility.FromJson<OpenAIResponseFuntion>(response);
    //         if (openAIResponse == null || openAIResponse.choices == null || openAIResponse.choices.Length == 0)
    //         {
    //             Debug.LogError("Function response did not contain expected choices");
    //             request.Dispose();
    //             yield break;
    //         }
    //         callback?.Invoke(openAIResponse);
    //     }
    //     request.Dispose();
    //     yield break;
    // }

    // // Example usage: calling SendFunctionRequest in Start()
    // void Start()
    // {
    //     myMaster = GetComponentInParent<Master>();
    //     apiKey = myMaster.key; // Get the API key from your Master script.

    //     // Example system and user messages.
    //     string systemMsg = "You are a helpful assistant that checks if personal information is shared.";
    //     string userMsg = "User: My name is John Doe.";

    //     // Build a sample function definition.
    //     List<FunctionDefinition> funcDefs = new List<FunctionDefinition>();
    //     FunctionDefinition def = new FunctionDefinition();
    //     def.name = "updatePersonalInfo";
    //     def.description = "Updates personal info if any info is shared.";
    //     def.strict = true;  // 'strict' must be included per documentation.

    //     // Build function parameters.
    //     FunctionParameters parameters = new FunctionParameters();
    //     parameters.type = "object";
    //     parameters.required = new List<string> { "found_personal_info", "personal_info" };

    //     FunctionParameterProperties props = new FunctionParameterProperties();
    //     props.found_personal_info = new FunctionParameter { type = "boolean", description = "Indicates if personal info was shared" };
    //     props.personal_info = new FunctionParameter { type = "string", description = "The personal information details" };
    //     parameters.properties = props;

    //     def.parameters = parameters;
    //     funcDefs.Add(def);

    //     // Call the function request coroutine.
    //     StartCoroutine(SendFunctionRequest(systemMsg, userMsg, funcDefs, (response) =>
    //     {
    //         // Process the OpenAIResponse here.
    //         Debug.Log("Received function response with " + response.choices.Length + " choices.");
    //         if (response.choices.Length > 0)
    //         {
    //             Debug.Log("Response: " + response.choices[0].message.content);
    //         }
    //     }));
    // }
}
