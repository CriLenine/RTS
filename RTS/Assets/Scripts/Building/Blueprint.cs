using UnityEngine;
using UnityEngine.InputSystem;

public class Blueprint : MonoBehaviour
{
    private static Building.Type _buildType;
    private SpriteRenderer _blueprintRenderer;

    [SerializeField] 
    private Color _availableColor;

    [SerializeField]
    private Color _notAvailableColor;

    private static int _outline;


    internal static Blueprint InstantiateWorldPos(Building.Type buildType)
    {
        SpawnableDataBuilding building = PrefabManager.GetBuildingData(buildType);

        _buildType = buildType;
        _outline = building.Outline;

        return Instantiate(building.BuildingBlueprint, Vector2.zero, Quaternion.identity);
    }

    private void Start()
    {
        _blueprintRenderer = GetComponent<SpriteRenderer>();

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        if(Physics2D.OverlapPoint(worldPos))
        {
            transform.position = worldPos;
        }
    }

    private void Update()
    {
        (Vector3 position, bool available) = TileMapManager.TilesAvailableForBuild(_outline);

        if (Physics2D.OverlapPoint(position))
        {
            transform.position = position;
        }

        _blueprintRenderer.color = available ? _availableColor : _notAvailableColor;


        if (Mouse.current.leftButton.wasPressedThisFrame && available)
        {
            int[] selected = CharacterSelectionManager.GetSelectedIds();

            NetworkManager.Input(TickInput.Build(selected, (int)_buildType, transform.position));

            Destroy(gameObject);
        }
    }
}
