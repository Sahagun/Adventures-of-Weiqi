using System.Collections.Generic;
using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    public float speed = 5f;  // Movement speed
    public FixedJoystick fixedJoystick;
    public float joystickDeadZone = 0.1f;
    public bool forceMobileJoystickInEditor = false; // Debug toggle to test mobile controls in the editor.

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movement;

    public LayerMask obstacleLayer;            // LayerMask to detect obstacles
    public LayerMask triggerLayer;             // LayerMask to detect triggers

    private bool isInTriggerZone = false;      // Track if the player is in a trigger zone
    private Collider2D characterCollider;      // Reference to the character's Collider2D

    private HashSet<UITrigger> activeTriggers = new HashSet<UITrigger>();
    private bool useJoystickInput;


    void Start ()
    {
        rb = GetComponent<Rigidbody2D>();         // Reference to the Rigidbody2D
        animator = GetComponent<Animator>();      // Reference to the Animator
        characterCollider = GetComponent<Collider2D>(); // Reference to the Collider2D

        // Allow mobile joystick flow on real mobile, or force it while testing in the editor.
        bool shouldUseMobileControls = Application.isMobilePlatform || (Application.isEditor && forceMobileJoystickInEditor);
        useJoystickInput = shouldUseMobileControls && fixedJoystick != null;

        if (fixedJoystick != null)
        {
            fixedJoystick.gameObject.SetActive(shouldUseMobileControls);
        }

        if (characterCollider == null)
        {
            Debug.LogError("No Collider2D component found on the character.");
        }

        // Ensure the enter button starts hidden until the player is inside a trigger.
        isInTriggerZone = false;
        EnterButton.Instance?.UnbindTrigger();
        EnterButton.Instance?.Hide();
    }

    void Update ()
    {
        float moveX;
        float moveY;

        if (useJoystickInput)
        {
            // Read joystick input on mobile and apply a small dead zone to avoid drift.
            moveX = Mathf.Abs(fixedJoystick.Horizontal) > joystickDeadZone ? fixedJoystick.Horizontal : 0f;
            moveY = Mathf.Abs(fixedJoystick.Vertical) > joystickDeadZone ? fixedJoystick.Vertical : 0f;
        }
        else
        {
            // Use keyboard/gamepad axis input on desktop and other non-mobile platforms.
            moveX = Input.GetAxisRaw("Horizontal");
            moveY = Input.GetAxisRaw("Vertical");
        }

        // Update movement vector
        movement = new Vector2(moveX,moveY).normalized;

        // Update animator's isWalking parameter (true if moving in any direction)
        animator.SetBool("isWalking",movement.sqrMagnitude > 0f);

        // Flip sprite based on horizontal direction
        if (Mathf.Abs(moveX) > 0f)
        {
            Vector3 characterScale = transform.localScale;
            characterScale.x = moveX > 0 ? 1 : -1;
            transform.localScale = characterScale;
        }
    }

    void FixedUpdate ()
    {
        // Calculate movement based on input
        Vector2 movementDelta = movement * speed;

        // Calculate potential new position
        Vector2 targetPosition = rb.position + movementDelta * Time.fixedDeltaTime;

        // Check for collision before applying movement
        if (!IsCollidingWithObstacle(targetPosition))
        {
            rb.velocity = movementDelta;
        }
        else
        {
            rb.velocity = Vector2.zero; // Stop movement if collision detected
        }

        // Check for trigger zones
        CheckForTriggerZones(targetPosition);
    }

    // Check if the player's future position would collide with any UI obstacles
    private bool IsCollidingWithObstacle (Vector2 targetPosition)
    {
        if (characterCollider == null)
        {
            return false; // No collider to check against
        }

        Vector2 currentPosition = rb.position;
        Vector2 direction = (targetPosition - currentPosition).normalized;
        float distance = Vector2.Distance(targetPosition,currentPosition);

        // Perform a BoxCast using the character's collider size
        RaycastHit2D hit = Physics2D.BoxCast(
            currentPosition,
            characterCollider.bounds.size,
            0f,
            direction,
            distance,
            obstacleLayer
        );

        // Only stop movement if the hit collider is NOT a trigger
        if (hit.collider != null && !hit.collider.isTrigger)
        {
            Debug.Log("Collision with UI obstacle detected");
            return true;  // Collision detected, prevent movement
        }

        return false;  // No collision detected, allow movement
    }


    // Check if the player is in a trigger zone
    private void CheckForTriggerZones (Vector2 targetPosition)
    {
        Collider2D[] triggers = Physics2D.OverlapPointAll(targetPosition,triggerLayer);

        HashSet<UITrigger> currentTriggers = new HashSet<UITrigger>();

        foreach (Collider2D trigger in triggers)
        {
            UITrigger uiTrigger = trigger.GetComponent<UITrigger>();

            if (uiTrigger != null)
            {
                currentTriggers.Add(uiTrigger);

                if (!activeTriggers.Contains(uiTrigger))
                {
                    uiTrigger.ShowUI();
                    activeTriggers.Add(uiTrigger);
                }
            }
        }

        // Check for triggers that are no longer active
        foreach (UITrigger uiTrigger in activeTriggers)
        {
            if (!currentTriggers.Contains(uiTrigger))
            {
                uiTrigger.HideUI();
            }
        }

        // Update the active triggers to the current ones
        activeTriggers = currentTriggers;

        // Only show the enter button while currently inside at least one trigger.
        bool isCurrentlyInTriggerZone = activeTriggers.Count > 0;
        if (isCurrentlyInTriggerZone)
        {
            UITrigger selectedTrigger = GetClosestTrigger(activeTriggers,targetPosition);
            EnterButton.Instance?.BindTrigger(selectedTrigger);
        }
        else
        {
            EnterButton.Instance?.UnbindTrigger();
        }

        if (isCurrentlyInTriggerZone != isInTriggerZone)
        {
            isInTriggerZone = isCurrentlyInTriggerZone;
            if (isInTriggerZone)
            {
                EnterButton.Instance?.Show();
            }
            else
            {
                EnterButton.Instance?.Hide();
            }
        }
    }

    // Chooses the nearest active trigger so the button activates the most relevant destination.
    private UITrigger GetClosestTrigger (HashSet<UITrigger> triggers, Vector2 origin)
    {
        UITrigger closestTrigger = null;
        float closestDistanceSquared = float.PositiveInfinity;

        foreach (UITrigger trigger in triggers)
        {
            if (trigger == null)
            {
                continue;
            }

            float distanceSquared = ((Vector2)trigger.transform.position - origin).sqrMagnitude;
            if (distanceSquared < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                closestTrigger = trigger;
            }
        }

        return closestTrigger;
    }
}
