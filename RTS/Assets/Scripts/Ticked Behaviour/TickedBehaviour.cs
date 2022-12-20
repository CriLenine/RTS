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

    public static TickedBehaviour Create(int performer, ButtonData data, TickedBehaviorType type) 
    {
        TickedBehaviour tickedBehaviour;

        if (type == TickedBehaviorType.Building)
        {
            Building building = Instantiate(DataManager.BuildingPrefab);
            building.InitData(data as BuildingData);
            tickedBehaviour = building;
        }
        else
        {
            Character character = Instantiate(DataManager.CharacterPrefab);
            character.InitData(data as CharacterData);
            tickedBehaviour = character;
        }

        if (performer != NetworkManager.Me)
            tickedBehaviour.DisableViewRadius();

        tickedBehaviour.ID = NextID;
        tickedBehaviour.Performer = performer;

        return tickedBehaviour;
    }

    #endregion

    public int ID { get; private set; }
    public int Performer { get; private set; }

    public Vector2Int Coords { get; protected set; }

    [Separator("View Specs")]

    [SerializeField]
    private int _viewRadius = 10;

    [SerializeField]
    private Transform _persistentView;

    [SerializeField]
    private Transform _currentView;

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
