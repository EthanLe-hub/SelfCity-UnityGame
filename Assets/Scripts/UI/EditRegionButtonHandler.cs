using UnityEngine; // We need UnityEngine for MonoBehaviour and other Unity features.
using UnityEngine.UI; // We need UnityEngine.UI for UI components like Button.
using TMPro; // We need TextMeshPro for text components.
using LifeCraft.Core; // We need to access the RegionEditManager for edit mode functionality. 

namespace LifeCraft.UI
{
    public class EditRegionButtonHandler : MonoBehaviour
    {
        [SerializeField] private Button editButton; // Reference to the Edit Region button.
        [SerializeField] private TMP_Text editButtonText; // Reference to the TextMeshPro text component for the Edit Region button. 

        private void Start()
        {
            if (editButton != null) // If the button is assigned in the Inspector:
            {
                editButton.onClick.AddListener(OnButtonClick); // Add a listener for button clicks. 
            }

            // Listen for edit mode changes to update the button text:
            if (RegionEditManager.Instance != null)
            {
                RegionEditManager.Instance.OnEditModeChanged.AddListener(UpdateButtonLabel); // Subscribe to the edit mode change event. 
            }

            // Set initial label:
            UpdateButtonLabel(RegionEditManager.Instance != null && RegionEditManager.Instance.IsEditModeActive); // Update the button label based on the current edit mode state; the parameter is true if edit mode is active, false otherwise. 
        }

        private void OnDestroy()
        {
            if (editButton != null)
            {
                editButton.onClick.RemoveListener(OnButtonClick); // Remove the listener to prevent memory leaks. 
            }

            if (RegionEditManager.Instance != null)
            {
                RegionEditManager.Instance.OnEditModeChanged.RemoveListener(UpdateButtonLabel); // Unsubscribe from the edit mode change event to prevent memory leaks. 
            }
        }

        private void OnButtonClick()
        {
            if (RegionEditManager.Instance != null)
            {
                RegionEditManager.Instance.ToggleEditMode(); // Toggle the edit mode when the button is clicked. 
            }
        }

        private void UpdateButtonLabel(bool isEditModeActive)
        {
            if (editButtonText != null) // If the TextMeshPro text component is assigned:
            {
                editButtonText.text = isEditModeActive ? "Exit Edit Mode" : "Edit Region"; // Update the Edit Button text to "Exit Edit Mode" if edit mode is true (active), otherwise set it to "Edit Region". 
            }
        }
    }
}