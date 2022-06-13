using HarmonyLib;

[HarmonyPatch]
public class ModMain
{
    [HarmonyPrefix, HarmonyPatch(typeof(Heart), "Beat")]
    public static void AlteredBeatStep(ref float seconds, ref float metaseconds)
    {
        if (seconds == 0 || seconds == metaseconds) return;
        seconds = 1; // TODO: read config
    }
}
