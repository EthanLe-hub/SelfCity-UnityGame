using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LifeCraft.Shop
{
    /// <summary>
    /// Handles the UI for a single building in the shop.
    /// </summary>
    public class ShopBuildingItemUI : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI priceText;
        public Image iconImage;
        public Button buyButton;

        public Image resourceIconImage; // Image for the resource icon (e.g., Energy Crystals) -- add field to Inspector and assign the appropriate sprite. 

        private BuildingShopItem buildingData;
        private System.Action<BuildingShopItem> onBuyClicked;

        /// <summary> 
        /// Setup the UI with building data and buy callback. 
        /// </summary>
        public void Setup(BuildingShopItem data, System.Action<BuildingShopItem> buyCallback, Sprite resourceIcon)
        {
            buildingData = data; // Store the building data for later use. 
            onBuyClicked = buyCallback; // Store the callback for when the buy button is clicked. 

            if (nameText != null)
            {
                nameText.text = data.name; // Set the name text to the building's name, if it exists. 
            }

            if (priceText != null)
            {
                priceText.text = data.price.ToString(); // Set the price text to the building's price, if it exists. 
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
            }

            if (buyButton != null)
            {
                buyButton.onClick.RemoveAllListeners(); // Clear any previous listeners to avoid duplicates. 
                buyButton.onClick.AddListener(() => onBuyClicked?.Invoke(buildingData)); // Add a listener that invokes the buy callback with the building data when clicked (uses lambda expression for simplicity). 
            }

            if (resourceIconImage != null)
            {
                resourceIconImage.sprite = resourceIcon; // Set the resource icon image to the appropriate sprite, if it exists. 
                resourceIconImage.gameObject.SetActive(resourceIcon != null); // Ensure the resource icon is only active if it exists. 
            }
        }
    }
}