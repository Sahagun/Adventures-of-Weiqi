using System.IO;
using UnityEditor;
using UnityEngine;

public class GoLessonBuilderWindow : EditorWindow
{
    private const string LessonInfoProperty = "lessonTitle";
    private const string LessonIdProperty = "lessonId";
    private const string SlidesProperty = "slides";
    private const string SlideNameProperty = "slideName";
    private const string SlideTypeProperty = "slideType";
    private const string SlideBodyTextProperty = "bodyText";
    private const string SlideBoardJsonProperty = "boardJsonFile";
    private const string SlideCorrectYesProperty = "correctYesAnswer";
    private const string SlideCorrectNumberProperty = "correctNumberAnswer";

    private GoLessonData selectedLesson;
    private SerializedObject serializedLesson;
    private Vector2 scrollPosition;

    [MenuItem("Tools/Go Lesson Builder")]
    public static void OpenWindow()
    {
        GetWindow<GoLessonBuilderWindow>("Go Lesson Builder");
    }

    public static void OpenWindow(GoLessonData lessonData)
    {
        GoLessonBuilderWindow window = GetWindow<GoLessonBuilderWindow>("Go Lesson Builder");
        window.SetSelectedLesson(lessonData);
    }

    private void OnEnable()
    {
        if (selectedLesson == null && Selection.activeObject is GoLessonData lessonData)
            SetSelectedLesson(lessonData);
    }

    private void OnSelectionChange()
    {
        if (Selection.activeObject is GoLessonData lessonData)
        {
            SetSelectedLesson(lessonData);
            Repaint();
        }
    }

    private void OnGUI()
    {
        DrawHeader();
        DrawLessonSelection();

        if (selectedLesson == null)
        {
            EditorGUILayout.HelpBox("Select an existing lesson asset or create a new one to start building slides.",MessageType.Info);
            return;
        }

        EnsureSerializedLesson();
        serializedLesson.Update();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        DrawLessonMetadata();
        GUILayout.Space(8f);
        DrawSlides();
        EditorGUILayout.EndScrollView();

        serializedLesson.ApplyModifiedProperties();
        if (GUI.changed)
            EditorUtility.SetDirty(selectedLesson);
    }

