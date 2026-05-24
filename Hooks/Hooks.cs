namespace HS2_SexRobotController
{
    internal class Hooks
    {
        public static void InstallHooks()
        {
            HarmonyLib.Harmony.CreateAndPatchAll(typeof(HSceneTriggers));
        }

        private static HS2_SexRobotController GetController()
        {
            return UnityEngine.Object.FindAnyObjectByType<HS2_SexRobotController>();
        }
    }
}
