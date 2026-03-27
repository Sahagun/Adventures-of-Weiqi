using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PuzzleLevelButton : MonoBehaviour
{
    public GameObject lockOverlay;
    public TMP_Text labelText;

    Button button;
    SceneLoader sceneLoader;

    public void Init(
        List<TextAsset> puzzleDatas,
        string poolId,
        string label,
        int sceneToLoad = -1,
        bool startLocked = true,
        float requiredSolveRateToUnlockNext = 1f,
        string nextPoolIdToUnlock = "")
    {
        button = GetComponent<Button>();
        sceneLoader = GetComponent<SceneLoader>() ?? GetComponentInParent<SceneLoader>();

        button.onClick.RemoveAllListeners();
        SetLockState(startLocked);

        if (labelText != null)
            labelText.text = label;

        if (startLocked)
            return;

        if (sceneLoader == null || sceneToLoad == -1)
        {
            Debug.LogWarning($"PuzzleLevelButton '{name}' is missing a SceneLoader or valid scene index.");
            button.interactable = false;
            return;
        }

        button.onClick.AddListener(() =>
        {
            if (PuzzlePersist.Instance == null)
            {
                Debug.LogError("PuzzlePersist is missing from the scene.");
                return;
            }

            PuzzlePersist.Instance.BeginPuzzlePoolSession(
                poolId: poolId,
                poolLabel: label,
                puzzlePool: puzzleDatas,
                unlockThreshold: requiredSolveRateToUnlockNext,
                nextPoolId: nextPoolIdToUnlock);

            sceneLoader.LoadThisSceneNumber(sceneToLoad);
        });
    }

    public void SetLockState(bool isLocked)
    {
        if (lockOverlay != null)
            lockOverlay.SetActive(isLocked);

        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.interactable = !isLocked;
    }
}
