using UnityEngine;
using LifeCraft.UI;

namespace LifeCraft.UI
{
    /// <summary>
    /// Automatically sets up the action menu system by finding and assigning
    /// the ActionMenuUI_Prefab to all HoldDownInteraction components and
    /// ensuring the ActionMenu_Parent exists in the scene.
    /// </summary>
    public class ActionMenuSetup : MonoBehaviour
    {
        [Header("Action Menu Configuration")]
        [SerializeField] private GameObject actionMenuPrefab;
        [SerializeField] private string actionMenuParentName = "ActionMenu_Parent";
        
        private void Start()
        {
            SetupActionMenuSystem();
        }
        
        /// <summary>
        /// Set up the action menu system for all placed items
        /// </summary>
        public void SetupActionMenuSystem()
        {
            Debug.Log("Setting up Action Menu system...");
            
            // Find the action menu prefab if not assigned
            if (actionMenuPrefab == null)
            {
                actionMenuPrefab = FindActionMenuPrefab();
            }
            
            if (actionMenuPrefab == null)
            {
                Debug.LogError("ActionMenuUI_Prefab not found! Please ensure it exists in Assets/Prefabs/UI/");
                return;
            }
            
            // Ensure the action menu parent exists
            Transform actionMenuParent = FindOrCreateActionMenuParent();
            
            // Set up all HoldDownInteraction components
            SetupAllHoldDownInteractions();
            
            Debug.Log("Action Menu system setup complete!");
        }
        
        /// <summary>
        /// Find the ActionMenuUI_Prefab in the Resources or Prefabs folder
        /// </summary>
        private GameObject FindActionMenuPrefab()
        {
            // Try to load from Resources first
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
        /// Find or create the ActionMenu_Parent in the Canvas hierarchy
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
        /// Set up all HoldDownInteraction components with the action menu prefab
        /// </summary>
        private void SetupAllHoldDownInteractions()
        {
            HoldDownInteraction[] interactions = FindObjectsByType<HoldDownInteraction>(FindObjectsSortMode.None);
            Debug.Log($"Found {interactions.Length} HoldDownInteraction components");
            
            foreach (HoldDownInteraction interaction in interactions)
            {
                SetupHoldDownInteraction(interaction);
            }
        }
        
        /// <summary>
        /// Set up a single HoldDownInteraction component
        /// </summary>
        private void SetupHoldDownInteraction(HoldDownInteraction interaction)
        {
            if (interaction == null) return;
            
            // Check if action menu prefab is already assigned
            if (interaction.actionMenuPrefab == null)
            {
                interaction.actionMenuPrefab = actionMenuPrefab;
                Debug.Log($"Assigned ActionMenuUI_Prefab to {interaction.gameObject.name}");
            }
        }
        
        /// <summary>
        /// Public method to manually set up a specific HoldDownInteraction
        /// </summary>
        public void SetupHoldDownInteraction(HoldDownInteraction interaction, GameObject prefab)
        {
            if (interaction == null || prefab == null) return;
            
            interaction.actionMenuPrefab = prefab;
            Debug.Log($"Manually assigned ActionMenuUI_Prefab to {interaction.gameObject.name}");
        }
        
        /// <summary>
        /// Context menu method for manual setup
        /// </summary>
        [ContextMenu("Setup Action Menu System")]
        private void SetupActionMenuSystemContext()
        {
            SetupActionMenuSystem();
        }
    }
} 