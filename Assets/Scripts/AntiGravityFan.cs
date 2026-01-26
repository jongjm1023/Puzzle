using UnityEngine;
using System.Collections.Generic;

public class AntiGravityFan : MonoBehaviour
{
    [Tooltip("Damping factor to prevent sliding (air resistance).")]
    public float damping = 2.0f;

    // Track Rigidbodies and the number of their colliders inside the trigger
    private Dictionary<Rigidbody, int> containedRigidbodies = new Dictionary<Rigidbody, int>();

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            if (containedRigidbodies.ContainsKey(rb))
            {
                containedRigidbodies[rb]++;
            }
            else
            {
                containedRigidbodies.Add(rb, 1);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && containedRigidbodies.ContainsKey(rb))
        {
            containedRigidbodies[rb]--;
            if (containedRigidbodies[rb] <= 0)
            {
                containedRigidbodies.Remove(rb);
            }
        }
    }

    private void FixedUpdate()
    {
        // Iterate over a copy of the keys to allow modification if needed (though we only read here mostly)
        // or just direct iteration. Direct iteration is safe if we don't modify the dictionary.
        // However, if an object is destroyed, rb might be null.
        
        List<Rigidbody> rbsToRemove = null;

        foreach (var kvp in containedRigidbodies)
        {
            Rigidbody rb = kvp.Key;

            if (rb == null)
            {
                if (rbsToRemove == null) rbsToRemove = new List<Rigidbody>();
                rbsToRemove.Add(rb);
                continue;
            }

            ApplyFanForce(rb);
        }

        // Cleanup destroyed objects
        if (rbsToRemove != null)
        {
            foreach (var rb in rbsToRemove)
            {
                containedRigidbodies.Remove(rb);
            }
        }
    }

    private void ApplyFanForce(Rigidbody rb)
    {
        // Calculate force magnitude: |gravity| * mass
        // This ensures F = mg, so a = g
        float forceMagnitude = Physics.gravity.magnitude * rb.mass;
        
        // Apply force in the fan's up direction
        Vector3 upForce = transform.up * forceMagnitude;
        
        // Apply damping force to stop horizontal sliding
        Vector3 dampingForce = -rb.linearVelocity * damping;

        rb.AddForce(upForce + dampingForce, ForceMode.Force);
    }

    private void OnDrawGizmos()
    {
        // Visualize the fan's direction
        Gizmos.color = new Color(0f, 1f, 1f, 0.5f); // Cyan with transparency
        Gizmos.DrawRay(transform.position, transform.up * 2f);
        Gizmos.DrawWireCube(transform.position + transform.up, Vector3.one * 0.5f);
    }
}
