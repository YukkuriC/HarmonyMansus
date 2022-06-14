using SecretHistories.Constants.Modding;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

[HarmonyPatch]
public class HarmonyMansus
{
    public static Dictionary<string, Harmony> patchers;
    public static void Initialise(ISecretHistoriesMod mod)
    {
        Patch(mod);
    }

    public static void Patch(ISecretHistoriesMod mod)
    {
        // init pool
        if (patchers == null) patchers = new Dictionary<string, Harmony>();

        // init patcher
        var modKey = mod.Author + "." + mod.Id;
        var patcher = patchers[modKey] = new Harmony(modKey);
        try
        {
            NoonUtility.Log("Patching HarmonyMansus: " + modKey);
            patcher.PatchAll(mod.LoadedAssembly);
            NoonUtility.Log("Patching HarmonyMansus done: " + modKey);
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