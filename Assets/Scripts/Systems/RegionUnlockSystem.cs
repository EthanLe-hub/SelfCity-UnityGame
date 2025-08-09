using System.Collections.Generic;
using UnityEngine;
using LifeCraft.Core;

namespace LifeCraft.Systems
{
    /// <summary>
    /// Manages region unlocking based on building placement progress.
    /// Extends the base UnlockSystem with region-specific functionality.
    /// </summary>
    [CreateAssetMenu(fileName = "RegionUnlockSystem", menuName = "LifeCraft/Region Unlock System")]
    public class RegionUnlockSystem : ScriptableObject
    {
        // Singleton instance
        private static RegionUnlockSystem _instance;
        public static RegionUnlockSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.Log("RegionUnlockSystem.Instance: Loading from Resources...");
                    _instance = Resources.Load<RegionUnlockSystem>("RegionUnlockSystem");
                    if (_instance == null)
                    {
                        Debug.LogError("RegionUnlockSystem not found in Resources folder!");
                    }
                    else
                    {
                        Debug.Log($"RegionUnlockSystem.Instance: Successfully loaded instance {_instance.GetInstanceID()}");
                    }
                }
                return _instance;
            }
        }

        [System.Serializable]
        public class RegionUnlockData
        {
            public AssessmentQuizManager.RegionType regionType;
            public string regionName;
            public bool isUnlocked = false;
            public int buildingsRequiredToUnlock = 3; // Number of buildings needed to unlock next region
            public int currentBuildingCount = 0;
            public Sprite regionIcon;
            public Color regionColor = Color.white;
        }

        [Header("Region Configuration")]
        [SerializeField] private List<RegionUnlockData> regions = new List<RegionUnlockData>();
        
        [Header("Unlock Requirements")]
        [SerializeField] private int defaultBuildingsRequired = 3;

        // Events
        public System.Action<AssessmentQuizManager.RegionType> OnRegionUnlocked;
        public System.Action<AssessmentQuizManager.RegionType, int> OnBuildingCountChanged;

        // Current state
        private AssessmentQuizManager.RegionType _startingRegion = AssessmentQuizManager.RegionType.HealthHarbor;
        private Dictionary<AssessmentQuizManager.RegionType, RegionUnlockData> _regionData = new Dictionary<AssessmentQuizManager.RegionType, RegionUnlockData>();
        private List<AssessmentQuizManager.RegionType> _unlockOrder = new List<AssessmentQuizManager.RegionType>();
        private int _currentUnlockIndex = 0;

        private void Awake()
        {
            InitializeRegions();
        }

        /// <summary>
        /// Ensure regions are initialized (called from public methods)
        /// </summary>
        private void EnsureInitialized()
        {
            if (_regionData.Count == 0)
            {
                Debug.Log("RegionUnlockSystem not initialized, initializing now...");
                InitializeRegions();
            }
        }

        /// <summary>
        /// Initialize the region unlock system
        /// </summary>
        private void InitializeRegions()
        {
            Debug.Log("Initializing RegionUnlockSystem...");
            _regionData.Clear();

            // Create default regions if none are configured
            if (regions.Count == 0)
            {
                regions = new List<RegionUnlockData>
                {
                    new RegionUnlockData 
                    { 
                        regionType = AssessmentQuizManager.RegionType.HealthHarbor,
                        regionName = "Health Harbor",
                        isUnlocked = false,
                        buildingsRequiredToUnlock = defaultBuildingsRequired,
                        regionColor = new Color(0.49f, 0.85f, 0.34f, 1f)
                    },
                    new RegionUnlockData 
                    { 
                        regionType = AssessmentQuizManager.RegionType.MindPalace,
                        regionName = "Mind Palace",
                        isUnlocked = false,
                        buildingsRequiredToUnlock = defaultBuildingsRequired,
                        regionColor = new Color(0.7f, 0.62f, 0.86f, 1f)
                    },
                    new RegionUnlockData 
                    { 
                        regionType = AssessmentQuizManager.RegionType.CreativeCommons,
                        regionName = "Creative Commons",
                        isUnlocked = false,
                        buildingsRequiredToUnlock = defaultBuildingsRequired,
                        regionColor = new Color(1f, 0.88f, 0.4f, 1f)
                    },
                    new RegionUnlockData 
                    { 
                        regionType = AssessmentQuizManager.RegionType.SocialSquare,
                        regionName = "Social Square",
                        isUnlocked = false,
                        buildingsRequiredToUnlock = defaultBuildingsRequired,
                        regionColor = new Color(1f, 0.7f, 0.28f, 1f)
                    }
                };
            }

            // Build lookup dictionary
            foreach (var region in regions)
            {
                _regionData[region.regionType] = region;
                Debug.Log($"Added region: {region.regionName} ({region.regionType})");
            }
            
            Debug.Log($"RegionUnlockSystem initialized with {_regionData.Count} regions");
        }

        /// <summary>
        /// Set the starting region and unlock order (from quiz results)
        /// </summary>
        public void SetStartingRegion(AssessmentQuizManager.RegionType region, Dictionary<AssessmentQuizManager.RegionType, int> quizScores = null) // The GameManager calls this method to set the starting region and unlock order based on quiz scores. 
        {
            Debug.Log($"=== SET STARTING REGION CALLED ===");
            Debug.Log($"Region parameter: {region}");
            Debug.Log($"Quiz scores: {(quizScores != null ? "provided" : "null")}");
            
            EnsureInitialized();
            
            Debug.Log($"SetStartingRegion called with region: {region}");
            Debug.Log($"Current _regionData count: {_regionData.Count}");
            
            _startingRegion = region;
            
            // Set up unlock order based on quiz scores
            if (quizScores != null)
            {
                Debug.Log("Setting unlock order from quiz scores...");
                SetUnlockOrderFromQuizScores(quizScores, region); // Send the parameters "quizScores" and "region" (the selected starting region) to the SetUnlockOrderFromQuizScores method to set the correct unlock order based on quiz scores and the selected starting region. 
            }
            else
            {
                Debug.Log("Setting default unlock order...");
                // Default unlock order if no quiz scores provided
                SetDefaultUnlockOrder(region);
            }
            
            // Unlock only the starting region
            Debug.Log("Setting unlock flags for all regions...");
            foreach (var kvp in _regionData)
            {
                bool shouldUnlock = (kvp.Key == region);
                Debug.Log($"Processing region {kvp.Key}: shouldUnlock = {shouldUnlock}");
                
                kvp.Value.isUnlocked = shouldUnlock;
                kvp.Value.currentBuildingCount = 0;
                
                Debug.Log($"Region {kvp.Key} ({kvp.Value.regionName}): isUnlocked = {shouldUnlock} (kvp.Key == region: {kvp.Key == region})");
            }

            _currentUnlockIndex = 0; // Start with the first region in unlock order

            Debug.Log($"Starting region set to: {AssessmentQuizManager.GetRegionDisplayName(region)}");
            Debug.Log($"Unlock order: {string.Join(" -> ", _unlockOrder.ConvertAll(r => AssessmentQuizManager.GetRegionDisplayName(r)))}");
            
            // Force unlock the starting region as a test
            Debug.Log("Calling ForceUnlockRegion as backup...");
            ForceUnlockRegion(region);
            
            // Debug: Log the current state
            LogRegionUnlockState();
            
            // Additional verification
            Debug.Log("=== Verification ===");
            var unlockedRegions = GetUnlockedRegions();
            Debug.Log($"GetUnlockedRegions() returns {unlockedRegions.Count} regions: {string.Join(", ", unlockedRegions.ConvertAll(r => AssessmentQuizManager.GetRegionDisplayName(r)))}");
            
            bool isStartingRegionUnlocked = IsRegionUnlocked(region);
            Debug.Log($"IsRegionUnlocked({region}) returns: {isStartingRegionUnlocked}");
            Debug.Log("=== End Verification ===");
            Debug.Log($"=== END SET STARTING REGION ===");
        }
        
        /// <summary>
        /// Force unlock a specific region (for testing)
        /// </summary>
        public void ForceUnlockRegion(AssessmentQuizManager.RegionType region)
        {
            EnsureInitialized();
            
            Debug.Log($"ForceUnlockRegion called with region: {region}");
            
            if (_regionData.TryGetValue(region, out var data))
            {
                data.isUnlocked = true;
                Debug.Log($"Force unlocked region: {data.regionName} (isUnlocked = {data.isUnlocked})");
            }
            else
            {
                Debug.LogError($"Region {region} not found in _regionData!");
            }
        }

        /// <summary>
        /// Debug method to log the current region unlock state
        /// </summary>
        public void LogRegionUnlockState()
        {
            Debug.Log("=== Region Unlock State ===");
            Debug.Log($"Starting region: {AssessmentQuizManager.GetRegionDisplayName(_startingRegion)}");
            Debug.Log($"Current unlock index: {_currentUnlockIndex}");
            
            var unlockedRegions = GetUnlockedRegions();
            Debug.Log($"Unlocked regions ({unlockedRegions.Count}): {string.Join(", ", unlockedRegions.ConvertAll(r => AssessmentQuizManager.GetRegionDisplayName(r)))}");
            
            var lockedRegions = GetLockedRegions();
            Debug.Log($"Locked regions ({lockedRegions.Count}): {string.Join(", ", lockedRegions.ConvertAll(r => AssessmentQuizManager.GetRegionDisplayName(r)))}");
            
            var nextRegion = GetNextRegionToUnlock();
            if (nextRegion.HasValue)
            {
                Debug.Log($"Next region to unlock: {AssessmentQuizManager.GetRegionDisplayName(nextRegion.Value)}");
            }
            else
            {
                Debug.Log("No next region to unlock (all regions unlocked or no regions available)");
            }
            Debug.Log("==========================");
        }

        /// <summary>
        /// Check if a region is unlocked
        /// </summary>
        public bool IsRegionUnlocked(AssessmentQuizManager.RegionType region)
        {
            EnsureInitialized();
            return _regionData.TryGetValue(region, out var data) && data.isUnlocked;
        }

        /// <summary>
        /// Get the starting region
        /// </summary>
        public AssessmentQuizManager.RegionType GetStartingRegion()
        {
            EnsureInitialized();
            return _startingRegion;
        }

        /// <summary>
        /// Add a building to a region
        /// </summary>
        public void AddBuildingToRegion(AssessmentQuizManager.RegionType region)
        {
            EnsureInitialized();
            if (!_regionData.TryGetValue(region, out var data))
                return;

            data.currentBuildingCount++;
            OnBuildingCountChanged?.Invoke(region, data.currentBuildingCount);

            Debug.Log($"Added building to {data.regionName}. Count: {data.currentBuildingCount}/{data.buildingsRequiredToUnlock}");
        }

        /// <summary>
        ///  Unlock a region when called by PlayerLevelManager (PlayerLevelManager is the controller).
        /// This is called when the first building of a region becomes available at the current level. 
        /// </summary>
        /// <param name="region"></param>
        public void UnlockRegionByLevel(AssessmentQuizManager.RegionType region)
        {
            EnsureInitialized();

            // _regionData is a Dictionary that stores: Key = RegionType, Value = RegionUnlockData
            if (!_regionData.TryGetValue(region, out var data)) // If the region cannot be found in the existing region data, 
            {
                Debug.LogError($"Region {region} not found in unlock system");
                return;
            }

            // "data" variable now contains the RegionUnlockData object for "region" (Health Harbor, Mind Palace, etc.)
            // It now contains information about "region":
            // regionType, regionName, isUnlocked flag, buildingsRequiredToUnlock, currentBuildingCount, regionIcon, regionColor. 

            if (data.isUnlocked) // If the region's unlock flag is already set as true, (boolean property of RegionUnlockDatato indicate unlock status)
            {
                Debug.LogWarning($"Region {region} is already unlocked!");
                return;
            }

            // Otherwise, unlock the region:
            data.isUnlocked = true; // Set the region's unlock flag as true. 

            // Update the current unlock index to point to the next region in the unlock order:
            for (int i = 0; i < _unlockOrder.Count; i++)
            {
                if (_unlockOrder[i] == region)
                {
                    _currentUnlockIndex = i + 1; // Move to the next region in the unlock order. (Increment the current unlock index to the next region to be unlocked)
                    break;
                } // This is needed so that methods like GetNextRegionToUnlock() know what is the next region to unlock and how many are left. It also keeps the system maintained for unlocking regions in the correct order. 
            }

            // Trigger events:
            OnRegionUnlocked?.Invoke(region); // Alerts systems that a region has been unlocked so that the UI only needs to update when this alert happens. 

            // Save the game to persist the unlock state
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SaveGame();
            }

            Debug.Log($"Region {AssessmentQuizManager.GetRegionDisplayName(region)} unlocked by level progression!");
        }

        /// <summary>
        /// Remove a building from a region
        /// </summary>
        public void RemoveBuildingFromRegion(AssessmentQuizManager.RegionType region)
        {
            EnsureInitialized();
            if (!_regionData.TryGetValue(region, out var data))
                return;

            data.currentBuildingCount = Mathf.Max(0, data.currentBuildingCount - 1);
            OnBuildingCountChanged?.Invoke(region, data.currentBuildingCount);

            Debug.Log($"Removed building from {data.regionName}. Count: {data.currentBuildingCount}/{data.buildingsRequiredToUnlock}");
        }

        /// <summary>
        /// Get building count for a region
        /// </summary>
        public int GetBuildingCount(AssessmentQuizManager.RegionType region)
        {
            EnsureInitialized();
            return _regionData.TryGetValue(region, out var data) ? data.currentBuildingCount : 0;
        }

        /// <summary>
        /// Get buildings required to unlock next region
        /// </summary>
        public int GetBuildingsRequired(AssessmentQuizManager.RegionType region)
        {
            EnsureInitialized();
            return _regionData.TryGetValue(region, out var data) ? data.buildingsRequiredToUnlock : defaultBuildingsRequired;
        }

        /// <summary>
        /// Get progress (0-1) towards unlocking next region
        /// </summary>
        public float GetUnlockProgress(AssessmentQuizManager.RegionType region)
        {
            EnsureInitialized();
            if (!_regionData.TryGetValue(region, out var data))
                return 0f;

            return Mathf.Clamp01((float)data.currentBuildingCount / data.buildingsRequiredToUnlock);
        }

        /// <summary>
        /// Get all unlocked regions
        /// </summary>
        public List<AssessmentQuizManager.RegionType> GetUnlockedRegions()
        {
            EnsureInitialized();
            var unlocked = new List<AssessmentQuizManager.RegionType>();
            
            Debug.Log($"=== GetUnlockedRegions called ===");
            Debug.Log($"Total regions in _regionData: {_regionData.Count}");
            
            foreach (var kvp in _regionData)
            {
                Debug.Log($"Region {kvp.Key} ({kvp.Value.regionName}): isUnlocked = {kvp.Value.isUnlocked}");
                if (kvp.Value.isUnlocked)
                {
                    unlocked.Add(kvp.Key);
                    Debug.Log($"Added {kvp.Key} to unlocked list");
                }
            }

            Debug.Log($"Returning {unlocked.Count} unlocked regions: {string.Join(", ", unlocked.ConvertAll(r => AssessmentQuizManager.GetRegionDisplayName(r)))}");
            Debug.Log($"=== End GetUnlockedRegions ===");
            return unlocked;
        }

        /// <summary>
        /// Get all locked regions
        /// </summary>
        public List<AssessmentQuizManager.RegionType> GetLockedRegions()
        {
            EnsureInitialized();
            var locked = new List<AssessmentQuizManager.RegionType>();
            foreach (var kvp in _regionData)
            {
                if (!kvp.Value.isUnlocked)
                {
                    locked.Add(kvp.Key);
                }
            }
            return locked;
        }

        /// <summary>
        /// Get region data for a specific region
        /// </summary>
        public RegionUnlockData GetRegionData(AssessmentQuizManager.RegionType region)
        {
            EnsureInitialized();
            return _regionData.TryGetValue(region, out var data) ? data : null;
        }

        /// <summary>
        /// Get all region data
        /// </summary>
        public List<RegionUnlockData> GetAllRegionData()
        {
            EnsureInitialized();
            return new List<RegionUnlockData>(regions);
        }

        /// <summary>
        /// Check if all regions are unlocked
        /// </summary>
        public bool AreAllRegionsUnlocked()
        {
            EnsureInitialized();
            foreach (var kvp in _regionData)
            {
                if (!kvp.Value.isUnlocked)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Get the unlock order list
        /// </summary>
        public List<AssessmentQuizManager.RegionType> GetUnlockOrder()
        {
            EnsureInitialized();
            return new List<AssessmentQuizManager.RegionType>(_unlockOrder);
        }

        /// <summary>
        /// Get the next region to unlock based on quiz score order
        /// </summary>
        public AssessmentQuizManager.RegionType? GetNextRegionToUnlock()
        {
            EnsureInitialized();
            // If all regions are unlocked, return null
            if (AreAllRegionsUnlocked())
                return null;

            // Find the next locked region in the unlock order
            for (int i = 0; i < _unlockOrder.Count; i++)
            {
                var region = _unlockOrder[i];
                if (!_regionData[region].isUnlocked)
                {
                    return region;
                }
            }
            
            return null;
        }

        /// <summary>
        /// Set unlock order based on quiz scores (highest to lowest) and based on the selected starting region
        /// </summary>
        private void SetUnlockOrderFromQuizScores(Dictionary<AssessmentQuizManager.RegionType, int> quizScores, AssessmentQuizManager.RegionType selectedRegion)
        {
            _unlockOrder.Clear();
            
            // Sort regions by quiz score (highest to lowest)
            var sortedRegions = new List<AssessmentQuizManager.RegionType>(quizScores.Keys);
            sortedRegions.Sort((a, b) => quizScores[b].CompareTo(quizScores[a])); // Descending order

            // Ensure the selected starting region is first in the order:
            if (sortedRegions.Contains(selectedRegion))
            {
                sortedRegions.Remove(selectedRegion); // Remove the selected starting region from the list.
                sortedRegions.Insert(0, selectedRegion); // Then re-insert it back into the list, placed at the start of the list (index 0).
            }
            
            _unlockOrder.AddRange(sortedRegions);
        }

        /// <summary>
        /// Set default unlock order if no quiz scores provided
        /// </summary>
        private void SetDefaultUnlockOrder(AssessmentQuizManager.RegionType startingRegion)
        {
            _unlockOrder.Clear();
            _unlockOrder.Add(startingRegion);
            
            // Add remaining regions in default order
            var allRegions = new List<AssessmentQuizManager.RegionType>
            {
                AssessmentQuizManager.RegionType.HealthHarbor,
                AssessmentQuizManager.RegionType.MindPalace,
                AssessmentQuizManager.RegionType.CreativeCommons,
                AssessmentQuizManager.RegionType.SocialSquare
            };
            
            foreach (var region in allRegions)
            {
                if (region != startingRegion)
                {
                    _unlockOrder.Add(region);
                }
            }
        }

        private void UnlockNextRegion()
        {
            var nextRegion = GetNextRegionToUnlock();
            if (nextRegion.HasValue)
            {
                _regionData[nextRegion.Value].isUnlocked = true;
                
                // Update current unlock index to point to the next region in the order
                for (int i = 0; i < _unlockOrder.Count; i++)
                {
                    if (_unlockOrder[i] == nextRegion.Value)
                    {
                        _currentUnlockIndex = i + 1; // Move to next region in unlock order
                        break;
                    }
                }
                
                OnRegionUnlocked?.Invoke(nextRegion.Value);
                
                Debug.Log($"Unlocked new region: {AssessmentQuizManager.GetRegionDisplayName(nextRegion.Value)}");
            }
        }

        /// <summary>
        /// Set unlock order without resetting unlock states (for load-time re-initialization)
        /// </summary>
        public void SetUnlockOrderOnly(AssessmentQuizManager.RegionType region, Dictionary<AssessmentQuizManager.RegionType, int> quizScores = null)
        {
            Debug.Log($"Setting unlock order only for region: {region}");
            
            EnsureInitialized();
            
            _startingRegion = region; // Assign the selected starting region to the variable. 
            
            // Set up unlock order based on quiz scores
            if (quizScores != null)
            {
                SetUnlockOrderFromQuizScores(quizScores, region);
            }
            else
            {
                SetDefaultUnlockOrder(region);
            }
            
            // DON'T reset unlock states - preserve existing unlocked regions
            Debug.Log($"Unlock order set. Starting region: {AssessmentQuizManager.GetRegionDisplayName(region)}");
            Debug.Log($"Unlock order: {string.Join(" -> ", _unlockOrder.ConvertAll(r => AssessmentQuizManager.GetRegionDisplayName(r)))}");
        }

        /// <summary>
        /// Save region unlock data
        /// </summary>
        public RegionUnlockSaveData GetSaveData()
        {
            EnsureInitialized();
            var saveData = new RegionUnlockSaveData
            {
                startingRegion = _startingRegion,
                regionStates = new Dictionary<AssessmentQuizManager.RegionType, RegionState>(),
                unlockOrder = new List<AssessmentQuizManager.RegionType>(_unlockOrder),
                currentUnlockIndex = _currentUnlockIndex
            };

            foreach (var kvp in _regionData)
            {
                saveData.regionStates[kvp.Key] = new RegionState
                {
                    isUnlocked = kvp.Value.isUnlocked,
                    currentBuildingCount = kvp.Value.currentBuildingCount
                };
            }

            return saveData;
        }

        /// <summary>
        /// Load region unlock data
        /// </summary>
        public void LoadSaveData(RegionUnlockSaveData saveData)
        {
            if (saveData == null) return;

            // Ensure regions are initialized before loading save data
            EnsureInitialized();

            _startingRegion = saveData.startingRegion;
            _unlockOrder = new List<AssessmentQuizManager.RegionType>(saveData.unlockOrder ?? new List<AssessmentQuizManager.RegionType>());
            _currentUnlockIndex = saveData.currentUnlockIndex;

            // Safely load region states - handle null dictionary
            if (saveData.regionStates != null)
            {
                foreach (var kvp in saveData.regionStates)
                {
                    if (_regionData.TryGetValue(kvp.Key, out var data))
                    {
                        data.isUnlocked = kvp.Value.isUnlocked;
                        data.currentBuildingCount = kvp.Value.currentBuildingCount;
                    }
                }
            }

            Debug.Log($"Loaded region unlock data. Starting region: {AssessmentQuizManager.GetRegionDisplayName(_startingRegion)}");
        }

        /// <summary>
        /// Reset all region unlocks (for testing)
        /// </summary>
        public void ResetRegionUnlocks()
        {
            EnsureInitialized();
            foreach (var kvp in _regionData)
            {
                kvp.Value.isUnlocked = false;
                kvp.Value.currentBuildingCount = 0;
            }
            
            // Only unlock starting region
            if (_regionData.TryGetValue(_startingRegion, out var data))
            {
                data.isUnlocked = true;
            }
        }

        /// <summary>
        /// Reset all regions to locked state (for testing)
        /// </summary>
        public void ResetAllRegionsToLocked()
        {
            Debug.Log("=== RESETTING ALL REGIONS TO LOCKED ===");
            
            EnsureInitialized();
            
            foreach (var kvp in _regionData)
            {
                kvp.Value.isUnlocked = false;
                kvp.Value.currentBuildingCount = 0;
                Debug.Log($"Reset {kvp.Value.regionName} to locked");
            }
            
            _currentUnlockIndex = 0;
            
            Debug.Log("All regions reset to locked state");
            Debug.Log("=== END RESET ===");
        }
        
        /// <summary>
        /// Test method to directly unlock a region (for debugging)
        /// </summary>
        public void TestUnlockRegion(AssessmentQuizManager.RegionType region)
        {
            Debug.Log($"=== TEST UNLOCK REGION ===");
            Debug.Log($"Testing unlock for region: {region}");
            
            EnsureInitialized();
            Debug.Log($"_regionData count: {_regionData.Count}");
            
            if (_regionData.TryGetValue(region, out var data))
            {
                Debug.Log($"Found region data: {data.regionName}");
                Debug.Log($"Before unlock: isUnlocked = {data.isUnlocked}");
                
                data.isUnlocked = true;
                
                Debug.Log($"After unlock: isUnlocked = {data.isUnlocked}");
                
                // Test the IsRegionUnlocked method
                bool isUnlocked = IsRegionUnlocked(region);
                Debug.Log($"IsRegionUnlocked({region}) returns: {isUnlocked}");
                
                // Test GetUnlockedRegions
                var unlockedRegions = GetUnlockedRegions();
                Debug.Log($"GetUnlockedRegions() returns: {string.Join(", ", unlockedRegions.ConvertAll(r => AssessmentQuizManager.GetRegionDisplayName(r)))}");
                
                Debug.Log($"=== END TEST ===");
            }
            else
            {
                Debug.LogError($"Region {region} not found in _regionData!");
                Debug.Log($"Available regions: {string.Join(", ", _regionData.Keys)}");
            }
        }
    }

    /// <summary>
    /// Save data for region unlock system
    /// </summary>
    [System.Serializable]
    public class RegionUnlockSaveData
    {
        public AssessmentQuizManager.RegionType startingRegion;
        public Dictionary<AssessmentQuizManager.RegionType, RegionState> regionStates;
        public List<AssessmentQuizManager.RegionType> unlockOrder;
        public int currentUnlockIndex;
    }

    /// <summary>
    /// Individual region state for saving
    /// </summary>
    [System.Serializable]
    public class RegionState
    {
        public bool isUnlocked;
        public int currentBuildingCount;
    }
} 