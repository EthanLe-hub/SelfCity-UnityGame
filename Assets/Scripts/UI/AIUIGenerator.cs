/*
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AIUIGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    public Transform uiParent;
    public Canvas targetCanvas;
    
    [Header("Prefab Templates")]
    public GameObject buttonTemplate;
    public GameObject panelTemplate;
    public GameObject modalTemplate;
    public GameObject cardTemplate;
    
    [Header("Modern Assets")]
    public TMP_FontAsset modernFont;
    public Sprite[] modernIcons;
    public Material glassMaterial;
    
    [System.Serializable]
    public class UIComponentRequest
    {
        public string componentType; // "button", "panel", "modal", "card", "list"
        public string description;
        public Vector2 size;
        public string text;
        public Sprite icon;
        public Color color;
    }
    
    public void GenerateUIFromDescription(string description)
    {
        // Parse description and generate appropriate UI
        UIComponentRequest request = ParseDescription(description);
        GameObject generatedUI = GenerateComponent(request);
        
        if (generatedUI != null)
        {
            generatedUI.transform.SetParent(uiParent);
            generatedUI.transform.localPosition = Vector3.zero;
        }
    }
    
    UIComponentRequest ParseDescription(string description)
    {
        UIComponentRequest request = new UIComponentRequest();
        
        // Simple parsing logic - in a real implementation, you'd use NLP
        description = description.ToLower();
        
        if (description.Contains("button"))
            request.componentType = "button";
        else if (description.Contains("panel") || description.Contains("container"))
            request.componentType = "panel";
        else if (description.Contains("modal") || description.Contains("popup"))
            request.componentType = "modal";
        else if (description.Contains("card"))
            request.componentType = "card";
        else if (description.Contains("list"))
            request.componentType = "list";
        else
            request.componentType = "button"; // Default
            
        // Extract text content
        if (description.Contains("text:"))
        {
            int startIndex = description.IndexOf("text:") + 5;
            int endIndex = description.IndexOf(" ", startIndex);
            if (endIndex == -1) endIndex = description.Length;
            request.text = description.Substring(startIndex, endIndex - startIndex);
        }
        
        // Extract size
        if (description.Contains("size:"))
        {
            // Parse size information
            request.size = new Vector2(200, 50); // Default size
        }
        
        // Extract color preferences
        if (description.Contains("blue"))
            request.color = Color.blue;
        else if (description.Contains("green"))
            request.color = Color.green;
        else if (description.Contains("red"))
            request.color = Color.red;
        else if (description.Contains("purple"))
            request.color = new Color(0.5f, 0f, 0.5f);
        else
            request.color = new Color(0.2f, 0.6f, 1f); // Default blue
            
        return request;
    }
    
    GameObject GenerateComponent(UIComponentRequest request)
    {
        switch (request.componentType)
        {
            case "button":
                return GenerateModernButton(request);
            case "panel":
                return GenerateModernPanel(request);
            case "modal":
                return GenerateModernModal(request);
            case "card":
                return GenerateModernCard(request);
            case "list":
                return GenerateModernList(request);
            default:
                return GenerateModernButton(request);
        }
    }
    
    GameObject GenerateModernButton(UIComponentRequest request)
    {
        GameObject buttonObj = new GameObject("AI_Generated_Button");
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = request.size;
        
        // Add Image component for background
        Image backgroundImage = buttonObj.AddComponent<Image>();
        backgroundImage.color = request.color;
        backgroundImage.sprite = CreateRoundedRectangleSprite();
        
        // Add Button component
        Button button = buttonObj.AddComponent<Button>();
        
        // Add text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 5);
        textRect.offsetMax = new Vector2(-10, -5);
        
        TMP_Text text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = string.IsNullOrEmpty(request.text) ? "Button" : request.text;
        text.font = modernFont;
        text.fontSize = 16;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        
        // Add modern effects
        AddModernEffects(buttonObj);
        
        return buttonObj;
    }
    
    GameObject GenerateModernPanel(UIComponentRequest request)
    {
        GameObject panelObj = new GameObject("AI_Generated_Panel");
        RectTransform rectTransform = panelObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = request.size;
        
        // Add Image component
        Image backgroundImage = panelObj.AddComponent<Image>();
        backgroundImage.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);
        backgroundImage.sprite = CreateRoundedRectangleSprite();
        
        // Add glassmorphism effect
        if (glassMaterial != null)
        {
            backgroundImage.material = glassMaterial;
        }
        
        // Add shadow
        Shadow shadow = panelObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.3f);
        shadow.effectDistance = new Vector2(2f, -2f);
        
        return panelObj;
    }
    
    GameObject GenerateModernModal(UIComponentRequest request)
    {
        GameObject modalObj = new GameObject("AI_Generated_Modal");
        RectTransform rectTransform = modalObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(400, 300);
        
        // Background
        Image backgroundImage = modalObj.AddComponent<Image>();
        backgroundImage.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        backgroundImage.sprite = CreateRoundedRectangleSprite();
        
        // Title bar
        GameObject titleBar = new GameObject("TitleBar");
        titleBar.transform.SetParent(modalObj.transform);
        RectTransform titleRect = titleBar.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.8f);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        
        Image titleBackground = titleBar.AddComponent<Image>();
        titleBackground.color = request.color;
        
        // Title text
        GameObject titleTextObj = new GameObject("TitleText");
        titleTextObj.transform.SetParent(titleBar.transform);
        RectTransform titleTextRect = titleTextObj.AddComponent<RectTransform>();
        titleTextRect.anchorMin = Vector2.zero;
        titleTextRect.anchorMax = Vector2.one;
        titleTextRect.offsetMin = new Vector2(20, 0);
        titleTextRect.offsetMax = new Vector2(-20, 0);
        
        TMP_Text titleText = titleTextObj.AddComponent<TextMeshProUGUI>();
        titleText.text = string.IsNullOrEmpty(request.text) ? "Modal" : request.text;
        titleText.font = modernFont;
        titleText.fontSize = 18;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;
        
        // Close button
        GameObject closeButton = GenerateModernButton(new UIComponentRequest
        {
            componentType = "button",
            text = "X",
            size = new Vector2(30, 30),
            color = Color.red
        });
        closeButton.transform.SetParent(titleBar.transform);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.9f, 0.5f);
        closeRect.anchorMax = new Vector2(0.95f, 0.9f);
        closeRect.offsetMin = Vector2.zero;
        closeRect.offsetMax = Vector2.zero;
        
        // Content area
        GameObject contentArea = new GameObject("ContentArea");
        contentArea.transform.SetParent(modalObj.transform);
        RectTransform contentRect = contentArea.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 0);
        contentRect.anchorMax = new Vector2(1, 0.8f);
        contentRect.offsetMin = new Vector2(20, 20);
        contentRect.offsetMax = new Vector2(-20, -20);
        
        return modalObj;
    }
    
    GameObject GenerateModernCard(UIComponentRequest request)
    {
        GameObject cardObj = new GameObject("AI_Generated_Card");
        RectTransform rectTransform = cardObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(250, 150);
        
        // Card background
        Image backgroundImage = cardObj.AddComponent<Image>();
        backgroundImage.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);
        backgroundImage.sprite = CreateRoundedRectangleSprite();
        
        // Add shadow
        Shadow shadow = cardObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.3f);
        shadow.effectDistance = new Vector2(3f, -3f);
        
        // Card header
        GameObject header = new GameObject("Header");
        header.transform.SetParent(cardObj.transform);
        RectTransform headerRect = header.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 0.7f);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.offsetMin = new Vector2(15, 0);
        headerRect.offsetMax = new Vector2(-15, -10);
        
        Image headerBackground = header.AddComponent<Image>();
        headerBackground.color = request.color;
        headerBackground.sprite = CreateRoundedRectangleSprite();
        
        // Header text
        GameObject headerTextObj = new GameObject("HeaderText");
        headerTextObj.transform.SetParent(header.transform);
        RectTransform headerTextRect = headerTextObj.AddComponent<RectTransform>();
        headerTextRect.anchorMin = Vector2.zero;
        headerTextRect.anchorMax = Vector2.one;
        headerTextRect.offsetMin = new Vector2(10, 5);
        headerTextRect.offsetMax = new Vector2(-10, -5);
        
        TMP_Text headerText = headerTextObj.AddComponent<TextMeshProUGUI>();
        headerText.text = string.IsNullOrEmpty(request.text) ? "Card Title" : request.text;
        headerText.font = modernFont;
        headerText.fontSize = 14;
        headerText.color = Color.white;
        headerText.fontStyle = FontStyles.Bold;
        
        // Card content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(cardObj.transform);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 0);
        contentRect.anchorMax = new Vector2(1, 0.7f);
        contentRect.offsetMin = new Vector2(15, 15);
        contentRect.offsetMax = new Vector2(-15, -10);
        
        return cardObj;
    }
    
    GameObject GenerateModernList(UIComponentRequest request)
    {
        GameObject listObj = new GameObject("AI_Generated_List");
        RectTransform rectTransform = listObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(300, 400);
        
        // List background
        Image backgroundImage = listObj.AddComponent<Image>();
        backgroundImage.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);
        backgroundImage.sprite = CreateRoundedRectangleSprite();
        
        // Scroll view
        GameObject scrollView = new GameObject("ScrollView");
        scrollView.transform.SetParent(listObj.transform);
        RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
        scrollRect.anchorMin = Vector2.zero;
        scrollRect.anchorMax = Vector2.one;
        scrollRect.offsetMin = new Vector2(10, 10);
        scrollRect.offsetMax = new Vector2(-10, -10);
        
        ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
        
        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(scrollView.transform);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        
        // Add some sample list items
        for (int i = 0; i < 5; i++)
        {
            GameObject listItem = GenerateModernButton(new UIComponentRequest
            {
                componentType = "button",
                text = $"List Item {i + 1}",
                size = new Vector2(280, 40),
                color = new Color(0.2f, 0.2f, 0.3f, 0.8f)
            });
            listItem.transform.SetParent(content.transform);
            RectTransform itemRect = listItem.GetComponent<RectTransform>();
            itemRect.anchoredPosition = new Vector2(0, -i * 50);
        }
        
        scroll.content = contentRect;
        
        return listObj;
    }
    
    Sprite CreateRoundedRectangleSprite()
    {
        // Create a rounded rectangle texture
        Texture2D texture = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float normalizedX = (float)x / 64f;
                float normalizedY = (float)y / 64f;
                
                // Create rounded corners
                float cornerRadius = 0.2f;
                bool isCorner = (normalizedX < cornerRadius && normalizedY < cornerRadius) ||
                               (normalizedX > 1f - cornerRadius && normalizedY < cornerRadius) ||
                               (normalizedX < cornerRadius && normalizedY > 1f - cornerRadius) ||
                               (normalizedX > 1f - cornerRadius && normalizedY > 1f - cornerRadius);
                
                if (isCorner)
                {
                    float cornerX = normalizedX < 0.5f ? normalizedX : 1f - normalizedX;
                    float cornerY = normalizedY < 0.5f ? normalizedY : 1f - normalizedY;
                    float distance = Mathf.Sqrt(cornerX * cornerX + cornerY * cornerY);
                    
                    if (distance > cornerRadius)
                    {
                        pixels[y * 64 + x] = Color.clear;
                        continue;
                    }
                }
                
                pixels[y * 64 + x] = Color.white;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
    }
    
    void AddModernEffects(GameObject obj)
    {
        // Add shadow
        Shadow shadow = obj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.3f);
        shadow.effectDistance = new Vector2(2f, -2f);
        
        // Add outline
        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.1f);
        outline.effectDistance = new Vector2(1f, 1f);
    }
    
    // Public methods for easy access
    public void GenerateButton(string text, Vector2 size, Color color)
    {
        UIComponentRequest request = new UIComponentRequest
        {
            componentType = "button",
            text = text,
            size = size,
            color = color
        };
        
        GameObject button = GenerateComponent(request);
        if (button != null && uiParent != null)
        {
            button.transform.SetParent(uiParent);
            button.transform.localPosition = Vector3.zero;
        }
    }
    
    public void GeneratePanel(Vector2 size, Color color)
    {
        UIComponentRequest request = new UIComponentRequest
        {
            componentType = "panel",
            size = size,
            color = color
        };
        
        GameObject panel = GenerateComponent(request);
        if (panel != null && uiParent != null)
        {
            panel.transform.SetParent(uiParent);
            panel.transform.localPosition = Vector3.zero;
        }
    }
    
    public void GenerateModal(string title)
    {
        UIComponentRequest request = new UIComponentRequest
        {
            componentType = "modal",
            text = title
        };
        
        GameObject modal = GenerateComponent(request);
        if (modal != null && uiParent != null)
        {
            modal.transform.SetParent(uiParent);
            modal.transform.localPosition = Vector3.zero;
        }
    }
} 
*/