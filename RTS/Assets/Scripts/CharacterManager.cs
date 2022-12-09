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

    private CharacterSelection _characterSelectionInputActions;
    private Locomotion _locomotionInputActions;

    //* Character Selection *//

    private Character.Type _selectedType = Character.Type.None;
    private HashSet<Character.Type> _selectedTypes = new HashSet<Character.Type>();

    private List<Character> _selectedCharacters = new List<Character>();
    private HashSet<Character> _allSelectedCharacters = new HashSet<Character>();

    public static Character.Type SelectedType => _instance._selectedType;
    public static HashSet<Character.Type> SelectedTypes => _instance._selectedTypes;

    public static List<Character> SelectedCharacters => _instance._selectedCharacters;

    public delegate void OnCharacterSelectionUpdatedHandler();

    private event OnCharacterSelectionUpdatedHandler _onCharacterSelectionUpdated;

    public static event OnCharacterSelectionUpdatedHandler OnCharacterSelectionUpdated
    {
        add => _instance._onCharacterSelectionUpdated += value;
        remove => _instance._onCharacterSelectionUpdated -= value;
    }

    //* Building Selection *//

    private Building _buildingSelected = null;

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

        _characterSelectionInputActions = new CharacterSelection();
        _characterSelectionInputActions.Enable();
        _characterSelectionInputActions.Selection.Click.started += _ => _selectionManager.InitSelection();
        _characterSelectionInputActions.Selection.Click.canceled += _ => _selectionManager.ProceedSelection();
        _characterSelectionInputActions.Selection.Shift.started += _ => _selectionManager._shifting = true;
        _characterSelectionInputActions.Selection.Shift.canceled += _ => _selectionManager._shifting = false;

        _locomotionInputActions = new Locomotion();
        _locomotionInputActions.Enable();
        _locomotionInputActions.Displacement.RightClick.performed += _ => GiveOrder(); ;
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

    public static bool Move(Character character, Vector2 position)
    {
        return _instance._locomotionManager.Move(character, position);
    }

    public void GiveOrder()
    {
        if (SelectedCharacters.Count == 0)
            return;

        // Retrieve the rallypoint's coordinates according to the input.

        Vector3 worldMousePos = _camera.ScreenToWorldPoint(_mouse.position.ReadValue());
        Vector2Int rallyPointCoords = TileMapManager.WorldToTilemapCoords(worldMousePos);

        LogicalTile rallyTile = TileMapManager.GetLogicalTile(rallyPointCoords);

        if (SelectedCharacters.Count == 1 &&
            (GameManager.ResourcesManager.HasRock(rallyPointCoords) || GameManager.ResourcesManager.HasTree(rallyPointCoords)))
        {
            NetworkManager.Input(TickInput.Harvest(rallyPointCoords, SelectedCharacters[0].ID));
            return;
        }

        if (rallyTile == null || !rallyTile.IsFree(NetworkManager.Me))
            return;

        int[] IDs = new int[SelectedCharacters.Count];

        for (int i = 0; i < SelectedCharacters.Count; ++i)
            IDs[i] = SelectedCharacters[i].ID;

        NetworkManager.Input(TickInput.Move(IDs, worldMousePos));
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

        _onCharacterSelectionUpdated();
    }

    #endregion

    public static void AddBuildingToSelected(Building building)
    {
        _instance._buildingSelected = building;
    }

    public static Building SelectedBuilding()
    {
        return _instance._buildingSelected;
    }

    public static void TestEntitieSelection(TickedBehaviour entitie)
    {
        if (entitie is Character character)
        {
            if (_instance._selectedCharacters.Contains(character))
                RemoveCharacterFromSelection(character);
        }
        else if (entitie is Building building && _instance._buildingSelected)
        {
            if (_instance._buildingSelected == building)
                DeselectAll();
        }
    }
    public static void DeselectAll()
    {
        _instance._buildingSelected = null;

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
