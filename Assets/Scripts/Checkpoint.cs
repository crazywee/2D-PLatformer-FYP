using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    GameController gameController; // Reference to the GameController script

    SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    public Sprite passive, active; // Sprites for passive (unchecked) and active (checked) checkpoints
    Collider2D checked_point; // Reference to the Collider2D component

    AudioManager audioManager; // Reference to the AudioManager script

    private void Awake()
    {
        // Find and store a reference to the GameController script attached to the player
        gameController = GameObject.FindGameObjectWithTag("Player").GetComponent<GameController>();

        // Find and store a reference to the AudioManager script
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();

        // Get a reference to the SpriteRenderer component attached to this object
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Get a reference to the Collider2D component attached to this object
        checked_point = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Play a checkpoint sound effect when the player touches the checkpoint
            audioManager.PlaySFX(audioManager.checkpoint);

            // Call the UpdateCheckpoint method in the GameController to set the checkpoint position
            gameController.UpdateCheckpoint(transform.position);

            // Change the sprite to the active checkpoint sprite
            spriteRenderer.sprite = active;

            // Disable the Collider2D component to prevent re-triggering the checkpoint
            checked_point.enabled = false;
        }
    }
}
