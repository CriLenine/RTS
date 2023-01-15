using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

[System.Serializable]
public class TNeedPeon : ActionNode
{
    [SerializeField]
    private int _minPeonCount;

    protected override State OnUpdate()
    {
        return context.PeonCount < _minPeonCount ? State.Success : State.Failure;
    }
}
