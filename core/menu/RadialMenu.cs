using System.Collections;
using InfoSkull.config;
using InfoSkull.config.profiles;
using InfoSkull.core.menu;
using TMPro;

namespace InfoSkull.core.components;

extern alias unityengineold;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public class RadialMenu : MonoBehaviour {
    internal float radius = 80f;
    internal float buttonSize = 60f;

    public Action<List<MenuButton>> buttonProvider = list => {

        var underCursor = getUIObjectUnderCursor();
        if (underCursor) {
            var controller = underCursor.GetComponent<ElementController>();
            if (controller && controller.menuSettings() != null) {
                if (controller.menuSettings().allowDeletion) {
                    list.Add(new MenuButton("Delete", menu => {
                        controller.delete();
                        menu.hideMenu();
                    }));
                }
                if (controller.menuSettings().menuButtonCallback != null) {
                    controller.menuSettings().menuButtonCallback(list);
                }
            }
        }
        list.Add(new MenuButton("New", menu => {
            List<MenuButton> subButtons = new List<MenuButton>();
            foreach (var type in InfoSkull.registeredTypes) {
                if (type.Value._allowCreation) {
                    subButtons.Add(new MenuButton($"{type.Key}", m => {
                        var element = InfoSkull.instantiateType(type.Value);
                        element.transform.position = Input.mousePosition;
                        element.openAdjustUI();
                        
                        menu.hideMenu();
                    }));
                }
            }
            menu.updateMenu(subButtons);
        }));

        list.Add(new MenuButton("Profiles", menu => {
            List<MenuButton> subButtons = new List<MenuButton>();
            for (var i = 0; i < Config.instance.profiles.Count; i++) {
                var s = i;
                subButtons.Add(new MenuButton($"{i}", m => {
                    InfoSkull.selectProfile(s);
                    menu.hideMenu();
                }));
            }
            subButtons.Add(new MenuButton("New", m => {
                var profile = ProfileConfig.create();
                Config.defaultProfile(profile);
                Config.instance.profiles.Add(profile);
                InfoSkull.selectProfile(Config.instance.profiles.Count - 1);
                menu.hideMenu();
            }));
            subButtons.Add(new MenuButton("Delete Current", m => {
                if (Config.instance.profiles.Count <= 1) return;
                Config.instance.profiles.RemoveAt(Config.instance.selectedProfile);
                InfoSkull.selectProfile(Math.Max(0, Config.instance.selectedProfile - 1));
                menu.hideMenu();
            }));
            menu.updateMenu(subButtons);
        }));
    };

    internal static RadialMenu instance;
    RectTransform rect;
    CanvasGroup canvasGroup;
    

    internal static void init() {
        if (instance != null) return;

        GameObject go = new GameObject("RadialMenu");
        go.transform.SetParent(GameObject.Find("Game UI")?.transform, false);

        instance = go.AddComponent<RadialMenu>();
        instance.canvasGroup = go.GetComponent<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();
    } 
    
    public static GameObject getUIObjectUnderCursor() {
        Canvas canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        var raycaster = canvas.GetComponent<GraphicRaycaster>();

        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);
        return results.Count == 0 ? null : results[0].gameObject;
    }
    
    void Update() {
        if (Input.GetKeyDown(KeyCode.LeftAlt) && InfoSkull.isAdjustingUI) {
            createMenu();
        }

        if (Input.GetMouseButtonDown(0) && InfoSkull.isAdjustingUI) {
            var under = getUIObjectUnderCursor();
            if (!under || under.transform.parent.parent != transform) {
                hideMenu();
            }
        }
    }

    void createMenu() {
        rect = gameObject.GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();

        rect.position = new Vector2(
            Math.Max(radius + buttonSize / 2, Math.Min(Input.mousePosition.x, Screen.width - radius - buttonSize / 2)),
            Math.Max(radius + buttonSize / 2, Math.Min(Input.mousePosition.y, Screen.height - radius - buttonSize / 2))
        );
        
        List<MenuButton> buttons = new List<MenuButton>();
        buttonProvider(buttons);
        updateMenu(buttons);
    }

    void updateMenu(List<MenuButton> buttons) {
        hideMenu();
        
        int n = buttons.Count;
        var _radius = n == 1 ? 0 : radius;
        float angleStep = 360f / n;

        for (int i = 0; i < n; i++) {
            GameObject buttonGO = new GameObject($"Button_{i}", typeof(RectTransform), typeof(Button));
            buttonGO.transform.SetParent(transform, false);

            RectTransform rt = buttonGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(buttonSize, buttonSize);

            float angle = (angleStep * i - 90) * Mathf.Deg2Rad;
            Vector2 pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _radius;
            rt.anchoredPosition = pos;

            // Label
            GameObject textGO = new GameObject("Label", typeof(RectTransform));
            textGO.transform.SetParent(buttonGO.transform, false);
            RectTransform textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = buttons[i].label;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 24;

            // Click
            Button btn = buttonGO.GetComponent<Button>();
            int t = i;
            btn.onClick.AddListener(() => {
                buttons[t].onClick(this);
            });

            EventTrigger trigger = buttonGO.AddComponent<EventTrigger>();

            EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener(_ => text.alpha = 0.5f);

            EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener(_ => text.alpha = 1f);

            trigger.triggers.Add(enter); 
            trigger.triggers.Add(exit);
        }
    }

    public void hideMenu() {
        for (int i = transform.childCount - 1; i >= 0; i--) {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}
