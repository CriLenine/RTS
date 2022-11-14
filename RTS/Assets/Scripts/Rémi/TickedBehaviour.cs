using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.Arm;

public abstract class TickedBehaviour : MonoBehaviour
{
    public static int GetNextID => _globalID++;

    private static int _globalID = 0;

    private int _id;
    public static T Create<T>(T prefab, Vector3 position, Quaternion quaternion = new Quaternion()) where T : TickedBehaviour
    {
        T tickedBehaviour = Instantiate(prefab, position, quaternion);
        tickedBehaviour._id = GetNextID;

        return tickedBehaviour;
    }
    protected abstract void Tick();

    protected abstract Hash128 GetHash128();
}
