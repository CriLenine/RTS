using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingUI : MonoBehaviour
{
    [SerializeField] private Image _image;

    [SerializeField] private TextMeshProUGUI _text;

    private PeonBuilds _buildingType;
    public void InitUI(Sprite buildingSprite, string buildingName,PeonBuilds buildingType)
    {
        _image.sprite = buildingSprite;
        _text.text = buildingName;
        _buildingType = buildingType;
        gameObject.transform.localScale = Vector3.one;
    }

    public void OnBuildingClick()
    {
        BuildingBlueprintsManager.SpawnBlueprint(_buildingType);
    }
}
