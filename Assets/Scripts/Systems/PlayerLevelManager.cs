using System.Collections.Generic;
using UnityEngine;
using LifeCraft.Core;
using LifeCraft.Systems;
using LifeCraft.Shop; // Import the Shop namespace to access BuildingShopItem and BuildingShopDatabase. 
using LifeCraft.UI; // Import the UI namespace to access AssessmentQuizUI and AssessmentQuizManager. 

namespace LifeCraft.Systems
{

    [System.Serializable]
    public class BuildingUnlockSaveData
    {
        public Dictionary<string, int> buildingUnlockLevels; // Dictionary to hold building names and their corresponding unlock levels: (Key = building name, Value = unlock level).
        public Dictionary<AssessmentQuizManager.RegionType, List<string>> regionBuildings; // Dictionary to hold region type and each of their corresponding buildings: (Key = region type, Value = list of building names).
        public Dictionary<string, float> buildingConstructionTimes; // Dictionary to hold building names and their corresponding construction times: (Key = building name, Value = construction time in minutes).
        public int currentLevel; // Current player level. 
        public int currentEXP; // Current player EXP. 
        public string saveVersion = "1.0"; // Version of the save data format (for future compatibility). 
    }

    /// <summary>
    /// Manages player leveling, EXP, and building unlocks based on level and region unlock sequence.
    /// </summary>
    public class PlayerLevelManager : MonoBehaviour
    {
        [Header("Level Configuration")]
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private int currentEXP = 0;

        [Header("EXP Configuration")]
        [SerializeField] private int baseEXPPerLevel = 50;
        [SerializeField] private float expMultiplier = 1.15f; // Exponential progression

        // Events
        public System.Action<int> OnLevelUp;
        public System.Action<int> OnEXPChanged;
        public System.Action<string> OnBuildingUnlocked;
        public System.Action<AssessmentQuizManager.RegionType> OnRegionUnlocked;

        // Building unlock data
        // Dictionary to hold building names and their corresponding unlock levels: (Key = building name, Value = unlock level)
        private Dictionary<string, int> _buildingUnlockLevels = new Dictionary<string, int>();
        // Dictionary to hold region type and each of their corresponding buildings: (Key = region type, Value = list of building names)
        private Dictionary<AssessmentQuizManager.RegionType, List<string>> _regionBuildings = new Dictionary<AssessmentQuizManager.RegionType, List<string>>();
        // Dictionary to hold building names and their corresponding construction times: (Key = building name, Value = construction time in minutes)
        private Dictionary<string, float> _buildingConstructionTimes = new Dictionary<string, float>();

        // Singleton
        private static PlayerLevelManager _instance;
        public static PlayerLevelManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<PlayerLevelManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("PlayerLevelManager");
                        _instance = go.AddComponent<PlayerLevelManager>();
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);

