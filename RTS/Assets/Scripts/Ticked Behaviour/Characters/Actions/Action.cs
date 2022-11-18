public abstract class Action
{
    protected Character _character;

    public Action(Character character)
    {
        _character = character;
    }

    /// <summary>
    /// Called every tick.
    /// </summary>
    /// <returns><see langword="true"/> if the action is completed</returns>
    public abstract bool Perform();
}

