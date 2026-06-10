using System;
using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Builds the "Daily Puzzles" set and launches it through the normal puzzle-pool flow.
///
/// Rules (from the design doc):
///  - Grabs 10 random puzzles from ALL unlocked puzzle-leveling pools.
///  - At least 3 of them come from the highest unlocked pool.
///  - The same set is reused for 24 hours, then a fresh set is rolled.
///  - Percent correct is reported by the existing pool-completion UI (MultiPuzzleManager).
///
/// Place this in the Training Hub scene and wire the top "Daily Puzzles" button to
/// <see cref="StartDailyPuzzles"/>. Fill <see cref="pools"/> from EASIEST (e.g. 15k) to
/// HARDEST (e.g. 1k); the "highest unlocked" pool is the most advanced unlocked entry.
/// </summary>
public class DailyPuzzleManager : MonoBehaviour
{
    [Serializable]
    public class DailyPool
    {
        [Tooltip("Stable id; must match the poolId used in Puzzle Leveling so unlock state lines up.")]
        public string poolId;
        public bool unlockedByDefault = false;
        public List<TextAsset> puzzles = new();
    }

    [Title("Pools (order EASIEST -> HARDEST)")]
    [SerializeField] private List<DailyPool> pools = new();
    [Tooltip("Optional: import pools from an existing Puzzle Leveling PuzzleSelectionPersist instead of filling the list above. Auto-found if left empty.")]
    [SerializeField] private PuzzleSelectionPersist poolSource;

    [Title("Selection")]
    [SerializeField] private int dailyPuzzleCount = 10;
    [SerializeField] private int minimumFromHighestPool = 3;
    [Tooltip("Hours before a fresh daily set is rolled.")]
    [SerializeField] private float refreshHours = 24f;

    [Title("Launch")]
    [SerializeField] private string poolLabel = "Daily Puzzles";
    [Tooltip("Scene that contains the MultiPuzzleManager and plays a pool.")]
    [SceneDropdown] public int puzzlePlaySceneIndex = -1;

    private const string PoolIdKey = "DailyPuzzles";
    private const string LastRollTicksKey = "DailyPuzzles.LastRollTicks";
    private const string SelectionKey = "DailyPuzzles.Selection";

    private void Awake()
    {
        EnsurePoolsFromSource();
    }

    // Imports pools from a Puzzle Leveling PuzzleSelectionPersist so the puzzle lists aren't duplicated.
    private void EnsurePoolsFromSource()
    {
        if (pools != null && pools.Count > 0)
            return;

        PuzzleSelectionPersist source = poolSource != null ? poolSource : FindObjectOfType<PuzzleSelectionPersist>(true);
        if (source == null || source.puzzlePools == null)
            return;

        pools = new List<DailyPool>();
        foreach (PuzzlePool sourcePool in source.puzzlePools)
        {
            if (sourcePool == null)
                continue;

            pools.Add(new DailyPool
            {
                poolId = sourcePool.PoolId,
                unlockedByDefault = !sourcePool.lockByDefault,
                puzzles = sourcePool.puzzleDatas != null ? new List<TextAsset>(sourcePool.puzzleDatas) : new List<TextAsset>()
            });
        }
    }

    /// <summary>Hook this to the Training Hub "Daily Puzzles" button.</summary>
    public void StartDailyPuzzles()
    {
        EnsurePoolsFromSource();
        List<TextAsset> selection = GetOrRollSelection();
        if (selection == null || selection.Count == 0)
        {
            Debug.LogWarning("DailyPuzzleManager: no unlocked puzzles available to build a daily set.");
            return;
        }

        if (PuzzlePersist.Instance == null)
        {
            Debug.LogWarning("DailyPuzzleManager: PuzzlePersist is required to start a pool.");
            return;
        }

        // unlockThreshold 1f and empty nextPool => completing the daily set unlocks nothing.
        PuzzlePersist.Instance.BeginPuzzlePoolSession(
            poolId: PoolIdKey,
            poolLabel: poolLabel,
            puzzlePool: selection,
            unlockThreshold: 1f,
            nextPoolId: string.Empty);

        LoadScene(puzzlePlaySceneIndex);
    }

    // ---------- Selection / 24h cache ----------

    private List<TextAsset> GetOrRollSelection()
    {
        if (IsSelectionStillFresh())
        {
            List<TextAsset> cached = RebuildSelectionFromStorage();
            if (cached != null && cached.Count > 0)
                return cached;
        }

        List<PuzzleRef> rolled = RollNewSelection();
        StoreSelection(rolled);
        return ResolveRefs(rolled);
    }

    private bool IsSelectionStillFresh()
    {
        string raw = PlayerPrefs.GetString(LastRollTicksKey, string.Empty);
        if (string.IsNullOrEmpty(raw) || !long.TryParse(raw, out long lastTicks))
            return false;

        DateTime lastRoll = new DateTime(lastTicks, DateTimeKind.Utc);
        double hoursElapsed = (DateTime.UtcNow - lastRoll).TotalHours;
        return hoursElapsed >= 0 && hoursElapsed < refreshHours;
    }

