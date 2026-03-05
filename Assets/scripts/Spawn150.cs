using System.Collections;
using UnityEngine;


//experiments : 
/*
    check basic transform override behaviors
            transform.position    
study functions of car controller and car ai controller
    add sample method that can be called from the ConstantDistanceScript
*/




public class Spawn150 : MonoBehaviour
{
    public GameObject car;
    public Transform carpos;
    public Vector3 pos;



    //public const float ControlledDistance = 2.5f; //max 1 decimal supported for now
    public float liveBikeXDiffPostiion = 0f;

    private GameObject spawnedCar1 = null;


    void Start()
    {
        StartCoroutine(SelfDestruct());

        //Debug.Log("Spawned Car Position:2 ");
    }

    private void FixedUpdate()
    {

        if (spawnedCar1 != null)
        {
            //Debug.Log("Spawned Car Position:1 ");

            Transform waypointBasedTransform = spawnedCar1.transform.Find("CarWaypointBased (1)");
            Transform constantDistanceTransform = spawnedCar1.transform.Find("GameObject");

            if (waypointBasedTransform != null)
            {



                //Debug.Log("Spawned Car Position:10 ");
                //Vector3 pos = spawnedCar1.transform.position + waypointBasedTransform.localPosition;                

                pos = waypointBasedTransform.localPosition;

                //Debug.Log("magic Car Position: " + pos);

                if (CSVWriter.Instance != null)
                {
                    CSVWriter.Instance.UpdateVehicle1Position(pos, carpos);
                    
                    if (constantDistanceTransform != null)
                    {
                        CSVWriter.Instance.GetBikePositionAndT1(out Transform bikeTransform, out float controlledDistance, out bool isBikeFrameOn);

                        if (isBikeFrameOn)
                        {
                            if ((Mathf.Round(constantDistanceTransform.position.x * 10f) / 10f) + controlledDistance != (Mathf.Round(liveBikeXDiffPostiion * 10f) / 10f))
                            {

                                constantDistanceTransform.position = new Vector3(bikeTransform.position.x + controlledDistance, constantDistanceTransform.position.y, constantDistanceTransform.position.z);
                                liveBikeXDiffPostiion = bikeTransform.position.x + controlledDistance;
                            }
                        }
                    }
                }

            }
            else
            {
                //Debug.LogError("subobject not found in the carPrefab");
            }
        }


    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Trigger entered by: " + other.gameObject.name);

        //Debug.Log("Spawned Car Position:3 ");
        if (other.gameObject.tag == "Player")
        {
            Invoke("CarSpawner", 0.5f);

            //Debug.Log("Spawned Car Position:4 ");
            if (CSVWriter.Instance != null)
            {
                CSVWriter.Instance.UpdateCurrentSection(150);
                //CSVWriter.Instance.UpdateVehicle1Details(1.42f, 1.11f); //change based on prefab. this is hardcoding

            }
        }
    }

    void CarSpawner()
    {
        //Debug.Log("Spawned Car Position:6 ");
        //if (spawnedCar1 != null) Destroy(spawnedCar1);


        //Debug.Log("Spawned Car Position:5 ");
        spawnedCar1 = Instantiate(car, carpos.position, carpos.rotation);
        //Debug.Log("Spawned Car Position:9 ");

        /*
        if (spawnedCar1 != null)
        {
            Transform constantDistanceTransform = spawnedCar1.transform.Find("GameObject");


            constantDistanceTransform.position += new Vector3(-7, 0, 0);

            Debug.Log("special algo transformed");
        }*/


        //Instantiate(carPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    IEnumerator SelfDestruct()
    {
        //Debug.Log("Spawned Car Position:7 ");
        yield return new WaitForSeconds(2f);
        if (spawnedCar1 != null)
        {

            if (CSVWriter.Instance != null)
            {
                CSVWriter.Instance.UpdateVehicle1Position(Vector3.zero, carpos);
                //CSVWriter.Instance.UpdateVehicle1Position(spawnedCar1);
            }
            //Debug.Log("Spawned Car Position:8 ");
            Destroy(spawnedCar1);
            spawnedCar1 = null;
        }
    }
}
