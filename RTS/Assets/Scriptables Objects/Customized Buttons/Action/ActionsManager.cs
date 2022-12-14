using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ActionsManager : MonoBehaviour
{
    private UIInputs _uiInputs;
    private void Start()
    {
        _uiInputs = HUDManager.GetUIInputs();
    }

    private void  OnClickCommon()
    {
        GameEventsManager.PlayEvent("UIClick");
    }
    public void OnClickAttack(ActionButton button)
    {
        OnClickCommon();

        if (!button.ToogleButton()) return;

        _uiInputs.UI.Attack.started += _ =>
        {
            _uiInputs.UI.Attack.canceled += _ => AttackCanceled(button);
            CharacterManager.DisableInputs();
            Attack();
        };
    }
    

    #region Attack
    private void Attack()
    {
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Collider2D collider = Physics2D.OverlapPoint(worldPoint);

        if (collider == null)
            return;

        GameEventsManager.PlayEvent("UIConfirm");
        if (collider.TryGetComponent(out TickedBehaviour entitie) && entitie.TryGetComponent(out IDamageable damageable)) //hit a tickedbehaviour damageable
        {
            if (CharacterManager.SelectedCharacters.Count > 0 && !GameManager.MyEntities.Contains(entitie)) //selected characters && not my entitie
                NetworkManager.Input(TickInput.Attack(entitie.ID, entitie.transform.position, CharacterManager.GetSelectedIds()));
        }
        else  // sinon hit le sol on y vas et on surveille (target id = -1)
        {
            if (CharacterManager.SelectedCharacters.Count > 0) //selected characters
                NetworkManager.Input(TickInput.Attack(-1, worldPoint, CharacterManager.GetSelectedIds()));
        }

    }

    private void AttackCanceled(ActionButton button)
    {
        button.ToogleButton();
        _uiInputs.UI.Attack.started -= _ => Attack();
        _uiInputs.UI.Attack.canceled -= _ => AttackCanceled(button);
        CharacterManager.EnableInputs();
    }
    #endregion

}
