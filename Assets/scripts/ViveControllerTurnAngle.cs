using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class ViveControllerTurnAngle : MonoBehaviour
{
    public Transform controllerTransform; // Assign this in the inspector
    private float initialYRotation;
    private bool isInitialRotationSet = false;
    public float turnAngle = 0f;

    public float lTA = 0f;
    float rTA = 0f;
    float cTA = 0f;
    float rightFactorC = 0f;
    float leftFactorC = 0f;
    float rightRange = 0f;
    float leftRange = 0f;

    bool isCalibDone = false;

    void Start()
    {
        /*
        if (controllerTransform != null)
        {
            // Store the initial Y rotation
            initialYRotation = controllerTransform.eulerAngles.z;
            isInitialRotationSet = true;
        }*/
    }


    void turnAngleCalibration()
    {        
        if (cTA >= 0f && cTA < 30f)
        {
            float extraValue = cTA;

            rightFactorC = Mathf.Abs((rTA - cTA) / 90f);
            leftFactorC = (- 1 * (360 - lTA + extraValue) / 90f);

            rightRange = Mathf.Abs(rTA - cTA);                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               
            leftRange = Mathf.Abs(360 - lTA + extraValue);

        }
        else if (cTA > 330f && cTA <= 360f)
        {
            float extraValue = 360f - cTA;

            rightFactorC = (rTA + extraValue) / 90f;
            leftFactorC = (-1 * (cTA - lTA)) / 90f;

            rightRange = Mathf.Abs(rTA + extraValue);
            leftRange = Mathf.Abs(cTA - lTA);
        }
    }

    void calculateTurnAngle(float rotationDegree)
    {
        if (cTA >= 0f && cTA < 30f)
        {
            float extraValue = cTA;

            if (rotationDegree > cTA && rotationDegree <= (rTA + 20))
            {
                turnAngle = (rotationDegree - extraValue) / rightFactorC;
            }
            else if (rotationDegree < cTA || rotationDegree >= lTA)
            {
                // if rotation > 0, cTA - rotation; if rotation <= 360 && >lta, 90 - (360 - rotation)
                if (rotationDegree > 0 && rotationDegree<=cTA)
                {
                    turnAngle = (extraValue - rotationDegree) / leftFactorC;
                }
                else if (rotationDegree <= 360 && rotationDegree>=lTA)
                {
                    turnAngle = (360 - rotationDegree + extraValue) / leftFactorC;
                }
            }
        }
        else if (cTA > 330f && cTA <= 360f)
        {
            float extraValue = 360f - cTA;
            if (rotationDegree < cTA && rotationDegree >= (lTA - 10))
            {
                turnAngle = (cTA - rotationDegree) / leftFactorC;
            }
            else if (rotationDegree > cTA || rotationDegree <= (rTA +10))
            {
                // if rotation > cta, 360 - rotation; if rotation <= 360 && >lta, 90 - (360 - rotation)
                if (rotationDegree > cTA)
                {
                    turnAngle = (rotationDegree - cTA) / rightFactorC;
                }
                else if (rotationDegree > 0)
                {
                    turnAngle = (rotationDegree + extraValue) / rightFactorC;
                }
            }
        }

        Debug.Log("Original Rotation Degree (Y-axis): " + rotationDegree);
        Debug.Log("Corrected Turn Angle (Y-axis): " + turnAngle);

    }

    void FixedUpdate()
    {

        float currentRotation = 0f;

        List<InputDevice> rightHandDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, rightHandDevices);

        foreach (var device in rightHandDevices)
        {
            if (device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation))
            {
                Vector3 degreeRotation = rotation.eulerAngles;
                float preprocessRotation = degreeRotation.y;

                Quaternion tiltRotation = Quaternion.Euler(0f, 0f, -45f);
                Quaternion controllerRotation = Quaternion.Euler(degreeRotation.x, preprocessRotation, degreeRotation.z);
                Quaternion adjustedRotation = tiltRotation * controllerRotation * Quaternion.Inverse(tiltRotation);
                currentRotation = adjustedRotation.eulerAngles.y;

                //Debug.Log("Right Vive Controller Rotation: " + preprocessRotation);
                //Debug.Log("Right Vive Controller Rotation After Processing: " + currentRotation);

                currentRotation = preprocessRotation;

            }
        }
        // Calculate the current Y rotation difference from the initial Y rotation

        // Convert to -90 to +90 range
        //turnAngle = currentYRotation;

        //Debug.Log("Turn Angle (Z-axis): " + currentYRotation);

        if (Input.GetKey(KeyCode.R))
        {
            rTA = currentRotation;
            Debug.Log("R Pressed " + rTA);
        }
        if (Input.GetKey(KeyCode.L))
        {
            lTA = currentRotation;
            Debug.Log("L Pressed " + lTA);
        }
        if (Input.GetKey(KeyCode.M))
        {
            cTA = currentRotation;
            Debug.Log("M Pressed " + cTA);
        }
        if (Input.GetKey(KeyCode.C))
        {
            turnAngleCalibration();
            calculateTurnAngle(currentRotation);
            isCalibDone = true;
            Debug.Log("C Pressed ");
        }
        if (Input.GetKey(KeyCode.D))
        {
            calculateTurnAngle(currentRotation);
            Debug.Log("D Pressed " + lTA + ", " + cTA + ", " + rTA + ", " + leftFactorC + ", " + rightFactorC + ", " + leftRange + ", " + rightRange + ", " + turnAngle + ", " + currentRotation);
        }

        //D Pressed 268.278, 1.124514, 98.93816, -1.031628, 1.086818, 92.8465, 97.81365, -0.5497865, 0.5573388
        //D Pressed 273.2797, 1.414614, 96.8205, -0.9792769, 1.060065, 88.13492, 95.40589, 1.327629, 2.821988
        //D Pressed 270.8426, 0.7659686, 97.10703, -0.9991487, 1.070456, 89.92338, 96.34106, 1.912045, 2.812729
        //D Pressed 268.9364, 0.5822886, 96.09564, -1.018288, 1.06126, 91.64589, 95.51335, 1.297329, 1.959091
        //D Pressed 268.78, 1.162209, 58.66495, -1.026469, 0.6389193, 92.38224, 57.50274, 7.692776, 6.077272

        //D Pressed 60.26297, 5.126487, 100.614, -3.387372, 1.060972, 304.8635, 95.48749, -2.551561, 356.4834

        if (Input.GetKey(KeyCode.T)) //temporary
        {
            lTA = 270.02334f;
            cTA = 1.00991884f;
            rTA = 89.525256f;
            leftFactorC = -1.01096212f;
            rightFactorC = 0.98350366f;
            leftRange = 90.986586f;
            rightRange = 88.515338f;
            turnAngle = 2.3359985f;
            currentRotation = 2.84568376f;

            turnAngleCalibration();
            calculateTurnAngle(currentRotation);
            isCalibDone = true;
        }

            if (isCalibDone)
        {
            calculateTurnAngle(currentRotation);
        }
        
    }

    void Update()
    {/*
        List<InputDevice> rightHandDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, rightHandDevices);

        foreach (var device in rightHandDevices)
        {
            if (device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation))
            {
                Vector3 degreeRotation = rotation.eulerAngles;
                Debug.Log("Right Vive Controller Rotation: " + degreeRotation.z);
            }
        }*/
        /*
         if (controllerTransform != null && isInitialRotationSet)
         {
             // Calculate the current Y rotation difference from the initial Y rotation
             float currentYRotation = controllerTransform.eulerAngles.z;
             float angleDifference = Mathf.DeltaAngle(initialYRotation, currentYRotation);

             // Convert to -90 to +90 range
             turnAngle = currentYRotation;

             Debug.Log("Turn Angle (Z-axis): " + currentYRotation);

         }*/
    }
}
