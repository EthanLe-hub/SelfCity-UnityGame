using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LifeCraft.UI
{
    /// <summary>
    /// UI component for the action menu that appears when holding down on placed items.
    /// Contains 5 buttons: Store, Sell, Confirm, Rotate, Reset.
    /// </summary>
    public class ActionMenuUI : MonoBehaviour
    {
        [Header("Action Buttons")]
        public Button storeButton; // Suitcase icon - Store to inventory
        public Button sellButton; // Coin icon - Sell for 50% cost
        public Button rotateButton; // Arrows icon - Rotate 90 degrees
        public Button closeButton; // Close button to hide the menu
        
        [Header("Button Icons")]
        public Image storeIcon;
        public Image sellIcon;
        public Image confirmIcon;
        public Image rotateIcon;
        public Image resetIcon;
        
        [Header("Button Text")]
        public TextMeshProUGUI storeText;
        public TextMeshProUGUI sellText;
        //public TextMeshProUGUI confirmText; 
        public TextMeshProUGUI rotateText;
        public TextMeshProUGUI closeText;
        
        private HoldDownInteraction holdDownInteraction;
        
        /// <summary>
        /// Initialize the action menu with the hold down interaction
        /// </summary>
        public void Initialize(HoldDownInteraction interaction)
        {
            holdDownInteraction = interaction;
            
            // Set up button listeners
            SetupButtonListeners();
            
            // Update button text and icons
            UpdateButtonDisplay();
            
            // Position the menu near the item
            PositionMenu();
        }
        
        /// <summary>
        /// Set up button click listeners
        /// </summary>
        private void SetupButtonListeners()
        {
            if (storeButton != null)
                storeButton.onClick.AddListener(OnStoreClicked);
                
            if (sellButton != null)
                sellButton.onClick.AddListener(OnSellClicked);
                
            if (rotateButton != null)
                rotateButton.onClick.AddListener(OnRotateClicked);
                
            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClicked);
        }
        
        /// <summary>
        /// Update button text and icons based on item type
        /// </summary>
        private void UpdateButtonDisplay()
        {
            if (holdDownInteraction == null)
                return;
                
            string itemName = holdDownInteraction.GetItemName();
            bool isBuilding = holdDownInteraction.IsBuilding();
            
            // Update button text
            if (storeText != null)
                storeText.text = "Store";
                
            if (sellText != null)
                sellText.text = "Sell";
                
            if (rotateText != null)
                rotateText.text = "Rotate";
                
            if (closeText != null)
                closeText.text = "Close";
                
            // Update sell button text to show currency type
            if (sellText != null)
            {
                if (isBuilding)
                {
                    sellText.text = "Sell";
                }
                else
                {
                    sellText.text = "Sell (Random)";
                }
            }
        }
        
        /// <summary>
        /// Position the menu near the item
        /// </summary>
        private void PositionMenu()
        {
            if (holdDownInteraction == null)
                return;
                
            // Get the clicked item's RectTransform for proper UI positioning
            var itemRectTransform = holdDownInteraction.GetComponent<RectTransform>();
            if (itemRectTransform != null)
            {
                // Position the menu relative to the item in UI space
                Vector3 itemPosition = itemRectTransform.position;
                RectTransform rectTransform = GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.position = itemPosition + Vector3.up * 150f; // Offset upward
                    Debug.Log($"ActionMenuUI positioned at UI position {itemPosition} + offset");
                }
            }
            else
            {
                // Fallback: try to use world position if no RectTransform
                Vector3 worldPos = holdDownInteraction.transform.position;
                Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
                
                RectTransform rectTransform = GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.position = screenPos + Vector3.up * 100f; // Offset upward
                    Debug.Log($"ActionMenuUI positioned at screen position {screenPos} (fallback)");
                }
            }
        }
        
        /// <summary>
        /// Handle store button click
        /// </summary>
        private void OnStoreClicked()
        {
            if (holdDownInteraction != null)
            {
                holdDownInteraction.StoreToInventory();
            }
            
            // Close the menu
            CloseMenu();
        }
        
        /// <summary>
        /// Handle sell button click
        /// </summary>
        private void OnSellClicked()
        {
            if (holdDownInteraction != null)
            {
                holdDownInteraction.SellItem();
            }
            
            // Close the menu
            CloseMenu();
        }
        
        /// <summary>
        /// Handle rotate button click
        /// </summary>
        private void OnRotateClicked()
        {
            if (holdDownInteraction != null)
            {
                holdDownInteraction.RotateItem();
            }
            
            // Keep menu open for additional rotations
        }
        
        /// <summary>
        /// Handle close button click
        /// </summary>
        private void OnCloseClicked()
        {
            if (holdDownInteraction != null)
            {
                holdDownInteraction.HideActionMenu();
            }
        }
        
        /// <summary>
        /// Close the action menu
        /// </summary>
        public void CloseMenu()
        {
            if (holdDownInteraction != null)
            {
                holdDownInteraction.HideActionMenu();
            }
        }
        
        /// <summary>
        /// Handle click outside menu to close it
        /// </summary>
        public void OnPointerClickOutside()
        {
            CloseMenu();
        }
        
        private void OnDestroy()
        {
            // Clean up button listeners
            if (storeButton != null)
                storeButton.onClick.RemoveListener(OnStoreClicked);
                
            if (sellButton != null)
                sellButton.onClick.RemoveListener(OnSellClicked);
                
            if (rotateButton != null)
                rotateButton.onClick.RemoveListener(OnRotateClicked);
                
            if (closeButton != null)
                closeButton.onClick.RemoveListener(OnCloseClicked);
        }
    }
} 