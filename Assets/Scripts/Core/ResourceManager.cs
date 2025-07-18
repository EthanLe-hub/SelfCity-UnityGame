using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LifeCraft.Core
{
    /// <summary>
    /// ScriptableObject-based resource manager that acts as the central "wallet" for the game.
    /// Replaces the Godot ResourceManager.gd autoload singleton.
    /// </summary>
    [CreateAssetMenu(fileName = "ResourceManager", menuName = "LifeCraft/Resource Manager")]
    public class ResourceManager : ScriptableObject
    {
        [System.Serializable]
        public enum ResourceType
        {
            EnergyCrystals,
            WisdomOrbs,
            HeartTokens,
            CreativitySparks,
            BalanceTickets
        }

        [System.Serializable]
        public class ResourceData
        {
            public ResourceType type;
            public int amount;
            public string displayName;
            public Sprite icon;
        }

        [System.Serializable]
        public class HabitReward
        {
            public string habitName;
            public ResourceType resource;
            public int amount;
        }

        [Header("Resource Configuration")]
        [SerializeField] private List<ResourceData> initialResources = new List<ResourceData>
        {
            new ResourceData { type = ResourceType.EnergyCrystals, amount = 100, displayName = "Energy Crystals" },
            new ResourceData { type = ResourceType.WisdomOrbs, amount = 10, displayName = "Wisdom Orbs" },
            new ResourceData { type = ResourceType.HeartTokens, amount = 10, displayName = "Heart Tokens" },
            new ResourceData { type = ResourceType.CreativitySparks, amount = 10, displayName = "Creativity Sparks" },
            new ResourceData { type = ResourceType.BalanceTickets, amount = 0, displayName = "Balance Tickets" }
        };

        [Header("Habit Rewards")]
        [SerializeField] private List<HabitReward> habitRewards = new List<HabitReward>
        {
            new HabitReward { habitName = "exercise", resource = ResourceType.EnergyCrystals, amount = 15 },
            new HabitReward { habitName = "hydration", resource = ResourceType.EnergyCrystals, amount = 5 },
            new HabitReward { habitName = "meditation", resource = ResourceType.WisdomOrbs, amount = 10 },
            new HabitReward { habitName = "journaling", resource = ResourceType.WisdomOrbs, amount = 10 },
            new HabitReward { habitName = "social_connection", resource = ResourceType.HeartTokens, amount = 20 },
            new HabitReward { habitName = "creative_hobby", resource = ResourceType.CreativitySparks, amount = 20 }
        };

        // Events (replacing Godot signals)
        [System.Serializable]
        public class ResourceUpdatedEvent : UnityEvent<ResourceType, int> { }
        
        public ResourceUpdatedEvent OnResourceUpdated = new ResourceUpdatedEvent();

        // Current resource amounts
        private Dictionary<ResourceType, int> _resources = new Dictionary<ResourceType, int>();
        private Dictionary<string, HabitReward> _habitRewardLookup = new Dictionary<string, HabitReward>();

        // Singleton instance
        private static ResourceManager _instance;
        public static ResourceManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<ResourceManager>("ResourceManager");
                    if (_instance == null)
                    {
                        Debug.LogError("ResourceManager not found in Resources folder!");
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            // Initialize resources
            _resources.Clear();
            foreach (var resource in initialResources)
            {
                _resources[resource.type] = resource.amount; 
            }

            // Build habit reward lookup
            _habitRewardLookup.Clear();
            foreach (var reward in habitRewards)
            {
                _habitRewardLookup[reward.habitName] = reward;
            }
        }

        /// <summary>
        /// Get the current amount of a specific resource
        /// </summary>
        public int GetResourceTotal(ResourceType resourceType)
        {
            return _resources.ContainsKey(resourceType) ? _resources[resourceType] : 0;
        }

        /// <summary>
        /// Add resources to the player's wallet
        /// </summary>
        public void AddResources(ResourceType resourceType, int amount)
        {
            if (!_resources.ContainsKey(resourceType))
                _resources[resourceType] = 0;

            _resources[resourceType] += amount;
            
            // Trigger event (replacing Godot signal)
            OnResourceUpdated?.Invoke(resourceType, _resources[resourceType]);
            
            Debug.Log($"Added {amount} {resourceType}. New total: {_resources[resourceType]}");
        }

        /// <summary>
        /// Spend resources from the player's wallet
        /// </summary>
        /// <returns>True if the player could afford it, false otherwise</returns>
        public bool SpendResources(ResourceType resourceType, int amount)
        {
            if (!_resources.ContainsKey(resourceType) || _resources[resourceType] < amount)
            {
                Debug.Log($"Not enough {resourceType} to spend.");
                return false;
            }

            _resources[resourceType] -= amount;
            
            // Trigger event (replacing Godot signal)
            OnResourceUpdated?.Invoke(resourceType, _resources[resourceType]);
            
            Debug.Log($"Spent {amount} {resourceType}. New total: {_resources[resourceType]}");
            return true;
        }

        /// <summary>
        /// Handle habit completion and award resources
        /// </summary>
        public void OnHabitCompleted(string habitName)
        {
            if (_habitRewardLookup.TryGetValue(habitName, out HabitReward reward))
            {
                AddResources(reward.resource, reward.amount);
            }
        }

        /// <summary>
        /// Get display name for a resource type
        /// </summary>
        public string GetResourceDisplayName(ResourceType resourceType)
        {
            var resource = initialResources.Find(r => r.type == resourceType);
            return resource?.displayName ?? resourceType.ToString();
        }

        /// <summary>
        /// Get icon for a resource type
        /// </summary>
        public Sprite GetResourceIcon(ResourceType resourceType)
        {
            var resource = initialResources.Find(r => r.type == resourceType);
            return resource?.icon;
        }

        /// <summary>
        /// Save current resource amounts to local device storage (PlayerPrefs)
        /// This ensures player progress is preserved between game sessions.
        /// 
        /// UPDATE: Implemented local persistence using PlayerPrefs for immediate data saving.
        /// Each resource type is saved with a unique key (e.g., "Resource_EnergyCrystals").
        /// This provides basic data persistence until cloud save is implemented.
        /// </summary>
        public void SaveResources()
        {
            try
            {
                // Save each resource type with a unique key
                foreach (var kvp in _resources)
                {
                    string key = $"Resource_{kvp.Key}";
                    PlayerPrefs.SetInt(key, kvp.Value);
                }
                PlayerPrefs.Save(); // Force write to device storage
                Debug.Log("Resources saved successfully to local storage!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save resources: {e.Message}");
            }
        }

        /// <summary>
        /// Load saved resource amounts from local device storage (PlayerPrefs)
        /// 
        /// UPDATE: Implemented local data loading with fallback to default values.
        /// If saved data exists, it loads the saved amounts and triggers UI updates.
        /// If no saved data exists or loading fails, it falls back to default initialization.
        /// </summary>
        public void LoadResources()
        {
            try
            {
                // Check if we have any saved data first
                bool hasSavedData = false;
                foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
                {
                    string key = $"Resource_{resourceType}";
                    if (PlayerPrefs.HasKey(key))
                    {
                        hasSavedData = true;
                        break;
                    }
                }

                if (hasSavedData)
                {
                    // Load saved values without resetting to defaults first
                    foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
                    {
                        string key = $"Resource_{resourceType}";
                        if (PlayerPrefs.HasKey(key))
                        {
                            int savedAmount = PlayerPrefs.GetInt(key);
                            _resources[resourceType] = savedAmount;
                            
                            // Trigger event to update UI with loaded values
                            OnResourceUpdated?.Invoke(resourceType, savedAmount);
                            Debug.Log($"Loaded {resourceType}: {savedAmount}");
                        }
                    }
                    Debug.Log("Resources loaded successfully from local storage!");
                }
                else
                {
                    // No saved data, initialize with defaults
                    Debug.Log("No saved resource data found, initializing with defaults");
                    Initialize();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load resources: {e.Message}");
                // Fall back to default initialization if loading fails
                Initialize();
            }
        }

        /*
        ================================================================================
        CLOUD SAVE INTEGRATION - TO BE IMPLEMENTED LATER
        ================================================================================
        
        When implementing user accounts and cloud save, replace the above methods with
        cloud-based storage. This will enable cross-device sync and data backup.
        
        Example implementation structure:
        
        public async Task SaveResourcesToCloud(string userId)
        {
            try
            {
                var resourceData = new ResourceSaveData
                {
                    resources = _resources,
                    saveTimestamp = DateTime.UtcNow,
                    version = "1.0"
                };
                
                string jsonData = JsonUtility.ToJson(resourceData);
                await CloudSaveManager.Instance.SaveData(userId, "resources", jsonData);
                
                // Also save locally as backup
                SaveResources();
            }
            catch (Exception e)
            {
                Debug.LogError($"Cloud save failed: {e.Message}");
                // Fall back to local save only
                SaveResources();
            }
        }
        
        public async Task LoadResourcesFromCloud(string userId)
        {
            try
            {
                string jsonData = await CloudSaveManager.Instance.LoadData(userId, "resources");
                if (!string.IsNullOrEmpty(jsonData))
                {
                    var resourceData = JsonUtility.FromJson<ResourceSaveData>(jsonData);
                    _resources = resourceData.resources;
                    
                    // Update UI for all resources
                    foreach (var kvp in _resources)
                    {
                        OnResourceUpdated?.Invoke(kvp.Key, kvp.Value);
                    }
                }
                else
                {
                    // No cloud data, load from local storage
                    LoadResources();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Cloud load failed: {e.Message}");
                // Fall back to local load
                LoadResources();
            }
        }
        
        [System.Serializable]
        public class ResourceSaveData
        {
            public Dictionary<ResourceType, int> resources;
            public DateTime saveTimestamp;
            public string version;
        }
        */
    }
} 