using UnityEngine;
using UnityLLMAvatar.Utility;

namespace UnityLLMAvatar.GoogleApi
{
    [CreateAssetMenu(fileName = "Voice", menuName = "GoogleTextToSpeech/Voice", order = 1)]
    public class VoiceScriptableObject : RuntimeScriptableObject
    {
        public string languageCode;
        public new string name;
        
        [Range(0.25f, 4f)]
        public float speed;

        [Range(-20f, 20f)]
        public float pitch;

        protected override void OnReset()
        {
            speed = 1f;
            pitch = 0f;
        }
    }
}
