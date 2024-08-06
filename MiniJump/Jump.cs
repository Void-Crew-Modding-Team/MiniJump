using BepInEx;
using CG;
using CG.Game;
using CG.Game.SpaceObjects.Controllers;
using CG.Input;
using CG.Ship.Modules;
using CG.Space;
using Gameplay.SpacePlatforms;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Reflection;
using UnityEngine;
using VoidManager.Utilities;

namespace MiniJump
{
    internal class Jump
    {
        private static readonly FieldInfo playerShipSpacePlatformField = AccessTools.Field(typeof(AbstractPlayerControlledShip), "playerShipSpacePlatform");

        internal static DateTime lastJumpTime = DateTime.MinValue;

        //Copied directly from VoidManager 1.1.8
        //TODO Use Tools.InVoid
        private static bool InVoid
        {
            get
            {
                VoidJumpSystem voidJumpSystem = ClientGame.Current?.PlayerShip?.GameObject?.GetComponent<VoidJumpSystem>();
                if (voidJumpSystem == null)
                {
                    return false;
                }

                VoidJumpState activeState = voidJumpSystem.ActiveState;
                if (activeState is VoidJumpTravellingStable || activeState is VoidJumpTravellingUnstable || activeState is VoidJumpInterdiction || activeState is VoidJumpApproachingDestination || activeState is VoidJumpSpinningDown)
                {
                    return true;
                }

                return false;
            }
        }

        internal static int CooldownRemaining()
        {
            int remaining = (int)(lastJumpTime.AddSeconds(Configs.jumpCooldownSeconds) - DateTime.Now).TotalSeconds;
            if (remaining > 0)
            {
                return remaining;
            }
            else
                return 0;
        }

        internal static void CheckDoJump(object sender, EventArgs e)
        {
            //TODO Use Tools.InVoid
            if (!Tools.PlayerShipExists || InVoid ||
            ServiceBase<InputService>.Instance?.CursorVisibilityControl?.IsCursorShown != false) return;

            if (Configs.miniJumpKeybind.Value != KeyCode.None && UnityInput.Current.GetKeyDown(Configs.miniJumpKeybind.Value))
            {
                AbstractPlayerControlledShip ship = ClientGame.Current.PlayerShip;
                TakeoverChair chair = ship.GetComponentInChildren<Helm>().Chair as TakeoverChair;
                if (chair.IsAvailable || !chair.photonView.AmOwner) return;

                VoidDriveModule voidDrive = ship.GameObject.GetComponentInChildren<VoidDriveModule>();
                if (!voidDrive.IsFullyCharged)
                {
                    Messaging.Notification("Void drive not charged", 8000);
                    return;
                }

                int cooldown = CooldownRemaining();
                if (cooldown > 0)
                {
                    Messaging.Notification($"Mini Jump on cooldown for {cooldown} seconds", 8000);
                    return;
                }

                if (PhotonNetwork.IsMasterClient)
                {
                    if (Configs.allowJumpConfig.Value)
                    {
                        DoJump();
                    }
                }
                else
                {
                    MessageHandler.RequestJump();
                }
            }
        }

        internal static void DoJump()
        {
            AbstractPlayerControlledShip ship = ClientGame.Current?.PlayerShip;
            VoidDriveModule voidDrive = ship.GameObject.GetComponentInChildren<VoidDriveModule>();

            voidDrive.StopEnginesCharging();
            MovingSpacePlatform platform = (MovingSpacePlatform)playerShipSpacePlatformField.GetValue(ship);
            Vector3 forward = platform.Rotation * Vector3.forward;

            AccelerateThenJump(platform, forward * 50, forward * 4500, 20);
        }

        private static void AccelerateThenJump(MovingSpacePlatform platform, Vector3 velocityIncrease, Vector3 jumpDistance, int durationTicks)
        {
            platform.Velocity += velocityIncrease;
            if (durationTicks > 0)
            {
                Tools.DelayDo(() => AccelerateThenJump(platform, velocityIncrease, jumpDistance, durationTicks - 1), 1);
            }
            else
            {
                platform.CorrectedPosition += jumpDistance;
                MessageHandler.NotifyJump();
            }
        }
    }
}
