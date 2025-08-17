using UnityEngine;
using TMPro;

namespace LifeCraft.UI
{
    /// <summary>
    /// Simple EXP popup component for the prefab
    /// </summary>
    public class EXPPopup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI expText;
        [SerializeField] private CanvasGroup canvasGroup;
        
        /// <summary>
        /// Set the EXP text
        /// </summary>
        public void SetEXPText(string text)
        {
            if (expText != null)
            {
                expText.text = text;
            }
        }
        
        /// <summary>
        /// Set the text color
        /// </summary>
        public void SetColor(Color color)
        {
            if (expText != null)
            {
                expText.color = color;
            }
        }
        
        /// <summary>
        /// Set alpha for fade effects
        /// </summary>
        public void SetAlpha(float alpha)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }
            else if (expText != null)
            {
                Color color = expText.color;
                color.a = alpha;
                expText.color = color;
            }
        }
    }
} 