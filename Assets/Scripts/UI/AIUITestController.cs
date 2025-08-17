/*
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Test controller for AI UI tools - provides easy testing in the editor
/// </summary>
public class AIUITestController : MonoBehaviour
{
    [Header("Test Components")]
    [SerializeField] private AIUIIntegrationNoDOTween integration;
    [SerializeField] private AIUIModernizerNoDOTween modernizer;
    [SerializeField] private AIUIGenerator generator;
    
    [Header("Test UI")]
    [SerializeField] private Button testModernizeButton;
    [SerializeField] private Button testThemeButton;
    [SerializeField] private Button testGenerateButton;
    [SerializeField] private Button testResetButton;
    
    [Header("Test Settings")]
    [SerializeField] private string testGenerationDescription = "Create a blue button with 'Test Button' text";
    
    private int currentThemeIndex = 0;
    private string[] availableThemes = { "Gaming Dark", "Modern Dark", "Cyber Neon", "Modern Light", "Minimal Clean" };
    
    void Start()
    {
        // Get references if not assigned
        if (integration == null)
            integration = FindFirstObjectByType<AIUIIntegrationNoDOTween>();
        if (modernizer == null)
            modernizer = FindFirstObjectByType<AIUIModernizerNoDOTween>();
        if (generator == null)
            generator = FindFirstObjectByType<AIUIGenerator>();
        
        // Setup test buttons
        SetupTestButtons();
        
        Debug.Log("🧪 AI UI Test Controller Ready!");
        Debug.Log("Available test commands:");
        Debug.Log("- Test Modernize: Modernizes all UI elements");
        Debug.Log("- Test Theme: Cycles through available themes");
        Debug.Log("- Test Generate: Generates a test UI component");
        Debug.Log("- Test Reset: Resets to original state");
    }
    
    void SetupTestButtons()
    {
        // Create test buttons if they don't exist
        if (testModernizeButton == null)
            CreateTestButton("Test Modernize", TestModernize, new Vector2(0, 100));
        if (testThemeButton == null)
            CreateTestButton("Test Theme", TestTheme, new Vector2(0, 50));
        if (testGenerateButton == null)
            CreateTestButton("Test Generate", TestGenerate, new Vector2(0, 0));
        if (testResetButton == null)
            CreateTestButton("Test Reset", TestReset, new Vector2(0, -50));
    }
    
    void CreateTestButton(string text, System.Action action, Vector2 position)
    {
        // Create button GameObject
        GameObject buttonObj = new GameObject($"Test_{text.Replace(" ", "")}");
        buttonObj.transform.SetParent(transform);
        
        // Add RectTransform
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(150, 40);
        
        // Add Image component
        Image image = buttonObj.AddComponent<Image>();
        image.color = Color.gray;
        
        // Add Button component
        Button button = buttonObj.AddComponent<Button>();
        button.onClick.AddListener(() => action?.Invoke());
        
        // Add Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TMPro.TextMeshProUGUI textComponent = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = 12;
        textComponent.color = Color.white;
        textComponent.alignment = TMPro.TextAlignmentOptions.Center;
        
        // Assign to appropriate field
        switch (text)
        {
            case "Test Modernize":
                testModernizeButton = button;
                break;
            case "Test Theme":
                testThemeButton = button;
                break;
            case "Test Generate":
                testGenerateButton = button;
                break;
            case "Test Reset":
                testResetButton = button;
                break;
        }
    }
    
    /// <summary>
    /// Test modernizing all UI elements
    /// </summary>
    [ContextMenu("Test Modernize")]
    public void TestModernize()
    {
        Debug.Log("🧪 Testing UI Modernization...");
        
        if (integration != null)
        {
            integration.ModernizeAllGameUI();
        }
        else if (modernizer != null)
        {
            modernizer.ModernizeAllUI();
        }
        else
        {
            Debug.LogError("❌ No AI UI components found!");
        }
    }
    
    /// <summary>
    /// Test theme switching
    /// </summary>
    [ContextMenu("Test Theme")]
    public void TestTheme()
    {
        currentThemeIndex = (currentThemeIndex + 1) % availableThemes.Length;
        string themeName = availableThemes[currentThemeIndex];
        
        Debug.Log($"🧪 Testing Theme: {themeName}");
        
        if (integration != null)
        {
            integration.SwitchTheme(themeName);
        }
        else if (modernizer != null)
        {
            modernizer.ApplyTheme(themeName);
        }
        else
        {
            Debug.LogError("❌ No AI UI components found!");
        }
    }
    
    /// <summary>
    /// Test UI generation
    /// </summary>
    [ContextMenu("Test Generate")]
    public void TestGenerate()
    {
        Debug.Log($"🧪 Testing UI Generation: {testGenerationDescription}");
        
        if (integration != null)
        {
            integration.GenerateUIComponent(testGenerationDescription);
        }
        else if (generator != null)
        {
            generator.GenerateUIFromDescription(testGenerationDescription);
        }
        else
        {
            Debug.LogError("❌ No AI UI components found!");
        }
    }
    
    /// <summary>
    /// Test reset functionality
    /// </summary>
    [ContextMenu("Test Reset")]
    public void TestReset()
    {
        Debug.Log("🧪 Testing Reset...");
        
        // This would typically reset to original state
        // For now, just apply the default theme
        if (integration != null)
        {
            integration.ApplyGamingTheme();
        }
        else if (modernizer != null)
        {
            modernizer.ApplyTheme("Gaming Dark");
        }
        else
        {
            Debug.LogError("❌ No AI UI components found!");
        }
    }
    
    /// <summary>
    /// Test specific theme
    /// </summary>
    public void TestSpecificTheme(string themeName)
    {
        Debug.Log($"🧪 Testing Specific Theme: {themeName}");
        
        if (integration != null)
        {
            integration.SwitchTheme(themeName);
        }
        else if (modernizer != null)
        {
            modernizer.ApplyTheme(themeName);
        }
    }
    
    /// <summary>
    /// Test modernizing specific element
    /// </summary>
    public void TestModernizeElement(GameObject element)
    {
        Debug.Log($"🧪 Testing Modernize Element: {element.name}");
        
        if (integration != null)
        {
            integration.ModernizeElement(element);
        }
        else if (modernizer != null)
        {
            modernizer.ModernizeSpecificElement(element);
        }
    }
    
    /// <summary>
    /// Test generating specific component
    /// </summary>
    public void TestGenerateComponent(string componentType, string description)
    {
        Debug.Log($"🧪 Testing Generate {componentType}: {description}");
        
        if (integration != null)
        {
            integration.GenerateUIComponent(description);
        }
        else if (generator != null)
        {
            generator.GenerateUIFromDescription(description);
        }
    }
    
    // Editor helper methods
    [ContextMenu("Test Gaming Theme")]
    public void TestGamingTheme() => TestSpecificTheme("Gaming Dark");
    
    [ContextMenu("Test Modern Theme")]
    public void TestModernTheme() => TestSpecificTheme("Modern Dark");
    
    [ContextMenu("Test Neon Theme")]
    public void TestNeonTheme() => TestSpecificTheme("Cyber Neon");
    
    [ContextMenu("Test Light Theme")]
    public void TestLightTheme() => TestSpecificTheme("Modern Light");
    
    [ContextMenu("Test Minimal Theme")]
    public void TestMinimalTheme() => TestSpecificTheme("Minimal Clean");
    
    [ContextMenu("Generate Test Button")]
    public void GenerateTestButton() => TestGenerateComponent("Button", "Create a red button with 'Test' text");
    
    [ContextMenu("Generate Test Panel")]
    public void GenerateTestPanel() => TestGenerateComponent("Panel", "Create a dark panel with rounded corners");
    
    [ContextMenu("Generate Test Modal")]
    public void GenerateTestModal() => TestGenerateComponent("Modal", "Create a modal with 'Settings' title");
} 
*/