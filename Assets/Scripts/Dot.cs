using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UIElements;

// Class representing each dot on the board
public class Dot : MonoBehaviour
{
    [Header("Board Variables")]
    public int column; // Current column index of the dot on the board
    public int row; // Current row index of the dot on the board
    public int previousColumn; // Previous column index for reverting moves
    public int previousRow; // Previous row index for reverting moves

    [Header("Position Variables")]
    public int targetX; // Target X position for smooth movement
    public int targetY; // Target Y position for smooth movement
    public bool isMatched = false; // Flag indicating if the dot is part of a match
    private Board board; // Reference to the Board instance for accessing board properties
    public GameObject otherDot; // Reference to the dot being swapped with
    private UnityEngine.Vector2 firstTouchPosition; // Position of the first touch for swipe detection
    private UnityEngine.Vector2 finalTouchPosition; // Position of the final touch for swipe detection
    private UnityEngine.Vector2 tempPosition; // Temporary position used for smooth movement
    public float swipeAngle = 0; // Angle of the swipe for determining movement direction
    public float swipeResist = 1f; // Resistance threshold for swipe detection
    private FindMatches findMatches; // Reference to the FindMatches script for checking matches
    private HintManager hintManager; // Reference to the HintManager script for generating hints on matches

    [Header("Power Up Variables")]
    public bool isColorBomb; // Flag indicating if this dot is a color bomb power-up
    public bool isColumnBomb; // Flag indicating if this dot is a column bomb power-up
    public bool isRowBomb; // Flag indicating if this dot is a row bomb power-up
    public bool isAdjacentBomb; // Flag indicating if this dot is an adjacent bomb
    public GameObject rowArrow; // Prefab for visual representation of the row bomb
    public GameObject columnArrow; // Prefab for visual representation of the column bomb
    public GameObject colorBomb; // Prefab for visual representation of the color bomb
    public GameObject adjacentMarker; // Prefab for visual representation of the adjacent bomb

    // Start is called before the first frame update
    void Start()
    {
        isColumnBomb = false; // Initialize column bomb status
        isRowBomb = false; // Initialize row bomb status
        isColorBomb = false; // Initialize color bomb status
        isAdjacentBomb = false; // Initialize adjacent bomb status
        board = FindObjectOfType<Board>(); // Find and reference the Board in the scene
        findMatches = FindObjectOfType<FindMatches>(); // Reference to the FindMatches instance
        hintManager = FindObjectOfType<HintManager>(); // Reference to the HintManager instance
    }

    // Handles right-click to create an adjacent bomb for debugging purposes
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isAdjacentBomb = true; // Set adjacent bomb flag to true
            GameObject marker = Instantiate(adjacentMarker, transform.position, UnityEngine.Quaternion.identity);
            marker.transform.parent = this.transform; // Set the adjacent bomb marker as a child of this dot
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Update the target position based on the current column and row
        targetX = column; // Update target X based on the column
        targetY = row; // Update target Y based on the row

