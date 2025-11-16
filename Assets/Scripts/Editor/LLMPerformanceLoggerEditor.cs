using UnityEditor;
using UnityEngine;
using UnityLLMAvatar.GoogleApi;

namespace UnityLLMAvatar.Editor
{
    [CustomEditor(typeof(LLMPerformanceLogger))]
    public class LLMPerformanceLoggerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            var component = (LLMPerformanceLogger)target;
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            EditorGUILayout.LabelField("TTS Script for Testing", EditorStyles.boldLabel);
            var style = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true
            };
            component.TTS_script = EditorGUILayout.TextArea(
                component.TTS_script,
                style,
                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.Height(80)
            );
            if (GUILayout.Button("Test TTS API"))
            {
                TestTTSAPI(component);
            }
            GUILayout.EndVertical();
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(component);
            }
        }
        
        private async void TestTTSAPI(LLMPerformanceLogger component)
        {
            try
            {
                await GoogleApiRequestService.SendTextToSpeechRequestAsync(
                    component.TTS_script,
                    component.TTS_voice);
                Debug.Log("TTS API test successful.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"TTS API test failed: {e.Message}");
            }
        }
    }
}