using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwich : MonoBehaviour
{
    public GameObject cam1;
    public GameObject cam2;

    void Start()
    {
        
    }
  
    private void OnTriggerEnter(Collider other)
    {
        /*
        if (other.gameObject.tag == "Player")
        {
            if (!other.GetComponent<PlayerMovement>().cameraChenge)
            {
                cam1.SetActive(false);
                cam2.SetActive(true);
                other.GetComponent<PlayerMovement>().cameraChenge = true;
            }
            else
            {
                cam1.SetActive(true);
                cam2.SetActive(false);
                other.GetComponent<PlayerMovement>().cameraChenge = false;
            }
        }
        */
    }

}
