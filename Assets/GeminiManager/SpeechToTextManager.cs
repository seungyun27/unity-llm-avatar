using System.IO;
using UnityEngine;
using UnityLLMAvatar;

namespace GoogleSpeechToText.Scripts
{
    public class SpeechToTextManager : MonoBehaviour
    {
        private readonly string apiKey = EnvManager.GetApiKey("TTS_API_KEY");

        public AIManager AIManager;
        
        private AudioClip clip;
        private byte[] bytes;
        private bool _isRecording = false;

        public void StartRecording()
        {
            if (_isRecording) return;
            
            clip = Microphone.Start(null, false, 10, 44100);
            _isRecording = true;
            
            Debug.Log("Recording started...");
        }
        
        public void StopRecording()
        {
            if (!_isRecording) return;
            
            var position = Microphone.GetPosition(null);
            Microphone.End(null);
            var samples = new float[position * clip.channels];
            clip.GetData(samples, 0);
            bytes = EncodeAsWAV(samples, clip.frequency, clip.channels);
            _isRecording = false;
            Debug.Log("Recording stopped.");
            
            GoogleCloudSpeechToText.SendSpeechToTextRequest(bytes, apiKey,
                (response) =>
                {
                    Debug.Log("Speech-to-Text Response: " + response);
                    // Parse the response if needed
                    var speechResponse = JsonUtility.FromJson<SpeechToTextResponse>(response);
                    var transcript = speechResponse.results[0].alternatives[0].transcript;
                    Debug.Log("Transcript: " + transcript);
                    AIManager.SubmitTranscript(transcript);
                },
                (error) => Debug.LogError("Error: " + error.error.message));
        }

        private byte[] EncodeAsWAV(float[] samples, int frequency, int channels)
        {
            using (var memoryStream = new MemoryStream(44 + samples.Length * 2))
            {
                using (var writer = new BinaryWriter(memoryStream))
                {
                    writer.Write("RIFF".ToCharArray());
                    writer.Write(36 + samples.Length * 2);
                    writer.Write("WAVE".ToCharArray());
                    writer.Write("fmt ".ToCharArray());
                    writer.Write(16);
                    writer.Write((ushort)1);
                    writer.Write((ushort)channels);
                    writer.Write(frequency);
                    writer.Write(frequency * channels * 2);
                    writer.Write((ushort)(channels * 2));
                    writer.Write((ushort)16);
                    writer.Write("data".ToCharArray());
                    writer.Write(samples.Length * 2);

                    foreach (var sample in samples)
                    {
                        writer.Write((short)(sample * short.MaxValue));
                    }
                }

                return memoryStream.ToArray();
            }
        }
    }
}