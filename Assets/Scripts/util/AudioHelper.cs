using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityLLMAvatar.GoogleApi;

namespace UnityLLMAvatar.util
{
    public static class AudioHelper
    {
        public static async Task<string> SaveAsMP3(byte[] audio)
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

        public static async Task<AudioClip> LoadAudioClipFromMP3(string filePath)
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

    public static class AudioClipExtensions
    {
        public static byte[] GetSamplesAsWAV(this AudioClip clip, int audioLength)
        {
            var samples = new float[audioLength * clip.channels];
            clip.GetData(samples, 0);
            
            using var memoryStream = new MemoryStream(44 + samples.Length * 2);
            using var writer = new BinaryWriter(memoryStream);
            writer.Write("RIFF".ToCharArray());
            writer.Write(36 + samples.Length * 2);
            writer.Write("WAVE".ToCharArray());
            writer.Write("fmt ".ToCharArray());
            writer.Write(16);
            writer.Write((ushort)1);
            writer.Write((ushort)clip.channels);
            writer.Write(clip.frequency);
            writer.Write(clip.frequency * clip.channels * 2);
            writer.Write((ushort)(clip.channels * 2));
            writer.Write((ushort)16);
            writer.Write("data".ToCharArray());
            writer.Write(samples.Length * 2);

            foreach (var sample in samples)
            {
                writer.Write((short)(sample * short.MaxValue));
            }

            return memoryStream.ToArray();
        }
    }
}