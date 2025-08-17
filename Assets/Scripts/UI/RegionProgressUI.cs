using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using LifeCraft.Core;
using LifeCraft.Systems;

namespace LifeCraft.UI
{
    /// <summary>
    /// Displays building count and progress toward unlocking the next region
    /// </summary>
    public class RegionProgressUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject progressPanel;
        [SerializeField] private TMP_Text progressTitleText;
        [SerializeField] private TMP_Text progressDescriptionText;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button toggleButton; // Button to show/hide the panel

        [Header("Settings")]
        [SerializeField] private bool showOnStart = true;
        //[SerializeField] private float updateInterval = 1f; // How often to update the display

        [Header("Level System Integration")]
        [SerializeField] private PlayerLevelManager playerLevelManager;

        private float _lastUpdateTime;

        private void Start()
        {
            Debug.Log("RegionProgressUI Start() called");

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(HideProgressPanel);
                Debug.Log("Close button listener added");
            }
            else
            {
                Debug.LogWarning("Close button is null!");
            }

            // Subscribe to PlayerLevelManager events for real-time updates
            if (PlayerLevelManager.Instance != null)
            {
                PlayerLevelManager.Instance.OnRegionUnlocked += OnRegionUnlocked; // Listen for when regions are unlocked. 
                PlayerLevelManager.Instance.OnLevelUp += OnLevelUp; // Listen for when the player levels up.
                playerLevelManager = PlayerLevelManager.Instance; // Assign the PlayerLevelManager instance to the variable. 
                Debug.Log("Subscribed to PlayerLevelManager events");
            }
            else
            {
                Debug.LogWarning("PlayerLevelManager.Instance is null in RegionProgressUI");
            }

