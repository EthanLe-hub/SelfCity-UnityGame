using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using LifeCraft.Core; // To access RegionEditManager and DecorationItem classes. 
using LifeCraft.UI; // To access InventoryUI and PlacedItemUI

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
            // Set icon (for now, just use a colored square)
            if (iconImage != null)
            {
                iconImage.color = GetRarityColor(_decorationItem.rarity);
                iconImage.sprite = CreatePlaceholderSprite(); // Replace with real sprite later
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

                    // Instantiate the placed item prefab as a child of the grid cell:
                    if (placedItemPrefab != null)
                    {
                        GameObject placedItem = Instantiate(placedItemPrefab, result.gameObject.transform); // Create the placed item in the grid cell. 
                        placedItem.transform.localPosition = Vector3.zero; // Reset position to center of the cell. 
                        placedItem.transform.localScale = Vector3.one; // Reset scale to 1. 

                        // Set icon and name on the placed item:
                        var iconImage = placedItem.transform.Find("IconImage")?.GetComponent<Image>();
                        if (iconImage != null && iconImage.sprite != null)
                        {
                            iconImage.sprite = this.iconImage.sprite; // Set the icon sprite from the draggable item. 
                        }
                        else if (iconImage != null)
                        {
                            iconImage.color = this.iconImage.color; // Set the icon color if no sprite is assigned (fallback for placeholder). 
                        }

                        var nameText = placedItem.transform.Find("Name")?.GetComponent<TMPro.TextMeshProUGUI>(); // Find the name TextMeshPro component in the placed item prefab. 
                        if (nameText != null)
                        {
                            nameText.text = _decorationItem.displayName; // Set the name text from the item. 
                        }

                        // Pass the DecorationItem to the placed item for removal logic
                        var placedItemUI = placedItem.GetComponent<PlacedItemUI>();
                        if (placedItemUI != null)
                            placedItemUI.Initialize(_decorationItem);
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

            // Show the inventory panel again after drag ends
            if (inventoryPanel != null)
                inventoryPanel.SetActive(true);
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
    }
} 