using HarmonyLib;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.UI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

[HarmonyPatch]
public class ModMain
{
    public const string settingID = "FastSpeedScale";

    public static float speedStep = 3 * 0.05f;
    public static SpeedupTracker tracker;

    [HarmonyPrefix, HarmonyPatch(typeof(Heart), "Awake")]
    public static void Init()
    {
        Setting setting = Watchman.Get<Compendium>().GetEntityById<Setting>(settingID);
        if (setting == null)
        {
            NoonUtility.LogWarning("SETTING MISSING: " + settingID);
            return;
        }
        setting.AddSubscriber(tracker = new SpeedupTracker());
        tracker.WhenSettingUpdated(setting.CurrentValue);
    }

    [HarmonyTranspiler, HarmonyPatch(typeof(Heart), "ProcessBeatCounter")]
    public static IEnumerable<CodeInstruction> AlteredBeatStep(IEnumerable<CodeInstruction> instructions)
    {
        var beatFunc = AccessTools.Method(typeof(Heart), nameof(Heart.Beat), new System.Type[] { typeof(float), typeof(float) });
        var codes = new List<CodeInstruction>(instructions);
        for (int i = 0; i < codes.Count; i++)
        {
            var curr = codes[i];
            if (curr.opcode == OpCodes.Call && (curr.operand as MethodInfo).Name == "Beat") // this.Beat(0.15f, 0.05f);
            {
                codes[i - 2] = new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(ModMain), nameof(speedStep)));
                break;
            }
        }
        return codes.AsEnumerable();
    }
}

public class SpeedupTracker : ISettingSubscriber
{
    static float[] factorMap = new float[] { 0.2f, 0.5f, 2, 3, 5, 8, 13, 21, 34 };
    public void WhenSettingUpdated(object newValue)
    {
        int idx = (newValue is int) ? (int)newValue : 1;
        idx = Mathf.Min(factorMap.Length - 1, Mathf.Max(idx, 0));
        ModMain.speedStep = factorMap[idx] * 0.05f;
    }
    public void BeforeSettingUpdated(object newValue) { }
}