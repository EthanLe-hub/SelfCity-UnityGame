using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace LifeCraft.Systems
{
    /// <summary>
    /// Comprehensive authentication manager supporting multiple sign-in methods
    /// and subscription management for premium features
    /// </summary>
    public class AuthenticationManager : MonoBehaviour
    {
        [Header("Authentication Settings")]
        [SerializeField] private bool enableGuestMode = true;
        [SerializeField] private bool enableGoogleSignIn = true;
        [SerializeField] private bool enableAppleSignIn = true;
        [SerializeField] private bool enableEmailSignIn = true;

        [Header("Subscription Settings")]
        [SerializeField] private string monthlySubscriptionId = "premium_monthly";
        [SerializeField] private string yearlySubscriptionId = "premium_yearly";

        [Header("Events")]
        public UnityEvent<AuthUser> OnUserSignedIn;
        public UnityEvent OnUserSignedOut;
        public UnityEvent<string> OnAuthenticationError;
        public UnityEvent<bool> OnSubscriptionStatusChanged;

        // Authentication state
        private AuthUser currentUser;
        private bool isAuthenticated = false;
        private bool hasPremiumSubscription = false;

        // Singleton pattern
        public static AuthenticationManager Instance { get; private set; }

        // User data structure
        [System.Serializable]
        public class AuthUser
        {
            public string userId;
            public string displayName;
            public string email;
            public string photoUrl;
            public AuthProvider provider;
            public DateTime lastSignInTime;
            public bool isEmailVerified;
            public Dictionary<string, object> customClaims;
        }

        public enum AuthProvider
        {
            Google,
            Apple,
            Email,
            Guest,
            Facebook,
            Steam
        }

        public enum AuthError
        {
            NetworkError,
            InvalidCredentials,
            UserNotFound,
            EmailAlreadyInUse,
            WeakPassword,
            Cancelled,
            Unknown
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
            InitializeAuthentication();
        }

        /// <summary>
        /// Initialize authentication systems
        /// </summary>
        private void InitializeAuthentication()
        {
            // Check for existing session
            CheckExistingSession();
            
            // Initialize platform-specific authentication
            #if UNITY_ANDROID
            if (enableGoogleSignIn)
                InitializeGoogleSignIn();
            #elif UNITY_IOS
            if (enableAppleSignIn)
                InitializeAppleSignIn();
            #endif
        }

        /// <summary>
        /// Check if user has existing session
        /// </summary>
        private void CheckExistingSession()
        {
            string savedUserId = PlayerPrefs.GetString("Auth_UserId", "");
            if (!string.IsNullOrEmpty(savedUserId))
            {
                // Restore user session
                RestoreUserSession(savedUserId);
            }
        }

        /// <summary>
        /// Sign in with Google
        /// </summary>
        public async Task<AuthResult> SignInWithGoogle()
        {
            try
            {
                Debug.Log("Attempting Google Sign-In...");
                
                // Simulate Google Sign-In for now
                // In production, integrate with Google Play Games Services
                await Task.Delay(1000); // Simulate network delay
                
                var user = new AuthUser
                {
                    userId = "google_" + System.Guid.NewGuid().ToString(),
                    displayName = "Google User",
                    email = "user@gmail.com",
                    provider = AuthProvider.Google,
                    lastSignInTime = DateTime.Now,
                    isEmailVerified = true
                };

                return await CompleteSignIn(user);
            }
            catch (Exception e)
            {
                Debug.LogError($"Google Sign-In failed: {e.Message}");
                return new AuthResult { Success = false, Error = AuthError.NetworkError };
            }
        }

        /// <summary>
        /// Sign in with Apple
        /// </summary>
        public async Task<AuthResult> SignInWithApple()
        {
            try
            {
                Debug.Log("Attempting Apple Sign-In...");
                
                // Simulate Apple Sign-In for now
                // In production, integrate with Apple Sign-In SDK
                await Task.Delay(1000);
                
                var user = new AuthUser
                {
                    userId = "apple_" + System.Guid.NewGuid().ToString(),
                    displayName = "Apple User",
                    email = "user@icloud.com",
                    provider = AuthProvider.Apple,
                    lastSignInTime = DateTime.Now,
                    isEmailVerified = true
                };

                return await CompleteSignIn(user);
            }
            catch (Exception e)
            {
                Debug.LogError($"Apple Sign-In failed: {e.Message}");
                return new AuthResult { Success = false, Error = AuthError.NetworkError };
            }
        }

        /// <summary>
        /// Sign in with email and password
        /// </summary>
        public async Task<AuthResult> SignInWithEmail(string email, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    return new AuthResult { Success = false, Error = AuthError.InvalidCredentials };
                }

                if (!IsValidEmail(email))
                {
                    return new AuthResult { Success = false, Error = AuthError.InvalidCredentials };
                }

                Debug.Log($"Attempting email sign-in for: {email}");
                
                // Simulate email authentication
                await Task.Delay(1000);
                
                var user = new AuthUser
                {
                    userId = "email_" + System.Guid.NewGuid().ToString(),
                    displayName = email.Split('@')[0],
                    email = email,
                    provider = AuthProvider.Email,
                    lastSignInTime = DateTime.Now,
                    isEmailVerified = false
                };

                return await CompleteSignIn(user);
            }
            catch (Exception e)
            {
                Debug.LogError($"Email Sign-In failed: {e.Message}");
                return new AuthResult { Success = false, Error = AuthError.NetworkError };
            }
        }

        /// <summary>
        /// Sign in as guest
        /// </summary>
        public async Task<AuthResult> SignInAsGuest()
        {
            try
            {
                Debug.Log("Attempting guest sign-in...");
                
                var user = new AuthUser
                {
                    userId = "guest_" + System.Guid.NewGuid().ToString(),
                    displayName = "Guest User",
                    email = "",
                    provider = AuthProvider.Guest,
                    lastSignInTime = DateTime.Now,
                    isEmailVerified = false
                };

                return await CompleteSignIn(user);
            }
            catch (Exception e)
            {
                Debug.LogError($"Guest Sign-In failed: {e.Message}");
                return new AuthResult { Success = false, Error = AuthError.NetworkError };
            }
        }

        /// <summary>
        /// Complete the sign-in process
        /// </summary>
        private async Task<AuthResult> CompleteSignIn(AuthUser user)
        {
            try
            {
                currentUser = user;
                isAuthenticated = true;

                // Save user session
                SaveUserSession(user);

                // Check subscription status
                await CheckSubscriptionStatus();

                // Trigger events
                OnUserSignedIn?.Invoke(user);

                Debug.Log($"Successfully signed in: {user.displayName} ({user.provider})");
                
                return new AuthResult { Success = true, User = user };
            }
            catch (Exception e)
            {
                Debug.LogError($"Sign-in completion failed: {e.Message}");
                return new AuthResult { Success = false, Error = AuthError.Unknown };
            }
        }

        /// <summary>
        /// Sign out current user
        /// </summary>
        public void SignOut()
        {
            if (currentUser != null)
            {
                Debug.Log($"Signing out user: {currentUser.displayName}");
                
                // Clear user data
                currentUser = null;
                isAuthenticated = false;
                hasPremiumSubscription = false;

                // Clear saved session
                PlayerPrefs.DeleteKey("Auth_UserId");
                PlayerPrefs.DeleteKey("Auth_UserData");
                PlayerPrefs.Save();

                // Trigger events
                OnUserSignedOut?.Invoke();
            }
        }

        /// <summary>
        /// Check subscription status
        /// </summary>
        public async Task<bool> CheckSubscriptionStatus()
        {
            try
            {
                // Simulate subscription check
                // In production, integrate with Unity IAP
                await Task.Delay(500);
                
                // For demo purposes, randomly assign premium status
                hasPremiumSubscription = UnityEngine.Random.Range(0, 3) == 0; // 33% chance
                
                OnSubscriptionStatusChanged?.Invoke(hasPremiumSubscription);
                
                Debug.Log($"Subscription status: {(hasPremiumSubscription ? "Premium" : "Free")}");
                return hasPremiumSubscription;
            }
            catch (Exception e)
            {
                Debug.LogError($"Subscription check failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Purchase premium subscription
        /// </summary>
        public async Task<bool> PurchasePremiumSubscription()
        {
            try
            {
                Debug.Log("Attempting to purchase premium subscription...");
                
                // Simulate purchase process
                // In production, integrate with Unity IAP
                await Task.Delay(2000);
                
                hasPremiumSubscription = true;
                OnSubscriptionStatusChanged?.Invoke(true);
                
                Debug.Log("Premium subscription purchased successfully!");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Premium purchase failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Save user session to PlayerPrefs
        /// </summary>
        private void SaveUserSession(AuthUser user)
        {
            PlayerPrefs.SetString("Auth_UserId", user.userId);
            PlayerPrefs.SetString("Auth_UserData", JsonUtility.ToJson(user));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Restore user session from PlayerPrefs
        /// </summary>
        private void RestoreUserSession(string userId)
        {
            try
            {
                string userDataJson = PlayerPrefs.GetString("Auth_UserData", "");
                if (!string.IsNullOrEmpty(userDataJson))
                {
                    currentUser = JsonUtility.FromJson<AuthUser>(userDataJson);
                    isAuthenticated = true;
                    
                    Debug.Log($"Restored session for: {currentUser.displayName}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to restore user session: {e.Message}");
                SignOut();
            }
        }

        /// <summary>
        /// Validate email format
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // Platform-specific initialization methods
        private void InitializeGoogleSignIn()
        {
            // Initialize Google Sign-In SDK
            Debug.Log("Initializing Google Sign-In...");
        }

        private void InitializeAppleSignIn()
        {
            // Initialize Apple Sign-In SDK
            Debug.Log("Initializing Apple Sign-In...");
        }

        // Public getters
        public bool IsAuthenticated => isAuthenticated;
        public bool HasPremiumSubscription => hasPremiumSubscription;
        public AuthUser CurrentUser => currentUser;

        // Auth result structure
        [System.Serializable]
        public class AuthResult
        {
            public bool Success;
            public AuthUser User;
            public AuthError Error;
        }
    }
} 