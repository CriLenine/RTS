using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TheKiwiCoder;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    #region Init & Variables

    public class TickedList<T> : KeyedCollection<int, T> where T : TickedBehaviour
    {
        public T At(int index)
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

    public TickedList<TickedBehaviour>[] _aiEntities;

    public static TickedList<TickedBehaviour> Entities => _instance._entities;
    public static TickedList<TickedBehaviour> MyEntities => _instance._myEntities;

    public static TickedList<TickedBehaviour> GetAIEntities(int performer) => _instance._aiEntities[performer - NetworkManager.PlayerCount];

    private TickedList<Character> _characters = new TickedList<Character>();
    private TickedList<Character> _myCharacters = new TickedList<Character>();
    public TickedList<Character>[] _aiCharacters;

    public static TickedList<Character> Characters => _instance._characters;
    public static TickedList<Character> MyCharacters => _instance._myCharacters;

    public static TickedList<Character> GetAICharacters(int performer) => _instance._aiCharacters[performer - NetworkManager.PlayerCount];

    private TickedList<Building> _buildings = new TickedList<Building>();
    private TickedList<Building> _myBuildings = new TickedList<Building>();

    public TickedList<Building>[] _aiBuildings;

    public static TickedList<Building> Buildings => _instance._buildings;
    public static TickedList<Building> MyBuildings => _instance._myBuildings;

    public static TickedList<Building> GetAIBuildings(int performer) => _instance._aiBuildings[performer - NetworkManager.PlayerCount];

    private HashSet<TickedBehaviour> _entitiesToDestroy = new HashSet<TickedBehaviour>();
    private HashSet<(int performer, int spawnerID, Character.Type type, Vector2 rallyPoint)> _characterToSpawn = new HashSet<(int, int, Character.Type, Vector2)>();

    private Dictionary<ResourceType, int[]> _playerResources = new Dictionary<ResourceType, int[]>();
    public static Dictionary<ResourceType, int[]> PlayerResources => _instance._playerResources;

    private Dictionary<int, int> _housing = new Dictionary<int, int>();
    public static Dictionary<int, int> Housing => _instance._housing;

    private void Awake()
    {
        UnityEngine.Random.InitState(0);

        _instance = this;

        DontDestroyOnLoad(this);

        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            _playerResources[type] = new int[4] { 0, 0, 0, 0 };

        _playerResources[ResourceType.Crystal] = new int[4] { 400, 400, 400, 400 };
        _playerResources[ResourceType.Wood] = new int[4] { 400, 400, 400, 400 };
        _playerResources[ResourceType.Gold] = new int[4] { 400, 400, 400, 400 };
    }

    #endregion

    public static void AddResource(ResourceType type, int amount, int performer)
    {
        _instance._playerResources[type][performer] += amount;
    }

    public static void Pay(ResourceType type, int amount, int performer)
    {
        _instance._playerResources[type][performer] -= amount;
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

        TickInput[] aiInputs = AIManager.Tick();

        List<TickInput> allInputs = new List<TickInput>();

        allInputs.AddRange(inputs);
        allInputs.AddRange(aiInputs);

        foreach (TickInput input in allInputs)
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
                case InputType.QueueSpawn:
                    if (!IsPerformerAbleToSpawn(input.Performer, input.Prefab)) break;

                    if(_instance._entities[input.ID] is Building building)
                        building.QueueSpawn((Character.Type)input.Prefab);

                    break;

                case InputType.UnqueueSpawn:

                    if (_instance._entities[input.ID] is Building spawner)
                        spawner.UnqueueSpawn(input.Prefab);

                    break;

                case InputType.UpdateRallyPoint:

                    if (_instance._entities[input.ID] is Building build)
                        build.SetRallyPoint(input.Position);

                    break;


                case InputType.Stop:
                    Stop(input.Targets);
                    break;

                case InputType.Kill:
                    Kill(input.Performer, input.Targets);
                    break;

                case InputType.NewBuild:
                    if (!IsPerformerAbleToBuild(input.Performer,input.Prefab)) break;

                    foreach (Resource.Amount cost in DataManager.GetBuildingData((Building.Type)input.Prefab).Cost)
                        Pay(cost.Type, cost.Value, input.Performer);

                    int newBuildingID = CreateBuilding(input.Performer, input.Prefab, input.Position);
                    MoveCharacters(input.Performer, Vector2.zero, newBuildingID, input.Targets, MoveType.ToBuilding);
                    AssignBuild(newBuildingID, input.Targets);
                    break;

                case InputType.Build:
                    MoveCharacters(input.Performer, Vector2.zero, input.ID, input.Targets, MoveType.ToBuilding);
                    AssignBuild(input.ID, input.Targets);
                    break;

                case InputType.Deposit:
                    MoveCharacters(input.Performer, Vector2.zero, input.ID, input.Targets, MoveType.ToBuilding);
                    AssignDeposit(input.ID, input.Targets);
                    break;

                case InputType.Destroy:
                    DestroyBuilding(input.Performer, input.ID);
                    break;

                case InputType.CancelConstruction:
                    CancelConstruction(input.Performer, input.ID);
                    break;

                case InputType.Move:
                    MoveCharacters(input.Performer, input.Position, -1, input.Targets, MoveType.ToPosition);
                    break;

                case InputType.Harvest:
                    Harvest(input.Position, input.Targets, input.Performer);
                    break;

                case InputType.Attack:
                    TickedBehaviour target = _instance._entities[input.ID];
                    MoveAndAttack(input.Performer, input.Position, input.Targets, target);
                    break;

                case InputType.GuardPosition:
                    MoveAndWatch(input.Performer, input.Position, input.Targets);
                    break;
                case InputType.GameOver:
                    if (input.Performer != NetworkManager.Me)
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

        #region Update Resources HUD
        HUDManager.UpdateResources(_instance._playerResources[ResourceType.Crystal][NetworkManager.Me], _instance._playerResources[ResourceType.Wood][NetworkManager.Me], _instance._playerResources[ResourceType.Gold][NetworkManager.Me], _instance._playerResources[ResourceType.Stone][NetworkManager.Me]);

        #endregion

        #region Compute Hash

        Hash128 hash = new Hash128();

        foreach (TickedBehaviour entity in _instance._entities)
        {
            entity.Tick();

            hash.Append(entity.GetHash128().GetHashCode());
        }

        #endregion

        #region Destroy Pending Entities

        foreach (TickedBehaviour entity in _instance._entitiesToDestroy)
        {
            SelectionManager.TestEntitySelection(entity);

            int ID = entity.ID;
            int aiId = entity.Performer - NetworkManager.PlayerCount;

            _instance._entities.Remove(ID);
            _instance._myEntities.Remove(ID);

            if (aiId >= 0)
                _instance._aiEntities[aiId].Remove(ID);

            if (entity is Character character)
            {
                _instance._characters.Remove(ID);
                _instance._performersCharaNbr[entity.Performer]--;
                _instance._myCharacters.Remove(ID);

                if (aiId >= 0)
                    _instance._aiCharacters[aiId].Remove(ID);

                QuadTreeNode.RemoveCharacter(ID);

                GameEventsManager.PlayEvent("CharacterDeath", character.gameObject);
            }
            else if (entity is Building building)
            {
                _instance._buildings.Remove(ID);
                _instance._myBuildings.Remove(ID);

                if (aiId >= 0)
                    _instance._aiBuildings[aiId].Remove(ID);

                TileMapManager.RemoveBuilding(building);


                GameEventsManager.PlayEvent("BuildingDestruction", building.gameObject);
            }

            Destroy(entity);
            EliminationManager.CheckForElimination();

            HUDManager.UpdateHousing(entity.Performer);
        }

        if (_instance._entitiesToDestroy.Count > 0)
        {
            HUDManager.UpdateHUD();
            ToolTipManager.HideToolTip();
        }

        _instance._entitiesToDestroy.Clear();

        #endregion

        #region Spawn Pending Entitie

        foreach(var (performer, spawnerID, type, rallyPoint) in _instance._characterToSpawn)
        {
            CreateCharacter(performer, spawnerID, (int)type, rallyPoint);
            HUDManager.UpdateHousing(performer);
        }

        if (_instance._characterToSpawn.Count > 0)
        {
            HUDManager.UpdateHUD();
            ToolTipManager.HideToolTip();
        }

        _instance._characterToSpawn.Clear();

        #endregion

        return _instance._simulateWrongHash ? 0 : hash.GetHashCode();
    }

    public static void Init(int aiCount)
    {
        _instance._aiEntities = new TickedList<TickedBehaviour>[aiCount];

        for (int i = 0; i < aiCount; ++i)
            _instance._aiEntities[i] = new TickedList<TickedBehaviour>();

        _instance._aiCharacters = new TickedList<Character>[aiCount];

        for (int i = 0; i < aiCount; ++i)
            _instance._aiCharacters[i] = new TickedList<Character>();

        _instance._aiBuildings = new TickedList<Building>[aiCount];

        for (int i = 0; i < aiCount; ++i)
            _instance._aiBuildings[i] = new TickedList<Building>();
    }

    private static bool IsPerformerAbleToBuild(int performer, int buildType)
    {
        var ressourcesCost = DataManager.GetBuildingData((Building.Type)buildType).Cost;

        foreach (var ressourceCost in ressourcesCost)
            if (PlayerResources[ressourceCost.Type][performer] < ressourceCost.Value)
                return false;

        return true;
    }

    private static bool IsPerformerAbleToSpawn(int performer, int charaType)
    {
        var charasCost = DataManager.GetCharacterData((Character.Type)charaType).Cost;

        foreach (var charaCost in charasCost)
            if (PlayerResources[charaCost.Type][performer] < charaCost.Value)
                return false;

        return true;
    }

    #region GamePlay Logic

    private void GameOver()
    {
        NetworkManager.Input(TickInput.GameOver());
        Debug.Log("You loose");
        NetworkManager.QuitRoom();
    }

    #endregion

    #region Apply Input Methods

    #region Move Methods

    public enum MoveType
    {
        ToPosition,
        ToBuilding
    }

    private static void MoveCharacters(int performer, Vector2 position, int buildingID, int[] targets, MoveType type)
    {
        List<Character> characters = new List<Character>();

        for (int i = 0; i < targets.Length; i++)
            characters.Add(_instance._entities[targets[i]] as Character);

        List<List<Character>> groups = GroupCreator.MakeGroups(performer, characters.ToArray());

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
        }
    }

    private static Dictionary<List<Character>, List<Vector2>> RetrieveGroupsAndPathfindings(int performer, TickedBehaviour target, int[] charasID)
    {
        Dictionary<List<Character>, List<Vector2>> output = new();
        List<Character> characters = new();
        Vector3 position;

        for (int i = 0; i < charasID.Length; i++)
            characters.Add((Character)_instance._entities[charasID[i]]);

        List<List<Character>> groups = GroupCreator.MakeGroups(performer, characters.ToArray());

        foreach (List<Character> group in groups)
        {
            position = target is Building building ? TileMapManager.TilemapCoordsToWorld(building.GetClosestOutlinePosition(group[0])) : target.transform.position;
            output.Add(group, LocomotionManager.RetrieveWayPoints(performer, group[0], TileMapManager.WorldToTilemapCoords(position)));
        }
        return output;
    }
    private static Dictionary<List<Character>, List<Vector2>> RetrieveGroupsAndPathfindings(int performer, Vector3 targetPos, int[] charasID)
    {
        Dictionary<List<Character>, List<Vector2>> output = new();
        List<Character> characters = new();

        for (int i = 0; i < charasID.Length; i++)
            characters.Add((Character)_instance._entities[charasID[i]]);

        List<List<Character>> groups = GroupCreator.MakeGroups(performer, characters.ToArray());

        foreach (List<Character> group in groups)
        {
            output.Add(group, LocomotionManager.RetrieveWayPoints(performer, group[0], TileMapManager.WorldToTilemapCoords(targetPos)));
        }
        return output;
    }

    #endregion

    #region Move& Methods
    private static void MoveAndAttack(int performer, Vector2 position, int[] attackers, TickedBehaviour target)
    {
        Dictionary<List<Character>, List<Vector2>> groupsAndPathfindings = RetrieveGroupsAndPathfindings(performer, target, attackers);
        foreach (List<Character> group in groupsAndPathfindings.Keys)
        {
            if (groupsAndPathfindings[group]?.Count > 0 == true)
            {

                groupsAndPathfindings[group][^1] = position;

                for (int i = 0; i < group.Count; ++i)
                {
                    group[i].SetAction(new MoveAttack(group[i], groupsAndPathfindings[group], target));
                    group[i].AddAction(new Attack(group[i], target,target is Building));
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

    private static void Stop(int[] targets)
    {
        foreach(int ID in targets)
            _instance._characters[ID].ClearActions();
    }

    private static void AssignBuild(int buildingID, int[] targets)
    {
        foreach (int ID in targets)
        {
            Character builder = _instance._characters[ID];

            if (builder?.Data.CanBuild != true)
                continue;

            builder.AddAction(new Build(builder, Buildings[buildingID]));
        }
    }

    private static void AssignDeposit(int buildingID, int[] targets)
    {
        foreach (int ID in targets)
        {
            Character harvester = _instance._characters[ID];

            if (harvester?.Data.CanHarvestResources != true)
                continue;

            Building building = Buildings[buildingID];

            if (!building.Data.CollectableResources.Contains(harvester.HarvestedResource.Type))
                continue;

            harvester.AddAction(new Deposit(harvester, building));
        }
    }

    private static void Harvest(Vector2 position, int[] targets, int performer)
    {
        Resource resource;
        Vector2Int inputCoords = new Vector2Int((int)position.x, (int)position.y);

        if (ResourcesManager.HasTree(position))
            resource = ResourcesManager.GetNearestForest(inputCoords, ResourceType.Wood);
        else if (ResourcesManager.HasRock(position))
            resource = ResourcesManager.GetNearestForest(inputCoords, ResourceType.Stone);
        else if (ResourcesManager.HasGold(position))
            resource = ResourcesManager.GetNearestAggregate(inputCoords, ResourceType.Gold);
        else if (ResourcesManager.HasCrystal(position))
            resource = ResourcesManager.GetNearestAggregate(inputCoords, ResourceType.Crystal);
        else
            return;

        for (int i = 0; i < targets.Length; ++i)
        {
            Character harvester = _instance._characters[targets[i]];
            if (!harvester.Data.CanHarvestResources)
                continue;

            Vector2Int? harvestingCoords = resource.GetHarvestingPosition(inputCoords, harvester.Coords, performer);
            if (harvestingCoords == null)
                break;

            List<Vector2> waypoints = LocomotionManager.RetrieveWayPoints(performer, harvester, (Vector2Int)harvestingCoords);

            if (!(waypoints?.Count != 0))
                continue;

            Vector2Int? coordsToHarvest = resource.GetTileToHarvest((Vector2Int)harvestingCoords, inputCoords);
            if (coordsToHarvest == null)
                break;

            harvester.SetAction(new Harvest(harvester, inputCoords, resource));
        }
    }

    #endregion

    #region Create & Destroy TickedBehaviours

    public static void CreateCharacter(int performer, int spawnerID, int characterType, Vector2 rallyPoint,
        bool inPlace = false, Vector2? preconfiguredSpawnPoint = null)
    {
        CharacterData characterData = DataManager.GetCharacterData((Character.Type)characterType);

        Character character = TickedBehaviour.Create(performer, characterData, TickedBehaviorType.Character) as Character;

        _instance._entities.Add(character);
        _instance._characters.Add(character);

        if (performer == NetworkManager.Me)
        {
            _instance._myEntities.Add(character);
            _instance._myCharacters.Add(character);
            HUDManager.UpdateHousing(performer);
        }

        int aiId = performer - NetworkManager.PlayerCount;

        if (aiId >= 0)
        {
            _instance._aiEntities[aiId].Add(character);
            _instance._aiCharacters[aiId].Add(character);
        }

        HUDManager.UpdateHousing(performer);
        character.SetPosition(inPlace ? (Vector3)preconfiguredSpawnPoint : Buildings[spawnerID].transform.position);

        QuadTreeNode.RegisterCharacter(character.ID, .3f, .5f, character.transform.position);

        if (!inPlace)
            MoveCharacters(performer, rallyPoint, -1, new int[1] { character.ID }, MoveType.ToPosition);
        else
            _instance._performersCharaNbr[performer]++;
    }

    private static void Kill(int performer, int[] targets)
    {
        foreach (int ID in targets)
        {
            DestroyEntity(ID);
            StatsManager.IncreaseUnitsLost(performer);
        }
    }

    public static int CreateBuilding(int performer, int buildingType, Vector2 position, bool autoComplete = false)
    {
        BuildingData data = DataManager.GetBuildingData((Building.Type)buildingType);

        Building building = TickedBehaviour.Create(performer, data, TickedBehaviorType.Building) as Building;

        _instance._entities.Add(building);
        _instance._buildings.Add(building);

        if (performer == NetworkManager.Me)
        {
            _instance._myEntities.Add(building);
            _instance._myBuildings.Add(building);
        }

        int aiId = performer - NetworkManager.PlayerCount;

        if (aiId >= 0)
        {
            _instance._aiEntities[aiId].Add(building);
            _instance._aiBuildings[aiId].Add(building);
        }
        TileMapManager.AddBuildingBlueprint(data.Size, position);
        building.SetPosition(position);

        if (data.CanSpawnUnits)
            NetworkManager.Input(TickInput.UpdateRallyPoint(building.ID, position + new Vector2(-1f, 1f)));

        if (autoComplete)
            building.CompleteBuild(building.Data.RequiredBuildTicks);

        return building.ID;
    }

    private static void DestroyBuilding(int performer, int buildingID)
    {
        Building building = _instance._buildings[buildingID];

        StatsManager.IncreaseBuildingsLost(performer);

        
        UpdateHousing(performer,-building.Data.HousingProvided);

        DestroyEntity(buildingID);
    }

    private static void CancelConstruction(int performer, int buildingID)
    {
        if (performer == NetworkManager.Me)
        {
            Building building = _instance._buildings[buildingID];

            foreach (Resource.Amount cost in building.Data.Cost)
                _instance._playerResources[cost.Type][performer] += cost.Value;
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
        _instance._aiEntities = null;



        _instance._characters.Clear();
        _instance._myCharacters.Clear();

        _instance._characterToSpawn.Clear();

        _instance._aiCharacters = null;

        _instance._buildings.Clear();
        _instance._myBuildings.Clear();
        _instance._aiBuildings = null;
    }

    private Dictionary<int, int> _performersCharaNbr = new();
    public static Dictionary<int, int> PerformersCharaNbr => _instance._performersCharaNbr;
    public static void AddEntity(int performer, int spawnerID, Character.Type type, Vector2 rallyPoint)
    {
        if (_instance._performersCharaNbr[performer] >= _instance._housing[performer]) return;

        _instance._performersCharaNbr[performer]++;
        _instance._characterToSpawn.Add((performer,spawnerID,type,rallyPoint));
    }
    #endregion

    public static void UpdateHousing(int performer, int delta)
    {
        _instance._housing[performer] += delta;
        HUDManager.UpdateHousing(performer);
    }

    public static Vector2Int startCoords;
    public static Vector2Int targetCoords;

    #region Debug

    private bool _simulateWrongHash = false;
    public readonly static Color[] Colors = { new Color(1f, 0f, 0f), new Color(0f, 1f, 0f), new Color(0f, 0f, 1f), new Color(1f, 1f, 0f), new Color(1f, 0f, 1f), new Color(0f, 1f, 1f), new Color(0f, 0f, 0f), new Color(0.5019607843137255f, 0f, 0f), new Color(0f, 0.5019607843137255f, 0f), new Color(0f, 0f, 0.5019607843137255f), new Color(0.5019607843137255f, 0.5019607843137255f, 0f), new Color(0.5019607843137255f, 0f, 0.5019607843137255f), new Color(0f, 0.5019607843137255f, 0.5019607843137255f), new Color(0.5019607843137255f, 0.5019607843137255f, 0.5019607843137255f), new Color(0.7529411764705882f, 0f, 0f), new Color(0f, 0.7529411764705882f, 0f), new Color(0f, 0f, 0.7529411764705882f), new Color(0.7529411764705882f, 0.7529411764705882f, 0f), new Color(0.7529411764705882f, 0f, 0.7529411764705882f), new Color(0f, 0.7529411764705882f, 0.7529411764705882f), new Color(0.7529411764705882f, 0.7529411764705882f, 0.7529411764705882f), new Color(0.25098039215686274f, 0f, 0f), new Color(0f, 0.25098039215686274f, 0f), new Color(0f, 0f, 0.25098039215686274f), new Color(0.25098039215686274f, 0.25098039215686274f, 0f), new Color(0.25098039215686274f, 0f, 0.25098039215686274f), new Color(0f, 0.25098039215686274f, 0.25098039215686274f), new Color(0.25098039215686274f, 0.25098039215686274f, 0.25098039215686274f), new Color(0.12549019607843137f, 0f, 0f), new Color(0f, 0.12549019607843137f, 0f), new Color(0f, 0f, 0.12549019607843137f), new Color(0.12549019607843137f, 0.12549019607843137f, 0f), new Color(0.12549019607843137f, 0f, 0.12549019607843137f), new Color(0f, 0.12549019607843137f, 0.12549019607843137f), new Color(0.12549019607843137f, 0.12549019607843137f, 0.12549019607843137f), new Color(0.3764705882352941f, 0f, 0f), new Color(0f, 0.3764705882352941f, 0f), new Color(0f, 0f, 0.3764705882352941f), new Color(0.3764705882352941f, 0.3764705882352941f, 0f), new Color(0.3764705882352941f, 0f, 0.3764705882352941f), new Color(0f, 0.3764705882352941f, 0.3764705882352941f), new Color(0.3764705882352941f, 0.3764705882352941f, 0.3764705882352941f), new Color(0.6274509803921569f, 0f, 0f), new Color(0f, 0.6274509803921569f, 0f), new Color(0f, 0f, 0.6274509803921569f), new Color(0.6274509803921569f, 0.6274509803921569f, 0f), new Color(0.6274509803921569f, 0f, 0.6274509803921569f), new Color(0f, 0.6274509803921569f, 0.6274509803921569f), new Color(0.6274509803921569f, 0.6274509803921569f, 0.6274509803921569f), new Color(0.8784313725490196f, 0f, 0f), new Color(0f, 0.8784313725490196f, 0f), new Color(0f, 0f, 0.8784313725490196f), new Color(0.8784313725490196f, 0.8784313725490196f, 0f), new Color(0.8784313725490196f, 0f, 0.8784313725490196f), new Color(0f, 0.8784313725490196f, 0.8784313725490196f), new Color(0.8784313725490196f, 0.8784313725490196f, 0.8784313725490196f) };

    public static Vector2Int dCoords;

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !Application.isEditor)
            return;

        Gizmos.color = Color.magenta;

        Gizmos.DrawCube(TileMapManager.TilemapCoordsToWorld(dCoords), Vector3.one * TileMapManager.TileSize);

        // Debug Performers //

        foreach (TickedBehaviour entity in Entities)
        {
            Gizmos.color = Colors[entity.Performer];

            Gizmos.DrawCube(entity.transform.position, Vector3.one * TileMapManager.TileSize / 3f);
        }

        // Debug Groups //

        List<List<Character>> groups = GroupCreator.MakeGroups(NetworkManager.Me, SelectionManager.SelectedCharacters.ToArray());

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