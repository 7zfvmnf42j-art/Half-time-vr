using UnityEngine;
using UnityEngine.UI;

/// <summary>Builds the first host/join panel for the HALFTIME VR menu.</summary>
public class MultiplayerMenu : MonoBehaviour
{
    Text status;
    InputField addressInput;

    public void Build(Transform parent, System.Action<string> report)
    {
        var panel = new GameObject("Multiplayer Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(790, 410);
        rect.anchoredPosition = new Vector2(0, -95);
        panel.GetComponent<Image>().color = new Color(0.025f, 0.045f, 0.09f);

        AddLabel(panel.transform, "ONLINE PLAY", 34, new Vector2(0, 145), new Vector2(700, 55));
        AddLabel(panel.transform, "HOST A SESSION OR JOIN A FRIEND ON THE SAME NETWORK", 17,
            new Vector2(0, 95), new Vector2(700, 40));

        var inputObject = new GameObject("Host Address", typeof(RectTransform), typeof(Image), typeof(InputField));
        inputObject.transform.SetParent(panel.transform, false);
        var inputRect = inputObject.GetComponent<RectTransform>();
        inputRect.sizeDelta = new Vector2(570, 58);
        inputRect.anchoredPosition = new Vector2(0, 35);
        inputObject.GetComponent<Image>().color = new Color(0.10f, 0.15f, 0.22f);
        addressInput = inputObject.GetComponent<InputField>();
        var text = AddLabel(inputObject.transform, "127.0.0.1", 22, Vector2.zero, new Vector2(530, 50));
        text.alignment = TextAnchor.MiddleLeft;
        addressInput.textComponent = text;
        addressInput.text = "127.0.0.1";
        var placeholder = AddLabel(inputObject.transform, "Friend's local IP address", 18,
            Vector2.zero, new Vector2(530, 50));
        placeholder.color = new Color(0.55f, 0.60f, 0.68f);
        addressInput.placeholder = placeholder;

        AddButton(panel.transform, "HOST", new Vector2(-170, -45), () =>
        {
            var session = GetSession();
            bool started = session != null && session.StartHosting();
            report(started ? "Host started. Share your local IP with a friend on the same network." :
                "Could not start host. Check whether a session is already running.");
        });
        AddButton(panel.transform, "JOIN", new Vector2(170, -45), () =>
        {
            var session = GetSession();
            bool started = session != null && session.JoinHost(addressInput.text);
            report(started ? "Connecting to host..." : "Could not start connection. Check the address and session.");
        });
        AddButton(panel.transform, "DISCONNECT", new Vector2(0, -125), () =>
        {
            var session = GetSession();
            if (session != null) session.Disconnect();
            report("Disconnected from the session.");
        });

        status = AddLabel(panel.transform,
            "First networking test: direct local-network connection only.", 16,
            new Vector2(0, -180), new Vector2(720, 38));
    }

    MultiplayerSession GetSession()
    {
        var existing = MultiplayerSession.Instance;
        if (existing != null) return existing;
        var go = new GameObject("HALFTIME Multiplayer Session");
        return go.AddComponent<MultiplayerSession>();
    }

    Text AddLabel(Transform parent, string value, int size, Vector2 position, Vector2 dimensions)
    {
        var go = new GameObject("Text - " + value, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = dimensions;
        rect.anchoredPosition = position;
        var text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = value;
        text.fontSize = size;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        return text;
    }

    void AddButton(Transform parent, string label, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        var go = new GameObject("Button - " + label, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(230, 62);
        rect.anchoredPosition = position;
        go.GetComponent<Image>().color = new Color(0.10f, 0.15f, 0.22f);
        go.GetComponent<Button>().onClick.AddListener(action);
        AddLabel(go.transform, label, 22, Vector2.zero, new Vector2(210, 52));
    }
}
