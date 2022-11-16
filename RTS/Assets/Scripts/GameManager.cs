using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [SerializeField] private LocomotionManager _locoManager;

    private static GameManager _instance;

    private List<TickedBehaviour> _tickedBehaviours = new();

    private void Awake()
    {
        _instance = this;

        DontDestroyOnLoad(this);
    }

    public static byte[] Tick(TickInput[] inputs)
    {
        foreach (TickInput input in inputs)
        {
            switch (input.Type)
            {
                case InputType.Spawn:
                    TickedBehaviour.Create(PrefabManager.GetCharacterData((Characters)input.ID).Character, input.Position);

                    break;

                case InputType.Build:
                    foreach (var chara in CharacterSelectionManager.charactersSelected)
                        chara.RallyPointGoal = Goal.Build;

                    BuildingManager.AddBuilding(TickedBehaviour.Create(PrefabManager.GetBuildingData((PeonBuilds)input.ID).Building, input.Position));
                    _instance._locoManager.SetRallyPointMethod(input.Position) ;

                    break;
            }
        }

        foreach (var tickedBehaviour in _instance._tickedBehaviours)
        {
            tickedBehaviour.Tick();
        }
        return new byte[1];
    }

    public static void AddTickedBehaviour(TickedBehaviour tickedBehaviour)
    {
        _instance._tickedBehaviours.Add(tickedBehaviour);
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
