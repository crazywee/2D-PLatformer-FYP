using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalController : MonoBehaviour
{
    public Transform destination; // The destination transform where the player will teleport
    GameObject player; // Reference to the player GameObject
    AudioManager audioManager; // Reference to the AudioManager script

    private void Awake()
    {
        // Find and store a reference to the player GameObject using the "Player" tag
        player = GameObject.FindGameObjectWithTag("Player");

        // Find and store a reference to the AudioManager script
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // Check if the collider has the "Player" tag
        {
            // Play a teleport-in sound effect from the AudioManager
            audioManager.PlaySFX(audioManager.TpIn);

            // Check if the player is not very close to the portal to avoid immediate teleportation
            if (Vector2.Distance(player.transform.position, transform.position) > 0.3f)
            {
                // Teleport the player to the destination position
                player.transform.position = destination.transform.position;

                // Play a teleport-out sound effect from the AudioManager
                audioManager.PlaySFX(audioManager.TpOut);
            }
        }
    }
}
