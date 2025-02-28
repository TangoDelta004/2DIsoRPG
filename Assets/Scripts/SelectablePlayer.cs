using UnityEngine;

public class SelectablePlayer : MonoBehaviour
{
    public bool isSelected = false;

    private void OnDrawGizmos()
    {
        if (isSelected)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f); // Draw selection indicator
        }
    }
}