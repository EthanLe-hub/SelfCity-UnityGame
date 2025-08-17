using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using LifeCraft.Core;
using LifeCraft.Systems;

namespace LifeCraft.UI
{
    /// <summary>
    /// Individual construction timer component for each building
    /// </summary>
    public class BuildingConstructionTimer : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject constructionPanel;
        public Slider progressBar;
        public TextMeshProUGUI timerText;
        public Button skipButton;
        public TextMeshProUGUI skipButtonText;
        
        [Header("Configuration")]
        public float updateInterval = 1f;
        
        private string buildingName;
        private Vector3Int gridPosition;
        private float constructionDuration;
        private string regionType;
        private float startTime;
        private bool isCompleted = false;
        private Coroutine updateCoroutine;
        // Removed allowSkipButtonUpdates flag - no longer needed since skip button text is not updated in UpdateUI
        
        // Quest tracking is now handled by ConstructionManager
        
        private void OnEnable()
        {
            // Re-synchronize with ConstructionManager when this component becomes active
            if (!string.IsNullOrEmpty(buildingName))
            {
                Debug.Log($"BuildingConstructionTimer OnEnable for {buildingName} - re-synchronizing with ConstructionManager");
                StartCoroutine(ReSyncWithManager());
            }
            else
            {
                Debug.Log($"BuildingConstructionTimer OnEnable for unnamed building - waiting for StartConstruction");
            }
        }
        
        /// <summary>
        /// Force refresh the UI immediately - useful for debugging
        /// </summary>
        [ContextMenu("Force Refresh UI")]
        public void ForceRefreshUI()
        {
            Debug.Log($"ForceRefreshUI: Manually refreshing UI for {buildingName}");
            if (!string.IsNullOrEmpty(buildingName))
            {
                StartCoroutine(ForceUIUpdate());
            }
            else
            {
                Debug.LogWarning("ForceRefreshUI: No building name set");
            }
        }
        
