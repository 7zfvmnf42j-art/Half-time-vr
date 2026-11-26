using UnityEngine;

public class FootballerController : MonoBehaviour
{
    public bool isGoalkeeper;
    public int goalEnd;
    public float moveSpeed = 4.2f;
    Transform ball;
    Vector3 homePosition;

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
            target = new Vector3(Mathf.Clamp(ball.position.x, -3.2f, 3.2f), 1f, goalEnd * 48f);
            if (Mathf.Abs(ball.position.z - goalEnd * 48f) < 18f) target.x = Mathf.Clamp(ball.position.x, -3.4f, 3.4f);
        }
        else if (Vector3.Distance(ball.position, homePosition) < 13f)
        {
            target = new Vector3(ball.position.x, 1f, ball.position.z + (homePosition.z < 0 ? 2f : -2f));
        }
        Vector3 next = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        transform.position = next;
        Vector3 look = ball.position - transform.position;
        look.y = 0;
        if (look.sqrMagnitude > 0.05f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), 6f * Time.deltaTime);
    }
}
