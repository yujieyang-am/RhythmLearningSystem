using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreRenderer : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private RectTransform scoreArea;

    [Header("通用 Prefab")]
    [SerializeField] private NoteView noteViewPrefab;

    [Header("音符 Sprites")]
    [SerializeField] private Sprite spriteWhole;
    [SerializeField] private Sprite spriteHalf;
    [SerializeField] private Sprite spriteDottedHalf;
    [SerializeField] private Sprite spriteQuarter;
    [SerializeField] private Sprite spriteDottedQuarter;
    [SerializeField] private Sprite spriteEighth;
    [SerializeField] private Sprite spriteDottedEighth;
    [SerializeField] private Sprite spriteSixteenth;
    [SerializeField] private Sprite spriteDottedSixteenth;
    [SerializeField] private Sprite spriteThirtySecond;

    [Header("連體音符 Sprites")]
    [SerializeField] private Sprite spriteBeamedEighth2;
    [SerializeField] private Sprite spriteBeamedEighth3;
    [SerializeField] private Sprite spriteBeamedEighth4;
    [SerializeField] private Sprite spriteBeamedSixteenth2;
    [SerializeField] private Sprite spriteBeamedSixteenth4;
    [SerializeField] private Sprite spriteBeamedThirtySecond2;
    [SerializeField] private Sprite spriteTriplet;

    [Header("休止符 Sprites")]
    [SerializeField] private Sprite spriteRestWhole;
    [SerializeField] private Sprite spriteRestHalf;
    [SerializeField] private Sprite spriteRestQuarter;
    [SerializeField] private Sprite spriteRestEighth;
    [SerializeField] private Sprite spriteRestSixteenth;
    [SerializeField] private Sprite spriteRestThirtySecond;

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
            NoteView noteView = Instantiate(noteViewPrefab, scoreArea);
            RectTransform rect = noteView.GetComponent<RectTransform>();

            float x = GetGroupXPosition(group, scoreModel.TimeSignature.BeatsPerMeasure);
            rect.anchoredPosition = new Vector2(x, noteY);

            Sprite sprite = GetGroupSprite(group);
            noteView.SetSprite(sprite);
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

    public void ResetAllNoteColors()
    {
        foreach (var noteView in groupViewMap.Values)
        {
            noteView.SetDefaultColor();
        }
    }

    private float GetGroupXPosition(RenderGroupModel group, float beatsPerMeasure)
    {
        float totalSpan = beatsPerMeasure + beatPadding * 2f;
        float shiftedBeat = group.CenterBeat + beatPadding;
        float t = shiftedBeat / totalSpan;
        return Mathf.Lerp(noteAreaLeftX, noteAreaRightX, t);
    }

    private Sprite GetGroupSprite(RenderGroupType type) => type switch
    {
        RenderGroupType.Single => GetSingleSprite(),
        RenderGroupType.BeamedEighth2      => spriteBeamedEighth2,
        RenderGroupType.BeamedEighth3      => spriteBeamedEighth3,
        RenderGroupType.BeamedEighth4      => spriteBeamedEighth4,
        RenderGroupType.BeamedSixteenth2   => spriteBeamedSixteenth2,
        RenderGroupType.BeamedSixteenth4   => spriteBeamedSixteenth4,
        RenderGroupType.BeamedThirtySecond2 => spriteBeamedThirtySecond2,
        RenderGroupType.Triplet            => spriteTriplet,
        RenderGroupType.Rest               => GetRestSprite(),
        _                                  => spriteQuarter,
    };

    // Single 時根據 NoteType 決定 Sprite
    private Sprite _currentSingleSprite;
    private Sprite GetSingleSprite() => _currentSingleSprite;

    private void ClearOldNotes()
    {
        foreach (Transform child in scoreArea)
        {
            if (child.GetComponent<NoteView>() != null)
                Destroy(child.gameObject);
        }
    }

    private Sprite GetRestSprite() => spriteRestQuarter; // 預設，之後細化

    private Sprite GetGroupSprite(RenderGroupModel group) => group.GroupType switch
    {
        RenderGroupType.BeamedEighth2       => spriteBeamedEighth2,
        RenderGroupType.BeamedEighth3       => spriteBeamedEighth3,
        RenderGroupType.BeamedEighth4       => spriteBeamedEighth4,
        RenderGroupType.BeamedSixteenth2    => spriteBeamedSixteenth2,
        RenderGroupType.BeamedSixteenth4    => spriteBeamedSixteenth4,
        RenderGroupType.BeamedThirtySecond2 => spriteBeamedThirtySecond2,
        RenderGroupType.Triplet             => spriteTriplet,
        RenderGroupType.Rest                => GetRestSprite(group.Notes[0].NoteType),
        _                                   => GetSingleSprite(group.Notes[0].NoteType),
    };

    private Sprite GetSingleSprite(NoteType noteType) => noteType switch
    {
        NoteType.Whole           => spriteWhole,
        NoteType.Half            => spriteHalf,
        NoteType.DottedHalf      => spriteDottedHalf,
        NoteType.Quarter         => spriteQuarter,
        NoteType.DottedQuarter   => spriteDottedQuarter,
        NoteType.Eighth          => spriteEighth,
        NoteType.DottedEighth    => spriteDottedEighth,
        NoteType.Sixteenth       => spriteSixteenth,
        NoteType.DottedSixteenth => spriteDottedSixteenth,
        NoteType.ThirtySecond    => spriteThirtySecond,
        _                        => spriteQuarter,
    };

    private Sprite GetRestSprite(NoteType noteType) => noteType switch
    {
        NoteType.RestWhole        => spriteRestWhole,
        NoteType.RestHalf         => spriteRestHalf,
        NoteType.RestQuarter      => spriteRestQuarter,
        NoteType.RestEighth       => spriteRestEighth,
        NoteType.RestSixteenth    => spriteRestSixteenth,
        NoteType.RestThirtySecond => spriteRestThirtySecond,
        _                         => spriteRestQuarter,
    };
}