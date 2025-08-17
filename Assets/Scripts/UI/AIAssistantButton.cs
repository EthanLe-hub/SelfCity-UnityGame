using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LifeCraft.Systems;

namespace LifeCraft.UI
{
    /// <summary>
    /// AI Assistant Button - Integration with existing bottom navigation.
    /// 
    /// DESIGN PHILOSOPHY:
    /// - Follows existing button patterns (InventoryButton, etc.)
    /// - Integrates with existing bottom navigation system
    /// - Provides visual feedback for AI availability
    /// - Consistent with existing UI styling
    /// - Easy integration with UIManager
    /// </summary>
    public class AIAssistantButton : MonoBehaviour
    {
        #region UI References
        [Header("Button Components")]
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField] private Image buttonIcon;
        
        [Header("Visual Feedback")]
        [SerializeField] private Image notificationBadge;
        [SerializeField] private TextMeshProUGUI notificationCount;
        [SerializeField] private GameObject pulseEffect;
        
        [Header("AI Integration")]
        [SerializeField] private AIAssistantManager aiManager;
        [SerializeField] private AIAssistantModal aiModal;
        
        [Header("Settings")]
        [SerializeField] private string buttonTextLabel = "AI Agent";
        [SerializeField] private Color activeColor = new Color(0.2f, 0.6f, 1f, 1f);
        [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        [SerializeField] private bool showNotificationBadge = true;
        #endregion

        #region Private Fields
        private bool isAIAvailable = false;
        private int unreadMessages = 0;
        private bool isInitialized = false;
        #endregion

        #region Initialization
        private void Start()
        {
            InitializeButton();
            SetupEventListeners();
            CheckAIAvailability();
        }

        private void OnDestroy()
        {
            // Clean up event subscriptions
            if (aiManager != null)
            {
                aiManager.OnAIResponseReceived -= OnAIResponseReceived;
                aiManager.OnAIErrorOccurred -= OnAIErrorOccurred;
            }
        }

        private void InitializeButton()
        {
            // REASONING: Auto-find components if not assigned, following existing patterns
            if (button == null)
                button = GetComponent<Button>();
            
            if (buttonText == null)
                buttonText = GetComponentInChildren<TextMeshProUGUI>();
            
            if (aiManager == null)
                aiManager = FindFirstObjectByType<AIAssistantManager>();
            
            if (aiModal == null)
                aiModal = FindFirstObjectByType<AIAssistantModal>();

            // Set initial button text
            if (buttonText != null)
            {
                buttonText.text = buttonTextLabel;
            }

            isInitialized = true;
        }

        private void SetupEventListeners()
        {
            // REASONING: Following existing button patterns for consistency
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClicked);
            }