        public IEnumerator ReSyncWithManager()
        {
            // Wait a frame to ensure ConstructionManager is ready
            yield return null;
            
            Debug.Log($"ReSyncWithManager: Checking ConstructionManager for {buildingName}");
            
            if (ConstructionManager.Instance != null)
            {
                Debug.Log($"ReSyncWithManager: ConstructionManager.Instance is valid");
                ConstructionProject project = ConstructionManager.Instance.GetProject(buildingName, gridPosition);
                
                if (project != null)
                {
                    Debug.Log($"ReSyncWithManager: Found existing project for {buildingName} - re-synchronizing UI");
                    Debug.Log($"ReSyncWithManager: Project details - Completed: {project.isCompleted}, Quest count: {project.originalQuestTexts.Count}");
                    
                    // Subscribe to events
                    ConstructionManager.Instance.OnConstructionCompleted += OnConstructionCompleted;
                    ConstructionManager.Instance.OnQuestDeleted += OnQuestDeleted;
                    
                    // Check if construction is already completed
                    if (project.isCompleted)
                    {
                        Debug.Log($"ReSyncWithManager: Project {buildingName} is already completed - hiding panel");
                        CompleteConstruction();
                    }
                    else
                    {
                        // Show panel and start UI updates
                        if (constructionPanel != null)
                        {
                            constructionPanel.SetActive(true);
                            Debug.Log($"ReSyncWithManager: Activated construction panel for {buildingName}");
                        }
                        
                        // Set up skip button
                        if (skipButton != null)
                        {
                            skipButton.onClick.RemoveAllListeners();
                            skipButton.onClick.AddListener(OnSkipButtonClicked);
                            Debug.Log($"ReSyncWithManager: Set up skip button for {buildingName}");
                        }
                        
                        // Set skip button text based on total quests needed (master list)
                        if (skipButtonText != null)
                        {
                            Debug.Log($"ReSyncWithManager: Current state for {buildingName} - Active quests: {project.activeQuestTexts.Count}, Master quests: {project.originalQuestTexts.Count}");
                            
                            // Use the same logic as UpdateSkipButtonTextOnResume to ensure consistency
                            ToDoListManager toDoListManager = FindFirstObjectByType<ToDoListManager>();
                            if (toDoListManager != null && toDoListManager.toDoListContainer != null)
                            {
                                // Count how many of our quests are still active in the To-Do List
                                int activeQuestsInToDoList = 0;
                                foreach (string questText in project.activeQuestTexts)
                                {
                                    // Check if this quest exists in the To-Do List by iterating through all items
                                    bool questFound = false;
                                    for (int i = 0; i < toDoListManager.toDoListContainer.childCount; i++)
                                    {
                                        Transform item = toDoListManager.toDoListContainer.GetChild(i);
                                        
                                        // Check TextMeshPro first
                                        TMP_Text tmpText = item.GetComponentInChildren<TMP_Text>();
                                        if (tmpText != null && tmpText.text == questText)
                                        {
                                            questFound = true;
                                            break;
                                        }
                                        
                                        // Check Unity UI Text
                                        Text uiText = item.GetComponentInChildren<Text>();
                                        if (uiText != null && uiText.text == questText)
                                        {
                                            questFound = true;
                                            break;
                                        }
                                    }
                                    
                                    if (questFound)
                                    {
                                        activeQuestsInToDoList++;
                                    }
                                }
                                
                                if (project.originalQuestTexts.Count > 0)
                                {
                                    // Regardless of having or not having active quests, show TOTAL quests remaining from original list:
                                    string newText = $"Skip ({project.originalQuestTexts.Count} quests remaining)";
                                    skipButtonText.text = newText;
                                    Debug.Log($"ReSyncWithManager: Updated Skip button text: '{newText}' for {buildingName} (ALWAYS showing total from original list)");
                                }
                                else
                                {
                                    // No quests at all - show default text
                                    skipButtonText.text = "Skip (Generate quests)";
                                    Debug.Log($"ReSyncWithManager: Updated Skip button text: 'Skip (Generate quests)' for {buildingName} (no quests found)");
                                }
                            }
                            else
                            {
                                // ToDoListManager not found - fallback to original logic
                                if (project.originalQuestTexts.Count > 0)
                                {
                                    // Show total quests needed from master list (not just active ones)
                                    skipButtonText.text = $"Skip ({project.originalQuestTexts.Count} quests remaining)";
                                    Debug.Log($"ReSyncWithManager: Set skip button text for {buildingName} - total quests needed: {project.originalQuestTexts.Count}");
                                }
                                else
                                {
                                    skipButtonText.text = "Skip (Generate quests)";
                                    Debug.Log($"ReSyncWithManager: Set skip button text for {buildingName} - no quests");
                                }
                            }
                        }
                        
                        // Start UI updates
                        if (updateCoroutine != null)
                            StopCoroutine(updateCoroutine);
                        updateCoroutine = StartCoroutine(UpdateUI());
                        Debug.Log($"ReSyncWithManager: Started UpdateUI coroutine for {buildingName}");
                    }
                }
                else
                {
                    Debug.LogWarning($"ReSyncWithManager: No existing project found for {buildingName} at {gridPosition} - this component may be orphaned");
                    Debug.LogWarning($"ReSyncWithManager: Available projects: {string.Join(", ", ConstructionManager.Instance.GetAllProjectKeys())}");
                }
            }
            else
            {
                Debug.LogError($"ReSyncWithManager: ConstructionManager.Instance is null!");
            }
        }
        
