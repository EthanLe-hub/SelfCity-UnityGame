using LifeCraft.Core;
using LifeCraft.Systems;
using UnityEngine;
using UnityEngine.SceneManagement;
using LifeCraft.UI;
using System.Collections.Generic;
using System.Linq; // Added for .Select()

namespace LifeCraft.Core
{
    /// <summary>
    /// Main game manager that coordinates all game systems and handles overall game flow.
    /// This is the central controller for the entire game.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Game Systems")]
        [SerializeField] private CityBuilder cityBuilder;
        [SerializeField] private QuestManager questManager;
        [SerializeField] private AssessmentQuizManager assessmentQuizManager;
        [SerializeField] private RegionUnlockSystem regionUnlockSystem;
    //    [SerializeField] private SelfCareManager selfCareManager;
    //    [SerializeField] private WeatherSystem weatherSystem;

        [Header("UI")]
        [SerializeField] private UIManager uiManager;
        [SerializeField] private AssessmentQuizUI assessmentQuizUI;

        [Header("Game State")]
        [SerializeField] private GameState currentGameState = GameState.MainMenu;
        [SerializeField] private bool isGamePaused = false;
        [SerializeField] private bool hasCompletedAssessment = false;

        // Events
        public System.Action<GameState> OnGameStateChanged;
        public System.Action<bool> OnGamePaused;

