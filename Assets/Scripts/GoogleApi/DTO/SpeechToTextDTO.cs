using System;

namespace UnityLLMAvatar.GoogleApi.DTO
{
    [Serializable]
    public struct SpeechToTextRequest
    {
        #region Nested fields

        [Serializable]
        public struct SpeechConfig
        {
            public string encoding;
            public int sampleRateHertz;
            public string languageCode;
            public bool enableWordTimeOffsets;
        }
        
        [Serializable]
        public struct AudioData
        {
            // public string uri;
            public string content;
        }

        #endregion
        
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
    public struct SpeechToTextResponse
    {
        [Serializable]
        public struct Result
        {
            [Serializable]
            public struct Alternative
            {
                public string transcript;
                public float confidence;
            }
            
            public Alternative[] alternatives;
        }

        public Result[] results;
    }
}