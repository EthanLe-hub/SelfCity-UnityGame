using LifeCraft.Core;
using LifeCraft.Systems;
using UnityEngine;
using UnityEngine.SceneManagement;
using LifeCraft.UI;
using System.Collections.Generic;

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
        [SerializeField] private UnlockSystem unlockSystem;
        [SerializeField] private QuestManager questManager;
    //    [SerializeField] private SelfCareManager selfCareManager;
    //    [SerializeField] private WeatherSystem weatherSystem;

        [Header("UI")]
        [SerializeField] private UIManager uiManager;

        [Header("Game State")]
        [SerializeField] private GameState currentGameState = GameState.MainMenu;
        [SerializeField] private bool isGamePaused = false;

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
            // Singleton pattern
            if (_instance == null)
            {
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

        /// <summary>
        /// Initialize all game systems
        /// </summary>
        private void InitializeGame()
        {
            // Find or create other systems
            if (cityBuilder == null)
                cityBuilder = FindFirstObjectByType<CityBuilder>();
            
            if (unlockSystem == null)
                unlockSystem = FindFirstObjectByType<UnlockSystem>();
            
            if (questManager == null)
                questManager = FindFirstObjectByType<QuestManager>();
            
            //if (selfCareManager == null)
                //selfCareManager = FindFirstObjectByType<SelfCareManager>();
            
            //if (weatherSystem == null)
                //weatherSystem = FindFirstObjectByType<WeatherSystem>();

            // Find UI manager
            if (uiManager == null)
                uiManager = FindFirstObjectByType<UIManager>();
        }

        /// <summary>
        /// Start the game
        /// </summary>
        private void StartGame()
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
                if (unlockSystem != null)
                {
                    var unlockData = unlockSystem.GetSaveData();
                    string unlockJson = JsonUtility.ToJson(unlockData);
                    PlayerPrefs.SetString("UnlockData", unlockJson);
                }

                // Save quest data (daily quests, custom quests, progress)
                if (questManager != null)
                    questManager.SaveQuests();

                // Save current game state and pause status
                PlayerPrefs.SetInt("GameState", (int)currentGameState);
                PlayerPrefs.SetInt("IsPaused", isGamePaused ? 1 : 0);
                
                // Save timestamp for tracking when the game was last saved
                PlayerPrefs.SetString("LastSaveTime", System.DateTime.UtcNow.ToString("O"));
                
                PlayerPrefs.Save(); // Force write to device storage
                Debug.Log("Game saved successfully to local storage!");
                
                if (uiManager != null)
                    uiManager.ShowNotification("Game saved!");
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
                // Load resource data (player's currency and materials)
                if (ResourceManager.Instance != null) ResourceManager.Instance.LoadResources();
                // --- Ensure the resource bar UI is updated after loading resources ---
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.InitializeUI(); // This will refresh the resource bar and all UI elements
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

                // Load unlock data (progressed items and features)
                if (unlockSystem != null && PlayerPrefs.HasKey("UnlockData"))
                {
                    string unlockJson = PlayerPrefs.GetString("UnlockData");
                    var unlockData = JsonUtility.FromJson<LifeCraft.Systems.UnlockSaveData>(unlockJson);
                    unlockSystem.LoadSaveData(unlockData);
                }

                // Load quest data (daily quests, custom quests, progress)
                if (questManager != null)
                    questManager.LoadQuests();

                // Load game state and pause status
                if (PlayerPrefs.HasKey("GameState"))
                {
                    GameState savedState = (GameState)PlayerPrefs.GetInt("GameState");
                    SetGameState(savedState);
                }
                
                if (PlayerPrefs.HasKey("IsPaused"))
                {
                    bool savedPause = PlayerPrefs.GetInt("IsPaused") == 1;
                    SetPaused(savedPause);
                }

                Debug.Log("Game loaded successfully from local storage!");
                
                if (uiManager != null)
                    uiManager.ShowNotification("Game loaded!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load game: {e.Message}");
                
                if (uiManager != null)
                    uiManager.ShowNotification("Failed to load game!");
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
        public UnlockSystem UnlockSystem => unlockSystem;

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
                unlockData = unlockSystem.GetSaveData(),
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
                unlockSystem.LoadSaveData(gameData.unlockData);
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
        public UnlockSaveData unlockData;
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