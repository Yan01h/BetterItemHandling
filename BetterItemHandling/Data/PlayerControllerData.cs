// PlayerData.cs
// Copyright (c) 2024 Yan01h
// MIT license

using GameNetcodeStuff;

namespace BetterItemHandling.Data
{
    internal static class PlayerControllerData
    {
        // Assigned in BeginGrabObject for use in InteractItem in GrabbaleObject
        internal static PlayerControllerB Controller { get; set; }
        // Player is currently holding a two handed object and needs to drop it
        internal static bool IsTwoHanded { get; set; }
    }
}
