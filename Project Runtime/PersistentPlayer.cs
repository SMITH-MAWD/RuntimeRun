Assets/PersistentPlayer.cs
using UnityEngine;

public class PersistentPlayer : MonoBehaviour
{
    private static PersistentPlayer s_instance;

    void Awake()
    {
        if (s_instance != null && s_instance != this)
        {
            Destroy(gameObject); // avoid duplicates if player prefab exists in multiple scenes
            return;
        }

        s_instance = this;
        DontDestroyOnLoad(gameObject);
    }
}