using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // Needed for List<string>
using LifeCraft.Core;
using LifeCraft.Systems;
using LifeCraft.Buildings;
using LifeCraft.Shop;

namespace LifeCraft.UI
{
    /// <summary>
    /// Handles action menu interactions for placed buildings and decorations.
    /// Shows action menu with 5 options: Store, Sell, Confirm, Rotate, Reset.
    /// Called from PlacedItemUI when in edit mode.
    /// </summary>
    public class HoldDownInteraction : MonoBehaviour
    {
        // Hold configuration removed - no longer needed
        
        [Header("Action Menu")]
        [SerializeField] public GameObject actionMenuPrefab;
        [Tooltip("Enter the exact name of the parent GameObject (e.g., 'ActionMenu_Parent')")]
        public string actionMenuParentName = "ActionMenu_Parent";
        
        // Cache for the parent transform
        private Transform cachedActionMenuParent;
        
        [Header("Item Data")]
        public bool isBuilding = true; // Whether this is a building (has construction time) or decoration
        public string itemName;
        public Vector3 originalPosition;
        public Quaternion originalRotation;
        public int originalCost;
        public ResourceManager.ResourceType originalCurrency;
        public string regionType;
        
        // Hold-down variables removed - no longer needed
        private GameObject currentActionMenu;
        private ActionMenuUI actionMenuUI;
        
        private void Start()
        {
            Debug.Log($"HoldDownInteraction Start() called on {gameObject.name}");
            originalPosition = transform.position;
            originalRotation = transform.rotation;
            InitializeItemData();
            
            // Ensure we have the required components for pointer events
            var graphic = GetComponent<Graphic>();
            if (graphic == null)
            {
                Debug.LogWarning($"HoldDownInteraction on {gameObject.name} has no Graphic component. Adding Image component for pointer events.");
                var image = gameObject.AddComponent<Image>();
                image.color = new Color(0, 0, 0, 0); // Transparent
            }
        }
        
        /// <summary>
        /// Initialize item data from existing components
        /// </summary>
        private void InitializeItemData()
        {
            // Try to get data from PlacedItemUI
            var placedItemUI = GetComponent<PlacedItemUI>();
            if (placedItemUI != null)
            {
                var decorationItem = placedItemUI.GetDecorationItem();
                if (decorationItem != null)
                {
                    itemName = decorationItem.displayName;
                    // Check if this is a building by looking at the region type
                    isBuilding = (decorationItem.region != RegionType.Decoration);
                    
                    if (isBuilding)
                    {
                        // Get building price from shop database
                        var buildingPrice = GetBuildingPriceFromShopDatabase(decorationItem.displayName, decorationItem.region);
                        originalCost = buildingPrice.price;
                        originalCurrency = GetCurrencyForRegion(decorationItem.region);
                        Debug.Log($"HoldDownInteraction: {itemName} isBuilding={isBuilding}, price={originalCost}, currency={originalCurrency}");
                    }
                    else
                    {
                        // Decorations don't have cost/currency in the current system
                        originalCost = 0;
                        originalCurrency = ResourceManager.ResourceType.EnergyCrystals; // Default
                        Debug.Log($"HoldDownInteraction: {itemName} isBuilding={isBuilding} (decoration)");
                    }
                    
                    regionType = decorationItem.region.ToString();
                }
            }
            
            // Try to get data from Building component
            var building = GetComponent<Building>();
            if (building != null)
            {
                itemName = building.BuildingName; // Use property instead of method
                isBuilding = true;
                
                // Get building price from shop database
                var buildingPrice = GetBuildingPriceFromShopDatabase(itemName, RegionType.HealthHarbor); // Default to HealthHarbor
                originalCost = buildingPrice.price;
                originalCurrency = buildingPrice.currency;
                Debug.Log($"HoldDownInteraction: {itemName} isBuilding={isBuilding}, price={originalCost}, currency={originalCurrency}");
            }
        }
        
        /// <summary>
        /// Public method to initialize item data from CityBuilder
        /// </summary>
        public void InitializeItemData(bool isBuildingItem, string itemName, int cost, ResourceManager.ResourceType currency)
        {
            this.isBuilding = isBuildingItem;
            this.itemName = itemName;
            this.originalCost = cost;
            this.originalCurrency = currency;
            
            Debug.Log($"HoldDownInteraction initialized: isBuilding={isBuildingItem}, itemName={itemName}, cost={cost}, currency={currency}");
        }
        
