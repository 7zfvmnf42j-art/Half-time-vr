using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class HalftimeCloudBuild
{
    public static void BuildAndroid()
    {
        // Select Android before creating the scene or invoking the build pipeline.
        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(
            BuildTargetGroup.Android,
            BuildTarget.Android
        ))
        {
            throw new BuildFailedException("Could not switch Unity to the Android build target.");
        }

        // Generate the starter match scene in the Unity project.
        SetupHalftimeScene.BuildScene();

        PlayerSettings.productName = "HALFTIME VR";
        PlayerSettings.SetApplicationIdentifier(
            BuildTargetGroup.Android,
            "com.halftimevr.game"
        );
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string outputDirectory = Path.Combine(projectRoot, "Build");
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
