using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Collections;
using TMPro;

public class GoBoard : MonoBehaviour
{
    #region SINGLETON

    public static GoBoard Instance;
    private void Awake ()
    {
        Instance = this;
        // Ensure presetTurns is initialized
        presetTurns = new List<Turn>();
    }
    #endregion

    public string puzzleName;
    public string presetPath;  // Editable in the Inspector
    public int boardSize;
    public GameObject rowPrefab;
    public GameObject squarePrefab;
    public TextMeshProUGUI errorMessage;  // Reference to the TMP text object
    private int[,] board;

    [SerializeField] private GameObject whiteStonePrefab;
    [SerializeField] private GameObject blackStonePrefab;
    private List<int[,]> previousBoardStates;
    [SerializeField] private List<Vector2Int> playedMoves = new List<Vector2Int>(); // Tracks moves already played
    [SerializeField] private List<Move> presetMoves = new List<Move>(); // Holds valid moves from the JSON




    private List<Turn> presetTurns; // Holds the turns from JSON
    private int currentTurnIndex = 0; // Tracks the current turn



    void Start ()
    {
        previousBoardStates = new List<int[,]>();
        InitializeBoard();

        if (!string.IsNullOrEmpty(puzzleName))
        {
            LoadBoardConfiguration($"{Application.dataPath}/BoardPuzzles/{puzzleName}.json");
        }

        LoadPresetMoves(presetPath);
        InstantiateStones();
    }

    private void InitializeBoard ()
    {
        board = new int[boardSize,boardSize];

        // Clear any existing children from the board (e.g., if you're regenerating it)
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Loop through each row and column
        for (int y = 0; y < boardSize; y++)  // Y represents the row (starts from 0)
        {
            GameObject row = Instantiate(rowPrefab,transform);  // Create a row for each 'y'
            row.name = $"Row {y + 1}";  // Name rows starting from 1
            for (int x = 0; x < boardSize; x++)  // X represents the column (starts from 0)
            {
                GameObject square = Instantiate(squarePrefab,row.transform);  // Create a square in each row
                square.name = $"Square ({y + 1}, {x + 1})";  // Name squares starting from (1,1), (1,2), ...
                square.transform.position = new Vector3(x,0,y);  // Set the correct position (X, 0, Y)
            }
        }

        Debug.Log("Board initialized with size: " + boardSize);
    }
    public void LoadBoardConfiguration (string path)
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            BoardData data = JsonUtility.FromJson<BoardData>(json);

            boardSize = data.boardSize;
            board = data.ToArray();

            InstantiateStones();

