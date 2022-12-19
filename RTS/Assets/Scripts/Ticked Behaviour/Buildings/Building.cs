using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class Building : TickedBehaviour, IDamageable
{
    public enum Type
    {
        HeadQuarters,
        Housing,
        Sawmill,
        Quarry,
        Barracks
    }

    [Separator("Base Data")]
    [Space]

    [ReadOnly]
    [SerializeField]
    private int _currentHealth;
    public int CurrentHealth => _currentHealth;

    [SerializeField]
    private BuildingData _buildingData;
    public BuildingData Data => _buildingData;

    

    [Separator("Utils")]
    [Space]

    [SerializeField]
    private BoxCollider2D _boxCollider;

    [SerializeField]
    protected LineRenderer _pathRenderer;
    public LineRenderer PathRenderer => _pathRenderer;

    [Separator("UI")]
    [Space]

    [SerializeField]
    private HealthBar HealthBar;

    [Separator("Art")]
    [Space]

    [SerializeField]
    private Transform _visualTransform;
    [SerializeField]
    private Transform _completionTransform, _healthBarTransform;

    [Space]

    [SerializeField]
    private SpriteRenderer _backgroundSprite;
    [SerializeField]
    private SpriteRenderer _completionSprite, _iconSprite;

    [Space]

    [SerializeField]
    private Color _completionStartColor;
    [SerializeField]
    private Color _completionEndColor, _iconSpriteStartColor, _iconSpriteEndColor, _selectedColor;
    [SerializeField]
    private Color _backgroundStartColor, _backgroundEndColor;


    private bool _buildComplete;
    public bool BuildComplete => _buildComplete;

    private int _completedBuildTicks;

    protected int MaxHealth => _buildingData.MaxHealth;
    public float BuildCompletionRatio => (float)_completedBuildTicks / _buildingData.RequiredBuildTicks;

    private bool _selected;

    #region SpawnerSpecs

    private Vector2 _rallyPoint;

    private Queue<CharacterData> _queuedSpawnCharactersData = new();
    private CharacterData _onGoingSpawnCharacterData;

    private int _spawningTicks = 0;

    private bool _onGoingSpawn = false;

    #endregion

    public void InitData(BuildingData data)
    {
        _buildingData = data;
    }

    protected override void Awake()
    {
        base.Awake();

        foreach (ButtonDataHUDParameters parameter in _buildingData.Actions)
            if (parameter.ButtonData is CharacterData)
            {
                _buildingData.CanSpawnUnits = true;
                break;
            }

        float scale = .95f * TileMapManager.TileSize * Data.Size;
        _visualTransform.localScale = new Vector3(scale, scale, 1);
        _boxCollider.size = new Vector2(scale, scale);

        _healthBarTransform.transform.position += new Vector3(0, .5f * scale);

        _currentHealth = MaxHealth;
        HealthBar.SetHealth(1);

        _completedBuildTicks = 0;
        _buildComplete = false;

        _completionTransform.localScale = new Vector3(0, 0, 1);

        _completionSprite.color = _completionStartColor;
        _iconSprite.sprite = Data.HUDIcon;
        _iconSprite.color = _iconSpriteStartColor;
        _backgroundSprite.color = _backgroundStartColor;

        _rallyPoint = (Vector2)transform.position + new Vector2(0.7f, 0.7f);
    }

    private void Update()
    {
        if (_selected && _buildingData.CanSpawnUnits)
        {
            Vector2 rallypoint = _rallyPoint;

            _pathRenderer.SetPosition(0, transform.position);
            _pathRenderer.SetPosition(1, rallypoint);

            _pathRenderer.transform.position = rallypoint;

            _pathRenderer.startColor = Color.cyan;
            _pathRenderer.endColor = Color.cyan;

            _pathRenderer.gameObject.SetActive(true);
        }
    }

    public override void Tick()
    {
        if (_onGoingSpawn)
        {
            _spawningTicks++;

            if (_spawningTicks >= _onGoingSpawnCharacterData.SpawnTicks && GameManager.MyCharacters.Count < GameManager.Housing)
            {
                _spawningTicks = 0;
                _onGoingSpawn = false;
                NetworkManager.Input(TickInput.Spawn((int)_onGoingSpawnCharacterData.Type, ID, _rallyPoint));
            }
        }

        if (!_onGoingSpawn && _queuedSpawnCharactersData.Count > 0)
        {
            _onGoingSpawn = true;
            _onGoingSpawnCharacterData = _queuedSpawnCharactersData.Dequeue();
        }
    }

    /// <returns><see langword="true"/> if it finishes the building's construction,
    /// <see langword="false"/> otherwise </returns>
    public bool CompleteBuild(int completionAmount)
    {
        if (_buildComplete)
            return true;

        _completedBuildTicks += completionAmount;

        if (BuildCompletionRatio >= 1f)
        {
            // REQUIRES FIXING POSITION WHEN BUILD ORDER IS SENT !
            // TileMapManager.AddBuilding(Data.Outline, transform.position);

            _completedBuildTicks = _buildingData.RequiredBuildTicks;
            _iconSprite.color = _iconSpriteEndColor;

            if (!_selected)
                _backgroundSprite.color = _backgroundEndColor;

            if (GameManager.MyBuildings.Contains(ID))
                GameManager.UpdateHousing(Data.HousingProvided);

            _buildComplete = true;
        }

        float completionValue = Mathf.Lerp(.5f, .98f, BuildCompletionRatio);
        _completionTransform.localScale = new Vector3(completionValue, completionValue, 1);

        _completionSprite.color = Color.Lerp(_completionStartColor, _completionEndColor, BuildCompletionRatio);

        return _buildComplete;
    }

    public void Select()
    {
        _selected = true;

        if (_buildComplete)
            _backgroundSprite.color = _selectedColor;
    }

    public void Unselect()
    {
        _selected = false;
        _backgroundSprite.color = _buildComplete ? _backgroundEndColor : _backgroundStartColor;

        if (_buildingData.CanSpawnUnits)
            _pathRenderer.gameObject.SetActive(false);
    }

    public bool TakeDamage(int damage)
    {
        if (_currentHealth == MaxHealth)
            HealthBar.gameObject.SetActive(true);

        _currentHealth -= damage;
        HealthBar.SetHealth((float)_currentHealth / MaxHealth);
        return _currentHealth <= 0;
    }

    public void GainHealth(int amount)
    {
        _currentHealth += amount;

        HealthBar.SetHealth((float)_currentHealth / MaxHealth);

        if (_currentHealth >= MaxHealth)
            HealthBar.gameObject.SetActive(false);

        if (_currentHealth > MaxHealth)
            _currentHealth = MaxHealth;
    }

    public Vector2Int GetClosestOutlinePosition(Character character)
    {
        List<Vector2Int> availableTiles = new List<Vector2Int>();
        for (int x = Coords.x - 1; x <= Coords.x + Data.Size; x += Data.Size + 1)
        {
            for (int y = Coords.y - 1; y <= Coords.y + Data.Size; y += Data.Size + 1)
            {
                Vector2Int tileCoords = new Vector2Int(x, y);

                if (TileMapManager.GetLogicalTile(tileCoords)?.IsFree(character.Performer) == true
                    && TileMapManager.FindPath(character.Performer, character.Coords, tileCoords)?.Count > 0)
                    availableTiles.Add(tileCoords);
            }

            if (availableTiles.Count > 0)
                return TileMapManager.FindClosestCoords(availableTiles, character.Coords);
        }

        Debug.LogError("Cannot find a valid outline tile");
        return character.Coords;
    }

    public void SetPosition(Vector3 position)
    {
        float offset = ((float)Data.Size - 1) / 2 * TileMapManager.TileSize;
        Vector3 centerPos = new Vector3(position.x + offset, position.y + offset);

        transform.position = centerPos;

        Coords = TileMapManager.WorldToTilemapCoords(position);
    }

    public void SetRallyPoint(Vector3 newRallyPoint)
    {
        _rallyPoint = newRallyPoint;
    }

    public void EnqueueSpawningCharas(CharacterData data)
    {
        _queuedSpawnCharactersData.Enqueue(data);
    }
}