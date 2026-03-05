using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyVehicle : MonoBehaviour
{
    public GameObject car;
    // public Transform carpos;



    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("NPCcar01"))
        {
            Destroy(collision.gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("NPCcar01"))
        {
            Destroy(other.gameObject);
        }
    }

}
    
