using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {

    // This is the blackboard container shared between all nodes.
    // Use this to store temporary data that multiple nodes need read and write access to.
    // Add other properties here that make sense for your specific use case.
    [System.Serializable]
    public class Blackboard {

        public int Performer;

        public List<TickInput> Inputs = new List<TickInput>();

        public GameManager.TickedList<TickedBehaviour> Entities => GameManager.GetAIEntities(Performer);
        public GameManager.TickedList<Character> Characters => GameManager.GetAICharacters(Performer);
        public GameManager.TickedList<Building> Buildings => GameManager.GetAIBuildings(Performer);
    }
}