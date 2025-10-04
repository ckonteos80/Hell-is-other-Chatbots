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
    public List<Message> messages;
}

public class saveController : MonoBehaviour
{
    /// <summary>
    /// Appends a new conversation entry as a JSON line to the specified file.
    /// Each entry will be saved in JSON Lines (JSONL) format.
    /// </summary>
    /// <param name="filePath">Full file path (including filename and .json extension).</param>
    /// <param name="newMessages">The conversation messages to append.</param>
    public void AppendEntry(string filePath, List<Message> newMessages)
    {
        // Create a ChatHistory object for this conversation.
        ChatHistory history = new ChatHistory { messages = newMessages };

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
    public void NewEntryDialogue(string system, string user, string assistant)
    {
        List<Message> conversationMessages = new List<Message>();
        conversationMessages.Add(new Message { role = "system", content = system });
        conversationMessages.Add(new Message { role = "user", content = user });
        conversationMessages.Add(new Message { role = "assistant", content = assistant });

        // For example, saving in a file named dialogue.json inside the Assets folder.
        // In the Editor, Application.dataPath points to your Assets folder.
        string filePath = Path.Combine(Application.dataPath, "dialogue.json");
        AppendEntry(filePath, conversationMessages);
    }

    /// <summary>
    /// Creates a new information entry and appends it as a separate JSON line.
    /// </summary>
    public void NewEntryInfo(string system, string user, string assistant)
    {
        List<Message> conversationMessages = new List<Message>();
        conversationMessages.Add(new Message { role = "system", content = system });
        conversationMessages.Add(new Message { role = "user", content = user });
        conversationMessages.Add(new Message { role = "assistant", content = assistant });

        string filePath = Path.Combine(Application.dataPath, "information.json");
        AppendEntry(filePath, conversationMessages);
    }

    public void NewCharacterInfo(string system, string user, string assistant)
    {
        List<Message> conversationMessages = new List<Message>();
        conversationMessages.Add(new Message { role = "system", content = system });
        conversationMessages.Add(new Message { role = "user", content = user });
        conversationMessages.Add(new Message { role = "assistant", content = assistant });

        string filePath = Path.Combine(Application.dataPath, "characters.json");
        AppendEntry(filePath, conversationMessages);
    }

    public void NewAdressing(string system, string user, string assistant)
    {
        List<Message> conversationMessages = new List<Message>();
        conversationMessages.Add(new Message { role = "system", content = system });
        conversationMessages.Add(new Message { role = "user", content = user });
        conversationMessages.Add(new Message { role = "assistant", content = assistant });

        string filePath = Path.Combine(Application.dataPath, "adress.json");
        AppendEntry(filePath, conversationMessages);
    }
}
