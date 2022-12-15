using UnityEngine;

public abstract class TickedBehaviour : MonoBehaviour
{
    #region Instantiation

    private static int _globalID = 0;

    private static int NextID => _globalID++;

    public static T Create<T>(int performer, T prefab, Vector3 position, Quaternion quaternion) where T : TickedBehaviour
    {
        T tickedBehaviour = Instantiate(prefab, position, quaternion);

        if (performer != NetworkManager.Me)
            tickedBehaviour.DisableViewRadius();

        tickedBehaviour.ID = NextID;
        tickedBehaviour.Performer = performer;
        tickedBehaviour.Coords = TileMapManager.WorldToTilemapCoords(position);

        return tickedBehaviour;
    }

    public static T Create<T>(int performer, T prefab, Vector3 position) where T : TickedBehaviour
    {
        return Create(performer, prefab, position, Quaternion.identity);
    }

    public static T Create<T>(int performer, T prefab) where T : TickedBehaviour
    {
        return Create(performer, prefab, Vector3.zero, Quaternion.identity);
    }

    #endregion

    public int ID { get; private set; }
    public int Performer { get; private set; }

    public Vector2Int Coords { get; protected set; }

    [SerializeField]
    private int _viewRadius = 10;

    [SerializeField]
    private Transform _persistentView;

    [SerializeField]
    private Transform _currenttView;

    public int ViewRadius
    {
        get => _viewRadius;
        
        protected set
        {
            _viewRadius = value;

            ApplyViewRadius();
        }
    }

    protected virtual void Awake()
    {
        _persistentView = transform.GetChild(0);
        _currenttView = transform.GetChild(1);

        ApplyViewRadius();
    }

    private void ApplyViewRadius()
    {
        _persistentView.localScale = _currenttView.localScale = Vector3.one * ViewRadius;
    }

    private void DisableViewRadius()
    {
        _persistentView.gameObject.SetActive(false);
        _currenttView.gameObject.SetActive(false);
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;

        Coords = TileMapManager.WorldToTilemapCoords(position);
    }

    public abstract void Tick();

    public virtual Hash128 GetHash128()
    {
        Hash128 hash = new Hash128();

        hash.Append(Performer);
        hash.Append(ID);

        return hash;
    }
}