        /// <summary>
        /// Start construction for this building
        /// </summary>
        public void StartConstruction(string buildingName, Vector3Int gridPosition, float constructionDurationMinutes, string regionType)
        {
            this.buildingName = buildingName;
            this.gridPosition = gridPosition;
            this.constructionDuration = constructionDurationMinutes * 60f;
            this.regionType = regionType;
            this.startTime = Time.time;
            this.isCompleted = false;
            
            // Register with the persistent manager
            if (ConstructionManager.Instance != null)
            {
                ConstructionManager.Instance.RegisterConstruction(buildingName, gridPosition, constructionDurationMinutes, regionType);
                
                // Subscribe to construction completion events
                ConstructionManager.Instance.OnConstructionCompleted += OnConstructionCompleted;
                ConstructionManager.Instance.OnQuestDeleted += OnQuestDeleted;
            }
            
            // Show the construction panel
            if (constructionPanel != null)
            {
                constructionPanel.SetActive(true);
            }
            
            // Set up the skip button
            if (skipButton != null)
            {
                skipButton.onClick.RemoveAllListeners();
                skipButton.onClick.AddListener(OnSkipButtonClicked);
            }
            
            // Set initial skip button text
            if (skipButtonText != null)
            {
                skipButtonText.text = "Skip (Generate quests)";
            }
            
            // Start UI update coroutine (this only updates the UI, not the timer logic)
            if (updateCoroutine != null)
                StopCoroutine(updateCoroutine);
            updateCoroutine = StartCoroutine(UpdateUI());
        }
        
        /// <summary>
        /// Resume construction for a building that was paused (project already registered)
        /// </summary>
        public void ResumeConstruction(string buildingName, Vector3Int gridPosition, float constructionDurationMinutes, string regionType, string savedSkipButtonText = "")
        {
            this.buildingName = buildingName;
            this.gridPosition = gridPosition;
            this.constructionDuration = constructionDurationMinutes * 60f;
            this.regionType = regionType;
            this.isCompleted = false;
            
            // Don't register with ConstructionManager - project should already exist
            if (ConstructionManager.Instance != null)
            {
                // Subscribe to construction completion events
                ConstructionManager.Instance.OnConstructionCompleted += OnConstructionCompleted;
                ConstructionManager.Instance.OnQuestDeleted += OnQuestDeleted;
            }
            
            // Show the construction panel
            if (constructionPanel != null)
            {
                constructionPanel.SetActive(true);
            }
            
            // Set up the skip button
            if (skipButton != null)
            {
                skipButton.onClick.RemoveAllListeners();
                skipButton.onClick.AddListener(OnSkipButtonClicked);
            }
            
            // IMMEDIATELY update Skip button text based on current quest state
            UpdateSkipButtonTextOnResume(savedSkipButtonText);
            
            // Start UI update coroutine (this only updates the UI, not the timer logic)
            if (updateCoroutine != null)
                StopCoroutine(updateCoroutine);
            updateCoroutine = StartCoroutine(UpdateUI());
            
            Debug.Log($"Resumed construction UI for {buildingName} at {gridPosition}");
        }
        
