using SecretHistories.Constants.Modding;
using HarmonyLib;

public class HarmonyMansus
{
    public static void Initialise(ISecretHistoriesMod mod)
    {
        var patcher = new Harmony("yukkuric." + mod.Id);
        patcher.PatchAll();
    }
}