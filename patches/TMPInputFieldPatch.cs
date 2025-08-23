using HarmonyLib;
using TMPro;
using UnityEngine.EventSystems;

namespace InfoSkull.patches;

[HarmonyPatch(typeof(TMP_InputField))]
internal static class TMPInputFieldPatch {
    // THIS IS USED TO CLEAR AN NRE THAT OCCURS WHEN YOU DRAG A DISPLAY TEXT ELEMENT. It doesnt affect anything else. its just to clear the log
    // i don't like big NRE's that clog up the log. Remove this if you want to see them(Why would you?)
    [HarmonyPatch("OnDrag")]
    [HarmonyPrefix]
    private static bool Prefix_OnDrag(TMP_InputField __instance, PointerEventData eventData) {
        if (Plugin.isAdjustingUI && __instance && __instance.GetComponent<InfoSkull.components.Display>()) {
            return false; // skip original
        }
        return true;
    }

    [HarmonyPatch("OnBeginDrag")]
    [HarmonyPrefix]
    private static bool Prefix_OnBeginDrag(TMP_InputField __instance, PointerEventData eventData) {
        if (Plugin.isAdjustingUI && __instance && __instance.GetComponent<InfoSkull.components.Display>()) {
            return false; // skip original
        }
        return true;
    }
}


