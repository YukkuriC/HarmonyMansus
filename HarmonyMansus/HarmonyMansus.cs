using SecretHistories.Constants.Modding;
using HarmonyLib;

public class HarmonyMansus
{
    public static void Initialise(ISecretHistoriesMod mod)
    {
        Harmony.CreateAndPatchAll(typeof(HarmonyMansus));
    }
}