        /// <summary>
        /// Public method to initialize item data from shop database
        /// </summary>
        public void InitializeItemDataFromShop(string itemName, RegionType region)
        {
            this.itemName = itemName;
            this.isBuilding = (region != RegionType.Decoration);
            
            if (isBuilding)
            {
                var buildingPrice = GetBuildingPriceFromShopDatabase(itemName, region);
                this.originalCost = buildingPrice.price;
                this.originalCurrency = buildingPrice.currency;
                Debug.Log($"HoldDownInteraction initialized from shop: isBuilding={isBuilding}, itemName={itemName}, cost={originalCost}, currency={originalCurrency}");
            }
            else
            {
                this.originalCost = 0;
                this.originalCurrency = ResourceManager.ResourceType.EnergyCrystals;
                Debug.Log($"HoldDownInteraction initialized from shop: isBuilding={isBuilding}, itemName={itemName} (decoration)");
            }
        }
        
        // Hold-down functionality removed - action menu is now shown immediately in edit mode via PlacedItemUI.OnPointerClick
        
        /// <summary>
        /// Show the action menu for this item
        /// </summary>
        public void ShowActionMenu()
        {
            Debug.Log($"ShowActionMenu called for {gameObject.name}");
            HideActionMenu();
            
            if (actionMenuPrefab != null)
            {
                Debug.Log($"Action menu prefab is assigned: {actionMenuPrefab.name}");
                
                if (cachedActionMenuParent == null)
                {
                    // Find the parent in the Canvas hierarchy
                    Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
                    foreach (Canvas canvas in canvases)
                    {
                        Transform parent = canvas.transform.Find(actionMenuParentName);
                        if (parent != null)
                        {
                            cachedActionMenuParent = parent;
                            Debug.Log($"Found action menu parent: {parent.name}");
                            break;
                        }
                    }
                    
                    // If parent not found, create it
                    if (cachedActionMenuParent == null)
                    {
                        cachedActionMenuParent = CreateActionMenuParent();
                    }
                }
                
                if (cachedActionMenuParent != null)
                {
                    currentActionMenu = Instantiate(actionMenuPrefab, cachedActionMenuParent);
                    
                    // Debug: Check if the Action Menu was created
                    Debug.Log($"Action Menu GameObject created: {currentActionMenu.name}");
                    Debug.Log($"Action Menu active in hierarchy: {currentActionMenu.activeInHierarchy}");
                    Debug.Log($"Action Menu has RectTransform: {currentActionMenu.GetComponent<RectTransform>() != null}");
                    Debug.Log($"Action Menu has ActionMenuUI script: {currentActionMenu.GetComponent<ActionMenuUI>() != null}");
                    
                    // Position the action menu near the clicked item
                    var rectTransform = currentActionMenu.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        // Ensure the Action Menu has a proper size
                        if (rectTransform.rect.width < 100f || rectTransform.rect.height < 100f)
                        {
                            rectTransform.sizeDelta = new Vector2(200f, 300f);
                            Debug.Log($"Set Action Menu size to {rectTransform.sizeDelta}");
                        }
                        
                        // Get the clicked item's RectTransform for proper UI positioning
                        var itemRectTransform = GetComponent<RectTransform>();
                        if (itemRectTransform != null)
                        {
                            // Position the menu relative to the item in UI space
                            Vector3 itemPosition = itemRectTransform.position;
                            Vector3 menuPosition = itemPosition + Vector3.up * 150f; // Offset upward
                            
                            // Ensure the menu is within screen bounds
                            float screenHeight = Screen.height;
                            float menuHeight = rectTransform.rect.height;
                            if (menuPosition.y + menuHeight > screenHeight)
                            {
                                // Move menu below the item if it would go off-screen above
                                menuPosition = itemPosition + Vector3.down * 150f;
                            }
                            
                            rectTransform.position = menuPosition;
                            
                            Debug.Log($"Action menu positioned at UI position {itemPosition} + offset");
                            Debug.Log($"Action menu final position: {rectTransform.position}");
                        }
                        else
                        {
                            // Fallback: try to use world position if no RectTransform
                            Vector3 worldPos = transform.position;
                            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
                            rectTransform.position = screenPos + Vector3.up * 100f;
                            
                            Debug.Log($"Action menu positioned at screen position {screenPos} (fallback)");
                            Debug.Log($"Action menu final position: {rectTransform.position}");
                        }
                        
                        // Make sure the action menu is visible and on top
                        currentActionMenu.SetActive(true);
                        
                        // Set the canvas to be on top
                        var canvas = currentActionMenu.GetComponent<Canvas>();
                        if (canvas != null)
                        {
                            canvas.sortingOrder = 999;
                            Debug.Log($"Set Action Menu Canvas sorting order to 999");
                        }
                        
                        // Also ensure the parent canvas is on top
                        var parentCanvas = cachedActionMenuParent.GetComponentInParent<Canvas>();
                        if (parentCanvas != null)
                        {
                            parentCanvas.sortingOrder = 998;
                            Debug.Log($"Set parent Canvas sorting order to 998");
                        }
                        
                        Debug.Log($"Action menu positioned and set active. Active: {currentActionMenu.activeInHierarchy}");
                    }
                    else
                    {
                        Debug.LogError("Action menu has no RectTransform component!");
                    }
                    
                    actionMenuUI = currentActionMenu.GetComponent<ActionMenuUI>();
                    if (actionMenuUI != null)
                    {
                        actionMenuUI.Initialize(this);
                        Debug.Log($"Action menu created and initialized for {gameObject.name}");
                    }
                    else
                    {
                        Debug.LogError("ActionMenuUI component not found on actionMenuPrefab!");
                        
                        // Try to add the ActionMenuUI script if it's missing
                        actionMenuUI = currentActionMenu.AddComponent<ActionMenuUI>();
                        if (actionMenuUI != null)
                        {
                            Debug.Log("Added ActionMenuUI script to Action Menu");
                            actionMenuUI.Initialize(this);
                        }
                        else
                        {
                            Debug.LogError("Failed to add ActionMenuUI script to Action Menu!");
                        }
                    }
                }
                else
                {
                    Debug.LogError($"Action menu parent '{actionMenuParentName}' not found in any Canvas!");
                }
            }
            else
            {
                Debug.LogError($"Action menu prefab is not assigned on {gameObject.name}! Please assign it in the Inspector.");
            }
        }
        
