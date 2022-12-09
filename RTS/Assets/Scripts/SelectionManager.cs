using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class SelectionManager : MonoBehaviour
{
    private Mouse _mouse;
    private Camera _camera;

    public bool _shifting;
    private bool _clicking;

    [SerializeField]
    private LayerMask _clickable, _environment;

    private float _minimumSelectionArea;

    // graphical
    [SerializeField]
    private RectTransform _boxVisual;
    // logical
    private Rect _selectionBox;
    private Vector2 _startpos, _endpos, _boxSize, _boxCenter;

    [SerializeField]
    private bool _debug;

    private void Start()
    {
        _camera = Camera.main;
        _mouse = Mouse.current;

        _startpos = Vector2.zero;
        _endpos = Vector2.zero;

        _clicking = false;
        _shifting = false;

        // conversion of a cell size in the canvas' referential to set the min selection area to that of the size of a cell
        _minimumSelectionArea = (_camera.WorldToScreenPoint(new Vector3(TileMapManager.TileSize, 0, 0)) -
            _camera.WorldToScreenPoint(new Vector3(0, 0, 0))).sqrMagnitude;
    }

    private void Update()
    {
        if (_clicking)
        {
            _endpos = _mouse.position.ReadValue();

            // Updates graphical selection Visual
            _boxCenter = (_startpos + _endpos) / 2.0f;
            _boxVisual.position = _boxCenter;
            _boxSize = new Vector2(Mathf.Abs(_startpos.x - _endpos.x), Mathf.Abs(_startpos.y - _endpos.y));
            _boxVisual.sizeDelta = _boxSize;

            // Updates logical selection Rect
            Vector2 extents = _boxSize / 2.0f;
            _selectionBox.min = _boxCenter - extents;
            _selectionBox.max = _boxCenter + extents;
        }
    }

    public void InitSelection()
    {
        if (EventSystem.current.IsPointerOverGameObject() || UIManager.CurrentManager?.CurrentView?.Blocklick == true)
            return;

        _startpos = _mouse.position.ReadValue();
        _clicking = true;
        _selectionBox = new Rect();
    }


    public void ProceedSelection()
    {
        if (!_clicking) return;

        if (_selectionBox.size.sqrMagnitude < _minimumSelectionArea) // Selection Box is too small : click select
        {
            Vector2 worldPoint = _camera.ScreenToWorldPoint(_mouse.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, _clickable);

            if (hit.collider != null) // if we hit a clickable object
            {

                if (hit.collider.gameObject.TryGetComponent(out Character selectedCharacter)) // Collider = character
                {
                    if (GameManager.MyEntities.Contains(selectedCharacter)) //Test if I is character owner
                    {
                        if (!_shifting) // Normal click
                        {
                            CharacterManager.DeselectAll();

                            CharacterManager.SelectedCharacters().Add(selectedCharacter);
                            selectedCharacter.SelectionMarker.SetActive(true);

                            //CharacterManager.ChangeView(selectedCharacter);
                            if (_debug)
                                selectedCharacter.DebugCoordinates();
                        }
                        else    // Shift click
                            if (!CharacterManager.SelectedCharacters().Contains(selectedCharacter)) // If the character is not already selected
                                CharacterManager.AddCharacterToSelection(selectedCharacter);
                            else
                                CharacterManager.RemoveCharacterFromSelection(selectedCharacter);
                    }
                    else if (CharacterManager.SelectedCharacters().Count > 0)// Ennemy => ATTACK 
                    {
                        // TOREVIEW: test if target is damageable before or after networking, for now its after see in GameManager
                        NetworkManager.Input(TickInput.Attack(selectedCharacter.ID, selectedCharacter.transform.position, CharacterManager.GetSelectedIds()));
                    }
                }
                else if (hit.collider.gameObject.TryGetComponent(out Building selectedBuilding))// Collider = building
                {
                    if (GameManager.MyEntities.Contains(selectedBuilding)) //Test if building owner
                    {
                        CharacterManager.DeselectAll();
                        CharacterManager.AddBuildingToSelected(selectedBuilding);
                    }
                    else if (CharacterManager.SelectedCharacters().Count > 0)// EnnemyBuilding => ATTACK 
                    {
                        // TOREVIEW: test if target is damageable before or after networking, for now its after see in GameManager
                        NetworkManager.Input(TickInput.Attack(selectedBuilding.ID, selectedBuilding.transform.position, CharacterManager.GetSelectedIds()));
                    }
                }
                else if (!_shifting) // If we didn't hit anything and shift is not being held
                    CharacterManager.DeselectAll();

            }
            else
                if (!_shifting) // If we didn't hit anything and shift is not being held
                CharacterManager.DeselectAll();
        }
        else    // Drag Select
        {
            if (!_shifting)
                CharacterManager.DeselectAll();

            foreach (Character character in GameManager.MyCharacters)
                if (_selectionBox.Contains(_camera.WorldToScreenPoint(character.transform.position)))
                    if (!(_shifting && CharacterManager.SelectedCharacters().Contains(character)))
                        CharacterManager.AddCharacterToSelection(character);
        }

        _startpos = Vector2.zero;
        _endpos = Vector2.zero;

        _boxVisual.sizeDelta = new Vector2(0, 0);

        _clicking = false;

        HUDManager.UpdateHUD(CharacterManager.SelectedCharacters());
    }

    /// <summary>
    /// Clusterize a list of Character taking into account the obstacles
    /// </summary>
    /// <para name="peons">Characters to clusterize</para>
    /// <para name="maxSqrtMagnitude">The square of the maximum distance to the leader of a group</para>
    /// <returns>A list of Character list</returns>
    public static List<List<Character>> MakeGroups(int performer, Character[] peons, float maxSqrtMagnitude = 100f)
    {
        /// <summary>
        /// Check if all characters can be contained in a square without obstacles
        /// </summary>
        bool IsComplexGroupsNeeded(Character[] characters)
        {
            if (characters.Length < 2)
                return false;

            int minX = characters[0].Coords.x;
            int maxX = characters[0].Coords.x;
            int minY = characters[0].Coords.y;
            int maxY = characters[0].Coords.y;

            for (int i = 1; i < characters.Length; ++i)
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

            return TileMapManager.ObstacleDetection(performer, minX, maxX, minY, maxY);
        }

        List<List<Character>> groups = new List<List<Character>>();

        if (!IsComplexGroupsNeeded(peons))
        {
            groups.Add(new List<Character>(peons));

            return groups;
        }

        List<Character> openSet = new List<Character>(peons);

        /// <summary>
        /// Return the Character closest to the center of gravity of openSet
        /// </summary>
        int GetLeader()
        {
            Vector2 gravityCenter = Vector2.zero;

            for (int i = 0; i < peons.Length; ++i)
                gravityCenter += (Vector2)peons[i].transform.position;

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

            Character leader = openSet[leaderIndex];

            openSet.RemoveAt(leaderIndex);

            group = new List<Character>() { leader };

            for (int i = 0; i < openSet.Count; ++i)
            {
                if (Vector2.SqrMagnitude(openSet[i].transform.position - leader.transform.position) <= maxSqrtMagnitude)
                {
                    if (TileMapManager.LineOfSight(performer, leader.Coords, openSet[i].Coords))
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
}
