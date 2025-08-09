using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using LifeCraft.Systems;

namespace LifeCraft.UI
{
    /// <summary>
    /// AI Assistant Modal - Chat interface for AI interactions.
    /// 
    /// DESIGN PHILOSOPHY:
    /// - Follows existing modal patterns (RewardModal, PurchaseConfirmModal)
    /// - Modern chat interface with smooth animations
    /// - Responsive design matching existing UI
    /// - Real-time message display with typing indicators
    /// - Integration with existing UI management system
    /// </summary>
    public class AIAssistantModal : MonoBehaviour
    {
        #region UI References
        [Header("Modal Structure")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform modalPanel;
        
        [Header("Chat Interface")]
        [SerializeField] private ScrollRect chatScrollRect;
        [SerializeField] private Transform chatContentContainer;
        [SerializeField] private GameObject messagePrefab;
        [SerializeField] private GameObject userMessagePrefab;
        [SerializeField] private GameObject aiMessagePrefab;
        
        [Header("Input System")]
        [SerializeField] private TMP_InputField messageInput;
        [SerializeField] private Button sendButton;
        [SerializeField] private Button clearButton;
        [SerializeField] private Button closeButton;
        
        [Header("Status Indicators")]
        [SerializeField] private GameObject typingIndicator;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Image sendButtonImage;
        
        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.2f;
        [SerializeField] private float messageAnimationDelay = 0.1f;
        #endregion

        #region Private Fields
        private AIAssistantManager aiManager;
        private List<GameObject> messageObjects = new List<GameObject>();
        private bool isTyping = false;
        private Coroutine typingCoroutine;
        #endregion

        #region Initialization
        private void Awake()
        {
            // REASONING: Auto-find AI manager if not assigned, following existing patterns
            if (aiManager == null)
                aiManager = FindFirstObjectByType<AIAssistantManager>();
            
            SetupEventListeners();
            Hide(); // Start hidden
        }

        private void Start()
        {
            // Subscribe to AI manager events
            if (aiManager != null)
            {
                aiManager.OnAIResponseReceived += OnAIResponseReceived;
                aiManager.OnAIErrorOccurred += OnAIErrorOccurred;
                aiManager.OnAIProcessingStateChanged += OnAIProcessingStateChanged;
            }
        }

        private void OnDestroy()
        {
            // Clean up event subscriptions to prevent memory leaks
            if (aiManager != null)
            {
                aiManager.OnAIResponseReceived -= OnAIResponseReceived;
                aiManager.OnAIErrorOccurred -= OnAIErrorOccurred;
                aiManager.OnAIProcessingStateChanged -= OnAIProcessingStateChanged;
            }
        }

        private void SetupEventListeners()
        {
            // REASONING: Following existing modal patterns for consistency
            if (sendButton != null)
                sendButton.onClick.AddListener(OnSendButtonClicked);
            
            if (clearButton != null)
                clearButton.onClick.AddListener(OnClearButtonClicked);
            
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);
            
            if (messageInput != null)
            {
                messageInput.onSubmit.AddListener(OnMessageSubmitted);
                messageInput.onValueChanged.AddListener(OnMessageInputChanged);
            }
        }
        #endregion

        #region Public API
        /// <summary>
        /// Show the AI assistant modal with fade-in animation.
        /// REASONING: Smooth animations improve user experience
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            LoadChatHistory();
            StartCoroutine(FadeIn());
            
            // Focus on input field for immediate typing
            if (messageInput != null)
            {
                messageInput.Select();
                messageInput.ActivateInputField();
            }
        }

        /// <summary>
        /// Hide the AI assistant modal with fade-out animation.
        /// REASONING: Consistent with existing modal behavior
        /// </summary>
        public void Hide()
        {
            StartCoroutine(FadeOut());
        }

        /// <summary>
        /// Clear all chat messages and history.
        /// REASONING: Privacy feature and performance management
        /// </summary>
        public void ClearChat()
        {
            if (aiManager != null)
            {
                aiManager.ClearChatHistory();
            }
            
            ClearMessageObjects();
            UpdateStatusText("Chat cleared");
        }
        #endregion

