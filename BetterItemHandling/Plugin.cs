// Plugin.cs
// Copyright (c) 2024 Yan01h
// MIT license

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BetterItemHandling.Data;
using GameNetcodeStuff;
using HarmonyLib;
using System.Reflection;

namespace BetterItemHandling
{
    /// <summary>
    /// Main BetterItemHandling plugin class.
    /// </summary>
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string ModGUID = "Yan01h.BetterItemHandling";
        private const string ModName = "BetterItemHandling";
        private const string ModVersion = "1.1.2";

        // Main logger used by this plugin
        public static ManualLogSource Log {  get; private set; }

        // Config entries
        public static ConfigEntry<bool> ConfigAllowDropAllScrap;
        public static ConfigEntry<bool> ConfigDropAllRequiresDoublePress;
        public static ConfigEntry<float> ConfigDoublePressTimeWindow;
        public static ConfigEntry<bool> ConfigAllowPlaceAllScrapOnDesk;

        private readonly Harmony _harmony = new Harmony(ModGUID);

        /// <summary>
        /// Gets called when the mod gets loaded.
        /// </summary>
        public void Awake()
        {
            // Create and add logger
            Log = BepInEx.Logging.Logger.CreateLogSource(ModGUID);
            Log.LogInfo($"Loading {ModName} (v{ModVersion})");

            ConfigAllowDropAllScrap = Config.Bind("General", "AllowDropAllScrap", true, "Allows dropping all scrap when pressing your drop key on an empty slot");
            ConfigDropAllRequiresDoublePress = Config.Bind("General", "DropAllRequiresDoublePress", true, "Requires the drop key to be pressed twice on order to drop everything");
            ConfigDoublePressTimeWindow = Config.Bind("General", "DoublePressTimeWindow", 0.25f, "The amount of time in which the second press needs to occur for it to count as a double press in seconds");
            ConfigAllowPlaceAllScrapOnDesk = Config.Bind("General", "AllowPlaceAllScrapOnDesk", true, "Allows to quickly place all scrap in your inventory on the deposit desk when pressing your sprint key");

            PlayerControllerData.SwitchToItemSlot = typeof(PlayerControllerB)
                .GetMethod("SwitchToItemSlot", BindingFlags.Instance | BindingFlags.NonPublic);

            _harmony.PatchAll();

            Log.LogInfo($"Plugin: {ModName}, Version: {ModVersion} loaded!");
        }
    }
}
