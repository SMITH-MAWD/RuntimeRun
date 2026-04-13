using UnityEngine;
using UnityEngine.SceneManagement;

public class Transition : MonoBehaviour
{
    [SerializeField] private string sceneName = "Path to Finale";
    [Tooltip("Optional spawn id the target scene will look for (match a GameObject name or your SceneInitializer's spawn names).")]
    [SerializeField] private string spawnId = "SpawnArea";
    [Tooltip("If true, uses player's world position as NextPosition instead of spawnId.")]
    [SerializeField] private bool usePlayerPosition = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (usePlayerPosition)
            SpawnStore.NextPosition = other.transform.position;
        else
            SpawnStore.NextSpawnName = spawnId;

        SceneManager.LoadScene(sceneName);
    }
}