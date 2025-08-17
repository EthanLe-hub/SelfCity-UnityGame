using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace LifeCraft.UI
{
    /// <summary>
    /// AI Chat Message UI - Individual message display component.
    /// 
    /// DESIGN PHILOSOPHY:
    /// - Reusable component for both user and AI messages
    /// - Different styling for user vs AI messages
    /// - Responsive text sizing and word wrapping
    /// - Timestamp display for conversation context
    /// - Consistent with existing UI styling patterns
    /// </summary>
    public class AIChatMessageUI : MonoBehaviour
    {
        #region UI References
        [Header("Message Content")]
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextMeshProUGUI timestampText;
        
        [Header("Message Styling")]
        [SerializeField] private Image messageBackground;
        [SerializeField] private RectTransform messageContainer;
        
        [Header("User Message Styling")]
        [SerializeField] private Color userMessageColor = new Color(0.2f, 0.6f, 1f, 0.9f);
        [SerializeField] private Color userTextColor = Color.white;
        [SerializeField] private Vector2 userMessageAlignment = new Vector2(1f, 0f); // Right-aligned
        
        [Header("AI Message Styling")]
        [SerializeField] private Color aiMessageColor = new Color(0.9f, 0.9f, 0.9f, 0.9f);
        [SerializeField] private Color aiTextColor = Color.black;
        [SerializeField] private Vector2 aiMessageAlignment = new Vector2(0f, 0f); // Left-aligned
        
        [Header("Message Settings")]
        [SerializeField] private float maxMessageWidth = 300f;
        [SerializeField] private float minMessageHeight = 40f;
        [SerializeField] private float padding = 10f;
        #endregion

        #region Private Fields
        private bool isUserMessage = false;
        private DateTime messageTime;
        #endregion

        #region Initialization
        private void Awake()
        {
            // REASONING: Auto-find components if not assigned for easier setup
            if (messageText == null)
                messageText = GetComponentInChildren<TextMeshProUGUI>();
            
            if (messageContainer == null)
                messageContainer = GetComponent<RectTransform>();
            
            if (messageBackground == null)
                messageBackground = GetComponent<Image>();
        }
        #endregion

        #region Public API
        /// <summary>
        /// Initialize the message with content and styling.
        /// REASONING: Centralized initialization for consistent message display
        /// </summary>
        public void Initialize(string content, bool isFromUser)
        {
            isUserMessage = isFromUser;
            messageTime = DateTime.Now;
            
            SetMessageContent(content);
            ApplyMessageStyling();
            UpdateTimestamp();
            AdjustLayout();
        }

        /// <summary>
        /// Set the message content text.
        /// REASONING: Separate method for content updates
        /// </summary>
        public void SetMessageContent(string content)
        {
            if (messageText != null)
            {
                messageText.text = content;
            }
        }

        /// <summary>
        /// Set a custom timestamp for the message.
        /// REASONING: Allows setting specific timestamps for loaded messages
        /// </summary>
        public void SetTimestamp(DateTime timestamp)
        {
            messageTime = timestamp;
            UpdateTimestamp();
        }
        #endregion

        #region Styling
        /// <summary>
        /// Apply appropriate styling based on message type.
        /// REASONING: Visual distinction between user and AI messages
        /// </summary>
        private void ApplyMessageStyling()
        {
            if (messageBackground != null)
            {
                messageBackground.color = isUserMessage ? userMessageColor : aiMessageColor;
                
                // Apply rounded corners if using a mask or custom shader
                ApplyRoundedCorners();
            }

            if (messageText != null)
            {
                messageText.color = isUserMessage ? userTextColor : aiTextColor;
                
                // Set text alignment
                messageText.alignment = isUserMessage ? 
                    TextAlignmentOptions.Right : TextAlignmentOptions.Left;
            }

            // Set container alignment
            if (messageContainer != null)
            {
                messageContainer.anchorMin = isUserMessage ? 
                    new Vector2(0.5f, 0f) : new Vector2(0f, 0f);
                messageContainer.anchorMax = isUserMessage ? 
                    new Vector2(1f, 1f) : new Vector2(0.5f, 1f);
                
                // Set pivot for proper alignment
                messageContainer.pivot = isUserMessage ? 
                    new Vector2(1f, 0f) : new Vector2(0f, 0f);
            }
        }

        /// <summary>
        /// Apply rounded corners to the message background.
        /// REASONING: Modern chat bubble appearance
        /// </summary>
        private void ApplyRoundedCorners()
        {
            if (messageBackground != null)
            {
                // REASONING: Use Unity's built-in rounded rectangle sprite or custom shader
                // For now, we'll use a simple approach - in production, consider using
                // a custom shader or sprite for better rounded corners
                
                // If you have a rounded rectangle sprite, assign it here
                // messageBackground.sprite = roundedRectangleSprite;
                
                // Alternative: Use a mask with rounded corners
                var mask = GetComponent<Mask>();
                if (mask == null)
                {
                    mask = gameObject.AddComponent<Mask>();
                }
            }
        }
        #endregion

        #region Layout
        /// <summary>
        /// Adjust the message layout based on content.
        /// REASONING: Responsive design that adapts to message length
        /// </summary>
        private void AdjustLayout()
        {
            if (messageContainer == null || messageText == null)
                return;

            // Force text to update its layout
            Canvas.ForceUpdateCanvases();
            
            // Get the preferred size of the text
            Vector2 preferredSize = messageText.GetPreferredValues();
            
            // Clamp width to maximum
            float width = Mathf.Min(preferredSize.x + padding * 2, maxMessageWidth);
            
            // Calculate height based on content
            float height = Mathf.Max(preferredSize.y + padding * 2, minMessageHeight);
            
            // If text is wider than max width, recalculate height with word wrapping
            if (preferredSize.x > maxMessageWidth - padding * 2)
            {
                // Set text wrapping mode
                if (messageText != null)
                {
                    messageText.textWrappingMode = TextWrappingModes.Normal;
                }
                messageText.rectTransform.sizeDelta = new Vector2(maxMessageWidth - padding * 2, 0);
                Canvas.ForceUpdateCanvases();
                height = Mathf.Max(messageText.GetPreferredValues().y + padding * 2, minMessageHeight);
            }
            else
            {
                // Set text wrapping mode
                if (messageText != null)
                {
                    messageText.textWrappingMode = TextWrappingModes.Normal;
                }
            }
            
            // Set the container size
            messageContainer.sizeDelta = new Vector2(width, height);
            
            // Position the text within the container
            if (messageText.rectTransform != null)
            {
                messageText.rectTransform.anchorMin = Vector2.zero;
                messageText.rectTransform.anchorMax = Vector2.one;
                messageText.rectTransform.offsetMin = new Vector2(padding, padding);
                messageText.rectTransform.offsetMax = new Vector2(-padding, -padding);
            }
        }

        /// <summary>
        /// Update the timestamp display.
        /// REASONING: Show when messages were sent for conversation context
        /// </summary>
        private void UpdateTimestamp()
        {
            if (timestampText != null)
            {
                // Format timestamp based on how recent the message is
                string timeFormat = GetTimeFormat();
                timestampText.text = messageTime.ToString(timeFormat);
                
                // Style timestamp based on message type
                timestampText.color = isUserMessage ? 
                    new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.5f);
                
                // Position timestamp
                timestampText.alignment = isUserMessage ? 
                    TextAlignmentOptions.Right : TextAlignmentOptions.Left;
            }
        }

        /// <summary>
        /// Get appropriate time format based on message age.
        /// REASONING: Show relevant time information (today vs yesterday vs date)
        /// </summary>
        private string GetTimeFormat()
        {
            var now = DateTime.Now;
            var messageDate = messageTime.Date;
            var today = now.Date;
            
            if (messageDate == today)
            {
                return "HH:mm"; // Today: show time only
            }
            else if (messageDate == today.AddDays(-1))
            {
                return "HH:mm 'yesterday'"; // Yesterday: show time + "yesterday"
            }
            else if (messageDate >= today.AddDays(-7))
            {
                return "HH:mm 'on' ddd"; // This week: show time + day name
            }
            else
            {
                return "MMM dd, HH:mm"; // Older: show date + time
            }
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// Get the message content.
        /// REASONING: Allow external access to message content
        /// </summary>
        public string GetMessageContent()
        {
            return messageText != null ? messageText.text : "";
        }

        /// <summary>
        /// Get the message timestamp.
        /// REASONING: Allow external access to message timing
        /// </summary>
        public DateTime GetMessageTime()
        {
            return messageTime;
        }

        /// <summary>
        /// Check if this is a user message.
        /// REASONING: Allow external components to check message type
        /// </summary>
        public bool IsUserMessage()
        {
            return isUserMessage;
        }

        /// <summary>
        /// Get the message height for layout calculations.
        /// REASONING: Useful for chat layout management
        /// </summary>
        public float GetMessageHeight()
        {
            return messageContainer != null ? messageContainer.sizeDelta.y : minMessageHeight;
        }
        #endregion

        #region Editor Helpers
        #if UNITY_EDITOR
        /// <summary>
        /// Preview message styling in the editor.
        /// REASONING: Helpful for designers to see message appearance
        /// </summary>
        [ContextMenu("Preview User Message")]
        public void PreviewUserMessage()
        {
            Initialize("This is a sample user message for preview.", true);
        }

        [ContextMenu("Preview AI Message")]
        public void PreviewAIMessage()
        {
            Initialize("This is a sample AI response message for preview.", false);
        }

        [ContextMenu("Preview Long Message")]
        public void PreviewLongMessage()
        {
            Initialize("This is a much longer message that should demonstrate word wrapping and how the layout adjusts to accommodate longer content in the chat interface.", false);
        }
        #endif
        #endregion
    }
}