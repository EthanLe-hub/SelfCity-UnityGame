using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using LifeCraft.Core; // To access RegionEditManager and DecorationItem classes. 
using LifeCraft.UI; // To access InventoryUI and PlacedItemUI
using LifeCraft.Systems; // To access PlayerLevelManager

namespace LifeCraft.UI
{
    /// <summary>
    /// Represents a draggable decoration item in the inventory UI.
    /// Attach this to a prefab with Image/Text components for icon, name, rarity, etc.
    /// Handles drag-and-drop and click events for inventory interactions.
    /// </summary>
    public class DraggableInventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage; // The main icon for the decoration
        [SerializeField] private TextMeshProUGUI nameText; // The name label
        [SerializeField] private TextMeshProUGUI rarityText; // The rarity label
        [SerializeField] private Image backgroundImage; // Background for color coding
        [SerializeField] private Image premiumBadge; // Badge for premium items

        public Transform placedCityItemsContainer; // Container for placed items in the city grid, assigned in the Inspector. 
        
        [Header("Drag Settings")]
        [SerializeField] private GameObject dragPreviewPrefab; // Optional: prefab for drag preview
        [SerializeField] private Canvas dragCanvas; // Canvas to drag on

        [SerializeField] private GameObject placedItemPrefab; // Prefab for placed item in the city grid. 
        
        [Header("Colors")]
        [SerializeField] private Color commonColor = Color.white;
        [SerializeField] private Color uncommonColor = Color.green;
        [SerializeField] private Color rareColor = Color.blue;
        [SerializeField] private Color epicColor = Color.magenta;
        [SerializeField] private Color legendaryColor = Color.yellow;
        
        // Data for this item
        private DecorationItem _decorationItem;
        private GameObject _dragPreview;
        private Vector3 _originalPosition;
        private Transform _originalParent;
        private CanvasGroup _canvasGroup;
        
        // Events for UI/logic to subscribe to
        public System.Action<DraggableInventoryItem> OnItemClicked;
        public System.Action<DraggableInventoryItem, Vector3> OnItemDropped;

        [SerializeField] private GameObject inventoryPanel; // Reference to the inventory panel GameObject

        // Called when the object is created
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // Automatically find the main Canvas if not assigned. 
            if (dragCanvas == null)
            {
                dragCanvas = FindFirstObjectByType<Canvas>(); // Use new Unity API
            }

            // Try to auto-find the inventory panel if not assigned
            if (inventoryPanel == null)
            {
                var invUI = FindFirstObjectByType<InventoryUI>();
                if (invUI != null)
                {
                    var panelField = invUI.GetType().GetField("inventoryPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (panelField != null)
                    {
                        inventoryPanel = panelField.GetValue(invUI) as GameObject;
                    }
                }
            }
        }
        
        /// <summary>
        /// Initialize the item with decoration data (called by InventoryUI).
        /// </summary>
        public void Initialize(DecorationItem decorationItem)
        {
            _decorationItem = decorationItem;
            UpdateVisuals();
        }
        
        /// <summary>
        /// Update the visual appearance based on the decoration data.
        /// </summary>
        private void UpdateVisuals()
        {
            if (_decorationItem == null) return;
            // Set name
            if (nameText != null)
                nameText.text = _decorationItem.displayName;
            // Set icon - try to get the real sprite from CityBuilder first
            if (iconImage != null)
            {
                iconImage.color = GetRarityColor(_decorationItem.rarity);
                
                // Try to get the real sprite from CityBuilder
                Sprite realSprite = null;
                if (LifeCraft.Core.CityBuilder.Instance != null)
                {
                    var buildingData = LifeCraft.Core.CityBuilder.Instance.GetBuildingTypeData(_decorationItem.displayName);
                    if (buildingData != null && buildingData.buildingSprite != null)
                    {
                        realSprite = buildingData.buildingSprite;
                        Debug.Log($"Found real sprite '{realSprite.name}' for {_decorationItem.displayName} in inventory");
                    }
                }
                
                // Use real sprite if found, otherwise fall back to placeholder
                if (realSprite != null)
                {
                    iconImage.sprite = realSprite;
                }
                else
                {
                    iconImage.sprite = CreatePlaceholderSprite();
                    Debug.LogWarning($"No real sprite found for {_decorationItem.displayName}, using placeholder");
                }
            }
            // Set rarity text
            if (rarityText != null)
                rarityText.text = _decorationItem.rarity.ToString();
            // Set background color based on rarity
            if (backgroundImage != null)
                backgroundImage.color = GetRarityColor(_decorationItem.rarity) * 0.3f;
            // Show premium badge if applicable
            if (premiumBadge != null)
                premiumBadge.gameObject.SetActive(_decorationItem.isPremium);
        }
        
