using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

public class KickPowerSelector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider powerSlider;
    [SerializeField] private Image fillImage;

    [Header("Animation")]
    [SerializeField] private float cycleSpeed = 1.5f; // скорость анимации
    [SerializeField] private AnimationCurve nonlinearCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Timing & Zones")]
    [SerializeField] private float goodZoneMin = 0.4f;
    [SerializeField] private float goodZoneMax = 0.6f;

    [Header("Colors")]
    [SerializeField] private Color lowColor = Color.red;
    [SerializeField] private Color midColor = Color.green;
    [SerializeField] private Color highColor = Color.red;

    public event Action<float> OnPowerSelected;

    private bool isActive = false;
    private float timer = 0f;
    private bool inputReceived = false;

    private void Awake()
    {
        powerSlider.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // подписка на SwipeInputHandler (ты подставишь свой вызов)
        SwipeInputHandler_NewInput.OnSwipeCompleted += ActivateSelector;
    }

    private void OnDisable()
    {
        SwipeInputHandler_NewInput.OnSwipeCompleted -= ActivateSelector;
    }

    private void Update()
    {
        if (!isActive || inputReceived) return;

        timer += Time.deltaTime * cycleSpeed;
        float pingPong = Mathf.PingPong(timer, 1f);

        float curvedValue = nonlinearCurve.Evaluate(pingPong);
        powerSlider.value = curvedValue;

        UpdateSliderColor(curvedValue);

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            inputReceived = true;
            ConfirmPower(curvedValue);
        }
    }

    private void ActivateSelector()
    {
        isActive = true;
        inputReceived = false;
        timer = 0f;
        powerSlider.value = 0f;
        powerSlider.gameObject.SetActive(true);
    }

    private void ConfirmPower(float power)
    {
        isActive = false;
        powerSlider.gameObject.SetActive(false);
        OnPowerSelected?.Invoke(power);
    }

    private void UpdateSliderColor(float value)
    {
        // пример с градиентом от зелёного в центр, к красному по краям
        if (value < goodZoneMin)
        {
            float t = Mathf.InverseLerp(0f, goodZoneMin, value);
            fillImage.color = Color.Lerp(lowColor, midColor, t);
        }
        else if (value > goodZoneMax)
        {
            float t = Mathf.InverseLerp(goodZoneMax, 1f, value);
            fillImage.color = Color.Lerp(midColor, highColor, t);
        }
        else
        {
            fillImage.color = midColor;
        }
    }
}
