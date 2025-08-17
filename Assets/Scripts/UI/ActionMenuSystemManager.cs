using UnityEngine;
using LifeCraft.UI;
using LifeCraft.Core;

namespace LifeCraft.UI
{
    /// <summary>
    /// Manager script that automatically sets up the action menu system for all placed items.
    /// Add this to any GameObject in your scene to ensure the action menu works properly.
    /// </summary>
    public class ActionMenuSystemManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GameObject actionMenuPrefab;
        [SerializeField] private string actionMenuParentName = "ActionMenu_Parent";
        [SerializeField] private bool setupOnStart = true;
        [SerializeField] private bool setupExistingItems = true;
        
        private Transform actionMenuParent;
        
        private void Start()
        {
            if (setupOnStart)
            {
                SetupActionMenuSystem();
            }
        }
        
        /// <summary>
        /// Set up the entire action menu system
        /// </summary>
        [ContextMenu("Setup Action Menu System")]
        public void SetupActionMenuSystem()
        {
            Debug.Log("=== Setting up Action Menu System ===");
            
            // Find or create the action menu prefab
            if (actionMenuPrefab == null)
            {
                actionMenuPrefab = FindActionMenuPrefab();
            }
            
            if (actionMenuPrefab == null)
            {
                Debug.LogError("ActionMenuUI_Prefab not found! Please ensure it exists in Assets/Prefabs/UI/");
                return;
            }
            
            // Find or create the action menu parent
            actionMenuParent = FindOrCreateActionMenuParent();
            
            if (actionMenuParent == null)
            {
                Debug.LogError("Could not create ActionMenu_Parent! No Canvas found in scene.");
                return;
            }
            
            // Set up all existing placed items
            if (setupExistingItems)
            {
                SetupAllExistingItems();
            }
            
            Debug.Log("Action Menu System setup complete!");
        }
        
        /// <summary>
        /// Find the ActionMenuUI_Prefab
        /// </summary>
        private GameObject FindActionMenuPrefab()
        {
            // Try to load from Resources
            GameObject prefab = Resources.Load<GameObject>("ActionMenuUI_Prefab");
            if (prefab != null)
            {
                Debug.Log("Found ActionMenuUI_Prefab in Resources folder");
                return prefab;
            }
            
            // Try to load from the specific path
            prefab = Resources.Load<GameObject>("Prefabs/UI/ActionMenuUI_Prefab");
            if (prefab != null)
            {
                Debug.Log("Found ActionMenuUI_Prefab in Prefabs/UI/");
                return prefab;
            }
            
            // Try to find it in the scene
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == "ActionMenuUI_Prefab" && obj.GetComponent<ActionMenuUI>() != null)
                {
                    Debug.Log("Found ActionMenuUI_Prefab in scene");
                    return obj;
                }
            }
            
            Debug.LogWarning("ActionMenuUI_Prefab not found. Please ensure it exists in Assets/Prefabs/UI/");
            return null;
        }
        
        /// <summary>
        /// Find or create the ActionMenu_Parent
        /// </summary>
        private Transform FindOrCreateActionMenuParent()
        {
            // Find existing Canvas
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            Canvas targetCanvas = null;
            
            // Prefer the main UI canvas (usually the first one)
            if (canvases.Length > 0)
            {
                targetCanvas = canvases[0];
                Debug.Log($"Using Canvas: {targetCanvas.name}");
            }
            
            if (targetCanvas == null)
            {
                Debug.LogError("No Canvas found in scene! Action menu cannot be created.");
                return null;
            }
            
            // Look for existing ActionMenu_Parent
            Transform existingParent = targetCanvas.transform.Find(actionMenuParentName);
            if (existingParent != null)
            {
                Debug.Log($"Found existing ActionMenu_Parent: {existingParent.name}");
                return existingParent;
            }
            
            // Create new ActionMenu_Parent
            GameObject newParent = new GameObject(actionMenuParentName);
            newParent.transform.SetParent(targetCanvas.transform, false);
            
            // Set up the parent as a UI container
            RectTransform rectTransform = newParent.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            Debug.Log($"Created new ActionMenu_Parent: {newParent.name}");
            return newParent.transform;
        }
        
        /// <summary>
        /// Set up all existing placed items with action menu
        /// </summary>
        private void SetupAllExistingItems()
        {
            // Find all HoldDownInteraction components
            HoldDownInteraction[] interactions = FindObjectsByType<HoldDownInteraction>(FindObjectsSortMode.None);
            Debug.Log($"Found {interactions.Length} HoldDownInteraction components");
            
            int setupCount = 0;
            foreach (HoldDownInteraction interaction in interactions)
            {
                if (interaction.actionMenuPrefab == null)
                {
                    interaction.actionMenuPrefab = actionMenuPrefab;
                    setupCount++;
                    Debug.Log($"Set up action menu for {interaction.gameObject.name}");
                }
            }
            
            Debug.Log($"Set up action menu for {setupCount} items");
        }
        
        /// <summary>
        /// Public method to set up a specific item
        /// </summary>
        public void SetupItem(HoldDownInteraction interaction)
        {
            if (interaction == null || actionMenuPrefab == null) return;
            
            if (interaction.actionMenuPrefab == null)
            {
                interaction.actionMenuPrefab = actionMenuPrefab;
                Debug.Log($"Set up action menu for {interaction.gameObject.name}");
            }
        }
        
        /// <summary>
        /// Test the action menu system
        /// </summary>
        [ContextMenu("Test Action Menu System")]
        public void TestActionMenuSystem()
        {
            Debug.Log("=== Testing Action Menu System ===");
            
            // Check if we have the prefab
            if (actionMenuPrefab == null)
            {
                Debug.LogError("Action menu prefab is null!");
                return;
            }
            
            // Check if we have the parent
            if (actionMenuParent == null)
            {
                Debug.LogError("Action menu parent is null!");
                return;
            }
            
            // Check if we're in edit mode
            if (RegionEditManager.Instance != null)
            {
                Debug.Log($"Edit mode active: {RegionEditManager.Instance.IsEditModeActive}");
            }
            else
            {
                Debug.LogWarning("RegionEditManager.Instance is null!");
            }
            
            // Count items with action menu set up
            HoldDownInteraction[] interactions = FindObjectsByType<HoldDownInteraction>(FindObjectsSortMode.None);
            int setupCount = 0;
            int totalCount = interactions.Length;
            
            foreach (HoldDownInteraction interaction in interactions)
            {
                if (interaction.actionMenuPrefab != null)
                {
                    setupCount++;
                }
            }
            
            Debug.Log($"Action menu setup: {setupCount}/{totalCount} items");
            
            if (setupCount == 0)
            {
                Debug.LogWarning("No items have action menu set up! Run 'Setup Action Menu System' to fix this.");
            }
            else if (setupCount < totalCount)
            {
                Debug.LogWarning($"Some items ({totalCount - setupCount}) don't have action menu set up!");
            }
            else
            {
                Debug.Log("All items have action menu set up correctly!");
            }
        }
    }
} 