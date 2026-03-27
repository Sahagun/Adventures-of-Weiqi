using EditorAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Title("Gameplay Settings")]
    [SerializeField] private bool computerOpponent = true;
    [SerializeField] private float timeToComplete = 45f;
    [SerializeField] private CubeGrid activeCubeGrid;
    [SerializeField] private GameUiManager gameUiManager;

    [Title("UI Navigation")]
    [SerializeField] private bool autoBindButtons = true;
    [SceneDropdown] public int returnToSelectionSceneIndex = -1;

    [Title("Runtime State")]
    [ReadOnly] public int turnCount = 0;
    [ReadOnly] public bool PlayerTurn = true;
    [ReadOnly] public int CurrentColour = 1;
    [ReadOnly] public bool gameIsOver = false;
    [HideInInspector] public float remainingTime;

    private MultiPuzzleManager activeMultiPuzzleManager;
    private int consecutivePasses = 0;
    private int lastPassTurnFrame = -1;
    private bool isTimerRunning = true;

    public GameObject EndGameCanvas => gameUiManager != null ? gameUiManager.EndGameOverlayCanvas : null;
    public GameUiManager UiManager => gameUiManager;
    public bool IsUsingMultiPuzzleFlow => activeMultiPuzzleManager != null && activeMultiPuzzleManager.IsRuntimeFlowActive;
    public bool IsUsingLessonMode => PuzzlePersist.Instance != null && PuzzlePersist.Instance.IsLessonModeActive;
    public bool IsUsingManagedSceneFlow => IsUsingMultiPuzzleFlow || IsUsingLessonMode;

    private void Awake()
    {
        Instance = this;
        ResolveReferences();
        BindButtons();
    }

    private void Start()
    {
        ResolveReferences();
        BindButtons();
        SubscribeCubeGrid();

        if (!IsUsingLessonMode)
            ClickManager.SetGlobalInputEnabled(true);

        remainingTime = timeToComplete;
        RefreshGameplayUi();
        SetPassTurnButtonInteractable(true);
        SetEndGameCanvasVisible(false);
    }

    private void Update()
    {
        if (!gameIsOver && isTimerRunning)
            TickTimer();
    }

    private void OnDestroy()
    {
        if (activeCubeGrid != null)
            activeCubeGrid.OnGameOver -= HandleGameOver;
    }

    public void StopTimer()
    {
        isTimerRunning = false;
    }

    public void PassTurn()
    {
        if (gameIsOver || lastPassTurnFrame == Time.frameCount)
            return;

        lastPassTurnFrame = Time.frameCount;
        consecutivePasses++;

        if (consecutivePasses >= 2)
        {
            SetGameOverMessage("Both players passed. Ending game.");
            EndGame(showEndGameUi: !IsUsingManagedSceneFlow);
            return;
        }

        ChangeTurn();
    }

    public void ChangeTurn()
    {
        turnCount++;
        UpdateTurnCounter();

        CurrentColour = CurrentColour == 1 ? 2 : 1;
        bool isAiTurn = computerOpponent && CurrentColour == 2;

        PlayerTurn = !isAiTurn;
        SetPassTurnButtonInteractable(!isAiTurn);

        if (isAiTurn)
        {
            Invoke(nameof(AIMove),Random.Range(0.5f,1.5f));
        }
        else
        {
            consecutivePasses = 0;
        }

        UpdateMainStatusText();
    }

    public void EndGame()
    {
        EndGame(showEndGameUi: !IsUsingManagedSceneFlow);
    }

    public void EndGame(bool showEndGameUi)
    {
        EndGame(null,showEndGameUi);
    }

    public void SetEndGameCanvasVisible(bool isVisible)
    {
        if (gameUiManager != null)
            gameUiManager.SetEndGameVisible(isVisible);
    }

    public void ResetManagedSceneState(bool keepTimerStopped)
    {
        CancelInvoke(nameof(AIMove));

        turnCount = 0;
        PlayerTurn = true;
        CurrentColour = 1;
        gameIsOver = false;
        consecutivePasses = 0;
        lastPassTurnFrame = -1;
        remainingTime = timeToComplete;
        isTimerRunning = !keepTimerStopped;

        if (keepTimerStopped)
            ClickManager.SetGlobalInputEnabled(false);
        else
            ClickManager.SetGlobalInputEnabled(true);

        SetEndGameCanvasVisible(false);
        SetPassTurnButtonInteractable(!keepTimerStopped);
        RefreshGameplayUi();
    }

    private void EndGame(string logMessage,bool showEndGameUi)
    {
        if (gameIsOver)
            return;

        if (!string.IsNullOrWhiteSpace(logMessage))
            Debug.Log(logMessage);

        gameIsOver = true;
        StopTimer();
        SetPassTurnButtonInteractable(false);
        SetEndGameCanvasVisible(showEndGameUi);
    }

    private void TickTimer()
    {
        if (remainingTime <= 0f)
            return;

        remainingTime -= Time.deltaTime;
        UpdateTimerText();

        if (remainingTime > 0f)
            return;

        SetGameOverMessage("Time's Up! GAME OVER");
        EndGame(showEndGameUi: !IsUsingManagedSceneFlow);
    }

    private void HandleGameOver()
    {
        Debug.Log("GameManager detected game over.");
        EndGame(showEndGameUi: !IsUsingManagedSceneFlow);
    }

    private void AIMove()
    {
        if (!PlayerTurn)
        {
            GoBoard.Instance.AIMove();
            ChangeTurn();
        }
    }

    private void RefreshGameplayUi()
    {
        UpdateTimerText();
        UpdateTurnCounter();
        UpdateMainStatusText();
    }

    private void UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);

        if (gameUiManager != null)
            gameUiManager.SetGameplayTimerText($"{minutes:00}:{seconds:00}");
    }

    public void UpdateTurnCounter()
    {
        if (gameUiManager != null)
            gameUiManager.SetGameplayTurnCounterText(gameUiManager.BuildGameplayTurnCounterValue(turnCount));
    }

    private void UpdateMainStatusText()
    {
        if (gameUiManager == null)
            return;

        string playerColor = CurrentColour == 1 ? "Black" : "White";
        string statusMessage = computerOpponent && CurrentColour == 2
            ? $"AI ({playerColor})'s Turn"
            : $"{playerColor}'s Turn";

        gameUiManager.SetGameplayMainStatusText(statusMessage);
    }

    private void SetPassTurnButtonInteractable(bool isInteractable)
    {
        if (gameUiManager != null)
            gameUiManager.SetGameplayPassTurnInteractable(isInteractable);
    }

    private void SetGameOverMessage(string message)
    {
        if (gameUiManager == null)
            return;

        if (IsUsingManagedSceneFlow)
        {
            gameUiManager.SetGameplayMainStatusText(message);
            return;
        }

        gameUiManager.SetEndGameMessage(message);
    }

    private void ResolveReferences()
    {
        if (activeCubeGrid == null)
            activeCubeGrid = FindObjectOfType<CubeGrid>();

        if (gameUiManager == null)
            gameUiManager = FindObjectOfType<GameUiManager>(true);

        if (activeMultiPuzzleManager == null)
            activeMultiPuzzleManager = FindObjectOfType<MultiPuzzleManager>();

        if (gameUiManager != null)
            gameUiManager.ResolveReferences();
    }

    private void SubscribeCubeGrid()
    {
        if (activeCubeGrid == null)
            return;

        activeCubeGrid.OnGameOver -= HandleGameOver;
        activeCubeGrid.OnGameOver += HandleGameOver;
    }

    private void BindButtons()
    {
        if (!autoBindButtons || gameUiManager == null)
            return;

        BindButton(gameUiManager.GameplayPassTurnButton,PassTurn);
        RemoveButtonBinding(gameUiManager.EndGameReplayPoolButton,RestartSceneFromButton);
        RemoveButtonBinding(gameUiManager.EndGameReturnToSelectionButton,ReturnToSelectionFromButton);

        if (IsUsingManagedSceneFlow)
            return;

        BindButton(gameUiManager.EndGameReplayPoolButton,RestartSceneFromButton);
        BindButton(gameUiManager.EndGameReturnToSelectionButton,ReturnToSelectionFromButton);
    }

    private void BindButton(Button button,UnityEngine.Events.UnityAction action)
    {
        if (button == null || action == null)
            return;

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private void RemoveButtonBinding(Button button,UnityEngine.Events.UnityAction action)
    {
        if (button == null || action == null)
            return;

        button.onClick.RemoveListener(action);
    }

    private void RestartSceneFromButton()
    {
        SceneLoader sceneLoader = ResolveSceneLoader(gameUiManager != null ? gameUiManager.EndGameReplayPoolButton : null);
        if (sceneLoader != null)
        {
            sceneLoader.RestartThisScene();
            return;
        }

        if (Moddwyn.SceneLoader.Instance != null)
        {
            Moddwyn.SceneLoader.Instance.RestartCurrentScene();
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ReturnToSelectionFromButton()
    {
        int targetSceneIndex = returnToSelectionSceneIndex >= 0 ? returnToSelectionSceneIndex : 0;
        SceneLoader sceneLoader = ResolveSceneLoader(gameUiManager != null ? gameUiManager.EndGameReturnToSelectionButton : null);
        if (sceneLoader != null)
        {
            sceneLoader.LoadThisSceneNumber(targetSceneIndex);
            return;
        }

        if (Moddwyn.SceneLoader.Instance != null)
        {
            Moddwyn.SceneLoader.Instance.LoadScene(targetSceneIndex);
            return;
        }

        SceneManager.LoadScene(targetSceneIndex);
    }

    private SceneLoader ResolveSceneLoader(Button sourceButton)
    {
        if (sourceButton != null)
        {
            SceneLoader buttonLoader = sourceButton.GetComponent<SceneLoader>();
            if (buttonLoader != null)
                return buttonLoader;

            SceneLoader parentLoader = sourceButton.GetComponentInParent<SceneLoader>();
            if (parentLoader != null)
                return parentLoader;
        }

        return FindObjectOfType<SceneLoader>();
    }
}
