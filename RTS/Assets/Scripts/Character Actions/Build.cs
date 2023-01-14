using UnityEngine;

public class Build : Action
{
    private Character _peon;
    private Building _building;

    public Build(Character peon, Building building) : base(peon)
    {
        _peon = peon;
        _building = building;
    }

    protected override bool Update()
    {
        _character.Animator.Play("Build");
        return !GameManager.Buildings.Contains(_building.ID) || _building.CompleteBuild(_peon.Data.BuildEfficiencyMultiplier);
    }
}
