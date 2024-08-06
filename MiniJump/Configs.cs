using BepInEx.Configuration;
using UnityEngine;

namespace MiniJump
{
    internal class Configs
    {
        public const int jumpCooldownSeconds = 90;

        internal static ConfigEntry<KeyCode> miniJumpKeybind;
        internal static ConfigEntry<bool> allowJumpConfig;

        internal static void Load(BepinPlugin plugin)
        {
            miniJumpKeybind = plugin.Config.Bind("MiniJump", "MiniJumpKey", KeyCode.G);
            allowJumpConfig = plugin.Config.Bind("MiniJump", "AllowJump", true);
        }
    }
}
