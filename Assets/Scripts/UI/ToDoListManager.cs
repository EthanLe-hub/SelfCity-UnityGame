// Code provided by AI Cursor, code manually written by Ethan Le with additional comments and explanations for code comprehension. Language: C# (C Sharp)

using UnityEngine; // we are using UnityEngine for MonoBehaviour, which is the base class for all scripts in Unity. 
using TMPro; // we are using TMPro for TextMeshPro, which is a text rendering system in Unity that provides advanced text formatting and rendering capabilities. 
using UnityEngine.UI; // we are using UnityEngine.UI for UI components like Button and InputField, which are used to create user interfaces in Unity. 
using LifeCraft.UI; // we are using LifeCraft.UI for the ResourceBarManager, which manages the resource bar UI and functionality. 
using LifeCraft.Systems; // we are using LifeCraft.Systems for the QuestManager, which manages the quests in the game. 
using LifeCraft.Core; // we are using LifeCraft.Core for the ResourceManager, which manages the player's resources.

public class ToDoListManager : MonoBehaviour // this class manages the to-do list UI and functionality 
{
    [Header("Assign in Inspector")] // this header is used to group the variables in the inspector for better organization. 
    public Transform toDoListContainer; // this is the parent object (it holds all the ToDoListItems) that holds all the to-do list items. 
    public GameObject toDoItemPrefab; // this is the prefab [object] for all the to-do list items, which is a template for creating new to-do list items. 

    public ResourceBarManager resourceBarManager; // this is a reference to the ResourceBarManager script, which manages the resource bar UI and functionality (this line adds this field into the Inspector). 

    public DailyQuestsButtonHandler dailyQuestsButtonHandler; // this line adds the field into the Inspector, which is a reference to the DailyQuestsButtonHandler script that handles the daily quests button functionality. 
    /// <summary>
    /// Call this method to add a quest/task to the To-Do List. 
    /// </summary> 
    /// <param name="questText"> The text of the quest/task to be added to the To-Do List. </param>
    public void AddToDo(string questText, bool fromDailyQuest = false, int rewardAmount = 5) // this method adds a new to-do item to the list, checks if it is from Daily Quests, and stores the dynamic reward amount.
    {
        if (toDoListContainer == null || toDoItemPrefab == null) // check if the container or prefab is not assigned.
        {
            Debug.LogError("ToDoListManager: Container or Prefab not assigned!"); // Log an error if they are not assigned. 
            return; // exit the method if the container or prefab is not assigned (we cannot add a new item if the container or prefab do not exist). 
        }

        // The following block of code (additional feature written by Ethan Le) checks if the questText already exists in the To-Do List and thus prevents duplicates. 
        bool alreadyExists = false; // this variable is used to check if the questText already exists in the To-Do List. 

        foreach (Transform toDoQuest in toDoListContainer) // iterate through each existing to-do item in the container. 
        {
            TMP_Text tmpText = toDoQuest.GetComponentInChildren<TMP_Text>(); // 1st way: try to get the TextMeshPro component from the existing to-do item. 
            if (tmpText != null && tmpText.text == questText) // if the TextMeshPro component (an existing to-do item in the container) exists and matches the questText (new to-do item to be added), then we have a duplicate. 
            {
                alreadyExists = true; // set the alreadyExists variable to true, indicating that the questText already exists in the To-Do List. 
                break; // exit the loop since we found a duplicate. 
            }

            Text uiText = toDoQuest.GetComponentInChildren<Text>(); // 2nd way: if the TextMeshPro component is NOT found, try to get the Unity UI Text component instead. 
            if (uiText != null && uiText.text == questText) // if the Unity UI Text component (an existing to-do item in the container) exists and matches the questText (new to-do item to be added), then we have a duplicate. 
            {
                alreadyExists = true; // set the alreadyExists variable to true, indicating that the questText already exists in the To-Do List. 
                break; // exit the loop since we have found a duplicate. 
            }
        }

        if (toDoListContainer.childCount <= 10 && !alreadyExists) // My own additional check statement (written by Ethan Le): this checks if the number of children in the container is less than or equal to 10, and if the new quest does not exist in the To-Do List yet. 
        {
            // Instantiate the To-Do item prefab (toDoItemPrefab) as a child of the container (toDoListContainer). 
            GameObject newItem = Instantiate(toDoItemPrefab, toDoListContainer); // this creates a new instance of the to-do item prefab and sets its parent to the to-do list container. 

            // Set the metadata for the new item (if it is from Daily Quests, then we set the isFromDailyQuest variable to true and set the dailyQuestText to the questText). 
            ToDoItemMeta meta = newItem.GetComponent<ToDoItemMeta>(); // Get the ToDoItemMeta component from the new item. 
            if (meta != null) // check if the ToDoItemMeta component is found. 
            {
                meta.isFromDailyQuest = fromDailyQuest; // set the isFromDailyQuest variable to the fromDailyQuest parameter (which is a boolean that indicates if the new item is from Daily Quests). 
                meta.dailyQuestText = questText; // set the dailyQuestText variable to the questText parameter (which is the text of the quest/task to be added from one of the 4 regions or the Daily Quests list). 
                meta.rewardAmount = rewardAmount; // set the dynamic reward amount for this to-do item.
                // Assign the LabelText and RewardAmountText fields in the Inspector to the correct TMP_Text objects on your prefab.
                // Set the quest label and reward amount text in the UI if the fields are assigned.
                if (meta.LabelText != null)
                    meta.LabelText.text = questText; // Set the quest/task label
                if (meta.RewardAmountText != null)
                    meta.RewardAmountText.text = $"+{rewardAmount}"; // Set the reward amount
                // Assign the ResourceIcon field in the Inspector to the correct Image component on your prefab. 
                if (meta.ResourceIcon != null)
                    meta.ResourceIcon.sprite = GetResourceSpriteForRegion(questText); // Set the resource icon based on the region. 
            }

            // Try to set the label text of the new item (supports both TextMeshPro and Unity's UI Text). 
            TMP_Text tmpLabel = newItem.GetComponentInChildren<TMP_Text>(); // 1st way: this gets the TextMeshPro component from the new item. 
            if (tmpLabel != null) // check if the TextMeshPro component is found. 
            {
                tmpLabel.text = questText; // set the text of the TextMeshPro component to the quest text (based on the questText parameter, which is the text of the quest/task to be added from one of the 4 regions or the Daily Quests list).
                return; // exit the method after setting the text. 
            }

            Text uiLabel = newItem.GetComponentInChildren<Text>(); // 2nd way: if the TextMeshPro component is NOT found, try to get the Unity UI Text component instead. 
            if (uiLabel != null) // check if the Unity UI Text component is found. 
            {
                uiLabel.text = questText; // set the text of the Unity UI Text component to the quest text (based on the questText parameter, which is the text of the quest/task to be added from one of the 4 regions or the Daily Quests list). 
                return; // exit the method after setting the text. 
            }

            Debug.LogWarning("ToDoListManager: No Text or TMP_Text component found in ToDoItemPrefab."); // Log a warning if neither component is found, indicating that the prefab does not have a text component to display the quest text. 

        }

        else if (toDoListContainer.childCount > 10)// if the number of children in the container is greater than 10, then we cannot add a new item.
        {
            Debug.LogWarning("ToDoListManager: Exceeded 10 items!"); // Log a warning if the number of children in the container is greater than 10, indicating that we cannot add a new item. 
        }

        else if (alreadyExists) // if the questText already exists in the To-Do List, then we cannot add a new item. 
        {
            Debug.LogWarning("ToDoListManager: Item already exists!"); // Log a warning if the questText already exists in the To-Do List, indicating that we cannot add a new item. 
        }
    }

