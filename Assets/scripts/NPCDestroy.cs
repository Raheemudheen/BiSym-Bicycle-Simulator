using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCDestroy : MonoBehaviour
{

    public GameObject npc;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("NPC"))
        {
            Destroy(collision.gameObject);
        }
    }
    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.gameObject.CompareTag("NPC"))
    //    {
    //        Destroy(other.gameObject);
    //    }
    //}
}
