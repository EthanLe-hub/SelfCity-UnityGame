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
        /// Hide/close the modal window.
        /// </summary>
        public void Hide()
        {
            Debug.Log("hiding modal!"); 
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                canvasGroup.blocksRaycasts = false;
            }
            gameObject.SetActive(false);
        }
    }
} 