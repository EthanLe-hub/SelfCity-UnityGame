// Code provided by AI Cursor, code manually written by Ethan Le with additional comments and explanations for code comprehension. Language: C# (C Sharp) 

using UnityEngine; // we are using UnityEngine for MonoBehaviour, which is the base class for all scripts in Unity. 
using TMPro; // we are using TMPro for TextMeshPro, which is a text rendering system in Unity that provides advanced text formatting and rendering capabilities. 
using UnityEngine.UI; // we are using UnityEngine.UI for UI components like Button and InputField, which are used to create user interfaces in Unity. 

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
        questRewardAmount = amount; // Store the dynamic reward amount for use when the quest is completed.

        ResourceIcon.sprite = GetResourceSpriteForRegion(region); // Assign the resource icon based on the region. This method should be defined elsewhere in my code to return the appropriate sprite based on the region of the quest. 
        if (ResourceAmountText != null)
            ResourceAmountText.text = $"+{amount}"; // Dynamically display the reward amount next to the icon.

        addToDoButton.onClick.RemoveAllListeners(); // clear any existing listeners on the button to avoid duplicates. 
        addToDoButton.onClick.AddListener(OnAddToDoClicked); // add a new listener that calls OnAddToDoClicked when the button is clicked. 

        Debug.Log($"Setting up quest: {description} (region: {region}, amount: {amount})");
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
            toDoListManager.AddToDo(questDescription, fromDailyQuest); // Use the correct flag. Update this if your ToDoListManager supports dynamic rewards.
        }
        else
        {
            Debug.LogWarning("ToDoListManager is null!");
        }
    }
}