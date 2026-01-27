using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionZone : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Name of the scene to load.")]
    public string targetSceneName;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object is the player
        // We can check by tag "Player" or if it has a PlayerController component
        // Also check parent as the collider might be on a child object
        if (other.CompareTag("Player") || 
            other.GetComponent<PlayerController>() != null || 
            other.GetComponentInParent<PlayerController>() != null)
        {
            LoadScene();
        }
    }

    private void LoadScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            Debug.Log($"Loading Scene by Name: {targetSceneName}");
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogWarning("SceneTransitionZone: No Target Scene Name specified!");
        }
    }
}
