using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using System.Diagnostics;

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

    [Header("Debug")]
    [SerializeField]
    private GameObject _pathMarker;
    private List<GameObject> _pathMarkers = new List<GameObject>();
    [SerializeField]
    private bool _debug;
    private Stopwatch _stopwatch;

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
        if (_instance._tiles.ContainsKey(coords))
            return _instance._tiles[coords];

        return null;
    }

    #endregion

    #region Tests

    public static bool OutofMap(Vector2Int coords)
    {
        return _instance._tiles.ContainsKey(coords);
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

                if (!_instance._tiles.TryGetValue(new Vector2Int(x, y), out tile) || !tile.IsFree)
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
            for (int y = _buildingMin.y; y < _buildingMax.y; ++y)
                UpdateTileState(new Vector2Int(x, y), TileState.Obstacle);
    }
    public static void RemoveBuilding(Vector2Int coords)
    {
        throw new NotImplementedException("RemoveBuildingToImplement");
    }

    #endregion

    #region Displacement
    public static bool ObstacleDetection(int minX, int maxX, int minY, int maxY)
    {
        LogicalTile tile;

        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
                if (!_instance._tiles.TryGetValue(new Vector2Int(x, y), out tile) || tile.IsObstacle)
                    return true;

        return false;
    }

    #region PathFinding

    public static List<Vector2Int> FindPath(Vector2Int startCoords, Vector2Int endCoords)
    {
        if (_instance._debug)
            _instance._stopwatch = Stopwatch.StartNew();

        List<Vector2Int> wayPoints = new List<Vector2Int>();
        HashSet<Vector2Int> open = new HashSet<Vector2Int>();
        HashSet<Vector2Int> closed = new HashSet<Vector2Int>();

        LogicalTile currentTile;

        if (!_instance._tiles.TryGetValue(startCoords, out currentTile) || !_instance._tiles.ContainsKey(endCoords))
            return wayPoints;

        closed.Add(startCoords);
        currentTile.g = 0;
        currentTile.h = 0;
        currentTile.Parent = null;

        float weight;
        Vector2Int[] displacementsToTest;

        List<LogicalTile> validNeighbors = new List<LogicalTile>();

        do
        {
            Vector2Int currentCoords = currentTile.Coords;
            displacementsToTest = _instance._unitDisplacements;

            for (int displacementIndex = 0; displacementIndex < displacementsToTest.Length; ++displacementIndex)
            {
                weight = 1 + displacementIndex > 3 ? 0 : .4f;

                LogicalTile neighborTile;

                Vector2Int neighborCoords = currentCoords + displacementsToTest[displacementIndex];

                if (!_instance._tiles.TryGetValue(neighborCoords, out neighborTile) || neighborTile.IsObstacle || closed.Contains(neighborCoords))
                    continue;

                if (open.Contains(neighborCoords))
                {
                    float hypotheticG = currentTile.g + weight;
                    if (hypotheticG < neighborTile.g)
                    {
                        neighborTile.Parent = currentTile;
                        validNeighbors.Add(neighborTile);
                    }
                }
                else
                {
                    neighborTile.h = (endCoords - neighborCoords).sqrMagnitude;
                    neighborTile.g = currentTile.g + weight;
                    neighborTile.Parent = currentTile;
                    open.Add(neighborCoords);
                    validNeighbors.Add(neighborTile);
                }
            }

            if (validNeighbors.Count == 0)
            {
                if (currentTile.Coords == startCoords)
                    break;
                currentTile = currentTile.Parent;
            }
            else
            {
                LogicalTile bestTile = validNeighbors[0];

                for (int i = 1; i < validNeighbors.Count; ++i)
                    if (bestTile.f > validNeighbors[i].f)
                        bestTile = validNeighbors[i];

                bestTile.Parent = currentTile;
                currentTile = bestTile;
                closed.Add(currentTile.Coords);
            }

            validNeighbors.Clear();

        } while (open.Count > 0 && currentTile.Coords != endCoords);

        if (currentTile.Coords == startCoords)
            return wayPoints;

        wayPoints.Add(currentTile.Coords);

        Vector2Int lastDirection = currentTile.Coords - currentTile.Parent.Coords;
        Vector2Int currentDirection;

        while (currentTile.Coords != startCoords)
        {
            currentDirection = currentTile.Coords - currentTile.Parent.Coords;

            if (lastDirection != currentDirection)
                wayPoints.Add(currentTile.Coords);

            currentTile = currentTile.Parent;
            lastDirection = currentDirection;
        }

        if (_instance._debug)
        {
            _instance._stopwatch.Stop();

            if (wayPoints.Contains(endCoords))
                UnityEngine.Debug.Log($"path found in {_instance._stopwatch.Elapsed.TotalMilliseconds} ms!");
            else
                UnityEngine.Debug.Log($"no path found in {_instance._stopwatch.Elapsed.TotalMilliseconds} ms!");

            foreach (GameObject go in _instance._pathMarkers)
                Destroy(go);

            _instance._pathMarkers.Clear();

            foreach (Vector2Int tileCoords in wayPoints)
            {
                GameObject GO = Instantiate(_instance._pathMarker);
                GO.transform.position = TilemapCoordsToWorld(tileCoords);
                _instance._pathMarkers.Add(GO);
            }
        }

        return wayPoints;
    }

    #endregion

    #endregion
}

