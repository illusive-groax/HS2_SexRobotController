namespace HS2_SexRobotController.Hooks
{
    internal partial class Hooks
    {
        public static void InstallHooks()
        {
            HarmonyLib.Harmony.CreateAndPatchAll(typeof(HSceneTriggers));
        }
    }
}