            if (showOnStart)
                ShowProgressPanel();
            else
                HideProgressPanel();
        }

        /// <summary>
        /// Called when a region is unlocked by PlayerLevelManager
        /// </summary>
        private void OnRegionUnlocked(AssessmentQuizManager.RegionType region)
        {
            Debug.Log($"RegionProgressUI: Region {AssessmentQuizManager.GetRegionDisplayName(region)} unlocked!");
            // Force refresh the progress display
            UpdateProgressDisplay();
        }

        /*
        private void Update()
        {
            // Only update if the panel is visible and enough time has passed
            if (progressPanel != null && progressPanel.activeSelf && Time.time - _lastUpdateTime > updateInterval)
            {
                UpdateProgressDisplay();
                _lastUpdateTime = Time.time;
            }
        }
        */

        /// <summary>
        /// Show the progress panel
        /// </summary>
        public void ShowProgressPanel()
        {
            if (progressPanel != null)
                progressPanel.SetActive(true);

            UpdateProgressDisplay();
        }

        /// <summary>
        /// Force update the progress display (called from GameManager)
        /// </summary>
        public void ForceUpdateDisplay()
        {
            UpdateProgressDisplay();
        }

        /// <summary>
        /// Hide the progress panel
        /// </summary>
        public void HideProgressPanel()
        {
            if (progressPanel != null)
                progressPanel.SetActive(false);
        }

        /// <summary>
        /// Toggle the progress panel visibility
        /// </summary>
        public void ToggleProgressPanel()
        {
            Debug.Log("ToggleProgressPanel called");

            if (progressPanel != null)
            {
                bool isActive = progressPanel.activeSelf;
                Debug.Log($"Progress panel is currently {(isActive ? "active" : "inactive")}");

                progressPanel.SetActive(!isActive);
                Debug.Log($"Progress panel set to {(isActive ? "inactive" : "active")}");

                if (!isActive)
                {
                    UpdateProgressDisplay(); // Refresh data when showing
                }
            }
            else
            {
                Debug.LogError("Progress panel is null! Check the Inspector assignment.");
            }
        }

        /// <summary>
        /// Update the progress display with current level-based unlock information
        /// </summary>
        private void UpdateProgressDisplay()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogWarning("GameManager.Instance is null!");
                return;
            }

            if (GameManager.Instance.RegionUnlockSystem == null)
            {
                Debug.LogWarning("RegionUnlockSystem is null!");
                return;
            }

            var unlockSystem = GameManager.Instance.RegionUnlockSystem;
            var unlockedRegions = unlockSystem.GetUnlockedRegions();
            var nextRegion = unlockSystem.GetNextRegionToUnlock();

            // If no regions are unlocked yet, show progress for the starting region
            if (unlockedRegions.Count == 0)
            {
                var startingRegion = unlockSystem.GetStartingRegion();
                
                if (progressTitleText != null)
                    progressTitleText.text = $"Complete {AssessmentQuizManager.GetRegionDisplayName(startingRegion)}";
                
                if (progressDescriptionText != null)
                    progressDescriptionText.text = $"Reach the required level to unlock the next region";
                
                // Show level-based progress for starting region
                int requiredLevel = GetRequiredLevelForRegion(startingRegion);
                int currentLevel = PlayerLevelManager.Instance != null ? PlayerLevelManager.Instance.GetCurrentLevel() : 1;
                
                if (progressBar != null)
                {
                    if (requiredLevel > 0)
                    {
                        float progress = currentLevel >= requiredLevel ? 1f : (float)currentLevel / requiredLevel;
                        progressBar.value = Mathf.Clamp01(progress);
                    }
                    else
                    {
                        progressBar.value = 0f;
                    }
                }
                
                if (progressText != null)
                {
                    if (requiredLevel > 0)
                    {
                        progressText.text = $"Level {currentLevel} / {requiredLevel}";
                    }
                    else
                    {
                        progressText.text = "Level requirement not found";
                    }
                }
                
                return;
            }

            // If all regions are unlocked
            if (!nextRegion.HasValue)
            {
                if (progressTitleText != null)
                    progressTitleText.text = "All Regions Unlocked!";
                
                if (progressDescriptionText != null)
                    progressDescriptionText.text = "Congratulations! You've unlocked all regions.";
                
                if (progressBar != null)
                    progressBar.value = 1f;
                
                if (progressText != null)
                    progressText.text = "Complete!";
                
                return;
            }

            // Show progress for next region to unlock
            if (progressTitleText != null)
                progressTitleText.text = $"Unlock {AssessmentQuizManager.GetRegionDisplayName(nextRegion.Value)}";

            // Get level requirements for the next region
            int nextRegionRequiredLevel = GetRequiredLevelForRegion(nextRegion.Value);
            int currentPlayerLevel = PlayerLevelManager.Instance != null ? PlayerLevelManager.Instance.GetCurrentLevel() : 1;

            // Update description with level requirement
            if (progressDescriptionText != null)
            {
                if (nextRegionRequiredLevel > 0)
                {
                    progressDescriptionText.text = $"Reach Level {nextRegionRequiredLevel} to unlock {AssessmentQuizManager.GetRegionDisplayName(nextRegion.Value)}";
                }
                else
                {
                    progressDescriptionText.text = $"Complete requirements to unlock {AssessmentQuizManager.GetRegionDisplayName(nextRegion.Value)}";
                }
            }

            // Calculate level-based progress
            if (progressBar != null)
            {
                if (nextRegionRequiredLevel > 0)
                {
                    float progress = 0f;
                    if (currentPlayerLevel >= nextRegionRequiredLevel)
                    {
                        progress = 1f; // Fully unlocked
                    }
                    else
                    {
                        // Calculate progress towards the required level
                        int previousLevel = nextRegionRequiredLevel - 1;
                        int levelsNeeded = nextRegionRequiredLevel - previousLevel;
                        int levelsGained = currentPlayerLevel - previousLevel;
                        progress = Mathf.Clamp01((float)levelsGained / levelsNeeded);
                    }
                    progressBar.value = progress;
                }
                else
                {
                    progressBar.value = 0f;
                }
            }

            // Update progress text
            if (progressText != null)
            {
                if (nextRegionRequiredLevel > 0)
                {
                    if (currentPlayerLevel >= nextRegionRequiredLevel)
                    {
                        progressText.text = $"Ready to unlock! (Level {currentPlayerLevel})";
                    }
                    else
                    {
                        int levelsNeeded = nextRegionRequiredLevel - currentPlayerLevel;
                        progressText.text = $"Level {currentPlayerLevel} / {nextRegionRequiredLevel} ({levelsNeeded} more level{(levelsNeeded == 1 ? "" : "s")} needed)";
                    }
                }
                else
                {
                    progressText.text = "Level requirement not found";
                }
            }
        }

        /// <summary>
        /// Handle level changes (called when player levels up)
        /// </summary>
        private void OnLevelUp(int newLevel)
        {
            Debug.Log($"Player reached level {newLevel}");
            
            // Only update if the progress panel is visible
            if (progressPanel != null && progressPanel.activeSelf)
            {
                UpdateProgressDisplay();
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from PlayerLevelManager events to prevent memory leaks
            if (PlayerLevelManager.Instance != null)
            {
                PlayerLevelManager.Instance.OnRegionUnlocked -= OnRegionUnlocked;
                PlayerLevelManager.Instance.OnLevelUp -= OnLevelUp;
            }
        }
        
        /// <summary>
        /// Get the required level to unlock a specific region
        /// </summary>
        private int GetRequiredLevelForRegion(AssessmentQuizManager.RegionType region)
        {
            if (PlayerLevelManager.Instance == null)
                return -1;
            
            var regionBuildings = PlayerLevelManager.Instance.GetRegionBuildings(region); // Get the list of building names for this specific region. 
            if (regionBuildings != null && regionBuildings.Count > 0)
            {
                string firstBuilding = regionBuildings[0]; // Get the first building of this specific region (at index 0 of the list).
                return PlayerLevelManager.Instance.GetBuildingUnlockLevel(firstBuilding);
            }
            
            return -1;
        }
    }
} 