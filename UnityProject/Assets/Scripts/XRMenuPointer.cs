using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using UnityEngine.UI;

/// <summary>
/// Simple headset-gaze UI pointer for Quest. Aim at a menu button and press the
/// right controller trigger (or primary button) to activate it.
/// </summary>
public class XRMenuPointer : MonoBehaviour
{
    EventSystem eventSystem;
    GraphicRaycaster raycaster;
    PointerEventData pointer;
    readonly List<RaycastResult> hits = new List<RaycastResult>();
    GameObject hovered;
    GameObject pressed;
    InputDevice rightHand;
    bool wasPressed;

    void Start()
    {
        eventSystem = EventSystem.current;
        raycaster = FindObjectOfType<GraphicRaycaster>();
        if (eventSystem != null) pointer = new PointerEventData(eventSystem);
        RefreshDevice();
    }

    void Update()
    {
        if (eventSystem == null) eventSystem = EventSystem.current;
        if (raycaster == null) raycaster = FindObjectOfType<GraphicRaycaster>();
        if (pointer == null && eventSystem != null) pointer = new PointerEventData(eventSystem);
        if (eventSystem == null || raycaster == null || pointer == null) return;

        pointer.Reset();
        pointer.position = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        hits.Clear();
        raycaster.Raycast(pointer, hits);
        GameObject target = hits.Count > 0 ? hits[0].gameObject : null;
        while (target != null && ExecuteEvents.GetEventHandler<IPointerClickHandler>(target) == null)
            target = target.transform.parent != null ? target.transform.parent.gameObject : null;

        if (target != hovered)
        {
            if (hovered != null) ExecuteEvents.Execute(hovered, pointer, ExecuteEvents.pointerExitHandler);
            hovered = target;
            if (hovered != null) ExecuteEvents.Execute(hovered, pointer, ExecuteEvents.pointerEnterHandler);
        }

        if (!rightHand.isValid) RefreshDevice();
        bool isPressed = false;
        if (rightHand.isValid)
        {
            if (!rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out isPressed))
                rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out isPressed);
        }

        if (isPressed && !wasPressed && hovered != null)
        {
            pressed = hovered;
            ExecuteEvents.Execute(pressed, pointer, ExecuteEvents.pointerDownHandler);
        }
        if (!isPressed && wasPressed && pressed != null)
        {
            ExecuteEvents.Execute(pressed, pointer, ExecuteEvents.pointerUpHandler);
            if (pressed == hovered) ExecuteEvents.Execute(pressed, pointer, ExecuteEvents.pointerClickHandler);
            pressed = null;
        }
        wasPressed = isPressed;
    }

    void RefreshDevice()
    {
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }
}
