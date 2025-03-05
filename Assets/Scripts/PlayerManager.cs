using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // List to track selected players
    public List<GameObject> selectedPlayers = new List<GameObject>();

    // Variables for drag selection
    private Vector3 dragStartPosition;
    private bool isDragging = false;

    // Variables for click-to-move
    private float clickStartTime;
    private Vector3 clickStartPosition;
    private const float clickThresholdTime = 0.2f;   // Time threshold for a click
    private const float clickThresholdDistance = 5.0f; // Distance threshold for a click

    // Flag to indicate if a player was clicked
    private bool clickedOnPlayer = false;

    // Formation spacing
    [SerializeField] private float formationSpacing = 1.0f;

    void Start()
    {
        // Automatically find and select players tagged "Player"
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log("Found " + playerObjects.Length + " players in the scene.");
        selectedPlayers.AddRange(playerObjects);
    }

    void Update()
    {
        // Clean up the selectedPlayers list by removing any null or missing players
        selectedPlayers.RemoveAll(player => player == null);

        HandleDragSelection();
        
        if (Input.GetMouseButtonDown(0))
        {
            clickStartTime = Time.time;
            clickStartPosition = Input.mousePosition;
            HandlePlayerSelection();

            // Start drag selection
            dragStartPosition = Input.mousePosition;
            isDragging = true;
        }
        

        if (Input.GetMouseButtonUp(0))
        {
            float clickDuration = Time.time - clickStartTime;
            float clickDistance = Vector3.Distance(clickStartPosition, Input.mousePosition);

            // Detect a click (short duration and small movement)
            if (clickDuration <= clickThresholdTime && clickDistance <= clickThresholdDistance)
            {
                if (!clickedOnPlayer)
                {
                    HandlePlayerMovement();
                }
            }
        }
    }

    void OnGUI()
    {
        if (isDragging)
        {
            Rect rect = GetScreenRect(dragStartPosition, Input.mousePosition);
            DrawScreenRect(rect, new Color(0, 1, 0, 0.25f));
            DrawScreenRectBorder(rect, 2, Color.green);
        }
    }

    void OnDrawGizmos()
    {
        foreach (GameObject player in selectedPlayers)
        {
            if (player != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(player.transform.position, 0.5f);
            }
        }
    }

    void HandlePlayerSelection()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Collider2D hit = Physics2D.OverlapPoint(mousePos);

        if (hit != null && hit.CompareTag("Player"))
        {
            // Deselect all players
            selectedPlayers.Clear();

            // Select the clicked player
            GameObject clickedPlayer = hit.gameObject;
            selectedPlayers.Add(clickedPlayer);
            Debug.Log("Player selected: " + clickedPlayer.name);

            // Indicate that a player was clicked
            clickedOnPlayer = true;
        }
        else
        {
            // No player was clicked
            clickedOnPlayer = false;
        }
    }

    void HandleDragSelection()
    {
        if (isDragging)
        {
            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                SelectPlayersInDrag();
            }
        }
    }

    void SelectPlayersInDrag()
    {
        List<GameObject> tempSelectedPlayers = new List<GameObject>(); // Temporary list

        // Assuming characters are tagged as "Player"
        GameObject[] characters = GameObject.FindGameObjectsWithTag("Player");

        // Get the corners of the selection rectangle in screen coordinates
        Vector2 startScreenPos = dragStartPosition;    // Screen coordinates when the drag started
        Vector2 endScreenPos = Input.mousePosition;    // Current screen coordinates

        // Calculate the rectangle boundaries
        float minX = Mathf.Min(startScreenPos.x, endScreenPos.x);
        float maxX = Mathf.Max(startScreenPos.x, endScreenPos.x);
        float minY = Mathf.Min(startScreenPos.y, endScreenPos.y);
        float maxY = Mathf.Max(startScreenPos.y, endScreenPos.y);

        foreach (GameObject character in characters) {
            // Get the screen position of the character
            Vector3 characterScreenPos = Camera.main.WorldToScreenPoint(character.transform.position);

            // Check if the character is within the selection rectangle
            if (characterScreenPos.x > minX && characterScreenPos.x < maxX && characterScreenPos.y > minY && characterScreenPos.y < maxY) {
                // Character is within selection box
                tempSelectedPlayers.Add(character);
                Debug.Log("Selected: " + character.name);
            }
        }

        if (tempSelectedPlayers.Count > 0) {
            selectedPlayers = tempSelectedPlayers; // Only update if at least one character is selected
        }
    }

    void HandlePlayerMovement()
    {
        if (selectedPlayers.Count > 0)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            AssignFormationPositions(mousePos);
        }
    }

    void AssignFormationPositions(Vector3 targetPosition)
    {
        if (selectedPlayers.Count == 0) return;

        // Leader is the first selected player
        GameObject leader = selectedPlayers[0];
        Vector3 leaderPosition = leader.transform.position;

        // Direction from leader to clicked spot
        Vector3 direction = (targetPosition - leaderPosition).normalized;

        // Perpendicular vector for side-to-side spacing
        Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0);

        // Leader goes to the clicked position
        leader.GetComponent<PlayerMovement>().SetTarget(targetPosition);

        // Fixed spacing between players
        float spacing = formationSpacing;

        // Assign positions to followers based on their order
        if (selectedPlayers.Count >= 2)
        {
            // Player 2: Behind and to the left
            GameObject player2 = selectedPlayers[1];
            Vector3 offset2 = (-direction * spacing) + (perpendicular * -spacing);
            Vector3 position2 = targetPosition + offset2;
            player2.GetComponent<PlayerMovement>().SetTarget(position2);
        }

        if (selectedPlayers.Count >= 3)
        {
            // Player 3: Behind and to the right
            GameObject player3 = selectedPlayers[2];
            Vector3 offset3 = (-direction * spacing) + (perpendicular * spacing);
            Vector3 position3 = targetPosition + offset3;
            player3.GetComponent<PlayerMovement>().SetTarget(position3);
        }

        if (selectedPlayers.Count >= 4)
        {
            // Player 4: Directly behind the leader
            GameObject player4 = selectedPlayers[3];
            Vector3 offset4 = (-direction * 2 * spacing);
            Vector3 position4 = targetPosition + offset4;
            player4.GetComponent<PlayerMovement>().SetTarget(position4);
        }
    }

    Vector3 CalculateFollowerOffset(int followerIndex, Vector3 direction, Vector3 perpendicular, float spacing)
    {
        // Formation pattern: V-shaped behind the leader
        int formationRow = (followerIndex - 1) / 2 + 1;           // Rows start from 1
        int sideMultiplier = (followerIndex % 2 == 0) ? 1 : -1;   // Alternate sides: -1, 1

        Vector3 offset = (-direction * spacing * formationRow) + (perpendicular * sideMultiplier * spacing * formationRow);
        return offset;
    }

    Rect GetScreenRect(Vector3 start, Vector3 end)
    {
        // Convert to top-left origin
        start.y = Screen.height - start.y;
        end.y = Screen.height - end.y;
        Vector3 topLeft = Vector3.Min(start, end);
        Vector3 bottomRight = Vector3.Max(start, end);
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }

    void DrawScreenRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        GUI.color = color;
        // Draw borders
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, thickness), Texture2D.whiteTexture); // Top
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), Texture2D.whiteTexture); // Bottom
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, thickness, rect.height), Texture2D.whiteTexture); // Left
        GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), Texture2D.whiteTexture); // Right
        GUI.color = Color.white;
    }
}
