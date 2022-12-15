using UnityEngine;

public abstract class HUD : MonoBehaviour
{
    [SerializeField]
    protected GameObject HUDSection;

    public void Show()
    {
        HUDSection.SetActive(true);
    }

    public void Hide()
    {
        HUDSection.SetActive(false);
    }
}
