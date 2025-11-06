using System;
using UnityEngine;
using UnityLLMAvatar.GoogleApi;
using UnityLLMAvatar.LLM;

namespace UnityLLMAvatar
{
    [RequireComponent(typeof(SpeechToTextManager), typeof(ChatBot))]
    public class AIManager : MonoBehaviour
    {
        [Tooltip("Drag and drop the Virtual Character prefab here.")]
        [SerializeField]
        private TextToSpeechManager _virtualCharacter;

        private ChatBot _chatBot;
        private SpeechToTextManager _speechToTextManager;

        private void Awake()
        {
            _speechToTextManager = GetComponent<SpeechToTextManager>();
            _chatBot = GetComponent<ChatBot>();
        }

        private void OnEnable()
        {
            _chatBot.OnResponseReceived += OnChatBotResponse;
            _speechToTextManager.OnSpeechRecorded += ProcessSpeech;
        }

        private void OnDisable()
        {
            _chatBot.OnResponseReceived -= OnChatBotResponse;
            _speechToTextManager.OnSpeechRecorded -= ProcessSpeech;
        }

        private async void ProcessSpeech(byte[] rawAudio)
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
                    _virtualCharacter.Voice);

                // 2. Play the speech audio
                _virtualCharacter.SaveAndPlaySpeech(ttsResponse);

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