        /// <summary>
        /// Create a simple placeholder sprite for the decoration (white square).
        /// Replace this with real art later.
        /// </summary>
        private Sprite CreatePlaceholderSprite()
        {
            Texture2D texture = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        }
        
        /// <summary>
        /// Get color based on rarity for UI coloring.
        /// </summary>
        private Color GetRarityColor(DecorationRarity rarity)
        {
            switch (rarity)
            {
                case DecorationRarity.Common: return commonColor;
                case DecorationRarity.Uncommon: return uncommonColor;
                case DecorationRarity.Rare: return rareColor;
                case DecorationRarity.Epic: return epicColor;
                case DecorationRarity.Legendary: return legendaryColor;
                default: return commonColor;
            }
        }
        
        /// <summary>
        /// Get the decoration item data for this UI element.
        /// </summary>
        public DecorationItem GetDecorationItem()
        {
            return _decorationItem;
        }
        
        #region Drag and Drop Implementation
        // Called when drag starts
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (RegionEditManager.Instance == null || !RegionEditManager.Instance.IsEditModeActive)
            {
                return; // Only allow dragging in edit mode. 
            }

            if (_decorationItem == null) return;
            _originalPosition = transform.position;
            _originalParent = transform.parent;
            // Create drag preview (optional)
            if (dragPreviewPrefab != null)
            {
                _dragPreview = Instantiate(dragPreviewPrefab, dragCanvas.transform);
                var previewImage = _dragPreview.GetComponent<Image>();
                if (previewImage != null)
                {
                    previewImage.sprite = iconImage.sprite;
                    previewImage.color = iconImage.color;
                }
            }
            // Make original semi-transparent
            if (_canvasGroup != null)
                _canvasGroup.alpha = 0.5f;
            // Move to drag canvas (so it appears above other UI)
            transform.SetParent(dragCanvas.transform);