        /// <summary>
        /// Update Skip button text immediately when resuming construction
        /// </summary>
        private void UpdateSkipButtonTextOnResume(string savedSkipButtonText)
        {
            if (skipButtonText == null) return;
            
            // Get the current project to check quest state
            ConstructionProject project = ConstructionManager.Instance?.GetProject(buildingName, gridPosition);
            if (project != null)
            {
                // Check if we have active quests in the To-Do List
                ToDoListManager toDoListManager = FindFirstObjectByType<ToDoListManager>();
                if (toDoListManager != null && toDoListManager.toDoListContainer != null)
                {
                    // Count how many of our quests are still active in the To-Do List
                    int activeQuestsInToDoList = 0;
                    foreach (string questText in project.activeQuestTexts)
                    {
                        // Check if this quest exists in the To-Do List by iterating through all items
                        bool questFound = false;
                        for (int i = 0; i < toDoListManager.toDoListContainer.childCount; i++)
                        {
                            Transform item = toDoListManager.toDoListContainer.GetChild(i);
                            
                            // Check TextMeshPro first
                            TMP_Text tmpText = item.GetComponentInChildren<TMP_Text>();
                            if (tmpText != null && tmpText.text == questText)
                            {
                                questFound = true;
                                break;
                            }
                            
                            // Check Unity UI Text
                            Text uiText = item.GetComponentInChildren<Text>();
                            if (uiText != null && uiText.text == questText)
                            {
                                questFound = true;
                                break;
                            }
                        }
                        
                        if (questFound)
                        {
                            activeQuestsInToDoList++;
                        }
                    }
                    
                    if (project.originalQuestTexts.Count > 0)
                    {
                        // Regardless of having or not having active quests, show TOTAL quests remaining from original list:
                        string newText = $"Skip ({project.originalQuestTexts.Count} quests remaining)";
                        skipButtonText.text = newText;
                        Debug.Log($"Updated Skip button text on resume: '{newText}' for {buildingName} (found {project.originalQuestTexts.Count} TOTAL quests in master list)");
                    }

                    else
                    {
                        // No quests at all - show default text
                        skipButtonText.text = "Skip (Generate quests)";
                        Debug.Log($"Updated Skip button text on resume: 'Skip (Generate quests)' for {buildingName} (no quests found)");
                    }
                }
                else
                {
                    // ToDoListManager not found - use saved text as fallback
                    if (!string.IsNullOrEmpty(savedSkipButtonText))
                    {
                        skipButtonText.text = savedSkipButtonText;
                        Debug.Log($"Used saved Skip button text on resume: '{savedSkipButtonText}' for {buildingName} (ToDoListManager not found)");
                    }
                    else
                    {
                        skipButtonText.text = "Skip (Generate quests)";
                        Debug.Log($"Set default Skip button text on resume for {buildingName} (ToDoListManager not found)");
                    }
                }
            }
            else
            {
                // No project found - use saved text as fallback
                if (!string.IsNullOrEmpty(savedSkipButtonText))
                {
                    skipButtonText.text = savedSkipButtonText;
                    Debug.Log($"Used saved Skip button text on resume: '{savedSkipButtonText}' for {buildingName} (no project found)");
                }
                else
                {
                    skipButtonText.text = "Skip (Generate quests)";
                    Debug.Log($"Set default Skip button text on resume for {buildingName} (no project found)");
                }
            }
        }

        // Replace your UpdateTimer coroutine with UpdateUI:
        private IEnumerator UpdateUI()
        {
            Debug.Log($"UpdateUI started for {buildingName}");
            
            while (!isCompleted && this != null && this.gameObject != null && this.enabled)
            {
                // Get current project status from manager
                ConstructionProject project = ConstructionManager.Instance?.GetProject(buildingName, gridPosition);
                
                if (project == null)
                {
                    Debug.LogWarning($"Project not found for {buildingName} at {gridPosition} - stopping UI updates");
                    break;
                }
                
                if (project.isCompleted)
                {
                    Debug.Log($"Project {buildingName} is completed - stopping UI updates and hiding panel");
                    CompleteConstruction();
                    break;
                }
                
                float elapsedTime = Time.time - project.startTime;
                float remainingTime = project.constructionDuration - elapsedTime;
                
                // Check if construction is complete
                if (remainingTime <= 0)
                {
                    Debug.Log($"Construction time expired for {buildingName} - completing construction");
                    CompleteConstruction();
                    break;
                }
                
                // Update progress bar
                if (progressBar != null)
                {
                    float progress = elapsedTime / project.constructionDuration;
                    progressBar.value = Mathf.Clamp01(progress);
                }
                
                // Update timer text
                if (timerText != null)
                {
                    timerText.text = FormatTime(remainingTime);
                }
                
                // REMOVED: Skip button text updates from UpdateUI
                // Skip button text should only update when quests are completed or re-added
                // This prevents unnecessary updates every second
                
                yield return new WaitForSeconds(updateInterval);
            }
            
            Debug.Log($"UpdateUI ended for {buildingName}");
        }
        
        /// <summary>
        /// Complete construction
        /// </summary>
        private void CompleteConstruction()
        {
            isCompleted = true;
            
            // Hide the construction panel only when construction is actually complete
            if (constructionPanel != null)
            {
                constructionPanel.SetActive(false);
            }
            
            Debug.Log($"Construction completed for {buildingName} - panel hidden");
        }
        
        /// <summary>
        /// Check if this building is under construction
        /// </summary>
        public bool IsUnderConstruction()
        {
            return !isCompleted;
        }
        
