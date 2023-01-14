using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

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

    private BuildingData _data;
    public BuildingData Data => _data;

    [Separator("Utils")]
    [Space]

    [SerializeField]
    private BoxCollider2D _boxCollider;

    [SerializeField]
    protected LineRenderer _pathRenderer;
    public LineRenderer PathRenderer => _pathRenderer;

    [Separator("UI")]
    [Space]

    public HealthBar HealthBar;

    [SerializeField]
    private GameObject HoverMarker;

    [SerializeField]
    private GameObject SelectionMarker;

    [Separator("Art")]
    [Space]

    [SerializeField]
    private Transform _visualTransform;
    [SerializeField]
    private Transform _minimapIconTransform, _structureTransform, _healthBarTransform;

    [Space]

    [SerializeField]
    private SpriteRenderer _structureSprite;

    [SerializeField]
    private SpriteRenderer _iconSprite, _minimapIconSprite, _backgroundSprite;

    [Space]

    [SerializeField]
    private Color _completionStartColor;
    [SerializeField]
    private Color _completionEndColor, _enemyCompletionStartColor, _enemyCompletionEndColor, _iconSpriteStartColor, _iconSpriteEndColor;
    [SerializeField]
    private Color _backgroundColor, _enemyBackgroundColor;


    private bool _buildComplete;
    public bool BuildComplete => _buildComplete;

    private int _completedBuildTicks;

    protected int MaxHealth => _data.MaxHealth;
    public float BuildCompletionRatio => (float)_completedBuildTicks / _data.RequiredBuildTicks;

    private bool _isSelected;

    #region SpawnerSpecs

    List<int> linkedKeys = new List<int> { -1, -1, -1, -1, -1, -1,-1 };
    
    public class DictionnaryQueue<T> : IEnumerable<T>
    {
        private int _lastKey = 0;

        private Dictionary<int, T> _dictionnary = new Dictionary<int, T>();
        private SortedSet<int> _keys = new SortedSet<int>();

        public int Count => _keys.Count;

        public int Queue(T item)
        {
            _keys.Add(_lastKey);

            _dictionnary[_lastKey] = item;

            return _lastKey++;
        }

        public void Remove(int index)
        {
            int key = GetNthMinKey(index);

            _dictionnary.Remove(key);

            _keys.Remove(key);
        }

        public T Dequeue()
        {
            if (_keys.Count > 0)
            {
                int _smallestKey = _keys.Min;

                T item = _dictionnary[_smallestKey];

                _dictionnary.Remove(_smallestKey);

                _keys.Remove(_smallestKey);

                return item;
            }

            return default(T);
        }

        public T Peek()
        {
            return _keys.Count > 0 ? _dictionnary[_keys.Min] : default(T);
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (int key in _keys)
                yield return _dictionnary[key];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private int GetNthMinKey(int index)
        {
            if (index < 0 || index >= _keys.Count)
                throw new IndexOutOfRangeException();

            int[] saves = new int[index];

            for (int i = 0; i < index; ++i)
                _keys.Remove(saves[i] = _keys.Min);

            int key = _keys.Min;

            for (int i = 0; i < index; ++i)
                _keys.Add(saves[i]);

            return key;
        }

        public T this[int index] => _dictionnary[GetNthMinKey(index)];
    }

    private Vector2 _rallyPoint;
    public DictionnaryQueue<(CharacterData data,Vector2 rallyPoint)> QueuedSpawnCharacters { get; private set; } = new();

    public (CharacterData data, Vector2 rallyPoint) OnGoingSpawnCharacterData { get; private set; } = (null,Vector2.zero);

    public int SpawningTicks { get; private set; }
    public bool OnGoingSpawn { get; private set; } = false;

    #endregion

    public override void InitData<T>(T data)
    {
        _data = data as BuildingData;

        foreach (ButtonDataHUDParameters parameter in _data.Actions)
            if (parameter.ButtonData is CharacterData)
            {
                _data.CanSpawnUnits = true;
                break;
            }

        float scale = .9f * TileMapManager.TileSize * Data.Size;
        _visualTransform.localScale = new Vector3(scale, scale, 1);
        // _minimapIconTransform.localScale = new Vector3(scale, scale, 1);
        _boxCollider.size = new Vector2(scale, scale);

        _healthBarTransform.transform.position += new Vector3(0, .5f * scale);

       

        _completedBuildTicks = 0;
        _buildComplete = false;

        _structureTransform.localScale = new Vector3(0, 0, 1);

        _structureSprite.color = Performer == NetworkManager.Me ? _completionStartColor : _enemyCompletionStartColor;
        _iconSprite.sprite = Data.HUDIcon;
        _minimapIconSprite.sprite = Data.HUDIcon;
        _iconSprite.color = _iconSpriteStartColor;

        Color tmp = _backgroundSprite.color;
        tmp.a = .25f;
        _backgroundSprite.color = tmp;
    }

    protected override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        if (_isSelected && _data.CanSpawnUnits && BuildComplete)
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
        if (OnGoingSpawn)
        {
            SpawningTicks++;

            if (SpawningTicks >= OnGoingSpawnCharacterData.data.SpawnTicks && GameManager.CharactersPerformer[Performer].Count < GameManager.Housing[Performer])
            {
                SpawningTicks = 0;
                OnGoingSpawn = false;
                QueuedSpawnCharacters.Dequeue();

                if (QueuedSpawnCharacters.Count == 0 && SelectionManager.SelectedBuilding == this)
                    HUDManager.UpdateSpawnPreview();

                GameManager.AddEntity(Performer, ID, OnGoingSpawnCharacterData.data.Type, OnGoingSpawnCharacterData.rallyPoint);
                
                OnGoingSpawnCharacterData = (null,Vector2.zero);
            }
        }
        else if (QueuedSpawnCharacters.Count > 0)
        {
            OnGoingSpawn = true;
            OnGoingSpawnCharacterData = QueuedSpawnCharacters.Peek();

            if (SelectionManager.SelectedBuilding == this)
                HUDManager.UpdateSpawnPreview();
        }
    }

    /// <returns><see langword="true"/> if it finishes the building's construction,
    /// <see langword="false"/> otherwise </returns>
    public bool CompleteBuild(int completionAmount)
    {
        if (_buildComplete)
            return true;

        _completedBuildTicks += completionAmount;

        //Setting health
        _currentHealth = Mathf.RoundToInt(MaxHealth * BuildCompletionRatio);
        HealthBar.SetHealth(BuildCompletionRatio);
        //

        if (BuildCompletionRatio >= 1f)
        {
            // REQUIRES FIXING POSITION WHEN BUILD ORDER IS SENT !
            // TileMapManager.AddBuilding(Data.Outline, transform.position);

            _completedBuildTicks = _data.RequiredBuildTicks;
            _iconSprite.color = _iconSpriteEndColor;

            _backgroundSprite.color = Performer == NetworkManager.Me ? _backgroundColor : _enemyBackgroundColor;


            if(Data.CanSpawnUnits)
                _rallyPoint = (Vector2)transform.position + new Vector2(1.1f * TileMapManager.TileSize * Data.Size / 2, 0);

            _buildComplete = true;

            if (SelectionManager.SelectedBuilding == this)
                HUDManager.UpdateHUD();
        }

        float completionValue = Mathf.Lerp(.5f, .98f, BuildCompletionRatio);
        _structureTransform.localScale = new Vector3(completionValue, completionValue, 1);

        _structureSprite.color = Color.Lerp(Performer == NetworkManager.Me ? _completionStartColor : _enemyCompletionStartColor,
            Performer == NetworkManager.Me ? _completionEndColor : _enemyCompletionEndColor, BuildCompletionRatio);

        return _buildComplete;
    }

    public void Select()
    {
        _isSelected = true;

        HoverMarker.SetActive(false);
        SelectionMarker.SetActive(true);
        HealthBar.gameObject.SetActive(true);
    }

    public void Unselect()
    {
        _isSelected = false;
        SelectionMarker.SetActive(false);

        if (CurrentHealth == Data.MaxHealth)
            HealthBar.gameObject.SetActive(false);

        if (_data.CanSpawnUnits)
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
        for (int x = Coords.x - 1; x <= Coords.x + Data.Size; x ++)
        {
            for (int y = Coords.y - 1; y <= Coords.y + Data.Size; y ++)
            {
                if (x != Coords.x - 1 && x != Coords.x + Data.Size && y != Coords.y - 1 && y != Coords.y + Data.Size) continue;

                Vector2Int tileCoords = new Vector2Int(x, y);

                if (TileMapManager.GetLogicalTile(tileCoords)?.IsFree(character.Performer) == true
                    && TileMapManager.FindPath(character.Performer, character.Coords, tileCoords)?.Count > 0)
                    availableTiles.Add(tileCoords);
            }
        }
        if (availableTiles.Count > 0)
            return TileMapManager.FindClosestCoords(availableTiles, character.Coords);

        Debug.LogError("Cannot find a valid outline tile");
        return character.Coords;
    }

    public void SetPosition(Vector3 position)
    {
        float offset = ((float)Data.Size - 1) / 2 * TileMapManager.TileSize;
        Vector3 centerPos = new Vector3(position.x + offset, position.y + offset);

        transform.position = centerPos;

        Position = position;
        Coords = TileMapManager.WorldToTilemapCoords(position);
    }

    public void SetRallyPoint(Vector3 newRallyPoint)
    {
        _rallyPoint = newRallyPoint;
    }

    public void EnqueueSpawningCharas(CharacterData data) // BeforeNetworking
    {
        NetworkManager.Input(TickInput.QueueSpawn((int)data.Type, ID, _rallyPoint));
    }

    public void QueueSpawn(Character.Type charaType, Vector2 rallyPoint) // After networking
    {
        QueuedSpawnCharacters.Queue((DataManager.GetCharacterData(charaType),rallyPoint));

        if (Performer != NetworkManager.Me) return;

        HUDManager.UpdateSpawnPreview();
    }
    public void CancelSpawn(int index)// BeforeNetworking
    {
        NetworkManager.Input(TickInput.UnqueueSpawn(index, ID));
    }
    public void UnqueueSpawn(int index)// After networking
    {
        if (index >= QueuedSpawnCharacters.Count)
            return;

        foreach (Resource.Amount cost in QueuedSpawnCharacters[index].data.Cost)
            GameManager.AddResource(cost.Type, cost.Value, Performer);

        if (index == 0)
        {
            SpawningTicks = 0;
            OnGoingSpawn = false;
            OnGoingSpawnCharacterData = (null,Vector2.zero);
        }

        QueuedSpawnCharacters.Remove(index);

        if (Performer != NetworkManager.Me) return;
        HUDManager.UpdateSpawnPreview();
    }

    public void Fill(Resource.Amount _ressourceAmount, int performer)
    {
        GameManager.AddResource(_ressourceAmount.Type, _ressourceAmount.Value, performer);
    }

    private void OnMouseOver()
    {
        if (!_isSelected && Performer == NetworkManager.Me)
            HoverMarker.SetActive(true);
    }

    private void OnMouseExit()
    {
        if (!_isSelected && Performer == NetworkManager.Me)
            HoverMarker.SetActive(false);
    }
}