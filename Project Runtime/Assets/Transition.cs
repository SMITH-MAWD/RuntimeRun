using UnityEngine;
using UnityEngine.SceneManagement;

public class Pathway : MonoBehaviour
{
    [SerializeField] private string sceneName = "Path to Finale"; // set this in Inspector or replace with actual scene name

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}