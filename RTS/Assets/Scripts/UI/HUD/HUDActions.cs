using UnityEngine;
using System.Collections.Generic;

public class HUDActions : HUD
{
    [SerializeField]
    private List<ActionButton> _actionButtons = new List<ActionButton>();

    [SerializeField]
    private List<ButtonDataHUDParameters> _characterActions, _blueprintActions = new List<ButtonDataHUDParameters>();

    public void ShowCharacterActions()
    {
        ShowActions(_characterActions);
    }

    public void ShowBuildingActions(Building building)
    {
        ShowActions(building.BuildComplete ? building.Data.Actions : _blueprintActions);
    }

    private void ShowActions(List<ButtonDataHUDParameters> actions)
    {
        List<ActionButton> toReset = new List<ActionButton>(_actionButtons);

        foreach (ButtonDataHUDParameters parameters in actions)
        {
            int index = parameters.ButtonPosition.x + parameters.ButtonPosition.y * 5;
            _actionButtons[index].SetupButton(parameters.ButtonData);
            toReset.Remove(_actionButtons[index]);
        }

        foreach (ActionButton buttonToReset in toReset)
            buttonToReset.ResetButton();
    }

    public void UnToggleActionButtons()
    {
        foreach (ActionButton button in _actionButtons)
        {
            button.UntoggleButton();
        }
    }
}
