using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LLMUnity;
using UnityEngine;
using UnityEngine.UI;
using UnityLLMAvatar.util;

namespace UnityLLMAvatar.LLM
{
    public class ChatBot : MonoBehaviour
    {
        #region Public Fields
        public LLMCharacter llmCharacter;
        public RectTransform chatContainer;
        public Color playerColor = new Color32(81, 164, 81, 255);
        public Color aiColor = new Color32(29, 29, 73, 255);
        public Color fontColor = Color.white;
        public Font font;
        public int fontSize = 16;
        public int bubbleWidth = 600;
        public float textPadding = 10f;
        public float bubbleSpacing = 10f;
        public Sprite sprite;
        public Button stopButton;
        #endregion

        #region LLM Response Callbacks

        public Action<string> OnResponseReceived;

        #endregion

        #region Private Fields
        private InputBubble inputBubble;
        private readonly List<Bubble> chatBubbles = new();
        private bool blockInput = true;
        private BubbleUI playerUI, aiUI;
        private bool warmUpDone = false;
        private int lastBubbleOutsideFOV = -1;
        #endregion

        #region Unity Event Functions
        private void Start()
        {
            InitializeUI();
            ShowLoadedMessages();
            // var modelName = llmCharacter.llm.model;
            // var temperature = llmCharacter.temperature;
            // var timestamp = System.DateTime.Now.ToString("yyyyMMdd-HHmmss");
            // var logDir = Path.Combine(Application.dataPath, "LLMTestResult~");
            // var logFile = $"{logDir}/{modelName}___temp-{temperature}___conveniencestore___{timestamp}";
            // llmCharacter.save = logFile;
            // print($"saving chat log to: {logFile}");
            InitializeLLM();
        }
        
        private void Update()
        {
            if (!inputBubble.inputFocused() && warmUpDone)
            {
                inputBubble.ActivateInputField();
                StartCoroutine(BlockInteraction());
            }
            if (lastBubbleOutsideFOV != -1)
            {
                // destroy bubbles outside the container
                for (int i = 0; i <= lastBubbleOutsideFOV; i++)
                {
                    chatBubbles[i].Destroy();
                }
                chatBubbles.RemoveRange(0, lastBubbleOutsideFOV + 1);
                lastBubbleOutsideFOV = -1;
            }
        }

        private bool onValidateWarning = true;

        private void OnValidate()
        {
            if (onValidateWarning && !llmCharacter.remote && llmCharacter.llm != null && llmCharacter.llm.model == "")
            {
                Debug.LogWarning($"Please select a model in the {llmCharacter.llm.gameObject.name} GameObject!");
                onValidateWarning = false;
            }
        }
        #endregion
        
        public async Task SendMessageAsync(string message)
        {
            if (blockInput || string.IsNullOrWhiteSpace(message))
            {
                Debug.LogWarning("Cannot send message: input is blocked or message is empty");
                return;
            }
            
            try
            {
                blockInput = true;
			
                // Normalize message
                var normalizedMessage = message.Replace("\v", "\n").Trim();
			
                // Display user message
                AddBubble(normalizedMessage, true);
			
                // Create AI bubble with loading indicator
                Bubble aiBubble = AddBubble("···", false);
			
                // Send to LLM
                var formattedMessage = $"<request>{normalizedMessage}</request>";
                var llmResponse = await llmCharacter.Chat(
                    query: formattedMessage, 
                    callback: aiBubble.SetThinkingText,
                    completionCallback: AllowInput);
			
                // Parse and display response
                var parsedResponse = XMLParser.ParseLLMResponse(llmResponse);
                Debug.Log(parsedResponse);
                aiBubble.SetText(parsedResponse.Answer);
			
                // Notify listeners
                OnResponseReceived?.Invoke(parsedResponse.Answer);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                blockInput = false;
            }
        }

