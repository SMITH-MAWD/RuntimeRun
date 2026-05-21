using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Transition : MonoBehaviour
{
    public Animator animator;
    public float transitionTime = 1f;

    [Header("Scene")]
    public string sceneName;   // set in Inspector, make sure it's in Build Settings

    private bool triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || triggered) return;
        triggered = true;
        StartCoroutine(LoadSceneWithFade());
    }

    IEnumerator LoadSceneWithFade()
    {
        animator.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(sceneName);
    }
}