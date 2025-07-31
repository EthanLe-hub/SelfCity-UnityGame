using UnityEngine; // We need UnityEngine for MonoBehaviour and ScriptableObject. 
using LifeCraft.Systems;

namespace LifeCraft.UI
{
    public class ShopTabManager : MonoBehaviour // Inherits from MonoBehaviour to allow it to be attached to a GameObject. 
    {
        [Header("Content Panels")] // Header for organization in the Unity Inspector. 
        public GameObject todayShopPanel; // Panel for today's decor items. 
        public GameObject healthHarborShopPanel; // Panel for Health Harbor building items. 
        public GameObject mindPalaceShopPanel; // Panel for Mind Palace building items. 
        public GameObject creativeCommonsShopPanel; // Panel for Creative Commons building items. 
        public GameObject socialSquareShopPanel; // Panel for Social Square building items. 

        [Header("Region Tab Buttons")]
        public UnityEngine.UI.Button healthHarborTabButton;
        public UnityEngine.UI.Button mindPalaceTabButton;
        public UnityEngine.UI.Button creativeCommonsTabButton;
        public UnityEngine.UI.Button socialSquareTabButton;

        [Header("Level System Integration")]
        [SerializeField] private PlayerLevelManager playerLevelManager; 

        private void Start() // Unity's Start method, called when the script instance is being loaded. 
        {
            ShowTodayContent(); // Show today's content by default when the game starts. 
            UpdateRegionTabVisibility(); // Update which region tabs are visible based on unlocks

            // Subscribe to PlayerLevelManager events for real-time updates
            if (PlayerLevelManager.Instance != null) // If the PlayerLevelManager instance exists, 
            {
                PlayerLevelManager.Instance.OnRegionUnlocked += OnRegionUnlocked; // then have ShopTabManager's OnRegionUnlocked method (to handle the region unlock events) subscribe to PlayerLevelManager's OnRegionUnlocked (AssessmentQuizManager.RegionType) event to listen for when PlayerLevelManager initiates a region-unlock. 
                playerLevelManager = PlayerLevelManager.Instance; // Place the PlayerLevelManager instance into the playerLevelManager variable. 
            }
            else
            {
                Debug.LogWarning("PlayerLevelManager.Instance is null in ShopTabManager");
            }

            // Force refresh after a short delay to ensure GameManager is initialized
            StartCoroutine(RefreshAfterDelay());
        }

        private void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            if (PlayerLevelManager.Instance != null)
            {
                PlayerLevelManager.Instance.OnRegionUnlocked -= OnRegionUnlocked;
            }
        }

        /// <summary>
        /// Called when a region is unlocked by PlayerLevelManager
        /// </summary>
        private void OnRegionUnlocked(AssessmentQuizManager.RegionType region)
        {
            Debug.Log($"ShopTabManager: Region {AssessmentQuizManager.GetRegionDisplayName(region)} unlocked!");
            // Force refresh the tab visibility to show the new region
            UpdateRegionTabVisibility();
        }
        
        /// <summary>
        /// Refresh tab visibility after a delay to ensure GameManager is initialized
        /// </summary>
        private System.Collections.IEnumerator RefreshAfterDelay()
        {
            yield return new WaitForSeconds(0.2f);
            Debug.Log("ShopTabManager: Forcing refresh after delay");
            UpdateRegionTabVisibility();
        }

        public void ShowTodayContent() // Method to show today's decor items. 
        {
            HideAllContentPanels(); // First, hide all content panels in the Shop tab. 
            if (todayShopPanel != null) // Check if the todayShopPanel exists. 
            {
                todayShopPanel.SetActive(true); // Activate the todayShopPanel to show it. 
            }
        }

        public void ShowHealthHarborContent() // Method to show Health Harbor building items. 
        {
            HideAllContentPanels(); // Hide all content panels first. 
            if (healthHarborShopPanel != null) // Check if the healthHarborShopPanel exists. 
            {
                healthHarborShopPanel.SetActive(true); // Activate the healthHarborShopPanel to show it. 
            }
        }

        public void ShowMindPalaceContent() // Method to show Mind Palace building items. 
        {
            HideAllContentPanels(); // Hide all content panels first. 
            if (mindPalaceShopPanel != null) // Check if the mindPalaceShopPanel exists. 
            {
                mindPalaceShopPanel.SetActive(true); // Activate the mindPalaceShopPanel to show it. 
            }
        }

        public void ShowCreativeCommonsContent() // Method to show Creative Commons building items. 
        {
            HideAllContentPanels(); // Hide all content panels first. 
            if (creativeCommonsShopPanel != null) // Check if the creativeCommonsShopPanel exists. 
            {
                creativeCommonsShopPanel.SetActive(true); // Activate the creativeCommonsShopPanel to show it. 
            }
        }

        public void ShowSocialSquareContent() // Method to show Social Square building items. 
        {
            HideAllContentPanels(); // Hide all content panels first. 
            if (socialSquareShopPanel != null) // Check if the socialSquareShopPanel exists. 
            {
                socialSquareShopPanel.SetActive(true); // Activate the socialSquareShopPanel to show it. 
            }
        }

