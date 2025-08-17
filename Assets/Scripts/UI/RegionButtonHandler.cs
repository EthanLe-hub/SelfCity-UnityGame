using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using LifeCraft.Systems; // Import the LifeCraft Systems namespace to access the QuestManager class. 

namespace LifeCraft.UI
{
    public class RegionButtonHandler : MonoBehaviour
    {
        public QuestManager questManager; // Assign in Inspector: this is the script that handles the quest logic and data. 
        public GameObject modalPanel;
        public TMP_Text modalTitle;
        public Transform taskListContainer;
        public GameObject taskItemPrefab; // Prefab with a TMP_Text for each task
        public ToDoListManager toDoListManager; // Assign in Inspector

        // Large, unique quest lists for each region (different from daily quests)
        // private Dictionary<string, List<string>> regionTasks = new Dictionary<string, List<string>> {
        //     { "Health Harbor", new List<string> {
        //         "Complete a 5k run.",
        //         "Prepare a week's worth of healthy meals.",
        //         "Join a local fitness class.",
        //         "Track your sleep for 7 days.",
        //         "Try a new outdoor sport.",
        //         "Organize a group hike.",
        //         "Host a healthy potluck dinner.",
        //         "Volunteer at a community garden.",
        //         "Create a personal wellness journal.",
        //         "Plan a tech-free day for relaxation.",
        //         "Try a new type of herbal tea.",
        //         "Set a new hydration goal.",
        //         "Take a cold shower challenge.",
        //         "Try a new breathing exercise.",
        //         "Host a healthy recipe swap."
        //     }},
        //     { "Mind Palace", new List<string> {
        //         "Complete a 7-day meditation streak.",
        //         "Read a new self-development book.",
        //         "Write a letter to your future self.",
        //         "Attend a mindfulness workshop.",
        //         "Start a gratitude journal.",
        //         "Try a digital detox for 24 hours.",
        //         "Create a vision board.",
        //         "Learn a new language phrase each day for a week.",
        //         "Host a book club meeting.",
        //         "Try a new brain-training app.",
        //         "Write a poem about your day.",
        //         "Practice mindful eating for a meal.",
        //         "Try a new puzzle or logic game.",
        //         "Reflect on your week and set intentions.",
        //         "Listen to a new genre of music."
        //     }},
        //     { "Creative Commons", new List<string> {
        //         "Paint or draw something from nature.",
        //         "Write a short story based on a dream.",
        //         "Compose a simple song or melody.",
        //         "Try a new craft (origami, knitting, etc.).",
        //         "Design a poster for a fictional event.",
        //         "Take creative photos around your neighborhood.",
        //         "Make a collage from old magazines.",
        //         "Host a virtual art night with friends.",
        //         "Create a comic strip about your week.",
        //         "Try a new recipe and plate it artistically.",
        //         "Write a script for a short play.",
        //         "Build something with recycled materials.",
        //         "Start a daily doodle challenge.",
        //         "Make a vision board for your goals.",
        //         "Try blackout poetry with a newspaper."
        //     }},
        //     { "Social Square", new List<string> {
        //         "Organize a virtual game night.",
        //         "Write a thank-you note to a mentor.",
        //         "Host a neighborhood clean-up.",
        //         "Plan a surprise for a friend.",
        //         "Join a new club or group.",
        //         "Volunteer for a local cause.",
        //         "Start a group chat for a shared interest.",
        //         "Help a neighbor with a task.",
        //         "Share a positive news story online.",
        //         "Introduce yourself to someone new.",
        //         "Host a potluck dinner.",
        //         "Reconnect with an old friend.",
        //         "Organize a community event.",
        //         "Send a care package to someone.",
        //         "Write a letter to a family member."
        //     }}
        // };

        // This method now fetches region quests from QuestManager, not a local dictionary.
        // This ensures all region quests are managed centrally and fixes errors about missing regionQuests.
        public List<string> GetRegionQuests(string regionName) 
        {
            // Delegate to QuestManager's method
            return questManager != null ? questManager.GetRegionQuests(regionName) : new List<string>();
        }

        public void ShowRegionModal(string regionName)
        {
            modalPanel.SetActive(true);
            modalTitle.text = regionName;

            // Clear old tasks
            foreach (Transform child in taskListContainer)
            {
                Destroy(child.gameObject); // Only destroy the quest items!
            }

            // Get tasks from QuestManager (now via our own GetRegionQuests method)
            List<string> tasks = GetRegionQuests(regionName);
            Debug.Log($"[RegionButtonHandler] {regionName} has {tasks.Count} tasks.");

            // Add new tasks
            if (tasks != null && tasks.Count > 0) // Check if tasks exist for the region. 
            {
                foreach (var task in tasks) // Iterate through each task in the list of tasks for the region. 
                {
                    Debug.Log($"[RegionButtonHandler] Setting up quest: {task}");
                    var taskItem = Instantiate(taskItemPrefab, taskListContainer);
                    // Use QuestItemUI to set up the quest and wire up the Add To-Do button
                    QuestItemUI questItemUI = taskItem.GetComponent<QuestItemUI>();
                    if (questItemUI != null)
                    {
                        // Extract the resource amount from the quest string (e.g., "+5") using a helper method.
                        // This allows the UI to display the correct reward amount dynamically for each quest.
                        int amount = ExtractAmountFromQuest(task); 
                        // Pass the quest text, region name, ToDoListManager reference, false for fromDailyQuest, and the dynamic amount to Setup.
                        questItemUI.Setup(task, regionName, toDoListManager, false, amount); 
                    }
                    else
                    {
                        // Fallback: just set the text if the script is missing
                        TMP_Text text = taskItem.GetComponentInChildren<TMP_Text>();
                        if (text != null) text.text = task;
                    }
                }
            }
            else
            {
                var taskItem = Instantiate(taskItemPrefab, taskListContainer);
                TMP_Text text = taskItem.GetComponentInChildren<TMP_Text>();
                if (text != null) text.text = "No tasks found for this region.";
            }
        }

        public void HideModal()
        {
            modalPanel.SetActive(false);
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
} 