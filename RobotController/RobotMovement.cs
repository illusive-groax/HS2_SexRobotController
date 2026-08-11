using AIChara;
using HS2_SexRobotController.Helpers;
using HS2_SexRobotController.Plugin;
using System;
using System.Linq;
using UnityEngine;

namespace HS2_SexRobotController.RobotController
{
    internal sealed class RobotMovement
    {
        internal const float L0_MOVEMENT_MULTIPLIER_MIN = 0.25f;
        internal const float L0_MOVEMENT_MULTIPLIER_MAX = 5.0f;

        internal int LoopType;
        internal ChaControl[] Males { get; set; }
        internal ChaControl[] Females { get; set; }
        internal bool UpdatePosition { get; set; }
        internal bool SpeedChanged { get; set; }
        internal string AnimationName { get; set; }
        internal string PrevAnimationName { get; set; }
        internal bool AnimationChanged { get; set; }
        // orgasm sets LoopType to IDLE, therefore check if orgasm for that last shake
        internal bool IsNowOrgasm { get; set; }

        // is the current animation a Service or Insertion type of animation?
        internal bool AnimationIsInsertion { get; set; }

        private float _autoRangeMin;
        private float _autoRangeMid;
        private float _autoRangeMax;
        private float _l0MovementMultiplier;

        private Transform _malePenisBase;
        private Transform _malePenisTip;
        private Transform _malePenisLeftBall;
        private Transform _malePenisRightBall;
        private Transform _femaleMouthLipsUpper;
        private Transform _femaleMouthLipsLower;
        private Transform _femaleMouthLeft;
        private Transform _femaleMouthRight;
        private Transform _femaleHip;
        private Transform _femaleVagina;
        private Transform _femaleAnus;
        private Transform _femaleMiddleBreastsLeft;
        private Transform _femaleMiddleBreastsRight;
        private Transform _femaleBreasts;
        private Transform _femaleMiddleFingerLeft;
        private Transform _femaleRingFingerLeft;
        private Transform _femaleHandLeft;
        private Transform _femaleMiddleFingerRight;
        private Transform _femaleRingFingerRight;
        private Transform _femaleHandRight;
        private Transform _femaleFootLeft;
        private Transform _femaleFootRight;
        private Transform _femaleToesLeft;
        private Transform _femaleToesRight;

        // keep track of sent commands, don't send duplicate T-Code commands
        private string _lastCommand;
        private static RobotMovement _instance;
        private static readonly object _lock = new();
        private static SerialPortConnection _serialPortConnection;
        private BoneAnimationDefiner.FemaleTargetType _currentFemaleTargetType;

        private RobotMovement()
        {
            Males = null;
            Females = null;
            AnimationName = "";
            LoopType = -1;
            SpeedChanged = false;
            UpdatePosition = false;
            _serialPortConnection = SerialPortConnection.GetInstance();
        }

