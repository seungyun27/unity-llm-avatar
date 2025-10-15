using System;

namespace UnityLLMAvatar.GoogleApi
{
    [Serializable]
    public class SpeechToTextRequest
    {
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
        
        public SpeechConfig config;
        public AudioData audio;

        public static SpeechToTextRequest MakeInstance(byte[] rawAudio)
        {
            return new SpeechToTextRequest
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

    // Response format for Google Speech-to-Text API
    [Serializable]
    public class SpeechToTextResponse
    {
        [Serializable]
        public class Result
        {
            [Serializable]
            public class Alternative
            {
                public string transcript;
                public float confidence;
            }
            
            public Alternative[] alternatives;
        }

        public Result[] results;
    }
}