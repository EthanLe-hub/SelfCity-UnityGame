using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace LifeCraft.UI
{
    /// <summary>
    /// UI component for displaying individual journal entries with mood tracking
    /// </summary>
    public class JournalEntryUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text dateText;
        [SerializeField] private TMP_Text contentText;
        [SerializeField] private TMP_Text moodText;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Button editButton;
        [SerializeField] private GameObject moodIcon;
        [SerializeField] private Button backgroundButton; // This will be the main clickable area

        private ProfileManager.JournalEntry entry;

        private void Start()
        {
            // Make the entire entry background clickable
            if (backgroundButton != null)
                backgroundButton.onClick.AddListener(OpenEditEntry);

            if (deleteButton != null)
                deleteButton.onClick.AddListener(DeleteEntry);

            if (editButton != null)
                editButton.onClick.AddListener(OpenEditEntry);
        }

        /// <summary>
        /// Initialize the journal entry UI
        /// </summary>
        public void Initialize(ProfileManager.JournalEntry journalEntry)
        {
            entry = journalEntry;

            if (dateText != null)
            {
                DateTime entryDate = DateTime.Parse(entry.date);
                dateText.text = entryDate.ToString("MMMM dd, yyyy");
            }

            if (contentText != null)
            {
                // Show a preview of the content (first 50 characters)
                string preview = entry.content.Length > 50 
                    ? entry.content.Substring(0, 50) + "..." 
                    : entry.content;
                contentText.text = preview;
            }

            if (moodText != null && !string.IsNullOrEmpty(entry.moodName))
            {
                moodText.text = entry.moodName;
            }

            // Show mood icon if available
            if (moodIcon != null)
            {
                moodIcon.SetActive(!string.IsNullOrEmpty(entry.moodName));
            }
        }

        /// <summary>
        /// Delete this journal entry
        /// </summary>
        private void DeleteEntry()
        {
            if (ProfileManager.Instance != null)
            {
                ProfileManager.Instance.DeleteJournalEntry(entry);
            }
        }

        /// <summary>
        /// Open the book interface for this journal entry with navigation
        /// </summary>
        private void OpenEditEntry()
        {
            if (ProfileManager.Instance != null)
            {
                // Open the book interface and navigate to this specific entry
                ProfileManager.Instance.OpenBookForViewing();
                
                // Find the index of this entry and navigate to it
                var entries = ProfileManager.Instance.GetJournalEntries();
                for (int i = 0; i < entries.Count; i++)
                {
                    if (entries[i] == entry)
                    {
                        // Navigate to this specific entry
                        ProfileManager.Instance.NavigateToSpecificEntry(i);
                        break;
                    }
                }
            }
        }
    }
} 