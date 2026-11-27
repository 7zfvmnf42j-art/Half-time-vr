using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerMenu : MonoBehaviour
{
    InputField codeInput;
    Transform roomListRoot;
    System.Action<string> report;

    public GameObject Build(Transform parent, System.Action<string> reportStatus)
    {
        report = reportStatus;
        var panel = new GameObject("Multiplayer Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(790, 500);
        rect.anchoredPosition = new Vector2(0, -65);
        panel.GetComponent<Image>().color = new Color(0.025f, 0.045f, 0.09f);

        AddLabel(panel.transform, "PUBLIC LOBBIES", 34, new Vector2(0, 205), new Vector2(700, 55));
        AddLabel(panel.transform, "CREATE A PUBLIC MATCH OR JOIN ONE THAT IS OPEN", 17,
            new Vector2(0, 160), new Vector2(700, 35));

        var inputObject = new GameObject("Room Code Input", typeof(RectTransform), typeof(Image), typeof(InputField));
        inputObject.transform.SetParent(panel.transform, false);
        var inputRect = inputObject.GetComponent<RectTransform>();
        inputRect.sizeDelta = new Vector2(400, 52);
        inputRect.anchoredPosition = new Vector2(0, 105);
        inputObject.GetComponent<Image>().color = new Color(0.10f, 0.15f, 0.22f);
        codeInput = inputObject.GetComponent<InputField>();
        var inputText = AddLabel(inputObject.transform, "", 22, Vector2.zero, new Vector2(370, 45));
        codeInput.textComponent = inputText;
        codeInput.characterLimit = 12;
        var placeholder = AddLabel(inputObject.transform, "OPTIONAL ROOM CODE", 17, Vector2.zero, new Vector2(370, 45));
        placeholder.color = new Color(0.55f, 0.60f, 0.68f);
        codeInput.placeholder = placeholder;

        AddButton(panel.transform, "CREATE PUBLIC", new Vector2(-190, 45), new Vector2(250, 58), () => GetSession()?.CreateRoom());
        AddButton(panel.transform, "JOIN CODE", new Vector2(190, 45), new Vector2(250, 58), () => GetSession()?.JoinRoom(codeInput.text));
        AddButton(panel.transform, "REFRESH LIST", new Vector2(0, -25), new Vector2(250, 54), RefreshRooms);
        AddButton(panel.transform, "DISCONNECT", new Vector2(0, -205), new Vector2(250, 50), () => GetSession()?.Disconnect());

        AddLabel(panel.transform, "OPEN ROOMS", 20, new Vector2(0, -75), new Vector2(700, 32));
        var list = new GameObject("Public Room List", typeof(RectTransform));
        list.transform.SetParent(panel.transform, false);
        var listRect = list.GetComponent<RectTransform>();
        listRect.sizeDelta = new Vector2(700, 115);
        listRect.anchoredPosition = new Vector2(0, -140);
        roomListRoot = list.transform;
        RefreshRooms();
        return panel;
    }

    void RefreshRooms()
    {
        var session = GetSession();
        if (session == null) { report?.Invoke("Online service isn't ready."); return; }
        report?.Invoke("Refreshing public rooms...");
        session.RefreshPublicRooms(DisplayRooms);
    }

    void DisplayRooms(List<LobbySummary> rooms)
    {
        if (roomListRoot == null) return;
        for (int i = roomListRoot.childCount - 1; i >= 0; i--)
            Destroy(roomListRoot.GetChild(i).gameObject);

        if (rooms == null || rooms.Count == 0)
        {
            AddLabel(roomListRoot, "NO OPEN ROOMS — CREATE ONE TO GET STARTED", 17,
                Vector2.zero, new Vector2(680, 42));
            return;
        }

        int shown = Mathf.Min(3, rooms.Count);
        for (int i = 0; i < shown; i++)
        {
            LobbySummary room = rooms[i];
            string roomId = room.Id;
            string label = room.Name + "   " + room.Players + "/" + room.Capacity + " PLAYERS";
            AddButton(roomListRoot, label, new Vector2(0, 36 - i * 42), new Vector2(660, 38),
                () => GetSession()?.JoinPublicRoom(roomId));
        }
    }

    MultiplayerSession GetSession()
    {
        if (MultiplayerSession.Instance != null) return MultiplayerSession.Instance;
        return new GameObject("HALFTIME Multiplayer Session").AddComponent<MultiplayerSession>();
    }

    Text AddLabel(Transform parent, string value, int size, Vector2 position, Vector2 dimensions)
    {
        var go = new GameObject("Text - " + value, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = dimensions;
        rect.anchoredPosition = position;
        var text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = value;
        text.fontSize = size;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        return text;
    }

    void AddButton(Transform parent, string label, Vector2 position, Vector2 dimensions, UnityEngine.Events.UnityAction action)
    {
        var go = new GameObject("Button - " + label, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = dimensions;
        rect.anchoredPosition = position;
        go.GetComponent<Image>().color = new Color(0.10f, 0.15f, 0.22f);
        go.GetComponent<Button>().onClick.AddListener(action);
        var text = AddLabel(go.transform, label, 18, Vector2.zero, dimensions - new Vector2(18, 8));
        text.fontStyle = FontStyle.Bold;
    }
}