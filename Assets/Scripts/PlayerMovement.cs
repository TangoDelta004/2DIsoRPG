using UnityEngine;
using Pathfinding;

public class PlayerMovement : MonoBehaviour
{
    // Movement variables
    public float moveSpeed = 5f;           // Speed of player movement
    private bool isMoving = false;         // Is the player currently moving?
    private Seeker seeker;                 // Reference to the A* Seeker component
    private Path path;                     // Current path being followed
    private int currentWaypoint = 0;       // Current waypoint index in the path

    // Animation variables (optional, adjust as needed)
    public GameObject playerSprite;        // Reference to the sprite GameObject for animation
    private Animator animator;             // Animator component for movement animation

    // Target indicator variables
    public GameObject targetIndicatorPrefab; // Assign this prefab in the Inspector
    private GameObject currentTargetIndicator; // The current target's GameObject instance

    // Store the target position
    public Vector3 currentTargetPosition { get; private set; }

    void Start()
    {
        seeker = GetComponent<Seeker>();
        if (playerSprite != null)
        {
            animator = playerSprite.GetComponent<Animator>();
        }
    }

    void Update()
    {
        if (isMoving && path != null)
        {
            FollowPath();
        }
    }

    /// <summary>
    /// Sets a new target position for the player to move to.
    /// </summary>
    /// <param name="target">The target position in world space.</param>
    public void SetTarget(Vector3 target)
    {
        target.z = 0; // Ensure z is 0 for 2D games
        currentTargetPosition = target; // Store the target position

        // Destroy any existing target indicator
        if (currentTargetIndicator != null)
        {
            Destroy(currentTargetIndicator);
        }

        // Instantiate a new target indicator at the target position
        if (targetIndicatorPrefab != null)
        {
            currentTargetIndicator = Instantiate(targetIndicatorPrefab, target, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("TargetIndicatorPrefab is not assigned in PlayerMovement script on " + gameObject.name);
        }

        // Start pathfinding to the target position
        seeker.StartPath(transform.position, target, OnPathComplete);
    }

    /// <summary>
    /// Callback when the pathfinding calculation is complete.
    /// </summary>
    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
            isMoving = true;
            if (animator != null)
            {
                animator.SetBool("isMoving", true);
            }
        }
        else
        {
            Debug.LogError("Pathfinding error: " + p.errorLog);
        }
    }

    /// <summary>
    /// Moves the player along the calculated path.
    /// </summary>
    void FollowPath()
    {
        if (path == null || currentWaypoint >= path.vectorPath.Count)
        {
            // Reached the end of the path
            isMoving = false;
            if (animator != null)
            {
                animator.SetBool("isMoving", false);
            }
            // Clean up target indicator if present
            if (currentTargetIndicator != null)
            {
                Destroy(currentTargetIndicator);
                currentTargetIndicator = null;
            }
            return;
        }

        // Current waypoint position
        Vector3 waypoint = path.vectorPath[currentWaypoint];
        waypoint.z = 0; // Ensure z is 0

        Vector3 direction = (waypoint - transform.position).normalized;
        float distanceThisFrame = moveSpeed * Time.deltaTime;

        // Prevent overshooting by clamping movement
        float distanceToWaypoint = Vector3.Distance(transform.position, waypoint);
        if (distanceToWaypoint <= distanceThisFrame)
        {
            // Snap to the waypoint if close enough
            transform.position = waypoint;
            currentWaypoint++;
        }
        else
        {
            // Move normally towards the waypoint
            transform.position += direction * distanceThisFrame;
        }

        // Update animation
        if (animator != null)
        {
            int dir = GetDirectionIndex(direction); // Assuming this sets direction
            animator.SetInteger("direction", dir);
            animator.SetBool("isMoving", true);
        }
    }

    /// <summary>
    /// Determines the direction index for animation based on movement direction.
    /// </summary>
    int GetDirectionIndex(Vector3 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle >= -22.5f && angle < 22.5f) return 6; // East
        if (angle >= 22.5f && angle < 67.5f) return 5;  // North-East
        if (angle >= 67.5f && angle < 112.5f) return 4; // North
        if (angle >= 112.5f && angle < 157.5f) return 3; // North-West
        if ((angle >= 157.5f && angle <= 180f) || (angle >= -180f && angle < -157.5f)) return 2; // West
        if (angle >= -157.5f && angle < -112.5f) return 1; // South-West
        if (angle >= -112.5f && angle < -67.5f) return 0;  // South
        if (angle >= -67.5f && angle < -22.5f) return 7;  // South-East
        return 0; // Default to South
    }
}
