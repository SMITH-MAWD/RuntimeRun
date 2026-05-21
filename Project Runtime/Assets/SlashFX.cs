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

    [Tooltip("If false, this FX keeps its authored local position/scale and is NOT mirrored when the character flips facing.")]
    [SerializeField] private bool mirrorWithFacing = true;

    public bool MirrorWithFacing => mirrorWithFacing;

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
