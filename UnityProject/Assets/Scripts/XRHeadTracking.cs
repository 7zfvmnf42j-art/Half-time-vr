using UnityEngine;
using UnityEngine.XR;

/// <summary>Reads the headset pose directly from the XR runtime.</summary>
public class XRHeadTracking : MonoBehaviour
{
    InputDevice head;

    void OnEnable()
    {
        RefreshHead();
    }

    void Update()
    {
        if (!head.isValid) RefreshHead();
        if (!head.isValid) return;

        if (head.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position))
            transform.localPosition = position;
        if (head.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation))
            transform.localRotation = rotation;
    }

    void RefreshHead()
    {
        head = InputDevices.GetDeviceAtXRNode(XRNode.Head);
    }
}
