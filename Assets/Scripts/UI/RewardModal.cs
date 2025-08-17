using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LifeCraft.UI
{
    /// <summary>
    /// Controls the reward modal popup window for showing rewards (decorations, buildings, etc.).
    /// Attach this script to your RewardModal prefab.
    /// </summary>
    public class RewardModal : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI messageText; // The main message (e.g., "You got a ___!")
        public Image iconImage; // Optional icon for the reward
        public Button closeButton; // Button to close the modal
        public CanvasGroup canvasGroup; // For fade/show/hide

        [Header("Modal Sizing")]
        private Vector2 defaultSize = new Vector2(600, 250); // Default modal size
        private bool isResized = false;

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);
        }

        /// <summary>
        /// Show the modal with a custom message and optional icon.
        /// </summary>
        /// <param name="message">The message to display (e.g., "You got a ___!")</param>
        /// <param name="icon">Optional sprite for the reward</param>
        public void Show(string message, Sprite icon = null)
        {
            Debug.Log("Showing modal!");
            if (messageText != null)
                messageText.text = message;
            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.gameObject.SetActive(icon != null);
            }
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1;
                canvasGroup.blocksRaycasts = true;
            }
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Show the modal with custom size for longer messages
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="icon">Optional sprite for the reward</param>
        /// <param name="customSize">Custom size for the modal</param>
        public void Show(string message, Sprite icon, Vector2 customSize)
        {
            Debug.Log($"Showing modal with custom size: {customSize}");
            
            // Resize the modal
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = customSize;
                isResized = true;
                Debug.Log($"[RewardModal] Modal resized to: {customSize}");
                
                // Adjust internal layout for larger modal
                AdjustLayoutForCustomSize(customSize);
            }
            
            Show(message, icon);
        }

        /// <summary>
        /// Adjust the internal layout when modal is resized
        /// </summary>
        /// <param name="customSize">The new size of the modal</param>
        private void AdjustLayoutForCustomSize(Vector2 customSize)
        {
            // Calculate the height difference to adjust positioning
            float heightDifference = customSize.y - defaultSize.y;
            
            // Move the message text up to use the extra space
            if (messageText != null)
            {
                var textRect = messageText.GetComponent<RectTransform>();
                if (textRect != null)
                {
                    // Move text up by half the extra height to center it in the larger space
                    Vector2 textPos = textRect.anchoredPosition;
                    textPos.y += heightDifference * 0.5f; // Move up by 30% of extra height
                    textRect.anchoredPosition = textPos;
                    Debug.Log($"[RewardModal] Moved text up by {heightDifference * 0.3f} pixels");
                }
            }
            
            // Move the close button down to the bottom of the larger modal
            if (closeButton != null)
            {
                var buttonRect = closeButton.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    // Move button down to use the extra space at the bottom
                    Vector2 buttonPos = buttonRect.anchoredPosition;
                    buttonPos.y -= heightDifference * 0.5f; // Move down by 60% of extra height
                    buttonRect.anchoredPosition = buttonPos;
                    Debug.Log($"[RewardModal] Moved button down by {heightDifference * 0.6f} pixels");
                }
            }
            
            // Adjust icon position if present
            if (iconImage != null && iconImage.gameObject.activeInHierarchy)
            {
                var iconRect = iconImage.GetComponent<RectTransform>();
                if (iconRect != null)
                {
                    // Move icon up slightly to stay with the text
                    Vector2 iconPos = iconRect.anchoredPosition;
                    iconPos.y += heightDifference * 0.2f; // Move up by 20% of extra height
                    iconRect.anchoredPosition = iconPos;
                    Debug.Log($"[RewardModal] Moved icon up by {heightDifference * 0.2f} pixels");
                }
            }
        }

        /// <summary>
        /// Reset the internal layout to default positions
        /// </summary>
        private void ResetLayoutToDefault()
        {
            // Reset text position
            if (messageText != null)
            {
                var textRect = messageText.GetComponent<RectTransform>();
                if (textRect != null)
                {
                    // Reset to original position (you may need to store original positions)
                    // For now, we'll use a reasonable default
                    Vector2 textPos = textRect.anchoredPosition;
                    textPos.y = 65f; // Default Y position from the prefab
                    textRect.anchoredPosition = textPos;
                }
            }
            
            // Reset button position
            if (closeButton != null)
            {
                var buttonRect = closeButton.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    // Reset to original position
                    Vector2 buttonPos = buttonRect.anchoredPosition;
                    buttonPos.y = -79f; // Default Y position from the prefab
                    buttonRect.anchoredPosition = buttonPos;
                }
            }
            
            // Reset icon position
            if (iconImage != null)
            {
                var iconRect = iconImage.GetComponent<RectTransform>();
                if (iconRect != null)
                {
                    // Reset to original position
                    Vector2 iconPos = iconRect.anchoredPosition;
                    iconPos.y = 30f; // Default Y position from the prefab
                    iconRect.anchoredPosition = iconPos;
                }
            }
        }

        /// <summary>
        /// Hide/close the modal window.
        /// </summary>
        public void Hide()
        {
            Debug.Log("hiding modal!"); 
            
            // Reset modal size if it was resized
            if (isResized)
            {
                var rectTransform = GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = defaultSize;
                    isResized = false;
                    Debug.Log($"[RewardModal] Modal reset to default size: {defaultSize}");
                }
                
                // Reset internal layout positions
                ResetLayoutToDefault();
                Debug.Log("[RewardModal] Reset internal layout to default positions");
            }
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                canvasGroup.blocksRaycasts = false;
            }
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Set the default size for the modal
        /// </summary>
        /// <param name="size">Default size to use</param>
        public void SetDefaultSize(Vector2 size)
        {
            defaultSize = size;
        }
    }
} 