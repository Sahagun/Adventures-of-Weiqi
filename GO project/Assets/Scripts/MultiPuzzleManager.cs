using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiPuzzleManager : MonoBehaviour
{
    [Title("Puzzle Pool Data")]
    [SerializeField] private List<TextAsset> assignedPoolPuzzleFiles = new();
    [SerializeField] private bool shuffleAssignedPoolOnStart = false;
    [SerializeField] private bool loadPuzzlesFromPersistWhenAvailable = true;
    [SerializeField] private bool shufflePersistedPoolOnStart = false;

    [Title("Scene References")]
    [SerializeField] private CubeGrid activeCubeGrid;
    [SerializeField] private GameManager activeGameManager;
    [SerializeField] private GameUiManager gameUiManager;

    [Title("Pool Progression")]
    [Range(0f,1f)]
    [SerializeField] private float fallbackRequiredSolveRateToUnlockNextPool = 1f;
    [SceneDropdown] public int returnToSelectionSceneIndex = -1;

    [Title("Gameplay Puzzle Controls")]
    [SerializeField] private string nextPuzzleButtonText = "Next Puzzle";
    [SerializeField] private string finishPoolButtonText = "Finish Pool";

    [Title("Runtime State")]
    [ReadOnly] [SerializeField] private int currentPuzzleIndex = 0;
    [ReadOnly] [SerializeField] private bool hasDisplayedCurrentPuzzleResolutionState = false;
    [ReadOnly] [SerializeField] private bool isUsingPersistedPool = false;
    [ReadOnly] [SerializeField] private float activeRequiredSolveRateToUnlockNextPool = 1f;

    private ConditionManager activeConditionManager;
    private int gameplaySceneIndex;
    private bool noMorePuzzles = false;
    private bool hasShuffledPuzzleOrder = false;
    private string activePoolId = string.Empty;
    private string activePoolLabel = string.Empty;
    private string nextPoolIdToUnlock = string.Empty;
    private bool isBlockedByLessonMode = false;

    public bool IsRuntimeFlowActive => !isBlockedByLessonMode && !noMorePuzzles;

    private void Awake()
    {
        if (PuzzlePersist.Instance != null && PuzzlePersist.Instance.IsLessonModeActive)
        {
            isBlockedByLessonMode = true;
            enabled = false;
            return;
        }

        gameplaySceneIndex = SceneManager.GetActiveScene().buildIndex;
        InitializePuzzlePool();

        if (assignedPoolPuzzleFiles == null || assignedPoolPuzzleFiles.Count == 0)
        {
            Debug.LogWarning("No puzzles assigned to MultiPuzzleManager.");
            noMorePuzzles = true;
            return;
        }

        LoadPuzzleAtIndex(currentPuzzleIndex);
    }

    private void Start()
    {
        if (noMorePuzzles || isBlockedByLessonMode)
            return;

        ResolveSceneReferences();
        BindGameplayUiButtons(force: true);
        ResetActivePuzzleUiState();
    }

    private void Update()
    {
        if (noMorePuzzles || isBlockedByLessonMode)
            return;

        ResolveSceneReferences();

        bool puzzleWasSolved = activeConditionManager != null && activeConditionManager.winTriggered;
        if (puzzleWasSolved)
        {
            RecordCurrentPuzzleResult(wasSolved: true);

            if (!hasDisplayedCurrentPuzzleResolutionState)
                ShowSolvedPuzzleState();

            return;
        }

        if (activeGameManager == null || !activeGameManager.gameIsOver)
            return;

        RecordCurrentPuzzleResult(wasSolved: false);

        if (!hasDisplayedCurrentPuzzleResolutionState)
            ShowFailedPuzzleState();
    }

    private void InitializePuzzlePool()
    {
        activeRequiredSolveRateToUnlockNextPool = Mathf.Clamp01(fallbackRequiredSolveRateToUnlockNextPool);

        PuzzlePersist puzzlePersist = PuzzlePersist.Instance;
        if (loadPuzzlesFromPersistWhenAvailable && puzzlePersist != null && puzzlePersist.HasSelectedPuzzlePool)
        {
            puzzlePersist.EnsureRuntimePoolState();

            List<TextAsset> persistedPuzzlePool = new List<TextAsset>(puzzlePersist.savedPuzzlePools);
            if (persistedPuzzlePool.Count > 0)
            {
                assignedPoolPuzzleFiles = persistedPuzzlePool;
                activePoolId = puzzlePersist.activePoolId;
                activePoolLabel = puzzlePersist.activePoolLabel;
                nextPoolIdToUnlock = puzzlePersist.nextPoolIdToUnlock;
                activeRequiredSolveRateToUnlockNextPool = Mathf.Clamp01(puzzlePersist.requiredSolveRateToUnlockNext);
                currentPuzzleIndex = Mathf.Clamp(puzzlePersist.activePoolCurrentPuzzleIndex,0,persistedPuzzlePool.Count - 1);
                isUsingPersistedPool = true;
            }
        }

        if (assignedPoolPuzzleFiles == null)
            assignedPoolPuzzleFiles = new List<TextAsset>();

        if (string.IsNullOrWhiteSpace(activePoolLabel))
            activePoolLabel = activePoolId;

        bool shouldShufflePool = ShouldShufflePuzzlePool();
        if (isUsingPersistedPool && puzzlePersist != null)
        {
            if (shouldShufflePool && !puzzlePersist.activePoolOrderInitialized)
            {
                ShuffleList(assignedPoolPuzzleFiles);
                puzzlePersist.SetPuzzlePoolOrder(assignedPoolPuzzleFiles);
            }
            else if (!puzzlePersist.activePoolOrderInitialized)
            {
                puzzlePersist.SetPuzzlePoolOrder(assignedPoolPuzzleFiles);
            }
        }
        else if (!hasShuffledPuzzleOrder && shouldShufflePool)
        {
            ShuffleList(assignedPoolPuzzleFiles);
            hasShuffledPuzzleOrder = true;
        }
    }

    private bool ShouldShufflePuzzlePool()
    {
        if (assignedPoolPuzzleFiles == null || assignedPoolPuzzleFiles.Count <= 1)
            return false;

        return isUsingPersistedPool ? shufflePersistedPoolOnStart : shuffleAssignedPoolOnStart;
    }

    private void ResolveSceneReferences()
    {
        if (activeCubeGrid == null)
            activeCubeGrid = FindObjectOfType<CubeGrid>();
        if (activeConditionManager == null)
            activeConditionManager = FindObjectOfType<ConditionManager>();
        if (activeGameManager == null)
            activeGameManager = FindObjectOfType<GameManager>();
        if (gameUiManager == null)
            gameUiManager = FindObjectOfType<GameUiManager>(true);
        if (gameUiManager != null)
            gameUiManager.ResolveReferences();
    }

    private void LoadPuzzleAtIndex(int puzzleIndex)
    {
        if (puzzleIndex < 0 || puzzleIndex >= assignedPoolPuzzleFiles.Count)
        {
            Debug.LogWarning("Puzzle index out of range.");
            return;
        }

        currentPuzzleIndex = puzzleIndex;
        TextAsset puzzleAsset = assignedPoolPuzzleFiles[puzzleIndex];

        if (PuzzlePersist.Instance != null)
        {
            PuzzlePersist.Instance.SetCurrentPuzzleIndex(puzzleIndex);
            PuzzlePersist.Instance.SetCurrentPuzzle(puzzleAsset);
        }

        if (activeCubeGrid == null)
            activeCubeGrid = FindObjectOfType<CubeGrid>();

        if (activeCubeGrid != null)
            activeCubeGrid.puzzleJsonFile = puzzleAsset;
    }

    public void GoToNextPuzzle()
    {
        if (noMorePuzzles || !CanAdvanceToNextPuzzle())
            return;

        int nextPuzzleIndex = currentPuzzleIndex + 1;
        if (nextPuzzleIndex < assignedPoolPuzzleFiles.Count)
        {
            ResetResolvedPuzzleState();
            LoadPuzzleAtIndex(nextPuzzleIndex);
            SceneManager.LoadScene(gameplaySceneIndex);
            return;
        }

        FinalizePoolRun();
    }

    public void RestartCurrentPuzzle()
    {
        if (noMorePuzzles || currentPuzzleIndex < 0 || currentPuzzleIndex >= assignedPoolPuzzleFiles.Count)
        {
            Debug.LogWarning("Cannot restart puzzle: either no puzzles or index out of range.");
            return;
        }

        ResetResolvedPuzzleState();
        LoadPuzzleAtIndex(currentPuzzleIndex);
        SceneManager.LoadScene(gameplaySceneIndex);
    }

    public void QuitCurrentPuzzle()
    {
        HideGameplayPuzzleUi();
        LoadConfiguredScene();
    }

    public void ReplayPoolFromStart()
    {
        if (assignedPoolPuzzleFiles == null || assignedPoolPuzzleFiles.Count == 0)
            return;

        if (ShouldShufflePuzzlePool())
            ShuffleList(assignedPoolPuzzleFiles);

        if (PuzzlePersist.Instance != null)
        {
            PuzzlePersist.Instance.BeginPuzzlePoolSession(
                poolId: activePoolId,
                poolLabel: activePoolLabel,
                puzzlePool: assignedPoolPuzzleFiles,
                unlockThreshold: activeRequiredSolveRateToUnlockNextPool,
                nextPoolId: nextPoolIdToUnlock);
        }

        noMorePuzzles = false;
        ResetResolvedPuzzleState();
        LoadPuzzleAtIndex(0);
        SceneManager.LoadScene(gameplaySceneIndex);
    }

    private void RecordCurrentPuzzleResult(bool wasSolved)
    {
        if (PuzzlePersist.Instance != null)
        {
            PuzzlePersist.Instance.TryRecordPuzzleFirstResult(currentPuzzleIndex,wasSolved);
            return;
        }

        Debug.LogWarning("PuzzlePersist is missing. MultiPuzzleManager cannot store pool progression correctly.");
    }

    private void FinalizePoolRun()
    {
        noMorePuzzles = true;

        int solvedCount = PuzzlePersist.Instance != null ? PuzzlePersist.Instance.activePoolSolvedCount : 0;
        int totalPuzzleCount = assignedPoolPuzzleFiles.Count;
        float solveRate = totalPuzzleCount > 0 ? (float)solvedCount / totalPuzzleCount : 0f;

        PuzzleProgressionStore.SavePoolResult(activePoolId,solvedCount,totalPuzzleCount);

        bool unlockedNextPool = !string.IsNullOrWhiteSpace(nextPoolIdToUnlock) &&
            solveRate >= Mathf.Clamp01(activeRequiredSolveRateToUnlockNextPool);

        if (unlockedNextPool)
            PuzzleProgressionStore.SetPoolUnlocked(nextPoolIdToUnlock);

        PuzzleProgressionStore.Save();

        HideGameplayPuzzleUi();
        ShowPoolCompletionUi(solveRate,unlockedNextPool);
    }

    private void ShowSolvedPuzzleState()
    {
        hasDisplayedCurrentPuzzleResolutionState = true;
        ShowResolvedPuzzleNavigation();
    }

    private void ShowFailedPuzzleState()
    {
        hasDisplayedCurrentPuzzleResolutionState = true;
        ShowResolvedPuzzleNavigation();
    }

    private void ShowResolvedPuzzleNavigation()
    {
        if (gameUiManager == null)
            return;

        gameUiManager.SetGameplayNextPuzzleButtonLabel(IsOnLastPuzzle() ? finishPoolButtonText : nextPuzzleButtonText);
        gameUiManager.SetGameplayNextPuzzleButtonInteractable(true);
        gameUiManager.SetGameplayNextPuzzleButtonVisible(true);
    }

    private void ShowPoolCompletionUi(float solveRate,bool unlockedNextPool)
    {
        if (gameUiManager == null)
            return;

        BindButton(gameUiManager.EndGameReplayPoolButton,ReplayPoolFromStart);
        BindButton(gameUiManager.EndGameReturnToSelectionButton,LoadConfiguredScene);
        gameUiManager.SetEndGameMessage(BuildPoolCompletionMessage(solveRate,unlockedNextPool));
        gameUiManager.SetEndGameVisible(true);
    }

    private string BuildPoolCompletionMessage(float solveRate,bool unlockedNextPool)
    {
        int solvedCount = PuzzlePersist.Instance != null ? PuzzlePersist.Instance.activePoolSolvedCount : 0;
        int totalPuzzleCount = assignedPoolPuzzleFiles.Count;
        int solvePercent = Mathf.RoundToInt(solveRate * 100f);
        int requiredPercent = Mathf.RoundToInt(Mathf.Clamp01(activeRequiredSolveRateToUnlockNextPool) * 100f);

        string poolLabel = string.IsNullOrWhiteSpace(activePoolLabel) ? "Pool" : activePoolLabel;
        string message = $"{poolLabel} complete.\nSolved {solvedCount}/{totalPuzzleCount} ({solvePercent}%).";

        if (string.IsNullOrWhiteSpace(nextPoolIdToUnlock))
            return message;

        return unlockedNextPool
            ? $"{message}\nNext pool unlocked."
            : $"{message}\nNeed {requiredPercent}% to unlock the next pool.";
    }

    private bool IsOnLastPuzzle()
    {
        return currentPuzzleIndex >= assignedPoolPuzzleFiles.Count - 1;
    }

    private bool CanAdvanceToNextPuzzle()
    {
        return hasDisplayedCurrentPuzzleResolutionState;
    }

    private void ResetActivePuzzleUiState()
    {
        ResetResolvedPuzzleState();
        HideGameplayPuzzleUi();
        HideEndGameUi();
        RefreshGameplayDifficultyText();
    }

    private void ResetResolvedPuzzleState()
    {
        hasDisplayedCurrentPuzzleResolutionState = false;
    }

    private void HideGameplayPuzzleUi()
    {
        if (gameUiManager == null)
            return;

        gameUiManager.SetGameplayNextPuzzleButtonInteractable(false);
        gameUiManager.SetGameplayNextPuzzleButtonVisible(false);
    }

    private void HideEndGameUi()
    {
        if (activeGameManager != null)
            activeGameManager.SetEndGameCanvasVisible(false);
        else if (gameUiManager != null)
            gameUiManager.SetEndGameVisible(false);
    }

    private void BindGameplayUiButtons(bool force)
    {
        if (gameUiManager == null)
            return;

        if (force)
            gameUiManager.ResolveReferences();

        BindButton(gameUiManager.GameplayNextPuzzleButton,GoToNextPuzzle);
        BindButton(gameUiManager.GameplayRestartCurrentPuzzleButton,RestartCurrentPuzzle);
        BindButton(gameUiManager.GameplayQuitPuzzleButton,QuitCurrentPuzzle);
    }

    private void RefreshGameplayDifficultyText()
    {
        if (gameUiManager == null)
            return;

        string poolLabel = !string.IsNullOrWhiteSpace(activePoolLabel) ? activePoolLabel : activePoolId;
        if (string.IsNullOrWhiteSpace(poolLabel))
            poolLabel = "Puzzle Pool";

        gameUiManager.SetGameplayDifficultyText($"[{currentPuzzleIndex + 1}] {poolLabel}");
    }

    private void LoadConfiguredScene()
    {
        if (PuzzlePersist.Instance != null)
            PuzzlePersist.Instance.ClearSelectedPuzzlePool();

        if (returnToSelectionSceneIndex < 0)
        {
            Debug.LogWarning("MultiPuzzleManager returnToSelectionSceneIndex is not configured.");
            return;
        }

        SceneLoader sceneLoader = FindObjectOfType<SceneLoader>();
        if (sceneLoader != null)
        {
            sceneLoader.LoadThisSceneNumber(returnToSelectionSceneIndex);
            return;
        }

        if (Moddwyn.SceneLoader.Instance != null)
        {
            Moddwyn.SceneLoader.Instance.LoadScene(returnToSelectionSceneIndex);
            return;
        }

        SceneManager.LoadScene(returnToSelectionSceneIndex);
    }

    private void BindButton(Button button,UnityEngine.Events.UnityAction action)
    {
        if (button == null || action == null)
            return;

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i,list.Count);
            (list[i],list[randomIndex]) = (list[randomIndex],list[i]);
        }
    }
}
