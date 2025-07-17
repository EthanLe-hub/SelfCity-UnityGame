using UnityEngine;
using LifeCraft.Core;
using System.Collections.Generic;

namespace LifeCraft.Core
{
    /// <summary>
    /// Debug script to help test and troubleshoot the save/load system.
    /// Attach this to a GameObject in your scene for testing.
    /// </summary>
    public class SaveLoadDebugger : MonoBehaviour
    {
        [Header("Debug Controls")]
        [SerializeField] private KeyCode saveKey = KeyCode.F5;
        [SerializeField] private KeyCode loadKey = KeyCode.F9;
        [SerializeField] private KeyCode clearSaveKey = KeyCode.F12;
        [SerializeField] private KeyCode testPlacementKey = KeyCode.T;

        private void Update()
        {
            // Save game
            if (Input.GetKeyDown(saveKey))
            {
                Debug.Log("=== MANUAL SAVE TRIGGERED ===");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SaveGame();
                }
                else
                {
                    Debug.LogError("GameManager.Instance is null!");
                }
            }

            // Load game
            if (Input.GetKeyDown(loadKey))
            {
                Debug.Log("=== MANUAL LOAD TRIGGERED ===");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.LoadGame();
                }
                else
                {
                    Debug.LogError("GameManager.Instance is null!");
                }
            }

            // Clear save data
            if (Input.GetKeyDown(clearSaveKey))
            {
                Debug.Log("=== CLEARING ALL SAVE DATA ===");
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Debug.Log("All save data cleared!");
            }

