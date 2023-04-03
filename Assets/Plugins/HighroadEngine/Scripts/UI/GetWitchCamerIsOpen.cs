using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetWitchCamerIsOpen : MonoBehaviour
{
    public GameObject IsoCameras;

    // Update is called once per frame
    void Update()
    {
        if (IsoCameras.active == true)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(true);
        }
    }
}