        /// <summary>
        /// Hide the action menu
        /// </summary>
        public void HideActionMenu()
        {
            if (currentActionMenu != null)
            {
                Destroy(currentActionMenu);
                currentActionMenu = null;
                actionMenuUI = null;
            }
        }
        
        /// <summary>
        /// Store item back to inventory
        /// </summary>
        public void StoreToInventory()
        {
            // Get inventory manager
            var inventoryManager = InventoryManager.Instance;
            if (inventoryManager == null)
            {
                Debug.LogError("InventoryManager not found!");
                return;
            }
            
            // Check if this building is under construction and save progress
            var constructionTimer = GetComponent<BuildingConstructionTimer>();
            if (constructionTimer != null && constructionTimer.IsUnderConstruction())
            {
                // Pause the construction timer UI
                constructionTimer.PauseConstruction();
                
                // Save and pause the construction progress
                SaveConstructionProgress();
            }
            
            // Get the original decoration item to preserve its region type
            var placedItemUI = GetComponent<PlacedItemUI>();
            DecorationItem decorationItem = null;
            
            if (placedItemUI != null)
            {
                decorationItem = placedItemUI.GetDecorationItem();
                if (decorationItem != null)
                {
                    // Clone the existing decoration item to preserve all its properties including region type
                    decorationItem = decorationItem.Clone();
                    Debug.Log($"Storing building {itemName} with region type: {decorationItem.region}");
                }
            }
            
            // If we couldn't get the original decoration item, create a new one with the correct region type
            if (decorationItem == null)
            {
                // Determine the region type based on the building name or stored region type
                RegionType regionType = DetermineRegionTypeFromBuildingName(itemName);
                decorationItem = new DecorationItem(itemName, "", DecorationRarity.Common, false);
                decorationItem.region = regionType;
                Debug.Log($"Created new decoration item for {itemName} with region type: {regionType}");
            }
            
            // Add to inventory
            inventoryManager.AddDecoration(decorationItem);
            
            // Remove from world
            RemoveFromWorld();
            
            Debug.Log($"Stored {itemName} to inventory with region type: {decorationItem.region}");
        }
        
