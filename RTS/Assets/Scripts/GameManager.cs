using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(LocomotionManager))]
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    private LocomotionManager _locomotionManager;

    #region Init & Variables

    List<TickedBehaviour> _entities;

    private void Awake()
    {
        _instance = this;

        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        _entities = new List<TickedBehaviour>();
        _locomotionManager = GetComponent<LocomotionManager>();
    }

    #endregion

    public int nCharThresh = 3;
    public QuadTreeNode holyNode;
    public int x0;
    public int y0;

    public List<Transform> transformList;

    public static void Clear()
    {
        foreach (TickedBehaviour entity in _instance._entities)
            Destroy(entity.gameObject);

        _instance._entities.Clear();
    }

    public static byte[] Tick(TickInput[] inputs)
    {
        foreach (TickInput input in inputs)
        {
            switch (input.Type)
            {
                case InputType.Spawn:
                    Character character = TickedBehaviour.Create(input.Performer, PrefabManager.GetCharacterData(input.ID).Character, input.Position);

                    _instance._entities.Add(character);

                    break;

                case InputType.Build:

                    SpawnableDataBuilding data = PrefabManager.GetBuildingData(input.ID);

                    Building building = TickedBehaviour.Create(input.Performer, data.Building, input.Position);
                    TileMapManager.AddBuilding(data.Outline, input.Position);

                    foreach (int ID in input.Targets) 
                    {
                        Peon builder = (Peon)_instance._entities[ID];
                        builder.SetAction(new Move(builder, input.Position));
                        builder.AddAction(new Build(builder, building));
                    }

                    _instance._entities.Add(building);

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

        foreach (TickedBehaviour entity in _instance._entities)
            entity.Tick();

        return new byte[1];
    }

    [ContextMenu("CreateQuadTree")]
    public void CreateQuadTree()
    {
        holyNode = QuadTreeNode.Init(nCharThresh, x0 + 20, y0 + 13);

        for (int i = 0; i < transformList.Count; i++)
        {
            Transform item = transformList[i];
            holyNode.GetNeighbours(i, 1, (Vector2)item.position + new Vector2(20, 13));
        }
        Debug.Log("QuadTree created");
    }
    [ContextMenu("clear")]
    public void ClearTree()
    {
        holyNode?.Clear();
    }

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
