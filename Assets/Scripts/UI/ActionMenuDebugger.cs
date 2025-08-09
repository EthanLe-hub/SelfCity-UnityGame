using UnityEngine;
using UnityEngine.UI;
using LifeCraft.UI;
using LifeCraft.Core;

namespace LifeCraft.UI
{
    /// <summary>
    /// Debug script to test and troubleshoot the action menu system
    /// </summary>
    public class ActionMenuDebugger : MonoBehaviour
    {
        [Header("Debug Controls")]
        [SerializeField] private bool enableDebugLogging = true;
        
        private HoldDownInteraction holdDownInteraction;
        
        private void Start()
        {
            holdDownInteraction = GetComponent<HoldDownInteraction>();
            
            if (enableDebugLogging)
            {
                Debug.Log($"ActionMenuDebugger started on {gameObject.name}");
                
                if (holdDownInteraction != null)
                {
                    Debug.Log($"HoldDownInteraction found on {gameObject.name}");
                    Debug.Log($"Action menu prefab assigned: {holdDownInteraction.actionMenuPrefab != null}");
                    Debug.Log($"Action menu parent name: {holdDownInteraction.actionMenuParentName}");
                }
                else
                {
                    Debug.LogWarning($"No HoldDownInteraction found on {gameObject.name}");
                }
            }
        }
        
        /// <summary>
        /// Manually trigger the action menu (for testing)
        /// </summary>
        [ContextMenu("Test Show Action Menu")]
        public void TestShowActionMenu()
        {
            if (holdDownInteraction != null)
            {
                Debug.Log($"Manually triggering action menu for {gameObject.name}");
                holdDownInteraction.ShowActionMenu();
            }
            else
            {
                Debug.LogError($"No HoldDownInteraction found on {gameObject.name}");
            }
        }
        
        /// <summary>
        /// Check the action menu setup
        /// </summary>
        [ContextMenu("Check Action Menu Setup")]
        public void CheckActionMenuSetup()
        {
            Debug.Log($"=== Action Menu Setup Check for {gameObject.name} ===");
            
            if (holdDownInteraction == null)
            {
                Debug.LogError("No HoldDownInteraction component found!");
                return;
            }
            
            Debug.Log($"Action menu prefab: {(holdDownInteraction.actionMenuPrefab != null ? holdDownInteraction.actionMenuPrefab.name : "NULL")}");
            Debug.Log($"Action menu parent name: {holdDownInteraction.actionMenuParentName}");
            
            // Check if we're in edit mode
            if (RegionEditManager.Instance != null)
            {
                Debug.Log($"Edit mode active: {RegionEditManager.Instance.IsEditModeActive}");
            }
            else
            {
                Debug.LogWarning("RegionEditManager.Instance is null!");
            }
            
            // Check if PlacedItemUI exists
            var placedItemUI = GetComponent<PlacedItemUI>();
            if (placedItemUI != null)
            {
                Debug.Log("PlacedItemUI component found");
            }
            else
            {
                Debug.LogWarning("No PlacedItemUI component found!");
            }
            
            // Check if we have the required components for pointer events
            var graphic = GetComponent<Graphic>();
            if (graphic != null)
            {
                Debug.Log($"Graphic component found: {graphic.GetType().Name}");
            }
            else
            {
                Debug.LogWarning("No Graphic component found - pointer events may not work!");
            }
        }
        
        /// <summary>
        /// Simulate a click on this object
        /// </summary>
        [ContextMenu("Simulate Click")]
        public void SimulateClick()
        {
            var placedItemUI = GetComponent<PlacedItemUI>();
            if (placedItemUI != null)
            {
                Debug.Log($"Simulating click on {gameObject.name}");
                // Create a fake pointer event data
                var eventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
                placedItemUI.OnPointerClick(eventData);
            }
            else
            {
                Debug.LogError($"No PlacedItemUI found on {gameObject.name}");
            }
        }
        
        private void OnMouseDown()
        {
            if (enableDebugLogging)
            {
                Debug.Log($"Mouse down detected on {gameObject.name}");
            }
        }
        
        private void OnMouseUp()
        {
            if (enableDebugLogging)
            {
                Debug.Log($"Mouse up detected on {gameObject.name}");
            }
        }
    }
} 