using UnityEngine;
using UnityEngine.UI;

namespace LifeCraft.Buildings
{
    /// <summary>
    /// Represents a building in the city. Handles building-specific logic and visual representation.
    /// Replaces the Godot Building.gd script.
    /// </summary>
    public class Building : MonoBehaviour
    {
        [Header("Building Properties")]
        [SerializeField] private string buildingName = "Generic Building";
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Image uiImage;
        [SerializeField] private bool isDamaged = false;
        
        [Header("Building Stats")]
        [SerializeField] private int health = 100;
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private float productionRate = 1f;
        [SerializeField] private float lastProductionTime = 0f;

        [Header("Visual Effects")]
        [SerializeField] private GameObject damageEffect;
        [SerializeField] private GameObject constructionEffect;
        [SerializeField] private bool isUnderConstruction = false;

        // Events
        public System.Action<Building> OnBuildingDamaged;
        public System.Action<Building> OnBuildingDestroyed;
        public System.Action<Building> OnBuildingRepaired;
        public System.Action<Building> OnProductionComplete;

        private void Awake()
        {
            // Get sprite renderer if not assigned
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            // Initialize building
            if (isUnderConstruction)
            {
                StartConstruction();
            }
        }

        private void Update()
        {
            // Handle production logic
            if (!isUnderConstruction && !isDamaged && productionRate > 0)
            {
                HandleProduction();
            }
        }

        /// <summary>
        /// Initialize the building with type and sprite
        /// </summary>
        public void Initialize(string name, Sprite sprite)
        {
            buildingName = name;
            SetSprite(sprite);
            
            Debug.Log($"Initialized building: {buildingName}");
        }

        /// <summary>
        /// Set the building's sprite
        /// </summary>
        public void SetSprite(Sprite sprite)
        {
            if (spriteRenderer != null)
                spriteRenderer.sprite = sprite;
            
            if (uiImage != null)
                uiImage.sprite = sprite;
        }

        /// <summary>
        /// Set the building's damaged state
        /// </summary>
        public void SetDamaged(bool damaged)
        {
            isDamaged = damaged;
            
            if (damaged)
            {
                // Apply damage visual effects
                if (damageEffect != null)
                    damageEffect.SetActive(true);
                
                OnBuildingDamaged?.Invoke(this);
            }
            else
            {
                // Remove damage effects
                if (damageEffect != null)
                    damageEffect.SetActive(false);
                
                OnBuildingRepaired?.Invoke(this);
            }
        }

        /// <summary>
        /// Take damage
        /// </summary>
        public void TakeDamage(int damage)
        {
            health -= damage;
            
            if (health <= 0)
            {
                DestroyBuilding();
            }
            else if (health < maxHealth * 0.5f && !isDamaged)
            {
                SetDamaged(true);
            }
        }

        /// <summary>
        /// Repair the building
        /// </summary>
        public void Repair(int repairAmount)
        {
            health = Mathf.Min(health + repairAmount, maxHealth);
            
            if (health >= maxHealth * 0.5f && isDamaged)
            {
                SetDamaged(false);
            }
        }

        /// <summary>
        /// Start construction process
        /// </summary>
        public void StartConstruction()
        {
            isUnderConstruction = true;
            
            if (constructionEffect != null)
                constructionEffect.SetActive(true);
            
            // TODO: Add construction timer and completion logic
        }

        /// <summary>
        /// Complete construction
        /// </summary>
        public void CompleteConstruction()
        {
            isUnderConstruction = false;
            
            if (constructionEffect != null)
                constructionEffect.SetActive(false);
            
            // Start production
            lastProductionTime = Time.time;
        }

        /// <summary>
        /// Handle production logic
        /// </summary>
        private void HandleProduction()
        {
            if (Time.time - lastProductionTime >= productionRate)
            {
                ProduceResources();
                lastProductionTime = Time.time;
            }
        }

        /// <summary>
        /// Produce resources based on building type
        /// </summary>
        private void ProduceResources()
        {
            // TODO: Implement resource production based on building type
            OnProductionComplete?.Invoke(this);
        }

        /// <summary>
        /// Destroy the building
        /// </summary>
        public void DestroyBuilding()
        {
            OnBuildingDestroyed?.Invoke(this);
            Destroy(gameObject);
        }

        /// <summary>
        /// Get building name
        /// </summary>
        public string BuildingName => buildingName;

        /// <summary>
        /// Get current health
        /// </summary>
        public int Health => health;

        /// <summary>
        /// Get max health
        /// </summary>
        public int MaxHealth => maxHealth;

        /// <summary>
        /// Get health percentage
        /// </summary>
        public float HealthPercentage => (float)health / maxHealth;

        /// <summary>
        /// Check if building is damaged
        /// </summary>
        public bool IsDamaged => isDamaged;

        /// <summary>
        /// Check if building is under construction
        /// </summary>
        public bool IsUnderConstruction => isUnderConstruction;

        /// <summary>
        /// Get production rate
        /// </summary>
        public float ProductionRate => productionRate;

        /// <summary>
        /// Set production rate
        /// </summary>
        public void SetProductionRate(float rate)
        {
            productionRate = rate;
        }

        /// <summary>
        /// Get building position in tilemap coordinates
        /// </summary>
        public Vector3Int GetTilePosition()
        {
            var tilemap = FindFirstObjectByType<UnityEngine.Tilemaps.Tilemap>();
            if (tilemap != null)
            {
                return tilemap.WorldToCell(transform.position);
            }
            return Vector3Int.zero;
        }

        /// <summary>
        /// Get save data for this building
        /// </summary>
        public BuildingSaveData GetSaveData()
        {
            return new BuildingSaveData
            {
                buildingType = buildingName,
                positionX = GetTilePosition().x,
                positionY = GetTilePosition().y,
                positionZ = GetTilePosition().z,
                health = health,
                isDamaged = isDamaged,
                isUnderConstruction = isUnderConstruction
            };
        }

        /// <summary>
        /// Load save data for this building
        /// </summary>
        public void LoadSaveData(BuildingSaveData data)
        {
            buildingName = data.buildingType;
            health = data.health;
            isDamaged = data.isDamaged;
            isUnderConstruction = data.isUnderConstruction;
            
            if (isDamaged)
                SetDamaged(true);
            
            if (isUnderConstruction)
                StartConstruction();
        }
    }

    /// <summary>
    /// Extended save data for buildings
    /// </summary>
    [System.Serializable]
    public class BuildingSaveData
    {
        public string buildingType;
        public int positionX;
        public int positionY;
        public int positionZ;
        public int health;
        public bool isDamaged;
        public bool isUnderConstruction;
    }
} 