// PlayerData.cs
// Copyright (c) 2024 Yan01h
// MIT license

using GameNetcodeStuff;
using HarmonyLib;
using System.Reflection;

namespace BetterItemHandling.Data
{
    internal static class PlayerControllerData
    {
        // Assigned in BeginGrabObject for use in InteractItem in GrabbaleObject
        internal static PlayerControllerB Controller { get; set; }
        // Player is currently holding a two handed object and needs to drop it
        internal static bool IsTwoHanded { get; set; }
        // SwitchToItemSlot from PlayerControllerB is private, thats why we need this
        internal static MethodInfo SwitchToItemSlot {  get; set; }

        /// <summary>
        /// Counts how many main inventory slots you have.
        /// This is for compatibility with mods like ReservedItemSlots or AdvancedCompany
        /// </summary>
        /// <returns>Amount of main inventory slots</returns>
        internal static int GetItemSlotCount(PlayerControllerB controller = null)
        {
            MethodInfo nextItemSlot = AccessTools.Method(typeof(PlayerControllerB), "NextItemSlot");

            if(controller == null)
            {
                controller = Controller;
            }

            int lastIndex = controller.currentItemSlot;
            int slotCount = 1;

            while ((controller.currentItemSlot = (int)nextItemSlot.Invoke(controller, new object[] { true })) != lastIndex)
            {
                slotCount++;
                if(slotCount > controller.ItemSlots.Length) { break; }
            }

            controller.currentItemSlot = lastIndex;

            return slotCount;
        }
    }
}
