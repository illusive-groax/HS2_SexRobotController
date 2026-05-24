using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace HS2_SexRobotController
{
    internal sealed class SerialPortConnection
    {
        private static SerialPortConnection _instance;
        private static readonly object _lock = new object();

        internal SerialPort serialPort { get; set; }
        internal ConfigEntry<string> serialPortConfig { get; set; }
        internal ConfigEntry<bool> serialPortConnected { get; set; }
        internal ConfigEntry<string> serialPortStatus { get; set; }
        internal static readonly string[] SerialPorts = {
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "COM10",
            "COM11", "COM12", "COM13", "COM14", "COM15", "COM16", "COM17", "COM18", "COM19",
            "COM20", "COM21", "COM22", "COM23", "COM24", "COM25", "COM26", "COM27", "COM28",
            "COM29", "COM30", "COM31", "COM32", "COM33"
        };

        internal ConfigEntry<KeyboardShortcut> toggleSerialPortConnection { get; set; }
        internal ConfigEntry<KeyboardShortcut> strokeLengthMultiplierIncrease { get; set; }
        internal ConfigEntry<KeyboardShortcut> strokeLengthMultiplierDecrease { get; set; }
        internal ConfigEntry<KeyboardShortcut> togglelimitRobotStrokeLength { get; set; }

        internal ConfigEntry<float> sexRobotUpdateFrequencyConfig { get; set; }
        internal ConfigEntry<bool> diagnosticsConfig { get; set; }
        internal ConfigEntry<bool> readPositionsFromFile { get; set; }
        internal ConfigEntry<bool> printSceneName { get; set; }
        internal ConfigEntry<bool> limitRobotL0Length { get; set; }
        internal ConfigEntry<float> limitRobotL0Multiplier { get; set; }
        internal ConfigEntry<float> robotL0Multiplier { get; set; }
        internal ConfigEntry<float> robotL0MultiplierStepValue { get; set; }
        internal ConfigEntry<float> robotL0Min { get; set; }
        internal ConfigEntry<float> robotL0Max { get; set; }
        internal ConfigEntry<float> robotL1Min { get; set; }
        internal ConfigEntry<float> robotL1Max { get; set; }
        internal ConfigEntry<float> robotL2Min { get; set; }
        internal ConfigEntry<float> robotL2Max { get; set; }
        internal ConfigEntry<float> robotR0Min { get; set; }
        internal ConfigEntry<float> robotR0Max { get; set; }
        internal ConfigEntry<float> robotR1Min { get; set; }
        internal ConfigEntry<float> robotR1Max { get; set; }
        internal ConfigEntry<float> robotR2Min { get; set; }
        internal ConfigEntry<float> robotR2Max { get; set; }

        internal Text buttonConnectRobotText;
        internal Text buttonDisconnectRobotText;
        internal Text buttonStrokeMultiplierIncreaseText;
        internal Text buttonStrokeMultiplierDecreaseText;
        internal Text buttonLimitRobotStrokeLengthText;
        internal bool buttonConnectRobotClicked = false;
        internal bool buttonDisconnectRobotClicked = false;
        internal bool buttonLimitRobotStrokeLengthClicked = false;
        internal bool buttonStrokeMultiplierIncreaseClicked = false;
        internal bool buttonStrokeMultiplierDecreaseClicked = false;
        internal bool fileIsRead = false;

        internal Transform buttonConnectRobot;
        internal Transform buttonDisconnectRobot;
        internal Transform buttonStrokeMultiplierIncrease;
        internal Transform buttonStrokeMultiplierDecrease;
        internal Transform buttonLimitRobotStrokeLength;

        private SerialPortConnection()
        {
            serialPort = new SerialPort();
        }

        internal static SerialPortConnection GetInstance()
        {
            // prevent threads stumbling over the lock once the instance is ready.
            if (_instance == null)
            {
                // if just launched, lock the instance
                lock (_lock)
                {
                    // only create a new instance, if one doesn't already exist
                    if (_instance == null)
                    {
                        _instance = new SerialPortConnection();
                    }
                }
            }
            return _instance;
        }

        internal List<string> UpdateSerialPort()
        {
            List<string> logList = new List<string>();
            serialPortConnected.Value = false;
            logList.Add("Serial COM port changed to: " + serialPortConfig.Value);

            if (serialPort != null)
            {
                if (serialPort.IsOpen)
                {
                    try
                    {
                        // Close the serial port connection
                        serialPort.Close();

                        logList.Add("Serial port " + serialPort.PortName + " has been disconnected.");
                    }
                    catch (Exception e)
                    {
                        logList.Add("Error: " + e.ToString());
                    }
                }
            }

            serialPortStatus.Value = serialPortConfig.Value + " port is disconnected.";
            return logList;
        }

        internal List<string> UpdateSerialPortConnection()
        {
            List<string> logList = new List<string>();
            // Disconnect serial port if currently connected
            if (serialPort != null)
            {
                if (serialPort.IsOpen)
                {
                    try
                    {
                        // Close the serial port connection
                        serialPort.Close();

                        serialPortStatus.Value = serialPort.PortName + " port is disconnected.";

                        logList.Add("Serial port " + serialPort.PortName + " has been disconnected.");
                    }
                    catch (Exception e)
                    {
                        serialPortStatus.Value = serialPort.PortName + " port is disconnected.";

                        logList.Add("Serial port " + serialPort.PortName + " has been disconnected.");

                        logList.Add("Error: " + e.ToString());
                    }
                }
            }

            // Connect to serial port
            if (serialPortConnected.Value)
            {
                // Setup COM port based on updated config selection
                serialPort = new SerialPort("\\\\.\\" + serialPortConfig.Value, 115200);

                try
                {
                    // Open the serial port connection
                    serialPort.Open();

                    if (serialPort.IsOpen)
                    {
                        serialPortStatus.Value = "Connected to serial port " + serialPortConfig.Value + ".";

                        serialPortConnected.Value = true;

                        logList.Add("Connected to serial port " + serialPort.PortName + ".");
                    }
                    else
                    {
                        serialPortStatus.Value = "Error connecting to serial port " + serialPortConfig.Value + ".";

                        serialPortConnected.Value = false;

                        logList.Add("Error connecting to serial port " + serialPort.PortName + ".");
                    }
                }
                catch (Exception e)
                {
                    serialPortStatus.Value = "Error connecting to serial port " + serialPortConfig.Value + ".";

                    serialPortConnected.Value = false;

                    logList.Add("Error: " + e.ToString());
                }
            }
            return logList;
        }

        internal async Task UpdateConnectRobotButton()
        {
            await Task.Run(async () =>
            {
                if (serialPort.IsOpen)
                {
                    buttonConnectRobotText.text = StringConstants.ButtonConnectRobot_Connected;
                }
                else
                {
                    buttonConnectRobotText.text = StringConstants.ButtonConnectRobot_NotConnected;
                }
                await Task.Delay(1000);
                buttonConnectRobotText.text = StringConstants.ButtonConnectRobot_Text;
            });
        }

        internal async Task UpdateDisconnectRobotButton()
        {
            await Task.Run(async () =>
            {
                if (!serialPort.IsOpen)
                {
                    buttonDisconnectRobotText.text = StringConstants.ButtonDisconnectRobot_Disconnected;
                }
                else
                {
                    buttonDisconnectRobotText.text = StringConstants.ButtonDisconnectRobot_NotDisconnected;
                }
                await Task.Delay(1000);
                buttonDisconnectRobotText.text = StringConstants.ButtonDisconnectRobot_Text;
            });
        }

        internal async Task UpdateLimitRobotStrokeLengthButton()
        {
            await Task.Run(async () =>
            {
                if (!limitRobotL0Length.Value)
                {
                    limitRobotL0Length.Value = true;
                    buttonLimitRobotStrokeLengthText.text = StringConstants.ButtonStrokeLengthLimiter_Enabled;
                }
                else
                {
                    limitRobotL0Length.Value = false;
                    buttonLimitRobotStrokeLengthText.text = StringConstants.ButtonStrokeLengthLimiter_Disabled;
                }
                await Task.Delay(1000);
                buttonLimitRobotStrokeLengthText.text = StringConstants.ButtonStrokeLengthLimiter_Text;
            });
        }

        internal async Task UpdateStrokeMultiplierIncreaseButton()
        {
            await Task.Run(async () =>
            {
                buttonStrokeMultiplierIncreaseText.text = robotL0Multiplier.Value.ToString();
                await Task.Delay(1000);
                buttonStrokeMultiplierIncreaseText.text = StringConstants.ButtonIncreaseStrokeLength_Text;
            });
        }

        internal async Task UpdateStrokeMultiplierDecreaseButton()
        {
            await Task.Run(async () =>
            {
                buttonStrokeMultiplierDecreaseText.text = robotL0Multiplier.Value.ToString();
                await Task.Delay(1000);
                buttonStrokeMultiplierDecreaseText.text = StringConstants.ButtonDecreaseStrokeLength_Text;
            });
        }

        internal List<string> CheckButtonAndSerialConnState()
        {
            List<string> logList = new List<string>();

            // Check if connect robot button was clicked
            if (buttonConnectRobotClicked)
            {
                buttonConnectRobotClicked = false;

                if (serialPortConnected.Value)
                {
                    UpdateSerialPortConnection();
                }
                else
                {
                    serialPortConnected.Value = true;
                }

                Task task = UpdateConnectRobotButton();
            }

            // Check if connect robot button was clicked
            if (buttonDisconnectRobotClicked)
            {
                buttonDisconnectRobotClicked = false;

                if (!serialPortConnected.Value)
                {
                    UpdateSerialPortConnection();
                }
                else
                {
                    serialPortConnected.Value = false;
                }

                Task task = UpdateDisconnectRobotButton();
            }

            // Check if increase stroke multiplier button was clicked
            if (buttonStrokeMultiplierIncreaseClicked)
            {
                buttonStrokeMultiplierIncreaseClicked = false;

                robotL0Multiplier.Value += robotL0MultiplierStepValue.Value;

                Task task = UpdateStrokeMultiplierIncreaseButton();

                logList.Add(StringConstants.Status_CurrentStrokeMultiplierValue + robotL0Multiplier.Value);
            }

            // Check if decrease stroke multiplier button was clicked
            if (buttonStrokeMultiplierDecreaseClicked)
            {
                buttonStrokeMultiplierDecreaseClicked = false;

                robotL0Multiplier.Value -= robotL0MultiplierStepValue.Value;

                Task task = UpdateStrokeMultiplierDecreaseButton();

                logList.Add(StringConstants.Status_CurrentStrokeMultiplierValue + robotL0Multiplier.Value);
            }

            // Check if speed limit button was clicked
            if (buttonLimitRobotStrokeLengthClicked)
            {
                buttonLimitRobotStrokeLengthClicked = false;

                logList.Add(StringConstants.Status_SpeedLimited + !limitRobotL0Length.Value);

                Task task = UpdateLimitRobotStrokeLengthButton();
            }

            // Check if increase stroke multiplier hotkey was pressed
            if (strokeLengthMultiplierIncrease.Value.IsDown())
            {
                robotL0Multiplier.Value += robotL0MultiplierStepValue.Value;

                Task task = UpdateStrokeMultiplierIncreaseButton();

                logList.Add(StringConstants.Status_CurrentStrokeMultiplierValue + robotL0Multiplier.Value);
            }

            // Check if decrease stroke multiplier hotkey was pressed
            if (strokeLengthMultiplierDecrease.Value.IsDown())
            {
                robotL0Multiplier.Value -= robotL0MultiplierStepValue.Value;

                Task task = UpdateStrokeMultiplierDecreaseButton();

                logList.Add(StringConstants.Status_CurrentStrokeMultiplierValue + robotL0Multiplier.Value);
            }

            // Check if speed limiter hotkey was pressed
            if (togglelimitRobotStrokeLength.Value.IsDown())
            {

                logList.Add(StringConstants.Status_SpeedLimited + !limitRobotL0Length.Value);

                Task task = UpdateLimitRobotStrokeLengthButton();
            }

            // Check if serial port connection toggle hotkey was pressed and toggle the serial port on/off if so
            if (toggleSerialPortConnection.Value.IsDown())
            {
                bool connectSerialPort = false;

                if (serialPort != null)
                {
                    if (serialPort.IsOpen)
                    {
                        try
                        {
                            serialPortConnected.Value = false;

                            serialPortStatus.Value = serialPort.PortName + " port is disconnected.";

                            // Close the serial port connection
                            serialPort.Close();

                            Task task = UpdateDisconnectRobotButton();

                            logList.Add("Serial port " + serialPort.PortName + " has been disconnected.");
                        }
                        catch (Exception e)
                        {
                            serialPortStatus.Value = serialPort.PortName + " port is disconnected.";

                            Task task = UpdateDisconnectRobotButton();

                            logList.Add("Serial port " + serialPort.PortName + " has been disconnected.");

                            logList.Add("Error: " + e.ToString());
                        }
                    }
                    else
                    {
                        connectSerialPort = true;
                    }
                }
                else
                {
                    connectSerialPort = true;
                }

                if (connectSerialPort)
                {
                    try
                    {
                        // Setup COM port based on config selection
                        serialPort = new SerialPort("\\\\.\\" + serialPortConfig.Value, 115200);

                        // Open the serial port connection
                        serialPort.Open();

                        if (serialPort.IsOpen)
                        {
                            serialPortConnected.Value = true;

                            serialPortStatus.Value = "Connected to serial port " + serialPortConfig.Value + ".";

                            Task task = UpdateConnectRobotButton();

                            logList.Add("Connected to serial port " + serialPort.PortName + ".");
                        }
                        else
                        {
                            serialPortStatus.Value = "Error connecting to serial port " + serialPort.PortName + ".";

                            Task task = UpdateConnectRobotButton();

                            logList.Add("Error connecting to serial port " + serialPort.PortName + ".");
                        }
                    }
                    catch (Exception e)
                    {
                        serialPortStatus.Value = "Error connecting to serial port " + serialPort.PortName + ".";

                        Task task = UpdateConnectRobotButton();

                        logList.Add("Error connecting to serial port " + serialPort.PortName + ".");

                        logList.Add("Error: " + e.ToString());
                    }
                }
            }
            return logList;
        }
    }
}
