// Patches.GrabbableObjectPatches.cs
// Copyright (c) 2024 Yan01h
// MIT license

using BetterItemHandling.Data;
using HarmonyLib;

namespace BetterItemHandling.Patches
{
    [HarmonyPatch(typeof(GrabbableObject))]
    internal class GrabbableObjectPatches
    {
        /// <summary>
        /// Gets called in BeginGrabObject.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch("InteractItem")]
        internal static void InteractItem(GrabbableObject __instance)
        {
            if(!__instance.grabbable) { return; }

            // Next free slot index
            // 4 is hardcoded since mods like ReservedItemSlots can add aditional slots that cant carry items
            int nextFreeSlotIndex = -1;
            for (int i = 0; i < 4; i++)
            {
                if (PlayerControllerData.Controller.ItemSlots[i] == null)
                {
                    nextFreeSlotIndex = i;
                    break;
                }
            }

            // If no space available then drop item currently in hand
            // If there is space and we are carrying a two handed item then we pickup the item into
            // the free item slot
            if (nextFreeSlotIndex == -1 || PlayerControllerData.IsTwoHanded)
            {
                Plugin.Log.LogInfo("No space available! Dropping item...");
                PlayerControllerData.Controller.DiscardHeldObject();
            }

            // Reset PlayerControllerB.twoHanded only when the item picked up is also two handed,
            // without this we wouldn't be able to pickup normal items aswell
            if (!__instance.itemProperties.twoHanded)
            {
                PlayerControllerData.IsTwoHanded = false;
            }
        }
    }
}
