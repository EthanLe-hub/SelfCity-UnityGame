using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using LifeCraft.Systems;

namespace LifeCraft.UI
{
    /// <summary>
    /// Authentication UI component that provides multiple sign-in options
    /// and integrates with the ProfileManager for seamless user experience
    /// </summary>
    public class AuthenticationUI : MonoBehaviour
    {
        [Header("Authentication Panels")]
        [SerializeField] private GameObject signInPanel;
        [SerializeField] private GameObject profilePanel;
        [SerializeField] private GameObject loadingPanel;

        [Header("Sign-In Buttons")]
        [SerializeField] private Button googleSignInButton;
        [SerializeField] private Button appleSignInButton;
        [SerializeField] private Button emailSignInButton;
        [SerializeField] private Button guestSignInButton;

        [Header("Email Sign-In UI")]
        [SerializeField] private GameObject emailSignInForm;
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private Button emailSubmitButton;
        [SerializeField] private Button emailBackButton;

        [Header("Profile Display")]
        [SerializeField] private TMP_Text userDisplayNameText;
        [SerializeField] private TMP_Text userEmailText;
        [SerializeField] private TMP_Text subscriptionStatusText;
        [SerializeField] private Image userAvatarImage;
        [SerializeField] private Button signOutButton;
        [SerializeField] private Button upgradeToPremiumButton;

        [Header("Premium Features")]
        [SerializeField] private GameObject premiumBadge;
        [SerializeField] private GameObject premiumFeaturesPanel;

        [Header("UI Settings")]
        [SerializeField] private bool showGuestOption = true;
        [SerializeField] private bool showEmailOption = true;
        [SerializeField] private bool enableAnimations = true;

        private AuthenticationManager authManager;
        private ProfileManager profileManager;

        private void Start()
        {
            InitializeUI();
            SetupEventListeners();
            CheckAuthenticationStatus();
        }

        /// <summary>
        /// Initialize UI components
        /// </summary>
        private void InitializeUI()
        {
            authManager = AuthenticationManager.Instance;
            profileManager = ProfileManager.Instance;

            // Hide email form initially
            if (emailSignInForm != null)
                emailSignInForm.SetActive(false);

            // Hide premium features initially
            if (premiumFeaturesPanel != null)
                premiumFeaturesPanel.SetActive(false);

            // Show sign-in panel by default
            ShowSignInPanel();
        }

        /// <summary>
        /// Setup event listeners
        /// </summary>
        private void SetupEventListeners()
        {
            // Sign-in button listeners
            if (googleSignInButton != null)
                googleSignInButton.onClick.AddListener(OnGoogleSignInClicked);

            if (appleSignInButton != null)
                appleSignInButton.onClick.AddListener(OnAppleSignInClicked);

            if (emailSignInButton != null)
                emailSignInButton.onClick.AddListener(OnEmailSignInClicked);

            if (guestSignInButton != null)
                guestSignInButton.onClick.AddListener(OnGuestSignInClicked);

            // Email form listeners
            if (emailSubmitButton != null)
                emailSubmitButton.onClick.AddListener(OnEmailSubmitClicked);

            if (emailBackButton != null)
                emailBackButton.onClick.AddListener(OnEmailBackClicked);

            // Profile listeners
            if (signOutButton != null)
                signOutButton.onClick.AddListener(OnSignOutClicked);

            if (upgradeToPremiumButton != null)
                upgradeToPremiumButton.onClick.AddListener(OnUpgradeToPremiumClicked);

            // Authentication manager events
            if (authManager != null)
            {
                authManager.OnUserSignedIn.AddListener(OnUserSignedIn);
                authManager.OnUserSignedOut.AddListener(OnUserSignedOut);
                authManager.OnSubscriptionStatusChanged.AddListener(OnSubscriptionStatusChanged);
            }
        }

        /// <summary>
        /// Check current authentication status
        /// </summary>
        private void CheckAuthenticationStatus()
        {
            if (authManager != null && authManager.IsAuthenticated)
            {
                ShowProfilePanel();
                UpdateProfileDisplay();
            }
            else
            {
                ShowSignInPanel();
            }
        }

        /// <summary>
        /// Show sign-in panel
        /// </summary>
        private void ShowSignInPanel()
        {
            if (signInPanel != null)
                signInPanel.SetActive(true);

            if (profilePanel != null)
                profilePanel.SetActive(false);

            if (loadingPanel != null)
                loadingPanel.SetActive(false);
        }

        /// <summary>
        /// Show profile panel
        /// </summary>
        private void ShowProfilePanel()
        {
            if (signInPanel != null)
                signInPanel.SetActive(false);

            if (profilePanel != null)
                profilePanel.SetActive(true);

            if (loadingPanel != null)
                loadingPanel.SetActive(false);
        }

        /// <summary>
        /// Show loading panel
        /// </summary>
        private void ShowLoadingPanel()
        {
            if (loadingPanel != null)
                loadingPanel.SetActive(true);
        }

