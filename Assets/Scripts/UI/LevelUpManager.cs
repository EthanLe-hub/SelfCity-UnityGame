using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using LifeCraft.Systems;

namespace LifeCraft.UI
{
    /// <summary>
    /// Manages level-up celebrations and visual feedback
    /// </summary>
    public class LevelUpManager : MonoBehaviour
    {
        [Header("Level-Up UI")]
        [SerializeField] private GameObject levelUpPanel;
        [SerializeField] private TextMeshProUGUI levelUpText;
        [SerializeField] private TextMeshProUGUI newLevelText;
        [SerializeField] private Button continueButton;
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("Unlock Notifications")]
        [SerializeField] private GameObject unlockNotificationPrefab;
        [SerializeField] private Transform unlockNotificationParent;
        
        [Header("Animation Settings")]
        [SerializeField] private float celebrationDuration = 3f;
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem levelUpParticles;
        [SerializeField] private AudioSource levelUpSound;
        
        private static LevelUpManager _instance;
        public static LevelUpManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<LevelUpManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("LevelUpManager");
                        _instance = go.AddComponent<LevelUpManager>();
                    }
                }
                return _instance;
            }
        }
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeUI();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // Subscribe to level-up events
            if (PlayerLevelManager.Instance != null)
            {
                PlayerLevelManager.Instance.OnLevelUp += OnPlayerLevelUp;
                PlayerLevelManager.Instance.OnBuildingUnlocked += OnBuildingUnlocked;
                PlayerLevelManager.Instance.OnRegionUnlocked += OnRegionUnlocked;
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (PlayerLevelManager.Instance != null)
            {
                PlayerLevelManager.Instance.OnLevelUp -= OnPlayerLevelUp;
                PlayerLevelManager.Instance.OnBuildingUnlocked -= OnBuildingUnlocked;
                PlayerLevelManager.Instance.OnRegionUnlocked -= OnRegionUnlocked;
            }
        }
        
        private void InitializeUI()
        {
            if (levelUpPanel != null)
                levelUpPanel.SetActive(false);
                
            if (continueButton != null)
                continueButton.onClick.AddListener(HideLevelUpPanel);
        }
        
        /// <summary>
        /// Called when player levels up
        /// </summary>
        private void OnPlayerLevelUp(int newLevel)
        {
            StartCoroutine(ShowLevelUpCelebration(newLevel));
        }
        
        /// <summary>
        /// Called when a building is unlocked
        /// </summary>
        private void OnBuildingUnlocked(string buildingName)
        {
            ShowUnlockNotification($"New Building Unlocked: {buildingName}", Color.green);
        }
        
        /// <summary>
        /// Called when a region is unlocked
        /// </summary>
        private void OnRegionUnlocked(AssessmentQuizManager.RegionType regionType)
        {
            string regionName = regionType.ToString().Replace("_", " ");
            ShowUnlockNotification($"New Region Unlocked: {regionName}!", Color.blue);
        }
        
        /// <summary>
        /// Show level-up celebration animation
        /// </summary>
        private IEnumerator ShowLevelUpCelebration(int newLevel)
        {
            // Play sound and particles
            if (levelUpSound != null)
                levelUpSound.Play();
                
            if (levelUpParticles != null)
                levelUpParticles.Play();
            
            // Update UI text
            if (levelUpText != null)
                levelUpText.text = "LEVEL UP!";
                
            if (newLevelText != null)
                newLevelText.text = $"Level {newLevel}";
            
            // Show panel with fade in
            if (levelUpPanel != null)
            {
                levelUpPanel.SetActive(true);
                
                if (canvasGroup != null)
                {
                    // Fade in
                    float elapsed = 0f;
                    while (elapsed < fadeInDuration)
                    {
                        elapsed += Time.deltaTime;
                        canvasGroup.alpha = elapsed / fadeInDuration;
                        yield return null;
                    }
                    canvasGroup.alpha = 1f;
                }
            }
            
            // Wait for celebration duration
            yield return new WaitForSeconds(celebrationDuration);
            
            // Auto-hide after duration (or wait for button click)
            if (continueButton == null)
            {
                yield return StartCoroutine(HideLevelUpPanelCoroutine());
            }
        }
        
        /// <summary>
        /// Hide the level-up panel
        /// </summary>
        private void HideLevelUpPanel()
        {
            StartCoroutine(HideLevelUpPanelCoroutine());
        }
        
        /// <summary>
        /// Hide level-up panel with fade out animation
        /// </summary>
        private IEnumerator HideLevelUpPanelCoroutine()
        {
            if (canvasGroup != null)
            {
                // Fade out
                float elapsed = 0f;
                while (elapsed < fadeOutDuration)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = 1f - (elapsed / fadeOutDuration);
                    yield return null;
                }
                canvasGroup.alpha = 0f;
            }
            
            if (levelUpPanel != null)
                levelUpPanel.SetActive(false);
        }
        
        /// <summary>
        /// Show unlock notification
        /// </summary>
        private void ShowUnlockNotification(string message, Color color)
        {
            if (unlockNotificationPrefab != null && unlockNotificationParent != null)
            {
                GameObject notification = Instantiate(unlockNotificationPrefab, unlockNotificationParent);
                TextMeshProUGUI textComponent = notification.GetComponentInChildren<TextMeshProUGUI>();
                
                if (textComponent != null)
                {
                    textComponent.text = message;
                    textComponent.color = color;
                }
                
                // Auto-destroy after 3 seconds
                Destroy(notification, 3f);
            }
        }
        
        /// <summary>
        /// Test method for building unlock notification (debug only)
        /// </summary>
        public void TestBuildingUnlockNotification(string buildingName)
        {
            ShowUnlockNotification($"New Building Unlocked: {buildingName}", Color.green);
        }
        
        /// <summary>
        /// Test method for region unlock notification (debug only)
        /// </summary>
        public void TestRegionUnlockNotification(AssessmentQuizManager.RegionType regionType)
        {
            string regionName = regionType.ToString().Replace("_", " ");
            ShowUnlockNotification($"New Region Unlocked: {regionName}!", Color.blue);
        }
    }
} 