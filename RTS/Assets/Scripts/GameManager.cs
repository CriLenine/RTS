using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(LocomotionManager))]
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    [SerializeField]
    private Transform _spawnPoints;

    private ResourcesManager _resourcesManager;

    public static ResourcesManager ResourcesManager => _instance._resourcesManager;

    #region Init & Variables

    public class TickedList<T> : KeyedCollection<int, T> where T : TickedBehaviour
    {
        public TickedBehaviour At(int index)
        {
            return Items[index];
        }

        protected override int GetKeyForItem(T tickedBehaviour)
        {
            return tickedBehaviour.ID;
        }
    }

    private TickedList<TickedBehaviour> _entities = new TickedList<TickedBehaviour>();
    private TickedList<TickedBehaviour> _myEntities = new TickedList<TickedBehaviour>();

    public static TickedList<TickedBehaviour> Entities => _instance._entities;
    public static TickedList<TickedBehaviour> MyEntities => _instance._myEntities;

    private TickedList<Character> _characters = new TickedList<Character>();
    private TickedList<Character> _myCharacters = new TickedList<Character>();

    public static TickedList<Character> Characters => _instance._characters;
    public static TickedList<Character> MyCharacters => _instance._myCharacters;

    private TickedList<Building> _buildings = new TickedList<Building>();
    private TickedList<Building> _myBuildings = new TickedList<Building>();

    public static TickedList<Building> Buildings => _instance._buildings;
    public static TickedList<Building> MyBuildings => _instance._myBuildings;

    private HashSet<TickedBehaviour> _destroyedEntities = new HashSet<TickedBehaviour>();

    private Dictionary<ResourceType, int> _myResources = new Dictionary<ResourceType, int>();

    private void Awake()
    {
        _instance = this;

        DontDestroyOnLoad(this);

        _resourcesManager = GetComponent<ResourcesManager>();

        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            _myResources[type] = 0;
        }
    }
    #endregion

    private SpriteMask _fogRepeller;

    public static void AddResource(ResourceType type, int amount)
    {
        _instance._myResources[type] += amount;
    }
    
    public static bool Pay(ResourceType type, int amount)
    {
        if (_instance._myResources[type] < amount)
            return false;
        _instance._myResources[type] -= amount;
        return true;
    }

    private void Start()
    {
        _fogRepeller = GetComponent<SpriteMask>();
    }

    private void Update()
    {
        #region Debug

        _simulateWrongHash = Input.GetKey(KeyCode.H);

        #endregion
    }

    public static int Tick(TickInput[] inputs)
    {
        #region Apply Inputs

        foreach (TickInput input in inputs)
        {
            //test of entity existances
            if(!_instance._entities.Contains(input.ID)) continue;

            if (input.Targets is not null)
            {
                List<int> targets = new();

                for (int i = 0; i < input.Targets.Length; i++)
                {
                    if (_instance._entities.Contains(input.Targets[i]))
                        targets.Add(input.Targets[i]);
                }

                if (targets.Count == 0) continue;

                input.Targets = targets.ToArray();
            }

            switch (input.Type)
            {
                case InputType.Spawn:
                    CreateCharacter(input.Performer,input.ID, input.Prefab, input.Position);

                    break;

                case InputType.Build:
                    MoveCharacters(input.Performer, input.Position, input.Targets);
                    CreateBuilding(input.Performer, input.ID, input.Position, input.Targets);
                    break;

                case InputType.Move:
                    MoveCharacters(input.Performer, input.Position, input.Targets);

                    break;

                case InputType.Harvest:
                    Peon harvester = (Peon)_instance._myEntities[input.Targets[0]];
                    Resource resource = null;
                    Vector2Int inputCoords = new Vector2Int((int)input.Position.x, (int)input.Position.y);
                    if (_instance._resourcesManager.HasTree(input.Position))
                    {
                        resource = _instance._resourcesManager.GetNearestForest(inputCoords);
                    }
                    else if (_instance._resourcesManager.HasRock(input.Position))
                    {
                        resource = _instance._resourcesManager.GetNearestAggregate(inputCoords);
                    }
                    Vector2Int harvestingCoords = resource.GetHarvestingPosition(inputCoords, harvester.Coords);
                    harvester.SetAction(new Move(harvester, TileMapManager.TilemapCoordsToWorld(harvestingCoords)));
                    harvester.AddAction(new Harvest(harvester, resource.GetTileToHarvest(harvestingCoords), resource));
                    break;
                    
                case InputType.Attack:
                    TickedBehaviour target = _instance._entities[input.ID];
                    MoveAndAttack(input.Performer, target.transform.position, input.Targets, target);
                    break;
            }
        }

        #endregion

        #region Update Views

        Vector2Int offset = Vector2Int.zero;

        foreach (TickedBehaviour entity in Entities)
            for (offset.x = -10; offset.x <= 10; ++offset.x)
                for (offset.y = -10; offset.y <= 10; ++offset.y)
                    if (offset.x * offset.x + offset.y * offset.y < entity.ViewSqrtMagnitude)
                        TileMapManager.UpdateView(entity.Performer, entity.Coords + offset);

        #endregion

        #region Destroy Pending Entities

        foreach (TickedBehaviour entity in _instance._destroyedEntities)
        {
            CharacterManager.TestEntitieSelection(entity);

            _instance._entities.Remove(entity);
            _instance._myEntities.Remove(entity);

            if (entity is Character character)
            {
                _instance._characters.Remove(character);
                _instance._myCharacters.Remove(character);
            }

            if (entity is Building building)
            {
                _instance._buildings.Remove(building);
                _instance._myBuildings.Remove(building);

                TileMapManager.RemoveBuilding(building);
            }

            Destroy(entity.gameObject);
        }

        _instance._destroyedEntities.Clear();

        #endregion

        #region Compute Hash

        Hash128 hash = new Hash128();

        foreach (TickedBehaviour entity in _instance._entities)
        {
            entity.Tick();

            hash.Append(entity.GetHash128().GetHashCode());
        }

        #endregion

        return _instance._simulateWrongHash ? 0 : hash.GetHashCode();
    }

    #region Create & Destroy TickedBehaviours

    private static Character CreateCharacter(int performer, ISpawner spawner , Character.Type type, Vector2 position)
    {
        SpawnableDataCharacter data = PrefabManager.GetCharacterData(type);

        Character character = TickedBehaviour.Create(performer, data.Character);

        _instance._entities.Add(character);
        _instance._characters.Add(character);

        if (performer == NetworkManager.Me)
        {
            _instance._myEntities.Add(character);
            _instance._myCharacters.Add(character);
        }

        character.SetPosition(position);

        QuadTreeNode.RegisterCharacter(character.ID, .3f, .5f, position);

        Vector2Int rallypoint = TileMapManager.WorldToTilemapCoords(spawner.GetRallyPoint());
        character.AddAction(new Move(character, LocomotionManager.RetrieveWayPoints(performer,character, rallypoint).ToArray()));

        return character;
    }
    private static Character CreateCharacter(int performer, Character.Type type, Vector2 position)
    {
        SpawnableDataCharacter data = PrefabManager.GetCharacterData(type);

        Character character = TickedBehaviour.Create(performer, data.Character);

        _instance._entities.Add(character);
        _instance._characters.Add(character);

        if (performer == NetworkManager.Me)
        {
            _instance._myEntities.Add(character);
            _instance._myCharacters.Add(character);
        }

        character.SetPosition(position);

        QuadTreeNode.RegisterCharacter(character.ID, .3f, .5f, position);

        return character;
    }

    private static Character CreateCharacter(int performer,int spawnerID, int prefabID, Vector2 position)
    {
        if (_instance._entities[spawnerID].TryGetComponent(out ISpawner spawner))
            return CreateCharacter(performer, spawner, (Character.Type)prefabID, position);
        else
            throw new Exception("Not a spawner trying to spawn");
    }

    private static void MoveCharacters(int performer, Vector2 position, int[] targets) ///TO REMOVE
    {
        List<Character> characters = new List<Character>();

        for (int i = 0; i < targets.Length; i++)
            characters.Add(_instance._entities[targets[i]] as Character);

        List<List<Character>> groups = SelectionManager.MakeGroups(performer, characters.ToArray());

        foreach (List<Character> group in groups)
        {
            List<Vector2> wayPoints = LocomotionManager.RetrieveWayPoints(performer, group[0], TileMapManager.WorldToTilemapCoords(position));

            if (wayPoints is not null)
            {
                wayPoints[^1] = position;

                for (int i = 0; i < group.Count; ++i)
                    group[i].SetAction(new Move(group[i], wayPoints.ToArray()));
            }
            else
                Debug.Log("Path not found!");
        }
    }
    private static Dictionary<List<Character>,List<Vector2>> RetrieveGroupsAndPathfindings(int performer, Vector2 position, int[] targets)
    {
        Dictionary<List<Character>, List<Vector2>> output = new();
        List<Character> characters = new();

        for (int i = 0; i < targets.Length; i++)
            characters.Add((Character)_instance._entities[targets[i]]);

        List<List<Character>> groups = SelectionManager.MakeGroups(performer, characters.ToArray());

        foreach (List<Character> group in groups)
        {
            output.Add(group, LocomotionManager.RetrieveWayPoints(performer, group[0], TileMapManager.WorldToTilemapCoords(position)));
        }
        return output;
    }

    private static void MoveAndAttack(int performer, Vector2 position, int[] attackers, TickedBehaviour target)
    {
        if (target is Building building)
            position = TileMapManager.GetClosestPosAroundBuilding(building, _instance._entities[attackers[0]] as Character);

        Dictionary<List<Character>, List<Vector2>> groupsAndPathfindings = RetrieveGroupsAndPathfindings(performer,position,attackers);
        foreach (List<Character> group in groupsAndPathfindings.Keys)
        {
            if (groupsAndPathfindings[group] != null)
            {
                groupsAndPathfindings[group][^1] = position;

                for (int i = 0; i < group.Count; ++i)
                {
                    group[i].SetAction(new MoveAttack(group[i], groupsAndPathfindings[group].ToArray(),target));
                    group[i].AddAction(new Attack(group[i],target,position));
                }

            }
            else
                Debug.Log("Path not found!");
        }

    }
    private static Building CreateBuilding(int performer, Building.Type type, Vector2 position, int[] targets)
    {
        SpawnableDataBuilding data = PrefabManager.GetBuildingData(type);

        Building building = TickedBehaviour.Create(performer, data.Building, position);

        _instance._entities.Add(building);
        _instance._buildings.Add(building);

        if (performer == NetworkManager.Me)
        {
            _instance._myEntities.Add(building);
            _instance._myBuildings.Add(building);
        }

        TileMapManager.AddBuilding(data.Outline, position);

        foreach (int ID in targets)
        {
            Peon builder = (Peon)_instance._entities[ID];

            if (!builder)
                continue;

            builder.AddAction(new Build(builder, building));
        }


        return building;
    }

    private static Building CreateBuilding(int performer, Building.Type type, Vector2 position) //CreateBuildingWithoutPeon
    {
        SpawnableDataBuilding data = PrefabManager.GetBuildingData(type);

        Building building = TickedBehaviour.Create(performer, data.Building, position);

        _instance._entities.Add(building);
        _instance._buildings.Add(building);

        if (performer == NetworkManager.Me)
        {
            _instance._myEntities.Add(building);
            _instance._myBuildings.Add(building);
        }

        TileMapManager.AddBuilding(data.Outline, position);

        building.AddWorkforce(building.Data.TotalWorkforce);

        return building;
    }
    private static Building CreateBuilding(int performer, int type, Vector2 position, int[] targets)
    {
        return CreateBuilding(performer, (Building.Type)type, position, targets);
    }

    public static void DestroyEntity(int id)
    {
        _instance._destroyedEntities.Add(_instance._entities[id]);
    }

    public static void DestroyAllEntities()
    {
        for (int i = 0; i < _instance._entities.Count; ++i)
            Destroy(_instance._entities.At(i).gameObject);

        _instance._entities.Clear();
        _instance._myEntities.Clear();

        _instance._characters.Clear();
        _instance._myCharacters.Clear();

        _instance._buildings.Clear();
        _instance._myBuildings.Clear();
    }

    #endregion

    public static void Prepare() {

        DestroyAllEntities();

        TileMapManager.ResetViews();

        QuadTreeNode.Init(3, 20, 13);

        for (int i = 0; i < NetworkManager.RoomSize; ++i)
        {
            Vector2 spawnPoint = _instance._spawnPoints.GetChild(i).position;

            CreateCharacter(i, Character.Type.Peon, spawnPoint + new Vector2(-0.5f, 0.5f));
            CreateCharacter(i, Character.Type.Peon, spawnPoint + new Vector2(0.5f, 0.5f));
            CreateCharacter(i, Character.Type.Naked, spawnPoint + new Vector2(0.5f, -0.5f));
            CreateCharacter(i+1, Character.Type.Peon, spawnPoint + new Vector2(-0.5f, -0.5f));

            CreateBuilding(i+1, Building.Type.Farm, spawnPoint + new Vector2(0f, -2f));
            CreateBuilding(i, Building.Type.PlutoniumOutpost, spawnPoint + new Vector2(-4f, -2f));
            CreateBuilding(i, Building.Type.GoldOutpost, spawnPoint + new Vector2(-2f, -5f));

            if (i == NetworkManager.Me)
                CameraMovement.SetPosition(spawnPoint);
        }

        _instance._fogRepeller.enabled = false;
    }

    #region Debug

    private bool _simulateWrongHash = false;

    public readonly static Color[] Colors = { new Color(1f, 0f, 0f), new Color(0f, 1f, 0f), new Color(0f, 0f, 1f), new Color(1f, 1f, 0f), new Color(1f, 0f, 1f), new Color(0f, 1f, 1f), new Color(0f, 0f, 0f), new Color(0.5019607843137255f, 0f, 0f), new Color(0f, 0.5019607843137255f, 0f), new Color(0f, 0f, 0.5019607843137255f), new Color(0.5019607843137255f, 0.5019607843137255f, 0f), new Color(0.5019607843137255f, 0f, 0.5019607843137255f), new Color(0f, 0.5019607843137255f, 0.5019607843137255f), new Color(0.5019607843137255f, 0.5019607843137255f, 0.5019607843137255f), new Color(0.7529411764705882f, 0f, 0f), new Color(0f, 0.7529411764705882f, 0f), new Color(0f, 0f, 0.7529411764705882f), new Color(0.7529411764705882f, 0.7529411764705882f, 0f), new Color(0.7529411764705882f, 0f, 0.7529411764705882f), new Color(0f, 0.7529411764705882f, 0.7529411764705882f), new Color(0.7529411764705882f, 0.7529411764705882f, 0.7529411764705882f), new Color(0.25098039215686274f, 0f, 0f), new Color(0f, 0.25098039215686274f, 0f), new Color(0f, 0f, 0.25098039215686274f), new Color(0.25098039215686274f, 0.25098039215686274f, 0f), new Color(0.25098039215686274f, 0f, 0.25098039215686274f), new Color(0f, 0.25098039215686274f, 0.25098039215686274f), new Color(0.25098039215686274f, 0.25098039215686274f, 0.25098039215686274f), new Color(0.12549019607843137f, 0f, 0f), new Color(0f, 0.12549019607843137f, 0f), new Color(0f, 0f, 0.12549019607843137f), new Color(0.12549019607843137f, 0.12549019607843137f, 0f), new Color(0.12549019607843137f, 0f, 0.12549019607843137f), new Color(0f, 0.12549019607843137f, 0.12549019607843137f), new Color(0.12549019607843137f, 0.12549019607843137f, 0.12549019607843137f), new Color(0.3764705882352941f, 0f, 0f), new Color(0f, 0.3764705882352941f, 0f), new Color(0f, 0f, 0.3764705882352941f), new Color(0.3764705882352941f, 0.3764705882352941f, 0f), new Color(0.3764705882352941f, 0f, 0.3764705882352941f), new Color(0f, 0.3764705882352941f, 0.3764705882352941f), new Color(0.3764705882352941f, 0.3764705882352941f, 0.3764705882352941f), new Color(0.6274509803921569f, 0f, 0f), new Color(0f, 0.6274509803921569f, 0f), new Color(0f, 0f, 0.6274509803921569f), new Color(0.6274509803921569f, 0.6274509803921569f, 0f), new Color(0.6274509803921569f, 0f, 0.6274509803921569f), new Color(0f, 0.6274509803921569f, 0.6274509803921569f), new Color(0.6274509803921569f, 0.6274509803921569f, 0.6274509803921569f), new Color(0.8784313725490196f, 0f, 0f), new Color(0f, 0.8784313725490196f, 0f), new Color(0f, 0f, 0.8784313725490196f), new Color(0.8784313725490196f, 0.8784313725490196f, 0f), new Color(0.8784313725490196f, 0f, 0.8784313725490196f), new Color(0f, 0.8784313725490196f, 0.8784313725490196f), new Color(0.8784313725490196f, 0.8784313725490196f, 0.8784313725490196f) };

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !Application.isEditor)
            return;

        // Debug Performers //

        foreach (TickedBehaviour entity in Entities)
        {
            Gizmos.color = Colors[entity.Performer];

            Gizmos.DrawCube(entity.transform.position, Vector3.one * TileMapManager.TileSize / 3f);
        }

        // Debug Groups //

        List<List<Character>> groups = SelectionManager.MakeGroups(NetworkManager.Me, CharacterManager.SelectedCharacters.ToArray());

        for (int i = 0, j; i < groups.Count; ++i)
        {
            Gizmos.color = Colors[i + 4];

            for (j = 0; j < groups[i].Count; ++j)
                Gizmos.DrawWireSphere(groups[i][j].transform.position, TileMapManager.TileSize);
        }

        // Debug Line Of Sight

        /*Gizmos.color = Color.grey;

        foreach (Vector2 point in Points)
            Gizmos.DrawCube(point + Vector2.one * TileMapManager.TileSize / 2f, Vector2.one * TileMapManager.TileSize);

        Gizmos.color = Color.red;

        Gizmos.DrawLine(TileMapManager.TilemapCoordsToWorld(PStart), TileMapManager.TilemapCoordsToWorld(PEnd));*/
    }

    #endregion
}
