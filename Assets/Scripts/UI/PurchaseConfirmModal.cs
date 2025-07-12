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

        private System.Action onConfirm;

        private void Awake()
        {
            if (yesButton != null)
                yesButton.onClick.AddListener(OnYes);
            if (noButton != null)
                noButton.onClick.AddListener(Hide);
            Hide();
        }

        public void Show(string message, System.Action confirmCallback)
        {
            if (messageText != null)
                messageText.text = message;
            onConfirm = confirmCallback;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1;
                canvasGroup.blocksRaycasts = true;
            }
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                canvasGroup.blocksRaycasts = false;
            }
            gameObject.SetActive(false);
        }

        private void OnYes()
        {
            onConfirm?.Invoke();
            Hide();
        }
    }
}