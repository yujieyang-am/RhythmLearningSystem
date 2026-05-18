public class HitResult
{
    public JudgeTarget Target { get; private set; }
    public HitResultType ResultType { get; private set; }
    public double DeltaTime { get; private set; }

    public HitResult(JudgeTarget target, HitResultType resultType, double deltaTime)
    {
        Target = target;
        ResultType = resultType;
        DeltaTime = deltaTime;
    }
}