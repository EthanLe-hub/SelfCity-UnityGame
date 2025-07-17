using System.Collections.Generic;
using UnityEngine;
using LifeCraft.Core;
using LifeCraft.UI;

namespace LifeCraft.Shop
{
    /// <summary>
    /// Manages the Health Harbor shop UI and purchase logic.
    /// </summary>
    public class HealthHarborShopUI : MonoBehaviour
    {
        [Header("Shop Data")]
        public BuildingShopDatabase buildingDatabase; // Assign the BuildingShopDatabase ScriptableObject in Inspector.

        [Header("UI References")]
        public Transform shopGrid; // Parent for shop items (e.g., Vertical/Horizontal/Grid Layout Group)
        public GameObject shopItemPrefab; // Assign ShopBuildingItemUI prefab
        public PurchaseConfirmModal confirmModal; // Assign modal in Inspector

        public Sprite resourceIcon; // Assign the Energy Crystals icon in Inspector. 

        public RewardModal rewardModal; // Assign the RewardModal in Inspector. 

        private void Start()
        {
            PopulateShop();
        }

        void PopulateShop()
        {
            // Clear old items
            foreach (Transform child in shopGrid)
                Destroy(child.gameObject);

            // Add new items
            foreach (var item in buildingDatabase.buildings)
            {
                var go = Instantiate(shopItemPrefab, shopGrid);
                var ui = go.GetComponent<ShopBuildingItemUI>();
                ui.Setup(item, OnBuyClicked, resourceIcon); // Pass the resource icon to the Setup method. 
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
                $"Buy {item.name} for {item.price} Energy Crystals?",
                () => TryPurchase(item),
                buildingSprite // Pass the sprite from CityBuilder
            );
        }

        void TryPurchase(BuildingShopItem item) // Handles the actual purchase logic. 
        {
            int current = ResourceManager.Instance.GetResourceTotal(ResourceManager.ResourceType.EnergyCrystals);
            if (current >= item.price) // If the player has enough resources to buy the item, proceed with the purchase. 
            {
                ResourceManager.Instance.SpendResources(ResourceManager.ResourceType.EnergyCrystals, item.price); // Deduct the item's price from the player's resources. 
                // Add the purchased item to the player's inventory and set its region for correct filtering in the UI.
                InventoryManager.Instance.AddDecorationByName(item.name, "ShopPurchase", false, RegionType.HealthHarbor);
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
                    rewardModal.Show("Not enough Energy Crystals to buy this item!", resourceIcon); // Show a warning modal if the player doesn't have enough resources. 
                }
                // Optionally show an error modal or notification here (Done!)
            }
        }
    }
}