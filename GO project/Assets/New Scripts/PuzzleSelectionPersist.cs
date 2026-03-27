using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;

[System.Serializable]
public class PuzzlePool
{
    public string poolId; // Stable ID used for saves. Falls back to poolName when empty.
    public bool lockByDefault = true; // Whether this pool should be locked by default
    [Range(0f,1f)] public float requiredSolveRateToUnlockNext = 1f;
    public string poolName; // "7K", "15K"
    public List<TextAsset> puzzleDatas = new(); // List of puzzles in this pool

    public string PoolId => string.IsNullOrWhiteSpace(poolId) ? poolName : poolId;
}

public class PuzzleSelectionPersist : MonoBehaviour
{
    [SceneDropdown] public int sceneToLoad;
    public PuzzleLevelButton buttonPrefab;
    public Transform buttonParent;

    [Line]

    public List<PuzzlePool> puzzlePools = new();
    
    List<PuzzleLevelButton> spawnedButtons = new();

    void Start()
    {
        SpawnAllButtons();
    }

    void SpawnAllButtons()
    {
        ClearSpawnedButtons();

        if (buttonPrefab == null || buttonParent == null)
        {
            Debug.LogError("PuzzleSelectionPersist is missing a button prefab or button parent.");
            return;
        }

        for (int i = 0; i < puzzlePools.Count; i++)
        {
            PuzzlePool pools = puzzlePools[i];
            if (pools == null)
                continue;

            PuzzleLevelButton button = Instantiate(buttonPrefab, buttonParent);
            button.Init(
                puzzleDatas: pools.puzzleDatas,
                poolId: pools.PoolId,
                label: pools.poolName,
                sceneToLoad: sceneToLoad,
                startLocked: !IsPoolUnlocked(pools),
                requiredSolveRateToUnlockNext: pools.requiredSolveRateToUnlockNext,
                nextPoolIdToUnlock: GetNextPoolId(i));
            spawnedButtons.Add(button);
        }
    }

    [Button]
    public void ClearAllSaves()
    {
        foreach (PuzzlePool puzzlePool in puzzlePools)
        {
            if (puzzlePool == null)
                continue;

            PuzzleProgressionStore.ClearPool(puzzlePool.PoolId);
        }

        PuzzleProgressionStore.Save();

        if (PuzzlePersist.Instance != null)
        {
            PuzzlePersist.Instance.ClearSelectedPuzzlePool();
        }

        RefreshSpawnedButtonStates();
    }

    bool IsPoolUnlocked(PuzzlePool puzzlePool)
    {
        if (puzzlePool == null)
            return false;

        return PuzzleProgressionStore.IsPoolUnlocked(puzzlePool.PoolId,unlockedByDefault: !puzzlePool.lockByDefault);
    }

    string GetNextPoolId(int currentIndex)
    {
        for (int i = currentIndex + 1; i < puzzlePools.Count; i++)
        {
            PuzzlePool nextPool = puzzlePools[i];
            if (nextPool == null)
                continue;

            if (!string.IsNullOrWhiteSpace(nextPool.PoolId))
                return nextPool.PoolId;
        }

        return string.Empty;
    }

    void ClearSpawnedButtons()
    {
        foreach (PuzzleLevelButton spawnedButton in spawnedButtons)
        {
            if (spawnedButton != null)
                Destroy(spawnedButton.gameObject);
        }

        spawnedButtons.Clear();
    }

    void RefreshSpawnedButtonStates()
    {
        int buttonCount = Mathf.Min(spawnedButtons.Count,puzzlePools.Count);

        for (int i = 0; i < buttonCount; i++)
        {
            PuzzleLevelButton button = spawnedButtons[i];
            if (button == null)
                continue;

            button.SetLockState(!IsPoolUnlocked(puzzlePools[i]));
        }
    }
}
