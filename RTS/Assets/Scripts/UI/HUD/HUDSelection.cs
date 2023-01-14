using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDSelection : HUD
{
    private static HUDSelection _instance;

    private void Awake()
    {
        if(_instance != null)
            Destroy(_instance);

        _instance = this;
    }

    [SerializeField]
    private List<SelectionButton> _selectionButtons = new List<SelectionButton>();

    public void SetupSelection()
    {
        List<Character.Type> types = new List<Character.Type>(SelectionManager.SelectedTypes);

        _selectionButtons[0].UpdateCount(Character.Type.All);

        for (int index = 1; index < _selectionButtons.Count; ++index)
            if (SelectionManager.SelectedTypes.Count >= index)
                _selectionButtons[index].SetupButton(types[index - 1]);
            else
                _selectionButtons[index].ResetButton();
    }

    public static void UpdatePulse()
    {
        int pulseElementIndex = 0;

        if (SelectionManager.SelectedType != Character.Type.All)
            foreach (Character.Type type in SelectionManager.SelectedTypes)
            {
                ++pulseElementIndex;

                if (type == SelectionManager.SelectedType)
                    break;
            }

        for (int index = 0; index < _instance._selectionButtons.Count; ++index)
            _instance._selectionButtons[index].SetActivePulse(index == pulseElementIndex);
    }
}
