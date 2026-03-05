using System.Collections;
using UnityEngine;

public class Spawn350 : MonoBehaviour
{
    public GameObject car;
    public Transform carpos;
    public Vector3 pos;

    private GameObject spawnedCar2 = null;

    public float liveBikeXDiffPostiion = 0f;

    void Start()
    {
        StartCoroutine(SelfDestruct());

        //Debug.Log("Spawned Car Position:2 ");
    }

    private void FixedUpdate()
    {

        if (spawnedCar2 != null)
        {
            //Debug.Log("Spawned Car Position:1 ");

            Transform waypointBasedTransform = spawnedCar2.transform.Find("CarWaypointBased (1)");
            Transform constantDistanceTransform = spawnedCar2.transform.Find("GameObject");

            if (waypointBasedTransform != null)
            {

                //Debug.Log("Spawned Car Position:10 ");
                //Vector3 pos = spawnedCar2.transform.position + waypointBasedTransform.localPosition;                

                pos = waypointBasedTransform.localPosition;

                //Debug.Log("magic Car Position: " + pos);

                if (CSVWriter.Instance != null)
                {
                    CSVWriter.Instance.UpdateVehicle2Position(pos, carpos);
                    
                    if (constantDistanceTransform != null)
                    {
                        CSVWriter.Instance.GetBikePositionAndT2(out Transform bikeTransform, out float controlledDistance, out bool isBikeFrameOn);

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
                CSVWriter.Instance.UpdateCurrentSection(350);
                //CSVWriter.Instance.UpdateVehicle1Details(1.42f, 1.11f); //change based on prefab. this is hardcoding

            }
        }
    }

    void CarSpawner()
    {
        //Debug.Log("Spawned Car Position:6 ");
        //if (spawnedCar2 != null) Destroy(spawnedCar2);


        //Debug.Log("Spawned Car Position:5 ");
        spawnedCar2 = Instantiate(car, carpos.position, carpos.rotation);
        //Debug.Log("Spawned Car Position:9 ");

        //Instantiate(carPrefab, spawnPoint.position, spawnPoint.rotation);


    }

    IEnumerator SelfDestruct()
    {

        //Debug.Log("Spawned Car Position:7 ");
        yield return new WaitForSeconds(2f);
        if (spawnedCar2 != null)
        {

            if (CSVWriter.Instance != null)
            {
                CSVWriter.Instance.UpdateVehicle2Position(Vector3.zero, carpos);
            }
            //Debug.Log("Spawned Car Position:8 ");
            Destroy(spawnedCar2);
            spawnedCar2 = null;
        }
    }
}
