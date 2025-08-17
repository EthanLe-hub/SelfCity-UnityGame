using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LifeCraft.Core;

namespace LifeCraft.UI
{
    /// <summary>
    /// Main UI for displaying the player's decoration inventory.
    /// Attach this to a UI panel with a grid, filter controls, and item prefab.
    /// Handles filtering, sorting, and displaying inventory items.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject inventoryPanel; // The main inventory panel (show/hide)
        [SerializeField] private Transform inventoryGrid; // The grid container for items
        [SerializeField] private GameObject inventoryItemPrefab; // Prefab for each inventory item
        [SerializeField] private TextMeshProUGUI itemCountText; // Text for item count
        [SerializeField] private TextMeshProUGUI titleText; // Title label
        [SerializeField] private Transform placedCityItemsContainer; // Container for placed items in the city grid, assigned in the Inspector. 
        
        [Header("Filter Controls")]
        [SerializeField] private TMP_Dropdown rarityFilter; // Dropdown for rarity filter
        [SerializeField] private Toggle premiumOnlyToggle; // Toggle for premium filter
        [SerializeField] private TMP_Dropdown sortDropdown; // Dropdown for sorting
        [SerializeField] private Button clearFiltersButton; // Button to clear filters
        
        [Header("Settings")]
        [SerializeField] private int itemsPerRow = 4; // Number of items per row in grid
        [SerializeField] private float itemSpacing = 10f; // Spacing between items
        
        // Internal lists for filtering/sorting
        private List<DecorationItem> _allItems = new List<DecorationItem>();
        private List<DecorationItem> _filteredItems = new List<DecorationItem>();
        private List<GameObject> _spawnedItems = new List<GameObject>();
        
        // Events for other UI/logic to subscribe to
        public System.Action<DecorationItem> OnItemSelected;
        public System.Action<DecorationItem, Vector3> OnItemDropped;
        
