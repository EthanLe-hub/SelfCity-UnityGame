using System.Collections.Generic;
using UnityEngine;
using System;

namespace LifeCraft.Systems
{
    /// <summary>
    /// Manages the optional health assessment quiz for new players.
    /// Determines recommended starting region based on quiz answers.
    /// </summary>
    [CreateAssetMenu(fileName = "AssessmentQuizManager", menuName = "LifeCraft/Assessment Quiz Manager")]
    public class AssessmentQuizManager : ScriptableObject
    {

        // Singleton Instance (to ensure only one instance of the class exists)
        private static AssessmentQuizManager _instance; // Create a private static instance of the class. 
        public static AssessmentQuizManager Instance // Create a public static instance of the class that can be accessed from other scripts. 
        {
            get 
            {
                if (_instance == null) // If the instance is null, create a new instance of the class. 
                {
                    _instance = Resources.Load<AssessmentQuizManager>("MyAssessmentQuizManager"); // Load the AssessmentQuizManager asset from the Assets/Resources/ folder on Unity Editor. 
                    
                    if (_instance == null) // If the instance is STILL null, throw an error. 
                    {
                        Debug.LogError("MyAssessmentQuizManager not found in Resources folder on Unity Editor!"); 
                    }
                }

                return _instance; // Return the instance of the class. 
            }
        }

        [System.Serializable]
        public class QuizQuestion
        {
            public string questionText;
            public List<QuizAnswer> answers;
        }

        [System.Serializable]
        public class QuizAnswer
        {
            public string answerText;
            public RegionType region;
            public int score;
        }

        [System.Serializable]
        public enum RegionType // RegionType enum is used to store the region types. 
        {
            HealthHarbor,
            MindPalace,
            CreativeCommons,
            SocialSquare,
            AllRegions
        }

        [Header("Quiz Configuration")]
        [SerializeField] private List<QuizQuestion> questions = new List<QuizQuestion>();
        [SerializeField] private int questionsToShow = 12; // Number of questions to randomly select

        [Header("Region Icons")]
        [SerializeField] private Sprite healthHarborIcon;
        [SerializeField] private Sprite mindPalaceIcon;
        [SerializeField] private Sprite creativeCommonsIcon;
        [SerializeField] private Sprite socialSquareIcon;

