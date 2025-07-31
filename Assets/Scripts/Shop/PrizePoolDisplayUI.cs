using UnityEngine;
using TMPro;
using UnityEngine.UI; // For TextMeshPro and Image components. 
using LifeCraft.UI; // For PrizePoolManager and other UI-related classes (like PrizePoolItemUI). 

namespace LifeCraft.Shop
{
    public class PrizePoolDisplayUI : MonoBehaviour
    {
        public PrizePoolManager prizePoolManager; // Assign in Inspector
        public Transform allPrizePoolContainer;   // Assign in Inspector
        public Transform premiumOnlyPrizePoolContainer; // Assign in Inspector
        public GameObject prizePoolItemPrefab; // Assign the PrizePoolItem prefab (renamed from prizePoolItemTextPrefab) 

        private void Start()
        {
            PopulatePrizePools(); // Populate the prize pools when the script starts. 
        }

        private void OnEnable()
        {
            if (prizePoolManager != null)
            {
                prizePoolManager.OnPrizePoolReset += PopulatePrizePools; // Subscribe to the OnPrizePoolReset event to repopulate the prize pools when they are reset. 
            }
        }

        private void OnDisable()
        {
            if (prizePoolManager != null)
            {
                prizePoolManager.OnPrizePoolReset -= PopulatePrizePools; // Unsubscribe from the OnPrizePoolReset event to avoid memory leaks. 
            }
        }

        public void PopulatePrizePools()
        {
            // Clear old items
            foreach (Transform child in allPrizePoolContainer) Destroy(child.gameObject); // Clear all items in the "All:" prize pool container. 
            foreach (Transform child in premiumOnlyPrizePoolContainer) Destroy(child.gameObject); // Clear all items in the "Premium Only:" prize pool container. 

            // Populate All pool
            foreach (var decor in prizePoolManager.currentFreeAndPremiumPool) // Loop through each decoration in the current free and premium prize pool. 
            {
                var go = Instantiate(prizePoolItemPrefab, allPrizePoolContainer); // Instantiate a new GameObject from the prizePoolItemPrefab and parent it to the allPrizePoolContainer. 
                var ui = go.GetComponent<PrizePoolItemUI>();
                if (ui != null)
                {
                    ui.Setup(decor); // Setup the UI with decoration name and sprite
                }
            }

            // Populate Premium Only pool
            foreach (var decor in prizePoolManager.currentPremiumOnlyPool) // Loop through each decoration in the current premium only prize pool. 
            {
                var go = Instantiate(prizePoolItemPrefab, premiumOnlyPrizePoolContainer); // Instantiate a new GameObject from the prizePoolItemPrefab and parent it to the premiumOnlyPrizePoolContainer.
                var ui = go.GetComponent<PrizePoolItemUI>();
                if (ui != null)
                {
                    ui.Setup(decor); // Setup the UI with decoration name and sprite
                }
            }
        }
    }
}