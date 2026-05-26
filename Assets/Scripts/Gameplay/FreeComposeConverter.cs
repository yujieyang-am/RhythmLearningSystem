using System.Collections.Generic;
using UnityEngine;

public static class FreeComposeConverter
{
    public static ScoreModel Convert(FreeComposeData data)
    {
        float spb = 60f / data.Bpm;
        var notes = new List<NoteModel>();

        foreach (var kv in data.Cells)
        {
            // 跳過休止符
            if (ScoreConverter.IsRestType(kv.Value)) continue;

            var parts        = kv.Key.Split('-');
            int measureIndex = int.Parse(parts[0]);
            int gridIndex    = int.Parse(parts[1]);

            float ab = ScoreConverter.GridToAbsoluteBeat(
                           measureIndex, gridIndex, data.BeatsPerMeasure);

            notes.Add(new NoteModel
            {
                NoteType      = kv.Value,
                AbsoluteBeat  = ab,
                TargetTimeSec = ab * spb,
                DurationBeats = ScoreConverter.GetDurationBeats(kv.Value),
                MeasureIndex  = measureIndex,
                GridIndex     = gridIndex,
                BeatInMeasure = ScoreConverter.GridToAbsoluteBeat(0, gridIndex, data.BeatsPerMeasure),
                IsRest        = false,
            });
        }

        // 重複次數
        if (data.RepeatCount > 1)
        {
            float totalBeats = data.MeasureCount * data.BeatsPerMeasure;
            var origNotes = new List<NoteModel>(notes);

            for (int r = 1; r < data.RepeatCount; r++)
            {
                float offset = totalBeats * r;
                foreach (var n in origNotes)
                {
                    notes.Add(new NoteModel
                    {
                        NoteType      = n.NoteType,
                        AbsoluteBeat  = n.AbsoluteBeat + offset,
                        TargetTimeSec = (n.AbsoluteBeat + offset) * spb,
                        DurationBeats = n.DurationBeats,
                        MeasureIndex  = n.MeasureIndex + data.MeasureCount * r,
                        GridIndex     = n.GridIndex,
                        BeatInMeasure = n.BeatInMeasure,
                        IsRest        = false,
                    });
                }
            }
        }

        notes.Sort((a, b) => a.AbsoluteBeat.CompareTo(b.AbsoluteBeat));

        // 組成 ScoreModel
        var model = new ScoreModel
        {
            ScoreId = "free_compose",
            Title   = "Free Compose",
            Bpm     = data.Bpm,
            TimeSignature = new TimeSignatureModel
            {
                BeatsPerMeasure = data.BeatsPerMeasure,
                BeatUnit        = 4
            }
        };

        // 整理進 Measures
        var measureDict = new Dictionary<int, MeasureModel>();
        foreach (var note in notes)
        {
            if (!measureDict.ContainsKey(note.MeasureIndex))
            {
                var measure = new MeasureModel { MeasureIndex = note.MeasureIndex };
                measureDict[note.MeasureIndex] = measure;
                model.Measures.Add(measure);
            }
            measureDict[note.MeasureIndex].Notes.Add(note);
            model.AllNotes.Add(note);
        }

        model.Measures.Sort((a, b) => a.MeasureIndex.CompareTo(b.MeasureIndex));

        return model;
    }
}