                if (AssessmentQuizUI.Instance != null) // If the AssessmentQuizUI instance exists, 
                {
                    AssessmentQuizUI.Instance.OnRegionSelected += InitializeBuildingUnlockSystem; // Subscribe to the on-region-selected event to initialize the building unlock system (the initialization method would be called AFTER the player clicks on their desired starting region). 
                }
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (AssessmentQuizUI.Instance != null) // If the AssessmentQuizUI instance exists, 
            {
                AssessmentQuizUI.Instance.OnRegionSelected -= InitializeBuildingUnlockSystem; // Unsubscribe from the on-region-selected event to avoid memory leaks. 
            }
        }

        /// <summary>
        /// Initialize the building unlock system based on region unlock sequence
        /// </summary>
                /// <summary>
        /// Initialize the building unlock system based on region unlock sequence
        /// This is the original method called during normal gameplay (after assessment quiz completion)
        /// </summary>
        /// <param name="selectedRegion">The region the player selected from the assessment quiz</param>
        public void InitializeBuildingUnlockSystem(AssessmentQuizManager.RegionType selectedRegion) // The RegionType parameter is the SELECTED REGION (the region the player clicks) from the AssessmentQuizManager, which is used to determine the player's starting region.
        {
            // Get quiz scores from AssessmentQuizManager for normal initialization
            // This method is called during normal gameplay when the assessment quiz is completed
            var quizScores = AssessmentQuizManager.Instance.GetRegionScores();
            InitializeBuildingUnlockSystem(selectedRegion, quizScores);
        }

        /// <summary>
        /// CRITICAL FIX: Overloaded version that accepts quiz scores as parameter for re-initialization during game load
        /// 
        /// PROBLEM SOLVED: During game load, the AssessmentQuizManager might not have the quiz scores loaded,
        /// so we need to pass them as parameters instead of trying to get them from the AssessmentQuizManager instance.
        /// 
        /// This method is called during game load to re-initialize the building unlock system with saved quiz scores
        /// and region selection, ensuring that buildings appear correctly in the shop after loading a saved game.
        /// </summary>
        /// <param name="selectedRegion">The region the player originally selected from the assessment quiz</param>
        /// <param name="quizScores">The quiz scores that determine the region unlock order</param>
        public void InitializeBuildingUnlockSystem(AssessmentQuizManager.RegionType selectedRegion, Dictionary<AssessmentQuizManager.RegionType, int> quizScores) // Overloaded version that accepts quiz scores as parameter for re-initialization during game load.
        {
            // STEP 1: Clear existing data to avoid duplicates and start fresh
            _buildingUnlockLevels.Clear(); // Clear the existing building unlock levels Dictionary to start fresh. (Key = building name, Value = unlock level)
            _regionBuildings.Clear(); // Clear the existing region buildings Dictionary to start fresh. (Key = region type, Value = list of building names) 

            // STEP 2: Get the RegionUnlockSystem instance to manage region unlocking
            var regionUnlockSystem = RegionUnlockSystem.Instance; // Get the instance of RegionUnlockSystem to get the correct unlock sequence. 

            if (regionUnlockSystem == null)
            {
                Debug.LogError("RegionUnlockSystem.Instance is null!");
                return; // Exit if the region unlock system is not initialized. 
            }

            // STEP 3: Validate quiz scores (can be null during game load if no quiz was taken)
            if (quizScores == null || quizScores.Count == 0)
            {
                Debug.LogWarning("No quiz scores available - using default unlock order.");
                // No need to return here, as we can still proceed with the default unlock order. 
            }

            // STEP 4: Set the starting region and determine unlock order based on quiz scores
            // This is the key step that determines which regions unlock in what order
            regionUnlockSystem.SetUnlockOrderOnly(selectedRegion, quizScores); // Set the REGION UNLOCK ORDER based on the player's selection for starting region and quiz scores. This will also determine the unlock order of regions based on the quiz scores.  
            var unlockOrder = regionUnlockSystem.GetUnlockOrder(); // Call the "GetUnlockOrder" method from the RegionUnlockSystem file to get the correct unlock order of regions. 

            Debug.Log($"THE PROPER Region unlock order: {string.Join(", ", unlockOrder)}"); // Log the unlock order for debugging purposes.

            // STEP 5: Sort all buildings from all regions based on the unlock sequence and excitement level
            // Buildings from regions that unlock first will be available at lower levels
            var sortedBuildings = SortBuildingsByUnlockSequence(unlockOrder); // Parameters are the unlock order, and the list of all buildings. 

            // STEP 6: Assign unlock levels to buildings (1-40) based on their position in the sorted list
            // This is what determines when each building becomes available in the shop
            AssignUnlockLevelsToBuildings(sortedBuildings); // Assign levels to buildings based on their position in the sorted list.  

            Debug.Log("Building unlock system initialized successfully");

            // STEP 7: Calculate and store construction times for all buildings
            Debug.Log("Calculating construction times for all buildings...");
            CalculateConstructionTimesForAllBuildings();

            // STEP 8: Check for region unlocks immediately after initialization
            Debug.Log("Checking for region unlocks after initialization...");
            CheckForRegionUnlocks();

            // STEP 9: Save the game to persist the building unlock data
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SaveGame();
            }
            
            Debug.Log($"=== END PLAYER LEVEL MANAGER INITIALIZE BUILDING UNLOCK SYSTEM ===");
        }

        /// <summary>
        /// Calculate construction times for all buildings based on their unlock levels
        /// This ensures proper distribution: 1 minute for lowest level, 6 hours for highest level
        /// </summary>
        private void CalculateConstructionTimesForAllBuildings()
        {
            Debug.Log("=== CALCULATING CONSTRUCTION TIMES ===");
            
            if (_buildingUnlockLevels.Count == 0)
            {
                Debug.LogWarning("No building unlock levels found - cannot calculate construction times");
                return;
            }

            // Clear existing construction times
            _buildingConstructionTimes.Clear();

            // Find the minimum and maximum unlock levels to establish the range
            int minUnlockLevel = int.MaxValue;
            int maxUnlockLevel = int.MinValue;
            
            foreach (var kvp in _buildingUnlockLevels)
            {
                minUnlockLevel = Mathf.Min(minUnlockLevel, kvp.Value);
                maxUnlockLevel = Mathf.Max(maxUnlockLevel, kvp.Value);
            }

            Debug.Log($"Unlock level range: {minUnlockLevel} to {maxUnlockLevel}");

            // Debug: Log all unlock levels to see what's happening
            Debug.Log("=== ALL UNLOCK LEVELS ===");
            foreach (var kvp in _buildingUnlockLevels)
            {
                Debug.Log($"Building: {kvp.Key}, Unlock Level: {kvp.Value}");
            }
            Debug.Log("=== END ALL UNLOCK LEVELS ===");

            // Calculate construction times for each building
            foreach (var kvp in _buildingUnlockLevels)
            {
                string buildingName = kvp.Key;
                int unlockLevel = kvp.Value;
                
                // Calculate construction time using linear interpolation
                // Formula: minTime + (unlockLevel - minLevel) * (maxTime - minTime) / (maxLevel - minLevel)
                float minTimeMinutes = 1f; // 1 minute for lowest level
                float maxTimeMinutes = 360f; // 6 hours (360 minutes) for highest level
                
                float constructionTimeMinutes;
                if (maxUnlockLevel == minUnlockLevel)
                {
                    // All buildings have the same unlock level, use average time
                    constructionTimeMinutes = (minTimeMinutes + maxTimeMinutes) / 2f;
                    Debug.Log($"All buildings have same level ({maxUnlockLevel}), using average time: {constructionTimeMinutes}");
                }
                else
                {
                    // Linear interpolation based on unlock level
                    float progress = (float)(unlockLevel - minUnlockLevel) / (maxUnlockLevel - minUnlockLevel);
                    constructionTimeMinutes = minTimeMinutes + (progress * (maxTimeMinutes - minTimeMinutes));
                    Debug.Log($"Level {unlockLevel}: progress = ({unlockLevel} - {minUnlockLevel}) / ({maxUnlockLevel} - {minUnlockLevel}) = {progress:F3}");
                    Debug.Log($"Level {unlockLevel}: time = {minTimeMinutes} + ({progress:F3} * {maxTimeMinutes - minTimeMinutes}) = {constructionTimeMinutes:F1} minutes");
                }
                
                // Store the construction time
                _buildingConstructionTimes[buildingName] = constructionTimeMinutes;
                
                Debug.Log($"Building '{buildingName}' (Level {unlockLevel}): {constructionTimeMinutes:F1} minutes construction time");
            }
            
            Debug.Log($"Calculated construction times for {_buildingConstructionTimes.Count} buildings");
            Debug.Log("=== END CALCULATING CONSTRUCTION TIMES ===");
        }

        // Helper Function (2): Sort buildings based on region unlock sequence and excitement level
        private List<BuildingShopItem> SortBuildingsByUnlockSequence(List<AssessmentQuizManager.RegionType> unlockOrder)
        {
            // Sort the buildings based on their region unlock sequence and excitement level (excitement level is already defined in each BuildingShopDatabase; least exciting to most exciting). 
            var sortedBuildings = new List<BuildingShopItem>(); // Create a new list to hold the sorted buildings.

            // First, put the names of the buildings for each region into 4 separate lists. (Use Resources.Load to load the building databases; it is the standard way to load ScriptableObjects in Unity.)
            var healthHarborBuildings = Resources.Load<BuildingShopDatabase>("HealthHarborBuildings"); // Load the Health Harbor buildings database (an existing ScriptableObject of type BuildingShopDatabase in Assets/Resources/ folder on Unity Editor), which is HealthHarborBuildings.asset.
            var mindPalaceBuildings = Resources.Load<BuildingShopDatabase>("MindPalaceBuildings"); // Load the Mind Palace buildings database (an existing ScriptableObject of type BuildingShopDatabase in Assets/Resources/ folder on Unity Editor), which is MindPalaceBuildings.asset. 
            var creativeCommonsBuildings = Resources.Load<BuildingShopDatabase>("CreativeCommonsBuildings"); // Load the Creative Commons buildings database (an existing ScriptableObject of type BuildingShopDatabase in Assets/Resources/ folder on Unity Editor), which is CreativeCommonsBuildings.asset. 
            var socialSquareBuildings = Resources.Load<BuildingShopDatabase>("SocialSquareBuildings"); // Load the Social Square buildings database (an existing ScriptableObject of type BuildingShopDatabase in Assets/Resources/ folder on Unity Editor), which is SocialSquareBuildings.asset. 

            foreach (var region in unlockOrder) // For each region in the unlock order (already determined), 
            {
                // Get the buildings for this region:

                if (region == AssessmentQuizManager.RegionType.HealthHarbor && healthHarborBuildings != null) // If the current region in the loop of unlockOrder is Health Harbor AND the Health Harbor buildings database exists, 
                {
                    sortedBuildings.AddRange(healthHarborBuildings.buildings); // Add the Health Harbor buildings to the sorted master list. The HealthHarborBuildings.asset BuildingShopDatabase contains a list called "buildings" of Health Harbor BuildingShopItem objects, which are added to the sorted master list. 

                    // Track which buildings belong to Health Harbor:
                    if (!_regionBuildings.ContainsKey(region)) // If the region is not already in the dictionary, 
                    {
                        _regionBuildings[region] = new List<string>(); // Create a new list for this region. 
                    }

                    foreach (var building in healthHarborBuildings.buildings) // For each building in the Health Harbor buildings database,
                    {
                        _regionBuildings[region].Add(building.name); // The buildings database holds BuildingShopItem objects, which contain a "name" field for the Building Name. Add the building name to the region's list in the dictionary. 
                    }
                }

                if (region == AssessmentQuizManager.RegionType.MindPalace && mindPalaceBuildings != null) // If the current region in the loop of unlockOrder is Mind Palace AND the Mind Palace buildings database exists, 
                {
                    sortedBuildings.AddRange(mindPalaceBuildings.buildings); // Add the Mind Palace buildings to the sorted master list. The MindPalaceBuildings.asset BuildingShopDatabase contains a list called "buildings" of BuildingShopItem objects, which are added to the sorted master list. 

                    // Track which buildings belong to Mind Palace:
                    if (!_regionBuildings.ContainsKey(region)) // If the region is not already in the dictionary, 
                    {
                        _regionBuildings[region] = new List<string>(); // Create a new list for this region.
                    }

                    foreach (var building in mindPalaceBuildings.buildings) // For each building in the Mind Palace buildings database, 
                    {
                        _regionBuildings[region].Add(building.name); // The buildings database holds BuildingShopItem objects, which contain a "name" field for the Building Name. Add the building name to the region's list in the dictionary.
                    }
                }

                if (region == AssessmentQuizManager.RegionType.CreativeCommons && creativeCommonsBuildings != null) // If the current region in the loop of unlockOrder is Creative Commons AND the Creative Commons buildings database exists, 
                {
                    sortedBuildings.AddRange(creativeCommonsBuildings.buildings); // Add the Creative Commons buildings to the sorted master list. The CreativeCommonsBuildings.asset BuildingShopDatabase contains a list called "buildings" of type BuildingShopItem objects, which are added to the sorted master list. 

                    // Track which buildings belong to Creative Commons:
                    if (!_regionBuildings.ContainsKey(region)) // If the region is not already in the dictionary, 
                    {
                        _regionBuildings[region] = new List<string>(); // Create a new list for this region. 
                    }

                    foreach (var building in creativeCommonsBuildings.buildings) // For each building in the Creative Commons buildings database, 
                    {
                        _regionBuildings[region].Add(building.name); // The buildings database holds BuildingShopItem objects, which contain a "name" field for the Building Name. Add the building name to the region's list in the dictionary. 
                    }
                }

                if (region == AssessmentQuizManager.RegionType.SocialSquare && socialSquareBuildings != null) // If the current region in the loop of unlockOrder is Social Square AND the Social Square buildings database exists, 
                {
                    sortedBuildings.AddRange(socialSquareBuildings.buildings); // Add the Social Square buildings to the sorted master list. The SocialSquareBuildings.asset BuildingShopDatabase contains a list called "buildings" of type BuildingShopItem objects, which are added to the sorted master list. 

                    // Track which buildings belong to Social Square:
                    if (!_regionBuildings.ContainsKey(region)) // If the region is not already in the dictionary, 
                    {
                        _regionBuildings[region] = new List<string>(); // Create a new list for this region. 
                    }

                    foreach (var building in socialSquareBuildings.buildings) // For each building in the Social Square buildings database, 
                    {
                        _regionBuildings[region].Add(building.name); // The buildings database holds BuildingShopItem objects, which contain a "name" field for the Building Name. Add the building name to the region's list in the dictionary.
                    }
                }
            }

            Debug.Log($"Sorted buildings by unlock sequence. Total sorted buildings: {sortedBuildings.Count}");
            return sortedBuildings; // Return the sorted list of buildings based on the unlock sequence. 
        }

        // Helper Function (3): Assign unlock levels to buildings (1-40)
        private void AssignUnlockLevelsToBuildings(List<BuildingShopItem> sortedBuildings)
        {
            Debug.Log("=== ASSIGNING UNLOCK LEVELS TO BUILDINGS ===");
            Debug.Log($"Total buildings to assign levels to: {sortedBuildings.Count}");
            
            int totalBuildings = sortedBuildings.Count; // Get the total number of buildings in the sorted list (currently 80). 
            int maxUnlockLevel = 40; // Maximum unlock level for buildings (1-40). 

            for (int i = 0; i < totalBuildings; i++) // For each building in the sorted list, 
            {
                // Formula: More buildings unlock early, fewer later:
                int unlockLevel = Mathf.RoundToInt(1 + (i * maxUnlockLevel) / (float)totalBuildings); // Calculate the unlock level based on the index of the building in the sorted list. 
                                                                                                      // Example: 
                                                                                                      // First building (i = 0): 1 + (0 * 40) / 80 = Level 1
                                                                                                      // Second building (i = 1): 1 + (1 * 40) / 80 = Level 1.5 (rounds to Level 2)
                                                                                                      // Third building (i = 2): 1 + (2 * 40) / 80 = Level 2 

                // Ensure the level is within bounds (1-40):
                unlockLevel = Mathf.Clamp(unlockLevel, 1, maxUnlockLevel); // Clamp the unlock level to be between 1 and 40 (clamp means to restrict a value to a specific range). 

                _buildingUnlockLevels[sortedBuildings[i].name] = unlockLevel; // Assign the calculated unlock level to the building's name in the dictionary (sortedBuildings is of type BuildingShopItem, which contains a "name" field for the Building Name). 

                Debug.Log($"Building '{sortedBuildings[i].name}' (index {i}) unlocks at level {unlockLevel}"); // Log the building name and its unlock level for debugging purposes. 
            }
            
            Debug.Log("=== END ASSIGNING UNLOCK LEVELS ===");
        }

        /// <summary>
        /// Add EXP to the player
        /// </summary>
        public void AddEXP(int expAmount)
        {
            currentEXP += expAmount;
            OnEXPChanged?.Invoke(currentEXP);

            // Check for level up
            CheckForLevelUp();
        }

        /// <summary>
        /// Check if player should level up
        /// </summary>
        private void CheckForLevelUp()
        {
            int expRequiredForNextLevel = GetEXPRequiredForLevel(currentLevel + 1);

            while (currentEXP >= expRequiredForNextLevel)
            {
                currentEXP -= expRequiredForNextLevel;
                currentLevel++;

                OnLevelUp?.Invoke(currentLevel);

                // Check for building unlocks
                CheckForBuildingUnlocks();

                // Check for region unlocks
                CheckForRegionUnlocks();

                expRequiredForNextLevel = GetEXPRequiredForLevel(currentLevel + 1);
            }
        }

        /// <summary>
        /// Calculate EXP required for a specific level (exponential progression)
        /// </summary>
        public int GetEXPRequiredForLevel(int level)
        {
            // Implement exponential EXP calculation:
            // Formula: baseEXPPerLevel * (expMultiplier ^ (level - 1))

            // baseEXPPerLevel (EXP required for Level 1) = 100 EXP required to reach Level 2
            // expMultiplier = 1.5 (50% increase per level) 

            // Example: 
            // Level 1 to Level 2: 100 * (1.5 ^ (1 - 1)) = 100 * 1.5^0 = 100 * 1 = 100
            // Level 2 to Level 3: 100 * (1.5 ^ (2 - 1)) = 100 * 1.5^1 = 150 

            // Level 2 to Level 3 in Code Format: 
            // Mathf.RoundToInt(100 * Mathf.Pow(1.5f, 2 - 1)) = Mathf.RoundToInt(100 * 1.5^1) = 150 
            int expRequiredForNextLevel = Mathf.RoundToInt(baseEXPPerLevel * Mathf.Pow(expMultiplier, level - 1));
            return expRequiredForNextLevel;
        }

        /// <summary>
        /// Check if any buildings should be unlocked at current level
        /// </summary>
        private void CheckForBuildingUnlocks()
        {
            // (1) Loop through all buildings in _BuildingUnlockLevels. 
            // (2) Check if any buildings unlock at the current level. 
            // (3) Trigger OnBuildingUnlocked event for each newly unlocked building. 

            foreach (var building in _buildingUnlockLevels) // _buildingUnlockLevels is a dictionary (or a HashMap) where the Key is the building name (string) and the Value is the unlock level (int). 
            { // For every building in the _buildingUnlockLevels dictionary, 
                if (building.Value == currentLevel) // If the building's unlock level (Value = int = unlock level) matches the current player level,
                {
                    OnBuildingUnlocked?.Invoke(building.Key); // Trigger the OnBuildingUnlocked event with the building name (Key = string = building name). 
                    Debug.Log($"Building '{building.Key}' unlocked at level {currentLevel}"); // Log the building name and current level for debugging purposes. 
                }
            }
        }

        /// <summary>
        /// Check if any regions should be unlocked at current level
        /// </summary>
        private void CheckForRegionUnlocks()
        {
            Debug.Log($"=== CHECK FOR REGION UNLOCKS ===");
            Debug.Log($"Current player level: {currentLevel}");
            
            // (1) Get the player's region unlock sequence from RegionUnlockSystem instance.
            // (2) Check each region to see if its first building unlocks at current level. 
            // (3) If yes, unlock the region and trigger OnRegionUnlocked event. 

            var regionUnlockSystem = RegionUnlockSystem.Instance; // Get the instance of RegionUnlockSystem to get the correct unlock sequence. 

            if (regionUnlockSystem == null)
            {
                Debug.LogError("RegionUnlockSystem.Instance is null in CheckForRegionUnlocks!");
                return; // Exit if the region unlock system is not initialized. 
            }

            foreach (var region in regionUnlockSystem.GetUnlockOrder()) // For each region in the player's region unlock order,
            {
                // Skip if region is already unlocked:
                if (regionUnlockSystem.IsRegionUnlocked(region)) continue; // If the region is already unlocked, skip to the next region in the loop. 

                // Otherwise, get the first building for this region (at index 0):
                if (_regionBuildings.ContainsKey(region) && _regionBuildings[region].Count > 0) // If the region exists in the dictionary and has at least one building, 
                {
                    string firstBuilding = _regionBuildings[region][0]; // Get the first building name for this region at index 0. 

                    // Check if this first building unlocks at the current player level:
                    if (_buildingUnlockLevels.ContainsKey(firstBuilding) && _buildingUnlockLevels[firstBuilding] <= currentLevel) // If the first building exists in the _buildingUnlockLevels dictionary and its unlock level matches the current player level, 
                    {
                        // Unlock the region:
                        regionUnlockSystem.UnlockRegionByLevel(region); // Call the UnlockRegionByLevel method from the RegionUnlockSystem to unlock the region if the player has reached the required level. 
                        OnRegionUnlocked?.Invoke(region); // Trigger the OnRegionUnlocked event with the region type in order to notify other systems that the region has been unlocked. 
                        Debug.Log($"Region {region} unlocked at level {currentLevel}!"); // Log the region name and current level for debugging purposes. 
                    }
                }
            }

            // Save the game to persist the unlock state
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SaveGame();
            }
            
            Debug.Log($"=== END CHECK FOR REGION UNLOCKS ===");
        }

        /// <summary>
        /// Get the unlock level for a specific building
        /// </summary>
        public int GetBuildingUnlockLevel(string buildingName)
        {
            if (_buildingUnlockLevels.ContainsKey(buildingName))
            {
                return _buildingUnlockLevels[buildingName];
            }
            return -1; // Building not found
        }

        /// <summary>
        /// Get the construction time for a specific building (in minutes)
        /// </summary>
        public float GetBuildingConstructionTime(string buildingName)
        {
            Debug.Log($"GetBuildingConstructionTime called for: {buildingName}");
            
            if (_buildingConstructionTimes.ContainsKey(buildingName))
            {
                float time = _buildingConstructionTimes[buildingName];
                Debug.Log($"Found construction time for '{buildingName}': {time} minutes");
                return time;
            }
            
            // Fallback: calculate on-the-fly if not found in dictionary
            Debug.LogWarning($"Construction time not found for '{buildingName}' - calculating on-the-fly");
            int unlockLevel = GetBuildingUnlockLevel(buildingName);
            if (unlockLevel > 0)
            {
                // Use the same calculation as before for fallback
                float minTimeMinutes = 1f;
                float maxTimeMinutes = 360f;
                float progress = Mathf.Clamp01((float)(unlockLevel - 1) / 39f); // Assume levels 1-40
                float constructionTimeMinutes = minTimeMinutes + (progress * (maxTimeMinutes - minTimeMinutes));
                Debug.Log($"Fallback calculation for '{buildingName}' (Level {unlockLevel}): {constructionTimeMinutes} minutes");
                return constructionTimeMinutes;
            }
            
            Debug.LogWarning($"No unlock level found for '{buildingName}', returning default 60 minutes");
            return 60f; // Default 1 hour if building not found
        }

        /// <summary>
        /// Check if a building is unlocked at current level
        /// </summary>
        public bool IsBuildingUnlocked(string buildingName)
        {
            int unlockLevel = GetBuildingUnlockLevel(buildingName);
            return unlockLevel > 0 && currentLevel >= unlockLevel;
        }

        /// <summary>
        /// Get current level
        /// </summary>
        public int GetCurrentLevel()
        {
            return currentLevel;
        }

        /// <summary>
        /// Get current EXP
        /// </summary>
        public int GetCurrentEXP()
        {
            return currentEXP;
        }

        /// <summary>
        /// Get EXP required for next level
        /// </summary>
        public int GetEXPRequiredForNextLevel()
        {
            return GetEXPRequiredForLevel(currentLevel + 1);
        }

        /// <summary>
        /// Get all unlocked buildings at current level
        /// </summary>
        public List<string> GetUnlockedBuildings()
        {
            List<string> unlockedBuildings = new List<string>();

            foreach (var building in _buildingUnlockLevels)
            {
                if (currentLevel >= building.Value)
                {
                    unlockedBuildings.Add(building.Key);
                }
            }

            return unlockedBuildings;
        }

        public List<string> GetRegionBuildings(AssessmentQuizManager.RegionType region)
        {
            if (_regionBuildings.TryGetValue(region, out var buildings)) // Try to get the list of buildings from the _regionBuildings Dictionary for the specified region. 
            {
                return new List<string>(buildings); // Return a new list with the building names of that specified region. 
            }

            return null; // Otherwise, return nothing. 
        }

        public BuildingUnlockSaveData GetSaveData()
        {
            return new BuildingUnlockSaveData
            {
                buildingUnlockLevels = _buildingUnlockLevels != null
                    ? new Dictionary<string, int>(_buildingUnlockLevels)
                    : new Dictionary<string, int>(), // Create a copy of the building unlock levels dictionary. 
                regionBuildings = _regionBuildings != null
                    ? new Dictionary<AssessmentQuizManager.RegionType, List<string>>(_regionBuildings)
                    : new Dictionary<AssessmentQuizManager.RegionType, List<string>>(), // Create a copy of the region buildings dictionary. 
                buildingConstructionTimes = _buildingConstructionTimes != null
                    ? new Dictionary<string, float>(_buildingConstructionTimes)
                    : new Dictionary<string, float>(), // Create a copy of the building construction times dictionary.
                currentLevel = currentLevel, // Save the current player level. 
                currentEXP = currentEXP // Save the current player EXP. 
            };
        }

        public void LoadSaveData(BuildingUnlockSaveData saveData)
        {
            if (saveData == null) return;

            // Safely load building unlock levels - handle null dictionaries
            _buildingUnlockLevels = saveData.buildingUnlockLevels != null 
                ? new Dictionary<string, int>(saveData.buildingUnlockLevels) 
                : new Dictionary<string, int>();

            // Safely load region buildings - handle null dictionaries
            _regionBuildings = saveData.regionBuildings != null 
                ? new Dictionary<AssessmentQuizManager.RegionType, List<string>>(saveData.regionBuildings) 
                : new Dictionary<AssessmentQuizManager.RegionType, List<string>>();

            // Safely load building construction times - handle null dictionaries
            _buildingConstructionTimes = saveData.buildingConstructionTimes != null 
                ? new Dictionary<string, float>(saveData.buildingConstructionTimes) 
                : new Dictionary<string, float>();

            currentLevel = saveData.currentLevel; // Re-load the saved current player level. 
            currentEXP = saveData.currentEXP; // Re-load the saved current EXP. 
        }

        /// <summary>
        /// Reset all building unlocks and player level (for testing)
        /// </summary>
        [ContextMenu("Reset All Building Unlocks and Player Level")]
        public void ResetAllBuildingUnlocks()
        {
            Debug.Log("=== RESETTING ALL BUILDING UNLOCKS AND PLAYER LEVEL ===");

            // Clear building unlock data
            _buildingUnlockLevels.Clear();
            _regionBuildings.Clear();
            _buildingConstructionTimes.Clear();

            // Reset player level and EXP
            currentLevel = 1;
            currentEXP = 0;

            // Clear saved data from PlayerPrefs
            PlayerPrefs.DeleteKey("BuildingUnlockData");
            PlayerPrefs.Save();

            Debug.Log("All building unlocks and player level reset to default");
            Debug.Log("=== END RESET ===");
        }

        /// <summary>
        /// Reset player level and EXP only (for testing)
        /// </summary>
        [ContextMenu("Reset Player Level")]
        public void ResetPlayerLevel()
        {
            Debug.Log("=== RESETTING PLAYER LEVEL ===");

            // Reset player level and EXP
            currentLevel = 1;
            currentEXP = 0;

            // Clear saved data from PlayerPrefs
            PlayerPrefs.DeleteKey("BuildingUnlockData");
            PlayerPrefs.Save();

            Debug.Log("Player level reset to 1, EXP reset to 0");
            Debug.Log("=== END RESET ===");
        }

        /// <summary>
        /// Reset building unlock data only (for testing)
        /// </summary>
        [ContextMenu("Reset Building Unlock Data")]
        public void ResetBuildingUnlockData()
        {
            Debug.Log("=== RESETTING BUILDING UNLOCK DATA ===");

            // Clear building unlock data
            _buildingUnlockLevels.Clear();
            _regionBuildings.Clear();
            _buildingConstructionTimes.Clear();

            // Clear saved data from PlayerPrefs
            PlayerPrefs.DeleteKey("BuildingUnlockData");
            PlayerPrefs.Save();

            Debug.Log("Building unlock data cleared");
            Debug.Log("=== END RESET ===");
        }
        
        /// <summary>
        /// Reset everything: regions, buildings, and player level (for testing)
        /// </summary>
        [ContextMenu("Reset Everything")]
        public void ResetEverything()
        {
            Debug.Log("=== RESETTING EVERYTHING ===");
            
            // Reset building unlocks and player level
            ResetAllBuildingUnlocks();
            
            // Reset regions (if GameManager is available)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetAllRegionsToLocked();
            }
            
            Debug.Log("Everything reset to default state");
            Debug.Log("=== END RESET ===");
        }
    }
} 