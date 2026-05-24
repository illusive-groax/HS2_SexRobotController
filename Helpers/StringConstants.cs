namespace HS2_SexRobotController
{
    internal sealed class StringConstants
    {
        /** Path to file containing the names of the animations and positions */
        internal const string ANIMATION_FILE_PATH = ".\\BepinEx\\Plugins\\HS2_SexRobotController\\sexRobotController.txt";

        /** BepinEx: Plugin Settings Menu **/
        // Multiplier settings
        internal const string SexRobotLimitsSection = "Sex Robot Limits";
        internal const string IncreaseStrokeMultiplierKey = "Increase Stroke Multiplier";
        internal const string DecreaseStrokeMultiplierKey = "Decrease Stroke Multiplier";
        internal const string RobotL0Multiplier = "Sex Robot (L0) Stroke Multiplier";
        internal const string RobotL0Multiplier_Tooltip = "Sex Robot (L0) Stroke Multiplier";
        internal const string RobotL0MultiplierStepValue = "Sex Robot (L0) Stroke Multiplier Step Value";
        internal const string RobotL0MultiplierStepValue_Tooltip = "Increase or decrease the amount of Sex Robot (L0) Stroke Multiplier steps when using Keyboard shortcuts";
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
        // Limiter settings
        internal const string SexRobotLimiterSection = "Sex Robot Secondary Limiter";
        internal const string ToggleStrokeLengthLimiter = "Enable/Disable L0 Limiter";
        internal const string StrokeLengthLimiter = "Enable Limiter for L0?";
        internal const string StrokeLengthLimiter_Tooltip = "Sets a limiter that overrides the default value. Useful when switching positions, where the default multiplier causes the stroking speed to be too fast/hard.";
        internal const string StrokeLengthLimiterMultiplierValue = "Sex Robot (L0) Stroke Limiter (Optional)";
        internal const string StrokeLengthLimiterMultiplierValue_Tooltip = "Sex Robot (L0) Stroke Multiplier to limit speed for animations where the speed is too fast.";
        // Connection settings
        internal const string SexRobotConnectionSection = "Sex Robot Connection";
        internal const string ToggleSerialPortConnection = "Connect/Disconnect Sex Robot Hotkey";
        internal const string SerialPortConfig = "Serial Port For Sex Robot";
        internal const string SerialPortConfig_Tooltip = "SerialPorts";
        internal const string SexRobotUpdateFrequencyConfig = "Sex Robot Update Frequency";
        internal const string SexRobotUpdateFrequencyConfig_Tooltip = "Sex Robot Update Frequencies";
        internal const string SerialPortStatus = "Serial Port Status Information";
        internal const string SerialPortStatus_Tooltip = "Serial Port is not connected";
        internal const string SerialPortStatus_Disconnected = " port is disconnected.";
        internal const string SerialPortConnected = "Connect via Serial Port";
        // General settings
        internal const string SexRobotGeneralSection = "General";
        internal const string BepinExDebugOutput = "BepInEx Debug: Console Output";
        internal const string BepinExPrintPosition = "BepInEx Debug: Print position (console)";
        internal const string BepinExPrintPosition_Tooltip = "Prints to console the current sex animation if not recorded in animation list";
        internal const string ReadPositionNamesFromFile = "Read positions from file";
        internal const string ReadPositionNamesFromFile_Tooltip = "Reads positions and their mapping from file instead of using the static list in this library.";

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
        // increase speed
        internal const string ButtonIncreaseStrokeLength_Name = "btnIncreaseStrokeLength";
        internal const string ButtonIncreaseStrokeLength_Text = "Stroke Multiplier +";
        // decrease speed
        internal const string ButtonDecreaseStrokeLength_Name = "btnDecreaseStrokeLength";
        internal const string ButtonDecreaseStrokeLength_Text = "Stroke Multiplier -";
        // speed limiter
        internal const string ButtonStrokeLengthLimiter_Name = "btnStrokeLengthLimiter";
        internal const string ButtonStrokeLengthLimiter_Text = "Speed limiter";
        internal const string ButtonStrokeLengthLimiter_Enabled = "Enabled";
        internal const string ButtonStrokeLengthLimiter_Disabled = "Disabled";

        /** Status messages **/
        internal const string Status_CurrentStrokeMultiplierValue = "Stroke multiplier: ";
        internal const string Status_SpeedLimited = "Speed limited: ";
    }
}
