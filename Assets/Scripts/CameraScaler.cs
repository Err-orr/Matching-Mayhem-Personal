using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class CameraScaler : MonoBehaviour
{
    private Board board; // Reference to the Board instance
    public float cameraOffset; // Distance of the camera from the board
    public float padding = 2; // Additional padding around the board
    public float aspectRatio = 0.625f; // Aspect ratio for camera scaling

    // Start is called before the first frame update
    void Start()
    {
        // Find the Board object in the scene
        board = FindObjectOfType<Board>();
        if (board != null) {
            // Reposition the camera based on the board's width and height
            RepositionCamera(board.width - 1, board.height - 1);
        }
    }

    // Method to reposition the camera based on board dimensions
    void RepositionCamera(float x, float y) {
        // Calculate the new camera position based on the board's dimensions
        UnityEngine.Vector3 tempPosition = new UnityEngine.Vector3(x / 2, y / 2, cameraOffset);
        transform.position = tempPosition; // Set the camera's position

        // Adjust the camera's orthographic size based on the board's aspect ratio
        if (board.width >= board.height) {
            Camera.main.orthographicSize = (board.width / 2 + padding) / aspectRatio;
        } else {
            Camera.main.orthographicSize = board.height / 2 + padding;
        }
    }
}