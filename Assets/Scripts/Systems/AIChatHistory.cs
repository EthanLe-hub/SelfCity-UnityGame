using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LifeCraft.Systems
{
    /// <summary>
    /// Chat History Manager - Handles storage and retrieval of AI conversation history.
    /// 
    /// DESIGN PHILOSOPHY:
    /// - Serializable for Unity's JSON system (matching existing save/load patterns)
    /// - Automatic message limiting to prevent memory issues
    /// - Timestamp tracking for conversation context
    /// - User/AI message distinction for UI display
    /// - Integration with existing PlayerPrefs save system
    /// </summary>
    [System.Serializable]
    public class AIChatHistory
    {
        #region Configuration
        private const int MAX_MESSAGES = 50; // Prevent memory bloat
        private const int MAX_MESSAGE_LENGTH = 1000; // Prevent extremely long messages
        #endregion

        #region Data
        [SerializeField] private List<AIChatMessage> messages = new List<AIChatMessage>();
        [SerializeField] private DateTime lastActivity = DateTime.Now;
        #endregion

        #region Public API
        /// <summary>
        /// Add a new message to the chat history.
        /// REASONING: Automatic cleanup prevents memory issues and maintains performance
        /// </summary>
        public void AddMessage(string content, bool isFromUser)
        {
            // Validate and truncate message if necessary
            if (string.IsNullOrEmpty(content))
            {
                Debug.LogWarning("Attempted to add empty message to chat history");
                return;
            }

            // Truncate overly long messages
            if (content.Length > MAX_MESSAGE_LENGTH)
            {
                content = content.Substring(0, MAX_MESSAGE_LENGTH) + "...";
                Debug.LogWarning($"Message truncated to {MAX_MESSAGE_LENGTH} characters");
            }

            // Create new message
            var message = new AIChatMessage
            {
                content = content,
                isFromUser = isFromUser,
                timestamp = DateTime.Now
            };

            // Add to history
            messages.Add(message);

            // Clean up old messages if we exceed the limit
            CleanupOldMessages();

            // Update last activity
            lastActivity = DateTime.Now;
        }

        /// <summary>
        /// Get all messages in chronological order.
        /// REASONING: Returns a copy to prevent external modification
        /// </summary>
        public List<AIChatMessage> GetMessages()
        {
            return new List<AIChatMessage>(messages);
        }

        /// <summary>
        /// Get recent messages (last N messages).
        /// REASONING: Useful for context when sending to AI service
        /// </summary>
        public List<AIChatMessage> GetRecentMessages(int count = 10)
        {
            return messages.Skip(Math.Max(0, messages.Count - count)).ToList();
        }

        /// <summary>
        /// Get conversation context for AI (recent messages formatted).
        /// REASONING: Provides context to AI for more relevant responses
        /// </summary>
        public string GetConversationContext()
        {
            var recentMessages = GetRecentMessages(5); // Last 5 messages for context
            var context = new System.Text.StringBuilder();

            foreach (var message in recentMessages)
            {
                string role = message.isFromUser ? "User" : "Assistant";
                context.AppendLine($"{role}: {message.content}");
            }

            return context.ToString();
        }

        /// <summary>
        /// Clear all chat history.
        /// REASONING: Privacy feature and memory management
        /// </summary>
        public void Clear()
        {
            messages.Clear();
            lastActivity = DateTime.Now;
            Debug.Log("Chat history cleared");
        }

        /// <summary>
        /// Get the number of messages in history.
        /// </summary>
        public int MessageCount => messages.Count;

        /// <summary>
        /// Get the last activity timestamp.
        /// </summary>
        public DateTime LastActivity => lastActivity;

        /// <summary>
        /// Check if chat history is empty.
        /// </summary>
        public bool IsEmpty => messages.Count == 0;
        #endregion

        #region Private Methods
        /// <summary>
        /// Remove old messages to maintain performance.
        /// REASONING: Prevents memory bloat and maintains responsive UI
        /// </summary>
        private void CleanupOldMessages()
        {
            if (messages.Count <= MAX_MESSAGES)
                return;

            // Remove oldest messages
            int messagesToRemove = messages.Count - MAX_MESSAGES;
            messages.RemoveRange(0, messagesToRemove);

            Debug.Log($"Cleaned up {messagesToRemove} old messages from chat history");
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// Get conversation statistics for analytics.
        /// REASONING: Useful for understanding user engagement patterns
        /// </summary>
        public ChatStatistics GetStatistics()
        {
            return new ChatStatistics
            {
                totalMessages = messages.Count,
                userMessages = messages.Count(m => m.isFromUser),
                aiMessages = messages.Count(m => !m.isFromUser),
                averageMessageLength = messages.Any() ? messages.Average(m => m.content.Length) : 0,
                conversationDuration = messages.Any() ? DateTime.Now - messages.First().timestamp : TimeSpan.Zero
            };
        }

        /// <summary>
        /// Export chat history as text for backup or analysis.
        /// REASONING: Privacy feature allowing users to save their conversations
        /// </summary>
        public string ExportAsText()
        {
            var export = new System.Text.StringBuilder();
            export.AppendLine("SelfCity AI Assistant - Chat History");
            export.AppendLine($"Exported on: {DateTime.Now}");
            export.AppendLine(new string('=', 50));

            foreach (var message in messages)
            {
                string role = message.isFromUser ? "You" : "AI Assistant";
                export.AppendLine($"[{message.timestamp:yyyy-MM-dd HH:mm:ss}] {role}:");
                export.AppendLine(message.content);
                export.AppendLine();
            }

            return export.ToString();
        }
        #endregion
    }

    /// <summary>
    /// Individual chat message data structure.
    /// REASONING: Serializable for Unity's JSON system and clear data organization
    /// </summary>
    [System.Serializable]
    public class AIChatMessage
    {
        [SerializeField] public string content;
        [SerializeField] public bool isFromUser;
        [SerializeField] public DateTime timestamp;

        public AIChatMessage()
        {
            timestamp = DateTime.Now;
        }

        public AIChatMessage(string content, bool isFromUser)
        {
            this.content = content;
            this.isFromUser = isFromUser;
            this.timestamp = DateTime.Now;
        }
    }

    /// <summary>
    /// Chat statistics for analytics and insights.
    /// REASONING: Helps understand user engagement and optimize AI responses
    /// </summary>
    [System.Serializable]
    public class ChatStatistics
    {
        public int totalMessages;
        public int userMessages;
        public int aiMessages;
        public double averageMessageLength;
        public TimeSpan conversationDuration;

        public double UserToAIRatio => aiMessages > 0 ? (double)userMessages / aiMessages : 0;
        public bool IsActive => totalMessages > 0;
    }
}