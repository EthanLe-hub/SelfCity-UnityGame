using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LifeCraft.UI
{
    /// <summary>
    /// Handles the UI for a single decoration item in the prize pool.
    /// Similar to ShopBuildingItemUI but simpler - just displays name and sprite.
    /// </summary>
    public class PrizePoolItemUI : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI nameText;
        public Image iconImage;

        private string decorationName;

        /// <summary> 
        /// Setup the UI with decoration data.
        /// </summary>
        public void Setup(string decorationName)
        {
            this.decorationName = decorationName;

            if (nameText != null)
            {
                nameText.text = decorationName;
            }

            if (iconImage != null)
            {
                // Get sprite from CityBuilder
                Sprite decorationSprite = null;
                if (LifeCraft.Core.CityBuilder.Instance != null)
                {
                    var buildingData = LifeCraft.Core.CityBuilder.Instance.GetBuildingTypeData(decorationName);
                    if (buildingData != null && buildingData.buildingSprite != null)
                    {
                        decorationSprite = buildingData.buildingSprite;
                    }
                }
                
                iconImage.sprite = decorationSprite;
                iconImage.gameObject.SetActive(decorationSprite != null);
            }
        }

        /// <summary>
        /// Get the decoration name for this UI element.
        /// </summary>
        public string GetDecorationName()
        {
            return decorationName;
        }
    }
} 