using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public enum VictoryCondition
{
    AllBuildingsDestroyed,
    AllUnitsKilled,
    CertainBuildingDestroyed
}

public class VictoryManager : MonoBehaviour
{
    private static VictoryManager _instance;

    private void Awake()
    {
        if (_instance != null)
            Destroy(_instance);

        _instance = this;
    }

    [SerializeField]
    private PostGameScreen _postGameScreen;

    [SerializeField]
    private VictoryCondition _victoryCondition;

    [SerializeField, ConditionalField(nameof(_victoryCondition), false, VictoryCondition.CertainBuildingDestroyed)]
    private Building.Type _buildingType;

    private List<int> _remainingPlayersID = new List<int>();
    private HashSet<int> _testing = new HashSet<int>();

    public static void Init(int roomSize)
    {
        for (int i = 0; i < roomSize; i++)
            _instance._remainingPlayersID.Add(i);
    }

    public static void CheckVictoryStatus()
    {
        _instance._testing.Clear();

        switch (_instance._victoryCondition)
        {
            case VictoryCondition.AllBuildingsDestroyed:

                foreach (Building building in GameManager.Buildings)
                    _instance._testing.Add(building.Performer);

                break;

            case VictoryCondition.AllUnitsKilled:

                foreach (Character character in GameManager.Characters)
                    _instance._testing.Add(character.Performer);

                break;

            case VictoryCondition.CertainBuildingDestroyed:

                foreach (Building building in GameManager.Buildings)
                    if (building.Data.Type == _instance._buildingType)
                        _instance._testing.Add(building.Performer);

                break;
        }

        if (_instance._testing.Count < _instance._remainingPlayersID.Count)
            _instance.EliminatePlayer();
    }

    private void EliminatePlayer()
    {
        int playerToEliminate = 0;

        foreach (int playerID in _instance._remainingPlayersID)
            if (!_instance._testing.Contains(playerID))
            {
                playerToEliminate = playerID;
                break;
            }

        _remainingPlayersID.Remove(playerToEliminate);

        if (playerToEliminate == NetworkManager.Me)
            _instance.DisplayPostGameScreen(false);

        else if (_remainingPlayersID.Count == 1)
            _instance.DisplayPostGameScreen(true);
    }

    private void DisplayPostGameScreen(bool gameWon)
    {
        HUDManager.HideAll();
        InputActionsManager.DisableInputs();

        _instance._postGameScreen.Show(gameWon);
    }
}
