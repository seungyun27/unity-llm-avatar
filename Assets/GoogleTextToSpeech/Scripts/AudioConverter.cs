using System;
using System.Collections;
using System.IO;
using GoogleTextToSpeech.Scripts.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace GoogleTextToSpeech.Scripts
{
    public class AudioConverter : MonoBehaviour
    {
        private static string Mp3FileName => "avatar-audio-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".mp3";

        public static void SaveTextToMp3(AudioData audioData)
        {
            var bytes = Convert.FromBase64String(audioData.audioContent);
            File.WriteAllBytes(Application.temporaryCachePath + "/" + Mp3FileName, bytes);
        }

        public static IEnumerator LoadClipFromMp3Cor(Action<AudioClip> onClipLoaded)
        {
            var downloadHandler =
                new DownloadHandlerAudioClip("file://" + Application.temporaryCachePath + "/" + Mp3FileName,
                    AudioType.MPEG);
            downloadHandler.compressed = false;

            using var webRequest = new UnityWebRequest("file://" + Application.temporaryCachePath + "/" + Mp3FileName,
                "GET",
                downloadHandler, null);

            yield return webRequest.SendWebRequest();

            if (webRequest.responseCode == 200)
            {
                onClipLoaded.Invoke(downloadHandler.audioClip);
            }
            
            downloadHandler.Dispose();
        }
    }
}