        /// <summary>
        /// Save construction progress before storing the building
        /// </summary>
        private void SaveConstructionProgress()
        {
            if (ConstructionManager.Instance == null) return;
            
            // Get the current grid position
            Vector3Int gridPosition = new Vector3Int(
                Mathf.RoundToInt(transform.position.x),
                Mathf.RoundToInt(transform.position.y),
                0
            );
            
            // Get the construction project
            ConstructionProject project = ConstructionManager.Instance.GetProject(itemName, gridPosition);
            if (project != null && !project.isCompleted)
            {
                // Calculate remaining time
                float elapsedTime = Time.time - project.startTime;
                float remainingTime = project.constructionDuration - elapsedTime;
                
                // New: Ensure active skip quests are removed from To-Do when storing the building,
                // but preserve the master list so they can be re-added later.
                ConstructionManager.Instance.RemoveActiveQuestsFromToDo(itemName, gridPosition);

                // PAUSE THE CONSTRUCTION: Remove the project from active projects
                ConstructionManager.Instance.PauseConstruction(itemName, gridPosition);
                
                // Save construction progress to the decoration item
                var placedItemUI = GetComponent<PlacedItemUI>();
                if (placedItemUI != null)
                {
                    var decorationItem = placedItemUI.GetDecorationItem();
                    if (decorationItem != null)
                    {
                        // Get the current Skip button text
                        var constructionTimer = GetComponent<BuildingConstructionTimer>();
                        string currentSkipButtonText = "";
                        if (constructionTimer != null && constructionTimer.skipButtonText != null)
                        {
                            currentSkipButtonText = constructionTimer.skipButtonText.text;
                        }
                        
                        // Store construction progress in the decoration item
                        decorationItem.constructionProgress = remainingTime;
                        decorationItem.constructionStartTime = project.startTime;
                        decorationItem.constructionDuration = project.constructionDuration;
                        decorationItem.originalQuestTexts = new List<string>(project.originalQuestTexts);
                        decorationItem.activeQuestTexts = new List<string>(project.activeQuestTexts);
                        decorationItem.completedSkipQuests = project.completedSkipQuests;
                        decorationItem.totalSkipQuests = project.totalSkipQuests;
                        decorationItem.skipButtonText = currentSkipButtonText;
                        
                        Debug.Log($"PAUSED construction for {itemName}: {remainingTime:F1}s remaining, {project.activeQuestTexts.Count} active quests, Skip button: '{currentSkipButtonText}'");
                    }
                }
            }
        }
        
        /// <summary>
        /// Sell item for 50% of original cost
        /// </summary>
        public void SellItem()
        {
            // Find the confirmation modal - search for inactive objects too
            var confirmModals = FindObjectsByType<PurchaseConfirmModal>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            PurchaseConfirmModal confirmModal = null;
            
            if (confirmModals.Length > 0)
            {
                confirmModal = confirmModals[0];
                Debug.Log($"Found PurchaseConfirmModal: {confirmModal.name}");
            }
            
            if (confirmModal == null)
            {
                Debug.LogError("PurchaseConfirmModal not found in scene! Cannot show sell confirmation.");
                return;
            }
            
            // Get item sprite for the confirmation dialog
            Sprite itemSprite = null;
            
            // Try to get sprite from CityBuilder (works for both buildings and decorations)
            if (CityBuilder.Instance != null)
            {
                var buildingData = CityBuilder.Instance.GetBuildingTypeData(itemName);
                if (buildingData != null && buildingData.buildingSprite != null)
                {
                    itemSprite = buildingData.buildingSprite;
                }
            }
            
            string message;
            if (isBuilding)
            {
                // Buildings sell for 50% of their original price from shop database
                int sellAmount = Mathf.RoundToInt(originalCost * 0.5f);
                message = $"Sell {itemName} for {sellAmount} {originalCurrency}?";
                
                confirmModal.Show(message, () => {
                    // Execute the sale
                    ResourceManager.Instance.AddResources(originalCurrency, sellAmount);
                    Debug.Log($"Sold {itemName} for {sellAmount} {originalCurrency} (50% of original price {originalCost})");
                    
                    // CRITICAL FIX: Remove construction project data when selling building
                    // This prevents old construction data from being reused by future buildings
                    if (ConstructionManager.Instance != null)
                    {
                        Vector3Int gridPosition = new Vector3Int(
                            Mathf.RoundToInt(transform.position.x),
                            Mathf.RoundToInt(transform.position.y),
                            0
                        );
                        ConstructionManager.Instance.RemoveConstructionProject(itemName, gridPosition);
                        Debug.Log($"Removed construction project for {itemName} at {gridPosition}");
                    }
                    
                    RemoveFromWorld();
                }, itemSprite);
            }
            else
            {
                // Decorations sell for 10 of a random region currency
                var randomCurrency = GetRandomRegionCurrency();
                message = $"Sell {itemName} for 10 {randomCurrency}?";
                
                confirmModal.Show(message, () => {
                    // Execute the sale
                    ResourceManager.Instance.AddResources(randomCurrency, 10);
                    Debug.Log($"Sold {itemName} for 10 {randomCurrency}");
                    RemoveFromWorld();
                }, itemSprite);
            }
        }
        
