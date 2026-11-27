using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LockerStore : MonoBehaviour
{
    static readonly Color Navy = new Color(0.025f, 0.045f, 0.09f);
    static readonly Color Panel = new Color(0.055f, 0.085f, 0.14f);
    static readonly Color Lime = new Color(0.72f, 0.94f, 0.25f);
    static readonly Color Muted = new Color(0.62f, 0.69f, 0.78f);

    GameObject root;
    Text itemName;
    Text itemDescription;
    Image shirt;
    Image shorts;
    Image boots;
    Image cap;
    Action<string> status;
    string selectedItem = "Claret home shirt";
    Color shirtColour = new Color(0.42f, 0.055f, 0.13f);
    Color shortsColour = new Color(0.08f, 0.19f, 0.34f);
    Color bootsColour = Color.white;
    bool capVisible;

    public GameObject Build(Transform parent, Action<string> onStatus)
    {
        status = onStatus;
        root = Panel("Store Preview", parent, Navy, new Vector2(850, 680), new Vector2(0, -45));
        Label(root.transform, "HALFTIME STORE", 34, Lime, new Vector2(0, 292), new Vector2(780, 55), TextAnchor.MiddleCenter, FontStyle.Bold);
        Label(root.transform, "TRY IT ON BEFORE YOU BUY", 17, Muted, new Vector2(0, 250), new Vector2(780, 35), TextAnchor.MiddleCenter, FontStyle.Bold);

        var preview = Panel("Player Preview Card", root.transform, Panel, new Vector2(310, 405), new Vector2(-245, 10));
        Label(preview.transform, "YOUR PLAYER", 19, Color.white, new Vector2(0, 175), new Vector2(270, 35), TextAnchor.MiddleCenter, FontStyle.Bold);

        // Simple in-menu player mannequin; selected kit colours update live.
        var avatar = new GameObject("Player mannequin", typeof(RectTransform));
        avatar.transform.SetParent(preview.transform, false);
        var ar = avatar.GetComponent<RectTransform>();
        ar.sizeDelta = new Vector2(230, 300);
        ar.anchoredPosition = new Vector2(0, -15);
        AddPart(avatar.transform, "Head", new Vector2(0, 105), new Vector2(58, 58), new Color(0.72f,0.49f,0.34f));
        cap = AddPart(avatar.transform, "Cap", new Vector2(0, 137), new Vector2(68, 20), Color.clear);
        shirt = AddPart(avatar.transform, "Shirt", new Vector2(0, 35), new Vector2(112, 105), shirtColour);
        shorts = AddPart(avatar.transform, "Shorts", new Vector2(0, -52), new Vector2(105, 65), shortsColour);
        AddPart(avatar.transform, "Left leg", new Vector2(-27, -115), new Vector2(28, 75), new Color(0.72f,0.49f,0.34f));
        AddPart(avatar.transform, "Right leg", new Vector2(27, -115), new Vector2(28, 75), new Color(0.72f,0.49f,0.34f));
        boots = AddPart(avatar.transform, "Boots", new Vector2(0, -160), new Vector2(90, 20), bootsColour);

        var catalogue = Panel("Store Catalogue", root.transform, Panel, new Vector2(460, 405), new Vector2(220, 10));
        Label(catalogue.transform, "FEATURED ITEMS", 19, Color.white, new Vector2(0, 175), new Vector2(410, 35), TextAnchor.MiddleCenter, FontStyle.Bold);
        string[] items = { "Claret home shirt", "Blue away shirt", "Black kit", "Gold boots", "White boots", "Training cap" };
        for (int i = 0; i < items.Length; i++)
        {
            string item = items[i];
            int col = i % 2;
            int row = i / 2;
            MakeButton(catalogue.transform, item, new Vector2(-105 + col * 210, 105 - row * 88), new Vector2(190, 68), () => Preview(item));
        }

        itemName = Label(root.transform, selectedItem, 20, Color.white, new Vector2(220, -225), new Vector2(430, 32), TextAnchor.MiddleCenter, FontStyle.Bold);
        itemDescription = Label(root.transform, "Preview equipped on your player", 15, Muted, new Vector2(220, -252), new Vector2(430, 28), TextAnchor.MiddleCenter, FontStyle.Normal);
        MakeButton(root.transform, "BUY  •  500 COINS", new Vector2(90, -300), new Vector2(250, 54), () =>
        {
            if (status != null) status("Store preview only — coins and purchases are not connected yet.");
        });
        MakeButton(root.transform, "SELFIE  /  SAVE", new Vector2(355, -300), new Vector2(190, 54), TakeSelfie);
        Preview(selectedItem);
        root.SetActive(false);
        return root;
    }

    void Preview(string item)
    {
        selectedItem = item;
        if (itemName != null) itemName.text = item;
        if (itemDescription != null) itemDescription.text = "Preview equipped on your player";
        if (item == "Claret home shirt") { shirtColour = new Color(0.42f,0.055f,0.13f); shortsColour = new Color(0.08f,0.19f,0.34f); }
        else if (item == "Blue away shirt") { shirtColour = new Color(0.08f,0.35f,0.72f); shortsColour = Color.white; }
        else if (item == "Black kit") { shirtColour = new Color(0.035f,0.045f,0.06f); shortsColour = new Color(0.035f,0.045f,0.06f); }
        else if (item == "Gold boots") { bootsColour = new Color(1f,0.68f,0.08f); }
        else if (item == "White boots") { bootsColour = Color.white; }
        else if (item == "Training cap") { capVisible = true; }
        if (item != "Training cap") capVisible = false;
        if (shirt != null) shirt.color = shirtColour;
        if (shorts != null) shorts.color = shortsColour;
        if (boots != null) boots.color = bootsColour;
        if (cap != null) cap.color = capVisible ? Lime : Color.clear;
        if (status != null) status("Previewing " + item + " — not purchased");
    }

    void TakeSelfie()
    {
        if (!Application.isMobilePlatform)
        {
            if (status != null) status("Selfies can be saved when running on your Quest headset.");
            return;
        }
        if (status != null) status("Capturing your HALFTIME selfie...");
        StartCoroutine(CaptureAndSave());
    }

    IEnumerator CaptureAndSave()
    {
        string fileName = "HALFTIME_Selfie_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        string path = Path.Combine(Application.persistentDataPath, fileName);
        ScreenCapture.CaptureScreenshot(path);
        // Give the XR runtime time to finish writing the screenshot before reading it.
        float timeout = 8f;
        while (timeout > 0f && (!File.Exists(path) || new FileInfo(path).Length == 0))
        {
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }
        if (!File.Exists(path) || new FileInfo(path).Length == 0)
        {
            if (status != null) status("Selfie capture failed. Please try again.");
            yield break;
        }

        byte[] bytes;
        try { bytes = File.ReadAllBytes(path); }
        catch (Exception e)
        {
            Debug.LogWarning("Could not read selfie screenshot: " + e.Message);
            if (status != null) status("Could not read the selfie. Please try again.");
            yield break;
        }
        bool saved = false;
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unity.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var resolver = activity.Call<AndroidJavaObject>("getContentResolver"))
            using (var values = new AndroidJavaObject("android.content.ContentValues"))
            using (var media = new AndroidJavaClass("android.provider.MediaStore$Images$Media"))
            {
                values.Call("put", "_display_name", fileName);
                values.Call("put", "mime_type", "image/png");
                values.Call("put", "relative_path", "Pictures/HALFTIME VR");
                AndroidJavaObject uri = resolver.Call<AndroidJavaObject>("insert",
                    media.GetStatic<AndroidJavaObject>("EXTERNAL_CONTENT_URI"), values);
                if (uri != null)
                {
                    using (var stream = resolver.Call<AndroidJavaObject>("openOutputStream", uri))
                    {
                        sbyte[] signed = new sbyte[bytes.Length];
                        Buffer.BlockCopy(bytes, 0, signed, 0, bytes.Length);
                        stream.Call("write", signed);
                        stream.Call("flush");
                    }
                    saved = true;
                }
            }
        }
        catch (Exception e) { Debug.LogWarning("Selfie save failed: " + e.Message); }
