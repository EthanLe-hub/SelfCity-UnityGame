/*
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;

public class AIUIModernizerNoDOTween : MonoBehaviour
{
    [Header("Modernization Settings")]
    public bool enableGlassmorphism = true;
    public bool enableAnimations = true;
    public bool enableParticleEffects = true;
    public bool enableResponsiveDesign = true;
    
    [Header("Glassmorphism Settings")]
    public float blurStrength = 5f;
    public float transparency = 0.8f;
    public Color glassColor = new Color(1f, 1f, 1f, 0.1f);
    
    [Header("Animation Settings")]
    public float animationDuration = 0.3f;
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Modern Color Palette")]
    public Color primaryColor = new Color(0.2f, 0.6f, 1f, 1f);
    public Color secondaryColor = new Color(0.9f, 0.3f, 0.7f, 1f);
    public Color accentColor = new Color(1f, 0.8f, 0.2f, 1f);
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.9f);
    
    [Header("References")]
    public Canvas targetCanvas;
    public List<Button> buttonsToModernize = new List<Button>();
    public List<Image> panelsToModernize = new List<Image>();
    
    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, Color> originalColors = new Dictionary<GameObject, Color>();
    private Dictionary<GameObject, Coroutine> activeAnimations = new Dictionary<GameObject, Coroutine>();
    
    void Start()
    {
        if (enableGlassmorphism)
            ApplyGlassmorphism();
            
        if (enableAnimations)
            SetupAnimations();
            
        if (enableResponsiveDesign)
            SetupResponsiveDesign();
    }
    
    public void ModernizeAllUI()
    {
        // Find all UI elements automatically
        FindAndModernizeButtons();
        FindAndModernizePanels();
        ApplyModernColorScheme();
        AddParticleEffects();
    }
    
    void FindAndModernizeButtons()
    {
        Button[] allButtons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (Button button in allButtons)
        {
            ModernizeButton(button);
        }
    }
    
    void FindAndModernizePanels()
    {
        Image[] allImages = Object.FindObjectsByType<Image>(FindObjectsSortMode.None);
        foreach (Image image in allImages)
        {
            if (image.raycastTarget && image.GetComponent<RectTransform>() != null)
            {
                ModernizePanel(image);
            }
        }
    }
    
    void ModernizeButton(Button button)
    {
        if (button == null) return;
        
        // Store original properties
        originalScales[button.gameObject] = button.transform.localScale;
        originalColors[button.gameObject] = button.GetComponent<Image>()?.color ?? Color.white;
        
        // Apply modern styling
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            // Apply glassmorphism effect
            if (enableGlassmorphism)
            {
                ApplyGlassmorphismToImage(buttonImage);
            }
            
            // Add gradient effect
            AddGradientEffect(buttonImage);
            
            // Add shadow
            AddShadowEffect(button.gameObject);
        }
        
        // Modernize text
        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            ModernizeText(buttonText);
        }
        
        // Add hover animations
        if (enableAnimations)
        {
            AddButtonAnimations(button);
        }
    }
    
    void ModernizePanel(Image panel)
    {
        if (panel == null) return;
        
        // Apply glassmorphism
        if (enableGlassmorphism)
        {
            ApplyGlassmorphismToImage(panel);
        }
        
        // Add subtle border
        AddBorderEffect(panel.gameObject);
        
        // Add shadow
        AddShadowEffect(panel.gameObject);
    }
    
    void ApplyGlassmorphismToImage(Image image)
    {
        // Create glassmorphism material
        Material glassMaterial = new Material(Shader.Find("UI/Default"));
        glassMaterial.SetFloat("_Blur", blurStrength);
        glassMaterial.SetColor("_GlassColor", glassColor);
        glassMaterial.SetFloat("_Transparency", transparency);
        
        image.material = glassMaterial;
    }
    
    void AddGradientEffect(Image image)
    {
        // Create gradient overlay
        GameObject gradientObj = new GameObject("GradientOverlay");
        gradientObj.transform.SetParent(image.transform);
        gradientObj.transform.SetAsFirstSibling();
        
        Image gradientImage = gradientObj.AddComponent<Image>();
        gradientImage.sprite = CreateGradientSprite();
        gradientImage.color = new Color(1f, 1f, 1f, 0.3f);
        
        RectTransform gradientRect = gradientImage.GetComponent<RectTransform>();
        gradientRect.anchorMin = Vector2.zero;
        gradientRect.anchorMax = Vector2.one;
        gradientRect.offsetMin = Vector2.zero;
        gradientRect.offsetMax = Vector2.zero;
    }
    
    Sprite CreateGradientSprite()
    {
        // Create a simple gradient texture
        Texture2D gradientTexture = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float t = (float)y / 64f;
                pixels[y * 64 + x] = Color.Lerp(primaryColor, secondaryColor, t);
            }
        }
        
        gradientTexture.SetPixels(pixels);
        gradientTexture.Apply();
        
        return Sprite.Create(gradientTexture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
    }
    
    void AddShadowEffect(GameObject target)
    {
        // Add shadow component
        Shadow shadow = target.GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = target.AddComponent<Shadow>();
        }
        
        shadow.effectColor = new Color(0f, 0f, 0f, 0.3f);
        shadow.effectDistance = new Vector2(2f, -2f);
    }
    
    void AddBorderEffect(GameObject target)
    {
        // Add outline component
        Outline outline = target.GetComponent<Outline>();
        if (outline == null)
        {
            outline = target.AddComponent<Outline>();
        }
        
        outline.effectColor = accentColor;
        outline.effectDistance = new Vector2(1f, 1f);
    }
    
    void ModernizeText(TMP_Text text)
    {
        if (text == null) return;
        
        // Apply modern font settings
        text.fontSize = Mathf.Max(text.fontSize, 14f);
        text.color = Color.white;
        text.fontStyle = FontStyles.Bold;
        
        // Add text shadow
        Shadow textShadow = text.GetComponent<Shadow>();
        if (textShadow == null)
        {
            textShadow = text.gameObject.AddComponent<Shadow>();
        }
        
        textShadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
        textShadow.effectDistance = new Vector2(1f, -1f);
    }
    
    void AddButtonAnimations(Button button)
    {
        // Add event triggers for hover effects
        EventTrigger eventTrigger = button.gameObject.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = button.gameObject.AddComponent<EventTrigger>();
        }
        
        // Pointer Enter Event
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => { OnButtonHoverEnter(button.gameObject); });
        eventTrigger.triggers.Add(enterEntry);
        
        // Pointer Exit Event
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { OnButtonHoverExit(button.gameObject); });
        eventTrigger.triggers.Add(exitEntry);
        
        // Pointer Click Event
        EventTrigger.Entry clickEntry = new EventTrigger.Entry();
        clickEntry.eventID = EventTriggerType.PointerClick;
        clickEntry.callback.AddListener((data) => { OnButtonClick(button.gameObject); });
        eventTrigger.triggers.Add(clickEntry);
    }
    
    void OnButtonHoverEnter(GameObject button)
    {
        if (originalScales.ContainsKey(button))
        {
            StopAnimation(button);
            activeAnimations[button] = StartCoroutine(AnimateScale(button, originalScales[button] * 1.1f));
        }
    }
    
    void OnButtonHoverExit(GameObject button)
    {
        if (originalScales.ContainsKey(button))
        {
            StopAnimation(button);
            activeAnimations[button] = StartCoroutine(AnimateScale(button, originalScales[button]));
        }
    }
    
    void OnButtonClick(GameObject button)
    {
        if (originalScales.ContainsKey(button))
        {
            StopAnimation(button);
            StartCoroutine(AnimateClick(button));
        }
    }
    
    void StopAnimation(GameObject obj)
    {
        if (activeAnimations.ContainsKey(obj))
        {
            if (activeAnimations[obj] != null)
            {
                StopCoroutine(activeAnimations[obj]);
            }
            activeAnimations.Remove(obj);
        }
    }
    
    IEnumerator AnimateScale(GameObject obj, Vector3 targetScale)
    {
        Vector3 startScale = obj.transform.localScale;
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            float curveValue = animationCurve.Evaluate(t);
            
            obj.transform.localScale = Vector3.Lerp(startScale, targetScale, curveValue);
            yield return null;
        }
        
        obj.transform.localScale = targetScale;
    }
    
    IEnumerator AnimateClick(GameObject obj)
    {
        Vector3 originalScale = originalScales[obj];
        Vector3 clickScale = originalScale * 0.95f;
        
        // Scale down
        yield return StartCoroutine(AnimateScale(obj, clickScale));
        
        // Scale back up
        yield return StartCoroutine(AnimateScale(obj, originalScale));
    }
    
    void ApplyModernColorScheme()
    {
        // Apply modern colors to all UI elements
        Image[] allImages = Object.FindObjectsByType<Image>(FindObjectsSortMode.None);
        foreach (Image image in allImages)
        {
            if (image.raycastTarget)
            {
                // Apply subtle color tinting
                Color currentColor = image.color;
                Color newColor = Color.Lerp(currentColor, backgroundColor, 0.1f);
                image.color = newColor;
            }
        }
    }
    
    void AddParticleEffects()
    {
        if (!enableParticleEffects) return;
        
        // Add particle effects to buttons
        Button[] allButtons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (Button button in allButtons)
        {
            AddParticleSystemToButton(button);
        }
    }
    
    void AddParticleSystemToButton(Button button)
    {
        // Create particle system for button effects
        GameObject particleObj = new GameObject("ButtonParticles");
        particleObj.transform.SetParent(button.transform);
        particleObj.transform.localPosition = Vector3.zero;
        
        ParticleSystem particles = particleObj.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startLifetime = 1f;
        main.startSpeed = 2f;
        main.startSize = 0.1f;
        main.startColor = accentColor;
        main.maxParticles = 20;
        
        var emission = particles.emission;
        emission.rateOverTime = 0f; // Only emit on events
        
        // Emit particles on button click
        EventTrigger eventTrigger = button.gameObject.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = button.gameObject.AddComponent<EventTrigger>();
        }
        
        EventTrigger.Entry clickEntry = new EventTrigger.Entry();
        clickEntry.eventID = EventTriggerType.PointerClick;
        clickEntry.callback.AddListener((data) => { particles.Emit(10); });
        eventTrigger.triggers.Add(clickEntry);
    }
    
    void SetupResponsiveDesign()
    {
        if (targetCanvas == null) return;
        
        // Add responsive canvas scaler
        CanvasScaler scaler = targetCanvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = targetCanvas.gameObject.AddComponent<CanvasScaler>();
        }
        
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }
    
    void ApplyGlassmorphism()
    {
        // Apply glassmorphism to all UI panels
        Image[] allImages = Object.FindObjectsByType<Image>(FindObjectsSortMode.None);
        foreach (Image image in allImages)
        {
            if (image.raycastTarget && image.GetComponent<RectTransform>() != null)
            {
                ApplyGlassmorphismToImage(image);
            }
        }
    }
    
    void SetupAnimations()
    {
        // Setup entrance animations for all UI elements
        CanvasGroup[] allCanvasGroups = Object.FindObjectsByType<CanvasGroup>(FindObjectsSortMode.None);
        foreach (CanvasGroup group in allCanvasGroups)
        {
            group.alpha = 0f;
            group.transform.localScale = Vector3.zero;
            
            // Animate in
            StartCoroutine(AnimateEntrance(group));
        }
    }
    
    IEnumerator AnimateEntrance(CanvasGroup group)
    {
        yield return new WaitForSeconds(0.1f);
        
        float elapsed = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = Vector3.one;
        float startAlpha = 0f;
        float targetAlpha = 1f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            float curveValue = animationCurve.Evaluate(t);
            
            group.transform.localScale = Vector3.Lerp(startScale, targetScale, curveValue);
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, curveValue);
            yield return null;
        }
        
        group.transform.localScale = targetScale;
        group.alpha = targetAlpha;
    }
    
    // Public methods for runtime modernization
    public void ModernizeSpecificElement(GameObject element)
    {
        Button button = element.GetComponent<Button>();
        if (button != null)
        {
            ModernizeButton(button);
        }
        
        Image image = element.GetComponent<Image>();
        if (image != null)
        {
            ModernizePanel(image);
        }
    }
    
    public void ApplyTheme(string themeName)
    {
        switch (themeName.ToLower())
        {
            case "dark":
                primaryColor = new Color(0.2f, 0.2f, 0.3f, 1f);
                secondaryColor = new Color(0.4f, 0.4f, 0.6f, 1f);
                accentColor = new Color(0.8f, 0.6f, 1f, 1f);
                backgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.9f);
                break;
                
            case "light":
                primaryColor = new Color(0.9f, 0.9f, 0.95f, 1f);
                secondaryColor = new Color(0.8f, 0.8f, 0.9f, 1f);
                accentColor = new Color(0.2f, 0.6f, 1f, 1f);
                backgroundColor = new Color(1f, 1f, 1f, 0.9f);
                break;
                
            case "neon":
                primaryColor = new Color(0f, 1f, 0.5f, 1f);
                secondaryColor = new Color(1f, 0f, 0.8f, 1f);
                accentColor = new Color(0f, 0.8f, 1f, 1f);
                backgroundColor = new Color(0.05f, 0.05f, 0.1f, 0.9f);
                break;
        }
        
        ApplyModernColorScheme();
    }
} 
*/