        /// <summary>
        /// Confirm the current position (finalize placement)
        /// </summary>
        public void ConfirmPosition()
        {
            Debug.Log($"ConfirmPosition called for {gameObject.name}");
            
            // Store the current position as the finalized position
            originalPosition = transform.position;
            originalRotation = transform.rotation;
            
            Debug.Log($"Position finalized for {gameObject.name} at {originalPosition}");
        }
        
        /// <summary>
        /// Rotate item 90 degrees
        /// </summary>
        public void RotateItem()
        {
            transform.Rotate(0, 0, 90);
            Debug.Log($"Rotated {itemName}");
        }
        
        /// <summary>
        /// Reset to the previous finalized position
        /// </summary>
        public void ResetPosition()
        {
            Debug.Log($"ResetPosition called for {gameObject.name}");
            
            // Reset to the last finalized position
            transform.position = originalPosition;
            transform.rotation = originalRotation;
            
            Debug.Log($"Position reset for {gameObject.name} to {originalPosition}");
        }
        
        /// <summary>
        /// Get random region currency for decoration sales
        /// </summary>
        private ResourceManager.ResourceType GetRandomRegionCurrency()
        {
            var currencies = new ResourceManager.ResourceType[]
            {
                ResourceManager.ResourceType.EnergyCrystals,
                ResourceManager.ResourceType.WisdomOrbs,
                ResourceManager.ResourceType.HeartTokens,
                ResourceManager.ResourceType.CreativitySparks
            };
            
            int randomIndex = Random.Range(0, currencies.Length);
            return currencies[randomIndex];
        }
        
        /// <summary>
        /// Remove item from world
        /// </summary>
        private void RemoveFromWorld()
        {
            // Notify CityBuilder to remove from tracking
            if (CityBuilder.Instance != null)
            {
                Vector3Int gridPosition = CityBuilder.Instance.WorldToMap(transform.position);
                CityBuilder.Instance.RemoveBuilding(gridPosition);
            }
            
            // Destroy the GameObject
            Destroy(gameObject);
        }
        
        // Update method removed - no longer needed since hold-down functionality was removed
        
        /// <summary>
        /// Get item name
        /// </summary>
        public string GetItemName()
        {
            return itemName;
        }
        
        /// <summary>
        /// Check if this is a building
        /// </summary>
        public bool IsBuilding()
        {
            return isBuilding;
        }
        
        /// <summary>
        /// Get building price from shop database
        /// </summary>
        private (int price, ResourceManager.ResourceType currency) GetBuildingPriceFromShopDatabase(string buildingName, RegionType region)
        {
            // Load the appropriate shop database based on region
            BuildingShopDatabase shopDatabase = null;
            
            switch (region)
            {
                case RegionType.HealthHarbor:
                    shopDatabase = Resources.Load<BuildingShopDatabase>("HealthHarborBuildings");
                    break;
                case RegionType.MindPalace:
                    shopDatabase = Resources.Load<BuildingShopDatabase>("MindPalaceBuildings");
                    break;
                case RegionType.CreativeCommons:
                    shopDatabase = Resources.Load<BuildingShopDatabase>("CreativeCommonsBuildings");
                    break;
                case RegionType.SocialSquare:
                    shopDatabase = Resources.Load<BuildingShopDatabase>("SocialSquareBuildings");
                    break;
                default:
                    // Default to HealthHarbor if region is unknown
                    shopDatabase = Resources.Load<BuildingShopDatabase>("HealthHarborBuildings");
                    break;
            }
            
            if (shopDatabase != null)
            {
                // Find the building in the shop database
                var buildingItem = shopDatabase.buildings.Find(b => b.name == buildingName);
                if (buildingItem != null)
                {
                    var currency = GetCurrencyForRegion(region);
                    Debug.Log($"Found building '{buildingName}' in {region} shop database, price: {buildingItem.price}");
                    return (buildingItem.price, currency);
                }
                else
                {
                    Debug.LogWarning($"Building '{buildingName}' not found in {region} shop database");
                }
            }
            else
            {
                Debug.LogError($"Shop database for region {region} not found!");
            }
            
            // Return default values if not found
            return (0, ResourceManager.ResourceType.EnergyCrystals);
        }
        
