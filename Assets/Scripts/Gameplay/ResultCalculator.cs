using System.Collections.Generic;
using UnityEngine;

public class ResultData
{
    public int TotalCount;
    public int PerfectCount;
    public int EarlyCount;
    public int LateCount;
    public int MissCount;

    public float AccuracyPercent;
    public float AverageDeltaMs;   // 平均偏移（毫秒）
    public float StabilityMs;      // 穩定度（標準差，毫秒）

    public List<float> DeltaList = new List<float>(); // 每次敲擊的 delta（毫秒），Miss 不算
}

public static class ResultCalculator
{
    public static ResultData Calculate(List<HitResult> results)
    {
        var data = new ResultData();
        data.TotalCount = results.Count;

        var hitDeltas = new List<float>();

        foreach (var r in results)
        {
            switch (r.ResultType)
            {
                case HitResultType.Perfect: data.PerfectCount++; break;
                case HitResultType.Early:   data.EarlyCount++;   break;
                case HitResultType.Late:    data.LateCount++;    break;
                case HitResultType.Miss:    data.MissCount++;    break;
            }

            if (r.ResultType != HitResultType.Miss)
            {
                float deltaMs = (float)(r.DeltaTime * 1000f);
                hitDeltas.Add(deltaMs);
                data.DeltaList.Add(deltaMs);
            }
        }

        // Accuracy
        int hitTotal = data.PerfectCount + data.EarlyCount + data.LateCount;
        data.AccuracyPercent = data.TotalCount > 0
            ? (float)hitTotal / data.TotalCount * 100f
            : 0f;

        // 平均偏移
        if (hitDeltas.Count > 0)
        {
            float sum = 0f;
            foreach (var d in hitDeltas) sum += d;
            data.AverageDeltaMs = sum / hitDeltas.Count;
        }

        // 穩定度（標準差）
        if (hitDeltas.Count > 1)
        {
            float mean = data.AverageDeltaMs;
            float variance = 0f;
            foreach (var d in hitDeltas)
                variance += (d - mean) * (d - mean);
            data.StabilityMs = Mathf.Sqrt(variance / hitDeltas.Count);
        }

        return data;
    }
}