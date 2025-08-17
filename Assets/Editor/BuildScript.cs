using UnityEditor;
using UnityEngine;

public class BuildScript
{
    [MenuItem("Build/Build iOS")]
    public static void BuildiOS()
    {
        // Set the build target to iOS
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
        
        // Configure build settings
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.sparqcapital.selfcity");
        PlayerSettings.productName = "SelfCity";
        PlayerSettings.companyName = "Sparq Capital";
        
        // Set iOS specific settings
        PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        PlayerSettings.iOS.appleDeveloperTeamID = ""; // Will be set by cloud build
        
        // Build the project
        string[] scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }
        
        string buildPath = "build/ios";
        BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.iOS, BuildOptions.None);
        
        Debug.Log("iOS build completed at: " + buildPath);
    }
    
    [MenuItem("Build/Build Android")]
    public static void BuildAndroid()
    {
        // Set the build target to Android
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        
        // Configure build settings
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.sparqcapital.selfcity");
        PlayerSettings.productName = "SelfCity";
        PlayerSettings.companyName = "Sparq Capital";
        
        // Build the project
        string[] scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }
        
        string buildPath = "build/android/SelfCity.apk";
        BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.Android, BuildOptions.None);
        
        Debug.Log("Android build completed at: " + buildPath);
    }
} 