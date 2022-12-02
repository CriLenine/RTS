using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class LocomotionManager : MonoBehaviour
{
    private Mouse _mouse;
    private Camera _camera;

    [SerializeField]
    private bool _debug;

    //private System.Random _random = new System.Random(10);

    private HashSet<int> neighborsID;

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

        // Retrieve the rallypoint's coordinates according to the input.

        Vector3 worldMousePos = _camera.ScreenToWorldPoint(_mouse.position.ReadValue());
        Vector2Int rallyPointCoords = TileMapManager.WorldToTilemapCoords(worldMousePos);

        LogicalTile rallyTile = TileMapManager.GetLogicalTile(rallyPointCoords);

        if (rallyTile == null || !rallyTile.IsFree(NetworkManager.Me))
            return;

        int[] IDs = new int[characters.Count];

        for (int i = 0; i < characters.Count; ++i)
            IDs[i] = characters[i].ID;

        NetworkManager.Input(TickInput.Move(IDs, worldMousePos));
    }

    public static List<Vector2> RetrieveWayPoints(Character leader, Vector2Int rallyPoint, bool smooth = true)
    {
        List<Vector2Int> wayPoints = TileMapManager.FindPath(leader.Coords, rallyPoint);

        if (wayPoints.Count == 0)
            return null;

        List<Vector2> positionWayPoints = new List<Vector2>();

        if (!smooth || (wayPoints.Count < 2))
        {
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
        character.SetPosition(LocalAvoidance(character, position));

        return character.transform.position == position;
    }

    private Vector2 LocalAvoidance(Character character, Vector3 position)
    {
        // Récupérer voisins KDTree

        //neighborsID = GameManager.holyNode.GetNeighbours(character.ID, .3f, .5f, character.transform.position);

        List<Vector2> trajectoryAdjustments = new List<Vector2>();

        List<Character> neighbors = new List<Character>(CharacterManager.SelectedCharacters());

        /*foreach (int ID in neighborsID)*/
        foreach (Character neighbor in neighbors)
        {
            if (neighbor == character)
                continue;

            //Character neighbor = (Character)GameManager.Entities[ID];

            Vector2 deltaPos = neighbor.transform.position - character.transform.position;

            if (deltaPos.sqrMagnitude < .3f)
                trajectoryAdjustments.Add(neighbor.CurrentAction is Move ? deltaPos.normalized : Vector2.Perpendicular(deltaPos).normalized);
        }

        if (trajectoryAdjustments.Count == 0)
            return Vector2.MoveTowards(character.transform.position, position, TileMapManager.TileSize / 10f);

        Vector2 targetPosition = Vector2.zero;

        foreach (Vector2 adjustment in trajectoryAdjustments)
            targetPosition += adjustment;

        return Vector2.MoveTowards(character.transform.position,
            (Vector2)character.transform.position - targetPosition /** (float)_random.NextDouble()*/, TileMapManager.TileSize / 10f);
    }
}
