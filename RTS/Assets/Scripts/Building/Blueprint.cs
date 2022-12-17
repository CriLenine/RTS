using UnityEngine;
using UnityEngine.InputSystem;

public class Blueprint : MonoBehaviour
{
    private static Blueprint _instance;

    private Mouse _mouse;

    private Building.Type _buildType;

    [SerializeField]
    private GameObject _holder;

    [SerializeField]
    private SpriteRenderer _blueprintSprite, _iconSprite;

    [SerializeField]
    private Color _buildableColor, _notBuildableColor;

    [SerializeField]
    private LayerMask _HUD;

    private int _outline;

    private bool _inBlueprintMode = false;

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

        _instance._buildType = data.Type;
        _instance._outline = data.Outline;

        float scale = TileMapManager.TileSize * (2 * _instance._outline + 1);
        _instance._holder.transform.localScale = new Vector3(scale, scale, 1);

        _instance._iconSprite.sprite = data.HUDIcon;
    }

    private void Update()
    {
        if (_inBlueprintMode)
        {
            if (_mouse.rightButton.wasPressedThisFrame)
            {
                _holder.SetActive(false);
                _inBlueprintMode = false;
            }

            (Vector3 position, bool available) = TileMapManager.TilesAvailableForBuild(_outline);

            _blueprintSprite.color = available ? _buildableColor : _notBuildableColor;

            _holder.transform.position = position;

            if (_mouse.leftButton.wasPressedThisFrame && available)
            {
                RaycastHit2D hit = Physics2D.Raycast(_mouse.position.ReadValue(), Vector2.zero, Mathf.Infinity, _HUD);

                if (hit.collider == null)
                    NetworkManager.Input(TickInput.NewBuild((int)_buildType, position, CharacterManager.GetSelectedIds()));

                _holder.SetActive(false);
                _inBlueprintMode = false;
            }
        }
    }
}
