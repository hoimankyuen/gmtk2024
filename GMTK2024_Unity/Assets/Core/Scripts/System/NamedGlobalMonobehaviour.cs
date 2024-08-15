using System.Collections.Generic;
using UnityEngine;

public class NamedGlobalMonobehaviour : MonoBehaviour
{
    private static readonly HashSet<string> Instances = new HashSet<string>();

    // ======== Unity Messages ========
    private void Awake()
    {
        if (Instances.Contains(gameObject.name))
        {
            Destroy(gameObject);
            return;
        }

        Instances.Add(gameObject.name);
        DontDestroyOnLoad(gameObject);
    }
}
