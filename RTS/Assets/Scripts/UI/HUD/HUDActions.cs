using UnityEngine;
using System.Collections.Generic;

public class HUDActions : HUD
{
    [SerializeField]
    private GameObject _characterActions;
    [SerializeField]
    private GameObject _HeadQuartersActions;
    [SerializeField]
    private GameObject _SawmillActions;

    private HashSet<Building.Type> _typesWithActions = new HashSet<Building.Type>
    {
        Building.Type.HeadQuarters,
        Building.Type.Sawmill
    };

    public void UpdateActions()
    {
        _characterActions.SetActive(true);
        _HeadQuartersActions.SetActive(false);
        _SawmillActions.SetActive(false);
    }

    public void UpdateActions(Building.Type type)
    {
        if (!_typesWithActions.Contains(type))
        {
            Hide();
            return;
        }

        _characterActions.SetActive(false);

        _HeadQuartersActions.SetActive(type == Building.Type.HeadQuarters);
        _SawmillActions.SetActive(type == Building.Type.Sawmill);
    }
}
