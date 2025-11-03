using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleTextToSpeech.Scripts.Data;
using TMPro;
using System;
using ReadyPlayerAvatar = ReadyPlayerMe.Core;

namespace GoogleTextToSpeech.Scripts
{
    public class TextToSpeechManager : MonoBehaviour
    {
        [SerializeField] private VoiceScriptableObject voice;
        [SerializeField] private TextToSpeech text_to_speech;

        private Action<AudioClip> _audioClipReceived;
        private Action<BadRequestData> _errorReceived;
        [Header("TTS Àç»ý¿ë AudioSource")]
        public AudioSource ttsSource;
        void Start()
        {
            SendTextToGoogle("Wow, Nice to meet you! Hi there, how are you doing today? I hope you're having a great week so far. It's been a while, I hope everything is going well with you.");
        }
        public void SendTextToGoogle(string _text)
        {
            _errorReceived += ErrorReceived;
            _audioClipReceived += AudioClipReceived;
            text_to_speech.GetSpeechAudioFromGoogle(_text, voice, _audioClipReceived, _errorReceived);
            
        }

        private void ErrorReceived(BadRequestData badRequestData)
        {
            Debug.Log($"Error {badRequestData.error.code} : {badRequestData.error.message}");
        }

        private void AudioClipReceived(AudioClip clip)
        {
            ttsSource.Stop();
            ttsSource.clip = clip;
            ttsSource.Play();
        }
    }
}

