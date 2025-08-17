using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LifeCraft.Systems;
using LifeCraft.UI; 
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DailyQuestsButtonHandler : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dailyQuestsModalPanel;
    public TMP_Text modalTitle;
    public Transform questListContainer;
    public GameObject questItemPrefab; // Prefab with TMP_Text for each quest
    public TMP_Text timerText;
    public ToDoListManager toDoListManager; // Assign in Inspector for Add To-Do functionality (this line adds the field to the Inspector for easy assignment of the ToDoListManager script, which manages the to-do list UI and functionality). 
    public TestToDoListManager testToDoListManager; // Assign in Inspector for testing purposes. 

    public ResourceBarManager resourceBarManager; // Assign in Inspector for Balance Ticket rewards (this line adds the field to the Inspector for easy assignment of the ResourceBarManager script, which manages the resource bar UI and functionality).

    [Header("Button")]
    public Button openDailyQuestsButton;
    public Button closeModalButton;

    [Header("Quest Settings")]
    //public int questsPerDay = 8;

    private QuestManager questManager;
    private float timerUpdateInterval = 1f;
    private float timerUpdateElapsed = 0f;
    public bool balanceTicketRewarded = false; // FIX: Tracks if the ticket has been rewarded for the current set of daily quests
    public bool dailyQuestsGenerated = false;  // FIX: Tracks if daily quests have been generated at least once

    private void Awake()
    {
        if (openDailyQuestsButton != null)
            openDailyQuestsButton.onClick.AddListener(ShowDailyQuestsModal);
        if (closeModalButton != null)
            closeModalButton.onClick.AddListener(HideModal);
    }

    private void Start()
    {
        questManager = FindFirstObjectByType<QuestManager>();
        if (dailyQuestsModalPanel != null)
            dailyQuestsModalPanel.SetActive(false);
    }

    private void Update()
    {
        if (dailyQuestsModalPanel != null && dailyQuestsModalPanel.activeSelf && timerText != null)
        {
            timerUpdateElapsed += Time.unscaledDeltaTime;
            if (timerUpdateElapsed >= timerUpdateInterval)
            {
                timerUpdateElapsed = 0f;
                UpdateTimerText();
            }
        }
    }

    public void ShowDailyQuestsModal()
    {
        if (dailyQuestsModalPanel != null)
            dailyQuestsModalPanel.SetActive(true);
        if (modalTitle != null)
            modalTitle.text = "Daily Quests";

        // Clear old quests
        foreach (Transform child in questListContainer)
        {
            Destroy(child.gameObject);
        }

        // Get today's quests
        if (questManager != null)
        {
            List<string> quests = questManager.GetDailyQuests();
            Debug.Log($"[DailyQuestsButtonHandler] {quests.Count} daily quests.");
            foreach (var quest in quests)
            {
                Debug.Log($"[DailyQuestsButtonHandler] Setting up quest: {quest}");
                var questItem = Instantiate(questItemPrefab, questListContainer);
                // Use QuestItemUI to set up the quest and wire up the Add To-Do button
                QuestItemUI questItemUI = questItem.GetComponent<QuestItemUI>();
                if (questItemUI != null)
                {
                    string region = ExtractRegionFromQuest(quest); // Extract region from quest string (e.g., "(Health Harbor)")
                    int amount = ExtractAmountFromQuest(quest); 
                    questItemUI.Setup(quest, region, toDoListManager, true, amount);
                }
                else
                {
                    // Fallback: just set the text if the script is missing
                    TMP_Text textComponent = questItem.GetComponentInChildren<TMP_Text>();
                    if (textComponent != null)
                        textComponent.text = quest;
                }
            }
            // FIX: Only reset the reward flag if new quests are generated (list is not empty). 
            if (quests.Count > 0 && !dailyQuestsGenerated)
            {
                dailyQuestsGenerated = true;
                balanceTicketRewarded = false;
            }
        }
            else
            {
                var questItem = Instantiate(questItemPrefab, questListContainer);
                TMP_Text textComponent = questItem.GetComponentInChildren<TMP_Text>();
                if (textComponent != null)
                    textComponent.text = "QuestManager not found.";
            }

        UpdateTimerText();
    }

    public void HideModal()
    {
        if (dailyQuestsModalPanel != null)
            dailyQuestsModalPanel.SetActive(false);
    }

    private void UpdateTimerText()
    {
        if (questManager == null || timerText == null)
            return;
        int lastGen = questManager.GetQuestGenerationTimestamp();
        int now = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        int secondsLeft = Mathf.Max(0, 86400 - (now - lastGen));
        TimeSpan t = TimeSpan.FromSeconds(secondsLeft);
        timerText.text = $"Next refresh in: {t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}";
    }

    public void RemoveQuestByText(string questText) // For handling Daily Quests completion. 
    {
        Debug.Log("[RemoveQuestByText] Called with questText: " + questText); // Debug log for when the method is called.
        foreach (Transform child in questListContainer) // Iterate through each child in the quest list container.
        {
            TMP_Text text = child.GetComponentInChildren<TMP_Text>(); // Get the TMP_Text component from the child. 
            Debug.Log("[RemoveQuestByText] Checking child with text: " + (text != null ? text.text : "null")); // Debug log for each child.
            if (text != null && text.text.Trim() == questText.Trim()) // Check if the text (from the Daily Quests list) matches the questText parameter (the current quest being completed). 
            {
                Debug.Log("[RemoveQuestByText] Match found, destroying quest item."); // Debug log for when a match is found.
                Destroy(child.gameObject); // If it matches, destroy the quest item from the Daily Quests list. 
                // FIX: Also remove from QuestManager's daily quest list to keep UI and data in sync.
                if (questManager != null)
                {
                    questManager.RemoveDailyQuest(questText);
                }
                break; // Exit the loop after removing the quest item from the Daily Quests list. 
            }
        }
        // FIX: The reward logic is now handled in the ToDoListManager after all removals, not here.
    }

    [ContextMenu("Force Reset Daily Quests (Developer Only)")]
    public void ForceResetDailyQuests()
    {
        Debug.Log("[DEV] Forcing daily quests reset!");
        var questManager = LifeCraft.Core.GameManager.Instance?.QuestManager;
        if (questManager != null)
        {
            // Assuming QuestManager has a method to reset daily quests
            questManager.ResetDailyQuests();
            Debug.Log("[DEV] Daily quests have been reset.");
        }
        else
        {
            Debug.LogWarning("[DEV] QuestManager not found. Cannot reset daily quests.");
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