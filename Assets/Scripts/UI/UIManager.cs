using LifeCraft.Core;
using LifeCraft.Systems; // For AssessmentQuizManager. 
using LifeCraft.Shop; // For BuildingShopDatabase
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace LifeCraft.UI
{
    /// <summary>
    /// Manages all UI elements and interactions in the game.
    /// Handles resource displays, building UI, and general UI navigation.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Resource Display")]
        [SerializeField] private Transform resourceDisplayContainer;
        [SerializeField] private GameObject resourceDisplayPrefab;

        [Header("Building UI")]
        [SerializeField] private GameObject buildingPanel;
        [SerializeField] private Transform buildingButtonContainer;
        [SerializeField] private GameObject buildingButtonPrefab;
        [SerializeField] private Button closeBuildingPanelButton;

        [Header("City UI")]
        [SerializeField] private Button cityViewButton;

        [Header("Dashboard UI")]
        [SerializeField] private Transform habitContainer;
        [SerializeField] private GameObject habitItemPrefab;

        [Header("Notifications")]
        [SerializeField] private GameObject notificationPanel;
        [SerializeField] private TextMeshProUGUI notificationText;
        [SerializeField] private float notificationDuration = 3f;

        [Header("Bottom Bar Navigation")]
        [SerializeField] private Button cityTabButton; // Button to switch to city view. 
        [SerializeField] private Button homeTabButton; // Button to switch to home view. 
        [SerializeField] private Button shopTabButton; // Button to switch to shop view. 
        [SerializeField] private Button profileTabButton; // Button to switch to profile view. 

        [SerializeField] private GameObject cityPanel; // Panel for city view. 
        [SerializeField] private GameObject homePanel; // Panel for home view. 
        [SerializeField] private GameObject shopPanel; // Panel for shop view. 
        [SerializeField] private GameObject profilePanel; // Panel for profile view. 

        [Header("Content Panels")]
        [SerializeField] private GameObject cityContentPanel; // Content panel for city view. 
        [SerializeField] private GameObject homeContentPanel; // Content panel for home view. 
        [SerializeField] private GameObject shopContentPanel; // Content panel for shop view. 
        [SerializeField] private GameObject profileContentPanel; // Content panel for profile view. 

        [Header("Building Shop")]
        [SerializeField] private BuildingShopDatabase buildingShopDatabase;

        [Header("Assessment Quiz")]
        [SerializeField] private AssessmentQuizManager assessmentQuizManager;

        // Private fields
        private Dictionary<ResourceManager.ResourceType, ResourceDisplay> resourceDisplays;
        private List<GameObject> buildingButtons = new List<GameObject>();
        private ResourceManager resourceManager;
        private CityBuilder cityBuilder;

        public static UIManager Instance { get; private set; } // Singleton instance of UIManager. 
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject); // Ensure only one instance exists. 
                return; 
            }

            Instance = this; // Set the singleton instance. 

            resourceDisplays = new Dictionary<ResourceManager.ResourceType, ResourceDisplay>();
        }

        private void Start()
        {
            InitializeUI();
            SetupEventListeners();
        }

        /// <summary>
        /// Initialize all UI elements
        /// </summary>
        public void InitializeUI()
        {
            // Get references
            resourceManager = ResourceManager.Instance;
            // REMOVED: resourceManager.Initialize(); // This was resetting resources to default values!
            cityBuilder = FindFirstObjectByType<LifeCraft.Core.CityBuilder>();

            // Setup resource displays
            SetupResourceDisplays();

            // Setup building panel
            SetupBuildingPanel();

            // Setup navigation
            SetupNavigation();

            // Show initial panel
            ShowCityPanel();
        }

        /// <summary>
        /// Setup resource display UI
        /// </summary>
        private void SetupResourceDisplays()
        {
            if (resourceDisplayContainer == null || resourceDisplayPrefab == null)
                return;

            // Clear existing displays
            foreach (Transform child in resourceDisplayContainer)
            {
                Destroy(child.gameObject);
            }

            // Create displays for each resource type
            foreach (ResourceManager.ResourceType resourceType in System.Enum.GetValues(typeof(ResourceManager.ResourceType)))
            {
                GameObject displayObj = Instantiate(resourceDisplayPrefab, resourceDisplayContainer);
                ResourceDisplay display = displayObj.GetComponent<ResourceDisplay>();
                
                if (display != null)
                {
                    display.Initialize(resourceType, resourceManager.GetResourceTotal(resourceType));
                    resourceDisplays[resourceType] = display;
                }
            }
        }

        /// <summary>
        /// Setup building panel with available buildings
        /// </summary>
        private void SetupBuildingPanel()
        {
            if (buildingButtonContainer == null || buildingButtonPrefab == null)
                return;

            // Clear existing buttons
            foreach (Transform child in buildingButtonContainer)
            {
                Destroy(child.gameObject);
            }

            // Create buttons for unlocked buildings from unlocked regions only
            if (buildingShopDatabase != null && GameManager.Instance?.RegionUnlockSystem != null)
            {
                var unlockedRegions = GameManager.Instance.RegionUnlockSystem.GetUnlockedRegions();
                var allBuildings = buildingShopDatabase.buildings;
                
                foreach (var building in allBuildings)
                {
                    // Check if this building belongs to an unlocked region
                    if (IsBuildingFromUnlockedRegion(building.name, unlockedRegions))
                    {
                        GameObject buttonObj = Instantiate(buildingButtonPrefab, buildingButtonContainer);
                        BuildingButton buildingButton = buttonObj.GetComponent<BuildingButton>();
                        
                        if (buildingButton != null)
                        {
                            buildingButton.Initialize(building, OnBuildingButtonClicked);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if a building belongs to an unlocked region
        /// </summary>
        private bool IsBuildingFromUnlockedRegion(string buildingName, List<AssessmentQuizManager.RegionType> unlockedRegions)
        {
            // Try to get the actual region from the building shop database first
            if (buildingShopDatabase != null)
            {
                // Look for the building in the shop database to get its actual region
                var buildingItem = buildingShopDatabase.buildings.Find(b => b.name == buildingName);
                if (buildingItem != null)
                {
                    // Convert the building's region to AssessmentQuizManager.RegionType
                    var buildingRegion = GetBuildingRegionFromShopItem(buildingItem);
                    return unlockedRegions.Contains(buildingRegion);
                }
            }
            
            // Fallback to name patterns if building not found in shop database
            if (buildingName.Contains("Wellness") || buildingName.Contains("Yoga") || 
                buildingName.Contains("Juice") || buildingName.Contains("Sleep") || 
                buildingName.Contains("Nutrition") || buildingName.Contains("Spa") || 
                buildingName.Contains("Running") || buildingName.Contains("Therapy") || 
                buildingName.Contains("Biohacking") || buildingName.Contains("Aquatic") || 
                buildingName.Contains("Hydration") || buildingName.Contains("Fresh Air"))
            {
                return unlockedRegions.Contains(AssessmentQuizManager.RegionType.HealthHarbor);
            }
            else if (buildingName.Contains("Meditation") || buildingName.Contains("Therapy") || 
                     buildingName.Contains("Gratitude") || buildingName.Contains("Boundary") || 
                     buildingName.Contains("Calm") || buildingName.Contains("Reflection") || 
                     buildingName.Contains("Monument") || buildingName.Contains("Tower") || 
                     buildingName.Contains("Maze") || buildingName.Contains("Library") || 
                     buildingName.Contains("Dream") || buildingName.Contains("Focus") || 
                     buildingName.Contains("Resilience"))
            {
                return unlockedRegions.Contains(AssessmentQuizManager.RegionType.MindPalace);
            }
            else if (buildingName.Contains("Writer") || buildingName.Contains("Art") || 
                     buildingName.Contains("Expression") || buildingName.Contains("Amphitheater") || 
                     buildingName.Contains("Innovation") || buildingName.Contains("Style") || 
                     buildingName.Contains("Music") || buildingName.Contains("Maker") || 
                     buildingName.Contains("Inspiration") || buildingName.Contains("Animation") || 
                     buildingName.Contains("Design") || buildingName.Contains("Sculpture") || 
                     buildingName.Contains("Film"))
            {
                return unlockedRegions.Contains(AssessmentQuizManager.RegionType.CreativeCommons);
            }
            else if (buildingName.Contains("Friendship") || buildingName.Contains("Kindness") || 
                     buildingName.Contains("Community") || buildingName.Contains("Cultural") || 
                     buildingName.Contains("Game") || buildingName.Contains("Coffee") || 
                     buildingName.Contains("Family") || buildingName.Contains("Support") || 
                     buildingName.Contains("Stage") || buildingName.Contains("Volunteer") || 
                     buildingName.Contains("Celebration") || buildingName.Contains("Pet") || 
                     buildingName.Contains("Teamwork"))
            {
                return unlockedRegions.Contains(AssessmentQuizManager.RegionType.SocialSquare);
            }

            // Default to true if no match found (for decorations and other items)
            return true;
        }

        /// <summary>
        /// Get the region for a building from the shop database
        /// </summary>
        private AssessmentQuizManager.RegionType GetBuildingRegionFromShopItem(LifeCraft.Shop.BuildingShopItem buildingItem)
        {
            // Check building name patterns to determine region (same logic as CityBuilder)
            if (buildingItem.name.Contains("Wellness") || buildingItem.name.Contains("Yoga") || 
                buildingItem.name.Contains("Juice") || buildingItem.name.Contains("Sleep") || 
                buildingItem.name.Contains("Nutrition") || buildingItem.name.Contains("Spa") || 
                buildingItem.name.Contains("Running") || buildingItem.name.Contains("Therapy") || 
                buildingItem.name.Contains("Biohacking") || buildingItem.name.Contains("Aquatic") || 
                buildingItem.name.Contains("Hydration") || buildingItem.name.Contains("Fresh Air"))
            {
                return AssessmentQuizManager.RegionType.HealthHarbor;
            }
            else if (buildingItem.name.Contains("Meditation") || buildingItem.name.Contains("Therapy") || 
                     buildingItem.name.Contains("Gratitude") || buildingItem.name.Contains("Boundary") || 
                     buildingItem.name.Contains("Calm") || buildingItem.name.Contains("Reflection") || 
                     buildingItem.name.Contains("Monument") || buildingItem.name.Contains("Tower") || 
                     buildingItem.name.Contains("Maze") || buildingItem.name.Contains("Library") || 
                     buildingItem.name.Contains("Dream") || buildingItem.name.Contains("Focus") || 
                     buildingItem.name.Contains("Resilience"))
            {
                return AssessmentQuizManager.RegionType.MindPalace;
            }
            else if (buildingItem.name.Contains("Writer") || buildingItem.name.Contains("Art") || 
                     buildingItem.name.Contains("Expression") || buildingItem.name.Contains("Amphitheater") || 
                     buildingItem.name.Contains("Innovation") || buildingItem.name.Contains("Style") || 
                     buildingItem.name.Contains("Music") || buildingItem.name.Contains("Maker") || 
                     buildingItem.name.Contains("Inspiration") || buildingItem.name.Contains("Animation") || 
                     buildingItem.name.Contains("Design") || buildingItem.name.Contains("Sculpture") || 
                     buildingItem.name.Contains("Film"))
            {
                return AssessmentQuizManager.RegionType.CreativeCommons;
            }
            else if (buildingItem.name.Contains("Friendship") || buildingItem.name.Contains("Kindness") || 
                     buildingItem.name.Contains("Community") || buildingItem.name.Contains("Cultural") || 
                     buildingItem.name.Contains("Game") || buildingItem.name.Contains("Coffee") || 
                     buildingItem.name.Contains("Family") || buildingItem.name.Contains("Support") || 
                     buildingItem.name.Contains("Stage") || buildingItem.name.Contains("Volunteer") || 
                     buildingItem.name.Contains("Celebration") || buildingItem.name.Contains("Pet") || 
                     buildingItem.name.Contains("Teamwork"))
            {
                return AssessmentQuizManager.RegionType.SocialSquare;
            }

            // Default to Health Harbor if no match found
            return AssessmentQuizManager.RegionType.HealthHarbor;
        }

        /// <summary>
        /// Setup navigation between panels
        /// </summary>
        private void SetupNavigation()
        {
            // Existing navigation setup:
            if (cityViewButton != null)
                cityViewButton.onClick.AddListener(ShowCityPanel);

            if (closeBuildingPanelButton != null)
                closeBuildingPanelButton.onClick.AddListener(HideBuildingPanel);

            // --- Bottom Bar Navigation ---
            if (cityTabButton != null)
                cityTabButton.onClick.AddListener(ShowCityPanel); // If the city tab is not null, add a listener to show the city panel when clicked. 

            if (homeTabButton != null)
                homeTabButton.onClick.AddListener(ShowHomePanel); // If the home tab is not null, add a listener to show the home panel when clicked. 

            if (shopTabButton != null)
                shopTabButton.onClick.AddListener(ShowShopPanel); // If the shop tab is not null, add a listener to show the shop panel when clicked. 

            if (profileTabButton != null)
                profileTabButton.onClick.AddListener(ShowProfilePanel); // If the profile tab is not null, add a listener to show the profile panel when clicked. 
        }

        // --- Buttom Bar Panel Switching Methods ---

        public void ShowHomePanel() // If the Home panel is selected, show the Home panel and hide the others. 
        {
            if (homePanel != null) homePanel.SetActive(true);
            if (cityPanel != null) cityPanel.SetActive(false);
            if (shopPanel != null) shopPanel.SetActive(false);
            if (profilePanel != null) profilePanel.SetActive(false); 
        }

        public void ShowCityPanel() // If the City panel is selected, show the City panel and hide the others. 
        {
            if (homePanel != null) homePanel.SetActive(false);
            if (cityPanel != null) cityPanel.SetActive(true);
            if (shopPanel != null) shopPanel.SetActive(false);
            if (profilePanel != null) profilePanel.SetActive(false); 
        }

        public void ShowShopPanel() // If the Shop panel is selected, show the Shop panel and hide the others. 
        {
            if (homePanel != null) homePanel.SetActive(false);
            if (cityPanel != null) cityPanel.SetActive(false);
            if (shopPanel != null) shopPanel.SetActive(true);
            if (profilePanel != null) profilePanel.SetActive(false); 
        }

        public void ShowProfilePanel() // If the Profile panel is selected, show the Profile panel and hide the others. 
        {
            if (homePanel != null) homePanel.SetActive(false);
            if (cityPanel != null) cityPanel.SetActive(false);
            if (shopPanel != null) shopPanel.SetActive(false);
            if (profilePanel != null) profilePanel.SetActive(true); 
        }

        /// <summary>
        /// Setup event listeners
        /// </summary>
        private void SetupEventListeners()
        {
            if (resourceManager != null)
            {
                resourceManager.OnResourceUpdated.AddListener(OnResourceUpdated);
            }
        }

        /// <summary>
        /// Handle resource updates
        /// </summary>
        private void OnResourceUpdated(ResourceManager.ResourceType resourceType, int newAmount)
        {
            if (resourceDisplays.TryGetValue(resourceType, out ResourceDisplay display))
            {
                display.UpdateAmount(newAmount);
            }
        }

        /// <summary>
        /// Handle building button clicks
        /// </summary>
        private void OnBuildingButtonClicked(string buildingType)
        {
            HideBuildingPanel();
            
            // TODO: Enter building placement mode
            Debug.Log($"Selected building: {buildingType}");
        }

        /// <summary>
        /// Show building panel
        /// </summary>
        public void ShowBuildingPanel()
        {
            if (buildingPanel != null)
            {
                buildingPanel.SetActive(true);
                RefreshBuildingPanel(); // Refresh the building list when showing the panel
            }
        }

        /// <summary>
        /// Hide building panel
        /// </summary>
        public void HideBuildingPanel()
        {
            if (buildingPanel != null)
                buildingPanel.SetActive(false);
        }

        /// <summary>
        /// Show notification
        /// </summary>
        public void ShowNotification(string message)
        {
            if (notificationPanel != null && notificationText != null)
            {
                notificationText.text = message;
                notificationPanel.SetActive(true);
                
                // Auto-hide after duration
                Invoke(nameof(HideNotification), notificationDuration);
            }
        }

        /// <summary>
        /// Hide notification
        /// </summary>
        public void HideNotification()
        {
            if (notificationPanel != null)
                notificationPanel.SetActive(false);
        }

        /// <summary>
        /// Refresh the building panel to show only buildings from unlocked regions
        /// </summary>
        public void RefreshBuildingPanel()
        {
            SetupBuildingPanel();
        }

        /// <summary>
        /// Update habit display
        /// </summary>
        public void UpdateHabitDisplay(List<HabitData> habits)
        {
            if (habitContainer == null || habitItemPrefab == null)
                return;

            // Clear existing habits
            foreach (Transform child in habitContainer)
            {
                Destroy(child.gameObject);
            }

            // Create habit items
            /**foreach (var habit in habits)
            {
                GameObject habitObj = Instantiate(habitItemPrefab, habitContainer);
                HabitItem habitItem = habitObj.GetComponent<HabitItem>();
                
                if (habitItem != null)
                {
                    habitItem.Initialize(habit);
                }
            }**/
        }

        /// <summary>
        /// Get resource display for a specific resource type
        /// </summary>
        public ResourceDisplay GetResourceDisplay(ResourceManager.ResourceType resourceType)
        {
            return resourceDisplays.TryGetValue(resourceType, out ResourceDisplay display) ? display : null;
        }

        /// <summary>
        /// Set UI interactable state
        /// </summary>
        public void SetUIInteractable(bool interactable)
        {
            // Set all UI elements interactable state
            var buttons = FindObjectsByType<Button>(FindObjectsSortMode.None); 
            foreach (var button in buttons)
            {
                button.interactable = interactable;
            }
        }
    }

    /// <summary>
    /// Data structure for habit information
    /// </summary>
    [System.Serializable]
    public class HabitData
    {
        public string habitName;
        public string displayName;
        public bool isCompleted;
        public Sprite icon;
    }
} 