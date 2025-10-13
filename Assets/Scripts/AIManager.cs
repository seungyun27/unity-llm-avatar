using System.Collections;
using System.Collections.Generic;
using GoogleSpeechToText.Scripts;
using GoogleTextToSpeech.Scripts;
using LLMUnity;
using UnityEngine;

namespace UnityLLMAvatar
{
    public class AIManager : MonoBehaviour
    {
        public LLM LLM;
        public LLMCharacter LLMCharacter;
        public ChatBot ChatBot;
        public TextToSpeechManager TextToSpeechManager;
        public SpeechToTextManager SpeechToTextManager;
    }
}