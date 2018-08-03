using HBS.Logging;
using Harmony;
using BattleTech;
using BattleTech.UI;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

namespace BattletechPerformanceFix
{
    [HarmonyPatch(typeof(MechLabPanel), "PopulateInventory")]

    public static class Patch_MechLabPanel_PopulateInventory {
        public static MechLabPanel inst;
        public static int index = 0;
        static int bound = 0;

        public static void Prefix(MechLabPanel __instance, ref List<MechComponentRef> ___storageInventory, MechLabInventoryWidget ___inventoryWidget, ref List<MechComponentRef> __state) {
            inst = __instance;
            index = index < 0 ? 0 : (index > (bound-10) ? (bound-10) : index);           
            ___inventoryWidget.ClearInventory();
            bound = ___storageInventory.Count;
            __state = ___storageInventory;
            ___storageInventory = __state.Skip(index).Take(10).ToList();
        }

        public static void Postfix(ref List<MechComponentRef> ___storageInventory, ref List<MechComponentRef> __state) {
            ___storageInventory = __state;
        }
    }

    [HarmonyPatch(typeof(UnityEngine.UI.ScrollRect), "OnScroll")]
    public static class OnScrollHook {
        public static void Prefix(UnityEngine.EventSystems.PointerEventData data) {
            Patch_MechLabPanel_PopulateInventory.index -= Convert.ToInt32(data.scrollDelta.y);
            data.scrollDelta = new UnityEngine.Vector2(0, 0);
            new Traverse(Patch_MechLabPanel_PopulateInventory.inst).Method("PopulateInventory").GetValue();
        }
    }
}