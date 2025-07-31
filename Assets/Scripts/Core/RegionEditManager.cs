using UnityEngine; // We need UnityEngine for MonoBehaviour and other Unity features. 
using UnityEngine.Events; // We need UnityEvents for event handling. 

namespace LifeCraft.Core
{
    public class RegionEditManager : MonoBehaviour
    {
        public static RegionEditManager Instance { get; private set; } // Singleton instance for easy access for other scripts. 
        public bool IsEditModeActive { get; private set; } // Flag to check if edit mode is active. 
        public GameObject gridOverlay; // Reference to the grid overlay GameObject. 
        public GameObject inventoryPanel; // Reference to the inventory panel GameObject. 

        public GameObject inventoryButton; // Reference to the inventory button GameObject to open the Inventory UI when in edit mode. 

        // Add an event for UI updates when edit mode changes:
        public UnityEvent<bool> OnEditModeChanged; 

        private void Awake() // Singleton pattern implementation. 
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject); // Ensure only one instance exists. 
                return;
            }

            Instance = this; // Set the singleton instance. 
        }

        public void ToggleEditMode()
        {
            if (IsEditModeActive)
            {
                ExitEditMode(); // If edit mode is active, exit it when toggled. 
            }

            else
            {
                EnterEditMode(); // If edit mode is NOT active, enter it when toggled. 
            }
        }

        public void EnterEditMode()
        {
            gridOverlay.SetActive(true); // Show the grid overlay when entering edit mode. 
            inventoryButton.SetActive(true); // Show the inventory button when entering edit mode. 
            IsEditModeActive = true; // Set the edit mode flag to true. 
            OnEditModeChanged?.Invoke(true); // Notify listeners that edit mode has changed. 
        }

        public void ExitEditMode()
        {
            gridOverlay.SetActive(false); // Hide the grid overlay when exiting edit mode. 
            inventoryButton.SetActive(false); // Hide the inventory button when exiting edit mode. 
            IsEditModeActive = false; // Set the edit mode flag to false. 
            OnEditModeChanged?.Invoke(false); // Notify listeners that edit mode has changed. 
        }
    }
}