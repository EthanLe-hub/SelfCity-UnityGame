# AI-Powered UI Modernization Tools for Unity

This package provides AI-powered tools to automatically modernize and enhance your Unity UI with modern design patterns, animations, and effects.

## 🚀 Quick Start

### 1. Setup
1. Add the scripts to your project's `Scripts/UI/` folder
2. Install DOTween from the Asset Store (required for animations)
3. Create a UI Theme Configuration asset (Right-click → Create → AI → UI Theme Configuration)

### 2. Basic Usage
```csharp
// Add AIUIModernizer to your Canvas or UI Manager
AIUIModernizer modernizer = gameObject.AddComponent<AIUIModernizer>();

// Modernize all UI elements automatically
modernizer.ModernizeAllUI();

// Apply a specific theme
modernizer.ApplyTheme("Modern Dark");
```

## 📦 Components

### AIUIModernizer.cs
The main component that automatically modernizes existing UI elements.

**Features:**
- Glassmorphism effects
- Modern animations and transitions
- Automatic color scheme application
- Particle effects
- Responsive design adjustments

**Usage:**
```csharp
// Attach to your Canvas or UI Manager
AIUIModernizer modernizer = GetComponent<AIUIModernizer>();

// Configure settings
modernizer.enableGlassmorphism = true;
modernizer.enableAnimations = true;
modernizer.enableParticleEffects = true;

// Modernize specific elements
modernizer.ModernizeSpecificElement(buttonGameObject);

// Apply themes
modernizer.ApplyTheme("Cyber Neon");
```

### AIUIGenerator.cs
Generates new UI components from descriptions using AI-like logic.

**Features:**
- Generate buttons, panels, modals, cards, and lists
- Automatic styling and effects
- Responsive layouts
- Modern design patterns

**Usage:**
```csharp
AIUIGenerator generator = GetComponent<AIUIGenerator>();

// Generate UI from description
generator.GenerateUIFromDescription("Create a blue button with 'Start Game' text");

// Generate specific components
generator.GenerateButton("Play", new Vector2(200, 50), Color.blue);
generator.GenerateModal("Settings");
generator.GeneratePanel(new Vector2(400, 300), Color.gray);
```

### AIUIThemeConfig.cs
ScriptableObject for managing UI themes and color schemes.

**Features:**
- Pre-built modern themes
- Custom theme creation
- Easy theme switching
- Comprehensive styling options

**Usage:**
```csharp
// Get theme configuration
AIUIThemeConfig themeConfig = Resources.Load<AIUIThemeConfig>("AIUITheme");

// Apply theme to modernizer
themeConfig.ApplyThemeToUI("Modern Dark", modernizer);

// Create custom theme
themeConfig.CreateCustomTheme("My Theme", "Custom description", 
    Color.blue, Color.cyan, Color.yellow);
```

## 🎨 Available Themes

### 1. Modern Dark
- Sleek dark theme with blue accents
- Perfect for modern applications
- High contrast and readability

### 2. Modern Light
- Clean light theme with subtle shadows
- Professional appearance
- Easy on the eyes

### 3. Cyber Neon
- Futuristic neon theme with glowing effects
- Perfect for sci-fi or gaming interfaces
- Bold and vibrant colors

### 4. Minimal Clean
- Minimal theme with clean lines
- Subtle effects and spacing
- Focus on content

### 5. Gaming Dark
- Gaming-focused theme with bold colors
- High impact visual effects
- Optimized for gaming interfaces

## 🔧 Configuration

### Glassmorphism Settings
```csharp
modernizer.blurStrength = 5f;        // Blur intensity
modernizer.transparency = 0.8f;      // Transparency level
modernizer.glassColor = Color.white; // Glass tint color
```

### Animation Settings
```csharp
modernizer.animationDuration = 0.3f; // Animation speed
modernizer.animationEase = Ease.OutBack; // Animation curve
```

### Color Palette
```csharp
modernizer.primaryColor = Color.blue;     // Main color
modernizer.secondaryColor = Color.cyan;   // Secondary color
modernizer.accentColor = Color.yellow;    // Accent color
modernizer.backgroundColor = Color.black; // Background color
```

## 🎯 Advanced Usage

### Custom Theme Creation
```csharp
AIUIThemeConfig themeConfig = ScriptableObject.CreateInstance<AIUIThemeConfig>();

// Create a custom theme
AIUIThemeConfig.UITheme customTheme = new AIUIThemeConfig.UITheme
{
    themeName = "My Custom Theme",
    description = "A custom theme for my game",
    primaryColor = new Color(0.2f, 0.6f, 1f, 1f),
    secondaryColor = new Color(0.9f, 0.3f, 0.7f, 1f),
    accentColor = new Color(1f, 0.8f, 0.2f, 1f),
    backgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.9f),
    blurStrength = 6f,
    transparency = 0.85f,
    animationDuration = 0.4f
};

themeConfig.themes.Add(customTheme);
```

