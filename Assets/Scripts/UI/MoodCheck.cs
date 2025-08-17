// Ethan Le (off-topic note for myself): I have developed this file completely by myself. 

using UnityEngine; // Base class for all Unity scripts. 
using TMPro; // Needed to reference TextMeshPro components. 
using LifeCraft.Systems; // Import the MoodManager class from the Systems namespace.

namespace LifeCraft.UI
{
    public class MoodCheck : MonoBehaviour
    {
        [Header("Mood Check Question")]
        [SerializeField] private TextMeshProUGUI questionText;

        [Header("Mood Buttons")]
        [SerializeField] private UnityEngine.UI.Button happyButton;
        [SerializeField] private UnityEngine.UI.Button sadButton;
        [SerializeField] private UnityEngine.UI.Button moodyButton;
        [SerializeField] private UnityEngine.UI.Button stressedButton;

        [Header("Button Texts")]
        [SerializeField] private TextMeshProUGUI happyButtonText;
        [SerializeField] private TextMeshProUGUI sadButtonText;
        [SerializeField] private TextMeshProUGUI moodyButtonText;
        [SerializeField] private TextMeshProUGUI stressedButtonText;

        [Header("Button Images")]
        [SerializeField] private UnityEngine.UI.Image happyImage;
        [SerializeField] private UnityEngine.UI.Image sadImage;
        [SerializeField] private UnityEngine.UI.Image moodyImage;
        [SerializeField] private UnityEngine.UI.Image stressedImage;

        // Flags to mark the selected mood (to be tied to the City page's Weather System) (no longer needed -- using MoodManager):
        private MoodManager moodManager; // Reference to the MoodManager instance - will be set in Start()
        //private bool isHappy = false;
        //private bool isSad = false;
        //private bool isMoody = false;
        //private bool isStressed = false;

        // Time Tracker to note when 24 hours have passed since the last mood selection:
        //private float lastMoodSelectionTime = 0f; // Timestamp of the last mood selection. 

        // Called when script instance is being loaded:
        private void Start()
        {
            // Get the MoodManager instance AFTER the scene has loaded
            moodManager = MoodManager.Instance;
            
            // Check if MoodManager exists
            if (moodManager == null)
            {
                Debug.LogError("MoodManager instance not found! Make sure MoodManager GameObject exists in the scene.");
                return;
            }

            // Set the question text for the mood check:
            if (questionText != null)
                questionText.text = "How are you feeling today?";

            // Get the text components and assign button labels:
            if (happyButtonText != null)
                happyButtonText.text = "Happy";
            if (sadButtonText != null)
                sadButtonText.text = "Sad";
            if (moodyButtonText != null)
                moodyButtonText.text = "Moody";
            if (stressedButtonText != null)
                stressedButtonText.text = "Stressed";

            // Assign button click listeners (have the buttons respond to the player's click):
            if (happyButton != null)
                happyButton.onClick.AddListener(OnHappyButtonClicked);
            if (sadButton != null)
                sadButton.onClick.AddListener(OnSadButtonClicked);
            if (moodyButton != null)
                moodyButton.onClick.AddListener(OnMoodyButtonClicked);
            if (stressedButton != null)
                stressedButton.onClick.AddListener(OnStressedButtonClicked);

            // Check if mood check should be shown based on saved data
            CheckMoodCheckTimer();
        }

        // Called when the Happy Button is clicked:
        private void OnHappyButtonClicked()
        {
            // Check if MoodManager is available
            if (moodManager == null)
            {
                Debug.LogError("MoodManager is null! Cannot change mood.");
                return;
            }

            // Ensure that all other moods are set to false, only set Happy flag as true:
            //isHappy = true;
            //isSad = false;
            //isMoody = false;
            //isStressed = false;
            moodManager.ChangeMood(MoodManager.Mood.Happy); // Change the player's mood to Happy using the MoodManager instance. 

            Debug.Log("Happy button clicked!" + moodManager.GetCurrentMood());

            HidePanel(); // Hide the mood check panel after selection. 
        }

