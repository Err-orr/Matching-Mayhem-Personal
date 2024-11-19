using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

// Enum to represent the current state of the game
public enum GameState {
    wait, // The game is waiting for a player action
    move  // The game is ready for player movement
}

// Enum to define different types of tiles
public enum TileKind {
    Breakable, // A tile that can be destroyed
    Blank,     // An empty space on the board
    Normal     // A standard tile
}

// Class to represent the type of tile with its position and kind
[System.Serializable]
public class TileType {
    public int x; // X coordinate of the tile
    public int y; // Y coordinate of the tile
    public TileKind tileKind; // Type of the tile
}

public class Board : MonoBehaviour
{
    [Header("Board Settings")]
    public GameState currentState = GameState.move; // Current state of the game
    public int width; // Width of the board in terms of columns
    public int height; // Height of the board in terms of rows
    public int offSet; // Offset for positioning tiles on the board
    public GameObject tilePrefab; // Prefab for creating the background tiles
    public GameObject breakableTilePrefab; // Prefab for creating breakable tiles
    private bool[,] blankSpaces; // 2D array to track blank spaces on the board
    private BackgroundTile[,] breakableTiles; // 2D array to hold breakable tile references

    [Header("Dot Settings")]
    public GameObject destroyEffect; // Effect to display when a dot is destroyed
    public Dot currentDot; // Reference to the currently selected dot
    public GameObject[] dots; // Array of available dot prefabs
    public GameObject[,] allDots; // 2D array to hold all the dots currently on the board
    public TileType[] boardLayout; // Array to define the layout of the board
    private FindMatches findMatches; // Reference to the FindMatches script for detecting matches


    // Start is called before the first frame update
    void Start()
    {
        // Initialize arrays and references
        breakableTiles = new BackgroundTile[width, height];
        findMatches = FindObjectOfType<FindMatches>(); // Retrieve the FindMatches component from the scene
        blankSpaces = new bool[width, height]; // Initialize the 2D array for blank spaces
        allDots = new GameObject[width, height]; // Initialize the 2D array for dots
        SetUp(); // Set up the board with tiles and randomly placed dots
    }

    // Generate blank spaces on the board based on the board layout
    public void GenerateBlankSpaces() {
        for (int i = 0; i < boardLayout.Length; i++) {
            if (boardLayout[i].tileKind == TileKind.Blank) {
                blankSpaces[boardLayout[i].x, boardLayout[i].y] = true; // Mark space as blank
            }
        }
    }

