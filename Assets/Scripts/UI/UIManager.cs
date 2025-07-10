using LifeCraft.Core;
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
        [SerializeField] private Dictionary<ResourceManager.ResourceType, ResourceDisplay> resourceDisplays;

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

        // References
        private ResourceManager resourceManager;
        private CityBuilder cityBuilder;
        private Systems.UnlockSystem unlockSystem;

        private void Awake()
        {
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
        private void InitializeUI()
        {
            // Get references
            resourceManager = ResourceManager.Instance;
            resourceManager.Initialize();
            cityBuilder = FindFirstObjectByType<LifeCraft.Core.CityBuilder>();
            unlockSystem = FindFirstObjectByType<Systems.UnlockSystem>();

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

            // Create buttons for unlocked buildings
            if (unlockSystem != null)
            {
                var unlockedBuildings = unlockSystem.GetUnlockedBuildings();
                foreach (var building in unlockedBuildings)
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
                buildingPanel.SetActive(true);
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
        /// Refresh building panel
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