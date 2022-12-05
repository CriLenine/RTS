using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SelectionManager))]
[RequireComponent(typeof(LocomotionManager))]
public class CharacterManager : MonoBehaviour
{
    private static CharacterManager _instance;

    private Camera _camera;
    private Mouse _mouse;

    private SelectionManager _selectionManager;
    public LocomotionManager _locomotionManager;

    private CharacterSelection _characterSelectionInputActions;
    private Locomotion _locomotionInputActions;

    private List<Character> _charactersSelected = new();
    private Building _buildingSelected = null;

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

    public static bool Move(Character character, Vector2 position)
    {
        return _instance._locomotionManager.Move(character, position);
    }

    public void GiveOrder()
    {
        List<Character> characters = SelectedCharacters();

        if (characters.Count == 0)
            return;

        // Retrieve the rallypoint's coordinates according to the input.

        Vector3 worldMousePos = _camera.ScreenToWorldPoint(_mouse.position.ReadValue());
        Vector2Int rallyPointCoords = TileMapManager.WorldToTilemapCoords(worldMousePos);

        LogicalTile rallyTile = TileMapManager.GetLogicalTile(rallyPointCoords);

        if (characters.Count == 1 &&
            (GameManager.RessourcesManager.HasRock(rallyPointCoords) || GameManager.RessourcesManager.HasTree(rallyPointCoords)))
        {
            NetworkManager.Input(TickInput.Harvest(rallyPointCoords, characters[0].ID));
            return;
        }

        if (rallyTile == null || !rallyTile.IsFree(NetworkManager.Me))
            return;

        int[] IDs = new int[characters.Count];

        for (int i = 0; i < characters.Count; ++i)
            IDs[i] = characters[i].ID;

        NetworkManager.Input(TickInput.Move(IDs, worldMousePos));
    }

    public static void ChangeView<T>(T owner) where T : TickedBehaviour
    {
        UIManager.ShowTickedBehaviourUI(owner);
    }

    public static List<Character> SelectedCharacters()
    {
        return _instance._charactersSelected;
    }
    public static Building SelectedBuilding()
    {
        return _instance._buildingSelected;
    }

    public static void AddBuildingToSelected(Building building)
    {
        _instance._buildingSelected = building;
    }

    public static void AddCharacterToSelection(Character character)
    {
        _instance._charactersSelected.Add(character);
        character.SelectionMarker.SetActive(true);
    }

    public static void AddCharactersToSelection(List<Character> characters)
    {
        _instance._charactersSelected.AddRange(characters);

        foreach (var chara in characters)
        {
            chara.SelectionMarker.SetActive(true);
        }
    }

    public static void RemoveCharacterFromSelection(Character character)
    {
        _instance._charactersSelected.Remove(character);
        character.SelectionMarker.SetActive(false);
    }

    public static void TestEntitieSelection(TickedBehaviour entitie)
    {
        if (entitie is Character character)
        {
            if (_instance._charactersSelected.Contains(character))
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
        UIManager.HideCurrentUI();

        _instance._buildingSelected = null;

        foreach (Character characterToRemove in _instance._charactersSelected)
            characterToRemove.SelectionMarker.SetActive(false);
        _instance._charactersSelected.Clear();
    }

    public static int[] GetSelectedIds()
    {
        int[] ids = new int[_instance._charactersSelected.Count];

        for (int i = 0; i < ids.Length; ++i)
            ids[i] = _instance._charactersSelected[i].ID;

        return ids;
    }

}