        [Header("Default Questions")]
        [SerializeField] private List<QuizQuestion> defaultQuestions = new List<QuizQuestion>
        {
            new QuizQuestion
            {
                questionText = "When you're feeling stressed or anxious, what's your natural instinct?",
                answers = new List<QuizAnswer>
                {
                    new QuizAnswer { answerText = "I need to move my body - exercise or physical activity", region = RegionType.HealthHarbor, score = 1 },
                    new QuizAnswer { answerText = "I seek quiet time to reflect and process my thoughts", region = RegionType.MindPalace, score = 1 },
                    new QuizAnswer { answerText = "I channel my emotions into creative expression", region = RegionType.CreativeCommons, score = 1 },
                    new QuizAnswer { answerText = "I reach out to someone I trust for support", region = RegionType.SocialSquare, score = 1 }
                }
            },
            new QuizQuestion
            {
                questionText = "How would you describe your current relationship with your physical health?",
                answers = new List<QuizAnswer>
                {
                    new QuizAnswer { answerText = "I'm actively working on improving my fitness and nutrition", region = RegionType.HealthHarbor, score = 1 },
                    new QuizAnswer { answerText = "I'm more focused on mental and emotional balance", region = RegionType.MindPalace, score = 1 },
                    new QuizAnswer { answerText = "I express myself through movement and physical creativity", region = RegionType.CreativeCommons, score = 1 },
                    new QuizAnswer { answerText = "I prefer activities that involve others and build community", region = RegionType.SocialSquare, score = 1 }
                }
            },
            new QuizQuestion
            {
                questionText = "What type of environment helps you feel most centered and at peace?",
                answers = new List<QuizAnswer>
                {
                    new QuizAnswer { answerText = "Outdoor spaces with fresh air and natural movement", region = RegionType.HealthHarbor, score = 1 },
                    new QuizAnswer { answerText = "Quiet, contemplative spaces for reflection", region = RegionType.MindPalace, score = 1 },
                    new QuizAnswer { answerText = "Creative studios filled with inspiration and tools", region = RegionType.CreativeCommons, score = 1 },
                    new QuizAnswer { answerText = "Warm, welcoming spaces where people gather", region = RegionType.SocialSquare, score = 1 }
                }
            },
            new QuizQuestion
            {
                questionText = "How do you typically process and work through difficult emotions?",
                answers = new List<QuizAnswer>
                {
                    new QuizAnswer { answerText = "Physical activity helps me release tension and clear my mind", region = RegionType.HealthHarbor, score = 1 },
                    new QuizAnswer { answerText = "I need time alone to understand and process my feelings", region = RegionType.MindPalace, score = 1 },
                    new QuizAnswer { answerText = "I express my emotions through art, music, or writing", region = RegionType.CreativeCommons, score = 1 },
                    new QuizAnswer { answerText = "I talk through my feelings with trusted friends or family", region = RegionType.SocialSquare, score = 1 }
                }
            },
            new QuizQuestion
            {
                questionText = "What motivates you most when setting personal goals?",
                answers = new List<QuizAnswer>
                {
                    new QuizAnswer { answerText = "Achieving physical milestones and seeing tangible progress", region = RegionType.HealthHarbor, score = 1 },
                    new QuizAnswer { answerText = "Personal growth and developing deeper self-awareness", region = RegionType.MindPalace, score = 1 },
                    new QuizAnswer { answerText = "Creating something meaningful and expressing my vision", region = RegionType.CreativeCommons, score = 1 },
                    new QuizAnswer { answerText = "Building connections and making a positive impact on others", region = RegionType.SocialSquare, score = 1 }
                }
            },
            new QuizQuestion
            {
                questionText = "When you're feeling disconnected from yourself, what helps you reconnect?",
                answers = new List<QuizAnswer>
                {
                    new QuizAnswer { answerText = "Getting back in touch with my body through movement", region = RegionType.HealthHarbor, score = 1 },
                    new QuizAnswer { answerText = "Taking time for introspection and mindfulness", region = RegionType.MindPalace, score = 1 },
                    new QuizAnswer { answerText = "Engaging in creative activities that feel authentic", region = RegionType.CreativeCommons, score = 1 },
                    new QuizAnswer { answerText = "Spending quality time with people who truly know me", region = RegionType.SocialSquare, score = 1 }
                }
            },
            new QuizQuestion
            {
                questionText = "How do you prefer to celebrate your achievements and successes?",
                answers = new List<QuizAnswer>
                {
                    new QuizAnswer { answerText = "By setting new physical challenges and pushing my limits", region = RegionType.HealthHarbor, score = 1 },
                    new QuizAnswer { answerText = "Taking time to reflect on my growth and learning", region = RegionType.MindPalace, score = 1 },
                    new QuizAnswer { answerText = "Creating something beautiful to commemorate the moment", region = RegionType.CreativeCommons, score = 1 },
                    new QuizAnswer { answerText = "Sharing the joy with friends and family who supported me", region = RegionType.SocialSquare, score = 1 }
                }
            },
            new QuizQuestion
            {
                questionText = "What type of learning experiences do you find most rewarding?",
                answers = new List<QuizAnswer>
                {
                    new QuizAnswer { answerText = "Mastering physical skills and techniques", region = RegionType.HealthHarbor, score = 1 },
                    new QuizAnswer { answerText = "Deepening my understanding of myself and others", region = RegionType.MindPalace, score = 1 },
                    new QuizAnswer { answerText = "Exploring new creative mediums and artistic forms", region = RegionType.CreativeCommons, score = 1 },
                    new QuizAnswer { answerText = "Collaborating with others and building relationships", region = RegionType.SocialSquare, score = 1 }
                }
            },
            new QuizQuestion
            {
                questionText = "How do you typically recharge when you're feeling drained?",
                answers = new List<QuizAnswer>
                {
                    new QuizAnswer { answerText = "Physical activity that gets my energy flowing", region = RegionType.HealthHarbor, score = 1 },
                    new QuizAnswer { answerText = "Quiet time for meditation, reading, or contemplation", region = RegionType.MindPalace, score = 1 },
                    new QuizAnswer { answerText = "Creative activities that let me express myself freely", region = RegionType.CreativeCommons, score = 1 },
                    new QuizAnswer { answerText = "Meaningful conversations and quality time with loved ones", region = RegionType.SocialSquare, score = 1 }
                }
            },
            new QuizQuestion
            {
                questionText = "What aspect of your life do you feel needs the most attention right now?",
                answers = new List<QuizAnswer>
                {
                    new QuizAnswer { answerText = "My physical health, fitness, and overall vitality", region = RegionType.HealthHarbor, score = 1 },
                    new QuizAnswer { answerText = "My mental health, emotional balance, and inner peace", region = RegionType.MindPalace, score = 1 },
                    new QuizAnswer { answerText = "My creative expression and artistic fulfillment", region = RegionType.CreativeCommons, score = 1 },
                    new QuizAnswer { answerText = "My relationships and social connections", region = RegionType.SocialSquare, score = 1 }
                }
            },
            new QuizQuestion
            {
                questionText = "When facing a challenge, what's your preferred approach?",
                answers = new List<QuizAnswer>
                {
                    new QuizAnswer { answerText = "I tackle it head-on with physical energy and determination", region = RegionType.HealthHarbor, score = 1 },
                    new QuizAnswer { answerText = "I step back to analyze and understand the situation deeply", region = RegionType.MindPalace, score = 1 },
                    new QuizAnswer { answerText = "I approach it creatively, looking for innovative solutions", region = RegionType.CreativeCommons, score = 1 },
                    new QuizAnswer { answerText = "I seek support and collaborate with others to find solutions", region = RegionType.SocialSquare, score = 1 }
                }
            },
            new QuizQuestion
            {
                questionText = "How do you define success in your personal wellness journey?",
                answers = new List<QuizAnswer>
                {
                    new QuizAnswer { answerText = "Achieving physical strength, endurance, and vitality", region = RegionType.HealthHarbor, score = 1 },
                    new QuizAnswer { answerText = "Finding inner peace, clarity, and emotional resilience", region = RegionType.MindPalace, score = 1 },
                    new QuizAnswer { answerText = "Expressing my authentic self through creative outlets", region = RegionType.CreativeCommons, score = 1 },
                    new QuizAnswer { answerText = "Building meaningful relationships and supporting others", region = RegionType.SocialSquare, score = 1 }
                }
            },
            new QuizQuestion
            {
                questionText = "What type of environment helps you feel most inspired and motivated?",
                answers = new List<QuizAnswer>
                {
                    new QuizAnswer { answerText = "Spaces that encourage movement and physical activity", region = RegionType.HealthHarbor, score = 1 },
                    new QuizAnswer { answerText = "Peaceful settings that promote reflection and growth", region = RegionType.MindPalace, score = 1 },
                    new QuizAnswer { answerText = "Creative spaces filled with art, music, and inspiration", region = RegionType.CreativeCommons, score = 1 },
                    new QuizAnswer { answerText = "Warm, community-oriented spaces where people connect", region = RegionType.SocialSquare, score = 1 }
                }
            },
            new QuizQuestion
            {
                questionText = "How do you prefer to spend your ideal day off?",
                answers = new List<QuizAnswer>
                {
                    new QuizAnswer { answerText = "Engaging in physical activities and outdoor adventures", region = RegionType.HealthHarbor, score = 1 },
                    new QuizAnswer { answerText = "Taking time for self-reflection, reading, and personal growth", region = RegionType.MindPalace, score = 1 },
                    new QuizAnswer { answerText = "Creating art, music, or exploring new creative pursuits", region = RegionType.CreativeCommons, score = 1 },
                    new QuizAnswer { answerText = "Spending quality time with friends, family, or community", region = RegionType.SocialSquare, score = 1 }
                }
            },
            new QuizQuestion
            {
                questionText = "What quality do you value most in your personal development?",
                answers = new List<QuizAnswer>
                {
                    new QuizAnswer { answerText = "Physical strength, resilience, and bodily awareness", region = RegionType.HealthHarbor, score = 1 },
                    new QuizAnswer { answerText = "Mental clarity, emotional intelligence, and self-awareness", region = RegionType.MindPalace, score = 1 },
                    new QuizAnswer { answerText = "Creative expression, imagination, and artistic vision", region = RegionType.CreativeCommons, score = 1 },
                    new QuizAnswer { answerText = "Empathy, connection, and the ability to support others", region = RegionType.SocialSquare, score = 1 }
                }
            }
        };