        internal static RobotMovement GetInstance()
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
                        _instance = new RobotMovement();
                    }
                }
            }
            return _instance;
        }

        internal void UpdateAnimationStatus()
        {

            if (BoneAnimationDefiner.animationFemaleTargetDictionary.ContainsKey(AnimationName))
            {
                bool isIdleLoop = (LoopType == (int)BoneAnimationDefiner.LoopType.OTHER);
                if (AnimationChanged)
                {
                    GetBonePositionData();
                    _autoRangeMin = 1.0f;
                    _autoRangeMax = 0.0f;
                    // speed change is only relevant when increasing/decreasing the speed
                    SpeedChanged = false;
                }
                else if (SpeedChanged && !isIdleLoop)
                {
                    _autoRangeMin = 1.0f;
                    _autoRangeMax = 0.0f;
                    SpeedChanged = false;
                }
                try
                {
                    UpdateL0MultiplierValues();
                    UpdateRobotPosition();
                }
                catch (Exception ex)
                {
                    HS2_SexRobotControllerPlugin.LogInfo("Error occurred upon position update: " + ex.ToString());
                }
            }
            else
            {
                // not a valid position, send it home
                SendTCodeHomeCommand();
            }
        }
        internal void HSceneEnding()
        {
            // HScene has ended, ensure the device stops moving
            UpdatePosition = false;
            AnimationChanged = false;
            _l0MovementMultiplier = HS2_SexRobotControllerPlugin.RobotL0MovementMultiplier_Idle.Value;
            // send device home
            SendTCodeHomeCommand();
        }

        private void UpdateL0MultiplierValues()
        {
            // set multiplier based on the current looptype playing
            _l0MovementMultiplier = LoopType switch
            {
                (int)BoneAnimationDefiner.LoopType.WEAK_LOOP => AnimationIsInsertion ? HS2_SexRobotControllerPlugin.RobotL0MovementMultiplierPenetration_Weak.Value
                                        : HS2_SexRobotControllerPlugin.RobotL0MovementMultiplierService_Weak.Value,
                (int)BoneAnimationDefiner.LoopType.STRONG_LOOP => AnimationIsInsertion ? HS2_SexRobotControllerPlugin.RobotL0MovementMultiplierPenetration_Strong.Value
                                        : HS2_SexRobotControllerPlugin.RobotL0MovementMultiplierService_Strong.Value,
                (int)BoneAnimationDefiner.LoopType.ORGASM_LOOP => AnimationIsInsertion ? HS2_SexRobotControllerPlugin.RobotL0MovementMultiplierPenetration_Orgasm.Value
                                        : HS2_SexRobotControllerPlugin.RobotL0MovementMultiplierService_Orgasm.Value,
                _ => (IsNowOrgasm) ? HS2_SexRobotControllerPlugin.RobotL0MovementMultiplier_Climax.Value : HS2_SexRobotControllerPlugin.RobotL0MovementMultiplier_Idle.Value,
            };
        }

        private void GetBonePositionData()
        {
            int maleIndex = 0;
            int girlIndex = 0;


            if (HS2_SexRobotControllerPlugin.DiagnosticsConfig.Value)
            {
                HS2_SexRobotControllerPlugin.LogInfo("Animation: " + AnimationName);
                HS2_SexRobotControllerPlugin.LogInfo("Females found: " + Females.Length.ToString());
            }

            if (Males.Length >= 1 && Females.Length > 0)
            {
                UpdateMaleTransforms(maleIndex);

                // Lookup in the animation dictionary the female target type for this current animation
                BoneAnimationDefiner.animationFemaleTargetDictionary.TryGetValue(AnimationName, out BoneAnimationDefiner.FemaleTargetType femaleTargetTypeCurrent);
                _currentFemaleTargetType = femaleTargetTypeCurrent;

                if (_currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.VAGINAL || _currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.ANAL || _currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.ORAL
                    || _currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.BREASTS || _currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.LEFTHAND || _currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.RIGHTHAND
                    || _currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.INTERCRURAL || _currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.LEFTFOOT || _currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.RIGHTFOOT
                    || _currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.BOTH_FEET)
                {
                    UpdateFemaleTransforms(girlIndex);
                }
                else if (_currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.VAGINALSWAP || _currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.ORALSWAP
                    || _currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.BREASTSWAP || _currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.LEFTHANDSWAP || _currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.RIGHTHANDSWAP
                    || _currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.INTERCRURALSWAP || _currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.LEFTFOOTSWAP || _currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.RIGHTFOOTSWAP)
                {
                    if (Females.Length == 2)
                    {
                        girlIndex = 1;
                        UpdateFemaleTransforms(girlIndex);
                    }
                    else
                    {
                        HS2_SexRobotControllerPlugin.LogInfo("ERROR: The current HScene (swap) doesn't have 2 Females.");
                    }
                }
                if (HS2_SexRobotControllerPlugin.DiagnosticsConfig.Value)
                {
                    HS2_SexRobotControllerPlugin.LogInfo("Current animation: " + AnimationName);
                }

                AnimationChanged = false;
                UpdatePosition = true;
            }
            else
            {
                UpdatePosition = false;
                HS2_SexRobotControllerPlugin.LogInfo("ERROR: The current HScene doesn't have 1 male and at least 1 female.");
            }

        }

        private void UpdateMaleTransforms(int maleIndex)
        {
            // Find/set all the male Transforms needed for the calculations
            // Get the base of the male's penis
            _malePenisBase = Males[maleIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.PENIS_BASE]).FirstOrDefault();

            // Get the tip of the male's penis
            _malePenisTip = Males[maleIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.PENIS_TIP]).FirstOrDefault();

            // Get the male's penis left ball
            _malePenisLeftBall = Males[maleIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.BALLS_L]).FirstOrDefault();

            // Get the male's penis right ball
            _malePenisRightBall = Males[maleIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.BALLS_R]).FirstOrDefault();
        }

        private void UpdateFemaleTransforms(int girlIndex)
        {
            // Find/set all the female Transforms needed for the VAGINAL / ANAL / INTERCRURAL calculations
            // Get the base of the female's hip
            _femaleHip = Females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_HIPS]).FirstOrDefault();

            // Get the base of the female's vagina
            _femaleVagina = Females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.VAGINA]).FirstOrDefault();

            // Get the base of the female's anus
            _femaleAnus = Females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.ANUS]).FirstOrDefault();

            // Find/set all the female Transforms needed for the ORAL calculations
            // Get the female's mouth upper lips
            _femaleMouthLipsUpper = Females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_MOUTH_UPPER_LIP]).FirstOrDefault();

            // Get the female's mouth lower lips
            _femaleMouthLipsLower = Females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_MOUTH_LOWER_LIP]).FirstOrDefault();

            // Get the female's mouth left
            _femaleMouthLeft = Females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_MOUTHL]).FirstOrDefault();

            // Get the female's mouth right
            _femaleMouthRight = Females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_MOUTHR]).FirstOrDefault();

            // Find/set all the female Transforms needed for the BREASTS calculations
            // Get the female's middle of the breasts left
            _femaleMiddleBreastsLeft = Females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_BREASTL]).FirstOrDefault();

            // Get the female's middle of the breasts right
            _femaleMiddleBreastsRight = Females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_BREASTR]).FirstOrDefault();

            // Get the female's breasts center on the chest
            _femaleBreasts = Females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_BREAST]).FirstOrDefault();

            // Find/set all the female Transforms needed for the LEFTHAND / RIGHTHAND calculations
            // Get the female's left hand's middle finger
            _femaleMiddleFingerLeft = Females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_HAND_MIDDLEL]).FirstOrDefault();

            // Get the female's left hand's ring fingers
            _femaleRingFingerLeft = Females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_HAND_RINGL]).FirstOrDefault();

            // Get the female's left hand's center
            _femaleHandLeft = Females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_HANDL]).FirstOrDefault();

            // Get the female's right hand's middle finger
            _femaleMiddleFingerRight = Females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_HAND_MIDDLER]).FirstOrDefault();

            // Get the female's right hand's ring fingers
            _femaleRingFingerRight = Females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_HAND_RINGR]).FirstOrDefault();

            // Get the female's right hand's center
            _femaleHandRight = Females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_HANDR]).FirstOrDefault();

            // Find/set all the female Transforms needed for the LEFTFOOT / RIGHTFOOT / BOTH_FEET calculations
            // Get the base of the female's left foot
            _femaleFootLeft = Females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_FOOTL]).FirstOrDefault();

            // Get the base of the female's right foot
            _femaleFootRight = Females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_FOOTR]).FirstOrDefault();

            // Get the base of the female's left toes
            _femaleToesLeft = Females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_TOESL]).FirstOrDefault();

            // Get the base of the female's right toes
            _femaleToesRight = Females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_TOESR]).FirstOrDefault();
        }

        private void UpdateRobotPosition()
        {

            if (UpdatePosition)
            {
                // Setup T-code reference coordinate system
                // X(L0) is up/down in reference to the selected male's penis vector and is positive up
                // Y(L1) is toward/away orthogonal to the selected male's penis vector and is positive away
                // Z(L2) is left/right orthogonal to the selected male's penis vector and is positive left
                // RX(R0) is positive according to the right hand rule around X(L0)
                // RY(R1) is positive according to the right hand rule around Y(L1)
                // RZ(R2) is positive according to the right hand rule around Z(L2)

                // Calculate the center point between the two penis's balls
                Vector3 malePenisBallsCenterPoint = (_malePenisLeftBall.position + _malePenisRightBall.position) / 2.0f;

                // Calculate male's penis length
                float malePenisLength = Vector3.Distance(_malePenisBase.position, _malePenisTip.position);

                // Vector from the selected male's penis's base to tip
                Vector3 malePenisXAxis = _malePenisTip.position - _malePenisBase.position;

                // Use the male's penis's base and the male's penis's balls center point to establish the Z reference axis
                Vector3 malePenisZAxis = Vector3.Cross(malePenisXAxis, malePenisBallsCenterPoint - _malePenisBase.position);
                malePenisZAxis = (malePenisXAxis.magnitude / malePenisZAxis.magnitude) * malePenisZAxis;

                // Use the reference X and Z axes to establish the orthogonal Y axis
                Vector3 malePenisYAxis = Vector3.Cross(malePenisXAxis, malePenisZAxis);
                malePenisYAxis = (malePenisXAxis.magnitude / malePenisYAxis.magnitude) * malePenisYAxis;

                GetFemaleTargetPosition(out Vector3 femaleTargetXAxis, out Vector3 femaleTargetZAxis, out Vector3 femaleTargetToMalePenisBase);

                // Calculate X(L0) for robot based on the reference X axis and the vector from the female's vagina's labia trigger to the male's penis's base collider
                float robotL0 = Vector3.Dot(malePenisXAxis, femaleTargetToMalePenisBase) / (malePenisXAxis.magnitude * malePenisXAxis.magnitude);

                // Calculate Y(L1) for robot based on the reference Y axis and the vector from the female's vagina's labia trigger to the male's penis's base collider
                float robotL1 = 0.5f + Vector3.Dot(malePenisYAxis, femaleTargetToMalePenisBase) / (malePenisYAxis.magnitude * malePenisYAxis.magnitude);

                // Calculate Z(L2) for robot based on the reference Z axis and the vector from the female's vagina's labia trigger to the male's penis's base collider
                float robotL2 = 0.5f + Vector3.Dot(malePenisZAxis, femaleTargetToMalePenisBase) / (malePenisZAxis.magnitude * malePenisZAxis.magnitude);

                // Determine the coordinate system orientation between the male and female, used for calculating the R0 rotation
                bool coordinateAxesMatch = true;

                if (Vector3.Dot(malePenisZAxis, femaleTargetZAxis) < 0)
                {
                    coordinateAxesMatch = false;
                }

                // Calculate RX(R0) for robot based on the angle between reference Z axis and the female's vagina to anus vector
                float robotR0Angle = Vector3.Angle(malePenisZAxis, femaleTargetZAxis);

                // Calculate RY(R1) for robot based on the reference Z axis and the vector from the female's vagina's labia to vagina triggers
                float robotR1Angle = -(90.0f - Vector3.Angle(malePenisZAxis, femaleTargetXAxis));

                if (!coordinateAxesMatch)
                {
                    robotR0Angle = 180.0f - robotR0Angle;
                    robotR1Angle *= -1.0f;
                }

                float robotR0 = 0.5f + robotR0Angle / 180.0f;

                float robotR1 = 0.5f + robotR1Angle / 180.0f;

                // Calculate RZ(R2) for robot based on the reference Y axis and the vector from the female's vagina's labia to vagina triggers
                float robotR2Angle = -(90.0f - Vector3.Angle(malePenisYAxis, femaleTargetXAxis));

                float robotR2 = 0.5f + robotR2Angle / 180.0f;

                // Calculate automatic range values
                if (robotL0 >= 0.0f && robotL0 <= 1.0f)
                {
                    if (robotL0 < _autoRangeMin)
                    {
                        _autoRangeMin = robotL0;
                    }

                    if (robotL0 > _autoRangeMax)
                    {
                        _autoRangeMax = robotL0;
                    }
                }

                // Get the automatic range midpoint
                _autoRangeMid = (_autoRangeMin + _autoRangeMax) / 2.0f;

                float multiplier = _l0MovementMultiplier;

                // Caclulate modified robotL0
                robotL0 = 0.5f + (robotL0 - _autoRangeMid) * multiplier;

                // Formulate T-Code command string
                string command = "L0" + GenerateTCode(robotL0, HS2_SexRobotControllerPlugin.RobotL0Min.Value, HS2_SexRobotControllerPlugin.RobotL0Max.Value) + "\n";
                command += "L1" + GenerateTCode(robotL1, HS2_SexRobotControllerPlugin.RobotL1Min.Value, HS2_SexRobotControllerPlugin.RobotL1Max.Value) + "\n";
                command += "L2" + GenerateTCode(robotL2, HS2_SexRobotControllerPlugin.RobotL2Min.Value, HS2_SexRobotControllerPlugin.RobotL2Max.Value) + "\n";
                command += "R0" + GenerateTCode(robotR0, HS2_SexRobotControllerPlugin.RobotR0Min.Value, HS2_SexRobotControllerPlugin.RobotR0Max.Value) + "\n";
                command += "R1" + GenerateTCode(robotR1, HS2_SexRobotControllerPlugin.RobotR1Min.Value, HS2_SexRobotControllerPlugin.RobotR1Max.Value) + "\n";
                command += "R2" + GenerateTCode(robotR2, HS2_SexRobotControllerPlugin.RobotR2Min.Value, HS2_SexRobotControllerPlugin.RobotR2Max.Value);

                if (HS2_SexRobotControllerPlugin.DiagnosticsConfig.Value)
                {
                    HS2_SexRobotControllerPlugin.LogInfo("_malePenisBase: " + _malePenisBase.position.x.ToString() + ", " + _malePenisBase.position.y.ToString() + ", " + _malePenisBase.position.z.ToString());
                    HS2_SexRobotControllerPlugin.LogInfo("_malePenisTip: " + _malePenisTip.position.x.ToString() + ", " + _malePenisTip.position.y.ToString() + ", " + _malePenisTip.position.z.ToString());
                    HS2_SexRobotControllerPlugin.LogInfo("_malePenisLeftBall: " + _malePenisLeftBall.position.x.ToString() + ", " + _malePenisLeftBall.position.y.ToString() + ", " + _malePenisLeftBall.position.z.ToString());
                    HS2_SexRobotControllerPlugin.LogInfo("_malePenisRightBall: " + _malePenisRightBall.position.x.ToString() + ", " + _malePenisRightBall.position.y.ToString() + ", " + _malePenisRightBall.position.z.ToString());
                    HS2_SexRobotControllerPlugin.LogInfo("malePenisBallsCenterPoint: " + malePenisBallsCenterPoint.x.ToString() + ", " + malePenisBallsCenterPoint.y.ToString() + ", " + malePenisBallsCenterPoint.z.ToString());
                    HS2_SexRobotControllerPlugin.LogInfo("malePenisLength: " + malePenisLength.ToString());
                    HS2_SexRobotControllerPlugin.LogInfo("malePenisXAxis: " + malePenisXAxis.x.ToString() + ", " + malePenisXAxis.y.ToString() + ", " + malePenisXAxis.z.ToString());
                    HS2_SexRobotControllerPlugin.LogInfo("malePenisZAxis: " + malePenisZAxis.x.ToString() + ", " + malePenisZAxis.y.ToString() + ", " + malePenisZAxis.z.ToString());
                    HS2_SexRobotControllerPlugin.LogInfo("malePenisYAxis: " + malePenisYAxis.x.ToString() + ", " + malePenisYAxis.y.ToString() + ", " + malePenisYAxis.z.ToString());
                    //debugging test
                    HS2_SexRobotControllerPlugin.LogInfo("Robot L0 Multiplier (actual): " + multiplier);
                    HS2_SexRobotControllerPlugin.LogInfo("AnimationName: " + AnimationName);
                    HS2_SexRobotControllerPlugin.LogInfo("LoopType: " + LoopType);
                    HS2_SexRobotControllerPlugin.LogInfo("IsNowOrgasm: " + IsNowOrgasm);
                    HS2_SexRobotControllerPlugin.LogInfo("Robot L0: " + robotL0);
                    HS2_SexRobotControllerPlugin.LogInfo("Robot L1: " + robotL1);
                    HS2_SexRobotControllerPlugin.LogInfo("Robot L2: " + robotL2);
                    HS2_SexRobotControllerPlugin.LogInfo("Robot R0: " + robotR0);
                    HS2_SexRobotControllerPlugin.LogInfo("Robot R1: " + robotR1);
                    HS2_SexRobotControllerPlugin.LogInfo("Robot R2: " + robotR2);
                    HS2_SexRobotControllerPlugin.LogInfo("Robot R0 Angle: " + robotR0Angle);
                    HS2_SexRobotControllerPlugin.LogInfo("Robot R1 Angle: " + robotR1Angle);
                    HS2_SexRobotControllerPlugin.LogInfo("Robot R2 Angle: " + robotR2Angle);
                    HS2_SexRobotControllerPlugin.LogInfo("_autoRangeMin: " + _autoRangeMin);
                    HS2_SexRobotControllerPlugin.LogInfo("_autoRangeMid: " + _autoRangeMid);
                    HS2_SexRobotControllerPlugin.LogInfo("_autoRangeMax: " + _autoRangeMax);
                    HS2_SexRobotControllerPlugin.LogInfo("T-Code Command: \n" + command);
                }

                // Only update the sex robot's position/servos
                if (robotL0 >= 0.0f && robotL0 <= 1.0f)
                    SendTCodeCommand(command);
            }
        }

        private void GetFemaleTargetPosition(out Vector3 femaleTargetXAxis, out Vector3 femaleTargetZAxis, out Vector3 femaleTargetToMalePenisBase)
        {
            // initialize vectors
            femaleTargetToMalePenisBase = Vector3.zero;
            femaleTargetZAxis = Vector3.up;
            femaleTargetXAxis = Vector3.right;
            // targeted female body part
            Vector3 targetPos;
            // get current positions for body parts
            Vector3 malePenisBase = _malePenisBase.position;
            Vector3 femaleHip = _femaleHip.position;
            Vector3 femaleAnus = _femaleAnus.position;
            Vector3 femaleVagina = _femaleVagina.position;
            Vector3 femaleHandL = _femaleHandLeft.position;
            Vector3 femaleHandR = _femaleHandRight.position;
            Vector3 femaleFootL = _femaleFootLeft.position;
            Vector3 femaleFootR = _femaleFootRight.position;

            switch (_currentFemaleTargetType)
            {
                case BoneAnimationDefiner.FemaleTargetType.VAGINAL:
                case BoneAnimationDefiner.FemaleTargetType.VAGINALSWAP:
                    {
                        // Vector from the selected female's vagina to hip
                        femaleTargetXAxis = femaleHip - femaleVagina;
                        // Use the female's vagina and hip vector and the female's anus to establish the Z reference axis
                        femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleAnus - femaleVagina);
                        femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;
                        // set the targeted position
                        targetPos = femaleVagina;
                        break;
                    }
                case BoneAnimationDefiner.FemaleTargetType.ANAL:
                    {
                        // Vector from the selected female's anus to hip
                        femaleTargetXAxis = femaleHip - femaleAnus;
                        // Use the female's vagina and hip vector and the female's anus to establish the Z reference axis
                        femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleAnus - femaleVagina);
                        femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;
                        // set the targeted position
                        targetPos = femaleAnus;
                        break;
                    }
                case BoneAnimationDefiner.FemaleTargetType.ORAL:
                case BoneAnimationDefiner.FemaleTargetType.ORALSWAP:
                    {
                        // Calculate the center point between the two lips of the mouth
                        Vector3 femaleMouthLipsCenterPoint = (_femaleMouthLipsUpper.position + _femaleMouthLipsLower.position) / 2.0f;
                        // Calculate the center point between the left and right sides of the mouth
                        Vector3 femaleMouthCenterPoint = (_femaleMouthLeft.position + _femaleMouthRight.position) / 2.0f;
                        // Vector from the selected female's mouth lips center point to mouth center point
                        femaleTargetXAxis = femaleMouthCenterPoint - femaleMouthLipsCenterPoint;
                        // Use the female's mouth and lips center points vector and the female's mouth to establish the Y reference axis
                        Vector3 femaleTargetYAxis = Vector3.Cross(femaleTargetXAxis, _femaleMouthRight.position - femaleMouthCenterPoint);
                        femaleTargetYAxis = (femaleTargetXAxis.magnitude / femaleTargetYAxis.magnitude) * femaleTargetYAxis;
                        // Use the reference X and Y axes to establish the orthogonal Z axis
                        femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleTargetYAxis);
                        femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;
                        // set the targeted position
                        targetPos = femaleMouthCenterPoint;
                        break;
                    }
                case BoneAnimationDefiner.FemaleTargetType.BREASTS:
                case BoneAnimationDefiner.FemaleTargetType.BREASTSWAP:
                    {
                        // Calculate the center point between the two middle breasts
                        Vector3 femaleMiddleBreastsCenterPoint = (_femaleMiddleBreastsLeft.position + _femaleMiddleBreastsRight.position) / 2.0f;
                        // Vector from the selected female's middle breasts to breasts on chest
                        Vector3 femaleTargetYAxis = _femaleBreasts.position - femaleMiddleBreastsCenterPoint;
                        // Use the female's middle breasts and breasts on chest vector and the female's middle breasts right to establish the X reference axis
                        femaleTargetXAxis = Vector3.Cross(femaleTargetYAxis, _femaleMiddleBreastsRight.position - femaleMiddleBreastsCenterPoint);
                        femaleTargetXAxis = (femaleTargetYAxis.magnitude / femaleTargetXAxis.magnitude) * femaleTargetXAxis;
                        // Use the reference X and Y axes to establish the orthogonal Z axis
                        femaleTargetZAxis = Vector3.Cross(femaleTargetYAxis, femaleTargetXAxis);
                        femaleTargetZAxis = (femaleTargetYAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;
                        // set the targeted position
                        targetPos = femaleMiddleBreastsCenterPoint;
                        break;
                    }
                case BoneAnimationDefiner.FemaleTargetType.LEFTHAND:
                case BoneAnimationDefiner.FemaleTargetType.LEFTHANDSWAP:
                    {
                        // Vector from the selected female's middle and ring fingers
                        femaleTargetXAxis = _femaleMiddleFingerLeft.position - _femaleRingFingerLeft.position;
                        // Use the female's middle and ring fingers vector and the female's hand to establish the Z reference axis
                        femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleHandL - _femaleMiddleFingerLeft.position);
                        femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;
                        // set the targeted position
                        targetPos = femaleHandL;
                        break;
                    }
                case BoneAnimationDefiner.FemaleTargetType.RIGHTHAND:
                case BoneAnimationDefiner.FemaleTargetType.RIGHTHANDSWAP:
                    {
                        // Vector from the selected female's middle and ring fingers
                        femaleTargetXAxis = _femaleMiddleFingerRight.position - _femaleRingFingerRight.position;
                        // Use the female's middle and ring fingers vector and the female's hand to establish the Z reference axis
                        femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleHandR - _femaleMiddleFingerRight.position);
                        femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;
                        // set the targeted position
                        targetPos = femaleHandR;
                        break;
                    }
                case BoneAnimationDefiner.FemaleTargetType.INTERCRURAL:
                case BoneAnimationDefiner.FemaleTargetType.INTERCRURALSWAP:
                    {
                        // Vector from the selected female's vagina to anus
                        femaleTargetXAxis = femaleVagina - femaleAnus;
                        // Use the female's vagina and hip vector and the female's anus to establish the Z reference axis
                        femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleHip - femaleVagina);
                        femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;
                        // set the targeted position
                        targetPos = femaleHip;
                        break;
                    }
                case BoneAnimationDefiner.FemaleTargetType.LEFTFOOT:
                case BoneAnimationDefiner.FemaleTargetType.LEFTFOOTSWAP:
                    {
                        // There aren't singular toes, therefore only one "toe" can be tracked
                        femaleTargetXAxis = _femaleToesLeft.position;
                        // Use the female's toe and foot to establish the Z reference axis
                        femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleFootL - _femaleToesLeft.position);
                        femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;
                        // set the targeted position
                        targetPos = femaleFootL;
                        break;
                    }
                case BoneAnimationDefiner.FemaleTargetType.RIGHTFOOT:
                case BoneAnimationDefiner.FemaleTargetType.RIGHTFOOTSWAP:
                    {
                        // There aren't singular toes, therefore only one "toe" can be tracked
                        femaleTargetXAxis = _femaleToesRight.position;
                        // Use the female's toe and foot to establish the Z reference axis
                        femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleFootR - _femaleToesRight.position);
                        femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;
                        // set the targeted position
                        targetPos = femaleFootR;
                        break;
                    }
                default:
                    {
                        femaleTargetXAxis = Vector3.right;
                        femaleTargetZAxis = Vector3.up;
                        targetPos = femaleHip;
                        break;
                    }
            }
            // Vector from the targeted female body part to the male's penis's base
            femaleTargetToMalePenisBase = targetPos - malePenisBase;
        }

        private string GenerateTCode(float input, float min, float max)
        {
            // clamp input to a value between 0 and 1, then to min/max
            input = Mathf.Clamp01(input);
            input = Mathf.Clamp(input, min, max);
            int servo = Mathf.RoundToInt(input * 10000f);
            // clamp to SR6 limits
            servo = Mathf.Clamp(servo, 0, 9999);
            return servo.ToString("D4");
        }
        private void SendTCodeHomeCommand()
        {
            // if an unknown/unsupported animation is playing, then
            // instead of "locking" the device in a weird position,
            // send it "home" (Value: Mid-point (50%))
            string command = "L05000\n" +
            "L15000\n" +
            "L25000\n" +
            "R05000\n" +
            "R15000\n" +
            "R25000";
            SendTCodeCommand(command);
        }

        private void SendTCodeCommand(string command)
        {

            try
            {
                // If serial port is open then and it's not a repeated command, send the command to the robot
                if (_serialPortConnection?.SRC_SerialPort != null &&
                    _serialPortConnection.SRC_SerialPort.IsOpen &&
                    command != _lastCommand)
                {
                    _serialPortConnection.SRC_SerialPort.WriteLine(command);
                    _lastCommand = command;
                }
            }
            catch (Exception e)
            {
                HS2_SexRobotControllerPlugin.LogInfo("Error: " + e.ToString());
            }
        }
    }
}