        #region Event Handlers
        private void OnSendButtonClicked()
        {
            SendMessage();
        }

        private void OnMessageSubmitted(string message)
        {
            SendMessage();
        }

        private void OnMessageInputChanged(string text)
        {
            // REASONING: Enable/disable send button based on input content
            bool hasText = !string.IsNullOrWhiteSpace(text);
            if (sendButton != null)
            {
                sendButton.interactable = hasText && !isTyping;
            }
        }

        private void OnClearButtonClicked()
        {
            // REASONING: Confirmation dialog for destructive action
            ShowClearConfirmation();
        }

        private void OnAIResponseReceived(string response)
        {
            // Add AI response to chat
            AddMessage(response, false);
            StopTypingIndicator();
        }

        private void OnAIErrorOccurred(string error)
        {
            // Show error message in chat
            AddMessage($"Error: {error}", false);
            StopTypingIndicator();
            UpdateStatusText("Connection error");
        }

        private void OnAIProcessingStateChanged(bool isProcessing)
        {
            isTyping = isProcessing;
            
            if (isProcessing)
            {
                StartTypingIndicator();
                UpdateStatusText("AI is thinking...");
            }
            else
            {
                StopTypingIndicator();
                UpdateStatusText("Ready");
            }
            
            // Update send button state
            if (sendButton != null)
            {
                sendButton.interactable = !isProcessing && !string.IsNullOrWhiteSpace(messageInput?.text);
            }
        }
        #endregion

