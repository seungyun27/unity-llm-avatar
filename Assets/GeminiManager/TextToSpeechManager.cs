using UnityEngine;
using GoogleTextToSpeech.Scripts.Data;
using UnityLLMAvatar;

namespace GoogleTextToSpeech.Scripts
{
    public class TextToSpeechManager : MonoBehaviour
    {
        [Header("Voice Settings")]
        public VoiceScriptableObject Voice;

        [Header("TTS AudioSource")]
        public AudioSource TTSAudioSource;

        public void OnRequestReceived(string requestData)
        {
            AudioConverter.SaveTextToMp3(JsonUtility.FromJson<AudioData>(requestData));
            StartCoroutine(AudioConverter.LoadClipFromMp3Cor(OnClipLoaded));
        }

        public void OnClipLoaded(AudioClip clip)
        {
            TTSAudioSource.Stop();
            TTSAudioSource.clip = clip;
            TTSAudioSource.Play();
        }

        public void SendTextToGoogle(string text)
        {
            RequestService.SendDataToGoogle(
                url:        "https://texttospeech.googleapis.com/v1/text:synthesize",
                apiKey:     EnvManager.GetApiKey("TTS_API_KEY"),
                payload:    DataToSend.MakeInstance(text, Voice),
                onSuccess:  requestData => OnRequestReceived(requestData),
                onError:    badRequestData => Debug.LogError($"Error {badRequestData.error.code} : {badRequestData.error.message}")
            );
        }
    }
}

