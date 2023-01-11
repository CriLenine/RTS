using UnityEngine;

public class StatsManager : MonoBehaviour
{
    private static StatsManager _instance;

    private void Awake()
    {
        if (_instance != null)
            Destroy(_instance);

        _instance = this;
    }

    private int[] _unitsKilledCount, _unitsLostCount, _buildingsDestroyedCount, _buildingsLostCount;
    public static int [] UnitsKilledCount => _instance._unitsKilledCount;
    public static int[] UnitsLostCount => _instance._unitsLostCount;
    public static int[] BuildingsDestroyedCount => _instance._buildingsDestroyedCount;
    public static int[] BuildingsLostCount => _instance._buildingsLostCount;

    public static void Init()
    {
        _instance._unitsKilledCount = new int[NetworkManager.RoomSize];
        _instance._unitsLostCount = new int[NetworkManager.RoomSize];
        _instance._buildingsLostCount = new int[NetworkManager.RoomSize];
        _instance._buildingsDestroyedCount = new int[NetworkManager.RoomSize];
    }

    public static void IncreaseUnitsKilled(int performer)
    {
        _instance._unitsKilledCount[performer]++;
    }
    public static void IncreaseUnitsLost(int performer)
    {
        _instance._unitsLostCount[performer]++;
    }
    public static void IncreaseBuildingsDestroyed(int performer)
    {
        _instance._buildingsDestroyedCount[performer]++;
    }
    public static void IncreaseBuildingsLost(int performer)
    {
        _instance._buildingsLostCount[performer]++;
    }
}