#endif
        if (saved)
        {
            if (status != null) status("Selfie saved to Pictures / HALFTIME VR on your headset.");
        }
        else if (status != null)
        {
            status("Selfie captured, but headset gallery saving needs testing on Quest.");
        }
    }

    Image AddPart(Transform parent, string name, Vector2 pos, Vector2 size, Color colour)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = pos;
        var image = go.GetComponent<Image>();
        image.color = colour;
        return image;
    }

    GameObject Panel(string name, Transform parent, Color colour, Vector2 size, Vector2 pos)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = pos;
        go.GetComponent<Image>().color = colour;
        return go;
    }

    Text Label(Transform parent, string value, int size, Color colour, Vector2 pos, Vector2 dimensions, TextAnchor align, FontStyle style)
    {
        var go = new GameObject("Text - " + value, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = dimensions;
        rect.anchoredPosition = pos;
        var text = go.GetComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = size;
        text.color = colour;
        text.alignment = align;
        text.fontStyle = style;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    void MakeButton(Transform parent, string value, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        var go = Panel("Button - " + value, parent, new Color(0.10f,0.15f,0.22f), size, pos);
        var button = go.AddComponent<Button>();
        var colours = button.colors;
        colours.normalColor = new Color(0.10f,0.15f,0.22f);
        colours.highlightedColor = new Color(0.20f,0.28f,0.34f);
        colours.pressedColor = Lime;
        button.colors = colours;
        button.onClick.AddListener(action);
        Label(go.transform, value, 17, Color.white, Vector2.zero, size - new Vector2(12,8), TextAnchor.MiddleCenter, FontStyle.Bold);
    }
}