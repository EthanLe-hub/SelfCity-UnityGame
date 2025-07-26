# Phase 5D Setup Instructions: Level-Up UI Integration

## Overview
This guide will help you set up the complete Level-Up UI system with real-time EXP progress bar updates and visual feedback.

## Step 1: Create EXP Progress Bar UI

### 1.1 Create the Progress Bar GameObject
1. In your Canvas, create a new GameObject called "EXPProgressBar"
2. Add a **Slider** component to it
3. Set the Slider's **Min Value** to 0 and **Max Value** to 1
4. Set the **Value** to 0

### 1.2 Add Text Elements
1. Create a child GameObject called "LevelText" with **TextMeshPro - Text (UI)**
2. Create a child GameObject called "EXPText" with **TextMeshPro - Text (UI)**
3. Create a child GameObject called "EXPRequiredText" with **TextMeshPro - Text (UI)**

### 1.3 Configure the Slider
1. In the Slider's **Fill Area**, find the **Fill** Image
2. Set a nice color (e.g., blue) for the fill
3. Make sure the **Background** is visible with a different color

## Step 2: Add EXPProgressBarManager Script

### 2.1 Attach the Script
1. Add the `EXPProgressBarManager` script to your "EXPProgressBar" GameObject
2. Assign the UI references in the Inspector:
   - **Exp Progress Bar**: Drag the Slider component
   - **Level Text**: Drag the LevelText TextMeshPro component
   - **Exp Text**: Drag the EXPText TextMeshPro component
   - **Exp Required Text**: Drag the EXPRequiredText TextMeshPro component
   - **Progress Bar Fill**: Drag the Fill Image from the Slider

### 2.2 Configure Settings
- **Fill Animation Duration**: 0.5 (adjust as needed)
- **Text Update Delay**: 0.1 (adjust as needed)
- **Normal Color**: Blue (#0066FF)
- **Level Up Color**: Gold (#FFD700)

## Step 3: Create EXP Popup Prefab

### 3.1 Create the Prefab Structure
1. Create a new GameObject called "EXPPopupPrefab"
2. Add **RectTransform** component
3. Add **CanvasGroup** component
4. Add the `EXPPopup` script

### 3.2 Add Text Component
1. Create a child GameObject called "EXPText"
2. Add **TextMeshPro - Text (UI)** component
3. Configure the text:
   - **Font Size**: 24
   - **Color**: White
   - **Alignment**: Center
   - **Text**: "+10 EXP" (placeholder)

### 3.3 Configure the EXPPopup Script
1. Assign the **Exp Text** reference to the TextMeshPro component
2. Assign the **Canvas Group** reference to the CanvasGroup component

### 3.4 Save as Prefab
1. Drag the "EXPPopupPrefab" from Hierarchy to your Prefabs folder
2. Delete the GameObject from the scene

## Step 4: Set Up EXPPopupManager

### 4.1 Create EXPPopupManager GameObject
1. Create a new GameObject called "EXPPopupManager"
2. Add the `EXPPopupManager` script

### 4.2 Configure References
1. **Exp Popup Prefab**: Drag the EXPPopupPrefab
2. **Popup Parent**: Create a GameObject called "PopupParent" and assign it
3. **Popup Duration**: 2.0
4. **Fade In Duration**: 0.3
5. **Move Distance**: 100

### 4.3 Configure Colors
- **Easy Color**: Green (#00FF00)
- **Medium Color**: Yellow (#FFFF00)
- **Hard Color**: Orange (#FFA500)
- **Expert Color**: Red (#FF0000)

## Step 5: Set Up LevelUpManager

### 5.1 Create LevelUpManager GameObject
1. Create a new GameObject called "LevelUpManager"
2. Add the `LevelUpManager` script

### 5.2 Create Level-Up UI Panel
1. Create a GameObject called "LevelUpPanel"
2. Add **CanvasGroup** component
3. Add **Image** component for background
4. Create child text elements:
   - "LevelUpText" (TextMeshPro - Text (UI))
   - "NewLevelText" (TextMeshPro - Text (UI))
   - "ContinueButton" (Button)

### 5.3 Configure LevelUpManager References
1. **Level Up Panel**: Drag the LevelUpPanel
2. **Level Up Text**: Drag the LevelUpText component
3. **New Level Text**: Drag the NewLevelText component
4. **Continue Button**: Drag the ContinueButton
5. **Canvas Group**: Drag the CanvasGroup component

### 5.4 Configure Settings
- **Celebration Duration**: 3.0
- **Fade In Duration**: 0.5
- **Fade Out Duration**: 0.5

## Step 6: Create Unlock Notification Prefab

### 6.1 Create Notification Structure
1. Create a GameObject called "UnlockNotificationPrefab"
2. Add **RectTransform** and **CanvasGroup**
3. Add **Image** component for background
4. Create child "NotificationText" with TextMeshPro

### 6.2 Configure Text
- **Font Size**: 18
- **Color**: White
- **Alignment**: Center
- **Text**: "New Building Unlocked!" (placeholder)

### 6.3 Save as Prefab
1. Drag to Prefabs folder
2. Delete from scene

### 6.4 Assign to LevelUpManager
1. **Unlock Notification Prefab**: Drag the prefab
2. **Unlock Notification Parent**: Create "NotificationParent" GameObject

## Step 7: Test the System

### 7.1 Test EXP Progress Bar
1. Complete a quest or place a building
2. Watch the progress bar animate
3. Check that text updates correctly

### 7.2 Test Level-Up
1. Gain enough EXP to level up
2. Watch the level-up celebration
3. Check that progress bar flashes
4. Verify unlock notifications appear

### 7.3 Test EXP Popups
1. Complete quests and place buildings
2. Watch for floating EXP numbers
3. Verify colors match difficulty

## Troubleshooting

### Progress Bar Not Updating
- Check that EXPProgressBarManager is subscribed to PlayerLevelManager events
- Verify UI references are assigned correctly
- Check console for error messages

### EXP Popups Not Appearing
- Verify EXPPopupManager prefab is assigned
- Check that popup parent is set
- Ensure EXPPopup component is on the prefab

### Level-Up Not Triggering
- Check that LevelUpManager is subscribed to PlayerLevelManager events
- Verify level-up panel references are assigned
- Check that PlayerLevelManager is working correctly

## Final Notes

- All managers use singleton pattern for easy access
- Events are automatically subscribed/unsubscribed
- Animations are smooth and configurable
- Colors can be customized in the Inspector
- The system is modular and extensible

Your Level-Up UI system is now complete! 🎉 