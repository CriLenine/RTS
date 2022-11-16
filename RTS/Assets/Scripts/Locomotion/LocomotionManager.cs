using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

public enum Goal
{
    None,
    Build,
    Attack
}
public class LocomotionManager : MonoBehaviour
{
    private Mouse _mouse;
    private Camera _camera;

    private Locomotion _locomotion;

    private Vector2Int _rallyPointCoords;

    private float _timer;

    private List<Troop> _movingTroops = new List<Troop>();
    private List<Troop> _troopsToRemove = new List<Troop>();

    [SerializeField]
    private bool _debug;
    private int _previousTroupCount;

    private void Start()
    {
        _timer = 0f;

        _mouse = Mouse.current;
        _camera = Camera.main;

        _locomotion = new Locomotion();
        _locomotion.Enable(); // DISABLE to test method technique
        _locomotion.Displacement.RightClick.performed += SetRallyPoint;
    }

    private void Update()
    {
        /* --------------------------------------------------------------- */
        /* Temporary Step based call of the method 'MoveCharacters' */
        /* This is for testing only */

        _timer += Time.deltaTime;
        if (_timer > .2f)
        {
            _timer = 0f;
            MoveCharacters();
            if (_debug)
            {
                if (_movingTroops.Count != _previousTroupCount)
                    Debug.Log($"Troops Count : {_movingTroops.Count}");
                _previousTroupCount = _movingTroops.Count;
            }
        }

        /* --------------------------------------------------------------- */
    }

    private void SetRallyPoint(InputAction.CallbackContext _)
    {
        // Retrieve the rallypoint's coordinates according to the input.

        Vector3 worldMousePos = _camera.ScreenToWorldPoint(_mouse.position.ReadValue());
        _rallyPointCoords = TileMapManager.WorldToTilemapCoords(worldMousePos);

        if (TileMapManager.OutofMap(_rallyPointCoords))
            return;

        LogicalTile rallyTile = TileMapManager.GetTile(_rallyPointCoords);

        if (rallyTile.State == TileState.Obstacle) // if the rallypoint tile is an obstacle : nothing happens
            return;

        // else : go through all the characters selected to create troops whose purpose will be to reach the rallypoint.

        List<Character> characters = CharacterSelectionManager.charactersSelected;

        if (_debug)
            Debug.Log($"RallyPoint coords : ({_rallyPointCoords.x}, {_rallyPointCoords.y})");

        foreach (Character character in characters)
            if (character.isInTroop)
                return;

        //List<Character> charactersToTest = new List<Character>();

        //foreach (Character character in characters) // Check if any selected troop is already in a troop : therefore currently moving.
        //    if (character.isInTroop)
        //        charactersToTest.Add(character);

        //if (charactersToTest.Count > 0) // If there are characters currently in a troop
        //{
        //    if (_debug)
        //        Debug.Log("ENTER REASSIGNMENT PROCESS");

        //    List<Character> remainingCharacters = new List<Character>();
        //    List<(List<Character>, LogicalTile)> forNewAssignment = new List<(List<Character>, LogicalTile)>();

        //    foreach (Troop troop in _movingTroops) // Go through every troop to proceed reassignment according to the characters left 
        //    {                                                               // and the corresponding rallypoint.
        //        foreach (Character character in charactersToTest)
        //            if (troop.Characters.Contains(character))
        //                remainingCharacters = troop.Characters;

        //        if (charactersToTest.Count == 0)
        //            continue;

        //        foreach (Character character in remainingCharacters)
        //            charactersToTest.Remove(character);

        //        forNewAssignment.Add((remainingCharacters, troop.Leader.RallyPoint));

        //        if (charactersToTest.Count == 0)
        //            break;
        //    }

        //    foreach ((List<Character>, LogicalTile) toAssign in forNewAssignment)
        //        AssignTroops(toAssign.Item1, toAssign.Item2);
        //}


        AssignTroops(characters, rallyTile);
    }

    private void AssignTroops(List<Character> characters, LogicalTile rallyTile)
    {
        // Sort the characters according to their distance to the rallypoint : therefore the troop leader will be the closest one.

        characters.Sort((a, b) => (_rallyPointCoords - a.Coords).sqrMagnitude.CompareTo((_rallyPointCoords - b.Coords).sqrMagnitude));

        Character firstCharacter = characters[0];
        Troop currentTroop = new Troop(firstCharacter);

        // Compute the path necessary to reach the rallypoint.

        firstCharacter.Path = TileMapManager.FindPath(firstCharacter.Coords, _rallyPointCoords);
        firstCharacter.RallyPoint = rallyTile;
        firstCharacter.isInTroop = true;

        if (characters.Count > 1) // if there are more than 1 character selected
        {
            // Sort them according to their distance with the leader.

            characters.Sort((a, b) => (firstCharacter.Coords - a.Coords).sqrMagnitude.CompareTo((firstCharacter.Coords - b.Coords).sqrMagnitude));

            for (int i = 1; i < characters.Count; ++i) // Proceed to go through all remaining selected characters
            {
                Character currentCharacter = characters[i];

                // if 'currentCharacter' is more than 1 tile away from the previous one in the current troop :
                if ((currentCharacter.Coords - currentTroop.Characters[^1].Coords).sqrMagnitude > 2)
                {
                    // Create a new troop whose leader is 'currentCharacter' and compute the pathfinding accordingly.

                    _movingTroops.Add(new Troop(currentTroop.Characters));
                    currentTroop = new Troop(currentCharacter);
                    currentCharacter.Path = TileMapManager.FindPath(currentCharacter.Coords, _rallyPointCoords);
                    currentCharacter.RallyPoint = rallyTile;
                }

                else // else : meaning 'currentCharacter is next to the last troop member, add it to the troop too !
                    currentTroop.Characters.Add(currentCharacter);

                currentCharacter.isInTroop = true;
            }
        }

        // Once completed, add the last troop not registered as currently moving.
        _movingTroops.Add(currentTroop);
    }

