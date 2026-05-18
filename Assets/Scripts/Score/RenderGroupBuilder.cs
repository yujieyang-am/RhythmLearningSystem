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

            if (i + 1 < notes.Count &&
                current.NoteType == NoteType.Eighth &&
                notes[i + 1].NoteType == NoteType.Eighth &&
                current.MeasureIndex == notes[i + 1].MeasureIndex &&
                Mathf.Approximately(notes[i + 1].BeatInMeasure - current.BeatInMeasure, 0.5f))
            {
                var next = notes[i + 1];

                var group = new RenderGroupModel
                {
                    GroupType = RenderGroupType.BeamedEighthPair,
                    CenterBeat = (current.BeatInMeasure + next.BeatInMeasure) / 2f
                };

                group.Notes.Add(current);
                group.Notes.Add(next);

                groups.Add(group);

                i++;
            }
            else
            {
                var group = new RenderGroupModel
                {
                    GroupType = RenderGroupType.Single,
                    CenterBeat = current.BeatInMeasure
                };

                group.Notes.Add(current);

                groups.Add(group);
            }
        }

        return groups;
    }
}