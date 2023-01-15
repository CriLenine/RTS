using UnityEngine;

public class Deposit : Action
{
    private Character _peon;
    private Building _building;

    public Deposit(Character peon, Building building) : base(peon)
    {
        _peon = peon;
        _building = building;
    }

    protected override bool Update()
    {
        //_character.Animator.Play("Deposit");

        _building.Fill(_character.HarvestedResource, _character.Performer);
        _character.SetResource(new Resource.Amount(_character.HarvestedResource.Type));

        return true;
    }
}