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
    /// Integrates with shop systems to provide premium content
    /// </summary>
    public class SubscriptionManager : MonoBehaviour
    {
        [Header("Subscription Configuration")]
        [SerializeField] private string monthlySubscriptionId = "premium_monthly";
        [SerializeField] private string yearlySubscriptionId = "premium_yearly";
        [SerializeField] private float monthlyPrice = 4.99f;
        [SerializeField] private float yearlyPrice = 39.99f;

        [Header("Premium Features")]
        [SerializeField] private bool enablePremiumDecorChest = true;
        [SerializeField] private bool enablePremiumBuildings = true;
        [SerializeField] private bool enablePremiumResources = true;
        [SerializeField] private bool enablePremiumJournalFeatures = true;

        [Header("Premium Content")]
        [SerializeField] private List<string> premiumDecorItems = new List<string>();
        [SerializeField] private List<string> premiumBuildings = new List<string>();
        [SerializeField] private int premiumResourceBonus = 50; // Percentage bonus

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
        private async void ValidateSubscriptionStatus()
        {
            if (hasActiveSubscription)
            {
                // Check if subscription has expired
                if (DateTime.Now > subscriptionExpiryDate)
                {
                    await CancelSubscription();
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
        /// Purchase monthly subscription
        /// </summary>
        public async Task<bool> PurchaseMonthlySubscription()
        {
            try
            {
                Debug.Log("Attempting to purchase monthly subscription...");
                
                // Simulate purchase process
                // In production, integrate with Unity IAP
                await Task.Delay(2000);
                
                // Set subscription data
                hasActiveSubscription = true;
                currentSubscriptionId = monthlySubscriptionId;
                currentSubscriptionType = SubscriptionType.Monthly;
                subscriptionExpiryDate = DateTime.Now.AddMonths(1);
                
                SaveSubscriptionData();
                
                // Trigger events
                OnSubscriptionStatusChanged?.Invoke(true);
                UnlockPremiumFeatures();
                
                Debug.Log("Monthly subscription purchased successfully!");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Monthly subscription purchase failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Purchase yearly subscription
        /// </summary>
        public async Task<bool> PurchaseYearlySubscription()
        {
            try
            {
                Debug.Log("Attempting to purchase yearly subscription...");
                
                // Simulate purchase process
                await Task.Delay(2000);
                
                // Set subscription data
                hasActiveSubscription = true;
                currentSubscriptionId = yearlySubscriptionId;
                currentSubscriptionType = SubscriptionType.Yearly;
                subscriptionExpiryDate = DateTime.Now.AddYears(1);
                
                SaveSubscriptionData();
                
                // Trigger events
                OnSubscriptionStatusChanged?.Invoke(true);
                UnlockPremiumFeatures();
                
                Debug.Log("Yearly subscription purchased successfully!");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Yearly subscription purchase failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cancel current subscription
        /// </summary>
        public async Task<bool> CancelSubscription()
        {
            try
            {
                Debug.Log("Cancelling subscription...");
                
                // Simulate cancellation process
                await Task.Delay(1000);
                
                // Clear subscription data
                hasActiveSubscription = false;
                currentSubscriptionId = "";
                currentSubscriptionType = SubscriptionType.None;
                subscriptionExpiryDate = DateTime.MinValue;
                
                SaveSubscriptionData();
                
                // Trigger events
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
        /// Get premium resource bonus multiplier
        /// </summary>
        public float GetPremiumResourceBonus()
        {
            return hasActiveSubscription ? (1f + premiumResourceBonus / 100f) : 1f;
        }

        /// <summary>
        /// Get list of premium decor items
        /// </summary>
        public List<string> GetPremiumDecorItems()
        {
            return hasActiveSubscription ? new List<string>(premiumDecorItems) : new List<string>();
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

        // Public getters
        public bool HasActiveSubscription => hasActiveSubscription;
        public SubscriptionType CurrentSubscriptionType => currentSubscriptionType;
        public DateTime SubscriptionExpiryDate => subscriptionExpiryDate;
        public float MonthlyPrice => monthlyPrice;
        public float YearlyPrice => yearlyPrice;
    }
} 