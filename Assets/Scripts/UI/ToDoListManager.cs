// Code provided by AI Cursor, code manually written by Ethan Le with additional comments and explanations for code comprehension. Language: C# (C Sharp)

using UnityEngine; // we are using UnityEngine for MonoBehaviour, which is the base class for all scripts in Unity. 
using TMPro; // we are using TMPro for TextMeshPro, which is a text rendering system in Unity that provides advanced text formatting and rendering capabilities. 
using UnityEngine.UI; // we are using UnityEngine.UI for UI components like Button and InputField, which are used to create user interfaces in Unity. 
using LifeCraft.UI; // we are using LifeCraft.UI for the ResourceBarManager, which manages the resource bar UI and functionality. 
using LifeCraft.Systems; // we are using LifeCraft.Systems for the QuestManager, which manages the quests in the game. 
using LifeCraft.Core; // we are using LifeCraft.Core for the ResourceManager, which manages the player's resources.
using System.Collections.Generic; // For List<string> in save/load functionality

public enum QuestDifficulty
{
    Easy,      // 5 EXP
    Medium,    // 10 EXP  
    Hard,      // 15 EXP
    Expert     // 20 EXP
}

[System.Serializable]
public class ToDoItemSaveData
{
    public string questText;
    public bool isFromDailyQuest;
    public string dailyQuestText;
    public int rewardAmount;
}

[System.Serializable]
public class ToDoListSaveWrapper
{
    public List<ToDoItemSaveData> items = new List<ToDoItemSaveData>();
}

public class ToDoListManager : MonoBehaviour // this class manages the to-do list UI and functionality 
{
    [Header("Assign in Inspector")] // this header is used to group the variables in the inspector for better organization. 
    public Transform toDoListContainer; // this is the parent object (it holds all the ToDoListItems) that holds all the to-do list items. 
    public GameObject toDoItemPrefab; // this is the prefab [object] for all the to-do list items, which is a template for creating new to-do list items. 

    public ResourceBarManager resourceBarManager; // this is a reference to the ResourceBarManager script, which manages the resource bar UI and functionality (this line adds this field into the Inspector). 

    public DailyQuestsButtonHandler dailyQuestsButtonHandler; // this line adds the field into the Inspector, which is a reference to the DailyQuestsButtonHandler script that handles the daily quests button functionality. 

    public RewardModal rewardModal; // This line adds the field into the Inspector, which is a reference to the RewardModal script that handles the reward modal popup functionality. 

    /// <summary>
    /// Save the current To-Do List items to PlayerPrefs
    /// </summary>
    public void SaveToDoList()
    {
        try
        {
            var saveData = new ToDoListSaveWrapper();
            
            // Collect all current To-Do items
            foreach (Transform child in toDoListContainer)
            {
                var meta = child.GetComponent<ToDoItemMeta>();
                if (meta != null)
                {
                    var itemData = new ToDoItemSaveData
                    {
                        questText = meta.dailyQuestText,
                        isFromDailyQuest = meta.isFromDailyQuest,
                        dailyQuestText = meta.dailyQuestText,
                        rewardAmount = meta.rewardAmount
                    };
                    saveData.items.Add(itemData);
                }
            }
            
            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString("ToDoListData", json);
            PlayerPrefs.Save();
            Debug.Log($"ToDoList saved: {saveData.items.Count} items");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save ToDoList: {e.Message}");
        }
    }

