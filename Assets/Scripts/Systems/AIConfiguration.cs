using UnityEngine;

namespace LifeCraft.Systems
{
    /// <summary>
    /// AI Configuration ScriptableObject - Stores API settings and AI parameters.
    /// 
    /// DESIGN PHILOSOPHY:
    /// - ScriptableObject allows easy configuration without code changes
    /// - Supports multiple AI services (Azure OpenAI, OpenAI API)
    /// - Secure API key storage with encryption
    /// - Configurable AI parameters for different use cases
    /// - Environment-specific settings (development vs production)
    /// </summary>
    [CreateAssetMenu(fileName = "AIConfiguration", menuName = "SelfCity/AI Configuration")]
    public class AIConfiguration : ScriptableObject
    {
        #region AI Service Configuration
        [Header("AI Service Settings")]
        [SerializeField] private AIServiceType serviceType = AIServiceType.AzureOpenAI;
        
        [Header("Azure OpenAI Settings")]
        [SerializeField] private string azureEndpoint = "https://your-resource.openai.azure.com/";
        [SerializeField] private string azureDeploymentName = "gpt-4";
        [SerializeField] private string azureApiVersion = "2024-02-15-preview";
        
        [Header("OpenAI API Settings")]
        [SerializeField] private string openAIEndpoint = "https://api.openai.com/v1/chat/completions";
        
        [Header("API Key")]
        [SerializeField] private string apiKey = "";
        [SerializeField] private bool useEncryptedKey = true;
        #endregion

        #region AI Parameters
        [Header("AI Response Parameters")]
        [SerializeField] private int maxTokens = 150;
        [SerializeField] private float temperature = 0.7f;
        [SerializeField] private string systemPrompt = @"You are a helpful AI assistant in SelfCity, a wellness-focused city-building game. 
Your role is to provide encouraging, supportive advice related to health, wellness, and personal development. 
Keep responses concise, positive, and relevant to the player's current situation in the game.";
        #endregion

        #region Environment Settings
        [Header("Environment Settings")]
        [SerializeField] private bool isDevelopmentMode = true;
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private float requestTimeout = 30f;
        #endregion

        #region Enums
        public enum AIServiceType
        {
            AzureOpenAI,
            OpenAIAPI
        }
        #endregion

        #region Public Properties
        public string ApiKey
        {
            get
            {
                // REASONING: Encrypt API keys in production builds for security
                if (useEncryptedKey && !isDevelopmentMode)
                {
                    return DecryptApiKey(apiKey);
                }
                return apiKey;
            }
        }

        public int MaxTokens => maxTokens;
        public float Temperature => temperature;
        public string SystemPrompt => systemPrompt;
        public bool IsDevelopmentMode => isDevelopmentMode;
        public bool EnableDebugLogging => enableDebugLogging;
        public float RequestTimeout => requestTimeout;
        #endregion

        #region Endpoint Generation
        /// <summary>
        /// Get the appropriate endpoint URL based on service type.
        /// REASONING: Supports multiple AI services with different URL structures
        /// </summary>
        public string GetEndpoint()
        {
            switch (serviceType)
            {
                case AIServiceType.AzureOpenAI:
                    return $"{azureEndpoint}openai/deployments/{azureDeploymentName}/chat/completions?api-version={azureApiVersion}";
                
                case AIServiceType.OpenAIAPI:
                    return openAIEndpoint;
                
                default:
                    Debug.LogError($"Unsupported AI service type: {serviceType}");
                    return openAIEndpoint;
            }
        }

        /// <summary>
        /// Get the appropriate API key header name based on service type.
        /// REASONING: Different services use different header names for authentication
        /// </summary>
        public string GetApiKeyHeaderName()
        {
            switch (serviceType)
            {
                case AIServiceType.AzureOpenAI:
                    return "api-key";
                
                case AIServiceType.OpenAIAPI:
                    return "Authorization";
                
                default:
                    return "api-key";
            }
        }

