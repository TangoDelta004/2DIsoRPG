using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    public float detectionRange = 5.0f; // Distance at which the enemy detects the player
    public float meleeRange = 1.0f; // Distance at which the enemy stops and attacks
    public float moveSpeed = 2.0f; // Speed at which the enemy moves
    public int attackDamage = 20; // Amount of damage the enemy deals
    public float attackCooldown = 2.0f; // Time between attacks

    private GameObject[] players;
    private PlayerHealth targetPlayerHealth;
    private GameObject targetPlayer;
    private bool isChasing = false;
    private float lastAttackTime;

    void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        lastAttackTime = -attackCooldown; // Allow the enemy to attack immediately
    }

    void Update()
    {
        FindNearestPlayer();

        if (targetPlayer != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.transform.position);

            if (distanceToPlayer <= detectionRange && !isChasing)
            {
                isChasing = true; // Start chasing the player
            }

            if (isChasing)
            {
                if (distanceToPlayer > meleeRange)
                {
                    MoveTowardsPlayer();
                }
                else if (Time.time - lastAttackTime >= attackCooldown)
                {
                    AttackPlayer();
                    lastAttackTime = Time.time; // Update the time of the last attack
                }
            }
        }
        else
        {
            isChasing = false; // No player to chase, stop chasing
        }
    }

    void FindNearestPlayer()
    {
        players = GameObject.FindGameObjectsWithTag("Player");

        float nearestDistance = float.MaxValue;
        targetPlayer = null;

        foreach (GameObject player in players)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer < nearestDistance)
            {
                nearestDistance = distanceToPlayer;
                targetPlayer = player;
                targetPlayerHealth = player.GetComponent<PlayerHealth>();
            }
        }
    }

    void MoveTowardsPlayer()
    {
        Vector3 direction = (targetPlayer.transform.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    void AttackPlayer()
    {
        if (targetPlayerHealth != null)
        {
            targetPlayerHealth.TakeDamage(attackDamage);
            Debug.Log("Enemy attacks player for " + attackDamage + " damage.");
        }
    }
}
