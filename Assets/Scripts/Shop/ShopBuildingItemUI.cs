using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LifeCraft.Shop
{
    /// <summary>
    /// Handles the UI for a single building in the shop.
    /// This component manages both unlocked and locked building states, providing
    /// visual feedback to players about their progression and available content.
    /// </summary>
    public class ShopBuildingItemUI : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI priceText;
        public Image iconImage;
        public Button buyButton;

        public Image resourceIconImage; // Image for the resource icon (e.g., Energy Crystals) -- add field to Inspector and assign the appropriate sprite. 

        [Header("Lock State UI")]
        public Image lockOverlay; // Gray overlay for locked buildings - provides visual indication that content is not yet available
        public TextMeshProUGUI unlockLevelText; // Text showing required level for locked buildings - informs players of unlock requirements

        private BuildingShopItem buildingData;
        private System.Action<BuildingShopItem> onBuyClicked;
        private bool isLocked = false;

        /// <summary> 
        /// Setup the UI with building data and buy callback. 
        /// This method initializes the shop item with appropriate visual state based on unlock status.
        /// </summary>
        /// <param name="data">Building data containing name, price, and other properties</param>
        /// <param name="buyCallback">Callback function to execute when buy button is clicked</param>
        /// <param name="resourceIcon">Sprite for the currency icon (e.g., Wisdom Orbs, Energy Crystals)</param>
        /// <param name="locked">Whether this building is currently locked/unavailable</param>
        /// <param name="unlockLevel">The player level required to unlock this building</param>
        public void Setup(BuildingShopItem data, System.Action<BuildingShopItem> buyCallback, Sprite resourceIcon, bool locked = false, int unlockLevel = 0)
        {
            buildingData = data; // Store the building data for later use. 
            onBuyClicked = buyCallback; // Store the callback for when the buy button is clicked. 
            isLocked = locked;

            if (nameText != null)
            {
                nameText.text = data.name; // Set the name text to the building's name, if it exists. 
            }

            if (priceText != null)
            {
                // UX IMPROVEMENT: Show different text based on lock state
                // Locked buildings display unlock requirement instead of price, helping players understand progression
                if (locked)
                {
                    priceText.text = $"Unlocks at Level {unlockLevel}";
                }
                else
                {
                    priceText.text = data.price.ToString(); // Set the price text to the building's price, if it exists. 
                }
            }

            if (iconImage != null)
            {
                // Get sprite from CityBuilder instead of BuildingShopItem
                Sprite buildingSprite = null;
                if (LifeCraft.Core.CityBuilder.Instance != null)
                {
                    var buildingData = LifeCraft.Core.CityBuilder.Instance.GetBuildingTypeData(data.name);
                    if (buildingData != null && buildingData.buildingSprite != null)
                    {
                        buildingSprite = buildingData.buildingSprite;
                    }
                }
                
                iconImage.sprite = buildingSprite; // Set the icon image to the building's sprite from CityBuilder
                iconImage.gameObject.SetActive(buildingSprite != null); // Ensure the icon is only active if it exists
                
                // VISUAL FEEDBACK: Apply gray tint to locked buildings
                // This provides immediate visual distinction between available and locked content
                if (locked)
                {
                    iconImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                }
                else
                {
                    iconImage.color = Color.white;
                }
            }

            if (buyButton != null)
            {
                buyButton.onClick.RemoveAllListeners(); // Clear any previous listeners to avoid duplicates. 
                
                // INTERACTION LOGIC: Enable/disable button based on lock state
                // Locked buildings cannot be purchased, so button is disabled to prevent invalid interactions
                if (locked)
                {
                    // Disable button for locked buildings
                    buyButton.interactable = false;
                }
                else
                {
                    // Enable button for unlocked buildings
                    buyButton.interactable = true;
                    buyButton.onClick.AddListener(() => onBuyClicked?.Invoke(buildingData)); // Add a listener that invokes the buy callback with the building data when clicked (uses lambda expression for simplicity). 
                }
            }

            if (resourceIconImage != null)
            {
                resourceIconImage.sprite = resourceIcon; // Set the resource icon image to the appropriate sprite, if it exists. 
                resourceIconImage.gameObject.SetActive(!locked); // Hide resource icon for locked buildings - no currency needed for unavailable content
            }

            // Handle lock overlay - provides visual barrier for locked content
            if (lockOverlay != null)
            {
                lockOverlay.gameObject.SetActive(locked);
            }

            // Handle unlock level text - shows progression requirement
            if (unlockLevelText != null)
            {
                if (locked)
                {
                    unlockLevelText.text = $"Level {unlockLevel}";
                    unlockLevelText.gameObject.SetActive(true);
                }
                else
                {
                    unlockLevelText.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Update the lock state of this building item dynamically.
        /// This method allows for smooth transitions when buildings unlock during gameplay,
        /// providing immediate visual feedback without requiring shop repopulation.
        /// </summary>
        /// <param name="locked">New lock state for this building</param>
        /// <param name="unlockLevel">The player level required to unlock this building (used for locked state)</param>
        public void UpdateLockState(bool locked, int unlockLevel = 0)
        {
            if (isLocked == locked) return; // No change needed - optimization to avoid unnecessary updates
            
            isLocked = locked;
            
            if (priceText != null)
            {
                // Update price text to reflect new lock state
                if (locked)
                {
                    priceText.text = $"Unlocks at Level {unlockLevel}";
                }
                else
                {
                    priceText.text = buildingData?.price.ToString() ?? "0";
                }
            }

            if (iconImage != null)
            {
                // Update icon color to reflect new lock state
                if (locked)
                {
                    iconImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                }
                else
                {
                    iconImage.color = Color.white;
                }
            }

            if (buyButton != null)
            {
                buyButton.onClick.RemoveAllListeners(); // Clear any previous listeners to avoid duplicates. 
                buyButton.interactable = !locked;

                // CRITICAL FIX: Only attach click listener for unlocked buildings
                // This prevents locked buildings from being purchasable while maintaining proper event handling
                if (!locked && onBuyClicked != null && buildingData != null)
                {
                    buyButton.onClick.AddListener(() => onBuyClicked.Invoke(buildingData));
                }
            }

            if (resourceIconImage != null)
            {
                resourceIconImage.gameObject.SetActive(!locked);
            }

            if (lockOverlay != null)
            {
                lockOverlay.gameObject.SetActive(locked);
            }

            if (unlockLevelText != null)
            {
                if (locked)
                {
                    unlockLevelText.text = $"Level {unlockLevel}";
                    unlockLevelText.gameObject.SetActive(true);
                }
                else
                {
                    unlockLevelText.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Get the building name for this UI item.
        /// Used by shop managers to identify buildings when updating lock states.
        /// </summary>
        /// <returns>The name of the building represented by this UI item</returns>
        public string GetBuildingName()
        {
            return buildingData?.name ?? "";
        }

        /// <summary>
        /// Get the building data for this UI item.
        /// Provides access to the complete building information for external systems.
        /// </summary>
        /// <returns>The BuildingShopItem data associated with this UI component</returns>
        public BuildingShopItem GetBuildingData()
        {
            return buildingData;
        }
    }
}