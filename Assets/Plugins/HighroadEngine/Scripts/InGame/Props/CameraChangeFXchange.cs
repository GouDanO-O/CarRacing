using MoreMountains.HighroadEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraChangeFXchange : MonoBehaviour
{
    private GameObject thirdCamera;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    private void Update()
    {
        try
        {
            thirdCamera = GameObject.Find("ThirdPersonCameraRig");
        }
        catch
        {
            return;
        }
        if (thirdCamera == null)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).localScale = Vector3.one;
            }
            return;
        }
           
        if (thirdCamera.gameObject.active == true)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).localScale = Vector3.one * 0.5f;
            }
        }     
    }
}
