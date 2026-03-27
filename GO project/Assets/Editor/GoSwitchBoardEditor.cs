using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class GoSwitchBoardEditor : EditorWindow
{
    private int boardSize = 9; // Default board size
    private int[,] board;
    private int currentPlayer = 1; // 1 for black, 2 for white
    private CubeGrid cubeGrid;

    [MenuItem("Window/Go Switch Board Editor")]
    public static void ShowWindow ()
    {
        GetWindow<GoSwitchBoardEditor>("Go Switch Board Editor");
    }

    private void OnEnable ()
    {
        RefreshBoardState();
    }

    private void RefreshBoardState ()
    {
        cubeGrid = FindObjectOfType<CubeGrid>();

        if (cubeGrid != null)
        {
            var gridTiles = cubeGrid.CubeObjects;
            boardSize = cubeGrid.gridSize;
            board = new int[boardSize,boardSize];

            foreach (var tile in gridTiles)
            {
                string tileName = tile.name;
                (int x, int y) = ParseTileName(tileName);

                if (x >= 0 && y >= 0 && x < boardSize && y < boardSize)
                {
                    foreach (Transform child in tile.transform)
                    {
                        if (child.CompareTag("Black"))
                        {
                            board[y,x] = 1; // Black stone
                        }
                        else if (child.CompareTag("White"))
                        {
                            board[y,x] = 2; // White stone
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"Invalid tile name or coordinates: {tileName}");
                }
            }

            Debug.Log("Board state refreshed from scene.");
        }
        else
        {
            Debug.LogError("CubeGrid not found in the scene.");
            board = null;
        }

        Repaint(); // Refresh the UI
    }

    private (int, int) ParseTileName (string name)
    {
        string[] parts = name.Trim('(',')').Split(',');
        if (parts.Length == 2 &&
            int.TryParse(parts[0],out int y) &&
            int.TryParse(parts[1],out int x))
        {
            return (x - 1, y - 1); // Convert to zero-based index
        }

        Debug.LogWarning($"Failed to parse tile name: {name}");
        return (-1, -1);
    }

    private void InitializeBoard ()
    {
        board = new int[boardSize,boardSize];
        Debug.Log("Switch Board initialized with size: " + boardSize);
    }

    private void OnGUI ()
    {
        GUILayout.Label("Go Switch Board Editor",EditorStyles.boldLabel);

        // Add Refresh button to reload CubeGrid
        if (GUILayout.Button("Refresh"))
        {
            RefreshBoardState();
        }

        if (cubeGrid != null)
        {
            int newBoardSize = EditorGUILayout.IntSlider("Board Size",boardSize,5,19);
            if (newBoardSize != boardSize)
            {
                boardSize = newBoardSize;
                InitializeBoard();
                cubeGrid.UpdateBoardState(board); // Sync with CubeGrid
            }

            GUILayout.Space(10);

            GUILayout.Label("Current Player: " + (currentPlayer == 1 ? "Black" : "White"));

            GUILayout.Space(10);

            if (board != null)
            {
                for (int y = boardSize - 1; y >= 0; y--)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label((y + 1).ToString(),GUILayout.Width(30)); // Display row numbers

                    for (int x = 0; x < boardSize; x++)
                    {
                        string label = board[y,x] == 1 ? "B" : (board[y,x] == 2 ? "W" : "O");

                        if (GUILayout.Button(label,GUILayout.Width(30),GUILayout.Height(30)))
                        {
                            if (board[y,x] == 0)
                            {
                                board[y,x] = currentPlayer; // Place a piece
                                cubeGrid.PlaceStoneAt(x,y,currentPlayer);
                            }
                            else
                            {
                                board[y,x] = 0; // Remove the piece
                                cubeGrid.PlaceStoneAt(x,y,0);
                            }

                            RefreshBoardState();
                        }
                    }

                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Switch Player"))
            {
                currentPlayer = currentPlayer == 1 ? 2 : 1;
            }

            if (GUILayout.Button("Clear Board"))
            {
                InitializeBoard(); // Reset the board
                cubeGrid.UpdateBoardState(board); // Sync cleared board to CubeGrid
                RefreshBoardState();
            }
        }
        else
        {
            GUILayout.Label("CubeGrid not found in the scene.");
        }
    }
}
