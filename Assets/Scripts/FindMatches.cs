using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FindMatches : MonoBehaviour
{
    private Board board; // Reference to the game board
    public List<GameObject> currentMatches = new List<GameObject>(); // List to hold currently matched dots

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>(); // Find and reference the Board object in the scene
    }

    // Public method to initiate the match-finding process
    public void FindAllMatches() {
        StartCoroutine(FindAllMatchesCo()); // Start the coroutine that finds all matches on the board
    }

    // Check for adjacent bombs and add their affected pieces to the matches list
    private List<GameObject> IsAdjacentBomb(Dot dot1, Dot dot2, Dot dot3) {
        List<GameObject> currentDots = new List<GameObject>();
        if (dot1.isAdjacentBomb) {
            // Add all dots adjacent to the first dot if it's an adjacent bomb
            currentMatches = currentMatches.Union(GetAdjacentPieces(dot1.column, dot1.row)).ToList();
        }
        if (dot2.isAdjacentBomb) {
            // Add all dots adjacent to the second dot if it's an adjacent bomb
            currentMatches = currentMatches.Union(GetAdjacentPieces(dot2.column, dot2.row)).ToList();
        }
        if (dot3.isAdjacentBomb) {
            // Add all dots adjacent to the third dot if it's an adjacent bomb
            currentMatches = currentMatches.Union(GetAdjacentPieces(dot3.column, dot3.row)).ToList();
        }
        return currentDots;
    }

    // Check for row bombs and add their affected pieces to the matches list
    private List<GameObject> IsRowBomb(Dot dot1, Dot dot2, Dot dot3) {
        List<GameObject> currentDots = new List<GameObject>();
        if (dot1.isRowBomb) {
            // Add all dots in the row of the first dot if it's a row bomb
            currentMatches = currentMatches.Union(GetRowPieces(dot1.row)).ToList();
        }
        if (dot2.isRowBomb) {
            // Add all dots in the row of the second dot if it's a row bomb
            currentMatches = currentMatches.Union(GetRowPieces(dot2.row)).ToList();
        }
        if (dot3.isRowBomb) {
            // Add all dots in the row of the third dot if it's a row bomb
            currentMatches = currentMatches.Union(GetRowPieces(dot3.row)).ToList();
        }
        return currentDots;
    }

    // Check for column bombs and add their affected pieces to the matches list
    private List<GameObject> IsColumnBomb(Dot dot1, Dot dot2, Dot dot3) {
        List<GameObject> currentDots = new List<GameObject>();
        if (dot1.isColumnBomb) {
            // Add all dots in the column of the first dot if it's a column bomb
            currentMatches = currentMatches.Union(GetColumnPieces(dot1.column)).ToList();
        }
        if (dot2.isColumnBomb) {
            // Add all dots in the column of the second dot if it's a column bomb
            currentMatches = currentMatches.Union(GetColumnPieces(dot2.column)).ToList();
        }
        if (dot3.isColumnBomb) {
            // Add all dots in the column of the third dot if it's a column bomb
            currentMatches = currentMatches.Union(GetColumnPieces(dot3.column)).ToList();
        }
        return currentDots;
    }

    // Add a dot to the current matches list and mark it as matched
    private void AddToListAndMatch(GameObject dot) {
        if (!currentMatches.Contains(dot)) {
            currentMatches.Add(dot); // Add the dot to matches if it's not already included
        }
        dot.GetComponent<Dot>().isMatched = true; // Mark the dot as matched
    }

    // Add nearby dots to the matches list
    private void GetNearbyPieces(GameObject dot1, GameObject dot2, GameObject dot3) {
        AddToListAndMatch(dot1); // Add the first dot to matches
        AddToListAndMatch(dot2); // Add the second dot to matches
        AddToListAndMatch(dot3); // Add the third dot to matches
    }

    // Coroutine to find all matches on the board
    private IEnumerator FindAllMatchesCo() {
        yield return new WaitForSeconds(.1f); // Wait briefly before starting the match check
        for (int i = 0; i < board.width; i++) { // Loop through each column
            for (int j = 0; j < board.height; j++) { // Loop through each row
                GameObject currentDot = board.allDots[i, j]; // Get the current dot at the specified position
                if (currentDot != null) { // Ensure the current dot exists
                    Dot currentDotDot = currentDot.GetComponent<Dot>(); // Get the Dot component of the current dot
                    // Check for horizontal matches
                    if (i > 0 && i < board.width - 1) { // Ensure indices are within bounds
                        GameObject leftDot = board.allDots[i - 1, j]; // Get the dot to the left
                        GameObject rightDot = board.allDots[i + 1, j]; // Get the dot to the right
                        if (leftDot != null && rightDot != null) { // Ensure adjacent dots exist
                            Dot rightDotDot = rightDot.GetComponent<Dot>(); // Get the Dot component of the right dot
                            Dot leftDotDot = leftDot.GetComponent<Dot>(); // Get the Dot component of the left dot
                            // Check if both adjacent dots match the current dot
                            if (leftDot.tag == currentDot.tag && rightDot.tag == currentDot.tag) {
                                // Add matches based on bomb types
                                currentMatches = currentMatches.Union(IsRowBomb(leftDotDot, currentDotDot, rightDotDot)).ToList();
                                currentMatches = currentMatches.Union(IsColumnBomb(leftDotDot, currentDotDot, rightDotDot)).ToList();
                                currentMatches = currentMatches.Union(IsAdjacentBomb(leftDotDot, currentDotDot, rightDotDot)).ToList();
                                GetNearbyPieces(leftDot, currentDot, rightDot); // Add the matched dots to the matches list
                            }
                        }
                    }
                    // Check for vertical matches
                    if (j > 0 && j < board.height - 1) { // Ensure indices are within bounds
                        GameObject upDot = board.allDots[i, j + 1]; // Get the dot above
                        GameObject downDot = board.allDots[i, j - 1]; // Get the dot below
                        if (upDot != null && downDot != null) { // Ensure adjacent dots exist
                            Dot downDotDot = downDot.GetComponent<Dot>(); // Get the Dot component of the down dot
                            Dot upDotDot = upDot.GetComponent<Dot>(); // Get the Dot component of the up dot
                            // Check if both adjacent dots match the current dot
                            if (upDot.tag == currentDot.tag && downDot.tag == currentDot.tag) {
                                // Add matches based on bomb types
                                currentMatches = currentMatches.Union(IsRowBomb(upDotDot, currentDotDot, downDotDot)).ToList();
                                currentMatches = currentMatches.Union(IsColumnBomb(upDotDot, currentDotDot, downDotDot)).ToList();
                                currentMatches = currentMatches.Union(IsAdjacentBomb(upDotDot, currentDotDot, downDotDot)).ToList();
                                GetNearbyPieces(upDot, currentDot, downDot); // Add the matched dots to the matches list
                            }
                        }
                    }
                }
            }
        }
    }

    // Mark all dots of the specified color as matched
    public void MatchPiecesOfColor(string color) {
        for (int i = 0; i < board.width; i++) { // Loop through each column
            for (int j = 0; j < board.height; j++) { // Loop through each row
                if (board.allDots[i, j] != null) { // Ensure the dot exists
                    if (board.allDots[i, j].tag == color) {
                        board.allDots[i, j].GetComponent<Dot>().isMatched = true; // Mark the dot as matched
                    }
                }
            }
        }
    }

    // Get all adjacent dots of the specified column and row and mark them as matched
    List<GameObject> GetAdjacentPieces(int column, int row) {
        List<GameObject> dots = new List<GameObject>();
        for (int i = column - 1; i <= column + 1; i++) {
            for (int j = row - 1; j <= row + 1; j++) {
                // Ensure the indices are within the board's bounds
                if (i >= 0 && i < board.width && j >= 0 && j < board.height) {
                    dots.Add(board.allDots[i, j]); // Add the dot to the list
                    board.allDots[i, j].GetComponent<Dot>().isMatched = true; // Mark the dot as matched
                }
            }
        }
        return dots; // Return the list of adjacent dots
    }

    // Get all dots in the specified column and mark them as matched
    List<GameObject> GetColumnPieces(int column) {
        List<GameObject> dots = new List<GameObject>();
        for (int i = 0; i < board.height; i++) {
            if (board.allDots[column, i] != null) {
                dots.Add(board.allDots[column, i]); // Add the dot to the list
                board.allDots[column, i].GetComponent<Dot>().isMatched = true; // Mark the dot as matched
            }
        }
        return dots; // Return the list of dots
    }

    // Get all dots in the specified row and mark them as matched
    List<GameObject> GetRowPieces(int row) {
        List<GameObject> dots = new List<GameObject>();
        for (int i = 0; i < board.width; i++) {
            if (board.allDots[i, row] != null) {
                dots.Add(board.allDots[i, row]); // Add the dot to the list
                board.allDots[i, row].GetComponent<Dot>().isMatched = true; // Mark the dot as matched
            }
        }
        return dots; // Return the list of dots
    }

    // Check if the current or other dot is a bomb and create a bomb if matched
    public void CheckBombs() {
        if (board.currentDot != null) {
            if (board.currentDot.isMatched) {
                board.currentDot.isMatched = false; // Reset the matched status for the current dot
                // Determine the type of bomb based on swipe direction
                if ((board.currentDot.swipeAngle > -45 && board.currentDot.swipeAngle <= 45) ||
                (board.currentDot.swipeAngle < -135 || board.currentDot.swipeAngle >= 135)) {
                    board.currentDot.MakeRowBomb(); // Create a row bomb based on swipe direction
                } else {
                    board.currentDot.MakeColumnBomb(); // Create a column bomb based on swipe direction
                }
            } else if (board.currentDot.otherDot != null) {
                Dot otherDot = board.currentDot.otherDot.GetComponent<Dot>(); // Get the Dot component of the other dot
                if (otherDot.isMatched) {
                    otherDot.isMatched = false; // Reset the matched status for the other dot
                    // Determine the type of bomb based on swipe direction
                    if ((board.currentDot.swipeAngle > -45 && board.currentDot.swipeAngle <= 45) ||
                    (board.currentDot.swipeAngle < -135 || board.currentDot.swipeAngle >= 135)) {
                        otherDot.MakeRowBomb(); // Create a row bomb based on swipe direction
                    } else {
                        otherDot.MakeColumnBomb(); // Create a column bomb based on swipe direction
                    }
                }
            }
        }
    }
}