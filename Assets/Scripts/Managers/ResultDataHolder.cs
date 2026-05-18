public static class ResultDataHolder
{
    public static ResultData Data { get; private set; }

    public static void Set(ResultData data)
    {
        Data = data;
    }
}