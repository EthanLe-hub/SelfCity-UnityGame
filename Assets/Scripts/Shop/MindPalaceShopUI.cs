using System.Collections.Generic;
using UnityEngine;
using LifeCraft.Core;
using LifeCraft.UI;

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
            confirmModal.Show(
                $"Buy {item.name} for {item.price} Wisdom Orbs?",
                () => TryPurchase(item)
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
                    rewardModal.Show($"You got a {item.name}! Congratulations!", item.icon);
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