using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LessonDisplayer : MonoBehaviour
{
    [Header("UI Elements")]
    public RawImage rawImage;
    public Sprite spriteSource;
    public GameObject textBox; // GameObject containing TextMeshProUGUI
    public string infoText;
    public Button startButton;

    [Header("Scene Settings")]
    public int sceneToLoad;
    public string messageToSend;

    private void Start()
    {
        // Hide raw image and start button initially
        if (rawImage != null)
            rawImage.gameObject.SetActive(false);
        if (startButton != null)
            startButton.gameObject.SetActive(false);
    }

    public void OnButtonPressed()
    {
        // Show RawImage and apply Sprite
        if (rawImage != null && spriteSource != null)
        {
            rawImage.gameObject.SetActive(true);
            rawImage.texture = SpriteToTexture(spriteSource);
        }

        // Replace text in TextMeshPro text box
        if (textBox != null)
        {
            TextMeshProUGUI tmpText = textBox.GetComponent<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = infoText;
            }
        }

        // Show and configure the start button
        if (startButton != null)
        {
            startButton.gameObject.SetActive(true);
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(() => LoadSceneWithMessage());
        }
    }

    private void LoadSceneWithMessage()
    {
        PlayerPrefs.SetString("SceneMessage", messageToSend);
        SceneManager.LoadScene(sceneToLoad);
    }

    private Texture2D SpriteToTexture(Sprite sprite)
    {
        if (sprite.rect.width != sprite.texture.width)
        {
            // Create new texture from sprite
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
