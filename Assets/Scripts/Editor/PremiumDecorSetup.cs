using UnityEngine;
using UnityEditor;
using LifeCraft.Shop;
using LifeCraft.Systems;

namespace LifeCraft.Editor
{
    /// <summary>
    /// Editor utility to quickly set up premium decor items in SubscriptionManager
    /// </summary>
    public class PremiumDecorSetup : EditorWindow
    {
        private SubscriptionManager subscriptionManager;
        private DecorationDatabase decorationDatabase;

        [MenuItem("Tools/Premium Decor Setup")]
        public static void ShowWindow()
        {
            GetWindow<PremiumDecorSetup>("Premium Decor Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Premium Decor Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Find SubscriptionManager
            subscriptionManager = FindFirstObjectByType<SubscriptionManager>();
            if (subscriptionManager == null)
            {
                EditorGUILayout.HelpBox("No SubscriptionManager found in scene! Please add one first.", MessageType.Error);
                if (GUILayout.Button("Create SubscriptionManager"))
                {
                    CreateSubscriptionManager();
                }
                return;
            }

            EditorGUILayout.LabelField("SubscriptionManager:", subscriptionManager.name);

            // Load DecorationDatabase
            decorationDatabase = Resources.Load<DecorationDatabase>("DecorationDatabase");
            if (decorationDatabase == null)
            {
                EditorGUILayout.HelpBox("DecorationDatabase not found in Resources folder!", MessageType.Error);
                return;
            }

            EditorGUILayout.LabelField("DecorationDatabase:", decorationDatabase.name);
            EditorGUILayout.LabelField("Premium Items Found:", decorationDatabase.premiumOnlyDecorations.Count.ToString());

            GUILayout.Space(10);

            if (GUILayout.Button("Load Premium Items from DecorationDatabase"))
            {
                LoadPremiumItems();
            }

            if (GUILayout.Button("Show Premium Items List"))
            {
                ShowPremiumItemsList();
            }

            if (GUILayout.Button("Test Premium Access"))
            {
                TestPremiumAccess();
            }
        }

        private void CreateSubscriptionManager()
        {
            GameObject go = new GameObject("SubscriptionManager");
            go.AddComponent<SubscriptionManager>();
            Selection.activeGameObject = go;
            Debug.Log("Created SubscriptionManager GameObject");
        }

        private void LoadPremiumItems()
        {
            if (subscriptionManager == null || decorationDatabase == null) return;

            // Use reflection to access the private field
            var field = typeof(SubscriptionManager).GetField("premiumDecorItems", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                var premiumItems = field.GetValue(subscriptionManager) as System.Collections.Generic.List<string>;
                if (premiumItems != null)
                {
                    premiumItems.Clear();
                    premiumItems.AddRange(decorationDatabase.premiumOnlyDecorations);
                    
                    EditorUtility.SetDirty(subscriptionManager);
                    Debug.Log($"Loaded {premiumItems.Count} premium decor items into SubscriptionManager");
                }
            }
            else
            {
                Debug.LogError("Could not access premiumDecorItems field in SubscriptionManager");
            }
        }

        private void ShowPremiumItemsList()
        {
            if (decorationDatabase == null) return;

            Debug.Log("=== PREMIUM DECOR ITEMS ===");
            for (int i = 0; i < decorationDatabase.premiumOnlyDecorations.Count; i++)
            {
                Debug.Log($"{i + 1}. {decorationDatabase.premiumOnlyDecorations[i]}");
            }
            Debug.Log("=== END PREMIUM DECOR ITEMS ===");
        }

        private void TestPremiumAccess()
        {
            if (subscriptionManager == null) return;

            // Simulate premium access
            var method = typeof(SubscriptionManager).GetMethod("SimulatePremiumSubscription", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (method != null)
            {
                method.Invoke(subscriptionManager, null);
                Debug.Log("Premium subscription simulated! Testing access...");
                
                var premiumItems = subscriptionManager.GetPremiumDecorItems();
                Debug.Log($"Premium user can access {premiumItems.Count} premium decor items");
            }
            else
            {
                Debug.LogError("Could not find SimulatePremiumSubscription method");
            }
        }
    }
}
