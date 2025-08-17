/**
Usually handles the data and logic for quests:
- Storing quest data, progress, completion, rewards, etc.
- Handles quest logic, saving/loading, etc.
- Does not directly manage UI elements.
*/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace LifeCraft.Systems
{
    /// <summary>
    /// Global manager for handling daily quests. Provides a list of daily quests and ensures
    /// they only reset after a full 24-hour period has passed since they were first generated.
    /// </summary>
    public class QuestManager : MonoBehaviour
    {
        // Singleton instance
        public static QuestManager Instance { get; private set; }

        // Master list of all possible daily quests, categorized by area (distinct from region quests)
        private static readonly Dictionary<string, List<string>> DAILY_QUEST_POOL = new Dictionary<string, List<string>>
        {
            { "Health Harbor", new List<string>
                {
                    "Try a new stretch routine for 5 minutes. (Health Harbor)",
                    "Go to bed 30 minutes earlier than usual tonight. (Health Harbor)",
                    "Take a walk after a meal, even if it's just around your home. (Health Harbor)",
                    "Replace a sugary drink with water today. (Health Harbor)",
                    "Do 20 jumping jacks. (Health Harbor)",
                    "Prepare a healthy snack for yourself. (Health Harbor)",
                    "Stand up and move around for 2 minutes every hour today. (Health Harbor)",
                    "Take a relaxing bath or shower. (Health Harbor)",
                    "Do a quick posture check and adjust your sitting position. (Health Harbor)",
                    "Try a new fruit or vegetable today. (Health Harbor)",
                    "Spend 10 minutes outside, even if just on a balcony or by a window. (Health Harbor)",
                    "Do a wall sit for 30 seconds. (Health Harbor)",
                    "Take three deep breaths before each meal. (Health Harbor)",
                    "Swap a processed snack for a whole food. (Health Harbor)",
                    "Do a balance exercise, like standing on one foot for 30 seconds. (Health Harbor)",
                    "Write down your sleep schedule for the week. (Health Harbor)",
                    "Do a short YouTube workout video. (Health Harbor)",
                    "Drink a glass of water first thing in the morning. (Health Harbor)",
                    "Take a power nap (10–20 minutes) if you feel tired. (Health Harbor)",
                    "Go for a walk and notice five things you see. (Health Harbor)",
                    "Do 10 squats. (Health Harbor)",
                    "Try a new herbal tea. (Health Harbor)",
                    "Stretch your arms above your head for 1 minute. (Health Harbor)",
                    "Plan a healthy meal for tomorrow. (Health Harbor)",
                    "Do a breathing exercise before bed. (Health Harbor)"
                }
            },
            { "Mind Palace", new List<string>
                {
                    "Read a news article on a topic you know little about. (Mind Palace)",
                    "Write down three things you're grateful for. (Mind Palace)",
                    "Meditate for 5 minutes. (Mind Palace)",
                    "Listen to a new podcast episode. (Mind Palace)",
                    "Try a brain teaser or puzzle. (Mind Palace)",
                    "Write a journal entry about your day. (Mind Palace)",
                    "Spend 10 minutes learning a new skill online. (Mind Palace)",
                    "Read a chapter from a book. (Mind Palace)",
                    "Practice mindful breathing for 2 minutes. (Mind Palace)",
                    "Watch a documentary or educational video. (Mind Palace)",
                    "Write down a dream you remember. (Mind Palace)",
                    "Try a new language learning app for 10 minutes. (Mind Palace)",
                    "Draw or doodle something that represents your mood. (Mind Palace)",
                    "Write a letter to your future self. (Mind Palace)",
                    "List five things you like about yourself. (Mind Palace)",
                    "Spend 5 minutes in silence, just observing your thoughts. (Mind Palace)",
                    "Read a poem and reflect on its meaning. (Mind Palace)",
                    "Set a small goal for the day and check it off when done. (Mind Palace)",
                    "Try a guided meditation video. (Mind Palace)",
                    "Write down a positive affirmation and repeat it. (Mind Palace)",
                    "Organize your workspace or study area. (Mind Palace)",
                    "Take a break from screens for 30 minutes. (Mind Palace)",
                    "Listen to instrumental music and relax. (Mind Palace)",
                    "Write a short story or creative piece. (Mind Palace)",
                    "Research a topic you've always been curious about. (Mind Palace)"
                }
            },
            { "Social Square", new List<string>
                {
                    "Send a message to someone you haven't talked to in a while. (Social Square)",
                    "Compliment a friend or family member. (Social Square)",
                    "Share a funny meme or video with someone. (Social Square)",
                    "Call or video chat with a loved one. (Social Square)",
                    "Write a thank-you note (digital or paper) to someone. (Social Square)",
                    "Ask someone how their day is going. (Social Square)",
                    "Offer to help a friend or family member with a task. (Social Square)",
                    "Share something positive on social media. (Social Square)",
                    "Listen actively to someone's story or problem. (Social Square)",
                    "Invite someone to join you for a walk or activity. (Social Square)",
                    "Leave a positive review for a local business. (Social Square)",
                    "Tell someone why you appreciate them. (Social Square)",
                    "Smile at someone you pass by. (Social Square)",
                    "Share a favorite recipe with a friend. (Social Square)",
                    "Recommend a book, movie, or show to someone. (Social Square)",
                    "Ask a family member about their favorite memory. (Social Square)",
                    "Play an online game with a friend. (Social Square)",
                    "Share a photo that makes you happy with someone. (Social Square)",
                    "Thank someone for something small they did. (Social Square)",
                    "Offer words of encouragement to someone. (Social Square)",
                    "Ask someone about their hobbies or interests. (Social Square)",
                    "Share a motivational quote with a friend. (Social Square)",
                    "Make a new connection online (safely). (Social Square)",
                    "Tell a joke to make someone laugh. (Social Square)",
                    "Check in on a neighbor or acquaintance. (Social Square)"
                }
            },
            { "Creative Commons", new List<string>
                {
                    "Draw or sketch something from your imagination. (Creative Commons)",
                    "Take a photo of something that inspires you. (Creative Commons)",
                    "Write a haiku or short poem. (Creative Commons)",
                    "Try a new recipe and plate it creatively. (Creative Commons)",
                    "Make a collage from old magazines or papers. (Creative Commons)",
                    "Listen to a new genre of music. (Creative Commons)",
                    "Paint or color a picture. (Creative Commons)",
                    "Rearrange a small area of your room for a new look. (Creative Commons)",
                    "Write a song lyric or melody. (Creative Commons)",
                    "Create a simple craft using household items. (Creative Commons)",
                    "Try a new dance move or routine. (Creative Commons)",
                    "Make a playlist for a specific mood. (Creative Commons)",
                    "Write a short story or comic strip. (Creative Commons)",
                    "Decorate your workspace with something handmade. (Creative Commons)",
                    "Try origami or paper folding. (Creative Commons)",
                    "Take a creative photo using interesting angles. (Creative Commons)",
                    "Design a bookmark or greeting card. (Creative Commons)",
                    "Make a vision board for your goals. (Creative Commons)",
                    "Try a new art app or digital drawing tool. (Creative Commons)",
                    "Write a letter in calligraphy or fancy handwriting. (Creative Commons)",
                    "Create a doodle on a sticky note. (Creative Commons)",
                    "Make up a new recipe and name it. (Creative Commons)",
                    "Record a short video expressing an idea or feeling. (Creative Commons)",
                    "Build something with blocks, Legos, or other materials. (Creative Commons)",
                    "Try a new creative hobby for 10 minutes. (Creative Commons)"
                }
            }
        };

        private static readonly Dictionary<string, List<string>> MASTER_QUEST_LIST = new Dictionary<string, List<string>>
        {
            { "Health Harbor", new List<string>
                {
                    "Go for a 20-minute brisk walk. (Health Harbor)",
                    "Try a new healthy recipe for one of your meals. (Health Harbor)",
                    "Do 15 minutes of stretching or yoga. (Health Harbor)",
                    "Drink 8 glasses of water throughout the day. (Health Harbor)",
                    "Get at least 30 minutes of sunlight. (Health Harbor)",
                    "Track your sleep hours for one night. (Health Harbor)",
                    "Do a set of 10 push-ups. (Health Harbor)",
                    "Take the stairs instead of the elevator today. (Health Harbor)",
                    "Prepare a balanced breakfast. (Health Harbor)",
                    "Go to bed without any screens for 30 minutes before sleep. (Health Harbor)"
                }
            },
            { "Mind Palace", new List<string>
                {
                    "Practice 10 minutes of focused breathing meditation. (Mind Palace)",
                    "Write down a 'worry list' to get thoughts out of your head. (Mind Palace)",
                    "Unfollow social media accounts that don't make you feel good. (Mind Palace)",
                    "Listen to a podcast about mental wellness. (Mind Palace)",
                    "Spend 15 minutes in a quiet space with no digital distractions. (Mind Palace)",
                    "Read a chapter from a self-help book. (Mind Palace)",
                    "Try a new journaling prompt. (Mind Palace)",
                    "Do a crossword or sudoku puzzle. (Mind Palace)",
                    "Write down your goals for the week. (Mind Palace)",
                    "Spend 5 minutes visualizing a positive outcome for something you care about. (Mind Palace)"
                }
            },
            { "Social Square", new List<string>
                {
                    "Give a genuine compliment to a stranger. (Social Square)",
                    "Schedule a call or video chat with a friend for later this week. (Social Square)",
                    "Write a thank-you note to someone who has helped you. (Social Square)",
                    "Do a small favor for a family member or roommate. (Social Square)",
                    "Share something positive or interesting with a friend. (Social Square)",
                    "Invite a friend to join you for a walk or coffee. (Social Square)",
                    "Ask someone about their day and listen closely. (Social Square)",
                    "Reconnect with an old friend online. (Social Square)",
                    "Help someone with a task, even if it's small. (Social Square)",
                    "Share a story from your childhood with someone. (Social Square)"
                }
            },
            { "Creative Commons", new List<string>
                {
                    "Take a photo of something interesting on your walk. (Creative Commons)",
                    "Write a short, one-paragraph story. (Creative Commons)",
                    "Create a new music playlist for a specific mood. (Creative Commons)",
                    "Try a new hairstyle or accessory. (Creative Commons)",
                    "Rearrange a small part of your room. (Creative Commons)",
                    "Draw a picture of your favorite animal. (Creative Commons)",
                    "Write a poem about your day. (Creative Commons)",
                    "Make a simple craft using recycled materials. (Creative Commons)",
                    "Design a new logo for a fictional company. (Creative Commons)",
                    "Paint or color a scene from your favorite movie. (Creative Commons)"
                }
            }
        };

        // Cached daily quests and generation timestamp
        private List<string> _dailyQuests = new List<string>();
        private DateTime _generationTime = DateTime.MinValue;
        private const int QUEST_COUNT = 8;
        private const int SECONDS_IN_DAY = 86400;

        // Custom quests created by the player, categorized by region.
        // This dictionary stores lists of custom quest strings for each region (e.g., Health Harbor, Mind Palace, etc.).
        // When a player adds a custom quest, it is stored here with the region label appended (e.g., "Walk my dog (Health Harbor)").
        // These quests are shown in the Custom Task modal and can be added to the To-Do list like other quests.
        private Dictionary<string, List<string>> customQuests = new Dictionary<string, List<string>>
        {
            { "Health Harbor", new List<string>() },
            { "Mind Palace", new List<string>() },
            { "Social Square", new List<string>() },
            { "Creative Commons", new List<string>() }
        };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // Helper to generate a random set of quests from the specified quest pool.
        private List<string> GenerateRandomQuestsFromPool(Dictionary<string, List<string>> questPool, int count, int seed)
        {
            List<string> allQuests = new List<string>();
            foreach (var category in questPool.Values)
            {
                allQuests.AddRange(category);
            }
            // Shuffle with seed
            System.Random rng = new System.Random(seed);
            int n = allQuests.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                var value = allQuests[k];
                allQuests[k] = allQuests[n];
                allQuests[n] = value;
            }
            // Take the first 'count' quests
            return allQuests.GetRange(0, Mathf.Min(count, allQuests.Count));
        }

        /// <summary>
        /// Get the current list of daily quests. Generates a new list if 24 hours have passed.
        /// </summary>
        public List<string> GetDailyQuests(int count = QUEST_COUNT)
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan timeSinceGeneration = now - _generationTime;

            if (_generationTime == DateTime.MinValue || timeSinceGeneration.TotalSeconds > SECONDS_IN_DAY)
            {
                // Generate new daily quests from the daily quest pool
                _generationTime = now;
                _dailyQuests = GenerateRandomQuestsFromPool(DAILY_QUEST_POOL, count, (int)_generationTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            }
            return new List<string>(_dailyQuests);
        }

        /// <summary>
        /// Get the UNIX timestamp (seconds since 1970) when the current quests were generated.
        /// </summary>
        public int GetQuestGenerationTimestamp()
        {
            if (_generationTime == DateTime.MinValue)
                return 0;
            return (int)_generationTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        /// <summary>
        /// Get a completely random set of quests (not tied to the 24-hour timer).
        /// </summary>
        public List<string> GetQuestsSimple(int count = QUEST_COUNT)
        {
            int seed = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            return GenerateRandomQuestsFromPool(DAILY_QUEST_POOL, count, seed);
        }

        // Empty stub methods added for Unity migration compatibility
        public void Initialize() { }
        public void UpdateQuests() { }
        
        /// <summary>
        /// Save quest data to local device storage (PlayerPrefs)
        /// 
        /// UPDATE: Implemented local persistence for quest system data:
        /// - Daily quest generation timestamp (ensures 24-hour reset cycle)
        /// - Custom quests created by the player (organized by region)
        /// 
        /// This preserves quest progress and custom quests between game sessions.
        /// </summary>
        public void SaveQuests()
        {
            try
            {
                // Save daily quests generation time (for 24-hour reset logic)
                PlayerPrefs.SetString("QuestGenerationTime", _generationTime.ToString("O"));
                
                // Save custom quests (player-created quests by region)
                string customQuestsJson = JsonUtility.ToJson(new CustomQuestsWrapper { quests = customQuests });
                PlayerPrefs.SetString("CustomQuests", customQuestsJson);
                
                PlayerPrefs.Save(); // Force write to device storage
                Debug.Log("Quest data saved successfully to local storage!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save quests: {e.Message}");
            }
        }
        
        /// <summary>
        /// Load quest data from local device storage (PlayerPrefs)
        /// 
        /// UPDATE: Implemented local data loading for quest system:
        /// - Restores daily quest generation timestamp
        /// - Loads custom quests created by the player
        /// 
        /// This ensures quest continuity and preserves player-created content.
        /// </summary>
        public void LoadQuests()
        {
            try
            {
                // Load daily quests generation time (for 24-hour reset logic)
                if (PlayerPrefs.HasKey("QuestGenerationTime"))
                {
                    string timeStr = PlayerPrefs.GetString("QuestGenerationTime");
                    if (DateTime.TryParse(timeStr, out DateTime savedTime))
                    {
                        _generationTime = savedTime;
                    }
                }
                
                // Load custom quests (player-created quests by region)
                if (PlayerPrefs.HasKey("CustomQuests"))
                {
                    string customQuestsJson = PlayerPrefs.GetString("CustomQuests");
                    var wrapper = JsonUtility.FromJson<CustomQuestsWrapper>(customQuestsJson);
                    if (wrapper?.quests != null)
                    {
                        customQuests = wrapper.quests;
                    }
                }
                
                Debug.Log("Quest data loaded successfully from local storage!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load quests: {e.Message}");
            }
        }

        // Add this method to allow other scripts to fetch the STATIC region quests (different from Daily Quests) by name
        public List<string> GetRegionQuests(string regionName)
        {
            // Return a copy of the quest list for the given region, or an empty list if not found
            return MASTER_QUEST_LIST.ContainsKey(regionName) ? new List<string>(MASTER_QUEST_LIST[regionName]) : new List<string>();
        }

        /// <summary>
        /// Add a custom quest for a specific region. Appends the region label to the quest text.
        /// Called when the player enters a custom quest and selects a region in the Custom Task modal.
        /// The quest is stored in the customQuests dictionary and will appear in the modal's quest list.
        /// </summary>
        public void AddCustomQuest(string questText, string region)
        {
            if (!string.IsNullOrWhiteSpace(questText) && !string.IsNullOrWhiteSpace(region))
            {
                string questWithRegion = $"{questText.Trim()} ({region})";
                if (!customQuests[region].Contains(questWithRegion))
                    customQuests[region].Add(questWithRegion);
            }
        }

        /// <summary>
        /// Get all custom quests for a specific region.
        /// Used to display the player's custom quests for a region in the Custom Task modal.
        /// </summary>
        public List<string> GetCustomQuests(string region)
        {
            return customQuests.ContainsKey(region) ? new List<string>(customQuests[region]) : new List<string>();
        }

        /// <summary>
        /// Get all custom quests for all regions.
        /// Used to display all custom quests in the Custom Task modal, grouped by region.
        /// </summary>
        public Dictionary<string, List<string>> GetAllCustomQuests()
        {
            // Returns a copy of all custom quests by region
            return new Dictionary<string, List<string>>(customQuests);
        }

        // FIX: Remove a quest from the current daily quests list to keep UI and data in sync.
        public void RemoveDailyQuest(string questText)
        {
            _dailyQuests.Remove(questText);
        }

        /// <summary>
        /// Developer-only: Force reset daily quests and regenerate them.
        /// </summary>
        public void ResetDailyQuests()
        {
            Debug.Log("[DEV] Resetting daily quests via developer command.");
            // Clear current daily quests
            if (_dailyQuests != null)
                _dailyQuests.Clear();
            // Reset generation time
            _generationTime = DateTime.UtcNow;
            // Generate new daily quests
            _dailyQuests = GenerateRandomQuestsFromPool(DAILY_QUEST_POOL, QUEST_COUNT, (int)_generationTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            Debug.Log($"[DEV] Generated {_dailyQuests.Count} new daily quests.");
            // Save immediately
            SaveQuests();
            Debug.Log("[DEV] Daily quests have been reset and saved.");
        }
    }

    /// <summary>
    /// Wrapper class for custom quests serialization
    /// </summary>
    [System.Serializable]
    public class CustomQuestsWrapper
    {
        public Dictionary<string, List<string>> quests;
    }

    /*
    ================================================================================
    CLOUD SAVE INTEGRATION - TO BE IMPLEMENTED LATER
    ================================================================================
    
    When implementing user accounts and cloud save, replace the above save/load methods
    with cloud-based storage. This will enable cross-device sync and data backup.
    
    Example implementation structure:
    
    public async Task SaveQuestsToCloud(string userId)
    {
        try
        {
            var questData = new QuestSaveData
            {
                generationTime = _generationTime,
                customQuests = customQuests,
                saveTimestamp = DateTime.UtcNow,
                version = "1.0"
            };
            
            string jsonData = JsonUtility.ToJson(questData);
            await CloudSaveManager.Instance.SaveData(userId, "quests", jsonData);
            
            // Also save locally as backup
            SaveQuests();
        }
        catch (Exception e)
        {
            Debug.LogError($"Cloud save failed: {e.Message}");
            // Fall back to local save only
            SaveQuests();
        }
    }
    
    public async Task LoadQuestsFromCloud(string userId)
    {
        try
        {
            string jsonData = await CloudSaveManager.Instance.LoadData(userId, "quests");
            if (!string.IsNullOrEmpty(jsonData))
            {
                var questData = JsonUtility.FromJson<QuestSaveData>(jsonData);
                _generationTime = questData.generationTime;
                customQuests = questData.customQuests;
            }
            else
            {
                // No cloud data, load from local storage
                LoadQuests();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Cloud load failed: {e.Message}");
            // Fall back to local load
            LoadQuests();
        }
    }
    
    [System.Serializable]
    public class QuestSaveData
    {
        public DateTime generationTime;
        public Dictionary<string, List<string>> customQuests;
        public DateTime saveTimestamp;
        public string version;
    }
    */
} 