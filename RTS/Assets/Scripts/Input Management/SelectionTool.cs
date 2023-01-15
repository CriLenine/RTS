using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class SelectionTool : MonoBehaviour
{
    private Mouse _mouse;
    private Camera _camera;

    private bool _clicking = false;
    private bool _requiresHUDUpdate;

    private float _minimumSelectionArea;

    // graphical
    [SerializeField]
    private RectTransform _boxVisual;
    // logical
    private Rect _selectionBox;
    private Vector2 _startpos, _endpos = Vector2.zero;
    private Vector2 _boxSize, _boxCenter;

    [SerializeField]
    private bool _debug;

    private void Start()
    {
        _mouse = Mouse.current;
        _camera = Camera.main;

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
        _startpos = _mouse.position.ReadValue();

        _clicking = true;
        _selectionBox = new Rect();

    }

    public void ProceedSelection()
    {
        if (!_clicking) return;

        _requiresHUDUpdate = true;

        if (_selectionBox.size.sqrMagnitude < _minimumSelectionArea) // Selection Box is too small : click select
        {
            GameEventsManager.PlayEvent("Click", _camera.ScreenToWorldPoint(_mouse.position.ReadValue()));

            Vector2 worldPoint = _camera.ScreenToWorldPoint(_mouse.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, SelectionManager.Clickable);

            if (hit.collider != null) // if we hit a clickable object
            {
                if (hit.collider.gameObject.TryGetComponent(out Character selectedCharacter)) // Collider = character
                {
                    if (GameManager.MyEntities.Contains(selectedCharacter)) //Test if I am the character owner
                    {
                        if (!InputActionsManager.IsShifting) // Normal click
                        {
                            SelectionManager.DeselectAll();
                            SelectionManager.AddCharacterToSelection(selectedCharacter);
                        }
                        else // Shift click
                        {
                            SelectionManager.UnselectBuilding();

                            if (!SelectionManager.SelectedCharacters.Contains(selectedCharacter)) // If the character is not already selected
                                SelectionManager.AddCharacterToSelection(selectedCharacter);
                            else
                            {
                                SelectionManager.RemoveCharacterFromSelection(selectedCharacter);
                                selectedCharacter.Unselect();
                            }
                        }
                    }
                    else
                    {
                        SelectionManager.DeselectAll();
                        HUDManager.UpdateHUD();
                        HUDManager.DisplayStats(selectedCharacter);
                        _requiresHUDUpdate = false;
                    }
                }
                else if (hit.collider.gameObject.TryGetComponent(out Building selectedBuilding))// Collider = building
                    if (GameManager.MyEntities.Contains(selectedBuilding)) //Test if building owner
                    {
                        if (selectedBuilding != SelectionManager.SelectedBuilding)
                        {
                            SelectionManager.DeselectAll();
                            SelectionManager.SetSelectedBuilding(selectedBuilding);
                        }
                    }
                    else
                    {
                        SelectionManager.DeselectAll();
                        HUDManager.UpdateHUD();
                        HUDManager.DisplayStats(selectedBuilding);
                        _requiresHUDUpdate = false;
                    }
            }
            else
                if (!InputActionsManager.IsShifting) // If we didn't hit anything and shift is not being held
                SelectionManager.DeselectAll();
        }
        else    // Drag Select
        {
            if (!InputActionsManager.IsShifting)
                SelectionManager.DeselectAll();

            foreach (Character character in GameManager.MyCharacters)
                if (_selectionBox.Contains(_camera.WorldToScreenPoint(character.transform.position)))
                    if (!(InputActionsManager.IsShifting && SelectionManager.SelectedCharacters.Contains(character)))
                        SelectionManager.AddCharacterToSelection(character);
        }

        foreach (Character.Type type in SelectionManager.SelectedTypes)
        {
            List<AudioClip> characterClips = DataManager.GetCharacterData(type).OnSelectionAudios;
            if (characterClips.Count == 0)
                continue;
            AudioManager.PlayNewSound(characterClips[(int)(Random.value * (characterClips.Count - 1))]);
        }

        if (SelectionManager.SelectedBuilding != null)
        {
            List<AudioClip> clips = SelectionManager.SelectedBuilding.Data.OnSelectionAudios;
            if (clips.Count > 0)
                AudioManager.PlayNewSound(clips[(int)(Random.value * (clips.Count - 1))]);
        }

        _startpos = Vector2.zero;
        _endpos = Vector2.zero;

        _boxVisual.sizeDelta = new Vector2(0, 0);

        _clicking = false;

        if(_requiresHUDUpdate)
            HUDManager.UpdateHUD();
    }
}
