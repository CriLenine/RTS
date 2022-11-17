using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class TileMapManager : MonoBehaviour
{
    private static TileMapManager _instance;

    private static Mouse _mouse;
    private static Camera _camera;

    [SerializeField]
    private Tilemap _graphicGroundTilemap, _graphicObstaclesTilemap, _graphicPreviewTilemap;

    [SerializeField]
    private int _mapOutline;

    // Grid Related
    private static Grid _grid;
    public static float _tileSize { get; private set; }
    private static float _tileSizeInverse;
    private static float _tilingOffset;

    private static Vector3 _defaultPosition;

    // Map Dimensions
    private BoundsInt _mapBounds;
    private static int _mapWidth;
    private static int _mapHeight;
    private static Vector2Int _mapDimensions;
    private static Vector2Int _halfMapDimensions;

    private static Vector2Int _minPlayable;
    private static Vector2Int _maxPlayable;

    private static LogicalTile[,] _logicalTiles;
    private static Vector2Int[] _unitCardinalDisplacements = new Vector2Int[]
    {
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(0, -1),
    };
    private static Vector2Int[] _unitDiagonalDisplacements = new Vector2Int[]
    {
        new Vector2Int(1, 1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1),
        new Vector2Int(1, -1),
    };

    // Building Positionning Tests 
    private static bool _previousAvailability;
    private static Vector2 _previousMousePos;
    private static Vector3 _hoveredTilePos;
    private static Vector2Int _hoveredTileCoords;
    private static Vector2Int _previewMin;
    private static Vector2Int _previewMax;

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
        _tileSize = _grid.cellSize.x;
        _tileSizeInverse = 1 / _tileSize;
        _tilingOffset = _tileSize / 2;

        // Retrieval of the tilemap's dimension properties.
        _mapBounds = _graphicGroundTilemap.cellBounds;
        _mapWidth = _mapBounds.xMax - _mapBounds.xMin;
        _mapHeight = _mapBounds.yMax - _mapBounds.yMin;
        _mapDimensions = new Vector2Int(_mapWidth, _mapHeight);
        _halfMapDimensions = _mapDimensions / 2;

        _minPlayable = new Vector2Int(_mapOutline, _mapOutline);
        _maxPlayable = new Vector2Int(_mapWidth - _mapOutline - 1, _mapHeight - _mapOutline - 1);

        // Corresponds to the bottom left corner tile position (x,y,z) whose coordinates are (0,0).
        _defaultPosition = new Vector3(_tilingOffset - _tileSize * _mapWidth * .5f, _tilingOffset - _tileSize * _mapHeight * .5f);

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
            _logicalTiles[position.x - _mapBounds.xMin, position.y - _mapBounds.yMin].state = TileState.Obstacle;
        }
    }

    public static Vector2Int WorldToTilemapCoords(Vector3 position)
    {
        Vector2Int coords = _halfMapDimensions + new Vector2Int(Mathf.FloorToInt(position.x * _tileSizeInverse),
                                                                Mathf.FloorToInt(position.y * _tileSizeInverse));
        return coords;
    }

    public static Vector3 TilemapCoordsToWorld(Vector2Int coords)
    {
        Vector3 pos = _defaultPosition + new Vector3(coords.x * _tileSize, coords.y * _tileSize);
        return pos;
    }

    public static void AddObstacle(Vector2Int coords)
    {
        _logicalTiles[coords.x, coords.y].state = TileState.Obstacle;
    }

    public static void RemoveObstacle(Vector2Int coords)
    {
        _logicalTiles[coords.x, coords.y].state = TileState.Free;
    }

    public static LogicalTile GetTile(int x, int y)
    {
        return _logicalTiles[x, y];
    }
    public static LogicalTile GetTile(Vector2Int coords)
    {
        return _logicalTiles[coords.x, coords.y];
    }

    public static (Vector3, bool) TilesAvailableForBuild(int outlinesCount)
    {
        // Retrieval of the mouse position.
        Vector2 currentMousePos = _camera.ScreenToWorldPoint(_mouse.position.ReadValue());

        // If the mouse hasn't moved, avoid unnecessary operations : return last returnedvalue.
        if (_previousMousePos == currentMousePos)
            return (_hoveredTilePos, _previousAvailability);

        if (WorldToTilemapCoords(currentMousePos) == _hoveredTileCoords)
            return (_hoveredTilePos, _previousAvailability);

        // Update of the "previous" variables according to the new entries.
        _previousAvailability = false;
        _previousMousePos = currentMousePos;

        _hoveredTileCoords = WorldToTilemapCoords(currentMousePos);
        _hoveredTilePos = TilemapCoordsToWorld(_hoveredTileCoords);

        // Defines respectively the coordinates of the building's preview location bottom left & top right corners.
        _previewMin = new Vector2Int(_hoveredTileCoords.x - outlinesCount, _hoveredTileCoords.y - outlinesCount);
        _previewMax = new Vector2Int(_hoveredTileCoords.x + outlinesCount, _hoveredTileCoords.y + outlinesCount);

        // If the building steps outside of the map.
        if (_previewMin.x < _minPlayable.x || _previewMin.y < _minPlayable.y 
            || _previewMax.x > _maxPlayable.x || _previewMax.y > _maxPlayable.y)
            return (_hoveredTilePos, _previousAvailability);

        // If any of the tiles located in the preview area are currently occupied by obstacles.
        for (int x = _previewMin.x; x <= _previewMax.x; ++x)
            for (int y = _previewMin.y; y <= _previewMax.y; ++y)
                if (_logicalTiles[x, y].state != TileState.Free)
                    return (_hoveredTilePos, _previousAvailability);

        // Else the building can be placed at this location.
        _previousAvailability = true;
        return (_hoveredTilePos, _previousAvailability);
    }

    #region PathFinding

    public static Stack<LogicalTile> FindPath(Vector2Int startCoords, Vector2Int endCoords)
    {
        Stack<LogicalTile> path = new Stack<LogicalTile>();
        List<LogicalTile> open = new List<LogicalTile>();
        List<LogicalTile> closed = new List<LogicalTile>();

        LogicalTile startTile = _logicalTiles[startCoords.x, startCoords.y];
        LogicalTile endTile = _logicalTiles[endCoords.x, endCoords.y];

        if (_instance._debug)
        {
            Debug.Log($"start tile : {startTile.coords}");
            Debug.Log($"end tile : {endTile.coords}");
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
            Vector2Int currentCoords = currentTile.coords;
            displacementsToTest = _unitCardinalDisplacements;

            for (int i = 0; i < 2; ++i, displacementsToTest = _unitDiagonalDisplacements)
            {
                weight = 1 + .4f * i;

                foreach (Vector2Int displacement in displacementsToTest)
                {
                    Vector2Int neighborCoords = currentCoords + displacement;

                    if (neighborCoords.x < _minPlayable.x || neighborCoords.y < _minPlayable.y 
                        || neighborCoords.x > _maxPlayable.x || neighborCoords.y > _maxPlayable.y)
                        continue;

                    LogicalTile neighbor = _logicalTiles[neighborCoords.x, neighborCoords.y];

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
                        neighbor.h = (endTile.coords - neighbor.coords).sqrMagnitude;
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
                    Debug.Log($"path tile considered : {currentTile.coords}");
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
                GO.transform.position = TilemapCoordsToWorld(tile.coords);
                _instance._pathMarkers.Add(GO);
            }
        }

        return path;
    }

    #endregion
}

