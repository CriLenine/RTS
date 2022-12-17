using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LongClickButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	private bool pointerDown;
	private float pointerDownTimer;

	public float RequiredHoldTime;

	public UnityEvent OnLongClick;

	public Image FillImage;

	public void OnPointerDown(PointerEventData _)
	{
		pointerDown = true;
	}

	public void OnPointerUp(PointerEventData _)
	{
		Reset();
	}

	private void Update()
	{
		if (pointerDown)
		{
			pointerDownTimer += Time.deltaTime;
			if (pointerDownTimer >= RequiredHoldTime)
			{
				if (OnLongClick != null)
					OnLongClick.Invoke();

				Reset();
			}
			FillImage.fillAmount = pointerDownTimer / RequiredHoldTime;
		}
	}

	private void Reset()
	{
		pointerDown = false;
		pointerDownTimer = 0;
		FillImage.fillAmount = pointerDownTimer / RequiredHoldTime;
	}

}