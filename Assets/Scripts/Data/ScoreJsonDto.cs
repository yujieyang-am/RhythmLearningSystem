using System;
using System.Collections.Generic;

[Serializable]
public class ScoreJsonDto
{
    public string scoreId;
    public string title;
    public float bpm;
    public TimeSignatureDto timeSignature;
    public List<MeasureDto> measures;
}

[Serializable]
public class TimeSignatureDto
{
    public int beatsPerMeasure;
    public int beatUnit;
}

[Serializable]
public class MeasureDto
{
    public int measureIndex;
    public List<NoteDto> notes;
}

[Serializable]
public class NoteDto
{
    public string noteId;
    public float beat;
    public float duration;
    public string noteType;
}