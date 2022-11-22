using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.Linq;

[RequireComponent(typeof(LocomotionManager))]
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    private LocomotionManager _locomotionManager;

    #region Init & Variables

    List<TickedBehaviour> _entities;

    private void Awake()
    {
        _instance = this;

        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        _entities = new List<TickedBehaviour>();
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

    private bool LineOfSight(Vector2Int start, Vector2Int end)
    {
        int dx = end.x - start.x;
        int dy = end.y - start.y;

        int nx = Mathf.Abs(dx);
        int ny = Mathf.Abs(dy);

        int signX = dx > 0 ? 1 : -1;
        int signY = dy > 0 ? 1 : -1;

        int ix = 0, iy = 0;

        //Points.Clear();

        //Points.Add(TileMapManager.TilemapCoordsToWorld(start));

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

            //Points.Add(TileMapManager.TilemapCoordsToWorld(start));

            if (TileMapManager.GetTile(start).isObstacle)
                return false;
        }

        return true;
    }

    private List<List<Character>> MakeGroups(Character[] peons, float maxSqrtMagnitude = 100f)
    {
        List<List<Character>> groups = new List<List<Character>>();

        List<Character> openSet = new List<Character>(peons);

        int GetLeader()
        {
            Vector2 gravityCenter = Vector2.zero;

            foreach (Peon peon in peons)
                gravityCenter += (Vector2)peon.transform.position;

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

            Character leader;

            if (useGravityCenter)
            {
                leader = openSet[leaderIndex];

                openSet.RemoveAt(leaderIndex);
            }
            else
            {
                leader = openSet[0];

                openSet.RemoveAt(0);
            }

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

    public Vector2Int PStart, PEnd;
    public Vector2Int CStart, CEnd;

    public List<Vector2> Points = new List<Vector2>();
    public List<List<Character>> Groups;
    public List<List<Character>> GroupsP;
    public List<string> Names1;
    public List<string> Names2;
    public List<string> Names3;
    public List<string> Names4;
    public List<string> Names5;
    public double lag;
    public bool lineOfSight;
    public float distance = 100f;
    public bool useGravityCenter = true;
    public bool useGeoffreyAlgorithme = true;
    public bool activatePaulOpti = true;

    public Vector3 off;

    public static bool RetreiveClustersModified(Character[] characters)
    { 
        if (characters.Length < 2)
            return false;

        int minX = characters[0].Coords.x;
        int maxX = characters[0].Coords.x;
        int minY = characters[0].Coords.y;
        int maxY = characters[0].Coords.y;

        for (int i = 0; i < characters.Length; ++i)
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

    public static List<List<Character>> RetreiveClusters(Character[] characters)
    {
        List<Character> remainingCharacters = new List<Character>(characters);
        List<List<Character>> clusters = new List<List<Character>>();

        List<Character> XSorted = new List<Character>(characters);
        List<Character> YSorted = new List<Character>(characters);

        XSorted.Sort((a, b) => a.Coords.x.CompareTo(b.Coords.x));
        YSorted.Sort((a, b) => a.Coords.y.CompareTo(b.Coords.y));

        int minXindex = XSorted[0].Coords.x;
        int maxXindex = XSorted[^1].Coords.x;
        int minYindex = YSorted[0].Coords.y;
        int maxYindex = YSorted[^1].Coords.y;

        bool[,] clusteringMap;
        int[] obstaclesCount;

        (clusteringMap, obstaclesCount) = TileMapManager.RetrieveClusteringMap(minXindex, maxXindex, minYindex, maxYindex);

        for (int i = 0; i < obstaclesCount.Length; i++)
        {
            if (obstaclesCount[i] != 0)
                break;
            else if (i == obstaclesCount.Length - 1)
            {
                clusters.Add(remainingCharacters);
                return clusters;
            }
        }

        int currentXmin;
        int currentXmax;
        int currentYmin;
        int currentYmax;

        List<Character> currentCluster = new List<Character>();
        List<Character> currentXSorted = new List<Character>();
        List<Character> currentYSorted = new List<Character>();

        Vector2 mean;

        HashSet<Character> edgeCharacters = new HashSet<Character>();

        Character farthestCharacter = null;
        float farthestDistance;

        while (remainingCharacters.Count > 1)
        {
            currentCluster.Clear();
            currentCluster.AddRange(remainingCharacters);

            mean = Vector2.zero;

            currentXSorted.Clear();
            currentYSorted.Clear();
            currentXSorted.AddRange(XSorted);
            currentYSorted.AddRange(YSorted);

            while (true)
            {
                currentXmin = currentXSorted[0].Coords.x - minXindex;
                currentXmax = currentXSorted[^1].Coords.x - minXindex;
                currentYmin = currentYSorted[0].Coords.y - minYindex;
                currentYmax = currentYSorted[^1].Coords.y - minYindex;

                bool obstacleDetected = false;

                for (int y = currentYmin; y <= currentYmax && !obstacleDetected; y++)
                    if (obstaclesCount[y] != 0)
                        for(int x = currentXmin; x <= currentXmax && !obstacleDetected; x++)
                            if(clusteringMap[x, y])
                                obstacleDetected = true;

                if (!obstacleDetected)
                    break;

                for (int i = 0; i < currentCluster.Count; ++i)
                    mean += currentCluster[i].Coords;

                mean /= currentCluster.Count;

                edgeCharacters.Clear();

                edgeCharacters.Add(currentXSorted[0]);
                edgeCharacters.Add(currentXSorted[^1]);
                edgeCharacters.Add(currentYSorted[0]);
                edgeCharacters.Add(currentYSorted[^1]);

                farthestDistance = 0f;

                foreach (Character character in edgeCharacters)
                {
                    float distance = (mean - character.Coords).sqrMagnitude;
                    if (distance > farthestDistance)
                    {
                        farthestDistance = distance;
                        farthestCharacter = character;
                    }
                }

                currentXSorted.Remove(farthestCharacter);
                currentYSorted.Remove(farthestCharacter);
                currentCluster.Remove(farthestCharacter);

            }

            for(int i = 0; i < currentCluster.Count; ++i) 
            {
                Character character = currentCluster[i];

                remainingCharacters.Remove(character);
                XSorted.Remove(character);
                YSorted.Remove(character);
            }

            clusters.Add(new List<Character>(currentCluster));
        }

        if (remainingCharacters.Count == 1)
            clusters.Add(remainingCharacters);

        return clusters;
    }

    private void Update()
    {
        Character[] peons = FindObjectsOfType<Character>();

        Stopwatch sw = Stopwatch.StartNew();

        //LineOfSight(PStart, PEnd); Groups = new List<List<Peon>>();

        if (activatePaulOpti)
        {
            if (RetreiveClustersModified(peons))
                Groups = MakeGroups(peons, distance);
            else
                Groups = new List<List<Character>>() { peons.ToList() };
        }
        else
            Groups = MakeGroups(peons, distance);

        sw.Stop();

        lag = sw.Elapsed.TotalMilliseconds * 1000.0;

        UnityEngine.Debug.Log(lag);

        if (useGeoffreyAlgorithme)
        {
            for (int i = 0; i < Groups.Count; ++i)
            {
                List<string> names = new List<string>(Groups[i].Count);

                for (int j = 0; j < Groups[i].Count; ++j)
                    names.Add(Groups[i][j].name);

                if (i == 0)
                    Names1 = names;
                else if (i == 1)
                    Names2 = names;
                else if (i == 2)
                    Names3 = names;
                else if (i == 3)
                    Names4 = names;
                else if (i == 4)
                    Names5 = names;
            }
        }
        else
        {
            Names1.Clear();
            Names2.Clear();
            Names3.Clear();
            Names4.Clear();
            Names5.Clear();

            for (int i = 0; i < GroupsP.Count; ++i)
            {
                List<string> names = new List<string>(GroupsP[i].Count);

                for (int j = 0; j < GroupsP[i].Count; ++j)
                    names.Add(GroupsP[i][j].name);

                if (i == 0)
                    Names1 = names;
                else if (i == 1)
                    Names2 = names;
                else if (i == 2)
                    Names3 = names;
                else if (i == 3)
                    Names4 = names;
                else if (i == 4)
                    Names5 = names;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        List<Color> EcolorList = new List<Color>() {
            Color.red,
            Color.green,
            Color.yellow,
            Color.magenta,
            new Color(255F, 0F, 255F),
            new Color(0F, 255F, 255F),
            new Color(255F, 255F, 0F),
            new Color(128F, 0F, 128F),
            new Color(128F, 0F, 0F)
        };

        // Debug Groups

        if (useGeoffreyAlgorithme)
        {
            for (int i = 0; i < Groups.Count; ++i)
            {
                Gizmos.color = EcolorList[i];

                foreach (Peon peon in Groups[i])
                {
                    Gizmos.DrawWireSphere(peon.transform.position, TileMapManager.TileSize);
                }
            }
        }
        else
        {
            for (int i = 0; i < GroupsP.Count; ++i)
            {
                Gizmos.color = EcolorList[i];

                foreach (Peon peon in GroupsP[i])
                {
                    Gizmos.DrawWireSphere(peon.transform.position, TileMapManager.TileSize);
                }
            }
        }

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
