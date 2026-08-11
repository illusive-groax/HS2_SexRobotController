namespace HS2_SexRobotController.Helpers
{
    internal sealed class StringConstants
    {
        internal const string GAME_NAME = "HoneySelect2";
        internal const string GAME_VR_NAME = "HoneySelect2VR";

        internal const string PLUGIN_VERSION = "2.3";
        internal const string PLUGIN_NAME = "HS2_SexRobotController";
        internal const string PLUGIN_GUID = "hs2robotics.HS2SexRobotController";

        internal static readonly string[] SerialPorts = [
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "COM10",
            "COM11", "COM12", "COM13", "COM14", "COM15", "COM16", "COM17", "COM18", "COM19",
            "COM20", "COM21", "COM22", "COM23", "COM24", "COM25", "COM26", "COM27", "COM28",
            "COM29", "COM30", "COM31", "COM32", "COM33"
        ];

        /** Path to file containing the names of the animations and positions */
        internal const string ANIMATION_FILE_PATH = ".\\BepinEx\\Plugins\\HS2_SexRobotController\\SexRobotController.txt";
        internal const string UNKNOWN_ANIMATIONS_FILE_PATH = ".\\BepinEx\\Plugins\\HS2_SexRobotController\\SRC_UnknownAnimations.txt";

        // the path in normal differs from VR
        internal const string ButtonPath_Settings = "btnTitle";
        internal const string ButtonPath_MainGame = "UI/ClothPanel/ClothGp/ClothAllBt/Button";
        internal const string ButtonPath_VR = "UI/Panel/ClothPanel/ClothGp/ClothAllBt/Button";

        /** BepinEx: Plugin Settings Menu **/
        // General settings
        internal const string SexRobotGeneralSection = "General";
        internal const string BepinExDebugOutput = "BepInEx Debug: Console Output";
        internal const string ReadAnimationsFromFile = "Read animations from file?";
        internal const string ReadAnimationsFromFile_Tooltip = "Reads animations and their mapping from file, allowing to include animations currently not implemented in the Plugin.";
        internal const string WriteNotFoundPositionsToFile = "Write animation names to file?";
        internal const string WriteNotFoundPositionsToFile_Tooltip = "Writes the name of the animations which currently are not available in the Plugin to a file.";
        // Connection settings
        internal const string SexRobotConnectionSection = "Serial Connection";
        internal const string SerialPortConfig = "Serial Port For Sex Robot";
        internal const string SerialPortConfig_Tooltip = "Available Serial ports";
        // Keyboard shortcuts
        internal const string SexRobotKeyboardShortcutsSection = "Keyboard shortcuts";
        internal const string ToggleSerialPortConnectionKey = "Connect/Disconnect Sex Robot";
        internal const string ToggleSerialPortConnection = "Connect/Disconnect Sex Robot Hotkey";
        // how often the physical device should be updated
        internal const string SexRobotUpdateFrequencyConfig = "Sex Robot Update Frequency";
        internal const string SexRobotUpdateFrequencyConfig_Tooltip = "Sex Robot Update Frequencies";
        internal const string SerialPortStatus = "Serial Port Status Information";
        internal const string SerialPortStatus_Tooltip = "Serial Port is not connected";
        internal const string SerialPortStatus_Disconnected = " port is disconnected.";
        internal const string SerialPortConnected = "Connect via Serial Port";
        // Multipliers for L0
        private const string RobotL0MovementMultiplier = "Movement Multiplier";
        internal const string RobotL0MovementMultiplier_Tooltip = "Sex Robot (L0) Length Multiplier: Movement range along the L0-Axis (how far up and down should the device go?).";
        private const string SexRobotL0 = "Sex Robot (L0): ";
        internal const string SexRobotL0IdleClimaxSection = SexRobotL0 + "1. Idle & Climax Multipliers";
        internal const string SexRobotL0ServiceSection = SexRobotL0 + "2. Service Multipliers";
        internal const string SexRobotL0InsertionSection = SexRobotL0 + "3. Insertion Multipliers";
        // movement
        internal const string RobotL0MovementMultiplierWeak = "Weak: " + RobotL0MovementMultiplier;
        internal const string RobotL0MovementMultiplierStrong = "Strong: " + RobotL0MovementMultiplier;
        internal const string RobotL0MovementMultiplierOrgasm = "Orgasm: " + RobotL0MovementMultiplier;
        internal const string RobotL0MovementMultiplierIdle = "Idle: " + RobotL0MovementMultiplier;
        internal const string RobotL0MovementMultiplierClimax = "Climax: " + RobotL0MovementMultiplier;
        // min/max for the different axes
        internal const string SexRobotMinMaxSection = "Sex Robot: Min/Max Values";
        internal const string RobotL0Min = "Sex Robot (L0) Up/Down Min";
        internal const string RobotL0Min_Tooltip = "Sex Robot (L0) Up/Down Min";
        internal const string RobotL0Max = "Sex Robot (L0) Up/Down Max";
        internal const string RobotL0Max_Tooltip = "Sex Robot (L0) Up/Down Max";
        internal const string RobotL1Min = "Sex Robot (L1) Forward/Backward Min";
        internal const string RobotL1Min_Tooltip = "Sex Robot (L1) Forward/Backward Min";
        internal const string RobotL1Max = "Sex Robot (L1) Forward/Backward Max";
        internal const string RobotL1Max_Tooltip = "Sex Robot (L1) Forward/Backward Max";
        internal const string RobotL2Min = "Sex Robot (L2) Left/Right Min";
        internal const string RobotL2Min_Tooltip = "Sex Robot (L2) Left/Right Min";
        internal const string RobotL2Max = "Sex Robot (L2) Left/Right Max";
        internal const string RobotL2Max_Tooltip = "Sex Robot (L2) Left/Right Max";
        internal const string RobotR0Min = "Sex Robot (R0) Twist Min";
        internal const string RobotR0Min_Tooltip = "Sex Robot (R0) Twist Min";
        internal const string RobotR0Max = "Sex Robot (R0) Twist Max";
        internal const string RobotR0Max_Tooltip = "Sex Robot (R0) Twist Max";
        internal const string RobotR1Min = "Sex Robot (R1) Roll Min";
        internal const string RobotR1Min_Tooltip = "Sex Robot (R1) Roll Min";
        internal const string RobotR1Max = "Sex Robot (R1) Roll Max";
        internal const string RobotR1Max_Tooltip = "Sex Robot (R1) Roll Max";
        internal const string RobotR2Min = "Sex Robot (R2) Pitch Min";
        internal const string RobotR2Min_Tooltip = "Sex Robot (R2) Pitch Min";
        internal const string RobotR2Max = "Sex Robot (R2) Pitch Max";
        internal const string RobotR2Max_Tooltip = "Sex Robot (R2) Pitch Max";

        /** Buttons **/
        // connect robot
        internal const string ButtonConnectRobot_Name = "btnConnectRobot";
        internal const string ButtonConnectRobot_Text = "Connect Robot";
        internal const string ButtonConnectRobot_Connected = "Connected";
        internal const string ButtonConnectRobot_NotConnected = "Can't Connect";
        // disconnect robot
        internal const string ButtonDisconnectRobot_Name = "btnDisconnectRobot";
        internal const string ButtonDisconnectRobot_Text = "Disconnect Robot";
        internal const string ButtonDisconnectRobot_Disconnected = "Disconnected";
        internal const string ButtonDisconnectRobot_NotDisconnected = "Can't Disconnect";

        /** Status messages **/
        internal const string Status_CurrentStrokeMultiplierValue = "Stroke multiplier: ";
    }
}
