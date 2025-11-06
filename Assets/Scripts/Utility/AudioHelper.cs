using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityLLMAvatar.Utility
{
    public static class AudioHelper
    {
        public static async Task<string> SaveAsMp3(byte[] audio)
        {
            var filename = $"avatar-audio-{DateTime.Now:yyyyMMdd-HHmmss}.mp3";
            var filePath = $"{Application.temporaryCachePath}/{filename}";
            await using var fs = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
            );
            await fs.WriteAsync(audio, 0, audio.Length);
            Debug.Log($"Audio saved to {filePath}");
            return filePath;
        }

        public static async Task<AudioClip> LoadAudioClipFromMp3(string filePath)
        {
            using var request = UnityWebRequestMultimedia.GetAudioClip(
                uri: $"file://{filePath}",
                audioType: AudioType.MPEG
            );
            
            await request.SendWebRequest();
            
            if (request.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
            {
                throw new Exception("Error loading MP3: " + request.error);
            }

            var clip = DownloadHandlerAudioClip.GetContent(request);
            return clip ?? throw new Exception($"Failed to get AudioClip content from {filePath}");
        }
    }
}