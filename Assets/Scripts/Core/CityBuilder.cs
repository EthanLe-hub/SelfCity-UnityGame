using LifeCraft.Systems;
using LifeCraft.Core;
using LifeCraft.Buildings;
using LifeCraft.UI; // Ensure all necessary namespaces are included for the CityBuilder functionality. 
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Events;

namespace LifeCraft.Core
{
    /// <summary>
    /// City building manager that handles building placement and city construction logic.
    /// Replaces the Godot CityBuilder.gd script.
    /// </summary>
    public class CityBuilder : MonoBehaviour
    {
        [System.Serializable]
        public class BuildingTypeData
        {
            public string buildingName;
            public GameObject buildingPrefab;
            public ResourceManager.ResourceType costResource;
            public int costAmount;
            public Sprite buildingSprite;
            
            [Header("Construction Time")]
            public bool hasConstructionTime = true; // Only buildings have construction time, not decorations
            public int constructionTimeMinutes = 60; // Default construction time in minutes
        }

        [Header("Building Configuration")]
        [SerializeField] private GameObject buildingPrefab;
        [SerializeField] private Transform buildingContainer;
        [SerializeField] private Grid grid;
        [SerializeField] private Vector2Int gridSize = new Vector2Int(20, 20);
        [SerializeField] private Vector3 gridOrigin = Vector3.zero;

        [Header("Building Types")]
        [SerializeField] private List<BuildingTypeData> availableBuildingTypes = new List<BuildingTypeData>();

        [Header("UI")]
        [SerializeField] private UIManager uiManager;

        [Header("Systems")]
        [SerializeField] private ResourceManager resourceManager;

        // Events (replacing Godot signals)
        [System.Serializable]
        public class BuildingPlacedEvent : UnityEvent<string, Vector3Int> { }
        public BuildingPlacedEvent OnBuildingPlaced = new BuildingPlacedEvent();

        // Track placed buildings
        private Dictionary<Vector3Int, GameObject> _placedBuildings = new Dictionary<Vector3Int, GameObject>();
        private Dictionary<string, BuildingTypeData> _buildingTypeLookup = new Dictionary<string, BuildingTypeData>();

        // --- BEGIN: UI-based Placement Support ---
        // This dictionary tracks placed items by world position for UI-based placement (not tilemap-based)
        private Dictionary<Vector3, string> _uiPlacedItems = new Dictionary<Vector3, string>();

        /// <summary>
        /// Debug method to track UI placed items dictionary changes
        /// </summary>
        private void LogUIPlacedItemsState(string operation)
        {
            Debug.Log($"[UI PLACED ITEMS] {operation} - Current count: {_uiPlacedItems.Count}");
            foreach (var kvp in _uiPlacedItems)
            {
                Debug.Log($"  - {kvp.Value} at {kvp.Key}");
            }
        }

        /// <summary>
        /// Record a placed item from the UI drag-and-drop system.
        /// </summary>
        public void RecordPlacedItem(DecorationItem item, Vector3 worldPosition)
        {
            if (item == null) return;
            _uiPlacedItems[worldPosition] = item.displayName; // Use displayName as type identifier
            Debug.Log($"Recorded UI-placed item '{item.displayName}' at {worldPosition}");
            LogUIPlacedItemsState("After recording item");
            
            // Update region unlock system using the actual region from the DecorationItem
            var regionType = ConvertDecorationRegionToAssessmentRegion(item.region);
            if (GameManager.Instance?.RegionUnlockSystem != null)
            {
                GameManager.Instance.RegionUnlockSystem.AddBuildingToRegion(regionType);
                Debug.Log($"Added building '{item.displayName}' to region {regionType} (from DecorationItem.region: {item.region})");
            }
            
            // Save the game immediately after recording a UI-placed item
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SaveGame();
                Debug.Log($"Saved game after recording UI-placed item '{item.displayName}' at {worldPosition}");
            }
        }

