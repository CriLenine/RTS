using UnityEngine;

public abstract class TickedBehaviour : MonoBehaviour
{
    #region Instantiation

    private static int _globalID = 0;

    public static int NextID => _globalID++;

    public static T Create<T>(int performer, T prefab, Vector3 position, Quaternion quaternion) where T : TickedBehaviour
    {
        T tickedBehaviour = Instantiate(prefab, position, quaternion);

        tickedBehaviour.ID = NextID;
        tickedBehaviour.Performer = performer;

        return tickedBehaviour;
    }

    public static T Create<T>(int performer, T prefab, Vector3 position) where T : TickedBehaviour
    {
        return Create(performer, prefab, position, Quaternion.identity);
    }

    public static T Create<T>(int performer, T prefab) where T : TickedBehaviour
    {
        return Create(performer, prefab, Vector3.zero, Quaternion.identity);
    }

    #endregion

    public int ID { get; private set; }
    public int Performer { get; private set; }

    public abstract void Tick();

    public virtual Hash128 GetHash128()
    {
        Hash128 hash = new Hash128();

        hash.Append(Performer);
        hash.Append(ID);

        return hash;
    }
}
