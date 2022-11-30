using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(SelectionManager))]
[RequireComponent(typeof(LocomotionManager))]
public class CharacterManager : MonoBehaviour
{
    private static CharacterManager _instance;

    private SelectionManager _selectionManager;
    private LocomotionManager _locomotionManager;

    private CharacterSelection _characterSelectionInputActions;
    private Locomotion _locomotionInputActions;

    private List<Character> _charactersSelected = new List<Character>();
    private Building _buildingSelected = null;

    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;
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
        _locomotionInputActions.Displacement.RightClick.performed += _ => _locomotionManager.RallySelectedCharacters(); ;
    }

    public static bool Move(Character character, Vector2 position)
    {
        return _instance._locomotionManager.Move(character, position);
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

    public static bool AddBuildingToSelected(Building building)
    {
        if (!_instance._buildingSelected)
        {
            _instance._buildingSelected = building;
            return true;
        }
        else return false;
    }

    public static void AddCharacterToSelection(Character character)
    {
        _instance._charactersSelected.Add(character);
    }

    public static void AddCharactersToSelection(List<Character> characters)
    {
        _instance._charactersSelected.AddRange(characters);
    }

    public static void RemoveCharacterFromSelection(Character character)
    {
        _instance._charactersSelected.Remove(character);
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
