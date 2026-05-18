using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BpmSliderController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text bpmText;

    [Header("BPM Range")]
    [SerializeField] private int minBpm = 40;
    [SerializeField] private int maxBpm = 200;

    [Header("BPM Snap Points")]
    [SerializeField] private int slowBpm = 60;
    [SerializeField] private int standardBpm = 120;
    [SerializeField] private int fastBpm = 180;

    [Header("Snap Setting")]
    [SerializeField] private int snapRange = 8;

    public int SelectedBpm { get; private set; }

    private int[] snapPoints;

    private void Awake()
    {
        snapPoints = new[] { slowBpm, standardBpm, fastBpm };

        slider.minValue = minBpm;
        slider.maxValue = maxBpm;
        slider.wholeNumbers = true;

        slider.value = standardBpm;
        SelectedBpm = standardBpm;
        UpdateText();
    }

    private void OnEnable()
    {
        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value)
    {
        int currentValue = Mathf.RoundToInt(value);
        int snappedValue = GetSnapValueIfClose(currentValue);

        if (snappedValue != currentValue)
        {
            slider.SetValueWithoutNotify(snappedValue);
            SelectedBpm = snappedValue;
        }
        else
        {
            SelectedBpm = currentValue;
        }

        UpdateText();
    }

    private int GetSnapValueIfClose(int value)
    {
        foreach (int point in snapPoints)
        {
            if (Mathf.Abs(value - point) <= snapRange)
            {
                return point;
            }
        }

        return value;
    }

    private void UpdateText()
    {
        bpmText.text = $"BPM: {SelectedBpm}";
    }
}