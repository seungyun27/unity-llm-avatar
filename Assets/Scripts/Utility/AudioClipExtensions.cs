using System.IO;
using UnityEngine;

namespace UnityLLMAvatar.Utility
{
    public static class AudioClipExtensions
    {
        public static byte[] GetSamplesAsWav(this AudioClip clip, int audioLength)
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