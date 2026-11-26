using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem.XR;

public static class SetupHalftimeScene
{
    [MenuItem("HALFTIME VR/Build Starter Match Scene")]
    public static void BuildScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var rig = new GameObject("XR Player");
        var origin = rig.AddComponent<XROrigin>();
        var offset = new GameObject("Camera Offset");
        offset.transform.SetParent(rig.transform, false);
        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(offset.transform, false);
        var camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<TrackedPoseDriver>();
        origin.Camera = camera;
        origin.CameraFloorOffsetObject = offset;
        origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
        rig.transform.position = new Vector3(0, 0, -5);

        var light = new GameObject("Stadium Lighting");
        var sun = light.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 1.15f;
        light.transform.rotation = Quaternion.Euler(50, -30, 0);

        var match = new GameObject("Match Bootstrap");
        match.AddComponent<MatchBootstrap>();
        match.AddComponent<HalftimeMainMenu>();

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/HalftimeMatch.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/HalftimeMatch.unity", true) };
        Debug.Log("HALFTIME VR starter match scene created. Open Assets/Scenes/HalftimeMatch.unity.");
    }
}
