using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class statictraffic : MonoBehaviour
{
    public GameObject[] prefabsToSpawn;
    public Transform[] waypoints;
    public float minSpawnInterval = 2f;
    public float maxSpawnInterval = 5f;
    public float moveSpeed = 5f;

    private void Start()
    {
        StartCoroutine(SpawnObjects());
    }

    IEnumerator SpawnObjects()
    {
        while (true)
        {
            GameObject prefabToSpawn = GetRandomPrefab();
            GameObject spawnedObject = Instantiate(prefabToSpawn, transform.position, Quaternion.identity);
            StartCoroutine(MoveObject(spawnedObject.transform));
            float spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    GameObject GetRandomPrefab()
    {
        int randomIndex = Random.Range(0, prefabsToSpawn.Length);
        return prefabsToSpawn[randomIndex];
    }

    IEnumerator MoveObject(Transform objectToMove)
    {
        int currentWaypoint = 0;

        while (currentWaypoint < waypoints.Length)
        {
            Vector3 targetPosition = waypoints[currentWaypoint].position;
            Vector3 moveDirection = (targetPosition - objectToMove.position).normalized;

            // Rotate towards the movement direction
            objectToMove.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);

            while (objectToMove.position != targetPosition)
            {
                objectToMove.position = Vector3.MoveTowards(objectToMove.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }

            currentWaypoint++;
            yield return null;
        }

        // Destroy the object when it reaches all waypoints
        Destroy(objectToMove.gameObject);
    }
}
