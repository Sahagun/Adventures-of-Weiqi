using UnityEngine;
using UnityEngine.UI;

public class CanvasScroller : MonoBehaviour
{
    public Transform player;         // Reference to the player object
    public float smoothSpeed = 5f;   // Speed of the camera follow
    public Vector2 minPosition;      // Minimum bounds (left, bottom)
    public Vector2 maxPosition;      // Maximum bounds (right, top)

    private Vector3 offset;

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player not assigned to the camera follow script!");
            return;
        }

        // Calculate the initial offset between the camera and the player
        offset = transform.position - player.position;
    }

    void LateUpdate()
    {
        if (player == null)
            return;

        // Calculate the desired position with offset
        Vector3 desiredPosition = player.position + offset;

        // Clamp the camera position within the set bounds
        float clampedX = Mathf.Clamp(desiredPosition.x, minPosition.x, maxPosition.x);
        float clampedY = Mathf.Clamp(desiredPosition.y, minPosition.y, maxPosition.y);

        // Smoothly move the camera to the target position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, new Vector3(clampedX, clampedY, transform.position.z), smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}