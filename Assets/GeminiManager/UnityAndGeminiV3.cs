using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using GoogleTextToSpeech.Scripts;
using LLMUnitySamples;
using UnityLLMAvatar;
using UnityLLMAvatar.util;


[Serializable]
public class UnityAndGeminiKey
{
    public string key;
}

[Serializable]
public class Response
{
    public Candidate[] candidates;
}

public class ChatRequest
{
    public Content[] contents;
}

[Serializable]
public class Candidate
{
    public Content content;
}

[Serializable]
public class Content
{
    public string role; 
    public Part[] parts;
}

[Serializable]
public class Part
{
    public string text;
}


public class UnityAndGeminiV3 : MonoBehaviour
{
    private readonly string apiKey = EnvManager.GetApiKey("LANG_API_KEY");
    private const string API_ENDPOINT = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-lite:generateContent";

    [Header("NPC Function")]
    [SerializeField] private TextToSpeechManager _textToSpeechManager;
    private Content[] chatHistory = Array.Empty<Content>();

    public ChatBot ChatBot;

    // Functions for sending a new prompt, or a chat to Gemini
    private IEnumerator SendPromptRequestToGemini(string promptText)
    {
        string url = $"{API_ENDPOINT}?key={apiKey}";

        string jsonData = "{\"contents\": [{\"parts\": [{\"text\": \"{" + promptText + "}\"}]}]}";

        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

        // Create a UnityWebRequest with the JSON data
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("Request complete!");
                Response response = JsonUtility.FromJson<Response>(www.downloadHandler.text);
                if (response.candidates.Length > 0 && response.candidates[0].content.parts.Length > 0)
                {
                    //This is the response to your request
                    string text = response.candidates[0].content.parts[0].text;
                    Debug.Log(text);
                }
                else
                {
                    Debug.Log("No text found.");
                }
            }
        }
    }

    public void SendChat(string userMessage)
    {
        // string userMessage = inputField.text;
        // StartCoroutine(SendChatRequestToGemini(userMessage));
    }

    /*private IEnumerator SendChatRequestToGemini(string newMessage)
    {
        string url = $"{API_ENDPOINT}?key={apiKey}";

        Content userContent = new Content
        {
            role = "user",
            parts = new Part[]
            {
                new Part { text = newMessage }
            }
        };

        List<Content> contentsList = new List<Content>(chatHistory);
        contentsList.Add(userContent);
        chatHistory = contentsList.ToArray();

        ChatRequest chatRequest = new ChatRequest { contents = chatHistory };

        string jsonData = JsonUtility.ToJson(chatRequest);

        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

        // Create a UnityWebRequest with the JSON data
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("Request complete!");
                Response response = JsonUtility.FromJson<Response>(www.downloadHandler.text);
                if (response.candidates.Length > 0 && response.candidates[0].content.parts.Length > 0)
                {
                    //This is the response to your request
                    string reply = response.candidates[0].content.parts[0].text;
                    Content botContent = new Content
                    {
                        role = "model",
                        parts = new Part[]
                        {
                                new Part { text = reply }
                        }
                    };

                    Debug.Log(reply);
                    googleServices.SendTextToGoogle(reply);


                    //This part shows the text in the Canvas
                    // uiText.text = reply;
                    //This part adds the response to the chat history, for your next message
                    contentsList.Add(botContent);
                    chatHistory = contentsList.ToArray();
                }
                else
                {
                    Debug.Log("No text found.");
                }
            }
        }
    }*/
}