using UnityEngine;
using Pathfinding;

public class PointAndClickPathfinding : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private Seeker seeker;
    private Path path;
    private int currentWaypoint = 0;
    public float nextWaypointDistance;

    void Start()
    {
        seeker = GetComponent<Seeker>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Convert mouse position to world position
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            // Start pathfinding to the clicked position
            seeker.StartPath(transform.position, mousePos, OnPathComplete);
        }

        if (isMoving && path != null)
        {
            FollowPath();
        }
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
            isMoving = true;
        }
    }

    void FollowPath()
    {
        if (currentWaypoint >= path.vectorPath.Count)
        {
            isMoving = false;
            return;
        }

        Vector3 direction = ((Vector3)path.vectorPath[currentWaypoint] - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        if (Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]) < nextWaypointDistance)
        {
            currentWaypoint++;
        }
    }
}