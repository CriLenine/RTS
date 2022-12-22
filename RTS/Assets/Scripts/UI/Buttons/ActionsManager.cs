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
        // TODO
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
        List<Character> selectedCharacters = SelectionManager.SelectedCharacters;

        int[] IDs = new int[selectedCharacters.Count];

        for (int i = 0; i < selectedCharacters.Count; ++i)
            IDs[i] = selectedCharacters[i].ID;

        NetworkManager.Input(TickInput.Kill(IDs));
    }

    public void DestroyBuilding()
    {
        NetworkManager.Input(TickInput.Destroy(SelectionManager.SelectedBuilding.ID));
    }

    public static void QueueUnitSpawn(CharacterData data)
    {
        foreach (Resource.Amount cost in data.Cost)
            GameManager.Pay(cost.Type, cost.Value, NetworkManager.Me);

        SelectionManager.SelectedBuilding.EnqueueSpawningCharas(data);
    }


    #region Attack
    private static void Attack(InputAction.CallbackContext ctx)
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

        _instance._currentButton.ToogleButton();
    }
    #endregion

}
