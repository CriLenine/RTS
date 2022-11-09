using System;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    private void Tick(List<(int type, int id, Vector2 position)> inputs)
    {
        for(int i = 0; i < inputs.Count; i++)
        {
            if (inputs[i].type == 0) //Buildings
                TickedBehaviour.Create(PrefabManager.GetBuildingData((PeonBuilds)inputs[i].id).Building, inputs[i].position);
            else //Characters
                TickedBehaviour.Create(PrefabManager.GetCharacterData((Characters)inputs[i].id).Character, inputs[i].position);
        }
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
