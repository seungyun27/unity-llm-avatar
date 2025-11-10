using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LLMUnity;
using UnityEngine;
using UnityEngine.UI;
using UnityLLMAvatar.Utility;

namespace UnityLLMAvatar.LLM
{
    public class ChatBot : MonoBehaviour
    {
        #region Public Fields
        public LLMCharacter LLMCharacter;
        public RectTransform ChatContainer;
        public Color PlayerColor = new Color32(81, 164, 81, 255);
        public Color AIColor = new Color32(29, 29, 73, 255);
        public Color FontColor = Color.white;
        public Font Font;
        public int FontSize = 16;
        public int BubbleWidth = 600;
        public float TextPadding = 10f;
        public float BubbleSpacing = 10f;
        public Sprite Sprite;
        public Button StopButton;
        #endregion

        #region LLM Response Callbacks

        public Action<string> OnResponseReceived;

        #endregion

        #region Private Fields
        private InputBubble _inputBubble;
        private readonly List<Bubble> _chatBubbles = new();
        private bool _blockInput = true;
        private BubbleUI _playerUI, _aiUI;
        private bool _warmUpDone;
        private int _lastBubbleOutsideFOV = -1;
        #endregion

        #region Unity Event Functions
        private void Start()
        {
            InitializeUI();
            ShowLoadedMessages();
            var modelName = LLMCharacter.llm.model;
            var temperature = LLMCharacter.temperature;
            var timestamp = System.DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var logDir = Path.Combine(Application.dataPath, "LLMTestResult~");
            var logFile = $"{logDir}/{modelName}___temp-{temperature}___conveniencestore___{timestamp}";
            LLMCharacter.save = logFile;
            print($"saving chat log to: {logFile}");
            InitializeLlm();
        }
        
        private void Update()
        {
            if (!_inputBubble.inputFocused() && _warmUpDone)
            {
                _inputBubble.ActivateInputField();
                StartCoroutine(BlockInteraction());
            }
            if (_lastBubbleOutsideFOV != -1)
            {
                // destroy bubbles outside the container
                for (int i = 0; i <= _lastBubbleOutsideFOV; i++)
                {
                    _chatBubbles[i].Destroy();
                }
                _chatBubbles.RemoveRange(0, _lastBubbleOutsideFOV + 1);
                _lastBubbleOutsideFOV = -1;
            }
        }

        private bool onValidateWarning = true;

        private void OnValidate()
        {
            if (onValidateWarning && !LLMCharacter.remote && LLMCharacter.llm != null && LLMCharacter.llm.model == "")
            {
                Debug.LogWarning($"Please select a model in the {LLMCharacter.llm.gameObject.name} GameObject!");
                onValidateWarning = false;
            }
        }
        #endregion
        
        public async Task SendMessageAsync(string message)
        {
            if (_blockInput || string.IsNullOrWhiteSpace(message))
            {
                Debug.LogWarning("Cannot send message: input is blocked or message is empty");
                return;
            }
            
            try
            {
                _blockInput = true;
			
                // Normalize message
                var normalizedMessage = message.Replace("\v", "\n").Trim();
			
                // Display user message
                AddBubble(normalizedMessage, true);
			
                // Create AI bubble with loading indicator
                Bubble aiBubble = AddBubble("···", false);

                // Send to LLM
                var formattedMessage = $"<request>{normalizedMessage}</request>";
                Debug.Log($"[LLM_REQUEST]{formattedMessage}");

                var llmResponse = await LLMCharacter.Chat(
                    query: formattedMessage,
                    callback: aiBubble.SetThinkingText,
                    completionCallback: AllowInput);
			
                // Parse and display response
                Debug.Log($"[LLM_RESPONSE]{llmResponse}");
                var parsedResponse = XMLParser.ParseLLMResponse(llmResponse);
                // Debug.Log(parsedResponse);
                aiBubble.SetText(parsedResponse.Answer);
			
                // Notify listeners
                OnResponseReceived?.Invoke(parsedResponse.Answer);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                _blockInput = false;
            }
        }