            // Hide the inventory panel when dragging starts
            UpdateInventoryPanelReference();
            if (inventoryPanel != null)
                inventoryPanel.SetActive(false);
        }
        // Called while dragging
        public void OnDrag(PointerEventData eventData)
        {
            if (RegionEditManager.Instance == null || !RegionEditManager.Instance.IsEditModeActive)
            {
                return; // Only allow dragging in edit mode. 
            }

            if (_decorationItem == null) return;
            transform.position = eventData.position;
            if (_dragPreview != null)
                _dragPreview.transform.position = eventData.position;
        }
        // Called when drag ends
        public void OnEndDrag(PointerEventData eventData)
        {
            if (RegionEditManager.Instance == null || !RegionEditManager.Instance.IsEditModeActive)
            {
                return; // Only allow dragging in edit mode. 
            }

            if (_decorationItem == null) return;
            // Restore original appearance
            if (_canvasGroup != null)
                _canvasGroup.alpha = 1f;
            // Return to original parent and position
            transform.SetParent(_originalParent);
            transform.position = _originalPosition;
            // Destroy drag preview
            if (_dragPreview != null)
            {
                Destroy(_dragPreview);
                _dragPreview = null;
            }
            // Check if dropped on a valid target (e.g., city grid)
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            bool droppedOnValidTarget = false;
            foreach (var result in results)
            {
                // Check if dropped on city grid or other valid target
                if (result.gameObject.CompareTag("CityGrid") || result.gameObject.CompareTag("BuildableArea"))
                {
                    droppedOnValidTarget = true;

                    // Remove from inventory data
                    if (_decorationItem != null && InventoryManager.Instance != null)
                    {
                        InventoryManager.Instance.RemoveDecoration(_decorationItem);
                    }

                    // Use the INVENTORY ITEM PREFAB approach - instantiate a modified version for placement
                    if (placedItemPrefab != null)
                    {
                        GameObject placedItem = Instantiate(placedItemPrefab, placedCityItemsContainer); // Create a new placed item using the working inventory prefab
                        placedItem.transform.position = result.gameObject.transform.position; // Set position to the grid cell where it was dropped
                        placedItem.transform.localScale = Vector3.one; // Reset scale to 1
                        
                        // Get the grid cell size from the GridOverlay's Grid Layout Group and adjust the placed item size
                        var gridOverlay = FindFirstObjectByType<GridPopulator>();
                        if (gridOverlay != null)
                        {
                            var gridLayoutGroup = gridOverlay.GetComponent<GridLayoutGroup>();
                            if (gridLayoutGroup != null)
                            {
                                // Set the placed item's RectTransform to match the grid cell size
                                var rectTransform = placedItem.GetComponent<RectTransform>();
                                if (rectTransform != null)
                                {
                                    rectTransform.sizeDelta = gridLayoutGroup.cellSize;
                                    Debug.Log($"Set placed item size to match grid cell: {gridLayoutGroup.cellSize}");
                                }
                            }
                        }

                        // Remove the DraggableInventoryItem script from the placed item (we don't want it to be draggable again)
                        var draggableScript = placedItem.GetComponent<DraggableInventoryItem>();
                        if (draggableScript != null)
                        {
                            DestroyImmediate(draggableScript);
                        }

                        // Hide the name text since we don't want it on placed items
                        var nameText = placedItem.transform.Find("Name");
                        if (nameText != null)
                        {
                            nameText.gameObject.SetActive(false);
                        }

                        // Hide the premium badge since we don't want it on placed items
                        var premiumBadge = placedItem.transform.Find("PremiumBadge");
                        if (premiumBadge != null)
                        {
                            premiumBadge.gameObject.SetActive(false);
                        }

                        // Add the PlacedItemUI script for removal functionality
                        var placedItemUI = placedItem.GetComponent<PlacedItemUI>();
                        if (placedItemUI == null)
                        {
                            placedItemUI = placedItem.AddComponent<PlacedItemUI>();
                        }
                        placedItemUI.Initialize(_decorationItem);

                        // Add HoldDownInteraction component for action menu
                        var holdDownInteraction = placedItem.GetComponent<HoldDownInteraction>();
                        if (holdDownInteraction == null)
                        {
                            holdDownInteraction = placedItem.AddComponent<HoldDownInteraction>();
                        }
                        
                        // Initialize the HoldDownInteraction with proper data
                        // Since this is a UI-based grid system, we'll handle building data directly
                        Debug.Log($"Initializing item: {_decorationItem.displayName}");
                        
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
                        
                        holdDownInteraction.InitializeItemDataFromShop(_decorationItem.displayName, _decorationItem.region);
                        Debug.Log($"Initialized HoldDownInteraction for {_decorationItem.displayName}: isBuilding={isBuilding}, region={_decorationItem.region}");
                        
                        // If this is a building, start construction directly (only on first placement)
                        if (isBuilding)
                        {
                            // Check if this building has saved construction progress
                            if (_decorationItem.constructionProgress > 0)
                            {
                                // Restore construction progress
                                RestoreConstructionProgress(placedItem, _decorationItem);
                            }
                            else
                            {
                                // Start new construction
                                Debug.Log($"Starting new construction for {_decorationItem.displayName} with {constructionTimeMinutes} minutes");
                                
                                // Use the BuildingConstructionTimer component on the placed item
                                var constructionTimer = placedItem.GetComponent<BuildingConstructionTimer>();
                                if (constructionTimer != null)
                                {
                                    // Use the placed item's position as a unique identifier
                                    Vector3Int gridPosition = new Vector3Int(
                                        Mathf.RoundToInt(placedItem.transform.position.x),
                                        Mathf.RoundToInt(placedItem.transform.position.y),
                                        0
                                    );
                                    
                                    constructionTimer.StartConstruction(_decorationItem.displayName, gridPosition, constructionTimeMinutes, _decorationItem.region.ToString());
                                    Debug.Log($"Started new construction for {_decorationItem.displayName} at {gridPosition}");
                                }
                                else
                                {
                                    Debug.LogError("BuildingConstructionTimer component not found on placed item!");
                                }
                            }
                        }

                        // Use the PROVEN inventory approach - get the IconImage and set the sprite directly
                        if (LifeCraft.Core.CityBuilder.Instance != null)
                        {
                            Debug.LogError($"Looking up building data for: {_decorationItem.displayName}");
                            var buildingData = LifeCraft.Core.CityBuilder.Instance.GetBuildingTypeData(_decorationItem.displayName);
                            
                            if (buildingData != null && buildingData.buildingSprite != null)
                            {
                                // Use the EXACT same approach as UpdateVisuals() in DraggableInventoryItem
                                var iconImage = placedItem.transform.Find("IconImage")?.GetComponent<Image>();
                                if (iconImage != null)
                                {
                                    iconImage.sprite = buildingData.buildingSprite;
                                    iconImage.enabled = true;
                                    iconImage.color = Color.white;
                                    Debug.LogError($"INVENTORY APPROACH: Set sprite '{buildingData.buildingSprite.name}' on IconImage for {_decorationItem.displayName}");
                                }
                                else
                                {
                                    Debug.LogError("IconImage child not found on placed item!");
                                }
                            }
                            else
                            {
                                Debug.LogError($"No building data or sprite found for {_decorationItem.displayName}");
                            }
                        }
                        else
                        {
                            Debug.LogError("CityBuilder.Instance is NULL!");
                        }

                        // --- NEW: Record the placed item in CityBuilder for persistent saving ---
                        if (LifeCraft.Core.CityBuilder.Instance != null)
                        {
                            LifeCraft.Core.CityBuilder.Instance.RecordPlacedItem(_decorationItem, placedItem.transform.position);
                        }
                    }

                    // Remove from inventory UI:
                    Destroy(gameObject); // Destroy the draggable item UI element. 

                    OnItemDropped?.Invoke(this, result.worldPosition); // Notify listeners that the item was dropped successfully. 
                    break;
                }
            }
            if (!droppedOnValidTarget)
            {
                Debug.Log($"Dropped {_decorationItem.displayName} but no valid target found");
            }
            // After successfully placing the item in the city grid:
            if (LifeCraft.Core.GameManager.Instance != null)
            {
                // Save the city layout immediately after placement to ensure persistence even if the game is closed or crashes.
                LifeCraft.Core.GameManager.Instance.SaveGame();
            }
        }
        // Called on click (not drag)
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.dragging) return;
            OnItemClicked?.Invoke(this);
        }
        #endregion
        
        /// <summary>
        /// Show item details in the console (for debugging or tooltips).
        /// </summary>
        public void ShowDetails()
        {
            if (_decorationItem == null) return;
            Debug.Log($"Decoration Details:\n" +
                     $"Name: {_decorationItem.displayName}\n" +
                     $"Description: {_decorationItem.description}\n" +
                     $"Rarity: {_decorationItem.rarity}\n" +
                     $"Size: {_decorationItem.size.x}x{_decorationItem.size.y}\n" +
                     $"Premium: {_decorationItem.isPremium}\n" +
                     $"Source: {_decorationItem.source}\n" +
                     $"Acquired: {_decorationItem.dateAcquired}");
        }

        // In OnBeginDrag and OnEndDrag, always get the inventoryPanel from InventoryUI to ensure correct reference after filtering:
        private void UpdateInventoryPanelReference()
        {
            if (inventoryPanel == null)
            {
                var invUI = FindFirstObjectByType<InventoryUI>();
                if (invUI != null)
                {
                    var panelField = invUI.GetType().GetField("inventoryPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (panelField != null)
                    {
                        inventoryPanel = panelField.GetValue(invUI) as GameObject;
                    }
                }
            }
        }
        
        /// <summary>
        /// Restore construction progress when placing a building back
        /// </summary>
        private void RestoreConstructionProgress(GameObject placedItem, DecorationItem decorationItem)
        {
            Debug.Log($"Restoring construction progress for {decorationItem.displayName}: {decorationItem.constructionProgress:F1}s remaining");
            
            // Use the BuildingConstructionTimer component on the placed item
            var constructionTimer = placedItem.GetComponent<BuildingConstructionTimer>();
            if (constructionTimer != null)
            {
                // Use the placed item's position as a unique identifier
                Vector3Int gridPosition = new Vector3Int(
                    Mathf.RoundToInt(placedItem.transform.position.x),
                    Mathf.RoundToInt(placedItem.transform.position.y),
                    0
                );
                
                // Calculate the adjusted start time to account for elapsed time
                float elapsedTime = decorationItem.constructionDuration - decorationItem.constructionProgress;
                float adjustedStartTime = Time.time - elapsedTime;
                
                // Register the construction project with saved progress
                if (ConstructionManager.Instance != null)
                {
                    // Create a custom construction project with saved progress
                    ConstructionManager.Instance.RegisterConstructionWithProgress(
                        decorationItem.displayName,
                        gridPosition,
                        decorationItem.constructionDuration / 60f, // Convert back to minutes
                        decorationItem.region.ToString(),
                        adjustedStartTime,
                        decorationItem.originalQuestTexts,
                        decorationItem.activeQuestTexts,
                        decorationItem.completedSkipQuests,
                        decorationItem.totalSkipQuests
                    );
                    
                    // Start the construction timer with the saved progress
                    constructionTimer.ResumeConstruction(decorationItem.displayName, gridPosition, decorationItem.constructionDuration / 60f, decorationItem.region.ToString(), decorationItem.skipButtonText);
                    
                    // IMMEDIATELY call ReSyncWithManager to update the Skip button text
                    // Start the coroutine directly on the construction timer component
                    constructionTimer.StartCoroutine(constructionTimer.ReSyncWithManager());
                    
                    Debug.Log($"Restored construction for {decorationItem.displayName} at {gridPosition} with {decorationItem.constructionProgress:F1}s remaining");
                    
                    // Clear the saved construction progress to prevent it from being restored again
                    decorationItem.constructionProgress = -1f;
                    decorationItem.constructionStartTime = -1f;
                    decorationItem.constructionDuration = -1f;
                    decorationItem.originalQuestTexts.Clear();
                    decorationItem.activeQuestTexts.Clear();
                    decorationItem.completedSkipQuests = 0;
                    decorationItem.totalSkipQuests = 0;
                    decorationItem.skipButtonText = "";
                }
                else
                {
                    Debug.LogError("ConstructionManager.Instance is null!");
                }
            }
            else
            {
                Debug.LogError("BuildingConstructionTimer component not found on placed item!");
            }
        }

    }
} 