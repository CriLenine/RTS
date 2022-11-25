using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine.Windows;
using UnityEngine;
using System;

[RequireComponent(typeof(LocomotionManager))]
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    [SerializeField]
    private Transform _spawnPoints;

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

    private void Awake()
    {
        _instance = this;

        DontDestroyOnLoad(this);
    }
    #endregion

    #region Debug
    private static Building _buildingToBuild;
    #endregion

    [SerializeField]
    private bool _simulateWrongHash = false;

    private void Update()
    {
        _simulateWrongHash = UnityEngine.Input.GetKey(KeyCode.H);
    }

    public static int Tick(TickInput[] inputs)
    {
        foreach (TickInput input in inputs)
        {
            switch (input.Type)
            {
                case InputType.Spawn:
                    CreateCharacter(input.Performer, input.ID, input.Position);

                    break;

                case InputType.Build:
                    CreateBuilding(input.Performer, input.ID, input.Position, input.Targets);

                    break;

                case InputType.Move:
                    foreach (int ID in input.Targets)
                    {
                        Character walker = (Character)_instance._entities[ID];
                        walker.SetAction(new Move(walker, input.Position));
                    }

                    break;
            }
        }

        if (_buildingToBuild != null)
            _buildingToBuild = _buildingToBuild.AddWorkforce(2) ? null : _buildingToBuild;

        Hash128 hash = new Hash128();

        foreach (TickedBehaviour entity in _instance._entities)
        {
            entity.Tick();

            hash.Append(entity.GetHash128().GetHashCode());
        }

        return _instance._simulateWrongHash ? 0 : hash.GetHashCode();
    }

    #region Create & Destroy TickedBehaviours

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

        character.transform.position = position;

        return character;
    }

    private static Character CreateCharacter(int performer, int id, Vector2 position)
    {
        return CreateCharacter(performer, (Character.Type)id, position);
    }

    private static Building CreateBuilding(int performer, Building.Type type, Vector2 position, int[] targets)
    {
        SpawnableDataBuilding data = PrefabManager.GetBuildingData(type);

        Building building = TickedBehaviour.Create(performer, data.Building,position);

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

                builder.SetAction(new Move(builder, position));
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

            
        _buildingToBuild = building;

        return building;
    }
    private static Building CreateBuilding(int performer, int type, Vector2 position, int[] targets)
    {
        return CreateBuilding(performer, (Building.Type)type, position, targets);
    }

    public static void DestroyEntity(int id)
    {
        Destroy(_instance._entities[id]);

        _instance._entities.Remove(id);
        _instance._myEntities.Remove(id);

        _instance._characters.Remove(id);
        _instance._myCharacters.Remove(id);

        _instance._buildings.Remove(id);
        _instance._myBuildings.Remove(id);
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
        for (int i = 0; i < _instance._entities.Count; ++i)
        {
            Destroy(_instance._entities.At(i).gameObject);
            _instance._entities.RemoveAt(i);
        }

        for (int i = 0; i < NetworkManager.RoomSize; ++i)
        {
            Vector2 spawnPoint = _instance._spawnPoints.GetChild(i).position;

            CreateCharacter(i, Character.Type.Peon, spawnPoint + new Vector2(-0.5f, 0.5f));
            CreateCharacter(i, Character.Type.Peon, spawnPoint + new Vector2(0.5f, 0.5f));
            CreateCharacter(i, Character.Type.Peon, spawnPoint + new Vector2(0.5f, -0.5f));
            CreateCharacter(i, Character.Type.Peon, spawnPoint + new Vector2(-0.5f, -0.5f));

            CreateBuilding(i, Building.Type.Farm, spawnPoint + new Vector2(0f, -2f));

            if (i == NetworkManager.Me)
                CameraMovement.SetPosition(spawnPoint);
        }
    }

    #region Debug

    public readonly Color[] Colors = { new Color(1f, 0f, 0f), new Color(0f, 1f, 0f), new Color(0f, 0f, 1f), new Color(1f, 1f, 0f), new Color(1f, 0f, 1f), new Color(0f, 1f, 1f), new Color(0f, 0f, 0f), new Color(0.5019607843137255f, 0f, 0f), new Color(0f, 0.5019607843137255f, 0f), new Color(0f, 0f, 0.5019607843137255f), new Color(0.5019607843137255f, 0.5019607843137255f, 0f), new Color(0.5019607843137255f, 0f, 0.5019607843137255f), new Color(0f, 0.5019607843137255f, 0.5019607843137255f), new Color(0.5019607843137255f, 0.5019607843137255f, 0.5019607843137255f), new Color(0.7529411764705882f, 0f, 0f), new Color(0f, 0.7529411764705882f, 0f), new Color(0f, 0f, 0.7529411764705882f), new Color(0.7529411764705882f, 0.7529411764705882f, 0f), new Color(0.7529411764705882f, 0f, 0.7529411764705882f), new Color(0f, 0.7529411764705882f, 0.7529411764705882f), new Color(0.7529411764705882f, 0.7529411764705882f, 0.7529411764705882f), new Color(0.25098039215686274f, 0f, 0f), new Color(0f, 0.25098039215686274f, 0f), new Color(0f, 0f, 0.25098039215686274f), new Color(0.25098039215686274f, 0.25098039215686274f, 0f), new Color(0.25098039215686274f, 0f, 0.25098039215686274f), new Color(0f, 0.25098039215686274f, 0.25098039215686274f), new Color(0.25098039215686274f, 0.25098039215686274f, 0.25098039215686274f), new Color(0.12549019607843137f, 0f, 0f), new Color(0f, 0.12549019607843137f, 0f), new Color(0f, 0f, 0.12549019607843137f), new Color(0.12549019607843137f, 0.12549019607843137f, 0f), new Color(0.12549019607843137f, 0f, 0.12549019607843137f), new Color(0f, 0.12549019607843137f, 0.12549019607843137f), new Color(0.12549019607843137f, 0.12549019607843137f, 0.12549019607843137f), new Color(0.3764705882352941f, 0f, 0f), new Color(0f, 0.3764705882352941f, 0f), new Color(0f, 0f, 0.3764705882352941f), new Color(0.3764705882352941f, 0.3764705882352941f, 0f), new Color(0.3764705882352941f, 0f, 0.3764705882352941f), new Color(0f, 0.3764705882352941f, 0.3764705882352941f), new Color(0.3764705882352941f, 0.3764705882352941f, 0.3764705882352941f), new Color(0.6274509803921569f, 0f, 0f), new Color(0f, 0.6274509803921569f, 0f), new Color(0f, 0f, 0.6274509803921569f), new Color(0.6274509803921569f, 0.6274509803921569f, 0f), new Color(0.6274509803921569f, 0f, 0.6274509803921569f), new Color(0f, 0.6274509803921569f, 0.6274509803921569f), new Color(0.6274509803921569f, 0.6274509803921569f, 0.6274509803921569f), new Color(0.8784313725490196f, 0f, 0f), new Color(0f, 0.8784313725490196f, 0f), new Color(0f, 0f, 0.8784313725490196f), new Color(0.8784313725490196f, 0.8784313725490196f, 0f), new Color(0.8784313725490196f, 0f, 0.8784313725490196f), new Color(0f, 0.8784313725490196f, 0.8784313725490196f), new Color(0.8784313725490196f, 0.8784313725490196f, 0.8784313725490196f) };

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        // Debug Performers //

        foreach (TickedBehaviour entity in Entities)
        {
            Gizmos.color = Colors[entity.Performer];

            Gizmos.DrawCube(entity.transform.position, Vector3.one * TileMapManager.TileSize / 3f);
        }

        // Debug Groups //

        List<Character> selected = CharacterManager.SelectedCharacters();

        List<List<Character>> groups = SelectionManager.MakeGroups(selected.ToArray());

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

    [Serializable]
    public class RessourceCost
    {
        [SerializeField]
        private Ressource _ressource;

        [SerializeField]
        private int _cost;

        public Ressource Ressource => _ressource;

        public int Cost => _cost;
    }
}
