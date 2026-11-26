using UnityEngine;

public class FootballerController : MonoBehaviour
{
    public bool isGoalkeeper;
    public int goalEnd;
    public float moveSpeed = 4.2f;
    public float acceleration = 8f;
    public float turnSpeed = 7f;
    Transform ball;
    Vector3 homePosition;
    Vector3 currentVelocity;

    void Start()
    {
        homePosition = transform.position;
        var found = GameObject.Find("Match Ball");
        if (found != null) ball = found.transform;
    }

    void Update()
    {
        if (ball == null) return;
        Vector3 target = homePosition;

        if (isGoalkeeper)
        {
            float goalZ = goalEnd * 48f;
            target = new Vector3(Mathf.Clamp(ball.position.x, -3.2f, 3.2f), 1f, goalZ);
            if (Mathf.Abs(ball.position.z - goalZ) < 18f)
                target.x = Mathf.Clamp(ball.position.x, -3.4f, 3.4f);
        }
        else if (Vector3.Distance(ball.position, homePosition) < 13f)
        {
            Vector3 support = new Vector3(ball.position.x, 1f, ball.position.z + (homePosition.z < 0 ? 2f : -2f));
            target = Vector3.Lerp(homePosition, support, 0.72f);
        }

        target.x = Mathf.Clamp(target.x, -pitchHalfWidth, pitchHalfWidth);
        target.z = Mathf.Clamp(target.z, -pitchHalfLength, pitchHalfLength);
        Vector3 desired = target - transform.position;
        desired.y = 0;
        Vector3 desiredVelocity = desired.sqrMagnitude > 0.04f ? desired.normalized * moveSpeed : Vector3.zero;
        currentVelocity = Vector3.MoveTowards(currentVelocity, desiredVelocity, acceleration * Time.deltaTime);
        transform.position += currentVelocity * Time.deltaTime;

        Vector3 look = ball.position - transform.position;
        look.y = 0;
        if (look.sqrMagnitude > 0.05f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), turnSpeed * Time.deltaTime);
    }

    const float pitchHalfWidth = 32f;
    const float pitchHalfLength = 49f;
}