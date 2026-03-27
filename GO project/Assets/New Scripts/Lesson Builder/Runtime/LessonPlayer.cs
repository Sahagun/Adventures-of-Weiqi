using EditorAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LessonPlayer : MonoBehaviour
{
    [Title("Scene References")]
    [SerializeField] private LessonModeUi lessonModeUi;
    [SerializeField] private GameUiManager gameUiManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ConditionManager conditionManager;
    [SerializeField] private CubeGrid cubeGrid;

    [Title("Lesson Navigation")]
    [SerializeField] private bool autoBindButtons = true;
    [SceneDropdown] public int returnToLessonSelectionSceneIndex = -1;

    [Title("Button Labels")]
    [SerializeField] private string nextSlideButtonText = "Next";
    [SerializeField] private string finishLessonButtonText = "Finish";
    [SerializeField] private string retryPuzzleButtonText = "Restart";
    [SerializeField] private string quitLessonButtonText = "Quit";

    [Title("Status Text")]
    [SerializeField] private string defaultSlideStatusText = string.Empty;
    [SerializeField] private string puzzlePromptStatusText = "Solve the puzzle.";
    [SerializeField] private string puzzleSolvedStatusText = "Puzzle complete. Continue.";
    [SerializeField] private string puzzleFailedStatusText = "Wrong move. Restart and try again.";
    [SerializeField] private string correctAnswerStatusText = "Correct.";
    [SerializeField] private string incorrectAnswerStatusText = "Incorrect. Try again.";

    [Title("Runtime State")]
    [ReadOnly] [SerializeField] private bool lessonModeActive = false;
    [ReadOnly] [SerializeField] private int currentSlideIndex = 0;
    [ReadOnly] [SerializeField] private GoLessonSlideType currentSlideType = GoLessonSlideType.Content;
    [ReadOnly] [SerializeField] private bool currentSlideResolved = false;

    private GoLessonData activeLessonData;
    private GoLessonSlideData currentSlideData;
    private bool hasShownPuzzleFailureState = false;

    private void Awake()
    {
        ResolveReferences();
    }

    private void Start()
    {
        ResolveReferences();
        InitializeLessonMode();
    }

    private void Update()
    {
        if (!lessonModeActive)
            return;

        if (currentSlideType == GoLessonSlideType.Number && lessonModeUi != null)
        {
            bool hasAnyInput = !string.IsNullOrWhiteSpace(lessonModeUi.GetNumberInputValue());
            if (gameUiManager != null)
                gameUiManager.SetGameplayNextPuzzleButtonInteractable(hasAnyInput);
        }

        if (currentSlideType != GoLessonSlideType.Puzzle || currentSlideResolved)
            return;

        if (conditionManager != null && conditionManager.winTriggered)
        {
            currentSlideResolved = true;
            ShowPuzzleSolvedState();
            return;
        }

        if (gameManager != null && gameManager.gameIsOver && !hasShownPuzzleFailureState)
            ShowPuzzleFailedState();
    }

    public void ResolveReferences()
    {
        if (lessonModeUi == null)
            lessonModeUi = FindObjectOfType<LessonModeUi>(true);

        if (gameUiManager == null)
            gameUiManager = FindObjectOfType<GameUiManager>(true);

        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (conditionManager == null)
            conditionManager = FindObjectOfType<ConditionManager>();

        if (cubeGrid == null)
            cubeGrid = FindObjectOfType<CubeGrid>();

        if (lessonModeUi != null)
            lessonModeUi.ResolveReferences();

        if (gameUiManager != null)
            gameUiManager.ResolveReferences();
    }

    public void RestartCurrentLessonSlide()
    {
        PuzzlePersist puzzlePersist = PuzzlePersist.Instance;
        if (puzzlePersist == null || !puzzlePersist.IsLessonModeActive)
            return;

        puzzlePersist.SetLessonSlideIndex(currentSlideIndex);
        ApplyCurrentSlideInScene();
    }

    public void QuitLesson()
    {
        if (returnToLessonSelectionSceneIndex < 0)
        {
            Debug.LogWarning("LessonPlayer returnToLessonSelectionSceneIndex is not configured.");
            return;
        }

        if (PuzzlePersist.Instance != null)
            PuzzlePersist.Instance.ClearLessonSession();

        LoadScene(returnToLessonSelectionSceneIndex);
    }

    public void RestartLessonFromBeginning()
    {
        PuzzlePersist puzzlePersist = PuzzlePersist.Instance;
        if (puzzlePersist == null || puzzlePersist.activeLessonData == null)
            return;

        puzzlePersist.BeginLessonSession(puzzlePersist.activeLessonData);
        ApplyCurrentSlideInScene();
    }

    private void InitializeLessonMode()
    {
        PuzzlePersist puzzlePersist = PuzzlePersist.Instance;
        if (puzzlePersist == null || !puzzlePersist.IsLessonModeActive || puzzlePersist.activeLessonData == null)
        {
            lessonModeActive = false;
            activeLessonData = null;
            currentSlideData = null;
            if (lessonModeUi != null)
                lessonModeUi.SetVisible(false);
            enabled = false;
            return;
        }

        activeLessonData = puzzlePersist.activeLessonData;
        if (activeLessonData.SlideCount == 0)
        {
            lessonModeActive = true;
            ConfigureSharedGameplayUi();
            ShowLessonCompletionOverlay("Lesson complete.\nThis lesson has no slides.");
            return;
        }

        lessonModeActive = true;
        currentSlideIndex = activeLessonData.ClampSlideIndex(puzzlePersist.activeLessonSlideIndex);
        puzzlePersist.SetLessonSlideIndex(currentSlideIndex);

        currentSlideData = activeLessonData.GetSlide(currentSlideIndex);
        if (currentSlideData == null)
        {
            Debug.LogWarning("LessonPlayer could not load the active lesson slide.");
            lessonModeActive = false;
            enabled = false;
            return;
        }

        currentSlideType = currentSlideData.slideType;
        currentSlideResolved = false;
        hasShownPuzzleFailureState = false;

        ConfigureSharedGameplayUi();
        ConfigureSlideUi();
        BindLessonButtons();
    }

    private void ConfigureSharedGameplayUi()
    {
        if (gameManager != null)
            gameManager.StopTimer();

        if (gameUiManager != null)
        {
            gameUiManager.SetEndGameVisible(false);
            gameUiManager.SetConditionFinalScoreVisible(false);
            gameUiManager.SetConditionTurnLimitVisible(false);
            gameUiManager.SetGameplayTimerVisible(false);
            gameUiManager.SetGameplayTurnCounterVisible(false);
            gameUiManager.SetGameplayPassTurnVisible(false);
            gameUiManager.SetGameplayNextPuzzleButtonVisible(false);
            gameUiManager.SetGameplayQuitPuzzleButtonVisible(false);
            gameUiManager.SetGameplayRestartCurrentPuzzleButtonVisible(false);
        }
    }

    private void ConfigureSlideUi()
    {
        if (activeLessonData == null || currentSlideData == null)
            return;

        bool isPuzzleSlide = currentSlideData.RequiresPuzzleCompletion;

        if (gameUiManager != null)
        {
            gameUiManager.SetGameplayDifficultyVisible(true);
            gameUiManager.SetGameplayDifficultyText(BuildLessonHeaderText());
            gameUiManager.SetGameplayMainStatusVisible(true);
            gameUiManager.SetGameplayMainStatusText(BuildMainStatusText(isPuzzleSlide ? puzzlePromptStatusText : defaultSlideStatusText));
            gameUiManager.SetGameplayRestartCurrentPuzzleButtonLabel(retryPuzzleButtonText);
            gameUiManager.SetGameplayQuitPuzzleButtonLabel(quitLessonButtonText);
            gameUiManager.SetGameplayQuitPuzzleButtonVisible(true);
            gameUiManager.SetGameplayQuitPuzzleButtonInteractable(true);
            gameUiManager.SetGameplayNextPuzzleButtonLabel(IsOnLastSlide() ? finishLessonButtonText : nextSlideButtonText);
            gameUiManager.SetGameplayRestartCurrentPuzzleButtonVisible(isPuzzleSlide);
            gameUiManager.SetGameplayRestartCurrentPuzzleButtonInteractable(isPuzzleSlide);

            if (currentSlideType == GoLessonSlideType.Content)
            {
                gameUiManager.SetGameplayNextPuzzleButtonVisible(true);
                gameUiManager.SetGameplayNextPuzzleButtonInteractable(true);
            }
            else if (currentSlideType == GoLessonSlideType.Number)
            {
                gameUiManager.SetGameplayNextPuzzleButtonVisible(true);
                gameUiManager.SetGameplayNextPuzzleButtonInteractable(false);
            }
            else
            {
                gameUiManager.SetGameplayNextPuzzleButtonVisible(false);
                gameUiManager.SetGameplayNextPuzzleButtonInteractable(false);
            }
        }

        ClickManager.SetGlobalInputEnabled(isPuzzleSlide);

        if (lessonModeUi != null)
        {
            lessonModeUi.SetVisible(true);
            lessonModeUi.SetYesNoVisible(currentSlideData.UsesYesNoAnswer);
            lessonModeUi.SetNumberInputVisible(currentSlideData.UsesNumberAnswer);
            lessonModeUi.ClearNumberInput();
        }
    }

    private void BindLessonButtons()
    {
        if (!autoBindButtons)
            return;

        if (gameUiManager != null)
        {
            BindButton(gameUiManager.GameplayNextPuzzleButton,HandleNextButtonPressed);
            BindButton(gameUiManager.GameplayQuitPuzzleButton,QuitLesson);
            BindButton(gameUiManager.GameplayRestartCurrentPuzzleButton,RestartCurrentLessonSlide);
        }

        if (lessonModeUi != null)
        {
            BindButton(lessonModeUi.LessonYesButton,HandleYesButtonPressed);
            BindButton(lessonModeUi.LessonNoButton,HandleNoButtonPressed);
        }
    }

    private void HandleNextButtonPressed()
    {
        if (!lessonModeActive)
            return;

        switch (currentSlideType)
        {
            case GoLessonSlideType.Content:
                AdvanceToNextSlideOrFinish();
                break;

            case GoLessonSlideType.Puzzle:
                if (currentSlideResolved)
                    AdvanceToNextSlideOrFinish();
                break;

            case GoLessonSlideType.Number:
                SubmitNumberAnswer();
                break;
        }
    }

    private void HandleYesButtonPressed()
    {
        SubmitYesNoAnswer(true);
    }

    private void HandleNoButtonPressed()
    {
        SubmitYesNoAnswer(false);
    }

    private void SubmitYesNoAnswer(bool answer)
    {
        if (!lessonModeActive || currentSlideType != GoLessonSlideType.YesNo)
            return;

        if (currentSlideData == null)
            return;

        if (answer == currentSlideData.correctYesAnswer)
        {
            AdvanceToNextSlideOrFinish();
            return;
        }

        RecordWrongAnswerAndStay();
    }

    private void SubmitNumberAnswer()
    {
        if (!lessonModeActive || currentSlideType != GoLessonSlideType.Number || lessonModeUi == null)
            return;

        if (currentSlideData == null)
            return;

        string rawInput = lessonModeUi.GetNumberInputValue();
        if (!int.TryParse(rawInput,out int enteredNumber) || enteredNumber != currentSlideData.correctNumberAnswer)
        {
            RecordWrongAnswerAndStay();
            return;
        }

        AdvanceToNextSlideOrFinish();
    }

    private void AdvanceToNextSlideOrFinish()
    {
        PuzzlePersist puzzlePersist = PuzzlePersist.Instance;
        if (puzzlePersist == null || puzzlePersist.activeLessonData == null)
            return;

        int nextSlideIndex = currentSlideIndex + 1;
        if (nextSlideIndex < puzzlePersist.activeLessonData.SlideCount)
        {
            puzzlePersist.SetLessonSlideIndex(nextSlideIndex);
            ApplyCurrentSlideInScene();
            return;
        }

        ShowLessonCompletionOverlay(BuildLessonCompletionMessage());
    }

    private void ShowPuzzleSolvedState()
    {
        if (gameUiManager != null)
        {
            gameUiManager.SetGameplayMainStatusText(BuildMainStatusText(puzzleSolvedStatusText));
            gameUiManager.SetGameplayNextPuzzleButtonLabel(IsOnLastSlide() ? finishLessonButtonText : nextSlideButtonText);
            gameUiManager.SetGameplayNextPuzzleButtonInteractable(true);
            gameUiManager.SetGameplayNextPuzzleButtonVisible(true);
        }
    }

    private void ShowPuzzleFailedState()
    {
        hasShownPuzzleFailureState = true;

        if (gameUiManager != null)
            gameUiManager.SetGameplayMainStatusText(BuildMainStatusText(puzzleFailedStatusText));
    }

    private void RecordWrongAnswerAndStay()
    {
        if (PuzzlePersist.Instance != null)
            PuzzlePersist.Instance.RecordLessonWrongAnswer();

        if (gameUiManager != null)
            gameUiManager.SetGameplayMainStatusText(BuildMainStatusText(incorrectAnswerStatusText));
    }

    private string BuildLessonHeaderText()
    {
        if (activeLessonData == null)
            return $"[{currentSlideIndex + 1}] Lesson";

        return $"[{currentSlideIndex + 1}/{activeLessonData.SlideCount}] {activeLessonData.GetDisplayTitle()}";
    }

    private string BuildMainStatusText(string statusMessage)
    {
        string bodyText = currentSlideData != null ? currentSlideData.bodyText : string.Empty;
        string trimmedBody = string.IsNullOrWhiteSpace(bodyText) ? string.Empty : bodyText.Trim();
        string trimmedStatus = string.IsNullOrWhiteSpace(statusMessage) ? string.Empty : statusMessage.Trim();

        int wrongAnswerCount = PuzzlePersist.Instance != null ? PuzzlePersist.Instance.activeLessonWrongAnswerCount : 0;
        string wrongAnswerText = wrongAnswerCount > 0 ? $"Wrong Answers: {wrongAnswerCount}" : string.Empty;

        if (string.IsNullOrWhiteSpace(trimmedBody) && string.IsNullOrWhiteSpace(trimmedStatus))
            return wrongAnswerText;

        if (string.IsNullOrWhiteSpace(trimmedStatus))
            return string.IsNullOrWhiteSpace(wrongAnswerText) ? trimmedBody : $"{trimmedBody}\n\n{wrongAnswerText}";

        if (string.IsNullOrWhiteSpace(trimmedBody))
            return string.IsNullOrWhiteSpace(wrongAnswerText) ? trimmedStatus : $"{trimmedStatus}\n\n{wrongAnswerText}";

        return string.IsNullOrWhiteSpace(wrongAnswerText)
            ? $"{trimmedBody}\n\n{trimmedStatus}"
            : $"{trimmedBody}\n\n{trimmedStatus}\n{wrongAnswerText}";
    }

    private string BuildLessonCompletionMessage()
    {
        PuzzlePersist puzzlePersist = PuzzlePersist.Instance;
        string lessonTitle = puzzlePersist != null && puzzlePersist.activeLessonData != null
            ? puzzlePersist.activeLessonData.GetDisplayTitle()
            : "Lesson";
        int slideCount = puzzlePersist != null && puzzlePersist.activeLessonData != null
            ? puzzlePersist.activeLessonData.SlideCount
            : 0;
        int wrongAnswers = puzzlePersist != null ? puzzlePersist.activeLessonWrongAnswerCount : 0;

        return $"{lessonTitle} complete.\nSlides: {slideCount}\nWrong answers: {wrongAnswers}.";
    }

    private void ShowLessonCompletionOverlay(string message)
    {
        currentSlideResolved = true;

        ClickManager.SetGlobalInputEnabled(false);

        if (gameUiManager != null)
        {
            gameUiManager.SetGameplayNextPuzzleButtonVisible(false);
            gameUiManager.SetGameplayQuitPuzzleButtonVisible(false);
            gameUiManager.SetGameplayRestartCurrentPuzzleButtonVisible(false);
            gameUiManager.SetEndGameMessage(message);
            gameUiManager.SetEndGameVisible(true);
            BindButton(gameUiManager.EndGameReplayPoolButton,RestartLessonFromBeginning);
            BindButton(gameUiManager.EndGameReturnToSelectionButton,QuitLesson);
        }

        if (lessonModeUi != null)
        {
            lessonModeUi.SetVisible(false);
            lessonModeUi.SetYesNoVisible(false);
            lessonModeUi.SetNumberInputVisible(false);
        }
    }

    private bool IsOnLastSlide()
    {
        return activeLessonData != null && currentSlideIndex >= activeLessonData.SlideCount - 1;
    }

    private void ApplyCurrentSlideInScene()
    {
        ResolveReferences();

        PuzzlePersist puzzlePersist = PuzzlePersist.Instance;
        if (puzzlePersist == null || !puzzlePersist.IsLessonModeActive || puzzlePersist.activeLessonData == null)
            return;

        activeLessonData = puzzlePersist.activeLessonData;
        currentSlideIndex = activeLessonData.ClampSlideIndex(puzzlePersist.activeLessonSlideIndex);
        currentSlideData = activeLessonData.GetSlide(currentSlideIndex);
        if (currentSlideData == null)
            return;

        currentSlideType = currentSlideData.slideType;
        currentSlideResolved = false;
        hasShownPuzzleFailureState = false;

        if (gameManager != null)
            gameManager.ResetManagedSceneState(keepTimerStopped: true);

        if (conditionManager != null)
            conditionManager.ResetRuntimeState();

        if (cubeGrid != null)
            cubeGrid.LoadPuzzleIntoCurrentScene(puzzlePersist.savedPuzzleData);

        ConfigureSharedGameplayUi();
        ConfigureSlideUi();
        BindLessonButtons();
    }

    private void LoadScene(int sceneIndex)
    {
        if (sceneIndex < 0)
            return;

        SceneLoader sceneLoader = FindObjectOfType<SceneLoader>();
        if (sceneLoader != null)
        {
            sceneLoader.LoadThisSceneNumber(sceneIndex);
            return;
        }

        if (Moddwyn.SceneLoader.Instance != null)
        {
            Moddwyn.SceneLoader.Instance.LoadScene(sceneIndex);
            return;
        }

        SceneManager.LoadScene(sceneIndex);
    }

    private void BindButton(Button button,UnityEngine.Events.UnityAction action)
    {
        if (button == null || action == null)
            return;

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }
}
