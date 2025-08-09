using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using LifeCraft.Systems;
using LifeCraft.Core;
using LifeCraft.UI;

namespace LifeCraft.UI
{
    /// <summary>
    /// Handles click/removal logic for placed items.
    /// </summary>
    public class PlacedItemUI : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        private DecorationItem _decorationItem;
        private bool isDragging = false;
        private Vector3 dragOffset;
        private Vector3 originalDragPosition;
        private bool isFirstPlacement = true; // Track if this is the first time being placed
        private float dragStartTime;

        [Header("EXP Reward Configuration")]
        [SerializeField] public int baseBuildingEXP = 10; // Base EXP for placing a building. 
        [SerializeField] public int decorationEXP = 5; // EXP for placing a decoration. 
        [SerializeField] public float expMultiplier = 1.2f; // Multiplier for EXP based on unlock level. 
        
        // Add debug component for troubleshooting
        private void Start()
        {
            if (GetComponent<PlacedItemDebugger>() == null)
            {
                gameObject.AddComponent<PlacedItemDebugger>();
            }
            
            // Manually initialize HoldDownInteraction if it wasn't done properly
            InitializeHoldDownInteraction();
            
            // Set up action menu for this placed item
            SetupActionMenu();
        }
        
        /// <summary>
        /// Manually initialize the HoldDownInteraction component for existing buildings
        /// </summary>
        private void InitializeHoldDownInteraction()
        {
            var holdDownInteraction = GetComponent<HoldDownInteraction>();
            if (holdDownInteraction != null && _decorationItem != null)
            {
                Debug.Log($"Manually initializing HoldDownInteraction for {_decorationItem.displayName}");
                
                // Use the new shop-based initialization method
                holdDownInteraction.InitializeItemDataFromShop(_decorationItem.displayName, _decorationItem.region);
                
                // Check if this is a building by looking at the region type
                bool isBuilding = (_decorationItem.region != RegionType.Decoration);
                
                // For buildings, get construction time from PlayerLevelManager
                float constructionTimeMinutes = 60f; // Default 1 hour
                if (isBuilding)
                {
                    // Use PlayerLevelManager to get the proper construction time based on unlock level
                    if (PlayerLevelManager.Instance != null)
                    {
                        constructionTimeMinutes = PlayerLevelManager.Instance.GetBuildingConstructionTime(_decorationItem.displayName);
                        Debug.Log($"Using PlayerLevelManager construction time for {_decorationItem.displayName}: {constructionTimeMinutes} minutes");
                    }
                    else
                    {
                        Debug.LogWarning("PlayerLevelManager.Instance is null - using default construction time");
                    }
                }
                
                Debug.Log($"Manually initialized HoldDownInteraction for {_decorationItem.displayName}: isBuilding={isBuilding}");
                
                // If this is a building, start construction directly
                if (isBuilding)
                {
                    Debug.Log($"Manually starting construction for {_decorationItem.displayName} with {constructionTimeMinutes} minutes");
                    
                    // Use the BuildingConstructionTimer component on this placed item
                    var constructionTimer = GetComponent<BuildingConstructionTimer>();
                    if (constructionTimer != null)
                    {
                        // Use the placed item's position as a unique identifier
                        Vector3Int gridPosition = new Vector3Int(
                            Mathf.RoundToInt(transform.position.x),
                            Mathf.RoundToInt(transform.position.y),
                            0
                        );
                        
                        constructionTimer.StartConstruction(_decorationItem.displayName, gridPosition, constructionTimeMinutes, _decorationItem.region.ToString());
                        Debug.Log($"Manually started construction for {_decorationItem.displayName} at {gridPosition}");
                    }
                    else
                    {
                        Debug.LogError("BuildingConstructionTimer component not found on placed item!");
                    }
                }
            }
        }

