using EditorAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LessonModeUi : MonoBehaviour
{
    [Title("Supplemental Lesson UI")]
    [SerializeField] private GameObject lessonRootUi;
    [SerializeField] private Button lessonYesButton;
    [SerializeField] private Button lessonNoButton;

    [Title("Number Answer")]
    [SerializeField] private TMP_InputField lessonNumberInputField;

    public Button LessonYesButton => lessonYesButton;
    public Button LessonNoButton => lessonNoButton;
    public TMP_InputField LessonNumberInputField => lessonNumberInputField;

    private bool isLessonUiVisible = true;
    private bool isYesNoVisible = false;
    private bool isNumberInputVisible = false;

    private void Awake()
    {
        ResolveReferences();
    }

    public void ResolveReferences()
    {
        if (lessonRootUi == null)
            lessonRootUi = FindNamedGameObject("LessonRootUI","LessonSupplementalUI","Lesson Controls","Lesson Extras");

        if (lessonYesButton == null)
            lessonYesButton = FindButtonInRoot(lessonRootUi,"LessonYesButton","YesButton");

        if (lessonNoButton == null)
            lessonNoButton = FindButtonInRoot(lessonRootUi,"LessonNoButton","NoButton");

        if (lessonNumberInputField == null)
            lessonNumberInputField = FindInputFieldInRoot(lessonRootUi,"LessonNumberInput","NumberInputField","InputField (TMP)");
    }

    public void SetVisible(bool isVisible)
    {
        isLessonUiVisible = isVisible;
        UpdateLessonRootVisibility();
    }

    public void SetYesNoVisible(bool isVisible)
    {
        isYesNoVisible = isVisible;

        if (lessonYesButton != null)
            lessonYesButton.gameObject.SetActive(isVisible);
        if (lessonNoButton != null)
            lessonNoButton.gameObject.SetActive(isVisible);

        UpdateLessonRootVisibility();
    }

    public void SetNumberInputVisible(bool isVisible)
    {
        isNumberInputVisible = isVisible;

        if (lessonNumberInputField != null)
            lessonNumberInputField.gameObject.SetActive(isVisible);

        UpdateLessonRootVisibility();
    }

    public void ClearNumberInput()
    {
        if (lessonNumberInputField != null)
            lessonNumberInputField.text = string.Empty;
    }

    public string GetNumberInputValue()
    {
        return lessonNumberInputField != null ? lessonNumberInputField.text : string.Empty;
    }

    private void UpdateLessonRootVisibility()
    {
        if (lessonRootUi != null)
            lessonRootUi.SetActive(isLessonUiVisible && (isYesNoVisible || isNumberInputVisible));
    }

    private GameObject FindNamedGameObject(params string[] candidateNames)
    {
        foreach (Transform transform in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (transform == null || !transform.gameObject.scene.IsValid())
                continue;

            GameObject gameObject = transform.gameObject;
            if (MatchesAnyName(gameObject.name,candidateNames))
                return gameObject;
        }

        return null;
    }

    private Button FindButtonInRoot(GameObject root,params string[] candidateNames)
    {
        return root != null
            ? FindChildComponentByName<Button>(root,candidateNames)
            : FindComponentByName<Button>(candidateNames);
    }

    private TMP_InputField FindInputFieldInRoot(GameObject root,params string[] candidateNames)
    {
        return root != null
            ? FindChildComponentByName<TMP_InputField>(root,candidateNames)
            : FindComponentByName<TMP_InputField>(candidateNames);
    }

    private T FindChildComponentByName<T>(GameObject root,params string[] candidateNames) where T : Component
    {
        if (root == null)
            return null;

        foreach (T component in root.GetComponentsInChildren<T>(true))
        {
            if (component != null && MatchesAnyName(component.name,candidateNames))
                return component;
        }

        return null;
    }

    private T FindComponentByName<T>(params string[] candidateNames) where T : Component
    {
        foreach (T component in FindObjectsOfType<T>(true))
        {
            if (component != null && MatchesAnyName(component.name,candidateNames))
                return component;
        }

        return null;
    }

    private bool MatchesAnyName(string candidateName,params string[] expectedNames)
    {
        if (string.IsNullOrWhiteSpace(candidateName) || expectedNames == null)
            return false;

        foreach (string expectedName in expectedNames)
        {
            if (string.IsNullOrWhiteSpace(expectedName))
                continue;

            if (string.Equals(candidateName,expectedName,System.StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
