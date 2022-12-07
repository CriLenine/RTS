using UnityEngine;

public class Farm : Building, ISpawner
{
    private Vector2 _rallyPoint;

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

    }

    public Vector2 GetRallyPoint()
    {
        return _rallyPoint;
    }

    public void SetRallyPoint(Vector2 newRallyPoint)
    {
        _rallyPoint = newRallyPoint;
    }
}
