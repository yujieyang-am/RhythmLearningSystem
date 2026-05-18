using System.Collections.Generic;
using UnityEngine;

public class RhythmJudge
{
    private const double PerfectWindow = 0.08;
    private const double HitWindow = 0.15;

    private List<JudgeTarget> targets;
    private int currentTargetIndex;
    private double inputStartDspTime;

    public void StartMeasure(List<JudgeTarget> judgeTargets, double startDspTime)
    {
        targets = judgeTargets;
        currentTargetIndex = 0;
        inputStartDspTime = startDspTime;
    }

    public HitResult JudgeTap(double tapDspTime)
    {
        if (targets == null || currentTargetIndex >= targets.Count)
        {
            return null;
        }

        JudgeTarget target = targets[currentTargetIndex];

        double expectedDspTime = inputStartDspTime + target.TargetTimeSec;
        double delta = tapDspTime - expectedDspTime;

        HitResultType resultType;

        if (System.Math.Abs(delta) <= PerfectWindow)
        {
            resultType = HitResultType.Perfect;
            target.MarkJudged();
            currentTargetIndex++;
        }
        else if (System.Math.Abs(delta) <= HitWindow)
        {
            resultType = delta < 0 ? HitResultType.Early : HitResultType.Late;
            target.MarkJudged();
            currentTargetIndex++;
        }
        else
        {
            if (delta < 0)
            {
                return null;
            }
            else
            {
                resultType = HitResultType.Miss;
                target.MarkJudged();
                currentTargetIndex++;
            }
        }

        return new HitResult(target, resultType, delta);
    }

    public List<HitResult> CheckMissedTargets(double currentDspTime)
    {
        List<HitResult> results = new List<HitResult>();

        if (targets == null)
            return results;

        while (currentTargetIndex < targets.Count)
        {
            JudgeTarget target = targets[currentTargetIndex];

            double expectedDspTime = inputStartDspTime + target.TargetTimeSec;
            double delta = currentDspTime - expectedDspTime;

            if (delta > HitWindow)
            {
                target.MarkJudged();
                results.Add(new HitResult(target, HitResultType.Miss, delta));
                currentTargetIndex++;
            }
            else
            {
                break;
            }
        }

        return results;
    }

    public bool IsFinished()
    {
        return targets == null || currentTargetIndex >= targets.Count;
    }
}