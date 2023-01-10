using UnityEngine;
using MyBox;

public enum TickedBehaviorType
{
    Building,
    Character
}

public abstract class TickedBehaviour : MonoBehaviour 
{
    #region Instantiation

    private static int _globalID = 0;

    private static int NextID => _globalID++;

    public static TickedBehaviour Create<T>(int performer, T data, TickedBehaviorType type) where T : TickedBehaviorData
    {
        TickedBehaviour tickedBehaviour = Instantiate(type == TickedBehaviorType.Building ? DataManager.BuildingPrefab : DataManager.CharacterPrefab) as TickedBehaviour;
        tickedBehaviour.InitData(data);

        if (performer != NetworkManager.Me)
            tickedBehaviour.DisableViewRadius();

        tickedBehaviour.ID = NextID;
        tickedBehaviour.Performer = performer;

        return tickedBehaviour;
    }

    #endregion

    public int ID { get; private set; }
    public int Performer { get; private set; }

    public Vector2 Position { get; protected set; }
    public Vector2Int Coords { get; protected set; }

    [Separator("View Specs")]

    [SerializeField]
    private int _viewRadius = 10;

    [SerializeField]
    private Transform _persistentView;

    [SerializeField]
    private Transform _currentView;

    public abstract void InitData<T>(T Data) where T : TickedBehaviorData;

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
        ApplyViewRadius();
    }

    private void ApplyViewRadius()
    {
        _persistentView.localScale = _currentView.localScale = Vector3.one * ViewRadius;
    }

    private void DisableViewRadius()
    {
        _persistentView.gameObject.SetActive(false);
        _currentView.gameObject.SetActive(false);
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
