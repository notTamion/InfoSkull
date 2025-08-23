using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using InfoSkull.utils;

namespace InfoSkull.components;

public class Positionable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
	Canvas canvas;
	CanvasGroup canvasGroup;
	Vector2 pointerOffset;
	RectTransform rectTransform;
	
	TMP_InputField inputField;

	readonly float snapThreshold = 20f;

	public void OnBeginDrag(PointerEventData eventData) {
		if (inputField) inputField.enabled = false;
		if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);
		canvasGroup.blocksRaycasts = false;

		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			rectTransform,
			eventData.position,
			eventData.pressEventCamera,
			out pointerOffset
		);
	}

	public void OnDrag(PointerEventData eventData) {
		Vector2 localPointerPosition;
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
			    canvas ? canvas.transform as RectTransform : rectTransform,
			    eventData.position,
			    eventData.pressEventCamera,
			    out localPointerPosition)) {
			var newPos = localPointerPosition - pointerOffset;

			var snappingEnabled = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

			if (snappingEnabled) {
				if (Mathf.Abs(newPos.x) <= snapThreshold) newPos.x = 0;
				if (Mathf.Abs(newPos.y) <= snapThreshold) newPos.y = 0;
			}

			rectTransform.anchoredPosition = newPos;
		}
	}

	public void OnEndDrag(PointerEventData eventData) {
		if (inputField) {
			inputField.enabled = true;
			inputField.ActivateInputField();
			Traverse.Create(inputField).Field("caretRectTrans").GetValue<RectTransform>().anchoredPosition = rectTransform.anchoredPosition;
		}
		canvasGroup.blocksRaycasts = true;

		var dyn = GetComponent<Display>();
		if (dyn) {
			var cfg = ConfigService.Data.elements.Find(e => e.id == dyn.elementId);
			if (cfg != null) {
				cfg.position = rectTransform.anchoredPosition;
				ConfigService.Save();
			}
		}
	}

	void Awake() {
		canvas = GetComponentInParent<Canvas>();
		canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
		
		var text = GetComponent<TextMeshProUGUI>();
		text.raycastTarget = true;

		rectTransform = GetComponent<RectTransform>();
		inputField = GetComponent<TMP_InputField>();
	}
}