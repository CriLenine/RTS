using UnityEngine;
using UnityEngine.InputSystem;

public class Blueprint : MonoBehaviour
{
    private Mouse _mouse;

    private Building.Type _buildType;

    [SerializeField]
    private SpriteRenderer _blueprintSprite, _iconSprite;

    [SerializeField]
    private Color _buildableColor, _notBuildableColor;

    [SerializeField]
    private LayerMask _HUD;

    private int _outline;

    private void Awake()
    {
        _mouse = Mouse.current;
    }

    public void SetActiveBlueprint(BuildingData data)
    {
        gameObject.SetActive(true);

        _buildType = data.Type;
        _outline = data.Outline;

        float scale = TileMapManager.TileSize * (2 * _outline + 1);
        transform.localScale = new Vector3(scale, scale, 1);

        _iconSprite.sprite = data.HUDIcon;
    }

    private void Update()
    {
        if (_mouse.rightButton.wasPressedThisFrame)
            gameObject.SetActive(false);

        (Vector3 position, bool available) = TileMapManager.TilesAvailableForBuild(_outline);

        _blueprintSprite.color = available ? _buildableColor : _notBuildableColor;

        transform.position = position;

        

        if (_mouse.leftButton.wasPressedThisFrame && available)
        {
            RaycastHit2D hit = Physics2D.Raycast(_mouse.position.ReadValue(), Vector2.zero, Mathf.Infinity, _HUD);

            if (hit.collider == null)
                NetworkManager.Input(TickInput.NewBuild((int)_buildType, position, CharacterManager.GetSelectedIds()));

            gameObject.SetActive(false);
        }
    }
}
