using UnityEngine;

public class DynamicSorting : MonoBehaviour
{
    public GameObject playerSelector;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Assume all obstacles are tagged as "Obstacle"
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

        foreach (GameObject obstacle in obstacles)
        {
            float selectorY = playerSelector.transform.position.y;
            float obstacleY = obstacle.transform.position.y;

            if (selectorY > obstacleY)
            {
                // Player is in front of the obstacle
                spriteRenderer.sortingOrder = obstacle.GetComponent<SpriteRenderer>().sortingOrder + 1;
            }
            else
            {
                // Player is behind the obstacle
                spriteRenderer.sortingOrder = obstacle.GetComponent<SpriteRenderer>().sortingOrder - 1;
            }
        }
    }
}