        /// <summary>
        /// Update profile display with current user data
        /// </summary>
        private void UpdateProfileDisplay()
        {
            if (authManager?.CurrentUser == null) return;

            var user = authManager.CurrentUser;

            // Update display name
            if (userDisplayNameText != null)
                userDisplayNameText.text = user.displayName;

            // Update email
            if (userEmailText != null)
                userEmailText.text = user.email;

            // Update subscription status
            if (subscriptionStatusText != null)
            {
                subscriptionStatusText.text = authManager.HasPremiumSubscription ? "Premium" : "Free";
                subscriptionStatusText.color = authManager.HasPremiumSubscription ? Color.green : Color.gray;
            }

            // Update premium badge
            if (premiumBadge != null)
                premiumBadge.SetActive(authManager.HasPremiumSubscription);

            // Update premium features panel
            if (premiumFeaturesPanel != null)
                premiumFeaturesPanel.SetActive(authManager.HasPremiumSubscription);

            // Update upgrade button visibility
            if (upgradeToPremiumButton != null)
                upgradeToPremiumButton.gameObject.SetActive(!authManager.HasPremiumSubscription);
        }

        /// <summary>
        /// Handle Google sign-in button click
        /// </summary>
        private async void OnGoogleSignInClicked()
        {
            ShowLoadingPanel();
            
            try
            {
                var result = await authManager.SignInWithGoogle();
                if (result.Success)
                {
                    Debug.Log("Google sign-in successful!");
                }
                else
                {
                    Debug.LogError($"Google sign-in failed: {result.Error}");
                    ShowSignInPanel();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Google sign-in error: {e.Message}");
                ShowSignInPanel();
            }
        }

        /// <summary>
        /// Handle Apple sign-in button click
        /// </summary>
        private async void OnAppleSignInClicked()
        {
            ShowLoadingPanel();
            
            try
            {
                var result = await authManager.SignInWithApple();
                if (result.Success)
                {
                    Debug.Log("Apple sign-in successful!");
                }
                else
                {
                    Debug.LogError($"Apple sign-in failed: {result.Error}");
                    ShowSignInPanel();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Apple sign-in error: {e.Message}");
                ShowSignInPanel();
            }
        }

        /// <summary>
        /// Handle email sign-in button click
        /// </summary>
        private void OnEmailSignInClicked()
        {
            if (emailSignInForm != null)
                emailSignInForm.SetActive(true);
        }

        /// <summary>
        /// Handle email form submit
        /// </summary>
        private async void OnEmailSubmitClicked()
        {
            if (emailInput == null || passwordInput == null) return;

            string email = emailInput.text.Trim();
            string password = passwordInput.text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                Debug.LogWarning("Please enter both email and password");
                return;
            }

            ShowLoadingPanel();

            try
            {
                var result = await authManager.SignInWithEmail(email, password);
                if (result.Success)
                {
                    Debug.Log("Email sign-in successful!");
                }
                else
                {
                    Debug.LogError($"Email sign-in failed: {result.Error}");
                    ShowSignInPanel();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Email sign-in error: {e.Message}");
                ShowSignInPanel();
            }
        }

        /// <summary>
        /// Handle email form back button
        /// </summary>
        private void OnEmailBackClicked()
        {
            if (emailSignInForm != null)
                emailSignInForm.SetActive(false);
        }

        /// <summary>
        /// Handle guest sign-in button click
        /// </summary>
        private async void OnGuestSignInClicked()
        {
            ShowLoadingPanel();
            
            try
            {
                var result = await authManager.SignInAsGuest();
                if (result.Success)
                {
                    Debug.Log("Guest sign-in successful!");
                }
                else
                {
                    Debug.LogError($"Guest sign-in failed: {result.Error}");
                    ShowSignInPanel();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Guest sign-in error: {e.Message}");
                ShowSignInPanel();
            }
        }

        /// <summary>
        /// Handle sign-out button click
        /// </summary>
        private void OnSignOutClicked()
        {
            authManager?.SignOut();
        }

        /// <summary>
        /// Handle upgrade to premium button click
        /// NOTE: From this UI, we ONLY open the PremiumUpgradePanel. Actual purchase is done by the panel's Upgrade button.
        /// </summary>
        private void OnUpgradeToPremiumClicked()
        {
            // Do not start purchase here. Only open the upgrade panel.
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowPremiumUpgradePanel();
            }
        }

        /// <summary>
        /// Handle user signed in event
        /// </summary>
        private void OnUserSignedIn(AuthenticationManager.AuthUser user)
        {
            ShowProfilePanel();
            UpdateProfileDisplay();
            
            // Sync with ProfileManager
            if (profileManager != null)
            {
                var profileData = profileManager.GetProfileData();
                profileData.username = user.displayName;
                profileData.email = user.email;
                // Note: ProfileManager will handle saving this data
            }
        }

        /// <summary>
        /// Handle user signed out event
        /// </summary>
        private void OnUserSignedOut()
        {
            ShowSignInPanel();
        }

        /// <summary>
        /// Handle subscription status changed event
        /// </summary>
        private void OnSubscriptionStatusChanged(bool hasPremium)
        {
            UpdateProfileDisplay();
        }

        private void OnDestroy()
        {
            // Clean up event listeners
            if (authManager != null)
            {
                authManager.OnUserSignedIn.RemoveListener(OnUserSignedIn);
                authManager.OnUserSignedOut.RemoveListener(OnUserSignedOut);
                authManager.OnSubscriptionStatusChanged.RemoveListener(OnSubscriptionStatusChanged);
            }
        }
    }
} 