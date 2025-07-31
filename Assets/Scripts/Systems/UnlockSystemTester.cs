using UnityEngine;
using UnityEngine.InputSystem;
using LifeCraft.Core;
using LifeCraft.Systems;

namespace LifeCraft.Systems
{
    /// <summary>
    /// Test script for the unlock system - for development and debugging only
    /// </summary>
    public class UnlockSystemTester : MonoBehaviour
    {
        [Header("Test Controls")]
        [SerializeField] private bool enableTesting = false;
        [SerializeField] private Key testUnlockKey = Key.U;
        [SerializeField] private Key testResetKey = Key.R;
        [SerializeField] private Key testAssessmentKey = Key.A;

        private void Update()
        {
            if (!enableTesting) return;

            // Test region unlock
            if (Keyboard.current != null && Keyboard.current[testUnlockKey].wasPressedThisFrame)
            {
                TestRegionUnlock();
            }

            // Test reset
            if (Keyboard.current != null && Keyboard.current[testResetKey].wasPressedThisFrame)
            {
                TestReset();
            }

            // Test assessment
            if (Keyboard.current != null && Keyboard.current[testAssessmentKey].wasPressedThisFrame)
            {
                TestAssessment();
            }
        }

        private void TestRegionUnlock()
        {
            if (GameManager.Instance?.RegionUnlockSystem == null)
            {
                Debug.LogWarning("RegionUnlockSystem not found!");
                return;
            }

            var regionSystem = GameManager.Instance.RegionUnlockSystem;
            var unlockedRegions = regionSystem.GetUnlockedRegions();
            var lockedRegions = regionSystem.GetLockedRegions();

            Debug.Log($"=== Region Unlock Test ===");
            Debug.Log($"Unlocked regions: {unlockedRegions.Count}");
            foreach (var region in unlockedRegions)
            {
                var data = regionSystem.GetRegionData(region);
                Debug.Log($"  - {AssessmentQuizManager.GetRegionDisplayName(region)}: {data.currentBuildingCount}/{data.buildingsRequiredToUnlock} buildings");
            }

            Debug.Log($"Locked regions: {lockedRegions.Count}");
            foreach (var region in lockedRegions)
            {
                Debug.Log($"  - {AssessmentQuizManager.GetRegionDisplayName(region)}: Locked");
            }

            // Test adding a building to the starting region
            var startingRegion = regionSystem.GetStartingRegion();
            Debug.Log($"Adding building to {AssessmentQuizManager.GetRegionDisplayName(startingRegion)}...");
            regionSystem.AddBuildingToRegion(startingRegion);
        }

        private void TestReset()
        {
            if (GameManager.Instance?.RegionUnlockSystem == null)
            {
                Debug.LogWarning("RegionUnlockSystem not found!");
                return;
            }

            Debug.Log("Resetting region unlocks...");
            GameManager.Instance.RegionUnlockSystem.ResetRegionUnlocks();
            
            // Refresh UI
            if (GameManager.Instance?.UIManager != null)
            {
                GameManager.Instance.UIManager.RefreshBuildingPanel();
            }

            Debug.Log("Region unlocks reset!");
        }

        private void TestAssessment()
        {
            if (GameManager.Instance?.AssessmentQuizManager == null)
            {
                Debug.LogWarning("AssessmentQuizManager not found!");
                return;
            }

            Debug.Log("Testing assessment quiz...");
            var quizManager = GameManager.Instance.AssessmentQuizManager;
            quizManager.InitializeQuiz();

            var currentQuestion = quizManager.GetCurrentQuestion();
            if (currentQuestion != null)
            {
                Debug.Log($"Current question: {currentQuestion.questionText}");
                for (int i = 0; i < currentQuestion.answers.Count; i++)
                {
                    var answer = currentQuestion.answers[i];
                    Debug.Log($"  {i + 1}. {answer.answerText} -> {answer.region} (+{answer.score})");
                }
            }

            // Actually show the assessment quiz UI
            if (GameManager.Instance?.AssessmentQuizUI != null)
            {
                Debug.Log("Showing assessment quiz UI...");
                GameManager.Instance.AssessmentQuizUI.ShowQuiz();
            }
            else
            {
                Debug.LogWarning("AssessmentQuizUI not found! Cannot show quiz interface.");
            }
        }

        private void OnGUI()
        {
            if (!enableTesting) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Unlock System Tester", GUI.skin.box);
            
            if (GUILayout.Button($"Test Unlock ({testUnlockKey})"))
            {
                TestRegionUnlock();
            }
            
            if (GUILayout.Button($"Test Reset ({testResetKey})"))
            {
                TestReset();
            }
            
            if (GUILayout.Button($"Test Assessment ({testAssessmentKey})"))
            {
                TestAssessment();
            }
            
            GUILayout.EndArea();
        }
    }
} 