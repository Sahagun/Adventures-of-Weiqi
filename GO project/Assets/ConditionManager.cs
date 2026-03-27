using System.Collections.Generic;
using System.Linq;
using EditorAttributes;
using Unity.VisualScripting;
using UnityEngine;

public class ConditionManager : MonoBehaviour
{
    [Title("Scene References")]
    [SerializeField] private CubeGrid cubeGrid;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameUiManager gameUiManager;

    [Title("Condition UI")]
    [SerializeField] private bool showFinalScore = true;
    [SerializeField] private bool enableTurnLimit = false;
    [SerializeField] private int maxTurns = 0; // 0 = unlimited turns

    [Title("Score Win Conditions")]
    [SerializeField] private bool enableScoreLimit = false;
    [SerializeField] private int playerWinScore = 0;
    [SerializeField] private int aiWinScore = 0;

    [Title("Pathway Move Conditions")]
    [SerializeField] private bool enablePathwayCheck = true;
    public bool strictPathwayCheck = false;

    private bool hasDisplayedScore = false;
    public bool winTriggered = false;
    private bool loseTriggered = false;

    private void Start()
    {
        if (cubeGrid == null)
            cubeGrid = FindObjectOfType<CubeGrid>();

        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (gameUiManager == null)
            gameUiManager = FindObjectOfType<GameUiManager>(true);

        if (gameUiManager != null)
        {
            gameUiManager.ResolveReferences();
            gameUiManager.SetConditionFinalScoreVisible(false);
            gameUiManager.SetConditionTurnLimitVisible(enableTurnLimit);

            if (enableTurnLimit && maxTurns > 0)
                gameUiManager.SetConditionTurnLimitText(gameUiManager.BuildConditionTurnLimitValue(0,maxTurns));
        }
    }

    private void Update()
    {
        if (gameManager == null || gameManager.gameIsOver)
            return;

        if (enableTurnLimit && maxTurns > 0)
        {
            if (gameUiManager != null)
                gameUiManager.SetConditionTurnLimitText(gameUiManager.BuildConditionTurnLimitValue(gameManager.turnCount,maxTurns));

            if (gameManager.turnCount >= maxTurns)
            {
                Debug.Log("Turn limit reached. Game Over.");
                SetResolutionMessage("Turn limit reached. Game Over.");

                if (gameManager != null)
                    gameManager.EndGame();
            }
        }

        if (!enableScoreLimit)
            return;

        int playerScore = cubeGrid.CountPlayerPieces();
        int aiScore = cubeGrid.CountAIPieces();

        if (playerWinScore > 0 && playerScore >= playerWinScore)
        {
            Debug.Log($"Player reached {playerWinScore} points. Player Wins!");
            SetResolutionMessage($"Player reached {playerWinScore} points. Player Wins!");

            if (gameManager != null)
                gameManager.EndGame();
            return;
        }

        if (aiWinScore > 0 && aiScore >= aiWinScore)
        {
            Debug.Log($"AI reached {aiWinScore} points. AI Wins!");
            SetResolutionMessage($"AI reached {aiWinScore} points. AI Wins!");

            if (gameManager != null)
                gameManager.EndGame();
        }
    }

    public void DisplayFinalScore()
    {
        if (hasDisplayedScore)
            return;

        CalculateFinalScores();
        hasDisplayedScore = true;
    }

    private void CalculateFinalScores()
    {
        if (cubeGrid == null)
        {
            Debug.LogError("ConditionManager: CubeGrid reference is missing!");
            return;
        }

        int playerScore = cubeGrid.CountPlayerPieces();
        int aiScore = cubeGrid.CountAIPieces();

        Debug.Log($"Final Score - Player: {playerScore} | AI: {aiScore}");

        if (gameUiManager != null && showFinalScore)
        {
            gameUiManager.SetConditionFinalScoreText($"Final Score:\nPlayer: {playerScore}\nAI: {aiScore}");
            gameUiManager.SetConditionFinalScoreVisible(true);
        }
    }

    public void CheckMovesPath(List<string> playerPath)
    {
        CheckMovesPath(playerPath,false);
    }