        // Called when the Sad Button is clicked:
        private void OnSadButtonClicked()
        {
            // Check if MoodManager is available
            if (moodManager == null)
            {
                Debug.LogError("MoodManager is null! Cannot change mood.");
                return;
            }

            // Ensure that all other moods are set to false, only set Sad flag as true:
            //isHappy = false;
            //isSad = true;
            //isMoody = false;
            //isStressed = false;
            moodManager.ChangeMood(MoodManager.Mood.Sad); // Change the player's mood to Sad using the MoodManager instance. 

            Debug.Log("Sad button clicked!" + moodManager.GetCurrentMood());

            HidePanel(); // Hide the mood check panel after selection. 
        }

        // Called when the Moody Button is clicked:
        private void OnMoodyButtonClicked()
        {
            // Check if MoodManager is available
            if (moodManager == null)
            {
                Debug.LogError("MoodManager is null! Cannot change mood.");
                return;
            }

            // Ensure that all other moods are set to false, only set Moody flag as true:
            //isHappy = false;
            //isSad = false;
            //isMoody = true;
            //isStressed = false;
            moodManager.ChangeMood(MoodManager.Mood.Moody); // Change the player's mood to Moody using the MoodManager instance. 

            Debug.Log("Moody button clicked!" + moodManager.GetCurrentMood());

            HidePanel(); // Hide the mood check panel after selection. 
        }

        // Called when the Stressed Button is clicked:
        private void OnStressedButtonClicked()
        {
            // Check if MoodManager is available
            if (moodManager == null)
            {
                Debug.LogError("MoodManager is null! Cannot change mood.");
                return;
            }

            // Ensure that all other moods are set to false, only set Stressed flag as true:
            //isHappy = false;
            //isSad = false;
            //isMoody = false;
            //isStressed = true;
            moodManager.ChangeMood(MoodManager.Mood.Stressed); // Change the player's mood to Stressed using the MoodManager instance. 

            Debug.Log("Stressed button clicked!" + moodManager.GetCurrentMood());

            HidePanel(); // Hide the mood check panel after selection. 
        }

        // Hide the mood check panel after selecting your mood:
        private void HidePanel()
        {
            // Start the 24-hour timer after mood selection: 
            //lastMoodSelectionTime = Time.time; // Reset the last mood selection time to the current time. 

            // Logic to hide the mood check panel:
            gameObject.SetActive(false); // Deactivate the GameObject this script is attached to ("MoodCheckPanel"). 

            // Schedule the mood check panel to reopen after 24 hours: 
            Invoke("ResetMoodSelection", 86400f); // Call the "ResetMoodSelection" method after 24 hours (86400 seconds). 
        }

        // Logic to reopen the MoodCheckPanel after 24 hours since the last mood selection: 
        [ContextMenu("Reset Mood Selection")]
        private void ResetMoodSelection()
        {
            // Check if 24 hours have passed since the last mood selection:
            gameObject.SetActive(true); // Activate the GameObject this script is attached to ("MoodCheckPanel"). 

            //Debug.Log("10 seconds have passed! Reopening MoodCheckPanel!" + Time.time); 
        }

        // Check if mood check should be shown based on saved data
        private void CheckMoodCheckTimer()
        {
            if (moodManager != null && moodManager.IsTimeForMoodCheck())
            {
                Debug.Log("24 hours have passed since last mood selection - showing mood check");
                gameObject.SetActive(true);
            }
            else if (moodManager != null)
            {
                float timeRemaining = moodManager.GetTimeUntilNextMoodCheck();
                Debug.Log($"Time until next mood check: {timeRemaining / 3600f:F1} hours");
                
                // Schedule the next mood check
                Invoke("ResetMoodSelection", timeRemaining);
            }
        }
    }
}