        // Events
        public System.Action<RegionType> OnQuizCompleted;
        public System.Action OnQuizSkipped;

        // Current quiz state
        private List<QuizQuestion> _currentQuizQuestions;
        private Dictionary<RegionType, int> _regionScores = new Dictionary<RegionType, int>();
        private int _currentQuestionIndex = 0;
        private bool _quizCompleted = false;

        /// <summary>
        /// Initialize the quiz with random questions
        /// </summary>
        public void InitializeQuiz()
        {
            _regionScores.Clear();
            _currentQuestionIndex = 0;
            _quizCompleted = false;

            // Initialize scores for all regions
            foreach (RegionType region in Enum.GetValues(typeof(RegionType)))
            {
                _regionScores[region] = 0;
            }

            // Use custom questions if available, otherwise use defaults
            var questionPool = questions.Count > 0 ? questions : defaultQuestions;
            
            // Randomly select questions
            _currentQuizQuestions = GetRandomQuestions(questionPool, questionsToShow);
        }

        /// <summary>
        /// Get the current question
        /// </summary>
        public QuizQuestion GetCurrentQuestion()
        {
            if (_currentQuizQuestions == null || _currentQuestionIndex >= _currentQuizQuestions.Count)
                return null;

            return _currentQuizQuestions[_currentQuestionIndex];
        }

