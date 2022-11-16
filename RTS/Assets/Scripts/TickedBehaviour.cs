using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.Arm;

public abstract class TickedBehaviour : MonoBehaviour
{
    public static int GetNextID => _globalID++;

    private static int _globalID = 0;

    private int _id;

    public static T CreateCharacter<T>(T prefab, Vector3 position, Quaternion quaternion = new Quaternion()) where T : Character
    {
        T tickedBehaviour = Instantiate(prefab, position, quaternion);
        //TileMapManager.AddCharacter(); TO BE IMPLEMENTED
        tickedBehaviour._id = GetNextID;

        return tickedBehaviour;
    }

    public static T CreateBuilding<T>(T prefab, Vector3 position, Quaternion quaternion = new Quaternion()) where T : Building
    {
        T tickedBehaviour = Instantiate(prefab, position, quaternion);
        //TileMapManager.AddBuilding(); TO BE IMPLEMENTED
        tickedBehaviour._id = GetNextID;

        return tickedBehaviour;
    }

    protected abstract void Tick();

    protected abstract Hash128 GetHash128();
}
