using System;
using System.Text;
using Unity.Plastic.Newtonsoft.Json;

namespace UnityLLMAvatar.GoogleApi.DTO
{
    [Serializable]
    public struct TextToSpeechRequest
    {
        #region Nested Fields

        [Serializable]
        public struct Input
        {
            public string text;
        }

        [Serializable]
        public struct Voice
        {
            public string languageCode;
            public string name;
        }

        [Serializable]
        public struct AudioConfig
        {
            public string audioEncoding;
            public float pitch;
            public float speakingRate;
        }

        #endregion
        
        public Input input;
        public Voice voice;
        public AudioConfig audioConfig;

        public static TextToSpeechRequest MakeInstance(string text, VoiceScriptableObject voice)
        {
            return new TextToSpeechRequest
            {
                input =
                    new Input()
                    {
                        text = text
                    },
                voice =
                    new Voice()
                    {
                        languageCode = voice.languageCode,
                        name = voice.name
                    },
                audioConfig =
                    new AudioConfig()
                    {
                        audioEncoding = "MP3",
                        pitch = voice.pitch,
                        speakingRate = voice.speed
                    }
            };
        }
    }
    
    [Serializable]
    public struct TextToSpeechResponse
    {
        public string audioContent;

    }
}