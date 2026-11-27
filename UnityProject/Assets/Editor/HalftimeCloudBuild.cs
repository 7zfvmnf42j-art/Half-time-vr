using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class HalftimeCloudBuild
{
    public static void BuildAndroid()
    {
        // Generate the starter match scene before building.
        SetupHalftimeScene.BuildScene();

        EditorUserBuildSettings.SwitchActiveBuildTarget(
            BuildTargetGroup.Android,
            BuildTarget.Android
        );

        PlayerSettings.productName = "HALFTIME VR";
        PlayerSettings.SetApplicationIdentifier(
            BuildTargetGroup.Android,
            "com.halftimevr.game"
        );
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        var outputDirectory = Path.GetFullPath("Build");
        Directory.CreateDirectory(outputDirectory);

        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/HalftimeMatch.unity" },
            locationPathName = Path.Combine(outputDirectory, "HALFTIME-VR.apk"),
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new BuildFailedException(
                "HALFTIME VR Android build failed: " + report.summary.result +
                " (" + report.summary.totalErrors + " errors)"
            );
        }

        Debug.Log("HALFTIME VR Android APK created at: " + options.locationPathName);
    }
}
