using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject objectPrefab; // Prefab of the object to be spawned
    public Transform spawnPoint; // Spawn point location
    //public Transform[] waypoints; // Waypoints for the object to follow
    //public float speed = 5f; // Speed of the object
    private bool hasSpawned = false;

    //private void Start()
    //{

    //    SpawnObject();
    //}
    private void OnTriggerEnter(Collider other)
    {
        // Check if the triggering object has a specific tag or layer if needed
        if (other.CompareTag("Player") && !hasSpawned)
        {
            SpawnObject();
            hasSpawned = true;
        }
    }

    void SpawnObject()
    {
        GameObject spawnedObject = Instantiate(objectPrefab, spawnPoint.position, Quaternion.identity);

        // Attach the object to this script for reference
        //ObjectMover objectMover = spawnedObject.AddComponent<ObjectMover>();
        //objectMover.SetWaypoints(waypoints);
        //objectMover.SetSpeed(speed);
    }
}

//public class ObjectMover : MonoBehaviour
//{
//    private Transform[] waypoints;
//    private float speed;
//    private int currentWaypointIndex = 0;

//    public void SetWaypoints(Transform[] waypoints)
//    {
//        this.waypoints = waypoints;
//    }

//    public void SetSpeed(float speed)
//    {
//        this.speed = speed;
//    }

//    private void Update()
//    {
//        if (waypoints == null || waypoints.Length == 0)
//        {
//            Debug.LogError("No waypoints assigned!");
//            return;
//        }

//        MoveObject();
//    }

//    void MoveObject()
//    {
//        // Move towards the current waypoint
//        transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypointIndex].position, speed * Time.deltaTime);

//        // Check if the object has reached the current waypoint
//        if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position) < 0.1f)
//        {
//            // Move to the next waypoint
//            currentWaypointIndex++;

//            // If the object reached the last waypoint, destroy it
//            if (currentWaypointIndex == waypoints.Length)
//            {
//                Destroy(gameObject);
//            }
//        }
//    }
//}
