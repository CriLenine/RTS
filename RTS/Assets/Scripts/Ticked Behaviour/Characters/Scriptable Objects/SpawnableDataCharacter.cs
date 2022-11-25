using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnableCharacter", menuName = "Spawnable/Character", order = 1)]
public class SpawnableDataCharacter : ScriptableObject
{
    [SerializeField]
    private Character.Type _type;

    [SerializeField]
    private Character _character;

    [SerializeField]
    private int _neededBuildingLevel;

    [SerializeField]
    private GameManager.RessourceCost[] _cost;

    [Header("UI")]
    [SerializeField]
    private Sprite _charaUiIcon;
    public Character.Type Type => _type;
    public Character Character => _character;
    public Sprite CharaUiIcon => _charaUiIcon;
}
