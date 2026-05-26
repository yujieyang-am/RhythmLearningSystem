using System.Collections.Generic;

public enum NoteType
{
    // 獨立音符
    Whole,
    Half,
    Quarter,
    Eighth,
    Sixteenth,
    ThirtySecond,
    DottedHalf,
    DottedQuarter,
    DottedEighth,
    DottedSixteenth,
    // 三連音
    TripletHalf,
    TripletQuarter,
    TripletEighth,
    // 休止符
    RestWhole,
    RestHalf,
    RestQuarter,
    RestEighth,
    RestSixteenth,
    RestThirtySecond,
}

public class ScoreModel
{
    public string ScoreId;
    public string Title;
    public float Bpm;
    public TimeSignatureModel TimeSignature;
    public List<MeasureModel> Measures = new();
    public List<NoteModel> AllNotes = new();
}

public class TimeSignatureModel
{
    public int BeatsPerMeasure;
    public int BeatUnit;
}

public class MeasureModel
{
    public int MeasureIndex;
    public List<NoteModel> Notes = new();
}

public class NoteModel
{
    public string NoteId;
    public int MeasureIndex;

    // 小節內第幾拍
    public float BeatInMeasure;

    // 從整首開始算的第幾拍
    public float AbsoluteBeat;

    // 音符時值（拍數）
    public float DurationBeats;

    public NoteType NoteType;

    // 示範音與判定用
    public double TargetTimeSec;

    // 自由編譜用
    public int GridIndex;

    // 休止符不產生判定點
    public bool IsRest;
}