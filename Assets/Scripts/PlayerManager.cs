using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // Lists to track players
    public List<GameObject> players = new List<GameObject>();         // All players in the scene
    public List<GameObject> selectedPlayers = new List<GameObject>(); // Currently selected players
    public LayerMask playerLayer;            // Layer for player detection

    // Variables for drag selection
    private Vector3 dragStartPosition;
    private bool isDragging = false;

    // Formation spacing
    [SerializeField] private float formationSpacing = 1.0f; // Adjust this in the Inspector for formation spread

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

        if (Input.GetMouseButtonDown(0)) 
        {
            HandlePlayerMovement();
        }
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
            mousePos.z = 0; // Ensure z-coordinate is zero
            Collider2D hit = Physics2D.OverlapPoint(mousePos, playerLayer);

            if (hit != null)
            {
                GameObject clickedPlayer = hit.gameObject;

                if (selectedPlayers.Contains(clickedPlayer))
                {
                    // Deselect the player
                    clickedPlayer.GetComponent<SelectablePlayer>().isSelected = false;
                    selectedPlayers.Remove(clickedPlayer);
                    Debug.Log("Player deselected: " + clickedPlayer.name);
                }
                else
                {
                    // Select the player
                    if (selectedPlayers.Count < 4)
                    {
                        clickedPlayer.GetComponent<SelectablePlayer>().isSelected = true;
                        selectedPlayers.Add(clickedPlayer);
                        Debug.Log("Player selected: " + clickedPlayer.name);
                    }
                }
            }
            else
            {
                // Optionally deselect all players if clicking on empty space
                // DeselectAllPlayers();
            }
        }
    }

    void HandleDragSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPosition = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                isDragging = false;
                SelectPlayersInDrag();
            }
        }
    }

    void SelectPlayersInDrag()
    {
        Rect selectionRect = GetScreenRect(dragStartPosition, Input.mousePosition);

        foreach (GameObject player in players)
        {
            Vector3 playerScreenPosition = Camera.main.WorldToScreenPoint(player.transform.position);

            // Invert y-coordinate because ScreenPoint has y increasing from bottom to top
            playerScreenPosition.y = Screen.height - playerScreenPosition.y;

            if (selectionRect.Contains(playerScreenPosition))
            {
                if (!selectedPlayers.Contains(player) && selectedPlayers.Count < 4)
                {
                    player.GetComponent<SelectablePlayer>().isSelected = true;
                    selectedPlayers.Add(player);
                    Debug.Log("Player selected by drag: " + player.name);
                }
            }
        }
    }

    void HandlePlayerMovement()
    {
        if (selectedPlayers.Count > 0)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0; // Ensure z-coordinate is zero

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
        // Move origin from bottom-left to top-left
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
        // Top
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, thickness), Texture2D.whiteTexture);
        // Bottom
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), Texture2D.whiteTexture);
        // Left
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, thickness, rect.height), Texture2D.whiteTexture);
        // Right
        GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}
