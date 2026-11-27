using UnityEngine;

// Lightweight football decision-making for the native Quest prototype.
// Each side presses with its nearest outfielder while teammates keep useful lanes.
public class FootballerController : MonoBehaviour
{
    public bool isGoalkeeper;
    public int goalEnd;
    public float moveSpeed = 4.8f;
    public float acceleration = 10f;
    public float turnSpeed = 8f;

    Transform ball;
    Rigidbody ballBody;
    Vector3 homePosition;
    Vector3 velocity;
    bool isHome;
    float nextKickTime;
    float nextDecisionTime;
    static FootballerController[] rosterCache;\n    static float nextRosterRefresh;

    void Start()
    {
        homePosition = transform.position;
        isHome = name.StartsWith("Home");
        var found = GameObject.Find("Match Ball");
        if (found != null)
        {
            ball = found.transform;
            ballBody = found.GetComponent<Rigidbody>();
        }
        // Slightly stagger decision updates to keep the headset frame rate stable.
        nextDecisionTime = Time.time + Random.Range(0f, .25f);
    }

    void Update()
    {
        if (ball == null || (MatchRules.Instance != null && MatchRules.Instance.IsGoalPending))
        {
            velocity = Vector3.zero;
            return;
        }

        float dt = Time.deltaTime;
        Vector3 ballFlat = Flat(ball.position);
        Vector3 myFlat = Flat(transform.position);
        float distanceToBall = Vector3.Distance(myFlat, ballFlat);
        Vector3 target;

        if (isGoalkeeper)
        {
            float ownGoalZ = goalEnd * 49f;
            float goalLineX = Mathf.Clamp(ball.position.x * .78f, -3.25f, 3.25f);
            float depth = Mathf.Clamp(Mathf.Abs(ball.position.z - ownGoalZ) * .12f, 1.0f, 5.5f);
            target = new Vector3(goalLineX, 1f, ownGoalZ - Mathf.Sign(ownGoalZ) * depth);
            if (Mathf.Abs(ball.position.z - ownGoalZ) < 12f)
                target.x = Mathf.Clamp(ball.position.x, -3.55f, 3.55f);
        }
        else
        {
            bool attackingPositiveZ = isHome;
            float attack = attackingPositiveZ ? 1f : -1f;
            FootballerController nearest = NearestTeammateToBall();
            bool press = nearest == this;
            float teamDistance = Mathf.Abs(ball.position.z - homePosition.z);

            if (press && distanceToBall < 26f)
            {
                // Approach slightly behind the ball so the player can turn towards goal.
                target = ballFlat + new Vector3(0f, 0f, -attack * 1.15f);
            }
            else if (distanceToBall < 17f && press)
            {
                target = ballFlat;
            }
            else
            {
                // Support runners spread into lanes and step forward when their team attacks.
                float forwardShift = Mathf.Clamp((ball.position.z - homePosition.z) * attack * .22f, -3f, 9f);
                Vector3 support = homePosition + Vector3.forward * (attack * forwardShift);
                if (teamDistance < 12f)
                    support = Vector3.Lerp(support, ballFlat + new Vector3(Mathf.Sign(homePosition.x) * 5f, 0f, -attack * 5f), .35f);
                target = support;
            }

            if (distanceToBall < 1.45f && Time.time >= nextKickTime)
                TryPlayBall(attack);
        }

        target.x = Mathf.Clamp(target.x, -31f, 31f);
        target.z = Mathf.Clamp(target.z, -49f, 49f);
        Vector3 delta = target - transform.position;
        delta.y = 0f;
        Vector3 desired = delta.sqrMagnitude > .08f ? delta.normalized * moveSpeed : Vector3.zero;
        velocity = Vector3.MoveTowards(velocity, desired, acceleration * dt);
        transform.position += velocity * dt;

        Vector3 look = (ball.position - transform.position);
        look.y = 0f;
        if (look.sqrMagnitude > .08f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), turnSpeed * dt);

        // Simple running animation on the prototype legs.
        AnimateLegs();
    }

    FootballerController NearestTeammateToBall()
    {
        if (rosterCache == null || Time.time >= nextRosterRefresh)
        {
            rosterCache = FindObjectsOfType<FootballerController>();
            nextRosterRefresh = Time.time + 0.5f;
        }
        FootballerController nearest = this;
        float best = float.MaxValue;
        foreach (var player in rosterCache)
        {
            if (player == null || player.isGoalkeeper || player.isHome != isHome) continue;
            float d = (Flat(player.transform.position) - Flat(ball.position)).sqrMagnitude;
            if (d < best) { best = d; nearest = player; }
        }
        return nearest;
    }

    void TryPlayBall(float attack)
    {
        if (ballBody == null || ballBody.isKinematic) return;
        Vector3 toGoal = new Vector3(-ball.position.x * .12f, 0f, attack).normalized;
        float distanceToGoal = Mathf.Abs((isHome ? 52.5f : -52.5f) - ball.position.z);
        Vector3 playDirection = toGoal;
        float power = 5.5f;

        // Shoot when close enough; otherwise carry the ball forward with a controlled touch.
        if (distanceToGoal < 25f && Mathf.Abs(ball.position.x) < 22f)
        {
            power = 12.5f;
            playDirection = (new Vector3(-ball.position.x * .12f, .12f, attack * 52.5f) - ball.position).normalized;
        }
        else
        {
            playDirection.y = .08f;
        }

        ballBody.AddForce(playDirection * power, ForceMode.VelocityChange);
        ballBody.AddTorque(Vector3.up * Random.Range(-1.2f, 1.2f), ForceMode.VelocityChange);
        nextKickTime = Time.time + Random.Range(1.0f, 1.8f);
    }

    void AnimateLegs()
    {
        if (velocity.sqrMagnitude < .08f) return;
        // Body remains a stable prototype mesh; movement speed drives a subtle bounce.
        float bob = Mathf.Abs(Mathf.Sin(Time.time * 10f)) * .035f;
        var p = transform.position;
        p.y = 1f + bob;
        transform.position = p;
    }

    static Vector3 Flat(Vector3 p) { p.y = 0f; return p; }
}
