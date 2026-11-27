using UnityEngine;
using UnityEngine.XR;

// Native Quest controls adapted from HALFTIME's pass / shoot / lob gameplay.
[RequireComponent(typeof(Rigidbody))]
public class BallControl : MonoBehaviour
{
    [Header("Kick feel")]
    public float passForce = 3.5f;
    public float shotForce = 8f;
    public float lobForce = 6f;
    public float maxSpeed = 32f;
    public float spinImpulse = 0.12f;
    public float kickReach = 1.25f;
    public float passLift = 0.22f;
    public float lobLift = 6.2f;

    Rigidbody body;
    InputDevice rightController;
    InputDevice leftController;
    bool rightTriggerWasPressed, leftTriggerWasPressed;
    bool rightPrimaryWasPressed, leftPrimaryWasPressed;
    bool rightSecondaryWasPressed, leftSecondaryWasPressed;

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

        // Quest trigger = shoot, A/X = pass, B/Y = lob.
        ReadKickButtons(rightController, ref rightTriggerWasPressed,
            ref rightPrimaryWasPressed, ref rightSecondaryWasPressed);
        ReadKickButtons(leftController, ref leftTriggerWasPressed,
            ref leftPrimaryWasPressed, ref leftSecondaryWasPressed);
    }

    void ReadKickButtons(InputDevice controller, ref bool triggerWasPressed,
        ref bool primaryWasPressed, ref bool secondaryWasPressed)
    {
        if (!controller.isValid)
        {
            triggerWasPressed = primaryWasPressed = secondaryWasPressed = false;
            return;
        }

        ReadButton(controller, CommonUsages.triggerButton, ref triggerWasPressed, shotForce, 0.55f);
        ReadButton(controller, CommonUsages.primaryButton, ref primaryWasPressed, passForce, passLift);
        ReadButton(controller, CommonUsages.secondaryButton, ref secondaryWasPressed, lobForce, lobLift);
    }

    void ReadButton(InputDevice controller, InputFeatureUsage<bool> usage,
        ref bool wasPressed, float power, float verticalLift)
    {
        if (!controller.TryGetFeatureValue(usage, out bool pressed))
        {
            wasPressed = false;
            return;
        }

        if (pressed && !wasPressed)
            TryKick(controller, power, verticalLift);

        wasPressed = pressed;
    }

    void TryKick(InputDevice controller, float power, float verticalLift)
    {
        if (MatchRules.Instance != null && MatchRules.Instance.IsGoalPending) return;

        if (!controller.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 localPosition))
            return;

        Transform trackingSpace = Camera.main != null ? Camera.main.transform.parent : null;
        Vector3 worldPosition = trackingSpace != null
            ? trackingSpace.TransformPoint(localPosition)
            : localPosition;

        if (Vector3.Distance(worldPosition, transform.position) > kickReach)
            return;

        Vector3 direction;
        if (controller.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion localRotation))
        {
            Quaternion worldRotation = trackingSpace != null
                ? trackingSpace.rotation * localRotation
                : localRotation;
            direction = worldRotation * Vector3.forward;
        }
        else
        {
            direction = Camera.main != null ? Camera.main.transform.forward : Vector3.forward;
        }

        direction.y = Mathf.Max(direction.y, verticalLift / Mathf.Max(power, 0.01f));
        direction.Normalize();
        body.AddForce(direction * power, ForceMode.Impulse);
        body.AddTorque(Vector3.Cross(Vector3.up, direction) * spinImpulse, ForceMode.Impulse);
    }

    void FixedUpdate()
    {
        Vector3 velocity = body.velocity;
        if (velocity.sqrMagnitude > maxSpeed * maxSpeed)
            body.velocity = velocity.normalized * maxSpeed;
    }
}
