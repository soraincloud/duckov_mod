using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class OnPointerClick : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public UnityEvent<PointerEventData> onPointerClick;

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		onPointerClick?.Invoke(eventData);
	}
}
