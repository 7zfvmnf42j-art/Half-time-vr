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

    Rigidbody body;
    InputDevice rightController;
    bool previousTrigger;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        body.maxAngularVelocity = 35f;
        RefreshController();
    }

    void RefreshController()
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
        if (devices.Count > 0) rightController = devices[0];
    }

    void Update()
    {
        if (!rightController.isValid) RefreshController();
        if (rightController.TryGetFeatureValue(CommonUsages.triggerButton, out bool pressed))
        {
            if (pressed && !previousTrigger && Camera.main != null &&
                Vector3.Distance(Camera.main.transform.position, transform.position) < 2.1f)
            {
                Vector3 direction = Camera.main.transform.forward;
                direction.y = Mathf.Max(lift, direction.y);
                body.AddForce(direction.normalized * kickForce, ForceMode.Impulse);
                body.AddTorque(Camera.main.transform.right * spinImpulse, ForceMode.Impulse);
            }
            previousTrigger = pressed;
        }
    }

    void FixedUpdate()
    {
        // A little air resistance and a firm speed ceiling keep the ball controllable.
        Vector3 velocity = body.velocity;
        if (velocity.sqrMagnitude > maxSpeed * maxSpeed)
            body.velocity = velocity.normalized * maxSpeed;
    }
}