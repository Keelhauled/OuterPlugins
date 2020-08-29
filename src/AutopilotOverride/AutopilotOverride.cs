using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace AutopilotOverride
{
    [BepInPlugin("keelhauled.autopilotoverride", "AutopilotOverride", "1.0.0")]
    public class AutopilotOverride : BaseUnityPlugin
    {
        private static bool lowSpeedMode = false;
        private static float lowSpeedMax = 0.49f;

        private static NotificationData modeActive = new NotificationData(NotificationTarget.Ship, "LOW SPEED MODE ACTIVE", 0f, true);
        private static NotificationData modeDeactivated = new NotificationData(NotificationTarget.Ship, "LOW SPEED MODE DEACTIVATED", 3f, true);

        private void Awake()
        {
            Harmony.CreateAndPatchAll(GetType());
        }

        private void Update()
        {
            if(OWInput.IsInputMode(InputMode.ShipCockpit) && Input.GetKeyDown(KeyCode.L))
            {
                lowSpeedMode = !lowSpeedMode;

                if(lowSpeedMode)
                {
                    NotificationManager.SharedInstance.PostNotification(modeActive, true);
                }
                else
                {
                    if(NotificationManager.SharedInstance.IsPinnedNotification(modeActive))
                        NotificationManager.SharedInstance.UnpinNotification(modeActive);

                    NotificationManager.SharedInstance.PostNotification(modeDeactivated);
                }
            }
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(ShipThrusterController), "ReadTranslationalInput")]
        private static IEnumerable<CodeInstruction> AllowInput(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();

            for(int i = 0; i < codes.Count; i++)
            {
                if(codes[i].opcode == OpCodes.Callvirt &&
                    codes[i].operand.ToString() == "Boolean IsFlyingToDestination()")
                {
                    codes[i + 2].opcode = OpCodes.Nop;
                    codes[i + 3].opcode = OpCodes.Nop;
                    break;
                }
            }

            return codes;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ShipThrusterController), "ReadTranslationalInput")]
        private static void AbortAutopilot(Autopilot ____autopilot, ref Vector3 __result)
        {
            if(____autopilot.IsFlyingToDestination() && __result.magnitude > 0.1f)
                ____autopilot.Abort();

            if(lowSpeedMode)
            {
                __result.x = Mathf.Clamp(__result.x, -lowSpeedMax, lowSpeedMax);
                __result.y = Mathf.Clamp(__result.y, -lowSpeedMax, lowSpeedMax);
                __result.z = Mathf.Clamp(__result.z, -lowSpeedMax, lowSpeedMax);
            }
        }
    }
}
