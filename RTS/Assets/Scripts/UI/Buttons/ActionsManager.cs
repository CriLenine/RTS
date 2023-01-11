using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActionsManager : MonoBehaviour
{
    private static ActionsManager _instance;

    private bool _isAttackToggled = false;

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
    public static void AttackToggle()
    {
        OnClickCommon();

        if(_instance._isAttackToggled)
        {
            CancelActions();
            _instance._isAttackToggled = false;
        }
        else
        {
            InputActionsManager.UpdateGameState(GameState.Attack);
            _instance._isAttackToggled = true;
        }
    }

    public void Stop()
    {
        OnClickCommon();
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


    #region Actions Logic
    public static void Attack()
    {
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Collider2D collider = Physics2D.OverlapPoint(worldPoint);

        if (collider == null)
            return;

        GameEventsManager.PlayEvent("UIConfirm");
        if (collider.TryGetComponent(out TickedBehaviour entity) && entity.TryGetComponent(out IDamageable damageable)) //hit a tickedbehaviour damageable
        {
            if (SelectionManager.SelectedCharacters.Count > 0 && !GameManager.MyEntities.Contains(entity)) //selected characters && not my entity
                NetworkManager.Input(TickInput.Attack(entity.ID, entity.transform.position, SelectionManager.GetSelectedIds()));
        }
        else  // sinon hit le sol on y vas et on surveille (target id = -1)
        {
            if (SelectionManager.SelectedCharacters.Count > 0) //selected characters
                NetworkManager.Input(TickInput.GuardPosition(worldPoint, SelectionManager.GetSelectedIds()));
        }

        CancelActions();
        _instance._isAttackToggled = false;
    }

    internal static void CancelActions()
    {
        HUDManager.UpdateActionButtons();
        InputActionsManager.UpdateGameState(GameState.None);
    }
    
    #region Attack
    public void GuardPosition()
    {
        InputActionsManager.UpdateGameState(GameState.Guard);
    }
    #endregion

}
