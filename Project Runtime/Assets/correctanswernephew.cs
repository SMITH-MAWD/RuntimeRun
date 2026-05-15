using UnityEngine;

public class correctanswernephew : MonoBehaviour
{
    [Header("Effect (Scene Object)")]
    [Tooltip("Assign an existing GameObject in the scene (e.g. particle system). It will be activated when triggered. Leave empty to use a prefab instead.")]
    [SerializeField] private GameObject effectObject;

    [Header("Effect (Prefab)")]
    [Tooltip("If no scene object is assigned, this prefab will be instantiated.")]
    [SerializeField] private GameObject effectPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnTarget;
    [SerializeField] private bool spawnAsChild = true;

    private void Start()
    {
        // Find any component that implements IAnswerCorrectNotifier
        foreach (var comp in GetComponents<MonoBehaviour>())
        {
            if (comp is IAnswerCorrectNotifier notifier)
            {
                notifier.OnCorrectAnswer.AddListener(TriggerEffect);
                Debug.Log($"correctanswernephew: Subscribed to {comp.GetType().Name}", this);
                return;
            }
        }

        Debug.LogWarning("correctanswernephew: No IAnswerCorrectNotifier found on this GameObject!", this);
    }

    private void TriggerEffect()
    {
        // If a scene object is assigned, just activate it
        if (effectObject != null)
        {
            effectObject.SetActive(true);
            return;
        }

        // Otherwise fall back to instantiating the prefab
        if (effectPrefab == null)
        {
            Debug.LogWarning("correctanswernephew: No effect object or prefab assigned.", this);
            return;
        }

        Transform parent = spawnAsChild ? (spawnTarget != null ? spawnTarget : transform) : null;
        Vector3 position = spawnTarget != null ? spawnTarget.position : transform.position;
        Quaternion rotation = spawnTarget != null ? spawnTarget.rotation : transform.rotation;

        Instantiate(effectPrefab, position, rotation, parent);
    }

    private void OnDestroy()
    {
        foreach (var comp in GetComponents<MonoBehaviour>())
        {
            if (comp is IAnswerCorrectNotifier notifier)
            {
                notifier.OnCorrectAnswer.RemoveListener(TriggerEffect);
                break;
            }
        }
    }
}