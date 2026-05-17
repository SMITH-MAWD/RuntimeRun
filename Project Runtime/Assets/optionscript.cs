using UnityEngine;
using UnityEngine.UI;

public class optionscript : MonoBehaviour
{
    public Button yesButton;
    public Button noButton;
    public GameObject OptionBox;
    public OtherLevelLoader otherLevelLoader;

    private PlayerMovement cachedPlayer;

    void Start()
    {
        if (yesButton != null)
        {
            yesButton.onClick.AddListener(OnYesPressed);
        }
        else
        {
            Debug.LogWarning("optionscript: yesButton is not assigned.");
        }

        if (noButton != null)
        {
            noButton.onClick.AddListener(OnNoPressed);
        }
        else
        {
            Debug.LogWarning("optionscript: noButton is not assigned.");
        }

        if (OptionBox == null)
        {
            OptionBox = gameObject;
        }

#if UNITY_2023_2_OR_NEWER
        cachedPlayer = Object.FindFirstObjectByType<PlayerMovement>();
#else
        cachedPlayer = FindObjectOfType<PlayerMovement>();
#endif
    }

    private void OnYesPressed()
    {
        if (otherLevelLoader != null)
        {
            otherLevelLoader.LoadNextLevel();
        }
        else
        {
            Debug.LogWarning("optionscript: OtherLevelLoader is not assigned. Assign it in the inspector or add it to the same scene.");
        }
    }

    private void OnNoPressed()
    {
        if (OptionBox != null)
        {
            OptionBox.SetActive(false);
        }

        if (cachedPlayer != null)
        {
            cachedPlayer.inputEnabled = true;
        }
    }
}
