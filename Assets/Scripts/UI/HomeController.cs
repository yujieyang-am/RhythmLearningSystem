using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;

public class HomeController : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Card Settings")]
    public List<HomeCardData> cards;
    public GameObject cardPrefab;
    public RectTransform cardContainer;

    [Header("Card Sizes")]
    public float centerCardWidth = 600f;
    public float centerCardHeight = 800f;
    public float sideCardScale = 0.75f;
    public float sideCardAlpha = 0.5f;
    public float cardSpacing = 40f;

    [Header("TopBar")]
    public Transform topBarContainer;
    public GameObject topBarItemPrefab;
    public float topBarMaxFontSize = 48f;
    public float topBarMinFontSize = 24f;

    [Header("Indicator Dots")]
    public Transform dotContainer;
    public GameObject dotPrefab;
    public float dotNormalWidth = 12f;
    public float dotActiveWidth = 32f;
    public float dotHeight = 12f;

    [Header("Animation")]
    public float swipeAnimDuration = 0.3f;

    private List<HomeCardController> _cardViews = new();
    private List<TextMeshProUGUI> _topBarItems = new();
    private List<RectTransform> _dots = new();

    private int _currentIndex = 0;
    private bool _isAnimating = false;
    private float _dragStartX;
    private float _dragThreshold = 80f;

    void Start()
    {
        BuildCards();
        BuildTopBar();
        BuildDots();
        RefreshLayout(animate: false);
    }

    // ── 建立卡片 ──────────────────────────────────────────

    void BuildCards()
    {
        foreach (var data in cards)
        {
            var go = Instantiate(cardPrefab, cardContainer);
            var ctrl = go.GetComponent<HomeCardController>();
            ctrl.Setup(data);
            _cardViews.Add(ctrl);
        }
    }

    // ── 建立 TopBar ───────────────────────────────────────

    void BuildTopBar()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            int idx = i;
            var go = Instantiate(topBarItemPrefab, topBarContainer);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = cards[i].titleText;
            tmp.GetComponent<Button>().onClick.AddListener(() => SwitchTo(idx));
            _topBarItems.Add(tmp);
        }
    }

    // ── 建立底部圓點 ──────────────────────────────────────

    void BuildDots()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            var go = Instantiate(dotPrefab, dotContainer);
            _dots.Add(go.GetComponent<RectTransform>());
        }
    }

    // ── 切換卡片 ──────────────────────────────────────────

    public void SwitchTo(int index)
    {
        if (_isAnimating || index == _currentIndex) return;
        if (index < 0 || index >= cards.Count) return;
        _currentIndex = index;
        RefreshLayout(animate: true);
    }

    void RefreshLayout(bool animate)
    {
        float totalWidth = centerCardWidth + cardSpacing;

        for (int i = 0; i < _cardViews.Count; i++)
        {
            var rt = _cardViews[i].cardRect;
            float offset = (i - _currentIndex) * totalWidth;

            float targetScale = (i == _currentIndex) ? 1f : sideCardScale;
            float targetAlpha = (i == _currentIndex) ? 1f : sideCardAlpha;

            _cardViews[i].SetAsCenter(i == _currentIndex);

            if (animate)
                StartCoroutine(AnimateCard(rt, _cardViews[i], offset, targetScale, targetAlpha));
            else
                ApplyCardLayout(rt, _cardViews[i], offset, targetScale, targetAlpha);
        }

        RefreshTopBar();
        RefreshDots();
    }

    void ApplyCardLayout(RectTransform rt, HomeCardController ctrl,
        float xPos, float scale, float alpha)
    {
        rt.anchoredPosition = new Vector2(xPos, 0);
        rt.localScale = Vector3.one * scale;

        var images = ctrl.GetComponentsInChildren<Image>();
        foreach (var img in images)
        {
            if (img == ctrl.overlayImage) continue; // 跳過 Overlay
            var c = img.color;
            img.color = new Color(c.r, c.g, c.b, alpha);
        }
        var texts = ctrl.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var t in texts)
        {
            var c = t.color;
            t.color = new Color(c.r, c.g, c.b, alpha);
        }
    }

    IEnumerator AnimateCard(RectTransform rt, HomeCardController ctrl,
        float targetX, float targetScale, float targetAlpha)
    {
        _isAnimating = true;
        float elapsed = 0f;
        float startX = rt.anchoredPosition.x;
        float startScale = rt.localScale.x;

        var imgs = ctrl.GetComponentsInChildren<Image>();
        var txts = ctrl.GetComponentsInChildren<TextMeshProUGUI>();
        float startAlpha = imgs.Length > 0 ? imgs[0].color.a : 1f;

        while (elapsed < swipeAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / swipeAnimDuration);

            rt.anchoredPosition = new Vector2(Mathf.Lerp(startX, targetX, t), 0);
            rt.localScale = Vector3.one * Mathf.Lerp(startScale, targetScale, t);
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            foreach (var img in imgs)
            {
                if (img == ctrl.overlayImage) continue; // 跳過 Overlay
                img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);
            }
            foreach (var txt in txts)
                txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, alpha);

            yield return null;
        }

        ApplyCardLayout(rt, ctrl, targetX, targetScale, targetAlpha);
        _isAnimating = false;
    }

    // ── TopBar 更新 ───────────────────────────────────────

    void RefreshTopBar()
    {
        for (int i = 0; i < _topBarItems.Count; i++)
        {
            float dist = Mathf.Abs(i - _currentIndex);
            float t = Mathf.Clamp01(dist / 2f);
            _topBarItems[i].fontSize = Mathf.Lerp(topBarMaxFontSize, topBarMinFontSize, t);
            float alpha = Mathf.Lerp(1f, 0.4f, t);
            _topBarItems[i].color = new Color(1, 1, 1, alpha);
        }
    }

    // ── 底部圓點更新 ──────────────────────────────────────

    void RefreshDots()
    {
        for (int i = 0; i < _dots.Count; i++)
        {
            bool isActive = (i == _currentIndex);
            _dots[i].sizeDelta = new Vector2(
                isActive ? dotActiveWidth : dotNormalWidth,
                dotHeight);
        }
    }

    private bool _wasDragging = false;

    public void OnBeginDrag(PointerEventData eventData)
    {
        _dragStartX = eventData.position.x;
        _wasDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _wasDragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float delta = eventData.position.x - _dragStartX;
        if (Mathf.Abs(delta) < _dragThreshold) return;

        if (delta < 0) SwitchTo(_currentIndex + 1);
        else SwitchTo(_currentIndex - 1);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isAnimating) return;
        if (_wasDragging)
        {
            _wasDragging = false;
            return;
        }
        
        string sceneName = cards[_currentIndex].sceneName;
        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
    }
}