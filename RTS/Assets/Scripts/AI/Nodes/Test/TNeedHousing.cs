using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

[System.Serializable]
public class TNeedHousing : ActionNode
{
    protected override State OnUpdate()
    {
        return context.Housing >= context.Characters.Count ? State.Failure : State.Success;
    }
}
