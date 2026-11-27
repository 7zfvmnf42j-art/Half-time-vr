using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Local match clock, score display, goal detection and kickoff reset.</summary>
public class MatchRules : MonoBehaviour
{
    public static MatchRules Instance { get; private set; }
    public bool IsGoalPending { get { return goalPending; } }
    public float matchLengthSeconds = 300f;
    public float goalResetDelay = 2.5f;
    int homeScore;
    int awayScore;
    float remaining;
    bool running = true;
    bool goalPending;
    Rigidbody ballBody;
    Vector3 ballKickoff;
    Transform[] players;
    Vector3[] playerKickoffs;
    Text scoreText;
    Text clockText;

    void Awake()
    {
        Instance = this;
        remaining = matchLengthSeconds;
    }

    void Start()
    {
        var ball = GameObject.Find("Match Ball");
        if (ball != null)
        {
            ballBody = ball.GetComponent<Rigidbody>();
            ballKickoff = ball.transform.position;
        }
        var footballers = FindObjectsOfType<FootballerController>();
        players = new Transform[footballers.Length];
        playerKickoffs = new Vector3[footballers.Length];
        for (int i = 0; i < footballers.Length; i++)
        {
            players[i] = footballers[i].transform;
            playerKickoffs[i] = players[i].position;
        }
        BuildScoreboard();
        UpdateScoreboard();
    }

    void Update()
    {
        if (running && remaining > 0f)
        {
            remaining = Mathf.Max(0f, remaining - Time.deltaTime);
            UpdateClock();
            if (remaining <= 0f) running = false;
        }
    }

    public void RegisterGoal(bool homeTeamScored)
    {
        if (!running || goalPending) return;
        if (homeTeamScored) homeScore++; else awayScore++;
        goalPending = true;
        UpdateScoreboard();
        StartCoroutine(ResetKickoff());
    }

    IEnumerator ResetKickoff()
    {
        if (ballBody != null)
        {
            ballBody.velocity = Vector3.zero;
            ballBody.angularVelocity = Vector3.zero;
        }
        yield return new WaitForSeconds(goalResetDelay);
        if (ballBody != null)
        {
            ballBody.transform.position = ballKickoff;
            ballBody.velocity = Vector3.zero;
            ballBody.angularVelocity = Vector3.zero;
        }
        for (int i = 0; i < players.Length; i++)
            if (players[i] != null) players[i].position = playerKickoffs[i];
        goalPending = false;
    }

    void BuildScoreboard()
    {
        var camera = Camera.main;
        if (camera == null) return;
        var canvasObject = new GameObject("HALFTIME Match Scoreboard", typeof(Canvas), typeof(CanvasScaler));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasObject.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 10;
        canvasObject.transform.SetParent(camera.transform, false);
        canvasObject.transform.localPosition = new Vector3(0f, 0.48f, 2.2f);
        canvasObject.transform.localRotation = Quaternion.identity;
        canvasObject.transform.localScale = Vector3.one * 0.0015f;
        var rect = canvas.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(620, 120);
        var background = new GameObject("Score Background", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(canvas.transform, false);
        background.GetComponent<RectTransform>().sizeDelta = new Vector2(620, 120);
        background.GetComponent<Image>().color = new Color(0.015f, 0.035f, 0.065f, 0.92f);
        scoreText = MakeText(background.transform, "HOME  0 - 0  AWAY", 34, new Vector2(0, 22), new Vector2(590, 62));
        clockText = MakeText(background.transform, "05:00", 22, new Vector2(0, -34), new Vector2(590, 35));
    }

    Text MakeText(Transform parent, string value, int size, Vector2 position, Vector2 dimensions)
    {
        var go = new GameObject("Score Text", typeof(RectTransform), typeof(Text));
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
        text.fontStyle = FontStyle.Bold;
        return text;
    }

    void UpdateScoreboard()
    {
        if (scoreText != null) scoreText.text = "HOME  " + homeScore + " - " + awayScore + "  AWAY";
        UpdateClock();
    }

    void UpdateClock()
    {
        if (clockText == null) return;
        int seconds = Mathf.CeilToInt(remaining);
        clockText.text = (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00") +
            (remaining <= 0f ? "   FULL TIME" : "");
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}

public class GoalSensor : MonoBehaviour
{
    public bool homeTeamScores;
    void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody == null || other.attachedRigidbody.gameObject.name != "Match Ball") return;
        if (MatchRules.Instance != null) MatchRules.Instance.RegisterGoal(homeTeamScores);
    }
}