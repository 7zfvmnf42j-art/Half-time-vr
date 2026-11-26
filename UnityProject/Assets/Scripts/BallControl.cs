using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(Rigidbody))]
public class BallControl : MonoBehaviour
{
    public float kickForce = 7.5f;
    Rigidbody body;
    InputDevice rightController;
    bool previousTrigger;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        var devices = new System.Collections.Generic.List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
        if (devices.Count > 0) rightController = devices[0];
    }

    void Update()
    {
        if (!rightController.isValid)
        {
            var devices = new System.Collections.Generic.List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
            if (devices.Count > 0) rightController = devices[0];
        }
        if (rightController.TryGetFeatureValue(CommonUsages.triggerButton, out bool pressed))
        {
            if (pressed && !previousTrigger && Camera.main != null)
            {
                Vector3 origin = Camera.main.transform.position;
                if (Vector3.Distance(origin, transform.position) < 2.4f)
                {
                    Vector3 direction = Camera.main.transform.forward;
                    direction.y = Mathf.Max(0.12f, direction.y);
                    body.AddForce(direction.normalized * kickForce, ForceMode.Impulse);
                }
            }
            previousTrigger = pressed;
        }
    }
}
