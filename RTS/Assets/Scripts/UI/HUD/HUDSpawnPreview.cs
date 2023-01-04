using System.Collections.Generic;
using UnityEngine;

public class HUDSpawnPreview : HUD
{
    [SerializeField]
    private List<PreviewSlot> _spawnPreviewSlots;

    private Building _currentBuilding;

    private CharacterData _spawningCharacterData;
    private CharacterData[] _spawnQueue;

    public void UpdateSpawnPreview()
    {
        foreach (PreviewSlot spawnPreviewSlot in _spawnPreviewSlots)
            spawnPreviewSlot.gameObject.SetActive(false);

        if (SelectionManager.SelectedBuilding.Performer == NetworkManager.Me)
        {
            _currentBuilding = SelectionManager.SelectedBuilding;
            _spawnQueue = _currentBuilding.QueuedSpawnCharacters.ToArray();

            if (_currentBuilding.OnGoingSpawn)
            {
                int index = 0;

                foreach (CharacterData data in _spawnQueue)
                {
                    if (index >= _spawnPreviewSlots.Count)
                        break;

                    _spawnPreviewSlots[index].SetActive();

                    if (index == 0)
                    {
                        _spawningCharacterData = _currentBuilding.OnGoingSpawnCharacterData;

                        Color color = _spawningCharacterData.Color;
                        _spawnPreviewSlots[0].Fill.color = new Color(color.r, color.g, color.b, .2f);
                    }

                    _spawnPreviewSlots[index].PreviewSprite.sprite = data.Icon;
                    _spawnPreviewSlots[index].PreviewSprite.color = data.Color;

                    index++;
                }
            }
            else
                _spawningCharacterData = null;
        }
    }

    private void Update()
    {
        if (_spawningCharacterData != null)
            _spawnPreviewSlots[0].Fill.fillAmount = _currentBuilding.SpawningTicks / _spawningCharacterData.SpawnTicks;
    }
}