    private List<PuzzleRef> RollNewSelection()
    {
        List<DailyPool> unlocked = GetUnlockedPools();
        List<PuzzleRef> result = new List<PuzzleRef>();
        if (unlocked.Count == 0)
            return result;

        HashSet<string> used = new HashSet<string>();

        // 1) At least N from the highest unlocked pool.
        DailyPool highest = unlocked[unlocked.Count - 1];
        foreach (PuzzleRef puzzleRef in TakeRandom(BuildRefs(highest), minimumFromHighestPool, used))
            result.Add(puzzleRef);

        // 2) Fill the rest from ALL unlocked pools (highest included).
        List<PuzzleRef> everything = new List<PuzzleRef>();
        foreach (DailyPool pool in unlocked)
            everything.AddRange(BuildRefs(pool));

        int remaining = Mathf.Max(0, dailyPuzzleCount - result.Count);
        foreach (PuzzleRef puzzleRef in TakeRandom(everything, remaining, used))
            result.Add(puzzleRef);

        return result;
    }

    private List<DailyPool> GetUnlockedPools()
    {
        List<DailyPool> unlocked = new List<DailyPool>();
        foreach (DailyPool pool in pools)
        {
            if (pool == null || pool.puzzles == null || pool.puzzles.Count == 0)
                continue;

            if (PuzzleProgressionStore.IsPoolUnlocked(pool.poolId, pool.unlockedByDefault))
                unlocked.Add(pool);
        }

        return unlocked;
    }

    private List<PuzzleRef> BuildRefs(DailyPool pool)
    {
        List<PuzzleRef> refs = new List<PuzzleRef>();
        for (int i = 0; i < pool.puzzles.Count; i++)
        {
            if (pool.puzzles[i] != null)
                refs.Add(new PuzzleRef(pool.poolId, i));
        }

        return refs;
    }

    // Fisher-Yates-style random pick of up to count entries not already in `used`.
    private List<PuzzleRef> TakeRandom(List<PuzzleRef> source, int count, HashSet<string> used)
    {
        List<PuzzleRef> picked = new List<PuzzleRef>();
        if (count <= 0 || source == null || source.Count == 0)
            return picked;

        List<PuzzleRef> pool = new List<PuzzleRef>(source);
        while (picked.Count < count && pool.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, pool.Count);
            PuzzleRef candidate = pool[index];
            pool.RemoveAt(index);

            string key = candidate.Key;
            if (used.Contains(key))
                continue;

            used.Add(key);
            picked.Add(candidate);
        }

        return picked;
    }

    private void StoreSelection(List<PuzzleRef> selection)
    {
        if (selection == null || selection.Count == 0)
            return;

        List<string> tokens = new List<string>(selection.Count);
        foreach (PuzzleRef puzzleRef in selection)
            tokens.Add(puzzleRef.Key);

        PlayerPrefs.SetString(SelectionKey, string.Join(",", tokens));
        PlayerPrefs.SetString(LastRollTicksKey, DateTime.UtcNow.Ticks.ToString());
        PlayerPrefs.Save();
    }

    private List<TextAsset> RebuildSelectionFromStorage()
    {
        string raw = PlayerPrefs.GetString(SelectionKey, string.Empty);
        if (string.IsNullOrEmpty(raw))
            return null;

        List<PuzzleRef> refs = new List<PuzzleRef>();
        foreach (string token in raw.Split(','))
        {
            if (PuzzleRef.TryParse(token, out PuzzleRef puzzleRef))
                refs.Add(puzzleRef);
        }

        return ResolveRefs(refs);
    }

    private List<TextAsset> ResolveRefs(List<PuzzleRef> refs)
    {
        List<TextAsset> assets = new List<TextAsset>();
        if (refs == null)
            return assets;

        foreach (PuzzleRef puzzleRef in refs)
        {
            DailyPool pool = pools.Find(p => p != null && p.poolId == puzzleRef.PoolId);
            if (pool == null || puzzleRef.Index < 0 || puzzleRef.Index >= pool.puzzles.Count)
                continue;

            TextAsset asset = pool.puzzles[puzzleRef.Index];
            if (asset != null)
                assets.Add(asset);
        }

        return assets;
    }

    private void LoadScene(int sceneIndex)
    {
        if (sceneIndex < 0)
        {
            Debug.LogWarning("DailyPuzzleManager: puzzlePlaySceneIndex is not configured.");
            return;
        }

        if (Moddwyn.SceneLoader.Instance != null)
        {
            Moddwyn.SceneLoader.Instance.LoadScene(sceneIndex);
            return;
        }

        SceneManager.LoadScene(sceneIndex);
    }

    private struct PuzzleRef
    {
        public readonly string PoolId;
        public readonly int Index;

        public PuzzleRef(string poolId, int index)
        {
            PoolId = poolId;
            Index = index;
        }

        public string Key => $"{PoolId}:{Index}";

        public static bool TryParse(string token, out PuzzleRef puzzleRef)
        {
            puzzleRef = default;
            if (string.IsNullOrWhiteSpace(token))
                return false;

            int separator = token.LastIndexOf(':');
            if (separator <= 0 || separator >= token.Length - 1)
                return false;

            string poolId = token.Substring(0, separator);
            if (!int.TryParse(token.Substring(separator + 1), out int index))
                return false;

            puzzleRef = new PuzzleRef(poolId, index);
            return true;
        }
    }
}
