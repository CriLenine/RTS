using UnityEngine;
using System.Collections.Generic;

public class HUDActions : HUD
{
    [SerializeField]
    private List<ActionButton> _actionButtons = new List<ActionButton>();

    [SerializeField]
    private List<ActionData> _characterActions = new List<ActionData>();

    public void ShowCharacterActions()
    {
        for (int i = 0; i < _actionButtons.Count; i++)
            if (_characterActions[i] != null)
                _actionButtons[i].SetupButton(_characterActions[i]);
            else
                _actionButtons[i].ResetButton();
    }

    public void ShowBuildingActions(BuildingData data)
    {
        for (int i = 0; i < data.BuildingActions.Count; i++)
            if (data.BuildingActions[i] != null)
                _actionButtons[i].SetupButton(data.BuildingActions[i]);
            else
                _actionButtons[i].ResetButton();
    }
}
