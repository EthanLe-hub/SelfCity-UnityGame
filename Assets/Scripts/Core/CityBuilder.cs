using LifeCraft.Systems;
using LifeCraft.Core;
using LifeCraft.Buildings; 
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Events;

namespace LifeCraft.Core
{
    /// <summary>
    /// City building manager that handles building placement and city construction logic.
    /// Replaces the Godot CityBuilder.gd script.
    /// </summary>
    public class CityBuilder : MonoBehaviour
    {
        [System.Serializable]
        public class BuildingTypeData
        {
            public string buildingName;
            public GameObject buildingPrefab;
            public ResourceManager.ResourceType costResource;
            public int costAmount;
            public Sprite buildingSprite;
        }

        [Header("Building Configuration")]
        [SerializeField] private List<BuildingTypeData> buildingTypes = new List<BuildingTypeData>();
        
        [Header("References")]
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private Transform buildingContainer;
        [SerializeField] private UnlockSystem unlockSystem;

        // Events (replacing Godot signals)
        [System.Serializable]
        public class BuildingPlacedEvent : UnityEvent<string, Vector3Int> { }
        public BuildingPlacedEvent OnBuildingPlaced = new BuildingPlacedEvent();

        // Track placed buildings
        private Dictionary<Vector3Int, GameObject> _placedBuildings = new Dictionary<Vector3Int, GameObject>();
        private Dictionary<string, BuildingTypeData> _buildingTypeLookup = new Dictionary<string, BuildingTypeData>();

        private void Awake()
        {
            // Build lookup dictionary
            foreach (var buildingType in buildingTypes)
            {
                _buildingTypeLookup[buildingType.buildingName] = buildingType;
            }

            // Validate references
            if (tilemap == null)
                tilemap = FindFirstObjectByType<Tilemap>();
            
            //if (unlockSystem == null)
                //unlockSystem = UnlockSystem.Instance;
        }

        /// <summary>
        /// Convert world position to tilemap coordinates
        /// </summary>
        public Vector3Int WorldToMap(Vector3 worldPos)
        {
            return tilemap.WorldToCell(worldPos);
        }

        /// <summary>
        /// Convert tilemap coordinates to world position
        /// </summary>
        public Vector3 MapToWorld(Vector3Int mapPos)
        {
            return tilemap.GetCellCenterWorld(mapPos);
        }

        /// <summary>
        /// Check if a position is valid for building placement
        /// </summary>
        public bool IsPlacementValid(Vector3Int mapPos)
        {
            // Check if tile is already occupied
            if (_placedBuildings.ContainsKey(mapPos))
                return false;

            // Check if tile exists in tilemap
            if (!tilemap.HasTile(mapPos))
                return false;

            // TODO: Add additional validation rules (e.g., district restrictions, adjacency rules)
            return true;
        }

        /// <summary>
        /// Attempt to place a building at the specified world position
        /// </summary>
        public bool AttemptToPlaceBuilding(string buildingType, Vector3 worldPos)
        {
            if (!_buildingTypeLookup.ContainsKey(buildingType))
            {
                Debug.LogWarning($"Building type '{buildingType}' does not exist.");
                return false;
            }

            // Check if building is unlocked
            if (unlockSystem != null && !unlockSystem.IsBuildingUnlocked(buildingType))
            {
                Debug.LogWarning($"Building '{buildingType}' is not unlocked yet.");
                return false;
            }

            Vector3Int mapPos = WorldToMap(worldPos);
            
            if (!IsPlacementValid(mapPos))
            {
                Debug.LogWarning($"Position {mapPos} is not valid for building placement.");
                return false;
            }

            var buildingData = _buildingTypeLookup[buildingType];

            // Check if player can afford the building
            if (ResourceManager.Instance.SpendResources(buildingData.costResource, buildingData.costAmount))
            {
                PlaceBuildingInWorld(buildingType, mapPos);
                return true;
            }
            else
            {
                Debug.LogWarning($"Could not afford to place '{buildingType}'.");
                return false;
            }
        }

        /// <summary>
        /// Physically place the building in the world
        /// </summary>
        private void PlaceBuildingInWorld(string buildingType, Vector3Int mapPos)
        {
            var buildingData = _buildingTypeLookup[buildingType];
            
            // Instantiate building prefab
            GameObject buildingInstance = Instantiate(buildingData.buildingPrefab, buildingContainer);
            buildingInstance.transform.position = MapToWorld(mapPos);

            // Set building properties
            var buildingComponent = buildingInstance.GetComponent<Building>();
            if (buildingComponent != null)
            {
                buildingComponent.Initialize(buildingType, buildingData.buildingSprite);
            }

            // Record building placement
            _placedBuildings[mapPos] = buildingInstance;

            // Trigger event
            OnBuildingPlaced?.Invoke(buildingType, mapPos);
            
            Debug.Log($"Placed '{buildingType}' at map position {mapPos}");
        }

        /// <summary>
        /// Remove a building from the world
        /// </summary>
        public void RemoveBuilding(Vector3Int mapPos)
        {
            if (_placedBuildings.TryGetValue(mapPos, out GameObject building))
            {
                Destroy(building);
                _placedBuildings.Remove(mapPos);
                Debug.Log($"Removed building at {mapPos}");
            }
        }

        /// <summary>
        /// Get building at specific position
        /// </summary>
        public GameObject GetBuildingAt(Vector3Int mapPos)
        {
            return _placedBuildings.TryGetValue(mapPos, out GameObject building) ? building : null;
        }

        /// <summary>
        /// Get all placed buildings
        /// </summary>
        public Dictionary<Vector3Int, GameObject> GetAllBuildings()
        {
            return new Dictionary<Vector3Int, GameObject>(_placedBuildings);
        }

        /// <summary>
        /// Get save data for all placed buildings
        /// </summary>
        public List<BuildingSaveData> GetSaveData()
        {
            var buildingsToSave = new List<BuildingSaveData>();
            
            foreach (var kvp in _placedBuildings)
            {
                var building = kvp.Value.GetComponent<Building>();
                if (building != null)
                {
                    buildingsToSave.Add(new BuildingSaveData
                    {
                        buildingType = building.BuildingName,
                        positionX = kvp.Key.x,
                        positionY = kvp.Key.y,
                        positionZ = kvp.Key.z
                    });
                }
            }
            
            return buildingsToSave;
        }

        /// <summary>
        /// Load city from save data
        /// </summary>
        public void LoadCity(List<BuildingSaveData> savedBuildings)
        {
            // Clear existing buildings
            foreach (var building in _placedBuildings.Values)
            {
                if (building != null)
                    Destroy(building);
            }
            _placedBuildings.Clear();

            // Place saved buildings
            foreach (var buildingData in savedBuildings)
            {
                var mapPos = new Vector3Int(buildingData.positionX, buildingData.positionY, buildingData.positionZ);
                PlaceBuildingInWorld(buildingData.buildingType, mapPos);
            }
        }

        /// <summary>
        /// Get building type data
        /// </summary>
        public BuildingTypeData GetBuildingTypeData(string buildingType)
        {
            return _buildingTypeLookup.TryGetValue(buildingType, out BuildingTypeData data) ? data : null;
        }

        /// <summary>
        /// Get all available building types
        /// </summary>
        public List<string> GetAvailableBuildingTypes()
        {
            return new List<string>(_buildingTypeLookup.Keys);
        }
    }

    /// <summary>
    /// Data structure for saving building information
    /// </summary>
    [System.Serializable]
    public class BuildingSaveData
    {
        public string buildingType;
        public int positionX;
        public int positionY;
        public int positionZ;
    }
} 