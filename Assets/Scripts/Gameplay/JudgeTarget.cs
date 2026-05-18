public class JudgeTarget
{
    public RenderGroupModel RenderGroup { get; private set; }
    public double TargetTimeSec { get; private set; }
    public bool IsJudged { get; private set; }

    public JudgeTarget(RenderGroupModel renderGroup, double targetTimeSec)
    {
        RenderGroup = renderGroup;
        TargetTimeSec = targetTimeSec;
        IsJudged = false;
    }

    public void MarkJudged()
    {
        IsJudged = true;
    }
}