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

    private void Start()
    {
        if (rawImage != null)
            rawImage.gameObject.SetActive(false);

        if (startButton != null)
            startButton.gameObject.SetActive(false);
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