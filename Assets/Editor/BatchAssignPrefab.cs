using UnityEngine;
using UnityEditor;
using LifeCraft.Core;

public class BatchAssignPrefab : EditorWindow
{
    private GameObject prefabToAssign;

    [MenuItem("Tools/Batch Assign Building Prefab")]
    public static void ShowWindow()
    {
        GetWindow<BatchAssignPrefab>("Batch Assign Building Prefab");
    }

    void OnGUI()
    {
        GUILayout.Label("Assign Prefab to All Building Types", EditorStyles.boldLabel);
        prefabToAssign = (GameObject)EditorGUILayout.ObjectField("Prefab", prefabToAssign, typeof(GameObject), false);

        if (GUILayout.Button("Assign to All"))
        {
            var cityBuilder = FindFirstObjectByType<CityBuilder>();
            if (cityBuilder == null)
            {
                Debug.LogError("No CityBuilder found in scene!");
                return;
            }
            var types = cityBuilder.GetType().GetField("buildingTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(cityBuilder) as System.Collections.IList;
            foreach (var type in types)
            {
                var field = type.GetType().GetField("buildingPrefab");
                field.SetValue(type, prefabToAssign);
            }
            EditorUtility.SetDirty(cityBuilder);
            Debug.Log("Assigned prefab to all building types!");
        }
    }
}