            // Subscribe to AI manager events
            if (aiManager != null)
            {
                aiManager.OnAIResponseReceived += OnAIResponseReceived;
                aiManager.OnAIErrorOccurred += OnAIErrorOccurred;
            }
        }
        #endregion

        #region Public API
        /// <summary>
        /// Show the AI assistant modal.
        /// REASONING: Centralized access point for AI functionality
        /// </summary>
        public void ShowAIAssistant()
        {
            if (aiModal != null)
            {
                aiModal.Show();
                ClearNotifications();
            }
            else
            {
                Debug.LogWarning("AI Modal not assigned to AI Assistant Button");
            }
        }

        /// <summary>
        /// Set the notification count for unread messages.
        /// REASONING: Visual feedback for new AI responses
        /// </summary>
        public void SetNotificationCount(int count)
        {
            unreadMessages = Mathf.Max(0, count);
            UpdateNotificationDisplay();
        }

        /// <summary>
        /// Clear all notifications.
        /// REASONING: Reset notification state when user interacts
        /// </summary>
        public void ClearNotifications()
        {
            unreadMessages = 0;
            UpdateNotificationDisplay();
        }

        /// <summary>
        /// Set AI availability status.
        /// REASONING: Visual feedback for AI service status
        /// </summary>
        public void SetAIAvailability(bool available)
        {
            isAIAvailable = available;
            UpdateButtonState();
        }
        #endregion

        #region Event Handlers
        private void OnButtonClicked()
        {
            // REASONING: Check AI availability before showing modal
            if (isAIAvailable)
            {
                ShowAIAssistant();
            }
            else
            {
                ShowAIOfflineMessage();
            }
        }

        private void OnAIResponseReceived(string response)
        {
            // Increment notification count for new AI responses
            if (!aiModal.gameObject.activeInHierarchy)
            {
                unreadMessages++;
                UpdateNotificationDisplay();
            }
        }

        private void OnAIErrorOccurred(string error)
        {
            // Handle AI errors gracefully
            Debug.LogWarning($"AI Assistant Error: {error}");
            
            // Optionally show error notification
            if (notificationBadge != null)
            {
                notificationBadge.color = Color.red;
            }
        }
        #endregion

        #region UI Updates
        /// <summary>
        /// Update button visual state based on AI availability.
        /// REASONING: Clear visual feedback for service status
        /// </summary>
        private void UpdateButtonState()
        {
            if (!isInitialized) return;

            Color targetColor = isAIAvailable ? activeColor : inactiveColor;
            
            if (button != null)
            {
                button.interactable = isAIAvailable;
            }

            if (buttonText != null)
            {
                buttonText.color = targetColor;
            }

            if (buttonIcon != null)
            {
                buttonIcon.color = targetColor;
            }

            // Update text to indicate status
            if (buttonText != null)
            {
                buttonText.text = isAIAvailable ? buttonTextLabel : $"{buttonTextLabel} (Offline)";
            }
        }

        /// <summary>
        /// Update notification badge display.
        /// REASONING: Visual feedback for unread messages
        /// </summary>
        private void UpdateNotificationDisplay()
        {
            if (!showNotificationBadge) return;

            if (notificationBadge != null)
            {
                notificationBadge.gameObject.SetActive(unreadMessages > 0);
            }

            if (notificationCount != null)
            {
                notificationCount.text = unreadMessages.ToString();
                notificationCount.gameObject.SetActive(unreadMessages > 0);
            }

            // Pulse effect for new notifications
            if (pulseEffect != null)
            {
                pulseEffect.SetActive(unreadMessages > 0);
            }
        }
        #endregion

        #region AI Availability Check
        /// <summary>
        /// Check if AI service is available.
        /// REASONING: Determine if AI functionality should be enabled
        /// </summary>
        private void CheckAIAvailability()
        {
            if (aiManager == null)
            {
                SetAIAvailability(false);
                return;
            }

            // REASONING: Check AI configuration and network connectivity
            // In a real implementation, you might want to ping the AI service
            // For now, we'll assume it's available if the manager exists
            
            bool hasValidConfig = CheckAIConfiguration();
            bool hasNetworkConnection = CheckNetworkConnection();
            
            SetAIAvailability(hasValidConfig && hasNetworkConnection);
        }

        /// <summary>
        /// Check if AI configuration is valid.
        /// REASONING: Ensure API keys and settings are properly configured
        /// </summary>
        private bool CheckAIConfiguration()
        {
            // This would check if the AI configuration is properly set up
            // For now, return true if the manager exists
            return aiManager != null;
        }

        /// <summary>
        /// Check network connectivity.
        /// REASONING: AI service requires internet connection
        /// </summary>
        private bool CheckNetworkConnection()
        {
            // REASONING: Simple network check - in production, you might want
            // to actually ping the AI service endpoint
            
            // For now, assume network is available
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
        #endregion

        #region Error Handling
        /// <summary>
        /// Show message when AI is offline.
        /// REASONING: Inform user when AI service is unavailable
        /// </summary>
        private void ShowAIOfflineMessage()
        {
            // REASONING: Use existing notification system or modal
            var notificationPanel = FindFirstObjectByType<UIManager>();
            if (notificationPanel != null)
            {
                // Show notification that AI is offline
                Debug.Log("AI Assistant is currently offline. Please check your internet connection.");
            }
        }
        #endregion

        #region Integration with UIManager
        /// <summary>
        /// Register this button with the UIManager for navigation integration.
        /// REASONING: Integrate with existing navigation system
        /// </summary>
        public void RegisterWithUIManager()
        {
            var uiManager = FindFirstObjectByType<UIManager>();
            if (uiManager != null)
            {
                // REASONING: Add AI assistant to the navigation system
                // This would require extending UIManager to support AI assistant
                Debug.Log("AI Assistant Button registered with UIManager");
            }
        }
        #endregion

        #region Editor Helpers
        #if UNITY_EDITOR
        /// <summary>
        /// Test button functionality in editor.
        /// REASONING: Helpful for testing during development
        /// </summary>
        [ContextMenu("Test AI Assistant")]
        public void TestAIAssistant()
        {
            Debug.Log("Testing AI Assistant functionality...");
            ShowAIAssistant();
        }

        [ContextMenu("Simulate New Message")]
        public void SimulateNewMessage()
        {
            unreadMessages++;
            UpdateNotificationDisplay();
        }

        [ContextMenu("Clear Notifications")]
        public void SimulateClearNotifications()
        {
            ClearNotifications();
        }
        #endif
        #endregion
    }
}