// Patches.PlayerControllerBPatches.cs
// Copyright (c) 2024 Yan01h
// MIT license

using BetterItemHandling.Data;
using GameNetcodeStuff;
using HarmonyLib;
using System.Reflection;

namespace BetterItemHandling.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatches
    {
        /// <summary>
        /// Gets called whenever we try to interact with something (pressing e).
        /// </summary>
        /// <param name="__instance">PlayerControllerB instance</param>
        [HarmonyPrefix]
        [HarmonyPatch("BeginGrabObject")]
        internal static void BeginGrabObjectPrefix(PlayerControllerB __instance)
        {
            PlayerControllerData.Controller = __instance;

            // Set twoHanded to false to bypass the check that occurs before InteractItem
            if (__instance.twoHanded)
            {
                Plugin.Log.LogInfo("Player is carrying two handed item");
                __instance.twoHanded = false;
                PlayerControllerData.IsTwoHanded = true;
            }
            else
            {
                PlayerControllerData.IsTwoHanded= false;
            }
        }

        /// <summary>
        /// Reason for this patch is to override the cursor tooltip for when the inventory is full
        /// or for when we carry a two handed item.
        /// </summary>
        /// /// <param name="__instance">PlayerControllerB instance</param>
        [HarmonyPostfix]
        [HarmonyPatch("SetHoverTipAndCurrentInteractTrigger")]
        internal static void SetHoverTipAndCurrentInteractTriggerPostfix(PlayerControllerB __instance)
        {
            if(__instance.cursorTip.text == "Inventory full!" ||
                (__instance.cursorTip.text == "Grab : [E]" && __instance.twoHanded))
            {
                __instance.cursorTip.text = "Swap : [E]";
            }
        }

        /// <summary>
        /// Here we check if we pressed g (drop) on an empty slot in order to drop all scrap
        /// </summary>
        /// <param name="__instance">PlayerControllerB instance</param>
        [HarmonyPostfix]
        [HarmonyPatch("Discard_performed")]
        internal static void Discard_performedPostfix(PlayerControllerB __instance)
        {
            if(!Plugin.ConfigAllowDropAllScrap.Value) { return; }

            if (__instance.ItemSlots[__instance.currentItemSlot] == null)
            {
                // SwitchToItemSlot method to switch to desired slot that we want to drop.
                // Afterwards we return to the old slot. This is a hacky way of doing it but it works
                // for now.
                MethodInfo switchMethod = typeof(PlayerControllerB).GetMethod("SwitchToItemSlot",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                int currentSlotIndex = __instance.currentItemSlot;
                for (int i = 0; i < 4; i++)
                {
                    GrabbableObject item = __instance.ItemSlots[i];
                    if (item != null && item.itemProperties.isScrap)
                    {
                        switchMethod.Invoke(__instance, new object[] { i, null });
                        __instance.DiscardHeldObject();
                    }
                    switchMethod.Invoke(__instance, new object[] { currentSlotIndex, null });
                }
            }
        }
    }
}
