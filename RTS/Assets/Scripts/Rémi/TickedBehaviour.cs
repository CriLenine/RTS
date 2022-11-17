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

    #endregion

    public int ID { get; private set; }
    public int Performer { get; private set; }

    public abstract void Tick();

    public abstract Hash128 GetHash128();
}
