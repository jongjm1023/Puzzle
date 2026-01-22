using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class MapFlipper : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The root object containing the entire level geometry.")]
    public Transform mapRoot;
    
    [Tooltip("The player controller to manage gravity.")]
    public PlayerController player;

    [Header("Settings")]
    [Tooltip("Time it takes to complete the flip (seconds).")]
    public float duration = 1.0f;

    private bool isFlipping = false;
    private float targetRoll = 0f; // Current Z rotation target

    void Update()
    {
        // Check for 'R' key press
        // Using the "New Input System" direct check since we didn't add an action for "Flip" in the asset yet
        if (!isFlipping && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            StartCoroutine(FlipRoutine());
        }
    }

    IEnumerator FlipRoutine()
    {
        isFlipping = true;

        Quaternion initialPlayerRotation = Quaternion.identity;

        // 1. Disable Gravity & Capture Rotation
        if (player != null)
        {
            player.SetGravityEnabled(false);
            initialPlayerRotation = player.transform.rotation;
        }

        // 2. Rotate Map
        // We rotate 180 degrees around the Z axis (Roll)
        Quaternion startRotation = mapRoot.rotation;
        
        // Toggle: If we are at 0, go to 180. If at 180, go to 360 (0).
        Quaternion endRotation = startRotation * Quaternion.Euler(0, 0, 180f);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // Smooth step for smoother start/end
            t = t * t * (3f - 2f * t);

            mapRoot.rotation = Quaternion.Slerp(startRotation, endRotation, t);

            // Fix Player Rotation (Keep them upright/facing same way)
            if (player != null)
            {
                player.transform.rotation = initialPlayerRotation;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure we end exactly at the target rotation
        mapRoot.rotation = endRotation;

        // 3. Enable Gravity
        if (player != null)
        {
            player.transform.rotation = initialPlayerRotation;
            player.SetGravityEnabled(true);
        }

        isFlipping = false;
    }
}
