public class Build : Action
{
    private Peon _peon;
    private Building _building;

    public Build(Peon peon, Building building) : base(peon)
    {
        _peon = peon;
        _building = building;

        _peon.SetBuild(building);
    }

    protected override bool Update()
    {
        if(_building.CompleteBuild(_peon.Data.BuildEfficiencyMultiplier))
        {
            _peon.SetBuild(null);
            return true;
        }

        return false;
    }
}
