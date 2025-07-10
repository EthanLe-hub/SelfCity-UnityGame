using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using LifeCraft.Core;

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
        
        // Called when the object is created
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
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
        }
        // Called while dragging
        public void OnDrag(PointerEventData eventData)
        {
            if (_decorationItem == null) return;
            transform.position = eventData.position;
            if (_dragPreview != null)
                _dragPreview.transform.position = eventData.position;
        }
        // Called when drag ends
        public void OnEndDrag(PointerEventData eventData)
        {
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
                    OnItemDropped?.Invoke(this, result.worldPosition);
                    break;
                }
            }
            if (!droppedOnValidTarget)
            {
                Debug.Log($"Dropped {_decorationItem.displayName} but no valid target found");
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
    }
} 