    // Helper method to get the correct resource sprite for the region. 
    private Sprite GetResourceSpriteForRegion(string questText)
    {
        string region = GetRegionFromQuest(questText); // Extract the region name from the quest text. 
        // Assign your sprites in the Inspector or load them dynamically from Resources. 
        switch (region)
        {
            case "Health Harbor": return healthHarborSprite; // Return the Health Harbor sprite. 
            case "Mind Palace": return mindPalaceSprite; // Return the Mind Palace sprite. 
            case "Social Square": return socialSquareSprite; // Return the Social Square sprite. 
            case "Creative Commons": return creativeCommonsSprite; // Return the Creative Commons sprite. 
            default: return defaultSprite; // Return the default sprite. 
        }
    }

    // Add these Sprite fields to your ToDoListManager GameObject and assign them in the Inspector. 
    public Sprite healthHarborSprite; // Assign in Inspector: the sprite for the Health Harbor region. 
    public Sprite mindPalaceSprite; // Assign in Inspector: the sprite for the Mind Palace region. 
    public Sprite socialSquareSprite; // Assign in Inspector: the sprite for the Social Square region. 
    public Sprite creativeCommonsSprite; // Assign in Inspector: the sprite for the Creative Commons region. 
    public Sprite defaultSprite; // Assign in Inspector: the default sprite for regions that don't have a specific sprite assigned. 

