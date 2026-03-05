using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    public GameObject car;
    void Start()
    {
        StartCoroutine(Destruct());
    }

   private IEnumerator Destruct()
    {
        yield return new WaitForSeconds(30f);
        Destroy(car);
    }
}
