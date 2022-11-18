using UnityEngine;
using System.Collections.Generic;

public abstract class Character : TickedBehaviour, IDamageable
{
    public enum Type
    {
        Peon,
        Knight
    }

    [SerializeField]
    protected CharacterData _data;
    public abstract bool Idle { get; }
    public CharacterData Data => _data;

    public int MaxHealth => _data.MaxHealth;

    public GameObject SelectionMarker;
    public Vector2Int Coords;

    public Stack<LogicalTile> Path;
    public LogicalTile RallyPoint;
    public Goal RallyPointGoal = Goal.None;
    public bool isInTroop;

    protected virtual void Start()
    {
        Coords = TileMapManager.WorldToTilemapCoords(gameObject.transform.position);
        TileMapManager.AddObstacle(Coords);
        CharacterSelectionManager.AddCharacter(this);
    }

    public void DebugCoordinates()
    {
        Debug.Log($"{gameObject.name} coords : ({Coords.x}, {Coords.y})");
    }

    private void OnDestroy()
    {
        CharacterSelectionManager.RemoveCharacter(this);
    }
}