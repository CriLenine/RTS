using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState
{
    None,
    CharacterSelection,
    BuildingSelection,
    Blueprint,
    Attack
}

public class InputActionsManager : MonoBehaviour
{
    private static InputActionsManager _instance;

    [SerializeField]
    private LayerMask _HUDLayerMask;
    public static LayerMask HUDLayerMask => _instance._HUDLayerMask;

    private Mouse _mouse;

    private Gameplay _inputActions;

    private GameState _currentGameState;

    public static bool IsShifting { get; private set; } = false;

    private bool _selectionInitiated = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(_instance);

        _instance = this;

        _mouse = Mouse.current;
    }

    private void Start()
    {
        _inputActions = new Gameplay();

        _inputActions.MouseControls.LeftClick.started += _ => LeftClickStarted();
        _inputActions.MouseControls.LeftClick.canceled += _ => LeftClickCanceled();

        _inputActions.MouseControls.RightClick.performed += _ => RightClickPerformed();

        _inputActions.KeyboardControls.Shift.started += _ => IsShifting = true;
        _inputActions.KeyboardControls.Shift.canceled += _ => IsShifting = false;

        _inputActions.Enable();
    }

    private bool TryHUDRaycast()
    {
        RaycastHit2D hit = Physics2D.Raycast(_mouse.position.ReadValue(), Vector2.zero, Mathf.Infinity, _HUDLayerMask);

        if (hit.collider != null)
            return true;

        return false;
    }

    private void LeftClickStarted()
    {
        if (TryHUDRaycast())
        {
            _selectionInitiated = false;
            return;
        }

        switch (_currentGameState)
        {
            case GameState.None:
                SelectionManager.InitSelection();
                _selectionInitiated = true;
                break;

            case GameState.CharacterSelection:
                SelectionManager.InitSelection();
                _selectionInitiated = true;
                break;

            case GameState.BuildingSelection:
                SelectionManager.InitSelection();
                _selectionInitiated = true;
                break;

            case GameState.Blueprint:
                Blueprint.TryPlaceBuilding();
                break;

            case GameState.Attack:
                ActionsManager.Attack();
                break;
        }
    }

    private void LeftClickCanceled()
    {
        if (_selectionInitiated)
            SelectionManager.ProceedSelection();
    }

    private void RightClickPerformed()
    {
        if (TryHUDRaycast())
            return;

        switch (_currentGameState)
        {
            case GameState.None:
                return;

            case GameState.CharacterSelection:
                OrderManager.GiveOrder();
                break;

            case GameState.BuildingSelection:
                OrderManager.TrySetBuildingRallyPoint();
                break;

            case GameState.Blueprint:
                Blueprint.CancelBuildingBlueprint();
                break;

            case GameState.Attack:
                ActionsManager.CancelActions();
                break;
        }
    }

    public static void UpdateGameState(GameState state)
    {
        if (state == _instance._currentGameState)
            return;

        _instance._currentGameState = state;
    }
}
