using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : TickedBehaviour, IDamageable
{
    [SerializeField]
    protected CharacterData _data;

    public CharacterData Data => _data;

    public int MaxHealth => _data.MaxHealth;

    public abstract bool Idle { get; }

    public GameObject SelectionMarker;
    public Vector2Int Coords;

    private void Start()
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
