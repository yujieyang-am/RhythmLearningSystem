using System.Collections.Generic;

public enum RenderGroupType
{
    // 獨立音符
    Single,
    // 連體八分音符
    BeamedEighth2,
    BeamedEighth3,
    BeamedEighth4,
    // 連體十六分音符
    BeamedSixteenth2,
    BeamedSixteenth4,
    // 連體三十二分音符
    BeamedThirtySecond2,
    // 三連音
    Triplet,
    // 休止符
    Rest,
}

public class RenderGroupModel
{
    public RenderGroupType GroupType;
    public List<NoteModel> Notes = new();
    public float CenterBeat;
}