        private async void OnKeyboardInput(string newText)
        {
            try
            {
                _inputBubble.ActivateInputField();

                if (_blockInput || newText.Trim() == "" || Input.GetKey(KeyCode.LeftShift) ||
                    Input.GetKey(KeyCode.RightShift))
                {
                    StartCoroutine(BlockInteraction());
                    return;
                }

                var message = _inputBubble.GetText();
                _inputBubble.SetText("");

                await SendMessageAsync(message);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        private void InitializeLlm()
        {
            LLMCharacter.SetPrompt(SystemPrompts.ConvenienceStoreClerk);
            LLMCharacter.grammarString = XMLGrammar.Grammar;

            _ = LLMCharacter.Warmup(WarmUpCallback);
        }

        private void InitializeUI()
        {
            if (Font == null) Font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _playerUI = new BubbleUI
            {
                sprite = Sprite,
                font = Font,
                fontSize = FontSize,
                fontColor = FontColor,
                bubbleColor = PlayerColor,
                bottomPosition = 0,
                leftPosition = 0,
                textPadding = TextPadding,
                bubbleOffset = BubbleSpacing,
                bubbleWidth = BubbleWidth,
                bubbleHeight = -1
            };
            _aiUI = _playerUI;
            _aiUI.bubbleColor = AIColor;
            _aiUI.leftPosition = 1;

            _inputBubble = new InputBubble(ChatContainer, _playerUI, "InputBubble", "Loading...", 4);
            _inputBubble.AddSubmitListener(OnKeyboardInput);
            _inputBubble.AddValueChangedListener(OnValueChanged);
            _inputBubble.setInteractable(false);
            StopButton.gameObject.SetActive(true);
        }

        private Bubble AddBubble(string message, bool isPlayerMessage)
        {
            Bubble bubble = new Bubble(ChatContainer, isPlayerMessage ? _playerUI : _aiUI, isPlayerMessage ? "PlayerBubble" : "AIBubble", message);
            _chatBubbles.Add(bubble);
            bubble.OnResize(UpdateBubblePositions);
            return bubble;
        }

        private void ShowLoadedMessages()
        {
            for (int i = 1; i < LLMCharacter.chat.Count; i++) AddBubble(LLMCharacter.chat[i].content, i % 2 == 1);
        }
        
        private async void WarmUpCallback()
        {
            try
            {
                _warmUpDone = true;

                // After warmup, greet the user first
                var message = "Hello!";
                Debug.Log($"[LLM_REQUEST]{message}");
                // AddBubble(message, true);
                Bubble aiBubble = AddBubble("⋯", false);

                message = $"<request>{message}</request>";
                string firstResponse = await LLMCharacter.Chat(message, aiBubble.SetThinkingText, AllowInput);
                Debug.Log($"[LLM_RESPONSE]{firstResponse}");

                var parsedResponse = XMLParser.ParseLLMResponse(firstResponse);
                Debug.Log(parsedResponse);
                aiBubble.SetText(parsedResponse.Answer);

                OnResponseReceived?.Invoke(parsedResponse.Answer);

                _inputBubble.SetPlaceHolderText("Message me");
                AllowInput();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void AllowInput()
        {
            _blockInput = false;
            _inputBubble.ReActivateInputField();
        }

        private IEnumerator<string> BlockInteraction()
        {
            // prevent from change until next frame
            _inputBubble.setInteractable(false);
            yield return null;
            _inputBubble.setInteractable(true);
            // change the caret position to the end of the text
            _inputBubble.MoveTextEnd();
        }

        private void OnValueChanged(string newText)
        {
            // Get rid of newline character added when we press enter
            if (Input.GetKey(KeyCode.Return))
            {
                if (_inputBubble.GetText().Trim() == "")
                    _inputBubble.SetText("");
            }
        }

        private void UpdateBubblePositions()
        {
            float y = _inputBubble.GetSize().y + _inputBubble.GetRectTransform().offsetMin.y + BubbleSpacing;
            // float containerHeight = chatContainer.GetComponent<RectTransform>().rect.height;
            float containerHeight = ChatContainer.rect.height;
            for (int i = _chatBubbles.Count - 1; i >= 0; i--)
            {
                Bubble bubble = _chatBubbles[i];
                RectTransform childRect = bubble.GetRectTransform();
                childRect.anchoredPosition = new Vector2(childRect.anchoredPosition.x, y);

                // last bubble outside the container
                if (y > containerHeight && _lastBubbleOutsideFOV == -1)
                {
                    _lastBubbleOutsideFOV = i;
                }
                y += bubble.GetSize().y + BubbleSpacing;
            }
        }

        public void CancelRequests()
        {
            LLMCharacter.CancelRequests();
            AllowInput();
        }
        
        public void ExitGame()
        {
            Debug.Log("Exit button clicked");
            Application.Quit();
        }
    }
}