### Batch UI Modernization
```csharp
// Modernize all buttons in the scene
Button[] allButtons = FindObjectsOfType<Button>();
foreach (Button button in allButtons)
{
    modernizer.ModernizeSpecificElement(button.gameObject);
}

// Modernize all panels
Image[] allImages = FindObjectsOfType<Image>();
foreach (Image image in allImages)
{
    if (image.raycastTarget)
    {
        modernizer.ModernizeSpecificElement(image.gameObject);
    }
}
```

### Dynamic Theme Switching
```csharp
public void SwitchTheme(string themeName)
{
    AIUIThemeConfig themeConfig = Resources.Load<AIUIThemeConfig>("AIUITheme");
    themeConfig.ApplyThemeToUI(themeName, modernizer);
}

// Usage
SwitchTheme("Cyber Neon");
SwitchTheme("Modern Light");
SwitchTheme("Gaming Dark");
```

## 🎮 Integration with Your Game

### For City Building Games
```csharp
// Modernize building buttons
public void ModernizeBuildingUI()
{
    BuildingButton[] buildingButtons = FindObjectsOfType<BuildingButton>();
    foreach (BuildingButton buildingButton in buildingButtons)
    {
        modernizer.ModernizeSpecificElement(buildingButton.gameObject);
    }
}

// Modernize resource displays
public void ModernizeResourceUI()
{
    ResourceDisplay[] resourceDisplays = FindObjectsOfType<ResourceDisplay>();
    foreach (ResourceDisplay display in resourceDisplays)
    {
        modernizer.ModernizeSpecificElement(display.gameObject);
    }
}
```

### For Inventory Systems
```csharp
// Modernize inventory items
public void ModernizeInventoryUI()
{
    InventoryItemPrefab[] inventoryItems = FindObjectsOfType<InventoryItemPrefab>();
    foreach (InventoryItemPrefab item in inventoryItems)
    {
        modernizer.ModernizeSpecificElement(item.gameObject);
    }
}
```

## 🔍 Troubleshooting

### Common Issues

1. **Animations not working**
   - Ensure DOTween is installed
   - Check if `enableAnimations` is true

2. **Glassmorphism not visible**
   - Verify `enableGlassmorphism` is true
   - Check material settings
   - Ensure proper rendering pipeline

3. **Colors not applying**
   - Check if UI elements have Image components
   - Verify color values are not transparent
   - Ensure proper layering

### Performance Tips

1. **Limit particle effects** on mobile devices
2. **Use object pooling** for frequently created UI elements
3. **Disable animations** on low-end devices
4. **Optimize texture sizes** for UI elements

## 📱 Mobile Optimization

```csharp
// Detect device performance and adjust settings
void OptimizeForMobile()
{
    if (SystemInfo.deviceType == DeviceType.Handheld)
    {
        modernizer.enableParticleEffects = false;
        modernizer.blurStrength = 2f; // Reduce blur for performance
        modernizer.animationDuration = 0.2f; // Faster animations
    }
}
```

## 🎨 Customization Examples

### Custom Button Style
```csharp
public void CreateCustomButton(string text, Color color)
{
    AIUIGenerator generator = GetComponent<AIUIGenerator>();
    
    // Generate base button
    generator.GenerateButton(text, new Vector2(200, 50), color);
    
    // Add custom effects
    GameObject button = GameObject.Find("AI_Generated_Button");
    if (button != null)
    {
        // Add custom components
        button.AddComponent<CustomButtonEffect>();
    }
}
```

### Animated UI Elements
```csharp
public void AddEntranceAnimation(GameObject uiElement)
{
    // Start off-screen
    uiElement.transform.localScale = Vector3.zero;
    uiElement.GetComponent<CanvasGroup>().alpha = 0f;
    
    // Animate in
    uiElement.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
    uiElement.GetComponent<CanvasGroup>().DOFade(1f, 0.5f);
}
```

## 📚 Additional Resources

- [Unity UI Documentation](https://docs.unity3d.com/Manual/UISystem.html)
- [DOTween Documentation](http://dotween.demigiant.com/)
- [Modern UI Design Principles](https://material.io/design)

## 🤝 Contributing

Feel free to extend these tools with:
- New theme presets
- Additional UI components
- Custom effects and animations
- Performance optimizations

## 📄 License

This package is provided as-is for educational and development purposes. 