        #region Message Handling
        /// <summary>
        /// Send the current message to the AI assistant.
        /// REASONING: Centralized message sending with validation
        /// </summary>
        private async void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(messageInput?.text) || isTyping)
                return;

            string message = messageInput.text.Trim();
            
            // Add user message to chat immediately
            AddMessage(message, true);
            
            // Clear input field
            messageInput.text = "";
            
            // Send to AI manager
            if (aiManager != null)
            {
                await aiManager.SendAIMessage(message);
            }
        }

        /// <summary>
        /// Add a message to the chat display.
        /// REASONING: Visual feedback for immediate user interaction
        /// </summary>
        private void AddMessage(string content, bool isFromUser)
        {
            GameObject messageObj = CreateMessageObject(content, isFromUser);
            if (messageObj != null)
            {
                messageObjects.Add(messageObj);
                StartCoroutine(AnimateMessageIn(messageObj));
                ScrollToBottom();
            }
        }

        /// <summary>
        /// Create a message object with appropriate styling.
        /// REASONING: Different styling for user vs AI messages
        /// </summary>
        private GameObject CreateMessageObject(string content, bool isFromUser)
        {
            GameObject prefab = isFromUser ? userMessagePrefab : aiMessagePrefab;
            if (prefab == null)
            {
                prefab = messagePrefab; // Fallback to generic message prefab
            }

            if (prefab == null)
            {
                Debug.LogError("No message prefab assigned to AI Assistant Modal");
                return null;
            }

            GameObject messageObj = Instantiate(prefab, chatContentContainer);
            var messageUI = messageObj.GetComponent<AIChatMessageUI>();
            
            if (messageUI != null)
            {
                messageUI.Initialize(content, isFromUser);
            }
            else
            {
                // Fallback for basic text display
                var textComponent = messageObj.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = content;
                }
            }

            return messageObj;
        }

        /// <summary>
        /// Load and display existing chat history.
        /// REASONING: Maintains conversation continuity across sessions
        /// </summary>
        private void LoadChatHistory()
        {
            ClearMessageObjects();
            
            if (aiManager != null)
            {
                var history = aiManager.GetChatHistory();
                foreach (var message in history)
                {
                    AddMessage(message.content, message.isFromUser);
                }
            }
        }

        /// <summary>
        /// Clear all message objects from the display.
        /// REASONING: Memory management and UI cleanup
        /// </summary>
        private void ClearMessageObjects()
        {
            foreach (var obj in messageObjects)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            messageObjects.Clear();
        }
        #endregion

        #region UI Animations
        /// <summary>
        /// Fade in animation for modal appearance.
        /// REASONING: Smooth transitions improve perceived performance
        /// </summary>
        private IEnumerator FadeIn()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = true;
                
                float elapsed = 0f;
                while (elapsed < fadeInDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                    yield return null;
                }
                
                canvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// Fade out animation for modal disappearance.
        /// REASONING: Consistent with fade-in behavior
        /// </summary>
        private IEnumerator FadeOut()
        {
            if (canvasGroup != null)
            {
                float elapsed = 0f;
                while (elapsed < fadeOutDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
                    yield return null;
                }
                
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
            }
            
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Animate message appearance with slight delay.
        /// REASONING: Creates natural conversation flow
        /// </summary>
        private IEnumerator AnimateMessageIn(GameObject messageObj)
        {
            if (messageObj == null) yield break;
            
            // Start with zero scale
            messageObj.transform.localScale = Vector3.zero;
            
            yield return new WaitForSeconds(messageAnimationDelay);
            
            // Animate to full scale
            float duration = 0.2f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / duration;
                messageObj.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, progress);
                yield return null;
            }
            
            messageObj.transform.localScale = Vector3.one;
        }
        #endregion

        #region UI Updates
        /// <summary>
        /// Start typing indicator animation.
        /// REASONING: Visual feedback that AI is processing
        /// </summary>
        private void StartTypingIndicator()
        {
            if (typingIndicator != null)
            {
                typingIndicator.SetActive(true);
                typingCoroutine = StartCoroutine(AnimateTypingIndicator());
            }
        }

        /// <summary>
        /// Stop typing indicator animation.
        /// REASONING: Clear indication that AI has finished processing
        /// </summary>
        private void StopTypingIndicator()
        {
            if (typingIndicator != null)
            {
                typingIndicator.SetActive(false);
            }
            
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }
        }

        /// <summary>
        /// Animate typing indicator dots.
        /// REASONING: Engaging visual feedback during AI processing
        /// </summary>
        private IEnumerator AnimateTypingIndicator()
        {
            if (typingIndicator == null) yield break;
            
            var dots = typingIndicator.GetComponentsInChildren<TextMeshProUGUI>();
            string[] dotStates = { ".", "..", "..." };
            int currentState = 0;
            
            while (true)
            {
                if (dots.Length > 0)
                {
                    dots[0].text = dotStates[currentState];
                }
                
                currentState = (currentState + 1) % dotStates.Length;
                yield return new WaitForSeconds(0.5f);
            }
        }

        /// <summary>
        /// Update status text display.
        /// REASONING: Keep user informed of current state
        /// </summary>
        private void UpdateStatusText(string status)
        {
            if (statusText != null)
            {
                statusText.text = status;
            }
        }

        /// <summary>
        /// Scroll chat to bottom to show latest messages.
        /// REASONING: Ensure new messages are visible
        /// </summary>
        private void ScrollToBottom()
        {
            if (chatScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                chatScrollRect.verticalNormalizedPosition = 0f;
            }
        }
        #endregion

        #region Confirmation Dialogs
        /// <summary>
        /// Show confirmation dialog for clearing chat.
        /// REASONING: Prevent accidental data loss
        /// </summary>
        private void ShowClearConfirmation()
        {
            // REASONING: Use existing confirmation modal system
            var confirmModals = FindObjectsByType<PurchaseConfirmModal>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            PurchaseConfirmModal confirmModal = null;
            
            if (confirmModals.Length > 0)
            {
                confirmModal = confirmModals[0];
            }
            
            if (confirmModal != null)
            {
                confirmModal.Show(
                    "Clear Chat History?",
                    () => ClearChat(),
                    null
                );
            }
            else
            {
                // Fallback to direct clear
                ClearChat();
            }
        }
        #endregion
    }
}