using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LifeCraft.Core;

namespace LifeCraft.UI
{
    /// <summary>
    /// Test controller for the inventory system.
    /// Attach this to a UI panel with test buttons to quickly add, clear, and open the inventory for debugging.
    /// </summary>
    public class InventoryTestController : MonoBehaviour
    {
        [Header("Test Controls")]
        [SerializeField] private Button addTestDecorationsButton; // Button to add test decorations
        [SerializeField] private Button clearInventoryButton; // Button to clear inventory
        [SerializeField] private Button openInventoryButton; // Button to open inventory UI
        [SerializeField] private TextMeshProUGUI statusText; // Text to show inventory status
        
        [Header("Test Data")]
        [SerializeField] private string[] testDecorationNames = {
            "Wooden Park Bench (Brown)",
            "Stone Bench (Granite)",
            "Classic Street Lamp (Black)",
            "Flower Bed (Tulips)",
            "Decorative Tree (Maple)",
            "Bioluminescent Flower Bed (Glowing Blue)",
            "Crystal Tree (Amethyst)",
            "Giant Rainbow Mushroom (Oversized)"
        };
        
        private InventoryUI inventoryUI;
        
        // Called when the script instance is being loaded
        private void Start()
        {
            // Find the inventory UI in the scene (for test/demo purposes)
            inventoryUI = FindFirstObjectByType<InventoryUI>();
            
            // Setup test button listeners
            if (addTestDecorationsButton != null)
                addTestDecorationsButton.onClick.AddListener(AddTestDecorations);
            
            if (clearInventoryButton != null)
                clearInventoryButton.onClick.AddListener(ClearInventory);
            
            if (openInventoryButton != null)
                openInventoryButton.onClick.AddListener(OpenInventory);
            
            // Update status text
            UpdateStatus();
        }
        
        // Called when the object is destroyed
        private void OnDestroy()
        {
            if (addTestDecorationsButton != null)
                addTestDecorationsButton.onClick.RemoveListener(AddTestDecorations);
            
            if (clearInventoryButton != null)
                clearInventoryButton.onClick.RemoveListener(ClearInventory);
            
            if (openInventoryButton != null)
                openInventoryButton.onClick.RemoveListener(OpenInventory);
        }
        
        /// <summary>
        /// Add test decorations to the inventory for debugging/demo.
        /// </summary>
        public void AddTestDecorations()
        {
            if (InventoryManager.Instance == null)
            {
                Debug.LogError("InventoryManager not found!");
                return;
            }
            
            int addedCount = 0;
            foreach (string decorationName in testDecorationNames)
            {
                // Mark some as premium for testing
                bool isPremium = decorationName.Contains("Bioluminescent") || 
                                decorationName.Contains("Crystal") || 
                                decorationName.Contains("Rainbow");
                string source = isPremium ? "PremiumDecorChest" : "DecorChest";
                bool success = InventoryManager.Instance.AddDecorationByName(decorationName, source, isPremium);
                if (success)
                    addedCount++;
            }
            Debug.Log($"Added {addedCount} test decorations to inventory");
            UpdateStatus();
        }
        
        /// <summary>
        /// Clear the inventory for debugging/demo.
        /// </summary>
        public void ClearInventory()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.ClearInventory();
                Debug.Log("Inventory cleared");
                UpdateStatus();
            }
        }
        
        /// <summary>
        /// Open the inventory panel for debugging/demo.
        /// </summary>
        public void OpenInventory()
        {
            if (inventoryUI != null)
            {
                inventoryUI.ShowInventory();
            }
            else
            {
                Debug.LogWarning("InventoryUI not found!");
            }
        }
        
        /// <summary>
        /// Update the status text to show inventory count.
        /// </summary>
        private void UpdateStatus()
        {
            if (statusText != null && InventoryManager.Instance != null)
            {
                int itemCount = InventoryManager.Instance.ItemCount;
                int maxSize = InventoryManager.Instance.MaxSize;
                statusText.text = $"Inventory: {itemCount}/{maxSize} items";
            }
        }
        
        /// <summary>
        /// Simulate winning a decoration from a chest (for test/demo).
        /// </summary>
        public void SimulateChestWin()
        {
            if (InventoryManager.Instance == null) return;
            // Randomly select a test decoration
            string randomDecoration = testDecorationNames[Random.Range(0, testDecorationNames.Length)];
            bool isPremium = randomDecoration.Contains("Bioluminescent") || 
                            randomDecoration.Contains("Crystal") || 
                            randomDecoration.Contains("Rainbow");
            string source = isPremium ? "PremiumDecorChest" : "DecorChest";
            bool success = InventoryManager.Instance.AddDecorationByName(randomDecoration, source, isPremium);
            if (success)
            {
                Debug.Log($"Won {randomDecoration} from {source}!");
                UpdateStatus();
            }
            else
            {
                Debug.LogWarning("Failed to add decoration - inventory might be full!");
            }
        }
        
        /// <summary>
        /// Test the drag and drop functionality (for test/demo).
        /// </summary>
        public void TestDragAndDrop()
        {
            Debug.Log("Drag and drop test: Try dragging items from the inventory to see if they respond to drag events.");
        }
        
        #region Context Menu Methods
        [ContextMenu("Add Test Decorations")]
        private void ContextAddTestDecorations() { AddTestDecorations(); }
        [ContextMenu("Clear Inventory")]
        private void ContextClearInventory() { ClearInventory(); }
        [ContextMenu("Simulate Chest Win")]
        private void ContextSimulateChestWin() { SimulateChestWin(); }
        #endregion
    }
} 