using UnityEngine;

public class MatchBootstrap : MonoBehaviour
{
    [Header("Pitch")]
    public float pitchLength = 105f;
    public float pitchWidth = 68f;
    public Material grassMaterial;
    public Material lineMaterial;
    public Material homeMaterial;
    public Material awayMaterial;
    public Material ballMaterial;

    void Start()
    {
        BuildPitch();
        BuildGoals();
        BuildBall();
        BuildTeams();
    }

    void BuildPitch()
    {
        var pitch = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pitch.name = "Full Size Pitch";
        pitch.transform.position = new Vector3(0, -0.15f, 0);
        pitch.transform.localScale = new Vector3(pitchWidth, 0.3f, pitchLength);
        Apply(pitch, grassMaterial, new Color(0.12f, 0.38f, 0.16f));

        Line("Halfway Line", Vector3.zero, new Vector3(pitchWidth, 0.035f, 0.12f));
        Circle("Centre Circle", Vector3.zero, 9.15f);
        BoxLine("Left Penalty Box", new Vector3(0, 0.025f, -pitchLength/2f + 16.5f), 40.3f, 0.12f, 16.5f);
        BoxLine("Right Penalty Box", new Vector3(0, 0.025f, pitchLength/2f - 16.5f), 40.3f, 0.12f, 16.5f);
        BoxLine("Left Six Yard Box", new Vector3(0, 0.03f, -pitchLength/2f + 5.5f), 18.32f, 0.12f, 5.5f);
        BoxLine("Right Six Yard Box", new Vector3(0, 0.03f, pitchLength/2f - 5.5f), 18.32f, 0.12f, 5.5f);
        BoxLine("Touchlines", Vector3.zero, pitchWidth, pitchLength, 0.12f);
    }

    void BuildGoals()
    {
        Goal(-pitchLength / 2f, "North Goal");
        Goal(pitchLength / 2f, "South Goal");
    }

    void Goal(float z, string label)
    {
        float direction = z < 0 ? -1 : 1;
        Vector3 centre = new Vector3(0, 0, z);
        Post(label + " Left Post", centre + new Vector3(-3.66f, 1.22f, 0), new Vector3(0.18f, 2.44f, 0.18f));
        Post(label + " Right Post", centre + new Vector3(3.66f, 1.22f, 0), new Vector3(0.18f, 2.44f, 0.18f));
        Post(label + " Crossbar", centre + new Vector3(0, 2.44f, 0), new Vector3(7.5f, 0.18f, 0.18f));
        var net = GameObject.CreatePrimitive(PrimitiveType.Cube);
        net.name = label + " Net";
        net.transform.position = centre + new Vector3(0, 1.2f, direction * 2.1f);
        net.transform.localScale = new Vector3(7.3f, 2.4f, 4.2f);
        Apply(net, null, new Color(0.85f,0.85f,0.85f,0.12f));
    }

    void BuildBall()
    {
        var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "Match Ball";
        ball.transform.position = new Vector3(0, 0.35f, 0);
        ball.transform.localScale = Vector3.one * 0.22f;
        Apply(ball, ballMaterial, Color.white);
        var rb = ball.AddComponent<Rigidbody>();
        rb.mass = 0.43f;
        rb.drag = 0.08f;
        rb.angularDrag = 0.05f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        ball.AddComponent<BallControl>();
    }

    void BuildTeams()
    {
        // Ten outfield players plus one goalkeeper for each side.
        for (int i = 0; i < 10; i++)
        {
            float x = ((i % 4) - 1.5f) * 9f;
            float z = -20f + (i / 4) * 14f;
            SpawnPlayer("Home", new Vector3(x, 0, z), homeMaterial, i == 0);
            SpawnPlayer("Away", new Vector3(x, 0, -z), awayMaterial, i == 0);
        }
        SpawnKeeper("Home Goalkeeper", new Vector3(0, 0, -48f), homeMaterial, -1);
        SpawnKeeper("Away Goalkeeper", new Vector3(0, 0, 48f), awayMaterial, 1);
    }

    void SpawnPlayer(string team, Vector3 p, Material mat, bool captain)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = team + (captain ? " Captain" : " Player");
        go.transform.position = p + Vector3.up;
        go.transform.localScale = new Vector3(0.7f, 1f, 0.7f);
        Apply(go, mat, team == "Home" ? new Color(0.45f,0.04f,0.12f) : new Color(0.05f,0.18f,0.55f));
        go.AddComponent<FootballerAI>();
    }

    void SpawnKeeper(string label, Vector3 p, Material mat, int end)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = label;
        go.transform.position = p + Vector3.up;
        go.transform.localScale = new Vector3(0.85f, 1.05f, 0.85f);
        Apply(go, mat, new Color(0.9f,0.72f,0.08f));
        var keeper = go.AddComponent<FootballerAI>();
        keeper.isGoalkeeper = true;
        keeper.goalEnd = end;
    }

    void Post(string label, Vector3 p, Vector3 scale)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = label;
        go.transform.position = p;
        go.transform.localScale = scale;
        Apply(go, null, Color.white);
    }

    void Line(string label, Vector3 p, Vector3 scale)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = label;
        go.transform.position = p + Vector3.up * 0.03f;
        go.transform.localScale = scale;
        Apply(go, lineMaterial, Color.white);
    }

    void BoxLine(string label, Vector3 centre, float width, float depth, float unused)
    {
        float z = depth == pitchLength ? pitchLength/2f : depth/2f;
        float sign = centre.z < 0 ? 1 : -1;
        Line(label + " Front", centre + new Vector3(0,0,sign*z), new Vector3(width,0.035f,0.12f));
        Line(label + " Left", centre + new Vector3(-width/2f,0,0), new Vector3(0.12f,0.035f,depth));
        Line(label + " Right", centre + new Vector3(width/2f,0,0), new Vector3(0.12f,0.035f,depth));
    }

    void Circle(string label, Vector3 centre, float radius)
    {
        var ring = new GameObject(label);
        for (int i=0;i<64;i++)
        {
            float a = i * Mathf.PI * 2f / 64f;
            float b = (i+1) * Mathf.PI * 2f / 64f;
            Vector3 mid = centre + new Vector3((Mathf.Cos(a)+Mathf.Cos(b))*radius/2f,0.035f,(Mathf.Sin(a)+Mathf.Sin(b))*radius/2f);
            var seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seg.transform.SetParent(ring.transform);
            seg.transform.position = mid;
            seg.transform.localScale = new Vector3(0.12f,0.035f,Vector3.Distance(new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)),new Vector3(Mathf.Cos(b),0,Mathf.Sin(b)))*radius);
            seg.transform.rotation = Quaternion.Euler(0, -i*360f/64f, 0);
            Apply(seg,lineMaterial,Color.white);
        }
    }

    void Apply(GameObject go, Material mat, Color fallback)
    {
        var r = go.GetComponent<Renderer>();
        if (r == null) return;
        r.material = mat != null ? mat : new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        if (mat == null) r.material.color = fallback;
    }
}