        /// <summary>
        /// Get the formatted API key value for the service.
        /// REASONING: OpenAI API requires "Bearer " prefix, Azure doesn't
        /// </summary>
        public string GetFormattedApiKey()
        {
            switch (serviceType)
            {
                case AIServiceType.AzureOpenAI:
                    return ApiKey;
                
                case AIServiceType.OpenAIAPI:
                    return $"Bearer {ApiKey}";
                
                default:
                    return ApiKey;
            }
        }
        #endregion

        #region Security
        /// <summary>
        /// Simple encryption for API keys in production builds.
        /// REASONING: Basic protection against casual inspection of build files
        /// Note: For production, consider using Unity's encryption utilities or external key management
        /// </summary>
        private string EncryptApiKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return key;
            
            // Simple XOR encryption with a fixed key
            // REASONING: This is basic protection - for production, use stronger encryption
            const string encryptionKey = "SelfCityAI2024";
            string result = "";
            
            for (int i = 0; i < key.Length; i++)
            {
                result += (char)(key[i] ^ encryptionKey[i % encryptionKey.Length]);
            }
            
            return result;
        }

        private string DecryptApiKey(string encryptedKey)
        {
            if (string.IsNullOrEmpty(encryptedKey)) return encryptedKey;
            
            // Simple XOR decryption
            const string encryptionKey = "SelfCityAI2024";
            string result = "";
            
            for (int i = 0; i < encryptedKey.Length; i++)
            {
                result += (char)(encryptedKey[i] ^ encryptionKey[i % encryptionKey.Length]);
            }
            
            return result;
        }
        #endregion

        #region Validation
        /// <summary>
        /// Validate the configuration settings.
        /// REASONING: Catch configuration errors early to prevent runtime issues
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogError("AI Configuration: API Key is not set!");
                return false;
            }

            if (maxTokens <= 0 || maxTokens > 1000)
            {
                Debug.LogError("AI Configuration: Max tokens must be between 1 and 1000!");
                return false;
            }

            if (temperature < 0f || temperature > 2f)
            {
                Debug.LogError("AI Configuration: Temperature must be between 0 and 2!");
                return false;
            }

            if (string.IsNullOrEmpty(systemPrompt))
            {
                Debug.LogError("AI Configuration: System prompt is not set!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Log configuration details for debugging.
        /// REASONING: Helpful for troubleshooting API connection issues
        /// </summary>
        public void LogConfiguration()
        {
            if (!enableDebugLogging) return;

            Debug.Log($"AI Configuration Loaded:");
            Debug.Log($"- Service Type: {serviceType}");
            Debug.Log($"- Endpoint: {GetEndpoint()}");
            Debug.Log($"- Max Tokens: {maxTokens}");
            Debug.Log($"- Temperature: {temperature}");
            Debug.Log($"- Development Mode: {isDevelopmentMode}");
            Debug.Log($"- API Key Set: {!string.IsNullOrEmpty(apiKey)}");
        }
        #endregion

        #region Editor Helpers
        #if UNITY_EDITOR
        /// <summary>
        /// Set up default configuration for development.
        /// REASONING: Makes it easy to get started with AI integration
        /// </summary>
        [ContextMenu("Setup Development Configuration")]
        public void SetupDevelopmentConfig()
        {
            serviceType = AIServiceType.AzureOpenAI;
            azureEndpoint = "https://your-resource.openai.azure.com/";
            azureDeploymentName = "gpt-4";
            azureApiVersion = "2024-02-15-preview";
            maxTokens = 150;
            temperature = 0.7f;
            isDevelopmentMode = true;
            enableDebugLogging = true;
            
            Debug.Log("Development configuration set up. Remember to add your API key!");
        }

        /// <summary>
        /// Set up production configuration with security settings.
        /// REASONING: Ensures proper security settings for production builds
        /// </summary>
        [ContextMenu("Setup Production Configuration")]
        public void SetupProductionConfig()
        {
            isDevelopmentMode = false;
            enableDebugLogging = false;
            useEncryptedKey = true;
            maxTokens = 100; // Shorter responses for production
            
            Debug.Log("Production configuration set up. Remember to encrypt your API key!");
        }
        #endif
        #endregion
    }
}