        private void HideAllContentPanels() // Method to hide all content panels in the Shop tab. 
        {
            // Deactivate all content panels to hide them. 
            if (todayShopPanel != null) todayShopPanel.SetActive(false);
            if (healthHarborShopPanel != null) healthHarborShopPanel.SetActive(false);
            if (mindPalaceShopPanel != null) mindPalaceShopPanel.SetActive(false);
            if (creativeCommonsShopPanel != null) creativeCommonsShopPanel.SetActive(false); 
            if (socialSquareShopPanel != null) socialSquareShopPanel.SetActive(false); 
        }

        /// <summary>
        /// Update which region tab buttons are visible based on unlocked regions
        /// </summary>
        public void UpdateRegionTabVisibility()
        {
            Debug.Log("=== SHOP TAB VISIBILITY UPDATE ===");
            Debug.Log($"UpdateRegionTabVisibility called at: {System.DateTime.Now}");
            
            var unlockSystem = LifeCraft.Systems.RegionUnlockSystem.Instance;
            if (unlockSystem == null)
            {
                Debug.LogWarning("RegionUnlockSystem.Instance is null in UpdateRegionTabVisibility");
                return;
            }

            var unlockedRegions = unlockSystem.GetUnlockedRegions();
            Debug.Log($"Unlocked regions for shop tabs: {string.Join(", ", unlockedRegions.ConvertAll(r => AssessmentQuizManager.GetRegionDisplayName(r)))}");

            // Check button references
            Debug.Log($"Button references - Health Harbor: {(healthHarborTabButton != null ? "FOUND" : "NULL")}");
            Debug.Log($"Button references - Mind Palace: {(mindPalaceTabButton != null ? "FOUND" : "NULL")}");
            Debug.Log($"Button references - Creative Commons: {(creativeCommonsTabButton != null ? "FOUND" : "NULL")}");
            Debug.Log($"Button references - Social Square: {(socialSquareTabButton != null ? "FOUND" : "NULL")}");

            // Update Health Harbor tab
            if (healthHarborTabButton != null)
            {
                bool isUnlocked = unlockedRegions.Contains(AssessmentQuizManager.RegionType.HealthHarbor);
                bool wasActive = healthHarborTabButton.gameObject.activeSelf;
                healthHarborTabButton.gameObject.SetActive(isUnlocked);
                healthHarborTabButton.interactable = isUnlocked;
                Debug.Log($"Health Harbor tab: was {(wasActive ? "ACTIVE" : "INACTIVE")}, now {(isUnlocked ? "SHOWN" : "HIDDEN")}");
            }
            else
            {
                Debug.LogError("Health Harbor tab button is null!");
            }

            // Update Mind Palace tab
            if (mindPalaceTabButton != null)
            {
                bool isUnlocked = unlockedRegions.Contains(AssessmentQuizManager.RegionType.MindPalace);
                bool wasActive = mindPalaceTabButton.gameObject.activeSelf;
                mindPalaceTabButton.gameObject.SetActive(isUnlocked);
                mindPalaceTabButton.interactable = isUnlocked;
                Debug.Log($"Mind Palace tab: was {(wasActive ? "ACTIVE" : "INACTIVE")}, now {(isUnlocked ? "SHOWN" : "HIDDEN")}");
            }
            else
            {
                Debug.LogError("Mind Palace tab button is null!");
            }

            // Update Creative Commons tab
            if (creativeCommonsTabButton != null)
            {
                bool isUnlocked = unlockedRegions.Contains(AssessmentQuizManager.RegionType.CreativeCommons);
                bool wasActive = creativeCommonsTabButton.gameObject.activeSelf;
                creativeCommonsTabButton.gameObject.SetActive(isUnlocked);
                creativeCommonsTabButton.interactable = isUnlocked;
                Debug.Log($"Creative Commons tab: was {(wasActive ? "ACTIVE" : "INACTIVE")}, now {(isUnlocked ? "SHOWN" : "HIDDEN")}");
            }
            else
            {
                Debug.LogError("Creative Commons tab button is null!");
            }

            // Update Social Square tab
            if (socialSquareTabButton != null)
            {
                bool isUnlocked = unlockedRegions.Contains(AssessmentQuizManager.RegionType.SocialSquare);
                bool wasActive = socialSquareTabButton.gameObject.activeSelf;
                socialSquareTabButton.gameObject.SetActive(isUnlocked);
                socialSquareTabButton.interactable = isUnlocked;
                Debug.Log($"Social Square tab: was {(wasActive ? "ACTIVE" : "INACTIVE")}, now {(isUnlocked ? "SHOWN" : "HIDDEN")}");
            }
            else
            {
                Debug.LogError("Social Square tab button is null!");
            }
            
            Debug.Log("=== END SHOP TAB VISIBILITY UPDATE ===");
        }

        /// <summary>
        /// Refresh the shop tab visibility (call this when regions are unlocked)
        /// </summary>
        public void RefreshTabVisibility()
        {
            Debug.Log("=== SHOP TAB REFRESH CALLED ===");
            Debug.Log($"RefreshTabVisibility called at: {System.DateTime.Now}");
            UpdateRegionTabVisibility();
            Debug.Log("=== END SHOP TAB REFRESH ===");
        }
    }
}