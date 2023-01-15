using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UIElements;

public class OrderManager : MonoBehaviour
{
    private static OrderManager _instance;

    private Camera _camera;
    private Mouse _mouse;

    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(_instance);

        _instance = this;

        _camera = Camera.main;
        _mouse = Mouse.current;
    }

    public static void GiveOrder()
    {
        Vector3 worldMousePos = _instance._camera.ScreenToWorldPoint(_instance._mouse.position.ReadValue());

        if (!IsMouseOnTickedBehavior(worldMousePos))
        {
            Vector2Int rallyPointCoords = TileMapManager.WorldToTilemapCoords(worldMousePos);

            LogicalTile rallyTile = TileMapManager.GetLogicalTile(rallyPointCoords);

            if (rallyTile == null)
                return;

            if (!rallyTile.IsFree(NetworkManager.Me))
            {
                if (ResourcesManager.Harvestable(rallyPointCoords))
                    NetworkManager.Input(TickInput.Harvest(rallyPointCoords, SelectionManager.GetSelectedIds()));
                return;
            }

            NetworkManager.Input(TickInput.Move(SelectionManager.GetSelectedIds(), worldMousePos));
        }
    }

    private static bool IsMouseOnTickedBehavior(Vector3 worldMousePos)
    {
        RaycastHit2D hit = Physics2D.Raycast(worldMousePos, Vector2.zero, Mathf.Infinity, SelectionManager.Clickable);

        if (hit.collider == null)
            return false;

        if (hit.collider.gameObject.TryGetComponent(out Building building))
        {
            if (GameManager.MyBuildings.Contains(building))
                if (building.BuildCompletionRatio < 1)
                    OrderBuild(building);
                else
                    Debug.Log("Right click on a built ally building not yet implemented.");
            else
                OrderAttack(building);
        }
        else if (hit.collider.gameObject.TryGetComponent(out Character character) && !GameManager.MyCharacters.Contains(character))
            OrderAttack(character);

        return true;
    }

    public static void OrderBuild(Building building)
    {
        NetworkManager.Input(TickInput.Build(building.ID, SelectionManager.GetSelectedIds()));

        SelectionManager.DeselectAll();
    }

    public static void OrderAttack(TickedBehaviour entity)
    {
        NetworkManager.Input(TickInput.Attack(entity.ID, entity.transform.position, SelectionManager.GetSelectedIds()));

        SelectionManager.DeselectAll();
    }

    public static void OrderGuard()
    {
        Vector3 worldMousePos = _instance._camera.ScreenToWorldPoint(_instance._mouse.position.ReadValue());

        if (!IsMouseOnTickedBehavior(worldMousePos))
            NetworkManager.Input(TickInput.GuardPosition(worldMousePos, SelectionManager.GetSelectedIds()));
    }

    public static void TrySetBuildingRallyPoint()
    {
        if (SelectionManager.SelectedBuilding.Performer == NetworkManager.Me && SelectionManager.SelectedBuilding.Data.CanSpawnUnits)
            NetworkManager.Input(
                TickInput.UpdateRallyPoint(
                    SelectionManager.SelectedBuilding.ID, _instance._camera.ScreenToWorldPoint(_instance._mouse.position.ReadValue())
                    ));
    }
}
