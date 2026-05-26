using System.Collections.Generic;
using UnityEngine;

public static class RenderGroupBuilder
{
    public static List<RenderGroupModel> Build(ScoreModel score)
    {
        return BuildFromNotes(score.AllNotes);
    }

    public static List<RenderGroupModel> BuildFromNotes(List<NoteModel> notes)
    {
        var groups = new List<RenderGroupModel>();

        for (int i = 0; i < notes.Count; i++)
        {
            var current = notes[i];

            // 休止符單獨成一組
            if (current.IsRest)
            {
                groups.Add(new RenderGroupModel
                {
                    GroupType  = RenderGroupType.Rest,
                    CenterBeat = current.BeatInMeasure,
                    Notes      = { current }
                });
                continue;
            }

            // 嘗試合成連體音符
            RenderGroupModel beamed = TryBuildBeamedGroup(notes, i);
            if (beamed != null)
            {
                groups.Add(beamed);
                i += beamed.Notes.Count - 1;
                continue;
            }

            // 獨立音符
            groups.Add(new RenderGroupModel
            {
                GroupType  = RenderGroupType.Single,
                CenterBeat = current.BeatInMeasure,
                Notes      = { current }
            });
        }

        return groups;
    }

    private static RenderGroupModel TryBuildBeamedGroup(List<NoteModel> notes, int startIndex)
    {
        var first = notes[startIndex];

        // 只處理八分、十六分、三十二分、三連音
        if (first.NoteType != NoteType.Eighth &&
            first.NoteType != NoteType.Sixteenth &&
            first.NoteType != NoteType.ThirtySecond &&
            first.NoteType != NoteType.TripletEighth)
            return null;

        var collected = new List<NoteModel> { first };
        float step = ScoreConverter.GetDurationBeats(first.NoteType);

        // 以拍為單位限制範圍，不允許跨越整數拍
        float groupStartBeat = Mathf.Floor(first.BeatInMeasure);
        float groupEndBeat   = groupStartBeat + 1f;

        for (int j = startIndex + 1; j < notes.Count; j++)
        {
            var next = notes[j];

            if (next.NoteType != first.NoteType) break;
            if (next.MeasureIndex != first.MeasureIndex) break;

            float expectedBeat = first.BeatInMeasure + step * collected.Count;
            if (!Mathf.Approximately(next.BeatInMeasure, expectedBeat)) break;

            // 不跨越整數拍
            if (next.BeatInMeasure >= groupEndBeat) break;

            collected.Add(next);

            if (collected.Count == 4) break;
        }

        // 單獨一顆不合成連體
        if (collected.Count < 2) return null;

        RenderGroupType groupType = DetermineGroupType(first.NoteType, collected.Count);
        float centerBeat = (collected[0].BeatInMeasure + collected[collected.Count - 1].BeatInMeasure) / 2f;

        return new RenderGroupModel
        {
            GroupType  = groupType,
            CenterBeat = centerBeat,
            Notes      = collected
        };
    }

    private static RenderGroupType DetermineGroupType(NoteType noteType, int count)
    {
        return noteType switch
        {
            NoteType.Eighth => count switch
            {
                2 => RenderGroupType.BeamedEighth2,
                3 => RenderGroupType.BeamedEighth3,
                _ => RenderGroupType.BeamedEighth4,
            },
            NoteType.Sixteenth => count switch
            {
                2 => RenderGroupType.BeamedSixteenth2,
                _ => RenderGroupType.BeamedSixteenth4,
            },
            NoteType.ThirtySecond  => RenderGroupType.BeamedThirtySecond2,
            NoteType.TripletEighth => RenderGroupType.Triplet,
            _                      => RenderGroupType.Single,
        };
    }
}