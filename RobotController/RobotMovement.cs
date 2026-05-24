using AIChara;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HS2_SexRobotController
{
    internal sealed class RobotMovement
    {
        private static RobotMovement _instance;
        private static readonly object _lock = new object();
        private static SerialPortConnection serialPortConnection;

        internal int loopType;
        internal const float LimitStrokeLengthMultiplier = 1.0f;
        internal string animationName { get; set; }
        internal string prevAnimationName { get; set; }
        internal bool animationChanged { get; set; }
        internal bool speedChanged { get; set; }
        internal ChaControl[] females { get; set; }
        internal ChaControl[] males { get; set; }

        private float autoRangeMin;
        private float autoRangeMid;
        private float autoRangeMax;
        internal bool updatePosition { get; set; }
        private BoneAnimationDefiner.FemaleTargetType currentFemaleTargetType;

        private Transform malePenisBase;
        private Transform malePenisTip;
        private Transform malePenisLeftBall;
        private Transform malePenisRightBall;
        private Transform femaleMouthLipsUpper;
        private Transform femaleMouthLipsLower;
        private Transform femaleMouthLeft;
        private Transform femaleMouthRight;
        private Transform femaleHip;
        private Transform femaleVagina;
        private Transform femaleAnus;
        private Transform femaleMiddleBreastsLeft;
        private Transform femaleMiddleBreastsRight;
        private Transform femaleBreasts;
        private Transform femaleMiddleFingerLeft;
        private Transform femaleRingFingerLeft;
        private Transform femaleHandLeft;
        private Transform femaleMiddleFingerRight;
        private Transform femaleRingFingerRight;
        private Transform femaleHandRight;
        private Transform femaleFootLeft;
        private Transform femaleFootRight;
        private Transform femaleToesLeft;
        private Transform femaleToesRight;

        private RobotMovement()
        {
            males = null;
            females = null;
            loopType = -1;
            animationName = "";
            speedChanged = false;
            updatePosition = false;
            serialPortConnection = SerialPortConnection.GetInstance();
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

        internal List<string> updateAnimationStatus()
        {
            List<string> logList = new List<string>();
            if (BoneAnimationDefiner.animationFemaleTargetDictionary.ContainsKey(animationName))
            {
                if (animationChanged)
                {
                    logList.AddRange(getBonePositionData());
                    autoRangeMin = 1.0f;
                    autoRangeMax = 0.0f;
                    // speed change is only relevant when increasing/decreasing the speed
                    speedChanged = false;
                }
                else if (speedChanged)
                {
                    autoRangeMin = 1.0f;
                    autoRangeMax = 0.0f;
                    speedChanged = false;
                }
                try
                {
                    logList.AddRange(updateRobotPosition());
                }
                catch (Exception ex)
                {
                    logList.Add("Error occurred upon position update: " + ex.ToString());
                }
            }
            return logList;
        }

        private List<string> getBonePositionData()
        {
            int maleIndex = 0;
            int girlIndex = 0;
            List<string> logList = new List<string>();

            if (serialPortConnection.diagnosticsConfig.Value)
            {
                logList.Add("Animation: " + animationName);
                logList.Add("females found: " + females.Length.ToString());
            }

            if (males.Length >= 1 && females.Length > 0)
            {
                updateMaleTransforms(maleIndex);

                // Lookup in the animation dictionary the female target type for this current animation
                BoneAnimationDefiner.FemaleTargetType femaleTargetTypeCurrent;
                BoneAnimationDefiner.animationFemaleTargetDictionary.TryGetValue(animationName, out femaleTargetTypeCurrent);
                currentFemaleTargetType = femaleTargetTypeCurrent;

                if (currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.VAGINAL || currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.ANAL || currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.ORAL
                    || currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.BREASTS || currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.LEFTHAND || currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.RIGHTHAND
                    || currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.INTERCRURAL || currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.LEFTFOOT || currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.RIGHTFOOT
                    || currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.BOTH_FEET)
                {
                    updateFemaleTransforms(girlIndex);
                }
                else if (currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.VAGINALSWAP || currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.ORALSWAP
                    || currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.BREASTSWAP || currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.LEFTHANDSWAP || currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.RIGHTHANDSWAP
                    || currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.INTERCRURALSWAP || currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.LEFTFOOTSWAP || currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.RIGHTFOOTSWAP)
                {
                    if (females.Length == 2)
                    {
                        girlIndex = 1;
                        updateFemaleTransforms(girlIndex);
                    }
                    else
                    {
                        logList.Add("ERROR: The current HScene (swap) doesn't have 2 females.");
                    }
                }
                if (serialPortConnection.diagnosticsConfig.Value)
                {
                    logList.Add("Current animation: " + animationName);
                }

                animationChanged = false;
                updatePosition = true;
            }
            else
            {
                updatePosition = false;
                logList.Add("ERROR: The current HScene doesn't have 1 male and at least 1 female.");
            }
            return logList;
        }

        private void updateMaleTransforms(int maleIndex)
        {
            // Find/set all the male Transforms needed for the calculations here so it doesn't have to happen each Update()
            // Get the base of the male's penis
            malePenisBase = males[maleIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.PENIS_BASE]).FirstOrDefault();

            // Get the tip of the male's penis
            malePenisTip = males[maleIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.PENIS_TIP]).FirstOrDefault();

            // Get the male's penis left ball
            malePenisLeftBall = males[maleIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.BALLS_L]).FirstOrDefault();

            // Get the male's penis right ball
            malePenisRightBall = males[maleIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.BALLS_R]).FirstOrDefault();
        }

        private void updateFemaleTransforms(int girlIndex)
        {
            // Find/set all the female Transforms needed for the VAGINAL / ANAL / INTERCRURAL calculations
            // Get the base of the female's hip
            femaleHip = females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_HIPS]).FirstOrDefault();

            // Get the base of the female's vagina
            femaleVagina = females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.VAGINA]).FirstOrDefault();

            // Get the base of the female's anus
            femaleAnus = females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.ANUS]).FirstOrDefault();

            // Find/set all the female Transforms needed for the ORAL calculations
            // Get the female's mouth upper lips
            femaleMouthLipsUpper = females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_MOUTH_UPPER_LIP]).FirstOrDefault();

            // Get the female's mouth lower lips
            femaleMouthLipsLower = females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_MOUTH_LOWER_LIP]).FirstOrDefault();

            // Get the female's mouth left
            femaleMouthLeft = females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_MOUTHL]).FirstOrDefault();

            // Get the female's mouth right
            femaleMouthRight = females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_MOUTHR]).FirstOrDefault();

            // Find/set all the female Transforms needed for the BREASTS calculations
            // Get the female's middle of the breasts left
            femaleMiddleBreastsLeft = females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_BREASTL]).FirstOrDefault();

            // Get the female's middle of the breasts right
            femaleMiddleBreastsRight = females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_BREASTR]).FirstOrDefault();

            // Get the female's breasts center on the chest
            femaleBreasts = females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_BREAST]).FirstOrDefault();

            // Find/set all the female Transforms needed for the LEFTHAND / RIGHTHAND calculations
            // Get the female's left hand's middle finger
            femaleMiddleFingerLeft = females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_HAND_MIDDLEL]).FirstOrDefault();

            // Get the female's left hand's ring fingers
            femaleRingFingerLeft = females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_HAND_RINGL]).FirstOrDefault();

            // Get the female's left hand's center
            femaleHandLeft = females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_HANDL]).FirstOrDefault();

            // Get the female's right hand's middle finger
            femaleMiddleFingerRight = females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_HAND_MIDDLER]).FirstOrDefault();

            // Get the female's right hand's ring fingers
            femaleRingFingerRight = females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_HAND_RINGR]).FirstOrDefault();

            // Get the female's right hand's center
            femaleHandRight = females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_HANDR]).FirstOrDefault();

            // Find/set all the female Transforms needed for the LEFTFOOT / RIGHTFOOT / BOTH_FEET calculations
            // Get the base of the female's left foot
            femaleFootLeft = females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_FOOTL]).FirstOrDefault();

            // Get the base of the female's right foot
            femaleFootRight = females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_FOOTR]).FirstOrDefault();

            // Get the base of the female's left toes
            femaleToesLeft = females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_TOESL]).FirstOrDefault();

            // Get the base of the female's right toes
            femaleToesRight = females[girlIndex].GetComponentsInChildren<Transform>().Where(x => x.name == BoneAnimationDefiner.bodyBoneDictionary[BoneAnimationDefiner.BodyBone.FEMALE_TOESR]).FirstOrDefault();
        }

        private List<string> updateRobotPosition()
        {
            List<string> logList = new List<string>();
            if (updatePosition)
            {
                // Setup T-code reference coordinate system
                // X(L0) is up/down in reference to the selected male's penis vector and is positive up
                // Y(L1) is toward/away orthogonal to the selected male's penis vector and is positive away
                // Z(L2) is left/right orthogonal to the selected male's penis vector and is positive left
                // RX(R0) is positive according to the right hand rule around X(L0)
                // RY(R1) is positive according to the right hand rule around Y(L1)
                // RZ(R2) is positive according to the right hand rule around Z(L2)

                // Calculate the center point between the two penis's balls
                Vector3 malePenisBallsCenterPoint = (malePenisLeftBall.position + malePenisRightBall.position) / 2.0f;

                // Calculate male's penis length
                float malePenisLength = Vector3.Distance(malePenisBase.position, malePenisTip.position);

                // Vector from the selected male's penis's base to tip
                Vector3 malePenisXAxis = malePenisTip.position - malePenisBase.position;

                // Use the male's penis's base and the male's penis's balls center point to establish the Z reference axis
                Vector3 malePenisZAxis = Vector3.Cross(malePenisXAxis, malePenisBallsCenterPoint - malePenisBase.position);
                malePenisZAxis = (malePenisXAxis.magnitude / malePenisZAxis.magnitude) * malePenisZAxis;

                // Use the reference X and Z axes to establish the orthogonal Y axis
                Vector3 malePenisYAxis = Vector3.Cross(malePenisXAxis, malePenisZAxis);
                malePenisYAxis = (malePenisXAxis.magnitude / malePenisYAxis.magnitude) * malePenisYAxis;

                Vector3 femaleTargetXAxis = new Vector3(0.0f, 0.0f, 0.0f);
                Vector3 femaleTargetZAxis = new Vector3(0.0f, 0.0f, 0.0f);
                Vector3 femaleTargetYAxis = new Vector3(0.0f, 0.0f, 0.0f);
                Vector3 femaleTargetToMalePenisBase = new Vector3(0.0f, 0.0f, 0.0f);

                if (currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.VAGINAL || currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.VAGINALSWAP)
                {
                    // Vector from the selected female's vagina to hip
                    femaleTargetXAxis = femaleHip.position - femaleVagina.position;

                    // Use the female's vagina and hip vector and the female's anus to establish the Z reference axis
                    femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleAnus.position - femaleVagina.position);
                    femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;

                    // Use the reference X and Z axes to establish the orthogonal Y axis
                    femaleTargetYAxis = Vector3.Cross(femaleTargetXAxis, femaleTargetZAxis);
                    femaleTargetYAxis = (femaleTargetXAxis.magnitude / femaleTargetYAxis.magnitude) * femaleTargetYAxis;

                    // Vector from the female's vagina to the male's penis's base
                    femaleTargetToMalePenisBase = femaleVagina.position - malePenisBase.position;
                }
                else if (currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.ANAL)
                {
                    // Vector from the selected female's anus to hip
                    femaleTargetXAxis = femaleHip.position - femaleAnus.position;

                    // Use the female's vagina and hip vector and the female's anus to establish the Z reference axis
                    femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleAnus.position - femaleVagina.position);
                    femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;

                    // Use the reference X and Z axes to establish the orthogonal Y axis
                    femaleTargetYAxis = Vector3.Cross(femaleTargetXAxis, femaleTargetZAxis);
                    femaleTargetYAxis = (femaleTargetXAxis.magnitude / femaleTargetYAxis.magnitude) * femaleTargetYAxis;

                    // Vector from the female's vagina to the male's penis's base
                    femaleTargetToMalePenisBase = femaleAnus.position - malePenisBase.position;
                }
                else if (currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.ORAL || currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.ORALSWAP)
                {
                    // Calculate the center point between the two lips of the mouth
                    Vector3 femaleMouthLipsCenterPoint = (femaleMouthLipsUpper.position + femaleMouthLipsLower.position) / 2.0f;

                    // Calculate the center point between the left and right sides of the mouth
                    Vector3 femaleMouthCenterPoint = (femaleMouthLeft.position + femaleMouthRight.position) / 2.0f;

                    // Vector from the selected female's mouth lips center point to mouth center point
                    femaleTargetXAxis = femaleMouthCenterPoint - femaleMouthLipsCenterPoint;

                    // Use the female's mouth and lips center points vector and the female's mouth to establish the Y reference axis
                    femaleTargetYAxis = Vector3.Cross(femaleTargetXAxis, femaleMouthRight.position - femaleMouthCenterPoint);
                    femaleTargetYAxis = (femaleTargetXAxis.magnitude / femaleTargetYAxis.magnitude) * femaleTargetYAxis;

                    // Use the reference X and Y axes to establish the orthogonal Z axis
                    femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleTargetYAxis);
                    femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;

                    // Vector from the female's mouth center point to the male's penis's base
                    femaleTargetToMalePenisBase = femaleMouthCenterPoint - malePenisBase.position;
                }
                else if (currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.BREASTS)
                {
                    // Calculate the center point between the two middle breasts
                    Vector3 femaleMiddleBreastsCenterPoint = (femaleMiddleBreastsLeft.position + femaleMiddleBreastsRight.position) / 2.0f;

                    // Vector from the selected female's middle breasts to breasts on chest
                    femaleTargetYAxis = femaleBreasts.position - femaleMiddleBreastsCenterPoint;

                    // Use the female's middle breasts and breasts on chest vector and the female's middle breasts right to establish the X reference axis
                    femaleTargetXAxis = Vector3.Cross(femaleTargetYAxis, femaleMiddleBreastsRight.position - femaleMiddleBreastsCenterPoint);
                    femaleTargetXAxis = (femaleTargetYAxis.magnitude / femaleTargetXAxis.magnitude) * femaleTargetXAxis;

                    // Use the reference X and Y axes to establish the orthogonal Z axis
                    femaleTargetZAxis = Vector3.Cross(femaleTargetYAxis, femaleTargetXAxis);
                    femaleTargetZAxis = (femaleTargetYAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;

                    // Vector from the female's breasts center point to the male's penis's base
                    femaleTargetToMalePenisBase = femaleMiddleBreastsCenterPoint - malePenisBase.position;
                }
                else if (currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.LEFTHAND)
                {
                    // Vector from the selected female's middle and ring fingers
                    femaleTargetXAxis = femaleMiddleFingerLeft.position - femaleRingFingerLeft.position;

                    // Use the female's middle and ring fingers vector and the female's hand to establish the Y reference axis
                    femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleHandLeft.position - femaleMiddleFingerLeft.position);
                    femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;

                    // Use the reference X and Y axes to establish the orthogonal Z axis
                    femaleTargetYAxis = Vector3.Cross(femaleTargetXAxis, femaleTargetZAxis);
                    femaleTargetYAxis = (femaleTargetXAxis.magnitude / femaleTargetYAxis.magnitude) * femaleTargetYAxis;

                    // Vector from the female's hand to the male's penis's base
                    femaleTargetToMalePenisBase = femaleHandLeft.position - malePenisBase.position;
                }
                else if (currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.RIGHTHAND || currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.RIGHTHANDSWAP)
                {
                    // Vector from the selected female's middle and ring fingers
                    femaleTargetXAxis = femaleMiddleFingerRight.position - femaleRingFingerRight.position;

                    // Use the female's middle and ring fingers vector and the female's hand to establish the Y reference axis
                    femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleHandRight.position - femaleMiddleFingerRight.position);
                    femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;

                    // Use the reference X and Y axes to establish the orthogonal Z axis
                    femaleTargetYAxis = Vector3.Cross(femaleTargetXAxis, femaleTargetZAxis);
                    femaleTargetYAxis = (femaleTargetXAxis.magnitude / femaleTargetYAxis.magnitude) * femaleTargetYAxis;

                    // Vector from the female's hand to the male's penis's base
                    femaleTargetToMalePenisBase = femaleHandRight.position - malePenisBase.position;
                }
                else if (currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.LEFTFOOT)
                {
                    //DUMMY Code, missing key/value for toes to properly map this
                    femaleTargetXAxis = femaleToesLeft.position; // femaleMiddleFingerLeft.position - femaleRingFingerLeft.position;

                    // Use the female's middle and ring fingers vector and the female's hand to establish the Y reference axis
                    femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleFootLeft.position - femaleToesLeft.position);
                    femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;

                    // Use the reference X and Y axes to establish the orthogonal Z axis
                    femaleTargetYAxis = Vector3.Cross(femaleTargetXAxis, femaleTargetZAxis);
                    femaleTargetYAxis = (femaleTargetXAxis.magnitude / femaleTargetYAxis.magnitude) * femaleTargetYAxis;

                    // Vector from the female's hand to the male's penis's base
                    femaleTargetToMalePenisBase = femaleFootLeft.position - malePenisBase.position;
                }
                else if (currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.RIGHTFOOT)
                {
                    //DUMMY Code, missing key/value for toes to properly map this
                    femaleTargetXAxis = femaleToesRight.position; // femaleMiddleFingerLeft.position - femaleRingFingerLeft.position;

                    // Use the female's middle and ring fingers vector and the female's hand to establish the Y reference axis
                    femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleFootRight.position - femaleToesRight.position);
                    femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;

                    // Use the reference X and Y axes to establish the orthogonal Z axis
                    femaleTargetYAxis = Vector3.Cross(femaleTargetXAxis, femaleTargetZAxis);
                    femaleTargetYAxis = (femaleTargetXAxis.magnitude / femaleTargetYAxis.magnitude) * femaleTargetYAxis;

                    // Vector from the female's hand to the male's penis's base
                    femaleTargetToMalePenisBase = femaleFootRight.position - malePenisBase.position;
                }
                else if (currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.INTERCRURAL || currentFemaleTargetType == BoneAnimationDefiner.FemaleTargetType.INTERCRURALSWAP)
                {
                    // Vector from the selected female's vagina to anus
                    femaleTargetXAxis = femaleVagina.position - femaleAnus.position;

                    // Use the female's vagina and hip vector and the female's anus to establish the Z reference axis
                    femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleHip.position - femaleVagina.position);
                    femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;

                    // Use the reference X and Z axes to establish the orthogonal Y axis
                    femaleTargetYAxis = Vector3.Cross(femaleTargetXAxis, femaleTargetZAxis);
                    femaleTargetYAxis = (femaleTargetXAxis.magnitude / femaleTargetYAxis.magnitude) * femaleTargetYAxis;

                    // Vector from the female's vagina to the male's penis's base
                    femaleTargetToMalePenisBase = femaleVagina.position - malePenisBase.position;
                }

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
                    if (robotL0 < autoRangeMin)
                    {
                        autoRangeMin = robotL0;
                    }

                    if (robotL0 > autoRangeMax)
                    {
                        autoRangeMax = robotL0;
                    }
                }

                // Get the automatic range midpoint
                autoRangeMid = (autoRangeMin + autoRangeMax) / 2.0f;

                float multiplier = getAnimationMultiplier();

                // Caclulate modified robotL0
                robotL0 = 0.5f + (robotL0 - autoRangeMid) * multiplier;

                // Formulate T-Code command string
                string command = "L0" + GenerateTCode(robotL0, serialPortConnection.robotL0Min.Value, serialPortConnection.robotL0Max.Value) + "\n";
                command += "L1" + GenerateTCode(robotL1, serialPortConnection.robotL1Min.Value, serialPortConnection.robotL1Max.Value) + "\n";
                command += "L2" + GenerateTCode(robotL2, serialPortConnection.robotL2Min.Value, serialPortConnection.robotL2Max.Value) + "\n";
                command += "R0" + GenerateTCode(robotR0, serialPortConnection.robotR0Min.Value, serialPortConnection.robotR0Max.Value) + "\n";
                command += "R1" + GenerateTCode(robotR1, serialPortConnection.robotR1Min.Value, serialPortConnection.robotR1Max.Value) + "\n";
                command += "R2" + GenerateTCode(robotR2, serialPortConnection.robotR2Min.Value, serialPortConnection.robotR2Max.Value);

                if (serialPortConnection.diagnosticsConfig.Value)
                {
                    logList.Add("malePenisBase: " + malePenisBase.position.x.ToString() + ", " + malePenisBase.position.y.ToString() + ", " + malePenisBase.position.z.ToString());
                    logList.Add("malePenisTip: " + malePenisTip.position.x.ToString() + ", " + malePenisTip.position.y.ToString() + ", " + malePenisTip.position.z.ToString());
                    logList.Add("malePenisLeftBall: " + malePenisLeftBall.position.x.ToString() + ", " + malePenisLeftBall.position.y.ToString() + ", " + malePenisLeftBall.position.z.ToString());
                    logList.Add("malePenisRightBall: " + malePenisRightBall.position.x.ToString() + ", " + malePenisRightBall.position.y.ToString() + ", " + malePenisRightBall.position.z.ToString());
                    logList.Add("malePenisBallsCenterPoint: " + malePenisBallsCenterPoint.x.ToString() + ", " + malePenisBallsCenterPoint.y.ToString() + ", " + malePenisBallsCenterPoint.z.ToString());
                    logList.Add("malePenisLength: " + malePenisLength.ToString());
                    logList.Add("malePenisXAxis: " + malePenisXAxis.x.ToString() + ", " + malePenisXAxis.y.ToString() + ", " + malePenisXAxis.z.ToString());
                    logList.Add("malePenisZAxis: " + malePenisZAxis.x.ToString() + ", " + malePenisZAxis.y.ToString() + ", " + malePenisZAxis.z.ToString());
                    logList.Add("malePenisYAxis: " + malePenisYAxis.x.ToString() + ", " + malePenisYAxis.y.ToString() + ", " + malePenisYAxis.z.ToString());
                    logList.Add("Robot L0 Multiplier: " + serialPortConnection.robotL0Multiplier.Value);
                    //debugging test
                    logList.Add("Robot L0 Multiplier (actual): " + multiplier);
                    logList.Add("animationName: " + animationName);
                    logList.Add("loopType: " + loopType);
                    logList.Add("Robot L0: " + robotL0);
                    logList.Add("Robot L1: " + robotL1);
                    logList.Add("Robot L2: " + robotL2);
                    logList.Add("Robot R0: " + robotR0);
                    logList.Add("Robot R1: " + robotR1);
                    logList.Add("Robot R2: " + robotR2);
                    logList.Add("Robot R0 Angle: " + robotR0Angle);
                    logList.Add("Robot R1 Angle: " + robotR1Angle);
                    logList.Add("Robot R2 Angle: " + robotR2Angle);
                    logList.Add("T-Code Command: \n" + command);
                    logList.Add("autoRangeMin: " + autoRangeMin);
                    logList.Add("autoRangeMid: " + autoRangeMid);
                    logList.Add("autoRangeMax: " + autoRangeMax);
                    logList.Add("robotL0 percent: " + (((robotL0 - autoRangeMin) / (autoRangeMax - autoRangeMin)) * 100.0f));
                }

                // Only update the sex robot's position/servos
                if (robotL0 >= 0.0f && robotL0 <= 1.0f)
                {
                    try
                    {
                        if (serialPortConnection.serialPort != null)
                        {
                            // If serial port is open then send the command to the robot
                            if (serialPortConnection.serialPort.IsOpen)
                            {
                                serialPortConnection.serialPort.WriteLine(command);

                                if (serialPortConnection.diagnosticsConfig.Value)
                                {
                                    logList.Add("Command value sent: " + command);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logList.Add("Error: " + e.ToString());
                    }
                }
            }
            return logList;
        }

        private float getAnimationMultiplier()
        {
            // set default value for multiplier
            float multiplier = (float)serialPortConnection.robotL0Multiplier.DefaultValue;
            bool isSlowLoop = (loopType == (int)BoneAnimationDefiner.LoopType.WEAK_LOOP);

            // check if normal or limited L0 stroke length should be used
            if (!serialPortConnection.limitRobotL0Length.Value)
            {
                multiplier = serialPortConnection.robotL0Multiplier.Value;
            }
            else
            {
                multiplier = serialPortConnection.limitRobotL0Multiplier.Value;
            }
            return multiplier;
        }

        private string GenerateTCode(float input, float min, float max)
        {
            if (input > max) input = max;
            if (input < min) input = min;

            input = input * 1000;

            string output;

            if (input >= 999f)
            {
                output = "999";
            }
            else if (input >= 1f)
            {
                output = input.ToString("000");
            }
            else
            {
                output = "000";
            }

            return output;
        }
    }
}
