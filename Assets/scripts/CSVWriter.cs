using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Threading.Tasks;
using SBPScripts;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System.Diagnostics;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEditor;

public class CSVWriter : MonoBehaviour
{
    BicycleController bicycleController;

    string filename = "";

    public ViveControllerTurnAngle viveTA;

    public GameObject bikeback;
    //public FloatVariable BlyncTurnAngle;
    public FloatVariable newTA;
    public BlyncControllerData sensorData;

    public float initialPositionLong = 0.0f;
    public float initialPositionLat = 0.0f;

    public static CSVWriter Instance;
    public int CurrentSection { get; private set; } = 0;

    public float t1ControlledDistance1DecimalMax = 3f;//hard set spawn150 here. max 1 decimal only.
    public float t2ControlledDistance1DecimalMax = 3f;//hard set spawn350 here. max 1 decimal only 
    public float t3ControlledDistance1DecimalMax = 3f;//hard set spawn550 here. max 1 decimal only 
    public bool isBikeFrameReference = false;//1 decimal only 

    public string time;
    public int hour;
    public int minutes;
    public int seconds;
    public int milliseconds;
    public string sceneName = "";
    [System.Serializable]
    public class Player
    {
        public float timestamp;
        public float xcord;
        public float ycord;
    }
    [System.Serializable]
    public class List
    {
        public Player[] player;
    }

    //public float BlyncSensorSpeed = 0.0f;
    public GameObject spd; //custom

    public long timePoint = 0;
    public long totalTime = 0;
    long initialTime = 0;
    float x1 = 0.0f;
    float y1 = 0.0f;
    float z1 = 0.0f;

    //  [SerializeField]
    //private BicycleController bicycleComponent; // Reference to the source script

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

        }
        else
        {
            Destroy(gameObject);
        }

        bicycleController = GameObject.Find("Biycycle Standard").GetComponent<BicycleController>();
    }

    Vector3 vehicle1pos, vehicle2pos, vehicle3pos;
    float lenght1 = 0f;
    float width1 = 0f;
    float lenght2 = 0f;
    float width2 = 0f;
    float lenght3 = 0f;
    float width3 = 0f;

    public void UpdateCurrentSection(int section)
    {
        CurrentSection = section;
        //Debug.Log("CurrentSection :" + CurrentSection);
    }

    /*
        public void UpdateVehicle1Details(float lenght, float width)
        {
            lenght1 = lenght;
            width1 = width;
        }

        public void UpdateVehicle2Details(float lenght, float width)
        {
            lenght2 = lenght;
            width2 = width;
        }

        public void UpdateVehicle3Details(float lenght, float width)
        {
            lenght3 = lenght;
            width3 = width;
        }
    */

    public void UpdateVehicle1Position(Vector3 position, Transform carpos)
    {
        //for CSV not into the Game. Refractoring now means have to update 3 to 9 places. so not refactoring ^.^
        vehicle1pos = position + carpos.position;
        //Debug.Log("vehicle1pos :" + vehicle1pos);
    }

    public void GetBikePositionAndT1(out Transform bikebackTransform, out float controlledDistance, out bool isBikeFrameOn)
    {
        bikebackTransform = bikeback.transform;
        controlledDistance = t1ControlledDistance1DecimalMax;
        isBikeFrameOn = false;
        if (sceneName == "NoLane")
        {
            isBikeFrameOn = true;
        }
        //Debug.Log("bikePos Got by Spawn - special algo");
    }
    public void GetBikePositionAndT2(out Transform bikebackTransform, out float controlledDistance, out bool isBikeFrameOn)
    {
        bikebackTransform = bikeback.transform;
        controlledDistance = t2ControlledDistance1DecimalMax;
        isBikeFrameOn = false;
        if (sceneName == "NoLane")
        {
            isBikeFrameOn = true;
        }
        //Debug.Log("bikePos Got by Spawn - special algo");
    }
    public void GetBikePositionAndT3(out Transform bikebackTransform, out float controlledDistance, out bool isBikeFrameOn)
    {
        bikebackTransform = bikeback.transform;
        controlledDistance = t3ControlledDistance1DecimalMax;
        isBikeFrameOn = false;
        if (sceneName == "NoLane")
        {
            isBikeFrameOn = true;
        }
        //Debug.Log("bikePos Got by Spawn - special algo");
    }

    public void UpdateVehicle2Position(Vector3 position, Transform carpos)
    {
        vehicle2pos = position + carpos.position;
        //Debug.Log("vehicle2pos :" + vehicle2pos);
    }

    public void UpdateVehicle3Position(Vector3 position, Transform carpos)
    {
        vehicle3pos = position + carpos.position;
        //Debug.Log("vehicle3pos :" + vehicle3pos);
    }


    string folderPathCSV = "";
    


    private Stopwatch stopwatch = new Stopwatch();