        // Called when the script instance is being loaded
        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
            RefreshInventory();
        }
        
        // Called when the object is destroyed
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        /// <summary>
        /// Initialize the UI controls (dropdowns, buttons, etc.).
        /// </summary>
        private void InitializeUI()
        {
            // Setup rarity filter dropdown
            if (rarityFilter != null)
            {
                rarityFilter.ClearOptions();
                rarityFilter.options.Add(new TMP_Dropdown.OptionData("All Rarities"));
                rarityFilter.options.Add(new TMP_Dropdown.OptionData("Common"));
                rarityFilter.options.Add(new TMP_Dropdown.OptionData("Uncommon"));
                rarityFilter.options.Add(new TMP_Dropdown.OptionData("Rare"));
                rarityFilter.options.Add(new TMP_Dropdown.OptionData("Epic"));
                rarityFilter.options.Add(new TMP_Dropdown.OptionData("Legendary"));
                rarityFilter.onValueChanged.AddListener(OnRarityFilterChanged);
            }
            // Setup sort dropdown
            if (sortDropdown != null)
            {
                sortDropdown.ClearOptions();
                sortDropdown.options.Add(new TMP_Dropdown.OptionData("All Items")); // Default option to show all items. 
                sortDropdown.options.Add(new TMP_Dropdown.OptionData("Decorations")); // Show only Decoration items. 
                sortDropdown.options.Add(new TMP_Dropdown.OptionData("Health Harbor Buildings")); // Show only Health Harbor buildings. 
                sortDropdown.options.Add(new TMP_Dropdown.OptionData("Mind Palace Buildings")); // Show only Mind Palace buildings. 
                sortDropdown.options.Add(new TMP_Dropdown.OptionData("Creative Commons Buildings")); // Show only Creative Commons buildings. 
                sortDropdown.options.Add(new TMP_Dropdown.OptionData("Social Square Buildings")); // Show only Social Square buildings. 
                sortDropdown.onValueChanged.AddListener(OnSortChanged);
            }
            // Setup premium toggle
            if (premiumOnlyToggle != null)
                premiumOnlyToggle.onValueChanged.AddListener(OnPremiumFilterChanged);
            // Setup clear filters button
            if (clearFiltersButton != null)
                clearFiltersButton.onClick.AddListener(ClearAllFilters);
        }
        
        /// <summary>
        /// Subscribe to inventory events so UI updates when inventory changes.
        /// </summary>
        private void SubscribeToEvents()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryUpdated.AddListener(RefreshInventory);
                InventoryManager.Instance.OnItemAdded.AddListener(OnItemAdded);
                InventoryManager.Instance.OnItemRemoved.AddListener(OnItemRemoved);
            }
        }
        
        /// <summary>
        /// Unsubscribe from inventory events (cleanup).
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryUpdated.RemoveListener(RefreshInventory);
                InventoryManager.Instance.OnItemAdded.RemoveListener(OnItemAdded);
                InventoryManager.Instance.OnItemRemoved.RemoveListener(OnItemRemoved);
            }
        }
        
        /// <summary>
        /// Refresh the entire inventory display (called when inventory changes).
        /// </summary>
        public void RefreshInventory()
        {
            if (InventoryManager.Instance == null) return;
            _allItems = InventoryManager.Instance.Inventory;
            ApplyFiltersAndSort();
            PopulateInventoryGrid();
            UpdateItemCount();
        }
        
        /// <summary>
        /// Apply current filters and sorting to the inventory.
        /// </summary>
        private void ApplyFiltersAndSort()
        {
            _filteredItems = new List<DecorationItem>(_allItems);
            // Apply rarity filter
            if (rarityFilter != null && rarityFilter.value > 0)
            {
                var selectedRarity = (DecorationRarity)(rarityFilter.value - 1);
                _filteredItems = _filteredItems.Where(item => item.rarity == selectedRarity).ToList();
            }
            // Apply premium filter
            if (premiumOnlyToggle != null && premiumOnlyToggle.isOn)
            {
                _filteredItems = _filteredItems.Where(item => item.isPremium).ToList();
            }
            // Apply sorting
            ApplySorting();
        }

        /// <summary>
        /// Apply sorting to the filtered items.
        /// </summary>
        private void ApplySorting()
        {
            if (sortDropdown == null) return;
            switch (sortDropdown.value)
            {
                //  case 0: // All Items, no filter needed. 
                case 1: // Decorations
                    _filteredItems = _filteredItems.Where(item => item.region == RegionType.Decoration).ToList();
                    break;
                case 2: // Health Harbor Buildings
                    _filteredItems = _filteredItems.Where(item => item.region == RegionType.HealthHarbor).ToList();
                    break;
                case 3: // Mind Palace Buildings
                    _filteredItems = _filteredItems.Where(item => item.region == RegionType.MindPalace).ToList();
                    break;
                case 4: // Creative Commons Buildings
                    _filteredItems = _filteredItems.Where(item => item.region == RegionType.CreativeCommons).ToList();
                    break;
                case 5: // Social Square Buildings
                    _filteredItems = _filteredItems.Where(item => item.region == RegionType.SocialSquare).ToList();
                    break;
            }

            // Always sort alphabetically by displayName:
            _filteredItems = _filteredItems.OrderBy(item => item.displayName).ToList(); 
        }
        
        /// <summary>
        /// Populate the inventory grid with filtered/sorted items.
        /// </summary>
        private void PopulateInventoryGrid()
        {
            // Clear existing items
            ClearInventoryGrid();
            if (inventoryGrid == null || inventoryItemPrefab == null) return;
            // Setup grid layout
            var gridLayout = inventoryGrid.GetComponent<GridLayoutGroup>();
            if (gridLayout != null)
            {
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = itemsPerRow;
                gridLayout.spacing = new Vector2(itemSpacing, itemSpacing);
            }
            // Spawn items
            foreach (var item in _filteredItems)
            {
                var itemGO = Instantiate(inventoryItemPrefab, inventoryGrid);
                var draggableItem = itemGO.GetComponent<DraggableInventoryItem>();
                if (draggableItem != null)
                {
                    draggableItem.Initialize(item);
                    draggableItem.OnItemClicked += OnInventoryItemClicked;
                    draggableItem.OnItemDropped += OnInventoryItemDropped;
                    draggableItem.placedCityItemsContainer = placedCityItemsContainer; // Assign the container for placed items in the city grid. 
                }
                _spawnedItems.Add(itemGO);
            }
        }
        
        /// <summary>
        /// Clear all items from the inventory grid.
        /// </summary>
        private void ClearInventoryGrid()
        {
            foreach (var item in _spawnedItems)
            {
                if (item != null)
                    Destroy(item);
            }
            _spawnedItems.Clear();
        }
        
        /// <summary>
        /// Update the item count and title labels.
        /// </summary>
        private void UpdateItemCount()
        {
            if (itemCountText != null)
            {
                itemCountText.text = $"Items: {_filteredItems.Count} / {_allItems.Count}";
            }
            if (titleText != null)
            {
                titleText.text = $"Decoration Inventory ({_allItems.Count})";
            }
        }
        
        /// <summary>
        /// Show or hide the inventory panel.
        /// </summary>
        public void ToggleInventory()
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(!inventoryPanel.activeSelf);
                if (inventoryPanel.activeSelf)
                {
                    RefreshInventory();
                }
            }
        }
        /// <summary>
        /// Show the inventory panel.
        /// </summary>
        public void ShowInventory()
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(true);
                RefreshInventory();
            }
        }
        /// <summary>
        /// Hide the inventory panel.
        /// </summary>
        public void HideInventory()
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }
        }
        #region Event Handlers
        // Called when an item is added
        private void OnItemAdded(DecorationItem item)
        {
            RefreshInventory();
        }
        // Called when an item is removed
        private void OnItemRemoved(DecorationItem item)
        {
            RefreshInventory();
        }
        // Called when rarity filter changes
        private void OnRarityFilterChanged(int value)
        {
            ApplyFiltersAndSort();
            PopulateInventoryGrid();
            UpdateItemCount();
        }
        // Called when premium filter changes
        private void OnPremiumFilterChanged(bool value)
        {
            ApplyFiltersAndSort();
            PopulateInventoryGrid();
            UpdateItemCount();
        }
        // Called when sort dropdown changes
        private void OnSortChanged(int value)
        {
            ApplyFiltersAndSort();
            PopulateInventoryGrid();
            UpdateItemCount();
        }
        // Called when clear filters button is pressed
        private void ClearAllFilters()
        {
            if (rarityFilter != null)
                rarityFilter.value = 0;
            if (premiumOnlyToggle != null)
                premiumOnlyToggle.isOn = false;
            if (sortDropdown != null)
                sortDropdown.value = 0; // Reset to "All Items". 
            ApplyFiltersAndSort();
            PopulateInventoryGrid();
            UpdateItemCount();
        }
        // Called when an inventory item is clicked
        private void OnInventoryItemClicked(DraggableInventoryItem item)
        {
            var decorationItem = item.GetDecorationItem();
            if (decorationItem != null)
            {
                item.ShowDetails();
                OnItemSelected?.Invoke(decorationItem);
            }
        }
        // Called when an inventory item is dropped (dragged out)
        private void OnInventoryItemDropped(DraggableInventoryItem item, Vector3 position)
        {
            var decorationItem = item.GetDecorationItem();
            if (decorationItem != null)
            {
                OnItemDropped?.Invoke(decorationItem, position);
                Debug.Log($"Dropped {decorationItem.displayName} at position {position}");
                // TODO: Implement actual placement in city
                // For now, just remove from inventory when dropped
                // InventoryManager.Instance.RemoveDecoration(decorationItem);
            }
        }
        #endregion
        #region Debug Methods
        
        /// <summary>
        /// Clear inventory for debugging.
        /// </summary>
        [ContextMenu("Clear Inventory")]
        public void ClearInventory()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.ClearInventory();
            }
        }
        #endregion
    }
} 