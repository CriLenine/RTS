using UnityEngine;
using UnityEngine.InputSystem;

public class UnitDrag : MonoBehaviour
{
    // graphical
    [SerializeField]
    private RectTransform _boxVisual;

    private UnitSelection _unitSelection;

    // logical
    private Rect _selectionBox;

    private Vector2 _startpos, _endpos, _boxSize, _boxCenter;

    private bool _dragging, _shifting;

    private void Start()
    {
        _startpos = Vector2.zero;
        _endpos = Vector2.zero;
        DrawVisual();

        _dragging = false;
        _shifting = false;

        _unitSelection = new UnitSelection();
        _unitSelection.Enable();
        _unitSelection.Selection.DragSelect.started += InitVisualStartPos;
        _unitSelection.Selection.DragSelect.canceled += Reset;
        _unitSelection.Selection.ShiftSelect.started += Shifting;
        _unitSelection.Selection.ShiftSelect.canceled += NotShifting;
    }
    private void Update()
    {
        if (_dragging)
        {
            _endpos = Mouse.current.position.ReadValue();
            DrawVisual();
            DrawSelection();
        }
    }

    private void InitVisualStartPos(InputAction.CallbackContext ctx)
    {
        _startpos = Mouse.current.position.ReadValue();
        _dragging = true;
        _selectionBox = new Rect();
    }
    private void DrawVisual()
    {
        _boxCenter = (_startpos + _endpos) / 2.0f;
        _boxVisual.position = _boxCenter;

        _boxSize = new Vector2(Mathf.Abs(_startpos.x - _endpos.x), Mathf.Abs(_startpos.y - _endpos.y));
        _boxVisual.sizeDelta = _boxSize;
    }
    private void Reset(InputAction.CallbackContext ctx)
    {
        UnitSelectionManager.DragSelect(_selectionBox, _shifting);

        _startpos = Vector2.zero;
        _endpos = Vector2.zero;

        DrawVisual();

        _dragging = false;
    }
    private void DrawSelection()
    {
        Vector2 extents = _boxSize / 2.0f;
        _selectionBox.min = _boxCenter - extents;
        _selectionBox.max = _boxCenter + extents;
    }
    private void Shifting(InputAction.CallbackContext ctx)
    {
        _shifting = true;
    }
    private void NotShifting(InputAction.CallbackContext ctx)
    {
        _shifting = false;
    }
}
