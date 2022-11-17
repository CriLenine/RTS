using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    [SerializeField] private LocomotionManager _locoManager;

    private static GameManager _instance;

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
    }

    #endregion

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
                    _instance._entities.Add(TickedBehaviour.Create(input.Performer, PrefabManager.GetCharacterData(input.ID).Character, input.Position));

                    break;

                case InputType.Build:
                    foreach (var chara in CharacterSelectionManager.charactersSelected)
                        chara.RallyPointGoal = Goal.Build;

                    Building building = TickedBehaviour.Create(input.Performer, PrefabManager.GetBuildingData(input.ID).Building, input.Position);

                    _instance._entities.Add(building);

                    BuildingManager.AddBuilding(building);
                    _instance._locoManager.SetRallyPointMethod(input.Position) ;

                    

                    break;
            }
        }

        foreach (TickedBehaviour entity in _instance._entities)
            entity.Tick();

        return new byte[1];
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
