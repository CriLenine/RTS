using UnityEngine;

public class CampEntity : TickedBehaviour
{
    private readonly Transform _startingPoint;
    private readonly Transform _endingPoint;

    public Vector3 Position => _startingPoint.position + (_endingPoint.position - _startingPoint.position) / 2;

    public CampEntity(Transform startingPoint, Transform endingPoint)
    {
        _startingPoint = startingPoint;
        _endingPoint = endingPoint;
    }

    public override Hash128 GetHash128()
    {
        return base.GetHash128();
    }

    public override void Tick()
    {
        transform.position += (_endingPoint.position - _startingPoint.position)/* * deltaTick*/;
        if (Vector3.Dot(_endingPoint.position - transform.position, _endingPoint.position - _startingPoint.position) <= 0)
        {
            (_endingPoint.position, _startingPoint.position) = (_startingPoint.position, _endingPoint.position);
        }
    }
}