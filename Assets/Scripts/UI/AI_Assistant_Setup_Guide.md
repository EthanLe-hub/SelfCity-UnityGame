# 🤖 AI Assistant Integration Setup Guide for SelfCity

## 📋 Overview

This guide provides step-by-step instructions for integrating the AI Assistant system into your existing SelfCity Unity project. The AI Assistant provides personalized wellness advice and integrates seamlessly with your existing game systems.

## 🎯 What We're Building

- **AI Assistant Manager**: Core system that handles AI interactions and game integration
- **AI Configuration**: ScriptableObject for easy API setup and management
- **Chat Interface**: Modern modal-based chat system with message history
- **Navigation Integration**: AI button in your existing bottom navigation
- **Game Context Integration**: AI responses based on current game state

## 🛠️ Phase 1: Core System Setup

### Step 1: Create AI Configuration Asset

1. **In Unity Project Window:**
   - Right-click in `Assets/Resources/` folder
   - Select `Create → SelfCity → AI Configuration`
   - Name it `AIConfiguration`

2. **Configure the AI Settings:**
   ```
   AI Service Settings:
   - Service Type: Azure OpenAI (recommended) or OpenAI API
   
   Azure OpenAI Settings:
   - Azure Endpoint: https://your-resource.openai.azure.com/
   - Deployment Name: gpt-4
   - API Version: 2024-02-15-preview
   
   API Key:
   - Enter your API key (will be encrypted in production)
   
   AI Response Parameters:
   - Max Tokens: 150
   - Temperature: 0.7
   - System Prompt: [Default wellness-focused prompt]
   
   Environment Settings:
   - Development Mode: true (for testing)
   - Enable Debug Logging: true
   ```

3. **Set up your API key:**
   - For Azure OpenAI: Get your endpoint and API key from Azure portal
   - For OpenAI API: Get your API key from OpenAI dashboard

### Step 2: Add AI Manager to Scene

1. **Create AI Manager GameObject:**
   - In Hierarchy: Right-click → Create Empty
   - Rename to `AIAssistantManager`
   - Add component: `AIAssistantManager.cs`

2. **Configure AI Manager:**
   ```
   AI Configuration:
   - Assign your AIConfiguration asset
   
   Game System References:
   - Quest Manager: Auto-find or assign manually
   - Resource Manager: Auto-find or assign manually
   - Player Level Manager: Auto-find or assign manually
   - Assessment Quiz Manager: Auto-find or assign manually
   ```

## 🎨 Phase 2: UI Integration

### Step 3: Create AI Assistant Modal

1. **Create Modal Structure:**
   ```
   AIAssistantModal (GameObject)
   ├── CanvasGroup
   ├── ModalPanel (Image)
   │   ├── Header
   │   │   ├── Title (TMP_Text - "AI Assistant")
   │   │   └── CloseButton (Button)
   │   ├── ChatArea (ScrollRect)
   │   │   ├── Viewport
   │   │   │   └── Content (VerticalLayoutGroup)
   │   │   └── Scrollbar
   │   ├── InputArea
   │   │   ├── MessageInput (TMP_InputField)
   │   │   ├── SendButton (Button)
   │   │   └── ClearButton (Button)
   │   └── StatusBar
   │       ├── StatusText (TMP_Text - "Ready")
   │       └── TypingIndicator (GameObject)
   ```

2. **Add AIAssistantModal Component:**
   - Select AIAssistantModal GameObject
   - Add component: `AIAssistantModal.cs`
   - Assign all UI references in the Inspector

### Step 4: Create Message Prefabs

1. **Create User Message Prefab:**
   ```
   UserMessagePrefab (GameObject)
   ├── Image (Background - Blue color)
   ├── AIChatMessageUI.cs
   ├── MessageText (TMP_Text)
   └── TimestampText (TMP_Text)
   ```

2. **Create AI Message Prefab:**
   ```
   AIMessagePrefab (GameObject)
   ├── Image (Background - Gray color)
   ├── AIChatMessageUI.cs
   ├── MessageText (TMP_Text)
   └── TimestampText (TMP_Text)
   ```

3. **Configure Message Prefabs:**
   - Add `AIChatMessageUI.cs` component to both prefabs
   - Set appropriate colors and styling
   - Assign text components in the Inspector

### Step 5: Add AI Button to Navigation

1. **Create AI Button:**
   ```
   AIAssistantButton (GameObject)
   ├── Button
   ├── AIAssistantButton.cs
   ├── ButtonText (TMP_Text - "AI Agent")
   ├── ButtonIcon (Image - Optional)
   ├── NotificationBadge (Image)
   └── NotificationCount (TMP_Text)
   ```

2. **Add to Bottom Navigation:**
   - Place AIAssistantButton in your existing bottom navigation bar
   - Position it between existing buttons (City, Home, Shop, Profile)
   - Ensure it follows the same styling as other navigation buttons

3. **Configure AI Button:**
   - Add `AIAssistantButton.cs` component
   - Assign button references in Inspector
   - Set button text to "AI Agent"

## 🔧 Phase 3: Integration with Existing Systems

### Step 6: Update UIManager (Optional)

If you want to integrate AI Assistant into your existing navigation system:

1. **Add AI Panel Reference:**
   ```csharp
   // In UIManager.cs, add to existing panel references:
   [SerializeField] private GameObject aiPanel;
   [SerializeField] private Button aiTabButton;
   ```

