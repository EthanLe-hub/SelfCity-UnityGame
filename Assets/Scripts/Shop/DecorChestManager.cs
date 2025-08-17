using UnityEngine;
using UnityEngine.UI; // For Button and Image components. 
using LifeCraft.Core; // Reference to the Core namespace for ResourceManager, which deals with resources. 
using LifeCraft.Systems; // Add this to access SubscriptionManager
using LifeCraft.UI; // Add this to use RewardModal

namespace LifeCraft.Shop
{
    public class DecorChestManager : MonoBehaviour
    {
        [Header("References")]
        public PrizePoolManager prizePoolManager; // Add the PrizePoolManager field to the Inspector. This will be used to access the prize pools for decorations. 
        public RewardModal rewardModal; // Reference to the RewardModal popup (assign in Inspector). 

        public Sprite balanceTicketIcon; // Icon for Balance Tickets (assign in Inspector). 

        public PurchaseConfirmModal purchaseConfirmModal; // Reference to the PurchaseConfirmModal (assign in Inspector). 

        [Header("ChestSprites")]        
        public Sprite decorChestSprite; // Sprite for the Decor Chest button (assign in Inspector)
        public Sprite premiumChestSprite; // Sprite for the Premium Decor Chest button (assign in Inspector)

        [Header("Chest Button References")]             
        public Button decorChestButton; // Reference to the Decor Chest button (assign in Inspector)
        public Button premiumChestButton; // Reference to the Premium Decor Chest button (assign in Inspector)

        private void Start()
        {
            // Set the chest button sprites
            SetChestButtonSprites();
        }

        /// <summary>
        /// Set the sprites for both chest buttons with faded appearance
        /// </summary>
        private void SetChestButtonSprites()
        {
            // Set Decor Chest button sprite with faded appearance
            if (decorChestButton != null && decorChestSprite != null)
            {
                var decorImage = decorChestButton.GetComponent<Image>();
                if (decorImage != null)
                {
                    decorImage.sprite = decorChestSprite;
                    // Make the sprite more faded by reducing alpha
                    Color fadedColor = decorImage.color;
                    fadedColor.a = 0.3f; // 30% opacity - adjust this value (0.0f = invisible, 10f = fully opaque)
                    decorImage.color = fadedColor;
                    Debug.Log("Set Decor Chest button sprite with faded appearance");
                }
            }

            // Set Premium Decor Chest button sprite with faded appearance
            if (premiumChestButton != null && premiumChestSprite != null)
            {
                var premiumImage = premiumChestButton.GetComponent<Image>();
                if (premiumImage != null)
                {
                    premiumImage.sprite = premiumChestSprite;
                    // Make the sprite more faded by reducing alpha
                    Color fadedColor = premiumImage.color;
                    fadedColor.a = 0.3f; // 30% opacity - adjust this value (0.0f = invisible, 10f = fully opaque)
                    premiumImage.color = fadedColor;
                    Debug.Log("Set Premium Decor Chest button sprite with faded appearance");
                }
            }
        }

        // Call this from the Decor Chest button (for free & premium players)
        public void OpenDecorChest()
        {
            // Show confirmation modal before opening the chest
            // The modal asks the player to confirm spending a Balance Ticket to open the chest
            if (purchaseConfirmModal != null)
            {
                purchaseConfirmModal.Show(
                    "Open Decor Chest? This will cost 1 Balance Ticket.",
                    () => {
                        // This will be called if the player confirms
                        OpenDecorChestConfirmed();
                    },
                    decorChestSprite // Pass the Decor Chest sprite
                );
            }
            else
            {
                Debug.LogWarning("PurchaseConfirmModal reference is missing! Assign it in the Inspector.");
            }
        }

