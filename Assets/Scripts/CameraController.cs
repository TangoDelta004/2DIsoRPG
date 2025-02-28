using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float zoomSpeed = 10f;
    public float minZoom = 5f;
    public float maxZoom = 20f;
    public float panSpeed = 0.5f;

    private Camera mainCamera;
    private Vector3 dragOrigin;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        HandleZoom();
        HandlePan();
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0.0f)
        {
            float newSize = mainCamera.orthographicSize - scroll * zoomSpeed;
            mainCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }

    void HandlePan()
    {
        if (Input.GetMouseButtonDown(2))
        {
            dragOrigin = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(2))
        {
            Vector3 difference = dragOrigin - mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mainCamera.transform.position += difference;
        }
    }
}