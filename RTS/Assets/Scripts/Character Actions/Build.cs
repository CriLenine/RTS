using UnityEngine;

public class Build : Action
{
    private Building _building;
    public Build(Character peon, Building building) : base(peon)
    {
        _building = building;
        if (NetworkManager.CurrentTick > 1)
            AudioManager.PlayBlueprintSound();
    }

    protected override bool Update()
    {
        _character.Animator.Play("Build");
        return !GameManager.Buildings.Contains(_building.ID) || _building.CompleteBuild(_character.Data.BuildEfficiencyMultiplier);
    }
}
