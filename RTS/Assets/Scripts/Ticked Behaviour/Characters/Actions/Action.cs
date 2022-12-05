using System.Collections.Generic;

public abstract class Action
{
    protected readonly Character _character;

    public Action(Character character)
    {
        _character = character;
    }

    /// <summary>
    /// Called every tick.
    /// </summary>
    /// <returns><see langword="true"/> if the action is completed</returns>
    public bool Perform()
    {
        if (_current is null)
            return Update();
        else if (_current.Perform())
        {
            if (_queue.Count > 0)
            {
                _current = _queue.Dequeue();

                return false;
            }

            return true;
        }

        return false;
    }

    protected abstract bool Update();

    private readonly Queue<Action> _queue = new Queue<Action>();

    private Action _current;

    protected void AddAction(Action action)
    {
        _queue.Enqueue(action);

        _current ??= _queue.Dequeue();
    }

    protected void SetAction(Action action)
    {
        _queue.Clear();

        AddAction(action);
    }
}

