using VoidManager.MPModChecks;

namespace MiniJump
{
    public class VoidManagerPlugin : VoidManager.VoidPlugin
    {
        public VoidManagerPlugin() {
            VoidManager.Events.Instance.LateUpdate += Jump.CheckDoJump;
        }

        public override MultiplayerType MPType => MultiplayerType.Host;

        public override string Author => "18107";

        public override string Description => "Allows the ship to make a short range jump within a system";
    }
}