        /// <summary>
        /// Pause construction (stop UI updates and hide panel)
        /// </summary>
        public void PauseConstruction()
        {
            // Stop the UI update coroutine
            if (updateCoroutine != null)
            {
                StopCoroutine(updateCoroutine);
                updateCoroutine = null;
                Debug.Log($"Paused UI updates for {buildingName}");
            }
            
            // Hide the construction panel
            if (constructionPanel != null)
            {
                constructionPanel.SetActive(false);
                Debug.Log($"Hidden construction panel for {buildingName}");
            }
        }
        
        /// <summary>
        /// Format time as MM:SS
        /// </summary>
        private string FormatTime(float timeInSeconds)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        

        
        /// <summary>
        /// Handle skip button click
        /// </summary>
        public void OnSkipButtonClicked()
        {
            Debug.Log($"Skip button clicked for {buildingName}");
            
            // Get current project to check progress
            ConstructionProject project = ConstructionManager.Instance?.GetProject(buildingName, gridPosition);
            if (project == null)
            {
                Debug.LogError($"Skip button clicked but no project found for {buildingName}");
                return;
            }
            
            Debug.Log($"Skip button logic for {buildingName}:");
            Debug.Log($"  - Master quests: {project.originalQuestTexts.Count}");
            Debug.Log($"  - Active quests: {project.activeQuestTexts.Count}");
            Debug.Log($"  - Completed quests: {project.completedSkipQuests}");
            Debug.Log($"  - Deleted quests: {project.deletedSkipQuests}");
            
            // Check if we have quests in master list that can be re-added (deleted quests)
            if (project.originalQuestTexts.Count > 0)
            {
                // We have quests in master list - check which ones are not currently active
                List<string> questsToReAdd = new List<string>();
                
                foreach (string questText in project.originalQuestTexts)
                {
                    if (!project.activeQuestTexts.Contains(questText))
                    {
                        questsToReAdd.Add(questText);
                    }
                }
                
                if (questsToReAdd.Count > 0)
                {
                    // Re-add the deleted quests from master list
                    Debug.Log($"Re-adding {questsToReAdd.Count} deleted quests for {buildingName}");
                    
                    // Find ToDoListManager in the scene
                    ToDoListManager toDoListManager = FindFirstObjectByType<ToDoListManager>();
                    if (toDoListManager != null)
                    {
                        // Re-add quests from master list to active list and To-Do List
                        foreach (string questText in questsToReAdd)
                        {
                            project.activeQuestTexts.Add(questText);
                            toDoListManager.AddToDo(questText);
                            Debug.Log($"Re-added quest: {questText}");
                        }
                        
                        // Reset deleted quests counter since we've re-added them
                        project.deletedSkipQuests = 0;
                        
                        // Update skip button text to reflect the total quests needed
                        if (skipButtonText != null)
                        {
                            string newText = $"Skip ({project.originalQuestTexts.Count} quests remaining)";
                            skipButtonText.text = newText;
                            Debug.Log($"Updated skip button text after re-adding quests: {newText} for {buildingName}");
                        }
                        
                        Debug.Log($"Successfully re-added {questsToReAdd.Count} quests for {buildingName}");
                    }
                    else
                    {
                        Debug.LogError("ToDoListManager not found in scene!");
                    }
                }
                else
                {
                    Debug.Log($"All quests from master list are already active for {buildingName}");
                }
            }
            else
            {
                // No quests in master list - this is the FIRST time clicking Skip, generate new quests
                Debug.Log($"First time clicking Skip for {buildingName} - generating new quests");
                
                // Calculate total quests needed based on remaining time
                float elapsedTime = Time.time - project.startTime;
                float remainingTime = project.constructionDuration - elapsedTime;
                float remainingMinutes = remainingTime / 60f;
                int totalQuestsNeeded = Mathf.CeilToInt(remainingMinutes / 36f); // 360 minutes / 10 quests = 36 minutes per quest
                totalQuestsNeeded = Mathf.Clamp(totalQuestsNeeded, 1, 10);
                
                Debug.Log($"  - Total quests needed: {totalQuestsNeeded}");
                
                // Generate new quests
                if (ConstructionManager.Instance != null)
                {
                    ConstructionManager.Instance.AddSkipQuests(buildingName, gridPosition, totalQuestsNeeded);
                    Debug.Log($"Generated {totalQuestsNeeded} new quests for {buildingName} (first time)");
                    
                    // Update skip button text immediately after generating quests
                    if (skipButtonText != null)
                    {
                        string newText = $"Skip ({totalQuestsNeeded} quests remaining)";
                        skipButtonText.text = newText;
                        Debug.Log($"Updated skip button text after generating quests: {newText} for {buildingName}");
                    }
                }
                else
                {
                    Debug.LogError($"Skip button clicked but ConstructionManager.Instance is null!");
                }
            }
        }
        
