using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using System;
namespace SBPScripts
{
[System.Serializable]
public class NoiseProperties
{
    public bool useNoise;
    public float noiseRange;
    [Range(1,100)]
    public float speedScale;
}

[System.Serializable]
public class BodyDampingProperties
{
    public bool useBodyDamping;
    public float chestDampAmount, chestDampTime, hipDampAmount, hipDampTime, chestCurveIn;
    [Header("Impact Damping")]
    public float impactIntensity;
    public float impactDamping;

}

public class ProceduralIKHandler : MonoBehaviour
{
    BicycleController bicycleController;
    CyclistAnimController cyclistAnimController;
    GameObject hipIKTarget, chestIKTarget, headIKTarget;
    public Vector2 chestIKRange,hipIKRange,headIKRange;
    TwoBoneIKConstraint chestRange;
    MultiParentConstraint hipRange;
    MultiAimConstraint headRange;
    [Header("Hip IK Settings")]
    public float hipVerticalOscillation;
    public float hipMovementAggression;
    public float hipHorizontalCounterRotation;
    [Header("Chest IK Settings")]
    public float chestVerticalOscillation;
    public float chestHorizontalCounterRotation;
    public float chestMovementAggression;
    float hVOsc, hHCOsc, cHCOsc, cVOsc;
    Vector3 hipOffset, chestOffset, headOffset;
    [Range(0,1)]
    public float centered; 
    [Header("Experimental Visual Effects")]
    public NoiseProperties noiseProperties;
    float perlinNoise, animatedNoise, snapTime, randomTime;
    int returnToOrg;
    public BodyDampingProperties bodyDampingProperties;
    float yCurrentPosChest, yLastPosChest, yDampChestCurrent, yDampChest;
    float yCurrentPosHip, yLastPosHip, yDampHipCurrent, yDampHip;
    float xCycleAngle, initialHipVerticalOscillation;
    float turnAngleZ, distanceToAlignment;
    float initialChestRotationX, initialHipRotationX;
    float bunnyHopCounterWeight;
    float delayBunnyHopCounterWeight;
    Vector3 impactDirection;
    Vector3 velocity = Vector3.zero;

    void Start()
    {
        bicycleController = transform.root.GetComponent<BicycleController>();
        cyclistAnimController = transform.GetComponent<CyclistAnimController>();
        hipIKTarget = cyclistAnimController.hipIK.GetComponent<MultiParentConstraint>().data.sourceObjects[0].transform.gameObject;
        chestIKTarget = cyclistAnimController.chestIK.GetComponent<TwoBoneIKConstraint>().data.target.gameObject;
        headIKTarget = cyclistAnimController.headIK.GetComponent<MultiAimConstraint>().data.sourceObjects[0].transform.gameObject;
        chestRange = cyclistAnimController.chestIK.GetComponent<TwoBoneIKConstraint>();
        hipRange = cyclistAnimController.hipIK.GetComponent<MultiParentConstraint>();
        headRange = cyclistAnimController.headIK.GetComponent<MultiAimConstraint>();

        hipOffset = hipIKTarget.transform.localPosition;
        chestOffset = chestIKTarget.transform.localPosition;
        headOffset = headIKTarget.transform.localPosition;
        initialHipVerticalOscillation = hipVerticalOscillation;
        initialChestRotationX = chestIKTarget.transform.eulerAngles.x;
        initialHipRotationX = hipIKTarget.transform.eulerAngles.x;

    }

