using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameController : MonoBehaviour
{
    Vector2 checkpointPosition; // Stores the position of the checkpoint
    SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    Rigidbody2D _rb; // Reference to the Rigidbody2D component
    AudioManager audioManager; // Reference to the AudioManager script

    private void Awake()
    {
        // Get a reference to the SpriteRenderer component attached to this GameObject
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Get a reference to the Rigidbody2D component attached to this GameObject
        _rb = GetComponent<Rigidbody2D>();

        // Find and store a reference to the AudioManager script using the "Audio" tag
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Initialize the checkpoint position to the current position
        checkpointPosition = transform.position;
    }

    // Detects collisions with other objects
    private void OnCollisionEnter2D(Collision2D collision)
    {
        TilemapCollider2D tilemapCollider = collision.collider.GetComponent<TilemapCollider2D>();
        
        // Check if the collision involves a TilemapCollider with the "Trap" tag
        if (tilemapCollider != null && tilemapCollider.CompareTag("Trap"))
        {
            // Call the PlayerDie function to handle player death
            PlayerDie();
        }

        EdgeCollider2D edgeCollider = collision.collider.GetComponent<EdgeCollider2D>();
        
        // Check if the collision involves an EdgeCollider2D with the "Trap" tag
        if (edgeCollider != null && edgeCollider.CompareTag("Trap"))
        {
            // Call the PlayerDie function to handle player death
            PlayerDie();
        }
    }

    // Updates the checkpoint position when the player interacts with a checkpoint
    public void UpdateCheckpoint(Vector2 position)
    {
        checkpointPosition = position;
    }

    // Handles player death
    void PlayerDie()
    {
        // Start a coroutine to handle player respawn with a delay
        StartCoroutine(Respawn(0.5f));

        // Play a death sound effect from the AudioManager
        audioManager.PlaySFX(audioManager.death);
    }

    // Coroutine for player respawn
    IEnumerator Respawn(float duration)
    {
        // Hide the player sprite during respawn
        spriteRenderer.enabled = false;

        // Reset the player's velocity
        _rb.velocity = new Vector2(0, 0);

        // Wait for the specified duration
        yield return new WaitForSeconds(duration);

        // Move the player to the checkpoint position
        transform.position = checkpointPosition;

        // Show the player sprite after respawn
        spriteRenderer.enabled = true;
    }
}
