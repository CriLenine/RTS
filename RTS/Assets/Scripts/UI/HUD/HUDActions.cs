using UnityEngine;
using System.Collections.Generic;

public class HUDActions : HUD
{
    [SerializeField]
    private List<ActionButton> _actionButtons = new List<ActionButton>();

    [SerializeField]
    private List<ButtonDataHUDParameters> _characterActions = new List<ButtonDataHUDParameters>();

    public void ShowCharacterActions()
    {
        ResetButtons();

        foreach (ButtonDataHUDParameters parameters in _characterActions)
            _actionButtons[parameters.ButtonPosition.x + parameters.ButtonPosition.y * 5].SetupButton(parameters.ButtonData);
    }

    public void ShowBuildingActions(BuildingData data)
    {
        ResetButtons();

        foreach (ButtonDataHUDParameters parameters in data.Actions)
                _actionButtons[parameters.ButtonPosition.x + parameters.ButtonPosition.y * 5].SetupButton(parameters.ButtonData);
    }

    private void ResetButtons()
    {
        foreach (ActionButton button in _actionButtons)
            button.ResetButton();
    }
}
