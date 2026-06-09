using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class FreeComposeController : MonoBehaviour
{
    [Header("Top Bar")]
    public Slider bpmSlider;
    public TextMeshProUGUI bpmLabel;
    public TMP_Dropdown repeatDropdown;

    [Header("Staff Area")]
    public RectTransform staffContainer;
    public GameObject measureLinePrefab;

    [Header("Note Palette")]
    public Button[] paletteButtons;
    public Sprite[] paletteSprites;

    [Header("Drag Ghost")]
    public Image dragGhostImage;

    [Header("Bottom Bar")]
    public Button clearButton;
    public Button startButton;

    [Header("Note Sprites - 根目錄")]
    public Sprite sp_Whole;
    public Sprite sp_Half;
    public Sprite sp_Quarter;
    public Sprite sp_Eighth;
    public Sprite sp_Sixteenth;
    public Sprite sp_ThirtySecond;
    public Sprite sp_DottedHalf;
    public Sprite sp_DottedQuarter;
    public Sprite sp_DottedEighth;
    public Sprite sp_DottedSixteenth;

    [Header("Note Sprites - 休止符 (partnew)")]
    public Sprite sp_RestWhole;
    public Sprite sp_RestHalf;
    public Sprite sp_RestQuarter;
    public Sprite sp_RestEighth;
    public Sprite sp_RestSixteenth;
    public Sprite sp_RestThirtySecond;

    [Header("Note Sprites - 連體 (partnew)")]
    public Sprite sp_Beamed8x2;
    public Sprite sp_Beamed8x3;
    public Sprite sp_Beamed8x4;
    public Sprite sp_Beamed16x2;
    public Sprite sp_Beamed16x4;

    private FreeComposeData _data = new();
    private NoteType _selectedNoteType = NoteType.Quarter;
    private bool _isDragging = false;
    private Sprite _dragSprite;
    private List<MeasureStaffView> _measureViews = new();
    private Dictionary<int, int> _beamedGroupStarts = new();

    private static readonly NoteType[] PaletteNoteTypes = new NoteType[]
    {
        NoteType.Whole,           // 0
        NoteType.Half,            // 1
        NoteType.Quarter,         // 2
        NoteType.Eighth,          // 3
        NoteType.Sixteenth,       // 4
        NoteType.ThirtySecond,    // 5
        NoteType.DottedHalf,      // 6
        NoteType.DottedQuarter,   // 7
        NoteType.DottedEighth,    // 8
        NoteType.DottedSixteenth, // 9
        NoteType.RestWhole,       // 10
        NoteType.RestHalf,        // 11
        NoteType.RestQuarter,     // 12
        NoteType.RestEighth,      // 13
        NoteType.RestSixteenth,   // 14
        NoteType.RestThirtySecond,// 15
        NoteType.Eighth,          // 16 連體八分乘二
        NoteType.Eighth,          // 17 連體八分乘三
        NoteType.Eighth,          // 18 連體八分乘四
        NoteType.Sixteenth,       // 19 連體十六分乘二
        NoteType.Sixteenth,       // 20 連體十六分乘四
    };

    private static readonly Dictionary<int, int> BeamedPaletteCount = new()
    {
        { 16, 2 }, { 17, 3 }, { 18, 4 }, { 19, 2 }, { 20, 4 }
    };

    private int _selectedPaletteIndex = 2;
    private int _beamedCount = 1;

    // ── 初始化 ────────────────────────────────────────────

    void Start()
    {
        InitBpmSlider();
        InitRepeatDropdown();
        InitPalette();
        InitStaff();
        InitButtons();
        dragGhostImage.gameObject.SetActive(false);
    }

    void InitBpmSlider()
    {
        bpmSlider.minValue = 40;
        bpmSlider.maxValue = 200;
        bpmSlider.value = _data.Bpm;
        bpmSlider.onValueChanged.AddListener(v =>
        {
            _data.Bpm = Mathf.Round(v);
            bpmLabel.text = $"BPM {_data.Bpm:0}";
        });
        bpmLabel.text = $"BPM {_data.Bpm:0}";
    }

    void InitRepeatDropdown()
    {
        repeatDropdown.ClearOptions();
        repeatDropdown.AddOptions(new List<string> { "x1", "x2", "x3", "x4" });
        repeatDropdown.value = 0;
        repeatDropdown.onValueChanged.AddListener(v => _data.RepeatCount = v + 1);
    }

    void InitPalette()
    {
        for (int i = 0; i < paletteButtons.Length; i++)
        {
            int idx = i;
            paletteButtons[i].onClick.AddListener(() => SelectPalette(idx));
            if (i < paletteSprites.Length && paletteSprites[i] != null)
                paletteButtons[i].GetComponent<Image>().sprite = paletteSprites[i];
        }
        SelectPalette(2);
    }

    void SelectPalette(int idx)
    {
        _selectedPaletteIndex = idx;
        _selectedNoteType = PaletteNoteTypes[idx];
        _beamedCount = BeamedPaletteCount.ContainsKey(idx) ? BeamedPaletteCount[idx] : 1;
        _dragSprite = idx < paletteSprites.Length ? paletteSprites[idx] : null;

        for (int i = 0; i < paletteButtons.Length; i++)
        {
            var colors = paletteButtons[i].colors;
            colors.normalColor = (i == idx) ? new Color(1f, 0.85f, 0.3f) : Color.white;
            paletteButtons[i].colors = colors;
        }
    }

    void InitStaff()
    {
        _measureViews.Clear();
        var children = staffContainer.Cast<Transform>().ToList();
        foreach (var t in children)
            Destroy(t.gameObject);

        for (int m = 0; m < FreeComposeData.MEASURES; m++)
        {
            var go = Instantiate(measureLinePrefab, staffContainer);
            var view = go.GetComponent<MeasureStaffView>();
            view.Init(m, this);
            _measureViews.Add(view);
        }
    }

    void InitButtons()
    {
        clearButton.onClick.AddListener(OnClearClicked);
        startButton.onClick.AddListener(OnStartClicked);
    }

    // ── 放音符 ────────────────────────────────────────────

    public bool TryPlaceNote(int measureIndex, int gridInMeasure)
    {
        int noteGridLen = GetGridLength(_selectedNoteType);
        int totalGridNeeded = noteGridLen * _beamedCount;
        int measureStart = measureIndex * FreeComposeData.GRID_RESOLUTION;

        // 找這個小節目前所有音符中，結束位置最右的一個
        int nextAvailableGrid = 0;
        foreach (var kv in _data.PlacedNotes)
        {
            if (kv.Key < measureStart || kv.Key >= measureStart + FreeComposeData.GRID_RESOLUTION) continue;
            int endGrid = (kv.Key - measureStart) + GetGridLength(kv.Value);
            if (endGrid > nextAvailableGrid)
                nextAvailableGrid = endGrid;
        }

        // 檢查放得下
        if (nextAvailableGrid + totalGridNeeded > FreeComposeData.GRID_RESOLUTION)
        {
            ShowRejectFeedback("小節已滿（超過 4 拍）");
            return false;
        }

        int globalGrid = measureStart + nextAvailableGrid;

        // 放入
        if (_beamedCount > 1)
        {
            for (int i = 0; i < _beamedCount; i++)
                _data.PlacedNotes[globalGrid + i * noteGridLen] = _selectedNoteType;
            _beamedGroupStarts[globalGrid] = _beamedCount;
        }
        else
        {
            _data.PlacedNotes[globalGrid] = _selectedNoteType;
        }

        RefreshMeasureView(measureIndex);
        return true;
    }

    // ── 移除音符 ──────────────────────────────────────────

    public void RemoveNote(int measureIndex, int gridInMeasure)
    {
        int globalGrid = measureIndex * FreeComposeData.GRID_RESOLUTION + gridInMeasure;

        if (!_data.PlacedNotes.TryGetValue(globalGrid, out var noteType))
            return;

        int removedGridLen;
        int shiftFromGlobalGrid; // 從這個位置之後的音符要往左移

        if (_beamedGroupStarts.TryGetValue(globalGrid, out int beamedCount))
        {
            // 刪掉整組連體
            int noteGridLen = GetGridLength(noteType);
            removedGridLen = noteGridLen * beamedCount;
            for (int i = 0; i < beamedCount; i++)
                _data.PlacedNotes.Remove(globalGrid + i * noteGridLen);
            _beamedGroupStarts.Remove(globalGrid);
            shiftFromGlobalGrid = globalGrid;
        }
        else if (IsBeamedChild(globalGrid, out int groupStart, out int groupCount, out NoteType groupType))
        {
            // 點到連體子音符，刪掉整組
            int noteGridLen = GetGridLength(groupType);
            removedGridLen = noteGridLen * groupCount;
            for (int i = 0; i < groupCount; i++)
                _data.PlacedNotes.Remove(groupStart + i * noteGridLen);
            _beamedGroupStarts.Remove(groupStart);
            shiftFromGlobalGrid = groupStart; // 從連體起點開始往右移
        }
        else
        {
            // 普通音符
            removedGridLen = GetGridLength(noteType);
            _data.PlacedNotes.Remove(globalGrid);
            shiftFromGlobalGrid = globalGrid;
        }

        // 把右邊所有音符往左移，補上空位
        ShiftNotesLeft(measureIndex, shiftFromGlobalGrid, removedGridLen);

        RefreshMeasureView(measureIndex);
    }

    // ── 往左移位 ──────────────────────────────────────────

    void ShiftNotesLeft(int measureIndex, int fromGlobalGrid, int shiftAmount)
    {
        int measureEnd = measureIndex * FreeComposeData.GRID_RESOLUTION + FreeComposeData.GRID_RESOLUTION;

        // 收集需要移動的音符（在 fromGlobalGrid 右邊），由左到右排序
        var toMove = _data.PlacedNotes
            .Where(kv => kv.Key > fromGlobalGrid && kv.Key < measureEnd)
            .OrderBy(kv => kv.Key)
            .ToList();

        foreach (var kv in toMove)
        {
            _data.PlacedNotes.Remove(kv.Key);
            _data.PlacedNotes[kv.Key - shiftAmount] = kv.Value;
        }

        // 同步移動 _beamedGroupStarts
        var beamedToMove = _beamedGroupStarts
            .Where(kv => kv.Key > fromGlobalGrid && kv.Key < measureEnd)
            .OrderBy(kv => kv.Key)
            .ToList();

        foreach (var kv in beamedToMove)
        {
            _beamedGroupStarts.Remove(kv.Key);
            _beamedGroupStarts[kv.Key - shiftAmount] = kv.Value;
        }
    }

    // ── 連體子音符判斷 ────────────────────────────────────

    bool IsBeamedChild(int grid, out int groupStart, out int groupCount, out NoteType groupType)
    {
        foreach (var kv in _beamedGroupStarts)
        {
            int start = kv.Key;
            int count = kv.Value;
            if (!_data.PlacedNotes.TryGetValue(start, out groupType)) continue;
            int len = GetGridLength(groupType);
            for (int i = 1; i < count; i++) // i 從 1 開始，起點本身不算子音符
            {
                if (start + i * len == grid)
                {
                    groupStart = start;
                    groupCount = count;
                    return true;
                }
            }
        }
        groupStart = -1;
        groupCount = 0;
        groupType = NoteType.Quarter;
        return false;
    }

    // ── 時值轉格數 ────────────────────────────────────────

    public static int GetGridLength(NoteType type)
    {
        return type switch
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
            _ => 8
        };
    }

    // ── 刷新視覺 ──────────────────────────────────────────

    void RefreshMeasureView(int measureIndex)
    {
        if (measureIndex < _measureViews.Count)
            _measureViews[measureIndex].Refresh(_data, _beamedGroupStarts, GetSpriteForNote);
    }

    void RefreshAllViews()
    {
        for (int m = 0; m < _measureViews.Count; m++)
            RefreshMeasureView(m);
    }

    // ── Sprite 取得 ───────────────────────────────────────

    public Sprite GetSpriteForNote(NoteType type, int globalGrid)
    {
        if (type == NoteType.Eighth || type == NoteType.Sixteenth)
        {
            if (_beamedGroupStarts.TryGetValue(globalGrid, out int count))
            {
                if (type == NoteType.Eighth)
                {
                    if (count == 2) return sp_Beamed8x2;
                    if (count == 3) return sp_Beamed8x3;
                    if (count >= 4) return sp_Beamed8x4;
                }
                else
                {
                    if (count == 2) return sp_Beamed16x2;
                    if (count >= 4) return sp_Beamed16x4;
                }
            }
            // 連體子音符不顯示（由起點的連體圖示代表整組）
            if (IsBeamedChild(globalGrid, out _, out _, out _))
                return null;
        }

        return type switch
        {
            NoteType.Whole            => sp_Whole,
            NoteType.Half             => sp_Half,
            NoteType.Quarter          => sp_Quarter,
            NoteType.Eighth           => sp_Eighth,
            NoteType.Sixteenth        => sp_Sixteenth,
            NoteType.ThirtySecond     => sp_ThirtySecond,
            NoteType.DottedHalf       => sp_DottedHalf,
            NoteType.DottedQuarter    => sp_DottedQuarter,
            NoteType.DottedEighth     => sp_DottedEighth,
            NoteType.DottedSixteenth  => sp_DottedSixteenth,
            NoteType.RestWhole        => sp_RestWhole,
            NoteType.RestHalf         => sp_RestHalf,
            NoteType.RestQuarter      => sp_RestQuarter,
            NoteType.RestEighth       => sp_RestEighth,
            NoteType.RestSixteenth    => sp_RestSixteenth,
            NoteType.RestThirtySecond => sp_RestThirtySecond,
            _ => sp_Quarter
        };
    }

    // ── Drag Ghost ────────────────────────────────────────

    void Update()
    {
        if (_isDragging && dragGhostImage.gameObject.activeSelf)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dragGhostImage.canvas.GetComponent<RectTransform>(),
                Input.mousePosition, null,
                out Vector2 localPos);
            dragGhostImage.rectTransform.anchoredPosition = localPos;
        }
    }

    public void BeginDrag()
    {
        if (_dragSprite == null) return;
        _isDragging = true;
        dragGhostImage.sprite = _dragSprite;
        dragGhostImage.gameObject.SetActive(true);
    }

    public void EndDrag()
    {
        _isDragging = false;
        dragGhostImage.gameObject.SetActive(false);
    }

    // ── Bottom Bar ────────────────────────────────────────

    void OnClearClicked()
    {
        _data.PlacedNotes.Clear();
        _beamedGroupStarts.Clear();
        RefreshAllViews();
    }

    void OnStartClicked()
    {
        if (_data.PlacedNotes.Count == 0)
        {
            ShowRejectFeedback("請先放入音符");
            return;
        }

        var score = FreeComposeConverter.Convert(_data);
        FreeComposeSessionHolder.PendingScore = score;
        UnityEngine.SceneManagement.SceneManager.LoadScene("SC_Game");
    }

    void ShowRejectFeedback(string msg)
    {
        Debug.LogWarning($"[FreeCompose] 拒絕放入：{msg}");
    }
}