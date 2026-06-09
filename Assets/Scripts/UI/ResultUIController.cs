using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ResultUIController : MonoBehaviour
{
    [Header("病歷板文字")]
    [SerializeField] private TMP_Text textSuggestion;

    [Header("數據文字")]
    [SerializeField] private TMP_Text textScore;
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
        textScore.text   = $"Score: {data.Score:F1}";
        textPerfect.text = $"Perfect: {data.PerfectCount}";
        textEarly.text   = $"Early: {data.EarlyCount}";
        textLate.text    = $"Late: {data.LateCount}";
        textMiss.text    = $"Miss: {data.MissCount}";
        textSuggestion.text = BuildSuggestion(data);
    }

    private string BuildSuggestion(ResultData data)
    {
        // 分析傾向
        bool hasLateTendency  = data.LateCount > data.EarlyCount
                                && data.LateCount > data.TotalCount * 0.3f;
        bool hasEarlyTendency = data.EarlyCount > data.LateCount
                                && data.EarlyCount > data.TotalCount * 0.3f;
        bool hasUnstableRhythm = data.EarlyCount > data.TotalCount * 0.2f
                                && data.LateCount > data.TotalCount * 0.2f;
        bool hasManyMisses    = data.MissCount > data.TotalCount * 0.3f;
        bool isExcellent      = data.Score >= 90f;

        if (isExcellent)
            return "節奏掌握相當精準，整體表現優異！可以嘗試提高 BPM 挑戰更快的速度，或選擇包含更複雜節奏型的練習曲目，進一步強化你的節拍控制能力。";

        if (hasManyMisses)
            return "本次有較多音符未能順利敲擊。建議先降低 BPM，讓自己有充裕的時間聆聽並跟上示範節拍。熟悉節奏型後再逐步提速，切勿急於求成。";

        if (hasUnstableRhythm)
            return "你的敲擊時機忽早忽晚，顯示內在節拍感尚不穩定。建議搭配節拍器從慢速開始練習，專注於讓每一拍落點一致，培養穩定的律動感後再提升速度。";

        if (hasLateTendency)
            return "你有明顯的拖拍傾向，敲擊時機持續偏晚。這通常是因為被動等待節拍到來而非主動預判所致。建議練習時在腦中提前預備下一個音符的時機點，讓敲擊動作走在節拍之前一點點。";

        if (hasEarlyTendency)
            return "你有搶拍的傾向，敲擊時機持續偏早。建議放慢 BPM 進行練習，養成完整聆聽每個拍點的習慣，避免過度預判節拍位置。可以嘗試跟著示範音哼唱，讓身體自然跟上節奏。";

        return "整體表現不錯！部分音符的時機仍有調整空間。建議多加練習，留意 Early 與 Late 的分布，找出自己節奏感的弱點並針對性加強。";
    }

    private void DrawDeltaChart(ResultData data)
    {
        Texture2D tex = new Texture2D(chartWidth, chartHeight);

        float hitWindowMs = 80f;
        int perfectLeft  = Mathf.RoundToInt(chartWidth / 2f - (hitWindowMs / chartRangeMs) * (chartWidth / 2f));
        int perfectRight = Mathf.RoundToInt(chartWidth / 2f + (hitWindowMs / chartRangeMs) * (chartWidth / 2f));

        for (int x = 0; x < chartWidth; x++)
        {
            Color bg;
            if (x < perfectLeft)
                bg = new Color(0.85f, 0.90f, 1.00f);
            else if (x > perfectRight)
                bg = new Color(1.00f, 0.95f, 0.80f);
            else
                bg = new Color(0.88f, 1.00f, 0.88f);

            for (int y = 0; y < chartHeight; y++)
                tex.SetPixel(x, y, bg);
        }

        int centerX = chartWidth / 2;
        for (int y = 0; y < chartHeight; y++)
            tex.SetPixel(centerX, y, new Color(0.4f, 0.4f, 0.4f));

        for (int y = 0; y < chartHeight; y++)
        {
            tex.SetPixel(perfectLeft,  y, new Color(0.6f, 0.6f, 0.6f));
            tex.SetPixel(perfectRight, y, new Color(0.6f, 0.6f, 0.6f));
        }

        int dotRadius = 5;
        System.Random rng = new System.Random(42);

        for (int i = 0; i < data.DeltaList.Count; i++)
        {
            float delta = data.DeltaList[i];
            float t = Mathf.Clamp(delta / chartRangeMs, -1f, 1f);
            int px = Mathf.RoundToInt(centerX + t * (chartWidth / 2f));
            int py = chartHeight / 2 + rng.Next(-chartHeight / 4, chartHeight / 4);

            Color dotColor;
            if (delta > hitWindowMs)
                dotColor = new Color(0.9f, 0.6f, 0.1f);
            else if (delta < -hitWindowMs)
                dotColor = new Color(0.2f, 0.5f, 1.0f);
            else
                dotColor = new Color(0.2f, 0.75f, 0.3f);

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
            SceneManager.LoadScene("SC_Home"));
    }

    private void ShowPigResult(ResultData data)
    {
        pigIdle.SetActive(false);
        pigGood.SetActive(false);
        pigBad.SetActive(false);

        if (data.Score >= 90f)
            pigGood.SetActive(true);
        else if (data.Score >= 70f)
            pigIdle.SetActive(true);
        else
            pigBad.SetActive(true);
    }
}