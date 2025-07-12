using UnityEngine; // We need UnityEngine for MonoBehaviour and other Unity features. 

public class RegionEditManager : MonoBehaviour
{
    public static RegionEditManager Instance { get; private set; } // Singleton instance for easy access for other scripts. 
    public bool IsEditModeActive { get; private set; } // Flag to check if edit mode is active. 
    public GameObject gridOverlay; // Reference to the grid overlay GameObject. 
    public GameObject inventoryPanel; // Reference to the inventory panel GameObject. 

    private void Awake() // Singleton pattern implementation. 
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject); // Ensure only one instance exists. 
            return;
        }

        Instance = this; // Set the singleton instance. 
    }

    public void EnterEditMode()
    {
        gridOverlay.SetActive(true); // Show the grid overlay when entering edit mode. 
        inventoryPanel.SetActive(true); // Show the inventory panel when entering edit mode. 
        IsEditModeActive = true; // Set the edit mode flag to true. 
    }

    public void ExitEditMode()
    {
        gridOverlay.SetActive(false); // Hide the grid overlay when exiting edit mode. 
        inventoryPanel.SetActive(false); // Hide the inventory panel when exiting edit mode. 
        IsEditModeActive = false; // Set the edit mode flag to false. 
    }
}