        /// <summary>
        /// Check if a quest completion matches our construction quests
        /// </summary>
        public void CheckSkipQuestCompletion(string completedQuestText)
        {
            Debug.Log($"CheckSkipQuestCompletion called for {buildingName} with quest: {completedQuestText}");
            
            // Delegate to ConstructionManager
            if (ConstructionManager.Instance != null)
            {
                ConstructionManager.Instance.CheckQuestCompletion(buildingName, gridPosition, completedQuestText);
                
                // Update skip button text immediately for quest completion
                UpdateSkipButtonTextForCompletion();
                Debug.Log($"CheckSkipQuestCompletion: Updated skip button text for {buildingName}");
            }
            else
            {
                Debug.LogError($"CheckSkipQuestCompletion: ConstructionManager.Instance is null!");
            }
        }
        
        /// <summary>
        /// Check if a quest deletion matches our construction quests
        /// </summary>
        public void CheckSkipQuestDeletion(string deletedQuestText)
        {
            Debug.Log($"CheckSkipQuestDeletion called for {buildingName} with quest: {deletedQuestText}");
            
            // Delegate to ConstructionManager (this will trigger the OnQuestDeleted event)
            if (ConstructionManager.Instance != null)
            {
                ConstructionManager.Instance.CheckQuestDeletion(buildingName, gridPosition, deletedQuestText);
                
                // DON'T update skip button text for deletions - let the player re-add quests if needed
                Debug.Log($"CheckSkipQuestDeletion: Quest deleted, skip button text unchanged for {buildingName}");
            }
            else
            {
                Debug.LogError($"CheckSkipQuestDeletion: ConstructionManager.Instance is null!");
            }
        }
        
        // Removed ReEnableSkipButtonUpdates method - no longer needed
        
        /// <summary>
        /// Update skip button text specifically for quest completion
        /// </summary>
        private void UpdateSkipButtonTextForCompletion()
        {
            ConstructionProject project = ConstructionManager.Instance?.GetProject(buildingName, gridPosition);
            if (project != null && skipButtonText != null)
            {
                Debug.Log($"UpdateSkipButtonTextForCompletion: Found project for {buildingName}, master quest count: {project.originalQuestTexts.Count}");
                
                if (project.originalQuestTexts.Count > 0)
                {
                    // Show total quests remaining from master list
                    string newText = $"Skip ({project.originalQuestTexts.Count} quests remaining)";
                    skipButtonText.text = newText;
                    Debug.Log($"UpdateSkipButtonTextForCompletion: Updated skip button text to: {newText} for {buildingName}");
                }
                else
                {
                    // No quests in master list - calculate how many more quests are needed
                    float elapsedTime = Time.time - project.startTime;
                    float remainingTime = project.constructionDuration - elapsedTime;
                    float remainingMinutes = remainingTime / 60f;
                    int totalQuestsNeeded = Mathf.CeilToInt(remainingMinutes / 36f);
                    totalQuestsNeeded = Mathf.Clamp(totalQuestsNeeded, 1, 10);
                    
                    // Calculate quests still needed (accounting for completed quests)
                    int questsStillNeeded = totalQuestsNeeded - project.completedSkipQuests;
                    questsStillNeeded = Mathf.Max(0, questsStillNeeded); // Don't go negative
                    
                    // Determine difficulty based on remaining time
                    QuestDifficulty difficulty;
                    if (remainingMinutes >= 270f) difficulty = QuestDifficulty.Expert;
                    else if (remainingMinutes >= 180f) difficulty = QuestDifficulty.Hard;
                    else if (remainingMinutes >= 90f) difficulty = QuestDifficulty.Medium;
                    else difficulty = QuestDifficulty.Easy;
                    
                    string difficultyText = difficulty.ToString();
                    string newText = $"Skip ({questsStillNeeded} {difficultyText} quests)";
                    skipButtonText.text = newText;
                    Debug.Log($"UpdateSkipButtonTextForCompletion: Updated skip button text to: {newText} for {buildingName}");
                }
            }
            else
            {
                Debug.LogWarning($"UpdateSkipButtonTextForCompletion: Project not found or skipButtonText is null for {buildingName}");
            }
        }
        
