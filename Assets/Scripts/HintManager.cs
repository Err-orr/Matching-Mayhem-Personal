using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    private Board board; // Reference to the Board instance to access the game grid and logic
    public float hintDelay; // Delay between showing hints (in seconds)
    private float hintDelaySeconds; // Timer for managing the delay between hints
    public GameObject hintParticle; // The particle or visual effect to show when a hint is displayed
    public GameObject currentHint; // The current hint object being shown (if any)

    // Start is called before the first frame update
    void Start()
    {
        // Find the Board object in the scene to interact with the game grid
        board = FindObjectOfType<Board>();

        // Initialize the hint delay timer with the configured delay value
        hintDelaySeconds = hintDelay;
    }

    // Update is called once per frame
    void Update()
    {
        // Countdown the hint delay timer
        hintDelaySeconds -= Time.deltaTime;

        // If the hint delay has elapsed and no hint is currently displayed, show a new hint
        if (hintDelaySeconds <= 0 && currentHint == null)
        {
            MarkHint(); // Call to display a hint
            hintDelaySeconds = hintDelay; // Reset the delay timer for the next hint
        }
    }

    // Method to find all possible valid moves on the board
    List<GameObject> FindAllMatches()
    {
        List<GameObject> possibleMoves = new List<GameObject>(); // List to store valid move candidates

        // Loop through every dot on the board and check if a valid move is possible
        for (int i = 0; i < board.width; i++) // Iterate over columns
        {
            for (int j = 0; j < board.height; j++) // Iterate over rows
            {
                if (board.allDots[i, j] != null) // Check if the dot at position (i, j) exists
                {
                    // Check if a valid move is possible by switching the current dot with one to the right
                    if (i < board.width - 1)
                    {
                        if (board.SwitchAndCheck(i, j, UnityEngine.Vector2.right)) // Try a move to the right
                        {
                            possibleMoves.Add(board.allDots[i, j]); // Add to possible moves if valid
                        }
                    }

                    // Check if a valid move is possible by switching the current dot with one above
                    if (j < board.height - 1)
                    {
                        if (board.SwitchAndCheck(i, j, UnityEngine.Vector2.up)) // Try a move upwards
                        {
                            possibleMoves.Add(board.allDots[i, j]); // Add to possible moves if valid
                        }
                    }
                }
            }
        }
        return possibleMoves; // Return the list of valid moves
    }

    // Method to pick a random valid move from the possible moves list
    GameObject PickOneRandomly()
    {
        // Get a list of all valid moves
        List<GameObject> possibleMoves = FindAllMatches();

        // If there are any valid moves, pick one at random
        if (possibleMoves.Count > 0)
        {
            int pieceToUse = Random.Range(0, possibleMoves.Count); // Select a random index
            return possibleMoves[pieceToUse]; // Return the randomly selected valid dot
        }

        // If no valid moves are found, return null
        return null;
    }

    // Method to display a hint by instantiating a visual effect (hint particle) at a valid move location
    private void MarkHint()
    {
        // Get a randomly selected valid move
        GameObject move = PickOneRandomly();

        // If a valid move was found, display a hint
        if (move != null)
        {
            currentHint = Instantiate(hintParticle, move.transform.position, UnityEngine.Quaternion.identity); // Instantiate the hint particle at the move's position
        }
    }

    // Method to destroy the currently displayed hint (if any)
    public void DestroyHint()
    {
        // If a hint is currently being shown, destroy it
        if (currentHint != null)
        {
            Destroy(currentHint); // Destroy the hint particle object
            currentHint = null; // Reset the currentHint reference
            hintDelaySeconds = hintDelay; // Reset the hint delay timer
        }
    }
}