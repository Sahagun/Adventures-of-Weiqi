using EditorAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUiManager : MonoBehaviour
{
    [Title("Text Prefixes")]
    [SerializeField] private string gameplayTimerPrefix = string.Empty;
    [SerializeField] private string gameplayTurnCounterPrefix = "Turn: ";
    [SerializeField] private string gameplayDifficultyPrefix = string.Empty;
    [SerializeField] private string gameplayMainStatusPrefix = string.Empty;
    [SerializeField] private string endGameMessagePrefix = string.Empty;
    [SerializeField] private string conditionFinalScorePrefix = string.Empty;
    [SerializeField] private string conditionTurnLimitPrefix = "Turn Limit: ";

    [Title("Gameplay Puzzle UI")]
    [SerializeField] private TextMeshProUGUI gameplayTimerText;
    [SerializeField] private TextMeshProUGUI gameplayTurnCounterText;
    [SerializeField] private TextMeshProUGUI gameplayDifficultyText;
    [SerializeField] private TextMeshProUGUI gameplayMainStatusText;
    [SerializeField] private Button gameplayPassTurnButton;
    [SerializeField] private Button gameplayNextPuzzleButton;
    [SerializeField] private Button gameplayRestartCurrentPuzzleButton;
    [SerializeField] private Button gameplayQuitPuzzleButton;

    [Title("End Game UI")]
    [SerializeField] private GameObject endGameOverlayCanvas;
    [SerializeField] private TextMeshProUGUI endGameMessageText;
    [SerializeField] private Button endGameReplayPoolButton;
    [SerializeField] private Button endGameReturnToSelectionButton;

    [Title("Condition UI")]
    [SerializeField] private TextMeshProUGUI conditionFinalScoreText;
    [SerializeField] private TextMeshProUGUI conditionTurnLimitText;

    public Button GameplayPassTurnButton => gameplayPassTurnButton;
    public Button GameplayNextPuzzleButton => gameplayNextPuzzleButton;
    public Button GameplayRestartCurrentPuzzleButton => gameplayRestartCurrentPuzzleButton;
    public Button GameplayQuitPuzzleButton => gameplayQuitPuzzleButton;
    public GameObject EndGameOverlayCanvas => endGameOverlayCanvas;
    public Button EndGameReplayPoolButton => endGameReplayPoolButton;
    public Button EndGameReturnToSelectionButton => endGameReturnToSelectionButton;

    private void Awake()
    {
        ResolveReferences();
    }

    public void ResolveReferences()
    {
        if (endGameOverlayCanvas == null)
            endGameOverlayCanvas = FindCanvas("GameOverCanvas","Game Over Canvas","EndGame","GameOver");

        if (gameplayPassTurnButton == null)
            gameplayPassTurnButton = FindGameplayButton("GameplayPassTurnButton","pass");

        if (gameplayNextPuzzleButton == null)
            gameplayNextPuzzleButton = FindGameplayButton("GameplayNextPuzzleButton","next");

        if (gameplayRestartCurrentPuzzleButton == null)
            gameplayRestartCurrentPuzzleButton = FindGameplayButton("GameplayRestartPuzzleButton","restart","replay");

        if (gameplayQuitPuzzleButton == null)
            gameplayQuitPuzzleButton = FindGameplayButton("GameplayQuitPuzzleButton","quit","exit","leave");

        if (endGameReplayPoolButton == null)
            endGameReplayPoolButton = FindButtonInCanvas(endGameOverlayCanvas,false,"restart","replay","again");

        if (endGameReturnToSelectionButton == null)
            endGameReturnToSelectionButton = FindButtonInCanvas(endGameOverlayCanvas,true,"return","menu","home","back","quit");
    }

    public void SetGameplayTimerText(string message)
    {
        if (gameplayTimerText != null)
            gameplayTimerText.text = FormatWithPrefix(gameplayTimerPrefix,message);
    }

    public void SetGameplayTimerVisible(bool isVisible)
    {
        SetTextObjectVisible(gameplayTimerText,isVisible);
    }

    public void SetGameplayTurnCounterText(string message)
    {
        if (gameplayTurnCounterText != null)
            gameplayTurnCounterText.text = FormatWithPrefix(gameplayTurnCounterPrefix,message);
    }

    public void SetGameplayTurnCounterVisible(bool isVisible)
    {
        SetTextObjectVisible(gameplayTurnCounterText,isVisible);
    }

    public void SetGameplayDifficultyText(string message)
    {
        if (gameplayDifficultyText != null)
            gameplayDifficultyText.text = FormatWithPrefix(gameplayDifficultyPrefix,message);
    }

    public void SetGameplayDifficultyVisible(bool isVisible)
    {
        SetTextObjectVisible(gameplayDifficultyText,isVisible);
    }

    public void SetGameplayMainStatusText(string message)
    {
        if (gameplayMainStatusText != null)
            gameplayMainStatusText.text = FormatWithPrefix(gameplayMainStatusPrefix,message);
    }

    public void SetGameplayMainStatusVisible(bool isVisible)
    {
        SetTextObjectVisible(gameplayMainStatusText,isVisible);
    }

    public void SetGameplayPassTurnInteractable(bool isInteractable)
    {
        if (gameplayPassTurnButton != null)
            gameplayPassTurnButton.interactable = isInteractable;
    }

    public void SetGameplayPassTurnVisible(bool isVisible)
    {
        SetButtonVisible(gameplayPassTurnButton,isVisible);
    }

    public void SetGameplayNextPuzzleButtonVisible(bool isVisible)
    {
        if (gameplayNextPuzzleButton != null)
            gameplayNextPuzzleButton.gameObject.SetActive(isVisible);
    }

    public void SetGameplayNextPuzzleButtonInteractable(bool isInteractable)
    {
        if (gameplayNextPuzzleButton != null)
            gameplayNextPuzzleButton.interactable = isInteractable;
    }

    public void SetGameplayNextPuzzleButtonLabel(string labelText)
    {
        SetButtonLabel(gameplayNextPuzzleButton,labelText);
    }

    public void SetGameplayRestartCurrentPuzzleButtonVisible(bool isVisible)
    {
        SetButtonVisible(gameplayRestartCurrentPuzzleButton,isVisible);
    }

    public void SetGameplayRestartCurrentPuzzleButtonInteractable(bool isInteractable)
    {
        if (gameplayRestartCurrentPuzzleButton != null)
            gameplayRestartCurrentPuzzleButton.interactable = isInteractable;
    }

    public void SetGameplayRestartCurrentPuzzleButtonLabel(string labelText)
    {
        SetButtonLabel(gameplayRestartCurrentPuzzleButton,labelText);
    }

    public void SetGameplayQuitPuzzleButtonVisible(bool isVisible)
    {
        SetButtonVisible(gameplayQuitPuzzleButton,isVisible);
    }

    public void SetGameplayQuitPuzzleButtonInteractable(bool isInteractable)
    {
        if (gameplayQuitPuzzleButton != null)
            gameplayQuitPuzzleButton.interactable = isInteractable;
    }

    public void SetGameplayQuitPuzzleButtonLabel(string labelText)
    {
        SetButtonLabel(gameplayQuitPuzzleButton,labelText);
    }

    public void SetEndGameVisible(bool isVisible)
    {
        if (endGameOverlayCanvas != null)
            endGameOverlayCanvas.SetActive(isVisible);
    }

    public void SetEndGameMessage(string message)
    {
        if (endGameMessageText != null)
            endGameMessageText.text = FormatWithPrefix(endGameMessagePrefix,message);
    }

    public void SetConditionFinalScoreVisible(bool isVisible)
    {
        if (conditionFinalScoreText != null)
            conditionFinalScoreText.gameObject.SetActive(isVisible);
    }

    public void SetConditionFinalScoreText(string message)
    {
        if (conditionFinalScoreText != null)
        {
            conditionFinalScoreText.text = FormatWithPrefix(conditionFinalScorePrefix,message);
            return;
        }

        SetGameplayMainStatusText(message);
    }

    public void SetConditionTurnLimitVisible(bool isVisible)
    {
        if (conditionTurnLimitText != null)
            conditionTurnLimitText.gameObject.SetActive(isVisible);
    }

    public void SetConditionTurnLimitText(string message)
    {
        if (conditionTurnLimitText != null)
            conditionTurnLimitText.text = FormatWithPrefix(conditionTurnLimitPrefix,message);
    }

    public string BuildGameplayTurnCounterValue(int turnCount)
    {
        return turnCount.ToString();
    }

    public string BuildConditionTurnLimitValue(int currentTurnCount,int maxTurnCount)
    {
        return maxTurnCount > 0 ? $"{currentTurnCount}/{maxTurnCount}" : currentTurnCount.ToString();
    }

    private GameObject FindCanvas(params string[] canvasNames)
    {
        foreach (string canvasName in canvasNames)
        {
            GameObject canvas = GameObject.Find(canvasName);
            if (canvas != null)
                return canvas;
        }

        return null;
    }

    private Button FindGameplayButton(string preferredButtonName,params string[] keywords)
    {
        Button namedButton = FindNamedButtonInScene(preferredButtonName);
        if (namedButton != null && !IsButtonInsideManagedCanvas(namedButton))
            return namedButton;

        foreach (Button button in FindObjectsOfType<Button>(true))
        {
            if (IsButtonInsideManagedCanvas(button))
                continue;

            if (MatchesButton(button,false,keywords))
                return button;
        }

        return null;
    }

    private Button FindNamedButtonInScene(string buttonName)
    {
        if (string.IsNullOrWhiteSpace(buttonName))
            return null;

        GameObject buttonObject = GameObject.Find(buttonName);
        return buttonObject != null ? buttonObject.GetComponent<Button>() : null;
    }

    private Button FindButtonInCanvas(GameObject canvas,bool excludeReplayMatches,params string[] keywords)
    {
        if (canvas == null)
            return null;

        foreach (Button button in canvas.GetComponentsInChildren<Button>(true))
        {
            if (MatchesButton(button,excludeReplayMatches,keywords))
                return button;
        }

        return null;
    }

    private bool MatchesButton(Button button,bool excludeReplayMatches,params string[] keywords)
    {
        if (button == null || keywords == null || keywords.Length == 0)
            return false;

        string buttonName = button.name.ToLowerInvariant();
        TextMeshProUGUI tmpLabel = button.GetComponentInChildren<TextMeshProUGUI>(true);
        string labelText = tmpLabel != null ? tmpLabel.text.ToLowerInvariant() : string.Empty;

        if (excludeReplayMatches && (buttonName.Contains("restart") || buttonName.Contains("replay") || labelText.Contains("restart") || labelText.Contains("replay")))
            return false;

        foreach (string keyword in keywords)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                continue;

            string normalizedKeyword = keyword.ToLowerInvariant();
            if (buttonName.Contains(normalizedKeyword) || labelText.Contains(normalizedKeyword))
                return true;
        }

        return false;
    }

    private bool IsButtonInsideManagedCanvas(Button button)
    {
        if (button == null)
            return false;

        return IsTransformInside(button.transform,endGameOverlayCanvas);
    }

    private bool IsTransformInside(Transform child,GameObject parentObject)
    {
        if (child == null || parentObject == null)
            return false;

        return child.IsChildOf(parentObject.transform);
    }

    private void SetButtonLabel(Button button,string labelText)
    {
        if (button == null || string.IsNullOrWhiteSpace(labelText))
            return;

        TextMeshProUGUI tmpLabel = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmpLabel != null)
        {
            tmpLabel.text = labelText;
            return;
        }

        Text legacyLabel = button.GetComponentInChildren<Text>(true);
        if (legacyLabel != null)
            legacyLabel.text = labelText;
    }

    private void SetTextObjectVisible(TMP_Text textObject,bool isVisible)
    {
        if (textObject != null)
            textObject.gameObject.SetActive(isVisible);
    }

    private void SetButtonVisible(Button button,bool isVisible)
    {
        if (button != null)
            button.gameObject.SetActive(isVisible);
    }

    private string FormatWithPrefix(string prefix,string message)
    {
        string safePrefix = prefix ?? string.Empty;
        string safeMessage = message ?? string.Empty;
        return safePrefix + safeMessage;
    }
}
