using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target; // The object the camera is looking at
    [Header("Distance")]
    [SerializeField] private float distance = 1f; // The distance from the target
    [SerializeField] private float minDistance = 0.5f; // The minimum distance from the target
    [SerializeField] private float maxDistance = 2f; // The maximum distance from the target
    [Header("Speed")]
    [SerializeField] private float zoomSpeed = 3f; // The speed of the zooming
    [SerializeField] private float xSpeed = 120.0f; // The speed of horizontal rotation
    [SerializeField] private float ySpeed = 120.0f; // The speed of vertical rotation
    [Header("Y-Limit")]
    [SerializeField] private float yMinLimit = -20f; // The minimum vertical angle
    [SerializeField] private float yMaxLimit = 80f; // The maximum vertical angle

    private float x = 0.0f;
    private float y = 0.0f;
    private Vector3 offset;
    private float touchZoomSpeed = 0.05f; // The speed of the zooming for mobile control
    private float touchDistance; // The distance between two fingers for mobile zooming

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
        offset = new Vector3(0, 0, -distance);
    }

    private void LateUpdate()
    {
        if (!target) return;

        // Mouse Control
        if (Input.GetMouseButton(0))
        {
            x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

            y = ClampAngle(y, yMinLimit, yMaxLimit);
        }
        // Mobile Control
        else if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            x += Input.GetTouch(0).deltaPosition.x * xSpeed * distance * 0.02f;
            y -= Input.GetTouch(0).deltaPosition.y * ySpeed * 0.02f;

            y = ClampAngle(y, yMinLimit, yMaxLimit);
        }
        // Zooming Control
        else
        {
            if (Input.touchCount == 2)
            {
                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);

                if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
                {
                    touchDistance = Vector2.Distance(touch1.position, touch2.position);
                }
                else if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved)
                {
                    float newTouchDistance = Vector2.Distance(touch1.position, touch2.position);

                    if (newTouchDistance < touchDistance)
                    {
                        distance += touchZoomSpeed;
                    }
                    else if (newTouchDistance > touchDistance)
                    {
                        distance -= touchZoomSpeed;
                    }
                    touchDistance = newTouchDistance;
                }
            }
            else 
                distance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        }

        distance = Mathf.Clamp(distance, minDistance, maxDistance);
        offset = new Vector3(0, 0, -distance);

        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 position = rotation * offset + target.position;

        transform.rotation = rotation;
        transform.position = position;

    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360.0f) angle += 360.0f;

        if (angle > 360.0f) angle -= 360.0f;

        return Mathf.Clamp(angle, min, max);
    }
}