    private void MoveCharacters()
    {
        foreach (Troop troop in _movingTroops) // going through every troop currently moving
        {
            Character leader = troop.Leader;
            List<Character> troopCharacters = troop.Characters;
            LogicalTile nextTile;

            if (!leader.Path.TryPeek(out nextTile)) // if the rally point has been reached
            {
                if(_debug)
                    Debug.Log("RALLYPOINT REACHED");
                Debug.Log(leader.RallyPointGoal);
                if (leader.RallyPointGoal == Goal.Build)
                    BuildingManager.StartBuild(leader.RallyPoint,troopCharacters.Cast<Peon>().ToList());

                _troopsToRemove.Add(troop);
            }

            else if (nextTile.State != TileState.Obstacle) // else if the next tile on the path is walkable : move the leader to the next tile
            {
                leader.Path.Pop();
                TileMapManager.RemoveObstacle(troop.Characters[^1].Coords);
                TileMapManager.AddObstacle(nextTile.Coords);

                Vector3 previousPosition = leader.gameObject.transform.position;
                leader.gameObject.transform.position = TileMapManager.TilemapCoordsToWorld(nextTile.Coords);

                Vector2Int previousCoords = leader.Coords;
                leader.Coords = nextTile.Coords;

                for (int i = 1; i < troopCharacters.Count; ++i) // move every ith member of the troup to the (i-1)th position
                {   
                    Vector2Int tmpCoords = troopCharacters[i].Coords;
                    troopCharacters[i].Coords = previousCoords;
                    previousCoords = tmpCoords;

                    Vector3 tmpPos = troopCharacters[i].gameObject.transform.position;
                    troopCharacters[i].gameObject.transform.position = previousPosition;
                    previousPosition = tmpPos;
                }
            }
            else // else : meaning the next tile on the path is NOT walkable
            {
                bool waitingForPotentialMerge = false;
                bool merged = false;

                foreach (Troop potentialCollidingTroop in _movingTroops) // going through moving troops to investigate for a merge
                {
                    if(_debug)
                        Debug.Log("INVESTIGATING MERGE OPPORTUNITIES");

                    if (potentialCollidingTroop.Leader == troop.Leader  // if the investigated troop is me or the troop goes elsewhere
                        || potentialCollidingTroop.Leader.RallyPoint != troop.Leader.RallyPoint)
                        continue; // keep investigating

                    // else : the investigated troop DOES go to the same rallypoint, meaning this could explain why the tile is blocked.

                    List<Character> characters = potentialCollidingTroop.Characters;

                    if (characters[^1].Coords != nextTile.Coords) // if the last troop member isn't the one blocking the tile
                        if (Vector2.Dot(_rallyPointCoords - troop.Leader.Coords, // and if the two troops are going in the same direction
                            _rallyPointCoords - potentialCollidingTroop.Leader.Coords) > 0)
                        { 
                            waitingForPotentialMerge = true; // Put the troop on standby and wait for the next iteration again
                            break;
                        }

                    if (_debug)
                    {
                        Debug.Log("MERGE ATTEMPT :");
                        Debug.Log($"Troop attempting merge - leader : {troop.Leader.gameObject.name}");
                        Debug.Log($"Troop to merge with - leader :{potentialCollidingTroop.Leader.gameObject.name}");
                    }

                    // else : meaning the other troop's last member IS the one blocking the tile : Merge the troops

                    potentialCollidingTroop.Characters.AddRange(troop.Characters);
                    _troopsToRemove.Add(troop);
                    merged = true;

                    break;
                }

                if (!merged && !waitingForPotentialMerge) // if the troop is blocked under any other circumstance, delete the troop
                    _troopsToRemove.Add(troop); 
            }
        }

        foreach (Troop troop in _troopsToRemove)
        {
            foreach (Character character in troop.Characters)
                character.isInTroop = false;
            _movingTroops.Remove(troop);
        }

        _troopsToRemove.Clear();
    }

    #region Test without input action but a method
    public void SetRallyPointMethod(Vector2 position)
    {
        _rallyPointCoords = TileMapManager.WorldToTilemapCoords(position);

        if (TileMapManager.OutofMap(_rallyPointCoords))
            return;

        LogicalTile rallyTile = TileMapManager.GetTile(_rallyPointCoords);

        if (rallyTile.State == TileState.Obstacle) // if the rallypoint tile is an obstacle : nothing happens
            return;

        // else : go through all the characters selected to create troops whose purpose will be to reach the rallypoint.

        List<Character> characters = CharacterSelectionManager.charactersSelected;

        if (_debug)
            Debug.Log($"RallyPoint coords : ({_rallyPointCoords.x}, {_rallyPointCoords.y})");

        foreach (Character character in characters)
            if (character.isInTroop)
                return;


        AssignTroops(characters, rallyTile);
    }
    #endregion
}
