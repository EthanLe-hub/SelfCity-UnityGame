using UnityEngine; // We need UnityEngine for MonoBehaviour and ScriptableObject. 

public class ShopTabManager : MonoBehaviour // Inherits from MonoBehaviour to allow it to be attached to a GameObject. 
{
    [Header("Content Panels")] // Header for organization in the Unity Inspector. 
    public GameObject todayShopPanel; // Panel for today's decor items. 
    public GameObject healthHarborShopPanel; // Panel for Health Harbor building items. 
    public GameObject mindPalaceShopPanel; // Panel for Mind Palace building items. 
    public GameObject creativeCommonsShopPanel; // Panel for Creative Commons building items. 
    public GameObject socialSquareShopPanel; // Panel for Social Square building items. 

    private void Start() // Unity's Start method, called when the script instance is being loaded. 
    {
        ShowTodayContent(); // Show today's content by default when the game starts. 
    }

    public void ShowTodayContent() // Method to show today's decor items. 
    {
        HideAllContentPanels(); // First, hide all content panels in the Shop tab. 
        if (todayShopPanel != null) // Check if the todayShopPanel exists. 
        {
            todayShopPanel.SetActive(true); // Activate the todayShopPanel to show it. 
        }
    }

    public void ShowHealthHarborContent() // Method to show Health Harbor building items. 
    {
        HideAllContentPanels(); // Hide all content panels first. 
        if (healthHarborShopPanel != null) // Check if the healthHarborShopPanel exists. 
        {
            healthHarborShopPanel.SetActive(true); // Activate the healthHarborShopPanel to show it. 
        }
    }

    public void ShowMindPalaceContent() // Method to show Mind Palace building items. 
    {
        HideAllContentPanels(); // Hide all content panels first. 
        if (mindPalaceShopPanel != null) // Check if the mindPalaceShopPanel exists. 
        {
            mindPalaceShopPanel.SetActive(true); // Activate the mindPalaceShopPanel to show it. 
        }
    }

    public void ShowCreativeCommonsContent() // Method to show Creative Commons building items. 
    {
        HideAllContentPanels(); // Hide all content panels first. 
        if (creativeCommonsShopPanel != null) // Check if the creativeCommonsShopPanel exists. 
        {
            creativeCommonsShopPanel.SetActive(true); // Activate the creativeCommonsShopPanel to show it. 
        }
    }

    public void ShowSocialSquareContent() // Method to show Social Square building items. 
    {
        HideAllContentPanels(); // Hide all content panels first. 
        if (socialSquareShopPanel != null) // Check if the socialSquareShopPanel exists. 
        {
            socialSquareShopPanel.SetActive(true); // Activate the socialSquareShopPanel to show it. 
        }
    }

    private void HideAllContentPanels() // Method to hide all content panels in the Shop tab. 
    {
        // Deactivate all content panels to hide them. 
        if (todayShopPanel != null) todayShopPanel.SetActive(false);
        if (healthHarborShopPanel != null) healthHarborShopPanel.SetActive(false);
        if (mindPalaceShopPanel != null) mindPalaceShopPanel.SetActive(false);
        if (creativeCommonsShopPanel != null) creativeCommonsShopPanel.SetActive(false); 
        if (socialSquareShopPanel != null) socialSquareShopPanel.SetActive(false); 
    }
}