        /// <summary>
        /// Submit an answer and move to next question
        /// </summary>
        public void SubmitAnswer(int answerIndex)
        {
            var currentQuestion = GetCurrentQuestion();
            if (currentQuestion == null || answerIndex < 0 || answerIndex >= currentQuestion.answers.Count)
                return;

            var selectedAnswer = currentQuestion.answers[answerIndex];
            _regionScores[selectedAnswer.region] += selectedAnswer.score;

            _currentQuestionIndex++;

            // Check if quiz is complete
            if (_currentQuestionIndex >= _currentQuizQuestions.Count)
            {
                CompleteQuiz();
            }
        }

        /// <summary>
        /// Skip the quiz
        /// </summary>
        public void SkipQuiz()
        {
            _quizCompleted = true;
            OnQuizSkipped?.Invoke();
        }

        /// <summary>
        /// Get the recommended region based on quiz scores
        /// </summary>
        public RegionType GetRecommendedRegion()
        {
            RegionType recommendedRegion = RegionType.HealthHarbor; // Default
            int highestScore = -1;

            foreach (var kvp in _regionScores)
            {
                if (kvp.Value > highestScore)
                {
                    highestScore = kvp.Value;
                    recommendedRegion = kvp.Key;
                }
            }

            return recommendedRegion;
        }

        /// <summary>
        /// Get all region scores for display
        /// </summary>
        public Dictionary<RegionType, int> GetRegionScores()
        {
            return new Dictionary<RegionType, int>(_regionScores);
        }

        /// <summary>
        /// Check if quiz is completed
        /// </summary>
        public bool IsQuizCompleted()
        {
            return _quizCompleted;
        }

        /// <summary>
        /// Get progress (0-1) through the quiz
        /// </summary>
        public float GetProgress()
        {
            if (_currentQuizQuestions == null || _currentQuizQuestions.Count == 0)
                return 0f;

            return (float)_currentQuestionIndex / _currentQuizQuestions.Count;
        }

        /// <summary>
        /// Get total number of questions in current quiz
        /// </summary>
        public int GetTotalQuestions()
        {
            return _currentQuizQuestions?.Count ?? 0;
        }

        /// <summary>
        /// Get current question index (1-based for display)
        /// </summary>
        public int GetCurrentQuestionNumber()
        {
            return _currentQuestionIndex + 1;
        }

        private void CompleteQuiz()
        {
            _quizCompleted = true;
            var recommendedRegion = GetRecommendedRegion();
            OnQuizCompleted?.Invoke(recommendedRegion);
        }

        private List<QuizQuestion> GetRandomQuestions(List<QuizQuestion> questionPool, int count)
        {
            var shuffled = new List<QuizQuestion>(questionPool);
            
            // Fisher-Yates shuffle
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                var temp = shuffled[i];
                shuffled[i] = shuffled[j];
                shuffled[j] = temp;
            }

            // Take the first 'count' questions
            return shuffled.GetRange(0, Mathf.Min(count, shuffled.Count));
        }

        /// <summary>
        /// Convert RegionType to display name
        /// </summary>
        public static string GetRegionDisplayName(RegionType region)
        {
            switch (region)
            {
                case RegionType.HealthHarbor: return "Health Harbor";
                case RegionType.MindPalace: return "Mind Palace";
                case RegionType.CreativeCommons: return "Creative Commons";
                case RegionType.SocialSquare: return "Social Square";
                default: return "Unknown";
            }
        }

        /// <summary>
        /// Convert RegionType to description
        /// </summary>
        public static string GetRegionDescription(RegionType region)
        {
            switch (region)
            {
                case RegionType.HealthHarbor: return "Focus on physical wellness, exercise, and healthy habits";
                case RegionType.MindPalace: return "Emphasize mental health, mindfulness, and personal growth";
                case RegionType.CreativeCommons: return "Explore creativity, artistic expression, and innovation";
                case RegionType.SocialSquare: return "Build relationships, community, and social connections";
                case RegionType.AllRegions: return "Health Harbor: Focus on physical wellness, exercise, and healthy habits.\nMind Palace: Emphasize mental health, mindfulness, and personal growth.\nCreative Commons: Explore creativity, artistic expression, and innovation.\nSocial Square: Build relationships, community, and social connections.";
                default: return "Unknown region";
            }
        }

        /// <summary>
        /// Get the region icon sprite
        /// </summary>
        public Sprite GetRegionIcon(RegionType region)
        {
            switch (region)
            {
                case RegionType.HealthHarbor: return healthHarborIcon;
                case RegionType.MindPalace: return mindPalaceIcon;
                case RegionType.CreativeCommons: return creativeCommonsIcon;
                case RegionType.SocialSquare: return socialSquareIcon;
                default: return null;
            }
        }
    }
} 