        /// <summary>
        /// Remove a placed item from the UI drag-and-drop system.
        /// </summary>
        public void RemovePlacedItem(Vector3 worldPosition)
        {
            LogUIPlacedItemsState("Before removing item");
            if (_uiPlacedItems.ContainsKey(worldPosition))
            {
                string itemType = _uiPlacedItems[worldPosition];
                Debug.Log($"Removed UI-placed item '{itemType}' at {worldPosition}");
                _uiPlacedItems.Remove(worldPosition);
                LogUIPlacedItemsState("After removing item");
                
                // Update region unlock system - we need to find the DecorationItem to get its region
                // For now, we'll use the name pattern method as fallback
                var regionType = GetRegionTypeForBuilding(itemType);
                if (GameManager.Instance?.RegionUnlockSystem != null)
                {
                    GameManager.Instance.RegionUnlockSystem.RemoveBuildingFromRegion(regionType);
                    Debug.Log($"Removed building '{itemType}' from region {regionType}");
                }
                
                // Save the game immediately after removing a UI-placed item
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SaveGame();
                    Debug.Log($"Saved game after removing UI-placed item '{itemType}' at {worldPosition}");
                }
            }
            else
            {
                Debug.LogWarning($"Attempted to remove UI-placed item at {worldPosition} but none was found");
            }
        }

        /// <summary>
        /// Get save data for all placed buildings and UI-placed items
        /// </summary>
        public List<BuildingSaveData> GetSaveData()
        {
            var buildingsToSave = new List<BuildingSaveData>();
            Debug.Log($"GetSaveData called - Saving {_placedBuildings.Count} tilemap buildings and {_uiPlacedItems.Count} UI-placed items.");
            
            // Save tilemap-based buildings
            foreach (var kvp in _placedBuildings)
            {
                var building = kvp.Value.GetComponent<Building>();
                if (building != null)
                {
                    buildingsToSave.Add(new BuildingSaveData
                    {
                        buildingType = building.BuildingName,
                        positionX = kvp.Key.x,
                        positionY = kvp.Key.y,
                        positionZ = kvp.Key.z,
                        worldX = null,
                        worldY = null,
                        worldZ = null
                    });
                    Debug.Log($"Saving tilemap building: {building.BuildingName} at {kvp.Key}");
                }
            }
            
            // Save UI-placed items
            foreach (var kvp in _uiPlacedItems)
            {
                buildingsToSave.Add(new BuildingSaveData
                {
                    buildingType = kvp.Value,
                    positionX = null,
                    positionY = null,
                    positionZ = null,
                    worldX = kvp.Key.x,
                    worldY = kvp.Key.y,
                    worldZ = kvp.Key.z
                });
                Debug.Log($"Saving UI-placed item: {kvp.Value} at {kvp.Key}");
            }
            
            Debug.Log($"GetSaveData returning {buildingsToSave.Count} total buildings to save");
            return buildingsToSave;
        }

        /// <summary>
        /// Load city from save data (supports both tilemap and UI-placed items)
        /// </summary>
        public void LoadCity(List<BuildingSaveData> savedBuildings)
        {
            // Clear existing buildings
            foreach (var building in _placedBuildings.Values)
            {
                if (building != null)
                    Destroy(building);
            }
            _placedBuildings.Clear();
            LogUIPlacedItemsState("Before clearing UI items");
            _uiPlacedItems.Clear();
            LogUIPlacedItemsState("After clearing UI items");
            Debug.Log($"Loading {savedBuildings.Count} buildings from save data.");
            
            // Ensure building container exists
            if (buildingContainer == null)
            {
                Debug.LogError("Building container is null! Cannot restore buildings.");
                return;
            }
            
            // Place saved buildings
            foreach (var buildingData in savedBuildings)
            {
                if (buildingData.positionX.HasValue && buildingData.positionY.HasValue && buildingData.positionZ.HasValue)
                {
                    // Tilemap-based
                    var mapPos = new Vector3Int(buildingData.positionX.Value, buildingData.positionY.Value, buildingData.positionZ.Value);
                    PlaceBuildingInWorld(buildingData.buildingType, mapPos);
                }
                else if (buildingData.worldX.HasValue && buildingData.worldY.HasValue && buildingData.worldZ.HasValue)
                {
                    // UI-placed
                    Vector3 worldPos = new Vector3(buildingData.worldX.Value, buildingData.worldY.Value, buildingData.worldZ.Value);
                    _uiPlacedItems[worldPos] = buildingData.buildingType;
                    LogUIPlacedItemsState($"After adding {buildingData.buildingType}");

                    Debug.Log($"Processing UI-placed building: {buildingData.buildingType} at {worldPos}");

                    // --- Instantiate the UI-placed prefab here ---
                    if (!_buildingTypeLookup.ContainsKey(buildingData.buildingType))
                    {
                        Debug.LogError($"Building type '{buildingData.buildingType}' not found in lookup dictionary during load!");
                        Debug.Log($"Available building types: {string.Join(", ", _buildingTypeLookup.Keys)}");
                        continue; // Skip this building and continue with others
                    }
                    
                    var buildingTypeData = _buildingTypeLookup[buildingData.buildingType];
                    var prefab = buildingTypeData.buildingPrefab;
                    
                    Debug.Log($"Building type data found: prefab={prefab != null}, sprite={buildingTypeData.buildingSprite != null}");
                    
                    if (prefab != null && buildingContainer != null)
                    {
                        GameObject placedItem = GameObject.Instantiate(prefab, buildingContainer);
                        placedItem.transform.position = worldPos;
                        placedItem.transform.localScale = Vector3.one;

                        Debug.Log($"Instantiated placed item: {placedItem.name} at position {placedItem.transform.position}");

                        // Check if the item is active and visible
                        Debug.Log($"Item active: {placedItem.activeInHierarchy}, visible: {placedItem.activeSelf}");

                        // Set the sprite from BuildingTypeData
                        var placedItemUI = placedItem.GetComponent<PlacedItemUI>();
                        if (placedItemUI != null && buildingTypeData.buildingSprite != null)
                        {
                            placedItemUI.SetSprite(buildingTypeData.buildingSprite);
                            Debug.Log($"Set sprite for {buildingData.buildingType}");
                            
                            // Check if the sprite was actually set
                            var sr = placedItem.GetComponent<SpriteRenderer>();
                            if (sr != null)
                            {
                                Debug.Log($"SpriteRenderer sprite: {sr.sprite != null}");
                            }
                            
                            var img = placedItem.GetComponent<UnityEngine.UI.Image>();
                            if (img != null)
                            {
                                Debug.Log($"UI Image sprite: {img.sprite != null}");
                            }
                        }
                        else if (placedItemUI != null)
                        {
                            Debug.LogWarning($"No sprite found for building type '{buildingData.buildingType}'");
                        }
                        else
                        {
                            Debug.LogError($"PlacedItemUI component not found on instantiated prefab for '{buildingData.buildingType}'");
                        }
                        
                        Debug.Log($"Successfully restored UI-placed item '{buildingData.buildingType}' at {worldPos}");
                    }
                    else
                    {
                        Debug.LogError($"Failed to instantiate building '{buildingData.buildingType}': prefab={prefab != null}, container={buildingContainer != null}");
                    }
                }
            }
        }
        // --- END: UI-based Placement Support ---

        public static CityBuilder Instance { get; private set; } // This is a singleton instance that can be accessed globally; it allows other scripts to easily access the city builder functionality. 
        private void Awake()
        {
            if (Instance != null && Instance != this) // If an instance already exists, destroy this one to enforce singleton pattern (only one instance of CityBuilder should exist at a time). 
            {
                Destroy(this.gameObject); // Destroy the duplicate instance to maintain a single instance of CityBuilder in the scene. 
                return;
            }

            Instance = this; // Set the static instance to this instance, allowing other scripts to access it globally. 
            
            // Build lookup dictionary
            foreach (var buildingType in availableBuildingTypes)
            {
                _buildingTypeLookup[buildingType.buildingName] = buildingType;
            }

            // Validate references
            if (grid == null)
                grid = FindFirstObjectByType<Grid>();

            // Ensure building container exists
            if (buildingContainer == null)
            {
                // Try to find a container in the scene
                buildingContainer = GameObject.Find("PlacedCityItemsContainer")?.transform;
                if (buildingContainer == null)
                {
                    // Create a container if none exists
                    GameObject containerGO = new GameObject("PlacedCityItemsContainer");
                    buildingContainer = containerGO.transform;
                    Debug.Log("Created PlacedCityItemsContainer automatically");
                }
            }
        }

        /// <summary>
        /// Convert world position to tilemap coordinates
        /// </summary>
        public Vector3Int WorldToMap(Vector3 worldPos)
        {
            if (grid == null)
            {
                Debug.LogError("Grid is null in WorldToMap! Returning Vector3Int.zero");
                return Vector3Int.zero;
            }
            return grid.WorldToCell(worldPos);
        }

        /// <summary>
        /// Convert tilemap coordinates to world position
        /// </summary>
        public Vector3 MapToWorld(Vector3Int mapPos)
        {
            return grid.GetCellCenterWorld(mapPos);
        }

        /// <summary>
        /// Check if a position is valid for building placement
        /// </summary>
        public bool IsPlacementValid(Vector3Int mapPos)
        {
            // Check if tile is already occupied
            if (_placedBuildings.ContainsKey(mapPos))
                return false;

            // Check if position is within grid bounds
            if (mapPos.x < 0 || mapPos.x >= gridSize.x || mapPos.y < 0 || mapPos.y >= gridSize.y)
                return false;

            // TODO: Add additional validation rules (e.g., district restrictions, adjacency rules)
            return true;
        }

        /// <summary>
        /// Attempt to place a building at the specified world position
        /// </summary>
        public bool AttemptToPlaceBuilding(string buildingType, Vector3 worldPos)
        {
            Debug.Log($"=== AttemptToPlaceBuilding called for {buildingType} at {worldPos} ===");
            
            // Convert world position to grid position
            Vector3Int mapPos = WorldToMap(worldPos);
            
            Debug.Log($"Converted to map position: {mapPos}");
            
            // Check if building type exists
            if (!_buildingTypeLookup.ContainsKey(buildingType))
            {
                Debug.LogWarning($"Building type '{buildingType}' does not exist.");
                return false;
            }

            var buildingData = _buildingTypeLookup[buildingType];
            Debug.Log($"Building data found: hasConstructionTime={buildingData.hasConstructionTime}, cost={buildingData.costAmount}");

            // Check if player can afford the building
            if (ResourceManager.Instance.SpendResources(buildingData.costResource, buildingData.costAmount))
            {
                PlaceBuildingInWorld(buildingType, mapPos);
                
                // Save the game immediately after placing a building
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SaveGame();
                    Debug.Log($"Saved game after placing building '{buildingType}' at {mapPos}");
                }
                
                Debug.Log($"Successfully placed {buildingType} at {mapPos}");
                return true;
            }
            else
            {
                Debug.LogWarning($"Could not afford to place '{buildingType}'.");
                return false;
            }
        }

        /// <summary>
        /// Physically place the building in the world
        /// </summary>
        private void PlaceBuildingInWorld(string buildingType, Vector3Int mapPos)
        {
            var buildingData = _buildingTypeLookup[buildingType];
            
            // Instantiate building prefab
            GameObject buildingInstance = Instantiate(buildingData.buildingPrefab, buildingContainer);
            buildingInstance.transform.position = MapToWorld(mapPos);

            // Set building properties
            var buildingComponent = buildingInstance.GetComponent<Building>();
            if (buildingComponent != null)
            {
                buildingComponent.Initialize(buildingType, buildingData.buildingSprite);
            }

            // Record building placement
            _placedBuildings[mapPos] = buildingInstance;

            // Update region unlock system
            UpdateRegionUnlockSystem(buildingType, true);

            // Start construction if this is a building (not a decoration)
            if (buildingData.hasConstructionTime)
            {
                StartBuildingConstruction(buildingType, mapPos);
            }

            // Add HoldDownInteraction component for all placed items
            var holdDownInteraction = buildingInstance.GetComponent<HoldDownInteraction>();
            if (holdDownInteraction == null)
            {
                holdDownInteraction = buildingInstance.AddComponent<HoldDownInteraction>();
            }
            
            // Initialize the HoldDownInteraction with building data
            if (buildingData.hasConstructionTime)
            {
                // This is a building, so set it up as a building
                holdDownInteraction.InitializeItemData(true, buildingType, buildingData.costAmount, buildingData.costResource);
                Debug.Log($"Initialized HoldDownInteraction for building: {buildingType}");
            }
            else
            {
                // This is a decoration
                holdDownInteraction.InitializeItemData(false, buildingType, 0, ResourceManager.ResourceType.EnergyCrystals);
                Debug.Log($"Initialized HoldDownInteraction for decoration: {buildingType}");
            }

            // Trigger event
            OnBuildingPlaced?.Invoke(buildingType, mapPos);
            
            Debug.Log($"Placed '{buildingType}' at map position {mapPos}");
        }

        /// <summary>
        /// Remove a building from the world
        /// </summary>
        public void RemoveBuilding(Vector3Int mapPos)
        {
            if (_placedBuildings.TryGetValue(mapPos, out GameObject building))
            {
                // Get building type before destroying
                string buildingType = "";
                var buildingComponent = building.GetComponent<Building>();
                if (buildingComponent != null)
                {
                    buildingType = buildingComponent.BuildingName;
                }

                Destroy(building);
                _placedBuildings.Remove(mapPos);
                
                // Update region unlock system
                if (!string.IsNullOrEmpty(buildingType))
                {
                    UpdateRegionUnlockSystem(buildingType, false);
                }
                
                Debug.Log($"Removed building at {mapPos}");
                
                // Save the game immediately after removing a building
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SaveGame();
                    Debug.Log($"Saved game after removing building at {mapPos}");
                }
            }
        }

        /// <summary>
        /// Get building at specific position
        /// </summary>
        public GameObject GetBuildingAt(Vector3Int mapPos)
        {
            return _placedBuildings.TryGetValue(mapPos, out GameObject building) ? building : null;
        }

        /// <summary>
        /// Get all placed buildings
        /// </summary>
        public Dictionary<Vector3Int, GameObject> GetAllBuildings()
        {
            return new Dictionary<Vector3Int, GameObject>(_placedBuildings);
        }

        /// <summary>
        /// Clear all placed buildings from the city and save the empty state (for developer testing)
        /// </summary>
        public void ClearCity()
        {
            foreach (var building in _placedBuildings.Values)
            {
                if (building != null)
                    Destroy(building);
            }
            _placedBuildings.Clear();
            Debug.Log("City cleared. Saving empty city layout.");
            if (LifeCraft.Core.GameManager.Instance != null)
                LifeCraft.Core.GameManager.Instance.SaveGame();
        }

        /// <summary>
        /// Get building type data by name
        /// </summary>
        public BuildingTypeData GetBuildingTypeData(string buildingType)
        {
            Debug.Log($"GetBuildingTypeData called for: {buildingType}");
            Debug.Log($"Available building types: {string.Join(", ", _buildingTypeLookup.Keys)}");
            
            if (_buildingTypeLookup.TryGetValue(buildingType, out BuildingTypeData data))
            {
                Debug.Log($"Found building data for {buildingType}: hasConstructionTime={data.hasConstructionTime}");
                return data;
            }
            else
            {
                Debug.LogError($"Building type '{buildingType}' not found in lookup!");
                return null;
            }
        }

        /// <summary>
        /// Get all available building types
        /// </summary>
        public List<string> GetAvailableBuildingTypes()
        {
            return new List<string>(_buildingTypeLookup.Keys);
        }

        /// <summary>
        /// Get list of premium decor items from available building types
        /// </summary>
        public List<string> GetPremiumDecorItems()
        {
            var premiumItems = new List<string>();
            
            // Check if we have access to DecorationDatabase
            var decorDatabase = Resources.Load<LifeCraft.Shop.DecorationDatabase>("DecorationDatabase");
            if (decorDatabase != null)
            {
                premiumItems.AddRange(decorDatabase.GetPremiumDecorItems());
                return premiumItems;
            }
            
            // Fallback: look for premium items in available building types
            foreach (var buildingType in availableBuildingTypes)
            {
                // Check if the building name contains premium indicators
                if (buildingType.buildingName.Contains("Premium") || 
                    buildingType.buildingName.Contains("Golden") ||
                    buildingType.buildingName.Contains("Crystal") ||
                    buildingType.buildingName.Contains("Diamond") ||
                    buildingType.buildingName.Contains("Luxury") ||
                    buildingType.buildingName.Contains("Animated") ||
                    buildingType.buildingName.Contains("Enchanted"))
                {
                    premiumItems.Add(buildingType.buildingName);
                }
            }
            
            return premiumItems;
        }

        /// <summary>
        /// Find a placed item GameObject at a specific world position
        /// </summary>
        private GameObject FindPlacedItemAtPosition(Vector3 worldPosition)
        {
            if (buildingContainer == null) return null;
            
            // Search through all children of the building container
            foreach (Transform child in buildingContainer)
            {
                if (Vector3.Distance(child.position, worldPosition) < 0.1f)
                {
                    return child.gameObject;
                }
            }
            
            return null;
        }

        /// <summary>
        /// Update the region unlock system when buildings are placed or removed
        /// </summary>
        private void UpdateRegionUnlockSystem(string buildingType, bool isPlacing)
        {
            if (GameManager.Instance?.RegionUnlockSystem == null)
                return;

            // Determine which region this building belongs to
            var regionType = GetRegionTypeForBuilding(buildingType);
            
            if (isPlacing)
            {
                GameManager.Instance.RegionUnlockSystem.AddBuildingToRegion(regionType);
            }
            else
            {
                GameManager.Instance.RegionUnlockSystem.RemoveBuildingFromRegion(regionType);
            }
        }

        /// <summary>
        /// Convert DecorationItem.RegionType to AssessmentQuizManager.RegionType
        /// </summary>
        private AssessmentQuizManager.RegionType ConvertDecorationRegionToAssessmentRegion(RegionType decorationRegion)
        {
            switch (decorationRegion)
            {
                case RegionType.HealthHarbor: return AssessmentQuizManager.RegionType.HealthHarbor;
                case RegionType.MindPalace: return AssessmentQuizManager.RegionType.MindPalace;
                case RegionType.CreativeCommons: return AssessmentQuizManager.RegionType.CreativeCommons;
                case RegionType.SocialSquare: return AssessmentQuizManager.RegionType.SocialSquare;
                case RegionType.Decoration: 
                default: return AssessmentQuizManager.RegionType.HealthHarbor; // Default to Health Harbor for decorations
            }
        }

        /// <summary>
        /// Get the region type for a building based on its name
        /// </summary>
        private AssessmentQuizManager.RegionType GetRegionTypeForBuilding(string buildingType)
        {
            // Check building name patterns to determine region
            if (buildingType.Contains("Wellness") || buildingType.Contains("Yoga") || 
                buildingType.Contains("Juice") || buildingType.Contains("Sleep") || 
                buildingType.Contains("Nutrition") || buildingType.Contains("Spa") || 
                buildingType.Contains("Running") || buildingType.Contains("Therapy") || 
                buildingType.Contains("Biohacking") || buildingType.Contains("Aquatic") || 
                buildingType.Contains("Hydration") || buildingType.Contains("Fresh Air"))
            {
                return AssessmentQuizManager.RegionType.HealthHarbor;
            }
            else if (buildingType.Contains("Meditation") || buildingType.Contains("Therapy") || 
                     buildingType.Contains("Gratitude") || buildingType.Contains("Boundary") || 
                     buildingType.Contains("Calm") || buildingType.Contains("Reflection") || 
                     buildingType.Contains("Monument") || buildingType.Contains("Tower") || 
                     buildingType.Contains("Maze") || buildingType.Contains("Library") || 
                     buildingType.Contains("Dream") || buildingType.Contains("Focus") || 
                     buildingType.Contains("Resilience"))
            {
                return AssessmentQuizManager.RegionType.MindPalace;
            }
            else if (buildingType.Contains("Writer") || buildingType.Contains("Art") || 
                     buildingType.Contains("Expression") || buildingType.Contains("Amphitheater") || 
                     buildingType.Contains("Innovation") || buildingType.Contains("Style") || 
                     buildingType.Contains("Music") || buildingType.Contains("Maker") || 
                     buildingType.Contains("Inspiration") || buildingType.Contains("Animation") || 
                     buildingType.Contains("Design") || buildingType.Contains("Sculpture") || 
                     buildingType.Contains("Film"))
            {
                return AssessmentQuizManager.RegionType.CreativeCommons;
            }
            else if (buildingType.Contains("Friendship") || buildingType.Contains("Kindness") || 
                     buildingType.Contains("Community") || buildingType.Contains("Cultural") || 
                     buildingType.Contains("Game") || buildingType.Contains("Coffee") || 
                     buildingType.Contains("Family") || buildingType.Contains("Support") || 
                     buildingType.Contains("Stage") || buildingType.Contains("Volunteer") || 
                     buildingType.Contains("Celebration") || buildingType.Contains("Pet") || 
                     buildingType.Contains("Teamwork"))
            {
                return AssessmentQuizManager.RegionType.SocialSquare;
            }

            // Default to Health Harbor if no match found
            return AssessmentQuizManager.RegionType.HealthHarbor;
        }

        /// <summary>
        /// Calculate construction time for a building based on its unlock level
        /// </summary>
        public float CalculateConstructionTime(string buildingName)
        {
            Debug.Log($"=== CITYBUILDER CALCULATE CONSTRUCTION TIME ===");
            Debug.Log($"Building name: {buildingName}");
            
            if (PlayerLevelManager.Instance == null)
            {
                Debug.LogError("PlayerLevelManager.Instance is null!");
                return 60f; // Default 1 hour if PlayerLevelManager not available
            }
                
            // Use the new pre-calculated construction time system
            float constructionTimeMinutes = PlayerLevelManager.Instance.GetBuildingConstructionTime(buildingName);
            
            Debug.Log($"Final construction time for {buildingName}: {constructionTimeMinutes} minutes (from PlayerLevelManager)");
            Debug.Log($"=== END CITYBUILDER CALCULATE CONSTRUCTION TIME ===");
            return constructionTimeMinutes;
        }
        
        /// <summary>
        /// Get the region type for a building
        /// </summary>
        public string GetBuildingRegionType(string buildingName)
        {
            // This would need to be implemented based on your building data structure
            // For now, return a default region type
            return "HealthHarbor"; // Default region
        }
        
        /// <summary>
        /// Called when construction is complete
        /// </summary>
        public void OnConstructionComplete(string buildingName, Vector3Int gridPosition)
        {
            Debug.Log($"Construction completed for {buildingName} at {gridPosition}");
            
            // The building is now fully constructed and functional
            // You might want to trigger any completion effects here
            
            // Save the game
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SaveGame();
            }
        }
        
        /// <summary>
        /// Start construction for a building at the specified position
        /// </summary>
        public void StartBuildingConstruction(string buildingName, Vector3Int gridPosition)
        {
            Debug.Log($"=== StartBuildingConstruction called for {buildingName} at {gridPosition} ===");
            
            // Calculate construction time
            float constructionTimeMinutes = CalculateConstructionTime(buildingName);
            string regionType = GetBuildingRegionType(buildingName);
            
            Debug.Log($"Construction time: {constructionTimeMinutes} minutes, Region: {regionType}");
            
            // Get the building GameObject
            if (_placedBuildings.TryGetValue(gridPosition, out GameObject building))
            {
                Debug.Log($"Found building GameObject: {building.name}");
                
                // Use the BuildingConstructionTimer component on the building
                var constructionTimer = building.GetComponent<BuildingConstructionTimer>();
                if (constructionTimer != null)
                {
                    Debug.Log("Found BuildingConstructionTimer component, starting construction...");
                    // Start construction
                    constructionTimer.StartConstruction(buildingName, gridPosition, constructionTimeMinutes, regionType);
                    
                    Debug.Log($"Started construction for {buildingName} at {gridPosition}. Duration: {constructionTimeMinutes} minutes");
                }
                else
                {
                    Debug.LogError("BuildingConstructionTimer component not found on building GameObject.");
                }
            }
            else
            {
                Debug.LogError($"Building not found in _placedBuildings at position {gridPosition}");
            }
        }
    }

    /// <summary>
    /// Data structure for saving building information
    /// </summary>
    [System.Serializable]
    public class BuildingSaveData
    {
        public string buildingType;
        public int? positionX;
        public int? positionY;
        public int? positionZ;
        public float? worldX;
        public float? worldY;
        public float? worldZ;
    }
} 