using MoreMountains.HighroadEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Banana : MonoBehaviour
{
    private PropManager propManager;

    private void Start()
    {
        propManager = GameObject.Find("GiftManager").GetComponent<PropManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            if (other.GetComponent<SolidController>().EngineForce > 300)
            {
                other.GetComponent<SolidController>().EngineForce -= 50;
                GameObject fx_SlowdDown=Instantiate(propManager.fxs[3], other.transform.position, Quaternion.identity);
                fx_SlowdDown.transform.SetParent(other.transform);
                Destroy(fx_SlowdDown, 1);
            }
            Destroy(gameObject);
        }
    }
}