    public void CheckMovesPath(List<string> playerPath,bool allowWin)
    {
        if (!enablePathwayCheck || winTriggered || loseTriggered)
            return;

        var currentPuzzle = cubeGrid.CurrentPuzzleData;
        if (currentPuzzle == null)
        {
            Debug.LogError("ConditionManager: Puzzle data is null.");
            return;
        }

        bool onValidPath = false;

        foreach (var moveGroup in currentPuzzle.moves)
        {
            List<string> expectedPlayerMoves = new List<string>();
            if (!string.IsNullOrEmpty(moveGroup.playerMove))
                expectedPlayerMoves.Add(moveGroup.playerMove);
            if (moveGroup.correctMoves != null && moveGroup.correctMoves.Count > 0)
                expectedPlayerMoves.AddRange(moveGroup.correctMoves.Select(m => m.playerMove));

            List<string> expectedAiMoves = new List<string>();
            if (!string.IsNullOrEmpty(moveGroup.playerMove) && moveGroup.correctMoves != null && moveGroup.correctMoves.Count > 0)
            {
                expectedAiMoves.Add(moveGroup.correctMoves[0].aiMove);
                for (int i = 1; i < moveGroup.correctMoves.Count; i++)
                    expectedAiMoves.Add(moveGroup.correctMoves[i].aiMove);
            }
            else if (moveGroup.correctMoves != null && moveGroup.correctMoves.Count > 0)
            {
                expectedAiMoves = moveGroup.correctMoves.Select(m => m.aiMove).ToList();
            }

            if (playerPath.SequenceEqual(expectedPlayerMoves))
            {
                bool lastAiIsSkip = expectedAiMoves.Count > 0 && expectedAiMoves[expectedAiMoves.Count - 1] == "(0,0)";
                if (allowWin || lastAiIsSkip)
                {
                    Debug.Log("!!! CORRECT PATH FOUND !!! Player wins!");
                    SetResolutionMessage("PUZZLE COMPLETE! GREAT JOB!");
                    winTriggered = true;

                    if (gameManager != null)
                        gameManager.EndGame();
                    return;
                }

                onValidPath = true;
            }

            if (expectedPlayerMoves.Take(playerPath.Count).SequenceEqual(playerPath))
                onValidPath = true;

            if (moveGroup.wrongMoves != null && moveGroup.wrongMoves.Count > 0)
            {
                List<string> wrongPlayerMoves = moveGroup.wrongMoves.Select(m => m.playerMove).ToList();

                if (playerPath.SequenceEqual(wrongPlayerMoves))
                {
                    Debug.Log("!!! WRONG PATH FOUND !!! Player loses!");
                    SetResolutionMessage("Wrong Move! GAME OVER");
                    loseTriggered = true;

                    if (gameManager != null)
                        gameManager.EndGame();
                    return;
                }

                if (wrongPlayerMoves.Take(playerPath.Count).SequenceEqual(playerPath))
                    onValidPath = true;
            }
        }

        if (strictPathwayCheck && !onValidPath)
        {
            Debug.Log("Player deviated from all valid paths. Game Over!");
            SetResolutionMessage("Wrong Move! GAME OVER");
            loseTriggered = true;

            if (gameManager != null)
                gameManager.EndGame();
        }
    }

    private void SetResolutionMessage(string message)
    {
        if (gameUiManager == null)
            return;

        if (gameManager != null && gameManager.IsUsingManagedSceneFlow)
        {
            gameUiManager.SetGameplayMainStatusText(message);
            return;
        }

        gameUiManager.SetEndGameMessage(message);
    }

    public void ResetRuntimeState()
    {
        hasDisplayedScore = false;
        winTriggered = false;
        loseTriggered = false;

        if (gameUiManager == null)
            gameUiManager = FindObjectOfType<GameUiManager>(true);

        if (gameUiManager != null)
        {
            gameUiManager.SetConditionFinalScoreVisible(false);
            gameUiManager.SetConditionTurnLimitVisible(enableTurnLimit);

            if (enableTurnLimit && maxTurns > 0)
            {
                int currentTurnCount = gameManager != null ? gameManager.turnCount : 0;
                gameUiManager.SetConditionTurnLimitText(gameUiManager.BuildConditionTurnLimitValue(currentTurnCount,maxTurns));
            }
        }
    }
}