    private void DrawHeader()
    {
        EditorGUILayout.LabelField("Go Lesson Builder",EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Author lesson slides, then launch them in PuzzleLeveling through LessonSceneLauncher.",EditorStyles.wordWrappedMiniLabel);
        GUILayout.Space(8f);
    }

    private void DrawLessonSelection()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        GoLessonData lessonSelection = (GoLessonData)EditorGUILayout.ObjectField("Lesson Asset",selectedLesson,typeof(GoLessonData),false);
        if (lessonSelection != selectedLesson)
            SetSelectedLesson(lessonSelection);

        if (GUILayout.Button("New Lesson",GUILayout.Width(100f)))
            CreateLessonAsset();

        using (new EditorGUI.DisabledScope(selectedLesson == null))
        {
            if (GUILayout.Button("Ping",GUILayout.Width(60f)))
                EditorGUIUtility.PingObject(selectedLesson);
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawLessonMetadata()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Lesson",EditorStyles.boldLabel);

        SerializedProperty lessonTitle = serializedLesson.FindProperty(LessonInfoProperty);
        SerializedProperty lessonId = serializedLesson.FindProperty(LessonIdProperty);

        EditorGUILayout.PropertyField(lessonTitle,new GUIContent("Lesson Name"));
        EditorGUILayout.PropertyField(lessonId,new GUIContent("Lesson Id"));
        DrawAssetFileNameField();
        EditorGUILayout.EndVertical();
    }

    private void DrawAssetFileNameField()
    {
        if (selectedLesson == null)
            return;

        string assetPath = AssetDatabase.GetAssetPath(selectedLesson);
        string assetFileName = string.IsNullOrWhiteSpace(assetPath) ? string.Empty : Path.GetFileNameWithoutExtension(assetPath);
        string newAssetFileName = EditorGUILayout.DelayedTextField("Asset File Name",assetFileName);

        if (string.IsNullOrWhiteSpace(newAssetFileName) || newAssetFileName == assetFileName)
            return;

        string renameResult = AssetDatabase.RenameAsset(assetPath,newAssetFileName);
        if (!string.IsNullOrWhiteSpace(renameResult))
            Debug.LogWarning($"Could not rename lesson asset: {renameResult}");
    }

    private void DrawSlides()
    {
        SerializedProperty slides = serializedLesson.FindProperty(SlidesProperty);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Slides ({slides.arraySize})",EditorStyles.boldLabel);

        if (GUILayout.Button("+ New Slide",GUILayout.Width(110f)))
            AddSlide(slides);

        EditorGUILayout.EndHorizontal();

        if (slides.arraySize == 0)
        {
            EditorGUILayout.HelpBox("This lesson has no slides yet.",MessageType.Info);
            EditorGUILayout.EndVertical();
            return;
        }

        for (int i = 0; i < slides.arraySize; i++)
        {
            SerializedProperty slideProperty = slides.GetArrayElementAtIndex(i);
            DrawSlide(slides,slideProperty,i);
            GUILayout.Space(6f);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawSlide(SerializedProperty slides,SerializedProperty slideProperty,int slideIndex)
    {
        SerializedProperty slideName = slideProperty.FindPropertyRelative(SlideNameProperty);
        SerializedProperty slideType = slideProperty.FindPropertyRelative(SlideTypeProperty);
        SerializedProperty bodyText = slideProperty.FindPropertyRelative(SlideBodyTextProperty);
        SerializedProperty boardJsonFile = slideProperty.FindPropertyRelative(SlideBoardJsonProperty);
        SerializedProperty correctYesAnswer = slideProperty.FindPropertyRelative(SlideCorrectYesProperty);
        SerializedProperty correctNumberAnswer = slideProperty.FindPropertyRelative(SlideCorrectNumberProperty);

        string slideTitle = string.IsNullOrWhiteSpace(slideName.stringValue) ? $"Slide {slideIndex + 1}" : slideName.stringValue;
        string slideTypeLabel = ((GoLessonSlideType)slideType.enumValueIndex).ToString();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"{slideIndex + 1}. {slideTitle} [{slideTypeLabel}]",EditorStyles.boldLabel);

        if (GUILayout.Button("Duplicate",GUILayout.Width(78f)))
        {
            DuplicateSlide(slides,slideIndex);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }

        using (new EditorGUI.DisabledScope(slideIndex <= 0))
        {
            if (GUILayout.Button("Up",GUILayout.Width(44f)))
            {
                slides.MoveArrayElement(slideIndex,slideIndex - 1);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }
        }

        using (new EditorGUI.DisabledScope(slideIndex >= slides.arraySize - 1))
        {
            if (GUILayout.Button("Down",GUILayout.Width(52f)))
            {
                slides.MoveArrayElement(slideIndex,slideIndex + 1);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }
        }

        if (GUILayout.Button("Delete",GUILayout.Width(60f)))
        {
            slides.DeleteArrayElementAtIndex(slideIndex);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(slideName,new GUIContent("Slide Name"));
        EditorGUILayout.PropertyField(slideType,new GUIContent("Slide Type"));
        EditorGUILayout.PropertyField(boardJsonFile,new GUIContent("Json File"));
        EditorGUILayout.PropertyField(bodyText,new GUIContent("Enter Text"));

        GoLessonSlideType selectedType = (GoLessonSlideType)slideType.enumValueIndex;
        if (selectedType == GoLessonSlideType.YesNo)
            EditorGUILayout.PropertyField(correctYesAnswer,new GUIContent("Correct Answer"));
        else if (selectedType == GoLessonSlideType.Number)
            EditorGUILayout.PropertyField(correctNumberAnswer,new GUIContent("Correct Number"));

        EditorGUILayout.EndVertical();
    }

    private void CreateLessonAsset()
    {
        string assetPath = EditorUtility.SaveFilePanelInProject(
            "Create Go Lesson",
            "NewGoLesson",
            "asset",
            "Choose where to save the new lesson asset.");

        if (string.IsNullOrWhiteSpace(assetPath))
            return;

        GoLessonData lessonData = CreateInstance<GoLessonData>();
        lessonData.lessonTitle = Path.GetFileNameWithoutExtension(assetPath);
        lessonData.lessonId = SanitizeId(lessonData.lessonTitle);
        AssetDatabase.CreateAsset(lessonData,assetPath);
        AssetDatabase.SaveAssets();
        SetSelectedLesson(lessonData);
        Selection.activeObject = lessonData;
    }

    private void AddSlide(SerializedProperty slides)
    {
        int newIndex = slides.arraySize;
        slides.InsertArrayElementAtIndex(newIndex);
        SerializedProperty newSlide = slides.GetArrayElementAtIndex(newIndex);
        ResetSlideValues(newSlide,newIndex);
    }

    private void DuplicateSlide(SerializedProperty slides,int sourceIndex)
    {
        slides.InsertArrayElementAtIndex(sourceIndex);
        slides.MoveArrayElement(sourceIndex,sourceIndex + 1);
    }

    private void ResetSlideValues(SerializedProperty slideProperty,int slideIndex)
    {
        slideProperty.FindPropertyRelative(SlideNameProperty).stringValue = $"Slide {slideIndex + 1}";
        slideProperty.FindPropertyRelative(SlideTypeProperty).enumValueIndex = (int)GoLessonSlideType.Content;
        slideProperty.FindPropertyRelative(SlideBodyTextProperty).stringValue = string.Empty;
        slideProperty.FindPropertyRelative(SlideBoardJsonProperty).objectReferenceValue = null;
        slideProperty.FindPropertyRelative(SlideCorrectYesProperty).boolValue = true;
        slideProperty.FindPropertyRelative(SlideCorrectNumberProperty).intValue = 0;
    }

    private void EnsureSerializedLesson()
    {
        if (selectedLesson == null)
        {
            serializedLesson = null;
            return;
        }

        if (serializedLesson == null || serializedLesson.targetObject != selectedLesson)
            serializedLesson = new SerializedObject(selectedLesson);
    }

    private void SetSelectedLesson(GoLessonData lessonData)
    {
        selectedLesson = lessonData;
        serializedLesson = lessonData != null ? new SerializedObject(lessonData) : null;
    }

    private string SanitizeId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "new_lesson";

        string trimmed = value.Trim().ToLowerInvariant();
        return trimmed.Replace(" ","_");
    }
}
