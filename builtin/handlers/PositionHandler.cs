using HarmonyLib;
using InfoSkull.config.conversions;
using InfoSkull.core;
using InfoSkull.core.components;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InfoSkull.builtin.handlers;

public class PositionHandler : ElementHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
	ElementController controller;
	
	public override void init(ElementController controller) {
		canvas = GetComponentInParent<Canvas>();
		canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

		var text = GetComponent<TextMeshProUGUI>();
		text.raycastTarget = true;

		rectTransform = GetComponent<RectTransform>();
		inputField = GetComponent<TMP_InputField>();
		
		if (controller.config().data.ContainsKey("position")) {
			var pos = ConfigVector2.fromDict(controller.config().data["position"]);
			pos.x *= Screen.width;
			pos.y *= Screen.height;
			controller.gameObject.transform.position = pos;
		}
		else {
			var pos = controller.gameObject.transform.position;
			controller.config().data["position"] = new ConfigVector2(pos.x / Screen.width, pos.y / Screen.height);
		}
		this.controller = controller;
		
		
	}

	public override void openAdjustUI() {
		
	}

	public override void closeAdjustUI() {
		var pos = controller.gameObject.transform.position;
		controller.config().data["position"] = new ConfigVector2(pos.x / Screen.width, pos.y / Screen.height);
	}
	
	readonly float snapThreshold = 20f;
	Canvas canvas;
	CanvasGroup canvasGroup;

	TMP_InputField inputField;
	Vector2 pointerOffset;
	RectTransform rectTransform;

	public void OnBeginDrag(PointerEventData eventData) {
		if (inputField) inputField.enabled = false;
		canvasGroup.blocksRaycasts = false;

		// Calculate the local offset between the pointer and the object's position in parent space
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			rectTransform.parent as RectTransform,
			eventData.position,
			eventData.pressEventCamera,
			out var localPointerPosInParent
		);

		pointerOffset = localPointerPosInParent - rectTransform.anchoredPosition;
	}

	public void OnDrag(PointerEventData eventData) {
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
			    rectTransform.parent as RectTransform,
			    eventData.position,
			    eventData.pressEventCamera,
			    out var localPointerPosInParent)) {
			var newAnchoredPos = localPointerPosInParent - pointerOffset;

			var snappingEnabled = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

			if (snappingEnabled) {
				if (Mathf.Abs(newAnchoredPos.x) <= snapThreshold) newAnchoredPos.x = 0;
				if (Mathf.Abs(newAnchoredPos.y) <= snapThreshold) newAnchoredPos.y = 0;
			}

			rectTransform.anchoredPosition = newAnchoredPos;
		}
	}

	public void OnEndDrag(PointerEventData eventData) {
		if (inputField) {
			inputField.enabled = true;
			inputField.ActivateInputField();
			Traverse.Create(inputField).Field("caretRectTrans").GetValue<RectTransform>().anchoredPosition =
				rectTransform.anchoredPosition;
		}

		canvasGroup.blocksRaycasts = true;
	}
}