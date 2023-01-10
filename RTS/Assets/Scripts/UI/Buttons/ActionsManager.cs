using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActionsManager : MonoBehaviour
{
    private static ActionsManager _instance;

    private ActionButton _currentButton;

    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this);
        else
            _instance = this;
    }

    private static void OnClickCommon()
    {
        GameEventsManager.PlayEvent("UIClick");
    }
    public static void OnClickAttack(/*ActionButton button*/)
    {
        OnClickCommon();

        //if (!button.ToogleButton()) return;

        //_instance._currentButton = button;
    }

    public void Stop()
    {
        NetworkManager.Input(TickInput.Stop(SelectionManager.GetSelectedIds()));
    }

    public void MoveToggle()
    {
        // TODO
    }

    public void Station()
    {
        // TODO
    }

    public void KillUnits()
    {
        NetworkManager.Input(TickInput.Kill(SelectionManager.GetSelectedIds()));
    }

    public void DestroyBuilding()
    {
        NetworkManager.Input(TickInput.Destroy(SelectionManager.SelectedBuilding.ID));
    }

    public void CancelBuildingConstruction()
    {
        NetworkManager.Input(TickInput.CancelConstruction(SelectionManager.SelectedBuilding.ID));
    }

    public void QueueUnitSpawn(CharacterData data)
    {
        foreach (Resource.Amount cost in data.Cost)
            GameManager.Pay(cost.Type, cost.Value, NetworkManager.Me);

        SelectionManager.SelectedBuilding.EnqueueSpawningCharas(data);
    }


    #region Attack
    public void GuardPosition()
    {
        InputActionsManager.UpdateGameState(GameState.Guard);
    }
    #endregion

}
