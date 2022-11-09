using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class TileMapManager : MonoBehaviour
{
    private static TileMapManager _instance;

    [SerializeField] 
    private Tilemap _graphicGroundTilemap, _graphicObstaclesTilemap, _graphicPreviewTilemap;

    // Grid Related
    private static Grid _grid;
    private static float _cellSize;
    private static float _cellSizeInverse;

    // Map Dimensions
    private BoundsInt _mapBounds;
    private static int _mapWidth;
    private static int _mapHeight;
    private static Vector2Int _mapDimensions;
    private static Vector2Int _halfMapDimensions;

    private static LogicalTile[,] _logicalTiles;

    // Building Positionning Tests 
    private static bool _previousAvailability;
    private static Vector2 _previousMousePos;
    private static Vector2Int _hoveredTilePos;
    private static Vector2Int _previewMin;
    private static Vector2Int _previewMax;

    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;

        // Retrieval of the grid component.
        _grid = _graphicGroundTilemap.GetComponentInParent<Grid>();
        _cellSize = _grid.cellSize.x;
        _cellSizeInverse = 1 / _cellSize;

        // Retrieval of the tilemap's dimension properties.
        _mapBounds = _graphicGroundTilemap.cellBounds;
        _mapWidth = _mapBounds.xMax - _mapBounds.xMin;
        _mapHeight = _mapBounds.yMax - _mapBounds.yMin;
        _mapDimensions = new Vector2Int(_mapWidth, _mapHeight);
        _halfMapDimensions = _mapDimensions / 2;

        // Initialization of the logic tilemap according to the ground graphic tilemap.
        _logicalTiles = new LogicalTile[_mapWidth, _mapHeight];
        for (int x = 0; x < _mapWidth; ++x)
            for (int y = 0; y < _mapHeight; ++y)
                _logicalTiles[x, y] = new LogicalTile(x, y);

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

    public static void AddObstacleTile(Vector3 position)
    {
        Vector2Int pos = _halfMapDimensions + new Vector2Int(Mathf.FloorToInt(position.x * _cellSizeInverse),
                                                                Mathf.FloorToInt(position.y * _cellSizeInverse));
    }

    public static LogicalTile GetTile(int x, int y)
    {
        return _logicalTiles[x, y];
    }

    public static bool TilesAvailableForBuild(int outlinesCount)
    {
        // Retrieval of the mouse position.
        Vector2 currentMousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        // If the mouse hasn't moved, avoid unnecessary operations : return last returnedvalue.
        if (_previousMousePos == currentMousePos)
            return _previousAvailability;

        // Update of the "previous" variables according to the new entries.
        _previousAvailability = false;
        _previousMousePos = currentMousePos;
        /*
            The position of the hovered is set by default to half the map's dimension since the origin of the grid is 
            located at the bottom left corner of the graphic grid.
            It is then updated to take into account the position of the mouse according to the size of individual grid cells.
         */
        _hoveredTilePos = _halfMapDimensions + new Vector2Int(Mathf.FloorToInt(_previousMousePos.x * _cellSizeInverse),
                                                                Mathf.FloorToInt(_previousMousePos.y * _cellSizeInverse));

        // Defines respectively the coordinates of the building's preview location bottom left & top right corners.
        _previewMin = new Vector2Int(_hoveredTilePos.x - outlinesCount, _hoveredTilePos.y - outlinesCount);
        _previewMax = new Vector2Int(_hoveredTilePos.x + outlinesCount, _hoveredTilePos.y + outlinesCount);

        // If the building steps outside of the map.
        if (_previewMin.x < 0 || _previewMin.y < 0 || _previewMax.x >= _mapWidth || _previewMax.y >= _mapHeight)
            return _previousAvailability;

        // If any of the tiles located in the preview area are currently occupied by obstacles.
        for (int x = _previewMin.x; x <= _previewMax.x; ++x)
            for (int y = _previewMin.y; y <= _previewMax.y; ++y)
                if (_logicalTiles[x, y].state != TileState.Free)
                    return _previousAvailability;

        // Else the building can be placed at this location.
        _previousAvailability = true;
        return _previousAvailability;
    }
}
