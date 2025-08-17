using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using LifeCraft.Core;
using LifeCraft.Systems;
using LifeCraft.UI;

namespace LifeCraft.Systems
{
    /// <summary>
    /// Main AI Assistant Manager - Handles all AI interactions and integrates with game systems.
    /// 
    /// DESIGN PHILOSOPHY:
    /// - Singleton pattern to match existing architecture (UIManager, GameManager, etc.)
    /// - Event-driven system for loose coupling with UI components
    /// - Context-aware responses based on current game state
    /// - Secure API integration with proper error handling
    /// - Integration with existing quest and progression systems
    /// </summary>
    public class AIAssistantManager : MonoBehaviour
    {
        #region Singleton Pattern
        // REASONING: Following the same singleton pattern used in UIManager, GameManager, etc.
        // This ensures consistent access patterns across the codebase and prevents multiple instances
        public static AIAssistantManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        #endregion

        #region Configuration
        [Header("AI Configuration")]
        [SerializeField] private AIConfiguration aiConfig;
        
        [Header("Game System References")]
        [SerializeField] private QuestManager questManager;
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private PlayerLevelManager playerLevelManager;
        [SerializeField] private AssessmentQuizManager assessmentManager;
        
        [Header("UI References")]
        [SerializeField] private AIAssistantModal aiModal;
        #endregion

        #region Events
        // REASONING: Event-driven architecture allows UI components to subscribe to AI events
        // without tight coupling, following the same pattern used in other managers
        public System.Action<string> OnAIResponseReceived;
        public System.Action<string> OnAIErrorOccurred;
        public System.Action<bool> OnAIProcessingStateChanged;
        #endregion

        #region Private Fields
        private AIChatHistory chatHistory;
        private bool isProcessingRequest = false;
        private float lastRequestTime = 0f;
        private const float RATE_LIMIT_DELAY = 1f; // Prevent spam requests
        #endregion

        #region Initialization
        private void Start()
        {
            InitializeAI();
            LoadChatHistory();
        }

        private void InitializeAI()
        {
            // REASONING: Auto-find references if not assigned, following existing patterns
            if (questManager == null)
                questManager = FindFirstObjectByType<QuestManager>();
            if (resourceManager == null)
                resourceManager = FindFirstObjectByType<ResourceManager>();
            if (playerLevelManager == null)
                playerLevelManager = FindFirstObjectByType<PlayerLevelManager>();
            if (assessmentManager == null)
                assessmentManager = FindFirstObjectByType<AssessmentQuizManager>();

            chatHistory = new AIChatHistory();
            
            Debug.Log("AI Assistant Manager initialized successfully");
        }
        #endregion

        #region Public API
        /// <summary>
        /// Send a message to the AI assistant and get a response.
        /// REASONING: Async method allows non-blocking UI during API calls
        /// </summary>
        public async Task<string> SendAIMessage(string userMessage)
        {
            // Rate limiting to prevent API abuse
            if (Time.time - lastRequestTime < RATE_LIMIT_DELAY)
            {
                return "Please wait a moment before sending another message.";
            }

            if (isProcessingRequest)
            {
                return "I'm still processing your previous message. Please wait.";
            }

            try
            {
                isProcessingRequest = true;
                OnAIProcessingStateChanged?.Invoke(true);
                lastRequestTime = Time.time;

                // Add user message to history
                chatHistory.AddMessage(userMessage, true);

                // Get game context for personalized responses
                string gameContext = GetGameContext();
                
                // Create AI prompt with context
                string aiPrompt = CreateAIPrompt(userMessage, gameContext);

                // Send to AI service
                string aiResponse = await SendToAIService(aiPrompt);

                // Add AI response to history
                chatHistory.AddMessage(aiResponse, false);

                // Save chat history
                SaveChatHistory();

                // Trigger events
                OnAIResponseReceived?.Invoke(aiResponse);

                return aiResponse;
            }
            catch (Exception e)
            {
                string errorMessage = "I'm having trouble connecting right now. Please try again later.";
                Debug.LogError($"AI Assistant Error: {e.Message}");
                OnAIErrorOccurred?.Invoke(errorMessage);
                return errorMessage;
            }
            finally
            {
                isProcessingRequest = false;
                OnAIProcessingStateChanged?.Invoke(false);
            }
        }

        /// <summary>
        /// Get the current chat history for display in UI
        /// </summary>
        public List<AIChatMessage> GetChatHistory()
        {
            return chatHistory.GetMessages();
        }

        /// <summary>
        /// Clear chat history (useful for privacy or performance)
        /// </summary>
        public void ClearChatHistory()
        {
            chatHistory.Clear();
            SaveChatHistory();
        }

        /// <summary>
        /// Show the AI assistant modal
        /// REASONING: Centralized access point for UI integration
        /// </summary>
        public void ShowAIAssistant()
        {
            if (aiModal != null)
            {
                aiModal.Show();
            }
            else
            {
                Debug.LogWarning("AI Modal not assigned to AIAssistantManager");
            }
        }
        #endregion

