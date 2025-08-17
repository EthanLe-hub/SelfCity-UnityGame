// Code provided by AI Cursor, code manually written by Ethan Le with additional comments and explanations for code comprehension. Language: C# (C Sharp) 

using UnityEngine; // we are using UnityEngine for MonoBehaviour, which is the base class for all scripts in Unity. 
using TMPro; // we are using TMPro for TextMeshPro, which is a text rendering system in Unity that provides advanced text formatting and rendering capabilities. 
using UnityEngine.UI; // we are using UnityEngine.UI for UI components like Button and InputField, which are used to create user interfaces in Unity. 
using LifeCraft.Systems; // Add this to access SubscriptionManager

namespace LifeCraft.UI
{
    public class QuestItemUI : MonoBehaviour // this class represents a single quest item in the UI. 
    {
        [Header("UI References")] // this header is used to group the variables in the inspector for better organization. 
        public TMP_Text questText; // Assign in Inspector: this is the TextMeshPro component that displays the quest text. 
        public Button addToDoButton; // Assign in Inspector: this is the button that adds the quest to the To-Do List. 

        public Image ResourceIcon; // Assign in Inspector: this is the ResourceIcon component that displays the icon for the quest. 

        private string questDescription; // this is the description of the quest, which is used to add the quest to the To-Do List. 
        private ToDoListManager toDoListManager; // this is the To-Do List Manager that handles adding quests to the To-Do List. 
        private bool fromDailyQuest = false; // FIX: Track if this quest is from the Daily Quests list.
        public TMP_Text ResourceAmountText; // Assign in Inspector: this is the TextMeshPro component that displays the resource amount (e.g., "+5").
        private int questRewardAmount = 5; // Stores the dynamic reward amount for this quest.

        /// <summary>
        /// Call this to set up the quest item with its description, region, ToDoListManager reference, origin flag, and dynamic reward amount.
        /// </summary> 
        public void Setup(string description, string region, ToDoListManager manager, bool isFromDailyQuest = true, int amount = 5)
        {
            questDescription = description; // set the quest description to that of the provided description parameter. 
            questText.text = description; // set the quest text to that of the provided description parameter. 
            toDoListManager = manager; // set the To-Do List Manager reference to that of the provided manager parameter. 
            fromDailyQuest = isFromDailyQuest; // FIX: Store the origin flag for use when adding to the To-Do List.
            
            // Debug: Check subscription status
            DebugSubscriptionStatus();
            
            // Calculate the actual reward amount based on premium status
            questRewardAmount = CalculateRewardAmount(amount);

            ResourceIcon.sprite = GetResourceSpriteForRegion(region); // Assign the resource icon based on the region. This method should be defined elsewhere in my code to return the appropriate sprite based on the region of the quest. 
            if (ResourceAmountText != null)
                ResourceAmountText.text = $"+{questRewardAmount}"; // Dynamically display the reward amount next to the icon.

            addToDoButton.onClick.RemoveAllListeners(); // clear any existing listeners on the button to avoid duplicates. 
            addToDoButton.onClick.AddListener(OnAddToDoClicked); // add a new listener that calls OnAddToDoClicked when the button is clicked. 

            Debug.Log($"Setting up quest: {description} (region: {region}, base amount: {amount}, final amount: {questRewardAmount})");
        }

        /// <summary>
        /// Debug method to check subscription status
        /// </summary>
        private void DebugSubscriptionStatus()
        {
            if (SubscriptionManager.Instance != null)
            {
                var (isActive, type, expiry, price) = SubscriptionManager.Instance.GetSubscriptionInfo();
                Debug.Log($"[QuestItemUI] Subscription Status: Active={isActive}, Type={type}, Expiry={expiry}, Price={price}");
                Debug.Log($"[QuestItemUI] HasActiveSubscription={SubscriptionManager.Instance.HasActiveSubscription()}");
                Debug.Log($"[QuestItemUI] HasPremiumDecorChestAccess={SubscriptionManager.Instance.HasPremiumDecorChestAccess()}");
                Debug.Log($"[QuestItemUI] HasPremiumResourcesAccess={SubscriptionManager.Instance.HasPremiumResourcesAccess()}");
                
                // Force validate subscription status
                Debug.Log($"[QuestItemUI] Forcing subscription validation...");
                SubscriptionManager.Instance.ValidateSubscriptionStatus();
                
                // Check again after validation
                var (isActiveAfter, typeAfter, expiryAfter, priceAfter) = SubscriptionManager.Instance.GetSubscriptionInfo();
                Debug.Log($"[QuestItemUI] After validation - Active={isActiveAfter}, Type={typeAfter}, Expiry={expiryAfter}, Price={priceAfter}");
                Debug.Log($"[QuestItemUI] After validation - HasActiveSubscription={SubscriptionManager.Instance.HasActiveSubscription()}");
                Debug.Log($"[QuestItemUI] After validation - HasPremiumDecorChestAccess={SubscriptionManager.Instance.HasPremiumDecorChestAccess()}");
            }
            else
            {
                Debug.LogWarning("[QuestItemUI] SubscriptionManager.Instance is null!");
            }
        }

        /// <summary>
        /// Debug method to force reset subscription status (for testing)
        /// </summary>
        [ContextMenu("Force Reset Subscription Status")]
        public void ForceResetSubscriptionStatus()
        {
            if (SubscriptionManager.Instance != null)
            {
                Debug.Log("[QuestItemUI] Force resetting subscription status...");
                SubscriptionManager.Instance.ResetSubscriptionForTesting();
                DebugSubscriptionStatus();
                
                // Force UI updates after reset
                ForceUIUpdate();
            }
        }

