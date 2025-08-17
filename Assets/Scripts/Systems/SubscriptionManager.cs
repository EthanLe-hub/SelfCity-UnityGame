using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Events;
using LifeCraft.Shop;

namespace LifeCraft.Systems
{
    /// <summary>
    /// Manages premium subscriptions and premium feature access
    /// SIMULATION MODE: Uses PlayerPrefs for testing, no real purchases
    /// </summary>
    public class SubscriptionManager : MonoBehaviour
    {
        [Header("Subscription Configuration")]
        [SerializeField] private string monthlySubscriptionId = "premium_monthly";
        [SerializeField] private string yearlySubscriptionId = "premium_yearly";
        [SerializeField] private float monthlyPrice = 6.99f; // Updated to $6.99
        [SerializeField] private float yearlyPrice = 69.99f; // ~17% discount for yearly

        [Header("Premium Features")]
        [SerializeField] private bool enablePremiumDecorChest = true;
        [SerializeField] private bool enablePremiumBuildings = true;
        [SerializeField] private bool enablePremiumResources = true;
        [SerializeField] private bool enablePremiumJournalFeatures = true;
        [SerializeField] private bool enableFasterConstruction = true; // New: 20% faster construction
        [SerializeField] private bool enableUnlimitedFriends = true; // New: Unlimited friends list

        [Header("Premium Content")]
        [SerializeField] private List<string> premiumDecorItems = new List<string>();
        [SerializeField] private List<string> premiumBuildings = new List<string>();
        [SerializeField] private int premiumResourceBonus = 50; // Percentage bonus
        [SerializeField] private float constructionTimeReduction = 0.2f; // 20% reduction

        [Header("Events")]
        public UnityEvent<bool> OnSubscriptionStatusChanged;
        public UnityEvent<string> OnPremiumFeatureUnlocked;
        public UnityEvent<string> OnPremiumFeatureLocked;

        // Subscription state
        private bool hasActiveSubscription = false;
        private string currentSubscriptionId = "";
        private DateTime subscriptionExpiryDate;
        private SubscriptionType currentSubscriptionType;

        // Singleton pattern
        public static SubscriptionManager Instance { get; private set; }

        public enum SubscriptionType
        {
            None,
            Monthly,
            Yearly
        }

        [System.Serializable]
        public class PremiumFeature
        {
            public string featureId;
            public string displayName;
            public string description;
            public bool isUnlocked;
            public Sprite icon;
        }

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

        private void Start()
        {
            InitializeSubscriptionManager();
            LoadPremiumDecorItemsFromSystems(); // Auto-load premium decor items
        }

        /// <summary>
        /// Initialize subscription manager
        /// </summary>
        private void InitializeSubscriptionManager()
        {
            LoadSubscriptionData();
            ValidateSubscriptionStatus();
        }

        /// <summary>
        /// Load subscription data from PlayerPrefs
        /// </summary>
        private void LoadSubscriptionData()
        {
            hasActiveSubscription = PlayerPrefs.GetInt("Subscription_Active", 0) == 1;
            currentSubscriptionId = PlayerPrefs.GetString("Subscription_Id", "");
            
            string expiryString = PlayerPrefs.GetString("Subscription_Expiry", "");
            if (!string.IsNullOrEmpty(expiryString))
            {
                subscriptionExpiryDate = DateTime.Parse(expiryString);
            }

            currentSubscriptionType = (SubscriptionType)PlayerPrefs.GetInt("Subscription_Type", 0);
        }

