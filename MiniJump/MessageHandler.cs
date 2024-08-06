using Photon.Pun;
using Photon.Realtime;
using System;
using VoidManager.ModMessages;
using VoidManager.Utilities;

namespace MiniJump
{
    internal class MessageHandler : ModMessage
    {
        private const int version = 2;

        private enum MessageType
        {
            request,
            approve,
            deny,
            notify
        }

        public override void Handle(object[] arguments, Player sender)
        {
            if (arguments.Length < 2) return;

            if ((int)arguments[0] != version)
            {
                BepinPlugin.Log.LogInfo($"Expected version {version}, got {(int)arguments[0]}");
                return;
            }

            switch ((MessageType)arguments[1])
            {
                case MessageType.request:
                    if (!PhotonNetwork.IsMasterClient) return;
                    if (!Configs.allowJumpConfig.Value) {
                        DenyJump(sender, 0);
                        break;
                    }
                    int cooldown = Jump.CooldownRemaining();
                    if (cooldown == 0)
                        ApproveJump(sender);
                    else
                        DenyJump(sender, cooldown);
                    break;
                case MessageType.approve:
                    if (sender != PhotonNetwork.MasterClient) return;
                    Jump.DoJump();
                    break;
                case MessageType.deny:
                    if (sender != PhotonNetwork.MasterClient) return;
                    if (arguments.Length >= 3)
                    {
                        int cooldown1 = (int)arguments[2];
                        if (cooldown1 == 0)
                            Messaging.Notification("Mini Jump currently disabled by host", 8000);
                        else
                            Messaging.Notification($"Mini Jump on cooldown for {cooldown1} seconds", 8000);
                    }
                    break;
                case MessageType.notify:
                    Jump.lastJumpTime = DateTime.Now;
                    break;
            }
        }

        internal static void RequestJump()
        {
            if (PhotonNetwork.IsMasterClient) return;

            Send(MyPluginInfo.PLUGIN_GUID, GetIdentifier(typeof(MessageHandler)), ReceiverGroup.MasterClient, new object[] { version, MessageType.request });
        }

        internal static void ApproveJump(Player sender)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            Send(MyPluginInfo.PLUGIN_GUID, GetIdentifier(typeof(MessageHandler)), sender, new object[] { version, MessageType.approve });
        }

        internal static void DenyJump(Player sender, int cooldown)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            Send(MyPluginInfo.PLUGIN_GUID, GetIdentifier(typeof(MessageHandler)), sender, new object[] { version, MessageType.deny, cooldown });
        }

        internal static void NotifyJump()
        {
            Send(MyPluginInfo.PLUGIN_GUID, GetIdentifier(typeof(MessageHandler)), ReceiverGroup.All, new object[] { version, MessageType.notify });
        }
    }
}
