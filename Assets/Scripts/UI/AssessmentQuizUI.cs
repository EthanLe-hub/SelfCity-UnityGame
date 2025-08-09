using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using LifeCraft.Systems;

namespace LifeCraft.UI
{
    /// <summary>
    /// Manages the assessment quiz UI and user interactions.
    /// </summary>
    public class AssessmentQuizUI : MonoBehaviour
    {
        // Singleton Instance (to ensure only one instance of the class exists)
        private static AssessmentQuizUI _instance; // Create a private static instance of the class. 
        public static AssessmentQuizUI Instance // Create a public static instance of the class that can be accessed from other scripts. 
        {
            get 
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<AssessmentQuizUI>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("AssessmentQuizUI");
                        _instance = go.AddComponent<AssessmentQuizUI>();
                    }
                }
                return _instance;
            }
        }

        [Header("Quiz Manager")]
        [SerializeField] private AssessmentQuizManager quizManager;

        [Header("UI References")]
        [SerializeField] private GameObject quizPanel;
        [SerializeField] private TMP_Text questionText;
        [SerializeField] private Transform answerButtonContainer;
        [SerializeField] private GameObject answerButtonPrefab;
        [SerializeField] private Button skipButton;
        [SerializeField] private Button closeButton;

        [Header("Progress")]
        [SerializeField] private Slider progressBar;
        [SerializeField] private TMP_Text progressText;

        [Header("Results")]
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private TMP_Text recommendedRegionText;
        [SerializeField] private TMP_Text regionDescriptionText;
        [SerializeField] private Transform regionScoresContainer;
        [SerializeField] private GameObject regionScorePrefab;
        //[SerializeField] private Button acceptRecommendationButton; // No need for this button; player can immediately choose their starting region. 
        //[SerializeField] private Button chooseDifferentRegionButton; // No need for this button; player can immediately choose their starting region. 

        [Header("Region Selection")]
        [SerializeField] private GameObject regionSelectionPanel; // Show the Region Selection Panel (to choose your starting region). 
        [SerializeField] private Transform regionButtonContainer; // Show the container for the region buttons. 
        //[SerializeField] private GameObject regionButtonPrefab;

        // Events
        // IMPORTANT: This event is subscribed to by GameManager. Ensure only ONE AssessmentQuizUI
        // instance exists in the scene to prevent subscription mismatches.
        public System.Action<AssessmentQuizManager.RegionType> OnRegionSelected;

        private List<Button> _answerButtons = new List<Button>();
        private List<Button> _regionButtons = new List<Button>();

        private void Start()
        {
            Debug.Log($"AssessmentQuizUI Start called - Instance ID: {this.GetInstanceID()}");
            SetupEventListeners();
        }

        /// <summary>
        /// Setup event listeners for buttons
        /// </summary>
        private void SetupEventListeners()
        {
            if (skipButton != null)
                skipButton.onClick.AddListener(OnSkipClicked);

            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClicked);

            /*
            if (acceptRecommendationButton != null)
                acceptRecommendationButton.onClick.AddListener(OnAcceptRecommendationClicked);
            */

            /*
            if (chooseDifferentRegionButton != null)
                chooseDifferentRegionButton.onClick.AddListener(OnChooseDifferentRegionClicked);
            */

            // Setup quiz manager events
            if (quizManager != null)
            {
                quizManager.OnQuizCompleted += OnQuizCompleted;
                quizManager.OnQuizSkipped += OnQuizSkipped;
            }
        }

        /// <summary>
        /// Show the assessment quiz
        /// </summary>
        public void ShowQuiz()
        {
            if (quizManager == null)
            {
                Debug.LogError("QuizManager not assigned!");
                return;
            }

            quizManager.InitializeQuiz();
            ShowQuizPanel();
            DisplayCurrentQuestion();
        }

        /// <summary>
        /// Show the quiz panel
        /// </summary>
        private void ShowQuizPanel()
        {
            if (quizPanel != null)
                quizPanel.SetActive(true);

            if (resultsPanel != null)
                resultsPanel.SetActive(false);

            if (regionSelectionPanel != null)
                regionSelectionPanel.SetActive(false);
        }

        /// <summary>
        /// Display the current question
        /// </summary>
        private void DisplayCurrentQuestion()
        {
            var currentQuestion = quizManager.GetCurrentQuestion();
            if (currentQuestion == null)
            {
                Debug.LogError("No current question available!");
                return;
            }

            // Update question text
            if (questionText != null)
                questionText.text = currentQuestion.questionText;

            // Update progress
            UpdateProgress();

            // Create answer buttons
            CreateAnswerButtons(currentQuestion);
        }

        /// <summary>
        /// Create answer buttons for the current question
        /// </summary>
        private void CreateAnswerButtons(AssessmentQuizManager.QuizQuestion question)
        {
            // Clear existing buttons
            ClearAnswerButtons();

            if (answerButtonContainer == null || answerButtonPrefab == null)
                return;

            // Create new buttons
            for (int i = 0; i < question.answers.Count; i++)
            {
                var answer = question.answers[i];
                var buttonObj = Instantiate(answerButtonPrefab, answerButtonContainer);
                var button = buttonObj.GetComponent<Button>();
                var buttonText = buttonObj.GetComponentInChildren<TMP_Text>();

                if (buttonText != null)
                    buttonText.text = answer.answerText;

                if (button != null)
                {
                    int answerIndex = i; // Capture the index
                    button.onClick.AddListener(() => OnAnswerSelected(answerIndex));
                    _answerButtons.Add(button);
                }
            }
        }

        /// <summary>
        /// Clear all answer buttons
        /// </summary>
        private void ClearAnswerButtons()
        {
            foreach (var button in _answerButtons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }
            _answerButtons.Clear();
        }

        /// <summary>
        /// Update progress display
        /// </summary>
        private void UpdateProgress()
        {
            if (progressBar != null)
                progressBar.value = quizManager.GetProgress();

            if (progressText != null)
                progressText.text = $"Question {quizManager.GetCurrentQuestionNumber()} of {quizManager.GetTotalQuestions()}";
        }

        /// <summary>
        /// Handle answer selection
        /// </summary>
        private void OnAnswerSelected(int answerIndex)
        {
            quizManager.SubmitAnswer(answerIndex);

            // Check if quiz is complete
            if (quizManager.IsQuizCompleted())
            {
                // Quiz will be completed via event
                return;
            }

            // Display next question
            DisplayCurrentQuestion();
        }

        /// <summary>
        /// Handle skip button click
        /// </summary>
        private void OnSkipClicked()
        {
            quizManager.SkipQuiz();
        }

        /// <summary>
        /// Handle close button click
        /// </summary>
        private void OnCloseClicked()
        {
            HideAllPanels();
        }

        /// <summary>
        /// Handle quiz completion
        /// </summary>
        private void OnQuizCompleted(AssessmentQuizManager.RegionType recommendedRegion)
        {
            ShowResultsPanel(recommendedRegion, true);
        }

        /// <summary>
        /// Handle quiz skip
        /// </summary>
        private void OnQuizSkipped()
        {
            ShowResultsPanel(AssessmentQuizManager.RegionType.HealthHarbor, false); // Default to Health Harbor (will not show in the Results Panel) if the quiz was skipped. 
        }

        /// <summary>
        /// Show the results panel
        /// </summary>
        private void ShowResultsPanel(AssessmentQuizManager.RegionType recommendedRegion, bool isQuizCompleted) // isQuizCompleted parameter is true if the quiz was completed, false if the quiz was skipped. 
        {
            if (quizPanel != null)
                quizPanel.SetActive(false);

            if (resultsPanel != null)
                resultsPanel.SetActive(true);

            if (regionSelectionPanel != null)
                regionSelectionPanel.SetActive(true); // After completing the quiz, show the region selection panel. 

            // Update results text if quiz was completed: 
            if (recommendedRegionText != null && isQuizCompleted) // If the recommended region text component exists and the quiz was completed, show the recommended region. 
                recommendedRegionText.text = $"We recommend starting with {AssessmentQuizManager.GetRegionDisplayName(recommendedRegion)}!";

            if (regionDescriptionText != null && isQuizCompleted) // If the region description text component exists and the quiz was completed, show the region description. 
                regionDescriptionText.text = AssessmentQuizManager.GetRegionDescription(recommendedRegion);

            // Update results text if the quiz was skipped: 
            if (recommendedRegionText != null && !isQuizCompleted) // If the recommended regiontext component exists and the quiz was skipped, show the message that the quiz was skipped. 
                recommendedRegionText.text = $"You skipped the quiz. Select your starting region below."; 

            if (regionDescriptionText != null && !isQuizCompleted) // If the region description text component exists and the quiz was skipped, show all 4 region descriptions. 
                regionDescriptionText.text = AssessmentQuizManager.GetRegionDescription(recommendedRegion = AssessmentQuizManager.RegionType.AllRegions); // Show all 4 region descriptions. 

            // Display region scores
            DisplayRegionScores();
        }

        /// <summary>
        /// Display region scores
        /// </summary>
        private void DisplayRegionScores()
        {
            if (regionScoresContainer == null || regionScorePrefab == null)
                return;

            // Clear existing score displays
            foreach (Transform child in regionScoresContainer)
            {
                Destroy(child.gameObject);
            }

            var scores = quizManager.GetRegionScores();
            foreach (var kvp in scores)
            {
                var scoreObj = Instantiate(regionScorePrefab, regionScoresContainer);
                var scoreText = scoreObj.GetComponentInChildren<TMP_Text>();
                var scoreImage = scoreObj.GetComponent<Image>();
                
                if (scoreText != null)
                {
                    scoreText.text = $"{AssessmentQuizManager.GetRegionDisplayName(kvp.Key)}: {kvp.Value} points";
                }

                // Set the region icon if available
                if (scoreImage != null && quizManager != null)
                {
                    var regionIcon = quizManager.GetRegionIcon(kvp.Key);
                    if (regionIcon != null)
                    {
                        scoreImage.sprite = regionIcon;
                    }
                }
            }
        }

        /// <summary>
        /// Show the region selection panel
        /// </summary>
        /*
        private void ShowRegionSelectionPanel()
        {
            if (quizPanel != null)
                quizPanel.SetActive(false);

            if (resultsPanel != null)
                resultsPanel.SetActive(false);

            if (regionSelectionPanel != null)
                regionSelectionPanel.SetActive(true);

            //CreateRegionButtons();
        }
        */

        /// <summary>
        /// Create region selection buttons
        /// </summary>
        /*
        private void CreateRegionButtons()
        {
            // Clear existing buttons
            ClearRegionButtons();

            if (regionButtonContainer == null || regionButtonPrefab == null)
                return;

            // Create buttons for each region
            var regions = new[]
            {
                AssessmentQuizManager.RegionType.HealthHarbor,
                AssessmentQuizManager.RegionType.MindPalace,
                AssessmentQuizManager.RegionType.CreativeCommons,
                AssessmentQuizManager.RegionType.SocialSquare
            };

            foreach (var region in regions)
            {
                var buttonObj = Instantiate(regionButtonPrefab, regionButtonContainer);
                var button = buttonObj.GetComponent<Button>();
                var buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
                var buttonImage = buttonObj.GetComponent<Image>();

                // Set the button text
                if (buttonText != null)
                    buttonText.text = AssessmentQuizManager.GetRegionDisplayName(region);

                // Set the button image (region icon)
                if (buttonImage != null && quizManager != null)
                {
                    var regionIcon = quizManager.GetRegionIcon(region);
                    if (regionIcon != null)
                    {
                        buttonImage.sprite = regionIcon;
                    }
                }

                if (button != null)
                {
                    AssessmentQuizManager.RegionType regionCopy = region; // Capture the region
                    button.onClick.AddListener(() => OnRegionButtonClicked(regionCopy));
                    _regionButtons.Add(button);
                }
            }
        }
        */

        /// <summary>
        /// Clear all region buttons
        /// </summary>
        private void ClearRegionButtons()
        {
            foreach (var button in _regionButtons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }
            _regionButtons.Clear();
        }

        /// <summary>
        /// Handle accept recommendation button click
        /// </summary>
        /*
        private void OnAcceptRecommendationClicked()
        {
            var recommendedRegion = quizManager.GetRecommendedRegion();
            OnRegionButtonClicked(recommendedRegion);
        }
        */

        /// <summary>
        /// Handle choose different region button click
        /// </summary>
        /*
        private void OnChooseDifferentRegionClicked()
        {
            ShowRegionSelectionPanel();
        }
        */

        /// <summary>
        /// Handle region selection
        /// </summary>
        private void OnRegionButtonClicked(AssessmentQuizManager.RegionType region) // AssessmentQuizManager.RegionType is the necessary parameter type because the appropriate region needs to be pulled from the Unlock Sequence array, allowing for correct unlocking sequence after the starting region is selected. 
        {
            HideAllPanels();
            OnRegionSelected?.Invoke(region);
        }

        public void OnHealthHarborButtonClicked()
        {
            Debug.Log("Health Harbor button clicked - triggering OnRegionSelected event");
            OnRegionSelected?.Invoke(AssessmentQuizManager.RegionType.HealthHarbor); // Call the OnRegionSelected event handler (from the GameManager) with the Health Harbor region to unlock the Health Harbor region. 
            Debug.Log("OnRegionSelected event triggered for Health Harbor");
            HideAllPanels(); // Close the canvas after selection
        }

        public void OnMindPalaceButtonClicked()
        {
            Debug.Log($"Mind Palace button clicked - triggering OnRegionSelected event");
            Debug.Log($"This AssessmentQuizUI instance: {this.GetInstanceID()}");
            Debug.Log($"OnRegionSelected has {OnRegionSelected?.GetInvocationList().Length ?? 0} subscribers");
            OnRegionSelected?.Invoke(AssessmentQuizManager.RegionType.MindPalace); // Call the OnRegionSelected event handler (from the GameManager) with the Mind Palace region to unlock the Mind Palace region. 
            Debug.Log("OnRegionSelected event triggered for Mind Palace");
            HideAllPanels(); // Close the canvas after selection
        }

        public void OnCreativeCommonsButtonClicked()
        {
            Debug.Log("Creative Commons button clicked - triggering OnRegionSelected event");
            OnRegionSelected?.Invoke(AssessmentQuizManager.RegionType.CreativeCommons); // Call the OnRegionSelected event handler (from the GameManager) with the Creative Commons region to unlock the Creative Commons region. 
            Debug.Log("OnRegionSelected event triggered for Creative Commons");
            HideAllPanels(); // Close the canvas after selection
        }

        public void OnSocialSquareButtonClicked()
        {
            Debug.Log("Social Square button clicked - triggering OnRegionSelected event");
            OnRegionSelected?.Invoke(AssessmentQuizManager.RegionType.SocialSquare); // Call the OnRegionSelected event handler (from the GameManager) with the Social Square region to unlock the Social Square region. 
            Debug.Log("OnRegionSelected event triggered for Social Square");
            HideAllPanels(); // Close the canvas after selection
        }

        /// <summary>
        /// Hide all panels
        /// </summary>
        private void HideAllPanels()
        {
            if (quizPanel != null)
                quizPanel.SetActive(false);

            if (resultsPanel != null)
                resultsPanel.SetActive(false);

            if (regionSelectionPanel != null)
                regionSelectionPanel.SetActive(false);
        }

        /// <summary>
        /// Hide the quiz UI completely
        /// </summary>
        public void HideQuiz()
        {
            HideAllPanels();
        }

        private void OnDestroy()
        {
            // Clean up event listeners
            if (quizManager != null)
            {
                quizManager.OnQuizCompleted -= OnQuizCompleted;
                quizManager.OnQuizSkipped -= OnQuizSkipped;
            }
        }
    }
} 