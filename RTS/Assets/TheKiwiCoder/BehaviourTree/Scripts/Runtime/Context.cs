using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace TheKiwiCoder {

    // The context is a shared object every node has access to.
    // Commonly used components and subsytems should be stored here
    // It will be somewhat specfic to your game exactly what to add here.
    // Feel free to extend this class 
    public class Context
    {
        public int Performer { get; private set; }

        public List<TickInput> Inputs = new List<TickInput>();

        #region Neighbors

        public Character Leader;

        public GameManager.TickedList<TickedBehaviour> Entities => GameManager.GetAIEntities(Performer);
        public GameManager.TickedList<Character> Characters => GameManager.GetAICharacters(Performer);
        public GameManager.TickedList<Building> Buildings => GameManager.GetAIBuildings(Performer);

        public List<Character> Enemies = new List<Character>();

        public int[] AllyIds = new int[0];
        public int[] EnemyIds = new int[0];

        public int PeonCount;
        public int SoldierCount;

        #endregion

        #region Environment

        public int GetResourceAmount(ResourceType resourceType) => GameManager.PlayerResources[resourceType][Performer];

        public Dictionary<ResourceType, ResourceQueue<Vector2Int>> KnownResources = new Dictionary<ResourceType, ResourceQueue<Vector2Int>>();

        #endregion

        #region Movements

        public Vector2 TargetMovePosition;
        public Vector2? StartPosition = null;
        public Vector2Int StartCoords;
        public Vector2 BalancePosition;

        #endregion

        #region Buildings

        public int Housing => GameManager.Housing[Performer];

        #endregion

        #region Actions

        public ResourceType? HarvestedResource;

        #endregion

        public Context(int performer)
        {
            Performer = performer;

            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
                KnownResources[type] = new ResourceQueue<Vector2Int>();
        }
    }
}