// Start is called before the first frame update
void Start()
    {

        stopwatch.Start(); // Start frame timing

        HDRenderPipelineAsset hdrp = GraphicsSettings.renderPipelineAsset as HDRenderPipelineAsset;
        if (hdrp != null)
        {
            UnityEngine.Debug.Log("HDRP Optimization Started...");
          
            if (hdrp == null)
            {
                UnityEngine.Debug.LogError(" HDRP not detected! Make sure HDRP is enabled in Graphics Settings.");
                return;
            }

            UnityEngine.Debug.Log("HDRP Optimization Started...");


            // Reduce Bloom Intensity (Post-Processing)
            VolumeProfile volumeProfile = FindObjectOfType<Volume>()?.profile;
            if (volumeProfile != null && volumeProfile.TryGet(out Bloom bloom))
            {
                bloom.intensity.overrideState = true;
                bloom.intensity.value = 0.2f;
                UnityEngine.Debug.Log("Bloom intensity reduced.");
            }

        }

        QualitySettings.SetQualityLevel(0);
        QualitySettings.vSyncCount = 0;
        QualitySettings.shadows = ShadowQuality.Disable;
        QualitySettings.lodBias = 0.5f;

        stopwatch.Stop(); // Stop frame timing
    UnityEngine.Debug.Log($"HDRP Optimizations Applied! Time Taken: {stopwatch.ElapsedMilliseconds} ms");

    //bicycleComponent = GetComponent<BicycleController>(); // Assuming the SourceScript component is attached to the same GameObject
    //int value = sourceScript.myVariable; // Accessing the variable from the source script
    // Debug.Log("Value: " + value);


    folderPathCSV = Path.Combine(Application.dataPath, "bikeLogs");
        if (!Directory.Exists(folderPathCSV))
        {
            Directory.CreateDirectory(folderPathCSV);
        }

        TimeZoneInfo isZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime isTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, isZone);
        string timeForPath = DateTime.UtcNow.ToString("yyyy-MMM-dd_HH-mm-ss-fff"); 


        filename = Path.Combine(folderPathCSV, $"bikeLog_{timeForPath}.csv");

        x1 = bikeback.transform.position.x;
        y1 = bikeback.transform.position.y;
        z1 = bikeback.transform.position.z;


        initialTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); //since 1970

        hour = System.DateTime.Now.Hour;
        minutes = System.DateTime.Now.Minute;
        seconds = System.DateTime.Now.Second;
        milliseconds = System.DateTime.Now.Millisecond;
        time = "" + hour + ":" + minutes + ":" + seconds + ":" + milliseconds;

        sceneName = SceneManager.GetActiveScene().name;


        /*

            Bike - 2.1, 0.8
            Testfab (car) - 2.4, 1.7
            Truck - 9.5, 3
            Bicycle - 2.25, 0.6

            scene1  NoLane
            v1 : testfab 
            v2 : truckfab
            v3 : truckfab 

            scene2 Boullards
            v1 : motorbikefab 
            v2 : testfab
            v3 : testfab

            scene3 BicycleLane
            v1 : testfab
            v2 : motorbikefab
            v3 : truckfab
        */
        if (sceneName == "NoLane")
        {
            lenght1 = 2.1f;
            width1 = 0.8f;
            lenght2 = 9.5f;
            width2 = 3f;
            lenght3 = 9.5f;
            width3 = 3f;
            //longOffset1 = -55f;
            //latOffset1 = -50f;
            //longOffset2 = -55f;
            //latOffset2 = -50f;
            //longOffset3 = -55f;
            //latOffset3 = -50f;
        }
        if (sceneName == "Boullards")
        {
            lenght1 = 2.1f;
            width1 = 0.8f;
            lenght2 = 2.4f;
            width2 = 1.7f;
            lenght3 = 2.4f;
            width3 = 1.7f;
            //longOffset1 = -55f;
            //latOffset1 = -50f;
            //longOffset2 = -55f;
            //latOffset2 = -50f;
            //longOffset3 = -55f;
            //latOffset3 = -50f;
        }
        if (sceneName == "BicycleLane")
        {
            lenght1 = 2.4f;
            width1 = 1.7f;
            lenght2 = 2.1f;
            width2 = 0.8f;
            lenght3 = 9.5f;
            width3 = 3f;
            //longOffset1 = -55f;
            //latOffset1 = -50f;
            //longOffset2 = -55f;
            //latOffset2 = -50f;
            //longOffset3 = -55f;
            //latOffset3 = -50f;
        }

        UpdateCurrentSection(0);

        //BlyncSensorSpeed = spd.GetComponent<FitnessEquipmentDisplay>().speed;
        ResetCSV();

        if (bikeback != null)
        {
            initialPosZExp1 = bikeback.transform.position.z;
        }

    }

    /* flag 1 has changed to scene name
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "trigger1")
        {
           flag1 = flag1++;
     }
    }
    */


    float initialPosZExp1 = 0;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ResetCSV();
        }

        // Debug.Log("timetotal : " + GetTotalTimePassed() + ", long s,a,d : " + GetLongSpeed() + ", " + GetLongAcceleration() + "," + GetLongitudinalDistance() +
        //      ", lat : " + GetLatSpeed() + "," + GetLatAcceleration() + "," + GetLatDistance());



        MovementDiff movDiff = GetCurrentMovementDiffLatLong();
        if (diffOneSecondFlag == 1)
        {
            //Debug.Log(movDiff.DiffTime + ", long d,s,a : " + movDiff.LongDistance + ", " + movDiff.LongSpeed + ", " + movDiff.LongAcceleration +
            //    ", lat d,s,a : " + movDiff.LatDistance + ", " + movDiff.LatSpeed + ", " + movDiff.LatAcceleration);

            //AppendCSV
            AppendAsync(movDiff);
        }
        else
        {
            //   MovementDiff movDiff = GetCurrentMovementDiffLatLong();
            // Debug.Log("hahaha");
        }
        /*
        if (bikeback != null)
        {
            /*Debug.Log("caesar" + bikeback.transform.position.z + "," + bikeback.transform.position.x + "," + bikeback.transform.position.y + "," + initialPositionLong 
                + "," + (bikeback.transform.position.z - initialPositionLong));
            Debug.Log("mountbaten " + diffTimeGlobal);
            
        }*/


        //Debug.Log("initpos x,y,z : " + x1+","+y1 + "," +z1 + ", curpos x,y,z:" +
        //   bikeback.transform.position.x + "," +bikeback.transform.position.y + "," +bikeback.transform.position.z);



        ////experiment 1
        //if (bikeback != null)
        //{
        //    bikeback.transform.position = new Vector3(bikeback.transform.position.x, bikeback.transform.position.y, -362 + 10f * Time.time);
        //    Debug.Log("sierra, " + bikeback.transform.position.z + " high sierra, " + Time.time);
        //}


        //experiment 2
        if (bikeback != null)
        {
            //Debug.Log("tut : " + (initialPosZExp1-bikeback.transform.position.z) + ", kamun : " + Time.time);
            //bikeback.transform.position = new Vector3(bikeback.transform.position.x, bikeback.transform.position.y, 10f * Time.time);
            elapsedTime = Time.time - lastSavedTime;
            

            if (elapsedTime > 0.2f)
            {
                lastSavedTime = Time.time;
                float s = (Math.Abs(bikeback.transform.position.z) - lastSavedZ) / elapsedTime;
                //Debug.Log("tsar, " + lastSavedZ + " earl, " + elapsedTime + " duke, " + s + " sierra, " + bikeback.transform.position.z + " high sierra, " + Time.time);
                lastSavedZ = bikeback.transform.position.z;
            }


            //Debug.Log("sierra, " + bikeback.transform.position.z + " high sierra, " + Time.time);
        }

    }
    float elapsedTime = 0f;
    float lastSavedTime = 0f;
    float lastSavedZ = 0f;



    public double diffTimeGlobal;


    public struct MovementDiff
    {
        public double DiffTime;

        public double LongDistance;
        public double LongSpeed;
        public double LongAcceleration;
        public double LatDistance;
        public double LatSpeed;
        public double LatAcceleration;


        public double Vehicle1LongDistance;
        public double Vehicle1LongSpeed;
        public double Vehicle1LongAcceleration;
        public double Vehicle1LatDistance;
        public double Vehicle1LatSpeed;
        public double Vehicle1LatAcceleration;


        public double Vehicle2LongDistance;
        public double Vehicle2LongSpeed;
        public double Vehicle2LongAcceleration;
        public double Vehicle2LatDistance;
        public double Vehicle2LatSpeed;
        public double Vehicle2LatAcceleration;


        public double Vehicle3LongDistance;
        public double Vehicle3LongSpeed;
        public double Vehicle3LongAcceleration;
        public double Vehicle3LatDistance;
        public double Vehicle3LatSpeed;
        public double Vehicle3LatAcceleration;

        public MovementDiff(double diffTime,
            double distanceLong, double speedLong, double accelerationLong, double distanceLat, double speedLat, double accelerationLat,
            double distanceLong1, double speedLong1, double accelerationLong1, double distanceLat1, double speedLat1, double accelerationLat1,
            double distanceLong2, double speedLong2, double accelerationLong2, double distanceLat2, double speedLat2, double accelerationLat2,
            double distanceLong3, double speedLong3, double accelerationLong3, double distanceLat3, double speedLat3, double accelerationLat3)
        {
            DiffTime = diffTime;
            LongDistance = distanceLong;
            LongSpeed = speedLong;
            LongAcceleration = accelerationLong;
            LatDistance = distanceLat;
            LatSpeed = speedLat;
            LatAcceleration = accelerationLat;


            Vehicle1LongDistance = distanceLong1;
            Vehicle1LongSpeed = speedLong1;
            Vehicle1LongAcceleration = accelerationLong1;
            Vehicle1LatDistance = distanceLat1;
            Vehicle1LatSpeed = speedLat1;
            Vehicle1LatAcceleration = accelerationLat1;

            Vehicle2LongDistance = distanceLong2;
            Vehicle2LongSpeed = speedLong2;
            Vehicle2LongAcceleration = accelerationLong2;
            Vehicle2LatDistance = distanceLat2;
            Vehicle2LatSpeed = speedLat2;
            Vehicle2LatAcceleration = accelerationLat2;

            Vehicle3LongDistance = distanceLong3;
            Vehicle3LongSpeed = speedLong3;
            Vehicle3LongAcceleration = accelerationLong3;
            Vehicle3LatDistance = distanceLat3;
            Vehicle3LatSpeed = speedLat3;
            Vehicle3LatAcceleration = accelerationLat3;
        }
    }

    double diffTimePointSave = 0D;

    double diffDistanceLongPointSave = 0f;
    double diffDistanceLatPointSave = 0f;
    double diffDistanceVehicle1LongPointSave = 0f;
    double diffDistanceVehicle1LatPointSave = 0f;
    double diffDistanceVehicle2LongPointSave = 0f;
    double diffDistanceVehicle2LatPointSave = 0f;
    double diffDistanceVehicle3LongPointSave = 0f;
    double diffDistanceVehicle3LatPointSave = 0f;

    int diffOneSecondFlag = 0;

    public MovementDiff GetCurrentMovementDiffLatLong()
    {
        double diffTime = 0d;

        double diffDistanceLong = 0f;
        double diffSpeedLong = 0f;
        double diffAccelerationLong = 0f;
        double diffDistanceLat = 0f;
        double diffSpeedLat = 0f;
        double diffAccelerationLat = 0f;

        double diffVehicle1DistanceLong = 0f;
        double diffVehicle1SpeedLong = 0f;
        double diffVehicle1AccelerationLong = 0f;
        double diffVehicle1DistanceLat = 0f;
        double diffVehicle1SpeedLat = 0f;
        double diffVehicle1AccelerationLat = 0f;

        double diffVehicle2DistanceLong = 0f;
        double diffVehicle2SpeedLong = 0f;
        double diffVehicle2AccelerationLong = 0f;
        double diffVehicle2DistanceLat = 0f;
        double diffVehicle2SpeedLat = 0f;
        double diffVehicle2AccelerationLat = 0f;

        double diffVehicle3DistanceLong = 0f;
        double diffVehicle3SpeedLong = 0f;
        double diffVehicle3AccelerationLong = 0f;
        double diffVehicle3DistanceLat = 0f;
        double diffVehicle3SpeedLat = 0f;
        double diffVehicle3AccelerationLat = 0f;


        //time diff
        diffTime = Time.time - diffTimePointSave;
        if (diffTime > 0.2)
        {
            diffTimePointSave = Time.time;
            diffTimeGlobal = diffTime;
            diffOneSecondFlag = 1;


            //bicycle
            //distance diff long z

            diffDistanceLong = bikeback.transform.position.z - diffDistanceLongPointSave;
            diffDistanceLongPointSave = bikeback.transform.position.z;
            //Debug.Log("long pos" + diffDistanceLongPointSave);
            //speed diff long
            diffSpeedLong = (diffDistanceLong / diffTime)*3.6;

            //acceleration diff long
            diffAccelerationLong = diffDistanceLong / (diffTime * diffTime);


            //distance diff lat x (bug, why?)

            diffDistanceLat = bikeback.transform.position.x - diffDistanceLatPointSave;
            diffDistanceLatPointSave = bikeback.transform.position.x;

            //speed diff lat
            diffSpeedLat = (diffDistanceLat / diffTime)*3.6;

            //acceleration diff long

            diffAccelerationLat = diffDistanceLat / (diffTime * diffTime);


            //vehicle 1
            //distance diff long z

            diffVehicle1DistanceLong = vehicle1pos.z - diffDistanceVehicle1LongPointSave;
            diffDistanceVehicle1LongPointSave = vehicle1pos.z;

            //speed diff long
            diffVehicle1SpeedLong = (diffVehicle1DistanceLong / diffTime) * 3.6;

            //acceleration diff long
            diffVehicle1AccelerationLong = diffVehicle1DistanceLong / (diffTime * diffTime);


            //distance diff lat x (bug, why?)

            diffVehicle1DistanceLat = vehicle1pos.x - diffDistanceVehicle1LatPointSave;
            diffDistanceVehicle1LatPointSave = vehicle1pos.x;

            //speed diff lat
            diffVehicle1SpeedLat = (diffVehicle1DistanceLat / diffTime) * 3.6;

            //acceleration diff long

            diffVehicle1AccelerationLat = diffVehicle1DistanceLat / (diffTime * diffTime);


            //vehicle 2
            //distance diff long z

            diffVehicle2DistanceLong = vehicle2pos.z - diffDistanceVehicle2LongPointSave;
            diffDistanceVehicle2LongPointSave = vehicle2pos.z;

            //speed diff long
            diffVehicle2SpeedLong = (diffVehicle2DistanceLong / diffTime) * 3.6;

            //acceleration diff long
            diffVehicle2AccelerationLong = diffVehicle2DistanceLong / (diffTime * diffTime);


            //distance diff lat x (bug, why?)

            diffVehicle2DistanceLat = vehicle2pos.x - diffDistanceVehicle2LatPointSave;
            diffDistanceVehicle2LatPointSave = vehicle2pos.x;

            //speed diff lat
            diffVehicle2SpeedLat = (diffVehicle2DistanceLat / diffTime) * 3.6;

            //acceleration diff long

            diffVehicle2AccelerationLat = diffVehicle2DistanceLat / (diffTime * diffTime);


            //vehicle 3
            //distance diff long z

            diffVehicle3DistanceLong = vehicle3pos.z - diffDistanceVehicle3LongPointSave;
            diffDistanceVehicle3LongPointSave = vehicle3pos.z;

            //speed diff long
            diffVehicle3SpeedLong = (diffVehicle3DistanceLong / diffTime) * 3.6;

            //acceleration diff long
            diffVehicle3AccelerationLong = diffVehicle3DistanceLong / (diffTime * diffTime);


            //distance diff lat x (bug, why?)

            diffVehicle3DistanceLat = vehicle3pos.x - diffDistanceVehicle3LatPointSave;
            diffDistanceVehicle3LatPointSave = vehicle3pos.x;

            //speed diff lat
            diffVehicle3SpeedLat = (diffVehicle3DistanceLat / diffTime) * 3.6;

            //acceleration diff long

            diffVehicle3AccelerationLat = diffVehicle3DistanceLat / (diffTime * diffTime);

        }
        else
        {
            diffOneSecondFlag = 0;
        }


        return new MovementDiff(diffTime,
            diffDistanceLong, diffSpeedLong, diffAccelerationLong, diffDistanceLat, diffSpeedLat, diffAccelerationLat,
            diffVehicle1DistanceLong, diffVehicle1SpeedLong, diffVehicle1AccelerationLong, diffVehicle1DistanceLat, diffVehicle1SpeedLat, diffVehicle1AccelerationLat,
            diffVehicle2DistanceLong, diffVehicle2SpeedLong, diffVehicle2AccelerationLong, diffVehicle2DistanceLat, diffVehicle2SpeedLat, diffVehicle2AccelerationLat,
            diffVehicle3DistanceLong, diffVehicle3SpeedLong, diffVehicle3AccelerationLong, diffVehicle3DistanceLat, diffVehicle3SpeedLat, diffVehicle3AccelerationLat
            );

    }


    public void ResetCSV()
    {
        if (bikeback != null)
        {

            TimeZoneInfo isZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            DateTime isTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, isZone);
            string timeForPath = DateTime.UtcNow.ToString("yyyy-MMM-dd_HH-mm-ss-fff");

            filename = Path.Combine(folderPathCSV, $"bikeLog_{timeForPath}.csv");

            TextWriter tw = new StreamWriter(filename, false);
            tw.WriteLine(
                "Vehicle ID"
                + "," + "Time (s)"
                + "," + "Vehicle Type"
                + "," + "Vehicle Length (m)"
                + "," + "Vehicle Width (m)"
                + "," + "Longitudinal Position (m)"
                + "," + "Lateral Position (m)"
                + "," + "Longitudinal Speed (m/s)"
                + "," + "Lateral Speed (m/s)"
                + "," + "Wahoo Speed (m/s)"
                + "," + "Longitudinal Acceleration (m/s^2)"
                + "," + "Lateral Acceleration (m/s^2)"
                + "," + "Flag 1"
                + "," + "Flag 2"
                + "," + "Cadence (rpm)"
                + "," + "Bicycle Turn Angle (degree)"
                + "," + "Direction of the Vehicle (same side/opposite side)"
                + "," + "Vehicle 1 ID"
                + "," + "Vehicle 1 Length"
                + "," + "Vehicle 1 Width"
                + "," + "Vehicle 1 Longitudinal Position"
                + "," + "Vehicle 1 Latitudinal Position"
                + "," + "Vehicle 1 Longitudinal Speed (m/s)"
                + "," + "Vehicle 1 Latitudinal Speed (m/s)"
                + "," + "Vehicle 1 Longitudinal Acceleration"
                + "," + "Vehicle 1 Latitudinal Acceleration"
                + "," + "Vehicle 2 ID"
                + "," + "Vehicle 2 Length"
                + "," + "Vehicle 2 Width"
                + "," + "Vehicle 2 Longitudinal Position"
                + "," + "Vehicle 2 Latitudinal Position"
                + "," + "Vehicle 2 Longitudinal Speed (m/s)"
                + "," + "Vehicle 2 Latitudinal Speed (m/s)"
                + "," + "Vehicle 2 Longitudinal Acceleration"
                + "," + "Vehicle 2 Latitudinal Acceleration"
                + "," + "Vehicle 3 ID"
                + "," + "Vehicle 3 Length"
                + "," + "Vehicle 3 Width"
                + "," + "Vehicle 3 Longitudinal Position"
                + "," + "Vehicle 3 Latitudinal Position"
                + "," + "Vehicle 3 Longitudinal Speed (m/s)"
                + "," + "Vehicle 3 Latitudinal Speed (m/s)"
                + "," + "Vehicle 3 Longitudinal Acceleration"
                + "," + "Vehicle 3 Latitudinal Acceleration"
                );
            tw.Close();

            initialPositionLong = bikeback.transform.position.z; //Longitudinal Position (m)
            initialPositionLat = bikeback.transform.position.x; //Lateral Position (m)
        }
    }
    //Vehicle ID    Time (s)    "Vehicle Type
    //" Vehicle Length (m)  Vehicle Width (m)   Longitudinal Position (m)   Lateral Position (m)    Longitudinal Speed (m/s)    
    //Lateral Speed (m/s)   Longitudinal Acceleration (m/s^2    Lateral Acceleration (m/s^2)    Flag 1  Flag 2  Cadence (rpm)   
    ////Bicycle Turn Angle (degree) Direction of the Vehicle (same side/opposite side)


    /*
     TODO: to decide -
    1. what axis is longitudinal, if not axis based, what is it based on it? longitudinal (z) 
    2. what is lateral lateral (x)
    3. what is flag 1, flag 2
    4. for cadence we need 
        a. chainring teeth count 46
        b. cog teeth count 15
        c. wheel diameter 700c/700mm (inflated)
        d. size of tires (in the standard inch tire measurement, or measure the width of the tire manually)
        reference : https://www.omnicalculator.com/sports/bike-cadence 38c
    5. convert whatever turnangle value unity outputs into the degree
    6. what equation or criteria to use for deciding direction of vehicle? if going in -z, same lane. if going in z , opposite traffic lane
    7. vehicle id
    8. Vehicle Type
    9. Vehicle Length (m)
    10. Vehicle Width (m)
    11. why do we need longitutdinal and lateral speeds separately? (any cycling paaradigm? ask prof, coz we can get total speed without marking them separately)
    12. is the computing fast enough to ignore time diffs in ref point storages? if slow, then create separate ref points. check for all uses of both time and position.
     */

    public float GetDistance3D()
    {
        float x2 = bikeback.transform.position.x;
        float y2 = bikeback.transform.position.y;
        float z2 = bikeback.transform.position.z;

        //add if 0, first time condition
        float dist = Mathf.Sqrt(Mathf.Pow(Mathf.Abs(x2 - x1), 2) + Mathf.Pow(Mathf.Abs(y2 - y1), 2) + Mathf.Pow(Mathf.Abs(z2 - z1), 2));
        if (dist != 0)
        {
            /*x1 = x2;
            y1 = y2;
            z1 = z2;*/
            return dist;
        }
        else
        {
            dist = Mathf.Sqrt(Mathf.Pow(Mathf.Abs(x2), 2) + Mathf.Pow(Mathf.Abs(y2), 2) + Mathf.Pow(Mathf.Abs(z2), 2));
            return dist;
        }
    }
    public float GetLongitudinalDistance()
    {
        float z2 = bikeback.transform.position.z;
       

        //add if 0, first time condition

        float dist = z2 - z1;
        if (dist != 0)
        {
            //z1 = z2;
            return dist;
        }
        else
        {
            return z2;
        }
    }
    public float GetLatDistance()
    {
        float x2 = bikeback.transform.position.x;

        //add if 0, first time condition

        float dist = x2 - x1;

        if (dist != 0)
        {
            //x1 = x2;
            return dist;
        }
        else
        {
            return x2;
        }
    }
    public long GetTotalTimePassed()
    {
        DateTime currentTime = DateTime.Now;
        //long time2 = GetUnixTimestamp(currentTime);
        long time3 = System.Environment.TickCount / 1000; //since system start in seconds
        long time4 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); //since 1970
        long totalTimePassed = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - initialTime) / 1000; //since 1970
        String displayTime = DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond.ToString();
        //long timePassed = DateTime.Now.Millisecond - initialTime;
        //timePoint = timePassed;
        return totalTimePassed; //todo test time3
    }
    public String GetDisplayTime() //todo test this
    {
        String displayTime = DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond.ToString();
        //timePoint = timePassed;
        return displayTime;
    }



    public long diffTimeLatPoint = 0;
    public long timeLat = 0;
    public long GetLatDiffTimePassed()
    {
        long timePassed = DateTime.Now.Millisecond - diffTimeLatPoint;

        /*timeLatPoint = timePassed;
        timeLat = timePassed;*/
        diffTimeLatPoint = DateTime.Now.Millisecond;
        return timePassed;

    }

    public long diffTimeLongPoint = 0;
    public long timeLong = 0;
    public long GetLongDiffTimePassed()
    {
        long timePassed = DateTime.Now.Millisecond - diffTimeLongPoint;
        /* timeLongPoint = timePassed;
            timeLong = timePassed;*/
        diffTimeLongPoint = DateTime.Now.Millisecond;
        return timePassed;

    }

    public float Get3DSpeed()
    {
        float speed = GetDistance3D() / GetTotalTimePassed();
        return speed;
    }
    public float GetLongSpeed()
    {
        float speed = GetLongitudinalDistance() / GetTotalTimePassed();
        return Math.Abs(speed);
    }
    public float GetLongAcceleration()
    {
        float acc = GetLongSpeed() / GetTotalTimePassed();
        return Math.Abs(acc);
    }
    public float GetLatSpeed()
    {
        float speed = (GetLatDistance() / GetTotalTimePassed());
        return Math.Abs(speed);
    }

    public float GetLatAcceleration()
    {
        float acc = GetLatSpeed() / GetTotalTimePassed();
        return Math.Abs(acc);
    }
    public double GetCadence()
    {
        //double cadence = 0;
        //todo
        //double cadence = Get3DSpeed() / (3.14 * (700 + 2*38) * (46d/15d));
        int cadence = spd.GetComponent<FitnessEquipmentDisplay>().cadence;
        return cadence;
    }


    public String GetDirection()
    {
        float z2 = bikeback.transform.position.z;
        String direction = z2 - z1 > 0 ? "opposite traffic lane" : "same lane";
        //z1 = z2;
        return direction;
    }

    private async Task AppendCSV(MovementDiff movDiff)
    {
        //todo add conditional time interval
        //await Task.Delay(100);
        if (bikeback != null)
        {
            //Debug.Log("bikeback not null");

            //filename not changed coz appending to same file
            TextWriter tw = new StreamWriter(filename, true); 
            /*if (BlyncTurnAngle == null)
            {
                tw.WriteLine(
                    "Vehicle ID"
                    + "," + GetDisplayTime() //DateTime.Now.Millisecond
                    + "," + "Vehicle Type"
                    + "," + "Vehicle Length (m)"
                    + "," + "Vehicle Width (m)"
                    + "," + (bikeback.transform.position.z - initialPositionLong) //Longitudinal Position (m)
                    + "," + (bikeback.transform.position.x - initialPositionLat) //Lateral Position (m)
                    + "," + GetLongSpeed() //spd.GetComponent<FitnessEquipmentDisplay>().speed //GetLongSpeed() //Longitudinal Speed (m)
                    + "," + GetLatSpeed() //Lateral Speed (m/s)
                    + "," + GetLongAcceleration() //"Longitudinal Acceleration (m/s^2)"
                    + "," + GetLatAcceleration() //"Longitudinal Acceleration (m/s^2)"
                    + "," + "Flag 1"
                    + "," + "Flag 2"
                    + "," + GetCadence() //"Cadence (rpm)"
                    + "," + "blync null" //BlyncTurnAngle.value //turnAngle TODO import this and check what valute and unit it outputs in, convert to degree if needed.
                    + "," + GetDirection() //"Direction of the Vehicle (same side/opposite side)
                    );
                tw.Close();
            }
            else
            {*/

            //Debug.Log("Writing to file" + movDiff.LongDistance);
            //Debug.Log("Writing to file" + GetCadence());
            //Debug.Log("Writing to file" + GetDirection());
            //Debug.Log("Writing to file ta" + viveTA.turnAngle);


            if (bikeback != null)
            {
                if (viveTA != null)
                {
                    //Debug.Log("viveTA not null");
                    tw.WriteLine(
                        "1" //Vehicle ID
                        + "," + GetDisplayTime() //DateTime.Now.Millisecond
                        + "," + "1" //Vehicle ID
                        + "," + "2.2" //Vehicle Length (m)
                        + "," + "0.6" //Vehicle Width (m)
                        + "," + bikeback.transform.position.z //(bikeback.transform.position.z - initialPositionLong) //movDiff.LongDistance //(bikeback.transform.position.z - initialPositionLong) //Longitudinal Position (m)
                        + "," + bikeback.transform.position.x //(bikeback.transform.position.x - initialPositionLat) //movDiff.LatDistance //(bikeback.transform.position.x - initialPositionLat) //Lateral Position (m)
                        + "," + movDiff.LongSpeed * 0.2778 //GetLongSpeed() //spd.GetComponent<FitnessEquipmentDisplay>().speed //GetLongSpeed() //Longitudinal Speed (m)
                        + "," + movDiff.LatSpeed * 0.2778 //GetLatSpeed() //Lateral Speed (m/s)
                        + "," + spd.GetComponent<FitnessEquipmentDisplay>().speed * 0.2778
                        + "," + movDiff.LongAcceleration //GetLongAcceleration() //"Longitudinal Acceleration (m/s^2)"
                        + "," + movDiff.LatAcceleration //GetLatAcceleration() //"Longitudinal Acceleration (m/s^2)"
                        + "," + sceneName //Flag 1 scene name
                        + "," + CurrentSection
                        + "," + GetCadence() //"Cadence (rpm)"
                        + "," + viveTA.turnAngle //BlyncTurnAngle.value //turnAngle TODO import this and check what value and unit it outputs in, convert to degree if needed.
                        + "," + GetDirection() //"Direction of the Vehicle (same side/opposite side)
                        +"," + "2" //"Vehicle 1 ID"
                        + "," + lenght1 //"Vehicle 1 Length"
                        + "," + width1 //"Vehicle 1 Width"
                        + "," + vehicle1pos.z //movDiff.Vehicle1LongDistance //"Vehicle 1 Longitudinal Position"
                        + "," + vehicle1pos.x //movDiff.Vehicle1LatDistance //"Vehicle 1 Latitudinal Position"
                        + "," + movDiff.Vehicle1LongSpeed * 0.2778  //"Vehicle 1 Longitudinal Speed in m/s"
                        + "," + movDiff.Vehicle1LatSpeed * 0.2778  //"Vehicle 1 Latitudinal Speed" in m/s
                        + "," + movDiff.Vehicle1LongAcceleration //"Vehicle 1 Longitudinal Acceleration"
                        + "," + movDiff.Vehicle1LatAcceleration //"Vehicle 1 Latitudinal Acceleration"
                        + "," + "3" //"Vehicle 2 ID"
                        + "," + lenght2 //"Vehicle 2 Length"
                        + "," + width2 //"Vehicle 2 Width"
                        + "," + vehicle2pos.z //movDiff.Vehicle2LongDistance //"Vehicle 2 Longitudinal Position"
                        + "," + vehicle2pos.x //movDiff.Vehicle2LatDistance //"Vehicle 2 Latitudinal Position"
                        + "," + movDiff.Vehicle2LongSpeed * 0.2778  //"Vehicle 2 Longitudinal Speed" in m/s
                        + "," + movDiff.Vehicle2LatSpeed * 0.2778 //"Vehicle 2 Latitudinal Speed" in m/s
                        + "," + movDiff.Vehicle2LongAcceleration //"Vehicle 2 Longitudinal Acceleration"
                        + "," + movDiff.Vehicle2LatAcceleration //"Vehicle 2 Latitudinal Acceleration"
                        + "," + "4" //"Vehicle 3 ID"
                        + "," + lenght3 //"Vehicle 3 Length"
                        + "," + width3 //"Vehicle 3 Width"
                        + "," + vehicle3pos.z //movDiff.Vehicle3LongDistance //"Vehicle 3 Longitudinal Position"
                        + "," + vehicle3pos.x //movDiff.Vehicle3LatDistance //"Vehicle 3 Latitudinal Position"
                        + "," + movDiff.Vehicle3LongSpeed * 0.2778 //"Vehicle 3 Longitudinal Speed"in m/s
                        + "," + movDiff.Vehicle3LatSpeed * 0.2778 //"Vehicle 3 Latitudinal Speed" in m/s
                        + "," + movDiff.Vehicle3LongAcceleration //"Vehicle 3 Longitudinal Acceleration"
                        + "," + movDiff.Vehicle3LatAcceleration //"Vehicle 3 Latitudinal Acceleration"
                        );
                }
            }
            tw.Close();

            //Debug.Log("Finished writing to file");
            //}
        }
        //Debug.Log("async running");
    }


    public async void AppendAsync(MovementDiff movDiff)
    {

        
        await AppendCSV(movDiff);

        //Debug.Log("async completed");
    }
}
