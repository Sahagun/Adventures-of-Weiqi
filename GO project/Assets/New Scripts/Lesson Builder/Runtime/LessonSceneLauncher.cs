using EditorAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LessonSceneLauncher : MonoBehaviour
{
    [Title("Lesson")]
    [SerializeField] private GoLessonData lessonData;
    [SerializeField] private bool autoApplyLessonTitleToButtonLabel = true;
    [SerializeField] private Button launchButton;
    [SerializeField] private TMP_Text buttonLabelText;

    [Title("Scene")]
    [SceneDropdown] public int lessonSceneToLoad = -1;

    private void Awake()
    {
        ResolveReferences();
        BindButton();
        RefreshButtonLabel();
    }

    private void OnValidate()
    {
        ResolveReferences();
        RefreshButtonLabel();
    }

    public void LaunchLesson()
    {
        if (lessonData == null)
        {
            Debug.LogWarning("LessonSceneLauncher is missing a lessonData reference.");
            return;
        }

        if (lessonSceneToLoad < 0)
        {
            Debug.LogWarning("LessonSceneLauncher lessonSceneToLoad is not configured.");
            return;
        }

        if (PuzzlePersist.Instance == null)
        {
            Debug.LogWarning("PuzzlePersist is required before launching a lesson.");
            return;
        }

        PuzzlePersist.Instance.BeginLessonSession(lessonData);
        LoadLessonScene();
    }

    public void ResolveReferences()
    {
        if (launchButton == null)
            launchButton = GetComponent<Button>();

        if (buttonLabelText == null)
            buttonLabelText = GetComponentInChildren<TMP_Text>(true);
    }

    private void BindButton()
    {
        if (launchButton == null)
            return;

        launchButton.onClick.RemoveListener(LaunchLesson);
        launchButton.onClick.AddListener(LaunchLesson);
    }

    private void RefreshButtonLabel()
    {
        if (!autoApplyLessonTitleToButtonLabel || buttonLabelText == null || lessonData == null)
            return;

        buttonLabelText.text = lessonData.GetDisplayTitle();
    }

    private void LoadLessonScene()
    {
        SceneLoader sceneLoader = GetComponent<SceneLoader>() ?? GetComponentInParent<SceneLoader>() ?? FindObjectOfType<SceneLoader>();
        if (sceneLoader != null)
        {
            sceneLoader.LoadThisSceneNumber(lessonSceneToLoad);
            return;
        }

        if (Moddwyn.SceneLoader.Instance != null)
        {
            Moddwyn.SceneLoader.Instance.LoadScene(lessonSceneToLoad);
            return;
        }

        SceneManager.LoadScene(lessonSceneToLoad);
    }

    public void SetLessonData(GoLessonData newLessonData)
    {
        lessonData = newLessonData;
        RefreshButtonLabel();
    }
}
