using System.Collections.Generic;

public class FreeComposeData
{
    public float Bpm = 80f;
    public int RepeatCount = 1;
    public const int MEASURES = 4;
    public const int BEATS_PER_MEASURE = 4;
    public const int GRID_RESOLUTION = 32; // 每小節32格
    public const int TOTAL_GRIDS = MEASURES * GRID_RESOLUTION; // 128格

    // key = gridIndex (0~127), value = 放入的音符
    public Dictionary<int, NoteType> PlacedNotes = new();
}