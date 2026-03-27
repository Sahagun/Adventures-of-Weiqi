using System.Collections.Generic;
using UnityEngine;

public class CaptureManager : MonoBehaviour
{
    [SerializeField] private CubeGrid cubeGrid;
    private int[,] boardState;
    private int gridSize;
    private List<GameObject> cubeObjects;

    // Simple-4 neighbors
    private readonly int[,] directions = new int[,]
    {
        { 0, 1 },  // Up    (y+1)
        { 0, -1 }, // Down  (y-1)
        { 1, 0 },  // Right (x+1)
        { -1, 0 }  // Left  (x-1)
    };

    // ---- KO STATE (one-point, one-turn) ----
    public Vector2Int? KoPoint { get; private set; } = null; // 0-based (x,y)
    public int KoBannedPlayer { get; private set; } = 0;      // 1 or 2; 0 = none

    // Exposed capture count per pass so CubeGrid can know if a placement captured anything
    public int LastRemovedCount { get; private set; } = 0;

    private void Start ()
    {
        RefreshFromCubeGrid();
    }

    // -------- Public ko helpers for CubeGrid --------

    // Call BEFORE placing a stone. x,y are 0-based board indices.
    public bool IsMoveKoBlocked (int x,int y,int player)
    {
        return KoPoint.HasValue &&
               KoBannedPlayer == player &&
               KoPoint.Value.x == x && KoPoint.Value.y == y;
    }

    // Call AFTER a stone is successfully placed (anywhere).
    // If the player who was banned just moved (and it wasn't the recapture, which is blocked anyway),
    // the ko restriction expires.
    public void ExpireKoIfBannedPlayerMoved (int player,Vector2Int placed)
    {
        if (KoBannedPlayer == player)
        {
            // Their turn has passed; ko no longer applies.
            ClearKo();
        }
    }

    // -------- Capture evaluation --------

    // Backward-compatible entry point
    public void CheckForCaptures ()
    {
        CheckForCaptures(null,0,false);
    }

