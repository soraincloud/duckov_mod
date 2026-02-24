using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DragHandler : MonoBehaviour, IDragHandler, IEventSystemHandler
{
	public UnityEvent<PointerEventData> onDrag;

	public void OnDrag(PointerEventData eventData)
	{
		onDrag?.Invoke(eventData);
	}
}
