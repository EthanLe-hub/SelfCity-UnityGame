using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;

namespace LifeCraft.UI
{
    /// <summary>
    /// Manages the Profile page functionality including user profile data and Journal feature
    /// Enhanced with validation, auto-save, mood tracking, and Game Credits
    /// </summary>
    public class ProfileManager : MonoBehaviour
    {
        [Header("Profile UI Elements")]
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private Toggle notificationsToggle;
        [SerializeField] private TMP_Dropdown ageRangeDropdown;
        [SerializeField] private TMP_Dropdown genderDropdown;
        [SerializeField] private Button saveProfileButton;
        [SerializeField] private Button journalButton;
        [SerializeField] private Button gameCreditsButton;

        [Header("Journal UI Elements")]
        [SerializeField] private GameObject journalPanel;
        [SerializeField] private GameObject bookObject;
        [SerializeField] private TMP_InputField journalInputField;
        [SerializeField] private TMP_Text journalDateText;
        [SerializeField] private TMP_Text characterCountText;
        [SerializeField] private Button openBookButton;
        [SerializeField] private Button closeJournalButton;
        [SerializeField] private Button closeBookButton;
        [SerializeField] private Button saveJournalButton;
        [SerializeField] private Button deleteEntryButton; // Delete button for book interface
        [SerializeField] private Transform journalEntriesContainer;
        [SerializeField] private GameObject journalEntryPrefab;
        [SerializeField] private TMP_Dropdown moodDropdown;
        [SerializeField] private Button leftArrowButton; // Navigation arrow for previous entry
        [SerializeField] private Button rightArrowButton; // Navigation arrow for next entry

        [Header("Game Credits UI Elements")]
        [SerializeField] private GameObject gameCreditsPanel;
        [SerializeField] private Button closeCreditsButton;

        [Header("Settings")]
        [SerializeField] private string journalDataPath = "journal_entries.json";
        [SerializeField] private int maxJournalCharacters = 1000;
        [SerializeField] private float autoSaveInterval = 30f; // seconds
        [SerializeField] private bool enableAutoSave = true;

        [Header("Journal Entry Edit UI Elements")]
        [SerializeField] private GameObject journalEntryEditPanel;
        [SerializeField] private TMP_InputField editJournalInputField; 
        [SerializeField] private TMP_Text editJournalDateText;
        [SerializeField] private TMP_Text editCharacterCountText;
        [SerializeField] private Button editSaveButton;
        [SerializeField] private Button editDeleteButton;
        [SerializeField] private Button editCloseButton;
        [SerializeField] private TMP_Dropdown editMoodDropdown;

        private JournalEntry currentEditingEntry; // Track which entry is being edited. 

        // Profile data
        [System.Serializable]
        public class ProfileData
        {
            public string username = "";
            public string email = "";
            public bool notificationsEnabled = true;
            public int ageRangeIndex = 0;
            public int genderIndex = 0;
        }

        // Enhanced Journal entry data with mood tracking
        [System.Serializable]
        public class JournalEntry
        {
            public string date;
            public string content;
            public string timestamp;
            public int moodIndex;
            public string moodName;
        }

        [System.Serializable]
        public class JournalData
        {
            public List<JournalEntry> entries = new List<JournalEntry>();
        }

        private ProfileData currentProfile;
        private JournalData journalData;
        private bool isBookOpen = false;
        private string currentDraft = "";
        private Coroutine autoSaveCoroutine;
        
        // Navigation tracking for journal entries
        private int currentEntryIndex = -1; // -1 means no entry is currently being viewed
        private bool isViewingEntry = false; // Whether we're viewing an existing entry or creating a new one

        // Events for external systems
        public static event System.Action<ProfileData> OnProfileUpdated;
        public static event System.Action<JournalEntry> OnJournalEntryAdded;
        public static event System.Action<JournalEntry> OnJournalEntryDeleted;

        public static ProfileManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            InitializeProfile();
            InitializeJournal();
            InitializeGameCredits();
            SetupEventListeners();
            LoadProfileData();
            LoadJournalData();
        }

