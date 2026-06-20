using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Single, project-wide source of board configuration.
///
/// Instead of every scene's CubeGrid carrying its own board-visuals list, padding,
/// and stone-sizing values, all of that lives here in ONE asset placed in a
/// Resources folder (Assets/Resources/GoBoardLibrary.asset). Every CubeGrid loads
/// it automatically by board size, so you configure boards in a single place.
///
/// If the asset is missing, CubeGrid falls back to its own serialized fields, so
/// nothing breaks.
/// </summary>
[CreateAssetMenu(fileName = "GoBoardLibrary", menuName = "Go/Board Library")]
public class GoBoardLibrary : ScriptableObject
{
    [System.Serializable]
    public class BoardEntry
    {
        [Tooltip("Board size this entry applies to (e.g. 5 for a 5x5 board).")]
        public int gridSize = 9;

        [Tooltip("Board image shown for this size.")]
        public Sprite boardSprite;

        [Tooltip("Moves the whole stone grid (world units) to line it up with this board image. " +
                 "X shifts left/right, Y shifts up/down on the board. Applied to every stone equally.")]
        public Vector2 gridOffset = Vector2.zero;

        [Tooltip("Pulls the OUTER stones inward by this many world units on each side. " +
                 "Use it when the edge stones sit just outside the painted lines. 0 = stones span the full image.")]
        public Vector2 gridInset = Vector2.zero;

        [Tooltip("Stone size for THIS board: fraction of a cell a stone fills (0.6 = 60%). " +
                 "Leave at 0 to use the global Stone Cell Fill Fraction below.")]
        [Range(0f, 1f)] public float stoneFillFraction = 0f;
    }

    [Header("Per-board-size art + alignment")]
    public List<BoardEntry> boards = new List<BoardEntry>();

    [Header("Global stone sizing (applied to every board when enabled)")]
    [Tooltip("When on, these stone-sizing values override whatever each scene's CubeGrid has.")]
    public bool overrideStoneSettings = true;
    public bool scaleStonesWithBoardSize = true;
    [Tooltip("Global stone size: fraction of a cell a stone fills (0.6 = 60%, 1.0 = touching). " +
             "Per-board entries can override this with their own Stone Fill Fraction.")]
    [Range(0.1f, 1f)] public float stoneCellFillFraction = 0.6f;
    public float stoneScaleMultiplier = 1f;
    public Vector2 stoneScaleClamp = new Vector2(0.1f, 6f);

    [Header("Optional shared prefabs (used only if a scene leaves its own empty)")]
    public GameObject playerStonePrefab;
    public GameObject computerStonePrefab;
    public GameObject gridTilePrefab;

    // ---------------- Global access (no scene reference needed) ----------------

    private static GoBoardLibrary cached;
    private static bool attemptedLoad;

    /// <summary>The shared library loaded from Resources, or null if none exists.</summary>
    public static GoBoardLibrary Instance
    {
        get
        {
            if (cached == null && !attemptedLoad)
            {
                attemptedLoad = true;
                cached = Resources.Load<GoBoardLibrary>("GoBoardLibrary");
                if (cached == null)
                    Debug.LogWarning("GoBoardLibrary: no asset found at Resources/GoBoardLibrary. " +
                                     "Scenes will use their own CubeGrid settings until one is created.");
            }
            return cached;
        }
    }

    /// <summary>
    /// Finds the entry for a board size. Returns the exact match if present,
    /// otherwise the closest available size (so a missing size still renders).
    /// </summary>
    public bool TryGetBoard(int size, out BoardEntry entry)
    {
        entry = null;
        if (boards == null || boards.Count == 0)
            return false;

        BoardEntry closest = null;
        int closestDistance = int.MaxValue;

        foreach (BoardEntry candidate in boards)
        {
            if (candidate == null || candidate.boardSprite == null)
                continue;

            if (candidate.gridSize == size)
            {
                entry = candidate;
                return true;
            }

            int distance = Mathf.Abs(candidate.gridSize - size);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = candidate;
            }
        }

        entry = closest;
        return entry != null;
    }
}
