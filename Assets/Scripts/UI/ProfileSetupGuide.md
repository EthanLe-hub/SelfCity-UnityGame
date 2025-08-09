# Profile Page & Journal Feature Setup Guide

This guide explains how to set up the Profile page and Journal feature in your Unity project.

## Overview

The Profile page includes:
- User profile information (username, email, notifications, age range, gender)
- Journal feature with a book interface for writing entries
- Persistent data storage using PlayerPrefs and JSON files

## Required Scripts

1. **ProfileManager.cs** - Main controller for profile and journal functionality
2. **JournalEntryUI.cs** - UI component for individual journal entries

## Unity Scene Setup

### 1. Create ProfileScrollView Structure

Follow the existing pattern in your scene:
- Create a new GameObject called `ProfileScrollView` under the `Canvas`
- Add the following components:
  - `RectTransform` (set to match other ScrollViews: 1080x1920)
  - `ScrollRect` component
  - `Image` component (for background)
  - `CanvasRenderer` component

### 2. Profile Content Structure

Create the following hierarchy under `ProfileScrollView`:

```
ProfileScrollView
├── Viewport
│   └── Content
│       ├── ProfileHeader (Text - "Profile")
│       ├── UsernameSection
│       │   ├── UsernameLabel (Text - "Username")
│       │   └── UsernameInput (TMP_InputField)
│       ├── EmailSection
│       │   ├── EmailLabel (Text - "Email")
│       │   └── EmailInput (TMP_InputField)
│       ├── NotificationsSection
│       │   ├── NotificationsLabel (Text - "Notifications")
│       │   └── NotificationsToggle (Toggle)
│       ├── AgeRangeSection
│       │   ├── AgeRangeLabel (Text - "Age Range")
│       │   └── AgeRangeDropdown (TMP_Dropdown)
│       ├── GenderSection
│       │   ├── GenderLabel (Text - "Gender")
│       │   └── GenderDropdown (TMP_Dropdown)
│       ├── SaveProfileButton (Button - "Save Profile")
│       └── JournalButton (Button - "Open Journal")
```

### 3. Journal Panel Structure

Create a separate panel for the journal feature:

```
JournalPanel (initially inactive)
├── Background (Image)
├── JournalHeader (Text - "My Journal")
├── OpenBookButton (Button - "Open Book")
├── JournalEntriesContainer (Empty GameObject)
└── CloseJournalButton (Button - "Close")
```

### 4. Book Interface Structure

Create the book interface that appears when writing:

```
BookObject (initially inactive)
├── BookBackground (Image - book sprite)
├── JournalDateText (Text - shows current date)
├── JournalInputField (TMP_InputField - for writing)
├── SaveJournalButton (Button - "Save Entry")
└── CloseBookButton (Button - "Close Book")
```

### 5. Journal Entry Prefab

Create a prefab for displaying journal entries:

```
JournalEntryPrefab
├── Background (Image)
├── DateText (TMP_Text)
├── ContentText (TMP_Text)
└── DeleteButton (Button - "X")
```

## Component Setup

### 1. ProfileManager Component

Add the `ProfileManager` script to the `ProfileScrollView` GameObject and assign the following references:

**Profile UI Elements:**
- Username Input: `UsernameInput` TMP_InputField
- Email Input: `EmailInput` TMP_InputField
- Notifications Toggle: `NotificationsToggle` Toggle
- Age Range Dropdown: `AgeRangeDropdown` TMP_Dropdown
- Gender Dropdown: `GenderDropdown` TMP_Dropdown
- Save Profile Button: `SaveProfileButton` Button
- Journal Button: `JournalButton` Button

**Journal UI Elements:**
- Journal Panel: `JournalPanel` GameObject
- Book Object: `BookObject` GameObject
- Journal Input Field: `JournalInputField` TMP_InputField
- Journal Date Text: `JournalDateText` TMP_Text
- Open Book Button: `OpenBookButton` Button
- Close Book Button: `CloseBookButton` Button
- Save Journal Button: `SaveJournalButton` Button
- Journal Entries Container: `JournalEntriesContainer` Transform
- Journal Entry Prefab: `JournalEntryPrefab` GameObject

### 2. JournalEntryUI Component

Add the `JournalEntryUI` script to the `JournalEntryPrefab` and assign:
- Date Text: `DateText` TMP_Text
- Content Text: `ContentText` TMP_Text
- Delete Button: `DeleteButton` Button

## UIManager Integration

### 1. Update UIManager References

In the `UIManager` component, assign:
- Profile Panel: `ProfileScrollView` GameObject
- Profile Content Panel: `Content` GameObject (under ProfileScrollView)

### 2. Verify Navigation

Ensure the `profileTabButton` in UIManager is properly connected to the Profile button in your bottom navigation bar.

## Data Persistence

### Profile Data
- Stored in PlayerPrefs with keys:
  - `Profile_Username`
  - `Profile_Email`
  - `Profile_Notifications`
  - `Profile_AgeRange`
  - `Profile_Gender`

### Journal Data
- Stored as JSON file: `journal_entries.json`
- Location: `Application.persistentDataPath`
- Contains array of journal entries with date, content, and timestamp

## Features

### Profile Management
- **Username & Email**: Text input fields
- **Notifications**: Toggle switch
- **Age Range**: Dropdown with 7 age categories
- **Gender**: Dropdown with 5 options including "Prefer not to say"
- **Save**: Persists all data to PlayerPrefs

### Journal Feature
- **Open Journal**: Shows journal panel with list of entries
- **Open Book**: Opens writing interface with current date
- **Write Entry**: Multi-line text input with character limit
- **Save Entry**: Saves to JSON file and refreshes display
- **View Entries**: Scrollable list of all journal entries
- **Delete Entries**: Individual delete buttons for each entry

## UI Styling Recommendations

### Colors
- Use consistent colors with your existing UI theme
- Profile sections: Light background with dark text
- Journal entries: Card-style with subtle shadows
- Buttons: Match your existing button styling

### Typography
- Headers: Bold, larger font size
- Labels: Medium weight, readable size
- Input text: Standard size, good contrast
- Journal content: Slightly larger for readability

### Layout
- Consistent spacing between sections (20-30px)
- Proper padding around input fields (10-15px)
- Scrollable content area for journal entries
- Responsive design for different screen sizes

## Testing Checklist

- [ ] Profile data saves and loads correctly
- [ ] Journal entries are created and saved
- [ ] Journal entries display with correct dates
- [ ] Delete functionality works for journal entries
- [ ] Navigation between Profile and other pages works
- [ ] UI elements are properly sized and positioned
- [ ] Data persists between game sessions
- [ ] Error handling works for invalid data

## Troubleshooting

### Common Issues

1. **Profile data not saving**: Check PlayerPrefs.Save() is called
2. **Journal entries not appearing**: Verify JSON file path and format
3. **UI elements not responding**: Check event listeners are properly assigned
4. **Scroll view not working**: Ensure ScrollRect components are configured correctly

### Debug Tips

- Use Debug.Log to track data flow
- Check Unity Console for error messages
- Verify file paths in Application.persistentDataPath
- Test with small amounts of data first

## Future Enhancements

- Add profile picture functionality
- Implement journal entry categories/tags
- Add search functionality for journal entries
- Include mood tracking in journal entries
- Add export/import functionality for journal data
- Implement journal entry templates or prompts 