    // Update is called once per frame
    void Update()
    {
        //Weights
        chestRange.weight = Math.Clamp((float)bicycleController.pickUpSpeed, chestIKRange.x, chestIKRange.y);
        hipRange.weight = Math.Clamp((float)bicycleController.pickUpSpeed, hipIKRange.x, hipIKRange.y);
        headRange.weight = Math.Clamp(cyclistAnimController.speed, headIKRange.x, headIKRange.y);


        //Noise
        if (noiseProperties.useNoise)
        {
            animatedNoise = Mathf.Lerp(animatedNoise, perlinNoise, Time.deltaTime*5);
            snapTime += Time.deltaTime;
            if (snapTime > randomTime)
            {
                randomTime = UnityEngine.Random.Range(1/noiseProperties.speedScale,1.1f);
                returnToOrg++;
                if(returnToOrg%2==0)
                perlinNoise = noiseProperties.noiseRange * Mathf.PerlinNoise(Time.time * 10, 0) - (0.5f * noiseProperties.noiseRange);
                else
                perlinNoise=0;
                snapTime = 0;
            }
        }

        if (bodyDampingProperties.useBodyDamping)
        {
            //Chest Damping
            yCurrentPosChest = bicycleController.cycleGeometry.lowerFork.transform.position.y;
            yDampChestCurrent = yLastPosChest - yCurrentPosChest;
            yLastPosChest = yCurrentPosChest;
            yDampChest = Mathf.Lerp(yDampChest, yDampChestCurrent, Time.deltaTime * bodyDampingProperties.chestDampTime);
            yDampChest = Mathf.Clamp(yDampChest, -0.005f, 0.005f);

            //Hip Damping
            yCurrentPosHip = bicycleController.cycleGeometry.rGear.transform.position.y;
            yDampHipCurrent = yLastPosHip - yCurrentPosHip;
            yLastPosHip = yCurrentPosHip;
            yDampHip = Mathf.Lerp(yDampHip, yDampHipCurrent, Time.deltaTime * bodyDampingProperties.hipDampTime);
            yDampHip = Mathf.Clamp(yDampHip, -0.005f, 0.005f);

            //Impact
            impactDirection = Vector3.SmoothDamp(impactDirection, -bicycleController.deceleration*0.1f, ref velocity, bodyDampingProperties.impactDamping);

        }

        //Calculate Stationary point for Hips and Chest
        turnAngleZ = bicycleController.transform.rotation.eulerAngles.z;
        if(turnAngleZ>180)
            turnAngleZ = bicycleController.transform.eulerAngles.z - 360;
        
        distanceToAlignment = 1.2f*Mathf.Tan(Mathf.Deg2Rad*(turnAngleZ));

            //Chest Target Position
            cVOsc = Mathf.Sin(Mathf.Deg2Rad * (float)(bicycleController.crankSpeed * 2 + 90)) * (float)(bicycleController.oscillationAmount * chestVerticalOscillation);
            cHCOsc = Mathf.Sin(Mathf.Deg2Rad * ((float)bicycleController.crankSpeed + 90)) * (float)(bicycleController.oscillationAmount * chestHorizontalCounterRotation);
            chestIKTarget.transform.localPosition = new Vector3((float)(-cHCOsc * 0.001f + distanceToAlignment * centered), (float)(-cVOsc * 0.001f + yDampChest * bodyDampingProperties.chestDampAmount - bicycleController.bunnyHopAmount * 0.01f + bunnyHopCounterWeight * 0.2f), (float)(Mathf.Clamp(-yDampChest * bodyDampingProperties.chestDampAmount, 0, 1))) + chestOffset + impactDirection * bodyDampingProperties.impactIntensity * 0.05f;
            chestIKTarget.transform.rotation = Quaternion.Euler((float)(bicycleController.transform.rotation.eulerAngles.x + initialChestRotationX - yDampChest * bodyDampingProperties.chestCurveIn + bicycleController.bunnyHopAmount - bunnyHopCounterWeight * 10), (float)(bicycleController.transform.rotation.eulerAngles.y + (animatedNoise * 300 * (1.5f - bicycleController.pickUpSpeed)) + bicycleController.cycleOscillation * chestMovementAggression * -0.1f), (float)(-bicycleController.cycleOscillation + bicycleController.turnLeanAmount));

            //Hip Target Position
            hVOsc = Mathf.Sin(Mathf.Deg2Rad * ((float)bicycleController.crankSpeed * 2 + 90)) * (float)(bicycleController.oscillationAmount * hipVerticalOscillation);
            hHCOsc = Mathf.Sin(Mathf.Deg2Rad * ((float)bicycleController.crankSpeed + 90)) * (float)(bicycleController.oscillationAmount * hipHorizontalCounterRotation);
            hipIKTarget.transform.localPosition = new Vector3((float)(-hHCOsc * 0.001f + distanceToAlignment * centered), (float)(-hVOsc * 0.001f + yDampHip * bodyDampingProperties.hipDampAmount - bicycleController.bunnyHopAmount * 0.1f + bunnyHopCounterWeight * 0.1f), (float)(-bicycleController.bunnyHopAmount * 0.05f)) + hipOffset + impactDirection * bodyDampingProperties.impactIntensity * 0.1f;
            hipIKTarget.transform.rotation = Quaternion.Euler((float)(bicycleController.transform.rotation.eulerAngles.x + initialHipRotationX - bicycleController.bunnyHopAmount), (float)(bicycleController.transform.rotation.eulerAngles.y - (animatedNoise * 300 * (1.5f - bicycleController.pickUpSpeed)) - bicycleController.bunnyHopAmount), (float)(bicycleController.cycleOscillation * hipMovementAggression * 0.1f + bicycleController.turnLeanAmount));

            //Head Target Position
            headIKTarget.transform.localPosition = new Vector3((float)(bicycleController.customLeanAxis * 1.5f + animatedNoise * bicycleController.pickUpSpeed), (float)(1 - (bicycleController.pickUpSpeed * 1.5f) + animatedNoise - bicycleController.bunnyHopAmount * 0.5f + bunnyHopCounterWeight), (float)animatedNoise * 3) + headOffset + impactDirection * bodyDampingProperties.impactIntensity;

            //Additional Features
            //Hip Vertical Oscillation increases on slopes
            xCycleAngle = bicycleController.transform.eulerAngles.x;
        xCycleAngle = Mathf.Repeat(xCycleAngle + 180, 360) - 180;
        hipVerticalOscillation = initialHipVerticalOscillation - xCycleAngle;
        hipVerticalOscillation = Mathf.Clamp(hipVerticalOscillation,initialHipVerticalOscillation,initialHipVerticalOscillation*1.5f);

        //When the rider bunny hops and uses counter weight to go up and then relaxes the motion.
        if (bicycleController.bunnyHopInputState==-1 && !bicycleController.isAirborne)
            delayBunnyHopCounterWeight = 1;
        else
            delayBunnyHopCounterWeight -= Time.deltaTime * 7;
        delayBunnyHopCounterWeight = Mathf.Clamp01(delayBunnyHopCounterWeight);
        
        if(delayBunnyHopCounterWeight>0)
            bunnyHopCounterWeight += Time.deltaTime * 7;
        else
            bunnyHopCounterWeight -= Time.deltaTime * 1.2f;
        bunnyHopCounterWeight = Mathf.Clamp01(bunnyHopCounterWeight);

    }
}
}