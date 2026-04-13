using UnityEngine;

public static class SpawnStore
{
    // Name of a spawn GameObject in the next scene (e.g. "SpawnArea")
    public static string NextSpawnName = null;

    // Optional explicit world position to spawn the player at
    public static Vector3? NextPosition = null;

    public static void Clear()
    {
        NextSpawnName = null;
        NextPosition = null;
    }
}