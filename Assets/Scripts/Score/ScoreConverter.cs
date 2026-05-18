using UnityEngine;

public static class ScoreConverter
{
    public static ScoreModel Convert(ScoreJsonDto dto)
    {
        if (dto == null)
        {
            Debug.LogError("ScoreJsonDto is null.");
            return null;
        }

        var model = new ScoreModel
        {
            ScoreId = dto.scoreId,
            Title = dto.title,
            Bpm = dto.bpm,
            TimeSignature = new TimeSignatureModel
            {
                BeatsPerMeasure = dto.timeSignature.beatsPerMeasure,
                BeatUnit = dto.timeSignature.beatUnit
            }
        };

        float secondsPerBeat = 60f / model.Bpm;

        foreach (var measureDto in dto.measures)
        {
            var measureModel = new MeasureModel
            {
                MeasureIndex = measureDto.measureIndex
            };

            foreach (var noteDto in measureDto.notes)
            {
                float absoluteBeat =
                    (measureDto.measureIndex - 1) * model.TimeSignature.BeatsPerMeasure
                    + noteDto.beat;

                var noteModel = new NoteModel
                {
                    NoteId = noteDto.noteId,
                    MeasureIndex = measureDto.measureIndex,
                    BeatInMeasure = noteDto.beat,
                    AbsoluteBeat = absoluteBeat,
                    DurationBeats = noteDto.duration,
                    NoteType = ParseNoteType(noteDto.noteType),
                    TargetTimeSec = absoluteBeat * secondsPerBeat
                };

                measureModel.Notes.Add(noteModel);
                model.AllNotes.Add(noteModel);
            }

            model.Measures.Add(measureModel);
        }

        return model;
    }

    private static NoteType ParseNoteType(string raw)
    {
        return raw switch
        {
            "quarter" => NoteType.Quarter,
            "eighth" => NoteType.Eighth,
            _ => NoteType.Quarter
        };
    }
}