        // Singleton instance
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<GameManager>();
                    // Removed runtime GameObject creation to avoid duplicates.
                }
                return _instance;
            }
        }

        private void Awake()
        {
            Debug.Log($"GameManager Awake called. Instance: {(_instance == null ? "null" : "exists")}");
            // Singleton pattern
            if (_instance == null)
            {
                Debug.Log("Setting this as the GameManager instance");
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
                // --- Load all game data (city, inventory, etc.) ---
                LoadGameData();
                // --- Update UI after loading data ---
                if (UIManager.Instance != null)
                    UIManager.Instance.InitializeUI();
            }
            else if (_instance != this)
            {
                Debug.Log("Destroying duplicate GameManager");
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Start the game
            StartGame();
        }

        private void Update()
        {
            UpdateGameSystems();
        }

        private void OnApplicationQuit()
        {
            SaveGame();
        }

        private void OnDestroy()
        {
            // Clean up event subscriptions
            if (assessmentQuizUI != null)
            {
                assessmentQuizUI.OnRegionSelected -= OnRegionSelected;
            }
            
            if (regionUnlockSystem != null)
            {
                regionUnlockSystem.OnRegionUnlocked -= OnRegionUnlocked;
            }
        }

        /// <summary>
        /// Initialize all game systems
        /// </summary>
        private void InitializeGame()
        {
            Debug.Log("=== INITIALIZING GAMEMANAGER ===");
            Debug.Log("Initializing GameManager...");
            
            // Find or create other systems
            if (cityBuilder == null)
                cityBuilder = FindFirstObjectByType<CityBuilder>();
            
            if (questManager == null)
                questManager = FindFirstObjectByType<QuestManager>();
            
            // Note: AssessmentQuizManager and RegionUnlockSystem are ScriptableObjects
            // They should be assigned in the Inspector, not found at runtime
            if (assessmentQuizManager == null)
            {
                Debug.LogWarning("AssessmentQuizManager not assigned in Inspector - attempting to load from Resources...");
                assessmentQuizManager = Resources.Load<AssessmentQuizManager>("MyAssessmentQuizManager");
                if (assessmentQuizManager == null)
                {
                    Debug.LogError("AssessmentQuizManager not found in Resources folder!");
                }
                else
                {
                    Debug.Log("AssessmentQuizManager loaded from Resources successfully");
                }
            }
            else
            {
                Debug.Log("AssessmentQuizManager found and assigned");
            }
            
            if (regionUnlockSystem == null)
            {
                Debug.LogWarning("RegionUnlockSystem not assigned in Inspector - attempting to load from Resources...");
                regionUnlockSystem = Resources.Load<RegionUnlockSystem>("RegionUnlockSystem");
                if (regionUnlockSystem == null)
                {
                    Debug.LogError("RegionUnlockSystem not found in Resources folder!");
                }
                else
                {
                    Debug.Log("RegionUnlockSystem loaded from Resources successfully");
                }
            }
            else
            {
                Debug.Log("RegionUnlockSystem found and assigned");
            }
            
            //if (selfCareManager == null)
                //selfCareManager = FindFirstObjectByType<SelfCareManager>();
            
            //if (weatherSystem == null)
                //weatherSystem = FindFirstObjectByType<WeatherSystem>();

            // Find UI manager
            if (uiManager == null)
                uiManager = FindFirstObjectByType<UIManager>();
            
            // CRITICAL: Ensure only ONE AssessmentQuizUI instance exists in the scene!
            // Multiple instances will cause event subscription issues where GameManager
            // subscribes to one instance but buttons call events on a different instance.
            // This leads to "0 subscribers" errors and region unlocking failures.
            if (assessmentQuizUI == null)
            {
                Debug.Log("AssessmentQuizUI is null, trying to find it...");
                assessmentQuizUI = FindFirstObjectByType<AssessmentQuizUI>();
                Debug.Log($"FindFirstObjectByType result: {(assessmentQuizUI != null ? "Found" : "Not found")}");
            }
            else
            {
                Debug.Log("AssessmentQuizUI already assigned");
            }
            
            if (assessmentQuizUI != null)
            {
                Debug.Log("Found AssessmentQuizUI, subscribing to OnRegionSelected event...");
                Debug.Log($"GameManager subscribing to AssessmentQuizUI instance: {assessmentQuizUI.GetInstanceID()}");
                assessmentQuizUI.OnRegionSelected += OnRegionSelected; // Assign the OnRegionSelected event handler to the OnRegionSelected event of the AssessmentQuizUI. 
                Debug.Log("AssessmentQuizUI found and event subscribed successfully"); 
            }
            else
            {
                Debug.LogError("AssessmentQuizUI not found!");
            }
            
            // Note: Region unlock event subscription will be done after region selection
            // to ensure the RegionUnlockSystem is properly initialized
                
            Debug.Log("GameManager initialization completed");
        }

        /// <summary>
        /// Start the game
        /// </summary>
        private void StartGame()
        {
            // Check if this is a new player who needs to take the assessment
            if (!hasCompletedAssessment)
            {
                ShowAssessmentQuiz();
            }
            else
            {
                SetGameState(GameState.Playing);
                
                // Initialize systems
                if (cityBuilder != null)
                    cityBuilder.enabled = true;
                
                if (questManager != null)
                    questManager.Initialize();
                
                //if (selfCareManager != null)
                    //selfCareManager.Initialize();

                Debug.Log("Game started successfully!");
            }
        }

        /// <summary>
        /// Update all game systems
        /// </summary>
        private void UpdateGameSystems()
        {
            if (isGamePaused) return;

            // Update weather
            //if (weatherSystem != null)
                //weatherSystem.UpdateWeather();

            // Update quests
            if (questManager != null)
                questManager.UpdateQuests();

            // Update self-care
            //if (selfCareManager != null)
                //selfCareManager.UpdateSelfCare();
        }

        /// <summary>
        /// Set the current game state
        /// </summary>
        public void SetGameState(GameState newState)
        {
            if (currentGameState != newState)
            {
                GameState previousState = currentGameState;
                currentGameState = newState;

                OnGameStateChanged?.Invoke(newState);

                Debug.Log($"Game state changed from {previousState} to {newState}");
            }
        }

        /// <summary>
        /// Toggle pause state
        /// </summary>
        public void TogglePause()
        {
            SetPaused(!isGamePaused);
        }

        /// <summary>
        /// Set pause state
        /// </summary>
        public void SetPaused(bool paused)
        {
            isGamePaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            
            OnGamePaused?.Invoke(paused);

            if (uiManager != null)
                uiManager.SetUIInteractable(!paused);

            Debug.Log($"Game {(paused ? "paused" : "resumed")}");
        }

        /// <summary>
        /// Save the current game state to local device storage (PlayerPrefs)
        /// 
        /// UPDATE: Implemented comprehensive local save system that preserves all game data:
        /// - Resource amounts and types
        /// - City layout and building states
        /// - Unlocked items and progression
        /// - Quest progress and custom quests
        /// - Current game state and pause status
        /// - Save timestamp for tracking
        /// 
        /// This ensures complete data persistence between game sessions.
        /// </summary>
        public void SaveGame()
        {
            try
            {
                // Save resource data (player's currency and materials)
                if (ResourceManager.Instance != null) ResourceManager.Instance.SaveResources();

                // Save city data (building positions, health, construction status)
                if (cityBuilder != null)
                {
                    var cityData = cityBuilder.GetSaveData();
                    Debug.Log($"Saving {cityData.Count} buildings");
                    string cityJson = JsonUtility.ToJson(new CitySaveWrapper { buildings = cityData });
                    PlayerPrefs.SetString("CityData", cityJson);
                    Debug.Log($"City data saved: {cityJson}");
                }
                else
                {
                    Debug.LogWarning("CityBuilder is null during save");
                }

                // Save unlock data (progressed items and features)
                if (regionUnlockSystem != null)
                {
                    var unlockData = regionUnlockSystem.GetSaveData();
                    string unlockJson = JsonUtility.ToJson(unlockData);
                    PlayerPrefs.SetString("UnlockData", unlockJson);
                }

                // Save region unlock data
                if (regionUnlockSystem != null)
                {
                    var regionUnlockData = regionUnlockSystem.GetSaveData();
                    string regionUnlockJson = JsonUtility.ToJson(regionUnlockData);
                    PlayerPrefs.SetString("RegionUnlockData", regionUnlockJson);
                }

                // Save assessment completion status
                PlayerPrefs.SetInt("HasCompletedAssessment", hasCompletedAssessment ? 1 : 0);

                // CRITICAL FIX: Save quiz scores and selected region for building unlock system re-initialization
                // PROBLEM: When the game loads from a saved state, the building unlock system needs to be re-initialized
                // with the original quiz scores and region selection to restore the correct building unlock levels.
                // Without this data, the buildings won't appear in the shop even though the save data exists.
                if (assessmentQuizManager != null)
                {
                    var quizScores = assessmentQuizManager.GetRegionScores();
                    if (quizScores != null && quizScores.Count > 0)
                    {
                        // Convert Dictionary to serializable list (Unity JsonUtility doesn't serialize dictionaries directly)
                        var quizScoresWrapper = new QuizScoresWrapper();
                        foreach (var kvp in quizScores)
                        {
                            quizScoresWrapper.scores.Add(new QuizScoreEntry 
                            { 
                                regionType = kvp.Key,  // Region type (HealthHarbor, MindPalace, etc.)
                                score = kvp.Value      // Quiz score for that region
                            });
                        }
                        
                        // Save quiz scores as JSON string
                        string quizScoresJson = JsonUtility.ToJson(quizScoresWrapper);
                        PlayerPrefs.SetString("QuizScores", quizScoresJson);
                        
                        // Save the selected starting region
                        PlayerPrefs.SetInt("SelectedRegion", (int)regionUnlockSystem.GetStartingRegion());
                        
                        Debug.Log("Quiz scores and selected region saved for building unlock system re-initialization");
                    }
                }

                // Save quest data (daily quests, custom quests, progress)
                if (questManager != null)
                    questManager.SaveQuests();

                // Save To-Do List data (active quests)
                var toDoListManager = FindFirstObjectByType<ToDoListManager>();
                if (toDoListManager != null)
                    toDoListManager.SaveToDoList();

                // Save construction data (construction projects and quest states)
                if (ConstructionManager.Instance != null)
                    ConstructionManager.Instance.SaveConstructionData();

                // Save current game state and pause status
                PlayerPrefs.SetInt("GameState", (int)currentGameState);
                PlayerPrefs.SetInt("IsPaused", isGamePaused ? 1 : 0);

                // Save timestamp for tracking when the game was last saved
                PlayerPrefs.SetString("LastSaveTime", System.DateTime.UtcNow.ToString("O"));

                PlayerPrefs.Save(); // Force write to device storage
                Debug.Log("Game saved successfully to local storage!");

                if (uiManager != null)
                    uiManager.ShowNotification("Game saved!");

                // Save building unlock data
                if (PlayerLevelManager.Instance != null)
                {
                    var buildingUnlockData = PlayerLevelManager.Instance.GetSaveData(); // Save the current variables from the PlayerLevelManager Instance. 
                    string buildingUnlockJson = JsonUtility.ToJson(buildingUnlockData); // Using JSON for saving. 
                    PlayerPrefs.SetString("BuildingUnlockData", buildingUnlockJson); // Save the JSON to PlayerPrefs. 
                    Debug.Log("Building unlock data saved");
                }

                // Save mood and weather system data
                if (MoodManager.Instance != null)
                {
                    MoodManager.Instance.SaveMoodData();
                    Debug.Log("Mood data saved");
                }
                else
                {
                    Debug.LogWarning("MoodManager.Instance is null - skipping mood save");
                }

                // Save weather system data
                var weatherSystem = FindFirstObjectByType<WeatherSystem>();
                if (weatherSystem != null)
                {
                    weatherSystem.SaveWeatherData();
                    Debug.Log("Weather data saved");
                }
                else
                {
                    Debug.LogWarning("WeatherSystem not found - skipping weather save");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save game: {e.Message}");

                if (uiManager != null)
                    uiManager.ShowNotification("Failed to save game!");
            }
        }

        /// <summary>
        /// Load a saved game from local device storage (PlayerPrefs)
        /// 
        /// UPDATE: Implemented comprehensive local load system that restores all game data:
        /// - Resource amounts and types
        /// - City layout and building states  
        /// - Unlocked items and progression
        /// - Quest progress and custom quests
        /// - Current game state and pause status
        /// 
        /// This ensures players can continue exactly where they left off.
        /// </summary>
        public void LoadGame()
        {
            try
            {
                Debug.Log("Starting game load process...");

                // Load resource data (player's currency and materials)
                if (ResourceManager.Instance != null)
                {
                    ResourceManager.Instance.LoadResources();
                    Debug.Log("Resources loaded successfully");
                }
                else
                {
                    Debug.LogWarning("ResourceManager.Instance is null - skipping resource load");
                }

                // --- Ensure the resource bar UI is updated after loading resources ---
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.InitializeUI(); // This will refresh the resource bar and all UI elements
                    Debug.Log("UI initialized successfully");
                }
                else
                {
                    Debug.LogWarning("UIManager.Instance is null - skipping UI initialization");
                }

                // Load city data (building positions, health, construction status)
                if (cityBuilder != null && PlayerPrefs.HasKey("CityData"))
                {
                    string cityJson = PlayerPrefs.GetString("CityData");
                    Debug.Log($"Loading city data: {cityJson}");
                    var cityWrapper = JsonUtility.FromJson<CitySaveWrapper>(cityJson);
                    if (cityWrapper != null && cityWrapper.buildings != null)
                    {
                        Debug.Log($"Loading {cityWrapper.buildings.Count} buildings");
                        cityBuilder.LoadCity(cityWrapper.buildings);
                    }
                    else
                    {
                        Debug.LogError("Failed to parse city data or buildings list is null");
                    }
                }
                else
                {
                    Debug.Log("No city data to load or cityBuilder is null");
                }

                // Load unlock data
                if (regionUnlockSystem != null && PlayerPrefs.HasKey("UnlockData"))
                {
                    try
                    {
                        // Ensure RegionUnlockSystem is initialized before loading
                        if (regionUnlockSystem.GetUnlockedRegions().Count == 0 && !PlayerPrefs.HasKey("RegionUnlockData"))
                        {
                            Debug.Log("RegionUnlockSystem not initialized, skipping unlock data load");
                        }
                        else
                        {
                            string unlockJson = PlayerPrefs.GetString("UnlockData");
                            var unlockData = JsonUtility.FromJson<LifeCraft.Systems.RegionUnlockSaveData>(unlockJson);
                            if (unlockData != null)
                            {
                                regionUnlockSystem.LoadSaveData(unlockData);
                                Debug.Log("Unlock data loaded successfully");
                            }
                            else
                            {
                                Debug.LogWarning("Failed to parse unlock data JSON");
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Failed to load unlock data: {e.Message}");
                    }
                }
                else
                {
                    Debug.Log("No unlock data to load or regionUnlockSystem is null");
                }

                // Load region unlock data (this is the main region unlock system data)
                if (regionUnlockSystem != null && PlayerPrefs.HasKey("RegionUnlockData"))
                {
                    try
                    {
                        string regionUnlockJson = PlayerPrefs.GetString("RegionUnlockData");
                        var regionUnlockData = JsonUtility.FromJson<LifeCraft.Systems.RegionUnlockSaveData>(regionUnlockJson);
                        if (regionUnlockData != null)
                        {
                            regionUnlockSystem.LoadSaveData(regionUnlockData);
                            Debug.Log("Region unlock data loaded successfully");
                        }
                        else
                        {
                            Debug.LogWarning("Failed to parse region unlock data JSON");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Failed to load region unlock data: {e.Message}");
                    }
                }
                else
                {
                    Debug.Log("No region unlock data to load or regionUnlockSystem is null");
                }

                // Load assessment completion status
                hasCompletedAssessment = PlayerPrefs.GetInt("HasCompletedAssessment", 0) == 1;
                Debug.Log($"Assessment completion status: {hasCompletedAssessment}");

                // Load quest data (daily quests, custom quests, progress)
                if (questManager != null)
                {
                    questManager.LoadQuests();
                    Debug.Log("Quest data loaded successfully");
                }
                else
                {
                    Debug.LogWarning("QuestManager is null - skipping quest load");
                }

                // Load To-Do List data (active quests)
                var toDoListManager = FindFirstObjectByType<ToDoListManager>();
                if (toDoListManager != null)
                {
                    toDoListManager.LoadToDoList();
                    Debug.Log("To-Do List data loaded successfully");
                }
                else
                {
                    Debug.LogWarning("ToDoListManager is null - skipping To-Do List load");
                }

                // Load construction data (construction projects and quest states)
                if (ConstructionManager.Instance != null)
                {
                    ConstructionManager.Instance.LoadConstructionData();
                    Debug.Log("Construction data loaded successfully");
                }
                else
                {
                    Debug.LogWarning("ConstructionManager is null - skipping construction load");
                }

                // Load game state and pause status
                if (PlayerPrefs.HasKey("GameState"))
                {
                    GameState savedState = (GameState)PlayerPrefs.GetInt("GameState");
                    SetGameState(savedState);
                    Debug.Log($"Game state loaded: {savedState}");
                }

                if (PlayerPrefs.HasKey("IsPaused"))
                {
                    bool savedPause = PlayerPrefs.GetInt("IsPaused") == 1;
                    SetPaused(savedPause);
                    Debug.Log($"Pause status loaded: {savedPause}");
                }

                Debug.Log("Game loaded successfully from local storage!");

                if (uiManager != null)
                    uiManager.ShowNotification("Game loaded!");

                // Load building unlock data
                if (PlayerLevelManager.Instance != null && PlayerPrefs.HasKey("BuildingUnlockData")) // If PlayerPrefs has saved data for BuildingUnlockData, 
                {
                    try
                    {
                        string buildingUnlockJson = PlayerPrefs.GetString("BuildingUnlockData"); // Get the JSON from PlayerPrefs. 
                        Debug.Log($"Building unlock JSON: {buildingUnlockJson}");
                        
                        var buildingUnlockData = JsonUtility.FromJson<BuildingUnlockSaveData>(buildingUnlockJson); // Retrieve the building unlock data from the JSON. 
                        if (buildingUnlockData != null)
                        {
                            Debug.Log($"Building unlock data deserialized successfully. Building count: {buildingUnlockData.buildingUnlockLevels?.Count ?? 0}, Region count: {buildingUnlockData.regionBuildings?.Count ?? 0}");
                            
                            PlayerLevelManager.Instance.LoadSaveData(buildingUnlockData); // If no building unlock data exists in the JSON, get it from the PlayerLevelManager instance. 
                            Debug.Log("Building unlock data loaded successfully");

                            RefreshUIAfterRegionUnlock(); // Refresh Shop UI after loading building unlock data. 

                            // Delayed refresh to ensure UI is ready:
                            StartCoroutine(RefreshUIAfterLoadDelay()); 
                        }
                        else
                        {
                            Debug.LogWarning("Building unlock data deserialized to null");
                        }
                    }
                    catch (System.Exception e) // Catch the error if we cannot load building unlock data.
                    {
                        Debug.LogWarning($"Failed to load building unlock data: {e.Message}"); 
                        Debug.LogWarning($"Stack trace: {e.StackTrace}");
                    }
                }

                // Load mood and weather system data
                if (MoodManager.Instance != null)
                {
                    MoodManager.Instance.LoadMoodData();
                    Debug.Log("Mood data loaded successfully");
                }
                else
                {
                    Debug.LogWarning("MoodManager.Instance is null - skipping mood load");
                }

                // Load weather system data
                var weatherSystem = FindFirstObjectByType<WeatherSystem>();
                if (weatherSystem != null)
                {
                    weatherSystem.LoadWeatherData();
                    weatherSystem.ApplyLoadedWeatherState();
                    Debug.Log("Weather data loaded and applied successfully");
                }
                else
                {
                    Debug.LogWarning("WeatherSystem not found - skipping weather load");
                }

                // CRITICAL FIX: Re-initialize building unlock system with saved quiz scores and selected region
                // LOGIC FLOW EXPLANATION:
                // 1. During first play: Assessment Quiz → OnRegionSelected → InitializeBuildingUnlockSystem → Buildings get unlock levels assigned
                // 2. During game load: Data loads successfully, but InitializeBuildingUnlockSystem was never called again
                // 3. RESULT: PlayerLevelManager had saved data, but building unlock levels were never re-assigned
                // 4. SOLUTION: Re-initialize the building unlock system using saved quiz scores and region selection
                if (hasCompletedAssessment && PlayerPrefs.HasKey("QuizScores") && PlayerPrefs.HasKey("SelectedRegion"))
                {
                    try
                    {
                        // STEP 1: Load the saved quiz scores from PlayerPrefs
                        string quizScoresJson = PlayerPrefs.GetString("QuizScores");
                        var quizScoresWrapper = JsonUtility.FromJson<QuizScoresWrapper>(quizScoresJson);
                        
                        if (quizScoresWrapper != null && quizScoresWrapper.scores != null)
                        {
                            // STEP 2: Convert the serialized list back to a dictionary for easier use
                            var quizScores = new Dictionary<AssessmentQuizManager.RegionType, int>();
                            foreach (var entry in quizScoresWrapper.scores)
                            {
                                quizScores[entry.regionType] = entry.score; // Rebuild the original quiz scores dictionary
                            }

                            // STEP 3: Load the saved selected region
                            var selectedRegion = (AssessmentQuizManager.RegionType)PlayerPrefs.GetInt("SelectedRegion");
                            
                            Debug.Log($"Re-initializing building unlock system with region: {AssessmentQuizManager.GetRegionDisplayName(selectedRegion)}");
                            Debug.Log($"Quiz scores: {string.Join(", ", quizScores.Select(kvp => $"{AssessmentQuizManager.GetRegionDisplayName(kvp.Key)}: {kvp.Value}"))}");

                            // STEP 4: Re-initialize the RegionUnlockSystem with the original data
                            // This sets up the correct region unlock order based on quiz scores
                            if (regionUnlockSystem != null)
                            {
                                // Do NOT call SetStartingRegion here - it resets the unlock states!
                                // Instad, just set the unlock order without changing unlock states. 
                                regionUnlockSystem.SetUnlockOrderOnly(selectedRegion, quizScores);
                            }
                            
                            // STEP 5: Re-initialize the PlayerLevelManager building unlock system
                            // This re-assigns unlock levels to all buildings based on the original quiz scores and region selection
                            if (PlayerLevelManager.Instance != null)
                            {
                                PlayerLevelManager.Instance.InitializeBuildingUnlockSystem(selectedRegion, quizScores);
                            }

                            Debug.Log("Building unlock system re-initialized successfully - buildings should now appear in shop!");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Failed to re-initialize building unlock system: {e.Message}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load game: {e.Message}");
                Debug.LogError($"Stack trace: {e.StackTrace}");

                // Clear corrupted save data and start fresh
                Debug.Log("Clearing corrupted save data and starting fresh...");
                ClearSaveData();

                if (uiManager != null)
                    uiManager.ShowNotification("Failed to load game! Starting fresh.");
            }
        }

        /// <summary>
        /// Load game data on initialization (called during game startup)
        /// 
        /// UPDATE: Implemented automatic save detection and loading.
        /// Checks for existing save data and automatically loads it if found.
        /// This ensures seamless continuation of player progress between app launches.
        /// </summary>
        private void LoadGameData()
        {
            // Check if we have saved data from a previous session
            if (PlayerPrefs.HasKey("LastSaveTime"))
            {
                string lastSaveTime = PlayerPrefs.GetString("LastSaveTime");
                Debug.Log($"Found saved game from: {lastSaveTime}");
                
                // Check if we have city data
                if (PlayerPrefs.HasKey("CityData"))
                {
                    string cityJson = PlayerPrefs.GetString("CityData");
                    Debug.Log($"Found city data: {cityJson}");
                }
                else
                {
                    Debug.Log("No city data found in save");
                }
                
                // Auto-load the game on startup to restore player progress
                LoadGame();
            }
            else
            {
                Debug.Log("No saved game found, starting fresh...");
            }
        }

        /// <summary>
        /// Clear all save data (useful for debugging or resetting the game)
        /// </summary>
        public void ClearSaveData()
        {
            PlayerPrefs.DeleteKey("CityData");
            PlayerPrefs.DeleteKey("UnlockData");
            PlayerPrefs.DeleteKey("RegionUnlockData");
            PlayerPrefs.DeleteKey("HasCompletedAssessment");
            PlayerPrefs.DeleteKey("GameState");
            PlayerPrefs.DeleteKey("IsPaused");
            PlayerPrefs.DeleteKey("LastSaveTime");
            PlayerPrefs.DeleteKey("BuildingUnlockData"); // Clear the data from the PlayerPrefs BuildingUnlockData key. 
            PlayerPrefs.DeleteKey("QuizScores"); // Clear quiz scores (part of the building unlock system fix)
            PlayerPrefs.DeleteKey("SelectedRegion"); // Clear selected region (part of the building unlock system fix)
            
            // Clear mood and weather system data
            PlayerPrefs.DeleteKey("CurrentMood");
            PlayerPrefs.DeleteKey("LastMoodSelectionTime");
            PlayerPrefs.DeleteKey("CurrentWeatherMood");
            PlayerPrefs.DeleteKey("WeatherTransitionDuration");
            
            // Note: ResourceManager and QuestManager don't have ClearSaveData methods
            // Their data will be reset when the game restarts

            PlayerPrefs.Save();
            Debug.Log("All save data cleared");
        }

        /// <summary>
        /// Quit the game
        /// </summary>
        public void QuitGame()
        {
            SaveGame();
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        /// <summary>
        /// Restart the game
        /// </summary>
        public void RestartGame()
        {
            SaveGame();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// Get current game state
        /// </summary>
        public GameState CurrentGameState => currentGameState;

        /// <summary>
        /// Check if game is paused
        /// </summary>
        public bool IsGamePaused => isGamePaused;

        /// <summary>
        /// Get city builder
        /// </summary>
        public CityBuilder CityBuilder => cityBuilder;

        /// <summary>
        /// Get unlock system
        /// </summary>
        public RegionUnlockSystem RegionUnlockSystem => regionUnlockSystem;

        /// <summary>
        /// Get quest manager
        /// </summary>
        public QuestManager QuestManager => questManager;

        /// <summary>
        /// Get self-care manager
        /// </summary>
        //public SelfCareManager SelfCareManager => selfCareManager;

        /// <summary>
        /// Get UI manager
        /// </summary>
        public UIManager UIManager => uiManager;

        /// <summary>
        /// Get assessment quiz manager
        /// </summary>
        public AssessmentQuizManager AssessmentQuizManager => assessmentQuizManager;

        /// <summary>
        /// Get assessment quiz UI
        /// </summary>
        public AssessmentQuizUI AssessmentQuizUI => assessmentQuizUI;

        /// <summary>
        /// Show the assessment quiz for new players
        /// </summary>
        private void ShowAssessmentQuiz()
        {
            if (assessmentQuizUI != null)
            {
                assessmentQuizUI.OnRegionSelected += OnRegionSelected;
                assessmentQuizUI.ShowQuiz();
            }
            else
            {
                Debug.LogWarning("AssessmentQuizUI not found! Starting with default region.");
                OnRegionSelected(AssessmentQuizManager.RegionType.HealthHarbor);
            }
        }

                        /// <summary>
                /// Handle region selection from assessment quiz
                /// </summary>
                private void OnRegionSelected(AssessmentQuizManager.RegionType selectedRegion) // Event handler for region selection from the Assessment Quiz UI. 
                {
                    Debug.Log($"=== GAMEMANAGER ONREGIONSELECTED CALLED ===");
                    Debug.Log($"OnRegionSelected called with region: {AssessmentQuizManager.GetRegionDisplayName(selectedRegion)}");
                    
                    // Set the starting region with quiz scores
                    if (regionUnlockSystem != null && assessmentQuizManager != null)
                    {
                        var quizScores = assessmentQuizManager.GetRegionScores();
                        Debug.Log($"Quiz scores: {string.Join(", ", quizScores.Select(kvp => $"{AssessmentQuizManager.GetRegionDisplayName(kvp.Key)}: {kvp.Value}"))}");
                        
                        Debug.Log("Calling SetStartingRegion...");
                        regionUnlockSystem.SetStartingRegion(selectedRegion, quizScores); // The GameManager calls this method to set the starting region and unlock order based on the selected region and quiz scores. 
                        Debug.Log("SetStartingRegion completed");
                        
                        // Force unlock the region as a backup
                        Debug.Log("Force unlocking region as backup...");
                        regionUnlockSystem.ForceUnlockRegion(selectedRegion);
                        Debug.Log("Force unlock completed");
                    }
                    else
                    {
                        Debug.LogError("RegionUnlockSystem or AssessmentQuizManager is null!");
                        Debug.LogError($"RegionUnlockSystem: {(regionUnlockSystem != null ? "not null" : "null")}");
                        Debug.LogError($"AssessmentQuizManager: {(assessmentQuizManager != null ? "not null" : "null")}");
                    }

                    // Mark assessment as completed
                    hasCompletedAssessment = true;
                    Debug.Log("Assessment marked as completed");

                    // Clean up event listener
                    if (assessmentQuizUI != null)
                    {
                        assessmentQuizUI.OnRegionSelected -= OnRegionSelected;
                        assessmentQuizUI.HideQuiz();
                        Debug.Log("AssessmentQuizUI event listener cleaned up");
                    }

                    // Start the game normally
                    SetGameState(GameState.Playing);
                    Debug.Log("Game state set to Playing");
                    
                    // Initialize systems
                    if (cityBuilder != null)
                        cityBuilder.enabled = true;
                    
                    if (questManager != null)
                        questManager.Initialize();
                    
                    // Initialize PlayerLevelManager with the selected region and quiz scores
                    if (PlayerLevelManager.Instance != null && assessmentQuizManager != null)
                    {
                        var quizScores = assessmentQuizManager.GetRegionScores();
                        Debug.Log("Initializing PlayerLevelManager building unlock system...");
                        PlayerLevelManager.Instance.InitializeBuildingUnlockSystem(selectedRegion, quizScores);
                        Debug.Log("PlayerLevelManager building unlock system initialized");
                    }
                    else
                    {
                        Debug.LogError("PlayerLevelManager.Instance or AssessmentQuizManager is null - cannot initialize building unlock system!");
                    }

                    // Subscribe to region unlock events to refresh UI when regions are unlocked
                    if (regionUnlockSystem != null)
                    {
                        Debug.Log("Subscribing to RegionUnlockSystem.OnRegionUnlocked event...");
                        regionUnlockSystem.OnRegionUnlocked += OnRegionUnlocked;
                        Debug.Log("RegionUnlockSystem.OnRegionUnlocked event subscribed successfully");
                    }
                    else
                    {
                        Debug.LogError("RegionUnlockSystem is null - cannot subscribe to unlock events!");
                    }
                    
                    // Refresh all UI components to reflect the unlocked region
                    Debug.Log("Starting UI refresh...");
                    RefreshUIAfterRegionUnlock();
                    
                    // Force refresh UI components after a short delay to ensure they're initialized
                    StartCoroutine(RefreshUIAfterDelay());
                    
                    // Additional refresh after a longer delay to ensure RegionUnlockSystem state is fully processed
                    StartCoroutine(RefreshUIAfterRegionUnlockDelayed());
                    
                    // Save the game immediately to persist the unlock state
                    SaveGame();
                    Debug.Log("Game saved after region selection");

                    Debug.Log($"Game started with region: {AssessmentQuizManager.GetRegionDisplayName(selectedRegion)}");
                }

        /// <summary>
        /// Handle region unlock events (called when a region is unlocked automatically)
        /// </summary>
        private void OnRegionUnlocked(AssessmentQuizManager.RegionType unlockedRegion)
        {
            Debug.Log($"=== REGION UNLOCKED EVENT ===");
            Debug.Log($"Region unlocked: {AssessmentQuizManager.GetRegionDisplayName(unlockedRegion)}");
            Debug.Log($"Event triggered at: {System.DateTime.Now}");
            
            // Start a coroutine to refresh UI after a short delay to ensure unlock is processed
            StartCoroutine(RefreshUIAfterRegionUnlockDelayed());
            
            // Save the game to persist the unlock state
            SaveGame();
            
            Debug.Log($"=== END REGION UNLOCKED EVENT ===");
        }

        /// <summary>
        /// Refresh all UI components after a region is unlocked
        /// </summary>
        private void RefreshUIAfterRegionUnlock()
        {
            Debug.Log("=== REFRESH UI AFTER REGION UNLOCK ===");
            Debug.Log($"RefreshUIAfterRegionUnlock called at: {System.DateTime.Now}");
            
            // Check RegionUnlockSystem state first
            if (regionUnlockSystem != null)
            {
                var unlockedRegions = regionUnlockSystem.GetUnlockedRegions();
                Debug.Log($"RegionUnlockSystem reports {unlockedRegions.Count} unlocked regions: {string.Join(", ", unlockedRegions.ConvertAll(r => AssessmentQuizManager.GetRegionDisplayName(r)))}");
            }
            else
            {
                Debug.LogError("RegionUnlockSystem is null in RefreshUIAfterRegionUnlock!");
            }
            
            // Refresh building panel
            if (uiManager != null)
            {
                Debug.Log("Refreshing building panel...");
                uiManager.RefreshBuildingPanel();
            }
            else
            {
                Debug.LogWarning("UIManager is null!");
            }

            // Refresh shop tabs
            var shopTabManager = FindFirstObjectByType<ShopTabManager>(FindObjectsInactive.Include);
            if (shopTabManager != null)
            {
                Debug.Log("Refreshing shop tab visibility...");
                Debug.Log($"Found ShopTabManager: {shopTabManager.name} (Instance ID: {shopTabManager.GetInstanceID()}) - Active: {shopTabManager.gameObject.activeSelf}");
                
                // If the ShopTabManager GameObject is inactive, activate it
                if (!shopTabManager.gameObject.activeSelf)
                {
                    Debug.Log("ShopTabManager GameObject was inactive, activating it...");
                    shopTabManager.gameObject.SetActive(true);
                }
                
                shopTabManager.RefreshTabVisibility();
            }
            else
            {
                Debug.LogError("ShopTabManager not found! Trying alternative search...");
                // Try to find it in the scene by name
                var allShopTabManagers = FindObjectsByType<ShopTabManager>(FindObjectsSortMode.None);
                Debug.Log($"Found {allShopTabManagers.Length} ShopTabManager instances in scene");
                foreach (var manager in allShopTabManagers)
                {
                    Debug.Log($"ShopTabManager: {manager.name} (Instance ID: {manager.GetInstanceID()}) - Active: {manager.gameObject.activeSelf}");
                }
            }

            // Refresh region progress UI
            var regionProgressUI = FindFirstObjectByType<RegionProgressUI>();
            if (regionProgressUI != null)
            {
                Debug.Log("Refreshing region progress UI...");
                regionProgressUI.ForceUpdateDisplay(); // Force update the display
            }
            else
            {
                Debug.LogWarning("RegionProgressUI not found!");
            }
            
            // Refresh city map zoom controller
            var cityMapZoomController = FindFirstObjectByType<CityMapZoomController>();
            if (cityMapZoomController != null)
            {
                Debug.Log("Refreshing city map zoom controller...");
                cityMapZoomController.ForceRefreshUnlockState();
            }
            else
            {
                Debug.LogWarning("CityMapZoomController not found!");
            }

            Debug.Log("=== END REFRESH UI AFTER REGION UNLOCK ===");
        }

        /// <summary>
        /// Refresh UI components after a short delay to ensure they're properly initialized
        /// </summary>
        private System.Collections.IEnumerator RefreshUIAfterDelay()
        {
            yield return new WaitForSeconds(0.2f); // Wait for UI components to initialize
            
            Debug.Log("Delayed UI refresh starting...");
            RefreshUIAfterRegionUnlock();
            Debug.Log("Delayed UI refresh completed");
        }

        /// <summary>
        /// Refresh UI components after a region unlock with a delay to ensure unlock is processed
        /// </summary>
        private System.Collections.IEnumerator RefreshUIAfterRegionUnlockDelayed()
        {
            yield return new WaitForSeconds(0.2f); // Wait for region unlock to be fully processed
            
            Debug.Log("=== DELAYED UI REFRESH AFTER REGION UNLOCK ===");
            Debug.Log($"Refreshing UI after region unlock at: {System.DateTime.Now}");
            RefreshUIAfterRegionUnlock();
            Debug.Log("=== END DELAYED UI REFRESH ===");
        }

        private System.Collections.IEnumerator RefreshUIAfterLoadDelay()
        {
            yield return new WaitForSeconds(1.0f); // Wait for UI to be fully initialized. 
            Debug.Log("Performing delayed UI refresh after loading building unlock data...");
            RefreshUIAfterRegionUnlock(); 
        }

        /// <summary>
        /// Reset all regions to locked state (for testing)
        /// </summary>
        [ContextMenu("Reset All Regions to Locked")]
        public void ResetAllRegionsToLocked()
        {
            Debug.Log("=== RESETTING ALL REGIONS FROM GAMEMANAGER ===");
            
            if (regionUnlockSystem == null)
            {
                Debug.LogError("RegionUnlockSystem is null!");
                return;
            }
            
            regionUnlockSystem.ResetAllRegionsToLocked();
            
            Debug.Log("Refreshing UI after reset...");
            RefreshUIAfterRegionUnlock();
            
            Debug.Log("=== END RESET ===");
        }
        
        /// <summary>
        /// Test method to unlock Health Harbor region (for debugging)
        /// </summary>
        [ContextMenu("Test Unlock Health Harbor")]
        public void TestUnlockHealthHarbor()
        {
            Debug.Log("=== TESTING UNLOCK SYSTEM ===");
            
            if (regionUnlockSystem == null)
            {
                Debug.LogError("RegionUnlockSystem is null!");
                return;
            }
            
            Debug.Log("Calling TestUnlockRegion for HealthHarbor...");
            regionUnlockSystem.TestUnlockRegion(AssessmentQuizManager.RegionType.HealthHarbor);
            
            Debug.Log("Refreshing UI after test unlock...");
            RefreshUIAfterRegionUnlock();
            
            Debug.Log("=== END TEST ===");
        }
        
        /// <summary>
        /// Test method to unlock all regions (for debugging)
        /// </summary>
        [ContextMenu("Test Unlock All Regions")]
        public void TestUnlockAllRegions()
        {
            Debug.Log("=== TESTING UNLOCK ALL REGIONS ===");
            
            if (regionUnlockSystem == null)
            {
                Debug.LogError("RegionUnlockSystem is null!");
                return;
            }
            
            var regions = new[]
            {
                AssessmentQuizManager.RegionType.HealthHarbor,
                AssessmentQuizManager.RegionType.MindPalace,
                AssessmentQuizManager.RegionType.CreativeCommons,
                AssessmentQuizManager.RegionType.SocialSquare
            };
            
            foreach (var region in regions)
            {
                Debug.Log($"Testing unlock for {region}...");
                regionUnlockSystem.TestUnlockRegion(region);
            }
            
            Debug.Log("Refreshing UI after test unlock...");
            RefreshUIAfterRegionUnlock();
            
            Debug.Log("=== END TEST ===");
        }
    }

    /// <summary>
    /// Game states
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        Building,
        Quest,
        Settings,
        GameOver
    }

    // Wrapper class for city save data serialization
    [System.Serializable]
    public class CitySaveWrapper
    {
        public List<BuildingSaveData> buildings;
    }

    /// <summary>
    /// CRITICAL FIX: Wrapper class for serializing quiz scores to PlayerPrefs
    /// 
    /// PROBLEM: Unity's JsonUtility cannot serialize Dictionary objects directly.
    /// SOLUTION: Convert Dictionary to a serializable List of QuizScoreEntry objects.
    /// 
    /// This allows us to save and load quiz scores for building unlock system re-initialization.
    /// </summary>
    [System.Serializable]
    public class QuizScoresWrapper
    {
        public List<QuizScoreEntry> scores = new List<QuizScoreEntry>(); // List of quiz score entries for each region
    }

    /// <summary>
    /// Individual quiz score entry for serialization
    /// Contains the region type and its corresponding quiz score
    /// </summary>
    [System.Serializable]
    public class QuizScoreEntry
    {
        public AssessmentQuizManager.RegionType regionType; // The region (HealthHarbor, MindPalace, etc.)
        public int score; // The quiz score for this region
    }

    /*
    ================================================================================
    CLOUD SAVE INTEGRATION - TO BE IMPLEMENTED LATER
    ================================================================================
    
    When implementing user accounts and cloud save, replace the above save/load methods
    with cloud-based storage. This will enable cross-device sync and data backup.
    
    Example implementation structure:
    
    public async Task SaveGameToCloud(string userId)
    {
        try
        {
            var gameData = new GameSaveData
            {
                resources = ResourceManager.Instance.GetAllResources(),
                cityData = cityBuilder.GetSaveData(),
                regionUnlockData = regionUnlockSystem.GetSaveData(),
                questData = questManager.GetSaveData(),
                gameState = currentGameState,
                isPaused = isGamePaused,
                saveTimestamp = DateTime.UtcNow,
                version = "1.0"
            };
            
            string jsonData = JsonUtility.ToJson(gameData);
            await CloudSaveManager.Instance.SaveData(userId, "gameData", jsonData);
            
            // Also save locally as backup
            SaveGame();
        }
        catch (Exception e)
        {
            Debug.LogError($"Cloud save failed: {e.Message}");
            // Fall back to local save only
            SaveGame();
        }
    }
    
    public async Task LoadGameFromCloud(string userId)
    {
        try
        {
            string jsonData = await CloudSaveManager.Instance.LoadData(userId, "gameData");
            if (!string.IsNullOrEmpty(jsonData))
            {
                var gameData = JsonUtility.FromJson<GameSaveData>(jsonData);
                
                // Restore all game systems
                ResourceManager.Instance.LoadFromData(gameData.resources);
                cityBuilder.LoadCity(gameData.cityData);
                regionUnlockSystem.LoadSaveData(gameData.regionUnlockData);
                questManager.LoadFromData(gameData.questData);
                SetGameState(gameData.gameState);
                SetPaused(gameData.isPaused);
            }
            else
            {
                // No cloud data, load from local storage
                LoadGame();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Cloud load failed: {e.Message}");
            // Fall back to local load
            LoadGame();
        }
    }
    
    [System.Serializable]
    public class GameSaveData
    {
        public Dictionary<ResourceType, int> resources;
        public List<BuildingSaveData> cityData;
        public RegionUnlockSaveData regionUnlockData;
        public QuestSaveData questData;
        public GameState gameState;
        public bool isPaused;
        public DateTime saveTimestamp;
        public string version;
    }
    
    // Cloud Save Manager Singleton (to be implemented)
    public class CloudSaveManager
    {
        public static CloudSaveManager Instance { get; private set; }
        
        public async Task SaveData(string userId, string dataKey, string jsonData)
        {
            // Implement cloud save logic (Firebase, PlayFab, etc.)
            // This will vary based on your chosen cloud service
        }
        
        public async Task<string> LoadData(string userId, string dataKey)
        {
            // Implement cloud load logic
            // Return JSON string or null if no data exists
        }
    }
    */
} 