using UnityEngine;

public abstract class HUD : MonoBehaviour
{
    [SerializeField]
    private GameObject HUDSection;

    public void Show()
    {
        HUDSection.SetActive(true);
    }

    public void Hide()
    {
        HUDSection.SetActive(false);
    }
}
