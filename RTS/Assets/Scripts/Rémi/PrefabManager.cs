using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    [SerializeField]
    private List<SpawnableDataCharacter> _dataCharacters;

    [SerializeField]
    private List<SpawnableDataBuilding> _dataBuildings;
}
