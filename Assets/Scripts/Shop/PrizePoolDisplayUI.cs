using UnityEngine;
using TMPro;

public class PrizePoolDisplayUI : MonoBehaviour
{
    public PrizePoolManager prizePoolManager; // Assign in Inspector
    public Transform allPrizePoolContainer;   // Assign in Inspector
    public Transform premiumOnlyPrizePoolContainer; // Assign in Inspector
    public GameObject prizePoolItemTextPrefab; // Assign in Inspector

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
            var go = Instantiate(prizePoolItemTextPrefab, allPrizePoolContainer); // Instantiate a new GameObject from the prizePoolItemTextPrefab and parent it to the allPrizePoolContainer. 
            go.GetComponent<TMP_Text>().text = decor; // Set the text of the instantiated GameObject to the decoration name. 
        }

        // Populate Premium Only pool
        foreach (var decor in prizePoolManager.currentPremiumOnlyPool) // Loop through each decoration in the current premium only prize pool. 
        {
            var go = Instantiate(prizePoolItemTextPrefab, premiumOnlyPrizePoolContainer); // Instantiate a new GameObject from the prizePoolItemTextPrefab and parent it to the premiumOnlyPrizePoolContainer.
            go.GetComponent<TMP_Text>().text = decor; // Set the text of the instantiated GameObject to the decoration name.
        }
    }
}