2. **Add AI Panel Method:**
   ```csharp
   public void ShowAIPanel()
   {
       if (homePanel != null) homePanel.SetActive(false);
       if (cityPanel != null) cityPanel.SetActive(false);
       if (shopPanel != null) shopPanel.SetActive(false);
       if (profilePanel != null) profilePanel.SetActive(false);
       if (aiPanel != null) aiPanel.SetActive(true);
   }
   ```

3. **Add Event Listener:**
   ```csharp
   // In SetupEventListeners method:
   if (aiTabButton != null)
       aiTabButton.onClick.AddListener(ShowAIPanel);
   ```

### Step 7: Test Integration

1. **Test AI Configuration:**
   - Select AIConfiguration asset
   - Right-click → "Setup Development Configuration"
   - Verify API key is set

2. **Test AI Manager:**
   - Play the scene
   - Check Console for "AI Assistant Manager initialized successfully"
   - Verify no errors in AI configuration

3. **Test UI Integration:**
   - Click AI Agent button
   - Verify modal opens with fade animation
   - Test message input and send functionality

## 🎮 Phase 4: Game Context Integration

### Step 8: Verify Game System Integration

The AI Assistant automatically integrates with your existing systems:

- **Quest Manager**: AI can suggest relevant quests based on current progress
- **Resource Manager**: AI considers current resources when giving advice
- **Player Level Manager**: AI provides level-appropriate suggestions
- **Assessment Quiz Manager**: AI uses wellness assessment results for personalized advice

### Step 9: Test Context-Aware Responses

Try these example interactions:

```
User: "Can you help me meal-prep healthy foods for this week?"
AI: [Considers current Health Harbor progress and available quests]

User: "What should I focus on next in the game?"
AI: [Analyzes current level, resources, and available buildings]

User: "I'm feeling stressed, any suggestions?"
AI: [Uses Mind Palace assessment results for personalized advice]
```

## 🔒 Phase 5: Security & Production Setup

### Step 10: Production Configuration

1. **Secure API Keys:**
   - Select AIConfiguration asset
   - Right-click → "Setup Production Configuration"
   - Enable encryption for API keys

2. **Environment Settings:**
   ```
   Development Mode: false
   Enable Debug Logging: false
   Use Encrypted Key: true
   Max Tokens: 100 (shorter responses for production)
   ```

3. **Build Settings:**
   - Ensure internet permissions are enabled
   - Test on target platforms (mobile, desktop)

## 🧪 Testing & Debugging

### Common Issues & Solutions:

1. **"AI Configuration not set" Error:**
   - Ensure AIConfiguration asset is assigned to AIAssistantManager
   - Check that API key is not empty

2. **"API request failed" Error:**
   - Verify API key is correct
   - Check internet connection
   - Ensure Azure endpoint is accessible

3. **Modal not opening:**
   - Check AIAssistantModal is assigned to AIAssistantButton
   - Verify CanvasGroup settings
   - Ensure modal is in correct Canvas

4. **Messages not displaying:**
   - Verify message prefabs are assigned
   - Check AIChatMessageUI components
   - Ensure ScrollRect is properly configured

### Debug Commands:

```csharp
// In AIAssistantManager Inspector:
[ContextMenu("Test AI Response")]
public void TestAIResponse()
{
    SendMessage("Hello, can you help me with wellness advice?");
}

// In AIAssistantButton Inspector:
[ContextMenu("Test AI Assistant")]
public void TestAIAssistant()
{
    ShowAIAssistant();
}
```

## 📊 Performance Considerations

### Optimization Tips:

1. **Message History:**
   - Limited to 50 messages to prevent memory bloat
   - Automatic cleanup of old messages
   - Efficient JSON serialization

2. **API Calls:**
   - Rate limiting (1 second between requests)
   - Async operations to prevent UI freezing
   - Error handling with graceful fallbacks

3. **UI Performance:**
   - Object pooling for message prefabs (future enhancement)
   - Efficient scrolling with ScrollRect
   - Minimal animations for smooth performance

## 🚀 Advanced Features (Future Enhancements)

### Potential Additions:

1. **Voice Integration:**
   - Text-to-speech for AI responses
   - Speech-to-text for user input

2. **Enhanced Context:**
   - Real-time health data integration
   - Calendar integration for scheduling
   - Weather-based wellness suggestions

3. **Personalization:**
   - Learning user preferences over time
   - Custom wellness goals tracking
   - Progress analytics and insights

## 📝 Summary

The AI Assistant system is now fully integrated into your SelfCity project! Key features:

✅ **Seamless Integration**: Follows existing architectural patterns
✅ **Context-Aware**: Uses game state for personalized responses  
✅ **Modern UI**: Smooth animations and responsive design
✅ **Secure**: API key encryption and proper error handling
✅ **Scalable**: Easy to extend with additional features

The AI Assistant will enhance player engagement by providing personalized wellness advice that's directly relevant to their current game progress and real-life wellness goals.

## 🆘 Support

If you encounter any issues during setup:

1. Check the Console for error messages
2. Verify all components are properly assigned
3. Test API connectivity independently
4. Review the debug commands for troubleshooting

The system is designed to be robust and provide clear error messages to help with troubleshooting.