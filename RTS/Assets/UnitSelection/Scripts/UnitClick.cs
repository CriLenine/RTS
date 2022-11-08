using UnityEngine;
using UnityEngine.InputSystem;

public class UnitClick : MonoBehaviour
{
    private Camera _camera;

    [SerializeField]
    private LayerMask _clickable, _environment;

    private UnitSelection _unitSelection;

    private bool _shifting;

    private void Start()
    {
        _camera = Camera.main;
        _shifting = false;
        _unitSelection = new UnitSelection();
        _unitSelection.Enable();
        _unitSelection.Selection.ClickSelect.performed += Clicked;
        _unitSelection.Selection.ShiftSelect.started += Shifting;
        _unitSelection.Selection.ShiftSelect.canceled += NotShifting;
    }

    private void Shifting(InputAction.CallbackContext ctx)
    {
        _shifting = true;
    }
    private void NotShifting(InputAction.CallbackContext ctx)
    {
        _shifting = false;
    }

    private void Clicked(InputAction.CallbackContext ctx)
    {
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, _clickable);

        if (hit.collider != null)
        {
            // if we hit a clickable object

            if (!_shifting)
                // normal click
                UnitSelectionManager.ClickSelect(hit.collider.gameObject);
            else
                // shift click
                UnitSelectionManager.ShiftClickSelect(hit.collider.gameObject);
        }
        else
            // if we didn't hit anything and shift is not being held
            if (!_shifting)
                UnitSelectionManager.DeselectAll();
    } 
}