        // This method contains the actual chest opening logic after confirmation
        private void OpenDecorChestConfirmed()
        {
            if (prizePoolManager == null)
            {
                Debug.LogError("PrizePoolManager reference is missing!");
                return;
            }

            // Check if the player has Balance Tickets:
            int tickets = ResourceManager.Instance.GetResourceTotal(ResourceManager.ResourceType.BalanceTickets); // Get the total number of Balance Tickets the player has using the ResourceManager. 
            if (tickets < 1)
            {
                //Debug.LogWarning("Not enough Balance tickets to open a Decor Chest!");
                // TODO: Show UI warning to the player that they need more Balance Tickets to open a Decor chest. (Done!) 
                if (rewardModal != null)
                {
                    rewardModal.Show("Not enough Balance Tickets to open a Decor Chest!", balanceTicketIcon); // Show a warning modal if the player doesn't have enough resources.
                }
                return; 
            }

            // If the player has enough Balance Tickets, spend one ticket:
            ResourceManager.Instance.SpendResources(ResourceManager.ResourceType.BalanceTickets, 1); // Spend one Balance Ticket using the ResourceManager.

            string reward = prizePoolManager.GetRandomFreeAndPremiumReward();
            if (!string.IsNullOrEmpty(reward))
            {
                Debug.Log("Player won from Decor Chest: " + reward);
                // Add the decoration to the player's inventory
                if (InventoryManager.Instance != null)
                {
                    bool success = InventoryManager.Instance.AddDecorationByName(reward, "DecorChest", false);
                    if (success)
                    {
                        Debug.Log($"Successfully added {reward} to inventory!");
                        // Show the reward modal popup to the player
                        // This creates a modal window in the center of the screen with a congratulatory message
                        // The modal blocks interaction with the rest of the UI until closed
                        if (rewardModal != null)
                        {
                            // Get the decoration sprite from CityBuilder
                            Sprite decorationSprite = null;
                            if (LifeCraft.Core.CityBuilder.Instance != null)
                            {
                                var buildingData = LifeCraft.Core.CityBuilder.Instance.GetBuildingTypeData(reward);
                                if (buildingData != null && buildingData.buildingSprite != null)
                                {
                                    decorationSprite = buildingData.buildingSprite;
                                }
                            }
                            
                            rewardModal.Show($"You got a {reward}! Congratulations!", decorationSprite);
                        }
                        else
                        {
                            Debug.LogWarning("RewardModal reference is missing! Assign it in the Inspector.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to add {reward} to inventory - inventory might be full!");
                        // TODO: Show error popup to player
                    }
                }
                else
                {
                    Debug.LogError("InventoryManager not found! Cannot add decoration to inventory.");
                }
            }
            else
            {
                Debug.LogWarning("No decorations available in today's All: prize pool.");
            }
        }

        // Call this from the Premium Decor Chest button (for premium only)
        public void OpenPremiumDecorChest()
        {
            // Check if user has premium subscription
            if (SubscriptionManager.Instance == null || !SubscriptionManager.Instance.HasPremiumDecorChestAccess())
            {
                Debug.Log("Free user attempted to access Premium Decor Chest - showing upgrade prompt");
                
                // Show a more user-friendly message with upgrade option
                if (rewardModal != null)
                {
                    // Create a concise message for free users
                    string upgradeMessage = 
                        "🌟 <b>Premium Decor Chest</b>\n\n" +
                        "Unlock exclusive premium decorations with higher chances for rare items!\n\n" +
                        "💎 <b>Upgrade to Premium for $6.99/month</b>\n" +
                        "to access this exclusive feature!";
                    
                    // Use slightly larger size for free users
                    Vector2 customSize = new Vector2(600, 600); // Just a bit larger than default
                    rewardModal.Show(upgradeMessage, premiumChestSprite, customSize);
                    Debug.Log("[DecorChestManager] Showing upgrade modal with custom size for free user");
                }
                else if (UIManager.Instance != null)
                {
                    // Fallback to UIManager notification if rewardModal is not available
                    UIManager.Instance.ShowNotification("Premium Decor Chest requires Premium subscription. Upgrade now!");
                }
                else
                {
                    Debug.LogWarning("Premium Decor Chest requires Premium subscription!");
                }
                return;
            }

            // Show confirmation modal before opening the premium chest
            // The modal asks the player to confirm spending a Balance Ticket to open the premium chest
            if (purchaseConfirmModal != null)
            {
                purchaseConfirmModal.Show(
                    "Open Premium Decor Chest? This will cost 1 Balance Ticket.",
                    () => {
                        // This will be called if the player confirms
                        OpenPremiumDecorChestConfirmed();
                    },
                    premiumChestSprite // Pass the Premium Decor Chest sprite
                );
            }
            else
            {
                Debug.LogWarning("PurchaseConfirmModal reference is missing! Assign it in the Inspector.");
            }
        }

        // This method contains the actual premium chest opening logic after confirmation
        private void OpenPremiumDecorChestConfirmed()
        {
            if (prizePoolManager == null)
            {
                Debug.LogError("PrizePoolManager reference is missing!");
                return;
            }

            // Check if the player has Balance tickets:
            int tickets = ResourceManager.Instance.GetResourceTotal(ResourceManager.ResourceType.BalanceTickets); // Get the total number of Balance Tickets the player has using the ResourceManager. 
            if (tickets < 1)
            {
                //Debug.LogWarning("Not enough Balance Tickets to open a Premium Decor Chest!");
                // TODO: Show UI warning to the player that they need more Balance Tickets to open a Premium Decor chest. (Done!)
                if (rewardModal != null)
                {
                    rewardModal.Show("Not enough Balance Tickets to open a Premium Decor Chest!", balanceTicketIcon); // Show a warning modal if the player doesn't have enough resources. 
                }
                return; 
            }

            // If the player has enough Balance Tickets, spend one ticket:
            ResourceManager.Instance.SpendResources(ResourceManager.ResourceType.BalanceTickets, 1); // Spend one Balance Ticket using the ResourceManager. 

            string reward = prizePoolManager.GetRandomPremiumOnlyReward();
            if (!string.IsNullOrEmpty(reward))
            {
                Debug.Log("Player won from Premium Decor Chest: " + reward);
                // Add the decoration to the player's inventory
                if (InventoryManager.Instance != null)
                {
                    bool success = InventoryManager.Instance.AddDecorationByName(reward, "PremiumDecorChest", true);
                    if (success)
                    {
                        Debug.Log($"Successfully added {reward} to inventory!");
                        // Show the reward modal popup to the player
                        // This creates a modal window in the center of the screen with a congratulatory message
                        // The modal blocks interaction with the rest of the UI until closed
                        if (rewardModal != null)
                        {
                            // Get the decoration sprite from CityBuilder
                            Sprite decorationSprite = null;
                            if (LifeCraft.Core.CityBuilder.Instance != null)
                            {
                                var buildingData = LifeCraft.Core.CityBuilder.Instance.GetBuildingTypeData(reward);
                                if (buildingData != null && buildingData.buildingSprite != null)
                                {
                                    decorationSprite = buildingData.buildingSprite;
                                }
                            }
                            
                            rewardModal.Show($"You got a {reward}! Congratulations!", decorationSprite);
                        }
                        else
                        {
                            Debug.LogWarning("RewardModal reference is missing! Assign it in the Inspector.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to add {reward} to inventory - inventory might be full!");
                        // TODO: Show error popup to player
                    }
                }
                else
                {
                    Debug.LogError("InventoryManager not found! Cannot add decoration to inventory.");
                }
            }
            else
            {
                Debug.LogWarning("No decorations available in today's Premium Only: prize pool.");
            }
        }
    }
} 