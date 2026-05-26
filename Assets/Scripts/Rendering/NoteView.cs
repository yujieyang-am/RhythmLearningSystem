using UnityEngine;
using UnityEngine.UI;

public class NoteView : MonoBehaviour
{
    [SerializeField] private Image noteImage;
    [SerializeField] private Image feedbackGlowImage;

    public RenderGroupModel RenderGroup { get; private set; }

    public void Bind(RenderGroupModel renderGroup)
    {
        RenderGroup = renderGroup;
        SetDefaultColor();
    }

    public void SetDefaultColor()
    {
        if (noteImage != null)
        {
            noteImage.color = Color.white;
        }

        if (feedbackGlowImage != null)
        {
            feedbackGlowImage.color = new Color(1f, 1f, 1f, 0f);
        }
    }

    public void ApplyHitResult(HitResultType resultType)
    {
        switch (resultType)
        {
            case HitResultType.Perfect:
                SetFeedbackColor(new Color(0.2f, 1f, 0.35f, 0.65f));
                break;

            case HitResultType.Early:
                SetFeedbackColor(new Color(0.2f, 0.55f, 1f, 0.65f));
                break;

            case HitResultType.Late:
                SetFeedbackColor(new Color(1f, 0.85f, 0.2f, 0.65f));
                break;

            case HitResultType.Miss:
                SetFeedbackColor(new Color(1f, 0.15f, 0.15f, 0.75f));
                break;

            default:
                SetDefaultColor();
                break;
        }
    }

    public void SetDemoPlayed()
    {
        Debug.Log("SetDemoPlayed called: " + gameObject.name);
        if (noteImage != null)
        {
            noteImage.color = new Color(1f, 1f, 1f, 0.25f);
            Debug.Log("noteImage color set to: " + noteImage.color);
        }
        else
        {
            Debug.LogWarning("noteImage is NULL");
        }
    }

    public void SetSprite(Sprite sprite)
    {
        if (noteImage != null)
            noteImage.sprite = sprite;
    }

    private void SetFeedbackColor(Color color)
    {
        if (feedbackGlowImage != null)
        {
            feedbackGlowImage.color = color;
        }
    }
}