using UnityEngine;
using UnityEngine.UI;

public class UIOverlay : SingletonMonobehaviour<UIOverlay>
{
    [SerializeField] private Image overlay;

    private OverlayController _overlayController;
    private Color _targetColor = Color.black;
    private float _duration = 0.5f;

    // ======== Unity Messages ========

    private void Start()
    {
        _overlayController = OverlayController.Instance;
        _overlayController.OverlayColorChanged += OnOverlayChanged;
        _overlayController.FadeDurationChanged += OnFadeDurationChanged;
        OnOverlayChanged();
        OnFadeDurationChanged();
    }

    private void Update()
    {
        FadeInner();
    }

    private void OnDestroy()
    {
        if (_overlayController != null)
        {
            _overlayController.OverlayColorChanged -= OnOverlayChanged;
            _overlayController.FadeDurationChanged -= OnFadeDurationChanged;
        }
    }

    // ======== Controller Events ========

    private void OnOverlayChanged()
    {
        _targetColor = _overlayController.OverlayColor;
    }

    private void OnFadeDurationChanged()
    {
        _duration = _overlayController.FadeDuration;
    }

    // ======== Functionality ========

    private void FadeInner()
    {
        if (overlay.color != _targetColor)
        {
            if (_duration > 0)
            {
                overlay.color = Vector4.MoveTowards(overlay.color, _targetColor, 1 / _duration * Time.deltaTime);
            }
            else
            {
                overlay.color = _targetColor;
            }
        }
    }
}
