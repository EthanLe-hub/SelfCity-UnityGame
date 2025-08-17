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

        [Header("Premium Features")]
        [SerializeField] private GameObject premiumDecorChestButton;
        [SerializeField] private GameObject premiumJournalButton;
        [SerializeField] private GameObject premiumFriendsButton;
        [SerializeField] private GameObject premiumBadge; // General premium indicator

        [Header("Premium Upgrade")]
        [SerializeField] private GameObject premiumUpgradePanel;
        [SerializeField] private Button upgradeToPremiumButton;
        [SerializeField] private Button closeUpgradePanelButton;
        [SerializeField] private TMP_Text premiumFeaturesDescriptionText; // Add this for feature descriptions

        [Header("Subscription Status")]
        [SerializeField] private TMP_Text subscriptionStatusText;
        [SerializeField] private Button subscriptionInfoButton;

        [Header("Authentication")]
        [SerializeField] private GameObject authenticationUI;
        [SerializeField] private bool requireAuthentication = true;

        // Private fields
        private Dictionary<ResourceManager.ResourceType, ResourceDisplay> resourceDisplays;
        private List<GameObject> buildingButtons = new List<GameObject>();
        private ResourceManager resourceManager;
        private CityBuilder cityBuilder;
        private AuthenticationManager authManager;

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
            authManager = AuthenticationManager.Instance;

            // Check authentication first
            if (requireAuthentication && authManager != null && !authManager.IsAuthenticated)
            {
                ShowAuthenticationUI();
                return;
            }

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
            Debug.Log("=== SETUP BUILDING PANEL ===");
            
            if (buildingButtonContainer == null || buildingButtonPrefab == null)
            {
                Debug.LogError("SetupBuildingPanel: buildingButtonContainer or buildingButtonPrefab is null!");
                return;
            }

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
                
                Debug.Log($"SetupBuildingPanel: Found {unlockedRegions.Count} unlocked regions and {allBuildings.Count} total buildings");
                
                int buildingsCreated = 0;
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
                            buildingsCreated++;
                            Debug.Log($"Created building button for: {building.name}");
                        }
                    }
                }
                Debug.Log($"SetupBuildingPanel: Created {buildingsCreated} building buttons");
            }
            else
            {
                Debug.LogError("SetupBuildingPanel: buildingShopDatabase or GameManager.Instance.RegionUnlockSystem is null!");
            }
            
            Debug.Log("=== END SETUP BUILDING PANEL ===");
        }

        /// <summary>
        /// Check if a building belongs to an unlocked region
        /// </summary>
        private bool IsBuildingFromUnlockedRegion(string buildingName, List<AssessmentQuizManager.RegionType> unlockedRegions)
        {
            Debug.Log($"Checking if building '{buildingName}' belongs to unlocked regions: {string.Join(", ", unlockedRegions.ConvertAll(r => AssessmentQuizManager.GetRegionDisplayName(r)))}");
            
            // Try to get the actual region from the building shop database first
            if (buildingShopDatabase != null)
            {
                // Look for the building in the shop database to get its actual region
                var buildingItem = buildingShopDatabase.buildings.Find(b => b.name == buildingName);
                if (buildingItem != null)
                {
                    // Convert the building's region to AssessmentQuizManager.RegionType
                    var buildingRegion = GetBuildingRegionFromShopItem(buildingItem);
                    bool isUnlocked = unlockedRegions.Contains(buildingRegion);
                    Debug.Log($"Building '{buildingName}' found in shop database, region: {buildingRegion}, isUnlocked: {isUnlocked}");
                    return isUnlocked;
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
                bool isUnlocked = unlockedRegions.Contains(AssessmentQuizManager.RegionType.HealthHarbor);
                Debug.Log($"Building '{buildingName}' matched HealthHarbor pattern, isUnlocked: {isUnlocked}");
                return isUnlocked;
            }
            else if (buildingName.Contains("Meditation") || buildingName.Contains("Therapy") || 
                     buildingName.Contains("Gratitude") || buildingName.Contains("Boundary") || 
                     buildingName.Contains("Calm") || buildingName.Contains("Reflection") || 
                     buildingName.Contains("Monument") || buildingName.Contains("Tower") || 
                     buildingName.Contains("Maze") || buildingName.Contains("Library") || 
                     buildingName.Contains("Dream") || buildingName.Contains("Focus") || 
                     buildingName.Contains("Resilience"))
            {
                bool isUnlocked = unlockedRegions.Contains(AssessmentQuizManager.RegionType.MindPalace);
                Debug.Log($"Building '{buildingName}' matched MindPalace pattern, isUnlocked: {isUnlocked}");
                return isUnlocked;
            }
            else if (buildingName.Contains("Writer") || buildingName.Contains("Art") || 
                     buildingName.Contains("Expression") || buildingName.Contains("Amphitheater") || 
                     buildingName.Contains("Innovation") || buildingName.Contains("Style") || 
                     buildingName.Contains("Music") || buildingName.Contains("Maker") || 
                     buildingName.Contains("Inspiration") || buildingName.Contains("Animation") || 
                     buildingName.Contains("Design") || buildingName.Contains("Sculpture") || 
                     buildingName.Contains("Film"))
            {
                bool isUnlocked = unlockedRegions.Contains(AssessmentQuizManager.RegionType.CreativeCommons);
                Debug.Log($"Building '{buildingName}' matched CreativeCommons pattern, isUnlocked: {isUnlocked}");
                return isUnlocked;
            }
            else if (buildingName.Contains("Friendship") || buildingName.Contains("Kindness") || 
                     buildingName.Contains("Community") || buildingName.Contains("Cultural") || 
                     buildingName.Contains("Game") || buildingName.Contains("Coffee") || 
                     buildingName.Contains("Family") || buildingName.Contains("Support") || 
                     buildingName.Contains("Stage") || buildingName.Contains("Volunteer") || 
                     buildingName.Contains("Celebration") || buildingName.Contains("Pet") || 
                     buildingName.Contains("Teamwork"))
            {
                bool isUnlocked = unlockedRegions.Contains(AssessmentQuizManager.RegionType.SocialSquare);
                Debug.Log($"Building '{buildingName}' matched SocialSquare pattern, isUnlocked: {isUnlocked}");
                return isUnlocked;
            }

            // Default to true if no match found (for decorations and other items)
            Debug.Log($"Building '{buildingName}' didn't match any region pattern, defaulting to true");
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

            // Setup authentication events
            if (authManager != null)
            {
                authManager.OnUserSignedIn.AddListener(OnUserSignedIn);
                authManager.OnUserSignedOut.AddListener(OnUserSignedOut);
                // Refresh premium visuals whenever subscription changes (from Auth)
                authManager.OnSubscriptionStatusChanged.AddListener(_ =>
                {
                    UpdatePremiumIndicators();
                    UpdateSubscriptionStatus();
                });
            }

            // Listen to SubscriptionManager events as the source of feature gating
            if (SubscriptionManager.Instance != null)
            {
                SubscriptionManager.Instance.OnSubscriptionStatusChanged.AddListener(_ =>
                {
                    UpdatePremiumIndicators();
                    UpdateSubscriptionStatus();
                });
            }

            // Setup premium upgrade panel events
            if (upgradeToPremiumButton != null)
                upgradeToPremiumButton.onClick.AddListener(OnUpgradeToPremiumClicked);

            if (closeUpgradePanelButton != null)
                closeUpgradePanelButton.onClick.AddListener(HidePremiumUpgradePanel);
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
        /// Handle user signed in event
        /// </summary>
        private void OnUserSignedIn(AuthenticationManager.AuthUser user)
        {
            Debug.Log($"User signed in: {user.displayName}");
            HideAuthenticationUI();
            InitializeUI(); // Re-initialize UI now that user is authenticated
        }

        /// <summary>
        /// Handle user signed out event
        /// </summary>
        private void OnUserSignedOut()
        {
            Debug.Log("User signed out");
            ShowAuthenticationUI();
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
            Debug.Log("=== UIMANAGER REFRESH BUILDING PANEL ===");
            Debug.Log($"RefreshBuildingPanel called at: {System.DateTime.Now}");
            
            // Check RegionUnlockSystem state
            if (GameManager.Instance?.RegionUnlockSystem != null)
            {
                var unlockedRegions = GameManager.Instance.RegionUnlockSystem.GetUnlockedRegions();
                Debug.Log($"UIManager: RegionUnlockSystem reports {unlockedRegions.Count} unlocked regions: {string.Join(", ", unlockedRegions.ConvertAll(r => AssessmentQuizManager.GetRegionDisplayName(r)))}");
            }
            else
            {
                Debug.LogError("UIManager: GameManager.Instance or RegionUnlockSystem is null!");
            }
            
            SetupBuildingPanel();
            Debug.Log("=== END UIMANAGER REFRESH BUILDING PANEL ===");
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
        /// Set UI interactability
        /// </summary>
        public void SetUIInteractable(bool interactable)
        {
            // Disable/enable main UI panels
            if (cityPanel != null) cityPanel.SetActive(interactable);
            if (homePanel != null) homePanel.SetActive(interactable);
            if (shopPanel != null) shopPanel.SetActive(interactable);
            if (profilePanel != null) profilePanel.SetActive(interactable);
        }

        /// <summary>
        /// Show the authentication UI if authentication is required and not authenticated.
        /// </summary>
        private void ShowAuthenticationUI()
        {
            if (authenticationUI != null)
            {
                authenticationUI.SetActive(true);
                // Optionally, you might want to disable other UI elements while showing the auth UI
                SetUIInteractable(false);
            }
        }

        /// <summary>
        /// Hide the authentication UI.
        /// </summary>
        private void HideAuthenticationUI()
        {
            if (authenticationUI != null)
            {
                authenticationUI.SetActive(false);
                // Re-enable other UI elements
                SetUIInteractable(true);
            }
        }

        /// <summary>
        /// Update premium feature indicators
        /// </summary>
        public void UpdatePremiumIndicators()
        {
            bool hasPremium = false;
            
            // Use ONLY SubscriptionManager as the single source of truth
            if (SubscriptionManager.Instance != null)
            {
                hasPremium = SubscriptionManager.Instance.HasActiveSubscription();
            }
            else
            {
                Debug.LogWarning("[UIManager] SubscriptionManager.Instance is null - defaulting to Free status");
            }

            Debug.Log($"[UIManager] UpdatePremiumIndicators - hasPremium = {hasPremium} (from SubscriptionManager)");

            // Update premium badge
            if (premiumBadge != null)
            {
                premiumBadge.SetActive(hasPremium);
                Debug.Log($"[UIManager] Updated premium badge visibility to: {hasPremium}");
            }

            // Update premium feature buttons
            // Note: Premium Decor Chest button is always clickable to show upgrade message
            if (premiumDecorChestButton != null)
            {
                var button = premiumDecorChestButton.GetComponent<Button>();
                if (button != null)
                {
                    // Always keep Premium Decor Chest button clickable so free users can see upgrade message
                    button.interactable = true;
                    
                    // Keep the button appearance consistent with Decor Chest button
                    // No visual changes based on subscription status
                }
            }

            if (premiumJournalButton != null)
            {
                var button = premiumJournalButton.GetComponent<Button>();
                if (button != null)
                    button.interactable = hasPremium;
            }

            if (premiumFriendsButton != null)
            {
                var button = premiumFriendsButton.GetComponent<Button>();
                if (button != null)
                    button.interactable = hasPremium;
            }

            // Update upgrade button visibility (show for free users, hide for premium users)
            if (upgradeToPremiumButton != null)
            {
                upgradeToPremiumButton.gameObject.SetActive(!hasPremium);
                Debug.Log($"[UIManager] Updated upgrade button visibility to: {!hasPremium}");
            }
        }

        /// <summary>
        /// Show premium upgrade prompt
        /// </summary>
        public void ShowPremiumUpgradePrompt(string featureName)
        {
            bool hasPremium = false;
            if (authManager != null)
            {
                hasPremium = authManager.HasPremiumSubscription; // property, not method
            }
            else if (SubscriptionManager.Instance != null)
            {
                hasPremium = SubscriptionManager.Instance.HasActiveSubscription();
            }

            if (hasPremium)
            {
                // User already has premium, no need to show prompt
                return;
            }

            if (premiumUpgradePanel != null)
            {
                premiumUpgradePanel.SetActive(true);
                ShowNotification($"Upgrade to Premium to access {featureName}!");
            }
        }

        /// <summary>
        /// Hide premium upgrade prompt
        /// </summary>
        public void HidePremiumUpgradePrompt()
        {
            if (premiumUpgradePanel != null)
                premiumUpgradePanel.SetActive(false);
        }

        /// <summary>
        /// Handle premium decor chest access
        /// </summary>
        public void OnPremiumDecorChestClicked()
        {
            if (SubscriptionManager.Instance != null && SubscriptionManager.Instance.HasPremiumDecorChestAccess())
            {
                // User has access, proceed with decor chest
                Debug.Log("Opening Premium Decor Chest...");
                // TODO: Open decor chest
            }
            else
            {
                ShowPremiumUpgradePrompt("Premium Decor Chest");
            }
        }

        /// <summary>
        /// Handle premium journal features access
        /// </summary>
        public void OnPremiumJournalClicked()
        {
            if (SubscriptionManager.Instance != null && SubscriptionManager.Instance.HasPremiumJournalAccess())
            {
                // User has access, proceed with premium journal features
                Debug.Log("Opening Premium Journal Features...");
                // TODO: Open premium journal features
            }
            else
            {
                ShowPremiumUpgradePrompt("Premium Journal Features");
            }
        }

        /// <summary>
        /// Handle premium friends access
        /// </summary>
        public void OnPremiumFriendsClicked()
        {
            if (SubscriptionManager.Instance != null && SubscriptionManager.Instance.HasUnlimitedFriends())
            {
                // User has access, proceed with unlimited friends
                Debug.Log("Opening Unlimited Friends...");
                // TODO: Open friends system
            }
            else
            {
                ShowPremiumUpgradePrompt("Unlimited Friends");
            }
        }

        /// <summary>
        /// Update subscription status display
        /// </summary>
        public void UpdateSubscriptionStatus()
        {
            if (subscriptionStatusText != null)
            {
                if (SubscriptionManager.Instance != null)
                {
                    var (isActive, type, expiry, price) = SubscriptionManager.Instance.GetSubscriptionInfo();
                    if (isActive)
                    {
                        int daysRemaining = SubscriptionManager.Instance.GetDaysRemaining();
                        subscriptionStatusText.text = $"Premium ({type}) - {daysRemaining} days remaining";
                    }
                    else
                    {
                        subscriptionStatusText.text = "Free";
                    }
                }
                else
                {
                    subscriptionStatusText.text = "Free";
                }
            }
        }

        /// <summary>
        /// Show premium upgrade panel with feature descriptions
        /// </summary>
        public void ShowPremiumUpgradePanel()
        {
            if (premiumUpgradePanel != null)
            {
                premiumUpgradePanel.SetActive(true);
                UpdatePremiumFeaturesDescription();
            }
        }

        /// <summary>
        /// Hide premium upgrade panel
        /// </summary>
        public void HidePremiumUpgradePanel()
        {
            if (premiumUpgradePanel != null)
                premiumUpgradePanel.SetActive(false);
        }

        /// <summary>
        /// Update premium features description text
        /// </summary>
        private void UpdatePremiumFeaturesDescription()
        {
            if (premiumFeaturesDescriptionText != null)
            {
                string featuresDescription = @"✨ Premium Features:

🎨 Premium Decor Chest
• Exclusive premium decorations
• Higher chance for rare items

📝 Enhanced Journal Features
• Daily prompt recommendations
• Drawing feature in entries
• Media attachments (photos, audio)

⚡ Faster Construction
• 20% faster building construction
• Get your city built quicker

💎 Resource Bonuses
• 50% bonus on all resources earned
• More materials for building

👥 Unlimited Friends
• No 20 friend limit
• Connect with unlimited players

🚫 Ad-Free Experience
• No advertisements
• Uninterrupted gameplay

Upgrade now for just $6.99/month!";

                premiumFeaturesDescriptionText.text = featuresDescription;
            }
        }

        /// <summary>
        /// Handle upgrade to premium button click
        /// </summary>
        private async void OnUpgradeToPremiumClicked()
        {
            // Safety: If the PremiumUpgradePanel is NOT open, treat this as a request to open it only.
            if (premiumUpgradePanel == null || !premiumUpgradePanel.activeInHierarchy)
            {
                ShowPremiumUpgradePanel();
                return;
            }

            // At this point, the panel is open and the click comes from the panel's Upgrade button → proceed to purchase
            if (authManager != null)
            {
                bool success = await authManager.PurchasePremiumSubscription();
                if (success)
                {
                    Debug.Log("Premium upgrade successful!");

                    // Keep feature gating in sync with the SubscriptionManager (simulation layer)
                    if (SubscriptionManager.Instance != null && !SubscriptionManager.Instance.HasActiveSubscription())
                    {
                        // Prefer monthly simulation purchase to mark subscription active and fire events
                        await SubscriptionManager.Instance.PurchaseMonthlySubscription();
                    }

                    HidePremiumUpgradePanel();
                    UpdatePremiumIndicators();
                    UpdateSubscriptionStatus();
                    ShowNotification("Welcome to Premium! All premium features are now unlocked.");
                }
                else
                {
                    Debug.LogError("Premium upgrade failed");
                    ShowNotification("Upgrade failed. Please try again.");
                }
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