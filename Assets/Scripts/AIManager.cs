using GoogleSpeechToText.Scripts;
using GoogleTextToSpeech.Scripts;
using LLMUnity;
using UnityEngine;
using UnityLLMAvatar.GoogleApi;

namespace UnityLLMAvatar
{
    public class AIManager : MonoBehaviour
    {
        [Header("LLM Unity")]
        [SerializeField] private LLM _llm;
        [SerializeField] private LLMCharacter _llmCharacter;
        [SerializeField] private ChatBot _chatBot;

        [Header("Google API")]
        [SerializeField] private TextToSpeechManager _textToSpeechManager;
        [SerializeField] private SpeechToTextManager _speechToTextManager;

        private void Awake()
        {
            _chatBot.OnChatBotResponse += OnChatBotResponse;
        }

        private void OnDestroy()
        {
            _chatBot.OnChatBotResponse -= OnChatBotResponse;
        }

        public async void SubmitTranscript(string transcript)
        {
            var llmResponse = await _chatBot.SubmitTranscript(transcript);
            GoogleApiRequestService.SendTextToSpeechRequest(
                text:       llmResponse,
                voice:      _textToSpeechManager.Voice,
                onSuccess:  _textToSpeechManager.OnRequestReceived
            );
        }

        private void OnChatBotResponse(string response)
        {
            _textToSpeechManager.SendTextToGoogle(response);
        }
    }
}