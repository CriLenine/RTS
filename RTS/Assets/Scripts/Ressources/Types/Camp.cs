using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camp : Ressource
{
    [SerializeField]
    private List<CampEntity> _entities;

    public void AddEntity(CampEntity entity) => _entities.Add(entity);
    public void Clear() => _entities?.Clear();

    public override void Tick()
    {
        return;
    }
    /// <summary>
    /// Called when an entity is killed.
    /// Removes the <paramref name="lastEntity"/> from the map.
    /// </summary>
    /// <returns>The position of the next entity to harvest, or <paramref name="lastEntity"/> if camp is empty</returns>
    public CampEntity GetNextEntity(CampEntity lastEntity)
    {
        /* 
         * TODO : update la tilemap
         */
        if (_entities.Count < 1)
            return lastEntity;
        //Find the nearest candidate in magnitude
        (float minMagnitude, int index) = (float.MaxValue, 0);

        for (int i = 0; i < _entities.Count; ++i)
        {
            float currentMagnitude = (_entities[i].transform.position - lastEntity.Position).sqrMagnitude;

            if (currentMagnitude < minMagnitude)
                (minMagnitude, index) = (currentMagnitude, i);
        }
        return _entities[index];
    }
}
