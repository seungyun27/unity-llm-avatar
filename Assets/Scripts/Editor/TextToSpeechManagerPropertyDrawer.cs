using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UnityLLMAvatar.Editor
{
    [CustomPropertyDrawer((typeof(TextToSpeechManager)))]
    public class TextToSpeechManagerPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            
            var objectField = new ObjectField(property.displayName)
            {
                objectType = typeof(TextToSpeechManager),
            };
            objectField.BindProperty(property);
            root.Add(objectField);

            var voiceLabel = new RadioButton()
            {
                style =
                {
                    marginTop = 4,
                    paddingLeft = 16,
                },
                value = true,
                focusable = false,
            };
            root.Add(voiceLabel);

            objectField.RegisterValueChangedCallback(evt =>
            {
                var newValue = evt.newValue as TextToSpeechManager;
                var voice = newValue?.Voice;
                voiceLabel.text = $"Model voice: <b>{voice?.name ?? "None"}</b>";
            });
            
            var currentValue = property.objectReferenceValue as TextToSpeechManager;
            if (currentValue != null)
            {
                voiceLabel.text = $"Model voice: <b>{currentValue.Voice?.name ?? "None"}</b>";
            }

            return root;
        }
    }
}