    // Generate breakable tiles on the board based on the board layout
    public void GenerateBreakableTiles() {
        for (int i = 0; i < boardLayout.Length; i++) {
            if (boardLayout[i].tileKind == TileKind.Breakable) {
                // Set the position for the breakable tile
                UnityEngine.Vector2 tempPosition = new UnityEngine.Vector2(boardLayout[i].x, boardLayout[i].y);
                // Instantiate the breakable tile prefab at the designated position
                GameObject tile = Instantiate(breakableTilePrefab, tempPosition, UnityEngine.Quaternion.identity);
                // Store a reference to the BackgroundTile component
                breakableTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }

    // Set up the board by instantiating tiles and randomly placing dots
    private void SetUp() {
        GenerateBlankSpaces(); // Create blank spaces
        GenerateBreakableTiles(); // Create breakable tiles
        // Loop through each column and row to place tiles and dots
        for (int i = 0; i < width; i++) { // Loop through each column
            for (int j = 0; j < height; j++) { // Loop through each row
                if (!blankSpaces[i, j]) { // Check if the current space is not blank
                    // Calculate the position for the tile based on its index
                    UnityEngine.Vector2 tempPosition = new(i, j + offSet); 
                    // Instantiate a new background tile
                    GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, UnityEngine.Quaternion.identity);
                    backgroundTile.transform.parent = this.transform; // Set the parent of the tile to the board for hierarchy organization
                    backgroundTile.name = "( " + i + ", " + j + " )"; // Name the tile for debugging purposes
                    
                    int dotToUse = Random.Range(0, dots.Length); // Randomly select a dot prefab from the array
                    int maxIterations = 0; // Counter to prevent infinite loops when checking for adjacent matches

                    // Ensure the randomly selected dot does not match existing adjacent dots
                    while (MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100) {
                        dotToUse = Random.Range(0, dots.Length); // Select a new dot if it matches
                        maxIterations++;
                    }
                    maxIterations = 0; // Reset counter for potential future use

                    // Instantiate the selected dot and set its position on the board
                    GameObject dot = Instantiate(dots[dotToUse], tempPosition, UnityEngine.Quaternion.identity);
                    dot.GetComponent<Dot>().row = j; // Set the row for the dot
                    dot.GetComponent<Dot>().column = i; // Set the column for the dot
                    dot.transform.parent = this.transform; // Set the parent of the dot to the board for hierarchy organization
                    dot.name = "( " + i + ", " + j + " )"; // Name the dot for debugging purposes
                    allDots[i, j] = dot; // Store the newly created dot in the array
                }
            }
        }
    }

    // Check if the current position has matching dots
    private bool MatchesAt(int column, int row, GameObject piece) {
        // Check for horizontal and vertical matches
        if (column > 1 && row > 1) {
            // Check for a horizontal match with the two dots to the left
            if (allDots[column - 1, row] != null && allDots[column - 2, row] != null) {
                if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag) {
                    return true; // Found a horizontal match
                }
            }
            // Check for a vertical match with the two dots above
            if (allDots[column, row - 1] != null && allDots[column, row - 2] != null) {
                if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag) {
                    return true; // Found a vertical match
                }
            }
        } else if (column <= 1 || row <= 1) {
            // Handle edge cases where the dot is near the edge of the board
            if (row > 1) {
                // Check for a vertical match with the two dots above
                if (allDots[column, row - 1] != null && allDots[column, row - 2] != null) {
                    if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag) {
                        return true; // Found a vertical match
                    }
                }
            }
            if (column > 1) {
                // Check for a horizontal match with the two dots to the left
                if (allDots[column - 1, row] != null && allDots[column - 2, row] != null) {
                    if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag) {
                        return true; // Found a horizontal match
                    }
                }
            }
        }
        return false; // No match found at the specified position
    }

    // Determine if the current matches are in a single row or column
    private bool ColumnOrRow() {
        int numberHorizontal = 0; // Counter for horizontal matches
        int numberVertical = 0; // Counter for vertical matches
        Dot firstPiece = findMatches.currentMatches[0].GetComponent<Dot>(); // Get the first matched dot

        if (firstPiece != null) {
            // Iterate through all matched pieces to count their orientation
            foreach (GameObject currentPiece in findMatches.currentMatches) {
                Dot dot = currentPiece.GetComponent<Dot>();
                // Increment horizontal count if dots are in the same row
                if (dot.row == firstPiece.row) {
                    numberHorizontal++;
                }
                // Increment vertical count if dots are in the same column
                if (dot.column == firstPiece.column) {
                    numberVertical++;
                }
            }
        }
        // Return true if there are five pieces in a row or column
        return numberVertical == 5 || numberHorizontal == 5;
    }

    // Check if any bombs need to be created based on current matches
    private void CheckToMakeBombs() {
        // Handle bomb creation for specific match counts
        if (findMatches.currentMatches.Count == 4 || findMatches.currentMatches.Count == 7) {
            findMatches.CheckBombs(); // Check for any bomb effects based on matches
        }
        
        // Check for potential color bomb creation based on the number of matches
        if (findMatches.currentMatches.Count == 5 || findMatches.currentMatches.Count == 8) {
            if (ColumnOrRow()) { // If matches are in a line
                if (currentDot != null) {
                    // Create a color bomb if the current dot is matched and not already a color bomb
                    if (currentDot.isMatched) {
                        if (!currentDot.isColorBomb) {
                            currentDot.isMatched = false; // Reset matched status
                            currentDot.MakeColorBomb(); // Create a color bomb
                        }
                    } else { // Check the other dot in case it is matched
                        if (currentDot.otherDot != null) {
                            Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                            if (otherDot.isMatched) {
                                if (!otherDot.isColorBomb) {
                                    otherDot.isMatched = false; // Reset matched status
                                    otherDot.MakeColorBomb(); // Create a color bomb
                                }
                            }
                        }
                    }
                }
            } else { // If matches are not in a line, check for adjacent bomb creation
                if (currentDot != null) {
                    // Create an adjacent bomb if the current dot is matched and not already an adjacent bomb
                    if (currentDot.isMatched) {
                        if (!currentDot.isAdjacentBomb) {
                            currentDot.isMatched = false; // Reset matched status
                            currentDot.MakeAdjacentBomb(); // Create an adjacent bomb
                        }
                    } else { // Check the other dot for adjacent bomb creation
                        if (currentDot.otherDot != null) {
                            Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                            if (otherDot.isMatched) {
                                if (!otherDot.isAdjacentBomb) {
                                    otherDot.isMatched = false; // Reset matched status
                                    otherDot.MakeAdjacentBomb(); // Create an adjacent bomb
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    // Destroy matched dots at the specified column and row
    private void DestroyMatchesAt(int column, int row) {
        // Check if the dot at the given position is marked as matched
        if (allDots[column, row].GetComponent<Dot>().isMatched) {
            // Check for special match conditions (e.g., if there are 4 or more matches)
            if (findMatches.currentMatches.Count >= 4) {
                CheckToMakeBombs(); // Handle bomb creation if applicable
            }
            
            // If there is a breakable tile at the current position, apply damage
            if (breakableTiles[column, row] != null) {
                breakableTiles[column, row].TakeDamage(1); // Inflict damage on the breakable tile
                // If the tile has no hit points left, remove its reference
                if (breakableTiles[column, row].hitPoints <= 0) {
                    breakableTiles[column, row] = null; // Clear the reference if destroyed
                }
            }
            
            // Instantiate a destruction effect at the matched dot's position
            GameObject particle = Instantiate(destroyEffect, allDots[column, row].transform.position, UnityEngine.Quaternion.identity);
            Destroy(particle, .5f); // Destroy the effect after 0.5 seconds
            Destroy(allDots[column, row]); // Remove the matched dot from the board
            allDots[column, row] = null; // Set the array position to null for cleanup
        }
    }

    // Loop through the entire board and destroy all matched dots
    public void DestroyMatches() {
        for (int i = 0; i < width; i++) { // Iterate through each column
            for (int j = 0; j < height; j++) { // Iterate through each row
                if (allDots[i, j] != null) {
                    DestroyMatchesAt(i, j); // Check and destroy matches at each position
                }
            }
        }
        findMatches.currentMatches.Clear(); // Clear the list of current matches
        StartCoroutine(DecreaseRowCo2()); // Start coroutine to handle row adjustments after destruction
    }

    // Coroutine to handle adjusting rows when dots are destroyed
    private IEnumerator DecreaseRowCo2() {
        // Loop through each column
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                // Check for blank spaces and null dots
                if (!blankSpaces[i, j] && allDots[i, j] == null) {
                    // Move dots down to fill the empty space
                    for (int k = j + 1; k < height; k++) {
                        if (allDots[i, k] != null) {
                            // Update the dot's row to the new position
                            allDots[i, k].GetComponent<Dot>().row = j;
                            allDots[i, k] = null; // Set the original position to null
                            break; // Exit the loop once a dot is moved
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(.4f); // Wait for 0.4 seconds before refilling the board
        StartCoroutine(FillBoardCo()); // Start coroutine to fill the board with new dots
    }

    // Coroutine to handle adjusting rows when dots are destroyed
    private IEnumerator DecreaseRowCo() {
        int nullCount = 0; // Count of null dots in a column
        for (int i = 0; i < width; i++) { // Iterate through each column
            for (int j = 0; j < height; j++) { // Iterate through each row
                if (allDots[i, j] == null) {
                    nullCount++; // Increment count of null positions
                } else if (nullCount > 0) {
                    // Move existing dots down by the number of nulls above them
                    allDots[i, j].GetComponent<Dot>().row -= nullCount; // Update the row position of the dot
                    allDots[i, j] = null; // Set the current position to null
                }
            }
            nullCount = 0; // Reset null count for the next column
        }
        yield return new WaitForSeconds(.4f); // Wait for 0.4 seconds before refilling the board
        StartCoroutine(FillBoardCo()); // Start coroutine to fill the board with new dots
    }

    // Refill the board with new dots in null positions
    private void RefillBoard() {
        for (int i = 0; i < width; i++) { // Iterate through each column
            for (int j = 0; j < height; j++) { // Iterate through each row
                // Check for empty positions that are not blank spaces
                if (allDots[i, j] == null && !blankSpaces[i, j]) {
                    // Set the new position for the dot
                    UnityEngine.Vector2 tempPosition = new UnityEngine.Vector2(i, j + offSet); 
                    int dotToUse = Random.Range(0, dots.Length); // Randomly select a dot prefab
                    // Instantiate a new dot at the calculated position
                    GameObject piece = Instantiate(dots[dotToUse], tempPosition, UnityEngine.Quaternion.identity); 
                    allDots[i, j] = piece; // Store the new dot in the array
                    piece.GetComponentInParent<Dot>().row = j; // Set the dot's row
                    piece.GetComponentInParent<Dot>().column = i; // Set the dot's column
                }
            }
        }
    }

    // Check if there are any matches currently present on the board
    private bool MatchesOnBoard() {
        for (int i = 0; i < width; i++) { // Iterate through each column
            for (int j = 0; j < height; j++) { // Iterate through each row
                if (allDots[i, j] != null) { // Check for a valid dot
                    if (allDots[i, j].GetComponent<Dot>().isMatched) {
                        return true; // A match has been found
                    }
                }
            }
        }
        return false; // No matches found on the board
    }

    // Coroutine to fill the board with new dots after destroying matches
    private IEnumerator FillBoardCo()
    {
        RefillBoard(); // Refill the board with new dots after clearing any matches
        yield return new WaitForSeconds(.2f); // Wait for 0.2 seconds before checking for new matches
                                              // Continuously check for new matches on the board and destroy them if found
        while (MatchesOnBoard())
        {
            yield return new WaitForSeconds(.2f); // Wait before checking for matches again
            DestroyMatches(); // Destroy any new matches found on the board
        }
        findMatches.currentMatches.Clear(); // Clear the list of matches after refilling
        yield return new WaitForSeconds(.2f); // Wait briefly before allowing player moves again
                                              // If the board is deadlocked, shuffle it to ensure more valid moves
        if (IsDeadlocked())
        {
            ShuffleBoard();
            Debug.Log("Deadlocked!!! :D"); // Log that the board was shuffled due to deadlock
        }
        currentState = GameState.move; // Set the game state to allow player to make moves again
    }

    // Switch the positions of two dots on the board
    private void SwitchPieces(int column, int row, UnityEngine.Vector2 direction)
    {
        // Temporarily hold the dot at the new position
        GameObject holder = allDots[column + (int)direction.x, row + (int)direction.y] as GameObject;
        // Move the dot from its original position to the new position
        allDots[column + (int)direction.x, row + (int)direction.y] = allDots[column, row];
        // Place the dot from the new position back to the original position
        allDots[column, row] = holder;
    }

    // Check if there are any valid matches (3 or more dots in a row/column)
    private bool CheckForMatches()
    {
        // Loop through each cell on the board to check for horizontal and vertical matches
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    // Check for horizontal matches (3 or more dots in a row)
                    if (i < width - 2)
                    {
                        if (allDots[i + 1, j] != null && allDots[i + 2, j] != null)
                        {
                            if (allDots[i + 1, j].tag == allDots[i, j].tag && allDots[i + 2, j].tag == allDots[i, j].tag)
                            {
                                return true; // Match found horizontally
                            }
                        }
                    }
                    // Check for vertical matches (3 or more dots in a column)
                    if (j < height - 2)
                    {
                        if (allDots[i, j + 1] != null && allDots[i, j + 2] != null)
                        {
                            if (allDots[i, j + 1].tag == allDots[i, j].tag && allDots[i, j + 2].tag == allDots[i, j].tag)
                            {
                                return true; // Match found vertically
                            }
                        }
                    }
                }
            }
        }
        return false; // No matches found
    }

    // Switch two adjacent dots and check if a match occurs
    private bool SwitchAndCheck(int column, int row, UnityEngine.Vector2 direction)
    {
        SwitchPieces(column, row, direction); // Swap the two dots
                                              // Check if the swap resulted in a match
        if (CheckForMatches())
        {
            SwitchPieces(column, row, direction); // If a match is found, revert the swap
            return true; // Return true if the swap resulted in a match
        }
        SwitchPieces(column, row, direction); // If no match, revert the swap
        return false; // Return false if no match was found
    }

    // Check if the board is deadlocked (no possible matches or valid moves)
    private bool IsDeadlocked()
    {
        // Loop through every dot on the board and check if a valid move is possible
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    // Check for a valid move to the right
                    if (i < width - 1)
                    {
                        if (SwitchAndCheck(i, j, UnityEngine.Vector2.right))
                        {
                            return false; // A valid move was found, so it's not deadlocked
                        }
                    }
                    // Check for a valid move upwards
                    if (j < height - 1)
                    {
                        if (SwitchAndCheck(i, j, UnityEngine.Vector2.up))
                        {
                            return false; // A valid move was found, so it's not deadlocked
                        }
                    }
                }
            }
        }
        return true; // No valid moves were found, the board is deadlocked
    }

    // Shuffle the board when it's deadlocked to ensure valid moves can be made
    private void ShuffleBoard()
    {
        List<GameObject> newBoard = new List<GameObject>();
        // Add all non-null dots to the newBoard list
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    newBoard.Add(allDots[i, j]);
                }
            }
        }

        // Shuffle the board by placing new random dots in the available spaces
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!blankSpaces[i, j]) // Only place dots in non-blank spaces
                {
                    int pieceToUse = Random.Range(0, newBoard.Count); // Randomly select a new dot
                    int maxIterations = 0; // Counter to prevent infinite loops when checking for adjacent matches
                                           // Ensure the randomly selected dot does not match any adjacent dots
                    while (MatchesAt(i, j, newBoard[pieceToUse]) && maxIterations < 100)
                    {
                        pieceToUse = Random.Range(0, newBoard.Count); // Select a new dot if it matches an adjacent one
                        maxIterations++; // Increment iteration count to avoid infinite loop
                    }
                    maxIterations = 0; // Reset counter for potential future use
                    Dot piece = newBoard[pieceToUse].GetComponent<Dot>();
                    piece.column = i;
                    piece.row = j;
                    allDots[i, j] = newBoard[pieceToUse]; // Place the new dot on the board
                    newBoard.Remove(newBoard[pieceToUse]); // Remove the selected dot from the newBoard list
                }
            }
        }
        // If the board is still deadlocked after shuffling, try shuffling again
        if (IsDeadlocked())
        {
            ShuffleBoard();
        }
    }
}