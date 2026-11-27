using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class BallControl : MonoBehaviour
{
    [Header("Kick feel")]
    public float kickForce = 7.5f;
    public float lift = 0.16f;
    public float maxSpeed = 32f;
    public float spinImpulse = 0.12f;
    public float kickReach = 1.55f;

    Rigidbody body;
    InputDevice rightController;
    InputDevice leftController;
    bool rightTriggerWasPressed;
    bool leftTriggerWasPressed;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        body.maxAngularVelocity = 35f;
        RefreshControllers();
    }

    void RefreshControllers()
    {
        rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
    }

    void Update()
    {
        if (!rightController.isValid || !leftController.isValid)
            RefreshControllers();

        TryKick(rightController, ref rightTriggerWasPressed);
        TryKick(leftController, ref leftTriggerWasPressed);
    }

    void TryKick(InputDevice controller, ref bool wasPressed)
    {
        if (!controller.isValid)
        {
            wasPressed = false;
            return;
        }

        if (!controller.TryGetFeatureValue(CommonUsages.triggerButton, out bool pressed))
        {
            wasPressed = false;
            return;
        }

        if (pressed && !wasPressed)
        {
            Vector3 handPosition;
            Quaternion handRotation;
            bool hasPosition = controller.TryGetFeatureValue(CommonUsages.devicePosition, out handPosition);
            bool hasRotation = controller.TryGetFeatureValue(CommonUsages.deviceRotation, out handRotation);
            Transform trackingSpace = Camera.main != null ? Camera.main.transform.parent : null;
            if (hasPosition && trackingSpace != null)
                handPosition = trackingSpace.TransformPoint(handPosition);
            if (hasRotation && trackingSpace != null)
                handRotation = trackingSpace.rotation * handRotation;

            if (hasPosition && Vector3.Distance(handPosition, transform.position) <= kickReach)
            {
                Vector3 direction = hasRotation ? handRotation * Vector3.forward :
                    (Camera.main != null ? Camera.main.transform.forward : Vector3.forward);
                direction.y = Mathf.Max(lift, direction.y);
                body.AddForce(direction.normalized * kickForce, ForceMode.Impulse);
                body.AddTorque(Vector3.Cross(Vector3.up, direction.normalized) * spinImpulse, ForceMode.Impulse);
            }
        }

        wasPressed = pressed;
    }

    void FixedUpdate()
    {
        // A little air resistance and a firm speed ceiling keep the ball controllable.
        Vector3 velocity = body.velocity;
        if (velocity.sqrMagnitude > maxSpeed * maxSpeed)
            body.velocity = velocity.normalized * maxSpeed;
    }
}
