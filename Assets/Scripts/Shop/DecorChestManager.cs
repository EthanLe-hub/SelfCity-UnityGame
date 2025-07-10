using UnityEngine;
using LifeCraft.Core; // Reference to the Core namespace for ResourceManager, which deals with resources. 
using LifeCraft.UI; // Add this to use RewardModal

public class DecorChestManager : MonoBehaviour
{
    [Header("References")]
    public PrizePoolManager prizePoolManager; // Add the PrizePoolManager field to the Inspector. This will be used to access the prize pools for decorations. 
    public RewardModal rewardModal; // Reference to the RewardModal popup (assign in Inspector). 

    public PurchaseConfirmModal purchaseConfirmModal; // Reference to the PurchaseConfirmModal (assign in Inspector). 

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
                }
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
            Debug.LogWarning("Not enough Balance tickets to open a Decor Chest!");
            // TODO: Show UI warning to the player that they need more Balance Tickets to open a Decor chest. 
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
                        rewardModal.Show($"You got a {reward}! Congratulations!"); // Only message, no icon
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
        // Show confirmation modal before opening the premium chest
        // The modal asks the player to confirm spending a Balance Ticket to open the premium chest
        if (purchaseConfirmModal != null)
        {
            purchaseConfirmModal.Show(
                "Open Premium Decor Chest? This will cost 1 Balance Ticket.",
                () => {
                    // This will be called if the player confirms
                    OpenPremiumDecorChestConfirmed();
                }
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
            Debug.LogWarning("Not enough Balance Tickets to open a Premium Decor Chest!");
            // TODO: Show UI warning to the player that they need more Balance Tickets to open a Premium Decor chest. 
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
                        rewardModal.Show($"You got a {reward}! Congratulations!"); // Only message, no icon
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