        /// <summary>
        /// Force an immediate UI update to reflect quest completion
        /// </summary>
        private IEnumerator ForceUIUpdate()
        {
            // Wait a frame to ensure ConstructionManager has processed the quest completion
            yield return null;
            
            Debug.Log($"ForceUIUpdate: Starting immediate UI update for {buildingName}");
            
            // Skip button text updates are now handled by UpdateSkipButtonTextForCompletion
            
            // Get current project status and update UI immediately
            ConstructionProject project = ConstructionManager.Instance?.GetProject(buildingName, gridPosition);
            if (project != null && skipButtonText != null)
            {
                Debug.Log($"ForceUIUpdate: Found project for {buildingName}, active quest count: {project.activeQuestTexts.Count}, master quest count: {project.originalQuestTexts.Count}");
                
                // Skip button text is now updated by UpdateSkipButtonTextForCompletion method
                // This method is only called for quest completions, not deletions
            }
            else
            {
                Debug.LogWarning($"ForceUIUpdate: Project not found or skipButtonText is null for {buildingName}");
            }
        }
        
        /// <summary>
        /// Check if this timer has the specified quest text in its tracking
        /// </summary>
        public bool HasQuest(string questText)
        {
            // Delegate to ConstructionManager
            if (ConstructionManager.Instance != null)
            {
                ConstructionProject project = ConstructionManager.Instance.GetProject(buildingName, gridPosition);
                bool hasQuest = project != null && project.activeQuestTexts.Contains(questText);
                Debug.Log($"HasQuest check for '{questText}' in {buildingName}: {hasQuest}");
                if (project != null)
                {
                    Debug.Log($"HasQuest: Active quests: {string.Join(", ", project.activeQuestTexts)}");
                }
                return hasQuest;
            }
            return false;
        }
        
        /// <summary>
        /// Handle construction completion event from ConstructionManager
        /// </summary>
        private void OnConstructionCompleted(string completedBuildingName, Vector3Int completedGridPosition)
        {
            // Check if this is the construction that was completed
            if (completedBuildingName == buildingName && completedGridPosition == gridPosition)
            {
                Debug.Log($"Construction completion event received for {buildingName} - hiding panel");
                CompleteConstruction();
            }
        }
        
        /// <summary>
        /// Handle quest deletion event from ConstructionManager
        /// </summary>
        private void OnQuestDeleted(string deletedBuildingName, Vector3Int deletedGridPosition)
        {
            // Check if this is the construction that had a quest deleted
            if (deletedBuildingName == buildingName && deletedGridPosition == gridPosition)
            {
                Debug.Log($"Quest deletion event received for {buildingName} - skip button text unchanged");
                // Skip button text is no longer updated in UpdateUI, so no action needed
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            if (ConstructionManager.Instance != null)
            {
                ConstructionManager.Instance.OnConstructionCompleted -= OnConstructionCompleted;
                ConstructionManager.Instance.OnQuestDeleted -= OnQuestDeleted;
            }
            
            // Stop coroutine if it's running
            if (updateCoroutine != null)
            {
                StopCoroutine(updateCoroutine);
            }
        }
    }
} 