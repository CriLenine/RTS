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

        public GameManager.TickedList<TickedBehaviour> Entities => GameManager.GetAIEntities(Performer);
        public GameManager.TickedList<Character> Characters => GameManager.GetAICharacters(Performer);
        public GameManager.TickedList<Building> Buildings => GameManager.GetAIBuildings(Performer);

        public int[] AllyIds = new int[0];
        public int[] EnemyIds = new int[0];

        public int PeonCount;
        public int SoldierCount;

        public List<Character> Enemies = new List<Character>();

        public Vector2 TargetMovePosition;
        public Vector2? StartPosition = null;
        public Vector2Int StartCoords;
        public Vector2 BalancePosition;

        public ResourceQueue<Vector2Int> Trees = new ResourceQueue<Vector2Int>();
        public ResourceQueue<Vector2Int> Crystals = new ResourceQueue<Vector2Int>();

        public Vector2Int? cuttedTree;

        public Context(int performer)
        {
            Performer = performer;
        }
    }
}