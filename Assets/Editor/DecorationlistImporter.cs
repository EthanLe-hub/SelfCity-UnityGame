using UnityEngine;
using UnityEditor;
using LifeCraft.Shop; // DecorationDatabase class is in this namespace. 

public class DecorationListImporter : EditorWindow
{
    private DecorationDatabase database;
    private string decorationsText;
    private bool isPremiumList = false;

    [MenuItem("Tools/Decoration List Importer")]
    public static void ShowWindow()
    {
        GetWindow<DecorationListImporter>("Decoration List Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Bulk Import Decorations", EditorStyles.boldLabel);

        database = (DecorationDatabase)EditorGUILayout.ObjectField("Decoration Database", database, typeof(DecorationDatabase), false);
        isPremiumList = EditorGUILayout.Toggle("Import to Premium Only List", isPremiumList);

        GUILayout.Label("Paste decorations (one per line):");
        decorationsText = EditorGUILayout.TextArea(decorationsText, GUILayout.Height(100));

        if (GUILayout.Button("Import"))
        {
            if (database != null && !string.IsNullOrEmpty(decorationsText))
            {
                var lines = decorationsText.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (isPremiumList)
                    database.premiumOnlyDecorations.AddRange(lines);
                else
                    database.freeAndPremiumDecorations.AddRange(lines);

                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();
                Debug.Log("Decorations imported!");
            }
        }
    }
}