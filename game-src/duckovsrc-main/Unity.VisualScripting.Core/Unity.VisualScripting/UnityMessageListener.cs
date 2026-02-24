using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.VisualScripting;

[AddComponentMenu("")]
[Obsolete("UnityMessageListener is deprecated and has been replaced by separate message listeners for each event, eg. UnityOnCollisionEnterMessageListener or UnityOnButtonClickMessageListener.")]
public sealed class UnityMessageListener : MessageListener, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IScrollHandler, ISelectHandler, IDeselectHandler, ISubmitHandler, ICancelHandler, IMoveHandler
{
	private void Start()
	{
		AddGUIListeners();
	}

	public void AddGUIListeners()
	{
		GetComponent<Button>()?.onClick?.AddListener(delegate
		{
			EventBus.Trigger("OnButtonClick", base.gameObject);
		});
		GetComponent<Toggle>()?.onValueChanged?.AddListener(delegate(bool value)
		{
			EventBus.Trigger("OnToggleValueChanged", base.gameObject, value);
		});
		GetComponent<Slider>()?.onValueChanged?.AddListener(delegate(float value)
		{
			EventBus.Trigger("OnSliderValueChanged", base.gameObject, value);
		});
		GetComponent<Scrollbar>()?.onValueChanged?.AddListener(delegate(float value)
		{
			EventBus.Trigger("OnScrollbarValueChanged", base.gameObject, value);
		});
		GetComponent<Dropdown>()?.onValueChanged?.AddListener(delegate(int value)
		{
			EventBus.Trigger("OnDropdownValueChanged", base.gameObject, value);
		});
		GetComponent<InputField>()?.onValueChanged?.AddListener(delegate(string value)
		{
			EventBus.Trigger("OnInputFieldValueChanged", base.gameObject, value);
		});
		GetComponent<InputField>()?.onEndEdit?.AddListener(delegate(string value)
		{
			EventBus.Trigger("OnInputFieldEndEdit", base.gameObject, value);
		});
		GetComponent<ScrollRect>()?.onValueChanged?.AddListener(delegate(Vector2 value)
		{
			EventBus.Trigger("OnScrollRectValueChanged", base.gameObject, value);
		});
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		EventBus.Trigger("OnPointerEnter", base.gameObject, eventData);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		EventBus.Trigger("OnPointerExit", base.gameObject, eventData);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		EventBus.Trigger("OnPointerDown", base.gameObject, eventData);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		EventBus.Trigger("OnPointerUp", base.gameObject, eventData);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		EventBus.Trigger("OnPointerClick", base.gameObject, eventData);
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		EventBus.Trigger("OnBeginDrag", base.gameObject, eventData);
	}

	public void OnDrag(PointerEventData eventData)
	{
		EventBus.Trigger("OnDrag", base.gameObject, eventData);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		EventBus.Trigger("OnEndDrag", base.gameObject, eventData);
	}

	public void OnDrop(PointerEventData eventData)
	{
		EventBus.Trigger("OnDrop", base.gameObject, eventData);
	}

	public void OnScroll(PointerEventData eventData)
	{
		EventBus.Trigger("OnScroll", base.gameObject, eventData);
	}

	public void OnSelect(BaseEventData eventData)
	{
		EventBus.Trigger("OnSelect", base.gameObject, eventData);
	}

	public void OnDeselect(BaseEventData eventData)
	{
		EventBus.Trigger("OnDeselect", base.gameObject, eventData);
	}

	public void OnSubmit(BaseEventData eventData)
	{
		EventBus.Trigger("OnSubmit", base.gameObject, eventData);
	}

	public void OnCancel(BaseEventData eventData)
	{
		EventBus.Trigger("OnCancel", base.gameObject, eventData);
	}

	public void OnMove(AxisEventData eventData)
	{
		EventBus.Trigger("OnMove", base.gameObject, eventData);
	}

	private void OnBecameInvisible()
	{
		EventBus.Trigger("OnBecameInvisible", base.gameObject);
	}

	private void OnBecameVisible()
	{
		EventBus.Trigger("OnBecameVisible", base.gameObject);
	}

	private void OnCollisionEnter(Collision collision)
	{
		EventBus.Trigger("OnCollisionEnter", base.gameObject, collision);
	}

	private void OnCollisionExit(Collision collision)
	{
		EventBus.Trigger("OnCollisionExit", base.gameObject, collision);
	}

	private void OnCollisionStay(Collision collision)
	{
		EventBus.Trigger("OnCollisionStay", base.gameObject, collision);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		EventBus.Trigger("OnCollisionEnter2D", base.gameObject, collision);
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		EventBus.Trigger("OnCollisionExit2D", base.gameObject, collision);
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		EventBus.Trigger("OnCollisionStay2D", base.gameObject, collision);
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		EventBus.Trigger("OnControllerColliderHit", base.gameObject, hit);
	}

	private void OnJointBreak(float breakForce)
	{
		EventBus.Trigger("OnJointBreak", base.gameObject, breakForce);
	}

	private void OnJointBreak2D(Joint2D brokenJoint)
	{
		EventBus.Trigger("OnJointBreak2D", base.gameObject, brokenJoint);
	}

	private void OnMouseDown()
	{
		EventBus.Trigger("OnMouseDown", base.gameObject);
	}

	private void OnMouseDrag()
	{
		EventBus.Trigger("OnMouseDrag", base.gameObject);
	}

	private void OnMouseEnter()
	{
		EventBus.Trigger("OnMouseEnter", base.gameObject);
	}

	private void OnMouseExit()
	{
		EventBus.Trigger("OnMouseExit", base.gameObject);
	}

	private void OnMouseOver()
	{
		EventBus.Trigger("OnMouseOver", base.gameObject);
	}

	private void OnMouseUp()
	{
		EventBus.Trigger("OnMouseUp", base.gameObject);
	}

	private void OnMouseUpAsButton()
	{
		EventBus.Trigger("OnMouseUpAsButton", base.gameObject);
	}

	private void OnParticleCollision(GameObject other)
	{
		EventBus.Trigger("OnParticleCollision", base.gameObject, other);
	}

	private void OnTransformChildrenChanged()
	{
		EventBus.Trigger("OnTransformChildrenChanged", base.gameObject);
	}

	private void OnTransformParentChanged()
	{
		EventBus.Trigger("OnTransformParentChanged", base.gameObject);
	}

	private void OnTriggerEnter(Collider other)
	{
		EventBus.Trigger("OnTriggerEnter", base.gameObject, other);
	}

	private void OnTriggerExit(Collider other)
	{
		EventBus.Trigger("OnTriggerExit", base.gameObject, other);
	}

	private void OnTriggerStay(Collider other)
	{
		EventBus.Trigger("OnTriggerStay", base.gameObject, other);
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		EventBus.Trigger("OnTriggerEnter2D", base.gameObject, other);
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		EventBus.Trigger("OnTriggerExit2D", base.gameObject, other);
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		EventBus.Trigger("OnTriggerStay2D", base.gameObject, other);
	}
}