        #region Game Context Integration
        /// <summary>
        /// Get current game state context for personalized AI responses.
        /// REASONING: This makes AI responses relevant to the player's current situation
        /// </summary>
        private string GetGameContext()
        {
            var context = new System.Text.StringBuilder();
            
            // Player level and progression
            if (playerLevelManager != null)
            {
                context.AppendLine($"Player Level: {playerLevelManager.GetCurrentLevel()}");
                context.AppendLine($"Current EXP: {playerLevelManager.GetCurrentEXP()}");
            }

            // Available quests
            if (questManager != null)
            {
                var dailyQuests = questManager.GetDailyQuests();
                var customQuests = questManager.GetCustomQuests("Health Harbor"); // Default to Health Harbor region
                
                context.AppendLine($"Available Daily Quests: {dailyQuests?.Count ?? 0}");
                context.AppendLine($"Custom Quests: {customQuests?.Count ?? 0}");
            }

            // Resource status
            if (resourceManager != null)
            {
                context.AppendLine("Current Resources:");
                // Use the available resource types and get their amounts
                var resourceTypes = System.Enum.GetValues(typeof(ResourceManager.ResourceType));
                foreach (ResourceManager.ResourceType resourceType in resourceTypes)
                {
                    int amount = resourceManager.GetResourceTotal(resourceType);
                    string displayName = resourceManager.GetResourceDisplayName(resourceType);
                    context.AppendLine($"- {displayName}: {amount}");
                }
            }

            // Assessment results for personalized advice
            if (assessmentManager != null)
            {
                var scores = assessmentManager.GetRegionScores();
                context.AppendLine("Wellness Assessment Results:");
                foreach (var score in scores)
                {
                    context.AppendLine($"- {score.Key}: {score.Value}/100");
                }
            }

            return context.ToString();
        }

        /// <summary>
        /// Create AI prompt with game context and user message.
        /// REASONING: Structured prompts ensure consistent, relevant responses
        /// </summary>
        private string CreateAIPrompt(string userMessage, string gameContext)
        {
            return $@"You are a helpful AI assistant in SelfCity, a wellness-focused city-building game. 
The player is asking: ""{userMessage}""

Current game context:
{gameContext}

Please provide helpful, encouraging advice that:
1. Relates to wellness, health, and personal development
2. Considers the player's current game progress
3. Suggests relevant quests or activities when appropriate
4. Maintains a supportive, motivational tone
5. Keeps responses concise (2-3 sentences maximum)

Response:";
        }
        #endregion

        #region AI Service Integration
        /// <summary>
        /// Send message to AI service (Azure OpenAI or OpenAI API).
        /// REASONING: Async method prevents UI freezing during API calls
        /// </summary>
        private async Task<string> SendToAIService(string prompt)
        {
            if (aiConfig == null)
            {
                throw new InvalidOperationException("AI Configuration not set");
            }

            // REASONING: Use UnityWebRequest for cross-platform compatibility
            using (var request = new UnityEngine.Networking.UnityWebRequest(aiConfig.GetEndpoint(), "POST"))
            {
                // Set headers for API authentication
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("api-key", aiConfig.ApiKey);

                // Create request body
                var requestBody = new
                {
                    messages = new[]
                    {
                        new { role = "system", content = aiConfig.SystemPrompt },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = aiConfig.MaxTokens,
                    temperature = aiConfig.Temperature
                };

                string jsonBody = JsonUtility.ToJson(requestBody);
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();

                // Send request
                var operation = request.SendWebRequest();

                // Wait for completion
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                // Handle response
                if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<AIResponse>(request.downloadHandler.text);
                    return response.choices[0].message.content;
                }
                else
                {
                    throw new Exception($"API request failed: {request.error}");
                }
            }
        }
        #endregion

        #region Data Persistence
        /// <summary>
        /// Save chat history to local storage.
        /// REASONING: Follows existing save/load patterns in the project
        /// </summary>
        private void SaveChatHistory()
        {
            try
            {
                string json = JsonUtility.ToJson(chatHistory);
                PlayerPrefs.SetString("AIChatHistory", json);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save chat history: {e.Message}");
            }
        }

        /// <summary>
        /// Load chat history from local storage.
        /// REASONING: Maintains conversation continuity across game sessions
        /// </summary>
        private void LoadChatHistory()
        {
            try
            {
                string json = PlayerPrefs.GetString("AIChatHistory", "");
                if (!string.IsNullOrEmpty(json))
                {
                    chatHistory = JsonUtility.FromJson<AIChatHistory>(json);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load chat history: {e.Message}");
                chatHistory = new AIChatHistory();
            }
        }
        #endregion

        #region Response Models
        // REASONING: Structured response models ensure type safety and easy parsing
        [System.Serializable]
        private class AIResponse
        {
            public Choice[] choices;
        }

        [System.Serializable]
        private class Choice
        {
            public Message message;
        }

        [System.Serializable]
        private class Message
        {
            public string content;
        }
        #endregion
    }
}