        /// <summary>
        /// Force UI components to update after subscription status change
        /// </summary>
        private void ForceUIUpdate()
        {
            Debug.Log("[QuestItemUI] Starting comprehensive UI update...");
            
            // Force subscription validation first
            if (SubscriptionManager.Instance != null)
            {
                Debug.Log("[QuestItemUI] Forcing subscription validation...");
                SubscriptionManager.Instance.ValidateSubscriptionStatus();
            }
            
            // Find and update UIManager
            var uiManager = FindFirstObjectByType<UIManager>();
            if (uiManager != null)
            {
                Debug.Log("[QuestItemUI] Forcing UIManager to update subscription indicators...");
                uiManager.UpdatePremiumIndicators();
                uiManager.UpdateSubscriptionStatus();
            }
            else
            {
                Debug.LogWarning("[QuestItemUI] UIManager not found!");
            }
            
            // Find and update ProfileManager if it exists
            var profileManager = FindFirstObjectByType<ProfileManager>();
            if (profileManager != null)
            {
                Debug.Log("[QuestItemUI] Forcing ProfileManager to update...");
                profileManager.UpdateProfileDisplay();
            }
            else
            {
                Debug.LogWarning("[QuestItemUI] ProfileManager not found!");
            }
            
            // Force a frame delay to ensure all updates are processed
            StartCoroutine(DelayedUIUpdate());
            
            Debug.Log("[QuestItemUI] Comprehensive UI update triggered - Profile page should now show 'Free' status");
        }

        /// <summary>
        /// Delayed UI update to ensure all changes are processed
        /// </summary>
        private System.Collections.IEnumerator DelayedUIUpdate()
        {
            yield return new WaitForEndOfFrame();
            
            // Force another update after frame processing
            var uiManager = FindFirstObjectByType<UIManager>();
            if (uiManager != null)
            {
                uiManager.UpdatePremiumIndicators();
                uiManager.UpdateSubscriptionStatus();
            }
            
            var profileManager = FindFirstObjectByType<ProfileManager>();
            if (profileManager != null)
            {
                profileManager.UpdateProfileDisplay();
            }
            
            Debug.Log("[QuestItemUI] Delayed UI update completed");
        }

        /// <summary>
        /// Calculate the reward amount based on premium subscription status
        /// Premium users get 8 currency per quest, free users get 5
        /// </summary>
        private int CalculateRewardAmount(int baseAmount)
        {
            // Check if user has premium subscription - use the same check as Premium Decor Chest
            bool hasPremium = SubscriptionManager.Instance != null && SubscriptionManager.Instance.HasPremiumDecorChestAccess();
            Debug.Log($"[QuestItemUI] Premium check: SubscriptionManager.Instance = {SubscriptionManager.Instance != null}, HasPremiumDecorChestAccess = {hasPremium}");
            
            if (hasPremium)
            {
                // Premium users get 8 currency per quest (60% increase from base 5)
                Debug.Log($"[QuestItemUI] Premium user detected - returning 8 currency (base was {baseAmount})");
                return 8;
            }
            else
            {
                // Free users get the base amount (typically 5)
                Debug.Log($"[QuestItemUI] Free user detected - returning base amount {baseAmount}");
                return baseAmount;
            }
        }

        private Sprite GetResourceSpriteForRegion(string region)
        {
            // Assign your sprites in the Inspector or load them dynamically based on the region from Resources. 
            switch (region)
            {
                case "Health Harbor": return healthHarborSprite; // Assign the sprite for Health Harbor. 
                case "Mind Palace": return mindPalaceSprite; // Assign the sprite for Mind Palace. 
                case "Social Square": return socialSquareSprite; // Assign the sprite for Social Square. 
                case "Creative Commons": return creativeCommonsSprite; // Assign the sprite for Creative Commons. 
                default: return defaultSprite; // Assign a default sprite if the region does not match any known regions. 
            }
        }

        // Assign these in the Inspector or load them dynamically from Resources. 
        public Sprite healthHarborSprite; // Assign in Inspector: sprite for Health Harbor region. 
        public Sprite mindPalaceSprite; // Assign in Inspector: sprite for Mind Palace region. 
        public Sprite socialSquareSprite; // Assign in Inspector: sprite for Social Square region. 
        public Sprite creativeCommonsSprite; // Assign in Inspector: sprite for Creative Commons region. 
        public Sprite defaultSprite; // Assign in Inspector: default sprite for unknown regions. 

        private void OnAddToDoClicked() // this method is called when the "Add To-Do" button is clicked. 
        {
            Debug.Log($"adding to-do clicked for: {questDescription} (reward: {questRewardAmount})");
            if (toDoListManager != null) // check if the To-Do List Manager is assigned. 
            {
                // FIX: Only flag as daily quest if this quest is actually from the Daily Quests list.
                // TODO: Use questRewardAmount here to actually reward the player dynamically (e.g., toDoListManager.AddToDo(questDescription, fromDailyQuest, questRewardAmount);)
                toDoListManager.AddToDo(questDescription, fromDailyQuest, questRewardAmount); // Use the correct flag and reward amount. Update this if your ToDoListManager supports dynamic rewards.
            }
            else
            {
                Debug.LogWarning("ToDoListManager is null!");
            }
        }
    }
}