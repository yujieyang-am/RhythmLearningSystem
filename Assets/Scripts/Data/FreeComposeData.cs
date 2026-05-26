using System.Collections.Generic;

public class FreeComposeData
{
    public float  Bpm             = 80f;
    public int    BeatsPerMeasure = 4;
    public int    MeasureCount    = 2;
    public int    RepeatCount     = 2;

    // key = "measureIndex-gridIndex", value = NoteType
    public Dictionary<string, NoteType> Cells = new Dictionary<string, NoteType>();
}