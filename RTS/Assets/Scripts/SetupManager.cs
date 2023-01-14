using MyBox;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SetupManager : MonoBehaviour
{
    private static SetupManager _instance;

    [Separator("Setups"), Space]

    [SerializeField]
    private List<PlayerSpawnSetup> _spawnSetups = new List<PlayerSpawnSetup>();

    private void Awake()
    {
        if( _instance != null)
            Destroy(_instance);

        _instance = this;
    }

    public static void CompleteReset()
    {
        GameManager.DestroyAllEntities();

        FogOfWarManager.ResetFog();
        FogOfWarManager.UpdateFog();
        FogOfWarManager.SetFogActive(true);

        TileMapManager.ResetViews();
    }

    public static void SetupGame()
    {
        QuadTreeNode.Init(3, 50, 50);
        StatsManager.Init();
        EliminationManager.Init();

        for (int i = 0; i < NetworkManager.RoomSize; ++i)
        {
            PlayerSpawnSetup currentSetup = _instance._spawnSetups[i];

            GameManager.CharactersPerformer.Add(i, new List<Character>());
            GameManager.Housing.Add(i, 0);

            foreach (TickedBehaviorSpawnSetup spawnSetup in currentSetup.TickedBehaviorSpawnSetups)
                if (spawnSetup.Type == TickedBehaviorType.Character)
                    GameManager.CreateCharacter(i, -1, (int)spawnSetup.CharacterData.Type, Vector2.zero, true, spawnSetup.SpawnPoint.position);
                else
                    GameManager.CreateBuilding(i, (int)spawnSetup.BuildingData.Type, spawnSetup.SpawnPoint.position, true);

            //DEBUG
            GameManager.CreateCharacter(i, -1, (int)Character.Type.Bowman, Vector2.zero, true, new Vector2(0,i));
            //

            if (i == NetworkManager.Me)
                CameraMovement.SetPosition(currentSetup.TickedBehaviorSpawnSetups.Count > 0 ? 
                    currentSetup.TickedBehaviorSpawnSetups[0].SpawnPoint.position : Vector2.zero);


        }

        HUDManager.StartTimer();
        HUDManager.ShowDefaultHUD();
    }
}

[Serializable]
public struct PlayerSpawnSetup
{
    public List<TickedBehaviorSpawnSetup> TickedBehaviorSpawnSetups;
}

[Serializable]
public struct TickedBehaviorSpawnSetup
{
    public TickedBehaviorType Type;

    [ConditionalField(nameof(Type), false, TickedBehaviorType.Character)]
    public CharacterData CharacterData;

    [ConditionalField(nameof(Type), false, TickedBehaviorType.Building)]
    public BuildingData BuildingData;

    public Transform SpawnPoint;
}
