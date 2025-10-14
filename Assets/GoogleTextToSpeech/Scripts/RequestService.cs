using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GoogleTextToSpeech.Scripts.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace GoogleTextToSpeech.Scripts
{
    public static class RequestService
    {
        public static void SendDataToGoogle(string url, string apiKey, DataToSend payload, Action<string> onSuccess,
            Action<BadRequestData> onError)
        {
            var headers = new Dictionary<string, string>
            {
                { "X-Goog-Api-Key", apiKey },
                { "Content-Type", "application/json; charset=utf-8" }
            };
            
            Post(url, JsonUtility.ToJson(payload), onSuccess,  onError, headers);
        }

        private static async void Post(string url, string bodyJsonString, Action<string> onSuccess,
            Action<BadRequestData> onError, Dictionary<string, string> headers = null)
        {
            var request = new UnityWebRequest(url, "POST");
            var bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }

            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (HasError(request, out var badRequest))
            {
                onError?.Invoke(badRequest);
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

            badRequestData = JsonUtility.FromJson<BadRequestData>(request.downloadHandler.text);

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