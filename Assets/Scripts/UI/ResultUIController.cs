using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ResultUIController : MonoBehaviour
{
    [Header("病歷板文字")]
    [SerializeField] private TMP_Text textAverageDelta;
    [SerializeField] private TMP_Text textStability;
    [SerializeField] private TMP_Text textSuggestion;

    [Header("數據文字")]
    [SerializeField] private TMP_Text textAccuracy;
    [SerializeField] private TMP_Text textPerfect;
    [SerializeField] private TMP_Text textEarly;
    [SerializeField] private TMP_Text textLate;
    [SerializeField] private TMP_Text textMiss;

    [Header("偏移點圖")]
    [SerializeField] private RawImage deltaChart;
    [SerializeField] private int chartWidth = 400;
    [SerializeField] private int chartHeight = 150;
    [SerializeField] private float chartRangeMs = 200f;

    [Header("按鈕")]
    [SerializeField] private Button btnBack;
    [SerializeField] private Button btnRetry;
    [SerializeField] private Button btnNext;
    [SerializeField] private Button btnHome;

    [Header("豬醫生")]
    [SerializeField] private GameObject pigIdle;
    [SerializeField] private GameObject pigGood;
    [SerializeField] private GameObject pigBad;

    private void Start()
    {
        ResultData data = ResultDataHolder.Data;

        if (data == null)
        {
            Debug.LogWarning("ResultData is null.");
            return;
        }

        FillTexts(data);
        DrawDeltaChart(data);
        SetupButtons();
        ShowPigResult(data);
    }

    private void FillTexts(ResultData data)
    {
        textAccuracy.text = $"Accuracy: {data.AccuracyPercent:F1}%";
        textPerfect.text  = $"Perfect: {data.PerfectCount}";
        textEarly.text    = $"Early: {data.EarlyCount}";
        textLate.text     = $"Late: {data.LateCount}";
        textMiss.text     = $"Miss: {data.MissCount}";

        string deltaDir = data.AverageDeltaMs > 0 ? "偏晚" : "偏早";
        textAverageDelta.text = $"偏移: {Mathf.Abs(data.AverageDeltaMs):F0}ms {deltaDir}";
        textStability.text    = $"穩定度: {data.StabilityMs:F0}ms";
        textSuggestion.text   = BuildSuggestion(data);
    }

    private string BuildSuggestion(ResultData data)
    {
        string offsetMsg = "";
        if (Mathf.Abs(data.AverageDeltaMs) > 40f)
            offsetMsg = data.AverageDeltaMs > 0
                ? $"節奏平均偏晚 {data.AverageDeltaMs:F0}ms, 試着提早敲擊."
                : $"節奏平均偏早 {Mathf.Abs(data.AverageDeltaMs):F0}ms, 稍微等一下再敲.";

        string stabilityMsg = "";
        if (data.StabilityMs < 30f)
            stabilityMsg = "節奏非常穩定!";
        else if (data.StabilityMs < 60f)
            stabilityMsg = "節奏穩定度尚可.";
        else
            stabilityMsg = $"節奏起伏較大(標準差 {data.StabilityMs:F0}ms), 建議放慢速度練習.";

        string missMsg = "";
        if (data.MissCount > 0)
            missMsg = $"有 {data.MissCount} 個音符未敲到, 注意聽示範音再演奏.";

        string result = stabilityMsg;
        if (offsetMsg != "") result += "\n" + offsetMsg;
        if (missMsg != "")   result += "\n" + missMsg;

        return result;
    }

    private void DrawDeltaChart(ResultData data)
    {
        Texture2D tex = new Texture2D(chartWidth, chartHeight);

        // 背景：三個區域不同深淺
        // 左側 Early 區（淡藍）、中間 Perfect 區（淡綠）、右側 Late 區（淡黃）
        float hitWindowMs = 80f;
        int perfectLeft  = Mathf.RoundToInt(chartWidth / 2f - (hitWindowMs / chartRangeMs) * (chartWidth / 2f));
        int perfectRight = Mathf.RoundToInt(chartWidth / 2f + (hitWindowMs / chartRangeMs) * (chartWidth / 2f));

        for (int x = 0; x < chartWidth; x++)
        {
            Color bg;
            if (x < perfectLeft)
                bg = new Color(0.85f, 0.90f, 1.00f);   // Early 區：淡藍
            else if (x > perfectRight)
                bg = new Color(1.00f, 0.95f, 0.80f);   // Late 區：淡黃
            else
                bg = new Color(0.88f, 1.00f, 0.88f);   // Perfect 區：淡綠

            for (int y = 0; y < chartHeight; y++)
                tex.SetPixel(x, y, bg);
        }

        // 中線
        int centerX = chartWidth / 2;
        for (int y = 0; y < chartHeight; y++)
            tex.SetPixel(centerX, y, new Color(0.4f, 0.4f, 0.4f));

        // 區域分隔線
        for (int y = 0; y < chartHeight; y++)
        {
            tex.SetPixel(perfectLeft,  y, new Color(0.6f, 0.6f, 0.6f));
            tex.SetPixel(perfectRight, y, new Color(0.6f, 0.6f, 0.6f));
        }

        // 點：Y 軸加入隨機抖動避免重疊
        int dotRadius = 5;
        System.Random rng = new System.Random(42);

        for (int i = 0; i < data.DeltaList.Count; i++)
        {
            float delta = data.DeltaList[i];
            float t = Mathf.Clamp(delta / chartRangeMs, -1f, 1f);
            int px = Mathf.RoundToInt(centerX + t * (chartWidth / 2f));

            // Y 軸隨機抖動，讓點不全疊在一起
            int py = chartHeight / 2 + rng.Next(-chartHeight / 4, chartHeight / 4);

            Color dotColor;
            if (delta > hitWindowMs)
                dotColor = new Color(0.9f, 0.6f, 0.1f);   // Late：橘黃
            else if (delta < -hitWindowMs)
                dotColor = new Color(0.2f, 0.5f, 1.0f);   // Early：藍
            else
                dotColor = new Color(0.2f, 0.75f, 0.3f);  // Perfect：綠

            for (int dx = -dotRadius; dx <= dotRadius; dx++)
                for (int dy = -dotRadius; dy <= dotRadius; dy++)
                    if (dx * dx + dy * dy <= dotRadius * dotRadius)
                    {
                        int fx = Mathf.Clamp(px + dx, 0, chartWidth - 1);
                        int fy = Mathf.Clamp(py + dy, 0, chartHeight - 1);
                        tex.SetPixel(fx, fy, dotColor);
                    }
        }

        tex.Apply();
        deltaChart.texture = tex;
    }

    private void SetupButtons()
    {
        btnBack.onClick.AddListener(() =>
            SceneManager.LoadScene("SC_Game"));

        btnRetry.onClick.AddListener(() =>
            SceneManager.LoadScene("SC_Game"));

        btnNext.onClick.AddListener(() =>
            Debug.Log("Next level - 待實作"));

        btnHome.onClick.AddListener(() =>
            SceneManager.LoadScene("SC_Progress"));
    }

    private void ShowPigResult(ResultData data)
    {
        pigIdle.SetActive(false);
        pigGood.SetActive(false);
        pigBad.SetActive(false);

        if (data.AccuracyPercent >= 90f)
            pigGood.SetActive(true);
        else if (data.AccuracyPercent >= 70f)
            pigIdle.SetActive(true);
        else
            pigBad.SetActive(true);
    }
}