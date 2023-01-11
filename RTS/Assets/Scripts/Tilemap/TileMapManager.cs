using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using System.Diagnostics;
using UnityEngine;
using System;

using Debug = UnityEngine.Debug;

public class TileMapManager : MonoBehaviour
{
    private static TileMapManager _instance;

    [Serializable]
    public class TilemapReference
    {
        public Tilemap Tilemap;
        public TileTag Tag;
    }

    private Mouse _mouse;
    private Camera _camera;

    [SerializeField, Header("Tilemaps")]
    private Grid _parentGrid;

    [SerializeField]
    private Tilemap _playableTileMap;

    [SerializeField]
    private List<TilemapReference> _obstacleTilemaps;

    public static float TileSize { get; private set; }

    private Dictionary<Vector2Int, LogicalTile> _tiles = new Dictionary<Vector2Int, LogicalTile>();

    private Vector2Int[] _unitDisplacements = new Vector2Int[]
    {
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(1, 1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1),
        new Vector2Int(1, -1),
    };

    // Building Positionning Tests 
    private bool _previousAvailability;
    private Vector2 _previousMousePos;
    private Vector3 _hoveredTilePos;
    private Vector2Int _hoveredTileCoords;
    private Vector2Int _previewMin;
    private Vector2Int _previewMax;

    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this);
        else
            _instance = this;

        _mouse = Mouse.current;
        _camera = Camera.main;

        TileSize = _parentGrid.cellSize.x;

        InitLogicalTiles(_playableTileMap, TileState.Free, TileTag.None);

        for (int i = 0; i < _obstacleTilemaps.Count; ++i)
            InitLogicalTiles(_obstacleTilemaps[i].Tilemap, TileState.Obstacle, _obstacleTilemaps[i].Tag);
    }

    private void Update()
    {
        #region Debug

        if (Input.GetKeyDown(KeyCode.F4))
            _debug = !_debug;

        #endregion
    }

    public static void ResetViews()
    {
        foreach (LogicalTile tile in _instance._tiles.Values)
            tile.Reset();
    }

    public static void UpdateView(int performer, Vector2Int coords)
    {
        if (_instance._tiles.ContainsKey(coords))
            _instance._tiles[coords].Update(performer);
    }

    #region Initialization

    private void InitLogicalTiles(Tilemap tilemap, TileState state, TileTag tag)
    {
        foreach (Vector3Int position in tilemap.cellBounds.allPositionsWithin)
            if (tilemap.HasTile(position))
            {
                Vector2Int tileCoordinates = new Vector2Int(position.x, position.y);

                if (_tiles.ContainsKey(tileCoordinates))
                {
                    _tiles[tileCoordinates].State = state;
                    _tiles[tileCoordinates].Tag = tag;
                }
                else
                    _tiles.Add(tileCoordinates, new LogicalTile(tileCoordinates, state, tag));
            }
    }

    #endregion

    #region Position Conversion

    public static Vector2Int WorldToTilemapCoords(Vector3 position)
    {
        return ((Vector2Int)_instance._parentGrid.WorldToCell(position));
    }

    public static Vector3 TilemapCoordsToWorld(Vector2Int coords)
    {
        return _instance._parentGrid.GetCellCenterWorld(((Vector3Int)coords));
    }

    #endregion

    #region Accessors

    public static void UpdateTileState(Vector2Int coords, TileState state)
    {
        if (_instance._tiles.ContainsKey(coords))
            _instance._tiles[coords].State = state;
    }
    public static void UpdateTilesState(Vector2Int minBounds, Vector2Int maxBounds, TileState state)
    {
        for (int x = minBounds.x; x <= maxBounds.x; ++x)
            for (int y = minBounds.y; y <= maxBounds.y; ++y)
                UpdateTileState(new Vector2Int(x, y), state);
    }

    public static LogicalTile GetLogicalTile(Vector2Int coords)
    {
        return _instance._tiles.ContainsKey(coords) ? _instance._tiles[coords] : null;
    }

    #endregion

    #region Tests

    public static bool OutofMap(Vector2Int coords)
    {
        return !_instance._tiles.ContainsKey(coords);
    }

    #endregion

    #region Buildings

    public static (Vector3, bool) TilesAvailableForBuild(int size)
    {
        // Retrieval of the mouse position.
        Vector2 currentMousePos = _instance._camera.ScreenToWorldPoint(_instance._mouse.position.ReadValue());

        // If the mouse hasn't moved, avoid unnecessary operations : return last returnedvalue.
        if (_instance._previousMousePos == currentMousePos)
            return (_instance._hoveredTilePos, _instance._previousAvailability);

        if (WorldToTilemapCoords(currentMousePos) == _instance._hoveredTileCoords)
            return (_instance._hoveredTilePos, _instance._previousAvailability);

        // Update of the "previous" variables according to the new entries.
        _instance._previousAvailability = false;
        _instance._previousMousePos = currentMousePos;

        _instance._hoveredTileCoords = WorldToTilemapCoords(currentMousePos);
        _instance._hoveredTilePos = TilemapCoordsToWorld(_instance._hoveredTileCoords);

        // Defines respectively the coordinates of the building's preview location bottom left & top right corners.
        _instance._previewMin = _instance._hoveredTileCoords;
        _instance._previewMax = new Vector2Int(_instance._hoveredTileCoords.x + size - 1, _instance._hoveredTileCoords.y + size - 1);

        // If the building steps outside of the map.
        for (int x = _instance._previewMin.x; x <= _instance._previewMax.x; ++x)
            for (int y = _instance._previewMin.y; y <= _instance._previewMax.y; ++y)
            {
                LogicalTile tile;

                if (!_instance._tiles.TryGetValue(new Vector2Int(x, y), out tile) || /*!tile.IsFree(NetworkManager.Me)*/ !(tile.State == TileState.Free))
                    return (_instance._hoveredTilePos, _instance._previousAvailability);

            }

        // Else the building can be placed at this location.
        _instance._previousAvailability = true;
        return (_instance._hoveredTilePos, _instance._previousAvailability);
    }

    public static void AddBuildingBlueprint(int size, Vector2 position)
    {
        Vector2Int bottomLeftCoords = WorldToTilemapCoords(position);

        Vector2Int buildingMin = bottomLeftCoords;
        Vector2Int buildingMax = new Vector2Int(bottomLeftCoords.x + size - 1, bottomLeftCoords.y + size - 1);

        UpdateTilesState(buildingMin, buildingMax, TileState.BuildingOutline);
    }

    public static void AddBuilding(int size, Vector2 position)
    {
        Vector2Int bottomLeftCoords = WorldToTilemapCoords(position);

        Vector2Int buildingMin = bottomLeftCoords;
        Vector2Int buildingMax = new Vector2Int(bottomLeftCoords.x + size - 1, bottomLeftCoords.y + size - 1);

        for (int x = buildingMin.x + 1; x < buildingMax.x; ++x)
            for (int y = buildingMin.y + 1; y < buildingMax.y; ++y)
                UpdateTileState(new Vector2Int(x, y), TileState.Obstacle);
    }

    public static void RemoveBuilding(Building building)
    {
        Vector2Int bottomLeftCorner = building.Coords;
        int size = building.Data.Size;

        //Set building tiles
        Vector2Int buildingMin = bottomLeftCorner;
        Vector2Int buildingMax = new Vector2Int(bottomLeftCorner.x + size - 1, bottomLeftCorner.y + size - 1);

        UpdateTilesState(buildingMin, buildingMax, TileState.Free);
    }

    #endregion

    #region Displacement

    public static bool ObstacleDetection(int performer, int minX, int maxX, int minY, int maxY)
    {
        LogicalTile tile;

        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
                if (!_instance._tiles.TryGetValue(new Vector2Int(x, y), out tile) || !tile.IsFree(performer))
                    return true;

        return false;
    }

    #endregion

    #region PathFinding

    private List<Vector2Int> _wayPoints = new List<Vector2Int>();
    private List<Vector2Int> _wayPointsLissed = new List<Vector2Int>();

    public static LogicalTile FindPathWithTag(int performer, Vector2Int startCoords, Vector2Int endCoords, TileTag tag, int additionnalWeight = 0)
    {
        if (_instance._debug)
            _instance._stopwatch = Stopwatch.StartNew();

        LogicalTile startTile = GetLogicalTile(startCoords);
        LogicalTile endTile = GetLogicalTile(endCoords);

        if (startTile is null || endTile is null)
        {
            if (_instance._debug)
                Debug.Log($"Start tile is {startTile} ; EndTile is {endTile}");

            return null;
        }

        List<LogicalTile> openTiles = new List<LogicalTile>();
        HashSet<LogicalTile> closedTiles = new HashSet<LogicalTile>();

        startTile.Parent = null;
        startTile.G = startTile.H = 0;

        openTiles.Add(startTile);

        while (openTiles.Count > 0)
        {
            int closestTileIndex = 0;

            for (int i = 1; i < openTiles.Count; ++i)
                if (openTiles[i].F < openTiles[closestTileIndex].F)
                    closestTileIndex = i;

            LogicalTile currentTile = openTiles[closestTileIndex];
            openTiles.RemoveAt(closestTileIndex);

            if (currentTile == endTile || currentTile.Tag == tag)
                return currentTile.Parent ?? startTile;

            for (int moveIndex = 0; moveIndex < _instance._unitDisplacements.Length; ++moveIndex)
            {
                LogicalTile neighborTile = GetLogicalTile(currentTile.Coords + _instance._unitDisplacements[moveIndex]);

                if (neighborTile is null || closedTiles.Contains(neighborTile))
                    continue;

                if (neighborTile.IsObstacle(performer) && neighborTile.Tag != tag)
                    continue;

                if (neighborTile.Tag == tag && moveIndex >= 4)
                    continue;

                float moveWeight = moveIndex < 4 ? 1f : 1.4f;

                if (neighborTile.Tag == tag)
                    moveWeight += additionnalWeight;

                float g = currentTile.G + moveWeight;

                bool isNew = !openTiles.Contains(neighborTile);

                if (g < neighborTile.G || isNew)
                {
                    neighborTile.G = g;

                    neighborTile.H = (endTile.Coords - neighborTile.Coords).sqrMagnitude;

                    neighborTile.Parent = currentTile;
                }

                if (isNew)
                    openTiles.Add(neighborTile);
            }

            closedTiles.Add(currentTile);
        }

        if (_instance._debug)
        {
            _instance._stopwatch.Stop();

            Debug.Log($"no path found in {_instance._stopwatch.Elapsed.TotalMilliseconds} ms!");
        }

        return null;
    }

    public static List<Vector2Int> FindPath(int performer, Vector2Int startCoords, Vector2Int endCoords)
    {
        if (_instance._debug)
            _instance._stopwatch = Stopwatch.StartNew();

        LogicalTile startTile = GetLogicalTile(startCoords);
        LogicalTile endTile = GetLogicalTile(endCoords);

        if (startTile is null || endTile is null)
        {
            if (_instance._debug)
                Debug.Log($"Start tile is {startTile} ; EndTile is {endTile}");

            return null;
        }

        List<LogicalTile> openTiles = new List<LogicalTile>();
        HashSet<LogicalTile> closedTiles = new HashSet<LogicalTile>();

        startTile.Parent = null;
        startTile.G = startTile.H = 0;

        openTiles.Add(startTile);

        while (openTiles.Count > 0)
        {
            int closestTileIndex = 0;

            for (int i = 1; i < openTiles.Count; ++i)
                if (openTiles[i].F < openTiles[closestTileIndex].F)
                    closestTileIndex = i;

            LogicalTile currentTile = openTiles[closestTileIndex];
            openTiles.RemoveAt(closestTileIndex);

            if (currentTile == endTile)
            {
                List<Vector2Int> path = new List<Vector2Int>();
                _instance._wayPoints = new List<Vector2Int>();

                Vector2Int lastDirection = Vector2Int.zero;

                while (currentTile != startTile)
                {
                    Vector2Int direction = currentTile.Parent.Coords - currentTile.Coords;

                    _instance._wayPoints.Add(currentTile.Coords);

                    if (direction != lastDirection)
                        path.Add(currentTile.Coords);

                    lastDirection = direction;
                    currentTile = currentTile.Parent;
                }

                _instance._wayPoints.Add(startTile.Coords);
                path.Add(startTile.Coords);

                _instance._wayPointsLissed = path;

                if (_instance._debug)
                {
                    _instance._stopwatch.Stop();

                    Debug.Log($"path found in {_instance._stopwatch.Elapsed.TotalMilliseconds} ms!");
                }

                return path;
            }

            for (int moveIndex = 0; moveIndex < _instance._unitDisplacements.Length; ++moveIndex)
            {
                float moveWeight = moveIndex < 4 ? 1f : 1.4f;

                LogicalTile neighborTile = GetLogicalTile(currentTile.Coords + _instance._unitDisplacements[moveIndex]);

                if (neighborTile is null || closedTiles.Contains(neighborTile) || neighborTile.IsObstacle(performer))
                    continue;

                float g = currentTile.G + moveWeight;

                bool isNew = !openTiles.Contains(neighborTile);

                if (g < neighborTile.G || isNew)
                {
                    neighborTile.G = g;

                    neighborTile.H = (endTile.Coords - neighborTile.Coords).sqrMagnitude;

                    neighborTile.Parent = currentTile;
                }

                if (isNew)
                    openTiles.Add(neighborTile);
            }

            closedTiles.Add(currentTile);
        }

        if (_instance._debug)
        {
            _instance._stopwatch.Stop();

            Debug.Log($"no path found in {_instance._stopwatch.Elapsed.TotalMilliseconds} ms!");
        }

        return null;
    }

    #endregion

    #region Tools

    /// <summary>
    /// Finds the <paramref name="availableTiles"/>'s closest element to <paramref name="attractionPoint"/> in euclidean distance.
    /// </summary>
    public static Vector2Int FindClosestCoords(List<Vector2Int> availableTiles, Vector2Int attractionPoint)
    {
        (int minMagnitude, int index) = ((availableTiles[0] - attractionPoint).sqrMagnitude, 0);
        for (int i = 1; i < availableTiles.Count; i++)
        {
            int currentMagnitude = (availableTiles[i] - attractionPoint).sqrMagnitude;
            if (currentMagnitude < minMagnitude)
                (minMagnitude, index) = (currentMagnitude, i);
        }
        return availableTiles[index];
    }

    public static bool LineOfSight(int performer, Vector2Int start, Vector2Int end)
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

            if (!GetLogicalTile(start).IsFree(performer))
                return false;
        }

        return true;
    }

    #endregion

    #region Debug

    private bool _debug = false;

    private Stopwatch _stopwatch;

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !Application.isEditor)
            return;

        if (!_debug)
            return;

        foreach (LogicalTile tile in _instance._tiles.Values)
        {
            Gizmos.color = tile.IsFree(0) ? Color.green : Color.red;

            //Gizmos.color = tile.Tag == TileTag.None ? Color.green : tile.Tag == TileTag.Tree ? Color.blue : Color.red;


            Gizmos.DrawWireSphere(TilemapCoordsToWorld(tile.Coords), TileSize / 3f);
        }

        foreach (Vector2Int waypoint in _wayPoints)
        {
            Gizmos.color = _wayPointsLissed.Contains(waypoint) ? Color.yellow : Color.blue;

            Gizmos.DrawWireSphere(TilemapCoordsToWorld(waypoint), TileSize / 5f);
        }
    }

    #endregion
}
