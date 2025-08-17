/*
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AIUITheme", menuName = "AI/UI Theme Configuration")]
public class AIUIThemeConfig : ScriptableObject
{
    [System.Serializable]
    public class UITheme
    {
        public string themeName;
        public string description;
        
        [Header("Color Palette")]
        public Color primaryColor = Color.white;
        public Color secondaryColor = Color.white;
        public Color accentColor = Color.white;
        public Color backgroundColor = Color.white;
        public Color textColor = Color.white;
        public Color shadowColor = Color.black;
        
        [Header("Visual Effects")]
        public float blurStrength = 5f;
        public float transparency = 0.8f;
        public float shadowDistance = 2f;
        public float borderWidth = 1f;
        
        [Header("Animation Settings")]
        public float animationDuration = 0.3f;
        public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public bool enableHoverEffects = true;
        public bool enableClickEffects = true;
        
        [Header("Typography")]
        public int baseFontSize = 16;
        public float lineSpacing = 1.2f;
        public FontStyle fontStyle = FontStyle.Normal;
        
        [Header("Layout")]
        public float cornerRadius = 8f;
        public float padding = 10f;
        public float spacing = 5f;
    }
    
    [Header("Available Themes")]
    public List<UITheme> themes = new List<UITheme>();
    
    [Header("Default Theme")]
    public int defaultThemeIndex = 0;
    
    void OnEnable()
    {
        // Create default themes if none exist
        if (themes.Count == 0)
        {
            CreateDefaultThemes();
        }
    }
    
    void CreateDefaultThemes()
    {
        // Modern Dark Theme
        UITheme darkTheme = new UITheme
        {
            themeName = "Modern Dark",
            description = "A sleek dark theme with blue accents",
            primaryColor = new Color(0.1f, 0.1f, 0.15f, 0.9f),
            secondaryColor = new Color(0.2f, 0.2f, 0.3f, 0.8f),
            accentColor = new Color(0.2f, 0.6f, 1f, 1f),
            backgroundColor = new Color(0.05f, 0.05f, 0.1f, 0.95f),
            textColor = Color.white,
            shadowColor = new Color(0f, 0f, 0f, 0.3f),
            blurStrength = 5f,
            transparency = 0.8f,
            shadowDistance = 2f,
            borderWidth = 1f,
            animationDuration = 0.3f,
            enableHoverEffects = true,
            enableClickEffects = true,
            baseFontSize = 16,
            lineSpacing = 1.2f,
            fontStyle = FontStyle.Normal,
            cornerRadius = 8f,
            padding = 10f,
            spacing = 5f
        };
        
        // Light Theme
        UITheme lightTheme = new UITheme
        {
            themeName = "Modern Light",
            description = "A clean light theme with subtle shadows",
            primaryColor = new Color(0.95f, 0.95f, 0.97f, 0.9f),
            secondaryColor = new Color(0.9f, 0.9f, 0.95f, 0.8f),
            accentColor = new Color(0.2f, 0.6f, 1f, 1f),
            backgroundColor = new Color(1f, 1f, 1f, 0.95f),
            textColor = new Color(0.1f, 0.1f, 0.15f, 1f),
            shadowColor = new Color(0f, 0f, 0f, 0.1f),
            blurStrength = 3f,
            transparency = 0.9f,
            shadowDistance = 1f,
            borderWidth = 1f,
            animationDuration = 0.25f,
            enableHoverEffects = true,
            enableClickEffects = true,
            baseFontSize = 14,
            lineSpacing = 1.3f,
            fontStyle = FontStyle.Normal,
            cornerRadius = 6f,
            padding = 8f,
            spacing = 4f
        };
        
        // Neon Theme
        UITheme neonTheme = new UITheme
        {
            themeName = "Cyber Neon",
            description = "A futuristic neon theme with glowing effects",
            primaryColor = new Color(0.05f, 0.05f, 0.1f, 0.9f),
            secondaryColor = new Color(0.1f, 0.1f, 0.2f, 0.8f),
            accentColor = new Color(0f, 1f, 0.5f, 1f),
            backgroundColor = new Color(0.02f, 0.02f, 0.05f, 0.95f),
            textColor = new Color(0f, 1f, 0.8f, 1f),
            shadowColor = new Color(0f, 1f, 0.5f, 0.3f),
            blurStrength = 8f,
            transparency = 0.7f,
            shadowDistance = 3f,
            borderWidth = 2f,
            animationDuration = 0.4f,
            enableHoverEffects = true,
            enableClickEffects = true,
            baseFontSize = 18,
            lineSpacing = 1.1f,
            fontStyle = FontStyle.Bold,
            cornerRadius = 12f,
            padding = 15f,
            spacing = 8f
        };
        
        // Minimal Theme
        UITheme minimalTheme = new UITheme
        {
            themeName = "Minimal Clean",
            description = "A minimal theme with clean lines and subtle effects",
            primaryColor = new Color(0.98f, 0.98f, 0.98f, 0.9f),
            secondaryColor = new Color(0.95f, 0.95f, 0.95f, 0.8f),
            accentColor = new Color(0.3f, 0.3f, 0.3f, 1f),
            backgroundColor = new Color(1f, 1f, 1f, 0.98f),
            textColor = new Color(0.2f, 0.2f, 0.2f, 1f),
            shadowColor = new Color(0f, 0f, 0f, 0.05f),
            blurStrength = 2f,
            transparency = 0.95f,
            shadowDistance = 0.5f,
            borderWidth = 0.5f,
            animationDuration = 0.2f,
            enableHoverEffects = false,
            enableClickEffects = true,
            baseFontSize = 12,
            lineSpacing = 1.4f,
            fontStyle = FontStyle.Normal,
            cornerRadius = 2f,
            padding = 5f,
            spacing = 3f
        };
        
        // Gaming Theme
        UITheme gamingTheme = new UITheme
        {
            themeName = "Gaming Dark",
            description = "A gaming-focused theme with bold colors and effects",
            primaryColor = new Color(0.08f, 0.08f, 0.12f, 0.9f),
            secondaryColor = new Color(0.15f, 0.15f, 0.2f, 0.8f),
            accentColor = new Color(1f, 0.4f, 0.2f, 1f),
            backgroundColor = new Color(0.05f, 0.05f, 0.08f, 0.95f),
            textColor = new Color(1f, 1f, 1f, 1f),
            shadowColor = new Color(0f, 0f, 0f, 0.4f),
            blurStrength = 6f,
            transparency = 0.8f,
            shadowDistance = 2.5f,
            borderWidth = 1.5f,
            animationDuration = 0.35f,
            enableHoverEffects = true,
            enableClickEffects = true,
            baseFontSize = 16,
            lineSpacing = 1.2f,
            fontStyle = FontStyle.Bold,
            cornerRadius = 10f,
            padding = 12f,
            spacing = 6f
        };
        
        themes.Add(darkTheme);
        themes.Add(lightTheme);
        themes.Add(neonTheme);
        themes.Add(minimalTheme);
        themes.Add(gamingTheme);
    }
    
    public UITheme GetTheme(string themeName)
    {
        foreach (UITheme theme in themes)
        {
            if (theme.themeName.ToLower() == themeName.ToLower())
            {
                return theme;
            }
        }
        
        // Return default theme if not found
        return themes.Count > 0 ? themes[defaultThemeIndex] : null;
    }
    
    public UITheme GetThemeByIndex(int index)
    {
        if (index >= 0 && index < themes.Count)
        {
            return themes[index];
        }
        
        return themes.Count > 0 ? themes[defaultThemeIndex] : null;
    }
    
    public List<string> GetThemeNames()
    {
        List<string> names = new List<string>();
        foreach (UITheme theme in themes)
        {
            names.Add(theme.themeName);
        }
        return names;
    }
    
    public void ApplyThemeToUI(string themeName, AIUIModernizerNoDOTween modernizer)
    {
        UITheme theme = GetTheme(themeName);
        if (theme != null && modernizer != null)
        {
            // Apply theme colors
            modernizer.primaryColor = theme.primaryColor;
            modernizer.secondaryColor = theme.secondaryColor;
            modernizer.accentColor = theme.accentColor;
            modernizer.backgroundColor = theme.backgroundColor;
            
            // Apply theme effects
            modernizer.blurStrength = theme.blurStrength;
            modernizer.transparency = theme.transparency;
            modernizer.animationDuration = theme.animationDuration;
            
            // Re-apply modernization
            modernizer.ModernizeAllUI();
        }
    }
    
    public void CreateCustomTheme(string name, string description, Color primary, Color secondary, Color accent)
    {
        UITheme customTheme = new UITheme
        {
            themeName = name,
            description = description,
            primaryColor = primary,
            secondaryColor = secondary,
            accentColor = accent,
            backgroundColor = new Color(primary.r * 0.1f, primary.g * 0.1f, primary.b * 0.1f, 0.95f),
            textColor = Color.white,
            shadowColor = new Color(0f, 0f, 0f, 0.3f),
            blurStrength = 5f,
            transparency = 0.8f,
            shadowDistance = 2f,
            borderWidth = 1f,
            animationDuration = 0.3f,
            enableHoverEffects = true,
            enableClickEffects = true,
            baseFontSize = 16,
            lineSpacing = 1.2f,
            fontStyle = FontStyle.Normal,
            cornerRadius = 8f,
            padding = 10f,
            spacing = 5f
        };
        
        themes.Add(customTheme);
    }
}
*/ 