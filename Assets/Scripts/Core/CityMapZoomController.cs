using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TMP_Text. 
using System.Collections;
using System.Collections.Generic;
using LifeCraft.Systems;

namespace LifeCraft.Core
{
    public class CityMapZoomController : MonoBehaviour
    {
        [Header("Assign the RectTransform of your city map (e.g., CityScrollView)")]
        public RectTransform cityMapRect;

        [Header("Zoom/Pan Settings")]
        public float zoomDuration = 0.5f;

        [Header("Unlock System Integration")]
        [SerializeField] private GameObject lockedRegionPopup;
        [SerializeField] private TMP_Text lockedRegionText;
        [SerializeField] private Button closeLockedRegionPopupButton;

        [Header("Level System Integration")]
        [SerializeField] private PlayerLevelManager playerLevelManager; 

        // Define target positions and scales for each region (customize as needed)
        [System.Serializable]
        public struct RegionZoomData
        {
            public string regionName;
            public Vector2 anchoredPosition;
            public float scale;
        }

        public RegionZoomData[] regions;

        // Optional: Button references for convenience
        public Button healthHarborButton;
        public Button mindPalaceButton;
        public Button creativeCommonsButton;
        public Button socialSquareButton;

        public GameObject regionButtonGroup; // Group for "Back to Overview" and "Edit Region" buttons; assigned in the Inspector. 

        private Coroutine zoomCoroutine;

        void Start()
        {
            // Hook up buttons if assigned
            if (healthHarborButton != null)
                healthHarborButton.onClick.AddListener(() => ZoomToRegion("Health Harbor"));
            if (mindPalaceButton != null)
                mindPalaceButton.onClick.AddListener(() => ZoomToRegion("Mind Palace"));
            if (creativeCommonsButton != null)
                creativeCommonsButton.onClick.AddListener(() => ZoomToRegion("Creative Commons"));
            if (socialSquareButton != null)
                socialSquareButton.onClick.AddListener(() => ZoomToRegion("Social Square"));

            // Set up locked region popup
            if (closeLockedRegionPopupButton != null)
                closeLockedRegionPopupButton.onClick.AddListener(HideLockedRegionPopup);

            // Hide popup initially
            if (lockedRegionPopup != null)
                lockedRegionPopup.SetActive(false);

            // Subscribe to PlayerLevelManager events for real-time updates:
            if (PlayerLevelManager.Instance != null) // If the PlayerLevelManager instance exists, 
            {
                PlayerLevelManager.Instance.OnRegionUnlocked += OnRegionUnlocked; // then have CityMapZoomController's OnRegionUnlocked method (to handle the region unlock events) subscribe to PlayerLevelManager's OnRegionUnlocked (AssessmentQuizManager.RegionType) event to listen for when PlayerLevelManager initiates a region-unlock. 
                playerLevelManager = PlayerLevelManager.Instance; // Place the PlayerLevelManager instance into the playerLevelManager variable. 
            }

            else
            {
                Debug.LogWarning("PlayerLevelManager.Instance is null in CityMapZoomController");
            }
                
            // Force refresh unlock state after a delay to ensure GameManager is initialized
                StartCoroutine(RefreshUnlockStateAfterDelay());
        }

        private void OnDestroy()
        {
            if (PlayerLevelManager.Instance != null)
            {
                PlayerLevelManager.Instance.OnRegionUnlocked -= OnRegionUnlocked; // Unsubscribe from the PlayerLevelManager's OnRegionUnlocked event. 
            }
        }

        /// <summary>
        /// Called when a region is unlocked by PlayerLevelManager.
        /// </summary>
        private void OnRegionUnlocked(AssessmentQuizManager.RegionType region)
        {
            Debug.Log($"CityMapZoomController: Region {AssessmentQuizManager.GetRegionDisplayName(region)} unlocked!");
            // Force refresh the unlock state to update the UI:
            ForceRefreshUnlockState();
        }
        
