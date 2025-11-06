using System;
using UnityEngine;
using UnityEngine.UI;
using UnityLLMAvatar.Utility;

namespace UnityLLMAvatar
{
    public class SpeechToTextManager : MonoBehaviour
    {        
        [SerializeField] 
        private Button _recordButton;
        
        [SerializeField] 
        private Button _recordStopButton;
        
        private AudioClip clip;
        private bool _isRecording;

        public Action<byte[]> OnSpeechRecorded = delegate { };

        #region Event Function

        private void Awake()
        {
            _recordButton.interactable = false;
            _recordStopButton.interactable = false;
        }
        
        private void OnEnable()
        {
            _recordButton.onClick.AddListener(StartRecording);
            _recordStopButton.onClick.AddListener(StopRecording);
        }

        private void OnDisable()
        {
            _recordButton.onClick.RemoveListener(StartRecording);
            _recordStopButton.onClick.RemoveListener(StopRecording);
        }

        #endregion
        
        public void ChangeRecordButtonState()
        {
            _recordButton.interactable = true;
            _recordButton.onClick.AddListener(StartRecording);

            _recordStopButton.interactable = false;
            _recordStopButton.onClick.AddListener(StopRecording);
        }

        public void StartRecording()
        {
            if (_isRecording) return;

            _recordButton.interactable = false;
            _recordStopButton.interactable = true;
            clip = Microphone.Start(null, false, 10, 44100);
            _isRecording = true;
            
            Debug.Log("Recording started...");
        }
        
        public void StopRecording()
        {
            if (!_isRecording) return;
            
            _recordButton.interactable = false;
            _recordStopButton.interactable = false;
            
            int position = Microphone.GetPosition(null);
            Microphone.End(null);
            var rawAudio = clip.GetSamplesAsWav(position);
            _isRecording = false;
            Debug.Log("Recording stopped.");
            
            OnSpeechRecorded?.Invoke(rawAudio);
        }
    }
}