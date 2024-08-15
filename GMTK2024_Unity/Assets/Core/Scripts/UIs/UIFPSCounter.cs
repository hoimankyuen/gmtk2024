using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UIFPSCounter : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TextMeshProUGUI fpsCounterText;

    [Header("Settings")]
    [SerializeField] private float refreshDuration = 1;

    // Working variables
    private float lastUpdateRealTime = 0;
    private readonly List<float> unscaledDeltaTimes = new List<float>();

    private void Awake()
    {
        SetFPSText(60);
    }

    private void Update()
    {
        unscaledDeltaTimes.Add(1f / Time.unscaledDeltaTime);
        if (Time.realtimeSinceStartup - lastUpdateRealTime > refreshDuration)
        {
            SetFPSText(unscaledDeltaTimes.Aggregate(0f, (total, next) => total + next) / unscaledDeltaTimes.Count);
            unscaledDeltaTimes.Clear();
            lastUpdateRealTime += refreshDuration;
        }
    }

    private void SetFPSText(float averageFPS)
    {
        fpsCounterText.text = $"{averageFPS:F2} FPS";
    }
}
