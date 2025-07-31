using UnityEngine;
using TMPro;
using System.Collections;

namespace LifeCraft.UI
{
    /// <summary>
    /// Manages EXP popup animations that show floating EXP numbers when quests are completed
    /// </summary>
    public class EXPPopupManager : MonoBehaviour
    {
        [Header("Popup Settings")]
        [SerializeField] private GameObject expPopupPrefab;
        [SerializeField] private Transform popupParent;
        [SerializeField] private float popupDuration = 2f;
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float moveDistance = 100f;
        
        [Header("Visual Settings")]
        [SerializeField] private Color easyColor = Color.green;
        [SerializeField] private Color mediumColor = Color.yellow;
        [SerializeField] private Color hardColor = Color.orange;
        [SerializeField] private Color expertColor = Color.red;
        
        private static EXPPopupManager _instance;
        public static EXPPopupManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<EXPPopupManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("EXPPopupManager");
                        _instance = go.AddComponent<EXPPopupManager>();
                    }
                }
                return _instance;
            }
        }
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Show an EXP popup at the specified position
        /// </summary>
        public void ShowEXPPopup(int expAmount, Vector3 worldPosition, QuestDifficulty difficulty = QuestDifficulty.Medium)
        {
            if (expPopupPrefab == null)
            {
                Debug.LogWarning("EXP Popup Prefab not assigned!");
                return;
            }
            
            // Convert world position to screen position
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            
            // Create popup
            GameObject popup = Instantiate(expPopupPrefab, popupParent);
            RectTransform rectTransform = popup.GetComponent<RectTransform>();
            EXPPopup expPopupComponent = popup.GetComponent<EXPPopup>();
            
            if (rectTransform != null && expPopupComponent != null)
            {
                // Set position
                rectTransform.position = screenPosition;
                
                // Set text and color using the component
                expPopupComponent.SetEXPText($"+{expAmount} EXP");
                expPopupComponent.SetColor(GetDifficultyColor(difficulty));
                
                // Start animation
                StartCoroutine(AnimatePopup(popup, rectTransform, expPopupComponent));
            }
        }
        
        /// <summary>
        /// Show an EXP popup at the center of the screen
        /// </summary>
        public void ShowEXPPopupCenter(int expAmount, QuestDifficulty difficulty = QuestDifficulty.Medium)
        {
            Vector3 centerPosition = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
            ShowEXPPopup(expAmount, centerPosition, difficulty);
        }
        
        /// <summary>
        /// Get color based on quest difficulty
        /// </summary>
        private Color GetDifficultyColor(QuestDifficulty difficulty)
        {
            return difficulty switch
            {
                QuestDifficulty.Easy => easyColor,
                QuestDifficulty.Medium => mediumColor,
                QuestDifficulty.Hard => hardColor,
                QuestDifficulty.Expert => expertColor,
                _ => mediumColor
            };
        }
        
        /// <summary>
        /// Animate the popup: fade in, move up, fade out
        /// </summary>
        private IEnumerator AnimatePopup(GameObject popup, RectTransform rectTransform, EXPPopup expPopupComponent)
        {
            Vector3 startPosition = rectTransform.position;
            Vector3 endPosition = startPosition + Vector3.up * moveDistance;
            
            // Fade in
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeInDuration;
                expPopupComponent.SetAlpha(t);
                yield return null;
            }
            
            // Move up and fade out
            elapsed = 0f;
            while (elapsed < popupDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / popupDuration;
                
                // Move up
                rectTransform.position = Vector3.Lerp(startPosition, endPosition, t);
                
                // Fade out in the last portion
                if (t > 0.7f)
                {
                    float fadeT = (t - 0.7f) / 0.3f;
                    expPopupComponent.SetAlpha(Mathf.Lerp(1f, 0f, fadeT));
                }
                
                yield return null;
            }
            
            // Destroy popup
            Destroy(popup);
        }
    }
} 