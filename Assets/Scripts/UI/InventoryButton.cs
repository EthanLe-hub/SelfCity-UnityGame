using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LifeCraft.UI
{
    /// <summary>
    /// Simple button component to toggle the inventory panel.
    /// Attach this to a UI Button. When clicked, it will show/hide the inventory UI.
    /// </summary>
    public class InventoryButton : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private InventoryUI inventoryUI; // Reference to the main inventory UI script
        [SerializeField] private Button button; // The button component this script is attached to
        [SerializeField] private TextMeshProUGUI buttonText; // Optional: Text label for the button

        [SerializeField] private Button closeButton; // Button to close the inventory. 
        
        //[Header("Settings")]
        //[SerializeField] private string openText = "Inventory"; // Text to show when inventory is closed
        //[SerializeField] private string closeText = "Close"; // Text to show when inventory is open
        
        // Called when the script instance is being loaded
        private void Start()
        {
            // Auto-find inventory UI if not assigned in Inspector
            if (inventoryUI == null)
                inventoryUI = FindFirstObjectByType<InventoryUI>();
            
            // Auto-find button if not assigned
            if (button == null)
                button = GetComponent<Button>();
            
            // Setup button click event
            if (button != null)
                button.onClick.AddListener(ToggleInventory);

            // Setup close button if assigned:
            if (closeButton != null)
                closeButton.onClick.AddListener(HideInventory); // Hide the inventory when close button is clicked. 
            
            // Update button text to reflect current state
                //UpdateButtonText();
        }

        // Called when the object is destroyed
        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(ToggleInventory); // Remove listener to prevent memory leaks. 

            if (closeButton != null)
                closeButton.onClick.RemoveListener(HideInventory); // Remove listener to prevent memory leaks. 
        }
        
        /// <summary>
        /// Toggle the inventory panel open/closed.
        /// </summary>
        public void ToggleInventory()
        {
            if (inventoryUI != null)
            {
                inventoryUI.ToggleInventory();
                //UpdateButtonText();
            }
            else
            {
                Debug.LogWarning("InventoryUI not found! Make sure it's assigned or in the scene.");
            }
        }
        
        /// <summary>
        /// Update the button text based on inventory state.
        /// </summary>
        /*
        private void UpdateButtonText()
        {
            if (buttonText != null && inventoryUI != null)
            {
                // Check if inventory panel is active (simple state check)
                bool isOpen = inventoryUI.gameObject.activeInHierarchy;
                buttonText.text = isOpen ? closeText : openText;
            }
        }
        */
        
        /// <summary>
        /// Show the inventory panel.
        /// </summary>
        public void ShowInventory()
        {
            if (inventoryUI != null)
            {
                inventoryUI.ShowInventory();
                //UpdateButtonText();
            }
        }
        
        /// <summary>
        /// Hide the inventory panel.
        /// </summary>
        public void HideInventory()
        {
            if (inventoryUI != null)
            {
                inventoryUI.HideInventory();
                //UpdateButtonText();
            }
        }
    }
} 