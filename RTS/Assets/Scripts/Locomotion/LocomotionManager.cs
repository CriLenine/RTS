using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class LocomotionManager : MonoBehaviour
{
    private Locomotion _locomotion;

    private Vector2Int _rallyPointCoords;

    [SerializeField]
    private bool _debug;

    private void Start()
    {
        _locomotion = new Locomotion();
        _locomotion.Enable();
        _locomotion.Displacement.RightClick.performed += SetDestination;
    }

    private void SetDestination(InputAction.CallbackContext ctx)
    {
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        _rallyPointCoords = TileMapManager.WorldToTilemapCoords(worldMousePos);

        if (_debug)
            Debug.Log($"Destination coords : ({_rallyPointCoords.x}, {_rallyPointCoords.y})");

        if (CharacterSelectionManager.charactersSelected.Count > 0)
            // PathfindingManager.MoveUnits(); TO BE IMPLEMENTED
            MoveCharacters();
    }

    private void MoveCharacters()
    {
        List<Character> characters = CharacterSelectionManager.charactersSelected;

        if (characters.Count == 1 && TileMapManager.GetTile(_rallyPointCoords).state == TileState.Free)
        {
            Character character = characters[0];
            character.transform.position = TileMapManager.TilemapCoordsToWorld(_rallyPointCoords);
            TileMapManager.GetTile(character.coords).state = TileState.Free;
            character.coords = _rallyPointCoords;
            TileMapManager.GetTile(_rallyPointCoords).state = TileState.Obstacle;
        }
    }
}
