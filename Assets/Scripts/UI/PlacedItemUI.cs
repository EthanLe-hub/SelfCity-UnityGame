using UnityEngine;
using UnityEngine.EventSystems;
using LifeCraft.Core; // For InventoryManager and DecorationItem

namespace LifeCraft.UI
{
    /// <summary>
    /// Handles click/removal logic for a placed item in the city grid.
    /// When removed, returns the item to the player's inventory.
    /// </summary>
    public class PlacedItemUI : MonoBehaviour, IPointerClickHandler
    {
        private DecorationItem _decorationItem;

        /// <summary>
        /// Initialize the placed item with its DecorationItem data.
        /// Call this right after instantiating the placed item.
        /// </summary>
        public void Initialize(DecorationItem item)
        {
            _decorationItem = item;
        }

        /// <summary>
        /// Called when the placed item is clicked.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            ReturnToInventory();
        }

        /// <summary>
        /// Returns the item to the inventory and removes it from the grid.
        /// </summary>
        private void ReturnToInventory()
        {
            if (_decorationItem != null && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddDecoration(_decorationItem);
            }
            Destroy(gameObject);
        }
    }
}