            // Test building placement
            if (Input.GetKeyDown(testPlacementKey))
            {
                Debug.Log("=== TESTING BUILDING PLACEMENT ===");
                TestBuildingPlacement();
            }
        }

        private void TestBuildingPlacement()
        {
            if (CityBuilder.Instance == null)
            {
                Debug.LogError("CityBuilder.Instance is null!");
                return;
            }

            // Try to place a test building
            Vector3 testPosition = new Vector3(0, 0, 0);
            bool success = CityBuilder.Instance.AttemptToPlaceBuilding("Test Building", testPosition);
            
            if (success)
            {
                Debug.Log("Test building placed successfully!");
            }
            else
            {
                Debug.LogWarning("Failed to place test building");
            }
        }

        [ContextMenu("Print Save Data")]
        public void PrintSaveData()
        {
            Debug.Log("=== CURRENT SAVE DATA ===");
            
            if (PlayerPrefs.HasKey("LastSaveTime"))
            {
                Debug.Log($"Last Save Time: {PlayerPrefs.GetString("LastSaveTime")}");
            }
            else
            {
                Debug.Log("No LastSaveTime found");
            }

            if (PlayerPrefs.HasKey("CityData"))
            {
                string cityData = PlayerPrefs.GetString("CityData");
                Debug.Log($"City Data: {cityData}");
            }
            else
            {
                Debug.Log("No CityData found");
            }

            if (PlayerPrefs.HasKey("UnlockData"))
            {
                string unlockData = PlayerPrefs.GetString("UnlockData");
                Debug.Log($"Unlock Data: {unlockData}");
            }
            else
            {
                Debug.Log("No UnlockData found");
            }

            // Check for resource data
            bool hasResources = false;
            foreach (ResourceManager.ResourceType resourceType in System.Enum.GetValues(typeof(ResourceManager.ResourceType)))
            {
                string key = $"Resource_{resourceType}";
                if (PlayerPrefs.HasKey(key))
                {
                    if (!hasResources)
                    {
                        Debug.Log("Resource Data:");
                        hasResources = true;
                    }
                    Debug.Log($"  {resourceType}: {PlayerPrefs.GetInt(key)}");
                }
            }

            if (!hasResources)
            {
                Debug.Log("No Resource data found");
            }
        }

        [ContextMenu("Print Current Building Count")]
        public void PrintCurrentBuildingCount()
        {
            if (CityBuilder.Instance != null)
            {
                var saveData = CityBuilder.Instance.GetSaveData();
                Debug.Log($"Current buildings in CityBuilder: {saveData.Count}");
                
                foreach (var building in saveData)
                {
                    if (building.positionX.HasValue)
                    {
                        Debug.Log($"  Tilemap Building: {building.buildingType} at ({building.positionX}, {building.positionY}, {building.positionZ})");
                    }
                    else if (building.worldX.HasValue)
                    {
                        Debug.Log($"  UI Building: {building.buildingType} at ({building.worldX}, {building.worldY}, {building.worldZ})");
                    }
                }
            }
            else
            {
                Debug.LogError("CityBuilder.Instance is null!");
            }
        }

        [ContextMenu("Print Current Resources")]
        public void PrintCurrentResources()
        {
            if (ResourceManager.Instance != null)
            {
                Debug.Log("=== CURRENT RESOURCES ===");
                foreach (ResourceManager.ResourceType resourceType in System.Enum.GetValues(typeof(ResourceManager.ResourceType)))
                {
                    int amount = ResourceManager.Instance.GetResourceTotal(resourceType);
                    Debug.Log($"  {resourceType}: {amount}");
                }
            }
            else
            {
                Debug.LogError("ResourceManager.Instance is null!");
            }
        }

        [ContextMenu("Test Resource Loading")]
        public void TestResourceLoading()
        {
            Debug.Log("=== TESTING RESOURCE LOADING ===");
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.LoadResources();
            }
            else
            {
                Debug.LogError("ResourceManager.Instance is null!");
            }
        }

        [ContextMenu("Test City Loading")]
        public void TestCityLoading()
        {
            Debug.Log("=== TESTING CITY LOADING ===");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadGame();
            }
            else
            {
                Debug.LogError("GameManager.Instance is null!");
            }
        }

        [ContextMenu("Monitor Save Calls")]
        public void MonitorSaveCalls()
        {
            Debug.Log("=== MONITORING SAVE CALLS ===");
            Debug.Log("This will help track when SaveGame() is called and what data is being saved.");
            Debug.Log("Watch the console for 'GetSaveData called' messages.");
            
            // Check current state
            if (CityBuilder.Instance != null)
            {
                var saveData = CityBuilder.Instance.GetSaveData();
                Debug.Log($"Current save data count: {saveData.Count}");
            }
        }

        [ContextMenu("Check Current Building State")]
        public void CheckCurrentBuildingState()
        {
            Debug.Log("=== CURRENT BUILDING STATE ===");
            if (CityBuilder.Instance != null)
            {
                var saveData = CityBuilder.Instance.GetSaveData();
                Debug.Log($"Total buildings to save: {saveData.Count}");
                
                foreach (var building in saveData)
                {
                    if (building.positionX.HasValue)
                    {
                        Debug.Log($"  Tilemap Building: {building.buildingType} at ({building.positionX}, {building.positionY}, {building.positionZ})");
                    }
                    else if (building.worldX.HasValue)
                    {
                        Debug.Log($"  UI Building: {building.buildingType} at ({building.worldX}, {building.worldY}, {building.worldZ})");
                    }
                }
            }
        }

        [ContextMenu("Print Building Types")]
        public void PrintBuildingTypes()
        {
            if (CityBuilder.Instance != null)
            {
                Debug.Log("=== AVAILABLE BUILDING TYPES ===");
                var buildingTypes = CityBuilder.Instance.GetAvailableBuildingTypes();
                foreach (var buildingType in buildingTypes)
                {
                    Debug.Log($"  {buildingType}");
                }
            }
            else
            {
                Debug.LogError("CityBuilder.Instance is null!");
            }
        }

        [ContextMenu("Print Building Type Data")]
        public void PrintBuildingTypeData()
        {
            if (CityBuilder.Instance != null)
            {
                Debug.Log("=== BUILDING TYPE DATA ===");
                var buildingTypes = CityBuilder.Instance.GetAvailableBuildingTypes();
                foreach (var buildingType in buildingTypes)
                {
                    var data = CityBuilder.Instance.GetBuildingTypeData(buildingType);
                    if (data != null)
                    {
                        Debug.Log($"  {buildingType}: prefab={data.buildingPrefab != null}, sprite={data.buildingSprite != null}");
                    }
                    else
                    {
                        Debug.Log($"  {buildingType}: NO DATA FOUND");
                    }
                }
            }
            else
            {
                Debug.LogError("CityBuilder.Instance is null!");
            }
        }

        [ContextMenu("Test Building Restoration")]
        public void TestBuildingRestoration()
        {
            Debug.Log("=== TESTING BUILDING RESTORATION ===");
            
            // First, let's see what's saved
            if (PlayerPrefs.HasKey("CityData"))
            {
                string cityJson = PlayerPrefs.GetString("CityData");
                Debug.Log($"Saved city data: {cityJson}");
                
                var cityWrapper = JsonUtility.FromJson<CitySaveWrapper>(cityJson);
                if (cityWrapper != null && cityWrapper.buildings != null)
                {
                    Debug.Log($"Found {cityWrapper.buildings.Count} saved buildings:");
                    foreach (var building in cityWrapper.buildings)
                    {
                        if (building.positionX.HasValue)
                        {
                            Debug.Log($"  Tilemap Building: {building.buildingType} at ({building.positionX}, {building.positionY}, {building.positionZ})");
                        }
                        else if (building.worldX.HasValue)
                        {
                            Debug.Log($"  UI Building: {building.buildingType} at ({building.worldX}, {building.worldY}, {building.worldZ})");
                        }
                    }
                }
            }
            else
            {
                Debug.Log("No city data found in PlayerPrefs");
            }
            
            // Now let's check if the building types exist
            if (CityBuilder.Instance != null)
            {
                var buildingTypes = CityBuilder.Instance.GetAvailableBuildingTypes();
                Debug.Log($"Available building types: {string.Join(", ", buildingTypes)}");
            }
        }

        [System.Serializable]
        public class CitySaveWrapper
        {
            public List<BuildingSaveData> buildings;
        }

        [System.Serializable]
        public class BuildingSaveData
        {
            public string buildingType;
            public int? positionX;
            public int? positionY;
            public int? positionZ;
            public float? worldX;
            public float? worldY;
            public float? worldZ;
        }
    }
} 