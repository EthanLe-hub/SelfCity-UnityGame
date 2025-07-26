using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace LifeCraft.Core
{
    /// <summary>
    /// Manages the player's decoration inventory with persistence.
    /// Stores, loads, and updates the player's collection of DecorationItems.
    /// This is a ScriptableObject singleton, so only one instance exists in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "InventoryManager", menuName = "LifeCraft/Inventory Manager")]
    public class InventoryManager : ScriptableObject
    {
        [Header("Inventory Settings")]
        [SerializeField] private int maxInventorySize = 100; // Maximum number of decorations the player can own
        
        // Events for UI and other systems to listen to
        [System.Serializable]
        public class InventoryUpdatedEvent : UnityEvent { }
        public InventoryUpdatedEvent OnInventoryUpdated = new InventoryUpdatedEvent(); // Fired when inventory changes
        
        [System.Serializable]
        public class ItemAddedEvent : UnityEvent<DecorationItem> { }
        public ItemAddedEvent OnItemAdded = new ItemAddedEvent(); // Fired when an item is added
        
        [System.Serializable]
        public class ItemRemovedEvent : UnityEvent<DecorationItem> { }
        public ItemRemovedEvent OnItemRemoved = new ItemRemovedEvent(); // Fired when an item is removed
        
        // The actual inventory list (not serialized by Unity, but saved/loaded manually)
        private List<DecorationItem> _inventory = new List<DecorationItem>();
        
        // Singleton instance pattern for easy access
        private static InventoryManager _instance;
        public static InventoryManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<InventoryManager>("InventoryManager");
                    if (_instance == null)
                    {
                        Debug.LogError("InventoryManager not found in Resources folder!");
                    }
                }
                return _instance;
            }
        }
        
        // Public properties for UI and logic
        public List<DecorationItem> Inventory => new List<DecorationItem>(_inventory); // Returns a copy for safety
        public int ItemCount => _inventory.Count;
        public int MaxSize => maxInventorySize;
        public bool IsFull => _inventory.Count >= maxInventorySize;
        
        // Called when the ScriptableObject is loaded
        private void Awake()
        {
            LoadInventory();
        }
        
        /// <summary>
        /// Add a decoration to the inventory. Returns true if successful.
        /// </summary>
        public bool AddDecoration(DecorationItem decoration)
        {
            if (IsFull)
            {
                Debug.LogWarning("Inventory is full! Cannot add decoration: " + decoration.displayName);
                return false;
            }
            if (decoration == null)
            {
                Debug.LogError("Cannot add null decoration to inventory!");
                return false;
            }
            _inventory.Add(decoration);
            SaveInventory();
            OnItemAdded?.Invoke(decoration);
            OnInventoryUpdated?.Invoke();
            Debug.Log($"Added decoration to inventory: {decoration.displayName} (Total: {_inventory.Count})");
            return true;
        }
        
        /// <summary>
        /// Add a decoration by name (creates a basic DecorationItem).
        /// Used when winning a decoration from a chest or purchasing from a shop.
        /// Now supports setting the region for correct shop filtering in the UI.
        /// </summary>
        public bool AddDecorationByName(string decorationName, string source = "Unknown", bool isPremium = false, RegionType region = RegionType.Decoration)
        {
            // Create a new DecorationItem with the specified name, source, premium status, and region.
            var decoration = new DecorationItem(decorationName, "", DecorationRarity.Common, isPremium)
            {
                source = source,
                region = region // Set the region for shop filtering
            };
            return AddDecoration(decoration);
        }
        
        /// <summary>
        /// Remove a decoration from the inventory.
        /// </summary>
        public bool RemoveDecoration(DecorationItem decoration)
        {
            if (_inventory.Remove(decoration))
            {
                SaveInventory();
                OnItemRemoved?.Invoke(decoration);
                OnInventoryUpdated?.Invoke();
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Remove a decoration by its unique ID.
        /// </summary>
        public bool RemoveDecorationById(string id)
        {
            var decoration = _inventory.FirstOrDefault(item => item.id == id);
            if (decoration != null)
            {
                return RemoveDecoration(decoration);
            }
            return false;
        }
        
        /// <summary>
        /// Get a decoration by its unique ID.
        /// </summary>
        public DecorationItem GetDecorationById(string id)
        {
            return _inventory.FirstOrDefault(item => item.id == id);
        }
        
        /// <summary>
        /// Get all decorations of a specific rarity.
        /// </summary>
        public List<DecorationItem> GetDecorationsByRarity(DecorationRarity rarity)
        {
            return _inventory.Where(item => item.rarity == rarity).ToList();
        }
        
        /// <summary>
        /// Get all premium decorations.
        /// </summary>
        public List<DecorationItem> GetPremiumDecorations()
        {
            return _inventory.Where(item => item.isPremium).ToList();
        }
        
        /// <summary>
        /// Clear the entire inventory (for debugging or reset).
        /// </summary>
        public void ClearInventory()
        {
            _inventory.Clear();
            SaveInventory();
            OnInventoryUpdated?.Invoke();
        }
        
        /// <summary>
        /// Save inventory to PlayerPrefs (persistent storage).
        /// Each DecorationItem is serialized to JSON and joined with '|'.
        /// </summary>
        private void SaveInventory()
        {
            var jsonList = new List<string>();
            foreach (var item in _inventory)
            {
                var json = JsonUtility.ToJson(item);
                jsonList.Add(json);
            }
            var combinedJson = string.Join("|", jsonList);
            PlayerPrefs.SetString("PlayerDecorationInventory", combinedJson);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Load inventory from PlayerPrefs (persistent storage).
        /// </summary>
        private void LoadInventory()
        {
            _inventory.Clear();
            if (!PlayerPrefs.HasKey("PlayerDecorationInventory"))
                return;
            var combinedJson = PlayerPrefs.GetString("PlayerDecorationInventory");
            if (string.IsNullOrEmpty(combinedJson))
                return;
            var jsonList = combinedJson.Split('|');
            foreach (var json in jsonList)
            {
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        var item = JsonUtility.FromJson<DecorationItem>(json);
                        if (item != null)
                        {
                            _inventory.Add(item);
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Failed to load decoration item from JSON: {e.Message}");
                    }
                }
            }
            Debug.Log($"Loaded {_inventory.Count} decorations from inventory");
        }
        
        /// <summary>
        /// For debugging: add some test decorations.
        /// </summary>
        [ContextMenu("Add Test Decorations")]
        public void AddTestDecorations()
        {
            AddDecorationByName("Test Bench", "DecorChest", false);
            AddDecorationByName("Test Fountain", "PremiumDecorChest", true);
            AddDecorationByName("Test Tree", "DecorChest", false);
        }
        
        /// <summary>
        /// For debugging: clear inventory.
        /// </summary>
        [ContextMenu("Clear Inventory")]
        public void DebugClearInventory()
        {
            ClearInventory();
        }
    }
} 