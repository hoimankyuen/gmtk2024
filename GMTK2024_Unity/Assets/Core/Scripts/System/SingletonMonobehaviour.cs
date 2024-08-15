using UnityEngine;

public abstract class SingletonMonobehaviour<T> : MonoBehaviour where T : SingletonMonobehaviour<T>
{
    public static T Instance { get; private set; }

    // ======== Unity Messages ========
    protected void Awake()
    {
        if (Instance != null && Instance != (T)this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = (T)this;
    }
}
