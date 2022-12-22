using UnityEngine;
using System.Collections.Generic;

public class SelectionManager : MonoBehaviour
{
    private static SelectionManager _instance;

    #region Variables

    [SerializeField]
    private SelectionTool _selectionTool;

    [SerializeField]
    private LayerMask _clickable, _environment;
    public static LayerMask Clickable => _instance._clickable;
    public static LayerMask Environment => _instance._environment;

    public static Building SelectedBuilding { get; private set; }
    public static List<Character> SelectedCharacters { get; private set; } = new List<Character>();
    public static Character.Type SelectedType { get; private set; } = Character.Type.All;
    public static HashSet<Character.Type> SelectedTypes { get; private set; } = new HashSet<Character.Type>();


    public delegate void OnCharacterSelectionUpdatedHandler();

    private event OnCharacterSelectionUpdatedHandler _onCharacterSelectionUpdated;

    public static event OnCharacterSelectionUpdatedHandler OnCharacterSelectionUpdated
    {
        add => _instance._onCharacterSelectionUpdated += value;
        remove => _instance._onCharacterSelectionUpdated -= value;
    }

    private HashSet<Character> _allSelectedCharacters = new HashSet<Character>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(_instance);

        _instance = this;
    }

    #endregion

    #region Character Selection

    public static void SpecializesSelection(Character.Type type)
    {
        SelectedType = type;

        _instance.UpdateCharacterSelection();
    }

    public static void AddCharacterToSelection(Character character)
    {
        _instance._allSelectedCharacters.Add(character);

        SpecializesSelection(Character.Type.All);
    }

    public static void AddCharactersToSelection(List<Character> characters)
    {
        for (int i = 0; i < characters.Count; ++i)
            _instance._allSelectedCharacters.Add(characters[i]);

        SpecializesSelection(Character.Type.All);


    }

    public static void RemoveCharacterFromSelection(Character character)
    {
        _instance._allSelectedCharacters.Remove(character);

        _instance.UpdateCharacterSelection();
    }

    private void UpdateCharacterSelection()
    {
        SelectedTypes.Clear();
        SelectedCharacters.Clear();

        foreach (Character character in _instance._allSelectedCharacters)
        {
            SelectedTypes.Add(character.Data.Type);

            if (SelectedType == Character.Type.All || character.Data.Type == SelectedType)
            {
                SelectedCharacters.Add(character);

                character.SelectionMarker.SetActive(true);
            }
        }

        _onCharacterSelectionUpdated?.Invoke();

        InputActionsManager.UpdateGameState(_allSelectedCharacters.Count > 0 ? GameState.CharacterSelection : GameState.None);
    }

    #endregion

    public static void SetSelectedBuilding(Building building)
    {
        DeselectAll();

        SelectedBuilding = building;
        building.Select();

        InputActionsManager.UpdateGameState(GameState.BuildingSelection);
    }

    public static void TestEntitySelection(TickedBehaviour entity)
    {
        if (entity is Character character)
        {
            if (SelectedCharacters.Contains(character))
                RemoveCharacterFromSelection(character);
        }
        else if (entity is Building building && SelectedBuilding != null)
        {
            if (SelectedBuilding == building)
                DeselectAll();
        }
    }
    public static void DeselectAll()
    {
        if (SelectedBuilding != null)
            SelectedBuilding.Unselect();

        SelectedBuilding = null;

        foreach (Character character in _instance._allSelectedCharacters)
            character.SelectionMarker.SetActive(false);

        _instance._allSelectedCharacters.Clear();

        _instance.UpdateCharacterSelection();

        HUDManager.UpdateHUD();
    }

    public static int[] GetSelectedIds()
    {
        int[] ids = new int[SelectedCharacters.Count];

        for (int i = 0; i < ids.Length; ++i)
            ids[i] = SelectedCharacters[i].ID;

        return ids;
    }

    public static void InitSelection()
    {
        _instance._selectionTool.InitSelection();
    }

    public static void ProceedSelection()
    {
        _instance._selectionTool.ProceedSelection();
    }
}