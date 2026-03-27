
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class GoPuzzleEditor : EditorWindow
{
    private int boardSize = 9;
    private int[,] board;
    private int currentPlayer = 1;
    private string puzzleName = "newPuzzle";
    private List<Move> presetMoves = new List<Move>();
    private Vector2 scrollPosition;

    private enum CoordOrder { RowCol, ColRow }
    private CoordOrder inputOrder = CoordOrder.RowCol;

    [MenuItem("Tools/Go Puzzle Builder (MATCH CUBEGRID)")]
    public static void ShowWindow ()
    {
        var window = GetWindow<GoPuzzleEditor>("Go Puzzle Builder");
        window.InitializeBoard();
    }

    private void InitializeBoard ()
    {
        board = new int[boardSize,boardSize]; // board[x,y], where y=0 is BOTTOM (matches CubeGrid)
    }

    private void OnGUI ()
    {
        GUILayout.Label("Go Puzzle Builder",EditorStyles.boldLabel);
        puzzleName = EditorGUILayout.TextField("Puzzle Name",puzzleName);

        int newBoardSize = EditorGUILayout.IntSlider("Board Size",boardSize,5,19);
        if (newBoardSize != boardSize)
        {
            boardSize = newBoardSize;
            InitializeBoard();
        }

        if (GUILayout.Button("Switch Player"))
            currentPlayer = currentPlayer == 1 ? 2 : 1;
        GUILayout.Label("Current Player: " + (currentPlayer == 1 ? "Black (1)" : "White (2)"),EditorStyles.helpBox);

        inputOrder = (CoordOrder)EditorGUILayout.EnumPopup(
            new GUIContent("Coordinate Input Order","How you TYPE coordinates. JSON always saves (row,col)."),
            inputOrder
        );

        EditorGUILayout.HelpBox("Storage matches runtime: boardFlat[0] = bottom row, left column. Bottom-left is (1,1).",MessageType.Info);

        DrawBoardGrid();
        GUILayout.Space(10);

        if (GUILayout.Button("Add Preset Move"))
            presetMoves.Add(new Move());

        DrawPresetMovesEditor();
        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save Puzzle to JSON"))
            SavePuzzleToJson();
        if (GUILayout.Button("Load Puzzle from JSON"))
            LoadPuzzleFromJson();
        GUILayout.EndHorizontal();
    }

    private void DrawBoardGrid ()
    {
        // Column labels (1..N)
        GUILayout.BeginHorizontal();
        GUILayout.Space(48);
        for (int x = 1; x <= boardSize; x++)
            GUILayout.Label(x.ToString(),GUILayout.Width(30),GUILayout.Height(22));
        GUILayout.EndHorizontal();

        // Draw TOP -> BOTTOM so left gutter shows boardSize at top, 1 at bottom (like your screenshot),
        // but DO NOT remap indices: board[x,y] keeps y=0 at bottom.
        for (int y = boardSize - 1; y >= 0; y--)
        {
            int displayRow = y + 1;

            GUILayout.BeginHorizontal();
            GUILayout.Label(displayRow.ToString(),GUILayout.Width(48),GUILayout.Height(30));

            for (int x = 0; x < boardSize; x++)
            {
                int value = board[x,y];
                string stone = value == 0 ? "." : (value == 1 ? "○" : "●");
                var content = new GUIContent(stone,$"(row,col)=({y + 1},{x + 1})");

                if (GUILayout.Button(content,GUILayout.Width(30),GUILayout.Height(30)))
                    board[x,y] = value == 0 ? currentPlayer : 0;
            }

            GUILayout.EndHorizontal();
        }

        // Column labels bottom
        GUILayout.BeginHorizontal();
        GUILayout.Space(48);
        for (int x = 1; x <= boardSize; x++)
            GUILayout.Label(x.ToString(),GUILayout.Width(30),GUILayout.Height(22));
        GUILayout.EndHorizontal();
    }

    private void DrawPresetMovesEditor ()
    {
        GUILayout.Label("Preset Turns",EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for (int i = 0; i < presetMoves.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Main Move {i + 1}",EditorStyles.boldLabel);
            Move move = presetMoves[i];

            move.playerMove = EditorGUILayout.TextField("Player Move",move.playerMove);
            move.aiMove = EditorGUILayout.TextField("AI Move",move.aiMove);

            move.isKoMove = EditorGUILayout.ToggleLeft(new GUIContent("Ko applies to this move"),move.isKoMove);

            // Correct list
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Correct Responses",EditorStyles.miniBoldLabel);
            if (move.correctMoves == null)
                move.correctMoves = new List<PlayerAIPair>();

            for (int j = 0; j < move.correctMoves.Count; j++)
            {
                var pair = move.correctMoves[j];
                EditorGUILayout.BeginVertical("helpbox");
                pair.playerMove = EditorGUILayout.TextField("Player",pair.playerMove);
                pair.aiMove = EditorGUILayout.TextField("AI",pair.aiMove);
                pair.isKoMove = EditorGUILayout.ToggleLeft(new GUIContent("Ko applies"),pair.isKoMove);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Delete",GUILayout.Width(80)))
                {
                    move.correctMoves.RemoveAt(j);
                    GUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                GUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUILayout.Space(4);
            }

            if (GUILayout.Button("Add Correct Move"))
                move.correctMoves.Add(new PlayerAIPair());

            // Wrong list
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Wrong Responses",EditorStyles.miniBoldLabel);
            if (move.wrongMoves == null)
                move.wrongMoves = new List<PlayerAIPair>();

            for (int j = 0; j < move.wrongMoves.Count; j++)
            {
                var pair = move.wrongMoves[j];
                EditorGUILayout.BeginVertical("helpbox");
                pair.playerMove = EditorGUILayout.TextField("Player",pair.playerMove);
                pair.aiMove = EditorGUILayout.TextField("AI",pair.aiMove);
                pair.isKoMove = EditorGUILayout.ToggleLeft(new GUIContent("Ko applies"),pair.isKoMove);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Delete",GUILayout.Width(80)))
                {
                    move.wrongMoves.RemoveAt(j);
                    EditorGUILayout.EndVertical();
                    break;
                }
                GUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUILayout.Space(4);
            }

            if (GUILayout.Button("Add Wrong Move"))
                move.wrongMoves.Add(new PlayerAIPair());

            EditorGUILayout.Space(6);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Delete Preset",GUILayout.Width(120)))
            {
                presetMoves.RemoveAt(i);
                EditorGUILayout.EndVertical();
                break;
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
        }

        EditorGUILayout.EndScrollView();
    }

    // --- Coordinate helpers: always normalize to (row,col) in JSON ---
    private bool TryParseCoord (string text,out int a,out int b)
    {
        a = b = 0;
        if (string.IsNullOrWhiteSpace(text))
            return false;
        string t = text.Trim();
        if (t.StartsWith("(") && t.EndsWith(")"))
            t = t.Substring(1,t.Length - 2);
        char[] seps = new[] { ',',' ',':',';','/' };
        var parts = t.Split(seps,System.StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
            return false;
        return int.TryParse(parts[0],out a) && int.TryParse(parts[1],out b);
    }

    private string NormalizeToRowCol (string textIn)
    {
        if (string.IsNullOrWhiteSpace(textIn))
            return textIn;
        if (!TryParseCoord(textIn,out int first,out int second))
        {
            string t = textIn.Trim();
            if (!t.StartsWith("(") && !t.EndsWith(")"))
                t = $"({t})";
            return t;
        }

        int row, col;
        if (inputOrder == CoordOrder.RowCol)
        { row = first; col = second; }
        else
        { col = first; row = second; }

        return $"({row},{col})";
    }

    private void SavePuzzleToJson ()
    {
        string path = EditorUtility.SaveFilePanel("Save Go Puzzle","",$"{puzzleName}.json","json");
        if (string.IsNullOrEmpty(path))
            return;

        // Normalize all moves to (row,col) before saving
        foreach (var move in presetMoves)
        {
            move.playerMove = NormalizeToRowCol(move.playerMove);
            move.aiMove = NormalizeToRowCol(move.aiMove);

            foreach (var correctMove in move.correctMoves)
            {
                correctMove.playerMove = NormalizeToRowCol(correctMove.playerMove);
                correctMove.aiMove = NormalizeToRowCol(correctMove.aiMove);
            }
            foreach (var wrongMove in move.wrongMoves)
            {
                wrongMove.playerMove = NormalizeToRowCol(wrongMove.playerMove);
                wrongMove.aiMove = NormalizeToRowCol(wrongMove.aiMove);
            }
        }

        GoPuzzleData data = new GoPuzzleData(board,boardSize,presetMoves);
        string json = JsonUtility.ToJson(data,true);
        File.WriteAllText(path,json);
        Debug.Log("Saved puzzle to: " + path);
    }

    private void LoadPuzzleFromJson ()
    {
        string path = EditorUtility.OpenFilePanel("Load Go Puzzle","","json");
        if (string.IsNullOrEmpty(path))
            return;

        string json = File.ReadAllText(path);
        GoPuzzleData data = JsonUtility.FromJson<GoPuzzleData>(json);
        boardSize = data.boardSize;
        board = data.ToArray();
        presetMoves = data.moves ?? new List<Move>();
        Debug.Log("Loaded puzzle from: " + path);
    }

    [System.Serializable]
    public class PlayerAIPair
    {
        public string playerMove;
        public string aiMove;
        public bool isKoMove;
    }

    [System.Serializable]
    public class Move
    {
        public string playerMove;
        public string aiMove;
        public bool isKoMove;
        public List<PlayerAIPair> correctMoves = new List<PlayerAIPair>();
        public List<PlayerAIPair> wrongMoves = new List<PlayerAIPair>();
    }

    [System.Serializable]
    public class GoPuzzleData
    {
        public int boardSize;
        public int[] boardFlat;
        public List<Move> moves;

        public GoPuzzleData (int[,] board,int size,List<Move> presetMoves)
        {
            boardSize = size;
            moves = presetMoves ?? new List<Move>();
            boardFlat = new int[boardSize * boardSize];

            // STORAGE MATCHES CUBEGRID: row 0 = bottom row. No flips.
            for (int y = 0; y < boardSize; y++)
                for (int x = 0; x < boardSize; x++)
                    boardFlat[y * boardSize + x] = board[x,y];
        }

        public int[,] ToArray ()
        {
            int[,] array = new int[boardSize,boardSize];
            // No flip on load either.
            for (int y = 0; y < boardSize; y++)
                for (int x = 0; x < boardSize; x++)
                    array[x,y] = boardFlat[y * boardSize + x];
            return array;
        }
    }
}
