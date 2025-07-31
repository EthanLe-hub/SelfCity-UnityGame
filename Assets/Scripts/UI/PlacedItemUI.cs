using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // For Image component. Make sure to add this if you use UI images.
using LifeCraft.Core; // For InventoryManager and DecorationItem
using LifeCraft.Systems; // For RegionEditManager and PlayerLevelManager. 

namespace LifeCraft.UI
{
    /// <summary>
    /// Handles click/removal logic for a placed item in the city grid.
    /// When removed, returns the item to the player's inventory.
    /// </summary>
    public class PlacedItemUI : MonoBehaviour, IPointerClickHandler
    {
        private DecorationItem _decorationItem;

        [Header("EXP Reward Configuration")]
        [SerializeField] public int baseBuildingEXP = 10; // Base EXP for placing a building. 
        [SerializeField] public int decorationEXP = 5; // EXP for placing a decoration. 
        [SerializeField] public float expMultiplier = 1.2f; // Multiplier for EXP based on unlock level. 
        
        // Add debug component for troubleshooting
        private void Start()
        {
            if (GetComponent<PlacedItemDebugger>() == null)
            {
                gameObject.AddComponent<PlacedItemDebugger>();
            }
        }

        /// <summary>
        /// Initialize the placed item with its DecorationItem data.
        /// Call this right after instantiating the placed item.
        /// </summary>
        public void Initialize(DecorationItem item)
        {
            _decorationItem = item;

            // Get the type for the item (decoration or region, and specific region):
            var itemType = _decorationItem.region;

            // Reward 5 EXP if placing a decoration:
            if (itemType == RegionType.Decoration && PlayerLevelManager.Instance != null)
            {
                PlayerLevelManager.Instance.AddEXP(decorationEXP); // Reward 5 EXP (decorationEXP) for placing a decoration. 
                
                // Show EXP popup animation
                if (EXPPopupManager.Instance != null)
                {
                    Vector3 buildingPosition = transform.position;
                    EXPPopupManager.Instance.ShowEXPPopup(decorationEXP, buildingPosition, QuestDifficulty.Easy);
                }
            }

            // Calculate and reward EXP based on unlock level if placing a building:
            if (itemType == RegionType.HealthHarbor || itemType == RegionType.MindPalace ||
                itemType == RegionType.CreativeCommons || itemType == RegionType.SocialSquare && 
                PlayerLevelManager.Instance != null)
            {
                // Calculate EXP based on the unlock level: EXP = baseBuildingEXP * (expMultiplier ^ (unlockLevel - 1))
                int expReward = baseBuildingEXP * Mathf.RoundToInt(Mathf.Pow(expMultiplier, PlayerLevelManager.Instance.GetBuildingUnlockLevel(_decorationItem.displayName) - 1)); // DecorationItem has displayName for the building name. 

                PlayerLevelManager.Instance.AddEXP(expReward); // Reward calculated EXP for placing a building. 
                
                // Show EXP popup animation
                if (EXPPopupManager.Instance != null)
                {
                    Vector3 buildingPosition = transform.position;
                    EXPPopupManager.Instance.ShowEXPPopup(expReward, buildingPosition, QuestDifficulty.Medium);
                }
            }
        }

        /// <summary>
        /// Sets the sprite for this item (legacy method - now handled by direct assignment).
        /// </summary>
        public void SetSprite(Sprite sprite)
        {
            Debug.LogError($"=== LEGACY SETSPRITE CALLED ON {gameObject.name} ===");
            Debug.LogError($"This method is deprecated - sprite assignment is now handled directly in DraggableInventoryItem");
            
            // This method is kept for compatibility but should not be used
            // The actual sprite assignment now happens directly in DraggableInventoryItem.OnEndDrag()
        }

        /// <summary>
        /// Called when the placed item is clicked.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            // Only allow removal if the player is in edit mode:
            if (RegionEditManager.Instance != null && RegionEditManager.Instance.IsEditModeActive)
            {
                ReturnToInventory();
            }
        }

        /// <summary>
        /// Returns the item to the inventory and removes it from the grid.
        /// </summary>
        private void ReturnToInventory()
        {
            if (_decorationItem != null && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddDecoration(_decorationItem);
            }
            // --- NEW: Remove the placed item from CityBuilder for persistent saving ---
            if (LifeCraft.Core.CityBuilder.Instance != null)
            {
                LifeCraft.Core.CityBuilder.Instance.RemovePlacedItem(transform.position);
            }
            Destroy(gameObject);

            // Save the city layout immediately after removal to ensure persistence even if the game is closed or crashes.
            if (LifeCraft.Core.GameManager.Instance != null)
            {
                LifeCraft.Core.GameManager.Instance.SaveGame();
            }
        }
    }
}