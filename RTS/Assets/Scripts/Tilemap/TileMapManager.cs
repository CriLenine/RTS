using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class TileMapManager : MonoBehaviour
{
    private static TileMapManager _instance;

    private Mouse _mouse;
    private Camera _camera;

    [SerializeField]
    private Tilemap _graphicGroundTilemap, _graphicObstaclesTilemap, _graphicPreviewTilemap;

    [SerializeField]
    private int _mapOutline;

    // Grid Related
    private Grid _grid;
    public static float TileSize { get; private set; }
    private float _tileSizeInverse;
    private float _tilingOffset;

    private static Vector3 _defaultPosition;

    // Map Dimensions
    private BoundsInt _mapBounds;
    private int _mapWidth;
    private int _mapHeight;
    private Vector2Int _mapDimensions;
    private Vector2Int _halfMapDimensions;

    private Vector2Int _minPlayable;
    private Vector2Int _maxPlayable;

    private LogicalTile[,] _logicalTiles;
    private Vector2Int[] _unitCardinalDisplacements = new Vector2Int[]
    {
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(0, -1),
    };
    private Vector2Int[] _unitDiagonalDisplacements = new Vector2Int[]
    {
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

    // Debug
    [SerializeField]
    private GameObject _pathMarker;
    private List<GameObject> _pathMarkers = new List<GameObject>();
    [SerializeField]
    private bool _debug;

    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;

        _mouse = Mouse.current;
        _camera = Camera.main;

        // Retrieval of the grid component.
        _grid = _graphicGroundTilemap.GetComponentInParent<Grid>();
        TileSize = _grid.cellSize.x;
        _tileSizeInverse = 1 / TileSize;
        _tilingOffset = TileSize / 2;

        // Retrieval of the tilemap's dimension properties.
        _mapBounds = _graphicGroundTilemap.cellBounds;
        _mapWidth = _mapBounds.xMax - _mapBounds.xMin;
        _mapHeight = _mapBounds.yMax - _mapBounds.yMin;
        _mapDimensions = new Vector2Int(_mapWidth, _mapHeight);
        _halfMapDimensions = _mapDimensions / 2;

        _minPlayable = new Vector2Int(_mapOutline, _mapOutline);
        _maxPlayable = new Vector2Int(_mapWidth - _mapOutline - 1, _mapHeight - _mapOutline - 1);

        // Corresponds to the bottom left corner tile position (x,y,z) whose coordinates are (0,0).
        _defaultPosition = new Vector3(_tilingOffset - TileSize * _mapWidth * .5f, _tilingOffset - TileSize * _mapHeight * .5f);

        // Initialization of the logic tilemap according to the ground graphic tilemap.
        _logicalTiles = new LogicalTile[_mapWidth, _mapHeight];
        for (int x = 0; x < _mapWidth; ++x)
            for (int y = 0; y < _mapHeight; ++y)
                _logicalTiles[x, y] = new LogicalTile(new Vector2Int(x, y));

        // Initialization of the obstacles in the logic tilemap according to the top layer tilemap.
        foreach (Vector3Int position in _graphicObstaclesTilemap.cellBounds.allPositionsWithin)
        {
            // If the tile is null, no obstacle is present.
            if (!_graphicObstaclesTilemap.HasTile(position))
                continue;

            // Else the state of the according logic tile is set to obstacle.
            _logicalTiles[position.x - _mapBounds.xMin, position.y - _mapBounds.yMin].State = TileState.Obstacle;
        }
    }

    public static Vector2Int WorldToTilemapCoords(Vector3 position)
    {
        Vector2Int coords = _instance._halfMapDimensions + new Vector2Int(Mathf.FloorToInt(position.x * _instance._tileSizeInverse),
                                                                Mathf.FloorToInt(position.y * _instance._tileSizeInverse));
        return coords;
    }

    public static Vector3 TilemapCoordsToWorld(Vector2Int coords)
    {
        Vector3 pos = _defaultPosition + new Vector3(coords.x * TileSize, coords.y * TileSize);
        return pos;
    }

    public static void AddObstacle(Vector2Int coords)
    {
        _instance._logicalTiles[coords.x, coords.y].State = TileState.Obstacle;
    }

    public static void RemoveObstacle(Vector2Int coords)
    {
        _instance._logicalTiles[coords.x, coords.y].State = TileState.Free;
    }

    public static LogicalTile GetTile(int x, int y)
    {
        return _instance._logicalTiles[x, y];
    }
    public static LogicalTile GetTile(Vector2Int coords)
    {
        return _instance._logicalTiles[coords.x, coords.y];
    }

    public static bool OutofMap(Vector2Int coords)
    {
        return coords.x < _instance._minPlayable.x || coords.x > _instance._maxPlayable.x 
            || coords.y < _instance._minPlayable.y || coords.y > _instance._maxPlayable.y;
    }

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
        if (_instance._previewMin.x < _instance._minPlayable.x || _instance._previewMin.y < _instance._minPlayable.y 
            || _instance._previewMax.x > _instance._maxPlayable.x || _instance._previewMax.y > _instance._maxPlayable.y)
            return (_instance._hoveredTilePos, _instance._previousAvailability);

        // If any of the tiles located in the preview area are currently occupied by obstacles.
        for (int x = _instance._previewMin.x; x <= _instance._previewMax.x; ++x)
            for (int y = _instance._previewMin.y; y <= _instance._previewMax.y; ++y)
                if (_instance._logicalTiles[x, y].State != TileState.Free)
                    return (_instance._hoveredTilePos, _instance._previousAvailability);

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

        for (int x = _buildingMin.x; x <= _buildingMax.x; ++x)
            for (int y = _buildingMin.y; y <= _buildingMax.y; ++y)
            {
                if (x == _buildingMin.x || x == _buildingMax.x || y == _buildingMin.y || y == _buildingMax.y)
                    _instance._logicalTiles[x, y].State = TileState.BuildingOutline;
                else
                    _instance._logicalTiles[x, y].State = TileState.Obstacle;
            }
    }
    public static void RemoveBuilding(Vector2Int coords)
    {
        throw new NotImplementedException("RemoveBuildingToImplement");
    }
    #endregion
    #region PathFinding

    public static Stack<LogicalTile> FindPath(Vector2Int startCoords, Vector2Int endCoords)
    {
        Stack<LogicalTile> path = new Stack<LogicalTile>();
        List<LogicalTile> open = new List<LogicalTile>();
        List<LogicalTile> closed = new List<LogicalTile>();

        LogicalTile startTile = _instance._logicalTiles[startCoords.x, startCoords.y];
        LogicalTile endTile = _instance._logicalTiles[endCoords.x, endCoords.y];

        if (_instance._debug)
        {
            Debug.Log($"start tile : {startTile.Coords}");
            Debug.Log($"end tile : {endTile.Coords}");
        }

        LogicalTile currentTile = startTile;
        closed.Add(currentTile);
        currentTile.g = 0;
        currentTile.h = 0;
        currentTile.parent = null;

        float weight;
        Vector2Int[] displacementsToTest;

        List<LogicalTile> validNeighbors = new List<LogicalTile>();

        do
        {
            Vector2Int currentCoords = currentTile.Coords;
            displacementsToTest = _instance._unitCardinalDisplacements;

            for (int i = 0; i < 2; ++i, displacementsToTest = _instance._unitDiagonalDisplacements)
            {
                weight = 1 + .4f * i;

                foreach (Vector2Int displacement in displacementsToTest)
                {
                    Vector2Int neighborCoords = currentCoords + displacement;

                    if (neighborCoords.x < _instance._minPlayable.x || neighborCoords.y < _instance._minPlayable.y 
                        || neighborCoords.x > _instance._maxPlayable.x || neighborCoords.y > _instance._maxPlayable.y)
                        continue;

                    LogicalTile neighbor = _instance._logicalTiles[neighborCoords.x, neighborCoords.y];

                    if (neighbor.isObstacle || closed.Contains(neighbor))
                        continue;

                    if (open.Contains(neighbor))
                    {
                        float hypotheticG = currentTile.g + weight;
                        if (hypotheticG < neighbor.g)
                        {
                            neighbor.parent = currentTile;
                            validNeighbors.Add(neighbor);
                        }
                    }
                    else
                    {
                        neighbor.h = (endTile.Coords - neighbor.Coords).sqrMagnitude;
                        neighbor.g = currentTile.g + weight;
                        neighbor.parent = currentTile;
                        open.Add(neighbor);
                        validNeighbors.Add(neighbor);
                    }
                }
            }

            if (validNeighbors.Count == 0)
            {
                if (currentTile == startTile)
                    break;
                currentTile = currentTile.parent;
            }
            else
            {
                LogicalTile bestTile = validNeighbors[0];
                for (int i = 1; i < validNeighbors.Count; ++i)
                    if (bestTile.f > validNeighbors[i].f)
                        bestTile = validNeighbors[i];

                bestTile.parent = currentTile;
                currentTile = bestTile;
                if(_instance._debug)
                    Debug.Log($"path tile considered : {currentTile.Coords}");
                closed.Add(currentTile); 
            }

            validNeighbors.Clear();

        } while (open.Count > 0 && currentTile != endTile);

        while (currentTile.parent != null)
        {
            path.Push(currentTile);
            currentTile = currentTile.parent;
        }

        if (_instance._debug)
        {
            if (path.Contains(endTile))
                Debug.Log("path found!");
            else
                Debug.Log("no path found!");

            foreach (GameObject go in _instance._pathMarkers)
                Destroy(go);

            _instance._pathMarkers.Clear();

            foreach (LogicalTile tile in path)
            {
                GameObject GO = Instantiate(_instance._pathMarker);
                GO.transform.position = TilemapCoordsToWorld(tile.Coords);
                _instance._pathMarkers.Add(GO);
            }
        }

        return path;
    }

    #endregion
}