        private async void OnKeyboardInput(string newText)
        {
            try
            {
                inputBubble.ActivateInputField();

                if (blockInput || newText.Trim() == "" || Input.GetKey(KeyCode.LeftShift) ||
                    Input.GetKey(KeyCode.RightShift))
                {
                    StartCoroutine(BlockInteraction());
                    return;
                }

                var message = inputBubble.GetText();
                inputBubble.SetText("");


                await SendMessageAsync(message);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        private void InitializeLLM()
        {
            llmCharacter.SetPrompt(SystemPrompts.ConvenienceStoreClerk);
            llmCharacter.grammarString = XMLGrammar.Grammar;

            _ = llmCharacter.Warmup(WarmUpCallback);
        }

        private void InitializeUI()
        {
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            playerUI = new BubbleUI
            {
                sprite = sprite,
                font = font,
                fontSize = fontSize,
                fontColor = fontColor,
                bubbleColor = playerColor,
                bottomPosition = 0,
                leftPosition = 0,
                textPadding = textPadding,
                bubbleOffset = bubbleSpacing,
                bubbleWidth = bubbleWidth,
                bubbleHeight = -1
            };
            aiUI = playerUI;
            aiUI.bubbleColor = aiColor;
            aiUI.leftPosition = 1;

            inputBubble = new InputBubble(chatContainer, playerUI, "InputBubble", "Loading...", 4);
            inputBubble.AddSubmitListener(OnKeyboardInput);
            inputBubble.AddValueChangedListener(OnValueChanged);
            inputBubble.setInteractable(false);
            stopButton.gameObject.SetActive(true);
        }

        private Bubble AddBubble(string message, bool isPlayerMessage)
        {
            Bubble bubble = new Bubble(chatContainer, isPlayerMessage ? playerUI : aiUI, isPlayerMessage ? "PlayerBubble" : "AIBubble", message);
            chatBubbles.Add(bubble);
            bubble.OnResize(UpdateBubblePositions);
            return bubble;
        }

        private void ShowLoadedMessages()
        {
            for (int i = 1; i < llmCharacter.chat.Count; i++) AddBubble(llmCharacter.chat[i].content, i % 2 == 1);
        }
        
        private async void WarmUpCallback()
        {
            try
            {
                warmUpDone = true;

                // After warmup, greet the user first
                var message = "Hello!";
                // AddBubble(message, true);
                Bubble aiBubble = AddBubble("⋯", false);

                message = $"<request>{message}</request>";
                string firstResponse = await llmCharacter.Chat(message, aiBubble.SetThinkingText, AllowInput);

                var parsedResponse = XMLParser.ParseLLMResponse(firstResponse);
                Debug.Log(parsedResponse);
                aiBubble.SetText(parsedResponse.Answer);

                OnResponseReceived?.Invoke(parsedResponse.Answer);

                inputBubble.SetPlaceHolderText("Message me");
                AllowInput();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void AllowInput()
        {
            blockInput = false;
            inputBubble.ReActivateInputField();
        }

        private IEnumerator<string> BlockInteraction()
        {
            // prevent from change until next frame
            inputBubble.setInteractable(false);
            yield return null;
            inputBubble.setInteractable(true);
            // change the caret position to the end of the text
            inputBubble.MoveTextEnd();
        }

        private void OnValueChanged(string newText)
        {
            // Get rid of newline character added when we press enter
            if (Input.GetKey(KeyCode.Return))
            {
                if (inputBubble.GetText().Trim() == "")
                    inputBubble.SetText("");
            }
        }

        private void UpdateBubblePositions()
        {
            float y = inputBubble.GetSize().y + inputBubble.GetRectTransform().offsetMin.y + bubbleSpacing;
            // float containerHeight = chatContainer.GetComponent<RectTransform>().rect.height;
            float containerHeight = chatContainer.rect.height;
            for (int i = chatBubbles.Count - 1; i >= 0; i--)
            {
                Bubble bubble = chatBubbles[i];
                RectTransform childRect = bubble.GetRectTransform();
                childRect.anchoredPosition = new Vector2(childRect.anchoredPosition.x, y);

                // last bubble outside the container
                if (y > containerHeight && lastBubbleOutsideFOV == -1)
                {
                    lastBubbleOutsideFOV = i;
                }
                y += bubble.GetSize().y + bubbleSpacing;
            }
        }

        public void CancelRequests()
        {
            llmCharacter.CancelRequests();
            AllowInput();
        }
        
        public void ExitGame()
        {
            Debug.Log("Exit button clicked");
            Application.Quit();
        }
    }
}
