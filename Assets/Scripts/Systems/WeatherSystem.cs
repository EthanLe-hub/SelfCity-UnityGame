using UnityEngine; // Base class for all Unity scripts. 
using LifeCraft.Systems; // Import the MoodManager class from the Systems namespace.
using System.Collections; // Needed for coroutines

namespace LifeCraft.Systems
{
    public class WeatherSystem : MonoBehaviour
    {
        [Header("Weather States")]
        [SerializeField] private GameObject sunnyWeather; // Weather effect for Happy mood
        [SerializeField] private GameObject rainyWeather; // Weather effect for Sad mood
        [SerializeField] private GameObject cloudyWeather; // Weather effect for Moody mood
        [SerializeField] private GameObject stormyWeather; // Weather effect for Stressed mood

        [Header("Weather Settings")]
        [SerializeField] private float transitionDuration = 2f; // How long weather transitions take

        // Current weather state
        private MoodManager.Mood currentWeatherMood = MoodManager.Mood.None;
        
        // Track if a transition is currently happening
        private bool isTransitioning = false;
        
        // Reference to current transition coroutine
        private Coroutine currentTransition;

        // Called when script instance is being loaded
        private void Start()
        {
            // Subscribe to mood change events
            MoodManager.OnMoodChanged += OnMoodChanged;
            
            // Initialize weather to match current mood
            UpdateWeatherToMood(MoodManager.Instance.GetCurrentMood());
        }

        // Called when the script is destroyed
        private void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            MoodManager.OnMoodChanged -= OnMoodChanged;
        }

        // Event handler for mood changes
        private void OnMoodChanged(MoodManager.Mood newMood)
        {
            Debug.Log($"WeatherSystem: Mood changed to {newMood}, updating weather...");
            UpdateWeatherToMood(newMood);
        }

        // Update weather effects based on mood with smooth transitions
        private void UpdateWeatherToMood(MoodManager.Mood mood)
        {
            // Don't update if weather is already correct
            if (currentWeatherMood == mood)
                return;

            // Check if we're already transitioning
            if (isTransitioning)
            {
                Debug.Log($"WeatherSystem: Transition already in progress, interrupting to switch to {mood} weather");
            }

            // Stop any current transition
            if (currentTransition != null)
            {
                StopCoroutine(currentTransition);
            }

            // Start new transition
            currentTransition = StartCoroutine(TransitionToWeather(mood));
        }

        // Coroutine for smooth weather transitions
        private IEnumerator TransitionToWeather(MoodManager.Mood newMood)
        {
            isTransitioning = true;
            Debug.Log($"WeatherSystem: Starting transition to {newMood} weather (Duration: {transitionDuration}s)");

            // Get the target weather GameObject
            GameObject targetWeather = GetWeatherGameObject(newMood);
            
            // If no target weather, just update the state
            if (targetWeather == null)
            {
                Debug.LogWarning($"WeatherSystem: No weather GameObject found for mood {newMood}");
                currentWeatherMood = newMood;
                isTransitioning = false;
                yield break;
            }

            // Fade out current weather effects
            yield return StartCoroutine(FadeOutCurrentWeather());

            // Fade in new weather effects
            yield return StartCoroutine(FadeInWeather(targetWeather));

            // Update current weather mood
            currentWeatherMood = newMood;
            isTransitioning = false;
            
            Debug.Log($"WeatherSystem: Transition to {newMood} weather completed successfully");
        }

