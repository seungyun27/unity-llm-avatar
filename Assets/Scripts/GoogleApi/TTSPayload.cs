using System;
using GoogleTextToSpeech.Scripts.Data;

namespace UnityLLMAvatar.GoogleApi
{
    [Serializable]
    public class TTSPayload
    {
        public Input input;
        public Voice voice;
        public AudioConfig audioConfig;

        public static TTSPayload MakeInstance(string text, VoiceScriptableObject voice)
        {
            return new TTSPayload
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
    public class Input
    {
        public string text;
    }

    [Serializable]
    public class Voice
    {
        public string languageCode;
        public string name;
    }

    [Serializable]
    public class AudioConfig
    {
        public string audioEncoding;
        public float pitch;
        public float speakingRate;
    }
}