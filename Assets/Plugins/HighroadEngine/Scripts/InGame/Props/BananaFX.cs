using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BananaFX : MonoBehaviour
{
    private Tween tween;
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up, 90 * Time.deltaTime);
    }
}
