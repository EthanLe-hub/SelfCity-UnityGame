using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LifeCraft.Shop;

namespace LifeCraft.UI
{
    public class BuildingButton : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button button;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text costText;

        private BuildingShopItem buildingData;
        private System.Action<string> onClickCallback;

        private void Start()
        {
            if (button == null)
                button = GetComponent<Button>();
        }

        public void Initialize(BuildingShopItem building, System.Action<string> onClick)
        {
            buildingData = building;
            onClickCallback = onClick;

            // Update UI elements
            if (nameText != null)
                nameText.text = building.name;

            if (costText != null)
                costText.text = $"{building.price}";

            if (iconImage != null)
            {
                // Note: BuildingShopItem doesn't have buildingSprite, you may need to get it from CityBuilder
                iconImage.enabled = false; // Disable for now since sprite is not available
            }

            // Setup button click
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnButtonClicked);
            }
        }

        private void OnButtonClicked()
        {
            if (buildingData != null && onClickCallback != null)
            {
                onClickCallback(buildingData.name);
            }
        }

        // Legacy method for migration compatibility
        public void Initialize() { }
    }
} 