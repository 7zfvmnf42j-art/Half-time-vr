using UnityEngine;

public class MatchBootstrap : MonoBehaviour
{
    [Header("Pitch dimensions (metres)")]
    public float pitchLength = 105f;
    public float pitchWidth = 68f;
    public Material grassMaterial;
    public Material lineMaterial;
    public Material homeMaterial;
    public Material awayMaterial;
    public Material ballMaterial;

    Material turfA, turfB, white, stadiumDark;

    void Start()
    {
        CreateMaterials();
        BuildPitch();
        BuildGoals();
        BuildStadiumDetails();
        BuildBall();
        BuildTeams();
    }

    void CreateMaterials()
    {
        turfA = MakeMaterial(new Color(0.055f, 0.27f, 0.105f), 0.05f);
        turfB = MakeMaterial(new Color(0.075f, 0.32f, 0.13f), 0.05f);
        white = MakeMaterial(new Color(0.92f, 0.94f, 0.88f), 0.1f);
        stadiumDark = MakeMaterial(new Color(0.035f, 0.055f, 0.075f), 0.15f);
    }

    Material MakeMaterial(Color colour, float metallic)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        var material = new Material(shader);
        material.color = colour;
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.28f);
        return material;
    }

    void BuildPitch()
    {
        var pitch = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pitch.name = "Full Size Pitch";
        pitch.transform.position = new Vector3(0, -0.16f, 0);
        pitch.transform.localScale = new Vector3(pitchWidth, 0.3f, pitchLength);
        Apply(pitch, grassMaterial, new Color(0.06f, 0.29f, 0.11f));

        // Alternating mowing bands give the grass visible depth.
        for (int i = 0; i < 10; i++)
        {
            float bandLength = pitchLength / 10f;
            var band = GameObject.CreatePrimitive(PrimitiveType.Cube);
            band.name = "Mowing Stripe " + (i + 1);
            band.transform.position = new Vector3(0, 0.002f, -pitchLength / 2f + bandLength * (i + 0.5f));
            band.transform.localScale = new Vector3(pitchWidth - 0.15f, 0.012f, bandLength);
            Apply(band, i % 2 == 0 ? turfA : turfB, Color.white);
            Destroy(band.GetComponent<Collider>());
        }

        Line("Halfway Line", Vector3.zero, new Vector3(pitchWidth, 0.035f, 0.12f));
        Circle("Centre Circle", Vector3.zero, 9.15f);
        Circle("Centre Spot Ring", Vector3.zero, 0.18f);
        BoxLine("North Penalty Box", new Vector3(0, 0.025f, -pitchLength / 2f + 16.5f), 40.3f, 33f, 0);
        BoxLine("South Penalty Box", new Vector3(0, 0.025f, pitchLength / 2f - 16.5f), 40.3f, 33f, 0);
        BoxLine("North Six Yard Box", new Vector3(0, 0.03f, -pitchLength / 2f + 5.5f), 18.32f, 11f, 0);
        BoxLine("South Six Yard Box", new Vector3(0, 0.03f, pitchLength / 2f - 5.5f), 18.32f, 11f, 0);
        Line("North Goal Line", new Vector3(0, 0.03f, -pitchLength / 2f), new Vector3(pitchWidth, 0.035f, 0.12f));
        Line("South Goal Line", new Vector3(0, 0.03f, pitchLength / 2f), new Vector3(pitchWidth, 0.035f, 0.12f));
        Line("West Touchline", new Vector3(-pitchWidth / 2f, 0.03f, 0), new Vector3(0.12f, 0.035f, pitchLength));
        Line("East Touchline", new Vector3(pitchWidth / 2f, 0.03f, 0), new Vector3(0.12f, 0.035f, pitchLength));
        Spot("North Penalty Spot", new Vector3(0, 0.045f, -pitchLength / 2f + 11f));
        Spot("South Penalty Spot", new Vector3(0, 0.045f, pitchLength / 2f - 11f));
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
        Post(label + " Left Post", centre + new Vector3(-3.66f, 1.22f, 0), new Vector3(0.16f, 2.44f, 0.16f));
        Post(label + " Right Post", centre + new Vector3(3.66f, 1.22f, 0), new Vector3(0.16f, 2.44f, 0.16f));
        Post(label + " Crossbar", centre + new Vector3(0, 2.44f, 0), new Vector3(7.32f, 0.16f, 0.16f));

        // Open goal frame with visible net bars rather than a solid block.
        for (int i = 0; i <= 12; i++)
        {
            float x = -3.6f + i * 0.6f;
            Post(label + " Net Roof " + i, centre + new Vector3(x, 2.35f, direction * 2f), new Vector3(0.025f, 0.025f, 4f));
        }
        for (int i = 0; i <= 6; i++)
        {
            float y = 0.2f + i * 0.36f;
            Post(label + " Net Back " + i, centre + new Vector3(-3.6f, y, direction * 4f), new Vector3(7.2f, 0.025f, 0.025f));
        }
        for (int i = 0; i <= 12; i++)
        {
            float x = -3.6f + i * 0.6f;
            Post(label + " Net Side " + i, centre + new Vector3(x, 1.2f, direction * 4f), new Vector3(0.025f, 2.4f, 0.025f));
        }
    }

    void BuildStadiumDetails()
    {
        // Low-detail stadium structure and roof supports, kept outside the touchlines.
        for (int side = -1; side <= 1; side += 2)
        {
            var stand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stand.name = side < 0 ? "West Stand" : "East Stand";
            stand.transform.position = new Vector3(side * 40f, 3.5f, 0);
            stand.transform.localScale = new Vector3(8f, 7f, 112f);
            Apply(stand, stadiumDark, Color.white);

            for (int tier = 0; tier < 3; tier++)
            {
                var seatBand = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seatBand.name = "Stand Tier " + tier;
                seatBand.transform.position = new Vector3(side * 35.5f, 1.2f + tier * 1.8f, 0);
                seatBand.transform.localScale = new Vector3(1.1f, 0.65f, 108f);
                Apply(seatBand, tier == 1 ? turfB : stadiumDark, Color.white);
            }
        }
        foreach (int end in new[] { -1, 1 })
        {
            var stand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stand.name = end < 0 ? "North Stand" : "South Stand";
            stand.transform.position = new Vector3(0, 3.5f, end * 59f);
            stand.transform.localScale = new Vector3(82f, 7f, 8f);
            Apply(stand, stadiumDark, Color.white);
        }
    }

    void BuildBall()
    {
        var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "Match Ball";
        ball.transform.position = new Vector3(0, 0.35f, 0);
        ball.transform.localScale = Vector3.one * 0.22f;
        Apply(ball, ballMaterial, Color.white);
        var physics = new PhysicMaterial("Match Ball Surface");
        physics.dynamicFriction = 0.24f;
        physics.staticFriction = 0.32f;
        physics.bounciness = 0.58f;
        physics.frictionCombine = PhysicMaterialCombine.Average;
        physics.bounceCombine = PhysicMaterialCombine.Maximum;
        ball.GetComponent<SphereCollider>().material = physics;
        var rb = ball.AddComponent<Rigidbody>();
        rb.mass = 0.43f;
        rb.drag = 0.025f;
        rb.angularDrag = 0.06f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        ball.AddComponent<BallControl>();
    }

    void BuildTeams()
    {
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
        Apply(go, mat, team == "Home" ? new Color(0.45f, 0.04f, 0.12f) : new Color(0.05f, 0.18f, 0.55f));
        go.AddComponent<FootballerController>();
    }

    void SpawnKeeper(string label, Vector3 p, Material mat, int end)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = label;
        go.transform.position = p + Vector3.up;
        go.transform.localScale = new Vector3(0.85f, 1.05f, 0.85f);
        Apply(go, mat, new Color(0.9f, 0.72f, 0.08f));
        var keeper = go.AddComponent<FootballerController>();
        keeper.isGoalkeeper = true;
        keeper.goalEnd = end;
    }

    void Spot(string label, Vector3 p)
    {
        var spot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        spot.name = label;
        spot.transform.position = p;
        spot.transform.localScale = new Vector3(0.22f, 0.012f, 0.22f);
        Apply(spot, white, Color.white);
        Destroy(spot.GetComponent<Collider>());
    }

    void Post(string label, Vector3 p, Vector3 scale)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = label;
        go.transform.position = p;
        go.transform.localScale = scale;
        Apply(go, white, Color.white);
    }

    void Line(string label, Vector3 p, Vector3 scale)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = label;
        go.transform.position = p + Vector3.up * 0.035f;
        go.transform.localScale = scale;
        Apply(go, lineMaterial, Color.white);
        Destroy(go.GetComponent<Collider>());
    }

    void BoxLine(string label, Vector3 centre, float width, float depth, float unused)
    {
        float sign = centre.z < 0 ? 1 : -1;
        Line(label + " Front", centre + new Vector3(0, 0, sign * depth / 2f), new Vector3(width, 0.035f, 0.12f));
        Line(label + " Left", centre + new Vector3(-width / 2f, 0, 0), new Vector3(0.12f, 0.035f, depth));
        Line(label + " Right", centre + new Vector3(width / 2f, 0, 0), new Vector3(0.12f, 0.035f, depth));
    }

    void Circle(string label, Vector3 centre, float radius)
    {
        var ring = new GameObject(label);
        for (int i = 0; i < 64; i++)
        {
            float a = i * Mathf.PI * 2f / 64f;
            float b = (i + 1) * Mathf.PI * 2f / 64f;
            Vector3 mid = centre + new Vector3((Mathf.Cos(a) + Mathf.Cos(b)) * radius / 2f, 0.04f, (Mathf.Sin(a) + Mathf.Sin(b)) * radius / 2f);
            var seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seg.transform.SetParent(ring.transform);
            seg.transform.position = mid;
            seg.transform.localScale = new Vector3(0.12f, 0.035f, Vector3.Distance(new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)), new Vector3(Mathf.Cos(b), 0, Mathf.Sin(b))) * radius);
            seg.transform.rotation = Quaternion.Euler(0, -i * 360f / 64f, 0);
            Apply(seg, lineMaterial, Color.white);
            Destroy(seg.GetComponent<Collider>());
        }
    }

    void Apply(GameObject go, Material mat, Color fallback)
    {
        var r = go.GetComponent<Renderer>();
        if (r == null) return;
        r.material = mat != null ? mat : MakeMaterial(fallback, 0.05f);
        if (mat == null) r.material.color = fallback;
    }
}