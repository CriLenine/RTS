using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class TileMapManager : MonoBehaviour
{
    private static TileMapManager _instance;

    private Mouse _mouse;
    private Camera _camera;

    [Header("Tilemaps")]
    [SerializeField]
    private Grid _parentGrid;
    [SerializeField]
    private Tilemap _playableTileMap;
    [SerializeField]
    private List<Tilemap> _obstacleTilemaps;

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

        InitLogicalTiles(_playableTileMap, TileState.Free);

        for (int i = 0; i < _obstacleTilemaps.Count; ++i)
            InitLogicalTiles(_obstacleTilemaps[i], TileState.Obstacle);
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

    private void InitLogicalTiles(Tilemap tilemap, TileState state)
    {
        foreach (Vector3Int position in tilemap.cellBounds.allPositionsWithin)
            if (!tilemap.HasTile(position))
                continue;
            else
            {
                Vector2Int tileCoordinates = new Vector2Int(position.x, position.y);

                if (_tiles.ContainsKey(tileCoordinates))
                    _tiles[tileCoordinates].State = state;
                else
                    _tiles.Add(tileCoordinates, new LogicalTile(tileCoordinates, state));
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

    public static (Vector3, bool) TilesAvailableForBuild(int outlinesCount)
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
        _instance._previewMin = new Vector2Int(_instance._hoveredTileCoords.x - outlinesCount, _instance._hoveredTileCoords.y - outlinesCount);
        _instance._previewMax = new Vector2Int(_instance._hoveredTileCoords.x + outlinesCount, _instance._hoveredTileCoords.y + outlinesCount);

        // If the building steps outside of the map.
        for (int x = _instance._previewMin.x; x < _instance._previewMax.x; ++x)
            for (int y = _instance._previewMin.y; y < _instance._previewMax.y; ++y)
            {
                LogicalTile tile;

                if (!_instance._tiles.TryGetValue(new Vector2Int(x, y), out tile) || !tile.IsFree(NetworkManager.Me))
                    return (_instance._hoveredTilePos, _instance._previousAvailability);
            }

        // Else the building can be placed at this location.
        _instance._previousAvailability = true;
        return (_instance._hoveredTilePos, _instance._previousAvailability);
    }

    public static void AddBuilding(int outlinesCount, Vector2 position)
    {
        Vector2Int centerCoords = WorldToTilemapCoords(position);

        //Set building tiles
        Vector2Int _buildingMin = new Vector2Int(centerCoords.x - outlinesCount, centerCoords.y - outlinesCount);
        Vector2Int _buildingMax = new Vector2Int(centerCoords.x + outlinesCount, centerCoords.y + outlinesCount);

        UpdateTilesState(_buildingMin, _buildingMax, TileState.BuildingOutline);

        for (int x = _buildingMin.x + 1; x < _buildingMax.x; ++x)
            for (int y = _buildingMin.y + 1; y < _buildingMax.y; ++y)
                UpdateTileState(new Vector2Int(x, y), TileState.Obstacle);
    }
    public static void RemoveBuilding(Building building)
    {
        Vector2Int centerCoords = WorldToTilemapCoords(building.transform.position);
        BuildingData data = PrefabManager.GetBuildingData(building.BuildingType);
        int outlineCount = data.Outline;
        //Set building tiles
        Vector2Int _buildingMin = new Vector2Int(centerCoords.x - outlineCount, centerCoords.y - outlineCount);
        Vector2Int _buildingMax = new Vector2Int(centerCoords.x + outlineCount, centerCoords.y + outlineCount);

        UpdateTilesState(_buildingMin, _buildingMax, TileState.Free);
    }

    public static Vector2 GetClosestPosAroundBuilding(Building building,Character character)
    {
        Vector2Int pos = WorldToTilemapCoords(building.transform.position);
        Vector2Int direction = (character.Coords - pos);

        Dictionary<Vector2Int, LogicalTile> tiles = _instance._tiles;

        while(!tiles[pos].IsFree(building.Performer))
        {
            pos.x += direction.x ==0 ? 0 : (int) Mathf.Sign(direction.x);
            pos.y += direction.y == 0 ? 0 : (int) Mathf.Sign(direction.y);
        }

        return TilemapCoordsToWorld(pos);
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

    [SerializeField]
    private List<Vector2Int> _wayPoints;

    [SerializeField]
    private List<Vector2Int> _wayPointsLissed;

    /*public static List<Vector2Int> FindRessourcePath(int performer, Vector2Int startCoords, Vector2Int endCoords, TileState state, int additionnalWeight = 0)
    {
        
    }*/

    public static List<Vector2Int> FindPath(int performer, Vector2Int startCoords, Vector2Int endCoords)
    {
        if (_instance._debug)
            _instance._stopwatch = Stopwatch.StartNew();

        LogicalTile startTile = GetLogicalTile(startCoords);
        LogicalTile endTile = GetLogicalTile(endCoords);

        if (startTile is null || endTile is null)
            return null;

        List<LogicalTile> openTiles = new List<LogicalTile>();
        HashSet<LogicalTile> closedTiles = new HashSet<LogicalTile>();

        startTile.Parent = null;
        startTile.G = startTile.H = 0;

        openTiles.Add(startTile);

        while (openTiles.Count > 0)
        {
            int closestTileIndex = 0;

            for (int i = 1; i < openTiles.Count; ++i)
                if (openTiles[i].H < openTiles[closestTileIndex].H)
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

                    UnityEngine.Debug.Log($"path found in {_instance._stopwatch.Elapsed.TotalMilliseconds} ms!");
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

        _instance._stopwatch.Stop();

        UnityEngine.Debug.Log($"no path found in {_instance._stopwatch.Elapsed.TotalMilliseconds} ms!");

        return null;
    }

    #endregion

    #region Tools

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