        /// <summary>
        /// Force refresh the unlock state (call this when regions are unlocked)
        /// </summary>
        public void ForceRefreshUnlockState()
        {
            Debug.Log("CityMapZoomController: Force refreshing unlock state");

            var unlockSystem = LifeCraft.Systems.RegionUnlockSystem.Instance;
            if (unlockSystem != null)
            {
                var unlockedRegions = unlockSystem.GetUnlockedRegions();
                Debug.Log($"CityMapZoomController: Current unlocked regions: {string.Join(", ", unlockedRegions.ConvertAll(r => AssessmentQuizManager.GetRegionDisplayName(r)))}");
            }
            else
            {
                Debug.LogWarning("CityMapZoomController: RegionUnlockSystem.Instance is null");
            }
        }

        /// <summary>
        /// Refresh unlock state after a delay to ensure GameManager is initialized
        /// </summary>
        private System.Collections.IEnumerator RefreshUnlockStateAfterDelay()
        {
            yield return new WaitForSeconds(0.2f);
            Debug.Log("CityMapZoomController: Refreshing unlock state after delay");
            
            // Log current unlock state for debugging
            var unlockSystem = LifeCraft.Systems.RegionUnlockSystem.Instance;
            if (unlockSystem != null)
            {
                var unlockedRegions = unlockSystem.GetUnlockedRegions();
                Debug.Log($"CityMapZoomController: Current unlocked regions: {string.Join(", ", unlockedRegions.ConvertAll(r => AssessmentQuizManager.GetRegionDisplayName(r)))}");
            }
            else
            {
                Debug.LogWarning("CityMapZoomController: RegionUnlockSystem.Instance is null in delayed refresh");
            }
        }

        public void ZoomToRegion(string regionName)
        {
            // Overview should always be accessible - skip unlock check for it
            if (regionName != "Overview") // If the region is not the Overview, check if it is unlocked. 
            {
                // Check if region is unlocked before allowing zoom
                if (!IsRegionUnlocked(regionName)) // If the region is not unlocked, show the locked region popup. 
                {
                    ShowLockedRegionPopup(regionName);
                    return;
                }
            }

            for (int i = 0; i < regions.Length; i++)
            {
                if (regions[i].regionName == regionName)
                {
                    ZoomTo(regions[i].anchoredPosition, regions[i].scale);

                    // Show the region button group (e.g., "Back to Overview" and "Edit Region" buttons) only if not in Overview:
                    if (regionName == "Overview")
                    {
                        regionButtonGroup.SetActive(false); // Hide the button group when zoomed out to overview. 
                    }
                    else
                    {
                        regionButtonGroup.SetActive(true); // Show the button group when zoomed into a specific region. 
                    }
                    
                    return;
                }
            }
            Debug.LogWarning("Region not found: " + regionName);
        }

        public void ZoomTo(Vector2 targetPosition, float targetScale)
        {
            if (zoomCoroutine != null)
                StopCoroutine(zoomCoroutine);
            zoomCoroutine = StartCoroutine(AnimateZoom(targetPosition, targetScale));
        }

        public void ZoomToOverview()
        {
            // "Overview" region in the CityScrollView GameObject (under "City Map Zoom Controller" script component) is the default position and scale. 
            ZoomToRegion("Overview"); 
        }

        private IEnumerator AnimateZoom(Vector2 targetPosition, float targetScale)
        {
            Vector2 startPos = cityMapRect.anchoredPosition;
            float startScale = cityMapRect.localScale.x;
            float elapsed = 0f;

            while (elapsed < zoomDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / zoomDuration);
                cityMapRect.anchoredPosition = Vector2.Lerp(startPos, targetPosition, t);
                float scale = Mathf.Lerp(startScale, targetScale, t);
                cityMapRect.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
            cityMapRect.anchoredPosition = targetPosition;
            cityMapRect.localScale = new Vector3(targetScale, targetScale, 1f);
        }

