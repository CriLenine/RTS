using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;

public class CharacterSelectionManager : MonoBehaviour
{
    /* ---------------------------------------------- */
    /* General Variables                              */
    /* ---------------------------------------------- */
    private static CharacterSelectionManager _instance;
    private CharacterSelection _characterSelectionInputActions;
    private Mouse _mouse;
    private Camera _camera;

    private static HashSet<Character> _charactersList = new HashSet<Character>();
    public static List<Character> charactersSelected = new List<Character>();

    private bool _clicking, _shifting;
    private View _actualView;
    /* ---------------------------------------------- */


    /* ---------------------------------------------- */
    /* Click Selection Variables                      */
    /* ---------------------------------------------- */
    [SerializeField]
    private LayerMask _clickable, _environment;
    /* ---------------------------------------------- */


    /* ---------------------------------------------- */
    /* Drag Selection Variables                       */
    /* ---------------------------------------------- */
    private float _minimumSelectionArea;

    // graphical
    [SerializeField]
    private RectTransform _boxVisual;
    // logical
    private Rect _selectionBox;
    private Vector2 _startpos, _endpos, _boxSize, _boxCenter;
    /* ---------------------------------------------- */

    /* ---------------------------------------------- */
    /* Debugging Variables                            */
    /* ---------------------------------------------- */
    [SerializeField]
    private bool _debug;
    /* ---------------------------------------------- */


    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;
    }


    private void Start()
    {
        _camera = Camera.main;
        _mouse = Mouse.current;

        _startpos = Vector2.zero;
        _endpos = Vector2.zero;

        _clicking = false;
        _shifting = false;

        // conversion of a cell size in the canvas' referential to set the min selection area to that of the size of a cell
        _minimumSelectionArea = (_camera.WorldToScreenPoint(new Vector3(TileMapManager._tileSize, 0, 0)) -
            _camera.WorldToScreenPoint(new Vector3(0, 0, 0))).sqrMagnitude;

        _characterSelectionInputActions = new CharacterSelection();
        _characterSelectionInputActions.Enable();
        _characterSelectionInputActions.Selection.Click.started += InitSelection;
        _characterSelectionInputActions.Selection.Click.canceled += ProceedSelection;
        _characterSelectionInputActions.Selection.Shift.started += _ => _shifting = true;
        _characterSelectionInputActions.Selection.Shift.canceled += _ => _shifting = false;
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

    private void InitSelection(InputAction.CallbackContext _)
    {
        if (EventSystem.current.IsPointerOverGameObject()) 
            return;

        _startpos = _mouse.position.ReadValue();
        _clicking = true;
        _selectionBox = new Rect();
    }


    private void ProceedSelection(InputAction.CallbackContext _)
    {
        if (!_clicking) return;

        if (_selectionBox.size.sqrMagnitude < _minimumSelectionArea ) // Selection Box is too small : click select
        {
            Vector2 worldPoint = _camera.ScreenToWorldPoint(_mouse.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, _clickable);

            if (hit.collider != null) // if we hit a clickable object
            {
                if(hit.collider.gameObject.TryGetComponent(out Character selectedCharacter))
                {
                    if (!_shifting) // Normal click
                    {
                        DeselectAll();

                        //CharaUIActivation
                        _actualView = ViewManager.Show<PeonView>(); //-----------------------CHANGE TO GENERIC

                        charactersSelected.Add(selectedCharacter);
                        selectedCharacter.SelectionMarker.SetActive(true);

                        if (_instance._debug)
                            selectedCharacter.DebugCoordinates();
                    }
                    else    // Shift click
                    {
                        if (!charactersSelected.Contains(selectedCharacter)) // If the character is not already selected
                        {
                            charactersSelected.Add(selectedCharacter);
                            selectedCharacter.SelectionMarker.SetActive(true);
                        }
                        else
                        {
                            charactersSelected.Remove(selectedCharacter);
                            selectedCharacter.SelectionMarker.SetActive(false);
                        }
                    }
                }
               
            }
            else
                if (!_shifting) // If we didn't hit anything and shift is not being held
                    DeselectAll();
        }
        else    // Drag Select
        {
            if (!_shifting)
                DeselectAll();

            foreach (Character character in _charactersList)
            {
                if (_selectionBox.Contains(_camera.WorldToScreenPoint(character.transform.position)))
                {
                    if (!(_shifting && charactersSelected.Contains(character)))
                    {
                        charactersSelected.Add(character);
                        character.SelectionMarker.SetActive(true);
                    }
                }
            }



            if(charactersSelected.Count > 0)
            {

                //CharaUIActivation
                _actualView = ViewManager.Show<PeonView>(); //-----------------------CHANGE TO GENERIC
            }

        }

        _startpos = Vector2.zero;
        _endpos = Vector2.zero;

        _boxVisual.sizeDelta = new Vector2(0, 0);

        _clicking = false;
    }


    private static void DeselectAll()
    {
        if (_instance._actualView != null)
            _instance._actualView.Hide();

        foreach (Character characterToRemove in charactersSelected)
            characterToRemove.SelectionMarker.SetActive(false);
        charactersSelected.Clear();
    }


    public static void AddCharacter(Character characterToAdd) // Every time a new character is created
    {
        _charactersList.Add(characterToAdd);
    }


    public static void RemoveCharacter(Character characterToRemove) // Every time a character dies
    {
        _charactersList.Remove(characterToRemove);
    }

}