        /// <summary>
        /// Initialize profile UI elements
        /// </summary>
        private void InitializeProfile()
        {
            // Setup age range dropdown
            if (ageRangeDropdown != null)
            {
                ageRangeDropdown.ClearOptions();
                List<string> ageRanges = new List<string>
                {
                    "Under 18",
                    "18-24",
                    "25-34", 
                    "35-44",
                    "45-54",
                    "55-64",
                    "65+"
                };
                ageRangeDropdown.AddOptions(ageRanges);
            }

            // Setup gender dropdown
            if (genderDropdown != null)
            {
                genderDropdown.ClearOptions();
                List<string> genders = new List<string>
                {
                    "Prefer not to say",
                    "Male",
                    "Female",
                    "Non-binary",
                    "Other"
                };
                genderDropdown.AddOptions(genders);
            }

            currentProfile = new ProfileData();
        }

        /// <summary>
        /// Initialize journal UI elements with mood tracking
        /// </summary>
        private void InitializeJournal()
        {
            if (journalPanel != null)
                journalPanel.SetActive(false);

            if (bookObject != null)
                bookObject.SetActive(false);

            // Setup mood dropdown
            if (moodDropdown != null)
            {
                moodDropdown.ClearOptions();
                List<string> moods = new List<string>
                {
                    "😊 Happy",
                    "😔 Sad",
                    "😤 Angry",
                    "😰 Anxious",
                    "😴 Tired",
                    "🤔 Thoughtful",
                    "😌 Calm",
                    "😃 Excited",
                    "😐 Neutral",
                    "😍 Grateful"
                };
                moodDropdown.AddOptions(moods);
            }

            journalData = new JournalData();
        }

        /// <summary>
        /// Initialize Game Credits panel
        /// </summary>
        private void InitializeGameCredits()
        {
            if (gameCreditsPanel != null)
                gameCreditsPanel.SetActive(false);
        }

        /// <summary>
        /// Setup event listeners for buttons and inputs
        /// </summary>
        private void SetupEventListeners()
        {
            // Profile buttons
            if (saveProfileButton != null)
                saveProfileButton.onClick.AddListener(SaveProfile);

            if (journalButton != null)
                journalButton.onClick.AddListener(OpenJournal);

            if (gameCreditsButton != null)
                gameCreditsButton.onClick.AddListener(OpenGameCredits);

            // Journal buttons
            if (openBookButton != null)
                openBookButton.onClick.AddListener(OpenBook);

            if (closeJournalButton != null)
                closeJournalButton.onClick.AddListener(CloseJournal);

            if (closeBookButton != null)
                closeBookButton.onClick.AddListener(CloseBook);

            if (saveJournalButton != null)
                saveJournalButton.onClick.AddListener(SaveJournalEntry);

            // Delete entry button
            if (deleteEntryButton != null)
                deleteEntryButton.onClick.AddListener(DeleteCurrentEntryFromBook);

            // Navigation arrow buttons
            if (leftArrowButton != null)
                leftArrowButton.onClick.AddListener(NavigateToPreviousEntry);

            if (rightArrowButton != null)
                rightArrowButton.onClick.AddListener(NavigateToNextEntry);

            // Game Credits buttons
            if (closeCreditsButton != null)
                closeCreditsButton.onClick.AddListener(CloseGameCredits);

            // Input field listeners
            if (journalInputField != null)
            {
                journalInputField.onValueChanged.AddListener(OnJournalInputChanged);
                journalInputField.onEndEdit.AddListener(OnJournalInputEndEdit);
            }

            // Profile input validation
            if (usernameInput != null)
                usernameInput.onValueChanged.AddListener(OnUsernameChanged);

            if (emailInput != null)
                emailInput.onValueChanged.AddListener(OnEmailChanged);

            // Edit panel buttons:
            if (editSaveButton != null)
                editSaveButton.onClick.AddListener(SaveEditedJournalEntry); 

            if (editDeleteButton != null)
                editDeleteButton.onClick.AddListener(DeleteCurrentEditingEntry);

            if (editCloseButton != null)
                editCloseButton.onClick.AddListener(CloseEditJournalEntry);

            // Edit input field character count
            if (editJournalInputField != null)
                editJournalInputField.onValueChanged.AddListener((value) => UpdateEditCharacterCount());
        }

