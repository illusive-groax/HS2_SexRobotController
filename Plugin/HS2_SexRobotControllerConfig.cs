using BepInEx;
using BepInEx.Configuration;
using HS2_SexRobotController.Helpers;
using HS2_SexRobotController.RobotController;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace HS2_SexRobotController.Plugin
{
    internal partial class HS2_SexRobotControllerPlugin : BaseUnityPlugin
    {
        internal static ConfigEntry<string> SerialPortConfig { get; set; }
        internal static ConfigEntry<string> SerialPortStatus { get; set; }
        internal static ConfigEntry<bool> SerialPortConnected { get; set; }
        internal static ConfigEntry<KeyboardShortcut> ToggleSerialPortConnection { get; set; }
        internal static ConfigEntry<float> SexRobotUpdateFrequencyConfig { get; set; }
        internal static ConfigEntry<bool> DiagnosticsConfig { get; set; }
        internal static ConfigEntry<bool> ReadAnimationsFromFile { get; set; }
        internal static ConfigEntry<bool> WriteAnimationsToFile { get; set; }
        internal static ConfigEntry<float> RobotL0Min { get; set; }
        internal static ConfigEntry<float> RobotL0Max { get; set; }
        internal static ConfigEntry<float> RobotL1Min { get; set; }
        internal static ConfigEntry<float> RobotL1Max { get; set; }
        internal static ConfigEntry<float> RobotL2Min { get; set; }
        internal static ConfigEntry<float> RobotL2Max { get; set; }
        internal static ConfigEntry<float> RobotR0Min { get; set; }
        internal static ConfigEntry<float> RobotR0Max { get; set; }
        internal static ConfigEntry<float> RobotR1Min { get; set; }
        internal static ConfigEntry<float> RobotR1Max { get; set; }
        internal static ConfigEntry<float> RobotR2Min { get; set; }
        internal static ConfigEntry<float> RobotR2Max { get; set; }
        // multiplier for idle
        internal static ConfigEntry<float> RobotL0MovementMultiplier_Idle { get; set; }
        internal static ConfigEntry<float> RobotL0MovementMultiplier_Climax { get; set; }
        // multipliers for service
        internal static ConfigEntry<float> RobotL0MovementMultiplierService_Weak { get; set; }
        internal static ConfigEntry<float> RobotL0MovementMultiplierService_Strong { get; set; }
        internal static ConfigEntry<float> RobotL0MovementMultiplierService_Orgasm { get; set; }
        // multipliers penetration
        internal static ConfigEntry<float> RobotL0MovementMultiplierPenetration_Weak { get; set; }
        internal static ConfigEntry<float> RobotL0MovementMultiplierPenetration_Strong { get; set; }
        internal static ConfigEntry<float> RobotL0MovementMultiplierPenetration_Orgasm { get; set; }

        internal static Text BtnConnectRobotText;

        internal static Transform BtnConnectRobot;

        internal static bool FileIsRead = false;

        internal static bool BtnConnectRobotClicked = false;

        internal static bool BtnDisconnectRobotClicked = false;

        private const float DefaultStrokeLength = 1.0f;

        private void SetupPluginConfigurations()
        {
            _serialPortConnection = SerialPortConnection.GetInstance();
            // Setup config file entries used in the in game menu
            // Creates a config file in BepInEx/config named KKSrobotics.KKSSexRobotController.cfg
            // general
            DiagnosticsConfig = Config.Bind(StringConstants.SexRobotGeneralSection, StringConstants.BepinExDebugOutput, false);
            ReadAnimationsFromFile = Config.Bind(StringConstants.SexRobotGeneralSection, StringConstants.ReadAnimationsFromFile, false, new ConfigDescription(StringConstants.ReadAnimationsFromFile_Tooltip));
            WriteAnimationsToFile = Config.Bind(StringConstants.SexRobotGeneralSection, StringConstants.WriteNotFoundPositionsToFile, false, new ConfigDescription(StringConstants.WriteNotFoundPositionsToFile_Tooltip));
            // connection
            ToggleSerialPortConnection = Config.Bind(StringConstants.SexRobotConnectionSection, StringConstants.ToggleSerialPortConnection, new KeyboardShortcut(KeyCode.S, KeyCode.LeftShift));
            (SerialPortConfig = Config.Bind(StringConstants.SexRobotConnectionSection, StringConstants.SerialPortConfig, StringConstants.SerialPorts[0], new ConfigDescription(StringConstants.SerialPortConfig_Tooltip, new AcceptableValueList<string>(StringConstants.SerialPorts)))).SettingChanged += (s, e) =>
            {
                _serialPortConnection.UpdateSerialPort();
            };
            SexRobotUpdateFrequencyConfig = Config.Bind(StringConstants.SexRobotConnectionSection, StringConstants.SexRobotUpdateFrequencyConfig, 30.0f, new ConfigDescription(StringConstants.SexRobotUpdateFrequencyConfig_Tooltip, new AcceptableValueRange<float>(1.0f, 120.0f)));
            SerialPortStatus = Config.Bind(StringConstants.SexRobotConnectionSection, StringConstants.SerialPortStatus, StringConstants.SerialPortStatus_Tooltip);
            SerialPortStatus.Value = SerialPortConfig.Value + StringConstants.SerialPortStatus_Disconnected;
            (SerialPortConnected = Config.Bind(StringConstants.SexRobotConnectionSection, StringConstants.SerialPortConnected, true)).SettingChanged += (s, e) =>
            {
                _serialPortConnection.UpdateSerialPortConnection();
            };
            // L0: Idle
            RobotL0MovementMultiplier_Idle = Config.Bind(StringConstants.SexRobotL0IdleClimaxSection, StringConstants.RobotL0MovementMultiplierIdle, DefaultStrokeLength,
                new ConfigDescription(StringConstants.RobotL0MovementMultiplier_Tooltip, new AcceptableValueRange<float>(RobotMovement.L0_MOVEMENT_MULTIPLIER_MIN, RobotMovement.L0_MOVEMENT_MULTIPLIER_MAX)));
            RobotL0MovementMultiplier_Climax = Config.Bind(StringConstants.SexRobotL0IdleClimaxSection, StringConstants.RobotL0MovementMultiplierClimax, DefaultStrokeLength,
                new ConfigDescription(StringConstants.RobotL0MovementMultiplier_Tooltip, new AcceptableValueRange<float>(RobotMovement.L0_MOVEMENT_MULTIPLIER_MIN, RobotMovement.L0_MOVEMENT_MULTIPLIER_MAX)));
            // L0: Non-insertion (Service) multipliers (LoopType)
            RobotL0MovementMultiplierService_Weak = Config.Bind(StringConstants.SexRobotL0ServiceSection, StringConstants.RobotL0MovementMultiplierWeak, DefaultStrokeLength,
                new ConfigDescription(StringConstants.RobotL0MovementMultiplier_Tooltip, new AcceptableValueRange<float>(RobotMovement.L0_MOVEMENT_MULTIPLIER_MIN, RobotMovement.L0_MOVEMENT_MULTIPLIER_MAX)));
            RobotL0MovementMultiplierService_Strong = Config.Bind(StringConstants.SexRobotL0ServiceSection, StringConstants.RobotL0MovementMultiplierStrong, DefaultStrokeLength,
                new ConfigDescription(StringConstants.RobotL0MovementMultiplier_Tooltip, new AcceptableValueRange<float>(RobotMovement.L0_MOVEMENT_MULTIPLIER_MIN, RobotMovement.L0_MOVEMENT_MULTIPLIER_MAX)));
            RobotL0MovementMultiplierService_Orgasm = Config.Bind(StringConstants.SexRobotL0ServiceSection, StringConstants.RobotL0MovementMultiplierOrgasm, DefaultStrokeLength,
                new ConfigDescription(StringConstants.RobotL0MovementMultiplier_Tooltip, new AcceptableValueRange<float>(RobotMovement.L0_MOVEMENT_MULTIPLIER_MIN, RobotMovement.L0_MOVEMENT_MULTIPLIER_MAX)));
            // L0: Insertion/Penetration multipliers (LoopType)
            RobotL0MovementMultiplierPenetration_Weak = Config.Bind(StringConstants.SexRobotL0InsertionSection, StringConstants.RobotL0MovementMultiplierWeak, DefaultStrokeLength,
                new ConfigDescription(StringConstants.RobotL0MovementMultiplier_Tooltip, new AcceptableValueRange<float>(RobotMovement.L0_MOVEMENT_MULTIPLIER_MIN, RobotMovement.L0_MOVEMENT_MULTIPLIER_MAX)));
            RobotL0MovementMultiplierPenetration_Strong = Config.Bind(StringConstants.SexRobotL0InsertionSection, StringConstants.RobotL0MovementMultiplierStrong, DefaultStrokeLength,
                new ConfigDescription(StringConstants.RobotL0MovementMultiplier_Tooltip, new AcceptableValueRange<float>(RobotMovement.L0_MOVEMENT_MULTIPLIER_MIN, RobotMovement.L0_MOVEMENT_MULTIPLIER_MAX)));
            RobotL0MovementMultiplierPenetration_Orgasm = Config.Bind(StringConstants.SexRobotL0InsertionSection, StringConstants.RobotL0MovementMultiplierOrgasm, DefaultStrokeLength,
                new ConfigDescription(StringConstants.RobotL0MovementMultiplier_Tooltip, new AcceptableValueRange<float>(RobotMovement.L0_MOVEMENT_MULTIPLIER_MIN, RobotMovement.L0_MOVEMENT_MULTIPLIER_MAX)));

            RobotL0Min = Config.Bind(StringConstants.SexRobotMinMaxSection, StringConstants.RobotL0Min, 0.0f, new ConfigDescription(StringConstants.RobotL0Min_Tooltip, new AcceptableValueRange<float>(0.0f, 0.5f)));
            RobotL0Max = Config.Bind(StringConstants.SexRobotMinMaxSection, StringConstants.RobotL0Max, 1.0f, new ConfigDescription(StringConstants.RobotL0Max_Tooltip, new AcceptableValueRange<float>(0.5f, 1.0f)));
            RobotL1Min = Config.Bind(StringConstants.SexRobotMinMaxSection, StringConstants.RobotL1Min, 0.0f, new ConfigDescription(StringConstants.RobotL1Min_Tooltip, new AcceptableValueRange<float>(0.0f, 0.5f)));
            RobotL1Max = Config.Bind(StringConstants.SexRobotMinMaxSection, StringConstants.RobotL1Max, 1.0f, new ConfigDescription(StringConstants.RobotL1Max_Tooltip, new AcceptableValueRange<float>(0.5f, 1.0f)));
            RobotL2Min = Config.Bind(StringConstants.SexRobotMinMaxSection, StringConstants.RobotL2Min, 0.0f, new ConfigDescription(StringConstants.RobotL2Min_Tooltip, new AcceptableValueRange<float>(0.0f, 0.5f)));
            RobotL2Max = Config.Bind(StringConstants.SexRobotMinMaxSection, StringConstants.RobotL2Max, 1.0f, new ConfigDescription(StringConstants.RobotL2Max_Tooltip, new AcceptableValueRange<float>(0.5f, 1.0f)));
            RobotR0Min = Config.Bind(StringConstants.SexRobotMinMaxSection, StringConstants.RobotR0Min, 0.0f, new ConfigDescription(StringConstants.RobotR0Min_Tooltip, new AcceptableValueRange<float>(0.0f, 0.5f)));
            RobotR0Max = Config.Bind(StringConstants.SexRobotMinMaxSection, StringConstants.RobotR0Max, 1.0f, new ConfigDescription(StringConstants.RobotR0Max_Tooltip, new AcceptableValueRange<float>(0.5f, 1.0f)));
            RobotR1Min = Config.Bind(StringConstants.SexRobotMinMaxSection, StringConstants.RobotR1Min, 0.0f, new ConfigDescription(StringConstants.RobotR1Min_Tooltip, new AcceptableValueRange<float>(0.0f, 0.5f)));
            RobotR1Max = Config.Bind(StringConstants.SexRobotMinMaxSection, StringConstants.RobotR1Max, 1.0f, new ConfigDescription(StringConstants.RobotR1Max_Tooltip, new AcceptableValueRange<float>(0.5f, 1.0f)));
            RobotR2Min = Config.Bind(StringConstants.SexRobotMinMaxSection, StringConstants.RobotR2Min, 0.0f, new ConfigDescription(StringConstants.RobotR2Min_Tooltip, new AcceptableValueRange<float>(0.0f, 0.5f)));
            RobotR2Max = Config.Bind(StringConstants.SexRobotMinMaxSection, StringConstants.RobotR2Max, 1.0f, new ConfigDescription(StringConstants.RobotR2Max_Tooltip, new AcceptableValueRange<float>(0.5f, 1.0f)));

            if (SerialPortConnected.Value)
                _serialPortConnection.UpdateSerialPortConnection();
        }

        internal static void SetupUIButtons(Transform btnTitle)
        {
            try
            {

                // Create connect robot button by instantiating main button, changing its name, text label, and adding a new listener to handle click events
                HS2_SexRobotControllerPlugin.BtnConnectRobot = UnityEngine.Object.Instantiate(btnTitle, btnTitle.parent);
                HS2_SexRobotControllerPlugin.BtnConnectRobot.name = StringConstants.ButtonConnectRobot_Name;
                HS2_SexRobotControllerPlugin.BtnConnectRobotText = HS2_SexRobotControllerPlugin.BtnConnectRobot.GetComponentInChildren<Text>();
                HS2_SexRobotControllerPlugin.BtnConnectRobotText.text = StringConstants.ButtonConnectRobot_Text;
                Button newButton = HS2_SexRobotControllerPlugin.BtnConnectRobot.GetComponentInChildren<Button>();
                newButton.onClick = new Button.ButtonClickedEvent();
                newButton.onClick.AddListener(delegate
                {
                    if (HS2_SexRobotControllerPlugin.SerialPortConnected.Value)
                    {
                        HS2_SexRobotControllerPlugin.BtnDisconnectRobotClicked = true;
                    }
                    else
                    {
                        HS2_SexRobotControllerPlugin.BtnConnectRobotClicked = true;
                    }
                });
            }
            catch (Exception e)
            {
                HS2_SexRobotControllerPlugin.LogDebug("Error upon creating Settings Button: " + e.ToString());
            }
        }
    }
}
