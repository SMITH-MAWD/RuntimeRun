Assets/SpawnStore.cs
using UnityEngine;

public static class SpawnStore
{
    // If you prefer IDs, set NextSpawnId; otherwise set NextPosition.
    public static string NextSpawnId = null;
    public static Vector3? NextPosition = null;

    public static void Clear()
    {
        NextSpawnId = null;
        NextPosition = null;
    }
}