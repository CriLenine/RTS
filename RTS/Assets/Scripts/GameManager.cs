using UnityEngine;
using System;


public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    private void Start()
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
                    TickedBehaviour.Create(PrefabManager.GetBuildingData((PeonBuilds)input.ID).Building, input.Position);

                    break;
            }
        }

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
