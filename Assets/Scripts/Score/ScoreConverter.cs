using UnityEngine;

public static class ScoreConverter
{
    public const int GRID_RESOLUTION = 32;

    // ── 原有方法（保留不動）──────────────────────────
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

                NoteType noteType = ParseNoteType(noteDto.noteType);

                var noteModel = new NoteModel
                {
                    NoteId        = noteDto.noteId,
                    MeasureIndex  = measureDto.measureIndex,
                    BeatInMeasure = noteDto.beat,
                    AbsoluteBeat  = absoluteBeat,
                    DurationBeats = noteDto.duration,
                    NoteType      = noteType,
                    TargetTimeSec = absoluteBeat * secondsPerBeat,
                    IsRest        = IsRestType(noteType),
                };

                measureModel.Notes.Add(noteModel);
                model.AllNotes.Add(noteModel);
            }

            model.Measures.Add(measureModel);
        }

        return model;
    }

    // ── 新增方法（自由編譜用）────────────────────────

    // gridIndex → absoluteBeat
    public static float GridToAbsoluteBeat(int measureIndex, int gridIndex, int beatsPerMeasure = 4)
        => measureIndex * beatsPerMeasure + (gridIndex / (float)GRID_RESOLUTION) * beatsPerMeasure;

    // absoluteBeat → 秒
    public static float BeatToTime(float absoluteBeat, float bpm)
        => absoluteBeat * (60f / bpm);

    // NoteType → 拍值長度
    public static float GetDurationBeats(NoteType t) => t switch
    {
        NoteType.Whole            => 4.000f,
        NoteType.Half             => 2.000f,
        NoteType.DottedHalf       => 3.000f,
        NoteType.Quarter          => 1.000f,
        NoteType.DottedQuarter    => 1.500f,
        NoteType.Eighth           => 0.500f,
        NoteType.DottedEighth     => 0.750f,
        NoteType.Sixteenth        => 0.250f,
        NoteType.DottedSixteenth  => 0.375f,
        NoteType.ThirtySecond     => 0.125f,
        NoteType.TripletEighth    => 1f / 3f,
        NoteType.TripletQuarter   => 2f / 3f,
        NoteType.TripletHalf      => 4f / 3f,
        NoteType.RestWhole        => 4.000f,
        NoteType.RestHalf         => 2.000f,
        NoteType.RestQuarter      => 1.000f,
        NoteType.RestEighth       => 0.500f,
        NoteType.RestSixteenth    => 0.250f,
        NoteType.RestThirtySecond => 0.125f,
        _                         => 1.000f,
    };

    // NoteType → 佔格數
    public static int GetGridSpan(NoteType t) => t switch
    {
        NoteType.Whole            => 32,
        NoteType.Half             => 16,
        NoteType.DottedHalf       => 24,
        NoteType.Quarter          => 8,
        NoteType.DottedQuarter    => 12,
        NoteType.Eighth           => 4,
        NoteType.DottedEighth     => 6,
        NoteType.Sixteenth        => 2,
        NoteType.DottedSixteenth  => 3,
        NoteType.ThirtySecond     => 1,
        NoteType.RestWhole        => 32,
        NoteType.RestHalf         => 16,
        NoteType.RestQuarter      => 8,
        NoteType.RestEighth       => 4,
        NoteType.RestSixteenth    => 2,
        NoteType.RestThirtySecond => 1,
        _                         => 8,
    };

    // 判斷是否為休止符
    public static bool IsRestType(NoteType t) =>
        t is NoteType.RestWhole or NoteType.RestHalf or NoteType.RestQuarter
          or NoteType.RestEighth or NoteType.RestSixteenth or NoteType.RestThirtySecond;

    // ── 原有私有方法（保留）──────────────────────────
    private static NoteType ParseNoteType(string raw) => raw switch
    {
        "whole"           => NoteType.Whole,
        "half"            => NoteType.Half,
        "dottedHalf"      => NoteType.DottedHalf,
        "quarter"         => NoteType.Quarter,
        "dottedQuarter"   => NoteType.DottedQuarter,
        "eighth"          => NoteType.Eighth,
        "dottedEighth"    => NoteType.DottedEighth,
        "sixteenth"       => NoteType.Sixteenth,
        "dottedSixteenth" => NoteType.DottedSixteenth,
        "thirtySecond"    => NoteType.ThirtySecond,
        "tripletEighth"   => NoteType.TripletEighth,
        "tripletQuarter"  => NoteType.TripletQuarter,
        "tripletHalf"     => NoteType.TripletHalf,
        "restWhole"       => NoteType.RestWhole,
        "restHalf"        => NoteType.RestHalf,
        "restQuarter"     => NoteType.RestQuarter,
        "restEighth"      => NoteType.RestEighth,
        "restSixteenth"   => NoteType.RestSixteenth,
        "restThirtySecond"=> NoteType.RestThirtySecond,
        _                 => NoteType.Quarter,
    };
}