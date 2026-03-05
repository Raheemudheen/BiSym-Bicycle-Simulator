using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveTarg : MonoBehaviour
{

    public GameObject target;
    public GameObject ped;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "NPC")
        {
            Invoke("Mover", 0.5f);
            Destroy(ped);
        }
    }
    void Mover()
    {
        target.transform.position += transform.forward * Time.deltaTime;
        
    }
}
