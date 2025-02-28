using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // Lists to track players
    public List<GameObject> players;         // All players in the scene
    public List<GameObject> selectedPlayers; // Currently selected players
    public LayerMask playerLayer;            // Layer for player detection

    // Adjustable formation variables exposed in the Inspector
    [Header("Formation Settings")]
    [Tooltip("Controls how far back followers are from the leader (multiplier of leader's radius)")]
    // Variables for drag selection
    private Vector3 dragStartPosition;
    private Rect selectionRect;
    private bool isDragging = false;

    void Start()
    {
        // Automatically find and select players tagged "Player"
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log("Found " + playerObjects.Length + " players in the scene.");

        foreach (GameObject player in playerObjects)
        {
            // Select up to 4 players initially
            if (selectedPlayers.Count < 4)
            {
                player.GetComponent<SelectablePlayer>().isSelected = true;
                selectedPlayers.Add(player);
                Debug.Log("Selected player: " + player.name);
            }
            players.Add(player); // Add to full player list
        }
    }

    void Update()
    {
        HandlePlayerSelection();
        HandleDragSelection();
    }

    void OnGUI()
    {
        if (isDragging)
        {
            // Draw the selection rectangle on screen
            Rect rect = GetScreenRect(dragStartPosition, Input.mousePosition);
            DrawScreenRect(rect, new Color(0, 1, 0, 0.25f));
            DrawScreenRectBorder(rect, 2, Color.green);
        }
    }

    void HandlePlayerSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (selectedPlayers.Count > 0)
            {
                AssignFormationPositions(mousePos);
            }
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

        // Fixed spacing between players
        float spacing = 1.0f;

        // Leader goes to the clicked position
        selectedPlayers[0].GetComponent<PlayerMovement>().SetTarget(targetPosition);

        // Place up to 3 followers in a grid behind the clicked spot
        if (selectedPlayers.Count >= 2)
        {
            // Follower 1: Behind and to the left
            Vector3 pos1 = targetPosition - direction * spacing + perpendicular * spacing;
            selectedPlayers[1].GetComponent<PlayerMovement>().SetTarget(pos1);
        }
        if (selectedPlayers.Count >= 3)
        {
            // Follower 2: Behind and to the right
            Vector3 pos2 = targetPosition - direction * spacing - perpendicular * spacing;
            selectedPlayers[2].GetComponent<PlayerMovement>().SetTarget(pos2);
        }
        if (selectedPlayers.Count >= 4)
        {
            // Follower 3: Further behind, centered
            Vector3 pos3 = targetPosition - direction * 2 * spacing;
            selectedPlayers[3].GetComponent<PlayerMovement>().SetTarget(pos3);
        }
    }
    
    void HandleDragSelection()
    {
        if (Input.GetMouseButtonDown(0)) dragStartPosition = Input.mousePosition;
        if (Input.GetMouseButton(0)) isDragging = true;
        if (Input.GetMouseButtonUp(0)) isDragging = false;
    }

    Rect GetScreenRect(Vector3 start, Vector3 end)
    {
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
    }

    void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, thickness), Texture2D.whiteTexture); // Top
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), Texture2D.whiteTexture); // Bottom
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, thickness, rect.height), Texture2D.whiteTexture); // Left
        GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), Texture2D.whiteTexture); // Right
    }
}