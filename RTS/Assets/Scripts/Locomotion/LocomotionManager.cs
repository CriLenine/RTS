using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;


public class LocomotionManager : MonoBehaviour
{
    private Mouse _mouse;
    private Camera _camera;

    [SerializeField]
    private bool _debug;

    private void Start()
    {
        _mouse = Mouse.current;
        _camera = Camera.main;
    }

    public void QueueDisplacement()
    {
        List<Character> characters = CharacterManager.SelectedCharacters();

        if (characters.Count == 0)
            return;

        //if (characters.Count > 1)
        //    Debug.Log(TileMapManager.RetreiveClusters(characters).Count);

        // Retrieve the rallypoint's coordinates according to the input.

        Vector3 worldMousePos = _camera.ScreenToWorldPoint(_mouse.position.ReadValue());
        Vector2Int rallyPointCoords = TileMapManager.WorldToTilemapCoords(worldMousePos);

        LogicalTile rallyTile = TileMapManager.GetLogicalTile(rallyPointCoords);

        if (rallyTile == null || !rallyTile.IsFree)
            return;

        int[] IDs = new int[characters.Count];

        for(int i = 0; i < characters.Count; ++i)
            IDs[i] = characters[i].ID;

        
        TileMapManager.FindPath(characters[0].Coords, rallyPointCoords);

        //NetworkManager.Input(TickInput.Move(IDs, rallyPointCoords));
    }

    public bool Move(Character character, Vector2 position)
    {
        character.transform.position = position;
        return true;
    }
}