        /// <summary>
        /// Initialize the placed item with its DecorationItem data.
        /// Call this right after instantiating the placed item.
        /// </summary>
        public void Initialize(DecorationItem item)
        {
            _decorationItem = item;

            // Get the type for the item (decoration or region, and specific region):
            var itemType = _decorationItem.region;

            // Reward 5 EXP if placing a decoration:
            if (itemType == RegionType.Decoration && PlayerLevelManager.Instance != null)
            {
                PlayerLevelManager.Instance.AddEXP(decorationEXP); // Reward 5 EXP (decorationEXP) for placing a decoration. 
                
                // Show EXP popup animation
                if (EXPPopupManager.Instance != null)
                {
                    Vector3 buildingPosition = transform.position;
                    EXPPopupManager.Instance.ShowEXPPopup(decorationEXP, buildingPosition, QuestDifficulty.Easy);
                }
            }

            // Calculate and reward EXP based on unlock level if placing a building:
            if (itemType == RegionType.HealthHarbor || itemType == RegionType.MindPalace ||
                itemType == RegionType.CreativeCommons || itemType == RegionType.SocialSquare && 
                PlayerLevelManager.Instance != null)
            {
                // Calculate EXP based on the unlock level: EXP = baseBuildingEXP * (expMultiplier ^ (unlockLevel - 1))
                int expReward = baseBuildingEXP * Mathf.RoundToInt(Mathf.Pow(expMultiplier, PlayerLevelManager.Instance.GetBuildingUnlockLevel(_decorationItem.displayName) - 1)); // DecorationItem has displayName for the building name. 

                PlayerLevelManager.Instance.AddEXP(expReward); // Reward calculated EXP for placing a building. 
                
                // Show EXP popup animation
                if (EXPPopupManager.Instance != null)
                {
                    Vector3 buildingPosition = transform.position;
                    EXPPopupManager.Instance.ShowEXPPopup(expReward, buildingPosition, QuestDifficulty.Medium);
                }
            }
        }

        /// <summary>
        /// Sets the sprite for this item (legacy method - now handled by direct assignment).
        /// </summary>
        public void SetSprite(Sprite sprite)
        {
            Debug.LogError($"=== LEGACY SETSPRITE CALLED ON {gameObject.name} ===");
            Debug.LogError($"This method is deprecated - sprite assignment is now handled directly in DraggableInventoryItem");
            
            // This method is kept for compatibility but should not be used
            // The actual sprite assignment now happens directly in DraggableInventoryItem.OnEndDrag()
        }

        /// <summary>
        /// Show the action menu for this item
        /// </summary>
        private void ShowActionMenu()
        {
            var holdDownInteraction = GetComponent<HoldDownInteraction>();
            if (holdDownInteraction != null)
            {
                holdDownInteraction.ShowActionMenu();
            }
            else
            {
                Debug.LogError($"HoldDownInteraction component not found on {gameObject.name}");
            }
        }
        
        /// <summary>
        /// Check if this is a building (has construction time)
        /// </summary>
        private bool IsBuilding()
        {
            var decorationItem = GetDecorationItem();
            if (decorationItem != null)
            {
                // If region is not Decoration, it's a building
                return decorationItem.region != RegionType.Decoration;
            }
            return false;
        }
        
        /// <summary>
        /// Get the item name
        /// </summary>
        private string GetItemName()
        {
            var decorationItem = GetDecorationItem();
            if (decorationItem != null)
            {
                return decorationItem.displayName;
            }
            return gameObject.name;
        }
        
        /// <summary>
        /// Handle drag start
        /// </summary>
        public void OnBeginDrag(PointerEventData eventData)
        {
            // Only allow dragging in edit mode
            if (RegionEditManager.Instance != null && RegionEditManager.Instance.IsEditModeActive)
            {
                isDragging = true;
                dragStartTime = Time.time;
                originalDragPosition = transform.position;
                
                // Calculate drag offset in world space
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
                dragOffset = transform.position - worldPos;
                dragOffset.z = 0;
                
                Debug.Log($"Started dragging {gameObject.name} from {originalDragPosition}");
            }
        }
        
        /// <summary>
        /// Handle drag
        /// </summary>
        public void OnDrag(PointerEventData eventData)
        {
            if (isDragging)
            {
                // Update position based on mouse/touch position
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
                Vector3 newPos = worldPos + dragOffset;
                
                // Keep z position constant
                newPos.z = originalDragPosition.z;
                
                // Snap to grid during drag
                Vector3 snappedPos = new Vector3(
                    Mathf.Round(newPos.x),
                    Mathf.Round(newPos.y),
                    newPos.z
                );
                
                transform.position = snappedPos;
                
                Debug.Log($"Dragging {gameObject.name} to snapped position {snappedPos}");
            }
        }
        
