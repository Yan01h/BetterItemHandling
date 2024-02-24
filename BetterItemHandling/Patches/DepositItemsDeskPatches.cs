// Patches.DepositItemDesk.cs
// Copyright (c) 2024 Yan01h
// MIT licenseusing System;

using BetterItemHandling.Data;
using HarmonyLib;

namespace BetterItemHandling.Patches
{
    [HarmonyPatch(typeof(DepositItemsDesk))]
    internal class DepositItemsDeskPatches
    {
        private static int _slotIndex = -1;

        /// <summary>
        /// When this is called the first time, PlayerControllerData.Index will be -1, then we
        /// switch to the first slot, if there is a item there, place the item on the counter and
        /// with this recursively call this hook again. Repeat this until Index is 4.
        /// This is very disgusting but with my current level of C# knowledge, I don't know a better way. Forgive me.
        /// </summary>
        /// <param name="__instance">DepositItemsDesk instance</param>
        [HarmonyPostfix]
        [HarmonyPatch("PlaceItemOnCounter")]
        internal static void PlaceItemOnCounterPostfix(DepositItemsDesk __instance)
        {
            if (!Plugin.ConfigAllowPlaceAllScrapOnDesk.Value) { return; }

            bool sprintPressed = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Sprint", false).IsPressed();
            int inventoryLength = PlayerControllerData.GetItemSlotCount();
            if (sprintPressed && _slotIndex != inventoryLength)
            {
                do
                {
                    _slotIndex++;
                    if (_slotIndex == inventoryLength)
                    {
                        return;
                    }
                } while (PlayerControllerData.Controller.ItemSlots[_slotIndex] == null
                || !PlayerControllerData.Controller.ItemSlots[_slotIndex].itemProperties.isScrap);

                Plugin.Log.LogInfo($"Placing item at index {_slotIndex} on the deposit desk");
                PlayerControllerData.SwitchToItemSlot.Invoke(PlayerControllerData.Controller, new object[] { _slotIndex, null });
                __instance.PlaceItemOnCounter(PlayerControllerData.Controller);
            }
            else
            {
                _slotIndex = -1;
            }
        }
    }
}
