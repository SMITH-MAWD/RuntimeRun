using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OtherLevelLoader : MonoBehaviour
{

    public Animator animator;

    public float transitionTime = 1f;

    public void LoadNextLevel()
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }

    IEnumerator LoadLevel(int levelIndex)
    {
        //play Animation
        animator.SetTrigger("Start");
        //wait for animation to stop 
        yield return new WaitForSeconds(transitionTime);


        //load scene 
        SceneManager.LoadScene(levelIndex);
    }
}
