using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using LifeCraft.Core;

[CustomEditor(typeof(CityBuilder))]
public class CityBuilderAutoPopulateEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CityBuilder cityBuilder = (CityBuilder)target;

        GUILayout.Space(10);
        GUILayout.Label("Auto-Populate Building Types", EditorStyles.boldLabel);

        if (GUILayout.Button("Populate from Name List (.txt)"))
        {
            string path = EditorUtility.OpenFilePanel("Select Name List", Application.dataPath, "txt");
            if (!string.IsNullOrEmpty(path))
            {
                var lines = File.ReadAllLines(path);
                var newTypes = new List<CityBuilder.BuildingTypeData>();
                foreach (var line in lines)
                {
                    string name = line.Trim();
                    if (!string.IsNullOrEmpty(name))
                    {
                        var data = new CityBuilder.BuildingTypeData
                        {
                            buildingName = name,
                            buildingPrefab = null, // Assign your PlacedItemPrefab manually after!
                            costResource = 0,      // Default, can edit later
                            costAmount = 0,
                            buildingSprite = null  // Assign later when you have sprites
                        };
                        newTypes.Add(data);
                    }
                }
                // Assign to CityBuilder
                Undo.RecordObject(cityBuilder, "Auto-populate Building Types");
                cityBuilder.GetType().GetField("buildingTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(cityBuilder, newTypes);
                EditorUtility.SetDirty(cityBuilder);
                Debug.Log($"Populated {newTypes.Count} building types from list!");
            }
        }
    }
} 