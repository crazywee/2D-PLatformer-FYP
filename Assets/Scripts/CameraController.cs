using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Transform target; // Reference to the transform of the target to follow (usually the player)
    Vector3 velocity = Vector3.zero; // Stores the current velocity of the camera movement (initially zero)
    
    [Range(0, 1)] // A slider in the Unity Inspector to control the smoothness of camera movement
    public float smoothTime;
    
    public Vector3 positionOffset; // Offset applied to the target's position for camera following
    public Vector2 xLimit; // Minimum and maximum X-axis positions for the camera
    public Vector2 yLimit; // Minimum and maximum Y-axis positions for the camera

    private void Awake()
    {
        // Find and store a reference to the target's transform using the "Player" tag
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void LateUpdate()
    {
        // Calculate the desired target position for the camera
        Vector3 targetPosition = target.position + positionOffset;

        // Clamp the target position within specified X and Y limits
        targetPosition = new Vector3(
            Mathf.Clamp(targetPosition.x, xLimit.x, xLimit.y),
            Mathf.Clamp(targetPosition.y, yLimit.x, yLimit.y),
            -10); // The "-10" value is used to position the camera in front of other objects

        // Smoothly move the camera towards the target position using SmoothDamp
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
