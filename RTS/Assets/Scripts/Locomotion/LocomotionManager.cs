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


        if (characters.Count == 1 && 
            (GameManager.RessourcesManager.HasRock(rallyPointCoords) || GameManager.RessourcesManager.HasTree(rallyPointCoords)))
        {
            NetworkManager.Input(TickInput.Harvest(rallyPointCoords, characters[0].ID));
            return;
        }

        if (rallyTile == null || !rallyTile.IsFree(NetworkManager.Me))
            return;

        int[] IDs = new int[characters.Count];

        for (int i = 0; i < characters.Count; ++i)
            IDs[i] = characters[i].ID;

        NetworkManager.Input(TickInput.Move(IDs, worldMousePos));
    }

    public static List<Vector2> RetrieveWayPoints(int performer, Character leader, Vector2Int rallyPoint, bool smooth = true)
    {
        List<Vector2Int> wayPoints = TileMapManager.FindPath(performer, leader.Coords, rallyPoint);

        if (wayPoints.Count == 0)
            return null;

        List<Vector2> positionWayPoints = new();

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
                if (TileMapManager.LineOfSight(performer, wayPoints[currentWayPointIndex], wayPoints[index]))
                    break;

            positionWayPoints.Add(TileMapManager.TilemapCoordsToWorld(wayPoints[index]));

            currentWayPointIndex = index;
        }

        return positionWayPoints;
    }

    public bool Move(Character character, Vector3 position)
    {
        Vector3 projectedPosition = LocalAvoidance(character, position);

        if ((position - character.transform.position).sqrMagnitude < (character.CurrentAction as Move).TestThreshold)
            if (MoveComplete(character, projectedPosition, position))
                return true;

        projectedPosition = ObstacleAvoidance(character, projectedPosition);
        if (projectedPosition == Vector3.zero)
            return true;

        Vector2 targetPosition = Vector2.MoveTowards(character.transform.position, projectedPosition, TileMapManager.TileSize / 10f);
        character.SetPosition(targetPosition);

        return character.transform.position == position;
    }

    private bool MoveComplete(Character character, Vector2 projectedPosition, Vector2 position)
    {
        Vector2 characterPos = character.transform.position;

        if (Vector2.Dot((projectedPosition - characterPos).normalized, (position - characterPos).normalized) > .9f)
            return false;

        if ((position - characterPos).sqrMagnitude > (character.CurrentAction as Move).CompletionThreshold)
            return false;

        return true;
    }

    private Vector2 ObstacleAvoidance(Character character, Vector2 projectedPosition)
    {
        Vector2 characterPos = (Vector2)character.transform.position;

        if (TileMapManager.GetLogicalTile(TileMapManager.WorldToTilemapCoords(projectedPosition)).IsFree(character.Performer))
            return projectedPosition;

        Debug.Log("Obstacle detected");

        Vector2 collisionPosition = projectedPosition - characterPos;

        Vector2 rightPath = characterPos + Vector2.Perpendicular(collisionPosition);

        if (TileMapManager.GetLogicalTile(TileMapManager.WorldToTilemapCoords(rightPath)).IsFree(character.Performer))
            return characterPos + rightPath;

        Vector2 leftPath = characterPos - Vector2.Perpendicular(collisionPosition);

        if (TileMapManager.GetLogicalTile(TileMapManager.WorldToTilemapCoords(leftPath)).IsFree(character.Performer))
            return characterPos + leftPath;

        return Vector2.zero;
    }

    private Vector2 LocalAvoidance(Character character, Vector2 position)
    {
        Vector2 characterPos = (Vector2)character.transform.position;

        // Récupérer voisins KDTree

        neighborsID = QuadTreeNode.GetNeighbours(character.ID, character.transform.position);

        List<Vector2> trajectoryAdjustments = new();

        //List<Character> neighbors = new(CharacterManager.SelectedCharacters());

        foreach (int ID in neighborsID)
        //foreach (Character neighbor in neighbors)
        {
            //if (neighbor == character)
            //    continue;

            Character neighbor = (Character)GameManager.Entities[ID];

            Vector2 deltaPos = (Vector2)neighbor.transform.position - characterPos;

            if (deltaPos.sqrMagnitude < .1f)
                trajectoryAdjustments.Add(neighbor.CurrentAction is Move ? -deltaPos.normalized : Vector2.Perpendicular(deltaPos).normalized);
        }

        if (trajectoryAdjustments.Count == 0)
            return position;

        Vector2 targetPosition = Vector2.zero;

        foreach (Vector2 adjustment in trajectoryAdjustments)
            targetPosition += adjustment;

        return characterPos + targetPosition;
    }
}
