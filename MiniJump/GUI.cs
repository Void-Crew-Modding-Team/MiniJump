using VoidManager.CustomGUI;
using VoidManager.Utilities;

namespace MiniJump
{
    internal class GUI : ModSettingsMenu
    {
        public override string Name() => "Mini Jump";

        public override void Draw()
        {
            GUITools.DrawCheckbox("Allow mini jump", ref Configs.allowJumpConfig);
            GUITools.DrawChangeKeybindButton("Mini jump keybind", ref Configs.miniJumpKeybind);
        }
    }
}
