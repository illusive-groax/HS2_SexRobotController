using BepInEx.Logging;
using HarmonyLib;
using IllusionUtility.GetUtility;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace HS2_SexRobotController
{
    internal class HSceneTriggers
    {
        private static RobotMovement robotMovement;
        private static SerialPortConnection serialPortConnection;

        // Hook method to grab the HScene instance from
        [HarmonyPostfix, HarmonyPatch(typeof(HScene), "SetStartVoice")]
        public static void HScene_SetStartVoice(HScene __instance)
        {
            robotMovement = RobotMovement.GetInstance();
            robotMovement.males = __instance.GetMales().Where(male => male != null).ToArray();
            robotMovement.females = __instance.GetFemales().Where(female => female != null).ToArray();
            HS2_SexRobotController.hScene = __instance;
            // create a button in the clothing menu which allows for enabling/disabling the limiter
            // currently the icon used is the same as for the dressing state (but has no effect on clothing state itself)
            HS2_SexRobotController.SetupUIButtons();
        }

        // Hook method to grab the HScene animation name from
        [HarmonyPrefix, HarmonyPatch(typeof(HScene), "ChangeAnimation")]
        private static void HScene_PreChangeAnimation(HScene.AnimationListInfo _info)
        {
            robotMovement = RobotMovement.GetInstance();
            serialPortConnection = SerialPortConnection.GetInstance();
            robotMovement.animationChanged = true;
            robotMovement.updatePosition = false;
            robotMovement.speedChanged = false;
            robotMovement.animationName = _info.nameAnimation;
            checkAnimationName();
            updateAnimationDictionary();
        }

        private static void checkAnimationName()
        {
            // check current animation name (for finding unregistered sex-animations)
            // verify that position doesn't exist and isn't already printed
            if (serialPortConnection.printSceneName.Value &&
                robotMovement.animationName != robotMovement.prevAnimationName &&
                !BoneAnimationDefiner.animationFemaleTargetDictionary.ContainsKey(robotMovement.animationName))
            {
                // set the currently unknown animation name
                robotMovement.prevAnimationName = robotMovement.animationName;
                // create a temporary logger object which prints the current animation name
                var animationLogger = new ManualLogSource("HS2_SexRobotController");
                BepInEx.Logging.Logger.Sources.Add(animationLogger);
                animationLogger.LogInfo("Current Animation: " + robotMovement.prevAnimationName);
                BepInEx.Logging.Logger.Sources.Remove(animationLogger);
            }
        }

        private static void updateAnimationDictionary()
        {
            //check if positions should be read from file
            if (serialPortConnection.readPositionsFromFile.Value 
                && !serialPortConnection.fileIsRead)
            {
                var logger = new ManualLogSource("HS2_SexRobotController");
                BepInEx.Logging.Logger.Sources.Add(logger);
                try
                {
                    // read positions from file
                    FileHandler.readPositionsFromFile();
                }
                catch (Exception e)
                {
                    logger.LogInfo("Error updating Animation dictionary: " + e.ToString());
                }
                // regardless of result, consider file to be read (no point re-reading file with errors)
                serialPortConnection.fileIsRead = true;
                BepInEx.Logging.Logger.Sources.Remove(logger);
            }
            else if (!serialPortConnection.readPositionsFromFile.Value)
            {
                // if disabled, set read to false, 
                // to enable the possibility to reload file content with new animations without restarting the game
                serialPortConnection.fileIsRead = false;
            }
        }

        // Hook method to inject UI buttons into the main Honey Select 2 config menu
        [HarmonyPostfix, HarmonyPatch(typeof(Config.ConfigWindow), "Initialize")]
        private static void SetupUIButtons(Config.ConfigWindow __instance, ref Button[] ___buttons)
        {
            serialPortConnection = SerialPortConnection.GetInstance();
            // Get main button to instantiate in order to create our new buttons
            Transform btnTitle = __instance.transform.FindLoop("btnTitle").transform;

            // Create connect robot button by instantiating main button, changing it's name, text label, and adding a new listener to handle click events
            serialPortConnection.buttonConnectRobot = UnityEngine.Object.Instantiate(btnTitle, btnTitle.parent);
            serialPortConnection.buttonConnectRobot.name = StringConstants.ButtonConnectRobot_Name;
            serialPortConnection.buttonConnectRobotText = serialPortConnection.buttonConnectRobot.GetComponentInChildren<Text>();
            serialPortConnection.buttonConnectRobotText.text = StringConstants.ButtonConnectRobot_Text;
            serialPortConnection.buttonConnectRobotText.fontSize = 18;
            Button newButton = serialPortConnection.buttonConnectRobot.GetComponentInChildren<Button>();
            newButton.onClick = new Button.ButtonClickedEvent();
            newButton.onClick.AddListener(() =>
            {
                serialPortConnection.buttonConnectRobotClicked = true;
            });

            // Create disconnect robot button by instantiating main button, changing it's name, text label, and adding a new listener to handle click events
            serialPortConnection.buttonDisconnectRobot = UnityEngine.Object.Instantiate(btnTitle, btnTitle.parent);
            serialPortConnection.buttonDisconnectRobot.name = StringConstants.ButtonDisconnectRobot_Name;
            serialPortConnection.buttonDisconnectRobotText = serialPortConnection.buttonDisconnectRobot.GetComponentInChildren<Text>();
            serialPortConnection.buttonDisconnectRobotText.text = StringConstants.ButtonDisconnectRobot_Text;
            serialPortConnection.buttonDisconnectRobotText.fontSize = 18;
            newButton = serialPortConnection.buttonDisconnectRobot.GetComponentInChildren<Button>();
            newButton.onClick = new Button.ButtonClickedEvent();
            newButton.onClick.AddListener(() =>
            {
                serialPortConnection.buttonDisconnectRobotClicked = true;
            });

            // Create robot stroke multiplier increase button by instantiating main button, changing it's name, text label, and adding a new listener to handle click events
            serialPortConnection.buttonStrokeMultiplierIncrease = UnityEngine.Object.Instantiate(btnTitle, btnTitle.parent);
            serialPortConnection.buttonStrokeMultiplierIncrease.name = StringConstants.ButtonIncreaseStrokeLength_Name;
            serialPortConnection.buttonStrokeMultiplierIncreaseText = serialPortConnection.buttonStrokeMultiplierIncrease.GetComponentInChildren<Text>();
            serialPortConnection.buttonStrokeMultiplierIncreaseText.text = StringConstants.ButtonIncreaseStrokeLength_Text;
            serialPortConnection.buttonStrokeMultiplierIncreaseText.fontSize = 18;
            newButton = serialPortConnection.buttonStrokeMultiplierIncrease.GetComponentInChildren<Button>();
            newButton.onClick = new Button.ButtonClickedEvent();
            newButton.onClick.AddListener(() =>
            {
                serialPortConnection.buttonStrokeMultiplierIncreaseClicked = true;
            });

            // Create robot stroke multiplier decrease button by instantiating main button, changing it's name, text label, and adding a new listener to handle click events
            serialPortConnection.buttonStrokeMultiplierDecrease = UnityEngine.Object.Instantiate(btnTitle, btnTitle.parent);
            serialPortConnection.buttonStrokeMultiplierDecrease.name = StringConstants.ButtonDecreaseStrokeLength_Name;
            serialPortConnection.buttonStrokeMultiplierDecreaseText = serialPortConnection.buttonStrokeMultiplierDecrease.GetComponentInChildren<Text>();
            serialPortConnection.buttonStrokeMultiplierDecreaseText.text = StringConstants.ButtonDecreaseStrokeLength_Text;
            serialPortConnection.buttonStrokeMultiplierDecreaseText.fontSize = 18;
            newButton = serialPortConnection.buttonStrokeMultiplierDecrease.GetComponentInChildren<Button>();
            newButton.onClick = new Button.ButtonClickedEvent();
            newButton.onClick.AddListener(() =>
            {
                serialPortConnection.buttonStrokeMultiplierDecreaseClicked = true;
            });

            // Create robot stroke limiter button by instantiating main button, changing it's name, text label, and adding a new listener to handle click events
            serialPortConnection.buttonLimitRobotStrokeLength = UnityEngine.Object.Instantiate(btnTitle, btnTitle.parent);
            serialPortConnection.buttonLimitRobotStrokeLength.name = StringConstants.ButtonStrokeLengthLimiter_Name;
            serialPortConnection.buttonLimitRobotStrokeLengthText = serialPortConnection.buttonLimitRobotStrokeLength.GetComponentInChildren<Text>();
            serialPortConnection.buttonLimitRobotStrokeLengthText.text = StringConstants.ButtonStrokeLengthLimiter_Text;
            serialPortConnection.buttonLimitRobotStrokeLengthText.fontSize = 18;
            newButton = serialPortConnection.buttonLimitRobotStrokeLength.GetComponentInChildren<Button>();
            newButton.onClick = new Button.ButtonClickedEvent();
            newButton.onClick.AddListener(() =>
            {
                serialPortConnection.buttonLimitRobotStrokeLengthClicked = true;
            });
        }

    }
}
