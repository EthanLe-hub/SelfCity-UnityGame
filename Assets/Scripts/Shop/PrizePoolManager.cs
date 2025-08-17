using System; // Using System for DateTime and other system functionalities. 
using System.Collections.Generic; // Using System.Collections.Generic for List<T> to store prize pool items. 
using UnityEngine; // We need UnityEngine for MonoBehaviour and ScriptableObject.

namespace LifeCraft.Shop
{
    public class PrizePoolManager : MonoBehaviour
    {
        [Header("References")] // Header for organization in the Unity Inspector.
        public DecorationDatabase decorationDatabase; // Add this field to the Inspector to link the DecorationDatabase lists (freeAndPremiumDecorations and premiumOnlyDecorations). 

        [Header("Prize Pool Settings")] // Header for organization in the Unity Inspector.
        public int freeAndPremiumPoolSize = 15; // Number of items in the free and premium prize pool.
        public int premiumOnlyPoolSize = 9; // Number of items in the premium only prize pool. 

        public event System.Action OnPrizePoolReset; // Event to notify when the prize pools are reset. 

        [Header("Current Prize Pools (Read Only)")] // Header for organization in the Unity Inspector.
        public List<string> currentFreeAndPremiumPool = new List<string>(); // Current prize pool for free and premium players. 
        public List<string> currentPremiumOnlyPool = new List<string>(); // Current prize pool for premium only players. 

        private const string LastResetKey = "PrizePoolLastReset"; // Key for PlayerPrefs to store the last reset date. 
        private const string FreeAndPremiumPoolKey = "PrizePoolFreeAndPremium"; // Key for PlayerPrefs to store the free and premium prize pool.
        private const string PremiumOnlyPoolKey = "PrizePoolPremiumOnly"; // Key for PlayerPrefs to store the premium only prize pool.

        private void Awake()
        {
            LoadOrResetPrizePools(); // Load or reset the prize pools when the game starts. 
        }

        private void LoadOrResetPrizePools()
        {
            DateTime lastReset = DateTime.MinValue; // Initialize the last reset date to the minimum value. 
            if (PlayerPrefs.HasKey(LastResetKey))
            {
                long binary = Convert.ToInt64(PlayerPrefs.GetString(LastResetKey)); // Get the last reset date from PlayerPrefs. 
                lastReset = DateTime.FromBinary(binary); // Convert the binary value to a DateTime object. 
            }

            if ((DateTime.UtcNow - lastReset).TotalHours >= 24 || !PlayerPrefs.HasKey(LastResetKey))
            {
                // Time to reset the prize pools!
                ResetPrizePools();
            }

            else
            {
                // Load from PlayerPrefs; PlayerPrefs are persistent data storage in Unity. 
                currentFreeAndPremiumPool = new List<string>(PlayerPrefs.GetString(FreeAndPremiumPoolKey).Split('|')); // Split the stored string into a list of items. 
                currentPremiumOnlyPool = new List<string>(PlayerPrefs.GetString(PremiumOnlyPoolKey).Split('|')); // Split the stored string into a list of items. 
            }
        }

        private void ResetPrizePools()
        {
            // Randomly select unique decorations for each pool:
            currentFreeAndPremiumPool = GetRandomUniqueDecorations(decorationDatabase.freeAndPremiumDecorations, freeAndPremiumPoolSize); // Get random unique decorations for the free and premium pool. 
            currentPremiumOnlyPool = GetRandomUniqueDecorations(decorationDatabase.premiumOnlyDecorations, premiumOnlyPoolSize); // Get random unique decorations for the premium only pool. 

            // Save to PlayerPrefs:
            PlayerPrefs.SetString(LastResetKey, DateTime.UtcNow.ToBinary().ToString()); // Save the current date as the last reset date in PlayerPrefs. 
            PlayerPrefs.SetString(FreeAndPremiumPoolKey, string.Join("|", currentFreeAndPremiumPool)); // Save the free and premium pool to PlayerPrefs as a string.
            PlayerPrefs.SetString(PremiumOnlyPoolKey, string.Join("|", currentPremiumOnlyPool)); // Save the premium only pool to PlayerPrefs as a string. 
            PlayerPrefs.Save(); // Save the PlayerPrefs to persist the data. 

            OnPrizePoolReset?.Invoke(); // Invoke the event to notify subscribers that the prize pools have been reset. 
        }

        private List<string> GetRandomUniqueDecorations(List<string> source, int count) // Get random unique decorations for a prize pool. 
        {
            List<string> copy = new List<string>(source); // Create a copy of the source list to avoid modifying the original. 
            List<string> result = new List<string>(); // Create a new list to store the selected decorations for the prize pool. 
            System.Random rng = new System.Random(); // Create a new random number generator. 

            for (int i = 0; i < count && copy.Count > 0; i++) // Loop until we have selected the desired number of items for the prize pool or until there are no more items left in the source list (latter should never happen). 
            {
                int idx = rng.Next(copy.Count); // Get a random index from the copy list (from the original source list). 
                result.Add(copy[idx]); // Fetch the item at the random index from the original source list, and add it to the result list (our current prize pool). 
                copy.RemoveAt(idx); // Remove the item from the copy list to ensure uniqueness in the prize pool (the original ORIGINAL source list from DecorationDatabase.cs is NOT modified). 
            }

            return result; // Return the list of selected decorations for the prize pool. 
        }

        // Call this to open a Decor Chest (free & premium)
        public string GetRandomFreeAndPremiumReward() // This method returns a random decoration from the free and premium prize pool. 
        {
            if (currentFreeAndPremiumPool.Count == 0) return null;
            int idx = UnityEngine.Random.Range(0, currentFreeAndPremiumPool.Count); // Get a random index from the current free and premium pool. 
            return currentFreeAndPremiumPool[idx]; // Return the decoration at the random index from the current free and premium pool. 
        }

        // Call this to open a Premium Decor Chest (premium only)
        public string GetRandomPremiumOnlyReward() // This method returns a random decoration from the premium only prize pool.
        {
            if (currentPremiumOnlyPool.Count == 0) return null;
            int idx = UnityEngine.Random.Range(0, currentPremiumOnlyPool.Count); // Get a random index from the current premium only pool.
            return currentPremiumOnlyPool[idx]; // Return the decoration at the random index from the current premium only pool.
        }

        // For debugging/testing: force a manual reset
        [ContextMenu("Force Reset Prize Pools")]
        public void ForceResetPrizePools()
        {
            ResetPrizePools();
            Debug.Log("Prize pools manually reset!");
        }

        public DateTime GetLastResetTime() // This method returns the last reset time of the prize pools. 
        {
            if (PlayerPrefs.HasKey("PrizePoolLastReset"))
            {
                long binary = Convert.ToInt64(PlayerPrefs.GetString("PrizePoolLastReset")); // Get the last reset date from PlayerPrefs. 
                return DateTime.FromBinary(binary); // Convert the binary value to a DateTime object and return it. 
            }
            return DateTime.UtcNow; // If no last reset date is found, return the current UTC time. 
        }
    }
}