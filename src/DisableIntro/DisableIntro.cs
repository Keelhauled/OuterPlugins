using BepInEx;
using HarmonyLib;

namespace DisableIntro
{
    [BepInPlugin("keelhauled.disableintro", "DisableIntro", "1.0.0")]
    public class DisableIntro : BaseUnityPlugin
    {
        private void Awake()
        {
            Harmony.CreateAndPatchAll(GetType());
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TitleScreenAnimation), "Awake")]
        private static void Patch1(TitleScreenAnimation __instance)
        {
            var instance = Traverse.Create(__instance);
            instance.Field("_gamepadSplash").SetValue(false);
            instance.Field("_introPan").SetValue(false);
            instance.Field("_fadeDuration").SetValue(0.001f);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(TitleAnimationController), "Awake")]
        private static void Patch2(TitleAnimationController __instance)
        {
            var instance = Traverse.Create(__instance);
            instance.Field("_logoFadeDelay").SetValue(0.001f);
            instance.Field("_logoFadeDuration").SetValue(0.001f);
            instance.Field("_optionsFadeDelay").SetValue(0.001f);
            instance.Field("_optionsFadeDuration").SetValue(0.001f);
            instance.Field("_optionsFadeSpacing").SetValue(0.001f);
        }
    }
}
