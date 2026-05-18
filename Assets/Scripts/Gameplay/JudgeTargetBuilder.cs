using System.Collections.Generic;

public static class JudgeTargetBuilder
{
    public static List<JudgeTarget> BuildFromRenderGroups(
        List<RenderGroupModel> groups,
        int bpm
    )
    {
        List<JudgeTarget> targets = new List<JudgeTarget>();

        float secondsPerBeat = 60f / bpm;

        foreach (RenderGroupModel group in groups)
        {
            if (group.Notes == null || group.Notes.Count == 0)
                continue;

            double targetTimeSec = group.CenterBeat * secondsPerBeat;

            JudgeTarget target = new JudgeTarget(
                group,
                targetTimeSec
            );

            targets.Add(target);
        }

        return targets;
    }
}