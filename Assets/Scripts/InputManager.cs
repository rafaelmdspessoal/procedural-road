using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
	public static InputManager Instance { get; private set; }

	public Action<Vector3, GameObject> OnMouseHold, OnMouseHover;
	public Action<GameObject> OnMouseClick;
	public Action OnMouseUp;

	public LayerMask groundMask;

    private void Awake() {
		Instance = this;
    }

    private void Update()
	{
		CheckClickDownEvent();
		CheckClickUpEvent();
		CheckClickHoldEvent();
		HanleMouseHover();
	}

	private Vector3 RaycastGround()
	{
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundMask))
		{
			return hit.point;
		}
		return Vector3.zero;
	}

	private GameObject RayCastObject()
    {
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out hit, Mathf.Infinity))
		{
			return hit.collider.gameObject;
		}
		return null;
	}

	private void HanleMouseHover()
	{
		if (EventSystem.current.IsPointerOverGameObject() == false)
		{
			Vector3 hitPos = RaycastGround();
			GameObject hitObject = RayCastObject();
			if (hitObject != null)
				OnMouseHover?.Invoke(hitPos, hitObject);

		}
	}


	private void CheckClickHoldEvent()
	{
		if (Input.GetMouseButton(0) && EventSystem.current.IsPointerOverGameObject() == false)
		{
			Vector3 hitPos= RaycastGround();
			GameObject hitObject = RayCastObject();
			if (hitPos != null)
				OnMouseHold?.Invoke(hitPos, hitObject);

		}
	}

	private void CheckClickUpEvent()
	{
		if (Input.GetMouseButtonUp(0) && EventSystem.current.IsPointerOverGameObject() == false)
		{
			OnMouseUp?.Invoke();

		}
	}

	private void CheckClickDownEvent()
	{
		if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject() == false)
		{
			GameObject hitObject = RayCastObject();
			if (hitObject != null)
				OnMouseClick?.Invoke(hitObject);

		}
	}
}