using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(LocomotionManager))]
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    private LocomotionManager _locomotionManager;

    #region Init & Variables

    public class TickedList : KeyedCollection<int, TickedBehaviour>
    {
        protected override int GetKeyForItem(TickedBehaviour tickedBehaviour)
        {
            return tickedBehaviour.ID;
        }
    }

    private TickedList _entities;

    public static TickedList Entities => _instance._entities;

    private void Awake()
    {
        _instance = this;

        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        _entities = new TickedList();

        _locomotionManager = GetComponent<LocomotionManager>();
    }

    #endregion

    public static void Clear()
    {
        foreach (TickedBehaviour entity in _instance._entities)
            Destroy(entity.gameObject);

        _instance._entities.Clear();
    }

    public static byte[] Tick(TickInput[] inputs)
    {
        foreach (TickInput input in inputs)
        {
            switch (input.Type)
            {
                case InputType.Spawn:
                    Character character = TickedBehaviour.Create(input.Performer, PrefabManager.GetCharacterData(input.ID).Character, input.Position);

                    _instance._entities.Add(character);

                    break;

                case InputType.Build:

                    SpawnableDataBuilding data = PrefabManager.GetBuildingData(input.ID);

                    Building building = TickedBehaviour.Create(input.Performer, data.Building, input.Position);
                    TileMapManager.AddBuilding(data.Outline, input.Position);

                    foreach (int ID in input.Targets)
                    {
                        Peon builder = (Peon)_instance._entities[ID];
                        builder.SetAction(new Move(builder, input.Position));
                        builder.AddAction(new Build(builder, building));
                    }

                    _instance._entities.Add(building);

                    break;

                case InputType.Move:

                    foreach (int ID in input.Targets)
                    {
                        Character walker = (Character)_instance._entities[ID];
                        walker.SetAction(new Move(walker, input.Position));
                    }

                    break;
            }
        }

        foreach (TickedBehaviour entity in _instance._entities)
            entity.Tick();

        return new byte[1];
    }

    private static bool LineOfSight(Vector2Int start, Vector2Int end)
    {
        int dx = end.x - start.x;
        int dy = end.y - start.y;

        int nx = Mathf.Abs(dx);
        int ny = Mathf.Abs(dy);

        int signX = dx > 0 ? 1 : -1;
        int signY = dy > 0 ? 1 : -1;

        int ix = 0, iy = 0;

        while (ix < nx || iy < ny)
        {
            int decision = (1 + 2 * ix) * ny - (1 + 2 * iy) * nx;

            if (decision == 0)
            {
                start.x += signX;
                start.y += signY;

                ++ix;
                ++iy;
            }
            else if (decision < 0)
            {
                start.x += signX;

                ++ix;
            }
            else
            {
                start.y += signY;

                ++iy;
            }

            if (TileMapManager.GetLogicalTile(start).IsObstacle)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Clusterize a list of Character taking into account the obstacles
    /// </summary>
    /// <para name="peons">Characters to clusterize</para>
    /// <para name="maxSqrtMagnitude">The square of the maximum distance to the leader of a group</para>
    /// <returns>A list of Character list</returns>
    private List<List<Character>> MakeGroups(Character[] peons, float maxSqrtMagnitude = 100f) 
    {
        /// <summary>
        /// Check if all characters can be contained in a square without obstacles
        /// </summary>
        bool IsComplexGroupsNeeded(Character[] characters)
        {
            if (characters.Length < 2)
                return false;

            int minX = characters[0].Coords.x;
            int maxX = characters[0].Coords.x;
            int minY = characters[0].Coords.y;
            int maxY = characters[0].Coords.y;

            for (int i = 1; i < characters.Length; ++i)
            {
                Vector2Int coords = characters[i].Coords;

                if (coords.x < minX)
                    minX = coords.x;

                if (coords.x > maxX)
                    maxX = coords.x;

                if (coords.y < minY)
                    minY = coords.y;

                if (coords.y > maxY)
                    maxY = coords.y;
            }

            return TileMapManager.ObstacleDetection(minX, maxX, minY, maxY);
        }

        List<List<Character>> groups = new List<List<Character>>();

        if (!IsComplexGroupsNeeded(peons))
        {
            groups.Add(new List<Character>(peons));

            return groups;
        }

        List<Character> openSet = new List<Character>(peons);

        /// <summary>
        /// Return the Character closest to the center of gravity of openSet
        /// </summary>
        int GetLeader()
        {
            Vector2 gravityCenter = Vector2.zero;

            for (int i = 0; i < peons.Length; ++i)
                gravityCenter += (Vector2)peons[i].transform.position;

            gravityCenter /= peons.Length;

            float bestSrtMagnitude = Vector2.SqrMagnitude((Vector2)openSet[0].transform.position - gravityCenter), sqrtMagnitude;
            int bestIndex = 0;

            for (int i = 1; i < openSet.Count; ++i)
            {
                sqrtMagnitude = Vector2.SqrMagnitude((Vector2)openSet[i].transform.position - gravityCenter);

                if (sqrtMagnitude < bestSrtMagnitude)
                {
                    bestIndex = i;

                    bestSrtMagnitude = sqrtMagnitude;
                }
            }

            return bestIndex;
        }

        List<Character> group;

        while (openSet.Count > 0)
        {
            int leaderIndex = GetLeader();

            Character leader = openSet[leaderIndex];

            openSet.RemoveAt(leaderIndex);

            group = new List<Character>() { leader };

            for (int i = 0; i < openSet.Count; ++i)
            {
                if (Vector2.SqrMagnitude(openSet[i].transform.position - leader.transform.position) <= maxSqrtMagnitude)
                {
                    if (LineOfSight(leader.Coords, openSet[i].Coords))
                    {
                        group.Add(openSet[i]);

                        openSet.RemoveAt(i);

                        --i;
                    }
                }
            }

            groups.Add(group);
        }

        return groups;
    }

    private void Update()
    {
        Character[] peons = FindObjectsOfType<Character>();

        Color[] colors = new Color[] { new Color(1f, 0f, 0f), new Color(0f, 1f, 0f), new Color(0f, 0f, 1f), new Color(1f, 1f, 0f), new Color(1f, 0f, 1f), new Color(0f, 1f, 1f), new Color(0f, 0f, 0f), new Color(0.5019607843137255f, 0f, 0f), new Color(0f, 0.5019607843137255f, 0f), new Color(0f, 0f, 0.5019607843137255f), new Color(0.5019607843137255f, 0.5019607843137255f, 0f), new Color(0.5019607843137255f, 0f, 0.5019607843137255f), new Color(0f, 0.5019607843137255f, 0.5019607843137255f), new Color(0.5019607843137255f, 0.5019607843137255f, 0.5019607843137255f), new Color(0.7529411764705882f, 0f, 0f), new Color(0f, 0.7529411764705882f, 0f), new Color(0f, 0f, 0.7529411764705882f), new Color(0.7529411764705882f, 0.7529411764705882f, 0f), new Color(0.7529411764705882f, 0f, 0.7529411764705882f), new Color(0f, 0.7529411764705882f, 0.7529411764705882f), new Color(0.7529411764705882f, 0.7529411764705882f, 0.7529411764705882f), new Color(0.25098039215686274f, 0f, 0f), new Color(0f, 0.25098039215686274f, 0f), new Color(0f, 0f, 0.25098039215686274f), new Color(0.25098039215686274f, 0.25098039215686274f, 0f), new Color(0.25098039215686274f, 0f, 0.25098039215686274f), new Color(0f, 0.25098039215686274f, 0.25098039215686274f), new Color(0.25098039215686274f, 0.25098039215686274f, 0.25098039215686274f), new Color(0.12549019607843137f, 0f, 0f), new Color(0f, 0.12549019607843137f, 0f), new Color(0f, 0f, 0.12549019607843137f), new Color(0.12549019607843137f, 0.12549019607843137f, 0f), new Color(0.12549019607843137f, 0f, 0.12549019607843137f), new Color(0f, 0.12549019607843137f, 0.12549019607843137f), new Color(0.12549019607843137f, 0.12549019607843137f, 0.12549019607843137f), new Color(0.3764705882352941f, 0f, 0f), new Color(0f, 0.3764705882352941f, 0f), new Color(0f, 0f, 0.3764705882352941f), new Color(0.3764705882352941f, 0.3764705882352941f, 0f), new Color(0.3764705882352941f, 0f, 0.3764705882352941f), new Color(0f, 0.3764705882352941f, 0.3764705882352941f), new Color(0.3764705882352941f, 0.3764705882352941f, 0.3764705882352941f), new Color(0.6274509803921569f, 0f, 0f), new Color(0f, 0.6274509803921569f, 0f), new Color(0f, 0f, 0.6274509803921569f), new Color(0.6274509803921569f, 0.6274509803921569f, 0f), new Color(0.6274509803921569f, 0f, 0.6274509803921569f), new Color(0f, 0.6274509803921569f, 0.6274509803921569f), new Color(0.6274509803921569f, 0.6274509803921569f, 0.6274509803921569f), new Color(0.8784313725490196f, 0f, 0f), new Color(0f, 0.8784313725490196f, 0f), new Color(0f, 0f, 0.8784313725490196f), new Color(0.8784313725490196f, 0.8784313725490196f, 0f), new Color(0.8784313725490196f, 0f, 0.8784313725490196f), new Color(0f, 0.8784313725490196f, 0.8784313725490196f), new Color(0.8784313725490196f, 0.8784313725490196f, 0.8784313725490196f) };

        List<List<Character>> groups = MakeGroups(peons);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        CharacterManager.SelectedCharacters();

        // Debug Groups

        /*for (int i = 0; i < Groups.Count; ++i)
        {
            Gizmos.color = EcolorList[i];

            foreach (Peon peon in Groups[i])
            {
                Gizmos.DrawWireSphere(peon.transform.position, TileMapManager.TileSize);
            }
        }*/

        // Debug Line Of Sight

        /*Gizmos.color = Color.grey;

        foreach (Vector2 point in Points)
            Gizmos.DrawCube(point + Vector2.one * TileMapManager.TileSize / 2f, Vector2.one * TileMapManager.TileSize);

        Gizmos.color = Color.red;

        Gizmos.DrawLine(TileMapManager.TilemapCoordsToWorld(PStart), TileMapManager.TilemapCoordsToWorld(PEnd));*/
    }

    [Serializable]
    public class RessourceCost
    {
        [SerializeField]
        private Ressource _ressource;

        [SerializeField]
        private int _cost;

        public Ressource Ressource => _ressource;

        public int Cost => _cost;
    }
}
