public class Build : Action
{
    private Peon _peon;
    private Building _building;

    public Build(Peon peon, Building building) : base(peon)
    {
        _peon = peon;
        _building = building;   
    }

    public override bool Perform()
    {
        return _building.AddWorkforce(_peon.Data.WorkPower);
    }
}
