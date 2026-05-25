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
    [SerializeField] private float chartRangeMs = 200f; // 顯示範圍 ±200ms

    [Header("按鈕")]
    [SerializeField] private Button btnBack;
    [SerializeField] private Button btnRetry;
    [SerializeField] private Button btnNext;
    [SerializeField] private Button btnHome; 

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
    }

    private void FillTexts(ResultData data)
    {
        textAccuracy.text  = $"Accuracy: {data.AccuracyPercent:F1}%";
        textPerfect.text   = $"Perfect: {data.PerfectCount}";
        textEarly.text     = $"Early:   {data.EarlyCount}";
        textLate.text      = $"Late:    {data.LateCount}";
        textMiss.text      = $"Miss:    {data.MissCount}";

        // 偏移:正數偏晚，負數偏早
        string deltaDir = data.AverageDeltaMs > 0 ? "偏晚" : "偏早";
        textAverageDelta.text = $"偏移:{Mathf.Abs(data.AverageDeltaMs):F0}ms {deltaDir}";

        textStability.text = $"穩定度:{data.StabilityMs:F0}ms";

        textSuggestion.text = BuildSuggestion(data);
    }

    private string BuildSuggestion(ResultData data)
    {
        if (data.MissCount == 0 && data.StabilityMs < 30f)
            return "建議:非常穩定！可以嘗試更快的速度。";

        if (data.AverageDeltaMs > 40f)
            return "建議:節奏偏晚，試著提早一點敲擊。";

        if (data.AverageDeltaMs < -40f)
            return "建議:節奏偏早，稍微等一下再敲。";

        if (data.StabilityMs > 80f)
            return "建議:節奏不太穩定，放慢速度多練習。";

        if (data.MissCount > 2)
            return "建議:有幾個音符漏掉了，注意聽示範音。";

        return "建議:表現不錯，繼續加油！";
    }

    private void DrawDeltaChart(ResultData data)
    {
        Texture2D tex = new Texture2D(chartWidth, chartHeight);

        // 背景填淡色
        Color bgColor = new Color(0.95f, 0.95f, 0.90f);
        for (int x = 0; x < chartWidth; x++)
            for (int y = 0; y < chartHeight; y++)
                tex.SetPixel(x, y, bgColor);

        // 中線（0ms 基準線）
        int centerX = chartWidth / 2;
        for (int y = 0; y < chartHeight; y++)
            tex.SetPixel(centerX, y, new Color(0.6f, 0.6f, 0.6f));

        // ±HitWindow 參考線
        float hitWindowMs = 150f;
        int earlyX = Mathf.RoundToInt(centerX + (hitWindowMs / chartRangeMs) * (chartWidth / 2f));
        int lateX  = Mathf.RoundToInt(centerX - (hitWindowMs / chartRangeMs) * (chartWidth / 2f));
        for (int y = 0; y < chartHeight; y++)
        {
            tex.SetPixel(earlyX, y, new Color(0.8f, 0.8f, 0.8f));
            tex.SetPixel(lateX,  y, new Color(0.8f, 0.8f, 0.8f));
        }

        // 每個 delta 畫一顆點
        int dotRadius = 4;
        for (int i = 0; i < data.DeltaList.Count; i++)
        {
            float delta = data.DeltaList[i];
            float t = Mathf.Clamp(delta / chartRangeMs, -1f, 1f);
            int px = Mathf.RoundToInt(centerX + t * (chartWidth / 2f));
            int py = chartHeight / 2;

            // 顏色:perfect=綠, early=藍, late=黃
            Color dotColor = Color.green;
            if (delta > 80f)       dotColor = new Color(1f, 0.85f, 0.2f);  // late
            else if (delta < -80f) dotColor = new Color(0.2f, 0.55f, 1f);  // early

            for (int dx = -dotRadius; dx <= dotRadius; dx++)
                for (int dy = -dotRadius; dy <= dotRadius; dy++)
                    if (dx * dx + dy * dy <= dotRadius * dotRadius)
                        tex.SetPixel(px + dx, py + dy, dotColor);
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
}