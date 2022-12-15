using UnityEngine;
using System.Collections.Generic;

public class HUDActions : HUD
{
    [SerializeField]
    private GameObject _characterActions, _defaultBuildingActions, _HeadQuartersActions, _BarracksActions;

    [SerializeField]
    private List<ActionButton> _HeadQuartersDynamicButtons = new List<ActionButton>();
    [SerializeField]
    private List<ActionButton> _BarracksDynamicButtons = new List<ActionButton>();
    
    private List<ActionButton> _dynamicButtons;

    private HashSet<Building.Type> _typesWithActions = new HashSet<Building.Type>
    {
        Building.Type.HeadQuarters,
        Building.Type.Barracks,
    };

    private bool _dynamicUpdate = false;

    public void UpdateActions()
    {
        _dynamicUpdate = false;

        _characterActions.SetActive(true);
        _defaultBuildingActions.SetActive(false);
        _HeadQuartersActions.SetActive(false);
        _BarracksActions.SetActive(false);
    }

    public void UpdateActions(Building.Type type)
    {
        _characterActions.SetActive(false);

        _defaultBuildingActions.SetActive(!_typesWithActions.Contains(type));
        _dynamicUpdate = _typesWithActions.Contains(type);
        _HeadQuartersActions.SetActive(type == Building.Type.HeadQuarters);
        _BarracksActions.SetActive(type == Building.Type.Barracks);

        if (type == Building.Type.HeadQuarters)
            _dynamicButtons = _HeadQuartersDynamicButtons;

        if (type == Building.Type.Barracks)
            _dynamicButtons = _BarracksDynamicButtons;
    }

    private void Update()
    {
        if(HUDSection.activeInHierarchy && _dynamicUpdate)
            foreach (ActionButton button in _dynamicButtons)
                button.SetButtonInteractability(GetInteractabilityValue(button));
    }

    private bool GetInteractabilityValue(ActionButton button)
    {
        SpawnResearchToolTip toolTip = button.ButtonToolTip as SpawnResearchToolTip;
        CharacterData data = toolTip.CharacterData;

        foreach (Resource.Amount cost in data.Cost)
            if (GameManager.MyResources[cost.Type] < cost.Value)
                return false;

        return true;
    }
}
