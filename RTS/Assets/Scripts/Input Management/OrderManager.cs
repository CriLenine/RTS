using UnityEngine.InputSystem;
using UnityEngine;

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

        RaycastHit2D hit = Physics2D.Raycast(worldMousePos, Vector2.zero, Mathf.Infinity, SelectionManager.Clickable);

        if (hit.collider != null)
        {
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
        }
        else
        {
            Vector2Int rallyPointCoords = TileMapManager.WorldToTilemapCoords(worldMousePos);

            LogicalTile rallyTile = TileMapManager.GetLogicalTile(rallyPointCoords);

            if (rallyTile == null)
                return;

            if (!rallyTile.IsFree(NetworkManager.Me))
            {
                if (ResourcesManager.Harvestable(rallyPointCoords))
                {
                    int[] ids = new int[SelectionManager.SelectedCharacters.Count];

                    for (int i = 0; i < SelectionManager.SelectedCharacters.Count; ++i)
                        ids[i] = SelectionManager.SelectedCharacters[i].ID;

                    NetworkManager.Input(TickInput.Harvest(rallyPointCoords, ids));
                }
                return;
            }

            int[] IDs = SelectionManager.GetSelectedIds();

            NetworkManager.Input(TickInput.Move(IDs, worldMousePos));

            SelectionManager.DeselectAll();
        }
    }

    public static void OrderBuild(Building building)
    {
        int[] builderIDs = SelectionManager.GetSelectedIds();

        NetworkManager.Input(TickInput.Build(building.ID, builderIDs));

        SelectionManager.DeselectAll();
    }

    public static void OrderAttack(TickedBehaviour entity)
    {
        int[] attackerIDs = SelectionManager.GetSelectedIds();

        NetworkManager.Input(TickInput.Attack(entity.ID, entity.transform.position, attackerIDs));

        SelectionManager.DeselectAll();
    }

    public static void TrySetBuildingRallyPoint()
    {
        if (SelectionManager.SelectedBuilding.Performer == NetworkManager.Me && SelectionManager.SelectedBuilding.Data.CanSpawnUnits)
            SelectionManager.SelectedBuilding.SetRallyPoint(_instance._camera.ScreenToWorldPoint(_instance._mouse.position.ReadValue()));
    }
}
