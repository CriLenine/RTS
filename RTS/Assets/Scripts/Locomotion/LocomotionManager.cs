using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class LocomotionManager : MonoBehaviour
{
    private static LocomotionManager _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(_instance);

        _instance = this;
    }

    public static List<Vector2> RetrieveWayPoints(int performer, Character leader, Vector2Int rallyPoint, bool smooth = true)
    {
        List<Vector2Int> wayPoints = TileMapManager.FindPath(performer, leader.Coords, rallyPoint);
        if (wayPoints is null)
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

    public static bool Move(Character character, Vector2 position)
    {
        Move characterMove = character.CurrentAction.SpecificAction as Move;

        Vector2? projectedPosition = LocalAvoidance(character, position);

        if (projectedPosition is null)
            return false;

        if (Vector2.SqrMagnitude(position - character.Position) < characterMove.TestThreshold)
            if (MoveComplete(character, projectedPosition.Value, position))
                return true;

        Vector2 projectedDirection = (projectedPosition.Value - character.Position).normalized;

        characterMove.LastDir = Vector2.Lerp(characterMove.LastDir, projectedDirection, 0.2f);

        projectedPosition = character.Position + characterMove.LastDir;

        Vector2 targetPosition = Vector2.MoveTowards(character.Position, projectedPosition.Value, 2f * TileMapManager.TileSize / 10f);
        character.SetPosition(targetPosition);

        return character.Position == position;
    }

    private static bool MoveComplete(Character character, Vector2 projectedPosition, Vector2 position)
    {
        if ((position - character.Position).sqrMagnitude < 0.05f)
            return true;

        if (Vector2.Dot((projectedPosition - character.Position).normalized, (position - character.Position).normalized) > .9f)
            return false;

        if (Vector2.SqrMagnitude(position - character.Position) > (character.CurrentAction.SpecificAction as Move).CompletionThreshold)
            return false;

        return true;
    }

    private static Vector2? LocalAvoidance(Character character, Vector2 position)
    {
        Move characterMove = character.CurrentAction.SpecificAction as Move;

        QuadTreeDebugger.Clear();

        HashSet<int> neighborIds = QuadTreeNode.GetNeighbours(character.ID, character.transform.position);

        neighborIds.Remove(character.ID);

        List<Vector2> avoidanceDirections = new List<Vector2>();

        float ComputeSide()
        {
            Vector2 gravityCenter = Vector2.zero;

            float total = 0f;

            foreach (int id in neighborIds)
            {
                float sqrDistance = Vector2.SqrMagnitude(GameManager.Entities[id].Position - character.Position);

                gravityCenter += GameManager.Entities[id].Position * sqrDistance;

                total += sqrDistance;
            }

            gravityCenter /= total;

            Vector2 end = character.Position + characterMove.Direction;

            return -Mathf.Sign((end.x - character.Position.x) * (gravityCenter.y - character.Position.y) - (end.y - character.Position.y) * (gravityCenter.x - character.Position.x));
        }

        //Debug.Log(neighborIds.Count);

        if (neighborIds.Count > 0)
        {
            bool isPathFree = true;

            foreach (int id in neighborIds)
            {
                Character neighbor = GameManager.Entities[id] as Character;

                if (IsRayIntersectingCircle(character.Position, characterMove.Direction * 1000f, neighbor.Position, 0.15f))
                {
                    isPathFree = false;

                    break;
                }
            }

            foreach (int id in neighborIds)
            {
                Character neighbor = GameManager.Entities[id] as Character;

                Vector2 neighborDirection = (Vector2)neighbor.transform.position - character.Position;
                float neighborSqrDistance = neighborDirection.sqrMagnitude;

                if (neighborSqrDistance < 10f)
                {
                    Move neighborMove = neighbor.CurrentAction?.SpecificAction as Move;
                    bool isNeighborMoving = neighborMove != null;

                    if (neighborSqrDistance > 0.15f)
                    {
                        if (isPathFree)
                        {
                            //Debug.Log("far but path free, do nothing");
                        }
                        else if (!isNeighborMoving || Vector2.Dot(characterMove.Direction.normalized, neighborMove.Direction.normalized) < 0.9f)
                        {
                            //Debug.Log("far and not same dir, avoiding " + (ComputeSide() == 1f ? "left" : "right"));
                            avoidanceDirections.Add(ComputeSide() * Vector2.Perpendicular(characterMove.Direction).normalized / Mathf.Pow(Mathf.Max(neighborSqrDistance, 1f), 0.5f));
                        }
                    }
                    else
                    {
                        if (isNeighborMoving)
                        {
                            //Debug.Log("near and moving, ejecting");
                            avoidanceDirections.Add(10f * -neighborDirection.normalized);
                        }
                        else
                        {
                            //Debug.Log("near but not moving, avoiding " + (ComputeSide() == 1f ? "left" : "right"));
                            avoidanceDirections.Add(10f * ComputeSide() * Vector2.Perpendicular(neighborDirection).normalized);
                        }
                    }
                }
            }
        }

        Vector2 finalPosition;

        if (avoidanceDirections.Count > 0)
        {
            avoidanceDirections.Add(characterMove.Direction.normalized);

            Vector2 averageDirection = Vector2.zero;

            foreach (Vector2 direction in avoidanceDirections)
                averageDirection += direction;

            finalPosition = character.Position + averageDirection;
        }
        else
            finalPosition = position;

        Vector2 testPosition = character.Position + (finalPosition - character.Position).normalized * TileMapManager.TileSize;

        if (TileMapManager.LineOfSight(character.Performer, character.Coords, TileMapManager.WorldToTilemapCoords(testPosition)))
        {
            characterMove.TestedAngles.Clear();
            characterMove.LastAngle = null;

            return finalPosition;
        }
        else
        {
            Vector2Int targetCoords = TileMapManager.WorldToTilemapCoords(position);

            Vector2Int nearestFreeCoords = Vector2Int.zero;
            float bestSqrDistance = float.MaxValue;

            for (int x = -1; x <= 1; ++x)
            {
                for (int y = -1; y <= 1; ++y)
                {
                    Vector2Int emergencyCoords = character.Coords - new Vector2Int(x, y);

                    if (!TileMapManager.LineOfSight(character.Performer, character.Coords, emergencyCoords))
                        continue;

                    bool occupied = false;

                    foreach (int id in neighborIds)
                    {
                        if (GameManager.Characters[id].Coords == emergencyCoords)
                        {
                            occupied = true;

                            break;
                        }
                    }

                    if (occupied)
                        continue;

                    float sqrDistance = (targetCoords - emergencyCoords).sqrMagnitude;

                    if (sqrDistance < bestSqrDistance)
                    {
                        nearestFreeCoords = emergencyCoords;
                        bestSqrDistance = sqrDistance;
                    }
                }
            }

            if (bestSqrDistance != float.MaxValue)
                return TileMapManager.TilemapCoordsToWorld(nearestFreeCoords);

            return null;
        }
    }

    private static bool IsRayIntersectingCircle(Vector2 origin, Vector2 direction, Vector2 circlePos, float circleRadius)
    {
        direction *= 1000f;

        float lambda = -(((origin.x - circlePos.x) * direction.x + (origin.y - circlePos.y) * direction.y) / direction.sqrMagnitude);

        float minSqrDistance = Mathf.Pow(origin.x + lambda * direction.x - circlePos.x, 2f) + Mathf.Pow(origin.y + lambda * direction.y - circlePos.y, 2f);

        return minSqrDistance < circleRadius * circleRadius;
    }
}