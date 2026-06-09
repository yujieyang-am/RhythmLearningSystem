using System.Collections.Generic;
using UnityEngine;

public class ResultData
{
    public int TotalCount;
    public int PerfectCount;
    public int EarlyCount;
    public int LateCount;
    public int MissCount;
    public float Score;
    public List<float> DeltaList = new List<float>();
}

public static class ResultCalculator
{
    public static ResultData Calculate(List<HitResult> results)
    {
        var data = new ResultData();
        data.TotalCount = results.Count;

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
                data.DeltaList.Add((float)(r.DeltaTime * 1000f));
        }

        // Score 計算
        if (data.TotalCount > 0)
        {
            float pointPerNote = 100f / data.TotalCount;
            data.Score = data.PerfectCount * pointPerNote
                       + (data.EarlyCount + data.LateCount) * pointPerNote * 0.5f;
            data.Score = Mathf.Clamp(data.Score, 0f, 100f);
        }

        return data;
    }
}