using System.Collections.Generic;

public enum RenderGroupType
{
    Single,
    BeamedEighthPair
}

public class RenderGroupModel
{
    public RenderGroupType GroupType;

    public List<NoteModel> Notes = new();

    // 用來決定畫在哪
    public float CenterBeat;
}