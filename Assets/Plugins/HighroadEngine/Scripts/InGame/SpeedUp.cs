using MoreMountains.HighroadEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedUp : MonoBehaviour
{
    public GameObject[] fxs;
    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<SolidController>().EngineForce += 100;
        //GameObject fx_light = Instantiate(fxs[0]);
        GameObject fx_fire_left = Instantiate(fxs[1]);
        GameObject fx_fire_right = Instantiate(fxs[1]);

        //fx_light.transform.SetParent(other.transform.GetChild(2).transform, false);
        fx_fire_left.transform.SetParent(other.transform.GetChild(2).transform, false);
        fx_fire_right.transform.SetParent(other.transform.GetChild(2).transform, false);

        //fx_light.transform.localScale = Vector3.one * 0.3f;

        fx_fire_left.transform.localPosition = Vector3.left;
        fx_fire_right.transform.localPosition = Vector3.right;

        StartCoroutine(speedSlow(other.gameObject));

        //Destroy(fx_light, 1);
        Destroy(fx_fire_left, 1);
        Destroy(fx_fire_right, 1);
    }
    IEnumerator speedSlow(GameObject car)
    {
        yield return new WaitForSeconds(1);
        car.GetComponent<SolidController>().EngineForce -= 100;
    }

}