        /// <summary>
        /// Get the currency type for a region
        /// </summary>
        private ResourceManager.ResourceType GetCurrencyForRegion(RegionType region)
        {
            switch (region)
            {
                case RegionType.HealthHarbor:
                    return ResourceManager.ResourceType.EnergyCrystals;
                case RegionType.MindPalace:
                    return ResourceManager.ResourceType.WisdomOrbs;
                case RegionType.CreativeCommons:
                    return ResourceManager.ResourceType.CreativitySparks;
                case RegionType.SocialSquare:
                    return ResourceManager.ResourceType.HeartTokens;
                default:
                    return ResourceManager.ResourceType.EnergyCrystals;
            }
        }
        
        /// <summary>
        /// Determine the region type based on building name patterns
        /// </summary>
        private RegionType DetermineRegionTypeFromBuildingName(string buildingName)
        {
            if (string.IsNullOrEmpty(buildingName)) return RegionType.Decoration;
            
            // Check building name patterns to determine region (same logic as used elsewhere in the codebase)
            if (buildingName.Contains("Wellness") || buildingName.Contains("Yoga") || 
                buildingName.Contains("Juice") || buildingName.Contains("Sleep") || 
                buildingName.Contains("Nutrition") || buildingName.Contains("Spa") || 
                buildingName.Contains("Running") || buildingName.Contains("Therapy") || 
                buildingName.Contains("Biohacking") || buildingName.Contains("Aquatic") || 
                buildingName.Contains("Hydration") || buildingName.Contains("Fresh Air"))
            {
                return RegionType.HealthHarbor;
            }
            else if (buildingName.Contains("Meditation") || buildingName.Contains("Therapy") || 
                     buildingName.Contains("Gratitude") || buildingName.Contains("Boundary") || 
                     buildingName.Contains("Calm") || buildingName.Contains("Reflection") || 
                     buildingName.Contains("Monument") || buildingName.Contains("Tower") || 
                     buildingName.Contains("Maze") || buildingName.Contains("Library") || 
                     buildingName.Contains("Dream") || buildingName.Contains("Focus") || 
                     buildingName.Contains("Resilience"))
            {
                return RegionType.MindPalace;
            }
            else if (buildingName.Contains("Writer") || buildingName.Contains("Art") || 
                     buildingName.Contains("Expression") || buildingName.Contains("Amphitheater") || 
                     buildingName.Contains("Innovation") || buildingName.Contains("Style") || 
                     buildingName.Contains("Music") || buildingName.Contains("Maker") || 
                     buildingName.Contains("Inspiration") || buildingName.Contains("Animation") || 
                     buildingName.Contains("Design") || buildingName.Contains("Sculpture") || 
                     buildingName.Contains("Film"))
            {
                return RegionType.CreativeCommons;
            }
            else if (buildingName.Contains("Friendship") || buildingName.Contains("Kindness") || 
                     buildingName.Contains("Community") || buildingName.Contains("Cultural") || 
                     buildingName.Contains("Game") || buildingName.Contains("Coffee") || 
                     buildingName.Contains("Family") || buildingName.Contains("Support") || 
                     buildingName.Contains("Stage") || buildingName.Contains("Volunteer") || 
                     buildingName.Contains("Celebration") || buildingName.Contains("Pet") || 
                     buildingName.Contains("Teamwork"))
            {
                return RegionType.SocialSquare;
            }

            // Default to Health Harbor if no match found
            return RegionType.HealthHarbor;
        }
        
        /// <summary>
        /// Create the ActionMenu_Parent if it doesn't exist
        /// </summary>
        private Transform CreateActionMenuParent()
        {
            // Find existing Canvas
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            Canvas targetCanvas = null;
            
            // Prefer the main UI canvas (usually the first one)
            if (canvases.Length > 0)
            {
                targetCanvas = canvases[0];
                Debug.Log($"Using Canvas: {targetCanvas.name}");
            }
            
            if (targetCanvas == null)
            {
                Debug.LogError("No Canvas found in scene! Action menu cannot be created.");
                return null;
            }
            
            // Create new ActionMenu_Parent
            GameObject newParent = new GameObject(actionMenuParentName);
            newParent.transform.SetParent(targetCanvas.transform, false);
            
            // Set up the parent as a UI container
            RectTransform rectTransform = newParent.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            Debug.Log($"Created new ActionMenu_Parent: {newParent.name}");
            return newParent.transform;
        }
    }
} 