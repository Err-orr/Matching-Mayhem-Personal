using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // Array of AudioSource components that hold different destruction sound clips
    public AudioSource[] destroyNoise;

    // Method to play a random destruction sound effect from the destroyNoise array
    public void PlayRandomDestroyNoise()
    {
        // Randomly select an index within the range of available sound clips
        int clipToPlay = Random.Range(0, destroyNoise.Length);

        // Play the selected audio clip from the destroyNoise array
        destroyNoise[clipToPlay].Play();
    }
}