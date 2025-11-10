using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityLLMAvatar.GoogleApi.DTO;
using UnityLLMAvatar.Utility;
using Debug = UnityEngine.Debug;

namespace UnityLLMAvatar.GoogleApi
{
    public class GoogleApiException : Exception
    {
        public string ErrorMessage { get; }
        public int Code { get; }

        public GoogleApiException(GoogleApiError error) : base(error.Error.Message)
        {
            ErrorMessage = error.Error.Message;
            Code = error.Error.Code;
        }
    }

    public static class GoogleApiRequestService
    {
        private const string STT_URL = "https://speech.googleapis.com/v1/speech:recognize";
        private const string TTS_URL = "https://texttospeech.googleapis.com/v1/text:synthesize";

        public static async Task<string> SendSpeechToTextRequestAsync(byte[] rawAudio)
        {
            var payload = SpeechToTextRequest.MakeInstance(rawAudio);

            var response = string.Empty;
            try
            {
                var timer = new Stopwatch();
                timer.Start();

                response = await PostAsync(
                    url: STT_URL,
                    body: payload.ToBytes(),
                    headers: new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json; charset=utf-8" },
                        { "X-Goog-Api-Key", EnvManager.GetApiKey("TTS_API_KEY") }
                    });

                timer.Stop();
                Debug.Log($"[STT_API]{timer.ElapsedMilliseconds}");
            }
            catch (GoogleApiException ex)
            {
                Debug.LogException(ex);
            }

            var speechResponse = JsonConvert.DeserializeObject<SpeechToTextResponse>(response);

            var transcript = speechResponse.results[0].alternatives[0].transcript;
            Debug.Log($"User speech transcript: {transcript}");
            return transcript;
        }

        public static async Task<TextToSpeechResponse> SendTextToSpeechRequestAsync(
            string text,
            VoiceScriptableObject voice)
        {
            var payload = TextToSpeechRequest.MakeInstance(text, voice);
            
            var response = string.Empty;
            try
            {
                var timer = new Stopwatch();
                timer.Start();
                
                response = await PostAsync(
                    url: TTS_URL,
                    body: payload.ToBytes(),
                    headers: new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json; charset=utf-8" },
                        { "X-Goog-Api-Key", EnvManager.GetApiKey("TTS_API_KEY") }
                    });

                timer.Stop();
                Debug.Log($"[TTS_API]{timer.ElapsedMilliseconds}");
            }
            catch (GoogleApiException ex)
            {
                Debug.LogException(ex);
            }

            return JsonConvert.DeserializeObject<TextToSpeechResponse>(response);
        }

        private static async Task<string> PostAsync(
            string url,
            byte[] body,
            Dictionary<string, string> headers)
        {
            using var request = new UnityWebRequest(
                url: url,
                method: "POST",
                uploadHandler: new UploadHandlerRaw(body),
                downloadHandler: new DownloadHandlerBuffer());

            // Set headers
            foreach (var (k, v) in headers)
            {
                request.SetRequestHeader(k, v);
            }

            await request.SendWebRequest();

            return HasError(request, out var apiError)
                ? throw new GoogleApiException(apiError)
                : request.downloadHandler.text;
        }

        private static bool HasError(UnityWebRequest request, out GoogleApiError googleApiError)
        {
            if (request.responseCode is 200 or 201)
            {
                googleApiError = default;
                return false;
            }

            try
            {
                googleApiError = JsonConvert.DeserializeObject<GoogleApiError>(request.downloadHandler.text);
                return true;
            }
            catch (Exception)
            {
                googleApiError = new GoogleApiError(request);
                return true;
            }
        }
    }
}