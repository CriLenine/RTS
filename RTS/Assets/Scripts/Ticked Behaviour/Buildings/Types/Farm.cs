using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Farm : Building, ISpawner
{
    private Vector2 _rallyPoint;

    private Queue<(Character.Type, CharacterData)> _spawningCharas = new();
    private (Character.Type type, CharacterData data) _currentSpawningChara;
    private float _spawningTimer = 0;

    float ISpawner.SpawningTime { get => _spawningTimer>0 ? _spawningTimer / _currentSpawningChara.data.InitialSpawningTime : 0;}

    private void Start()
    {
        _rallyPoint = (Vector2)transform.position + new Vector2(0.7f,0.7f);
        SetType(Type.Farm);
    }
    public override Hash128 GetHash128()
    {
        return base.GetHash128();
    }

    public override void Tick()
    {
        if (_spawningTimer > 0)
        {
            _spawningTimer += NetworkManager.TickPeriod;

            if (_spawningTimer >= _currentSpawningChara.data.InitialSpawningTime)
            {
                _spawningTimer = 0;
                NetworkManager.Input(TickInput.Spawn((int)_currentSpawningChara.type, ID, transform.position));
            }
        }
        else if (_spawningCharas.Count > 0)
        {
            _currentSpawningChara = _spawningCharas.Dequeue();
            _spawningTimer += NetworkManager.TickPeriod;
        }
    }

    public Vector2 GetRallyPoint()
    {
        return _rallyPoint;
    }

    public void SetRallyPoint(Vector2 newRallyPoint)
    {
        _rallyPoint = newRallyPoint;
    }

    void ISpawner.EnqueueSpawningCharas(Character.Type type)
    {
        _spawningCharas.Enqueue((type, PrefabManager.GetCharacterData(_currentSpawningChara.type)));
    }
}
