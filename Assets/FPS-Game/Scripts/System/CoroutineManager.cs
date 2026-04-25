using UnityEngine;

/// <summary>
/// Singleton MonoBehaviour that allows static/non-MonoBehaviour classes to run coroutines.
/// </summary>
public class CoroutineManager : MonoBehaviour
{
    public static CoroutineManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
