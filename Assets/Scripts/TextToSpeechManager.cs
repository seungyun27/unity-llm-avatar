using System;
using GoogleTextToSpeech.Scripts.Data;
using UnityEngine;
using UnityLLMAvatar.GoogleApi;
using UnityLLMAvatar.util;

namespace UnityLLMAvatar
{
    public class TextToSpeechManager : MonoBehaviour
    {
        [Header("Voice Settings")]
        public VoiceScriptableObject Voice;

        [Header("Avatar's AudioSource")]
        public AudioSource AvatarAudioSource;

        public async void SaveAndPlaySpeech(TextToSpeechResponse ttsResponse)
        {
            try
            {
                var filePath = await AudioHelper.SaveAsMP3(Convert.FromBase64String(ttsResponse.audioContent));
                var clip = await AudioHelper.LoadAudioClipFromMP3(filePath);
                
                AvatarAudioSource.Stop();
                AvatarAudioSource.clip = clip;
                AvatarAudioSource.Play();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}

