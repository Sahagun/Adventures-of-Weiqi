using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LessonDisplayer : MonoBehaviour
{
    [Header("UI Elements")]
    public RawImage rawImage;
    public Sprite spriteSource;
    public GameObject textBox; // GameObject containing TextMeshProUGUI
    [TextArea] public string infoText;
    public Button startButton;

    [Header("Lesson Assignment")]
    public LessonSceneLauncher targetLessonButton;
    public GoLessonData lessonDataToAssign;

    [Header("Lock (like Puzzle Leveling)")]
    [Tooltip("Lock icon shown when this lesson's prerequisites aren't met. Hidden when unlocked.")]
    public GameObject lockOverlay;
    [Tooltip("This lesson row's button. Auto-found on this object if left empty; made non-interactable while locked.")]
    public Button lessonButton;

    private void Start()
    {
        if (rawImage != null)
            rawImage.gameObject.SetActive(false);

        if (startButton != null)
            startButton.gameObject.SetActive(false);

        RefreshLockState();
    }

    private void OnEnable()
    {
        RefreshLockState();
    }

    // Auto-fills the button + lock references in the editor so they show in the Inspector.
    private void OnValidate()
    {
        if (lessonButton == null)
            lessonButton = GetComponent<Button>();

        if (lockOverlay == null)
            lockOverlay = FindFirstChildImage();
    }

    // Shows the lock icon and blocks the row when the lesson's prerequisites aren't met.
    public void RefreshLockState()
    {
        if (lessonButton == null)
            lessonButton = GetComponent<Button>();

        if (lockOverlay == null)
            lockOverlay = FindFirstChildImage();

        bool unlocked = true;
        if (LessonUnlockManager.Instance != null && lessonDataToAssign != null)
            unlocked = LessonUnlockManager.Instance.IsLessonUnlocked(lessonDataToAssign);

        if (lockOverlay != null)
            lockOverlay.SetActive(!unlocked);

        if (lessonButton != null)
            lessonButton.interactable = unlocked;
    }

    // Grabs the first Image among children (excluding this object's own button image) to use as the lock icon.
    private GameObject FindFirstChildImage()
    {
        foreach (Image image in GetComponentsInChildren<Image>(true))
        {
            if (image != null && image.gameObject != gameObject)
                return image.gameObject;
        }
        return null;
    }

    public void OnButtonPressed()
    {
        if (rawImage != null && spriteSource != null)
        {
            rawImage.gameObject.SetActive(true);
            rawImage.texture = SpriteToTexture(spriteSource);
        }

        if (textBox != null)
        {
            TextMeshProUGUI tmpText = textBox.GetComponent<TextMeshProUGUI>();
            if (tmpText != null)
                tmpText.text = infoText;
        }

        if (startButton != null)
        {
            startButton.gameObject.SetActive(true);
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(AssignLessonToButton);
        }
    }

    private void AssignLessonToButton()
    {
        if (targetLessonButton == null)
        {
            Debug.LogWarning("LessonDisplayer is missing a targetLessonButton reference.");
            return;
        }

        if (lessonDataToAssign == null)
        {
            Debug.LogWarning("LessonDisplayer is missing a lessonDataToAssign reference.");
            return;
        }

        targetLessonButton.SetLessonData(lessonDataToAssign);
        Debug.Log($"Assigned lesson '{lessonDataToAssign.name}' to '{targetLessonButton.name}'.");

        // Launch immediately so the Start button starts the lesson in a single click
        // instead of only assigning it (which previously required a second click).
        targetLessonButton.LaunchLesson();
    }

    private Texture2D SpriteToTexture(Sprite sprite)
    {
        if (sprite == null)
            return null;

        if (sprite.rect.width != sprite.texture.width || sprite.rect.height != sprite.texture.height)
        {
            Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            Color[] newColors = sprite.texture.GetPixels(
                (int)sprite.textureRect.x,
                (int)sprite.textureRect.y,
                (int)sprite.textureRect.width,
                (int)sprite.textureRect.height);

            newText.SetPixels(newColors);
            newText.Apply();
            return newText;
        }

        return sprite.texture;
    }
}