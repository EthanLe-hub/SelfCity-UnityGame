// Code provided and pasted by AI Cursor, additional comments and explanations for code comprehension manually written by Ethan Le. 

/**
Handles the UI representation of quests: 
- Instantiates quest UI prefabs (like your QuestItem prefab).
- Populates the quest lists in the UI (region, daily, custom).
- Wires up the "Add To-Do" button for each quest. 
- Does NOT handle quest logic, saving/loading, data storage, etc. 
*/

using UnityEngine; // we are using UnityEngine for MonoBehaviour, which is the base class for all scripts in Unity. 
using LifeCraft.Systems; // we are using LifeCraft.Systems to access the QuestManager class, which handles the quest logic and data. 
using LifeCraft.UI;

public class QuestListManager : MonoBehaviour // this class manages the quest list UI and functionality. 
{
    [Header("References")] // this header is used to group the variables in the inspector for better organization. 
    public QuestManager questManager; // Assign your QuestManager in the Inspector, which is the script that handles the quest logic and data. 
    public ToDoListManager toDoListManager; // Assign your ToDoListManager in the Inspector, which is the script that manages the to-do list UI and functionality. 
    public GameObject questItemPrefab; // Assign your QuestItem prefab in the Inspector, which is the template for creating new quest items in the UI. 
    public Transform regionQuestListContainer; // Assign the parent object that holds all the region quest items in the Inspector. 
    public Transform dailyQuestListContainer; // Assign the parent object that holds all the daily quest items in the Inspector. 
    public Transform customQuestListContainer; // Assign this in the Inspector to the Content object of your Custom Tasks ScrollView

    void Start() // this method is called when the script is first run, which is when the game starts or when the script is enabled. 
    {
        // Only populate daily quests at start, if desired
        // Region quests should be populated when a region is selected, not at startup
        if (questManager != null)
        {
            PopulateDailyQuests(questManager.GetDailyQuests().ToArray());
        }
        // Do NOT populate region quests here!
    }
    
    /// <summary>
    /// Populates the region quest list UI with static quests.
    /// </summary>
    /// <param name="regionQuests">Array of region quest descriptions.</param>
    public void PopulateRegionQuests(string[] regionQuests) // this method populates the region quest list UI with static quests. 
    {
        if (regionQuestListContainer == null || questItemPrefab == null) return; // check if the container or prefab is not assigned, if so, exit the method. 
        foreach (string quest in regionQuests) // iterate through each quest in the regionQuests array. 
        {
            AddQuestToList(quest, regionQuestListContainer); // call the AddQuestToList method to add the quest to the region quest LIST CONTAINER. 
        }
    }

    /// <summary>
    /// Populates the daily quest list UI with daily quests.
    /// </summary>
    /// <param name="dailyQuests">Array of daily quest descriptions.</param>
    public void PopulateDailyQuests(string[] dailyQuests) // this method populates the daily quest list UI with daily quests. 
    {
        if (dailyQuestListContainer == null || questItemPrefab == null) return; // check if the container or prefab is not assigned, if so, exit the method. 
        foreach (string quest in dailyQuests) // iterate through each quest in the dailyQuests array. 
        {
            AddQuestToList(quest, dailyQuestListContainer); // call the AddQuestToList method to add the quest to the daily quest LIST CONTAINER. 
        }
    }

    /// <summary>
    /// Adds a custom quest to the daily quest list and updates QuestManager.
    /// </summary>
    /// <param name="customQuest">The custom quest description.</param>
    public void AddCustomQuest(string customQuest, string region)
    {
        if (questManager != null)
            questManager.AddCustomQuest(customQuest, region); // Add to custom quests dictionary

        // Add to the custom quest UI list (not the daily quest list!)
        AddQuestToList($"{customQuest} ({region})", customQuestListContainer);
    }

    /// <summary>
    /// Instantiates a QuestItem prefab, sets it up, and adds it to the specified container.
    /// </summary>
    /// <param name="questDescription">The quest description to display.</param>
    /// <param name="container">The UI container to add the quest item to.</param>
    private void AddQuestToList(string questDescription, Transform container) // this method instantiates a QuestItem prefab, sets it up, and adds it to the specified container. 
    { // Handles all quest item instantiation and setup (static, daily, and custom quests). 
        GameObject questGO = Instantiate(questItemPrefab, container); // Instantiate the QuestItem prefab as a child of the specified container (region or daily quest list). 
        QuestItemUI questItemUI = questGO.GetComponent<QuestItemUI>(); // Get the QuestItemUI component from the instantiated prefab. 
        if (questItemUI != null) // check if the QuestItemUI component is found. 
        {
            string region = ExtractRegionFromQuest(questDescription); // Extract region from quest description (e.g., "(Health Harbor)")
            // Extract the resource amount from the quest description (e.g., "+5") using a helper method.
            // This allows the UI to display the correct reward amount dynamically for each quest.
            int amount = ExtractAmountFromQuest(questDescription); 
            // Call the Setup method with quest description, region, ToDoListManager reference, true for daily quest, and the dynamic amount.
            questItemUI.Setup(questDescription, region, toDoListManager, true, amount); 
        }
        else // if the QuestItemUI component is NOT found, log a warning. 
        {
            Debug.LogWarning("QuestItemUI component missing on questItemPrefab.");
        }
    }

    // Helper method to extract region from quest string
    private string ExtractRegionFromQuest(string quest)
    {
        int start = quest.LastIndexOf('(');
        int end = quest.LastIndexOf(')');
        if (start != -1 && end != -1 && end > start)
        {
            return quest.Substring(start + 1, end - start - 1).Trim();
        }
        return "Unknown";
    }

    // Helper method to extract the resource amount from a quest string.
    // Looks for a "+<number>" pattern (e.g., "+5") and returns the number as an int.
    // Returns 5 by default if no amount is found.
    private int ExtractAmountFromQuest(string quest)
    {
        var match = System.Text.RegularExpressions.Regex.Match(quest, @"\+(\d+)");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int amount))
            return amount;
        return 5; // default
    }
}