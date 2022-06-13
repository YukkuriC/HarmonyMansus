using SecretHistories.Infrastructure.Modding;
using HarmonyLib;
using System;

[HarmonyPatch]
public class AdjustableFastForward
{
    public static Harmony patcher;
    public static void Initialise(ISecretHistoriesMod mod)
    {
        patcher = new Harmony("yukkuric." + mod.Id);
        try
        {
            patcher.PatchAll();
        }
        catch (Exception e)
        {
            NoonUtility.LogException(e);
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(MenuScreenController), "Exit")]
    public static void ExitBeforeCrash()
    {
        patcher.UnpatchAll();
    }
}