        // Move towards the target X position
        if (Mathf.Abs(targetX - transform.position.x) > .1)
        {
            tempPosition = new UnityEngine.Vector2(targetX, transform.position.y); // Set new position for X
            transform.position = UnityEngine.Vector2.Lerp(transform.position, tempPosition, .1f); // Smooth movement towards the target
            // Update the board reference if this dot is in the new position
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject; // Update board array with the current dot
            }
            findMatches.FindAllMatches(); // Check for matches after moving
        }
        else
        {
            // Directly set position if close enough to target
            tempPosition = new UnityEngine.Vector2(targetX, transform.position.y);
            transform.position = tempPosition; // Set to target position
            board.allDots[column, row] = this.gameObject; // Update board reference with the current dot
        }

        // Move towards the target Y position
        if (Mathf.Abs(targetY - transform.position.y) > .1)
        {
            tempPosition = new UnityEngine.Vector2(transform.position.x, targetY); // Set new position for Y
            transform.position = UnityEngine.Vector2.Lerp(transform.position, tempPosition, .1f); // Smooth movement towards target
            // Update the board reference if this dot is in the new position
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject; // Update board array with the current dot
            }
            findMatches.FindAllMatches(); // Check for matches after moving
        }
        else
        {
            // Directly set position if close enough to target
            tempPosition = new UnityEngine.Vector2(transform.position.x, targetY);
            transform.position = tempPosition; // Set to target position
            board.allDots[column, row] = this.gameObject; // Update board reference with the current dot
        }
    }

    // Coroutine to check the validity of the move after a swipe
    public IEnumerator CheckMoveCo()
    {
        // Check for color bomb matches
        if (isColorBomb)
        {
            findMatches.MatchPiecesOfColor(otherDot.tag); // Match pieces of the same color as the other dot
            isMatched = true; // Mark current dot as matched
        }
        else if (otherDot.GetComponent<Dot>().isColorBomb)
        {
            findMatches.MatchPiecesOfColor(this.gameObject.tag); // Match pieces of the current dot's color
            otherDot.GetComponent<Dot>().isMatched = true; // Mark the other dot as matched
        }

        yield return new WaitForSeconds(.2f); // Wait before checking match validity

        if (otherDot != null)
        {
            if (!isMatched && !otherDot.GetComponent<Dot>().isMatched)
            {
                // If no match is found, revert the move
                otherDot.GetComponent<Dot>().row = row; // Revert the other dot's row to its previous position
                otherDot.GetComponent<Dot>().column = column; // Revert the other dot's column to its previous position
                row = previousRow; // Restore the previous row for the current dot
                column = previousColumn; // Restore the previous column for the current dot
                board.currentDot = null; // Clear the current dot reference
                board.currentState = GameState.move; // Allow player to move again
            }
            else
            {
                // If there is a match, trigger destruction of matched dots
                board.DestroyMatches(); // Start the match destruction process
                yield return new WaitForSeconds(.2f); // Wait for destruction to complete
            }
        }
    }

    // Handle mouse down event for initiating touch input (used for detecting when the player clicks or taps)
    private void OnMouseDown()
    {
        // If a hint manager exists, destroy any active hints to ensure they don't interfere with the current input
        if (hintManager != null)
        {
            hintManager.DestroyHint(); // Destroy any visual hints to prevent the player from being distracted by them
        }

        // Check if the current game state allows the player to make a move (i.e., the game is not in a state like "waiting" or "paused")
        if (board.currentState == GameState.move)
        {
            // Get the initial touch or mouse position in world space (converts screen position to world coordinates)
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Store the position where the player first clicks or taps
        }
    }


    // Handle mouse up event for finalizing touch input
    private void OnMouseUp()
    {
        if (board.currentState == GameState.move)
        {
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Get final touch position
            CalculateAngle(); // Calculate swipe angle and direction based on touch positions
        }
    }

    // Calculate the angle of the swipe to determine movement direction
    void CalculateAngle()
    {
        // Check if swipe distance exceeds the resistance threshold
        if (Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
        {
            // board.currentState = GameState.wait; // Change state to wait while processing the move
            // Calculate the swipe angle in degrees
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            Debug.Log(swipeAngle); // Log the angle for debugging
            MovePieces(); // Move the pieces based on calculated swipe direction
            board.currentDot = this; // Set the current dot reference for tracking
        }
        else
        {
            board.currentState = GameState.move; // Reset state if swipe distance is too short
        }
    }

    // Move the pieces in the specified direction based on the swipe
    void MovePiecesActual(UnityEngine.Vector2 direction)
    {
        // Get the dot in the direction of the swipe based on current column and row
        otherDot = board.allDots[column + (int)direction.x, row + (int)direction.y];

        // Store the current row and column to revert moves if necessary
        previousRow = row;
        previousColumn = column;

        // Check if there is a dot in the target position
        if (otherDot != null)
        {
            // Update the column and row of the other dot to move it
            otherDot.GetComponent<Dot>().column += -1 * (int)direction.x; // Adjust the other dot's column
            otherDot.GetComponent<Dot>().row += -1 * (int)direction.y; // Adjust the other dot's row

            // Update the current dot's position
            column += (int)direction.x; // Move current dot's column
            row += (int)direction.y; // Move current dot's row

            // Start a coroutine to check the validity of the move
            StartCoroutine(CheckMoveCo());
        }
        else
        {
            // If there is no dot to swap with, revert to the 'move' state
            board.currentState = GameState.move;
        }
    }

    // Move the pieces based on the swipe direction
    void MovePieces()
    {
        // Determine the direction of the swipe and move the dots accordingly
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1)
        {
            MovePiecesActual(UnityEngine.Vector2.right); // Move right
        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1)
        {
            MovePiecesActual(UnityEngine.Vector2.up); // Move up
        }
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            MovePiecesActual(UnityEngine.Vector2.left); // Move left
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {
            MovePiecesActual(UnityEngine.Vector2.down); // Move down
        }
        else
        {
            board.currentState = GameState.move; // Reset state if swipe is invalid
        }
    }

    // Find matches with adjacent dots
    void FindMatches()
    {
        // Check for matches with dots to the left and right
        if (column > 0 && column < board.width - 1)
        {
            GameObject leftDot1 = board.allDots[column - 1, row]; // Dot to the left
            GameObject rightDot1 = board.allDots[column + 1, row]; // Dot to the right
            if (leftDot1 != null && rightDot1 != null)
            {
                // Check if both adjacent dots match the current dot's tag
                if (leftDot1.tag == this.gameObject.tag && rightDot1.tag == this.gameObject.tag)
                {
                    leftDot1.GetComponent<Dot>().isMatched = true; // Mark left dot as matched
                    rightDot1.GetComponent<Dot>().isMatched = true; // Mark right dot as matched
                    isMatched = true; // Mark current dot as matched
                }
            }
        }

        // Check for matches with dots above and below
        if (row > 0 && row < board.height - 1)
        {
            GameObject upDot1 = board.allDots[column, row + 1]; // Dot above
            GameObject downDot1 = board.allDots[column, row - 1]; // Dot below
            if (upDot1 != null && downDot1 != null)
            {
                // Check if both adjacent dots match the current dot's tag
                if (upDot1.tag == this.gameObject.tag && downDot1.tag == this.gameObject.tag)
                {
                    upDot1.GetComponent<Dot>().isMatched = true; // Mark upper dot as matched
                    downDot1.GetComponent<Dot>().isMatched = true; // Mark lower dot as matched
                    isMatched = true; // Mark current dot as matched
                }
            }
        }
    }

    // Method to create a row bomb power-up
    public void MakeRowBomb()
    {
        isRowBomb = true; // Set row bomb flag
        GameObject arrow = Instantiate(rowArrow, transform.position, UnityEngine.Quaternion.identity);
        arrow.transform.parent = this.transform; // Set arrow as a child of this dot
    }

    // Method to create a column bomb power-up
    public void MakeColumnBomb()
    {
        isColumnBomb = true; // Set column bomb flag
        GameObject arrow = Instantiate(columnArrow, transform.position, UnityEngine.Quaternion.identity);
        arrow.transform.parent = this.transform; // Set arrow as a child of this dot
    }

    // Method to create a color bomb power-up (a special item that can clear all dots of a specific color)
    public void MakeColorBomb()
    {
        // Set the flag indicating this dot is now a color bomb
        isColorBomb = true;

        // Instantiate the color bomb prefab at the current position of the dot
        GameObject color = Instantiate(colorBomb, transform.position, UnityEngine.Quaternion.identity);

        // Set the color bomb as a child of this dot for organization in the hierarchy
        color.transform.parent = this.transform;

        // Tag the current dot as "Color" to distinguish it from regular dots
        this.gameObject.tag = "Color";
    }

    // Method to create an adjacent bomb power-up
    public void MakeAdjacentBomb()
    {
        isAdjacentBomb = true; // Set adjacent bomb flag to true
        GameObject marker = Instantiate(adjacentMarker, transform.position, UnityEngine.Quaternion.identity);
        marker.transform.parent = this.transform; // Set the adjacent bomb marker as a child of this dot
    }
}