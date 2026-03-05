using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficSpawner : MonoBehaviour
{
  
    public GameObject car1;
    public GameObject car2;
    public GameObject car3;
    public GameObject car4;
    public GameObject car5;
    public GameObject car6;
    public GameObject car7;
    public Transform carpos;
    public Transform carpos2;

    void Start()
    {
        StartCoroutine(SelfDestruct());
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Invoke("CarSpawner1", 0.5f);
            Invoke("CarSpawner2", 10f);
            Invoke("CarSpawner3", 20f);
            Invoke("CarSpawner4", 30f);
            Invoke("CarSpawner5", 40f);
            Invoke("CarSpawner6", 50f);
            Invoke("CarSpawner7", 60f);

        }
    }
    void CarSpawner1()
    {
        Instantiate(car1, carpos.position, carpos.rotation);
    }
    void CarSpawner2()
    {
        Instantiate(car2, carpos2.position, carpos2.rotation);
    }
    void CarSpawner3()
    {
        Instantiate(car3, carpos.position, carpos.rotation);
    }
    void CarSpawner4()
    {
        Instantiate(car4, carpos2.position, carpos2.rotation);
    }
    void CarSpawner5()
    {
        Instantiate(car5, carpos.position, carpos.rotation);
    }
    void CarSpawner6()
    {
        Instantiate(car6, carpos2.position, carpos2.rotation);
    }
    void CarSpawner7()
    {
        Instantiate(car7, carpos.position, carpos.rotation);
    }

    IEnumerator SelfDestruct()
    {
        yield return new WaitForSeconds(300f);
        Destroy(car1);
        yield return new WaitForSeconds(300f);
        Destroy(car2);
        yield return new WaitForSeconds(300f);
        Destroy(car3);
        yield return new WaitForSeconds(300f);
        Destroy(car4);
        yield return new WaitForSeconds(1000f);
        Destroy(car5);
        yield return new WaitForSeconds(1000f);
        Destroy(car6);
        yield return new WaitForSeconds(1000f);
        Destroy(car7);
    }

}


