using UnityEngine;
using UnityEngine.InputSystem;

public class Blueprint : MonoBehaviour
{
    private static Blueprint _instance;

    private Mouse _mouse;

    private BuildingData _data;

    [SerializeField]
    private GameObject _holder;

    [SerializeField]
    private SpriteRenderer _blueprintSprite, _iconSprite;

    [SerializeField]
    private Color _buildableColor, _notBuildableColor;

    [SerializeField]
    private LayerMask _HUD;

    private int _size;

    private bool _available = false;
    private bool _inBlueprintMode = false;

    private Vector3 _position;

    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this);
        else
            _instance = this;

        _mouse = Mouse.current;
    }

    public static void SetActiveBlueprint(BuildingData data)
    {
        _instance._inBlueprintMode = true;

        _instance._holder.SetActive(true);

        _instance._data = data;
        _instance._size = data.Size;

        float scale = TileMapManager.TileSize * _instance._size;
        _instance._holder.transform.localScale = new Vector3(scale, scale, 1);

        _instance._iconSprite.sprite = data.HUDIcon;

        InputActionsManager.UpdateGameState(GameState.Blueprint);
    }

    private void Update()
    {
        if (_inBlueprintMode)
        {
            (_instance._position, _instance._available) = TileMapManager.TilesAvailableForBuild(_size);

            _blueprintSprite.color = _instance._available ? _buildableColor : _notBuildableColor;

            float offset = ((float)_data.Size - 1) / 2 * TileMapManager.TileSize;

            _holder.transform.position = new Vector3(_instance._position.x + offset, _instance._position.y + offset);
        }
    }

    public static void TryPlaceBuilding()
    {
        if (_instance._available)
        {
            NetworkManager.Input(TickInput.NewBuild((int)_instance._data.Type, _instance._position, SelectionManager.GetSelectedIds()));

            _instance._holder.SetActive(false);
            _instance._inBlueprintMode = false;

            InputActionsManager.UpdateGameState(GameState.CharacterSelection);
        }
    }

    public static void CancelBuildingBlueprint()
    {
        _instance._holder.SetActive(false);
        _instance._inBlueprintMode = false;

        InputActionsManager.UpdateGameState(GameState.CharacterSelection);
    }
}
