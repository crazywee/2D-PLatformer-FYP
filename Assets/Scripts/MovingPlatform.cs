using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform posA, posB; // Define two positions that the platform moves between
    public float speed; // Define the movement speed of the platform
    Vector3 targetPos; // Store the target position for movement

    private Movement2D movementController; // Reference to the Movement2D script of the player
    private Rigidbody2D rb; // Reference to the Rigidbody2D component of the platform
    private Vector3 moveDirection; // Store the movement direction of the platform

    private void Awake()
    {
        // Find and store a reference to the player's Movement2D script
        movementController = GameObject.FindGameObjectWithTag("Player").GetComponent<Movement2D>();

        // Get a reference to the Rigidbody2D component of the platform
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        targetPos = posB.position; // Set the initial target position to posB
        DirectionCalculate(); // Calculate the initial movement direction
    }

    private void Update()
    {
        // Check if the platform has reached posA and update the target position accordingly
        if (Vector2.Distance(transform.position, posA.position) < 0.05f)
        {
            targetPos = posB.position;
            DirectionCalculate();
        }

        // Check if the platform has reached posB and update the target position accordingly
        if (Vector2.Distance(transform.position, posB.position) < 0.05f)
        {
            targetPos = posA.position;
            DirectionCalculate();
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = moveDirection * speed; // Move the platform based on the calculated direction and speed
    }

    void DirectionCalculate()
    {
        moveDirection = (targetPos - transform.position).normalized; // Calculate the normalized movement direction
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Calculate the position change required to match the platform's velocity
            Vector2 positionChange = rb.velocity * Time.fixedDeltaTime;

            // Move the player by the positionChange to match the platform's movement
            movementController.transform.position += new Vector3(positionChange.x, positionChange.y, 0f);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // When the player exits the trigger, update the player's platform-related variables
            movementController.isOnPlatform = false;
            movementController.platformRb = null;
        }
    }
}
