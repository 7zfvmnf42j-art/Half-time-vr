using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HalftimeMainMenu : MonoBehaviour
{
    static readonly Color Navy = new Color(0.025f, 0.045f, 0.09f);
    static readonly Color Panel = new Color(0.055f, 0.085f, 0.14f);
    static readonly Color Lime = new Color(0.72f, 0.94f, 0.25f);
    static readonly Color Muted = new Color(0.62f, 0.69f, 0.78f);
    Canvas canvas;
    Text pageTitle;
    Text pageBody;
    Text status;
    GameObject contentPanel;
    GameObject lockerBadgePanel;
    GameObject multiplayerPanel;
    GameObject storePanel;
    Text badgePreview;
    string selectedBadge = "CC CREATOR";

    void Start()
    {
        Build();
    }

    void Build()
    {
        var canvasObject = new GameObject("HALFTIME Main Menu Canvas");
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        canvasObject.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 12;
        canvasObject.AddComponent<GraphicRaycaster>();
        canvasObject.transform.position = new Vector3(0, 1.65f, -1.8f);
        canvasObject.transform.rotation = Quaternion.identity;
        canvasObject.transform.localScale = Vector3.one * 0.0022f;
        canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1500, 900);

        var root = PanelObject("Main Panel", canvas.transform, Navy, new Vector2(1500,900), Vector2.zero);
        var header = PanelObject("Header", root.transform, Panel, new Vector2(1450,130), new Vector2(0,365));
        Label(header.transform, "HALFTIME", 58, Lime, new Vector2(-490,12), new Vector2(420,85), TextAnchor.MiddleLeft, FontStyle.Bold);
        Label(header.transform, "VR", 38, Color.white, new Vector2(-250,12), new Vector2(90,80), TextAnchor.MiddleLeft, FontStyle.Bold);
        Label(header.transform, "THE BEAUTIFUL GAME. IN YOUR HANDS.", 20, Muted, new Vector2(310,8), new Vector2(520,60), TextAnchor.MiddleRight, FontStyle.Normal);

        var nav = PanelObject("Navigation", root.transform, Panel, new Vector2(300,680), new Vector2(-565,-45));
        string[] names = {"PLAY", "TRAINING", "CAREER", "CLUBS", "STORE", "MULTIPLAYER", "LOCKER ROOM", "SETTINGS"};
        for (int i=0;i<names.Length;i++)
        {
            string key = names[i];
            MakeButton(nav.transform, key, new Vector2(0, 270-i*85), new Vector2(260,72), () => ShowPage(key));
        }

        contentPanel = PanelObject("Content", root.transform, Panel, new Vector2(850,680), new Vector2(145,-45));
        Label(contentPanel.transform, "WELCOME TO THE PITCH", 38, Lime, new Vector2(0,245), new Vector2(760,70), TextAnchor.MiddleLeft, FontStyle.Bold);
        pageTitle = Label(contentPanel.transform, "MATCH DAY", 54, Color.white, new Vector2(0,125), new Vector2(760,100), TextAnchor.MiddleLeft, FontStyle.Bold);
        pageBody = Label(contentPanel.transform, "Choose your mode and get ready to play.", 26, Muted, new Vector2(0,35), new Vector2(760,110), TextAnchor.MiddleLeft, FontStyle.Normal);
        MakeButton(contentPanel.transform, "KICK OFF", new Vector2(-205,-105), new Vector2(340,88), StartMatch);
        MakeButton(contentPanel.transform, "PRACTICE", new Vector2(205,-105), new Vector2(340,88), StartMatch);
        status = Label(contentPanel.transform, "HALFTIME VR  •  PRE-SEASON BUILD", 18, Muted, new Vector2(0,-260), new Vector2(760,45), TextAnchor.MiddleCenter, FontStyle.Normal);

        BuildLockerBadges();
        var storeUI = contentPanel.AddComponent<LockerStore>();
        storePanel = storeUI.Build(contentPanel.transform, SetStatus);
        var multiplayerUI = contentPanel.AddComponent<MultiplayerMenu>();
        multiplayerPanel = multiplayerUI.Build(contentPanel.transform, SetStatus);
        multiplayerPanel.SetActive(false);

        var footer = PanelObject("Footer", root.transform, Navy, new Vector2(1450,48), new Vector2(0,-420));
        Label(footer.transform, "HOME   /   MATCH CENTRE", 17, Muted, new Vector2(-520,0), new Vector2(500,40), TextAnchor.MiddleLeft, FontStyle.Normal);
        Label(footer.transform, "QUEST EDITION", 17, Lime, new Vector2(520,0), new Vector2(400,40), TextAnchor.MiddleRight, FontStyle.Bold);
        if (FindObjectOfType<EventSystem>() == null) new GameObject("EventSystem", typeof(EventSystem));
    }

    void StartMatch()
    {
        if (MatchRules.Instance != null)
            MatchRules.Instance.BeginMatch();
        if (canvas != null) canvas.gameObject.SetActive(false);
        SetStatus("Match started. Trigger to shoot, A/X to pass, B/Y to lob near the ball.");
    }

    void ShowPage(string page)
    {
        pageTitle.text = page == "PLAY" ? "MATCH DAY" : page;
        if (lockerBadgePanel != null) lockerBadgePanel.SetActive(page == "LOCKER ROOM");
        if (multiplayerPanel != null) multiplayerPanel.SetActive(page == "MULTIPLAYER");
        if (storePanel != null) storePanel.SetActive(page == "STORE");
        switch (page)
        {
            case "PLAY": pageBody.text = "Choose a match and take to the pitch. Full match setup is coming online."; break;
            case "TRAINING": pageBody.text = "Work on your touch, passing and shooting before match day."; break;
            case "CAREER": pageBody.text = "Build your player, develop your club and chase silverware."; break;
            case "CLUBS": pageBody.text = "Create your club identity and prepare your squad."; break;
            case "LOCKER ROOM": pageBody.text = "Make your player yours with boots, kits and accessories."; break;
            case "STORE": pageBody.text = "Try kits, boots and accessories on your player before buying. Take a selfie in your new look."; break;
            case "MULTIPLAYER": pageBody.text = "Connect with friends. Online rooms and full match syncing are being built."; break;
            default: pageBody.text = "Adjust your game preferences and comfort settings."; break;
        }
        SetStatus(page + " selected");
    }

    void BuildLockerBadges()
    {
        lockerBadgePanel = PanelObject("Locker Badge Collection", contentPanel.transform, Navy,
            new Vector2(810, 450), new Vector2(0, -95));
        badgePreview = Label(lockerBadgePanel.transform, "", 30, Lime,
            new Vector2(0, 145), new Vector2(720, 65), TextAnchor.MiddleCenter, FontStyle.Bold);

        string[] badges = {
            "CC CREATOR", "PRO", "CAPTAIN",
            "LEGEND", "DEV TESTER"
        };

        for (int i = 0; i < badges.Length; i++)
        {
            string badge = badges[i];
            int column = i % 3;
            int row = i / 3;
            Vector2 position = new Vector2(-235 + column * 235, 45 - row * 105);
            MakeButton(lockerBadgePanel.transform, badge,
                position, new Vector2(210, 76), () => SelectBadge(badge));
        }

        var ownerCard = PanelObject("Reserved Owner Badge", lockerBadgePanel.transform,
            new Color(0.12f, 0.17f, 0.24f), new Vector2(690, 62), new Vector2(0, -175));
        Label(ownerCard.transform, "★  OWNER BADGE  ★   RESERVED FOR THE GAME OWNER",
            21, Lime, Vector2.zero, new Vector2(660, 52), TextAnchor.MiddleCenter, FontStyle.Bold);

        SelectBadge(selectedBadge);
        lockerBadgePanel.SetActive(false);
    }

    void SelectBadge(string badge)
    {
        selectedBadge = badge;
        if (badgePreview != null)
            badgePreview.text = "★  " + badge + " BADGE  ★";
        SetStatus(badge + " badge selected (cosmetic preview)");
    }

    void SetStatus(string message)
    {
        if (status != null) status.text = message;
    }

    GameObject PanelObject(string label, Transform parent, Color colour, Vector2 size, Vector2 position)
    {
        var go = new GameObject(label, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        go.GetComponent<Image>().color = colour;
        return go;
    }

    Text Label(Transform parent, string value, int size, Color colour, Vector2 position, Vector2 dimensions, TextAnchor alignment, FontStyle style)
    {
        var go = new GameObject("Text - " + value, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = dimensions;
        rect.anchoredPosition = position;
        var text = go.GetComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.color = colour;
        text.alignment = alignment;
        text.fontStyle = style;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    void MakeButton(Transform parent, string value, Vector2 position, Vector2 dimensions, UnityEngine.Events.UnityAction action)
    {
        var go = PanelObject("Button - " + value, parent, new Color(0.10f,0.15f,0.22f), dimensions, position);
        var button = go.AddComponent<Button>();
        var colours = button.colors;
        colours.normalColor = new Color(0.10f,0.15f,0.22f);
        colours.highlightedColor = new Color(0.20f,0.28f,0.34f);
        colours.pressedColor = Lime;
        colours.selectedColor = new Color(0.20f,0.28f,0.34f);
        button.colors = colours;
        button.onClick.AddListener(action);
        Label(go.transform, value, 25, Color.white, Vector2.zero, dimensions - new Vector2(24,12), TextAnchor.MiddleCenter, FontStyle.Bold);
    }
}
