using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    private Board board;
    public Text scoreText; // Reference to the UI Text component that displays the score
    public int score; // The player's current score
    public Image scoreBar;

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
    }

    // Update is called once per frame
    void Update()
    {
        // Update the scoreText UI element to display the current score value
        scoreText.text = score.ToString(); // Convert the score to a string and display it in the UI text element
    }

    // Method to increase the score by a specified amount
    public void IncreaseScore(int amountToIncrease)
    {
        score += amountToIncrease; // Increase the score by the specified amount
        if (board != null && scoreBar != null) {
            int length = board.scoreGoals.Length;
            scoreBar.fillAmount = (float) score / (float) board.scoreGoals[length - 1];
        } 
    }
}