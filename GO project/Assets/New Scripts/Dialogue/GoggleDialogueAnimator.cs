using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Goggle's dialogue presence (the flying guy; Heiqi is the player).
/// Goggle idles hidden above, flies DOWN beside Heiqi when a story conversation starts, plays
/// his sprite animation, then flies back UP when it ends. While shown he follows the player,
/// keeping the offset he started with.
///
/// IMPORTANT: Goggle must be a SIBLING of the player (same parent), NOT a child of it - the
/// player flips localScale.x to face left/right, and any child would be mirrored. Put Goggle
/// directly under the Canvas next to Character, and set Follow Target = the Character (player).
///
/// Works for any Canvas render mode (Overlay / Camera / World Space) because it follows in the
/// parent's local space, not via the camera.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class GoggleDialogueAnimator : MonoBehaviour
{
    [Header("Follow")]
    [Tooltip("The player (Heiqi). Goggle must be a SIBLING of this, not a child, or the player's " +
             "facing-flip mirrors Goggle.")]
    [SerializeField] private Transform followTarget;

    [Header("Sprite animation")]
    [SerializeField] private Image image;
    [SerializeField] private List<Sprite> frames = new();
    [Min(0f)] [SerializeField] private float fps = 8f;

    [Header("Hide / fly")]
    [Tooltip("How far ABOVE his shown spot Goggle hides (anchored units). He flies down from here.")]
    [SerializeField] private float hideRise = 473f;
    [Tooltip("Smoothing for both the fly in/out and the follow. Smaller = snappier.")]
    [Min(0.0001f)] [SerializeField] private float followSmoothTime = 0.25f;

    private RectTransform rect;
    private RectTransform parentRect;
    private Vector2 showingAnchored;
    private Vector2 followOffset;
    private bool offsetCaptured;
    private Vector2 velocity;
    private int frameIndex;
    private float frameTimer;

    private void OnValidate()
    {
        if (image == null)
            image = GetComponent<Image>();
    }

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        parentRect = rect.parent as RectTransform;
        if (image == null)
            image = GetComponent<Image>();

        showingAnchored = rect.anchoredPosition;
        rect.anchoredPosition = showingAnchored + Vector2.up * hideRise; // start hidden above

        if (frames != null && frames.Count > 0 && image != null)
            image.sprite = frames[0];
    }

    private void LateUpdate()
    {
        EnsureOffset();

        Vector2 followBase = ComputeFollowBase();
        bool shown = StoryDialogueRunner.IsAnyDialogueActive;
        Vector2 desired = shown ? followBase : followBase + Vector2.up * hideRise;

        rect.anchoredPosition = Vector2.SmoothDamp(rect.anchoredPosition, desired, ref velocity, followSmoothTime);
        AdvanceAnimation();
    }

    private void EnsureOffset()
    {
        if (offsetCaptured || followTarget == null)
            return;

        followOffset = showingAnchored - TargetInParentSpace();
        offsetCaptured = true;
    }

    private Vector2 ComputeFollowBase()
    {
        if (followTarget == null)
            return showingAnchored;

        return TargetInParentSpace() + followOffset;
    }

    // The player's position expressed in Goggle's parent's local space, so following is just
    // "player + offset" with no camera math. Immune to the player's facing-flip (uses position).
    private Vector2 TargetInParentSpace()
    {
        if (parentRect == null)
            return showingAnchored;

        Vector3 local = parentRect.InverseTransformPoint(followTarget.position);
        return new Vector2(local.x, local.y);
    }

    private void AdvanceAnimation()
    {
        if (image == null || frames == null || frames.Count == 0 || fps <= 0f)
            return;

        frameTimer += Time.deltaTime;
        float frameDuration = 1f / fps;
        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            frameIndex = (frameIndex + 1) % frames.Count;
            image.sprite = frames[frameIndex];
        }
    }
}
