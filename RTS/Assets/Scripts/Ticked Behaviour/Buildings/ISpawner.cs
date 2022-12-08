using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpawner
{
    public float SpawningTime { get;}
    public Vector2 GetRallyPoint();

    public void SetRallyPoint(Vector2 newRallyPoint);

    public void EnqueueSpawningCharas(Character.Type type);
}
