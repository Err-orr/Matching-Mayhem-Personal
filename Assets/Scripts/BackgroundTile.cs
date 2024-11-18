using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BackgroundTile : MonoBehaviour
{
    public int hitPoints; // The health points of the background tile
    private SpriteRenderer sprite; // Reference to the SpriteRenderer component

    // Method to apply damage to the tile
    public void TakeDamage(int damage) {
        hitPoints -= damage; // Reduce hit points by the damage amount
        MakeLighter(); // Change the tile's color to indicate damage
    }
    
    // Start is called before the first frame update
    void Start()
    {
        // Get the SpriteRenderer component attached to this GameObject
        sprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    public void Update()
    {
        // Check if hit points are zero or less
        if (hitPoints <= 0) {
            Destroy(this.gameObject); // Destroy the tile if it has no hit points left
        }
    }

    // Method to lighten the tile's color
    void MakeLighter() {
        Color color = sprite.color; // Get the current color of the sprite
        float newAlpha = color.a * 0.5f; // Reduce the alpha value to make it lighter
        // Create a new color with the same RGB values but a lighter alpha
        sprite.color = new Color(color.r, color.g, color.b, newAlpha);
    }
}