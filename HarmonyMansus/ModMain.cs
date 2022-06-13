using HarmonyLib;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.UI;
using UnityEngine;

[HarmonyPatch]
public class ModMain
{
    public const string settingID = "FastSpeedScale";

    public static float speedFactor = 3;
    public static SpeedupTracker tracker;

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

    [HarmonyPrefix, HarmonyPatch(typeof(Heart), "Beat")]
    public static void AlteredBeatStep(ref float seconds, ref float metaseconds)
    {
        if (seconds == 0 || seconds == metaseconds) return;
        if (tracker == null) Init();
        seconds = metaseconds * speedFactor;
    }
}

public class SpeedupTracker : ISettingSubscriber
{
    static float[] factorMap = new float[] { 0.2f, 0.5f, 2, 3, 5, 8, 13, 21, 34 };
    public void WhenSettingUpdated(object newValue)
    {
        int idx = (newValue is int) ? (int)newValue : 1;
        idx = Mathf.Min(factorMap.Length - 1, Mathf.Max(idx, 0));
        ModMain.speedFactor = factorMap[idx];
    }
    public void BeforeSettingUpdated(object newValue) { }
}