    /// <summary>
    /// Capture check that can treat the just-placed group as alive for that tick (used for ko/throw-in semantics).
    /// </summary>
    public void CheckForCaptures (Vector2Int? exemptStone,int exemptPlayer,bool treatExemptGroupAsAlive)
    {
        if (cubeGrid == null)
        {
            Debug.LogError("CaptureManager: CubeGrid reference is missing.");
            return;
        }

        boardState = cubeGrid.GetBoardState();
        gridSize = cubeGrid.gridSize;
        cubeObjects = cubeGrid.GetCubeObjects();

        // Per-pass bookkeeping
        LastRemovedCount = 0;

        // IMPORTANT: recompute ko fresh each pass; don't carry old ko forward inadvertently
        ClearKo();

        // 1) Scan all groups, collect ALL stones to remove this pass
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        List<Vector2Int> allCaptured = new List<Vector2Int>();

        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                int player = boardState[y,x];
                if (player == 0)
                    continue;

                Vector2Int p = new Vector2Int(x,y);
                if (visited.Contains(p))
                    continue;

                List<Vector2Int> group = new List<Vector2Int>();
                HashSet<Vector2Int> libs = new HashSet<Vector2Int>();

                bool hasLiberties = FloodFill(
                    x,y,player,visited,group,
                    exemptStone,
                    // If the discovered group is the freshly placed player group and a ko-test flag is set,
                    // consider it alive for this pass only.
                    treatExemptGroupAsAlive && exemptStone.HasValue && player == exemptPlayer,
                    libs
                );

                if (!hasLiberties)
                {
                    // Capture this whole group
                    Debug.Log($"Group at ({x}, {y}) is captured. Removing pieces.");
                    allCaptured.AddRange(group);
                }
            }
        }

        // 2) Perform removals (if any)
        if (allCaptured.Count > 0)
        {
            RemoveCapturedStones(allCaptured);
            LastRemovedCount = allCaptured.Count;
        }

        // 3) Compute ko (simple-ko): only when a single stone was captured and the capturing group’s
        //    only liberty afterwards is exactly the captured point.
        if (exemptStone.HasValue && allCaptured.Count == 1)
        {
            // Re-evaluate the placed stone group AFTER removal
            HashSet<Vector2Int> v2 = new HashSet<Vector2Int>();
            List<Vector2Int> g2 = new List<Vector2Int>();
            HashSet<Vector2Int> libs2 = new HashSet<Vector2Int>();

            // No exemptions here; we want actual liberties after capture
            FloodFill(exemptStone.Value.x,exemptStone.Value.y,exemptPlayer,v2,g2,null,false,libs2);

            // If the only liberty is the captured point, set ko against the opponent.
            if (libs2.Count == 1 && libs2.Contains(allCaptured[0]))
            {
                SetKo(allCaptured[0],OpponentOf(exemptPlayer));
            }
            // else: leave ko cleared (no ko snapshot)
        }
        // else: leave ko cleared
    }

    /// <summary>
    /// Flood fill: collects the connected group for (startX,startY) and optionally its liberties into libertiesOut.
    /// If treatExemptGroupAsAlive == true and the group contains exemptStone, we force "alive" for this pass.
    /// </summary>
    public bool FloodFill (
        int startX,
        int startY,
        int player,
        HashSet<Vector2Int> visited,
        List<Vector2Int> group,
        Vector2Int? exemptStone = null,
        bool treatExemptGroupAsAlive = false,
        HashSet<Vector2Int> libertiesOut = null)
    {
        Queue<Vector2Int> toVisit = new Queue<Vector2Int>();
        toVisit.Enqueue(new Vector2Int(startX,startY));
        bool hasLiberties = false;

        while (toVisit.Count > 0)
        {
            Vector2Int current = toVisit.Dequeue();
            if (visited.Contains(current))
                continue;

            visited.Add(current);
            group.Add(current);

            for (int d = 0; d < 4; d++)
            {
                int nx = current.x + directions[d,0];
                int ny = current.y + directions[d,1];

                if (nx < 0 || ny < 0 || nx >= gridSize || ny >= gridSize)
                    continue;

                if (boardState[ny,nx] == 0)
                {
                    hasLiberties = true;
                    libertiesOut?.Add(new Vector2Int(nx,ny));
                }
                else if (boardState[ny,nx] == player)
                {
                    Vector2Int np = new Vector2Int(nx,ny);
                    if (!visited.Contains(np))
                        toVisit.Enqueue(np);
                }
            }
        }

        // Bless the just-placed group for ko/throw-in test placements
        if (!hasLiberties && treatExemptGroupAsAlive && exemptStone.HasValue && group.Contains(exemptStone.Value))
        {
            hasLiberties = true;
        }

        return hasLiberties;
    }

    private void RemoveCapturedStones (List<Vector2Int> stones)
    {
        foreach (Vector2Int pos in stones)
        {
            if (pos.x < 0 || pos.x >= gridSize || pos.y < 0 || pos.y >= gridSize)
                continue;
            if (boardState[pos.y,pos.x] == 0)
                continue;

            boardState[pos.y,pos.x] = 0; // Update the board state

            string tileName = $"({pos.y + 1},{pos.x + 1})";
            GameObject gridTile = cubeGrid.CubeObjects.Find(cube => cube.name == tileName);

            if (gridTile != null)
            {
                foreach (Transform child in gridTile.transform)
                    Object.Destroy(child.gameObject);
            }
        }

        // Persist for debugging/restore
        cubeGrid.SaveBoardStateToJson(Application.persistentDataPath + "/TempBoardState.json");
        Debug.Log("Captured stones removed and board state saved.");
    }

    private void SetKo (Vector2Int pt,int bannedPlayer)
    {
        KoPoint = pt;
        KoBannedPlayer = bannedPlayer;
        Debug.Log($"KO set at ({pt.y + 1},{pt.x + 1}); banned player = {bannedPlayer}");
    }

    public void ClearKo ()
    {
        KoPoint = null;
        KoBannedPlayer = 0;
    }

    public void RefreshFromCubeGrid ()
    {
        cubeGrid = FindObjectOfType<CubeGrid>();

        if (cubeGrid == null)
        {
            Debug.LogError("CaptureManager: CubeGrid not found!");
            return;
        }

        boardState = cubeGrid.GetBoardState();
        gridSize = cubeGrid.gridSize;
        cubeObjects = cubeGrid.GetCubeObjects();
        LastRemovedCount = 0;
        ClearKo();
    }

    private static int OpponentOf (int player) => player == 1 ? 2 : 1;

    public void DebugBoardState ()
    {
        string boardVisual = "";
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
                boardVisual += boardState[y,x] + " ";
            boardVisual += "\n";
        }
        Debug.Log($"Current Board State:\n{boardVisual}");
    }
}
