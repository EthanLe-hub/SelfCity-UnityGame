using System.Collections.Generic;
using UnityEngine;
using LifeCraft.Systems;

namespace LifeCraft.Systems
{
    /// <summary>
    /// Provides region-specific quests for the construction skip system.
    /// These quests have no currency rewards and are designed to encourage healthy habits.
    /// </summary>
    [CreateAssetMenu(fileName = "ConstructionQuestPool", menuName = "LifeCraft/Construction Quest Pool")]
    public class ConstructionQuestPool : ScriptableObject
    {
        // Singleton instance
        private static ConstructionQuestPool _instance;
        public static ConstructionQuestPool Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<ConstructionQuestPool>("ConstructionQuestPool");
                    if (_instance == null)
                    {
                        Debug.LogError("ConstructionQuestPool not found in Resources folder!");
                    }
                }
                return _instance;
            }
        }

        [System.Serializable]
        public class RegionQuestData
        {
            public AssessmentQuizManager.RegionType regionType;
            public List<string> easyQuests = new List<string>();
            public List<string> mediumQuests = new List<string>();
            public List<string> hardQuests = new List<string>();
            public List<string> expertQuests = new List<string>();
        }

        [Header("Region-Specific Quests")]
        [SerializeField] private List<RegionQuestData> regionQuests = new List<RegionQuestData>();

        [Header("Default Quests (if region not found)")]
        [SerializeField] private List<string> defaultEasyQuests = new List<string>();
        [SerializeField] private List<string> defaultMediumQuests = new List<string>();
        [SerializeField] private List<string> defaultHardQuests = new List<string>();
        [SerializeField] private List<string> defaultExpertQuests = new List<string>();

        // Public properties for Editor access
        public List<RegionQuestData> RegionQuests => regionQuests;
        public List<string> DefaultEasyQuests => defaultEasyQuests;
        public List<string> DefaultMediumQuests => defaultMediumQuests;
        public List<string> DefaultHardQuests => defaultHardQuests;
        public List<string> DefaultExpertQuests => defaultExpertQuests;

        // Public methods for Editor access
        public void ClearAllQuests()
        {
            regionQuests.Clear();
            defaultEasyQuests.Clear();
            defaultMediumQuests.Clear();
            defaultHardQuests.Clear();
            defaultExpertQuests.Clear();
        }

        public void AddRegionQuestData(RegionQuestData data)
        {
            regionQuests.Add(data);
        }

        /// <summary>
        /// Get a random quest for the specified region
        /// </summary>
        public string GetRandomQuest(string regionTypeString)
        {
            // Try to parse the region type
            if (System.Enum.TryParse<AssessmentQuizManager.RegionType>(regionTypeString, out var regionType))
            {
                return GetRandomQuest(regionType);
            }
            
            // Fallback to default quests if region parsing fails
            return GetRandomDefaultQuest();
        }

        /// <summary>
        /// Get a random quest for the specified region type
        /// </summary>
        public string GetRandomQuest(AssessmentQuizManager.RegionType regionType)
        {
            var regionData = GetRegionQuestData(regionType);
            if (regionData != null)
            {
                // Randomly select difficulty based on remaining construction time
                QuestDifficulty difficulty = GetRandomDifficulty();
                return GetRandomQuestByDifficulty(regionData, difficulty);
            }
            
            // Fallback to default quests
            return GetRandomDefaultQuest();
        }

        /// <summary>
        /// Get a random quest with specified difficulty for the region
        /// </summary>
        public string GetRandomQuest(AssessmentQuizManager.RegionType regionType, QuestDifficulty difficulty)
        {
            var regionData = GetRegionQuestData(regionType);
            if (regionData != null)
            {
                return GetRandomQuestByDifficulty(regionData, difficulty);
            }
            
            // Fallback to default quests
            return GetRandomDefaultQuestByDifficulty(difficulty);
        }
        
        /// <summary>
        /// Get all quests for the specified region type
        /// </summary>
        public List<string> GetQuestsForRegion(string regionTypeString)
        {
            // Try to parse the region type
            if (System.Enum.TryParse<AssessmentQuizManager.RegionType>(regionTypeString, out var regionType))
            {
                return GetQuestsForRegion(regionType);
            }
            
            // Fallback to default quests if region parsing fails
            return GetAllDefaultQuests();
        }
        
        /// <summary>
        /// Get all quests for the specified region type
        /// </summary>
        public List<string> GetQuestsForRegion(AssessmentQuizManager.RegionType regionType)
        {
            var regionData = GetRegionQuestData(regionType);
            if (regionData != null)
            {
                var allQuests = new List<string>();
                allQuests.AddRange(regionData.easyQuests);
                allQuests.AddRange(regionData.mediumQuests);
                allQuests.AddRange(regionData.hardQuests);
                allQuests.AddRange(regionData.expertQuests);
                return allQuests;
            }
            
            // Fallback to default quests if region not found
            return GetAllDefaultQuests();
        }

        /// <summary>
        /// Get region quest data for the specified region type
        /// </summary>
        public RegionQuestData GetRegionQuestData(AssessmentQuizManager.RegionType regionType)
        {
            foreach (var regionData in regionQuests)
            {
                if (regionData.regionType == regionType)
                {
                    return regionData;
                }
            }
            return null;
        }

        /// <summary>
        /// Get a random quest by difficulty from region data
        /// </summary>
        private string GetRandomQuestByDifficulty(RegionQuestData regionData, QuestDifficulty difficulty)
        {
            List<string> questList = GetQuestListByDifficulty(regionData, difficulty);
            
            if (questList != null && questList.Count > 0)
            {
                int randomIndex = Random.Range(0, questList.Count);
                return questList[randomIndex];
            }
            
            // Fallback to default quests if region quests are empty
            return GetRandomDefaultQuestByDifficulty(difficulty);
        }

        /// <summary>
        /// Get quest list by difficulty from region data
        /// </summary>
        private List<string> GetQuestListByDifficulty(RegionQuestData regionData, QuestDifficulty difficulty)
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

        /// <summary>
        /// Get a random default quest
        /// </summary>
        private string GetRandomDefaultQuest()
        {
            QuestDifficulty difficulty = GetRandomDifficulty();
            return GetRandomDefaultQuestByDifficulty(difficulty);
        }

        /// <summary>
        /// Get a random default quest by difficulty
        /// </summary>
        private string GetRandomDefaultQuestByDifficulty(QuestDifficulty difficulty)
        {
            List<string> questList = GetDefaultQuestListByDifficulty(difficulty);
            
            if (questList != null && questList.Count > 0)
            {
                int randomIndex = Random.Range(0, questList.Count);
                return questList[randomIndex];
            }
            
            // Ultimate fallback
            return "Complete a healthy habit of your choice";
        }

        /// <summary>
        /// Get default quest list by difficulty
        /// </summary>
        private List<string> GetDefaultQuestListByDifficulty(QuestDifficulty difficulty)
        {
            switch (difficulty)
            {
                case QuestDifficulty.Easy:
                    return defaultEasyQuests;
                case QuestDifficulty.Medium:
                    return defaultMediumQuests;
                case QuestDifficulty.Hard:
                    return defaultHardQuests;
                case QuestDifficulty.Expert:
                    return defaultExpertQuests;
                default:
                    return defaultEasyQuests;
            }
        }
        
        /// <summary>
        /// Get all default quests
        /// </summary>
        private List<string> GetAllDefaultQuests()
        {
            var allQuests = new List<string>();
            allQuests.AddRange(defaultEasyQuests);
            allQuests.AddRange(defaultMediumQuests);
            allQuests.AddRange(defaultHardQuests);
            allQuests.AddRange(defaultExpertQuests);
            return allQuests;
        }

        /// <summary>
        /// Get random difficulty based on construction time remaining
        /// </summary>
        private QuestDifficulty GetRandomDifficulty()
        {
            // For construction quests, we want to encourage completion
            // So we lean towards easier difficulties
            float random = Random.Range(0f, 1f);
            
            if (random < 0.4f) // 40% chance
                return QuestDifficulty.Easy;
            else if (random < 0.7f) // 30% chance
                return QuestDifficulty.Medium;
            else if (random < 0.9f) // 20% chance
                return QuestDifficulty.Hard;
            else // 10% chance
                return QuestDifficulty.Expert;
        }

        /// <summary>
        /// Initialize default quests if they're empty
        /// </summary>
        private void OnValidate()
        {
            // Initialize default quests if they're empty
            if (defaultEasyQuests.Count == 0)
            {
                defaultEasyQuests.AddRange(new string[]
                {
                    "Take 5 deep breaths",
                    "Drink a glass of water",
                    "Stand up and stretch for 30 seconds",
                    "Look away from screen for 20 seconds",
                    "Take a short walk around the room"
                });
            }

            if (defaultMediumQuests.Count == 0)
            {
                defaultMediumQuests.AddRange(new string[]
                {
                    "Do 10 jumping jacks",
                    "Practice mindful breathing for 2 minutes",
                    "Call a friend or family member",
                    "Write down 3 things you're grateful for",
                    "Do some light stretching exercises"
                });
            }

            if (defaultHardQuests.Count == 0)
            {
                defaultHardQuests.AddRange(new string[]
                {
                    "Go for a 15-minute walk outside",
                    "Practice a hobby for 30 minutes",
                    "Cook a healthy meal from scratch",
                    "Read a book for 20 minutes",
                    "Do a 10-minute meditation session"
                });
            }

            if (defaultExpertQuests.Count == 0)
            {
                defaultExpertQuests.AddRange(new string[]
                {
                    "Complete a full workout session",
                    "Learn something new for 1 hour",
                    "Volunteer or help someone in need",
                    "Create something artistic",
                    "Plan and execute a personal goal"
                });
            }
        }
    }
} 