        /// <summary>
        /// Load profile data from PlayerPrefs
        /// </summary>
        private void LoadProfileData()
        {
            try
            {
                currentProfile.username = PlayerPrefs.GetString("Profile_Username", "");
                currentProfile.email = PlayerPrefs.GetString("Profile_Email", "");
                currentProfile.notificationsEnabled = PlayerPrefs.GetInt("Profile_Notifications", 1) == 1;
                currentProfile.ageRangeIndex = PlayerPrefs.GetInt("Profile_AgeRange", 0);
                currentProfile.genderIndex = PlayerPrefs.GetInt("Profile_Gender", 0);

                UpdateProfileUI();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading profile data: {e.Message}");
                currentProfile = new ProfileData();
            }
        }

        /// <summary>
        /// Save profile data to PlayerPrefs with validation
        /// </summary>
        private void SaveProfile()
        {
            // Validate inputs
            if (!ValidateProfileData())
                return;

            // Get current values from UI
            if (usernameInput != null)
                currentProfile.username = usernameInput.text.Trim();

            if (emailInput != null)
                currentProfile.email = emailInput.text.Trim();

            if (notificationsToggle != null)
                currentProfile.notificationsEnabled = notificationsToggle.isOn;

            if (ageRangeDropdown != null)
                currentProfile.ageRangeIndex = ageRangeDropdown.value;

            if (genderDropdown != null)
                currentProfile.genderIndex = genderDropdown.value;

            try
            {
                // Save to PlayerPrefs
                PlayerPrefs.SetString("Profile_Username", currentProfile.username);
                PlayerPrefs.SetString("Profile_Email", currentProfile.email);
                PlayerPrefs.SetInt("Profile_Notifications", currentProfile.notificationsEnabled ? 1 : 0);
                PlayerPrefs.SetInt("Profile_AgeRange", currentProfile.ageRangeIndex);
                PlayerPrefs.SetInt("Profile_Gender", currentProfile.genderIndex);
                PlayerPrefs.Save();

                // Trigger event
                OnProfileUpdated?.Invoke(currentProfile);

                // Show success notification
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowNotification("Profile saved successfully!");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving profile data: {e.Message}");
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowNotification("Error saving profile. Please try again.");
            }
        }

