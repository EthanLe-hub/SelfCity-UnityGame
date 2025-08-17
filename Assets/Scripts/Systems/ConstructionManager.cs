// This script run as a singleton that persis across scene/page changes. 
// It will store all active construction projects inside a data structure (allows for multiple buildings' construction times to be tracked). 
// It will update timers using Time.time instead of coroutines, and it will handle all the actual construction completion logic. 

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using LifeCraft.Core;
using LifeCraft.Systems;
using LifeCraft.UI;

namespace LifeCraft.Systems
{
    [System.Serializable]
    public class ConstructionProject
    {
        public string buildingName;
        public Vector3Int gridPosition;
        public float constructionDuration; // in seconds
        public string regionType;
        public float startTime;
        public bool isCompleted;
        public List<string> originalQuestTexts = new List<string>(); // Master list of ALL required quests
        public List<string> skipQuestIds = new List<string>();
        public int totalSkipQuests;
        public int completedSkipQuests;
        public int deletedSkipQuests; // Track deleted quests separately
        public List<string> activeQuestTexts = new List<string>(); // NEW: Currently active quests in To-Do List
    }

    [System.Serializable]
    public class ConstructionManagerSaveData
    {
        public List<ConstructionProject> projects = new List<ConstructionProject>();
    }

    public class ConstructionManager : MonoBehaviour
    {
        public static ConstructionManager Instance { get; private set; }
        
        [Header("Configuration")]
        public float updateInterval = 1f;
        
        private Dictionary<string, ConstructionProject> activeProjects = new Dictionary<string, ConstructionProject>();
        private Coroutine updateCoroutine;
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // Start the persistent update coroutine
            if (updateCoroutine != null)
                StopCoroutine(updateCoroutine);
            updateCoroutine = StartCoroutine(UpdateAllConstructions());
        }

        /// <summary>
        /// Save all construction projects to PlayerPrefs
        /// </summary>
        public void SaveConstructionData()
        {
            try
            {
                var saveData = new ConstructionManagerSaveData();
                
                // Convert dictionary to list for serialization
                foreach (var kvp in activeProjects)
                {
                    saveData.projects.Add(kvp.Value);
                }
                
                string json = JsonUtility.ToJson(saveData);
                PlayerPrefs.SetString("ConstructionManagerData", json);
                PlayerPrefs.Save();
                Debug.Log($"ConstructionManager saved: {saveData.projects.Count} projects");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save ConstructionManager: {e.Message}");
            }
        }

