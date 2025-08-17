using UnityEngine; // Base class for all Unity scripts. 

namespace LifeCraft.Systems
{
    public class MoodManager : MonoBehaviour
    {
        // Singleton instance to be used in other scripts - called before Start()
        public static MoodManager Instance { get; private set; }

        private void Awake()
        {
            // Check if an instance already exists:
            if (Instance == null)
            {
                // If no instance exists, set this as the instance
                Instance = this;

                // Make sure this object persists between scene loads:
                DontDestroyOnLoad(gameObject);
            }

            else
            {
                // If an instance already exists, destroy this duplicate:
                Destroy(gameObject);
            }
        }

        public static event System.Action<Mood> OnMoodChanged; // Event to notify when mood changes. 
        
        public enum Mood // An enum is a special "class" that represents a group of constants (unchangeable/read-only variables). 
        {
            Happy,
            Sad,
            Moody,
            Stressed,
            None
        }

        // Current mood of the player (default is None) -- Instance Variable. 
        private Mood currentMood = Mood.None;

        // Time tracking for mood persistence
        private float lastMoodSelectionTime = 0f;

        // Method to change the player's mood (will be called by MoodCheck.cs script when player selects their mood):
        public void ChangeMood(Mood newMood)
        {
            // Check if the new mood is different from the current mood
            if (currentMood != newMood)
            {
                Debug.Log("Changing mood from " + currentMood + " to " + newMood); 
                
                // Update the current mood:
                currentMood = newMood;
                
                // Record the time when mood was selected
                lastMoodSelectionTime = Time.time;

                // Trigger the mood change event to notify other systems
                OnMoodChanged?.Invoke(currentMood);

                Debug.Log("Mood changed to: " + currentMood); 
            }
        }

        // Method to get the player's current mood:
        public Mood GetCurrentMood()
        {
            return currentMood;
        }

        // Method to get the last mood selection time:
        public float GetLastMoodSelectionTime()
        {
            return lastMoodSelectionTime;
        }

        // Save mood data to PlayerPrefs
        public void SaveMoodData()
        {
            try
            {
                // Save current mood
                PlayerPrefs.SetInt("CurrentMood", (int)currentMood);
                
                // Save last mood selection time
                PlayerPrefs.SetFloat("LastMoodSelectionTime", lastMoodSelectionTime);
                
                PlayerPrefs.Save();
                Debug.Log($"Mood data saved: {currentMood} at time {lastMoodSelectionTime}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save mood data: {e.Message}");
            }
        }

        // Load mood data from PlayerPrefs
        public void LoadMoodData()
        {
            try
            {
                // Load current mood
                if (PlayerPrefs.HasKey("CurrentMood"))
                {
                    currentMood = (Mood)PlayerPrefs.GetInt("CurrentMood");
                    Debug.Log($"Mood data loaded: {currentMood}");
                }
                else
                {
                    Debug.Log("No saved mood data found, using default: None");
                    currentMood = Mood.None;
                }

                // Load last mood selection time
                if (PlayerPrefs.HasKey("LastMoodSelectionTime"))
                {
                    lastMoodSelectionTime = PlayerPrefs.GetFloat("LastMoodSelectionTime");
                    Debug.Log($"Last mood selection time loaded: {lastMoodSelectionTime}");
                }
                else
                {
                    Debug.Log("No saved mood selection time found, using current time");
                    lastMoodSelectionTime = Time.time;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load mood data: {e.Message}");
                // Set defaults on error
                currentMood = Mood.None;
                lastMoodSelectionTime = Time.time;
            }
        }

        // Clear mood save data (for debugging or resetting)
        public void ClearMoodData()
        {
            PlayerPrefs.DeleteKey("CurrentMood");
            PlayerPrefs.DeleteKey("LastMoodSelectionTime");
            PlayerPrefs.Save();
            Debug.Log("Mood data cleared");
        }

        // Check if it's time for a new mood check (24 hours since last selection)
        public bool IsTimeForMoodCheck()
        {
            float timeSinceLastMood = Time.time - lastMoodSelectionTime;
            float twentyFourHoursInSeconds = 24f * 60f * 60f; // 24 hours in seconds
            
            return timeSinceLastMood >= twentyFourHoursInSeconds;
        }

        // Get time remaining until next mood check
        public float GetTimeUntilNextMoodCheck()
        {
            float timeSinceLastMood = Time.time - lastMoodSelectionTime;
            float twentyFourHoursInSeconds = 24f * 60f * 60f; // 24 hours in seconds
            
            return Mathf.Max(0f, twentyFourHoursInSeconds - timeSinceLastMood);
        }
    }
}