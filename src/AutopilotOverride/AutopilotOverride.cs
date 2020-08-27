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
        private void Awake()
        {
            Harmony.CreateAndPatchAll(GetType());
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
            if(____autopilot.IsFlyingToDestination() && __result.magnitude > 0f)
                ____autopilot.Abort();
        }
    }
}
