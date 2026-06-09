using System.Collections.Generic;
using UnityEngine;

public static class FreeComposeConverter
{
    public static ScoreModel Convert(FreeComposeData data)
    {
        var score = new ScoreModel
        {
            ScoreId = "free_compose",
            Title = "自由編譜",
            Bpm = data.Bpm,
            TimeSignature = new TimeSignatureModel
            {
                BeatsPerMeasure = FreeComposeData.BEATS_PER_MEASURE,
                BeatUnit = 4
            },
            Measures = new List<MeasureModel>(),
            AllNotes = new List<NoteModel>()
        };

        float beatsPerGrid = 1f / (FreeComposeData.GRID_RESOLUTION / FreeComposeData.BEATS_PER_MEASURE);
        float secondsPerBeat = 60f / data.Bpm;

        for (int m = 0; m < FreeComposeData.MEASURES; m++)
        {
            var measure = new MeasureModel
            {
                MeasureIndex = m + 1, // 改成從 1 開始
                Notes = new List<NoteModel>()
            };

            int measureStart = m * FreeComposeData.GRID_RESOLUTION;

            foreach (var kv in data.PlacedNotes)
            {
                int globalGrid = kv.Key;
                if (globalGrid / FreeComposeData.GRID_RESOLUTION != m) continue;

                int gridInMeasure = globalGrid % FreeComposeData.GRID_RESOLUTION;
                NoteType type = kv.Value;
                float beatInMeasure = gridInMeasure * beatsPerGrid;
                float absoluteBeat = m * FreeComposeData.BEATS_PER_MEASURE + beatInMeasure;
                float durationBeats = FreeComposeController.GetGridLength(type) * beatsPerGrid;
                double targetTimeSec = absoluteBeat * secondsPerBeat;

                var note = new NoteModel
                {
                    NoteId = $"n_{globalGrid}",
                    MeasureIndex = m + 1, // 改成從 1 開始
                    BeatInMeasure = beatInMeasure,
                    AbsoluteBeat = absoluteBeat,
                    DurationBeats = durationBeats,
                    NoteType = type,
                    TargetTimeSec = targetTimeSec,
                    GridIndex = gridInMeasure,
                    IsRest = IsRest(type)
                };

                measure.Notes.Add(note);
                score.AllNotes.Add(note);
            }

            score.Measures.Add(measure);
        }

        return score;
    }

    static bool IsRest(NoteType type)
    {
        return type == NoteType.RestWhole
            || type == NoteType.RestHalf
            || type == NoteType.RestQuarter
            || type == NoteType.RestEighth
            || type == NoteType.RestSixteenth
            || type == NoteType.RestThirtySecond;
    }
}