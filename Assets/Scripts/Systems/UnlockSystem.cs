using LifeCraft.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LifeCraft.Systems
{
    /// <summary>
    /// Manages building unlocks and progression system.
    /// Replaces the Godot UnlockSystem.gd script.
    /// </summary>
    [CreateAssetMenu(fileName = "UnlockSystem", menuName = "LifeCraft/Unlock System")]
    public class UnlockSystem : ScriptableObject
    {
        [System.Serializable]
        public class UnlockableItem
        {
            public string itemId;
            public string displayName;
            public string description;
            public Sprite icon;
            public bool isUnlocked = false;
            public List<string> prerequisites = new List<string>();
            public int unlockLevel = 1;
            public ResourceManager.ResourceType unlockCostResource;
            public int unlockCostAmount;
        }

        [System.Serializable]
        public class UnlockableBuilding : UnlockableItem
        {
            public GameObject buildingPrefab;
            public Vector2Int size = Vector2Int.one;
        }

        [Header("Unlockable Items")]
        [SerializeField] private List<UnlockableBuilding> unlockableBuildings = new List<UnlockableBuilding>();
        [SerializeField] private List<UnlockableItem> unlockableFeatures = new List<UnlockableItem>();

        // Events
        [System.Serializable]
        public class ItemUnlockedEvent : UnityEvent<string> { }
        public ItemUnlockedEvent OnItemUnlocked = new ItemUnlockedEvent();

        // Current unlock state
        private Dictionary<string, UnlockableItem> _allItems = new Dictionary<string, UnlockableItem>();
        private Dictionary<string, bool> _unlockedItems = new Dictionary<string, bool>();

        private void Awake()
        {
            InitializeUnlockSystem();
        }

        /// <summary>
        /// Initialize the unlock system
        /// </summary>
        private void InitializeUnlockSystem()
        {
            _allItems.Clear();
            _unlockedItems.Clear();

            // Add buildings
            foreach (var building in unlockableBuildings)
            {
                _allItems[building.itemId] = building;
                _unlockedItems[building.itemId] = building.isUnlocked;
            }

            // Add features
            foreach (var feature in unlockableFeatures)
            {
                _allItems[feature.itemId] = feature;
                _unlockedItems[feature.itemId] = feature.isUnlocked;
            }
        }

        /// <summary>
        /// Check if a building is unlocked
        /// </summary>
        public bool IsBuildingUnlocked(string buildingId)
        {
            return _unlockedItems.TryGetValue(buildingId, out bool unlocked) && unlocked;
        }

        /// <summary>
        /// Check if a feature is unlocked
        /// </summary>
        public bool IsFeatureUnlocked(string featureId)
        {
            return _unlockedItems.TryGetValue(featureId, out bool unlocked) && unlocked;
        }

        /// <summary>
        /// Unlock an item
        /// </summary>
        public bool UnlockItem(string itemId)
        {
            if (!_allItems.TryGetValue(itemId, out UnlockableItem item))
            {
                Debug.LogWarning($"Item '{itemId}' not found in unlock system.");
                return false;
            }

            if (_unlockedItems[itemId])
            {
                Debug.LogWarning($"Item '{itemId}' is already unlocked.");
                return false;
            }

            // Check prerequisites
            if (!CheckPrerequisites(item))
            {
                Debug.LogWarning($"Prerequisites not met for '{itemId}'.");
                return false;
            }

            // Unlock the item
            _unlockedItems[itemId] = true;
            item.isUnlocked = true;

            // Trigger event
            OnItemUnlocked?.Invoke(itemId);

            Debug.Log($"Unlocked: {item.displayName}");
            return true;
        }

        /// <summary>
        /// Check if prerequisites are met for an item
        /// </summary>
        private bool CheckPrerequisites(UnlockableItem item)
        {
            foreach (string prerequisite in item.prerequisites)
            {
                if (!_unlockedItems.TryGetValue(prerequisite, out bool unlocked) || !unlocked)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Get unlock cost for an item
        /// </summary>
        public (ResourceManager.ResourceType resource, int amount) GetUnlockCost(string itemId)
        {
            if (_allItems.TryGetValue(itemId, out UnlockableItem item))
            {
                return (item.unlockCostResource, item.unlockCostAmount);
            }
            return (ResourceManager.ResourceType.EnergyCrystals, 0);
        }

        /// <summary>
        /// Get all unlockable buildings
        /// </summary>
        public List<UnlockableBuilding> GetUnlockableBuildings()
        {
            return new List<UnlockableBuilding>(unlockableBuildings);
        }

        /// <summary>
        /// Get all unlockable features
        /// </summary>
        public List<UnlockableItem> GetUnlockableFeatures()
        {
            return new List<UnlockableItem>(unlockableFeatures);
        }

        /// <summary>
        /// Get unlocked buildings
        /// </summary>
        public List<UnlockableBuilding> GetUnlockedBuildings()
        {
            var unlocked = new List<UnlockableBuilding>();
            foreach (var building in unlockableBuildings)
            {
                if (_unlockedItems.TryGetValue(building.itemId, out bool unlockedFlag) && unlockedFlag)
                {
                    unlocked.Add(building);
                }
            }
            return unlocked;
        }

        /// <summary>
        /// Get unlocked features
        /// </summary>
        public List<UnlockableItem> GetUnlockedFeatures()
        {
            var unlocked = new List<UnlockableItem>();
            foreach (var feature in unlockableFeatures)
            {
                if (_unlockedItems.TryGetValue(feature.itemId, out bool unlockedFlag) && unlockedFlag)
                {
                    unlocked.Add(feature);
                }
            }
            return unlocked;
        }

        /// <summary>
        /// Get building prefab by ID
        /// </summary>
        public GameObject GetBuildingPrefab(string buildingId)
        {
            foreach (var building in unlockableBuildings)
            {
                if (building.itemId == buildingId)
                {
                    return building.buildingPrefab;
                }
            }
            return null;
        }

        /// <summary>
        /// Get item data by ID
        /// </summary>
        public UnlockableItem GetItemData(string itemId)
        {
            return _allItems.TryGetValue(itemId, out UnlockableItem item) ? item : null;
        }

        /// <summary>
        /// Get building data by ID
        /// </summary>
        public UnlockableBuilding GetBuildingData(string buildingId)
        {
            foreach (var building in unlockableBuildings)
            {
                if (building.itemId == buildingId)
                {
                    return building;
                }
            }
            return null;
        }

        /// <summary>
        /// Save unlock state
        /// </summary>
        public UnlockSaveData GetSaveData()
        {
            return new UnlockSaveData
            {
                unlockedItems = new Dictionary<string, bool>(_unlockedItems)
            };
        }

        /// <summary>
        /// Load unlock state
        /// </summary>
        public void LoadSaveData(UnlockSaveData saveData)
        {
            if (saveData?.unlockedItems != null)
            {
                _unlockedItems = new Dictionary<string, bool>(saveData.unlockedItems);
                
                // Update item states
                foreach (var kvp in _unlockedItems)
                {
                    if (_allItems.TryGetValue(kvp.Key, out UnlockableItem item))
                    {
                        item.isUnlocked = kvp.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Reset all unlocks (for testing)
        /// </summary>
        public void ResetUnlocks()
        {
            foreach (var item in _allItems.Values)
            {
                item.isUnlocked = false;
            }
            _unlockedItems.Clear();
            InitializeUnlockSystem();
        }
    }

    /// <summary>
    /// Save data for unlock system
    /// </summary>
    [System.Serializable]
    public class UnlockSaveData
    {
        public Dictionary<string, bool> unlockedItems;
    }
} 