            Debug.Log("Board configuration loaded from: " + path);
        }
        else
        {
            Debug.LogError("File not found at path: " + path);
        }
    }

    // PlayerMove method returns true if the move is valid
    public bool PlayerMove (int x,int y,int playerColor)
    {
        Vector2Int playerMove = new Vector2Int(x,y);
        string playerMoveString = $"{x},{y}"; // Convert the move to string format to compare with JSON

        // Check if the player's move matches any valid preset move
        foreach (var move in presetMoves)
        {
            if (move.playerMove == playerMoveString) // Compare player move with the preset
            {
                if (PlaceStone(x,y,playerColor)) // Try to place player's stone
                {
                    Debug.Log($"Player placed a stone at ({x}, {y})");

                    // Track this move as played
                    playedMoves.Add(playerMove);

                    // Process AI's move based on the matching AI response
                    ProcessAITurn(move.aiMove);
                    return true; // Move is valid
                }
            }
        }

        // Show error message and allow the player to try again
        StartCoroutine(ShowErrorMessage("Invalid move! Try again."));
        return false; // Invalid move
    }

    private IEnumerator ShowErrorMessage (string message)
    {
        // Set the error message text and make it visible
        errorMessage.text = message;
        errorMessage.gameObject.SetActive(true);

        // Wait for 2 seconds
        yield return new WaitForSeconds(2);

        // Hide the error message
        errorMessage.gameObject.SetActive(false);
    }

    public void ChangeTurn ()
    {
        Debug.Log("Changing to the next turn...");
        PopulateValidMovesForTurn(); // Ensure new valid moves are populated and exclude played moves
    }


    public void ProcessAITurn (string aiMoveString)
    {
        // Parse AI move
        string[] aiMoveParts = aiMoveString.Split(',');
        if (aiMoveParts.Length == 2)
        {
            int aiX = int.Parse(aiMoveParts[0].Trim());
            int aiY = int.Parse(aiMoveParts[1].Trim());

            StartCoroutine(AIDelayedMove(aiX,aiY));
        }
        else
        {
            Debug.LogError("Invalid AI move format.");
        }
    }

    private void PopulateValidMovesForTurn ()
    {
        if (currentTurnIndex < presetTurns.Count)
        {
            Turn currentTurn = presetTurns[currentTurnIndex];
            // validMovesForCurrentTurn = new List<Vector2Int>();  // Initialize empty list for valid moves

            // Populate the list with valid moves for the current turn
            foreach (Move move in currentTurn.moves)
            {
                string[] playerMoveParts = move.playerMove.Split(',');
                if (playerMoveParts.Length == 2)
                {
                    int validPlayerX = int.Parse(playerMoveParts[0].Trim());
                    int validPlayerY = int.Parse(playerMoveParts[1].Trim());

                    Vector2Int validMove = new Vector2Int(validPlayerX,validPlayerY);

                    // Only add the move if it hasn't been played yet
                    if (!playedMoves.Contains(validMove))
                    {
                        // validMovesForCurrentTurn.Add(validMove);  // Add valid move
                        Debug.Log($"Added valid move ({validPlayerX}, {validPlayerY}) for turn {currentTurnIndex}.");
                    }
                    else
                    {
                        Debug.Log($"Skipping move ({validPlayerX}, {validPlayerY}) because it has already been played.");
                    }
                }
            }

        }
        else
        {
            Debug.LogError("No more preset turns available.");
        }
    }



    private bool IsMoveValid (int x,int y,int player)
    {
        if (board[x,y] != 0)
            return false;

        board[x,y] = player;

        List<Vector2Int> group = GetGroup(x,y);
        if (IsGroupSurrounded(group))
        {
            foreach (Vector2Int neighbor in GetNeighbors(x,y))
            {
                int neighborPlayer = board[neighbor.x,neighbor.y];
                if (neighborPlayer != 0 && neighborPlayer != player)
                {
                    List<Vector2Int> neighborGroup = GetGroup(neighbor.x,neighbor.y);
                    if (IsGroupSurrounded(neighborGroup))
                    {
                        board[x,y] = 0;
                        return true;
                    }
                }
            }

            board[x,y] = 0;
            return false;
        }

        int[,] newState = (int[,])board.Clone();
        foreach (int[,] previousState in previousBoardStates)
        {
            if (AreBoardsEqual(previousState,newState))
            {
                board[x,y] = 0;
                return false;
            }
        }

        board[x,y] = 0;
        return true;
    }

    public bool PlaceStone (int x,int y,int player)
    {
        // Check if the coordinates are within the bounds of the board
        if (x < 0 || x >= boardSize || y < 0 || y >= boardSize)
        {
            Debug.LogError($"Invalid move coordinates: ({x}, {y}) are out of bounds.");
            return false;
        }

        // Check if the spot is already occupied or if the player number is invalid
        if (board[x,y] != 0 || player < 1 || player > 2)
        {
            return false;
        }

        board[x,y] = player;
        previousBoardStates.Add((int[,])board.Clone()); // Save board state for Ko rule

        InstantiateStones();
        return true;
    }

    public int CalculateScore (int player)
    {
        int score = 0;
        bool[,] visited = new bool[boardSize,boardSize];

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                if (board[x,y] == player)
                {
                    score++;
                }
                else if (board[x,y] == 0 && !visited[x,y])
                {
                    int areaOwner = DetermineTerritoryOwner(x,y,visited);
                    if (areaOwner == player)
                    {
                        score++;
                    }
                }
            }
        }

        return score;
    }

    private int DetermineTerritoryOwner (int x,int y,bool[,] visited)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(x,y));

        bool touchesBlack = false;
        bool touchesWhite = false;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            visited[current.x,current.y] = true;

            foreach (Vector2Int neighbor in GetNeighbors(current.x,current.y))
            {
                if (board[neighbor.x,neighbor.y] == 1)
                {
                    touchesBlack = true;
                }
                else if (board[neighbor.x,neighbor.y] == 2)
                {
                    touchesWhite = true;
                }
                else if (board[neighbor.x,neighbor.y] == 0 && !visited[neighbor.x,neighbor.y])
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        if (touchesBlack && touchesWhite)
            return 0;
        return touchesBlack ? 1 : 2;
    }

    private bool AreBoardsEqual (int[,] board1,int[,] board2)
    {
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                if (board1[x,y] != board2[x,y])
                    return false;
            }
        }
        return true;
    }

    private bool IsBoardFull ()
    {
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                if (board[x,y] == 0)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void EndGame ()
    {
        int blackScore = CalculateScore(1);
        int whiteScore = CalculateScore(2);

        Debug.Log($"Game Over! Black: {blackScore}, White: {whiteScore}");

        // Additional logic for end game, such as displaying the result
    }

    public void AIMove ()
    {
        Vector2Int move = PredictMove(GameManager.Instance.CurrentColour);
        if (move.x == -1 && move.y == -1)
            return;
        PlaceStone(move.x,move.y,GameManager.Instance.CurrentColour);
    }

    private void InstantiateStones ()
    {
        GameObject[] stones = GameObject.FindGameObjectsWithTag("Stone");
        foreach (GameObject stone in stones)
        {
            Destroy(stone);
        }

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                int color = board[x,y];

                if (color == 1)
                {
                    Instantiate(blackStonePrefab,new Vector3(x,0,y),Quaternion.identity);
                }
                else if (color == 2)
                {
                    Instantiate(whiteStonePrefab,new Vector3(x,0,y),Quaternion.identity);
                }
            }
        }
    }

    private IEnumerable<Vector2Int> GetNeighbors (int x,int y)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        if (x > 0)
            neighbors.Add(new Vector2Int(x - 1,y));
        if (x < boardSize - 1)
            neighbors.Add(new Vector2Int(x + 1,y));
        if (y > 0)
            neighbors.Add(new Vector2Int(x,y - 1));
        if (y < boardSize - 1)
            neighbors.Add(new Vector2Int(x,y + 1));
        return neighbors;
    }

    private List<Vector2Int> GetGroup (int x,int y)
    {
        int player = board[x,y];
        List<Vector2Int> group = new List<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Vector2Int startPoint = new Vector2Int(x,y);

        queue.Enqueue(startPoint);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            group.Add(current);
            visited.Add(current);

            foreach (Vector2Int neighbor in GetNeighbors(current.x,current.y))
            {
                if (board[neighbor.x,neighbor.y] == player && !visited.Contains(neighbor))
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        return group;
    }

    private bool IsGroupSurrounded (List<Vector2Int> group)
    {
        foreach (Vector2Int stone in group)
        {
            foreach (Vector2Int neighbor in GetNeighbors(stone.x,stone.y))
            {
                if (board[neighbor.x,neighbor.y] == 0)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void RemoveGroup (List<Vector2Int> group)
    {
        foreach (Vector2Int stone in group)
        {
            board[stone.x,stone.y] = 0;
        }
    }

    public Vector2Int PredictMove (int player)
    {
        int opponent = player == 1 ? 2 : 1;
        List<Vector2Int> candidateMoves = new List<Vector2Int>();

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                if (board[x,y] == 0)
                {
                    bool isCandidateMove = false;

                    foreach (Vector2Int neighbor in GetNeighbors(x,y))
                    {
                        if (board[neighbor.x,neighbor.y] == opponent)
                        {
                            List<Vector2Int> opponentGroup = GetGroup(neighbor.x,neighbor.y);
                            if (IsGroupSurrounded(opponentGroup))
                            {
                                isCandidateMove = true;
                                break;
                            }
                        }
                    }

                    if (!isCandidateMove)
                    {
                        foreach (Vector2Int neighbor in GetNeighbors(x,y))
                        {
                            if (board[neighbor.x,neighbor.y] == player)
                            {
                                List<Vector2Int> playerGroup = GetGroup(neighbor.x,neighbor.y);
                                if (!IsGroupSurrounded(playerGroup))
                                {
                                    isCandidateMove = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (isCandidateMove)
                    {
                        candidateMoves.Add(new Vector2Int(x,y));
                    }
                }
            }
        }

        if (candidateMoves.Count == 0)
        {
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    if (board[x,y] == 0)
                    {
                        candidateMoves.Add(new Vector2Int(x,y));
                    }
                }
            }
        }

        if (candidateMoves.Count > 0)
        {
            return candidateMoves[Random.Range(0,candidateMoves.Count)];
        }
        else
        {
            return new Vector2Int(-1,-1);
        }
    }

    [System.Serializable]
    public class BoardData
    {
        public int[] boardFlat;
        public int boardSize;

        public BoardData (int[,] board,int size)
        {
            boardSize = size;
            boardFlat = new int[boardSize * boardSize];
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    boardFlat[y * boardSize + x] = board[x,y];
                }
            }
        }

        public int[,] ToArray ()
        {
            int[,] array = new int[boardSize,boardSize];
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    array[x,y] = boardFlat[y * boardSize + x];
                }
            }
            return array;
        }
    }

    public void LoadPresetMoves (string path)
    {
        if (!Path.IsPathRooted(path))
        {
            path = Path.Combine(Application.dataPath,path);
        }

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            PresetMovesData data = JsonUtility.FromJson<PresetMovesData>(json);
            presetMoves = data.presetMoves;
            Debug.Log("Preset moves loaded successfully.");
        }
        else
        {
            Debug.LogError($"Preset moves file not found at path: {path}");
        }
    }

    private IEnumerator AIDelayedMove (int aiX,int aiY)
    {
        yield return new WaitForSeconds(1.0f);

        // Place AI's stone
        if (PlaceStone(aiX,aiY,2)) // 2 for white (AI)
        {
            Debug.Log($"AI placed a stone at ({aiX}, {aiY})");
            GameManager.Instance.ChangeTurn();
        }
        else
        {
            Debug.LogError($"Failed to place AI stone at ({aiX}, {aiY})");
        }
    }

    // JSON data structure for preset turns
    [System.Serializable]
    public class Move
    {
        public string playerMove;
        public string aiMove;
    }

    [System.Serializable]
    public class Turn
    {
        public List<Move> moves = new List<Move>();
    }

    [System.Serializable]
    public class PresetMovesData
    {
        public List<Move> presetMoves;
    }



}
