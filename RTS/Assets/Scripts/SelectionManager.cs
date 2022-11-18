using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionManager : MonoBehaviour
{
    private Mouse _mouse;
    private Camera _camera;

    public bool _shifting;
    private bool _clicking;

   [SerializeField]
    private LayerMask _clickable, _environment;

    private float _minimumSelectionArea;

    // graphical
    [SerializeField]
    private RectTransform _boxVisual;
    // logical
    private Rect _selectionBox;
    private Vector2 _startpos, _endpos, _boxSize, _boxCenter;

    [SerializeField]
    private bool _debug;

    private void Start()
    {
        _camera = Camera.main;
        _mouse = Mouse.current;

        _startpos = Vector2.zero;
        _endpos = Vector2.zero;

        _clicking = false;
        _shifting = false;

        // conversion of a cell size in the canvas' referential to set the min selection area to that of the size of a cell
        _minimumSelectionArea = (_camera.WorldToScreenPoint(new Vector3(TileMapManager.TileSize, 0, 0)) -
            _camera.WorldToScreenPoint(new Vector3(0, 0, 0))).sqrMagnitude;
    }

    private void Update()
    {

        if (_clicking)
        {
            _endpos = _mouse.position.ReadValue();

            // Updates graphical selection Visual
            _boxCenter = (_startpos + _endpos) / 2.0f;
            _boxVisual.position = _boxCenter;
            _boxSize = new Vector2(Mathf.Abs(_startpos.x - _endpos.x), Mathf.Abs(_startpos.y - _endpos.y));
            _boxVisual.sizeDelta = _boxSize;

            // Updates logical selection Rect
            Vector2 extents = _boxSize / 2.0f;
            _selectionBox.min = _boxCenter - extents;
            _selectionBox.max = _boxCenter + extents;
        }
    }

    public void InitSelection()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        _startpos = _mouse.position.ReadValue();
        _clicking = true;
        _selectionBox = new Rect();
    }


    public void ProceedSelection()
    {
        if (!_clicking) return;

        if (_selectionBox.size.sqrMagnitude < _minimumSelectionArea) // Selection Box is too small : click select
        {
            Vector2 worldPoint = _camera.ScreenToWorldPoint(_mouse.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, _clickable);

            if (hit.collider != null) // if we hit a clickable object
            {
                if (hit.collider.gameObject.TryGetComponent(out Character selectedCharacter))
                {
                    if (!_shifting) // Normal click
                    {
                        CharacterManager.DeselectAll();

                        CharacterManager.ChangeView();

                        CharacterManager.SelectedCharacters().Add(selectedCharacter);
                        selectedCharacter.SelectionMarker.SetActive(true);

                        if (_debug)
                            selectedCharacter.DebugCoordinates();
                    }
                    else    // Shift click
                    {
                        if (!CharacterManager.SelectedCharacters().Contains(selectedCharacter)) // If the character is not already selected
                        {
                            CharacterManager.AddCharacterToSelection(selectedCharacter);
                            selectedCharacter.SelectionMarker.SetActive(true);
                        }
                        else
                        {
                            CharacterManager.RemoveCharacterFromSelection(selectedCharacter);
                            selectedCharacter.SelectionMarker.SetActive(false);
                        }
                    }
                }

            }
            else
                if (!_shifting) // If we didn't hit anything and shift is not being held
                    CharacterManager.DeselectAll();
        }
        else    // Drag Select
        {
            if (!_shifting)
                CharacterManager.DeselectAll();

            foreach (Character character in CharacterManager.SelectableCharacters())
            {
                if (_selectionBox.Contains(_camera.WorldToScreenPoint(character.transform.position)))
                {
                    if (!(_shifting && CharacterManager.SelectedCharacters().Contains(character)))
                    {
                        CharacterManager.AddCharacterToSelection(character);
                        character.SelectionMarker.SetActive(true);
                    }
                }
            }

            if (CharacterManager.SelectedCharacters().Count > 0)
                CharacterManager.ChangeView();
        }

        _startpos = Vector2.zero;
        _endpos = Vector2.zero;

        _boxVisual.sizeDelta = new Vector2(0, 0);

        _clicking = false;
    }
}
