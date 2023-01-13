using TheKiwiCoder;
using UnityEngine;
using System.Collections.Generic;

class AIManager : MonoBehaviour
{
    private static AIManager _instance;

    [SerializeField]
    private BehaviourTree _behaviourTree;

    private List<BehaviourTree> _trees;

    private void Awake()
    {
        _instance = this;
    }

    public static void InitAI(int firstAIPerformer, int aiCount)
    {
        _instance._trees = new List<BehaviourTree>(aiCount);

        for (int i = 0; i < aiCount; ++i)
        {
            _instance._trees.Add(_instance._behaviourTree.Clone());

            _instance._trees[i].Bind(new Context(firstAIPerformer + i));
        }

        GameManager.Init(aiCount);
    }

    public static TickInput[] Tick()
    {
        List<TickInput> inputs = new List<TickInput>();

        for (int i = 0; i < _instance._trees.Count; ++i)
        {
            _instance._trees[i].Context.Inputs.Clear();

            _instance._trees[i].Update();

            inputs.AddRange(_instance._trees[i].Context.Inputs);
        }

        return inputs.ToArray();
    }
}
