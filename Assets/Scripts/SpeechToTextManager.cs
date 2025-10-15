using UnityEngine;
using UnityEngine.UI;
using UnityLLMAvatar.GoogleApi;
using UnityLLMAvatar.util;

namespace UnityLLMAvatar
{
    public class SpeechToTextManager : MonoBehaviour
    {
        public AIManager AIManager;
        public Button RecordButton;
        public Button RecordStopButton;
        
        private AudioClip clip;
        private bool _isRecording;

        private void Awake()
        {
            RecordButton.interactable = false;
            RecordStopButton.interactable = false;
        }

        private void OnDisable()
        {
            RecordButton.onClick.RemoveListener(StartRecording);
            RecordStopButton.onClick.RemoveListener(StopRecording);
        }

        public void ChangeRecordButtonState()
        {
            RecordButton.interactable = true;
            RecordButton.onClick.AddListener(StartRecording);

            RecordStopButton.interactable = false;
            RecordStopButton.onClick.AddListener(StopRecording);
        }

        public void StartRecording()
        {
            if (_isRecording) return;

            RecordButton.interactable = false;
            RecordStopButton.interactable = true;
            clip = Microphone.Start(null, false, 10, 44100);
            _isRecording = true;
            
            Debug.Log("Recording started...");
        }
        
        public void StopRecording()
        {
            if (!_isRecording) return;
            
            RecordButton.interactable = false;
            RecordStopButton.interactable = false;
            
            int position = Microphone.GetPosition(null);
            Microphone.End(null);
            var rawAudio = clip.GetSamplesAsWAV(position);
            _isRecording = false;
            Debug.Log("Recording stopped.");
            
            AIManager.ProcessSpeech(rawAudio);
        }
    }
}