        // Coroutine to fade out current weather
        private IEnumerator FadeOutCurrentWeather()
        {
            GameObject currentWeather = GetWeatherGameObject(currentWeatherMood);
            
            if (currentWeather != null && currentWeather.activeSelf)
            {
                // Get all renderers in the current weather
                Renderer[] renderers = currentWeather.GetComponentsInChildren<Renderer>();
                
                // Store original colors
                Color[] originalColors = new Color[renderers.Length];
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i].material != null)
                    {
                        originalColors[i] = renderers[i].material.color;
                    }
                }

                // Fade out over half the transition duration
                float fadeTime = 0f;
                float fadeDuration = transitionDuration * 0.5f;
                
                while (fadeTime < fadeDuration)
                {
                    fadeTime += Time.deltaTime;
                    float alpha = Mathf.Lerp(1f, 0f, fadeTime / fadeDuration);
                    
                    for (int i = 0; i < renderers.Length; i++)
                    {
                        if (renderers[i].material != null)
                        {
                            Color newColor = originalColors[i];
                            newColor.a = alpha;
                            renderers[i].material.color = newColor;
                        }
                    }
                    
                    yield return null;
                }

                // Disable the weather effect
                currentWeather.SetActive(false);
                
                // Reset colors
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i].material != null)
                    {
                        renderers[i].material.color = originalColors[i];
                    }
                }
            }
        }

        // Coroutine to fade in new weather
        private IEnumerator FadeInWeather(GameObject weatherObject)
        {
            // Enable the weather effect
            weatherObject.SetActive(true);
            
            // Get all renderers in the weather
            Renderer[] renderers = weatherObject.GetComponentsInChildren<Renderer>();
            
            // Store original colors
            Color[] originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i].material != null)
                {
                    originalColors[i] = renderers[i].material.color;
                    // Start with transparent
                    Color transparentColor = originalColors[i];
                    transparentColor.a = 0f;
                    renderers[i].material.color = transparentColor;
                }
            }

            // Fade in over half the transition duration
            float fadeTime = 0f;
            float fadeDuration = transitionDuration * 0.5f;
            
            while (fadeTime < fadeDuration)
            {
                fadeTime += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, fadeTime / fadeDuration);
                
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i].material != null)
                    {
                        Color newColor = originalColors[i];
                        newColor.a = alpha;
                        renderers[i].material.color = newColor;
                    }
                }
                
                yield return null;
            }

            // Ensure final colors are correct
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i].material != null)
                {
                    renderers[i].material.color = originalColors[i];
                }
            }
        }

        // Helper method to get weather GameObject based on mood
        private GameObject GetWeatherGameObject(MoodManager.Mood mood)
        {
            switch (mood)
            {
                case MoodManager.Mood.Happy:
                    return sunnyWeather;
                case MoodManager.Mood.Sad:
                    return rainyWeather;
                case MoodManager.Mood.Moody:
                    return cloudyWeather;
                case MoodManager.Mood.Stressed:
                    return stormyWeather;
                default:
                    return null;
            }
        }

        // Disable all weather effects (for initialization)
        private void DisableAllWeatherEffects()
        {
            if (sunnyWeather != null) sunnyWeather.SetActive(false);
            if (rainyWeather != null) rainyWeather.SetActive(false);
            if (cloudyWeather != null) cloudyWeather.SetActive(false);
            if (stormyWeather != null) stormyWeather.SetActive(false);
        }

        // Method to manually test weather changes (for debugging)
        [ContextMenu("Test Happy Weather")]
        private void TestHappyWeather()
        {
            UpdateWeatherToMood(MoodManager.Mood.Happy);
        }

        [ContextMenu("Test Sad Weather")]
        private void TestSadWeather()
        {
            UpdateWeatherToMood(MoodManager.Mood.Sad);
        }

        [ContextMenu("Test Moody Weather")]
        private void TestMoodyWeather()
        {
            UpdateWeatherToMood(MoodManager.Mood.Moody);
        }

        [ContextMenu("Test Stressed Weather")]
        private void TestStressedWeather()
        {
            UpdateWeatherToMood(MoodManager.Mood.Stressed);
        }

        // Public methods for external systems to check weather state
        public bool IsTransitioning()
        {
            return isTransitioning;
        }

        public MoodManager.Mood GetCurrentWeatherMood()
        {
            return currentWeatherMood;
        }

        public float GetTransitionDuration()
        {
            return transitionDuration;
        }

        // Method to get weather system status for debugging
        [ContextMenu("Print Weather System Status")]
        private void PrintWeatherSystemStatus()
        {
            Debug.Log($"WeatherSystem Status:");
            Debug.Log($"  Current Mood: {currentWeatherMood}");
            Debug.Log($"  Is Transitioning: {isTransitioning}");
            Debug.Log($"  Transition Duration: {transitionDuration}s");
            Debug.Log($"  Sunny Weather: {(sunnyWeather != null ? "Assigned" : "Not Assigned")}");
            Debug.Log($"  Rainy Weather: {(rainyWeather != null ? "Assigned" : "Not Assigned")}");
            Debug.Log($"  Cloudy Weather: {(cloudyWeather != null ? "Assigned" : "Not Assigned")}");
            Debug.Log($"  Stormy Weather: {(stormyWeather != null ? "Assigned" : "Not Assigned")}");
        }

        // Save weather system data to PlayerPrefs
        public void SaveWeatherData()
        {
            try
            {
                // Save current weather mood
                PlayerPrefs.SetInt("CurrentWeatherMood", (int)currentWeatherMood);
                
                // Save transition duration (in case it was changed in inspector)
                PlayerPrefs.SetFloat("WeatherTransitionDuration", transitionDuration);
                
                PlayerPrefs.Save();
                Debug.Log($"Weather data saved: {currentWeatherMood} with transition duration {transitionDuration}s");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save weather data: {e.Message}");
            }
        }

        // Load weather system data from PlayerPrefs
        public void LoadWeatherData()
        {
            try
            {
                // Load current weather mood
                if (PlayerPrefs.HasKey("CurrentWeatherMood"))
                {
                    currentWeatherMood = (MoodManager.Mood)PlayerPrefs.GetInt("CurrentWeatherMood");
                    Debug.Log($"Weather data loaded: {currentWeatherMood}");
                }
                else
                {
                    Debug.Log("No saved weather data found, using default: None");
                    currentWeatherMood = MoodManager.Mood.None;
                }

                // Load transition duration (optional - can keep inspector value)
                if (PlayerPrefs.HasKey("WeatherTransitionDuration"))
                {
                    transitionDuration = PlayerPrefs.GetFloat("WeatherTransitionDuration");
                    Debug.Log($"Weather transition duration loaded: {transitionDuration}s");
                }
                else
                {
                    Debug.Log("No saved transition duration found, using inspector value");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load weather data: {e.Message}");
                // Set defaults on error
                currentWeatherMood = MoodManager.Mood.None;
            }
        }

        // Clear weather save data (for debugging or resetting)
        public void ClearWeatherData()
        {
            PlayerPrefs.DeleteKey("CurrentWeatherMood");
            PlayerPrefs.DeleteKey("WeatherTransitionDuration");
            PlayerPrefs.Save();
            Debug.Log("Weather data cleared");
        }

        // Apply loaded weather state to visual effects
        public void ApplyLoadedWeatherState()
        {
            if (currentWeatherMood != MoodManager.Mood.None)
            {
                Debug.Log($"Applying loaded weather state: {currentWeatherMood}");
                
                // Disable all weather effects first
                DisableAllWeatherEffects();
                
                // Enable the appropriate weather effect based on loaded mood
                GameObject targetWeather = GetWeatherGameObject(currentWeatherMood);
                if (targetWeather != null)
                {
                    targetWeather.SetActive(true);
                    Debug.Log($"Applied weather effect for {currentWeatherMood}");
                }
                else
                {
                    Debug.LogWarning($"No weather GameObject found for loaded mood: {currentWeatherMood}");
                }
            }
            else
            {
                Debug.Log("No weather state to apply (mood is None)");
            }
        }
    }
} 