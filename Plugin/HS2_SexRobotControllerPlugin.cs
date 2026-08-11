using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HS2_SexRobotController.Helpers;
using HS2_SexRobotController.RobotController;
using System;
using System.Diagnostics;

namespace HS2_SexRobotController.Plugin
{
    [BepInProcess(StringConstants.GAME_NAME)]
    [BepInProcess(StringConstants.GAME_VR_NAME)]
    [BepInPlugin(StringConstants.PLUGIN_GUID, StringConstants.PLUGIN_NAME, StringConstants.PLUGIN_VERSION)]

    internal partial class HS2_SexRobotControllerPlugin : BaseUnityPlugin
    {

        internal static HScene hScene;
        private static ManualLogSource _Log;
        private RobotMovement _robotMovement;
        private SerialPortConnection _serialPortConnection;
        private readonly Stopwatch _sw = Stopwatch.StartNew();

        private void Start()
        {
            _serialPortConnection = SerialPortConnection.GetInstance();
            _robotMovement = RobotMovement.GetInstance();
            Hooks.Hooks.InstallHooks();
            Harmony.CreateAndPatchAll(typeof(HS2_SexRobotControllerPlugin));
        }

        private void Awake()
        {
            _Log = base.Logger;
            SetupPluginConfigurations();
        }

        private void OnDestroy()
        {
            _sw.Reset();
            hScene = null;
            RobotMovement.GetInstance().HSceneEnding();
        }

        internal static void LogInfo(string log)
        {
            _Log.LogInfo(log);
        }

        internal static void LogDebug(string log)
        {
            _Log.LogDebug(log);
        }

        internal static void AnimationNameCheck()
        {
            // check if animations should be read from file, 
            // then check if the current animation exists
            UpdateAnimationDictionary();
            CheckAnimationName();
            IsAnimationInsertion();
        }

        private static void UpdateAnimationDictionary()
        {
            //check if positions should be read from file
            if (HS2_SexRobotControllerPlugin.ReadAnimationsFromFile.Value
                && !HS2_SexRobotControllerPlugin.FileIsRead)
            {
                try
                {
                    // read positions from file
                    FileHandler.ReadAnimationsFromFile();
                }
                catch (Exception e)
                {
                    HS2_SexRobotControllerPlugin.LogDebug("Error updating Animation dictionary: " + e.ToString());
                }
                // regardless of result, consider file to be read (no point re-reading file with errors)
                HS2_SexRobotControllerPlugin.FileIsRead = true;
            }
            else if (!HS2_SexRobotControllerPlugin.ReadAnimationsFromFile.Value)
            {
                // if disabled, set read to false, 
                // to enable the possibility to reload file content with new animations without restarting the game
                HS2_SexRobotControllerPlugin.FileIsRead = false;
            }
        }

        private static void CheckAnimationName()
        {
            RobotMovement robotMovement = RobotMovement.GetInstance();
            // check current animation name (for finding unregistered sex-animations)
            // verify that position doesn't exist and isn't already printed
            if (robotMovement.AnimationName != robotMovement.PrevAnimationName &&
                !BoneAnimationDefiner.animationFemaleTargetDictionary.ContainsKey(robotMovement.AnimationName))
            {
                // set the currently unknown animation name
                robotMovement.PrevAnimationName = robotMovement.AnimationName;
                HS2_SexRobotControllerPlugin.LogInfo("Current Animation: " + robotMovement.PrevAnimationName);
                WriteAnimationToFile(robotMovement.PrevAnimationName);
            }
        }

        private static void IsAnimationInsertion()
        {
            RobotMovement robotMovement = RobotMovement.GetInstance();
            // check in what way the animation should be tracked
            // (if insertion/penetration, calculate L0 based on the Penis bones. If e.g. handjob, footjob, etc., then calculate the L0 based on the female target)
            BoneAnimationDefiner.animationFemaleTargetDictionary.TryGetValue(robotMovement.AnimationName, out BoneAnimationDefiner.FemaleTargetType currentFemaleTargetType);
            robotMovement.AnimationIsInsertion = currentFemaleTargetType switch
            {
                BoneAnimationDefiner.FemaleTargetType.VAGINAL
                or BoneAnimationDefiner.FemaleTargetType.VAGINALSWAP
                or BoneAnimationDefiner.FemaleTargetType.ANAL
                => true,
                _ => false,
            };
        }

        private static void WriteAnimationToFile(string animationName)
        {
            // check current animation name (for finding unregistered sex-animations)
            // verify that animation doesn't exist and isn't already printed
            if (WriteAnimationsToFile.Value)
            {
                // set previous to the current to avoid multiple rewrites on current animation refresh
                FileHandler.WriteToFile(animationName);
                LogInfo("The animation name '" + animationName + "' was written to file!");
            }
        }

        private void Update()
        {
            try
            {
                _serialPortConnection.CheckButtonAndSerialConnState();

                // Return if not in an HScene
                if (hScene == null)
                {
                    return;
                }

                // Get ms elapsed since current stopwatch interval
                float msElapsed = _sw.ElapsedMilliseconds;

                // If the ms elapsed is greater than the period based on the robot's update frequency then
                // stop the stopwatch, call the robot update function, and restart the stopwatch
                if (msElapsed >= (1000.0 / SexRobotUpdateFrequencyConfig.Value))
                {
                    _sw.Stop();

                    // check here if the speed needs to be updated, as updates only handle loops and not speed adjustment
                    if (_robotMovement.LoopType != hScene.ctrlFlag.loopType && !_robotMovement.AnimationChanged)
                    {
                        _robotMovement.SpeedChanged = true;
                        _robotMovement.LoopType = hScene.ctrlFlag.loopType;
                    }
                    _robotMovement.IsNowOrgasm = hScene.ctrlFlag.nowOrgasm;
                    _robotMovement.UpdateAnimationStatus();
                    _sw.Restart();
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug("Error in Update(): " + ex.ToString());
            }
        }
    }
}
