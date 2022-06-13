using HarmonyLib;
using SecretHistories.Constants.Modding;

[HarmonyPatch]
public class TheMod
{
    public static void Initialise(ISecretHistoriesMod mod) => HarmonyMansus.Patch(mod);

    [HarmonyPostfix,HarmonyPatch(typeof(Heart),"Beat")]
    public static void test(float seconds,float metaseconds)
    {
        NoonUtility.Log(string.Format("BEAT: {0},{1}", seconds, metaseconds));
    }
}
