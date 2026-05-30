using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System; // For Exception

// // Data structure for a single chat message.
// [System.Serializable]
// public class Message
// {
//     public string role;    // "system", "user", or "assistant"
//     public string content;
// }

// Wrapper for a conversation (each example)
[System.Serializable]
public class ChatHistory
{
    public string modelName;
    public float temperature;
    public int maxTokens;
    public List<Message> messages;
}

public class saveController : MonoBehaviour
{
    private string sessionTimestamp;
    private string sessionId;

    void Awake()
    {
        sessionTimestamp = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss");
        sessionId = Guid.NewGuid().ToString("N").Substring(0, 8);
    }

    /// <summary>
    /// Appends a new conversation entry as a JSON line to the specified file.
    /// Each entry will be saved in JSON Lines (JSONL) format.
    /// </summary>
    /// <param name="filePath">Full file path (including filename and .json extension).</param>
    /// <param name="newMessages">The conversation messages to append.</param>
    public void AppendEntry(string filePath, List<Message> newMessages, string modelName = "", float temperature = 0f, int maxTokens = 0)
    {
        ChatHistory history = new ChatHistory { modelName = modelName, temperature = temperature, maxTokens = maxTokens, messages = newMessages };

        // Convert the history to JSON.
        string jsonEntry = JsonUtility.ToJson(history, true);

        try
        {
            // Append the JSON entry followed by a newline.
            File.AppendAllText(filePath, jsonEntry + "\n");
            Debug.Log("Appended new entry to " + filePath);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error appending entry: " + ex.Message);
        }
    }

    /// <summary>
    /// Creates a new dialogue entry and appends it as a separate JSON line.
    /// </summary>
    public void NewEntryDialogue(string system, string user, string assistant, string modelName = "", float temperature = 0f, int maxTokens = 0)
    {
        List<Message> conversationMessages = new List<Message>();
        conversationMessages.Add(new Message { role = "system", content = system });
        conversationMessages.Add(new Message { role = "user", content = user });
        conversationMessages.Add(new Message { role = "assistant", content = assistant });

        string filePath = Path.Combine(Application.dataPath, "Saved Info", "Dialogue", "dialogue_" + sessionId + "_" + sessionTimestamp + ".json");
        AppendEntry(filePath, conversationMessages, modelName, temperature, maxTokens);
    }

    /// <summary>
    /// Creates a new information entry and appends it as a separate JSON line.
    /// </summary>
    public void NewEntryInfo(string system, string user, string assistant, string modelName = "", float temperature = 0f, int maxTokens = 0)
    {
        List<Message> conversationMessages = new List<Message>();
        conversationMessages.Add(new Message { role = "system", content = system });
        conversationMessages.Add(new Message { role = "user", content = user });
        conversationMessages.Add(new Message { role = "assistant", content = assistant });

        string filePath = Path.Combine(Application.dataPath, "Saved Info", "Information", "information_" + sessionId + "_" + sessionTimestamp + ".json");
        AppendEntry(filePath, conversationMessages, modelName, temperature, maxTokens);
    }

    public void NewCharacterInfo(string system, string user, string assistant, string modelName = "", float temperature = 0f, int maxTokens = 0)
    {
        List<Message> conversationMessages = new List<Message>();
        conversationMessages.Add(new Message { role = "system", content = system });
        conversationMessages.Add(new Message { role = "user", content = user });
        conversationMessages.Add(new Message { role = "assistant", content = assistant });

        string filePath = Path.Combine(Application.dataPath, "Saved Info", "Character", "character_" + sessionId + "_" + sessionTimestamp + ".json");
        AppendEntry(filePath, conversationMessages, modelName, temperature, maxTokens);
    }

    public void NewAdressing(string system, string user, string assistant, string modelName = "", float temperature = 0f, int maxTokens = 0)
    {
        List<Message> conversationMessages = new List<Message>();
        conversationMessages.Add(new Message { role = "system", content = system });
        conversationMessages.Add(new Message { role = "user", content = user });
        conversationMessages.Add(new Message { role = "assistant", content = assistant });

        string filePath = Path.Combine(Application.dataPath, "Saved Info", "Addressing", "addressing_" + sessionId + "_" + sessionTimestamp + ".json");
        AppendEntry(filePath, conversationMessages, modelName, temperature, maxTokens);
    }
}
