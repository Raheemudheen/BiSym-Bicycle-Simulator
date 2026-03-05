using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveTarget : MonoBehaviour
{

    public GameObject target;
    // Start is called before the first frame update
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Peds")
        {
            Invoke("Mover",0.5f);
           
        }
    }
    void Mover()
    {
        target.transform.position += transform.forward * Time.deltaTime;

    }
}