    /// <summary>
    /// Load the To-Do List items from PlayerPrefs
    /// </summary>
    public void LoadToDoList()
    {
        try
        {
            if (PlayerPrefs.HasKey("ToDoListData"))
            {
                string json = PlayerPrefs.GetString("ToDoListData");
                var saveData = JsonUtility.FromJson<ToDoListSaveWrapper>(json);
                
                if (saveData != null && saveData.items != null)
                {
                    // Clear existing items
                    foreach (Transform child in toDoListContainer)
                    {
                        Destroy(child.gameObject);
                    }
                    
                    // Restore saved items
                    foreach (var itemData in saveData.items)
                    {
                        AddToDo(itemData.questText, itemData.isFromDailyQuest, itemData.rewardAmount);
                    }
                    
                    Debug.Log($"ToDoList loaded: {saveData.items.Count} items");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load ToDoList: {e.Message}");
        }
    }

    /// <summary>
    /// Clear all To-Do List items and save the empty state
    /// </summary>
    public void ClearToDoList()
    {
        foreach (Transform child in toDoListContainer)
        {
            Destroy(child.gameObject);
        }
        SaveToDoList();
    }

    /// <summary>
    /// Call this method to add a quest/task to the To-Do List. 
    /// </summary> 
    /// <param name="questText"> The text of the quest/task to be added to the To-Do List. </param>
    public void AddToDo(string questText, bool fromDailyQuest = false, int rewardAmount = 5) // this method adds a new to-do item to the list, checks if it is from Daily Quests, and stores the dynamic reward amount.
    {
        Debug.Log($"[ToDoListManager] AddToDo called: quest='{questText}', fromDailyQuest={fromDailyQuest}, rewardAmount={rewardAmount}");
        
        Debug.Log($"ToDoListManager: Attempting to add quest: '{questText}' (Container has {toDoListContainer?.childCount ?? 0} items)");
        
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

        if (toDoListContainer.childCount <= 20 && !alreadyExists) // Increased limit to 20 to accommodate construction skip quests 
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
                
                Debug.Log($"[ToDoListManager] Stored rewardAmount={meta.rewardAmount} in ToDoItemMeta for quest: '{questText}'");
                
                // Assign the LabelText and RewardAmountText fields in the Inspector to the correct TMP_Text objects on your prefab.
                // Set the quest label and reward amount text in the UI if the fields are assigned.
                if (meta.LabelText != null)
                    meta.LabelText.text = questText; // Set the quest/task label
                
                // CRITICAL FIX: Check if this is a construction quest (Skip Quest)
                // Construction quests should NOT show reward amounts or currency sprites
                bool isConstructionQuest = IsConstructionQuest(questText);
                
                if (meta.RewardAmountText != null)
                {
                    if (isConstructionQuest)
                    {
                        // Hide reward text for construction quests
                        meta.RewardAmountText.text = "";
                        meta.RewardAmountText.gameObject.SetActive(false);
                    }
                    else
                    {
                        // Show reward text for regular quests
                        meta.RewardAmountText.text = $"+{rewardAmount}";
                        meta.RewardAmountText.gameObject.SetActive(true);
                    }
                }
                
                                // Assign the ResourceIcon field in the Inspector to the correct Image component on your prefab.
                if (meta.ResourceIcon != null)
                {
                    if (isConstructionQuest)
                    {
                        // Hide currency sprite for construction quests
                        meta.ResourceIcon.gameObject.SetActive(false);
                    }
                    else
                    {
                        // Show currency sprite for regular quests
                        meta.ResourceIcon.gameObject.SetActive(true);
                    }
                }
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

                else if (toDoListContainer.childCount > 20)// if the number of children in the container is greater than 20, then we cannot add a new item.
        {
            Debug.LogWarning("ToDoListManager: Exceeded 20 items!"); // Log a warning if the number of children in the container is greater than 20, indicating that we cannot add a new item.
        }

        else if (alreadyExists) // if the questText already exists in the To-Do List, then we cannot add a new item. 
        {
            Debug.LogWarning("ToDoListManager: Item already exists!"); // Log a warning if the questText already exists in the To-Do List, indicating that we cannot add a new item. 
        }
    }

    /// <summary>
    /// Remove a quest/task from the To-Do List by text
    /// </summary>
    /// <param name="questText">The text of the quest/task to remove</param>
    public void RemoveToDo(string questText)
    {
        if (toDoListContainer == null) return;
        
        // Find and remove the item with matching text
        for (int i = toDoListContainer.childCount - 1; i >= 0; i--)
        {
            Transform item = toDoListContainer.GetChild(i);
            
            // Check TextMeshPro first
            TMP_Text tmpText = item.GetComponentInChildren<TMP_Text>();
            if (tmpText != null && tmpText.text == questText)
            {
                Destroy(item.gameObject);
                Debug.Log($"Removed quest from To-Do List: {questText}");
                return;
            }
            
            // Check Unity UI Text
            Text uiText = item.GetComponentInChildren<Text>();
            if (uiText != null && uiText.text == questText)
            {
                Destroy(item.gameObject);
                Debug.Log($"Removed quest from To-Do List: {questText}");
                return;
            }
        }
        
        Debug.LogWarning($"Quest not found in To-Do List: {questText}");
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
    public Sprite balanceTicketSprite; // Assign in Inspector: the sprite for the Balance Ticket reward. 
    public Sprite defaultSprite; // Assign in Inspector: the default sprite for regions that don't have a specific sprite assigned. 

    /// <summary>
    /// Enhanced difficulty detection that covers ALL quests in QuestManager
    /// </summary>
    private QuestDifficulty DetermineQuestDifficulty(string questText)
    {
        string lowerText = questText.ToLower();
        
        // EASY: Short duration, simple actions, quick tasks
        if (lowerText.Contains("5 minute") || lowerText.Contains("2 minute") || 
            lowerText.Contains("quick") || lowerText.Contains("simple") || 
            lowerText.Contains("small") || lowerText.Contains("just") || 
            lowerText.Contains("even if") || lowerText.Contains("30 second") ||
            lowerText.Contains("three deep breath") || lowerText.Contains("1 minute") ||
            lowerText.Contains("try a new") || lowerText.Contains("replace") ||
            lowerText.Contains("swap") || lowerText.Contains("take a") ||
            lowerText.Contains("do a quick") || lowerText.Contains("spend 10 minute") ||
            lowerText.Contains("do 10") || lowerText.Contains("do 20") ||
            lowerText.Contains("write down three") || lowerText.Contains("meditate for 5") ||
            lowerText.Contains("practice mindful") || lowerText.Contains("draw or doodle") ||
            lowerText.Contains("list five") || lowerText.Contains("spend 5 minute") ||
            lowerText.Contains("set a small goal") || lowerText.Contains("write down a positive") ||
            lowerText.Contains("take a break") || lowerText.Contains("listen to instrumental") ||
            lowerText.Contains("send a message") || lowerText.Contains("compliment") ||
            lowerText.Contains("share a funny") || lowerText.Contains("ask someone how") ||
            lowerText.Contains("smile at") || lowerText.Contains("tell a joke") ||
            lowerText.Contains("draw or sketch") || lowerText.Contains("take a photo") ||
            lowerText.Contains("write a haiku") || lowerText.Contains("listen to a new genre") ||
            lowerText.Contains("try a new dance") || lowerText.Contains("create a doodle") ||
            lowerText.Contains("try a new creative"))
            return QuestDifficulty.Easy;
        
        // MEDIUM: Moderate duration, practice/learn actions
        if (lowerText.Contains("10 minute") || lowerText.Contains("15 minute") ||
            lowerText.Contains("try") || lowerText.Contains("practice") ||
            lowerText.Contains("listen") || lowerText.Contains("write down") ||
            lowerText.Contains("read a") || lowerText.Contains("watch a") ||
            lowerText.Contains("learn") || lowerText.Contains("organize") ||
            lowerText.Contains("prepare") || lowerText.Contains("make a") ||
            lowerText.Contains("create") || lowerText.Contains("design") ||
            lowerText.Contains("paint") || lowerText.Contains("color") ||
            lowerText.Contains("call or video") || lowerText.Contains("write a thank-you") ||
            lowerText.Contains("offer to help") || lowerText.Contains("share something") ||
            lowerText.Contains("invite someone") || lowerText.Contains("leave a positive") ||
            lowerText.Contains("tell someone why") || lowerText.Contains("share a favorite") ||
            lowerText.Contains("recommend") || lowerText.Contains("ask a family member") ||
            lowerText.Contains("play an online") || lowerText.Contains("share a photo") ||
            lowerText.Contains("thank someone") || lowerText.Contains("offer words") ||
            lowerText.Contains("ask someone about") || lowerText.Contains("share a motivational") ||
            lowerText.Contains("make a new connection") || lowerText.Contains("check in on") ||
            lowerText.Contains("try a new recipe") || lowerText.Contains("make a collage") ||
            lowerText.Contains("write a song") || lowerText.Contains("try origami") ||
            lowerText.Contains("take a creative photo") || lowerText.Contains("make a vision board") ||
            lowerText.Contains("try a new art app") || lowerText.Contains("write a letter in") ||
            lowerText.Contains("make up a new recipe") || lowerText.Contains("record a short video") ||
            lowerText.Contains("build something"))
            return QuestDifficulty.Medium;
        
        // HARD: Longer duration, complete/organize actions
        if (lowerText.Contains("20 minute") || lowerText.Contains("30 minute") ||
            lowerText.Contains("complete") || lowerText.Contains("go for a") ||
            lowerText.Contains("drink 8 glass") || lowerText.Contains("get at least 30 minute") ||
            lowerText.Contains("track your") || lowerText.Contains("do a set of") ||
            lowerText.Contains("take the stairs") || lowerText.Contains("prepare a balanced") ||
            lowerText.Contains("go to bed without") || lowerText.Contains("write down a 'worry list'") ||
            lowerText.Contains("unfollow social media") || lowerText.Contains("listen to a podcast") ||
            lowerText.Contains("spend 15 minute") || lowerText.Contains("read a chapter") ||
            lowerText.Contains("try a new journaling") || lowerText.Contains("do a crossword") ||
            lowerText.Contains("write down your goal") || lowerText.Contains("spend 5 minute visualizing") ||
            lowerText.Contains("give a genuine") || lowerText.Contains("schedule a call") ||
            lowerText.Contains("write a thank-you note") || lowerText.Contains("do a small favor") ||
            lowerText.Contains("share something positive") || lowerText.Contains("invite a friend") ||
            lowerText.Contains("ask someone about their day") || lowerText.Contains("reconnect with") ||
            lowerText.Contains("help someone") || lowerText.Contains("share a story") ||
            lowerText.Contains("write a short") || lowerText.Contains("create a new music") ||
            lowerText.Contains("try a new hairstyle") || lowerText.Contains("rearrange a small") ||
            lowerText.Contains("draw a picture") || lowerText.Contains("write a poem") ||
            lowerText.Contains("make a simple craft") || lowerText.Contains("design a new logo"))
            return QuestDifficulty.Hard;
        
        // EXPERT: Week-long, planning, tracking, substantial activities
        if (lowerText.Contains("week") || lowerText.Contains("schedule") ||
            lowerText.Contains("plan") || lowerText.Contains("track") ||
            lowerText.Contains("goal") || lowerText.Contains("research") ||
            lowerText.Contains("write down your sleep schedule") || lowerText.Contains("go to bed 30 minute") ||
            lowerText.Contains("take a power nap") || lowerText.Contains("write a letter to your future") ||
            lowerText.Contains("research a topic") || lowerText.Contains("write a short story") ||
            lowerText.Contains("decorate your workspace") || lowerText.Contains("paint or color a scene"))
            return QuestDifficulty.Expert;
        
        return QuestDifficulty.Medium; // Default fallback
    }

    /// <summary>
    /// New method: Calculates EXP using hybrid difficulty system
    /// </summary>
    private int CalculateQuestEXP(string questText, bool isDailyQuest, bool isCustomQuest)
    {
        QuestDifficulty difficulty = DetermineQuestDifficulty(questText);
        
        // Base EXP by difficulty
        int baseEXP = difficulty switch
        {
            QuestDifficulty.Easy => 5,
            QuestDifficulty.Medium => 10,
            QuestDifficulty.Hard => 15,
            QuestDifficulty.Expert => 20,
            _ => 10
        };
        
        // Bonus for quest type
        if (isDailyQuest) baseEXP += 2;      // Daily quests get +2 bonus
        if (isCustomQuest) baseEXP += 1;     // Custom quests get +1 bonus
        
        Debug.Log($"Quest: '{questText}' | Difficulty: {difficulty} | Base EXP: {baseEXP - (isDailyQuest ? 2 : 0) - (isCustomQuest ? 1 : 0)} | Type Bonus: +{(isDailyQuest ? 2 : 0) + (isCustomQuest ? 1 : 0)} | Total: {baseEXP}");
        
        return baseEXP;
    }

    /// <summary>
    /// Checks if a quest is a construction quest (Skip Quest)
    /// </summary>
    private bool IsConstructionQuest(string questText)
    {
        // Check if this quest exists in any active construction project
        if (ConstructionManager.Instance != null)
        {
            var allProjects = ConstructionManager.Instance.GetAllProjectKeys();
            foreach (string projectKey in allProjects)
            {
                // Parse the project key to get building name and position
                string[] parts = projectKey.Split('_');
                if (parts.Length >= 4)
                {
                    string buildingName = parts[0];
                    Vector3Int gridPosition = new Vector3Int(
                        int.Parse(parts[1]),
                        int.Parse(parts[2]),
                        int.Parse(parts[3])
                    );
                    
                    // Check if this project has the quest
                    ConstructionProject project = ConstructionManager.Instance.GetProject(buildingName, gridPosition);
                    if (project != null && project.originalQuestTexts.Contains(questText))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if a quest is from the region quest lists
    /// </summary>
    private bool IsRegionQuest(string questText)
    {
        // Check if it matches any of the standard region quest patterns from QuestManager
        string[] regionQuestPatterns = {
            "Go for a 20-minute brisk walk",
            "Try a new healthy recipe",
            "Do 15 minutes of stretching",
            "Drink 8 glasses of water",
            "Get at least 30 minutes of sunlight",
            "Track your sleep hours",
            "Do a set of 10 push-ups",
            "Take the stairs instead",
            "Prepare a balanced breakfast",
            "Go to bed without any screens",
            "Practice 10 minutes of focused breathing",
            "Write down a 'worry list'",
            "Unfollow social media accounts",
            "Listen to a podcast about mental wellness",
            "Spend 15 minutes in a quiet space",
            "Read a chapter from a self-help book",
            "Try a new journaling prompt",
            "Do a crossword or sudoku puzzle",
            "Write down your goals for the week",
            "Spend 5 minutes visualizing",
            "Give a genuine compliment",
            "Schedule a call or video chat",
            "Write a thank-you note to someone",
            "Do a small favor for a family member",
            "Share something positive or interesting",
            "Invite a friend to join you",
            "Ask someone about their day",
            "Reconnect with an old friend",
            "Help someone with a task",
            "Share a story from your childhood",
            "Take a photo of something interesting",
            "Write a short, one-paragraph story",
            "Create a new music playlist",
            "Try a new hairstyle or accessory",
            "Rearrange a small part of your room",
            "Draw a picture of your favorite animal",
            "Write a poem about your day",
            "Make a simple craft using recycled materials",
            "Design a new logo for a fictional company",
            "Paint or color a scene from your favorite movie"
        };
        
        foreach (string pattern in regionQuestPatterns)
        {
            if (questText.Contains(pattern))
                return true;
        }
        
        return false;
    }

    /// <summary>
    /// Show a summary of completed tasks and total EXP gained
    /// </summary>
    private void ShowCompletionSummary(int completedTasks, int totalEXP)
    {
        // Show a brief summary in the console for now
        Debug.Log($"🎉 Completed {completedTasks} task(s) for a total of {totalEXP} EXP!");
        
        // TODO: In the future, this could show a more elaborate UI popup
        // For now, we'll use the existing reward modal if available
        if (rewardModal != null)
        {
            string message = completedTasks == 1 
                ? $"Task completed! You gained {totalEXP} EXP!" 
                : $"Completed {completedTasks} tasks! You gained {totalEXP} EXP!";
            
            rewardModal.Show(message, defaultSprite);
        }
    }

    /// <summary>
    /// Removes all checked (completed) tasks and gives a currency reward for each.
    /// Call this from the Complete Task(s) button.
    /// </summary>
    public void CompleteSelectedTasks()
    {
        int totalEXP = 0;
        int completedTasks = 0;
        
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
                
                // Check if this is a construction skip quest
                // Query ConstructionManager directly instead of looking for BuildingConstructionTimer components
                bool isConstructionQuest = false;
                
                Debug.Log($"[CompleteSelectedTasks] Checking quest: '{questText}' against ConstructionManager");
                
                if (ConstructionManager.Instance != null)
                {
                    // Get all active projects from ConstructionManager
                    var allProjects = ConstructionManager.Instance.GetAllProjectKeys();
                    Debug.Log($"[CompleteSelectedTasks] Found {allProjects.Count} active construction projects");
                    
                    foreach (string projectKey in allProjects)
                    {
                        // Parse the project key to get building name and position
                        string[] parts = projectKey.Split('_');
                        if (parts.Length >= 4)
                        {
                            string buildingName = parts[0];
                            Vector3Int gridPosition = new Vector3Int(
                                int.Parse(parts[1]),
                                int.Parse(parts[2]),
                                int.Parse(parts[3])
                            );
                            
                            // Check if this project has the quest
                            ConstructionProject project = ConstructionManager.Instance.GetProject(buildingName, gridPosition);
                            if (project != null && project.originalQuestTexts.Contains(questText))
                            {
                                isConstructionQuest = true;
                                Debug.Log($"[CompleteSelectedTasks] Found construction quest in project {buildingName}! Calling CheckQuestCompletion for '{questText}'");
                                ConstructionManager.Instance.CheckQuestCompletion(buildingName, gridPosition, questText);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[CompleteSelectedTasks] ConstructionManager.Instance is null!");
                }
                
                if (!isConstructionQuest)
                {
                    Debug.Log($"[CompleteSelectedTasks] Quest '{questText}' is NOT a construction quest");
                }
                
                int rewardAmount = 5;
                ToDoItemMeta metaReward = item.GetComponent<ToDoItemMeta>();
                if (metaReward != null)
                    rewardAmount = metaReward.rewardAmount;
                
                Debug.Log($"[ToDoListManager] Quest completion: '{questText}' - rewardAmount from meta: {rewardAmount}");
                GiveCurrencyReward(questText, rewardAmount); // Give the dynamic reward amount for this quest.

                // Reward EXP for completing the quest/task:
                if (PlayerLevelManager.Instance != null) // Check if the PlayerLevelManager instance exists. 
                {
                    // Determine quest type for EXP calculation
                    bool isDailyQuest = meta?.isFromDailyQuest ?? false;
                    bool isCustomQuest = !isDailyQuest && !IsRegionQuest(questText);
                    
                    int expReward = CalculateQuestEXP(questText, isDailyQuest, isCustomQuest); // Calculate the EXP reward using hybrid difficulty system
                    PlayerLevelManager.Instance.AddEXP(expReward); // Add the calculated EXP to the player's level manager. 
                    
                    // Track total EXP and completed tasks
                    totalEXP += expReward;
                    completedTasks++;
                    
                    // Show EXP popup animation
                    QuestDifficulty difficulty = DetermineQuestDifficulty(questText);
                    if (EXPPopupManager.Instance != null)
                    {
                        EXPPopupManager.Instance.ShowEXPPopupCenter(expReward, difficulty);
                    }
                }
                
                Destroy(item.gameObject); // Destroy the item after giving the reward. 
            }
        }
        
        // Show completion summary if tasks were completed
        if (completedTasks > 0)
        {
            ShowCompletionSummary(completedTasks, totalEXP);
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
                ResourceManager.Instance.AddResources(ResourceManager.ResourceType.BalanceTickets, 100); // Add 1 Balance Ticket to the player's resources using the ResourceManager. 
                dailyQuestsButtonHandler.balanceTicketRewarded = true; // Mark as rewarded for this set

                rewardModal.Show(
                    "You got 1 Balance Ticket for completing today's Daily Quests! Keep up the good work!",
                    balanceTicketSprite // Assign the sprite for the Balance Ticket reward in the Inspector. 
                ); // Show the reward modal popup to the player with a congratulatory message and the Balance Ticket icon. 
                //Debug.Log("All daily quests completed! Rewarding 1 Balance Ticket.");
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
                string questText = GetQuestTextFromItem(item); // Get the quest text before destroying the item
                
                // Check if this is a construction skip quest that was deleted
                // Query ConstructionManager directly instead of looking for BuildingConstructionTimer components
                bool isConstructionQuest = false;
                
                Debug.Log($"[DeleteSelectedTasks] Checking quest: '{questText}' against ConstructionManager");
                
                if (ConstructionManager.Instance != null)
                {
                    // Get all active projects from ConstructionManager
                    var allProjects = ConstructionManager.Instance.GetAllProjectKeys();
                    Debug.Log($"[DeleteSelectedTasks] Found {allProjects.Count} active construction projects");
                    
                    foreach (string projectKey in allProjects)
                    {
                        // Parse the project key to get building name and position
                        string[] parts = projectKey.Split('_');
                        if (parts.Length >= 4)
                        {
                            string buildingName = parts[0];
                            Vector3Int gridPosition = new Vector3Int(
                                int.Parse(parts[1]),
                                int.Parse(parts[2]),
                                int.Parse(parts[3])
                            );
                            
                            // Check if this project has the quest
                            ConstructionProject project = ConstructionManager.Instance.GetProject(buildingName, gridPosition);
                            if (project != null && project.originalQuestTexts.Contains(questText))
                            {
                                isConstructionQuest = true;
                                Debug.Log($"[DeleteSelectedTasks] Found construction quest in project {buildingName}! Calling CheckQuestDeletion for '{questText}'");
                                ConstructionManager.Instance.CheckQuestDeletion(buildingName, gridPosition, questText);
                                Debug.Log($"Construction quest deleted: {questText} - notifying ConstructionManager");
                                break;
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[DeleteSelectedTasks] ConstructionManager.Instance is null!");
                }
                
                if (!isConstructionQuest)
                {
                    Debug.Log($"[DeleteSelectedTasks] Quest '{questText}' is NOT a construction quest");
                }
                
                Destroy(item.gameObject); // Destroy the item without giving a reward. 
            }
        }
    }

    /// <summary>
    /// Helper to get the quest/task text from a to-do item.
    /// </summary>
    public string GetQuestTextFromItem(Transform item)
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
        Debug.Log($"[ToDoListManager] GiveCurrencyReward called: quest='{questText}', rewardAmount={rewardAmount}");
        
        string region = GetRegionFromQuest(questText);
        if (!string.IsNullOrEmpty(region))
        {
            Debug.Log($"[ToDoListManager] Adding {rewardAmount} currency for region: {region}");
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