        /// <summary>
        /// Save subscription data to PlayerPrefs
        /// </summary>
        private void SaveSubscriptionData()
        {
            PlayerPrefs.SetInt("Subscription_Active", hasActiveSubscription ? 1 : 0);
            PlayerPrefs.SetString("Subscription_Id", currentSubscriptionId);
            PlayerPrefs.SetString("Subscription_Expiry", subscriptionExpiryDate.ToString());
            PlayerPrefs.SetInt("Subscription_Type", (int)currentSubscriptionType);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Validate current subscription status
        /// </summary>
        public void ValidateSubscriptionStatus()
        {
            if (hasActiveSubscription)
            {
                // Check if subscription has expired
                if (DateTime.Now > subscriptionExpiryDate)
                {
                    _ = CancelSubscription(); // Fire and forget
                }
                else
                {
                    // Subscription is still valid
                    OnSubscriptionStatusChanged?.Invoke(true);
                    UnlockPremiumFeatures();
                }
            }
            else
            {
                OnSubscriptionStatusChanged?.Invoke(false);
                LockPremiumFeatures();
            }
        }

        /// <summary>
        /// SIMULATION: Purchase monthly subscription (for testing)
        /// </summary>
        public async Task<bool> PurchaseMonthlySubscription()
        {
            try
            {
                Debug.Log("SIMULATION: Purchasing monthly subscription...");
                
                // Simulate purchase delay
                await Task.Delay(1000);
                
                // Handle monthly subscription purchase
                hasActiveSubscription = true;
                currentSubscriptionId = monthlySubscriptionId;
                currentSubscriptionType = SubscriptionType.Monthly;
                subscriptionExpiryDate = DateTime.Now.AddMonths(1);
                
                SaveSubscriptionData();
                OnSubscriptionStatusChanged?.Invoke(true);
                UnlockPremiumFeatures();
                
                Debug.Log("SIMULATION: Monthly subscription purchased successfully!");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Monthly subscription purchase failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// SIMULATION: Purchase yearly subscription (for testing)
        /// </summary>
        public async Task<bool> PurchaseYearlySubscription()
        {
            try
            {
                Debug.Log("SIMULATION: Purchasing yearly subscription...");
                
                // Simulate purchase delay
                await Task.Delay(1000);
                
                // Handle yearly subscription purchase
                hasActiveSubscription = true;
                currentSubscriptionId = yearlySubscriptionId;
                currentSubscriptionType = SubscriptionType.Yearly;
                subscriptionExpiryDate = DateTime.Now.AddYears(1);
                
                SaveSubscriptionData();
                OnSubscriptionStatusChanged?.Invoke(true);
                UnlockPremiumFeatures();
                
                Debug.Log("SIMULATION: Yearly subscription purchased successfully!");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Yearly subscription purchase failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cancel subscription
        /// </summary>
        public async Task<bool> CancelSubscription()
        {
            try
            {
                Debug.Log("Cancelling subscription...");
                
                hasActiveSubscription = false;
                currentSubscriptionId = "";
                currentSubscriptionType = SubscriptionType.None;
                
                SaveSubscriptionData();
                
                OnSubscriptionStatusChanged?.Invoke(false);
                LockPremiumFeatures();
                
                Debug.Log("Subscription cancelled successfully!");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Subscription cancellation failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Unlock premium features
        /// </summary>
        private void UnlockPremiumFeatures()
        {
            if (enablePremiumDecorChest)
            {
                OnPremiumFeatureUnlocked?.Invoke("Premium Decor Chest");
                Debug.Log("Premium Decor Chest unlocked!");
            }

            if (enablePremiumBuildings)
            {
                OnPremiumFeatureUnlocked?.Invoke("Premium Buildings");
                Debug.Log("Premium Buildings unlocked!");
            }

            if (enablePremiumResources)
            {
                OnPremiumFeatureUnlocked?.Invoke("Premium Resources");
                Debug.Log("Premium Resources unlocked!");
            }

            if (enablePremiumJournalFeatures)
            {
                OnPremiumFeatureUnlocked?.Invoke("Premium Journal Features");
                Debug.Log("Premium Journal Features unlocked!");
            }

            if (enableFasterConstruction)
            {
                OnPremiumFeatureUnlocked?.Invoke("Faster Construction");
                Debug.Log("Faster Construction unlocked!");
            }

            if (enableUnlimitedFriends)
            {
                OnPremiumFeatureUnlocked?.Invoke("Unlimited Friends");
                Debug.Log("Unlimited Friends unlocked!");
            }
        }

        /// <summary>
        /// Lock premium features
        /// </summary>
        private void LockPremiumFeatures()
        {
            if (enablePremiumDecorChest)
            {
                OnPremiumFeatureLocked?.Invoke("Premium Decor Chest");
            }

            if (enablePremiumBuildings)
            {
                OnPremiumFeatureLocked?.Invoke("Premium Buildings");
            }

            if (enablePremiumResources)
            {
                OnPremiumFeatureLocked?.Invoke("Premium Resources");
            }

            if (enablePremiumJournalFeatures)
            {
                OnPremiumFeatureLocked?.Invoke("Premium Journal Features");
            }

            if (enableFasterConstruction)
            {
                OnPremiumFeatureLocked?.Invoke("Faster Construction");
            }

            if (enableUnlimitedFriends)
            {
                OnPremiumFeatureLocked?.Invoke("Unlimited Friends");
            }
        }

        /// <summary>
        /// Check if user has active subscription
        /// </summary>
        public bool HasActiveSubscription()
        {
            return hasActiveSubscription;
        }

        /// <summary>
        /// Check if user has access to premium decor chest
        /// </summary>
        public bool HasPremiumDecorChestAccess()
        {
            return hasActiveSubscription && enablePremiumDecorChest;
        }

        /// <summary>
        /// Check if user has access to premium buildings
        /// </summary>
        public bool HasPremiumBuildingsAccess()
        {
            return hasActiveSubscription && enablePremiumBuildings;
        }

        /// <summary>
        /// Check if user has access to premium resources
        /// </summary>
        public bool HasPremiumResourcesAccess()
        {
            return hasActiveSubscription && enablePremiumResources;
        }

        /// <summary>
        /// Check if user has access to premium journal features
        /// </summary>
        public bool HasPremiumJournalAccess()
        {
            return hasActiveSubscription && enablePremiumJournalFeatures;
        }

        /// <summary>
        /// Check if user has faster construction
        /// </summary>
        public bool HasFasterConstruction()
        {
            return hasActiveSubscription && enableFasterConstruction;
        }

        /// <summary>
        /// Check if user has unlimited friends
        /// </summary>
        public bool HasUnlimitedFriends()
        {
            return hasActiveSubscription && enableUnlimitedFriends;
        }

        /// <summary>
        /// Get premium resource bonus multiplier
        /// </summary>
        public float GetPremiumResourceBonus()
        {
            return hasActiveSubscription ? (1f + premiumResourceBonus / 100f) : 1f;
        }

        /// <summary>
        /// Get construction time reduction multiplier
        /// </summary>
        public float GetConstructionTimeReduction()
        {
            return hasActiveSubscription ? (1f - constructionTimeReduction) : 1f;
        }

        /// <summary>
        /// Get list of premium decor items (dynamically loaded)
        /// </summary>
        public List<string> GetPremiumDecorItems()
        {
            if (hasActiveSubscription)
            {
                // Try to load from DecorationDatabase first
                var decorDatabase = Resources.Load<DecorationDatabase>("DecorationDatabase");
                if (decorDatabase != null)
                {
                    return decorDatabase.GetPremiumDecorItems();
                }
                
                // Fallback to CityBuilder
                if (LifeCraft.Core.CityBuilder.Instance != null)
                {
                    return LifeCraft.Core.CityBuilder.Instance.GetPremiumDecorItems();
                }
                
                // Final fallback to hardcoded list
                return new List<string>(premiumDecorItems);
            }
            
            return new List<string>();
        }

        /// <summary>
        /// Load premium decor items from existing systems
        /// </summary>
        public void LoadPremiumDecorItemsFromSystems()
        {
            premiumDecorItems.Clear();
            
            // Try DecorationDatabase first
            var decorDatabase = Resources.Load<DecorationDatabase>("DecorationDatabase");
            if (decorDatabase != null)
            {
                var items = decorDatabase.GetPremiumDecorItems();
                premiumDecorItems.AddRange(items);
                Debug.Log($"Loaded {items.Count} premium decor items from DecorationDatabase");
            }
            
            // Also try CityBuilder
            if (LifeCraft.Core.CityBuilder.Instance != null)
            {
                var cityBuilderItems = LifeCraft.Core.CityBuilder.Instance.GetPremiumDecorItems();
                foreach (var item in cityBuilderItems)
                {
                    if (!premiumDecorItems.Contains(item))
                    {
                        premiumDecorItems.Add(item);
                    }
                }
                Debug.Log($"Added {cityBuilderItems.Count} premium decor items from CityBuilder");
            }
            
            Debug.Log($"Total premium decor items loaded: {premiumDecorItems.Count}");
        }

        /// <summary>
        /// Get list of premium buildings
        /// </summary>
        public List<string> GetPremiumBuildings()
        {
            return hasActiveSubscription ? new List<string>(premiumBuildings) : new List<string>();
        }

        /// <summary>
        /// Get subscription info
        /// </summary>
        public (bool isActive, SubscriptionType type, DateTime expiry, float price) GetSubscriptionInfo()
        {
            float price = currentSubscriptionType == SubscriptionType.Monthly ? monthlyPrice : 
                         currentSubscriptionType == SubscriptionType.Yearly ? yearlyPrice : 0f;
            
            return (hasActiveSubscription, currentSubscriptionType, subscriptionExpiryDate, price);
        }

        /// <summary>
        /// Get days remaining in subscription
        /// </summary>
        public int GetDaysRemaining()
        {
            if (!hasActiveSubscription) return 0;
            
            TimeSpan remaining = subscriptionExpiryDate - DateTime.Now;
            return Math.Max(0, (int)remaining.TotalDays);
        }

        /// <summary>
        /// Check if subscription is expiring soon (within 7 days)
        /// </summary>
        public bool IsSubscriptionExpiringSoon()
        {
            return hasActiveSubscription && GetDaysRemaining() <= 7;
        }

        /// <summary>
        /// SIMULATION: Reset subscription for testing
        /// </summary>
        public void ResetSubscriptionForTesting()
        {
            Debug.Log("[SubscriptionManager] Starting subscription reset...");
            
            // Clear all subscription data
            PlayerPrefs.DeleteKey("Subscription_Active");
            PlayerPrefs.DeleteKey("Subscription_Id");
            PlayerPrefs.DeleteKey("Subscription_Expiry");
            PlayerPrefs.DeleteKey("Subscription_Type");
            PlayerPrefs.Save();
            
            // Reset internal state
            hasActiveSubscription = false;
            currentSubscriptionId = "";
            currentSubscriptionType = SubscriptionType.None;
            subscriptionExpiryDate = DateTime.MinValue;
            
            Debug.Log("[SubscriptionManager] All subscription data cleared");
            
            // Reload data and validate
            LoadSubscriptionData();
            ValidateSubscriptionStatus();
            
            // Force UI update events
            OnSubscriptionStatusChanged?.Invoke(false);
            LockPremiumFeatures();
            
            Debug.Log("[SubscriptionManager] Subscription reset completed - Status: Free");
        }
    }
} 