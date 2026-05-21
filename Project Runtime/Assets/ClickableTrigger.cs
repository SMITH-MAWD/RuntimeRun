using UnityEngine;
using System.Collections;

public class ClickableTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelLoader levelLoader;      // The LevelLoader you want to trigger
    [SerializeField] private Animator buttonAnimator;      // Animator on this button object (optional)

    [Header("Settings")]
    [SerializeField] private string clickTriggerName = "Press";  // Animator trigger to play
    [SerializeField] private float animationWaitTime = 0.3f;     // How long to wait before loading (if not using animation length)

    private bool isClicked = false;  // Prevents double clicks

    private void OnMouseDown()
    {
        if (isClicked) return;          // Already clicked – ignore
        if (levelLoader == null)
        {
            Debug.LogWarning("LevelLoader reference missing on " + gameObject.name);
            return;
        }

        isClicked = true;
        StartCoroutine(HandleClick());
    }

    private IEnumerator HandleClick()
    {
        // 1. Play the button press animation (if we have an Animator)
        if (buttonAnimator != null)
        {
            buttonAnimator.SetTrigger(clickTriggerName);

            // Wait for the animation to finish (or use a fixed time)
            // Option A: Wait based on animation length (more accurate)
            AnimatorStateInfo state = buttonAnimator.GetCurrentAnimatorStateInfo(0);
            float animLength = state.length;
            yield return new WaitForSeconds(animLength > 0 ? animLength : animationWaitTime);
        }
        else
        {
            // No animator – just use the fixed wait time
            yield return new WaitForSeconds(animationWaitTime);
        }

        // 2. Now trigger the LevelLoader (which plays its own transition animation)
        levelLoader.LoadNextLevel();
    }
}