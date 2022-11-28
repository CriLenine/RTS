using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;


public class LocomotionManager : MonoBehaviour
{
    private Mouse _mouse;
    private Camera _camera;

    [SerializeField]
    private bool _debug;

    QuadTreeNode holyNode = QuadTreeNode.Init(4, 20, 13);

    private void Start()
    {
        _mouse = Mouse.current;
        _camera = Camera.main;
    }

    public void RallySelectedCharacters()
    {
        List<Character> characters = CharacterManager.SelectedCharacters();

        if (characters.Count == 0)
            return;

        //if (characters.Count > 1)
        //    Debug.Log(TileMapManager.RetreiveClusters(characters).Count);

        // Retrieve the rallypoint's coordinates according to the input.

        Vector3 worldMousePos = _camera.ScreenToWorldPoint(_mouse.position.ReadValue());
        Vector2Int rallyPointCoords = TileMapManager.WorldToTilemapCoords(worldMousePos);

        LogicalTile rallyTile = TileMapManager.GetLogicalTile(rallyPointCoords);

        if (rallyTile == null || !rallyTile.IsFree)
            return;

        int[] IDs = new int[characters.Count];

        for (int i = 0; i < characters.Count; ++i)
            IDs[i] = characters[i].ID;

        NetworkManager.Input(TickInput.Move(IDs, worldMousePos));

        // HashSet<int> neighbors = holyNode.GetNeighbours(characters[0].ID, .3f, .5f, characters[0].transform.position);
    }


    public static List<Vector2> RetrieveWayPoints(Character leader, Vector2Int rallyPoint, bool smooth = true)
    {
        List<Vector2Int> wayPoints = TileMapManager.FindPath(leader.Coords, rallyPoint);

        if (wayPoints.Count == 0)
            return null;

        List<Vector2> positionWayPoints = new List<Vector2>();

        if (!smooth || (wayPoints.Count < 2)) {
            for (int i = 0; i < wayPoints.Count - 1; ++i)
                positionWayPoints.Add(TileMapManager.TilemapCoordsToWorld(wayPoints[i]));
            return positionWayPoints;
        }

        int currentWayPointIndex = wayPoints.Count - 1;
        int index;

        while (currentWayPointIndex != 0)
        {
            for (index = 0; index < currentWayPointIndex; ++index)
                if (TileMapManager.LineOfSight(wayPoints[currentWayPointIndex], wayPoints[index]))
                    break;

            positionWayPoints.Add(TileMapManager.TilemapCoordsToWorld(wayPoints[index]));

            currentWayPointIndex = index;
        }

        return positionWayPoints;
    }

    public bool Move(Character character, Vector3 position)
    {
        character.transform.position = Vector2.MoveTowards(character.transform.position, position, TileMapManager.TileSize / 10f);

        return character.transform.position == position;
    }
}
