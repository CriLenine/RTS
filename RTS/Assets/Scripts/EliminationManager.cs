using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public enum EliminationCondition
{
    AllBuildingsDestroyed,
    AllUnitsKilled,
    CertainBuildingDestroyed
}

public class EliminationManager : MonoBehaviour
{
    private static EliminationManager _instance;

    private void Awake()
    {
        if (_instance != null)
            Destroy(_instance);

        _instance = this;
    }

    [SerializeField]
    private PostGameScreen _postGameScreen;

    [SerializeField]
    private EliminationCondition _eliminationCondition;

    [SerializeField, ConditionalField(nameof(_eliminationCondition), false, EliminationCondition.CertainBuildingDestroyed)]
    private Building.Type _buildingType;

    public static List<int> RemainingPlayersID { get; private set; } = new List<int>();
    private HashSet<int> _testing = new HashSet<int>();

    public static void Init()
    {
        for (int i = 0; i < NetworkManager.RoomSize; i++)
            RemainingPlayersID.Add(i);
    }

    public static void CheckForElimination()
    {
        _instance._testing.Clear();

        switch (_instance._eliminationCondition)
        {
            case EliminationCondition.AllBuildingsDestroyed:

                foreach (Building building in GameManager.Buildings)
                    _instance._testing.Add(building.Performer);

                break;

            case EliminationCondition.AllUnitsKilled:

                foreach (Character character in GameManager.Characters)
                    _instance._testing.Add(character.Performer);

                break;

            case EliminationCondition.CertainBuildingDestroyed:

                foreach (Building building in GameManager.Buildings)
                    if (building.Data.Type == _instance._buildingType)
                        _instance._testing.Add(building.Performer);

                break;
        }

        if (_instance._testing.Count < RemainingPlayersID.Count)
            _instance.EliminatePlayer();
    }

    private void EliminatePlayer()
    {
        int playerToEliminate = 0;

        foreach (int playerID in RemainingPlayersID)
            if (!_instance._testing.Contains(playerID))
            {
                playerToEliminate = playerID;
                break;
            }

        RemainingPlayersID.Remove(playerToEliminate);

        if (playerToEliminate == NetworkManager.Me)
            _instance.SetGameOver(false);

        else if (RemainingPlayersID.Count == 1)
            _instance.SetGameOver(true);
    }

    private void SetGameOver(bool gameWon)
    {
        HUDManager.HideAll();
        HUDManager.StopTimer();
        InputActionsManager.DisableInputs();

        _instance._postGameScreen.Show(gameWon);
    }
}
