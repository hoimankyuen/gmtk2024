using System;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance;

    public string Diaogue
    {
        get
        {
            return _dialogue;
        }
        private set
        {
            if (!_dialogue.Equals(value))
            {
                _dialogue = value;
                OnDialogueChangedEvent?.Invoke();
            }
        }
    }
    private string _dialogue = "";
    public event Action OnDialogueChangedEvent;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public static void SayOnce(string dialogue)
    {
        if (Instance != null)
        {
            Instance.Diaogue = dialogue;
        }
    }

    public static void Say(string dialogue)
    {
        SayOnce("");
        SayOnce(dialogue);
    }
}
