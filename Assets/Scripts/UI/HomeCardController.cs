using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HomeCardController : MonoBehaviour
{
    [Header("References")]
    public Image backgroundImage;
    public Image overlayImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI subtitleText;
    public RectTransform cardRect;

    private HomeCardData _data;

    public void Setup(HomeCardData data)
    {
        _data = data;

        if (data.backgroundSprite != null)
        {
            backgroundImage.sprite = data.backgroundSprite;
            backgroundImage.color = Color.white;
        }
        else
        {
            backgroundImage.color = data.placeholderColor;
        }

        // Overlay 固定深色半透明
        if (overlayImage != null)
            overlayImage.color = new Color(0, 0, 0, 0.5f);

        titleText.text = data.titleText;
        subtitleText.text = data.subtitleText;
        subtitleText.gameObject.SetActive(false);
    }

    public void SetAsCenter(bool isCenter)
    {
        subtitleText.gameObject.SetActive(isCenter);
    }
}