using System;
using UnityEngine;

public class OverlayController : SingletonMonobehaviour<OverlayController>
{
    public Color OverlayColor
    {
        get
        {
            return _overlayColor;
        }
        private set
        {
            if (_overlayColor != value)
            {
                _overlayColor = value;
                OverlayColorChanged?.Invoke();
            }
        }
    }
    private Color _overlayColor;
    public event Action OverlayColorChanged;

    public float FadeDuration
    {
        get
        {
            return _fadeDuration;
        }
        private set
        {
            if (_fadeDuration != value)
            {
                _fadeDuration = value;
                FadeDurationChanged?.Invoke();
            }
        }
    }
    private float _fadeDuration;
    public event Action FadeDurationChanged;

    // ======= Functionality ========

    public static void SetColor(Color targetColor)
    {
        Fade(targetColor, 0);
    }

    public static void Fade(Color targetColor, float duration = 0.5f)
    {
        if (Instance == null)
            return;

        Instance.OverlayColor = targetColor;
        Instance.FadeDuration = duration;
    }
}
