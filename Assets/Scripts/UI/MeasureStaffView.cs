using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MeasureStaffView : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("References")]
    public RectTransform staffLine;
    public RectTransform noteContainer;
    public Image[] beatMarkers;
    public TextMeshProUGUI measureLabel;

    private int _measureIndex;
    private FreeComposeController _controller;
    private List<GameObject> _noteObjects = new();

    public void Init(int measureIndex, FreeComposeController controller)
    {
        _measureIndex = measureIndex;
        _controller = controller;
        if (measureLabel != null)
            measureLabel.text = $"{measureIndex + 1}";
    }

    public void Refresh(
        FreeComposeData data,
        Dictionary<int, int> beamedGroupStarts,
        Func<NoteType, int, Sprite> getSpriteFunc)
    {
        foreach (var go in _noteObjects)
            Destroy(go);
        _noteObjects.Clear();

        float lineWidth = noteContainer.rect.width;

        // 收集這個小節的所有音符，依 grid 排序
        var measureNotes = new List<(int globalGrid, int gridInMeasure, NoteType type)>();
        foreach (var kv in data.PlacedNotes)
        {
            int globalGrid = kv.Key;
            if (globalGrid / FreeComposeData.GRID_RESOLUTION != _measureIndex) continue;
            int gridInMeasure = globalGrid % FreeComposeData.GRID_RESOLUTION;
            measureNotes.Add((globalGrid, gridInMeasure, kv.Value));
        }
        measureNotes.Sort((a, b) => a.gridInMeasure.CompareTo(b.gridInMeasure));

        int total = measureNotes.Count;
        for (int i = 0; i < total; i++)
        {
            var (globalGrid, gridInMeasure, type) = measureNotes[i];

            Sprite sp = getSpriteFunc(type, globalGrid);
            if (sp == null) continue;

            // 平均分配 X 位置
            float xPos = total == 1
                ? lineWidth * 0.5f
                : lineWidth * (i + 1) / (total + 1);

            var noteGo = new GameObject($"Note_{globalGrid}",
                typeof(RectTransform), typeof(Image));
            noteGo.transform.SetParent(noteContainer, false);

            var rt = noteGo.GetComponent<RectTransform>();
            var img = noteGo.GetComponent<Image>();
            img.sprite = sp;
            img.SetNativeSize();

            float targetHeight = 120f;
            if (rt.sizeDelta.y > 0)
            {
                float scale = targetHeight / rt.sizeDelta.y;
                rt.sizeDelta = new Vector2(rt.sizeDelta.x * scale, targetHeight);
            }

            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(xPos, -30f);

            int capturedGrid = gridInMeasure;
            var btn = noteGo.AddComponent<Button>();
            btn.onClick.AddListener(() => _controller.RemoveNote(_measureIndex, capturedGrid));

            _noteObjects.Add(noteGo);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        _controller.EndDrag();
        int gridInMeasure = GetGridFromPointer(eventData.position);
        _controller.TryPlaceNote(_measureIndex, gridInMeasure);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            int gridInMeasure = GetGridFromPointer(eventData.position);
            _controller.TryPlaceNote(_measureIndex, gridInMeasure);
        }
    }

    int GetGridFromPointer(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            noteContainer, screenPos, null, out Vector2 local);
        float ratio = Mathf.Clamp01(
            (local.x + noteContainer.rect.width * 0.5f) / noteContainer.rect.width);
        int raw = Mathf.RoundToInt(ratio * FreeComposeData.GRID_RESOLUTION);
        return Mathf.Clamp(raw, 0, FreeComposeData.GRID_RESOLUTION - 1);
    }
}