using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace HS2_SexRobotController
{
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]

    public class HS2_SexRobotController : BaseUnityPlugin
    {
        public const string pluginGUID = "hs2robotics.HS2SexRobotController";
        public const string pluginName = "HS2_SexRobotController";
        public const string pluginVersion = "2.2";

        internal static HScene hScene;
        private Stopwatch sw = Stopwatch.StartNew();
        private RobotMovement robotMovement;
        private SerialPortConnection serialPortConnection;
        private static List<string> debugLogList;

        // the path in normal differs from VR
        private const string ButtonPath_MainGame = "UI/ClothPanel/ClothGp/ClothAllBt/Button";
        private const string ButtonPath_VR = "UI/Panel/ClothPanel/ClothGp/ClothAllBt/Button";

        void Start()
        {
            Hooks.InstallHooks();
            Harmony.CreateAndPatchAll(typeof(HS2_SexRobotController));
        }

        protected internal void Awake()
        {
            List<string> logList = new List<string>();
            debugLogList = new List<string>();
            HarmonyLib.Harmony.CreateAndPatchAll(typeof(HS2_SexRobotController));
            serialPortConnection = SerialPortConnection.GetInstance();
            robotMovement = RobotMovement.GetInstance();

            // Setup config file entries used in the in game menu
            // Creates a config file in BepInEx/config named hs2robotics.HS2SexRobotController.cfg
            // general
            serialPortConnection.diagnosticsConfig = Config.Bind(StringConstants.SexRobotGeneralSection, StringConstants.BepinExDebugOutput, false);
            serialPortConnection.printSceneName = Config.Bind(StringConstants.SexRobotGeneralSection, StringConstants.BepinExPrintPosition, false, new ConfigDescription(StringConstants.BepinExPrintPosition_Tooltip));
            serialPortConnection.readPositionsFromFile = Config.Bind(StringConstants.SexRobotGeneralSection, StringConstants.ReadPositionNamesFromFile, false, new ConfigDescription(StringConstants.ReadPositionNamesFromFile_Tooltip));
            // connection
            serialPortConnection.toggleSerialPortConnection = Config.Bind(StringConstants.SexRobotConnectionSection, StringConstants.ToggleSerialPortConnection, new KeyboardShortcut(KeyCode.S, KeyCode.LeftShift));
            (serialPortConnection.serialPortConfig = Config.Bind(StringConstants.SexRobotConnectionSection, StringConstants.SerialPortConfig, SerialPortConnection.SerialPorts[0], new ConfigDescription(StringConstants.SerialPortConfig_Tooltip, new AcceptableValueList<string>(SerialPortConnection.SerialPorts)))).SettingChanged += (s, e) =>
            {
                logList.AddRange(serialPortConnection.UpdateSerialPort());
            };
            serialPortConnection.sexRobotUpdateFrequencyConfig = Config.Bind(StringConstants.SexRobotConnectionSection, StringConstants.SexRobotUpdateFrequencyConfig, 30.0f, new ConfigDescription(StringConstants.SexRobotUpdateFrequencyConfig_Tooltip, new AcceptableValueRange<float>(1.0f, 120.0f)));
            serialPortConnection.serialPortStatus = Config.Bind(StringConstants.SexRobotConnectionSection, StringConstants.SerialPortStatus, StringConstants.SerialPortStatus_Tooltip);
            serialPortConnection.serialPortStatus.Value = serialPortConnection.serialPortConfig.Value + StringConstants.SerialPortStatus_Disconnected;
            (serialPortConnection.serialPortConnected = Config.Bind(StringConstants.SexRobotConnectionSection, StringConstants.SerialPortConnected, true)).SettingChanged += (s, e) =>
            {
                logList.AddRange(serialPortConnection.UpdateSerialPortConnection());
            };
            //multipliers
            serialPortConnection.strokeLengthMultiplierIncrease = Config.Bind(StringConstants.SexRobotLimitsSection, StringConstants.IncreaseStrokeMultiplierKey, new KeyboardShortcut(KeyCode.U));
            serialPortConnection.strokeLengthMultiplierDecrease = Config.Bind(StringConstants.SexRobotLimitsSection, StringConstants.DecreaseStrokeMultiplierKey, new KeyboardShortcut(KeyCode.T));
            serialPortConnection.robotL0Multiplier = Config.Bind(StringConstants.SexRobotLimitsSection, StringConstants.RobotL0Multiplier, 1.0f, new ConfigDescription(StringConstants.RobotL0Multiplier_Tooltip, new AcceptableValueRange<float>(0.25f, 5.0f)));
            serialPortConnection.robotL0MultiplierStepValue = Config.Bind(StringConstants.SexRobotLimitsSection, StringConstants.RobotL0MultiplierStepValue, 0.25f, new ConfigDescription(StringConstants.RobotL0MultiplierStepValue_Tooltip, new AcceptableValueRange<float>(0.01f, 1.0f)));
            serialPortConnection.robotL0Min = Config.Bind(StringConstants.SexRobotLimitsSection, StringConstants.RobotL0Min, 0.0f, new ConfigDescription(StringConstants.RobotL0Min_Tooltip, new AcceptableValueRange<float>(0.0f, 0.5f)));
            serialPortConnection.robotL0Max = Config.Bind(StringConstants.SexRobotLimitsSection, StringConstants.RobotL0Max, 1.0f, new ConfigDescription(StringConstants.RobotL0Max_Tooltip, new AcceptableValueRange<float>(0.5f, 1.0f)));
            serialPortConnection.robotL1Min = Config.Bind(StringConstants.SexRobotLimitsSection, StringConstants.RobotL1Min, 0.0f, new ConfigDescription(StringConstants.RobotL1Min_Tooltip, new AcceptableValueRange<float>(0.0f, 0.5f)));
            serialPortConnection.robotL1Max = Config.Bind(StringConstants.SexRobotLimitsSection, StringConstants.RobotL1Max, 1.0f, new ConfigDescription(StringConstants.RobotL1Max_Tooltip, new AcceptableValueRange<float>(0.5f, 1.0f)));
            serialPortConnection.robotL2Min = Config.Bind(StringConstants.SexRobotLimitsSection, StringConstants.RobotL2Min, 0.0f, new ConfigDescription(StringConstants.RobotL2Min_Tooltip, new AcceptableValueRange<float>(0.0f, 0.5f)));
            serialPortConnection.robotL2Max = Config.Bind(StringConstants.SexRobotLimitsSection, StringConstants.RobotL2Max, 1.0f, new ConfigDescription(StringConstants.RobotL2Max_Tooltip, new AcceptableValueRange<float>(0.5f, 1.0f)));
            serialPortConnection.robotR0Min = Config.Bind(StringConstants.SexRobotLimitsSection, StringConstants.RobotR0Min, 0.0f, new ConfigDescription(StringConstants.RobotR0Min_Tooltip, new AcceptableValueRange<float>(0.0f, 0.5f)));
            serialPortConnection.robotR0Max = Config.Bind(StringConstants.SexRobotLimitsSection, StringConstants.RobotR0Max, 1.0f, new ConfigDescription(StringConstants.RobotR0Max_Tooltip, new AcceptableValueRange<float>(0.5f, 1.0f)));
            serialPortConnection.robotR1Min = Config.Bind(StringConstants.SexRobotLimitsSection, StringConstants.RobotR1Min, 0.0f, new ConfigDescription(StringConstants.RobotR1Min_Tooltip, new AcceptableValueRange<float>(0.0f, 0.5f)));
            serialPortConnection.robotR1Max = Config.Bind(StringConstants.SexRobotLimitsSection, StringConstants.RobotR1Max, 1.0f, new ConfigDescription(StringConstants.RobotR1Max_Tooltip, new AcceptableValueRange<float>(0.5f, 1.0f)));
            serialPortConnection.robotR2Min = Config.Bind(StringConstants.SexRobotLimitsSection, StringConstants.RobotR2Min, 0.0f, new ConfigDescription(StringConstants.RobotR2Min_Tooltip, new AcceptableValueRange<float>(0.0f, 0.5f)));
            serialPortConnection.robotR2Max = Config.Bind(StringConstants.SexRobotLimitsSection, StringConstants.RobotR2Max, 1.0f, new ConfigDescription(StringConstants.RobotR2Max_Tooltip, new AcceptableValueRange<float>(0.5f, 1.0f)));
            //limiter
            serialPortConnection.togglelimitRobotStrokeLength = Config.Bind(StringConstants.SexRobotLimiterSection, StringConstants.ToggleStrokeLengthLimiter, new KeyboardShortcut(KeyCode.Space));
            serialPortConnection.limitRobotL0Length = Config.Bind(StringConstants.SexRobotLimiterSection, StringConstants.StrokeLengthLimiter, false, new ConfigDescription(StringConstants.StrokeLengthLimiter_Tooltip));
            serialPortConnection.limitRobotL0Multiplier = Config.Bind(StringConstants.SexRobotLimiterSection, StringConstants.StrokeLengthLimiterMultiplierValue, RobotMovement.LimitStrokeLengthMultiplier, new ConfigDescription(StringConstants.StrokeLengthLimiterMultiplierValue_Tooltip, new AcceptableValueRange<float>(0.25f, 5.0f)));

            if (serialPortConnection.serialPortConnected.Value)
            {
                logList.AddRange(serialPortConnection.UpdateSerialPortConnection());
            }
            printLog(logList);
        }

        private void printLog(List<string> logList)
        {
            if (logList != null)
            {
                foreach (string log in logList)
                {
                    Logger.LogInfo(log);
                }
            }
            if (debugLogList != null)
            {
                foreach (string log in debugLogList)
                {
                    Logger.LogInfo(log);
                }
                // empty list to avoid duplicate being printed
                debugLogList.Clear();
            }
        }

        internal static void SetupUIButtons()
        {
            SerialPortConnection serialPortConnection = SerialPortConnection.GetInstance();
            try
            {
                GameObject original = GameObject.Find(ButtonPath_MainGame);
                if (original == null)
                {
                    original = GameObject.Find(ButtonPath_VR);
                    if (original == null)
                        return;
                }

                serialPortConnection.buttonLimitRobotStrokeLength = Instantiate(original, original.transform.parent).transform;
                serialPortConnection.buttonLimitRobotStrokeLength.localPosition = new Vector3(0.0f, -100.0f, 0.0f);
                serialPortConnection.buttonLimitRobotStrokeLength.name = StringConstants.ButtonStrokeLengthLimiter_Name;

                Button button = serialPortConnection.buttonLimitRobotStrokeLength.GetComponentInChildren<Button>();
                button.onClick = new Button.ButtonClickedEvent();
                button.onClick.AddListener(() =>
                {
                    serialPortConnection.buttonLimitRobotStrokeLengthClicked = true;
                });

            }
            catch (Exception e)
            {
                debugLogList.Add("---> Error attempting to create GUI Button: " + e.ToString());
            }
        }


        private void Update()
        {
            try
            {
                printLog(serialPortConnection.CheckButtonAndSerialConnState());

                // Return if not in an HScene
                if (hScene == null)
                {
                    return;
                }

                // Get ms elapsed since current stopwatch interval
                float msElapsed = sw.ElapsedMilliseconds;

                // If the ms elapsed is greater than the period based on the robot's update frequency then
                // stop the stopwatch, call the robot update function, and restart the stopwatch
                if (msElapsed >= (1000.0 / serialPortConnection.sexRobotUpdateFrequencyConfig.Value))
                {
                    sw.Stop();

                    // check here if the speed needs to be updated, as updates only handle loops and not speed adjustment
                    if (robotMovement.loopType != hScene.ctrlFlag.loopType && !robotMovement.animationChanged)
                    {
                        robotMovement.speedChanged = true;
                        robotMovement.loopType = hScene.ctrlFlag.loopType;
                    }
                    printLog(robotMovement.updateAnimationStatus());
                    sw = Stopwatch.StartNew();
                }
            }
            catch (Exception ex)
            {
                Logger.LogInfo("UPDATE() ---> ERROR: " + ex.ToString());
            }
        }
    }
}

