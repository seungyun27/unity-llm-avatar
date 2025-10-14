using System;

namespace UnityLLMAvatar.GoogleApi
{
    [Serializable]
    public class STTPayload
    {
        public SpeechConfig config;
        public AudioData audio;

        public static STTPayload MakeInstance(byte[] rawAudio)
        {
            return new STTPayload
            {
                config = new SpeechConfig
                {
                    encoding = "LINEAR16",
                    // sampleRateHertz = 16000,
                    languageCode = "en-US",
                    enableWordTimeOffsets = false
                },
                audio = new AudioData
                {
                    // uri = audioUri
                    content = Convert.ToBase64String(rawAudio),
                }
            };
        }
    }

    [Serializable]
    public class SpeechConfig
    {
        public string encoding;
        public int sampleRateHertz;
        public string languageCode;
        public bool enableWordTimeOffsets;
    }

    [Serializable]
    public class AudioData
    {
        // public string uri;
        public string content;
    }

    // Response format for Google Speech-to-Text API
    [Serializable]
    public class SpeechToTextResponse
    {
        public Result[] results;
    }

    [Serializable]
    public class Result
    {
        public Alternative[] alternatives;
    }

    [Serializable]
    public class Alternative
    {
        public string transcript;
        public float confidence;
    }

    // Error response format
    [Serializable]
    public class BadRequestData
    {
        public Error error;
    }

    [Serializable]
    public class Error
    {
        public int code;
        public string message;
    }
}