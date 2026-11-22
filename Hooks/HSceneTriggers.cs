using HarmonyLib;
using HS2_SexRobotController.Helpers;
using HS2_SexRobotController.Plugin;
using HS2_SexRobotController.RobotController;
using IllusionUtility.GetUtility;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace HS2_SexRobotController.Hooks
{
    internal static partial class Hooks
    {

        private static class HSceneTriggers
        {

            // Hook method to grab the HScene instance from
            [HarmonyPostfix, HarmonyPatch(typeof(HScene), "SetStartVoice")]
            public static void HScene_SetStartVoice(HScene __instance)
            {
                RobotMovement robotMovement = RobotMovement.GetInstance();
                robotMovement.Males = __instance.GetMales().Where(male => male != null).ToArray();
                robotMovement.Females = __instance.GetFemales().Where(female => female != null).ToArray();
                HS2_SexRobotControllerPlugin.hScene = __instance;
            }

            // Hook method to grab the HScene animation name from
            [HarmonyPrefix, HarmonyPatch(typeof(HScene), "ChangeAnimation")]
            private static void HScene_PreChangeAnimation(HScene.AnimationListInfo _info)
            {
                RobotMovement robotMovement = RobotMovement.GetInstance();
                robotMovement.AnimationChanged = true;
                robotMovement.UpdatePosition = false;
                robotMovement.SpeedChanged = false;
                robotMovement.AnimationName = _info.nameAnimation;
                HS2_SexRobotControllerPlugin.AnimationNameCheck();
            }

            // Hook method to inject UI buttons into the main Honey Select 2 config menu
            [HarmonyPostfix, HarmonyPatch(typeof(Config.ConfigWindow), "Initialize")]
            private static void SetupUIButtons(Config.ConfigWindow __instance, ref Button[] ___buttons)
            {
                // Get main button to instantiate in order to create our new buttons
                Transform btnTitle = __instance.transform.FindLoop(StringConstants.ButtonPath_Settings).transform;

                HS2_SexRobotControllerPlugin.SetupUIButtons(btnTitle);
            }
        }
    }
}
