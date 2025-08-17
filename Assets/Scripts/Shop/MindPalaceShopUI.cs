using System.Collections.Generic;
using UnityEngine;
using LifeCraft.Core;
using LifeCraft.UI;
using LifeCraft.Systems; // Needed for PlayerLevelManager. 

namespace LifeCraft.Shop
{
    /// <summary>
    /// Manages the Mind Palace shop UI and purchase logic.
    /// </summary>
    public class MindPalaceShopUI : MonoBehaviour
    {
        [Header("Shop Data")]
        public BuildingShopDatabase buildingDatabase; // Assign the BuildingShopDatabase ScriptableObject in Inspector.

        [Header("UI References")]
        public Transform shopGrid; // Parent for shop items (e.g., Vertical/Horizontal/Grid Layout Group)
        public GameObject shopItemPrefab; // Assign ShopBuildingItemUI prefab
        public PurchaseConfirmModal confirmModal; // Assign modal in Inspector

        public Sprite resourceIcon; // Assign the Wisdom Orbs icon in Inspector. 

        public RewardModal rewardModal; // Assign the RewardModal in Inspector. 

        private void Start()
        {
            PopulateShop(); // Populate the shop with items based on the current player level and unlocked buildings. 

            // Subscribe to level up events to refresh the shop (dynamic refresh):
            if (PlayerLevelManager.Instance != null)
            {
                PlayerLevelManager.Instance.OnLevelUp += OnPlayerLevelUp; // Subscribe to level up event. 
            }
        }

        private void OnPlayerLevelUp(int newLevel) // Called when the player levels up. 
        {
            // OPTIMIZATION: Instead of repopulating the entire shop (which destroys and recreates all UI elements),
            // we now only update the lock states of existing shop items. This provides better performance
            // and smoother user experience when transitioning from locked to unlocked states.
            UpdateShopItemLockStates(); // Update lock states instead of repopulating
        }

        private void OnDestroy()
        {
            // Unsubscribe from level up events to prevent memory leaks. 
            if (PlayerLevelManager.Instance != null)
            {
                PlayerLevelManager.Instance.OnLevelUp -= OnPlayerLevelUp; // Unsubscribe from level up event. 
            }
        }

        /// <summary>
        /// Populates the shop with ALL available buildings, displaying both unlocked and locked items.
        /// This creates a better UX by showing players their complete progression path and what's coming next.
        /// </summary>
        void PopulateShop()
        {
            // Clear old items
            foreach (Transform child in shopGrid)
                Destroy(child.gameObject);

            // UI CHANGE: Show ALL buildings instead of only unlocked ones
            // This allows players to see their complete progression path and creates anticipation
            // for future unlocks, improving overall user engagement and satisfaction.
            foreach (var item in buildingDatabase.buildings)
            {
                var go = Instantiate(shopItemPrefab, shopGrid);
                var ui = go.GetComponent<ShopBuildingItemUI>();
                
                // Determine lock state for each building based on current player level
                // This enables the visual distinction between available and future content
                bool isUnlocked = PlayerLevelManager.Instance.IsBuildingUnlocked(item.name);
                int unlockLevel = PlayerLevelManager.Instance.GetBuildingUnlockLevel(item.name);
                
                // Setup UI with appropriate lock state - locked buildings will show gray overlay,
                // unlock level requirement, and disabled buy button while maintaining visibility
                ui.Setup(item, OnBuyClicked, resourceIcon, !isUnlocked, unlockLevel);
            }
        }

        /// <summary>
        /// Updates the lock state of all existing shop items when the player levels up.
        /// This method provides smooth transitions from locked to unlocked state without
        /// destroying and recreating UI elements, resulting in better performance and UX.
        /// </summary>
        void UpdateShopItemLockStates()
        {
            // Iterate through all existing shop items and update their lock states
            // This approach is more efficient than repopulating the entire shop
            foreach (Transform child in shopGrid)
            {
                var ui = child.GetComponent<ShopBuildingItemUI>();
                if (ui != null)
                {
                    // Get the building name to check its current unlock status
                    string buildingName = ui.GetBuildingName();
                    if (!string.IsNullOrEmpty(buildingName))
                    {
                        // Re-evaluate unlock status based on new player level
                        // This ensures the UI accurately reflects the current game state
                        bool isUnlocked = PlayerLevelManager.Instance.IsBuildingUnlocked(buildingName);
                        int unlockLevel = PlayerLevelManager.Instance.GetBuildingUnlockLevel(buildingName);
                        
                        // Update the UI to reflect the new lock state
                        // This will trigger visual changes (overlay removal, button enabling, etc.)
                        ui.UpdateLockState(!isUnlocked, unlockLevel);
                    }
                }
            }
        }

        void OnBuyClicked(BuildingShopItem item) // Called when a buy button is clicked in the shop. 
        {
            // Get sprite from CityBuilder
            Sprite buildingSprite = null;
            if (LifeCraft.Core.CityBuilder.Instance != null)
            {
                var buildingData = LifeCraft.Core.CityBuilder.Instance.GetBuildingTypeData(item.name);
                if (buildingData != null && buildingData.buildingSprite != null)
                {
                    buildingSprite = buildingData.buildingSprite;
                }
            }
            
            confirmModal.Show(
                $"Buy {item.name} for {item.price} Wisdom Orbs?",
                () => TryPurchase(item),
                buildingSprite // Pass the sprite from CityBuilder
            );
        }

        void TryPurchase(BuildingShopItem item) // Handles the actual purchase logic. 
        {
            int current = ResourceManager.Instance.GetResourceTotal(ResourceManager.ResourceType.WisdomOrbs);
            if (current >= item.price) // If the player has enough resources to buy the item, proceed with the purchase. 
            {
                ResourceManager.Instance.SpendResources(ResourceManager.ResourceType.WisdomOrbs, item.price); // Deduct the item's price from the player's resources. 
                // Add the purchased item to the player's inventory and set its region for correct filtering in the UI.
                InventoryManager.Instance.AddDecorationByName(item.name, "ShopPurchase", false, RegionType.MindPalace);
                if (rewardModal != null)
                {
                    // Get sprite from CityBuilder
                    Sprite buildingSprite = null;
                    if (LifeCraft.Core.CityBuilder.Instance != null)
                    {
                        var buildingData = LifeCraft.Core.CityBuilder.Instance.GetBuildingTypeData(item.name);
                        if (buildingData != null && buildingData.buildingSprite != null)
                        {
                            buildingSprite = buildingData.buildingSprite;
                        }
                    }
                    rewardModal.Show($"You got a {item.name}! Congratulations!", buildingSprite);
                }
                // Optionally show a success modal or notification here (Done!)
            }
            else
            {
                if (rewardModal != null)
                {
                    rewardModal.Show("Not enough Wisdom Orbs to buy this item!", resourceIcon); // Show a warning modal if the player doesn't have enough resources. 
                }
                // Optionally show an error modal or notification here (Done!)
            }
        }
    }
}