using System.Collections.Generic;
using UnityEngine;

public class HeadQuarters : Building, ISpawner, IResourceStorer
{
    private Vector2 _rallyPoint;

    private Queue<CharacterData> _queuedSpawnCharactersData = new();
    private CharacterData _onGoingSpawnCharacterData;

    private int _spawningTicks = 0;

    private bool _onGoingSpawn = false;

    public List<ResourceType> StorableResources => new List<ResourceType>
    {
        ResourceType.Crystal,
        ResourceType.Wood,
        ResourceType.Gold,
        ResourceType.Stone
    };

    private void Start()
    {
        _rallyPoint = (Vector2)transform.position + new Vector2(0.7f, 0.7f);
    }
    public override Hash128 GetHash128()
    {
        return base.GetHash128();
    }

    public override void Tick()
    {
        if (_onGoingSpawn)
        {
            _spawningTicks++;

            if (_spawningTicks >= _onGoingSpawnCharacterData.SpawnTicks && GameManager.MyCharacters.Count < GameManager.Housing)
            {
                _spawningTicks = 0;
                _onGoingSpawn = false;
                NetworkManager.Input(TickInput.Spawn((int)_onGoingSpawnCharacterData.Type, ID, _rallyPoint));
            }
        }

        if (!_onGoingSpawn && _queuedSpawnCharactersData.Count > 0)
        {
            _onGoingSpawn = true;
            _onGoingSpawnCharacterData = _queuedSpawnCharactersData.Dequeue();
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

    void ISpawner.EnqueueSpawningCharas(CharacterData data)
    {
        _queuedSpawnCharactersData.Enqueue(data);
    }
}
