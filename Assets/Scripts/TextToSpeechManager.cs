using System;
using UnityEngine;
using UnityLLMAvatar.GoogleApi;
using UnityLLMAvatar.GoogleApi.DTO;
using UnityLLMAvatar.Utility;

namespace UnityLLMAvatar
{
    public class TextToSpeechManager : MonoBehaviour
    {
        [Header("Voice Settings")]
        public VoiceScriptableObject Voice;

        [Header("Avatar's AudioSource")]
        [Tooltip("Drag and drop the AudioSource component attached to the avatar here.")]
        public AudioSource AvatarAudioSource;

        public async void SaveAndPlaySpeech(TextToSpeechResponse ttsResponse)
        {
            try
            {
                var filePath = await AudioHelper.SaveAsMp3(Convert.FromBase64String(ttsResponse.audioContent));
                
                AudioClip clip = null;
                try
                {
                    clip = await AudioHelper.LoadAudioClipFromMp3(filePath);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
                
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