        /// <summary>
        /// Load all construction projects from PlayerPrefs
        /// </summary>
        public void LoadConstructionData()
        {
            try
            {
                if (PlayerPrefs.HasKey("ConstructionManagerData"))
                {
                    string json = PlayerPrefs.GetString("ConstructionManagerData");
                    var saveData = JsonUtility.FromJson<ConstructionManagerSaveData>(json);
                    
                    if (saveData != null && saveData.projects != null)
                    {
                        // Clear existing projects
                        activeProjects.Clear();
                        
                        // Restore saved projects
                        foreach (var project in saveData.projects)
                        {
                            string projectKey = $"{project.buildingName}_{project.gridPosition.x}_{project.gridPosition.y}_{project.gridPosition.z}";
                            activeProjects[projectKey] = project;
                        }
                        
                        Debug.Log($"ConstructionManager loaded: {saveData.projects.Count} projects");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load ConstructionManager: {e.Message}");
            }
        }

        /// <summary>
        /// Clear all construction data and save the empty state
        /// </summary>
        public void ClearConstructionData()
        {
            activeProjects.Clear();
            SaveConstructionData();
        }
        
        /// <summary>
        /// Register a new construction project with premium time reduction applied (ONLY method that applies premium reduction)
        /// This method is called when a building is first placed and construction begins
        /// </summary>
        public void RegisterConstruction(string buildingName, Vector3Int gridPosition, float constructionTimeMinutes, string regionType)
        {
            // Apply premium construction time reduction if user has subscription
            float adjustedConstructionTime = constructionTimeMinutes;
            if (SubscriptionManager.Instance != null && SubscriptionManager.Instance.HasFasterConstruction())
            {
                float reductionMultiplier = SubscriptionManager.Instance.GetConstructionTimeReduction();
                adjustedConstructionTime = constructionTimeMinutes * reductionMultiplier;
                Debug.Log($"Premium user: Construction time reduced from {constructionTimeMinutes} to {adjustedConstructionTime} minutes for {buildingName}");
            }

            string projectKey = $"{buildingName}_{gridPosition.x}_{gridPosition.y}_{gridPosition.z}";
            
            if (!activeProjects.ContainsKey(projectKey))
            {
                ConstructionProject project = new ConstructionProject
                {
                    buildingName = buildingName,
                    gridPosition = gridPosition,
                    constructionDuration = adjustedConstructionTime * 60f, // Convert to seconds
                    regionType = regionType,
                    startTime = Time.time,
                    isCompleted = false
                };
                
                activeProjects[projectKey] = project;
                SaveConstructionData();
                
                Debug.Log($"Registered construction: {projectKey} for {adjustedConstructionTime} minutes");
            }
            else
            {
                Debug.LogWarning($"Construction project already exists: {projectKey}");
            }
        }

        /// <summary>
        /// Register a construction project with saved progress (NO premium time reduction applied)
        /// This method should only restore the exact saved construction time without modifications
        /// </summary>
        public void RegisterConstructionWithProgress(string buildingName, Vector3Int gridPosition, float constructionTimeMinutes, string regionType, float startTime, List<string> originalQuestTexts, List<string> activeQuestTexts, int completedSkipQuests, int totalSkipQuests)
        {
            // DO NOT apply premium time reduction here - this method restores saved progress
            // The construction time should already be the correct value from when it was first registered
            float adjustedConstructionTime = constructionTimeMinutes;
            Debug.Log($"Restoring construction with progress: {buildingName} for {adjustedConstructionTime} minutes (no premium reduction applied)");

            string projectKey = $"{buildingName}_{gridPosition.x}_{gridPosition.y}_{gridPosition.z}";
            
            if (!activeProjects.ContainsKey(projectKey))
            {
                ConstructionProject project = new ConstructionProject
                {
                    buildingName = buildingName,
                    gridPosition = gridPosition,
                    constructionDuration = adjustedConstructionTime * 60f, // Convert to seconds
                    regionType = regionType,
                    startTime = startTime,
                    isCompleted = false,
                    originalQuestTexts = new List<string>(originalQuestTexts),
                    activeQuestTexts = new List<string>(activeQuestTexts),
                    completedSkipQuests = completedSkipQuests,
                    totalSkipQuests = totalSkipQuests
                };
                
                activeProjects[projectKey] = project;
                SaveConstructionData();
                
                Debug.Log($"Registered construction with progress: {projectKey} for {adjustedConstructionTime} minutes");
            }
            else
            {
                Debug.LogWarning($"Construction project already exists: {projectKey}");
            }
        }
        
        public ConstructionProject GetProject(string buildingName, Vector3Int gridPosition)
        {
            string projectKey = $"{buildingName}_{gridPosition.x}_{gridPosition.y}_{gridPosition.z}";
            activeProjects.TryGetValue(projectKey, out ConstructionProject project);
            return project;
        }
        
        public bool IsProjectActive(string buildingName, Vector3Int gridPosition)
        {
            string projectKey = $"{buildingName}_{gridPosition.x}_{gridPosition.y}_{gridPosition.z}";
            return activeProjects.ContainsKey(projectKey) && !activeProjects[projectKey].isCompleted;
        }
        
        /// <summary>
        /// Pause construction by removing the project from active projects
        /// </summary>
        public void PauseConstruction(string buildingName, Vector3Int gridPosition)
        {
            string projectKey = $"{buildingName}_{gridPosition.x}_{gridPosition.y}_{gridPosition.z}";
            if (activeProjects.ContainsKey(projectKey))
            {
                activeProjects.Remove(projectKey);
                Debug.Log($"Paused construction for {buildingName} at {gridPosition}");
            }
        }

        /// <summary>
        /// Remove construction project completely (for when building is sold)
        /// </summary>
        public void RemoveConstructionProject(string buildingName, Vector3Int gridPosition)
        {
            string projectKey = $"{buildingName}_{gridPosition.x}_{gridPosition.y}_{gridPosition.z}";
            if (activeProjects.ContainsKey(projectKey))
            {
                // Clean up any quests from the To-Do List
                var project = activeProjects[projectKey];
                if (project.originalQuestTexts.Count > 0)
                {
                    RemoveConstructionQuests(project);
                }
                
                // Remove the project
                activeProjects.Remove(projectKey);
                Debug.Log($"Removed construction project for {buildingName} at {gridPosition}");
            }
        }
        
        public List<string> GetAllProjectKeys()
        {
            return new List<string>(activeProjects.Keys);
        }
        
        private IEnumerator UpdateAllConstructions()
        {
            while (true)
            {
                List<string> completedProjects = new List<string>();
                
                foreach (var kvp in activeProjects)
                {
                    ConstructionProject project = kvp.Value;
                    
                    if (!project.isCompleted)
                    {
                        float elapsedTime = Time.time - project.startTime;
                        float remainingTime = project.constructionDuration - elapsedTime;
                        
                        if (remainingTime <= 0)
                        {
                            project.isCompleted = true;
                            completedProjects.Add(kvp.Key);
                            CompleteConstruction(project);
                        }
                    }
                }
                
                // Remove completed projects
                foreach (string key in completedProjects)
                {
                    activeProjects.Remove(key);
                }
                
                yield return new WaitForSeconds(updateInterval);
            }
        }
        
        // Event system for construction completion
        public System.Action<string, Vector3Int> OnConstructionCompleted;
        
        // Event system for quest deletion (to notify UI components)
        public System.Action<string, Vector3Int> OnQuestDeleted;
        
        private void CompleteConstruction(ConstructionProject project)
        {
            Debug.Log($"Construction completed for {project.buildingName}");
            
            // Clean up any quests if they exist
            if (project.originalQuestTexts.Count > 0)
            {
                RemoveConstructionQuests(project);
            }
            
            // Notify UI components that construction is complete
            OnConstructionCompleted?.Invoke(project.buildingName, project.gridPosition);
        }

        /// <summary>
        /// Remove ONLY the currently active construction quests for a project from the To-Do List,
        /// preserving the master list so they can be re-added when construction resumes.
        /// </summary>
        public void RemoveActiveQuestsFromToDo(string buildingName, Vector3Int gridPosition)
        {
            ConstructionProject project = GetProject(buildingName, gridPosition);
            if (project == null)
            {
                Debug.LogWarning($"[RemoveActiveQuestsFromToDo] No construction project found for {buildingName} at {gridPosition}");
                return;
            }

            // If there are no active quests, nothing to do
            if (project.activeQuestTexts == null || project.activeQuestTexts.Count == 0)
            {
                Debug.Log($"[RemoveActiveQuestsFromToDo] No active quests to remove for {buildingName} at {gridPosition}");
                return;
            }

            // Find ToDoListManager in the scene
            ToDoListManager toDoListManager = FindFirstObjectByType<ToDoListManager>();
            if (toDoListManager == null)
            {
                Debug.LogError("[RemoveActiveQuestsFromToDo] ToDoListManager not found in scene!");
                return;
            }

            // Remove only the active quests from the UI list
            foreach (string questText in project.activeQuestTexts)
            {
                toDoListManager.RemoveToDo(questText);
                Debug.Log($"[RemoveActiveQuestsFromToDo] Removed active construction quest from To-Do: {questText}");
            }

            // Preserve master/original list for resume; clear active list
            project.activeQuestTexts.Clear();
            project.deletedSkipQuests = 0; // reset deletion counter since we intentionally cleared

            // Do NOT change originalQuestTexts, completedSkipQuests, or totalSkipQuests
            Debug.Log($"[RemoveActiveQuestsFromToDo] Preserved {project.originalQuestTexts.Count} master quests for {buildingName}; cleared active list.");
        }

        public void AddSkipQuests(string buildingName, Vector3Int gridPosition, int questCount)
        {
            ConstructionProject project = GetProject(buildingName, gridPosition);
            if (project == null) return;

            // Find ToDoListManager in the scene
            ToDoListManager toDoListManager = FindFirstObjectByType<ToDoListManager>();
            if (toDoListManager == null)
            {
                Debug.LogError("ToDoListManager not found in scene!");
                return;
            }
            
            // Get ConstructionQuestPool
            if (ConstructionQuestPool.Instance == null) // Verify that the file to obtain Construction-Skip quests exists. 
            {
                Debug.LogError("ConstructionQuestPool.Instance is null!");
                return;
            }

            // Don't clear existing quests - we want to preserve progress
            // Only add new quests to the existing ones

            // Calculate remaining time and difficulty
            float elapsedTime = Time.time - project.startTime;
            float remainingTime = project.constructionDuration - elapsedTime;
            float remainingMinutes = remainingTime / 60f;
            QuestDifficulty targetDifficulty = CalculateDifficultyForTime(remainingMinutes);

            // Get quests based on region type and difficulty
            var quests = GetQuestsForRegion(project.regionType, questCount, targetDifficulty);

            // Add each quest to BOTH master list and active list
            foreach (string questText in quests)
            {
                // Generate a unique quest ID
                string questId = $"construction_skip_{System.Guid.NewGuid().ToString().Substring(0, 8)}";

                // Store the quest text in BOTH lists
                project.originalQuestTexts.Add(questText);
                project.activeQuestTexts.Add(questText);
                project.skipQuestIds.Add(questId);

                // Add to To-Do List (without the ugly prefix)
                toDoListManager.AddToDo(questText);

                Debug.Log($"Added construction quest: {questText}");
            }

            // Update total quests (add to existing count)
            project.totalSkipQuests += quests.Count;
            // Don't reset completedSkipQuests - preserve progress

            Debug.Log($"Added {quests.Count} construction skip quests for {project.buildingName} (smart system - preserving progress)");
        }

        // Helper method to determine the quest difficulty based on remaining construction time:
        private QuestDifficulty CalculateDifficultyForTime(float remainingMinutes)
        {
            // Updated for 6-hour maximum construction time
            // 6 hours = 360 minutes = max 10 quests
            // 4.5+ hours (270+ minutes) = Expert (hardest)
            // 3-4.5 hours (180-270 minutes) = Hard
            // 1.5-3 hours (90-180 minutes) = Medium
            // 0-1.5 hours (0-90 minutes) = Easy
            if (remainingMinutes >= 270f) return QuestDifficulty.Expert; // Return an Expert Skip Quest if remaining construction time is between 270 and 360 minutes (6 hours max).
            if (remainingMinutes >= 180f) return QuestDifficulty.Hard; // Return a Hard Skip Quest if remaining construction time is between 180 and 270 minutes.
            if (remainingMinutes >= 90f) return QuestDifficulty.Medium; // Return a Hard Skip Quest if remaining construction time is between 90 and 180 minutes.
            return QuestDifficulty.Easy; // Return an Easy Skip Quest if remaining construction time is less than 90 minutes. 
        }

        private List<string> GetQuestsForRegion(string regionType, int questCount, QuestDifficulty targetDifficulty)
        {
            var quests = new List<string>();
            
            // Try to parse the region type
            if (System.Enum.TryParse<AssessmentQuizManager.RegionType>(regionType, out var regionTypeEnum))
            {
                // Get quests directly by difficulty from ConstructionQuestPool
                var regionData = ConstructionQuestPool.Instance.GetRegionQuestData(regionTypeEnum);
                if (regionData != null)
                {
                    // Get the quest list for the target difficulty
                    List<string> difficultyQuests = GetQuestListByDifficulty(regionData, targetDifficulty);
                    
                    if (difficultyQuests != null && difficultyQuests.Count > 0)
                    {
                        // Shuffle the quests
                        var shuffledQuests = new List<string>(difficultyQuests);
                        for (int i = shuffledQuests.Count - 1; i > 0; i--)
                        {
                            int j = Random.Range(0, i + 1);
                            string temp = shuffledQuests[i];
                            shuffledQuests[i] = shuffledQuests[j];
                            shuffledQuests[j] = temp;
                        }
                        
                        // Take the required number of quests
                        for (int i = 0; i < Mathf.Min(questCount, shuffledQuests.Count); i++)
                        {
                            quests.Add(shuffledQuests[i]);
                        }
                    }
                }
            }
            
            // If we don't have enough region-specific quests, add generic ones
            while (quests.Count < questCount)
            {
                quests.Add(GetGenericQuestForDifficulty(targetDifficulty));
            }
            
            return quests;
        }
        
        private List<string> GetQuestListByDifficulty(ConstructionQuestPool.RegionQuestData regionData, QuestDifficulty difficulty)
        {
            switch (difficulty)
            {
                case QuestDifficulty.Easy:
                    return regionData.easyQuests;
                case QuestDifficulty.Medium:
                    return regionData.mediumQuests;
                case QuestDifficulty.Hard:
                    return regionData.hardQuests;
                case QuestDifficulty.Expert:
                    return regionData.expertQuests;
                default:
                    return regionData.easyQuests;
            }
        }



        private QuestDifficulty DetermineQuestDifficulty(string questText)
        {
            string lowerText = questText.ToLower();
            
            // EASY: Short duration, simple actions
            if (lowerText.Contains("5 minute") || lowerText.Contains("2 minute") || 
                lowerText.Contains("quick") || lowerText.Contains("simple") || 
                lowerText.Contains("small") || lowerText.Contains("just") || 
                lowerText.Contains("30 second") || lowerText.Contains("1 minute") ||
                lowerText.Contains("take a") || lowerText.Contains("do a quick") ||
                lowerText.Contains("smile at") || lowerText.Contains("compliment"))
                return QuestDifficulty.Easy;
            
            // MEDIUM: Moderate duration, practice actions
            if (lowerText.Contains("10 minute") || lowerText.Contains("15 minute") ||
                lowerText.Contains("try") || lowerText.Contains("practice") ||
                lowerText.Contains("write down") || lowerText.Contains("read a") ||
                lowerText.Contains("watch a") || lowerText.Contains("learn") ||
                lowerText.Contains("organize") || lowerText.Contains("make a") ||
                lowerText.Contains("create") || lowerText.Contains("design") ||
                lowerText.Contains("call or video") || lowerText.Contains("share something"))
                return QuestDifficulty.Medium;
            
            // HARD: Longer duration, complete actions
            if (lowerText.Contains("20 minute") || lowerText.Contains("30 minute") ||
                lowerText.Contains("complete") || lowerText.Contains("go for a") ||
                lowerText.Contains("drink 8 glass") || lowerText.Contains("get at least 30 minute") ||
                lowerText.Contains("track your") || lowerText.Contains("do a set of"))
                return QuestDifficulty.Hard;
            
            // EXPERT: Very long duration, complex actions
            if (lowerText.Contains("60 minute") || lowerText.Contains("2 hour") ||
                lowerText.Contains("complete a full") || lowerText.Contains("spend the entire") ||
                lowerText.Contains("master") || lowerText.Contains("advanced"))
                return QuestDifficulty.Expert;
            
            // Default to Medium if unclear
            return QuestDifficulty.Medium;
        }

        private string GetGenericQuestForDifficulty(QuestDifficulty difficulty)
        {
            switch (difficulty)
            {
                case QuestDifficulty.Easy:
                    return "Take a 5-minute break and stretch";
                case QuestDifficulty.Medium:
                    return "Spend 15 minutes on a hobby you enjoy";
                case QuestDifficulty.Hard:
                    return "Complete a 30-minute workout session";
                case QuestDifficulty.Expert:
                    return "Spend 2 hours learning a new skill";
                default:
                    return "Complete a healthy habit task";
            }
        }
        
        public void CheckQuestCompletion(string buildingName, Vector3Int gridPosition, string completedQuestText)
        {
            Debug.Log($"CheckQuestCompletion called for {buildingName} at {gridPosition} with quest: {completedQuestText}");
            
            ConstructionProject project = GetProject(buildingName, gridPosition);
            if (project == null)
            {
                Debug.LogWarning($"No construction project found for {buildingName} at {gridPosition}");
                return;
            }
            
            Debug.Log($"Found project for {buildingName}. Master quests: {string.Join(", ", project.originalQuestTexts)}");
            Debug.Log($"Found project for {buildingName}. Active quests: {string.Join(", ", project.activeQuestTexts)}");
            
            if (project.originalQuestTexts.Contains(completedQuestText))
            {
                // Remove the completed quest from BOTH master list and active list
                project.originalQuestTexts.Remove(completedQuestText);
                project.activeQuestTexts.Remove(completedQuestText);
                project.completedSkipQuests++;
                
                Debug.Log($"Construction quest completed for {project.buildingName}: {completedQuestText}");
                Debug.Log($"  - Master quests remaining: {project.originalQuestTexts.Count}");
                Debug.Log($"  - Active quests remaining: {project.activeQuestTexts.Count}");
                Debug.Log($"  - Completed quests: {project.completedSkipQuests}");
                
                // Check if all quests are completed (master list is empty)
                if (project.originalQuestTexts.Count == 0)
                {
                    Debug.Log($"All construction quests completed for {project.buildingName}. Completing construction.");
                    project.isCompleted = true;
                    CompleteConstruction(project);
                }
            }
            else
            {
                Debug.Log($"Quest '{completedQuestText}' not found in project quests for {buildingName}");
            }
        }
        
        public void CheckQuestDeletion(string buildingName, Vector3Int gridPosition, string deletedQuestText)
        {
            Debug.Log($"CheckQuestDeletion called for {buildingName} at {gridPosition} with quest: {deletedQuestText}");
            
            ConstructionProject project = GetProject(buildingName, gridPosition);
            if (project == null)
            {
                Debug.LogWarning($"No construction project found for {buildingName} at {gridPosition}");
                return;
            }
            
            Debug.Log($"Found project for {buildingName}. Master quests: {string.Join(", ", project.originalQuestTexts)}");
            Debug.Log($"Found project for {buildingName}. Active quests: {string.Join(", ", project.activeQuestTexts)}");
            
            if (project.originalQuestTexts.Contains(deletedQuestText))
            {
                // Remove the quest ONLY from active list (keep in master list)
                project.activeQuestTexts.Remove(deletedQuestText);
                project.deletedSkipQuests++;
                
                Debug.Log($"Construction quest deleted for {project.buildingName}: {deletedQuestText}");
                Debug.Log($"  - Master quests remaining: {project.originalQuestTexts.Count}");
                Debug.Log($"  - Active quests remaining: {project.activeQuestTexts.Count}");
                Debug.Log($"  - Deleted quests: {project.deletedSkipQuests}");
                
                // Notify UI components immediately that a quest was deleted
                OnQuestDeleted?.Invoke(buildingName, gridPosition);
                
                // DON'T complete construction when quests are deleted - only when they're completed
            }
            else
            {
                Debug.Log($"Quest '{deletedQuestText}' not found in project quests for {buildingName}");
            }
        }
        
        private void RemoveConstructionQuests(ConstructionProject project)
        {
            // Find ToDoListManager in the scene
            ToDoListManager toDoListManager = FindFirstObjectByType<ToDoListManager>();
            if (toDoListManager == null) return;
            
            // Remove quests from To-Do List that match our active texts
            foreach (string questText in project.activeQuestTexts)
            {
                toDoListManager.RemoveToDo(questText);
            }
            
            // Clear our tracking
            project.originalQuestTexts.Clear();
            project.activeQuestTexts.Clear();
            project.skipQuestIds.Clear();
            project.totalSkipQuests = 0;
            project.completedSkipQuests = 0;
            project.deletedSkipQuests = 0;
        }
        
        // Debug method to test the system
        [ContextMenu("Test Construction System")]
        public void TestConstructionSystem()
        {
            Debug.Log("=== Testing Construction System ===");
            Debug.Log($"Active projects: {activeProjects.Count}");
            Debug.Log($"Current Time.time: {Time.time}");
            Debug.Log($"ConstructionManager Instance: {(Instance != null ? "Valid" : "NULL")}");
            
            if (activeProjects.Count == 0)
            {
                Debug.Log("No active construction projects found.");
                return;
            }
            
            foreach (var kvp in activeProjects)
            {
                var project = kvp.Value;
                Debug.Log($"Project: {project.buildingName} at {project.gridPosition}");
                Debug.Log($"  - Duration: {project.constructionDuration} seconds");
                Debug.Log($"  - Start time: {project.startTime}");
                Debug.Log($"  - Elapsed: {Time.time - project.startTime} seconds");
                Debug.Log($"  - Remaining: {project.constructionDuration - (Time.time - project.startTime)} seconds");
                Debug.Log($"  - Is completed: {project.isCompleted}");
                Debug.Log($"  - Quest count: {project.originalQuestTexts.Count}");
                Debug.Log($"  - Quests: {string.Join(", ", project.originalQuestTexts)}");
                
                // Check if this project should be completed
                float elapsedTime = Time.time - project.startTime;
                float remainingTime = project.constructionDuration - elapsedTime;
                if (remainingTime <= 0 && !project.isCompleted)
                {
                    Debug.LogWarning($"Project {project.buildingName} should be completed but isn't marked as completed!");
                }
            }
        }
        
        // Additional debug method to force complete all projects
        [ContextMenu("Force Complete All Projects")]
        public void ForceCompleteAllProjects()
        {
            Debug.Log("=== Force Completing All Projects ===");
            List<string> projectsToComplete = new List<string>();
            
            foreach (var kvp in activeProjects)
            {
                projectsToComplete.Add(kvp.Key);
            }
            
            foreach (string key in projectsToComplete)
            {
                if (activeProjects.TryGetValue(key, out ConstructionProject project))
                {
                    Debug.Log($"Force completing project: {project.buildingName}");
                    project.isCompleted = true;
                    CompleteConstruction(project);
                }
            }
        }
        
        // Method to clear all construction data (for debugging)
        [ContextMenu("Clear All Construction Data")]
        public void ClearAllConstructionData()
        {
            Debug.Log("=== Clearing All Construction Data ===");
            Debug.Log($"Clearing {activeProjects.Count} active projects");
            
            // Clean up quests from To-Do List
            ToDoListManager toDoListManager = FindFirstObjectByType<ToDoListManager>();
            if (toDoListManager != null)
            {
                foreach (var kvp in activeProjects)
                {
                    var project = kvp.Value;
                    foreach (string questText in project.originalQuestTexts)
                    {
                        toDoListManager.RemoveToDo(questText);
                    }
                }
            }
            
            activeProjects.Clear();
            Debug.Log("All construction data cleared");
        }
    }
}