    /// <summary>
    /// Removes all checked (completed) tasks and gives a currency reward for each.
    /// Call this from the Complete Task(s) button.
    /// </summary>
    public void CompleteSelectedTasks()
    {
        for (int i = toDoListContainer.childCount - 1; i >= 0; i--)
        {
            Transform item = toDoListContainer.GetChild(i); // Get the child item at index i from the to-do list container, starting from the last item to avoid index issues when removing items (remember how arrays work if you remove an item). 
            Toggle toggle = item.GetComponentInChildren<Toggle>(); // Get the Toggle component from the item to check if it is marked as selected. 
            if (toggle != null && toggle.isOn) // Check if the item is checkmarked as selected. 
            {
                // Check if this item is from Daily Quests and remove from Daily Quests modal if so
                ToDoItemMeta meta = item.GetComponent<ToDoItemMeta>(); // Get the ToDoItemMeta component from the item.
                if (meta != null)
                {
                    Debug.Log("[CompleteSelectedTasks] meta.isFromDailyQuest: " + meta.isFromDailyQuest + ", meta.dailyQuestText: " + meta.dailyQuestText);
                }
                if (meta != null && meta.isFromDailyQuest && dailyQuestsButtonHandler != null) // Check if the item is from a daily quest and the handler exists.
                {
                    Debug.Log("[CompleteSelectedTasks] Calling RemoveQuestByText with: " + meta.dailyQuestText);
                    dailyQuestsButtonHandler.RemoveQuestByText(meta.dailyQuestText); // Remove from Daily Quests modal.
                }

                string questText = GetQuestTextFromItem(item); // Get the quest text from the item to use for the reward. 
                int rewardAmount = 5;
                ToDoItemMeta metaReward = item.GetComponent<ToDoItemMeta>();
                if (metaReward != null)
                    rewardAmount = metaReward.rewardAmount;
                GiveCurrencyReward(questText, rewardAmount); // Give the dynamic reward amount for this quest.
                Destroy(item.gameObject); // Destroy the item after giving the reward. 
            }
        }
        // FIX: Only reward a Balance Ticket if daily quests have been generated and not already rewarded for the current set.
        StartCoroutine(CheckAndRewardDailyQuestCompletion());
    }

    // FIX: Coroutine to check and reward after all removals, to prevent abuse and ensure correct logic.
    private System.Collections.IEnumerator CheckAndRewardDailyQuestCompletion()
    {
        yield return null; // Wait for end of frame so all removals are processed
        if (dailyQuestsButtonHandler != null &&
            dailyQuestsButtonHandler.dailyQuestsGenerated &&
            !dailyQuestsButtonHandler.balanceTicketRewarded &&
            dailyQuestsButtonHandler.questListContainer.childCount == 0)
        {
            if (dailyQuestsButtonHandler.resourceBarManager != null)
            {
                //dailyQuestsButtonHandler.resourceBarManager.AddBalanceTickets(1);
                ResourceManager.Instance.AddResources(ResourceManager.ResourceType.BalanceTickets, 1); // Add 1 Balance Ticket to the player's resources using the ResourceManager. 
                dailyQuestsButtonHandler.balanceTicketRewarded = true; // Mark as rewarded for this set
                Debug.Log("All daily quests completed! Rewarding 1 Balance Ticket.");
            }
        }
    }

    /// <summary>
    /// Removes all checked (selected) tasks without giving a reward.
    /// Call this from the Delete Task(s) button.
    /// </summary>
    public void DeleteSelectedTasks()
    {
        for (int i = toDoListContainer.childCount - 1; i >= 0; i--)
        {
            Transform item = toDoListContainer.GetChild(i); // Get the child item at index i from the to-do list container, starting from the last item to avoid index issues when removing items (remember how arrays work if you remove an item). 
            Toggle toggle = item.GetComponentInChildren<Toggle>(); // Get the Toggle component from the item to check if it is marked as selected. 
            if (toggle != null && toggle.isOn) // Check if the item is checkmarked as selected. 
            {
                Destroy(item.gameObject); // Destroy the item without giving a reward. 
            }
        }
    }

    /// <summary>
    /// Helper to get the quest/task text from a to-do item.
    /// </summary>
    private string GetQuestTextFromItem(Transform item)
    {
        TMP_Text tmp = item.GetComponentInChildren<TMP_Text>();
        if (tmp != null) return tmp.text; // If the TextMeshPro component is found, return its text. 
        Text uiText = item.GetComponentInChildren<Text>();
        if (uiText != null) return uiText.text; // If the Unity UI Text component is found, return its text. 
        return string.Empty;
    }

    /// <summary>
    /// Placeholder for giving a currency reward for a completed quest/task.
    /// </summary>
    private void GiveCurrencyReward(string questText, int rewardAmount = 5)
    {
        string region = GetRegionFromQuest(questText);
        if (!string.IsNullOrEmpty(region))
        {
            switch (region)
            {
                case "Health Harbor":
                    ResourceManager.Instance.AddResources(ResourceManager.ResourceType.EnergyCrystals, rewardAmount);
                    break;
                case "Mind Palace":
                    ResourceManager.Instance.AddResources(ResourceManager.ResourceType.WisdomOrbs, rewardAmount);
                    break;
                case "Social Square":
                    ResourceManager.Instance.AddResources(ResourceManager.ResourceType.HeartTokens, rewardAmount);
                    break;
                case "Creative Commons":
                    ResourceManager.Instance.AddResources(ResourceManager.ResourceType.CreativitySparks, rewardAmount);
                    break;
            }
        }
        Debug.Log($"Reward player for completing: {questText} (+{rewardAmount})");
    }

    private string GetRegionFromQuest(string questText)
    {
        // Try to extract the region from the text in parentheses at the end (we use the region text at the end of the quest text that is in parentheses): 
        int start = questText.LastIndexOf('(');
        int end = questText.LastIndexOf(')');
        if (start != -1 && end != -1 && end > start)
        {
            return questText.Substring(start + 1, end - start - 1).Trim();
        }
        return null;
    }
} 