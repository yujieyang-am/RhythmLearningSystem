using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FreeComposeController : MonoBehaviour
{
    [Header("頂部參數")]
    [SerializeField] private TMP_Dropdown dropdownBpm;
    [SerializeField] private TMP_Dropdown dropdownTimeSignature;
    [SerializeField] private TMP_Dropdown dropdownMeasureCount;
    [SerializeField] private TMP_Dropdown dropdownRepeatCount;

    [Header("格子區")]
    [SerializeField] private RectTransform gridContent;
    [SerializeField] private float cellWidth  = 30f;
    [SerializeField] private float cellHeight = 60f;
    [SerializeField] private float measureLabelWidth = 60f;

    [Header("音符選板按鈕")]
    [SerializeField] private Button btnWhole;
    [SerializeField] private Button btnHalf;
    [SerializeField] private Button btnDottedHalf;
    [SerializeField] private Button btnQuarter;
    [SerializeField] private Button btnDottedQuarter;
    [SerializeField] private Button btnEighth;
    [SerializeField] private Button btnDottedEighth;
    [SerializeField] private Button btnSixteenth;
    [SerializeField] private Button btnDottedSixteenth;
    [SerializeField] private Button btnThirtySecond;
    [SerializeField] private Button btnBeamedEighth2;
    [SerializeField] private Button btnBeamedEighth3;
    [SerializeField] private Button btnBeamedEighth4;
    [SerializeField] private Button btnBeamedSixteenth2;
    [SerializeField] private Button btnBeamedSixteenth4;
    [SerializeField] private Button btnBeamedThirtySecond2;
    [SerializeField] private Button btnTriplet;
    [SerializeField] private Button btnRestQuarter;
    [SerializeField] private Button btnRestEighth;
    [SerializeField] private Button btnEraser;

    [Header("音符 Sprites（選板顯示用）")]
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
    [SerializeField] private Sprite spriteBeamedEighth2;
    [SerializeField] private Sprite spriteBeamedEighth3;
    [SerializeField] private Sprite spriteBeamedEighth4;
    [SerializeField] private Sprite spriteBeamedSixteenth2;
    [SerializeField] private Sprite spriteBeamedSixteenth4;
    [SerializeField] private Sprite spriteBeamedThirtySecond2;
    [SerializeField] private Sprite spriteTriplet;
    [SerializeField] private Sprite spriteRestQuarter;
    [SerializeField] private Sprite spriteRestEighth;

    [Header("底部按鈕")]
    [SerializeField] private Button btnClear;
    [SerializeField] private Button btnStartPractice;
    [SerializeField] private Button btnPreview;

    [Header("格子 Prefab")]
    [SerializeField] private Button cellPrefab;

    private FreeComposeData currentData = new FreeComposeData();
    private NoteType? selectedNoteType  = null;
    private bool isEraserMode           = false;

    private Dictionary<string, Button> cellButtons = new Dictionary<string, Button>();

    private readonly int[] bpmOptions = { 60, 70, 80, 90, 100, 120 };

    private void Start()
    {
        SetupDropdowns();
        SetupNoteButtons();
        SetupBottomButtons();
        RebuildGrid();
    }

    private void SetupDropdowns()
    {
        dropdownBpm.ClearOptions();
        var bpmList = new List<string>();
        foreach (var b in bpmOptions) bpmList.Add(b.ToString());
        dropdownBpm.AddOptions(bpmList);
        dropdownBpm.value = 2;
        dropdownBpm.onValueChanged.AddListener(_ => OnSettingChanged());

        dropdownTimeSignature.ClearOptions();
        dropdownTimeSignature.AddOptions(new List<string> { "4/4", "3/4", "6/8" });
        dropdownTimeSignature.onValueChanged.AddListener(_ => OnSettingChanged());

        dropdownMeasureCount.ClearOptions();
        dropdownMeasureCount.AddOptions(new List<string> { "1", "2", "4", "8" });
        dropdownMeasureCount.value = 1;
        dropdownMeasureCount.onValueChanged.AddListener(_ => OnSettingChanged());

        dropdownRepeatCount.ClearOptions();
        dropdownRepeatCount.AddOptions(new List<string> { "1", "2", "3", "4" });
        dropdownRepeatCount.onValueChanged.AddListener(_ => OnSettingChanged());
    }

    private void OnSettingChanged()
    {
        currentData.Bpm             = bpmOptions[dropdownBpm.value];
        currentData.BeatsPerMeasure = GetBeatsPerMeasure();
        currentData.MeasureCount    = GetMeasureCount();
        currentData.RepeatCount     = dropdownRepeatCount.value + 1;
        RebuildGrid();
    }

    private int GetBeatsPerMeasure()
    {
        return dropdownTimeSignature.value switch
        {
            1 => 3,
            2 => 6,
            _ => 4,
        };
    }

    private int GetMeasureCount()
    {
        return dropdownMeasureCount.value switch
        {
            0 => 1,
            1 => 2,
            2 => 4,
            _ => 8,
        };
    }

    private void SetupNoteButtons()
    {
        SetupNoteBtn(btnWhole,               NoteType.Whole,          spriteWhole);
        SetupNoteBtn(btnHalf,                NoteType.Half,           spriteHalf);
        SetupNoteBtn(btnDottedHalf,          NoteType.DottedHalf,     spriteDottedHalf);
        SetupNoteBtn(btnQuarter,             NoteType.Quarter,        spriteQuarter);
        SetupNoteBtn(btnDottedQuarter,       NoteType.DottedQuarter,  spriteDottedQuarter);
        SetupNoteBtn(btnEighth,              NoteType.Eighth,         spriteEighth);
        SetupNoteBtn(btnDottedEighth,        NoteType.DottedEighth,   spriteDottedEighth);
        SetupNoteBtn(btnSixteenth,           NoteType.Sixteenth,      spriteSixteenth);
        SetupNoteBtn(btnDottedSixteenth,     NoteType.DottedSixteenth,spriteDottedSixteenth);
        SetupNoteBtn(btnThirtySecond,        NoteType.ThirtySecond,   spriteThirtySecond);
        SetupNoteBtn(btnBeamedEighth2,       NoteType.Eighth,         spriteBeamedEighth2);
        SetupNoteBtn(btnBeamedEighth3,       NoteType.Eighth,         spriteBeamedEighth3);
        SetupNoteBtn(btnBeamedEighth4,       NoteType.Eighth,         spriteBeamedEighth4);
        SetupNoteBtn(btnBeamedSixteenth2,    NoteType.Sixteenth,      spriteBeamedSixteenth2);
        SetupNoteBtn(btnBeamedSixteenth4,    NoteType.Sixteenth,      spriteBeamedSixteenth4);
        SetupNoteBtn(btnBeamedThirtySecond2, NoteType.ThirtySecond,   spriteBeamedThirtySecond2);
        SetupNoteBtn(btnTriplet,             NoteType.TripletEighth,  spriteTriplet);
        SetupNoteBtn(btnRestQuarter,         NoteType.RestQuarter,    spriteRestQuarter);
        SetupNoteBtn(btnRestEighth,          NoteType.RestEighth,     spriteRestEighth);

        btnEraser.onClick.AddListener(() =>
        {
            isEraserMode     = true;
            selectedNoteType = null;
        });
    }

    private void SetupNoteBtn(Button btn, NoteType noteType, Sprite sprite)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img != null && sprite != null) img.sprite = sprite;
        btn.onClick.AddListener(() =>
        {
            selectedNoteType = noteType;
            isEraserMode     = false;
        });
    }

    private void SetupBottomButtons()
    {
        btnClear.onClick.AddListener(OnClearClicked);
        btnStartPractice.onClick.AddListener(OnStartPracticeClicked);
        btnPreview.onClick.AddListener(OnPreviewClicked);
    }

    private void OnClearClicked()
    {
        currentData.Cells.Clear();
        RefreshAllCells();
    }

    private void OnStartPracticeClicked()
    {
        if (currentData.Cells.Count == 0)
        {
            Debug.LogWarning("請先放入至少一個音符");
            return;
        }
        var score = FreeComposeConverter.Convert(currentData);
        FreeComposeSessionHolder.PendingScore = score;
        FreeComposeSessionHolder.SourceData   = currentData;
        SceneManager.LoadScene("SC_Game");
    }

    private void OnPreviewClicked()
    {
        Debug.Log("Preview - 待實作");
    }

    private void RebuildGrid()
    {
        foreach (Transform child in gridContent)
            Destroy(child.gameObject);
        cellButtons.Clear();

        float totalHeight = (cellHeight + 10f) * currentData.MeasureCount;
        float totalWidth  = measureLabelWidth + cellWidth * ScoreConverter.GRID_RESOLUTION;
        gridContent.sizeDelta = new Vector2(totalWidth, totalHeight);

        for (int m = 0; m < currentData.MeasureCount; m++)
        {
            float yOffset = -(m * (cellHeight + 10f)) - cellHeight / 2f;

            // 小節標籤
            var label = new GameObject("Label_" + m);
            label.transform.SetParent(gridContent, false);
            var labelRect = label.AddComponent<RectTransform>();
            var labelText = label.AddComponent<TextMeshProUGUI>();
            labelRect.sizeDelta        = new Vector2(measureLabelWidth, cellHeight);
            labelRect.anchorMin        = new Vector2(0, 1f);
            labelRect.anchorMax        = new Vector2(0, 1f);
            labelRect.anchoredPosition = new Vector2(measureLabelWidth / 2f, yOffset);
            labelText.text      = $"M{m + 1}";
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.fontSize  = 14;

            // 32 個格子
            for (int g = 0; g < ScoreConverter.GRID_RESOLUTION; g++)
            {
                string key  = $"{m}-{g}";
                var    cell = Instantiate(cellPrefab, gridContent);
                var    rect = cell.GetComponent<RectTransform>();

                rect.sizeDelta        = new Vector2(cellWidth, cellHeight);
                rect.anchorMin        = new Vector2(0, 1f);
                rect.anchorMax        = new Vector2(0, 1f);
                rect.anchoredPosition = new Vector2(
                    measureLabelWidth + g * cellWidth + cellWidth / 2f,
                    yOffset
                );

                cell.GetComponent<Image>().color = (g % 8 == 0)
                    ? new Color(0.7f, 0.7f, 0.9f)
                    : new Color(0.9f, 0.9f, 0.9f);

                string capturedKey = key;
                cell.onClick.AddListener(() => OnCellClicked(capturedKey));
                cellButtons[key] = cell;
            }
        }

        RefreshAllCells();
    }

    private void OnCellClicked(string key)
    {
        if (isEraserMode)
        {
            currentData.Cells.Remove(key);
            UpdateCellVisual(key);
            return;
        }

        if (selectedNoteType == null) return;

        var parts        = key.Split('-');
        int measureIndex = int.Parse(parts[0]);
        int gridIndex    = int.Parse(parts[1]);
        int span         = ScoreConverter.GetGridSpan(selectedNoteType.Value);

        if (gridIndex + span > ScoreConverter.GRID_RESOLUTION)
        {
            Debug.LogWarning("音符超出小節範圍");
            return;
        }

        currentData.Cells[key] = selectedNoteType.Value;
        UpdateCellVisual(key);
    }

    private void RefreshAllCells()
    {
        foreach (var key in cellButtons.Keys)
            UpdateCellVisual(key);
    }

    private void UpdateCellVisual(string key)
    {
        if (!cellButtons.TryGetValue(key, out Button cell)) return;

        var img = cell.GetComponent<Image>();

        if (currentData.Cells.TryGetValue(key, out NoteType noteType))
            img.color = GetNoteColor(noteType);
        else
        {
            var parts = key.Split('-');
            int g     = int.Parse(parts[1]);
            img.color = (g % 8 == 0)
                ? new Color(0.7f, 0.7f, 0.9f)
                : new Color(0.9f, 0.9f, 0.9f);
        }
    }

    private Color GetNoteColor(NoteType t)
    {
        if (ScoreConverter.IsRestType(t))
            return new Color(0.7f, 0.7f, 0.7f);
        if (t == NoteType.Whole || t == NoteType.Half || t == NoteType.DottedHalf)
            return new Color(1f, 0.9f, 0.4f);
        if (t == NoteType.Quarter || t == NoteType.DottedQuarter)
            return new Color(0.4f, 0.6f, 1f);
        if (t == NoteType.Eighth || t == NoteType.DottedEighth)
            return new Color(0.4f, 0.9f, 0.5f);
        if (t == NoteType.TripletEighth)
            return new Color(0.9f, 0.5f, 0.9f);
        return new Color(0.4f, 0.85f, 0.85f);
    }
}