        /// <summary>
        /// Check if a region is unlocked
        /// </summary>
        private bool IsRegionUnlocked(string regionName)
        {
            var unlockSystem = LifeCraft.Systems.RegionUnlockSystem.Instance;
            if (unlockSystem == null)
            {
                Debug.LogWarning($"IsRegionUnlocked({regionName}): RegionUnlockSystem.Instance is null, defaulting to unlocked");
                return true; // Default to unlocked if system not available
            }

            // Convert region name to RegionType
            var regionType = GetRegionTypeFromName(regionName);
            bool isUnlocked = unlockSystem.IsRegionUnlocked(regionType);
            
            Debug.Log($"IsRegionUnlocked({regionName}): {isUnlocked} (RegionType: {regionType})");
            
            // Also log all unlocked regions for debugging
            var unlockedRegions = unlockSystem.GetUnlockedRegions();
            Debug.Log($"All unlocked regions: {string.Join(", ", unlockedRegions.ConvertAll(r => AssessmentQuizManager.GetRegionDisplayName(r)))}");
            
            return isUnlocked;
        }

        /// <summary>
        /// Convert region name to RegionType
        /// </summary>
        private AssessmentQuizManager.RegionType GetRegionTypeFromName(string regionName)
        {
            switch (regionName)
            {
                case "Health Harbor": return AssessmentQuizManager.RegionType.HealthHarbor;
                case "Mind Palace": return AssessmentQuizManager.RegionType.MindPalace;
                case "Creative Commons": return AssessmentQuizManager.RegionType.CreativeCommons;
                case "Social Square": return AssessmentQuizManager.RegionType.SocialSquare;
                default: return AssessmentQuizManager.RegionType.HealthHarbor;
            }
        }

        /// <summary>
        /// Show locked region popup with unlock requirements
        /// </summary>
        private void ShowLockedRegionPopup(string regionName)
        {
            if (lockedRegionPopup == null || lockedRegionText == null)
                return;

            var regionType = GetRegionTypeFromName(regionName); // Get the region. 

            // Get the level requirement from the PlayerLevelManager:
            int requiredLevel = GetRequiredLevelForRegion(regionType); // Call the function to get the unlock level for this region. 
            int currentLevel = PlayerLevelManager.Instance != null ? PlayerLevelManager.Instance.GetCurrentLevel() : 1; // If the PlayerLevelManager instance exists, get the current player level. Otherwise, default the player level to 1. 

            if (requiredLevel > 0)
            {
                string message = $"<b>{regionName} is locked!</b>\n\n";
                message += $"To unlock {regionName}, you must reach <color=#FFD700>Level {requiredLevel}</color>.\n\n";
                message += $"Current Level: <color=#00FF00>{currentLevel}</color>\n";

                if (currentLevel < requiredLevel)
                {
                    int levelsNeeded = requiredLevel - currentLevel; // Use subtraction to calculate how many more levels the player needs before reaching the required unlock level. 
                    message += $"Progress: <color=#FF6B6B>{levelsNeeded} more level{(levelsNeeded == 1 ? "" : "s")} needed</color>"; // Append nothing if needed levels left is just 1, otherwise append an "s" for plurality. 
                }

                else
                {
                    message += $"<color=#00FF00>Ready to unlock!</color>";
                }

                lockedRegionText.text = message; // Set the whole combined message onto the UI text. 
            }

            else
            {
                lockedRegionText.text = $"<b>{regionName} is locked!</b>\n\nThis region will be unlocked as you progress through the game.";
            }

            lockedRegionPopup.SetActive(true); // Show the UI text panel now. 
        }

        private int GetRequiredLevelForRegion(AssessmentQuizManager.RegionType region)
        {
            if (PlayerLevelManager.Instance == null)
            {
                return -1;
            }

            var regionBuildings = PlayerLevelManager.Instance.GetRegionBuildings(region); // Get the specific region's list of buildings. 
            if (regionBuildings != null && regionBuildings.Count > 0) // If the specific region's list of buildings exists and has more than 0 buildings, 
            {
                string firstBuilding = regionBuildings[0]; // Get the first building in the region's list (at index 0).
                return PlayerLevelManager.Instance.GetBuildingUnlockLevel(firstBuilding); // Return the unlock level for that first building in the region's list. 
            }

            return -1; // Otherwise, return -1 as the unlock level. 
        }

        /// <summary>
        /// Hide the locked region popup
        /// </summary>
        private void HideLockedRegionPopup()
        {
            if (lockedRegionPopup != null)
                lockedRegionPopup.SetActive(false);
        }
    }
}