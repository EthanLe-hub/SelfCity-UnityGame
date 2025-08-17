using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LifeCraft.UI
{
    /// <summary>
    /// Modal window for confirming a purchase.
    /// </summary>
    public class PurchaseConfirmModal : MonoBehaviour
    {
        public TextMeshProUGUI messageText;
        public Button yesButton;
        public Button noButton;
        public CanvasGroup canvasGroup;
        public Image iconImage; // Add this field for the item sprite

        private System.Action onConfirm;

        private void Awake()
        {
            Debug.Log("[PurchaseConfirmModal] Awake called - setting up button listeners");
            
            // Force the modal to be active immediately
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogWarning("[PurchaseConfirmModal] Modal was inactive in Awake, forcing activation");
                gameObject.SetActive(true);
            }
            
            if (yesButton != null)
                yesButton.onClick.AddListener(OnYes);
            if (noButton != null)
                noButton.onClick.AddListener(Hide);
            
            // Initialize the modal as hidden but ensure it's properly set up
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                canvasGroup.blocksRaycasts = false;
            }
            
            // Double-check activation
            gameObject.SetActive(true);
        }

        private void Start()
        {
            Debug.Log("[PurchaseConfirmModal] Start called - ensuring modal is ready");
            
            // Force activation again in Start to be absolutely sure
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogWarning("[PurchaseConfirmModal] Modal was inactive in Start, forcing activation");
                gameObject.SetActive(true);
            }
            
            // Ensure CanvasGroup is properly set up
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                canvasGroup.blocksRaycasts = false;
            }
            
            Debug.Log("[PurchaseConfirmModal] Modal initialization complete - ready to respond to clicks");
        }

        private void OnEnable()
        {
            Debug.Log("[PurchaseConfirmModal] OnEnable called - modal is now active");
        }

        public void Show(string message, System.Action confirmCallback, Sprite itemSprite = null)
        {
            Debug.Log($"[PurchaseConfirmModal] Show called with message: {message}");
            
            // Force the modal to be active and ready
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogWarning("[PurchaseConfirmModal] Modal was inactive in Show, forcing activation");
                gameObject.SetActive(true);
                
                // Give Unity a frame to process the activation
                StartCoroutine(ShowAfterActivation(message, confirmCallback, itemSprite));
                return;
            }
            
            ShowModalInternal(message, confirmCallback, itemSprite);
        }

        private System.Collections.IEnumerator ShowAfterActivation(string message, System.Action confirmCallback, Sprite itemSprite)
        {
            // Wait for end of frame to ensure activation is processed
            yield return new WaitForEndOfFrame();
            
            Debug.Log("[PurchaseConfirmModal] Showing modal after activation");
            ShowModalInternal(message, confirmCallback, itemSprite);
        }

        private void ShowModalInternal(string message, System.Action confirmCallback, Sprite itemSprite)
        {
            if (messageText != null)
                messageText.text = message;
            else
                Debug.LogError("[PurchaseConfirmModal] messageText is null!");
                
            onConfirm = confirmCallback;
            
            // Set the item sprite if provided
            if (iconImage != null && itemSprite != null)
            {
                iconImage.sprite = itemSprite;
                iconImage.enabled = true;
            }
            else if (iconImage != null)
            {
                iconImage.enabled = false;
            }
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1;
                canvasGroup.blocksRaycasts = true;
            }
            else
            {
                Debug.LogError("[PurchaseConfirmModal] canvasGroup is null!");
            }
            
            Debug.Log("[PurchaseConfirmModal] Modal is now visible and interactive");
        }

        public void Hide()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                canvasGroup.blocksRaycasts = false;
            }
            // Don't deactivate the GameObject - just hide it via CanvasGroup
            gameObject.SetActive(false); // This was causing the double-click issue
        }

        private void OnYes()
        {
            onConfirm?.Invoke();
            Hide();
        }
    }
}