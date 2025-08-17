using UnityEngine;
using TMPro;
using UnityEngine.UI;
using LifeCraft.Systems;
using LifeCraft.UI;

public class CustomTaskModalHandler : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField customQuestInputField;
    public Button healthHarborButton, mindPalaceButton, socialSquareButton, creativeCommonsButton;
    public Transform customQuestListContainer; // Assign the Content object of your Custom Quest ScrollView
    public GameObject questItemPrefab; // Assign your QuestItem prefab
    public ToDoListManager toDoListManager; // Assign in Inspector
    public QuestManager questManager; // Assign in Inspector
    public Button closeButton;

    void Start()
    {
        healthHarborButton.onClick.AddListener(() => AddCustomQuest("Health Harbor"));
        mindPalaceButton.onClick.AddListener(() => AddCustomQuest("Mind Palace"));
        socialSquareButton.onClick.AddListener(() => AddCustomQuest("Social Square"));
        creativeCommonsButton.onClick.AddListener(() => AddCustomQuest("Creative Commons"));
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    public void OpenModal()
    {
        gameObject.SetActive(true);
        RefreshCustomQuestList();
        customQuestInputField.text = "";
    }

    void AddCustomQuest(string region)
    {
        string questText = customQuestInputField.text;
        if (!string.IsNullOrWhiteSpace(questText))
        {
            questManager.AddCustomQuest(questText, region);
            RefreshCustomQuestList();
            customQuestInputField.text = "";
        }
    }

    void RefreshCustomQuestList()
    {
        // Clear old items
        foreach (Transform child in customQuestListContainer)
            Destroy(child.gameObject);

        // Define the desired region display order
        string[] regionOrder = { "Health Harbor", "Mind Palace", "Creative Commons", "Social Square" };
        var allCustomQuests = questManager.GetAllCustomQuests();

        // Display custom quests in the fixed region order
        foreach (string region in regionOrder)
        {
            if (allCustomQuests.ContainsKey(region))
            {
                foreach (var quest in allCustomQuests[region])
                {
                    // Instantiate a quest item prefab for each custom quest
                    GameObject questGO = Instantiate(questItemPrefab, customQuestListContainer);
                    QuestItemUI questItemUI = questGO.GetComponent<QuestItemUI>();
                    if (questItemUI != null)
                    {
                        // Extract the resource amount from the quest string (e.g., "+5") using a helper method.
                        // This allows the UI to display the correct reward amount dynamically for each quest.
                        int amount = ExtractAmountFromQuest(quest); 
                        // Pass quest, region, ToDoListManager, false for fromDailyQuest, and the dynamic amount to Setup.
                        questItemUI.Setup(quest, region, toDoListManager, false, amount); 
                    }
                }
            }
        }
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