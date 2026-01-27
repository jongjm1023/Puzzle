using UnityEngine;

public class Booster : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("가속할 힘의 크기입니다.")]
    public float boostForce = 20.0f;
    
    [Header("Visual Settings")]
    public Renderer beltRenderer;
    public string textureName = "_BaseMap";
    public float scrollSpeedMultiplier = 0.03f;

    private void Update()
    {
        if (beltRenderer != null)
        {
            // 속도에 비례
            float offset = Time.time * boostForce * scrollSpeedMultiplier;
            float currentOffsetY = beltRenderer.material.GetTextureOffset(textureName).y;
            beltRenderer.material.SetTextureOffset(textureName, new Vector2(offset, currentOffsetY));
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        Rigidbody rb = collision.collider.attachedRigidbody;
        
        if (rb != null && !rb.isKinematic)
        {
            Vector3 pushDirection = transform.right;

            rb.AddForce(pushDirection * boostForce * 10f, ForceMode.Force);
        }
    }
}