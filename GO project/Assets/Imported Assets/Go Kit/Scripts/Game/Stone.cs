using System.Collections.Generic;
using UnityEngine;

public class Stone : MonoBehaviour
{
    public enum StoneColor { None, Black, White }
    public StoneColor color;

    // Serialize collision states for visibility in the Inspector
    [Header("Collision States")]
    [SerializeField] private bool upCollision = false;
    [SerializeField] private bool downCollision = false;
    [SerializeField] private bool leftCollision = false;
    [SerializeField] private bool rightCollision = false;

    private bool isCaptured;

    // Debug visuals for colliders in the editor
    [Header("Debug Settings")]
    [SerializeField] private bool debugDraw = true;
    [SerializeField] private Color debugLineColor = Color.red;

    void Start ()
    {
        isCaptured = false;

        // Initialize all collision states to false
        upCollision = false;
        downCollision = false;
        leftCollision = false;
        rightCollision = false;

        Debug.Log($"[{gameObject.name}] Initialized with color: {color}");
    }


    public void OnTriggerEnter (Collider other)
    {
        Stone otherStone = other.GetComponentInParent<Stone>();
        if (otherStone != null)
        {
            string position = other.gameObject.name;

            // Only process collisions with stones of the opposite color
            if (otherStone.color == color || otherStone.color == StoneColor.None)
            {
                Debug.Log($"[{gameObject.name}] Ignoring collision with {otherStone.gameObject.name} (same or no color).");
                return;
            }

            Debug.Log($"[{gameObject.name}] Detected collision with {otherStone.gameObject.name} at position: {position}");

            // Update the correct collision boolean
            if (position == "UpCollision")
                upCollision = true;
            else if (position == "DownCollision")
                downCollision = true;
            else if (position == "LeftCollision")
                leftCollision = true;
            else if (position == "RightCollision")
                rightCollision = true;
            else
                Debug.LogWarning($"[{gameObject.name}] Unknown position: {position}");

            // Check if the stone is captured after updating
            CheckIfCaptured();
        }
        else
        {
            Debug.Log($"[{gameObject.name}] Triggered with a non-stone object: {other.gameObject.name}");
        }
    }




    public void OnTriggerExit (Collider other)
    {
        Stone otherStone = other.GetComponentInParent<Stone>();
        if (otherStone != null)
        {
            string position = other.gameObject.name;

            Debug.Log($"[{gameObject.name}] Collision exited with {otherStone.gameObject.name} at position: {position}");

            // Reset the correct collision boolean
            if (position == "UpCollision")
                upCollision = false;
            else if (position == "DownCollision")
                downCollision = false;
            else if (position == "LeftCollision")
                leftCollision = false;
            else if (position == "RightCollision")
                rightCollision = false;

            // Re-check if the stone is still captured after the exit
            CheckIfCaptured();
        }
    }


    private void UpdateCollisionState (string position,bool state)
    {
        switch (position)
        {
            case "Up":
                upCollision = state;
                break;
            case "Down":
                downCollision = state;
                break;
            case "Left":
                leftCollision = state;
                break;
            case "Right":
                rightCollision = state;
                break;
            default:
                Debug.LogWarning($"[{gameObject.name}] Unknown position: {position}");
                break;
        }
    }

    private void CheckIfCaptured ()
    {
        // Log the current state of all collisions
        Debug.Log($"[{gameObject.name}] Checking capture - Up: {upCollision}, Down: {downCollision}, Left: {leftCollision}, Right: {rightCollision}");

        // Ensure all four sides are surrounded
        if (upCollision && downCollision && leftCollision && rightCollision)
        {
            isCaptured = true;
            CaptureStone(); // Capture the stone if all sides are surrounded
        }
        else
        {
            isCaptured = false;
            Debug.Log($"[{gameObject.name}] Not captured. Missing collision(s).");
        }
    }



    private void CaptureStone ()
    {
        Debug.Log($"[{gameObject.name}] is captured!");
        Destroy(gameObject); // Remove the stone
    }

    void OnDrawGizmos ()
    {
        if (!debugDraw)
            return;

        Gizmos.color = debugLineColor;

        // Draw lines in the directions where collisions are detected
        if (upCollision)
            Gizmos.DrawLine(transform.position,transform.position + Vector3.up * 0.5f);
        if (downCollision)
            Gizmos.DrawLine(transform.position,transform.position + Vector3.down * 0.5f);
        if (leftCollision)
            Gizmos.DrawLine(transform.position,transform.position + Vector3.left * 0.5f);
        if (rightCollision)
            Gizmos.DrawLine(transform.position,transform.position + Vector3.right * 0.5f);
    }
}