        /// <summary>
        /// Validate profile data before saving
        /// </summary>
        private bool ValidateProfileData()
        {
            string username = usernameInput?.text?.Trim() ?? "";
            string email = emailInput?.text?.Trim() ?? "";

            // Username validation
            if (string.IsNullOrWhiteSpace(username))
            {
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowNotification("Username cannot be empty.");
                return false;
            }

            if (username.Length < 3)
            {
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowNotification("Username must be at least 3 characters long.");
                return false;
            }

            if (username.Length > 20)
            {
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowNotification("Username cannot exceed 20 characters.");
                return false;
            }

            // Email validation (if provided)
            if (!string.IsNullOrWhiteSpace(email))
            {
                if (!IsValidEmail(email))
                {
                    if (UIManager.Instance != null)
                        UIManager.Instance.ShowNotification("Please enter a valid email address.");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Validate email format using regex
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                return Regex.IsMatch(email, pattern);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Update profile UI with current data
        /// </summary>
        private void UpdateProfileUI()
        {
            if (usernameInput != null)
                usernameInput.text = currentProfile.username;

            if (emailInput != null)
                emailInput.text = currentProfile.email;

            if (notificationsToggle != null)
                notificationsToggle.isOn = currentProfile.notificationsEnabled;

            if (ageRangeDropdown != null)
                ageRangeDropdown.value = currentProfile.ageRangeIndex;

            if (genderDropdown != null)
                genderDropdown.value = currentProfile.genderIndex;
        }

        /// <summary>
        /// Open the journal panel
        /// </summary>
        private void OpenJournal()
        {
            if (journalPanel != null)
            {
                journalPanel.SetActive(true);
                UpdateJournalDate();
                RefreshJournalEntries();
            }
        }

        /// <summary>
        /// Close the journal panel
        /// </summary>
        public void CloseJournal()
        {
            if (journalPanel != null)
                journalPanel.SetActive(false);

            if (bookObject != null)
            {
                bookObject.SetActive(false);
                isBookOpen = false;
            }

            // Stop auto-save if running
            if (autoSaveCoroutine != null)
            {
                StopCoroutine(autoSaveCoroutine);
                autoSaveCoroutine = null;
            }
        }

        /// <summary>
        /// Open the book for creating a new entry
        /// </summary>
        private void OpenBook()
        {
            if (bookObject != null)
            {
                bookObject.SetActive(true);
                isBookOpen = true;
                
                // Always start with a fresh new entry when "Add Entry" is clicked
                StartNewEntry();

                // Start auto-save if enabled
                if (enableAutoSave)
                {
                    autoSaveCoroutine = StartCoroutine(AutoSaveDraft());
                }
            }
        }

        /// <summary>
        /// Close the book
        /// </summary>
        private void CloseBook()
        {
            if (bookObject != null)
            {
                bookObject.SetActive(false);
                isBookOpen = false;
            }

            // Stop auto-save
            if (autoSaveCoroutine != null)
            {
                StopCoroutine(autoSaveCoroutine);
                autoSaveCoroutine = null;
            }

            // Clear draft
            currentDraft = "";
        }

        /// <summary>
        /// Auto-save draft functionality
        /// </summary>
        private IEnumerator AutoSaveDraft()
        {
            while (isBookOpen && enableAutoSave)
            {
                yield return new WaitForSeconds(autoSaveInterval);
                
                if (isBookOpen && !string.IsNullOrWhiteSpace(journalInputField?.text))
                {
                    currentDraft = journalInputField.text;
                    Debug.Log("Auto-saved journal draft");
                }
            }
        }

        /// <summary>
        /// Save the current journal entry
        /// </summary>
        private void SaveJournalEntry()
        {
            if (journalInputField == null || string.IsNullOrWhiteSpace(journalInputField.text))
            {
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowNotification("Please write something before saving.");
                return;
            }

            try
            {
                if (isViewingEntry && currentEntryIndex >= 0 && currentEntryIndex < journalData.entries.Count)
                {
                    // Update existing entry
                    JournalEntry existingEntry = journalData.entries[currentEntryIndex];
                    existingEntry.content = journalInputField.text.Trim();
                    existingEntry.moodIndex = moodDropdown?.value ?? 0;
                    existingEntry.moodName = moodDropdown?.options[moodDropdown.value]?.text ?? "😐 Neutral";
                    existingEntry.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    SaveJournalData();
                    RefreshJournalEntries();
                    
                    // Show success notification
                    if (UIManager.Instance != null)
                        UIManager.Instance.ShowNotification("Journal entry updated!");

                    // Close the book
                    CloseBook();
                }
                else
                {
                    // Create new entry
                    JournalEntry newEntry = new JournalEntry
                    {
                        date = DateTime.Now.ToString("yyyy-MM-dd"),
                        content = journalInputField.text.Trim(),
                        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        moodIndex = moodDropdown?.value ?? 0,
                        moodName = moodDropdown?.options[moodDropdown.value]?.text ?? "😐 Neutral"
                    };

                    journalData.entries.Add(newEntry);
                    SaveJournalData();
                    
                    // Clear input field and draft
                    journalInputField.text = "";
                    currentDraft = "";
                    UpdateCharacterCount("");
                    
                    // Refresh entries display
                    RefreshJournalEntries();
                    
                    // Trigger event
                    OnJournalEntryAdded?.Invoke(newEntry);
                    
                    // Show success notification
                    if (UIManager.Instance != null)
                        UIManager.Instance.ShowNotification("Journal entry saved!");

                    // Close the book
                    CloseBook();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving journal entry: {e.Message}");
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowNotification("Error saving entry. Please try again.");
            }
        }

        /// <summary>
        /// Handle journal input field changes
        /// </summary>
        private void OnJournalInputChanged(string value)
        {
            UpdateCharacterCount(value);
            
            // Enable/disable save button based on content
            if (saveJournalButton != null)
                saveJournalButton.interactable = !string.IsNullOrWhiteSpace(value) && value.Length <= maxJournalCharacters;
        }

        /// <summary>
        /// Handle journal input end edit (for auto-save)
        /// </summary>
        private void OnJournalInputEndEdit(string value)
        {
            currentDraft = value;
        }

        /// <summary>
        /// Update character count display
        /// </summary>
        private void UpdateCharacterCount(string text)
        {
            if (characterCountText != null)
            {
                int count = text?.Length ?? 0;
                characterCountText.text = $"{count}/{maxJournalCharacters}";
                
                // Change color based on character count
                if (count > maxJournalCharacters * 0.9f)
                    characterCountText.color = Color.red;
                else if (count > maxJournalCharacters * 0.7f)
                    characterCountText.color = Color.yellow;
                else
                    characterCountText.color = Color.white;
            }
        }

        /// <summary>
        /// Update the journal date display
        /// </summary>
        private void UpdateJournalDate()
        {
            if (journalDateText != null)
            {
                string currentDate = DateTime.Now.ToString("MMMM dd, yyyy");
                journalDateText.text = currentDate;
            }
        }

        /// <summary>
        /// Refresh the journal entries display
        /// </summary>
        public void RefreshJournalEntries()
        {
            if (journalEntriesContainer == null || journalEntryPrefab == null)
                return;

            // Clear existing entries
            foreach (Transform child in journalEntriesContainer)
            {
                Destroy(child.gameObject);
            }

            // Sort entries by date (newest first)
            var sortedEntries = new List<JournalEntry>(journalData.entries);
            sortedEntries.Sort((a, b) => DateTime.Parse(b.date).CompareTo(DateTime.Parse(a.date)));

            // Create entry objects for each journal entry
            foreach (var entry in sortedEntries)
            {
                GameObject entryObj = Instantiate(journalEntryPrefab, journalEntriesContainer);
                JournalEntryUI entryUI = entryObj.GetComponent<JournalEntryUI>();
                
                if (entryUI != null)
                {
                    entryUI.Initialize(entry);
                }
            }
        }

        /// <summary>
        /// Load journal data from file
        /// </summary>
        private void LoadJournalData()
        {
            string filePath = Path.Combine(Application.persistentDataPath, journalDataPath);
            
            if (File.Exists(filePath))
            {
                try
                {
                    string jsonData = File.ReadAllText(filePath);
                    journalData = JsonUtility.FromJson<JournalData>(jsonData);
                    
                    // Validate loaded data
                    if (journalData == null || journalData.entries == null)
                    {
                        journalData = new JournalData();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error loading journal data: {e.Message}");
                    journalData = new JournalData();
                }
            }
            else
            {
                journalData = new JournalData();
            }
        }

        /// <summary>
        /// Save journal data to file
        /// </summary>
        public void SaveJournalData()
        {
            string filePath = Path.Combine(Application.persistentDataPath, journalDataPath);
            
            try
            {
                string jsonData = JsonUtility.ToJson(journalData, true);
                File.WriteAllText(filePath, jsonData);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving journal data: {e.Message}");
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowNotification("Error saving journal data.");
            }
        }

        /// <summary>
        /// Open the edit panel for a specific journal entry. 
        /// </summary>
        public void OpenEditJournalEntry(JournalEntry entry)
        {
            currentEditingEntry = entry; // Get the saved entry we are editing and assign it as our current editing entry. 

            // Populate the edit panel with entry data:
            if (editJournalInputField != null)
            {
                editJournalInputField.text = entry.content; // Get the existing text from the saved entry. 
                UpdateEditCharacterCount();
            }

            if (editJournalDateText != null)
            {
                DateTime entryDate = DateTime.Parse(entry.date); // Get the existing date from the saved entry. 
                editJournalDateText.text = entryDate.ToString("MMMM dd, yyyy"); // Convert the date into a String. 
            }

            if (editMoodDropdown != null)
            {
                // Populate the edit mood dropdown with the same options as the original mood dropdown
                editMoodDropdown.ClearOptions();
                List<string> moods = new List<string>
                {
                    "😊 Happy",
                    "😔 Sad",
                    "😤 Angry",
                    "😰 Anxious",
                    "😴 Tired",
                    "🤔 Thoughtful",
                    "😌 Calm",
                    "😃 Excited",
                    "😐 Neutral",
                    "😍 Grateful"
                };
                editMoodDropdown.AddOptions(moods);

                // Set the mood dropdown to the entry's mood:
                for (int i = 0; i < editMoodDropdown.options.Count; i++) // Loop through the different mood options until we find a match to the saved entry's mood. 
                {
                    if (editMoodDropdown.options[i].text.Contains(entry.moodName))
                    {
                        editMoodDropdown.value = i; // Once we find a match, set that mood to our current editing entry. 
                        break;
                    }
                }
            }

            // Show the edit panel:
            if (journalEntryEditPanel != null)
            {
                journalEntryEditPanel.SetActive(true); 
            }
        }

        /// <summary>
        /// Close the edit panel.
        /// </summary>
        public void CloseEditJournalEntry()
        {
            if (journalEntryEditPanel != null)
            {
                journalEntryEditPanel.SetActive(false); // Close the journal entry edit panel by setting its activeness to "false". 
            }

            currentEditingEntry = null; // No entries are now being edited. 
        }

        /// <summary>
        /// Update the character count for the edit input field. 
        /// </summary>
        private void UpdateEditCharacterCount()
        {
            if (editCharacterCountText != null && editJournalInputField != null)
            {
                int currentLength = editJournalInputField.text.Length; // Get the length of the updated edited entry. 
                editCharacterCountText.text = $"{currentLength}/{maxJournalCharacters}"; // Set the updated length as the Character Count text. 
            }
        }

        /// <summary>
        /// Save the edited journal entry.
        /// </summary>
        public void SaveEditedJournalEntry()
        {
            if (currentEditingEntry == null || editJournalInputField == null) // Do not do anything if there is no currently-editing entry to save. 
                return;

            // Update the entry with new content:
            currentEditingEntry.content = editJournalInputField.text;

            // Update mood if dropdown is available:
            if (editMoodDropdown != null && editMoodDropdown.value < editMoodDropdown.options.Count)
            {
                string selectedMood = editMoodDropdown.options[editMoodDropdown.value].text; // Get the text of the updated mood. 
                currentEditingEntry.moodName = selectedMood; // Assign the updated mood to the currently-editing entry. 
                currentEditingEntry.moodIndex = editMoodDropdown.value; // Assign the index of the updated mood to the currently-editing entry. 
            }

            // Save the updated data
            SaveJournalData();
            RefreshJournalEntries();
            
            // Close the edit panel
            CloseEditJournalEntry();
            
            // Show success notification
            if (UIManager.Instance != null)
                UIManager.Instance.ShowNotification("Journal entry updated successfully!");
        }

        /// <summary>
        /// Delete the currently editing journal entry
        /// </summary>
        public void DeleteCurrentEditingEntry()
        {
            if (currentEditingEntry == null)
                return;
            
            // Remove the entry from the list
            journalData.entries.Remove(currentEditingEntry);
            
            // Save the updated data
            SaveJournalData();
            RefreshJournalEntries();
            
            // Close the edit panel
            CloseEditJournalEntry();
            
            // Show success notification
            if (UIManager.Instance != null)
                UIManager.Instance.ShowNotification("Journal entry deleted successfully!");
        }

        /// <summary>
        /// Open Game Credits panel
        /// </summary>
        private void OpenGameCredits()
        {
            if (gameCreditsPanel != null)
                gameCreditsPanel.SetActive(true);
        }

        /// <summary>
        /// Close Game Credits panel
        /// </summary>
        private void CloseGameCredits()
        {
            if (gameCreditsPanel != null)
                gameCreditsPanel.SetActive(false);
        }

        /// <summary>
        /// Handle username input changes
        /// </summary>
        private void OnUsernameChanged(string value)
        {
            // Real-time validation feedback could be added here
        }

        /// <summary>
        /// Handle email input changes
        /// </summary>
        private void OnEmailChanged(string value)
        {
            // Real-time validation feedback could be added here
        }

        /// <summary>
        /// Delete a journal entry
        /// </summary>
        public void DeleteJournalEntry(JournalEntry entry)
        {
            try
            {
                if (journalData.entries.Remove(entry))
                {
                    SaveJournalData();
                    RefreshJournalEntries();
                    
                    // Trigger event
                    OnJournalEntryDeleted?.Invoke(entry);
                    
                    if (UIManager.Instance != null)
                        UIManager.Instance.ShowNotification("Journal entry deleted.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error deleting journal entry: {e.Message}");
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowNotification("Error deleting entry.");
            }
        }

        /// <summary>
        /// Get current profile data
        /// </summary>
        public ProfileData GetProfileData()
        {
            return currentProfile;
        }

        /// <summary>
        /// Get journal entries
        /// </summary>
        public List<JournalEntry> GetJournalEntries()
        {
            return journalData.entries;
        }

        /// <summary>
        /// Navigate to a specific journal entry by index
        /// </summary>
        public void NavigateToSpecificEntry(int entryIndex)
        {
            if (entryIndex >= 0 && entryIndex < journalData.entries.Count)
            {
                currentEntryIndex = entryIndex;
                LoadEntryIntoBook(journalData.entries[currentEntryIndex]);
                UpdateNavigationButtons();
            }
        }

        /// <summary>
        /// Navigate to the previous journal entry
        /// </summary>
        public void NavigateToPreviousEntry()
        {
            if (journalData.entries.Count == 0)
                return;

            if (currentEntryIndex <= 0)
            {
                // Wrap to the last entry
                currentEntryIndex = journalData.entries.Count - 1;
            }
            else
            {
                currentEntryIndex--;
            }

            LoadEntryIntoBook(journalData.entries[currentEntryIndex]);
            UpdateNavigationButtons();
        }

        /// <summary>
        /// Navigate to the next journal entry
        /// </summary>
        public void NavigateToNextEntry()
        {
            if (journalData.entries.Count == 0)
                return;

            if (currentEntryIndex >= journalData.entries.Count - 1)
            {
                // Wrap to the first entry
                currentEntryIndex = 0;
            }
            else
            {
                currentEntryIndex++;
            }

            LoadEntryIntoBook(journalData.entries[currentEntryIndex]);
            UpdateNavigationButtons();
        }

        /// <summary>
        /// Load a specific journal entry into the book interface
        /// </summary>
        private void LoadEntryIntoBook(JournalEntry entry)
        {
            if (entry == null)
                return;

            isViewingEntry = true;

            // Load the entry data into the book interface
            if (journalInputField != null)
            {
                journalInputField.text = entry.content;
                UpdateCharacterCount(journalInputField.text);
            }

            if (journalDateText != null)
            {
                DateTime entryDate = DateTime.Parse(entry.date);
                journalDateText.text = entryDate.ToString("MMMM dd, yyyy");
            }

            if (moodDropdown != null)
            {
                // Set the mood dropdown to the entry's mood
                for (int i = 0; i < moodDropdown.options.Count; i++)
                {
                    if (moodDropdown.options[i].text.Contains(entry.moodName))
                    {
                        moodDropdown.value = i;
                        break;
                    }
                }
            }

            // Change save button text to indicate we're editing
            if (saveJournalButton != null)
            {
                var buttonText = saveJournalButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                    buttonText.text = "Update Entry";
            }

            // Show delete button when viewing existing entries
            if (deleteEntryButton != null)
            {
                deleteEntryButton.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Update the navigation arrow buttons visibility and state
        /// </summary>
        private void UpdateNavigationButtons()
        {
            if (leftArrowButton != null)
            {
                // Show arrows when viewing entries and there are multiple entries
                bool shouldShowArrows = isViewingEntry && journalData.entries.Count > 1;
                leftArrowButton.gameObject.SetActive(shouldShowArrows);
            }

            if (rightArrowButton != null)
            {
                // Show arrows when viewing entries and there are multiple entries
                bool shouldShowArrows = isViewingEntry && journalData.entries.Count > 1;
                rightArrowButton.gameObject.SetActive(shouldShowArrows);
            }
        }

        /// <summary>
        /// Start viewing entries (called when opening book to view entries)
        /// </summary>
        public void StartViewingEntries()
        {
            if (journalData.entries.Count == 0)
            {
                // No entries to view, start fresh
                StartNewEntry();
                return;
            }

            // Start with the first entry
            currentEntryIndex = 0;
            LoadEntryIntoBook(journalData.entries[currentEntryIndex]);
            UpdateNavigationButtons();
        }

        /// <summary>
        /// Start a new entry (clear the book interface)
        /// </summary>
        public void StartNewEntry()
        {
            isViewingEntry = false;
            currentEntryIndex = -1;

            // Clear the book interface
            if (journalInputField != null)
            {
                journalInputField.text = "";
                UpdateCharacterCount("");
            }

            if (journalDateText != null)
            {
                UpdateJournalDate();
            }

            if (moodDropdown != null)
            {
                moodDropdown.value = 0; // Reset to first mood
            }

            // Change save button text back to "Save Entry"
            if (saveJournalButton != null)
            {
                var buttonText = saveJournalButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                    buttonText.text = "Save Entry";
            }

            // Hide delete button for new entries
            if (deleteEntryButton != null)
            {
                deleteEntryButton.gameObject.SetActive(false);
            }

            // Hide navigation arrows for new entry
            UpdateNavigationButtons();
        }

        /// <summary>
        /// Open the book for viewing existing entries
        /// </summary>
        public void OpenBookForViewing()
        {
            if (bookObject != null)
            {
                bookObject.SetActive(true);
                isBookOpen = true;
                
                // Start viewing existing entries
                if (journalData.entries.Count > 0)
                {
                    StartViewingEntries();
                }
                else
                {
                    StartNewEntry();
                }

                // Start auto-save if enabled
                if (enableAutoSave)
                {
                    autoSaveCoroutine = StartCoroutine(AutoSaveDraft());
                }
            }
        }

        /// <summary>
        /// Check if notifications are enabled
        /// </summary>
        public bool AreNotificationsEnabled()
        {
            return currentProfile.notificationsEnabled;
        }

        /// <summary>
        /// Export journal data as JSON
        /// </summary>
        public string ExportJournalData()
        {
            try
            {
                return JsonUtility.ToJson(journalData, true);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error exporting journal data: {e.Message}");
                return "";
            }
        }

        /// <summary>
        /// Clear all journal data
        /// </summary>
        public void ClearAllJournalData()
        {
            try
            {
                journalData.entries.Clear();
                SaveJournalData();
                RefreshJournalEntries();
                
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowNotification("All journal entries cleared.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error clearing journal data: {e.Message}");
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowNotification("Error clearing journal data.");
            }
        }

        /// <summary>
        /// Delete the currently viewing journal entry from the book interface
        /// </summary>
        public void DeleteCurrentEntryFromBook()
        {
            if (currentEntryIndex >= 0 && currentEntryIndex < journalData.entries.Count)
            {
                JournalEntry entryToDelete = journalData.entries[currentEntryIndex];
                
                // Remove the entry from the list
                journalData.entries.RemoveAt(currentEntryIndex);
                
                // Save the updated data
                SaveJournalData();
                RefreshJournalEntries();
                
                // Navigate to the next available entry or close the book
                if (journalData.entries.Count > 0)
                {
                    // Navigate to the next entry (or previous if at the end)
                    if (currentEntryIndex >= journalData.entries.Count)
                    {
                        currentEntryIndex = journalData.entries.Count - 1;
                    }
                    LoadEntryIntoBook(journalData.entries[currentEntryIndex]);
                    UpdateNavigationButtons();
                }
                else
                {
                    // No entries left, close the book
                    CloseBook();
                }
                
                // Show success notification
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowNotification("Journal entry deleted successfully!");
            }
        }

        /// <summary>
        /// Navigate to a specific journal entry by index
        /// </summary>
    }
} 