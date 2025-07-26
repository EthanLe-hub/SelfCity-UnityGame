using LifeCraft.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LifeCraft.UI
{
    /// <summary>
    /// Displays a single resource type with its current amount and icon.
    /// </summary>
    public class ResourceDisplay : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image resourceIcon;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private TextMeshProUGUI resourceNameText;
        
        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string updateTriggerName = "Update";
        [SerializeField] private string gainTriggerName = "Gain";
        [SerializeField] private string spendTriggerName = "Spend";

        // Data
        private ResourceManager.ResourceType resourceType;
        private int currentAmount;
        private int previousAmount;

        private void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Initialize the resource display
        /// </summary>
        public void Initialize(ResourceManager.ResourceType type, int initialAmount)
        {
            resourceType = type;
            currentAmount = initialAmount;
            previousAmount = initialAmount;

            // Set resource name
            if (resourceNameText != null)
            {
                resourceNameText.text = GetResourceDisplayName(type);
            }

            // Set resource icon
            if (resourceIcon != null)
            {
                var icon = GetResourceIcon(type);
                if (icon != null)
                {
                    resourceIcon.sprite = icon;
                }
            }

            UpdateDisplay();
        }

        /// <summary>
        /// Update the displayed amount
        /// </summary>
        public void UpdateAmount(int newAmount)
        {
            previousAmount = currentAmount;
            currentAmount = ResourceManager.Instance.GetResourceTotal(resourceType);
            UpdateDisplay();
            PlayUpdateAnimation();
        }

        /// <summary>
        /// Update the visual display
        /// </summary>
        private void UpdateDisplay()
        {
            if (amountText != null)
            {
                amountText.text = currentAmount.ToString();
            }
        }

        /// <summary>
        /// Play appropriate animation based on amount change
        /// </summary>
        private void PlayUpdateAnimation()
        {
            if (animator == null) return;

            int difference = currentAmount - previousAmount;
            
            if (difference > 0)
            {
                // Resource gained
                animator.SetTrigger(gainTriggerName);
            }
            else if (difference < 0)
            {
                // Resource spent
                animator.SetTrigger(spendTriggerName);
            }
            else
            {
                // Just update
                animator.SetTrigger(updateTriggerName);
            }
        }

        /// <summary>
        /// Get display name for resource type
        /// </summary>
        private string GetResourceDisplayName(ResourceManager.ResourceType type)
        {
            switch (type)
            {
                case ResourceManager.ResourceType.EnergyCrystals:
                    return "Energy Crystals";
                case ResourceManager.ResourceType.WisdomOrbs:
                    return "Wisdom Orbs";
                case ResourceManager.ResourceType.HeartTokens:
                    return "Heart Tokens";
                case ResourceManager.ResourceType.CreativitySparks:
                    return "Creativity Sparks";
                case ResourceManager.ResourceType.BalanceTickets:
                    return "Balance Tickets";
                default:
                    return type.ToString();
            }
        }

        /// <summary>
        /// Get icon for resource type
        /// </summary>
        private Sprite GetResourceIcon(ResourceManager.ResourceType type)
        {
            // TODO: Load from Resources or ScriptableObject
            // For now, return null and let the UI handle default icons
            return null;
        }

        /// <summary>
        /// Get current resource type
        /// </summary>
        public ResourceManager.ResourceType ResourceType => resourceType;

        /// <summary>
        /// Get current amount
        /// </summary>
        public int CurrentAmount => currentAmount;

        /// <summary>
        /// Set the resource icon
        /// </summary>
        public void SetIcon(Sprite icon)
        {
            if (resourceIcon != null)
            {
                resourceIcon.sprite = icon;
            }
        }

        /// <summary>
        /// Set the display name
        /// </summary>
        public void SetDisplayName(string name)
        {
            if (resourceNameText != null)
            {
                resourceNameText.text = name;
            }
        }

        /// <summary>
        /// Animate a resource gain
        /// </summary>
        public void AnimateGain(int amount)
        {
            if (animator != null)
            {
                animator.SetTrigger(gainTriggerName);
            }
        }

        /// <summary>
        /// Animate a resource spend
        /// </summary>
        public void AnimateSpend(int amount)
        {
            if (animator != null)
            {
                animator.SetTrigger(spendTriggerName);
            }
        }

        /// <summary>
        /// Flash the display to draw attention
        /// </summary>
        public void Flash()
        {
            if (animator != null)
            {
                animator.SetTrigger("Flash");
            }
        }

        /// <summary>
        /// Set the display color
        /// </summary>
        public void SetColor(Color color)
        {
            if (resourceIcon != null)
            {
                resourceIcon.color = color;
            }
        }

        /// <summary>
        /// Reset the display color to default
        /// </summary>
        public void ResetColor()
        {
            if (resourceIcon != null)
            {
                resourceIcon.color = Color.white;
            }
        }
    }
} 