        /// <summary>
        /// Handle drag end
        /// </summary>
        public void OnEndDrag(PointerEventData eventData)
        {
            if (isDragging)
            {
                isDragging = false;
                
                // Snap to grid
                Vector3 snappedPos = new Vector3(
                    Mathf.Round(transform.position.x),
                    Mathf.Round(transform.position.y),
                    transform.position.z
                );
                transform.position = snappedPos;
                
                // Mark that this is no longer the first placement
                isFirstPlacement = false;
                
                Debug.Log($"Finished dragging {gameObject.name} to {snappedPos}. First placement: {isFirstPlacement}");
            }
        }

        /// <summary>
        /// Called when the placed item is clicked.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"PlacedItemUI OnPointerClick() called on {gameObject.name}");
            
            // Simple check: if we were dragging, don't treat as a click
            if (isDragging)
            {
                Debug.Log("Ignoring click because we were dragging");
                return;
            }
            
            // Check if we're in edit mode
            if (RegionEditManager.Instance != null && RegionEditManager.Instance.IsEditModeActive)
            {
                // EDIT MODE: Show action menu immediately
                Debug.Log("Edit mode active - showing action menu immediately");
                ShowActionMenu();
                return;
            }
            
            // NORMAL MODE: Check for construction time (buildings only)
            if (IsBuilding())
            {
                // Check if this building has a construction timer
                var constructionTimer = GetComponent<BuildingConstructionTimer>();
                if (constructionTimer != null && constructionTimer.IsUnderConstruction())
                {
                    Debug.Log($"Building {GetItemName()} is under construction");
                    // The construction timer is already visible on the building
                    return;
                }
            }
            
            Debug.Log("No construction time to show and not in edit mode");
        }
        
        /// <summary>
        /// Get the decoration item data
        /// </summary>
        public DecorationItem GetDecorationItem()
        {
            return _decorationItem;
        }
        
        /// <summary>
        /// Set up the action menu for this placed item
        /// </summary>
        private void SetupActionMenu()
        {
            var holdDownInteraction = GetComponent<HoldDownInteraction>();
            if (holdDownInteraction != null && holdDownInteraction.actionMenuPrefab == null)
            {
                // Try to find the ActionMenuUI_Prefab
                GameObject actionMenuPrefab = FindActionMenuPrefab();
                if (actionMenuPrefab != null)
                {
                    holdDownInteraction.actionMenuPrefab = actionMenuPrefab;
                    Debug.Log($"Set up action menu for {gameObject.name}");
                }
                else
                {
                    Debug.LogWarning($"Could not find ActionMenuUI_Prefab for {gameObject.name}");
                }
            }
        }
        
        /// <summary>
        /// Find the ActionMenuUI_Prefab
        /// </summary>
        private GameObject FindActionMenuPrefab()
        {
            // Try to load from Resources
            GameObject prefab = Resources.Load<GameObject>("ActionMenuUI_Prefab");
            if (prefab != null)
            {
                return prefab;
            }
            
            // Try to load from the specific path
            prefab = Resources.Load<GameObject>("Prefabs/UI/ActionMenuUI_Prefab");
            if (prefab != null)
            {
                return prefab;
            }
            
            // Try to load from Assets path (for editor)
            #if UNITY_EDITOR
            prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/ActionMenuUI_Prefab.prefab");
            if (prefab != null)
            {
                return prefab;
            }
            #endif
            
            // Try to find it in the scene
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == "ActionMenuUI_Prefab" && obj.GetComponent<ActionMenuUI>() != null)
                {
                    return obj;
                }
            }
            
            Debug.LogError("ActionMenuUI_Prefab not found in Resources, Assets, or scene!");
            return null;
        }

        /// <summary>
        /// Returns the item to the inventory and removes it from the grid.
        /// </summary>
        private void ReturnToInventory()
        {
            if (_decorationItem != null && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddDecoration(_decorationItem);
            }
            // --- NEW: Remove the placed item from CityBuilder for persistent saving ---
            if (LifeCraft.Core.CityBuilder.Instance != null)
            {
                LifeCraft.Core.CityBuilder.Instance.RemovePlacedItem(transform.position);
            }
            Destroy(gameObject);

            // Save the city layout immediately after removal to ensure persistence even if the game is closed or crashes.
            if (LifeCraft.Core.GameManager.Instance != null)
            {
                LifeCraft.Core.GameManager.Instance.SaveGame();
            }
        }
    }
}