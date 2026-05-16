using UnityEngine;

[DisallowMultipleComponent]
public class SlashFX : MonoBehaviour
{
    [Tooltip("Leave empty to use this GameObject's name. Must match the animation event String.")]
    [SerializeField] private string eventName;

    public string EventName => string.IsNullOrEmpty(eventName) ? gameObject.name : eventName;
}
