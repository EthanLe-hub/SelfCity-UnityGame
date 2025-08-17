using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LifeCraft.Core;

public class BatchAssignSprites : EditorWindow
{
    private string spritesFolderPath = "Assets/Sprites/Buildings/";
    private bool useExactMatch = true;
    private bool previewOnly = true;

    [MenuItem("Tools/Batch Assign Building Sprites")]
    public static void ShowWindow()
    {
        GetWindow<BatchAssignSprites>("Batch Assign Building Sprites");
    }

    void OnGUI()
    {
        GUILayout.Label("Batch Assign Sprites to Building Types", EditorStyles.boldLabel);
        
        GUILayout.Space(10);
        
        spritesFolderPath = EditorGUILayout.TextField("Sprites Folder", spritesFolderPath);
        useExactMatch = EditorGUILayout.Toggle("Exact Name Match", useExactMatch);
        previewOnly = EditorGUILayout.Toggle("Preview Only", previewOnly);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Scan and Preview"))
        {
            ScanAndPreview();
        }
        
        if (GUILayout.Button("Apply Assignments"))
        {
            ApplyAssignments();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Clear All Sprites"))
        {
            ClearAllSprites();
        }
    }

    void ScanAndPreview()
    {
        var cityBuilder = FindFirstObjectByType<CityBuilder>();
        if (cityBuilder == null)
        {
            Debug.LogError("No CityBuilder found in scene!");
            return;
        }

        // Get all sprites in the folder
        var spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { spritesFolderPath });
        var sprites = new Dictionary<string, Sprite>();
        
        foreach (var guid in spriteGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                sprites[fileName] = sprite;
            }
        }

        Debug.Log($"Found {sprites.Count} sprites in {spritesFolderPath}");
        
        // Get building types
        var buildingTypes = cityBuilder.GetType().GetField("buildingTypes", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(cityBuilder) as List<CityBuilder.BuildingTypeData>;

        if (buildingTypes == null)
        {
            Debug.LogError("Could not access building types!");
            return;
        }

        int matched = 0;
        int unmatched = 0;

        foreach (var buildingType in buildingTypes)
        {
            Sprite matchingSprite = null;
            
            if (useExactMatch)
            {
                // Try exact match first
                if (sprites.ContainsKey(buildingType.buildingName))
                {
                    matchingSprite = sprites[buildingType.buildingName];
                }
            }
            else
            {
                // Try partial matches
                var buildingNameLower = buildingType.buildingName.ToLower();
                foreach (var kvp in sprites)
                {
                    if (kvp.Key.ToLower().Contains(buildingNameLower) || 
                        buildingNameLower.Contains(kvp.Key.ToLower()))
                    {
                        matchingSprite = kvp.Value;
                        break;
                    }
                }
            }

            if (matchingSprite != null)
            {
                Debug.Log($"✓ {buildingType.buildingName} -> {matchingSprite.name}");
                if (!previewOnly)
                {
                    buildingType.buildingSprite = matchingSprite;
                }
                matched++;
            }
            else
            {
                Debug.LogWarning($"✗ {buildingType.buildingName} -> NO MATCH");
                unmatched++;
            }
        }

        Debug.Log($"\nSummary: {matched} matched, {unmatched} unmatched out of {buildingTypes.Count} building types");

        if (!previewOnly)
        {
            EditorUtility.SetDirty(cityBuilder);
            Debug.Log("Sprite assignments applied!");
        }
    }

    void ApplyAssignments()
    {
        previewOnly = false;
        ScanAndPreview();
    }

    void ClearAllSprites()
    {
        var cityBuilder = FindFirstObjectByType<CityBuilder>();
        if (cityBuilder == null)
        {
            Debug.LogError("No CityBuilder found in scene!");
            return;
        }

        var buildingTypes = cityBuilder.GetType().GetField("buildingTypes", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(cityBuilder) as List<CityBuilder.BuildingTypeData>;

        if (buildingTypes == null)
        {
            Debug.LogError("Could not access building types!");
            return;
        }

        foreach (var buildingType in buildingTypes)
        {
            buildingType.buildingSprite = null;
        }

        EditorUtility.SetDirty(cityBuilder);
        Debug.Log("All sprites cleared!");
    }
} 