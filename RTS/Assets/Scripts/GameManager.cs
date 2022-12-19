using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;
using System;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

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

    private HashSet<TickedBehaviour> _entitiesToDestroy = new HashSet<TickedBehaviour>();

    private Dictionary<ResourceType, int> _myResources = new Dictionary<ResourceType, int>();
    public static Dictionary<ResourceType, int> MyResources => _instance._myResources;

    [SerializeField]
    private List<Sprite> _resourcesSprites;
    public static List<Sprite> ResourcesSprites => _instance._resourcesSprites;

    private int _housing;
    public static int Housing => _instance._housing;

    private void Awake()
    {
        _instance = this;

        DontDestroyOnLoad(this);

        _resourcesManager = GetComponent<ResourcesManager>();

        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            _myResources[type] = 1000;
    }
    #endregion

    public static void AddResource(ResourceType type, int amount)
    {
        _instance._myResources[type] += amount;
    }

    public static void Pay(ResourceType type, int amount)
    {
        _instance._myResources[type] -= amount;
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
            if (input.ID != -1 && !_instance._entities.Contains(input.ID))
                continue;

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
                    CreateCharacter(input.Performer, input.ID, input.Prefab, input.Position);
                    break;

                case InputType.Kill:
                    Kill(input.Performer, input.Targets);
                    break;

                case InputType.NewBuild:
                    int newBuildingID = CreateBuilding(input.Performer, (Building.Type)input.Prefab, input.Position);
                    MoveCharacters(input.Performer, Vector2.zero, newBuildingID, input.Targets, MoveType.ToBuilding);
                    AssignBuild(newBuildingID, input.Targets);
                    break;

                case InputType.Build:
                    MoveCharacters(input.Performer, Vector2.zero, input.ID, input.Targets, MoveType.ToBuilding);
                    AssignBuild(input.ID, input.Targets);
                    break;

                case InputType.Destroy:
                    DestroyBuilding(input.Performer, input.ID);
                    break;

                case InputType.Move:
                    MoveCharacters(input.Performer, input.Position, -1, input.Targets, MoveType.ToPosition);
                    break;

                case InputType.Harvest:
                    Resource resource = null;
                    Vector2Int inputCoords = new Vector2Int((int)input.Position.x, (int)input.Position.y);
                    if (ResourcesManager.HasTree(input.Position))
                    {
                        resource = ResourcesManager.GetNearestForest(inputCoords);
                    }
                    else if (ResourcesManager.HasRock(input.Position))
                    {
                        resource = ResourcesManager.GetNearestAggregate(inputCoords);
                    }

                    for (int i = 0; i < input.Targets.Length; ++i)
                    {
                        Character harvester = _instance._myEntities[input.Targets[i]] as Character;

                        Vector2Int? harvestingCoords = resource.GetHarvestingPosition(inputCoords, harvester.Coords, input.Performer);
                        if (harvestingCoords == null)
                            break;

                        List<Vector2> wayPoints = LocomotionManager.RetrieveWayPoints(input.Performer, harvester, (Vector2Int)harvestingCoords);

                        Vector2Int? coordsToHarvest = resource.GetTileToHarvest((Vector2Int)harvestingCoords, inputCoords);
                        if (coordsToHarvest == null)
                            break;

                        harvester.SetAction(new Move(harvester, wayPoints));
                        harvester.AddAction(new Harvest(harvester, inputCoords, resource));
                    }
                    break;

                case InputType.Attack:
                    TickedBehaviour target = _instance._entities[input.ID];
                    MoveAndAttack(input.Performer, input.Position, input.Targets, target);
                    break;

                case InputType.GuardPosition:
                    MoveAndWatch(input.Performer, input.Position, input.Targets);
                    break;
                case InputType.GameOver:
                    if(input.Performer != NetworkManager.Me)
                    {
                        Debug.Log("Player " + input.Performer + " is bad and loses. GameOver");
                        NetworkManager.QuitRoom();
                    }
                    break;
            }
        }

        #endregion

        #region Update Views

        Vector2Int offset = Vector2Int.zero;

        foreach (TickedBehaviour entity in Entities)
            for (offset.x = -entity.ViewRadius; offset.x <= entity.ViewRadius; ++offset.x)
                for (offset.y = -entity.ViewRadius; offset.y <= entity.ViewRadius; ++offset.y)
                    if (offset.x * offset.x + offset.y * offset.y <= entity.ViewRadius * entity.ViewRadius)
                        TileMapManager.UpdateView(entity.Performer, entity.Coords + offset);

        #endregion

        #region

        HUDManager.UpdateResources(_instance._myResources[ResourceType.Crystal], _instance._myResources[ResourceType.Wood],
            _instance._myResources[ResourceType.Gold], _instance._myResources[ResourceType.Stone]);

        #endregion

        #region Destroy Pending Entities

        foreach (TickedBehaviour entity in _instance._entitiesToDestroy)
        {
            CharacterManager.TestEntitySelection(entity);

            int ID = entity.ID;

            _instance._entities.Remove(ID);
            _instance._myEntities.Remove(ID);

            if (entity is Character)
            {
                _instance._characters.Remove(ID);
                _instance._myCharacters.Remove(ID);
                QuadTreeNode.RemoveCharacter(ID);
            }

            if (entity is Building building)
            {
                _instance._buildings.Remove(ID);
                _instance._myBuildings.Remove(ID);

                TileMapManager.RemoveBuilding(building);

                if (building.Data.Type == Building.Type.HeadQuarters && building.Performer == NetworkManager.Me) //WIN CONDITION
                    _instance.GameOver();
            }

            Destroy(entity.gameObject);
        }

        if (_instance._entitiesToDestroy.Count > 0)
        {
            HUDManager.UpdateHUD();
            HUDManager.UpdateHousing();
            ToolTipManager.HideToolTip();
        }

        _instance._entitiesToDestroy.Clear();

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

    #region GamePlay Logic
    
    private void GameOver()
    {
        NetworkManager.Input(TickInput.GameOver());
        Debug.Log("You loose");
        NetworkManager.QuitRoom();
    }

    #endregion

    #region Create & Destroy TickedBehaviours

    private static void CreateCharacter(int performer, int spawnerID, int prefabID, Vector2 rallyPoint,
        bool inPlace = false, Vector2? preconfiguredSpawnPoint = null)
    {
        CharacterData data = DataManager.GetCharacterData((Character.Type)prefabID);

        Character character = TickedBehaviour.Create(performer, data.Prefab);

        _instance._entities.Add(character);
        _instance._characters.Add(character);

        if (performer == NetworkManager.Me)
        {
            _instance._myEntities.Add(character);
            _instance._myCharacters.Add(character);
        }

        HUDManager.UpdateHousing();

        character.SetPosition(inPlace ? (Vector3)preconfiguredSpawnPoint : Buildings[spawnerID].transform.position);

        QuadTreeNode.RegisterCharacter(character.ID, .3f, .5f, character.transform.position);

        if (!inPlace)
            MoveCharacters(performer, rallyPoint, -1, new int[1] { character.ID }, MoveType.ToPosition);
    }

    private static void Kill(int performer, int[] targets)
    {
        for (int i = 0; i < targets.Length; i++)
            DestroyEntity(targets[i]);
    }

    public enum MoveType
    {
        ToPosition,
        ToBuilding
    }

    private static void MoveCharacters(int performer, Vector2 position, int buildingID ,int[] targets, MoveType type) 
    {
        List<Character> characters = new List<Character>();

        for (int i = 0; i < targets.Length; i++)
            characters.Add(_instance._entities[targets[i]] as Character);

        List<List<Character>> groups = SelectionManager.MakeGroups(performer, characters.ToArray());

        foreach (List<Character> group in groups)
        {
            Vector3 targetPos = type == MoveType.ToPosition ? position : 
                TileMapManager.TilemapCoordsToWorld(_instance._buildings[buildingID].GetClosestOutlinePosition(group[0]));

            List<Vector2> wayPoints = LocomotionManager.RetrieveWayPoints(performer, group[0], TileMapManager.WorldToTilemapCoords(targetPos));

            if (wayPoints != null && wayPoints.Count != 0)
            {
                wayPoints[^1] = targetPos;

                for (int i = 0; i < group.Count; ++i)
                    group[i].SetAction(new Move(group[i], wayPoints));
            }
            else
                Debug.Log("Path not found!");
        }
    }
    private static Dictionary<List<Character>, List<Vector2>> RetrieveGroupsAndPathfindings(int performer, Vector2 position, int[] targets)
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

    #region Attack methods
    private static void MoveAndAttack(int performer, Vector2 position, int[] attackers, TickedBehaviour target)
    {
        if (target is Building building)
            position = TileMapManager.GetClosestPosAroundBuilding(building, _instance._entities[attackers[0]] as Character);

        Dictionary<List<Character>, List<Vector2>> groupsAndPathfindings = RetrieveGroupsAndPathfindings(performer, position, attackers);
        foreach (List<Character> group in groupsAndPathfindings.Keys)
        {
            if (groupsAndPathfindings[group]?.Count > 0 == true)
            {
                groupsAndPathfindings[group][^1] = position;

                for (int i = 0; i < group.Count; ++i)
                {
                    group[i].SetAction(new MoveAttack(group[i], groupsAndPathfindings[group], target));
                    group[i].AddAction(new Attack(group[i], target, position));
                }
            }
            else
                Debug.Log("Path not found!");
        }

    }
    private static void MoveAndWatch(int performer, Vector2 position, int[] supervisors)
    {

        Dictionary<List<Character>, List<Vector2>> groupsAndPathfindings = RetrieveGroupsAndPathfindings(performer, position, supervisors);
        foreach (List<Character> group in groupsAndPathfindings.Keys)
        {
            if (groupsAndPathfindings[group] != null)
            {
                groupsAndPathfindings[group][^1] = position;

                for (int i = 0; i < group.Count; ++i)
                {
                    group[i].SetAction(new Move(group[i], groupsAndPathfindings[group]));
                    group[i].BeginWatch();
                }
            }
            else
                Debug.Log("Path not found!");
        }

    }
    #endregion

    private static int CreateBuilding(int performer, Building.Type type, Vector2 position, bool autoComplete = false)
    {
        BuildingData data = DataManager.GetBuildingData(type);

        Building building = TickedBehaviour.Create(performer, data.Building, position);

        _instance._entities.Add(building);
        _instance._buildings.Add(building);

        if (performer == NetworkManager.Me)
        {
            _instance._myEntities.Add(building);
            _instance._myBuildings.Add(building);
        }

        TileMapManager.AddBuildingBlueprint(data.Size, position);

        building.SetPosition(position);

        if (autoComplete)
            building.CompleteBuild(building.Data.RequiredBuildTicks);

        return building.ID;
    }

    private static void AssignBuild(int buildingID, int[] targets)
    {
        foreach (int ID in targets)
        {
            Character builder = _instance._entities[ID] as Character;

            if (!builder)
                continue;

            builder.AddAction(new Build(builder, Buildings[buildingID]));
        }
    }

    private static void DestroyBuilding(int performer, int buildingID)
    {
        if (performer == NetworkManager.Me)
        {
            Building building = _instance._buildings[buildingID];

            if (!building.BuildComplete)
                foreach (Resource.Amount cost in building.Data.Cost)
                    _instance._myResources[cost.Type] += cost.Value;
            else
                _instance._housing -= building.Data.HousingProvided;
        }

        DestroyEntity(buildingID);
    }

    public static void DestroyEntity(int id)
    {
        _instance._entitiesToDestroy.Add(_instance._entities[id]);
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

    public static void Prepare()
    {

        DestroyAllEntities();

        FogOfWarManager.ResetFog();
        FogOfWarManager.UpdateFog();
        FogOfWarManager.SetFogActive(true);

        TileMapManager.ResetViews();

        QuadTreeNode.Init(3, 25, 25);

        for (int i = 0; i < NetworkManager.RoomSize; ++i)
        {
            Vector2 spawnPoint = _instance._spawnPoints.GetChild(i).position;

            CreateCharacter(i,-1, (int)Character.Type.Peon, Vector2.zero, true, spawnPoint + new Vector2(2f, 0));
            CreateCharacter(i, -1,(int)Character.Type.Peon, Vector2.zero, true, spawnPoint + new Vector2(-2f, 0));
            CreateCharacter(i, -1, (int)Character.Type.Peon, Vector2.zero, true, spawnPoint + new Vector2(-1f, -2f));
            CreateCharacter(i, -1,(int)Character.Type.Peon, Vector2.zero, true, spawnPoint + new Vector2(1f, -2f));

            CreateBuilding(i, Building.Type.HeadQuarters, spawnPoint , true);

            if (i == NetworkManager.Me)
                CameraMovement.SetPosition(spawnPoint);
        }
    }

    public static void UpdateHousing(int delta)
    {
        _instance._housing += delta;
        HUDManager.UpdateHousing();
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
                Gizmos.DrawWireSphere(groups[i][j].transform.position, groups[i][j].Data.AttackRange);
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
