using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class CameraScaler : MonoBehaviour
{
    private Board board; // Reference to the Board instance to access board dimensions
    public float cameraOffset; // The distance of the camera from the board (along the Z-axis)
    public float padding = 2; // Additional padding around the board to ensure it fits well in the view
    public float aspectRatio = 0.625f; // Aspect ratio for the camera's orthographic size (used for scaling)
    public float yOffset = 1; // Vertical offset to adjust the camera's height for better framing

    // Start is called before the first frame update
    void Start()
    {
        // Find the Board object in the scene. This is where we get the board's dimensions (width and height)
        board = FindObjectOfType<Board>();

        // If a board object is found, adjust the camera based on its dimensions
        if (board != null)
        {
            // Reposition the camera based on the board's width and height
            // The -1 is because board dimensions are zero-indexed
            RepositionCamera(board.width - 1, board.height - 1);
        }
    }

    // Method to reposition the camera based on the board dimensions
    void RepositionCamera(float x, float y)
    {
        // Calculate the new camera position
        // The camera will be placed at the center of the board with an additional yOffset for vertical adjustment
        // The x/2 and y/2 ensure that the camera is positioned at the center of the board
        UnityEngine.Vector3 tempPosition = new UnityEngine.Vector3(x / 2, y / 2 + yOffset, cameraOffset);
        transform.position = tempPosition; // Set the camera's position in the scene

        // Adjust the camera's orthographic size based on the aspect ratio and the larger of the board's dimensions
        if (board.width >= board.height)
        {
            // If the board is wider than it is tall, adjust the camera's orthographic size based on width
            Camera.main.orthographicSize = (board.width / 2 + padding) / aspectRatio;
        }
        else
        {
            // If the board is taller than it is wide, adjust the camera's orthographic size based on height
            Camera.main.orthographicSize = board.height / 2 + padding;
        }
    }
}