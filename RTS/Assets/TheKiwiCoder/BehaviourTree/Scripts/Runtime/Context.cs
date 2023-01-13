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

        public List<Character> Enemies = new List<Character>();

        public Context(int performer)
        {
            Performer = performer;
        }
    }
}