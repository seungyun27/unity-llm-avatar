using System;
using LLMUnity;
using UnityEngine;
using UnityLLMAvatar.GoogleApi;
using UnityLLMAvatar.LLM;

namespace UnityLLMAvatar
{
    public class AIManager : MonoBehaviour
    {
        [Header("LLM Unity")]
        [SerializeField]
        private LLMUnity.LLM _llm;

        [SerializeField]
        private LLMCharacter _llmCharacter;

        [SerializeField]
        private ChatBot _chatBot;

        [Header("Google API")]
        [SerializeField]
        private TextToSpeechManager _textToSpeechManager;

        [SerializeField]
        private SpeechToTextManager _speechToTextManager;

        private void Awake()
        {
            _chatBot.OnResponseReceived += OnChatBotResponse;
        }

        private void OnDisable()
        {
            _chatBot.OnResponseReceived -= OnChatBotResponse;
        }

        public async void ProcessSpeech(byte[] rawAudio)
        {
            try
            {
                var transcript = await GoogleApiRequestService.SendSpeechToTextRequestAsync(rawAudio);
                await _chatBot.SendMessageAsync(transcript);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        private async void OnChatBotResponse(string llmResponse)
        {
            try
            {
                // 1. LLM response -> Google TTS -> Speech audio
                var ttsResponse = await GoogleApiRequestService.SendTextToSpeechRequestAsync(
                    llmResponse,
                    _textToSpeechManager.Voice);
                
                // 2. Play the speech audio!
                _textToSpeechManager.SaveAndPlaySpeech(ttsResponse);
                
                // 3. Change button state to allow new recording
                _speechToTextManager.ChangeRecordButtonState();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}