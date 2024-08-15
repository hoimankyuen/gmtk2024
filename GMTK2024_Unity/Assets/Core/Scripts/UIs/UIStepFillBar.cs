using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIStepFillBar : MonoBehaviour
{
    [SerializeField] private Button decreaseButton;
    [SerializeField] private Button increaseButton;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI backText;
    [SerializeField] private TextMeshProUGUI frontText;

    public int MinValue
    {
        get
        {
            return minValue;
        }
        set
        {
            if (minValue != value)
            {
                minValue = value;
                RedrawElements();
                Value = Mathf.Clamp(Value, MinValue, MaxValue);
            }
        }
    }
    [SerializeField] private int minValue;

    public int MaxValue
    {
        get
        {
            return maxValue;
        }
        set
        {
            if (maxValue != value)
            {
                maxValue = value;
                RedrawElements();
                Value = Mathf.Clamp(Value, MinValue, MaxValue);
            }
        }
    }
    [SerializeField] private int maxValue;

    public int Step
    {
        get
        {
            return step;
        }
        set
        {
            if (step != value)
            {
                step = value;
            }
        }
    }
    [SerializeField] private int step;

    public int Value
    {
        get
        {
            return value;
        }
        set
        {
            value = Mathf.Clamp(value, MinValue, MaxValue);
            if (this.value != value)
            {
                this.value = value;
                RedrawElements();
                OnValueChange.Invoke(value);
            }
        }
    }
    [SerializeField] private int value;

    public UnityEvent<int> OnValueChange;

    // ======== Unity Messages ========

    private void Awake()
    {
        decreaseButton.onClick.AddListener(DecreaseValue);
        increaseButton.onClick.AddListener(IncreaseValue);
    }

    private void OnValidate()
    {
        RedrawElements();
    }

    // ======== Functionalities ========

    public void DecreaseValue()
    {
        Value -= Step;
    }

    public void IncreaseValue()
    {
        Value += Step;
    }

    public void SetValueWithoutNotify(int value)
    {
        value = Mathf.Clamp(value, MinValue, MaxValue);
        if (this.value != value)
        {
            this.value = value;
            RedrawElements();
        }
    }

    // ======== Graphics ========

    private void RedrawElements()
    {
        decreaseButton.interactable = Value != MinValue;
        increaseButton.interactable = Value != MaxValue;
        fillImage.fillAmount = Mathf.InverseLerp(MinValue, MaxValue, Value);
        backText.text = Value.ToString();
        frontText.text = Value.ToString();
    }
}
