using UnityEngine;

public abstract class Character : TickedBehaviour, IDamageable
{
    [SerializeField]
    protected CharacterData _data;

    public CharacterData Data => _data;
    public int MaxHealth => _data.MaxHealth;

    public GameObject selectionMarker;
    public Vector2Int coords;

    private void Start()
    {
        coords = TileMapManager.WorldToTilemapCoords(gameObject.transform.position);
        TileMapManager.AddObstacle(coords);
        CharacterSelectionManager.AddCharacter(this);
    }

    public void DebugCoordinates()
    {
        Debug.Log($"{gameObject.name} coords : ({coords.x}, {coords.y})");
    }

    private void OnDestroy()
    {
        CharacterSelectionManager.RemoveCharacter(this);
    }
}
