using System.Collections.Generic;

public enum NoteType
{
    Quarter,
    Eighth
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

    // 小節內第幾拍，例如 0, 1, 1.5, 2
    public float BeatInMeasure;

    // 從整首開始算的第幾拍
    public float AbsoluteBeat;

    // 音符時值，例如四分音符 1，八分音符 0.5
    public float DurationBeats;

    public NoteType NoteType;

    // 之後示範音與判定會用
    public double TargetTimeSec;
}