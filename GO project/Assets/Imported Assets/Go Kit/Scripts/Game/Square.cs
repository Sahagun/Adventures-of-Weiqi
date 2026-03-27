using UnityEngine;

/// <summary>
/// Represents a square on the Go board
/// </summary>
public class Square : MonoBehaviour
{
    private LayerMask gridSquareLayerMask;

    void Start ()
    {
        // Set the LayerMask to only include the GridSquare layer
        gridSquareLayerMask = LayerMask.GetMask("Gridsquare");
    }

    /// <summary>
    /// Handles click logic
    /// </summary>
    void OnMouseDown ()
    {
        // Use a raycast to ensure only the grid square layer is clicked
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray,out hit,Mathf.Infinity,gridSquareLayerMask))
        {
            // Check if it's the player's turn and handle the player's move
            if (GameManager.Instance.PlayerTurn)
            {
                int playerColor = GameManager.Instance.CurrentColour;
                // Attempt to place the player's stone on the clicked square
                bool validMove = GoBoard.Instance.PlayerMove(
                    (int)transform.position.x,
                    (int)transform.position.z,
                    playerColor
                );

                // If the move is valid, the AI's move will be handled automatically
                if (validMove)
                {
                    // Switch turn to AI (or handle turn change logic)
                    GameManager.Instance.PlayerTurn = false;

                    // Turn logic for the AI is handled in the GoBoard script after a valid move
                }
            }
        }
    }
}
