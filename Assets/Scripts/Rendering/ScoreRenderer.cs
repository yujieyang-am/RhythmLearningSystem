using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreRenderer : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private RectTransform scoreArea;

    [Header("Note Prefabs")]
    [SerializeField] private NoteView quarterNotePrefab;
    [SerializeField] private NoteView beamedEighthPairPrefab;

    [Header("Time Signature")]
    [SerializeField] private TMP_Text timeSigTopText;
    [SerializeField] private TMP_Text timeSigBottomText;

    [Header("Layout")]
    [SerializeField] private float noteAreaLeftX = -500f;
    [SerializeField] private float noteAreaRightX = 700f;
    [SerializeField] private float noteY = 0f;

    [Header("Padding")]
    [SerializeField] private float beatPadding = 0.5f;

    private Dictionary<RenderGroupModel, NoteView> groupViewMap =
        new Dictionary<RenderGroupModel, NoteView>();

    public void RenderMeasure(
        ScoreModel scoreModel,
        int measureIndex,
        List<RenderGroupModel> groups
    )
    {
        ClearOldNotes();
        groupViewMap.Clear();

        timeSigTopText.text = scoreModel.TimeSignature.BeatsPerMeasure.ToString();
        timeSigBottomText.text = scoreModel.TimeSignature.BeatUnit.ToString();

        foreach (var group in groups)
        {
            NoteView prefab = GetGroupPrefab(group.GroupType);
            NoteView noteView = Instantiate(prefab, scoreArea);

            RectTransform rect = noteView.GetComponent<RectTransform>();

            float x = GetGroupXPosition(
                group,
                scoreModel.TimeSignature.BeatsPerMeasure
            );

            rect.anchoredPosition = new Vector2(x, noteY);

            noteView.Bind(group);

            groupViewMap[group] = noteView;
        }
    }

    public void ApplyHitResult(HitResult hitResult)
    {
        if (hitResult == null || hitResult.Target == null)
            return;

        RenderGroupModel group = hitResult.Target.RenderGroup;

        if (!groupViewMap.TryGetValue(group, out NoteView noteView))
        {
            Debug.LogWarning("No NoteView found for RenderGroup.");
            return;
        }

        noteView.ApplyHitResult(hitResult.ResultType);
    }

    public void ApplyDemoPlayed(RenderGroupModel group)
    {
        if (groupViewMap.TryGetValue(group, out NoteView noteView))
        {
            noteView.SetDemoPlayed();
        }
    }

    private float GetGroupXPosition(RenderGroupModel group, float beatsPerMeasure)
    {
        float totalSpan = beatsPerMeasure + beatPadding * 2f;
        float shiftedBeat = group.CenterBeat + beatPadding;

        float t = shiftedBeat / totalSpan;
        return Mathf.Lerp(noteAreaLeftX, noteAreaRightX, t);
    }

    private NoteView GetGroupPrefab(RenderGroupType type)
    {
        return type switch
        {
            RenderGroupType.BeamedEighthPair => beamedEighthPairPrefab,
            _ => quarterNotePrefab
        };
    }

    private void ClearOldNotes()
    {
        foreach (Transform child in scoreArea)
        {
            if (child.GetComponent<NoteView>() != null)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void ResetAllNoteColors()
    {
        foreach (var noteView in groupViewMap.Values)
        {
            noteView.SetDefaultColor();
        }
    }
}