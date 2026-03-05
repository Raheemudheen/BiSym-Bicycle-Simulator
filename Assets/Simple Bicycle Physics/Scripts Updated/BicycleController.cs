using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace SBPScripts
{

    [System.Serializable]
    public class CycleGeometry
    {
        public GameObject handles, lowerFork, fWheelVisual, RWheel, crank, lPedal, rPedal, fGear, rGear;
    }
    [System.Serializable]
    public class PedalAdjustments
    {
        public double crankRadius;
        public Vector3 lPedalOffset, rPedalOffset;
        public double pedalingSpeed;
    }
    [System.Serializable]
    public class WheelFrictionSettings
    {
        public PhysicMaterial fPhysicMaterial, rPhysicMaterial;
        public Vector2 fFriction, rFriction;
    }
    [System.Serializable]
    public class WayPointSystem
    {
        
        public enum RecordingState { DoNothing, Record, Playback };
        public RecordingState recordingState = RecordingState.DoNothing;
        [Range(1, 10)]
        public int frameIncrement;
        [HideInInspector]
        public List<Vector3> bicyclePositionTransform;
        [HideInInspector]
        public List<Quaternion> bicycleRotationTransform;
        [HideInInspector]
        public List<Vector2Int> movementInstructionSet;
        [HideInInspector]
        public List<bool> sprintInstructionSet;
        [HideInInspector]
        public List<int> bHopInstructionSet;
    }
    public class BicycleController : MonoBehaviour
    {
        [Header("Blync Sensors")]
        // public doubleVariable BlyncSensorSpeed;
        public double BlyncSensorSpeed;
        public double BlyncSensorSpeedAcuurate;
        public FloatVariable BlyncTurnAngle;
        [Space]
        public CycleGeometry cycleGeometry;
        public GameObject fPhysicsWheel, rPhysicsWheel;
        public WheelFrictionSettings wheelFrictionSettings;
        public double steerAngle, axisAngle, leanAngle;
        public double torque, topSpeed, reversingSpeed, speedGain;
        public GameObject spd; //custom
        public double sped; //custom
        public Vector3 COM;
        [HideInInspector]
        public bool isReversing, isAirborne;
        [Range(0, 8)]
        public double oscillationAmount;
        [Range(0, 1)]
        public double oscillationAffectSteerRatio;
        double oscillationSteerEffect;
        [HideInInspector]
        public double cycleOscillation;
        Rigidbody rb, fWheelRb, rWheelRb;
        double turnAngle;
        double xQuat, zQuat;
        [HideInInspector]
        public double crankSpeed, crankCurrentQuat, crankLastQuat, restingCrank;
        public PedalAdjustments pedalAdjustments;
        [HideInInspector]
        public double turnLeanAmount;
        RaycastHit hit;
        [HideInInspector]
        public double customSteerAxis, customLeanAxis, customAccelerationAxis, rawCustomAccelerationAxis;
        bool isRaw, sprint;
        [HideInInspector]
        public int bunnyHopInputState;
        [HideInInspector]
        public double relaxedSpeed, initialTopSpeed, pickUpSpeed;
        Quaternion initialLowerForkLocalRotaion, initialHandlesRotation;
        ConfigurableJoint fPhysicsWheelConfigJoint, rPhysicsWheelConfigJoint;
        public double steerfactor = 7;
        //Ground Conformity
        public bool groundConformity;
        RaycastHit hitGround;
        Vector3 theRay;
        double groundZ;
        JointDrive fDrive, rYDrive, rZDrive;
        public bool inelasticCollision;
        [HideInInspector]
        public Vector3 lastVelocity, deceleration, lastDeceleration;
        int impactFrames;
        bool isBunnyHopping;
        [HideInInspector]
        public double bunnyHopAmount;
        public double bunnyHopStrength;
        public WayPointSystem wayPointSystem;
        
        public BlyncControllerData sensorData; //03022023

        public ViveControllerTurnAngle viveTA;

        private void BlyncConnectionCallback(bool isConnected)
        {
            if (isConnected)
            {
                Debug.Log($"BlyncConnectionCallback Connected - {isConnected}");

                //Start game session
                sensorData.ChangeSession(true);
            }
            else
            {
                Debug.Log($"BlyncConnectionCallback Disconnected - {isConnected}");
            }
        }
        void Start()
        {
            //RegisteringBlync
          //  sensorData.RegisterBlyncConnectedListener(BlyncConnectionCallback);

            rb = GetComponent<Rigidbody>();
            rb.maxAngularVelocity = Mathf.Infinity;

            fWheelRb = fPhysicsWheel.GetComponent<Rigidbody>();
            fWheelRb.maxAngularVelocity = Mathf.Infinity;

            rWheelRb = rPhysicsWheel.GetComponent<Rigidbody>();
            rWheelRb.maxAngularVelocity = Mathf.Infinity;

            initialTopSpeed = topSpeed;
            relaxedSpeed = topSpeed / 2;

            //initialHandlesRotation = cycleGeometry.handles.transform.localRotation;
            initialLowerForkLocalRotaion = cycleGeometry.lowerFork.transform.localRotation;

            fPhysicsWheelConfigJoint = fPhysicsWheel.GetComponent<ConfigurableJoint>();
            rPhysicsWheelConfigJoint = rPhysicsWheel.GetComponent<ConfigurableJoint>();

            //Recording set to 0
            if (wayPointSystem.recordingState == WayPointSystem.RecordingState.Record || wayPointSystem.recordingState == WayPointSystem.RecordingState.DoNothing)
            {
                wayPointSystem.bicyclePositionTransform.Clear();
                wayPointSystem.bicycleRotationTransform.Clear();
                wayPointSystem.movementInstructionSet.Clear();
                wayPointSystem.sprintInstructionSet.Clear();
                wayPointSystem.bHopInstructionSet.Clear();
            }
        }


        double rTA = 0d;
        double lTA = 0d;
        double rangeTA = 0d;
        double addCorrection = 0d;
        double factorCorrection = 0d;
       


        void calcNew()
        {
            rangeTA = rTA - lTA;
            addCorrection = lTA;
            factorCorrection = Math.Abs(rangeTA / 172.5d);

        }

        public double newTA = 0d;

        double newTurnAngle()
        {
            if (Mathf.Abs(BlyncTurnAngle.value) < Math.Abs(addCorrection))
            {
                newTA = Math.Abs(addCorrection) - Math.Abs(BlyncTurnAngle.value);
            }
            else
            {
                newTA = Math.Abs(BlyncTurnAngle.value) - Math.Abs(addCorrection);
            }
            newTA = newTA / factorCorrection;
            newTA = newTA + 7.5d;

            newTA = newTA - 90d; //todo !



            //return newTA;

            if (viveTA != null)
            {
                //Debug.Log("vive is fine : " + viveTA.turnAngle);
                return viveTA.turnAngle;
            }
            else
            {
                //Debug.Log("vive null");
                return 0d;

            }
        }

        Vector3 targetRotation;

        void FixedUpdate()
        {
          // Debug.Log("turnAngle FixedUpdate " + BlyncTurnAngle.value);
          /*
            if (Input.GetKey(KeyCode.L)){
                lTA = Math.Abs(BlyncTurnAngle.value);
                Debug.Log("L Pressed " + lTA);
            }
            if (Input.GetKey(KeyCode.R))
            {
                rTA = Math.Abs(BlyncTurnAngle.value);
                Debug.Log("R Pressed " + rTA);
            }
            if (Input.GetKey(KeyCode.C)){
                calcNew();
                double newTA = newTurnAngle();
                Debug.Log($"C Pressed \nCalculated Range {rangeTA} newTA = {newTA} addCorrection {addCorrection}, factorCorrection {factorCorrection}");
            }
            if (Input.GetKey(KeyCode.D)){
                double newTA = newTurnAngle();
                Debug.Log($"D Pressed \nCalculated Range {rangeTA} newTA = {newTA} addCorrection {addCorrection}, factorCorrection {factorCorrection}");

            }*/
           // Debug.Log($"Testing!! Nothing Pressed! \nCalculated Range {rangeTA} newTA = {newTA} addCorrection {addCorrection}, factorCorrection {factorCorrection}");

            if (Input.GetKey(KeyCode.G))
            {
                sensorData.setCenterCorrection();
            }
            if (Input.GetKey(KeyCode.H))
            {
                sensorData.resetCenterCorrection();
            }

            // double newTA2 = newTurnAngle();
            // Debug.Log($"D Pressed \nCalculated Range {rangeTA} newTA = {newTA2} addCorrection {addCorrection}, factorCorrection {factorCorrection}");


            //BlyncSensorSpeedAcuurate = spd.GetComponent<FitnessEquipmentDisplay>().speed * 0.04484; //speed from wahoo //steerfactor
                                                                                                    // BlyncSensorSpeed = BlyncSensorSpeed / 4; //custom

            BlyncSensorSpeedAcuurate = spd.GetComponent<FitnessEquipmentDisplay>().speed * 0.27778f;

            BlyncSensorSpeed = /*12 * 0.04484;*/(double)BlyncSensorSpeedAcuurate;

            //BlyncSensorSpeed = 30*0.27778f;




            float t = (4.2f * Mathf.Pow((float)BlyncSensorSpeed/3.6f, 3))/ spd.GetComponent<FitnessEquipmentDisplay>().cadence;
            //torque = t;
            Debug.Log("speed now " + spd.GetComponent< FitnessEquipmentDisplay>().speed + ", speed? " + BlyncSensorSpeed + ", torque : " + t + ", cadence : " + spd.GetComponent<FitnessEquipmentDisplay>().cadence);




            if (spd == null)
            {
                Debug.LogError("spd not found");
            }
            //SteerControl
           
            steerAngle = Math.Abs(newTurnAngle()/4);
            fPhysicsWheel.transform.rotation = Quaternion.Euler((float)transform.rotation.eulerAngles.x, (float)(transform.rotation.eulerAngles.y + customSteerAxis * steerAngle + oscillationSteerEffect), 0);
            //targetRotation = transform.eulerAngles + new Vector3(steerAngle, 0f, 0f);



            //Debug.Log("fPhysicsWheel.transform.rotation"); Debug.Log(fPhysicsWheel.transform.rotation);


            fPhysicsWheelConfigJoint.axis = new Vector3(1, 0, 0);
           
            //PowerControl
            if (!sprint)
                topSpeed = Mathf.Lerp((float)topSpeed, (float)relaxedSpeed, Time.deltaTime);
            else
                topSpeed = Mathf.Lerp((float)topSpeed, (float)initialTopSpeed, Time.deltaTime);

            if (rb.velocity.magnitude < topSpeed && rawCustomAccelerationAxis > 0)
                rWheelRb.AddTorque(transform.right * (float)torque * (float)customAccelerationAxis);

            if (BlyncSensorSpeed < 0.5f) //BlyncSensorSpeed.value
                rb.drag = 10f;
            else
                rb.drag = 0.1f;

            //Body
            rb.centerOfMass = COM;
            speedGain = BlyncSensorSpeed > 0 ? BlyncSensorSpeed * 20 : 200; //BlyncSensorSpeed.value

            bool isTestModeOn = true;

            if (isTestModeOn)
            {
                Vector3 forwardV = rb.transform.forward * (float)BlyncSensorSpeed;

                rb.GetComponent<Rigidbody>().velocity = new Vector3(forwardV.x,
                                                                         rb.GetComponent<Rigidbody>().velocity.y,
                                                                         forwardV.z);

            }
            else
            {
                if (rb.velocity.magnitude < topSpeed && rawCustomAccelerationAxis > 0 && !isAirborne && !isBunnyHopping)
                    rb.AddForce(transform.forward * (float)speedGain);

                if (rb.velocity.magnitude < reversingSpeed && rawCustomAccelerationAxis < 0)
                    rb.AddForce(-transform.forward * (float)speedGain * 0.5f);

                if (transform.InverseTransformDirection(rb.velocity).z < 0)
                    isReversing = true;
                else
                    isReversing = false;

                if (rawCustomAccelerationAxis < 0 && isReversing == false)
                    rb.AddForce(-transform.forward * (float)speedGain * 2);
            }
            //Handles
            //cycleGeometry.handles.transform.localRotation = Quaternion.Euler(0, customSteerAxis * steerAngle + oscillationSteerEffect * 5, 0) * initialHandlesRotation;

            //LowerFork
            axisAngle = Math.Abs(newTurnAngle()) / factorCorrection;
            //cycleGeometry.lowerFork.transform.localRotation = Quaternion.Euler(0, customSteerAxis * steerAngle + oscillationSteerEffect * 5, customSteerAxis * -axisAngle) * initialLowerForkLocalRotaion;
            cycleGeometry.lowerFork.transform.localRotation = initialLowerForkLocalRotaion * Quaternion.Euler(0, (float)(customSteerAxis * steerAngle + oscillationSteerEffect * 5), 0);


            //FWheelVisual
            //the below code got commented the below line to try and fix the front wheel popping out issue self align
            xQuat = Math.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y));
            //the below code got commented the below line to try and fix the front wheel popping out issue
            zQuat = Math.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y));
            //the below code got the below code got commented the below line to try and fix the front wheel popping out issue
            cycleGeometry.fWheelVisual.transform.rotation = Quaternion.Euler((float)(xQuat * (customSteerAxis * -axisAngle)), (float)(customSteerAxis * steerAngle + oscillationSteerEffect * 5), (float)(zQuat * (customSteerAxis * -axisAngle)));

            //fix here for front wheel popping out issue
            //cycleGeometry.fWheelVisual.transform.GetChild(0).transform.localRotation = cycleGeometry.RWheel.transform.rotation;

            //Crank
            crankCurrentQuat = cycleGeometry.RWheel.transform.rotation.eulerAngles.x;
            if (customAccelerationAxis > 0 && !isAirborne && !isBunnyHopping)
            {
                crankSpeed += Math.Sqrt(customAccelerationAxis * Math.Abs(Mathf.DeltaAngle((float)crankCurrentQuat, (float)crankLastQuat) * pedalAdjustments.pedalingSpeed));
                crankSpeed %= 360;
            }
            else if (Math.Floor(crankSpeed) > restingCrank)
                crankSpeed += -6;
            else if (Math.Floor(crankSpeed) < restingCrank)
                crankSpeed = Mathf.Lerp((float)crankSpeed, (float)restingCrank, Time.deltaTime * 5);

            crankLastQuat = crankCurrentQuat;
            cycleGeometry.crank.transform.localRotation = Quaternion.Euler((float)crankSpeed, 0, 0);

            //Pedals
            cycleGeometry.lPedal.transform.localPosition = pedalAdjustments.lPedalOffset + new Vector3(0, Mathf.Cos(Mathf.Deg2Rad * (float)((crankSpeed + 180)) * (float)pedalAdjustments.crankRadius), Mathf.Sin(Mathf.Deg2Rad * ((float)crankSpeed + 180)) * (float)pedalAdjustments.crankRadius);
            cycleGeometry.rPedal.transform.localPosition = pedalAdjustments.rPedalOffset + new Vector3(0, Mathf.Cos(Mathf.Deg2Rad * ((float)crankSpeed)) * (float)(float)pedalAdjustments.crankRadius, Mathf.Sin(Mathf.Deg2Rad * ((float)crankSpeed)) * (float)pedalAdjustments.crankRadius);

            //FGear
            if (cycleGeometry.fGear != null)
                cycleGeometry.fGear.transform.rotation = cycleGeometry.crank.transform.rotation;
            //RGear
            if (cycleGeometry.rGear != null)
                cycleGeometry.rGear.transform.rotation = rPhysicsWheel.transform.rotation;

            //CycleOscillation
            if ((sprint && rb.velocity.magnitude > 5 && isReversing == false) || isAirborne || isBunnyHopping)
                pickUpSpeed += Time.deltaTime * 2;
            else
                pickUpSpeed -= Time.deltaTime * 2;

            pickUpSpeed = Math.Clamp(pickUpSpeed, 0.1d, 1d);

            cycleOscillation = -Math.Sin(Mathf.Deg2Rad * (crankSpeed + 90)) * (oscillationAmount * (Math.Clamp(topSpeed / rb.velocity.magnitude, 1, 1.5f))) * pickUpSpeed;
            turnLeanAmount = customLeanAxis * -leanAngle * Math.Clamp(rb.velocity.magnitude * 0.1f, 0, 1);
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, (float)(turnLeanAmount + cycleOscillation + GroundConformity(groundConformity)));
            oscillationSteerEffect = cycleOscillation * Mathf.Clamp01((float)customAccelerationAxis) * (oscillationAffectSteerRatio * (Math.Clamp(topSpeed / rb.velocity.magnitude, 1f, 1.5f)));

            //FrictionSettings
            wheelFrictionSettings.fPhysicMaterial.staticFriction = wheelFrictionSettings.fFriction.x;
            wheelFrictionSettings.fPhysicMaterial.dynamicFriction = wheelFrictionSettings.fFriction.y;
            wheelFrictionSettings.rPhysicMaterial.staticFriction = wheelFrictionSettings.rFriction.x;
            wheelFrictionSettings.rPhysicMaterial.dynamicFriction = wheelFrictionSettings.rFriction.y;

            if (Physics.Raycast(fPhysicsWheel.transform.position, Vector3.down, out hit, Mathf.Infinity))
                if (hit.distance < 0.5f)
                {
                    Vector3 velf = fPhysicsWheel.transform.InverseTransformDirection(fWheelRb.velocity);
                    velf.x *= Mathf.Clamp01(1 / (wheelFrictionSettings.fFriction.x + wheelFrictionSettings.fFriction.y));
                    fWheelRb.velocity = fPhysicsWheel.transform.TransformDirection(velf);
                }
            if (Physics.Raycast(rPhysicsWheel.transform.position, Vector3.down, out hit, Mathf.Infinity))
                if (hit.distance < 0.5f)
                {
                    Vector3 velr = rPhysicsWheel.transform.InverseTransformDirection(rWheelRb.velocity);
                    velr.x *= Mathf.Clamp01(1 / (wheelFrictionSettings.rFriction.x + wheelFrictionSettings.rFriction.y));
                    rWheelRb.velocity = rPhysicsWheel.transform.TransformDirection(velr);
                }

            //Impact sensing
            deceleration = (fWheelRb.velocity - lastVelocity) / Time.fixedDeltaTime;
            lastVelocity = fWheelRb.velocity;
            impactFrames--;
            impactFrames = Mathf.Clamp(impactFrames, 0, 15);
            if (deceleration.y > 200 && lastDeceleration.y < -1)
                impactFrames = 30;

            lastDeceleration = deceleration;

            if (impactFrames > 0 && inelasticCollision)
            {
                fWheelRb.velocity = new Vector3(fWheelRb.velocity.x, -Mathf.Abs(fWheelRb.velocity.y), fWheelRb.velocity.z);
                rWheelRb.velocity = new Vector3(rWheelRb.velocity.x, -Mathf.Abs(rWheelRb.velocity.y), rWheelRb.velocity.z);
            }

            //Make the wheels stiffer on the y axis in air
            rWheelRb.velocity = new Vector3(rWheelRb.velocity.x, rWheelRb.velocity.y * 0.95f, rWheelRb.velocity.z);

            //AirControl
            if (Physics.Raycast(transform.position + new Vector3(0, 1.2f, 1), -transform.up, out hit, Mathf.Infinity))
            {
                if (hit.distance > 1.5f || impactFrames > 0)
                {
                    isAirborne = true;
                    restingCrank = 100;
                    //Stabilize Rider in air to be parallel to the ground
                    var rot = Quaternion.FromToRotation(transform.up, Vector3.up);
                    rb.AddTorque(new Vector3(rot.x, rot.y, rot.z) * 1200);
                    rb.AddTorque(Vector3.up * (float)customSteerAxis, ForceMode.Impulse);
                }
                else if (isBunnyHopping)
                {
                    restingCrank = 100;
                }
                else
                {
                    isAirborne = false;
                    restingCrank = 10;
                }
                Debug.DrawLine(transform.position + new Vector3(0, 1.2f, 1), -transform.up);
            }


        }
        //bool turnset = false; //03022023

        int updateLoopCount = 1;
        void Update()
        {

            //To recenter
          //  if (Input.GetKey(KeyCode.C)) //03022023
           // {
            //    if (!turnset)           //03022023
            //    {
            //       turnset = true;     //03022023
            //        RecenterHandlebar();//03022023
             //   }
            //}
            //call to recenter the user's handlebar to the middle //03022023
           // void RecenterHandlebar()    //03022023
          //  {
           //     sensorData.setCenterCorrection();//03022023
         //   }
            ApplyCustomInput();

            //GetKeyUp/Down requires an Update Cycle
            //BunnyHopping
            if (bunnyHopInputState == 1)
            {
                isBunnyHopping = true;
                bunnyHopAmount += Time.deltaTime * 8f;
            }
            if (bunnyHopInputState == -1)
                StartCoroutine(DelayBunnyHop());

            if (bunnyHopInputState == -1 && !isAirborne)
                rb.AddForce(transform.up * (float)(bunnyHopAmount * bunnyHopStrength), ForceMode.VelocityChange);
            else
                bunnyHopAmount = Mathf.Lerp((float)bunnyHopAmount, 0, Time.deltaTime * 8f);

            bunnyHopAmount = Mathf.Clamp01((float)bunnyHopAmount);





        }
        double GroundConformity(bool toggle)
        {
            if (toggle)
            {
                groundZ = transform.rotation.eulerAngles.z;
            }
            return groundZ;

        }

        void ApplyCustomInput()
        {
            if (wayPointSystem.recordingState == WayPointSystem.RecordingState.DoNothing || wayPointSystem.recordingState == WayPointSystem.RecordingState.Record)
            {
                CustomInput("Horizontal", ref customSteerAxis, 5, 5, false);
                CustomInput("Vertical", ref customAccelerationAxis, 1, 1, false);
                CustomInput("Horizontal", ref customLeanAxis, 1, 1, false);
                CustomInput("Vertical", ref rawCustomAccelerationAxis, 1, 1, true);

                if (!Input.anyKey)
                {
                    customSteerAxis = Math.Clamp(newTurnAngle(), -1, 1);
                    BlyncInput(BlyncSensorSpeed, ref customAccelerationAxis, 50, 1, false); //BlyncSensorSpeed.value
                    BlyncInput(-newTurnAngle(), ref customLeanAxis, 0.2f, 1, false);
                    BlyncInput(BlyncSensorSpeed, ref rawCustomAccelerationAxis, 1, 1, true); //BlyncSensorSpeed.value
                }

                sprint = BlyncSensorSpeed > 10 ? true : false; //BlyncSensorSpeed.value

                //Stateful Input - bunny hopping
                if (Input.GetKey(KeyCode.B))
                    bunnyHopInputState = 1;
                else if (Input.GetKeyUp(KeyCode.B))
                    bunnyHopInputState = -1;
                else
                    bunnyHopInputState = 0;

                //Record
                if (wayPointSystem.recordingState == WayPointSystem.RecordingState.Record)
                {
                    if (Time.frameCount % wayPointSystem.frameIncrement == 0)
                    {
                        wayPointSystem.bicyclePositionTransform.Add(new Vector3(Mathf.Round(transform.position.x * 100f) * 0.01f, Mathf.Round(transform.position.y * 100f) * 0.01f, Mathf.Round(transform.position.z * 100f) * 0.01f));
                        wayPointSystem.bicycleRotationTransform.Add(transform.rotation);
                        wayPointSystem.movementInstructionSet.Add(new Vector2Int((int)Input.GetAxisRaw("Horizontal"), (int)Input.GetAxisRaw("Vertical")));
                        wayPointSystem.sprintInstructionSet.Add(sprint);
                        wayPointSystem.bHopInstructionSet.Add(bunnyHopInputState);
                    }
                }
            }

            else
            {
                if (wayPointSystem.recordingState == WayPointSystem.RecordingState.Playback)
                {
                    if (wayPointSystem.movementInstructionSet.Count - 1 > Time.frameCount / wayPointSystem.frameIncrement)
                    {
                        transform.position = Vector3.Lerp(transform.position, wayPointSystem.bicyclePositionTransform[Time.frameCount / wayPointSystem.frameIncrement], Time.deltaTime * wayPointSystem.frameIncrement);
                        transform.rotation = Quaternion.Lerp(transform.rotation, wayPointSystem.bicycleRotationTransform[Time.frameCount / wayPointSystem.frameIncrement], Time.deltaTime * wayPointSystem.frameIncrement);
                        WayPointInput(wayPointSystem.movementInstructionSet[Time.frameCount / wayPointSystem.frameIncrement].x, ref customSteerAxis, 5, 5, false);
                        WayPointInput(wayPointSystem.movementInstructionSet[Time.frameCount / wayPointSystem.frameIncrement].y, ref customAccelerationAxis, 1, 1, false);
                        WayPointInput(wayPointSystem.movementInstructionSet[Time.frameCount / wayPointSystem.frameIncrement].x, ref customLeanAxis, 1, 1, false);
                        WayPointInput(wayPointSystem.movementInstructionSet[Time.frameCount / wayPointSystem.frameIncrement].y, ref rawCustomAccelerationAxis, 1, 1, true);
                        sprint = wayPointSystem.sprintInstructionSet[Time.frameCount / wayPointSystem.frameIncrement];
                        bunnyHopInputState = wayPointSystem.bHopInstructionSet[Time.frameCount / wayPointSystem.frameIncrement];
                    }
                }
            }
        }

        //Input Manager Controls
        double CustomInput(string name, ref double axis, double sensitivity, double gravity, bool isRaw)
        {
            var r = Input.GetAxisRaw(name);
            var s = sensitivity;
            var g = gravity;
            var t = Time.unscaledDeltaTime;

            if (isRaw)
                axis = r;
            else
            {
                if (r != 0)
                    axis = Math.Clamp(axis + r * s * t, -1f, 1f);
                else
                    axis = Mathf.Clamp01((float)(Math.Abs(axis) - g * t)) * Math.Sign(axis);
            }

            return axis;
        }

        double WayPointInput(double instruction, ref double axis, double sensitivity, double gravity, bool isRaw)
        {
            var r = instruction;
            var s = sensitivity;
            var g = gravity;
            var t = Time.unscaledDeltaTime;

            if (isRaw)
                axis = r;
            else
            {
                if (r != 0)
                    axis = Math.Clamp(axis + r * s * t, -1f, 1f);
                else
                    axis = Mathf.Clamp01((float)(Math.Abs(axis) - g * t)) * Math.Sign(axis);
            }

            return axis;
        }

        double BlyncInput(double instruction, ref double axis, double sensitivity, double gravity, bool isRaw)
        {
            var r = instruction;
            var s = sensitivity;
            var g = gravity;
            var t = Time.unscaledDeltaTime;

            if (isRaw)
                axis = r;
            else
            {
                if (r != 0)
                    axis = Math.Clamp(axis + r * s * t, -1f, 1f);
                else
                    axis = Mathf.Clamp01((float)(Math.Abs(axis) - g * t)) * Math.Sign(axis);
            }

            return axis;
        }

        IEnumerator DelayBunnyHop()
        {
            yield return new WaitForSeconds(0.5f);
            isBunnyHopping = false;
            yield return null;
        }

    }
} 

