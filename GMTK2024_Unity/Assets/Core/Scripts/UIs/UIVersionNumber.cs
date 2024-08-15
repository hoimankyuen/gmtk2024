using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UIVersionNumber : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TextMeshProUGUI versionNumberText;

    private void Awake()
    {
        SetFPSText(Application.version);
    }

    private void SetFPSText(string versionNumber)
    {
        versionNumberText.text = $"version {versionNumber}";
    }
}
