using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GoogleTextToSpeech.Scripts.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityLLMAvatar.GoogleApi
{
    public class GoogleApiRequestService
    {
        private static readonly Dictionary<string, string> _headers = new()
        {
            { "Content-Type", "application/json; charset=utf-8" }
        };

        private const string STT_URL = "https://speech.googleapis.com/v1/speech:recognize";
        private const string TTS_URL = "https://texttospeech.googleapis.com/v1/text:synthesize";

        public static void SendSpeechToTextRequest(
            byte[] rawAudio,
            Action<string> onSuccess,
            Action<BadRequestData> onError = null)
        {
            _headers.Add("X-Goog-Api-Key", EnvManager.GetApiKey("TTS_API_KEY"));

            var payload = JsonUtility.ToJson(STTPayload.MakeInstance(rawAudio));

            DoPost(
                url: STT_URL,
                body: Encoding.UTF8.GetBytes(payload),
                onSuccess: onSuccess,
                onError: onError
            );
        }

        public static void SendTextToSpeechRequest(
            string text,
            VoiceScriptableObject voice,
            Action<string> onSuccess,
            Action<BadRequestData> onError = null)
        {
            _headers.Add("X-Goog-Api-Key", EnvManager.GetApiKey("TTS_API_KEY"));

            var payload = JsonUtility.ToJson(TTSPayload.MakeInstance(text, voice));

            DoPost(
                url: TTS_URL,
                body: Encoding.UTF8.GetBytes(payload),
                onSuccess: onSuccess,
                onError: onError
            );
        }

        private static async void DoPost(
            string url,
            byte[] body,
            Action<string> onSuccess,
            Action<BadRequestData> onError)
        {
            // Create the request
            var request = new UnityWebRequest(url, "POST")
            {
                uploadHandler = new UploadHandlerRaw(body),
                downloadHandler = new DownloadHandlerBuffer()
            };

            // Set headers for the request
            foreach (var (k, v) in _headers)
            {
                request.SetRequestHeader(k, v);
            }

            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (HasError(request, out var badRequest))
            {
                if (onError != null)
                {
                    onError.Invoke(badRequest);
                }
                else
                {
                    Debug.LogError($"Error {badRequest.error.code} : {badRequest.error.message}");
                }
            }
            else
            {
                onSuccess?.Invoke(request.downloadHandler.text);
            }

            request.Dispose();
        }

        private static bool HasError(UnityWebRequest request, out BadRequestData badRequestData)
        {
            if (request.responseCode is 200 or 201)
            {
                badRequestData = null;
                return false;
            }

            try
            {
                badRequestData = JsonUtility.FromJson<BadRequestData>(request.downloadHandler.text);
                return true;
            }
            catch (Exception)
            {
                badRequestData = new BadRequestData
                {
                    error = new Error
                    {
                        code = (int)request.responseCode,
                        message = request.error
                    }
                };

                return true;
            }
        }
    }
}