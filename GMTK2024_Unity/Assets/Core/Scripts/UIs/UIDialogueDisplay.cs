using System.Collections;
using TMPro;
using UnityEngine;

public class UIDialogueDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameController gameController;

    [Header("Components")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI text;

    [Header("Settings")]
    [SerializeField] private float dialogueOnScreenDuration = 4f;
    [SerializeField] private float dialogueTypingCPS = 40f;
    [SerializeField] private float dialogueFadeoutDuration = 0.5f;

    private Coroutine _dialogueDisplaySequence;
    private bool _isPaused;

    // ======== Unity Messages ========
    
    private void Awake()
    {
        canvasGroup.alpha = 0;
    }

    private void Start()
    {
        gameController.OnLevelPausedEvent += OnLevelPaused;
        gameController.OnLevelResumedEvent += OnLevelResumed;
        gameController.OnLevelEndedEvent += OnLevelEnded;

        DialogueController.Instance.OnDialogueChangedEvent += OnDialogueChanged;
    }

    private void OnDestroy()
    {
        if (gameController != null)
        {
            gameController.OnLevelPausedEvent -= OnLevelPaused;
            gameController.OnLevelResumedEvent -= OnLevelResumed;
            gameController.OnLevelEndedEvent -= OnLevelEnded;
        }

        if (DialogueController.Instance != null)
        {
            DialogueController.Instance.OnDialogueChangedEvent -= OnDialogueChanged;
        }
    }

    // ======== Functionality ========

    private void OnLevelPaused()
    {
        _isPaused = true;
    }

    private void OnLevelResumed()
    {
        _isPaused = false;
    }

    private void OnLevelEnded()
    {
        canvasGroup.alpha = 0f;
    }

    private void OnDialogueChanged()
    {
        if (_dialogueDisplaySequence != null)
        {
            StopCoroutine(_dialogueDisplaySequence);
        }
        _dialogueDisplaySequence = StartCoroutine(DialogueDisplaySequence(DialogueController.Instance.Diaogue));
    }

    private IEnumerator DialogueDisplaySequence(string dialogue)
    {
        canvasGroup.alpha = 1f;

        float currentTime = 0f;
        float duration = (float)dialogue.Length / dialogueTypingCPS;
        while (currentTime < duration)
        {
            if (!_isPaused)
            {
                text.text = dialogue.Substring(0, (int)(dialogue.Length * currentTime / duration));
                currentTime += Time.deltaTime;
            }
            yield return null;
        }
        text.text = dialogue;

        currentTime = 0f;
        duration = dialogueOnScreenDuration;
        while (currentTime < duration)
        {
            if (!_isPaused)
            {
                currentTime += Time.deltaTime;
            }
            yield return null;
        }

        currentTime = 0f;
        duration = dialogueFadeoutDuration;
        while (currentTime < duration)
        {
            if (!_isPaused)
            {
                canvasGroup.alpha = 1f - (currentTime / duration);
                currentTime += Time.deltaTime;
            }
            yield return null;
        }
        canvasGroup.alpha = 0f;

        _dialogueDisplaySequence = null;
    }
}
