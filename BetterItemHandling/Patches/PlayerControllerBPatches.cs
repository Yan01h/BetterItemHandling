// Patches.PlayerControllerBPatches.cs
// Copyright (c) 2024 Yan01h
// MIT license

using BetterItemHandling.Data;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace BetterItemHandling.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatches
    {
        private static bool _discardPressedFirst = false;
        private static float _discardPressedLast = 0;

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
                PlayerControllerData.IsTwoHanded = false;
            }
        }

        /// <summary>
        /// Reset twoHanded here or else we can change slots while twoHanded and thus pickup multiple
        /// two handed items.
        /// </summary>
        /// <param name="__instance">PlayerControllerB instance</param>
        [HarmonyPostfix]
        [HarmonyPatch("BeginGrabObject")]
        internal static void BeginGrabObjectPostfix(PlayerControllerB __instance)
        {
            if (PlayerControllerData.IsTwoHanded)
            {
                Plugin.Log.LogInfo("Item was two handed, reseting PlayerControllerB.twoHanded");
                PlayerControllerData.IsTwoHanded = false;
                __instance.twoHanded = true;
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
            bool sprintPressed = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Sprint", false).IsPressed();
            if (__instance.cursorTip.text == "Inventory full!" ||
                (__instance.cursorTip.text == "Grab : [E]" && __instance.twoHanded))
            {
                __instance.cursorTip.text = "Swap : [E]";
            }
            else if (sprintPressed && __instance.cursorTip.text == "Sell item : [E]")
            {
                __instance.cursorTip.text = "Sell All : [E]";
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
            if (!Plugin.ConfigAllowDropAllScrap.Value || !__instance.isPlayerControlled) { return; }

            if (ShouldDropAll(__instance))
            {
                // Switch to desired slot that we want to drop. Afterwards we return to the old slot.
                // This is a hacky way of doing it but it works for now.
                int currentSlotIndex = __instance.currentItemSlot;
                int inventoryLength = PlayerControllerData.GetItemSlotCount(__instance);
                for (int i = 0; i < inventoryLength; i++)
                {
                    GrabbableObject item = __instance.ItemSlots[i];
                    if (item != null && item.itemProperties.isScrap)
                    {
                        PlayerControllerData.SwitchToItemSlot.Invoke(__instance, new object[] { i, null });
                        __instance.DiscardHeldObject();
                    }
                }
                PlayerControllerData.SwitchToItemSlot.Invoke(__instance, new object[] { currentSlotIndex, null });
            }
        }

        private static bool ShouldDropAll(PlayerControllerB controller)
        {
            if (!Plugin.ConfigDropAllRequiresDoublePress.Value &&
                controller.ItemSlots[controller.currentItemSlot] == null)
                return true;

            if (_discardPressedFirst)
            {
                if (Time.time - _discardPressedLast <= Plugin.ConfigDoublePressTimeWindow.Value)
                {
                    _discardPressedFirst = false;
                    return true;
                }
            }
            else
            {
                _discardPressedFirst = true;
            }
            _discardPressedLast = Time.time;

            return false;
        }
    }
}
