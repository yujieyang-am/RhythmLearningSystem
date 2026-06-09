using UnityEngine;
using UnityEngine.EventSystems;

public class PaletteButtonDrag : MonoBehaviour, 
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private FreeComposeController _controller;

    void Awake()
    {
        _controller = GetComponentInParent<FreeComposeController>();
        if (_controller == null)
            _controller = FindObjectOfType<FreeComposeController>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _controller.BeginDrag();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Ghost 跟隨由 FreeComposeController.Update() 處理
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _controller.EndDrag();

        // 找到滑鼠放開位置是哪個 MeasureStaffView
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            var staffView = result.gameObject.GetComponent<MeasureStaffView>();
            if (staffView == null)
                staffView = result.gameObject.GetComponentInParent<MeasureStaffView>();

            if (staffView != null)
            {
                staffView.OnDrop(eventData);
                break;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 點擊 Palette 按鈕只選取，不放音符
        // SelectPalette 已經在 FreeComposeController.InitPalette() 綁定
    }
}