using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using LifeCraft.Systems;

namespace LifeCraft.UI
{
    /// <summary>
    /// Manages the EXP progress bar with real-time updates and smooth animations
    /// </summary>
    public class EXPProgressBarManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider expProgressBar;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI expText;
        [SerializeField] private TextMeshProUGUI expRequiredText;
        
        [Header("Animation Settings")]
        [SerializeField] private float fillAnimationDuration = 0.5f;
        [SerializeField] private float textUpdateDelay = 0.1f;
        
        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = Color.blue;
        [SerializeField] private Color levelUpColor = Color.gold;
        [SerializeField] private Image progressBarFill;
        
        private int currentLevel = 1;
        private int currentEXP = 0;
        private int expRequiredForNextLevel = 100;
        private bool isAnimating = false;
        
        private void Start()
        {
            // Subscribe to EXP and level changes
            if (PlayerLevelManager.Instance != null)
            {
                PlayerLevelManager.Instance.OnEXPChanged += OnEXPChanged;
                PlayerLevelManager.Instance.OnLevelUp += OnLevelUp;
                
                // Initialize with current values
                UpdateDisplay();
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (PlayerLevelManager.Instance != null)
            {
                PlayerLevelManager.Instance.OnEXPChanged -= OnEXPChanged;
                PlayerLevelManager.Instance.OnLevelUp -= OnLevelUp;
            }
        }
        
        /// <summary>
        /// Called when EXP changes
        /// </summary>
        private void OnEXPChanged(int newEXP)
        {
            currentEXP = newEXP;
            StartCoroutine(AnimateEXPChange());
        }
        
        /// <summary>
        /// Called when player levels up
        /// </summary>
        private void OnLevelUp(int newLevel)
        {
            currentLevel = newLevel;
            expRequiredForNextLevel = PlayerLevelManager.Instance.GetEXPRequiredForNextLevel();
            
            // Flash the progress bar during level up
            StartCoroutine(FlashProgressBar());
            
            // Update display after a short delay
            StartCoroutine(UpdateDisplayDelayed());
        }
        
        /// <summary>
        /// Animate EXP change with smooth progress bar fill
        /// </summary>
        private IEnumerator AnimateEXPChange()
        {
            if (isAnimating) yield break;
            isAnimating = true;
            
            // Calculate progress
            float progress = (float)currentEXP / expRequiredForNextLevel;
            float startProgress = expProgressBar.value;
            float targetProgress = Mathf.Clamp01(progress);
            
            // Animate progress bar
            float elapsed = 0f;
            while (elapsed < fillAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fillAnimationDuration;
                
                expProgressBar.value = Mathf.Lerp(startProgress, targetProgress, t);
                yield return null;
            }
            
            expProgressBar.value = targetProgress;
            
            // Update text after animation
            yield return new WaitForSeconds(textUpdateDelay);
            UpdateEXPText();
            
            isAnimating = false;
        }
        
        /// <summary>
        /// Flash the progress bar during level up
        /// </summary>
        private IEnumerator FlashProgressBar()
        {
            if (progressBarFill == null) yield break;
            
            Color originalColor = progressBarFill.color;
            
            // Flash to level up color
            progressBarFill.color = levelUpColor;
            yield return new WaitForSeconds(0.2f);
            
            // Return to normal color
            progressBarFill.color = originalColor;
            yield return new WaitForSeconds(0.2f);
            
            // Flash again
            progressBarFill.color = levelUpColor;
            yield return new WaitForSeconds(0.2f);
            
            // Return to normal
            progressBarFill.color = originalColor;
        }
        
        /// <summary>
        /// Update display after level up
        /// </summary>
        private IEnumerator UpdateDisplayDelayed()
        {
            yield return new WaitForSeconds(0.3f);
            UpdateDisplay();
        }
        
        /// <summary>
        /// Update all display elements
        /// </summary>
        private void UpdateDisplay()
        {
            if (PlayerLevelManager.Instance == null) return;
            
            currentLevel = PlayerLevelManager.Instance.GetCurrentLevel();
            currentEXP = PlayerLevelManager.Instance.GetCurrentEXP();
            expRequiredForNextLevel = PlayerLevelManager.Instance.GetEXPRequiredForNextLevel();
            
            UpdateLevelText();
            UpdateEXPText();
            UpdateProgressBar();
        }
        
        /// <summary>
        /// Update level text
        /// </summary>
        private void UpdateLevelText()
        {
            if (levelText != null)
            {
                levelText.text = $"Level {currentLevel}";
            }
        }
        
        /// <summary>
        /// Update EXP text
        /// </summary>
        private void UpdateEXPText()
        {
            if (expText != null)
            {
                expText.text = $"{currentEXP} / {expRequiredForNextLevel} EXP";
            }
            
            if (expRequiredText != null)
            {
                int expNeeded = expRequiredForNextLevel - currentEXP;
                expRequiredText.text = expNeeded > 0 ? $"{expNeeded} EXP to next level" : "MAX LEVEL!";
            }
        }
        
        /// <summary>
        /// Update progress bar
        /// </summary>
        private void UpdateProgressBar()
        {
            if (expProgressBar != null)
            {
                float progress = (float)currentEXP / expRequiredForNextLevel;
                expProgressBar.value = Mathf.Clamp01(progress);
            }
        }
        
        /// <summary>
        /// Public method to force update display (for testing)
        /// </summary>
        public void ForceUpdateDisplay()
        {
            UpdateDisplay();
        }
    }
} 