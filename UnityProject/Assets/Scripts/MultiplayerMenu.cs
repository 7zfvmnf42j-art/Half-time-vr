using UnityEngine;
using UnityEngine.UI;

/// <summary>Builds the HALFTIME VR online room panel.</summary>
public class MultiplayerMenu : MonoBehaviour
{
    Text status;
    InputField codeInput;
    System.Action<string> report;

    public GameObject Build(Transform parent, System.Action<string> reportStatus)
    {
        report = reportStatus;
        var panel = new GameObject("Multiplayer Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(790, 410);
        rect.anchoredPosition = new Vector2(0, -95);
        panel.GetComponent<Image>().color = new Color(0.025f, 0.045f, 0.09f);

        AddLabel(panel.transform, "ONLINE PLAY", 34, new Vector2(0, 145), new Vector2(700, 55));
        AddLabel(panel.transform, "CREATE A PRIVATE ROOM OR ENTER A FRIEND'S CODE", 17,
            new Vector2(0, 95), new Vector2(700, 40));

        var inputObject = new GameObject("Room Code Input", typeof(RectTransform), typeof(Image), typeof(InputField));
        inputObject.transform.SetParent(panel.transform, false);
        var inputRect = inputObject.GetComponent<RectTransform>();
        inputRect.sizeDelta = new Vector2(570, 58);
        inputRect.anchoredPosition = new Vector2(0, 35);
        inputObject.GetComponent<Image>().color = new Color(0.10f, 0.15f, 0.22f);
        codeInput = inputObject.GetComponent<InputField>();
        var inputText = AddLabel(inputObject.transform, "", 25, Vector2.zero, new Vector2(530, 50));
        inputText.alignment = TextAnchor.MiddleCenter;
        codeInput.textComponent = inputText;
        codeInput.characterLimit = 12;
        codeInput.contentType = InputField.ContentType.Standard;
        var placeholder = AddLabel(inputObject.transform, "ENTER ROOM CODE", 18,
            Vector2.zero, new Vector2(530, 50));
        placeholder.color = new Color(0.55f, 0.60f, 0.68f);
        codeInput.placeholder = placeholder;

        AddButton(panel.transform, "CREATE ROOM", new Vector2(-170, -45), () =>
        {
            var session = GetSession();
            if (session == null) { report("Online service isn't ready."); return; }
            session.CreateRoom();
        });
        AddButton(panel.transform, "JOIN ROOM", new Vector2(170, -45), () =>
        {
            var session = GetSession();
            if (session == null) { report("Online service isn't ready."); return; }
            session.JoinRoom(codeInput.text);
        });
        AddButton(panel.transform, "DISCONNECT", new Vector2(0, -125), () =>
        {
            var session = GetSession();
            if (session != null) session.Disconnect();
            else report("No active room.");
        });

        AddLabel(panel.transform, "ROOM CODES CONNECT PLAYERS OVER THE INTERNET", 16,
            new Vector2(0, -180), new Vector2(720, 38));
        return panel;
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