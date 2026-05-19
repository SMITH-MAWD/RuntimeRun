using System.Text.RegularExpressions;
using UnityEngine;

[DisallowMultipleComponent]
public class SlashFX : MonoBehaviour
{
    private static readonly Regex HitKeyPattern = new(
        @"^(HitVar\d+|Hit\d+)",
        RegexOptions.IgnoreCase);

    [Tooltip("Leave empty to use Hit1 / HitVar1 from this object's name.")]
    [SerializeField] private string eventName;

    public string EventName
    {
        get
        {
            string raw = string.IsNullOrEmpty(eventName) ? gameObject.name : eventName;
            Match match = HitKeyPattern.Match(raw);
            return match.Success ? match.Groups[1].Value : raw;
        }
    }
}
