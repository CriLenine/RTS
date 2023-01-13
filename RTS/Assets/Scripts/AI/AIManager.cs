using TheKiwiCoder;
using UnityEngine;
using System.Collections.Generic;
using MyBox;

class AIManager : MonoBehaviour
{
    private static AIManager _instance;

    [SerializeField]
    private BehaviourTree _behaviourTree;

    private List<BehaviourTree> _trees;

    private int _firstPerformerAI;
    private int _aiCount;

    private void Awake()
    {
        _instance = this;
    }

    public static void InitAI(int firstAIPerformer, int aiCount)
    {
        _instance._firstPerformerAI = firstAIPerformer;
        _instance._aiCount = aiCount;

        _instance._trees = new List<BehaviourTree>(aiCount);

        for (int i = 0; i < aiCount; ++i)
        {
            _instance._trees[i] = _instance._behaviourTree.Clone();

            _instance._trees[i].blackboard.Performer = firstAIPerformer + i;
        }

        GameManager.Init(aiCount);
    }

    public static TickInput[] Tick()
    {
        List<TickInput> inputs = new List<TickInput>();

        for (int i = 0; i < _instance._aiCount; ++i)
        {
            _instance._trees[i].blackboard.Inputs.Clear();

            _instance._trees[i].Update();

            inputs.AddRange(_instance._trees[i].blackboard.Inputs);
        }

        return inputs.ToArray();
    }
}
