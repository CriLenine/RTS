using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

[RequireComponent(typeof(SelectionManager))]
[RequireComponent(typeof(LocomotionManager))]
public class CharacterManager : MonoBehaviour
{
    private static CharacterManager _instance;

    #region Variables

    private Camera _camera;
    private Mouse _mouse;

    private SelectionManager _selectionManager;
    private LocomotionManager _locomotionManager;

    private CharacterInputs _characterInputActions;
    private Locomotion _locomotionInputActions;

    //* Character Selection *//

    private Character.Type _selectedType = Character.Type.None;
    private HashSet<Character.Type> _selectedTypes = new HashSet<Character.Type>();

    private List<Character> _selectedCharacters = new List<Character>();
    private HashSet<Character> _allSelectedCharacters = new HashSet<Character>();

    private Building _selectedBuilding;

    public static Character.Type SelectedType => _instance._selectedType;
    public static HashSet<Character.Type> SelectedTypes => _instance._selectedTypes;

    public static List<Character> SelectedCharacters => _instance._selectedCharacters;
    public static Building SelectedBuilding => _instance._selectedBuilding;

    public delegate void OnCharacterSelectionUpdatedHandler();

    private event OnCharacterSelectionUpdatedHandler _onCharacterSelectionUpdated;

    public static event OnCharacterSelectionUpdatedHandler OnCharacterSelectionUpdated
    {
        add => _instance._onCharacterSelectionUpdated += value;
        remove => _instance._onCharacterSelectionUpdated -= value;
    }

    #endregion

    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;

        _mouse = Mouse.current;
        _camera = Camera.main;
    }

    private void Start()
    {
        _selectionManager = GetComponent<SelectionManager>();
        _locomotionManager = GetComponent<LocomotionManager>();

        _characterInputActions = new CharacterInputs();
        _characterInputActions.Selection.Click.started += _ => _selectionManager.InitSelection();
        _characterInputActions.Selection.Click.canceled += _ => _selectionManager.ProceedSelection();
        _characterInputActions.Selection.Shift.started += _ => _selectionManager._shifting = true;
        _characterInputActions.Selection.Shift.canceled += _ => _selectionManager._shifting = false;

        _locomotionInputActions = new Locomotion();
        _locomotionInputActions.Displacement.RightClick.performed += _ => _selectionManager.GiveOrder();

        EnableInputs();
    }

    private void Update()
    {
        #region Debug

        if (Input.GetKeyDown(KeyCode.F5))
            _debug = !_debug;

        #endregion
    }

    #region Debug

    private bool _debug = true;

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !Application.isEditor)
            return;

        if (!_debug)
            return;
    }

    private void OnGUI()
    {
        if (!Application.isPlaying || !Application.isEditor)
            return;

        if (!_debug)
            return;

        const int lineHeight = 15;

        int lineCount = SelectedTypes.Count + 1;

        if (lineCount < 1)
            return;

        GUILayout.BeginArea(new Rect(Screen.width - 50 - 10, 10, 50, lineHeight * (3 * lineCount + 2) / 2), new GUIStyle(GUI.skin.box));

        void AddButton(Character.Type type)
        {
            GUIStyle style = GUI.skin.button;

            Color color = style.normal.textColor;

            style.normal.textColor = type == _selectedType ? Color.green : color;

            if (GUILayout.Button(type.ToString()))
                SpecializesSelection(type);

            style.normal.textColor = color;
        }

        AddButton(Character.Type.None);

        foreach (Character.Type type in SelectedTypes)
            AddButton(type);

        GUILayout.EndArea();
    }

    #endregion Debug

    public static void EnableInputs()
    {
        _instance._characterInputActions.Enable();
        _instance._locomotionInputActions.Enable();
    }

    public static void DisableInputs()
    {
        _instance._characterInputActions.Disable();
        _instance._locomotionInputActions.Disable();
    }
    public static bool Move(Character character, Vector2 position)
    {
        return _instance._locomotionManager.Move(character, position);
    }

    public static void ChangeView<T>(T owner) where T : TickedBehaviour
    {
        UIManager.ShowTickedBehaviourUI(owner);
    }

    #region Character Selection

    public static void SpecializesSelection(Character.Type type)
    {
        _instance._selectedType = type;

        _instance.UpdateCharacterSelection();
    }

    public static void AddCharacterToSelection(Character character)
    {
        _instance._allSelectedCharacters.Add(character);

        SpecializesSelection(Character.Type.None);
    }

    public static void AddCharactersToSelection(List<Character> characters)
    {
        for (int i = 0; i < characters.Count; ++i)
            _instance._allSelectedCharacters.Add(characters[i]);

        SpecializesSelection(Character.Type.None);
    }

    public static void RemoveCharacterFromSelection(Character character)
    {
        _instance._allSelectedCharacters.Remove(character);

        _instance.UpdateCharacterSelection();
    }

    private void UpdateCharacterSelection()
    {
        foreach (Character character in GameManager.MyCharacters)
            character.SelectionMarker.SetActive(false);

        SelectedTypes.Clear();
        SelectedCharacters.Clear();

        foreach (Character character in _instance._allSelectedCharacters)
        {
            SelectedTypes.Add(character.CharaType);

            if (_instance._selectedType == Character.Type.None || character.CharaType == _instance._selectedType)
            {
                SelectedCharacters.Add(character);

                character.SelectionMarker.SetActive(true);
            }
        }

        _onCharacterSelectionUpdated?.Invoke();
    }

    #endregion

    public static void SetSelectedBuilding(Building building)
    {
        DeselectAll();
        _instance._selectedBuilding = building;
        building.Select();
    }

    public static void TestEntitieSelection(TickedBehaviour entity)
    {
        if (entity is Character character)
        {
            if (SelectedCharacters.Contains(character))
                RemoveCharacterFromSelection(character);
        }
        else if (entity is Building building && _instance._selectedBuilding != null)
        {
            if (_instance._selectedBuilding == building)
                DeselectAll();
        }
    }
    public static void DeselectAll()
    {
        if(_instance._selectedBuilding != null)
            _instance._selectedBuilding.Unselect();

        _instance._selectedBuilding = null;

        _instance._allSelectedCharacters.Clear();

        _instance.UpdateCharacterSelection();
    }

    public static int[] GetSelectedIds()
    {
        int[] ids = new int[_instance._selectedCharacters.Count];

        for (int i = 0; i < ids.Length; ++i)
            ids[i] = _instance._selectedCharacters[i].ID;

        return ids;
    }

}
