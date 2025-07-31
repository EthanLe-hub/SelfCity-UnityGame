using UnityEngine;
using LifeCraft.Systems;

namespace LifeCraft.UI
{
    /// <summary>
    /// Debug script to test level-up and building unlock notifications
    /// </summary>
    public class LevelUpDebugger : MonoBehaviour
    {
        [Header("Debug Controls")]
        [SerializeField] private KeyCode testLevelUpKey = KeyCode.L;
        [SerializeField] private KeyCode testBuildingUnlockKey = KeyCode.B;
        [SerializeField] private KeyCode testRegionUnlockKey = KeyCode.R;
        
        private void Update()
        {
            // Test level up
            if (Input.GetKeyDown(testLevelUpKey))
            {
                Debug.Log("=== TESTING LEVEL UP ===");
                if (PlayerLevelManager.Instance != null)
                {
                    PlayerLevelManager.Instance.AddEXP(1000); // Add lots of EXP to force level up
                    Debug.Log("Added 1000 EXP - should trigger level up");
                }
                else
                {
                    Debug.LogError("PlayerLevelManager.Instance is null!");
                }
            }
            
            // Test building unlock notification directly
            if (Input.GetKeyDown(testBuildingUnlockKey))
            {
                Debug.Log("=== TESTING BUILDING UNLOCK NOTIFICATION ===");
                if (LevelUpManager.Instance != null)
                {
                    // Test the notification system directly
                    LevelUpManager.Instance.TestBuildingUnlockNotification("Test Building");
                    Debug.Log("Triggered test building unlock notification");
                }
                else
                {
                    Debug.LogError("LevelUpManager.Instance is null!");
                }
            }
            
            // Test region unlock notification directly
            if (Input.GetKeyDown(testRegionUnlockKey))
            {
                Debug.Log("=== TESTING REGION UNLOCK NOTIFICATION ===");
                if (LevelUpManager.Instance != null)
                {
                    // Test the notification system directly
                    LevelUpManager.Instance.TestRegionUnlockNotification(AssessmentQuizManager.RegionType.MindPalace);
                    Debug.Log("Triggered test region unlock notification");
                }
                else
                {
                    Debug.LogError("LevelUpManager.Instance is null!");
                }
            }
        }
    }
} 